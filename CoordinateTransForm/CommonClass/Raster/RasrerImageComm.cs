using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.Framework.Dev;

namespace ZJUGIS.CoordinateTrans.CommonClass.Raster
{
   /// <summary>
   /// 影像数据公共方法
   /// </summary>
    public class RasrerImageComm
    {
        private const string m_Log = "栅格转换";
        /// <summary>
        /// 通过文件获取栅格数据
        /// </summary>
        /// <param name="sRasterLayer"></param>
        /// <returns></returns>
        public static IRasterLayer GetRasterLayerFromFile(string sRasterLayer)
        {
            IRasterLayer pRasterLayer = new RasterLayerClass();

            try
            {
                if (string.IsNullOrWhiteSpace(sRasterLayer))
                {
                    DevMessageBox.ShowInformation("栅格数据文件不存在");
                    return null;
                }
                if (!File.Exists(sRasterLayer))
                {
                    DevMessageBox.ShowInformation("栅格数据文件不存在");
                    return null;
                }

                pRasterLayer.CreateFromFilePath(sRasterLayer);
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(m_Log, ex.Message, false);
            }

            return pRasterLayer;
        }

        /// <summary>
        /// 获取栅格数据的空间参考
        /// </summary>
        /// <param name="sFile"></param>
        public static ISpatialReference RefreshSourceSpatialReference(string sFile)
        {
            ISpatialReference pSr = null;
            try
            {
                IRasterLayer pRasterLayer = new RasterLayerClass();
                pRasterLayer.CreateFromFilePath(sFile);
                if (pRasterLayer == null)
                {
                    return pSr;
                }
                IRaster pRaster = pRasterLayer.Raster;
                if (pRaster != null)
                {
                    IRasterProps rasterProps = pRaster as IRasterProps;
                    if (rasterProps == null)
                    {
                        return pSr;
                    }
                    pSr = rasterProps.SpatialReference;

                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(m_Log, ex.Message, false);
            }
            return pSr;
        }


        /// <summary>
        /// 生成坐标文件
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <returns></returns>
        public static bool RegistRaster(string sFilePath)
        {
            bool bIsOk = false;
            try
            {
                //获取栅格图层
                IRasterLayer pRasterLayer = new RasterLayerClass();
                pRasterLayer.CreateFromFilePath(sFilePath);
                if (pRasterLayer == null)
                {
                    return false;
                }
                ///获取栅格属性接口
                IRaster raster = pRasterLayer.Raster;
                IRasterProps rasterProps = raster as IRasterProps;
                if (rasterProps == null)
                {
                    return false;
                }
                IGeoReference pGeoRef = pRasterLayer as IGeoReference;
                pGeoRef.Register();
                bIsOk = true;
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(m_Log, ex.Message, false);
            }
            return bIsOk;
        }

        /// <summary>
        /// 修改TWF文件
        /// </summary>
        /// <param name="dicTFW"></param>
        public static void RepairWTF(Dictionary<string, string> dicTFW)
        {
            try
            {
                RasterTFWCls pRasterTFWCls = new RasterTFWCls();
                ///TFW文件中
                string sFileTFW = string.Empty;
                if (dicTFW.ContainsKey(".tfw"))
                {
                    dicTFW.TryGetValue(".tfw", out sFileTFW);
                    using (FileStream fs = new FileStream(sFileTFW, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Lock(0, fs.Length);
                        StreamReader sr = new StreamReader(fs, Encoding.Default);
                        pRasterTFWCls.A = sr.ReadLine();
                        pRasterTFWCls.D = sr.ReadLine();
                        pRasterTFWCls.B = sr.ReadLine();
                        pRasterTFWCls.E = sr.ReadLine();
                        fs.Unlock(0, fs.Length);//一定要用在Flush()方法以前，否则抛出异常。
                        fs.Flush();
                    }
                    ///删除文件
                    File.Delete(sFileTFW);
                }
                //
                if (dicTFW.ContainsKey(".tfwx"))
                {
                    string sFileTFWX = string.Empty;
                    dicTFW.TryGetValue(".tfwx", out sFileTFWX);

                    using (FileStream fs = new FileStream(sFileTFWX, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Lock(0, fs.Length);
                        StreamReader sr = new StreamReader(fs, Encoding.Default);
                        sr.ReadLine();
                        sr.ReadLine();
                        sr.ReadLine();
                        sr.ReadLine();
                        string sX = sr.ReadLine();
                        string sY = sr.ReadLine();
                        pRasterTFWCls.C = sX;
                        pRasterTFWCls.F = sY;
                        fs.Unlock(0, fs.Length);//一定要用在Flush()方法以前，否则抛出异常。
                        fs.Flush();
                    }
                    ///删除坐标文件
                    File.Delete(sFileTFWX);
                    //string ssFile = System.IO.Path.GetDirectoryName(sFileTFWX) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sFileTFWX) + ".tfw";
                    //System.IO.File.Move(sFileTFWX, ssFile);
                }
                using (FileStream fs = new FileStream(sFileTFW, FileMode.Create, FileAccess.Write))
                {
                    fs.Lock(0, fs.Length);
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sw.WriteLine(pRasterTFWCls.A);
                    sw.WriteLine(pRasterTFWCls.D);
                    sw.WriteLine(pRasterTFWCls.B);
                    sw.WriteLine(pRasterTFWCls.E);
                    sw.WriteLine(pRasterTFWCls.C);
                    sw.WriteLine(pRasterTFWCls.F);

                    fs.Unlock(0, fs.Length);//一定要用在Flush()方法以前，否则抛出异常。
                    sw.Flush();
                    fs.Flush();
                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog("影像转换", ex.Message, false);
            }
        }

        /// <summary>
        /// Adds the display cross.
        /// </summary>
        /// <param name="pMap">The p map.</param>
        /// <param name="pPnt">The p PNT.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IMarkerElement AddDisplayCross(IMap pMap, IPoint pPnt, int color)
        {
            if (pMap == null || pPnt == null)
            {
                return null;
            }
            IMarkerElement pMarkerEle;
            ISimpleMarkerSymbol pMarkerSymbol;

            IColor pColor;
            pMarkerEle = new MarkerElementClass();
            pMarkerSymbol = new SimpleMarkerSymbolClass();
            pColor = new RgbColorClass();
            try
            {
                if (color == 0)
                {
                    pColor = GetRgbColor(0, 255, 0);
                }
                else
                {
                    pColor = GetRgbColor(255, 0, 0);
                }
                pMarkerSymbol = SetSimpleMarkSymbol(pMarkerSymbol, pColor, 10, esriSimpleMarkerStyle.esriSMSCross);

                pMarkerEle.Symbol = pMarkerSymbol;
                (pMarkerEle as IElement).Geometry = pPnt;
                (pMap as IGraphicsContainer).AddElement(pMarkerEle as IElement, 0);
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(m_Log, ex);
            }

            return pMarkerEle;
        }

        /// <summary>
        /// 设置简单SimpleMarkerSymbol
        /// </summary>
        /// <param name="pMarkerSymbol"></param>
        /// <param name="pColor"></param>
        /// <param name="width"></param>
        /// <param name="pesriSimpleMarkerStyle"></param>
        /// <returns></returns>
        public static ISimpleMarkerSymbol SetSimpleMarkSymbol(ISimpleMarkerSymbol pMarkerSymbol,
                                                                IColor pColor,
                                                                double width,
                                                                esriSimpleMarkerStyle pesriSimpleMarkerStyle)
        {
            try
            {
                pMarkerSymbol.Color = pColor;
                pMarkerSymbol.Size = width;
                pMarkerSymbol.Style = pesriSimpleMarkerStyle;
            }
            catch (Exception ex)
            {
                LogComm.WriteLog("SetSimpleMarkSymbol", ex);
            }
            return pMarkerSymbol;
        }

        /// <summary>
        /// 获取RGB颜色
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IColor GetRgbColor(int r, int g, int b)
        {
            IColor pColor = null;
            try
            {
                pColor = new RgbColorClass();
                (pColor as IRgbColor).Red = r;
                (pColor as IRgbColor).Green = g;
                (pColor as IRgbColor).Blue = b;
            }
            catch (Exception ex)
            {
            }
            return pColor;
        }
    }
}
