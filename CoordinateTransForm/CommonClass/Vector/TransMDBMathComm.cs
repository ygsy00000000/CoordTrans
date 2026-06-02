using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.Framework.CommonModule;
using ZJUGIS.Framework.Dev;
using ZJUGIS.GIS.CommonMethod;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 转换MDB数据库文件坐标系
    /// </summary>
    public class TransMDBMathComm
    {
        /// <summary>
        /// Mdb,Gdb空间数据转换
        /// </summary>
        /// <param name="sSourceFile"></param>
        /// <param name="sTargetFile"></param>
        /// <param name="sErrors"></param>
        /// <returns></returns>
        public static bool ConvertMdb(string sSourceFile, string sTargetFile, TCoordinate pTClass, List<string> listNotConvetLayerName, ref List<string> sErrors, IProgressDialog pProgressDialog = null)
        {
            bool flag = false;
            IWorkspace pWks = null;
            IWorkspace tWks = null;
            IEnumDataset pInDatasets = null;
            IEnumDataset pDatasets = null;
            IDataset pDataset = null;
            IDataset pInDataset = null;
            bool bMDB = false;

            try
            {
                string ext = System.IO.Path.GetExtension(sSourceFile).ToLower();
                if (ext.Equals(".mdb"))
                {
                    bMDB = true;
                    pWks = WorkspaceComm.OpenAccessWorkspace(sSourceFile);
                }
                else
                {
                    pWks = WorkspaceComm.OpenFileGDBWorkspace(sSourceFile);
                }

                if (pWks == null)
                {
                    string sError = "原数据工作空间获取失败！";
                    sErrors.Add(sError);
                    return flag;
                }

                ext = System.IO.Path.GetExtension(sTargetFile).ToLower();
                if (ext.Equals(".gdb"))
                {
                    if (Directory.Exists(sTargetFile))
                    {
                        try
                        {
                            Directory.Delete(sTargetFile);
                        }
                        catch (Exception ex)
                        {
                            string sError = string.Format("数据库文件【{0}】正在被占用，无法删除", System.IO.Path.GetFullPath(sTargetFile));
                            sErrors.Add(sError);
                            return flag;
                        }
                    }
                }
                else
                {
                    if (System.IO.File.Exists(sTargetFile))
                    {
                        try
                        {
                            File.Delete(sTargetFile);
                        }
                        catch (Exception ex)
                        {
                            string sError = string.Format("数据库文件【{0}】正在被占用，无法删除", System.IO.Path.GetFullPath(sTargetFile));
                            sErrors.Add(sError);
                            return flag;
                        }
                    }
                }
                

                if (bMDB)
                {
                    tWks = WorkspaceComm.CreateAccessWorkspace(sTargetFile);
                }
                else
                {
                    tWks = WorkspaceComm.CreateGDBWorkspace(sTargetFile);
                }

                //判断是否有不需要转换的图层
                bool bHaveNotConvertLayer = false;
                if (listNotConvetLayerName != null && listNotConvetLayerName.Count > 0)
                {
                    bHaveNotConvertLayer = true;
                }

                ISpatialReference pSpr = pTClass.GetSpatialReference();

                List<IDatasetName> pDatasetNames = WorkspaceComm.GetAllFeatureClass(pWks,string.Empty);
                
                if (pProgressDialog != null)
                {
                    pProgressDialog.Max = pDatasetNames.Count;
                    pProgressDialog.Min = 0;
                    pProgressDialog.Message = "空间数据转换";
                    pProgressDialog.Show();
                    pProgressDialog.Position = 0;
                }
                
                int iCount = 0;
                //// 遍历原数据的FeatureClass
                pDatasets = pWks.get_Datasets(esriDatasetType.esriDTFeatureClass);
                pDataset = pDatasets.Next();
                while (pDataset != null)
                {
                    try
                    {
                        IFeatureClass pfeaCls = pDataset as IFeatureClass;
                        if (pfeaCls == null)
                        {
                            string sError = string.Format("原图层：{0}, 获取失败！", pDataset.Name);
                            sErrors.Add(sError);
                            pDataset = pDatasets.Next();
                            continue;
                        }

                        iCount++;
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Position = iCount - 1;
                            pProgressDialog.Description = string.Format("正在转换图层【{0}】", pfeaCls.AliasName);
                            pProgressDialog.Step(1);
                        }

                        //// 判断如果不是未知坐标系，则判断空间参考一致性
                        if (!GISComm.Judge(pfeaCls) && !pTClass.JudgeSpatialReference(pfeaCls))
                        {
                            pDataset = pDatasets.Next();
                            string sError = string.Format("原图层：{0}, 原图层的空间参考与当前转换功能不一致！", pDataset.Name);
                            sErrors.Add(sError);
                            continue;
                        }

                        //// 创建新图层
                        IFeatureClass tfeaCls = WorkspaceComm.CreateFeatureClass(pfeaCls, tWks, pSpr);
                        if (tfeaCls == null)
                        {
                            pDataset = pDatasets.Next();
                            string sError = string.Format("新图层：{0}, 创建失败！", pDataset.Name);
                            sErrors.Add(sError);
                            continue;
                        }

                        //排除不需要转换的图层
                        if (bHaveNotConvertLayer)
                        {
                            string sLayerName = FeatureClassComm.GetSuffixName(pfeaCls);
                            if (listNotConvetLayerName.Contains(sLayerName))
                            {
                                pDataset = pDatasets.Next();
                                continue;
                            }
                        }
                       // (tfeaCls as IClassSchemaEdit).AlterAliasName(pfeaCls.AliasName);
                        //// 新的要素类赋值新的空间参考
                        //if (!pTClass.SetSpatialReference(tfeaCls))
                        //{
                        //    pDataset = pDatasets.Next();
                        //    string sError = string.Format("新图层：{0}, 设置空间参考失败！", pDataset.Name);
                        //    sErrors.Add(sError);
                        //}
                        //// 开始数据转换
                        if (GISComm.FeatureClassToFeatureClass(pTClass, pfeaCls, tfeaCls, ref sErrors, pProgressDialog))
                        {
                            flag = true;
                        }

                    }
                    catch (Exception ex)
                    {
                        string sError = string.Format("图层：{0}, 出现错误：{1}", pDataset.Name, ex.Message);
                        sErrors.Add(sError);
                    }

                    pDataset = pDatasets.Next();
                }

                AEComm.ReleaseCOMObject(pDatasets);

                //// 遍历原数据的所有数据集
                pDatasets = pWks.get_Datasets(esriDatasetType.esriDTFeatureDataset);
                pDataset = pDatasets.Next();
                while (pDataset != null)
                {
                    IFeatureDataset tDataset = (tWks as IFeatureWorkspace).CreateFeatureDataset(pDataset.Name, pSpr);
                    //// 遍历数据集中的数据
                    IFeatureDataset pFD = pDataset as IFeatureDataset;
                    if (pFD == null)
                    {
                        pDataset = pDatasets.Next();
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
                                string sError = string.Format("原图层：{0}, 获取失败！", pInDataset.Name);
                                sErrors.Add(sError);
                                continue;
                            }

                            iCount++;
                            if (pProgressDialog != null)
                            {
                                pProgressDialog.Position = iCount - 1;
                                pProgressDialog.Description = string.Format("正在转换图层【{0}】", pfeaCls.AliasName);
                                pProgressDialog.Step(1);
                            }

                            //// 判断如果不是未知坐标系，则判断空间参考一致性
                            if (!GISComm.Judge(pfeaCls) && !pTClass.JudgeSpatialReference(pfeaCls))
                            {
                                pInDataset = pInDatasets.Next();
                                string sError = string.Format("原图层：{0}, 原图层的空间参考与当前转换功能不一致！", pInDataset.Name);
                                sErrors.Add(sError);
                                continue;
                            }
                            IFeatureClass tfeaCls = WorkspaceComm.CreateFeatureClass(pfeaCls, tDataset);
                            if (tfeaCls == null)
                            {
                                pInDataset = pInDatasets.Next();
                                string sError = string.Format("新图层：{0}, 创建失败！", pInDataset.Name);
                                sErrors.Add(sError);
                                continue;
                            }

                            //排除不需要转换的图层
                            if (bHaveNotConvertLayer)
                            {
                                string sLayerName = FeatureClassComm.GetSuffixName(pfeaCls);
                                if (listNotConvetLayerName.Contains(sLayerName))
                                {
                                    pInDataset = pInDatasets.Next();
                                    continue;
                                }
                            }
                            //FeatureClassComm.AlterAliasName(tfeaCls, pfeaCls.AliasName);
                            //// 新的要素类赋值新的空间参考
                            //if (!pTClass.SetSpatialReference(tfeaCls))
                            //{
                            //    string sError = string.Format("新图层：{0}, 设置空间参考失败！", pInDataset.Name);
                            //    sErrors.Add(sError);
                            //}
                            //// 开始数据转换
                            if (GISComm.FeatureClassToFeatureClass(pTClass, pfeaCls, tfeaCls, ref sErrors, pProgressDialog))
                            {
                                flag = true;
                            }

                            pInDataset = pInDatasets.Next();
                        }
                        catch (Exception ex)
                        {
                            string sError = string.Format("图层：{0}, 出现错误：{1}", pInDataset.Name, ex.Message);
                            sErrors.Add(sError);
                            pInDataset = pInDatasets.Next();
                            break;
                        }

                    }

                    pDataset = pDatasets.Next();

                }

            }
            catch (Exception ex)
            {
                DevMessageBox.ShowInformation("转换出错：" + ex.Message);
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInDatasets);
                AEComm.ReleaseCOMObject(pDatasets);
                AEComm.ReleaseCOMObject(pDataset);
                AEComm.ReleaseCOMObject(pInDataset);
                AEComm.ReleaseCOMObject(pWks);
                AEComm.ReleaseCOMObject(tWks);
            }

            return flag;
        }
    }
}
