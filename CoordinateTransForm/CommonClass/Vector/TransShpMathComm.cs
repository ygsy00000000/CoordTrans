using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZJUGIS.Framework.CommonModule;
using ZJUGIS.Framework.Dev;
using ZJUGIS.GIS.CommonMethod;
using ZJUGIS.GISModule.CommonMethod;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 转换shp文件
    /// </summary>
    public class TransShpMathComm
    {
        /// <summary>
        /// shp数据转换
        /// </summary>
        /// <param name="sSourceFile"></param>
        /// <param name="sTargetFile"></param>
        /// <param name="sErrors"></param>
        /// <param name="sLogName"></param>
        /// <returns></returns>
        public static bool ConvertShp(string sSourceFile, string sTargetFile, TCoordinate pTClass, ref List<string> sErrors, IProgressDialog pProgressDialog = null)
        {
            bool flag = false;
            try
            {
                IFeatureClass pSourcefeaCls = FeatureClassComm.GetShpFeatureClass(sSourceFile);
                if (pSourcefeaCls == null)
                {
                    string sError = string.Format("原图层：{0}, 获取失败！", "");
                    sErrors.Add(sError);
                    return flag;
                }
                //// 判断原数据的空间参考
                IGeoDataset pSourceGeoDs = pSourcefeaCls as IGeoDataset;
                //// 判断空间参考是不是未知
                if (!(pSourceGeoDs.SpatialReference is IUnknownCoordinateSystem))
                {
                    //// 判断空间参考一致性
                    if (!pTClass.JudgeSpatialReference(pSourcefeaCls))
                    {
                        sErrors.Add("原数据的空间参考与当前转换功能不一致，请检查！");
                        return flag;
                    }
                }

                //// 创建新的要素类
                ISpatialReference pSpr = pTClass.GetSpatialReference();
                IFeatureClass pTargetfeaCls = GISComm.CreateShpFeatureClass(sTargetFile, pSourcefeaCls, pSpr);
                if (pTargetfeaCls == null)
                {
                    string sError = string.Format("新图层：{0}, 创建失败！", (pTargetfeaCls as IDataset).Name);
                    sErrors.Add(sError);
                    return flag;
                }
                //// 新的要素类赋值新的空间参考
                //if (!pTClass.SetSpatialReference(pTargetfeaCls))
                //{
                //    string sError = string.Format("新图层：{0}, 设置空间参考失败！", (pTargetfeaCls as IDataset).Name);
                //    sErrors.Add(sError);
                //}
                //// 开始数据转换
                if (pProgressDialog != null)
                {
                    pProgressDialog.Show();
                }

                if (GISComm.FeatureClassToFeatureClass(pTClass, pSourcefeaCls, pTargetfeaCls, ref sErrors, pProgressDialog, true))
                {
                    flag = true;
                }

            }
            catch (Exception ex)
            {
                if (pProgressDialog != null)
                {
                    pProgressDialog.Hide();
                    pProgressDialog = null;
                }
                string sError = string.Format("转换失败：{0}", ex.Message);
            }

            return flag;
        }
    }
}
