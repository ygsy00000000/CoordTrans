using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.GIS.CommonMethod;
using ZJUGIS.GISModule.CommonMethod;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 转换CAD文件坐标系
    /// </summary>
    public class TransCADMathComm
    {

        public static bool TransCAD(string sSourcePath, string sTargetPath, TCoordinate TClass, ref List<string> sListError)
        {
            IFeatureCursor pSourceCursor = null;
            bool isSuccess = false;
            try
            {
                Dictionary<string, IFeatureClass> pListSourceFeatureClass = CreateFeatClsForCADFeatCls(sSourcePath, TClass, ref sListError);
                if (pListSourceFeatureClass == null || pListSourceFeatureClass.Count == 0)
                {
                    return isSuccess;
                }
                DXFWriter dxfExport = new DXFWriter();
                dxfExport.AddObj(new DXFWriter.DxfLType());
                foreach (IFeatureClass pFeatureClass in pListSourceFeatureClass.Values)
                {
                    if (pFeatureClass == null)
                    {
                        continue;
                    }
                    pSourceCursor = pFeatureClass.Search(null, false);
                    int iCount = pFeatureClass.FeatureCount(null);
                    string sLayerName = FeatureClassComm.GetSuffixName(pFeatureClass);
                    sLayerName = sLayerName.Substring(1, sLayerName.Length - 1);
                    dxfExport.ExportLayer(pSourceCursor, sLayerName, pFeatureClass.ObjectClassID);
                    AEComm.ReleaseCOMObject(pSourceCursor);
                }
                dxfExport.Save(sTargetPath);

                isSuccess = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                GC.Collect();
                AEComm.ReleaseCOMObject(pSourceCursor);
            }
            return isSuccess;
        }

        /// <summary>
        /// 生成CAD内的图层
        /// </summary>
        /// <param name="sSourcePath">CAD文件全路径</param>
        /// <returns></returns>
        public static Dictionary<string, IFeatureClass> CreateFeatClsForCADFeatCls(string sSourcePath, TCoordinate TClass, ref List<string> sListErr)
        {
            Dictionary<string, IFeatureClass> pHaveFeatureClass = new Dictionary<string, IFeatureClass>();
            try
            {
                //检查CAD文件是否存在
                if (File.Exists(sSourcePath))
                {
                    string sCADName = System.IO.Path.GetFileName(sSourcePath);
                    IWorkspace pSourceCADWorkspace = WorkspaceComm.OpenCADWorkspace(sSourcePath);
                    IFeatureDataset pFeatureDataset = (pSourceCADWorkspace as IFeatureWorkspace).OpenFeatureDataset(sCADName);
                    IFeatureClassContainer pFeatureClassContainer = (IFeatureClassContainer)pFeatureDataset;
                    if (pFeatureClassContainer != null)
                    {
                        //对CAD文件中的要素进行遍历处理
                        int iClassCount = pFeatureClassContainer.ClassCount;


                        for (int i = 3; i >= 0; i--)//处理顺序是面、线、点、注记， 0注记、1点、2线、3面
                        {
                            for (int j = 0; j < iClassCount; j++)
                            {
                                IFeatureClass pFeatureClass = pFeatureClassContainer.get_Class(j);
                                if (pFeatureClass == null)
                                {
                                    continue;
                                }
                                //注记图层
                                if (pFeatureClass.FeatureType == esriFeatureType.esriFTAnnotation || pFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                                {
                                    if (i == 0)
                                    {
                                        CreateFeatClsForCADFeatCls(pFeatureClass, ref pHaveFeatureClass, TClass, ref sListErr);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                //点、线、面图层
                                esriGeometryType pGeometryType = pFeatureClass.ShapeType;
                                switch (i)
                                {
                                    case 1:
                                        if (pGeometryType == esriGeometryType.esriGeometryMultipoint ||
                                            pGeometryType == esriGeometryType.esriGeometryPoint)
                                        {
                                            CreateFeatClsForCADFeatCls(pFeatureClass, ref pHaveFeatureClass, TClass, ref sListErr);
                                        }
                                        break;
                                    case 2:
                                        if (pGeometryType == esriGeometryType.esriGeometryLine ||
                                            pGeometryType == esriGeometryType.esriGeometryPath ||
                                            pGeometryType == esriGeometryType.esriGeometryPolyline)
                                        {
                                            CreateFeatClsForCADFeatCls(pFeatureClass, ref pHaveFeatureClass, TClass, ref sListErr);
                                        }
                                        break;
                                    case 3:
                                        if (pGeometryType == esriGeometryType.esriGeometryPolygon ||
                                            pGeometryType == esriGeometryType.esriGeometryRing)
                                        {
                                            CreateFeatClsForCADFeatCls(pFeatureClass, ref pHaveFeatureClass, TClass, ref sListErr);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        string sErr = "打开CAD文件失败";
                        sListErr.Add(sErr);
                    }
                }
                else
                {
                    string sErr = "选择的CAD文件不存在";
                    sListErr.Add(sErr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pHaveFeatureClass;
        }

        /// <summary>
        /// 获取CAD单个图层内的图层
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="pHaveFeatureClass"></param>
        public static void CreateFeatClsForCADFeatCls(IFeatureClass pSourceFeatureClass, ref Dictionary<string, IFeatureClass> pHaveFeatureClass, TCoordinate TClass, ref List<string> sListErr)
        {
            IFeatureCursor pSearchCursor = null;
            try
            {
                if (pSourceFeatureClass == null || pSourceFeatureClass.FeatureCount(null) < 1)
                {
                    return;
                }
                if (pHaveFeatureClass == null)
                {
                    pHaveFeatureClass = new Dictionary<string, IFeatureClass>();
                }
                string sTempGDBPath = AppFileComm.AppTempPath + "/CADTemp.gdb";
                IWorkspace pTempWorkspae = null;
                if (Directory.Exists(sTempGDBPath))
                {
                    pTempWorkspae = WorkspaceComm.OpenFileGDBWorkspace(sTempGDBPath);
                }
                else
                {
                    pTempWorkspae = WorkspaceComm.CreateGDBWorkspace(sTempGDBPath);
                }

                IPoint point_New = new PointClass();
                pSearchCursor = pSourceFeatureClass.Search(null, true);
                IFeature pFeature = pSearchCursor.NextFeature();
                while (pFeature != null)
                {
                    try
                    {
                        IGeometry pSourceGeo = pFeature.ShapeCopy;
                        if (pSourceGeo == null || pSourceGeo.IsEmpty)
                        {
                            pFeature = pSearchCursor.NextFeature();
                            continue;
                        }
                        string sLayerName = FeatureComm.GetStringValue(pFeature, ConstantValueComm._sFieldLayerName);
                        sLayerName = "A" + sLayerName;
                        IFeatureClass pFindFeatureCls = null;
                        //查找图层
                        try
                        {
                            pFindFeatureCls = pHaveFeatureClass[sLayerName];
                        }
                        catch (Exception)
                        {
                            pFindFeatureCls = FeatureClassComm.GetFeatureClass(pTempWorkspae, sLayerName);
                            if (pFindFeatureCls != null)
                            {
                                IDataset m_datads = pFindFeatureCls as IDataset;
                                if (m_datads.CanDelete())
                                {
                                    m_datads.Delete();
                                    pFindFeatureCls = null;
                                }
                                else
                                {
                                    FeatureClassComm.DeleteFeatures(pFindFeatureCls, "");
                                }
                            }
                            if (pFindFeatureCls == null)
                            {
                                pFindFeatureCls = CreateFeatureClass(pSourceFeatureClass, pTempWorkspae, TClass.GetSpatialReference(), sLayerName);
                            }
                            //TClass.SetSpatialReference(pFindFeatureCls);
                            pHaveFeatureClass.Add(sLayerName, pFindFeatureCls);
                        }

                        if (pSourceGeo.GeometryType != pFindFeatureCls.ShapeType)
                        {
                            pFeature = pSearchCursor.NextFeature();
                            continue;
                        }
                        IFeature pNewFeature = pFindFeatureCls.CreateFeature();
                        //赋值属性
                        FeatureComm.CopyFieldValue(pFeature, ref pNewFeature);
                        //修改CAD弧线
                        IGeometry pGeo = GeometryComm.ConvertPolygonFromCAD(pSourceGeo);
                        pGeo = GISComm.GetNewGeometry(TClass, pGeo, point_New);

                        pNewFeature.Shape = pGeo;

                        //注记赋值
                        if (pSourceFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation || pSourceFeatureClass.FeatureType == esriFeatureType.esriFTAnnotation)
                        {
                            GISComm.SetAnnotationFeature(pNewFeature, pFeature, TClass);
                        }
                        pNewFeature.Store();
                    }
                    catch (Exception ex)
                    {
                        string sErr = "图层【" + pSourceFeatureClass.AliasName + "】ID：" + pFeature.OID + "转换失败，失败原因：" + ex.Message;
                        sListErr.Add(sErr);
                    }

                    pFeature = pSearchCursor.NextFeature();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pSearchCursor);
            }
        }

        #region 公共方法代码
        /// <summary>
        /// 以新空间参考创建IFeatureClass
        /// </summary>
        /// <param name="pSourceFeatureClass">IFeatureClass</param>
        /// <param name="pTargetWorkspace">IWorkspace</param>
        /// <param name="pTargetSpatialReference">ISpatialReference</param>
        /// <param name="sNewFeaClsName"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureClass pSourceFeatureClass, IWorkspace pTargetWorkspace, ISpatialReference pTargetSpatialReference, string sNewFeaClsName = "")
        {
            IFeatureClass pFeaCls = null;

            try
            {
                IFields pFields = CopyFields(pSourceFeatureClass, pTargetSpatialReference);
                if (pFields != null)
                {
                    IFields pTargetField = FeatureClassComm.ValidateFields(pFields, pTargetWorkspace);
                    if (pTargetField != null)
                    {
                        if (string.IsNullOrEmpty(sNewFeaClsName))
                        {
                            sNewFeaClsName = FeatureClassComm.GetSuffixName(pSourceFeatureClass);
                        }
                        //在当前FeatureWorkspcae中创建名为sLayerName的Featureclass
                        pFeaCls = (pTargetWorkspace as IFeatureWorkspace).CreateFeatureClass(sNewFeaClsName, pTargetField, null, null, pSourceFeatureClass.FeatureType, pSourceFeatureClass.ShapeFieldName, "");
                        if (pFeaCls != null)
                        {
                            FeatureClassComm.AlterAliasName(pFeaCls, pSourceFeatureClass.AliasName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog("CreateFeatureClass", ex);
            }

            return pFeaCls;
        }

        /// <summary>
        /// 拷贝图层字段
        /// </summary>
        /// <param name="pSourceFeaCls">IFeatureClass</param>
        /// <param name="pSpatialRef">ISpatialReference</param>
        /// <returns>IFields</returns>
        public static IFields CopyFields(IFeatureClass pSourceFeaCls, ISpatialReference pSpatialRef)
        {
            IFields pNewFields = null;

            try
            {
                if (pSourceFeaCls == null)
                {
                    return pNewFields;
                }

                if (pSpatialRef == null)
                {
                    pSpatialRef = (pSourceFeaCls as IGeoDataset).SpatialReference;
                }

                pNewFields = new FieldsClass();
                IFieldsEdit pNewFieldsEdit = pNewFields as IFieldsEdit;

                IGeometryDef pGeoDef = new GeometryDefClass();
                IGeometryDefEdit pGeoDefEdit = pGeoDef as IGeometryDefEdit;

                pGeoDefEdit.GeometryType_2 = pSourceFeaCls.ShapeType;
                pGeoDefEdit.SpatialReference_2 = pSpatialRef;
                //先添加Shape字段
                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Name_2 = "Shape";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                pFieldEdit.GeometryDef_2 = pGeoDef;
                pNewFieldsEdit.AddField(pField);

                //再拷贝其他字段
                for (int i = 0; i < pSourceFeaCls.Fields.FieldCount; i++)
                {
                    IField sIField = pSourceFeaCls.Fields.get_Field(i);

                    esriFieldType tmpType = sIField.Type;
                    if ((tmpType != esriFieldType.esriFieldTypeGeometry) &&
                        (tmpType != esriFieldType.esriFieldTypeOID) &&
                        (tmpType != esriFieldType.esriFieldTypeGlobalID) &&
                                         (sIField.Name.ToLower() != "shape_length") &&
                                         (sIField.Name.ToLower() != "shape_area"))
                    {
                        IField tempIField = new FieldClass();
                        IFieldEdit tempFieldEdit = tempIField as IFieldEdit;
                        tempFieldEdit.Name_2 = sIField.Name;
                        tempFieldEdit.AliasName_2 = sIField.AliasName;
                        tempFieldEdit.Type_2 = tmpType;
                        pNewFieldsEdit.AddField(tempIField);
                    }
                }
            }
            catch (Exception ex)
            {
                pNewFields = null;
                LogComm.WriteLog("CopyFields", ex);
                throw ex;
            }

            return pNewFields;
        }

        #endregion
        /// <summary>
        /// 在内存中创建图层
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="sLayerName">创建的图层英文名</param>
        /// <param name="sLayerAliasName">创建的图层中文名</param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClassInmemeory(IFeatureClass pSourceFeatureClass, string sLayerName, string sLayerAliasName, ISpatialReference pSpatialReference)
        {

            IWorkspaceFactory workspaceFactory = null;
            IWorkspace inmemWorkspace = null;
            IFeatureClass pFeaCls = null;

            try
            {
                if (pSourceFeatureClass == null)
                {
                    return null;
                }
                workspaceFactory = new InMemoryWorkspaceFactoryClass();
                ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", "MyWorkspace", null, 0);
                ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
                inmemWorkspace = (IWorkspace)name.Open();

                //IFields pFields = GISComm.CreateFields(pSourceFeatureClass);
                IFields pFields = FeatureClassComm.CopyFields(pSourceFeatureClass);

                if (pFields != null)
                {
                    IFields pTargetField = FeatureClassComm.ValidateFields(pFields, inmemWorkspace);
                    if (pTargetField != null)
                    {
                        //创建注记内存图层
                        if (pSourceFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation || pSourceFeatureClass.FeatureType == esriFeatureType.esriFTAnnotation)
                        {
                            IAnnoClass annoClass = pSourceFeatureClass.Extension as IAnnoClass;
                            if (annoClass != null)
                            {
                                //设置注记图层的比例尺和单位
                                IGraphicsLayerScale pGraphicsLayerScale = new GraphicsLayerScaleClass();
                                pGraphicsLayerScale.ReferenceScale = annoClass.ReferenceScale;
                                pGraphicsLayerScale.Units = annoClass.ReferenceScaleUnits;
                                IFeatureWorkspaceAnno pFeatureWorkspaceAnno = inmemWorkspace as IFeatureWorkspaceAnno;
                                pFeaCls = pFeatureWorkspaceAnno.CreateAnnotationClass(sLayerName, pTargetField, pSourceFeatureClass.CLSID, pSourceFeatureClass.EXTCLSID, pSourceFeatureClass.ShapeFieldName, string.Empty, null, null, annoClass.AnnoProperties, pGraphicsLayerScale, annoClass.SymbolCollection, true);
                            }
                        }
                        else
                        {
                            pFeaCls = (inmemWorkspace as IFeatureWorkspace).CreateFeatureClass(sLayerName, pTargetField, pSourceFeatureClass.CLSID, pSourceFeatureClass.EXTCLSID, pSourceFeatureClass.FeatureType, pSourceFeatureClass.ShapeFieldName, "");

                        }

                        if (pFeaCls != null)
                        {
                            FeatureClassComm.AlterAliasName(pFeaCls, sLayerAliasName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog("CreateAnnotationFeatureLayerInmemeory", ex);
            }
            finally
            {
                AEComm.ReleaseCOMObject(inmemWorkspace);
                AEComm.ReleaseCOMObject(workspaceFactory);
            }

            return pFeaCls;
        }

        /// <summary>
        /// 根据原注记的要素修改新的注记要素Element
        /// </summary>
        /// <param name="pTargetFeatureBuffer">新注记要素</param>
        /// <param name="pSourceFeature">原注记要素</param>
        /// <returns></returns>
        public static bool SetAnnotationFeature(IFeatureBuffer pTargetFeatureBuffer, IFeature pSourceFeature)
        {
            try
            {
                bool isSuccess = false;
                IAnnotationFeature pTargetAnno = pTargetFeatureBuffer as IAnnotationFeature;
                IAnnotationFeature pSourceAnnp = pSourceFeature as IAnnotationFeature;
                if (pTargetAnno == null || pSourceAnnp == null)
                {
                    return isSuccess;
                }
                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                //将新注记的几何修改为新的
                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 根据原注记的要素修改新的注记要素Element
        /// </summary>
        /// <param name="pTargetFeature">新注记要素</param>
        /// <param name="pSourceFeature">原注记要素</param>
        /// <returns></returns>
        public static bool SetAnnotationFeature(IFeature pTargetFeature, IFeature pSourceFeature)
        {
            try
            {
                bool isSuccess = false;
                IAnnotationFeature pTargetAnno = pTargetFeature as IAnnotationFeature;
                IAnnotationFeature pSourceAnnp = pSourceFeature as IAnnotationFeature;
                if (pTargetAnno == null || pSourceAnnp == null)
                {
                    return isSuccess;
                }
                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                //将新注记的几何修改为新的
                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取新建的空间参考未知坐标系
        /// </summary>
        /// <returns></returns>
        public static ISpatialReference CreateUnknownSpatialReference()
        {
            ISpatialReference pSpr = null;
            try
            {
                //// 创建未知坐标系
                pSpr = new UnknownCoordinateSystemClass();
                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)pSpr;
                spatialReferenceResolution.ConstructFromHorizon();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)pSpr;
                spatialReferenceTolerance.SetDefaultXYTolerance();
            }
            catch (Exception ex)
            {

            }
            return pSpr;
        }

        #region GP工具
        public static bool gpTransCAD(string sSourcePath, string sTargetPath, TCoordinate TClass, ref List<string> sListError)
        {
            bool isSuccess = false;
            IWorkspace pWorkspace = null;
            IFeatureCursor pFeatureCursor = null;
            IFeatureCursor pInFeatureCursor = null;
            try
            {
                string sTempDsName = System.IO.Path.GetFileNameWithoutExtension(sSourcePath);
                sTempDsName = sTempDsName + "_Source";
                string sTempGDBPath = AppFileComm.AppTempPath + "CADTemp.gdb";
                if (!Directory.Exists(sTempGDBPath))
                {
                    pWorkspace = WorkspaceComm.CreateGDBWorkspace(sTempGDBPath);
                }
                else
                {
                    pWorkspace = WorkspaceComm.OpenFileGDBWorkspace(sTempGDBPath);
                }

                bool isTrue = gpCADtoGeodatabase(pWorkspace, sTempDsName, sSourcePath, sTempGDBPath, TClass);
                if (isTrue)
                {
                    string sNewFeaDsName = System.IO.Path.GetFileNameWithoutExtension(sTargetPath);

                    IFeatureDataset pSFeatureDataset = FeatureDatasetComm.GetDataset(pWorkspace, sTempDsName);
                    if (pSFeatureDataset == null)
                    {
                        sListError.Add("创建GP失败！");
                        return false;
                    }
                    List<IDataset> pListData = FeatureClassComm.GetFeatureClassList(pSFeatureDataset);
                    if (pListData != null)
                    {
                        ISpatialReference pSpatialReference = TClass.GetSpatialReference();
                        IFeatureDataset pNewFeaDs = FeatureDatasetComm.GetDataset(pWorkspace, sNewFeaDsName);
                        if (pNewFeaDs != null)
                        {
                            (pNewFeaDs as IDataset).Delete();
                            pNewFeaDs = null;
                        }

                        if (pNewFeaDs == null)
                        {
                            pNewFeaDs = (pWorkspace as IFeatureWorkspace).CreateFeatureDataset(sNewFeaDsName, pSpatialReference);
                        }

                        foreach (IDataset pDataset in pListData)
                        {
                            IFeatureClass pFeatureClass = pDataset as IFeatureClass;
                            if (pFeatureClass == null || pFeatureClass.ShapeType == esriGeometryType.esriGeometryMultiPatch)
                            {
                                continue;
                            }

                            string sNewFeaClsName = pDataset.Name;
                            string sNewTempFeaClsName = sNewFeaClsName + "1";
                            IFeatureClass pTFeaCls = FeatureClassComm.GetFeatureClass(pWorkspace, sNewTempFeaClsName);
                            if (pTFeaCls != null)
                            {
                                (pTFeaCls as IDataset).Delete();
                                pTFeaCls = null;
                            }
                            if (pTFeaCls == null)
                            {
                                pTFeaCls = WorkspaceComm.CreateFeatureClass(pFeatureClass, pNewFeaDs, sNewTempFeaClsName);
                            }

                            pInFeatureCursor = pTFeaCls.Insert(true);
                            pFeatureCursor = pFeatureClass.Search(null, true);

                            IFeature pFeature = pFeatureCursor.NextFeature();
                            int iCount = 0;
                            IPoint point_New = new PointClass();
                            while (pFeature != null)
                            {
                                IGeometry pGeo = pFeature.ShapeCopy;
                                pGeo = GeometryComm.ConvertPolygonFromCAD(pGeo);
                                IGeometry pNewGeo = GISComm.GetNewGeometry(TClass, pGeo, point_New);
                                if (!GeometryComm.CheckIsNullOrEmpty(pNewGeo))
                                {
                                    iCount++;
                                    IFeatureBuffer pFeatureBuffer = pTFeaCls.CreateFeatureBuffer();
                                    pFeatureBuffer.Shape = pNewGeo;
                                    FeatureComm.CopyFieldValue(pFeature, ref pFeatureBuffer);
                                    pInFeatureCursor.InsertFeature(pFeatureBuffer);

                                    if (iCount % 1000 == 0)
                                    {
                                        pInFeatureCursor.Flush();
                                    }
                                }
                                pFeature = pFeatureCursor.NextFeature();
                            }

                            pInFeatureCursor.Flush();
                            AEComm.ReleaseCOMObject(pFeatureCursor);
                            AEComm.ReleaseCOMObject(pInFeatureCursor);

                            (pFeatureClass as IDataset).Delete();
                            pFeatureClass = null;

                        }

                        (pSFeatureDataset as IDataset).Delete();
                        pSFeatureDataset = null;

                        gpExporttoCAD(pWorkspace, sTempGDBPath, sTargetPath);

                        (pNewFeaDs as IDataset).Delete();
                        pNewFeaDs = null;

                        isSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog("gpTransCAD", ex.Message);
                sListError.Add(ex.Message);

            }
            finally
            {
                AEComm.ReleaseCOMObject(pFeatureCursor);
                AEComm.ReleaseCOMObject(pInFeatureCursor);
                AEComm.ReleaseCOMObject(pWorkspace);
                pWorkspace = null;
            }
            return isSuccess;
        }

        /// <summary>
        /// gp工具CAD转gdb
        /// </summary>
        /// <param name="sCADPath"></param>
        /// <param name="sGDBPath"></param>
        /// <returns></returns>
        public static bool gpCADtoGeodatabase(IWorkspace pWks, string sTempDsName, string sCADPath, string sGDBPath, TCoordinate TClass)
        {
            bool isSuccess = false;
            try
            {
                if (!File.Exists(sCADPath))
                {
                    throw new Exception("CAD文件不存在！");
                }

                IFeatureDataset pFeatureDataset = FeatureDatasetComm.GetDataset(pWks, sTempDsName);
                if (pFeatureDataset != null)
                {
                    (pFeatureDataset as IDataset).Delete();
                    pFeatureDataset = null;
                }

                Geoprocessor geoprocessor = new Geoprocessor();
                CADToGeodatabase pCADToGeodatabase = new CADToGeodatabase();
                pCADToGeodatabase.input_cad_datasets = sCADPath;
                pCADToGeodatabase.out_gdb_path = sGDBPath;
                int iScale = GetCADScale(sCADPath);
                if (iScale == 0)
                {
                    iScale = 1000;
                }
                pCADToGeodatabase.reference_scale = (double)iScale;
                pCADToGeodatabase.out_dataset_name = sTempDsName;

                gpRunTool(geoprocessor, pCADToGeodatabase);

                AEComm.ReleaseCOMObject(geoprocessor);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSuccess;
        }

        /// <summary>
        /// gp工具 gdb转CAD
        /// </summary>
        /// <param name="sGDBPath"></param>
        /// <param name="sCADPath"></param>
        /// <returns></returns>
        public static bool gpExporttoCAD(IWorkspace pWorkspace, string sGDBPath, string sCADPath)
        {
            bool isSuccess = false;
            try
            {

                string sDsName = System.IO.Path.GetFileNameWithoutExtension(sCADPath);
                IFeatureDataset pSFeatureDataset = FeatureDatasetComm.GetDataset(pWorkspace, sDsName);
                if (pSFeatureDataset == null)
                {
                    return false;
                }
                List<IDataset> pListData = FeatureClassComm.GetFeatureClassList(pSFeatureDataset);
                if (pListData != null)
                {
                    List<string> pListFeatureClassPath = new List<string>();
                    foreach (IDataset pDataset in pListData)
                    {
                        IFeatureClass pFeatureClass = pDataset as IFeatureClass;
                        if (pFeatureClass == null || pFeatureClass.ShapeType == esriGeometryType.esriGeometryMultiPatch)
                        {
                            continue;
                        }

                        string sPath = GetAllFeaturcClassPath(pFeatureClass);
                        if (!string.IsNullOrWhiteSpace(sPath))
                        {
                            pListFeatureClassPath.Add(sPath);
                        }
                    }
                    if (pListFeatureClassPath.Count > 0)
                    {
                        string[] aFeatureClassPath = pListFeatureClassPath.ToArray();
                        ExportCAD pExportCAD = new ExportCAD();
                        string sin_features = String.Join(";", aFeatureClassPath);
                        pExportCAD.in_features = sin_features;
                        string sExtens = System.IO.Path.GetExtension(sCADPath);
                        string sOutput_Type = string.Empty;
                        if (sExtens.ToLower() == ".dxf")
                        {
                            sOutput_Type = "DXF_R2000";
                        }
                        else
                        {
                            sOutput_Type = "DWG_R2000";
                        }
                        pExportCAD.Output_Type = sOutput_Type;
                        pExportCAD.Output_File = sCADPath;
                        Geoprocessor gp = new Geoprocessor();
                        gpRunTool(gp, pExportCAD);
                        AEComm.ReleaseCOMObject(gp);
                    }
                }
            }
            catch (Exception)
            {
            }
            return isSuccess;
        }

        /// <summary>
        /// 执行GP
        /// </summary>
        /// <param name="geoprocessor"></param>
        /// <param name="process"></param>
        /// <param name="TC"></param>
        public static void gpRunTool(Geoprocessor geoprocessor, IGPProcess process)
        {
            geoprocessor.OverwriteOutput = true;
            try
            {
                geoprocessor.Execute(process, null);
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                AEComm.ReleaseCOMObject(geoprocessor);
            }
        }

        /// <summary>
        /// 获取CAD文件的比例尺
        /// </summary>
        /// <param name="sCADPath"></param>
        /// <returns></returns>
        public static int GetCADScale(string sCADPath)
        {
            int iScale = 0;
            try
            {
                if (File.Exists(sCADPath))
                {
                    string sCADName = System.IO.Path.GetFileName(sCADPath);
                    IWorkspace pSourceCADWorkspace = WorkspaceComm.OpenCADWorkspace(sCADPath);
                    IFeatureDataset pFeatureDataset = (pSourceCADWorkspace as IFeatureWorkspace).OpenFeatureDataset(sCADName);
                    IFeatureClassContainer pFeatureClassContainer = (IFeatureClassContainer)pFeatureDataset;
                    if (pFeatureClassContainer != null)
                    {
                        //对CAD文件中的要素进行遍历处理
                        int iClassCount = pFeatureClassContainer.ClassCount;
                        for (int j = 0; j < iClassCount; j++)
                        {
                            IFeatureClass pFeatureClass = pFeatureClassContainer.get_Class(j);
                            if (pFeatureClass.FeatureType == esriFeatureType.esriFTAnnotation || pFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                            {
                                IAnnoClass pAnnoClass = pFeatureClass.Extension as IAnnoClass;
                                if (pAnnoClass != null)
                                {
                                    iScale = (int)pAnnoClass.ReferenceScale;
                                    break;
                                }
                            }
                        }
                    }

                    AEComm.ReleaseCOMObject(pSourceCADWorkspace);
                    pSourceCADWorkspace = null;
                }
            }
            catch (Exception ex)
            {
            }
            return iScale;
        }

        /// <summary>
        /// 获取图层的全路径
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <returns></returns>
        public static string GetAllFeaturcClassPath(IFeatureClass pFeatureClass)
        {
            string sPath = string.Empty;
            try
            {
                if (pFeatureClass != null)
                {
                    IDataset pDataset = pFeatureClass as IDataset;
                    sPath = pDataset.Workspace.PathName;
                    IFeatureDataset pFeatureDataset = pFeatureClass.FeatureDataset;
                    if (pFeatureDataset != null)
                    {
                        sPath += "\\" + pFeatureDataset.Name;
                    }
                    sPath += "\\" + pDataset.Name;
                }
            }
            catch (Exception)
            {
            }
            return sPath;
        }
        #endregion
    }
}
