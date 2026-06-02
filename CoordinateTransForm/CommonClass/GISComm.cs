using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using ZJUGIS.Framework.CommonModule;
using ZJUGIS.GIS.CommonMethod;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// AE相关公告方法类
    /// </summary>
    public class GISComm
    {
        #region 创建坐标系
        /// <summary>
        /// 创建地理坐标系
        /// </summary>
        /// <param name="earth"></param>
        /// <returns></returns>
        public static ISpatialReference CreateGeographicCoordinateSystem(EarthParams earth)
        {
            ISpatialReference pSpr = null;
            try
            {
                ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                double A = earth.A;             //// 长半轴
                double F = earth.F;             //// 扁率
                double B = earth.B;             //// 短半轴

                if (Math.Abs(A - 6378245.0) < 1E-09 && Math.Abs(F - 298.3) < 1E-09)
                {
                    //// 北京54
                    esriSRGeoCSType geoSystem = esriSRGeoCSType.esriSRGeoCS_Beijing1954;
                    pSpr = pSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)geoSystem);
                }
                else if (Math.Abs(A - 6378140.0) < 1E-09 && Math.Abs(F - 298.257) < 1E-09)
                {
                    //// 西安80
                    esriSRGeoCS3Type geoSystem = esriSRGeoCS3Type.esriSRGeoCS_Xian1980;
                    pSpr = pSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)geoSystem);
                }
                else if (Math.Abs(A - 6378137.0) < 1E-09 && Math.Abs(F - 298.257222101) < 1E-09 && Math.Abs(B - 6356752.314140356) < 1E-09)
                {
                    //// 国家2000
                    pSpr = pSpatialReferenceFactory.CreateGeographicCoordinateSystem(4490);
                }
                else if (Math.Abs(A - 6378137.0) < 1E-09 && Math.Abs(F - 298.257223563) < 1E-09 && Math.Abs(B - 6356752.314245179) < 1E-09)
                {
                    //// WGS84
                    esriSRGeoCSType geoSystem = esriSRGeoCSType.esriSRGeoCS_WGS1984;
                    pSpr = pSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)geoSystem);
                }
                else
                {
                    //// 自定义
                    IGeographicCoordinateSystem pGCS = new GeographicCoordinateSystemClass();
                    IGeographicCoordinateSystemEdit pGCSE = pGCS as IGeographicCoordinateSystemEdit;
                    //// 设置地理坐标相关信息
                    object name = earth.Name;
                    object Alias = earth.Name;
                    object abbreviation = "";
                    object remarks = "";
                    object useage = "";
                    object oA = A;
                    object oB = B;
                    object oF = 1.0 / F;


                    //// 设置椭球
                    ISpheroid pSp = new SpheroidClass();
                    (pSp as ISpheroidEdit).Define(ref name, ref Alias, ref abbreviation, ref remarks, ref oA, ref oF);
                    object oSpheroid = pSp;

                    //// 设置信息块   
                    IDatum dt = new DatumClass();
                    (dt as IDatumEdit).Define(ref name, ref Alias, ref abbreviation, ref remarks, ref oSpheroid);
                    object oDatum = dt;

                    //// 设置初始子午线
                    IPrimeMeridian pm = new PrimeMeridianClass();
                    pm = pSpatialReferenceFactory.CreatePrimeMeridian((int)esriSRPrimeMType.esriSRPrimeM_Greenwich);
                    object primeMeridian = pm;

                    //// 设置单位 "Degree (0.017453292519943299)"
                    //ILinearUnit pUnit = new LinearUnitClass();
                    IUnit pUnit = new AngularUnitClass();
                    pUnit = pSpatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_Degree) as IUnit;
                    object oUnit = pUnit;

                    pGCSE.Define(ref name, ref Alias, ref abbreviation, ref remarks, ref useage, ref oDatum, ref primeMeridian, ref oUnit);
                    pSpr = pGCS as ISpatialReference;
                }
                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)pSpr;
                spatialReferenceResolution.ConstructFromHorizon();
                spatialReferenceResolution.SetDefaultXYResolution();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)pSpr;
                spatialReferenceTolerance.SetDefaultXYTolerance();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pSpr;
        }

        /// <summary>
        /// 创建投影坐标系
        /// </summary>
        /// <param name="earth"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="dAddY"></param>
        /// <returns></returns>
        public static ISpatialReference CreateProjectCoordinateSystem(EarthParams earth, double dCentralMeridian, double dAddY)
        {
            ISpatialReference pSpr = null;
            try
            {
                IGeographicCoordinateSystem pGCS = CreateGeographicCoordinateSystem(earth) as IGeographicCoordinateSystem;
                ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();

                #region 自定义投影
                //自定义投影
                IProjectedCoordinateSystem pPrj = new ProjectedCoordinateSystemClass();
                IProjectedCoordinateSystemEdit pPrjE = pPrj as IProjectedCoordinateSystemEdit;

                //定义投影方式，参考： esriSRProjectionType
                IProjection pProjection = new ProjectionClass();
                pProjection = pSpatialReferenceFactory.CreateProjection((int)esriSRProjectionType.esriSRProjection_GaussKruger);

                //定义投影单位，参考：esriSRUnitType
                ILinearUnit pUnit = new LinearUnitClass();
                pUnit = pSpatialReferenceFactory.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;

                //定义其他参数，参考：esriSRParameterType
                IParameter[] pParm = new IParameter[6];
                pParm[0] = pSpatialReferenceFactory.CreateParameter((int)esriSRParameterType.esriSRParameter_FalseEasting);
                pParm[0].Value = dAddY;

                pParm[1] = pSpatialReferenceFactory.CreateParameter((int)esriSRParameterType.esriSRParameter_FalseNorthing);
                pParm[1].Value = 0;

                pParm[2] = pSpatialReferenceFactory.CreateParameter((int)esriSRParameterType.esriSRParameter_CentralMeridian);
                pParm[2].Value = dCentralMeridian;

                pParm[3] = pSpatialReferenceFactory.CreateParameter((int)esriSRParameterType.esriSRParameter_ScaleFactor);
                pParm[3].Value = 1.0;

                pParm[4] = pSpatialReferenceFactory.CreateParameter((int)esriSRParameterType.esriSRParameter_LatitudeOfOrigin);
                pParm[4].Value = 0.0;

                //设置投影相关信息
                Regex oRegex = new Regex(@"[\u4E00-\u9FA5\-]*");
                string sName = oRegex.Replace(earth.Name, "") + "_User_Defined"; ;
                object name = sName;
                object Alias = earth.Name;
                object abbreviation = "GaussKruger";
                object remarks = "";
                object usage = "";
                object oGCS = pGCS;
                object oUnit = pUnit;
                object projection = pProjection;
                object parameters = pParm;

                pPrjE.Define(ref name, ref Alias, ref abbreviation, ref remarks, ref usage, ref oGCS, ref  oUnit, ref projection, ref parameters);
                pSpr = pPrj as ISpatialReference;
                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)pSpr;
                spatialReferenceResolution.ConstructFromHorizon();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)pSpr;
                spatialReferenceTolerance.SetDefaultXYTolerance();

                #endregion


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pSpr;
        }

        #endregion

        #region 空间数据转换

        /// <summary>
        /// 根据shp文件创建要素类
        /// </summary>
        /// <param name="shpFile">目标路径</param>
        /// <param name="pSourcefeaCls">源要素类</param>
        /// <param name="bResetSpatialReference">是否清除空间参考</param>
        /// <returns></returns>
        public static IFeatureClass CreateShpFeatureClass(string shpFile, IFeatureClass pSourcefeaCls, ISpatialReference pSpr)
        {
            IFeatureClass pFeaCls = null;
            IFeatureWorkspace pFeaWks = null;
            IWorkspace pWks = null;
            try
            {
                string filepath = System.IO.Path.GetDirectoryName(shpFile);
                string filename = System.IO.Path.GetFileNameWithoutExtension(shpFile);

                pWks = WorkspaceComm.OpenShpWorkspace(filepath);
                pFeaWks = pWks as IFeatureWorkspace;
                string sNewshpFile = filepath + "\\" + filename + "\\" + filename + ".shp";
                //检查图层是否已存在
                if (System.IO.File.Exists(sNewshpFile))
                {
                    try
                    {
                        pFeaCls = pFeaWks.OpenFeatureClass(filename);
                        if (pFeaCls != null)
                        {
                            IDataset pDst = pFeaCls as IDataset;
                            if (pDst.CanDelete())
                            {
                                pDst.Delete();
                                pFeaCls = null;
                            }
                        }
                    }
                    catch { }
                }

                if (pFeaCls == null)
                {
                    pFeaCls = WorkspaceComm.CreateFeatureClass(pSourcefeaCls, pWks, pSpr, filename);

                    FeatureClassComm.AddShapeAreaField(pSourcefeaCls, pFeaCls);
                }

                //if (pFeaCls != null)
                //{
                //    if (pFeaCls.FeatureCount(null) > 0)
                //    {
                //        WorkspaceComm.DelFeatures(pFeaCls, string.Empty);
                //    }
                //}

                //class2 = pWks.CreateFeatureClass(name, pfields, null, null, esriFeatureType.esriFTSimple, "SHAPE", null);
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            finally
            {
                ESRI.ArcGIS.ADF.ComReleaser.ReleaseCOMObject(pFeaWks);
                ESRI.ArcGIS.ADF.ComReleaser.ReleaseCOMObject(pWks);
            }
            return pFeaCls;
        }

        public static IFeatureClass CreateFeatureClass(IFeatureClass pSourceFeatureClass, IWorkspace pTargetWorkspace, ISpatialReference pTargetSpatialReference, string sNewFeaClsName = "")
        {
            IFeatureClass pFeaCls = null;

            try
            {
                IFields pFields = FeatureClassComm.CopyFields(pSourceFeatureClass, pTargetSpatialReference);
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
            }

            return pFeaCls;
        }

        /// <summary>
        /// 创建字段集
        /// </summary>
        /// <param name="dicfields"></param>
        /// <returns></returns>
        public static IFieldsEdit CreateFields(IFeatureClass pfeaCls)
        {
            IFieldsEdit pFieldsEdit = new FieldsClass();
            try
            {
                //序号字段
                IFieldEdit pFieldEdit = new FieldClass();
                pFieldEdit.Name_2 = "OBJECTID";
                pFieldEdit.AliasName_2 = "序号";
                pFieldEdit.IsNullable_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                pFieldsEdit.AddField(pFieldEdit);

                //几何字段
                pFieldEdit = new FieldClass();
                IGeometryDef pGeometryDef = new GeometryDef();
                IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
                pGeometryDefEdit.GeometryType_2 = pfeaCls.ShapeType;
                pGeometryDefEdit.SpatialReference_2 = null;                         //坐标系
                pFieldEdit.Name_2 = "SHAPE";
                pFieldEdit.AliasName_2 = "几何";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = true;
                pFieldEdit.Editable_2 = true;
                pFieldEdit.GeometryDef_2 = pGeometryDef;
                pFieldsEdit.AddField(pFieldEdit);

                for (int i = 0; i < pfeaCls.Fields.FieldCount; i++)
                {
                    IField pfield = pfeaCls.Fields.get_Field(i);
                    esriFieldType type = pfield.Type;
                    if ((((type != esriFieldType.esriFieldTypeGeometry) && (type != esriFieldType.esriFieldTypeOID)) && ((type != esriFieldType.esriFieldTypeGlobalID) && (pfield.Name.ToLower() != "shape_length"))) && (pfield.Name.ToLower() != "shape_area"))
                    {
                        pFieldEdit = new FieldClass();
                        pFieldEdit.Name_2 = pfield.Name;
                        pFieldEdit.AliasName_2 = pfield.AliasName;
                        pFieldEdit.IsNullable_2 = true;
                        pFieldEdit.Type_2 = type;
                        pFieldsEdit.AddField(pFieldEdit);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pFieldsEdit;
        }

        /// <summary>
        /// 对要素类进行坐标转换
        /// </summary>
        /// <param name="pSourcefeaCls"></param>
        /// <param name="pTargetfeaCls"></param>
        public static bool FeatureClassToFeatureClass(TCoordinate TClass, IFeatureClass pSourcefeaCls, IFeatureClass pTargetfeaCls, ref List<string> sErrorMsgs, IProgressDialog pProgressDialog = null, bool bShapeFile = false)
        {
            bool flag = false;
            IFeatureCursor pSfeaCur = null;
            IFeatureCursor pTfeaCur = null;
            try
            {
                if (TClass == null || pSourcefeaCls == null || pTargetfeaCls == null)
                {
                    return flag;
                }
                int iCount = pSourcefeaCls.FeatureCount(null);
                if (iCount == 0)
                {
                    return true;
                }

                string sourcefeaClsName = pSourcefeaCls.AliasName;
                if (pProgressDialog != null)
                {
                    int iMAX = iCount;
                    if (iCount > 2000)
                    {
                        iMAX = (int)(Math.Ceiling(iCount / 2000.0));
                    }
                    pProgressDialog.Max = iMAX;
                    pProgressDialog.Position = 0;
                    pProgressDialog.Message = string.Format("转换图层【{0}】", sourcefeaClsName);
                }

                pSfeaCur = pSourcefeaCls.Search(null, true);
                pTfeaCur = pTargetfeaCls.Insert(true);
                bool isAnno = false;//是否是注记图层;
                if (pSourcefeaCls.FeatureType == esriFeatureType.esriFTAnnotation || pSourcefeaCls.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                {
                    isAnno = true;
                }
                IFeature pSourcefea = pSfeaCur.NextFeature();
                IFeatureBuffer pfea = null;
                IGeometry pGeo = null;
                IGeometry pNewGeo = null;
                int count = 0;
                bool bAddShapeArea = false;
                if (bShapeFile)
                {
                    int iDx = pTargetfeaCls.Fields.FindField("SHAPE_Area");
                    if (iDx > -1)
                    {
                        bAddShapeArea = true;
                    }
                }

                IPoint point_New = new PointClass();
                while (pSourcefea != null)
                {
                    try
                    {
                        count++;

                        pfea = pTargetfeaCls.CreateFeatureBuffer();
                        pGeo = pSourcefea.ShapeCopy;

                        if (pProgressDialog != null)
                        {
                            if (iCount > 2000)
                            {
                                if (count % 2000 == 0)
                                {
                                    pProgressDialog.Description = string.Format("正在转换{0}/{1}", count, iCount);
                                    pProgressDialog.Step(1);
                                }
                            }
                            else
                            {
                                pProgressDialog.Description = string.Format("正在转换{0}/{1}", count, iCount);
                                pProgressDialog.Step(1);
                            }
                        }                        

                        //// 执行坐标转换，生成新的图形
                        pNewGeo = GetNewGeometry(TClass, pGeo, point_New);
                        if (!GeometryComm.CheckIsNullOrEmpty(pNewGeo))
                        {
                            pfea.Shape = pNewGeo;
                            //// 赋值属性信息
                            if (!FeatureComm.CopyFieldValue(pSourcefea, ref pfea))
                            {
                                string sError = string.Format("图层：{0}, 要素OID：{1}, 转换图形复制属性值失败", sourcefeaClsName, pSourcefea.OID);
                                sErrorMsgs.Add(sError);
                            }
                            if (bAddShapeArea)
                            {
                                FeatureComm.SetValue(ref pfea, "SHAPE_Area", Math.Abs((pNewGeo as IArea).Area));
                            }

                            if (isAnno)
                            {
                                SetAnnotationFeature(pfea, pSourcefea, TClass);
                            }

                            //// 添加要素
                            pTfeaCur.InsertFeature(pfea);

                        }

                        if (count % 2000 == 0)
                        {
                            pTfeaCur.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        string sError = string.Format("图层：{0}, 要素OID：{1}, 转换图形出现错误：{2}", sourcefeaClsName, pSourcefea.OID, ex.Message);
                        sErrorMsgs.Add(sError);
                        pSourcefea = pSfeaCur.NextFeature();
                        continue;
                    }

                    pSourcefea = pSfeaCur.NextFeature();
                }

                pTfeaCur.Flush();
                flag = true;
            }
            catch (Exception ex)
            {
                string sError = string.Format("图层：{0}, 转换图形出现错误：{1}", (pSourcefeaCls as IDataset).Name, ex.Message);
                sErrorMsgs.Add(sError);
            }
            finally
            {
                AEComm.ReleaseCOMObject(pSfeaCur);
                AEComm.ReleaseCOMObject(pTfeaCur);
                pSfeaCur = null;
                pTfeaCur = null;
            }
            return flag;
        }

        /// <summary>
        /// 获取坐标转换后的几何图形
        /// </summary>
        /// <param name="TClass"></param>
        /// <param name="pGeo"></param>
        /// <returns></returns>
        public static IGeometry GetNewGeometry(TCoordinate TClass, IGeometry pGeo, IPoint point_New)
        {
            if (TClass == null || pGeo == null)
            {
                return null;
            }
            IGeometry pNewGeo = null;
            object missing = Missing.Value;
            IPoint point;
            
            try
            {
                if (pGeo.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    point = pGeo as IPoint;
                    pNewGeo = TClass.PointCompute(point, point_New);

                }
                else if (pGeo.GeometryType == esriGeometryType.esriGeometryPolyline || pGeo.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    IGeometryCollection pGeoCol = pGeo as IGeometryCollection;
                    if (pGeoCol == null || pGeoCol.GeometryCount == 0)
                    {
                        return pNewGeo;
                    }
                    IGeometryCollection pGeoCol_New = null;
                    IPointCollection pointCol_New = null;
                    IPointCollection pointCol = null;
                    bool bPolygon = true;
                    if (pGeo.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        pNewGeo = new PolylineClass();
                        pGeoCol_New = pNewGeo as IGeometryCollection;
                        pointCol_New = new PathClass();
                        bPolygon = false;
                    }
                    else
                    {
                        pNewGeo = new PolygonClass();
                        pGeoCol_New = pNewGeo as IGeometryCollection;
                        pointCol_New = new RingClass();
                    }

                    //// 遍历图形集
                    for (int i = 0; i < pGeoCol.GeometryCount; i++)
                    {
                        pointCol = pGeoCol.get_Geometry(i) as IPointCollection;
                        for (int j = 0; j < pointCol.PointCount; j++)
                        {
                            point = pointCol.get_Point(j);
                            point_New = TClass.PointCompute(point, point_New);
                            pointCol_New.AddPoint(point_New, ref missing, ref missing);
                        }
                        pGeoCol_New.AddGeometry(pointCol_New as IGeometry, ref missing, ref missing);
                        if (bPolygon)
                        {
                            pointCol_New = new RingClass();
                        }
                        else
                        {
                            pointCol_New = new PathClass();
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pNewGeo;
        }

        /// <summary>
        /// 根据原注记的要素修改新的注记要素Element
        /// </summary>
        /// <param name="pTargetFeatureBuffer">新注记要素</param>
        /// <param name="pSourceFeature">原注记要素</param>
        /// <param name="pGeo">新注记的几何</param>
        /// <returns></returns>
        public static bool SetAnnotationFeature(IFeatureBuffer pTargetFeatureBuffer, IFeature pSourceFeature, TCoordinate TClass)
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
                //IGeometry pGeo = GetNewGeometry(TClass, pSourceAnnp.Annotation.Geometry);

                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                //将新注记的几何修改为新的
                //pTargetAnno.Annotation.Geometry = pGeo;               

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
        /// <param name="pGeo">新注记的几何</param>
        /// <returns></returns>
        public static bool SetAnnotationFeature(IFeature pTargetFeature, IFeature pSourceFeature, TCoordinate TClass)
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
                //IGeometry pGeo = GetNewGeometry(TClass, pSourceAnnp.Annotation.Geometry);
                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                //将新注记的几何修改为新的
                //pTargetAnno.Annotation.Geometry = pGeo;
                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region 栅格数据
        /// <summary>
        /// 采用逐点转换的方式
        /// </summary>
        /// <param name="TClass"></param>
        /// <param name="sTargetFile"></param>
        /// <param name="rasterLayer"></param>
        /// <param name="sErrorMsgs"></param>
        /// <returns></returns>
        public static bool RasterLayerTransFormByPoints(TCoordinate TClass, string sTargetFile, IRasterLayer rasterLayer, ref List<string> sErrorMsgs)
        {
            bool flag = false;
            try
            {
                IPointCollection pSourceCol = new MultipointClass();
                IPointCollection pTargetCol = new MultipointClass();

                IRaster raster = rasterLayer.Raster;
                IRasterBandCollection rasterBands = (IRasterBandCollection)raster;      //// 光栅带

                //ReadWriteRawBlocks(rasterBands);

                IRasterBand rasterBand = rasterBands.Item(0);
                //像素
                IRawPixels rawPixels = (IRawPixels)rasterBand;
                IRasterProps rasterProps = (IRasterProps)rawPixels;

                //IPnt pntSize = new PntClass();
                //pntSize.SetCoords(1, 1);
                //IPixelBlock3 pixelBlock = (IPixelBlock3)rawPixels.CreatePixelBlock(pntSize);     //// 指定像素块大小来创建像素快

                IRaster2 raster2 = raster as IRaster2;
                IPoint pnt = new PointClass();
                IPoint pNewPnt = new PointClass();
                double x = 0.0, y = 0.0;
                for (int i = 0; i < rasterProps.Height; i = i + 100)
                {
                    for (int j = 0; j < rasterProps.Width; j = j + 50)
                    {
                        raster2.PixelToMap(j, i, out x, out y);
                        pnt.PutCoords(x, y);
                        pNewPnt = TClass.PointCompute(pnt, pNewPnt);

                        pSourceCol.AddPoint(pnt);
                        pTargetCol.AddPoint(pNewPnt);
                    }
                }

                ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                ISpatialReference pSpr = pSpatialReferenceFactory.CreateProjectedCoordinateSystem(4528);

                IGeoReference pGR = rasterLayer as IGeoReference;
                pGR.Warp(pSourceCol, pTargetCol, 1);                //// 最后一个参数，平移变换、相形变换、仿射变换

                ///栅格重采样
                double dNewCellSize = (rasterProps.MeanCellSize().X + rasterProps.MeanCellSize().Y) / 2.0;
                IRasterGeometryProc rasterGeometryProc = new RasterGeometryProcClass();
                rasterGeometryProc.Resample(rstResamplingTypes.RSP_BilinearInterpolation, dNewCellSize, raster);            //// 第一个参数，使用双线性内插法

                //要保存的图层；
                IRaster pRaster = rasterLayer.Raster;
                IRaster2 pRaster2 = pRaster as IRaster2;
                ISaveAs pSaveAs = pRaster2 as ISaveAs;
                //// @"F:\Work\DEM高程矩阵\DEM高程矩阵\bin\Debug\渲染图层2.tif",null,"TIFF"
                string sExt = System.IO.Path.GetExtension(sTargetFile);
                sExt = sExt.Substring(1);
                IDataset pNewDs = pSaveAs.SaveAs(sTargetFile, null, sExt);

                IGeoDataset pGeo = pNewDs as IGeoDataset;

                flag = TClass.SetSpatialReference(pGeo);
            }
            catch (Exception ex)
            {
                string sError = string.Format("图层：{0}, 转换图形出现错误：{1}", rasterLayer.Name, ex.Message);
                sErrorMsgs.Add(sError);
            }
            finally
            {

            }
            return flag;
        }

        public static void ReadWriteRawBlocks(IRasterBandCollection rasBandCol)
        {
            //IRasterBandCollection rasBandCol = (IRasterBandCollection)rasDs;
            IRawBlocks rawBlocks;
            IRasterInfo rasInfo;
            IPixelBlock pb;

            // Iterate through each band of the dataset.
            for (int m = 0; m <= rasBandCol.Count - 1; m++)
            {
                // QI to IRawBlocks from IRasterBandCollection.
                rawBlocks = (IRawBlocks)rasBandCol.Item(m);
                rasInfo = rawBlocks.RasterInfo;
                // Create the pixel block.
                pb = rawBlocks.CreatePixelBlock();

                // Determine the tiling scheme for the raster dataset.

                int bStartX = (int)Math.Floor((rasInfo.Extent.Envelope.XMin -
                    rasInfo.Origin.X) / (rasInfo.BlockWidth * rasInfo.CellSize.X));
                int bEndX = (int)Math.Ceiling((rasInfo.Extent.Envelope.XMax -
                    rasInfo.Origin.X) / (rasInfo.BlockWidth * rasInfo.CellSize.X));
                int bStartY = (int)Math.Floor((rasInfo.Origin.Y -
                    rasInfo.Extent.Envelope.YMax) / (rasInfo.BlockHeight *
                    rasInfo.CellSize.Y));
                int bEndY = (int)Math.Ceiling((rasInfo.Origin.Y -
                    rasInfo.Extent.Envelope.YMin) / (rasInfo.BlockHeight *
                    rasInfo.CellSize.Y));

                // Iterate through the pixel blocks.
                for (int pbYcursor = bStartY; pbYcursor < bEndY; pbYcursor++)
                {
                    for (int pbXcursor = bStartX; pbXcursor < bEndX; pbXcursor++)
                    {
                        // Get the pixel block.
                        rawBlocks.ReadBlock(pbXcursor, pbYcursor, 0, pb);
                        System.Array safeArray;
                        // Put the pixel block into a SafeArray for manipulation.
                        safeArray = (System.Array)pb.get_SafeArray(0);

                        // Iterate through the pixels in the pixel block.
                        for (int safeArrayHeight = 0; safeArrayHeight < pb.Height;
                            safeArrayHeight++)
                        {
                            for (int safeArrayWidth = 0; safeArrayWidth < pb.Width;
                                safeArrayWidth++)
                            {
                                // Use System.Array.SetValue to write the new pixel value back into the SafeArray.
                                safeArray.SetValue(Convert.ToByte(128), safeArrayWidth,
                                    safeArrayHeight);
                            }
                        }
                        // Set the SafeArray back to the pixel block.
                        pb.set_SafeArray(0, safeArray);

                        // Write the pixel block back to the dataset.
                        rawBlocks.WriteBlock(pbXcursor, pbYcursor, 0, pb);
                    }
                }
            }
        }

        #endregion

        #region 坐标系
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

        /// <summary>
        /// 判断是否要清除原有空间参考
        /// </summary>
        /// <param name="pfeaCls"></param>
        /// <returns></returns>
        public static bool Judge(IFeatureClass pfeaCls)
        {
            bool flag = false;
            try
            {
                //// 判断原数据的空间参考
                IGeoDataset pGeoDs = pfeaCls as IGeoDataset;
                //// 判断空间参考是不是未知
                if (pGeoDs.SpatialReference is IUnknownCoordinateSystem)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {

            }
            return flag;
        }

        /// <summary>
        /// 判断两个空间参考是否一致
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <returns></returns>
        public static bool JudgeSpatialReference(ISpatialReference pSpr1, ISpatialReference pSpr2)
        {
            bool flag = false;
            try
            {
                //如果是大地坐标，判断长半轴和扁率是否相同
                if (pSpr1 is IGeographicCoordinateSystem && pSpr2 is IGeographicCoordinateSystem)
                {
                    IGeographicCoordinateSystem sysType1 = pSpr1 as IGeographicCoordinateSystem;
                    IGeographicCoordinateSystem sysType2 = pSpr2 as IGeographicCoordinateSystem;
                    double A1 = sysType1.Datum.Spheroid.SemiMajorAxis;            //// 长半轴
                    double F1 = 1.0 / sysType1.Datum.Spheroid.Flattening;               //// 扁率
                    double A2 = sysType2.Datum.Spheroid.SemiMajorAxis;            //// 长半轴
                    double F2 = 1.0 / sysType2.Datum.Spheroid.Flattening;               //// 扁率
                    if (Math.Abs(A1 - A2) < 1E-09 && Math.Abs(F1 - F2) < 1E-09)
                    {
                        flag = true;
                    }
                }
                //如果是投影坐标，比较两个坐标系的长半轴、扁率、中央经线、向东偏移量都是否一致
                else if (pSpr1 is IProjectedCoordinateSystem && pSpr2 is IProjectedCoordinateSystem)
                {
                    IProjectedCoordinateSystem sysType1 = pSpr1 as IProjectedCoordinateSystem;
                    IProjectedCoordinateSystem sysType2 = pSpr2 as IProjectedCoordinateSystem;
                    double dCentralMeridian1 = sysType1.get_CentralMeridian(false);                             //// 中央经线
                    double dFalseEastint1 = sysType1.FalseEasting;                                              //// 向东偏移量
                    double A1 = sysType1.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;               //// 长半轴
                    double F1 = 1.0 / sysType1.GeographicCoordinateSystem.Datum.Spheroid.Flattening;            //// 扁率

                    double dCentralMeridian2 = sysType2.get_CentralMeridian(false);                             //// 中央经线
                    double dFalseEastint2 = sysType2.FalseEasting;                                              //// 向东偏移量
                    double A2 = sysType2.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;               //// 长半轴
                    double F2 = 1.0 / sysType2.GeographicCoordinateSystem.Datum.Spheroid.Flattening;            //// 扁率
                    if (Math.Abs(A1 - A2) < 1E-09 && Math.Abs(F1 - F2) < 1E-09
                        && Math.Abs(dCentralMeridian1 - dCentralMeridian2) < 1E-09 && Math.Abs(dFalseEastint1 - dFalseEastint2) < 1E-09)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }
        #endregion

        #region 图层属性管理

        /// <summary>
        /// 面积字段赋值
        /// </summary>
        /// <param name="pTargetFeatureClass">目标图层</param>
        /// <param name="sAreaFieldName">面积字段名</param>
        /// <param name="pListErr">记录错误集合</param>
        /// <param name="isNewAddAreaField">是否新增面积字段，默认不新增</param>
        /// <returns></returns>
        public static bool AssignmentAreaForAreaField(IFeatureClass pTargetFeatureClass, string sAreaFieldName, ref List<string> pListErr, bool isNewAddAreaField = false)
        {
            IFeatureCursor pUpdateCursor = null;
            try
            {
                bool isSuccess = false;
                if (pTargetFeatureClass == null)
                {
                    return isSuccess;
                }
                pUpdateCursor = pTargetFeatureClass.Update(null, false);
                //不新增字段
                if (!isNewAddAreaField)
                {
                    IFeature pFeature = pUpdateCursor.NextFeature();
                    while (pFeature != null)
                    {
                        try
                        {
                            //计算椭球面积
                            double dArea = AreaComm.CalculateGlobeArea(pFeature.ShapeCopy);
                            //修改面积字段
                            bool bSuccess = FeatureComm.SetValue(pFeature, sAreaFieldName, dArea);
                            if (!bSuccess)
                            {
                                string sErr = "要素【" + pFeature.OID + "】赋值失败";
                                pListErr.Add(sErr);
                            }
                        }
                        catch (Exception ex)
                        {
                            string sErr = "要素【" + pFeature.OID + "】赋值失败，错误原因：" + ex.Message;
                            pListErr.Add(sErr);
                        }
                        pFeature = pUpdateCursor.NextFeature();
                    }

                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pUpdateCursor);
            }
        }

        #endregion

        #region 获取图层范围
        /// <summary>
        /// 获取图层的图形范围
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <returns></returns>
        public static IEnvelope GetEnvelopeByFeatureClass(IFeatureClass pFeatureClass)
        {
            try
            {
                IEnvelope pEnvelope = new EnvelopeClass();
                //关键代码，替换了要素遍历，提高效率
                IEnumGeometryBind enumGeometryBind = new EnumFeatureGeometryClass();
                enumGeometryBind.BindGeometrySource(null, pFeatureClass);
                IEnumGeometry enumGeometry = (IEnumGeometry)enumGeometryBind;
                IGeometryFactory geoFactory = new GeometryEnvironment() as IGeometryFactory;
                IGeometry geo = geoFactory.CreateGeometryFromEnumerator(enumGeometry);
                pEnvelope.Union(geo.Envelope);

                //释放数据
                AEComm.ReleaseCOMObject(geo);
                AEComm.ReleaseCOMObject(geoFactory);
                AEComm.ReleaseCOMObject(enumGeometry);
                return pEnvelope;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
