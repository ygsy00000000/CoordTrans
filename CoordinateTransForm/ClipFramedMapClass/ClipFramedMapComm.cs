using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.Framework.Dev;
using ZJUGIS.GIS.CommonMethod;
using ZJUGIS.GISModule.CommonMethod;

namespace ZJUGIS.CoordinateTrans.ClipFramedMapClass
{
    /// <summary>
    /// 分幅切割
    /// </summary>
    public class ClipFramedMapComm
    {
        private static string m_TemMdbFile = AppFileComm.AppPath + @"\Temp\temp.mdb";//临时库

        /// <summary>
        /// 切割形成新的分幅图
        /// </summary>
        /// <param name="list_MdbPath"></param>
        /// <param name="sExportFFTMdbFile"></param>
        public static void ClipTheFramedMap(List<string> list_MdbPath, string sExportFFTMdbFile, List<string> pListNotTansLayer, string sFFTLayerName,ref List<string> pListErr)
        {
            IWorkspace pTempWks = null;
            try
            {

                //删除临时文件
                if (System.IO.File.Exists(m_TemMdbFile))
                {
                    System.IO.File.Delete(m_TemMdbFile);
                }
                if (list_MdbPath == null || list_MdbPath.Count.Equals(0) || string.IsNullOrWhiteSpace(m_TemMdbFile))
                {
                    return;
                }
                ///创建临时工作空间
                pTempWks = WorkspaceComm.CreateAccessWorkspace(m_TemMdbFile);
                if (pTempWks == null)
                {
                    return;
                }

                ///遍历每个分幅mdb数据，将其中数据合并到一个新建的（上面所建）临时mdb中
                LoopEachFramedMdbAndUnionDataToOneTempMdb(list_MdbPath, pListNotTansLayer, pTempWks);

                ///遍历每个分幅要素，找到和其相切的要素，形成新的MDB
                LoopFFTFeatureAndCreateNewMdb(list_MdbPath, sExportFFTMdbFile, pTempWks, pListNotTansLayer, sFFTLayerName, ref pListErr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pTempWks);
            }
        }

        /// <summary>
        /// 切割形成新的分幅图
        /// </summary>
        /// <param name="list_MdbPath"></param>
        /// <param name="sExportFFTMdbFile"></param>
        public static void ClipTheFramedMap(List<string> list_MdbPath, string sExportFFTMdbFile, List<string> pListNotTansLayer, IEnvelope pEnveope, int iScale)
        {
            try
            {
                //删除临时文件
                if (System.IO.File.Exists(m_TemMdbFile))
                {
                    System.IO.File.Delete(m_TemMdbFile);
                }
                if (list_MdbPath == null || list_MdbPath.Count.Equals(0) || string.IsNullOrWhiteSpace(m_TemMdbFile))
                {
                    return;
                }
                ///创建临时工作空间
                IWorkspace pTempWks = WorkspaceComm.CreateAccessWorkspace(m_TemMdbFile);
                if (pTempWks == null)
                {
                    return;
                }

                ///遍历每个分幅mdb数据，将其中数据合并到一个新建的（上面所建）临时mdb中
                LoopEachFramedMdbAndUnionDataToOneTempMdb(list_MdbPath, pListNotTansLayer, pTempWks);
                //MapIndexComm.CreateMapIndex
                ///遍历每个分幅要素，找到和其相切的要素，形成新的MDB
                //LoopFFTFeatureAndCreateNewMdb(sExportFFTMdbFile, pTempWks, pListNotTansLayer, sFFTLayerName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 遍历每个分幅要素，找到和其相切的要素，形成新的MDB
        /// </summary>
        /// <param name="sExportFFTMdbFile"></param>
        /// <param name="pTempWks"></param>
        private static void LoopFFTFeatureAndCreateNewMdb(List<string> list_MdbPath, string sExportFFTMdbFile, IWorkspace pTempWks, List<string> pListNotTansLayer, string sFFTLayerName, ref List<string> pListErr)
        {
            IWorkspace pSourceWorskspace = null;
            IWorkspace pTatWorkspace = null;
            try
            {

                //判断是否有不需要裁剪和补充的图层
                bool bHaveNotTansLayer = false;
                if (pListNotTansLayer == null || pListNotTansLayer.Count > 0)
                {
                    bHaveNotTansLayer = true;
                }

                ///遍历每一个分幅图形，获取其中包含的要素
                if (list_MdbPath == null || list_MdbPath.Count > 0)
                {
                    foreach (string sSourceMDB in list_MdbPath)
                    {
                        pSourceWorskspace = WorkspaceComm.OpenAccessWorkspace(sSourceMDB);
                        if (pSourceWorskspace == null)
                        {
                            continue;
                        }
                        try
                        {
                            IFeatureClass pFFfeatureClass = FeatureClassComm.GetFeatureClass(pSourceWorskspace, sFFTLayerName);
                            if (pFFfeatureClass == null)
                            {
                                string sErr = "数据源《" + sSourceMDB + "》缺少图廓图层，无法完成裁剪补充操作！";
                                pListErr.Add(sErr);
                                continue;
                            }
                            //获取图廓要素
                            List<IFeature> pListFeature = FeatureComm.GetFeatures(pFFfeatureClass, "");
                            if (pListFeature == null || pListFeature.Count == 0)
                            {
                                string sErr = "数据源《" + sSourceMDB + "》缺少图廓要素，无法完成裁剪补充操作！";
                                pListErr.Add(sErr);
                                continue;
                            }
                            IFeature pTFFeature = pListFeature[0];
                            if (pTFFeature == null || GeometryComm.CheckIsNullOrEmpty(pTFFeature.ShapeCopy))
                            {
                                string sErr = "数据源《" + sSourceMDB + "》缺少图廓要素，无法完成裁剪补充操作！";
                                pListErr.Add(sErr);
                                continue;
                            }
                            IGeometry pTFGeometry = pTFFeature.ShapeCopy;

                            string sTartPath = sExportFFTMdbFile + "\\" + System.IO.Path.GetFileName(sSourceMDB);
                            if (File.Exists(sTartPath))
                            {
                                File.Delete(sTartPath);
                            }
                            pTatWorkspace = WorkspaceComm.CreateAccessWorkspace(sTartPath);
                            if (pTatWorkspace == null)
                            {
                                string sErr = "数据源《" + sSourceMDB + "》裁剪补充失败！";
                                pListErr.Add(sErr);
                            }

                            //复制不需要裁剪的图层
                            if (bHaveNotTansLayer)
                            {
                                foreach (string sNotTranLayer in pListNotTansLayer)
                                {
                                    if (string.IsNullOrWhiteSpace(sNotTranLayer))
                                    {
                                        continue;
                                    }
                                    IFeatureClass pFeatureClass = FeatureClassComm.GetFeatureClass(pSourceWorskspace, sNotTranLayer);
                                    if (pFeatureClass != null)
                                    {
                                        IFeatureClass pTargetFeatureClass = MDBCretreFeatureClass(pFeatureClass, pTatWorkspace);
                                        CopyFeatureClass(pFeatureClass, pTargetFeatureClass);
                                    }
                                }
                            }

                            List<IDatasetName> pListTempDatasetName = WorkspaceComm.GetAllFeatureClass(pTempWks, null);
                            foreach (IDatasetName pDatasetName in pListTempDatasetName)
                            {
                                IName pName = pDatasetName as IName;
                                if (pName == null)
                                {
                                    continue;
                                }
                                IFeatureClass pTmpFeatureClass = pName.Open() as IFeatureClass;
                                if (pTmpFeatureClass == null)
                                {
                                    continue;
                                }
                                IFeatureClass pTargetFeature = MDBCretreFeatureClass(pTmpFeatureClass, pTatWorkspace);
                                if (pTargetFeature == null)
                                {
                                    continue;
                                }
                                FeatureClassComm.CutFeatureClassData(pTmpFeatureClass, pTFGeometry, pTargetFeature);
                            }
                        }
                        catch (Exception)
                        {
                            string sErr = "数据源《" + sSourceMDB + "》裁剪补充过程发生错误，无法完成裁剪补充操作！";
                            pListErr.Add(sErr);
                        }


                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 遍历每个分幅mdb数据，将其中数据合并到一个新建的（上面所建）临时mdb中
        /// </summary>
        /// <param name="list_MdbPath"></param>
        /// <param name="pTempWks"></param>
        private static void LoopEachFramedMdbAndUnionDataToOneTempMdb(List<string> list_MdbPath, List<string> pListNotTansLayer, IWorkspace pTempWks)
        {
            IDataset pInDataset = null;
            IDataset pDataset1 = null;
            IEnumDataset pInDatasets = null;
            try
            {

                //判断是否有不需要转换的图层
                bool bHaveNotConvertLayer = false;
                if (pListNotTansLayer != null && pListNotTansLayer.Count > 0)
                {
                    bHaveNotConvertLayer = true;
                }

                ///遍历获取要素并插入已有的一个mdb中
                IWorkspace pEachWks = null;
                foreach (string pPathMDB in list_MdbPath)
                {
                    pEachWks = WorkspaceComm.OpenAccessWorkspace(pPathMDB);
                    if (pEachWks == null)
                    {
                        continue;
                    }
                    ///遍历FeatureClass
                    IEnumDataset pDatasets = pEachWks.get_Datasets(esriDatasetType.esriDTFeatureClass);
                    for (IDataset pDataset = pDatasets.Next(); pDataset != null; pDataset = pDatasets.Next())
                    {
                        IFeatureClass pfeaCls = pDataset as IFeatureClass;
                        string sFeaClassName = pfeaCls.AliasName;
                        //图层是方里网、注记整饰则不处理
                        //排除不需要转换的图层
                        if (bHaveNotConvertLayer)
                        {
                            string sLayerName = FeatureClassComm.GetSuffixName(pfeaCls);
                            if (pListNotTansLayer.Contains(sLayerName))
                            {
                                continue;
                            }
                        }

                        ///先检查是否有该图层，如果没有则创建
                        IFeatureClass tfeaCls = FeatureClassComm.GetFeatureClass(pTempWks, sFeaClassName);
                        if (tfeaCls == null)
                        {
                            ///创建新的FeatureClass
                            tfeaCls = WorkspaceComm.CreateFeatureClass(pfeaCls, pTempWks, pDataset.Name);
                            (tfeaCls as IClassSchemaEdit).AlterAliasName(pfeaCls.AliasName);
                        }
                        if (tfeaCls == null)
                        {
                            continue;
                        }

                        ///将要素合并到唯一的图层中
                        InSertFeatureFromOneFeatureClassToAnExistFeatureClass(pfeaCls, tfeaCls);
                    }

                    AEComm.ReleaseCOMObject(pDatasets);

                    //// 遍历原数据的所有数据集
                    pDatasets = pEachWks.get_Datasets(esriDatasetType.esriDTFeatureDataset);
                    pDataset1 = pDatasets.Next();
                    while (pDataset1 != null)
                    {
                        IFeatureDataset tDataset = (pTempWks as IFeatureWorkspace).CreateFeatureDataset(pDataset1.Name, (pDataset1 as IGeoDataset).SpatialReference);
                        //// 遍历数据集中的数据
                        IFeatureDataset pFD = pDataset1 as IFeatureDataset;
                        if (pFD == null)
                        {
                            pDataset1 = pDatasets.Next();
                            continue;
                        }
                        pInDatasets = pFD.Subsets;
                        pInDataset = pInDatasets.Next();
                        while (pInDataset != null)
                        {
                            try
                            {
                                IFeatureClass pfeaCls = pInDataset as IFeatureClass;
                                if (pfeaCls == null)
                                {
                                    pInDataset = pInDatasets.Next();
                                    continue;
                                }

                                IFeatureClass tfeaCls = WorkspaceComm.CreateFeatureClass(pfeaCls, tDataset);
                                if (tfeaCls == null)
                                {
                                    pInDataset = pInDatasets.Next();
                                    continue;
                                }

                                //排除不需要转换的图层
                                if (bHaveNotConvertLayer)
                                {
                                    string sLayerName = FeatureClassComm.GetSuffixName(pfeaCls);
                                    if (pListNotTansLayer.Contains(sLayerName))
                                    {
                                        pInDataset = pInDatasets.Next();
                                        continue;
                                    }
                                }

                                ///将要素合并到唯一的图层中
                                InSertFeatureFromOneFeatureClassToAnExistFeatureClass(pfeaCls, tfeaCls);

                                pInDataset = pInDatasets.Next();
                            }
                            catch (Exception ex)
                            {
                                pInDataset = pInDatasets.Next();
                                break;
                            }

                        }

                        pDataset1 = pDatasets.Next();

                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将要素合并到唯一的图层中
        /// </summary>
        /// <param name="pSourcefeaCls"></param>
        /// <param name="tfeaCls"></param>
        private static void InSertFeatureFromOneFeatureClassToAnExistFeatureClass(IFeatureClass pSourcefeaCls, IFeatureClass tfeaCls)
        {
            IFeatureBuffer featureBuffer = null;
            IFeatureCursor insertCursor = null;
            IFeatureCursor pFeaCursor = null;
            try
            {

                if (pSourcefeaCls == null || tfeaCls == null)
                {
                    return;
                }
                bool isAnnoFeatureType = false;//判断是否是注记图层
                if (pSourcefeaCls.FeatureType == tfeaCls.FeatureType &&
                    (pSourcefeaCls.FeatureType == esriFeatureType.esriFTAnnotation ||
                    pSourcefeaCls.FeatureType == esriFeatureType.esriFTCoverageAnnotation))
                {
                    isAnnoFeatureType = true;
                }
                pFeaCursor = FeatureClassComm.GetFeatureCursor(pSourcefeaCls, "", false);
                insertCursor = tfeaCls.Insert(true);
                IFeature pFea = pFeaCursor.NextFeature();
                int iCount = 0;
                while (pFea != null)
                {
                    // Create the feature buffer.
                    featureBuffer = tfeaCls.CreateFeatureBuffer();
                    featureBuffer.Shape = pFea.ShapeCopy;
                    FeatureComm.CopyFieldValue(pFea, ref featureBuffer);
                    if (isAnnoFeatureType)
                    {
                        AnnoComm.CopyAnnotationFeature(featureBuffer, pFea);
                    }
                    insertCursor.InsertFeature(featureBuffer);
                    AEComm.ReleaseCOMObject(featureBuffer);
                    iCount++;
                    if (iCount % 1000 == 0)
                    {
                        insertCursor.Flush();
                    }
                    pFea = pFeaCursor.NextFeature();
                }
                if (insertCursor != null)
                {
                    insertCursor.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
                // Handle the failure in a way appropriate to the application.
            }
            finally
            {
                AEComm.ReleaseCOMObject(insertCursor);
                AEComm.ReleaseCOMObject(pFeaCursor);
                AEComm.ReleaseCOMObject(featureBuffer);
            }
        }

        /// <summary>
        /// 根据原图层创建图层
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="pTargetWorkspace"></param>
        /// <returns></returns>
        public static IFeatureClass MDBCretreFeatureClass(IFeatureClass pSourceFeatureClass, IWorkspace pTargetWorkspace)
        {
            IFeatureClass pCreatFeatureClass = null;
            try
            {
                if (pSourceFeatureClass == null || pTargetWorkspace == null)
                {
                    return pCreatFeatureClass;
                }
                pCreatFeatureClass = FeatureClassComm.GetFeatureClass(pTargetWorkspace, FeatureClassComm.GetSuffixName(pSourceFeatureClass));
                if (pCreatFeatureClass!=null)
                {
                    return pCreatFeatureClass;
                }
                if (pSourceFeatureClass.FeatureDataset != null)
                {
                    IFeatureDataset pFeatureDaset=FeatureDatasetComm.GetDataset(pTargetWorkspace, pSourceFeatureClass.FeatureDataset.Name);
                    if (pFeatureDaset==null)
                    {
                        pFeatureDaset = (pTargetWorkspace as IFeatureWorkspace).CreateFeatureDataset(pSourceFeatureClass.FeatureDataset.Name, (pSourceFeatureClass as IGeoDataset).SpatialReference);
                    } 
                    pCreatFeatureClass = WorkspaceComm.CreateFeatureClass(pSourceFeatureClass, pFeatureDaset);
                }
                else
                {
                    pCreatFeatureClass = WorkspaceComm.CreateFeatureClass(pSourceFeatureClass, pTargetWorkspace, FeatureClassComm.GetSuffixName(pSourceFeatureClass));
                }
            }
            catch (Exception)
            {
            }
            return pCreatFeatureClass;
        }

        /// <summary>
        /// 复制图层到另外一个图层中
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="pTatgetFeatureClass"></param>
        /// <returns></returns>
        public static bool CopyFeatureClass(IFeatureClass pSourceFeatureClass, IFeatureClass pTatgetFeatureClass)
        {
            bool isSuccess = false;
            IFeatureCursor pInsertCursor = null;
            IFeatureCursor pSerachCursor = null;
            IFeatureBuffer pInsertFeatureBuffer = null;
            try
            {
                if (pSourceFeatureClass == null || pTatgetFeatureClass == null)
                {
                    return isSuccess;
                }
                if (pSourceFeatureClass.ShapeType!=pTatgetFeatureClass.ShapeType)
                {
                    return isSuccess;
                }
                bool isAnnoFeatureType = false;//判断是否是注记图层
                if (pSourceFeatureClass.FeatureType==pTatgetFeatureClass.FeatureType&&
                    (pSourceFeatureClass.FeatureType==esriFeatureType.esriFTAnnotation||
                    pSourceFeatureClass.FeatureType==esriFeatureType.esriFTCoverageAnnotation))
                {
                    isAnnoFeatureType = true;
                }
                pSerachCursor = pSourceFeatureClass.Search(null, true);
                pInsertCursor = pTatgetFeatureClass.Insert(true);
                IFeature pSourceFeature = pSerachCursor.NextFeature();
                int iCount = 0;
                while (pSourceFeature != null)
                {
                    pInsertFeatureBuffer = pTatgetFeatureClass.CreateFeatureBuffer();
                    FeatureComm.CopyFieldValue(pSourceFeature, ref pInsertFeatureBuffer);
                    pInsertFeatureBuffer.Shape = pSourceFeature.ShapeCopy;
                    if (isAnnoFeatureType)
                    {
                        AnnoComm.CopyAnnotationFeature(pInsertFeatureBuffer, pSourceFeature);
                    }
                    pInsertCursor.InsertFeature(pInsertFeatureBuffer);
                    iCount++;
                    if (iCount % 1000 == 0)
                    {
                        pInsertCursor.Flush();
                    }
                    AEComm.ReleaseCOMObject(pInsertFeatureBuffer);
                    pSourceFeature = pSerachCursor.NextFeature();
                }
                if (pInsertCursor != null)
                {
                    pInsertCursor.Flush();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInsertCursor);
                AEComm.ReleaseCOMObject(pSerachCursor);
            }
            return isSuccess;
        }
    }
}
