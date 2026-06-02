using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.GIS.CommonMethod;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 方里网公共方法类
    /// </summary>
    public class SquareNetComm
    {
        /// <summary>
        /// 根据原方里网创建新的方里网
        /// </summary>
        /// <param name="pSourceQuareNet">原方里网层</param>
        /// <param name="pSpatialReference">新方里网的坐标系</param>
        /// <param name="pTargetWorkSpace">新方里网的空间</param>
        /// <param name="dMinX">图幅最小X坐标</param>
        /// <param name="dMinY">图幅最小Y坐标</param>
        /// <param name="dMaxX">图幅最大X坐标</param>
        /// <param name="dMaxY">图幅最大Y坐标</param>
        /// <param name="iLength">方里网范围，默认是1公里</param>
        /// <returns></returns>
        public static bool CreateSquareNet(IFeatureClass pNewQuareNet, double dMinX, double dMinY, double dMaxX,
                                                    double dMaxY, int iLength = 1000)
        {
            IFeatureCursor pInsertCursor = null;
            try
            {
                if (pNewQuareNet == null)
                {
                    return false;
                }

                double dSquareX = 0.0;//计算的方立网X坐标
                double dSquareY = 0.0;//计算的方里网Y坐标
                double dRemainderX = (double)((decimal)dMinX % (decimal)iLength);//方里网X范围最小值与方里网格长度的余数
                double dRemainderY = (double)((decimal)dMinY % (decimal)iLength);//方里网Y范围最小值与方里网格长度的余数

                //计算给定范围内方里网的最小X坐标
                if (dRemainderX == 0)
                {
                    dSquareX = dMinX;
                }
                else
                {
                    dSquareX = dMinX - dRemainderX + iLength;
                }

                pInsertCursor = pNewQuareNet.Insert(true);
                IFeatureBuffer pFeatureBuffer = pNewQuareNet.CreateFeatureBuffer();
                IPoint pPoint = new PointClass();
                int iCount = 0;
                //生成纵线方里线
                while (dSquareX <= dMaxX)
                {
                    IPolyline pLine = CreateLineForTwoPoint(dSquareX, dMinY, dSquareX, dMaxY);
                    pFeatureBuffer.Shape = pLine;
                    pInsertCursor.InsertFeature(pFeatureBuffer);
                    dSquareX += iLength;
                    //500条压缩一次
                    iCount++;
                    if (iCount % 500 == 0)
                    {
                        pInsertCursor.Flush();
                    }
                }

                //计算给定范围内方里网的最小Y坐标
                if (dRemainderY == 0)
                {
                    dSquareY = dMinY;
                }
                else
                {
                    dSquareY = dMinY - dRemainderY + iLength;
                }

                //生成横线方里线
                while (dSquareY <= dMaxY)
                {
                    IPolyline pLine = CreateLineForTwoPoint(dMinX, dSquareY, dMaxX, dSquareY);
                    pFeatureBuffer.Shape = pLine;
                    pInsertCursor.InsertFeature(pFeatureBuffer);
                    dSquareY += iLength;
                    //500条压缩一次
                    if (iCount % 500 == 0)
                    {
                        pInsertCursor.Flush();
                    }
                }
                if (pInsertCursor != null)
                {
                    pInsertCursor.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInsertCursor);
            }
        }

        /// <summary>
        /// 根据原方里网创建新的方里网,带有图幅框
        /// </summary>
        /// <param name="pSourceQuareNet">原方里网层</param>
        /// <param name="pNewAnoFeatureClass">新注记层</param>
        /// <param name="pSpatialReference">新方里网的坐标系</param>
        /// <param name="pTargetWorkSpace">新方里网的空间</param>
        /// <param name="dMinX">图幅最小X坐标</param>
        /// <param name="dMinY">图幅最小Y坐标</param>
        /// <param name="dMaxX">图幅最大X坐标</param>
        /// <param name="dMaxY">图幅最大Y坐标</param>
        /// <param name="iLength">方里网范围，默认是1公里</param>
        /// <returns></returns>
        public static bool CreateSquareNet(IFeatureClass pNewQuareNet, string sTFBH, EarthParams pEarth,
                                            double dAddY, double dCentralMeridian, int iLength = 1000)
        {
            IFeatureCursor pInsertCursor = null;
            try
            {
                double dMinX, dMinY, dMaxX, dMaxY;
                double minLat = 0.0, minLon = 0.0, maxLat = 0.0, maxLon = 0.0;
                if (pNewQuareNet == null || !TFComm.CheckTFHIsValid(sTFBH))
                {
                    return false;
                }

                TFComm.GetFourDSFromTFH(sTFBH, ref minLon, ref minLat, ref maxLon, ref maxLat);

                IGeometry pAreaGeo = TFComm.CreateTFPolygon(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //创建边框线
                IGeometry pGeo = TFComm.CreateTFPolline(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //根据图幅框的外边距生成方里网
                IEnvelope pEnvelope = pAreaGeo.Envelope;
                dMinX = pEnvelope.XMin;
                dMinY = pEnvelope.YMin;
                dMaxX = pEnvelope.XMax;
                dMaxY = pEnvelope.YMax;

                double dSquareX = 0.0;//计算的方立网X坐标
                double dSquareY = 0.0;//计算的方里网Y坐标
                double dRemainderX = (double)((decimal)dMinX % (decimal)iLength);//方里网X范围最小值与方里网格长度的余数
                double dRemainderY = (double)((decimal)dMinY % (decimal)iLength);//方里网Y范围最小值与方里网格长度的余数

                //计算给定范围内方里网的最小X坐标
                if (dRemainderX == 0)
                {
                    dSquareX = dMinX;
                }
                else
                {
                    dSquareX = dMinX - dRemainderX + iLength;
                }

                pInsertCursor = pNewQuareNet.Insert(true);
                IFeatureBuffer pFeatureBuffer = pNewQuareNet.CreateFeatureBuffer();

                pFeatureBuffer.Shape = pGeo;
                pInsertCursor.InsertFeature(pFeatureBuffer);

                //生成边框面

                IPoint pPoint = new PointClass();
                int iCount = 0;
                //生成纵线方里线
                while (dSquareX <= dMaxX)
                {
                    IPolyline pLine = CreateLineForTwoPoint(dSquareX, dMinY, dSquareX, dMaxY);
                    pLine = GeometryComm.GetIntersectGeo(pLine, pAreaGeo, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    pFeatureBuffer.Shape = pLine;
                    pInsertCursor.InsertFeature(pFeatureBuffer);
                    dSquareX += iLength;
                    //500条压缩一次
                    iCount++;
                    if (iCount % 500 == 0)
                    {
                        pInsertCursor.Flush();
                    }
                }

                //计算给定范围内方里网的最小Y坐标
                if (dRemainderY == 0)
                {
                    dSquareY = dMinY;
                }
                else
                {
                    dSquareY = dMinY - dRemainderY + iLength;
                }

                //生成横线方里线
                while (dSquareY <= dMaxY)
                {
                    IPolyline pLine = CreateLineForTwoPoint(dMinX, dSquareY, dMaxX, dSquareY);
                    pLine = GeometryComm.GetIntersectGeo(pLine, pAreaGeo, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    pFeatureBuffer.Shape = pLine;
                    pInsertCursor.InsertFeature(pFeatureBuffer);
                    dSquareY += iLength;
                    //500条压缩一次
                    if (iCount % 500 == 0)
                    {
                        pInsertCursor.Flush();
                    }
                }

                if (pInsertCursor != null)
                {
                    pInsertCursor.Flush();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInsertCursor);
            }
        }

        /// <summary>
        /// 生成方里十字丝
        /// </summary>
        /// <param name="pSourceQuareNet">原方里网层</param>
        /// <param name="pSpatialReference">新方里网图层坐标系</param>
        /// <param name="pTargetWorkSpace">新方里网图层空间</param>
        /// <param name="dMinX">最小X坐标</param>
        /// <param name="dMinY">最小Y坐标</param>
        /// <param name="dMaxX">最大X坐标</param>
        /// <param name="dMaxY">做大Y坐标</param>
        /// <param name="iScale">比例尺</param>
        /// <param name="iLength">方里网距离，默认一公里</param>
        /// <returns></returns>
        public static bool CreateSquareCrossWire(IFeatureClass pNewQuareNet, double dMinX, double dMinY, double dMaxX,
                                                            double dMaxY, int iScale, int iLength = 1000)
        {
            IFeatureCursor pInsertCursor = null;
            try
            {
                double dCrossWire = 0.005 * iScale;//十字丝长度的一半
                if (pNewQuareNet == null)
                {
                    return false;
                }
                double dSquareX = 0.0;//计算的方立网X坐标
                double dSquareY = 0.0;//计算的方里网Y坐标
                double dRemainderX = (double)((decimal)dMinX % (decimal)iLength);//方里网X范围最小值与方里网格长度的余数
                double dRemainderY = (double)((decimal)dMinY % (decimal)iLength);//方里网Y范围最小值与方里网格长度的余数

                //计算给定范围内方里网的最小X坐标
                if (dRemainderX == 0)
                {
                    dSquareX = dMinX;
                }
                else
                {
                    dSquareX = dMinX - dRemainderX + iLength;
                }

                //计算给定范围内方里网的最小Y坐标
                if (dRemainderY == 0)
                {
                    dSquareY = dMinY;
                }
                else
                {
                    dSquareY = dMinY - dRemainderY + iLength;
                }

                pInsertCursor = pNewQuareNet.Insert(true);
                IFeatureBuffer pFeatureBuffer = pNewQuareNet.CreateFeatureBuffer();
                IPoint pPoint = new PointClass();
                int iCount = 0;
                //生成十字丝
                IPolyline pLine = null;
                while (dSquareX <= dMaxX)
                {
                    double dY = dSquareY;
                    while (dY <= dMaxY)
                    {
                        //画横线
                        pLine = CreateLineForTwoPoint(dSquareX - dCrossWire, dY, dSquareX + dCrossWire, dY);
                        pFeatureBuffer.Shape = pLine;
                        pInsertCursor.InsertFeature(pFeatureBuffer);

                        //画纵线
                        pLine = CreateLineForTwoPoint(dSquareX, dY - dCrossWire, dSquareX, dY + dCrossWire);
                        pFeatureBuffer.Shape = pLine;
                        pInsertCursor.InsertFeature(pFeatureBuffer);
                        //500条压缩一次
                        iCount++;
                        if (iCount % 500 == 0)
                        {
                            pInsertCursor.Flush();
                        }
                        dY += iLength;
                    }
                    dSquareX += iLength;
                }
                if (pInsertCursor != null)
                {
                    pInsertCursor.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInsertCursor);
            }
        }

        /// <summary>
        /// 生成方里十字丝,带有图幅边框
        /// </summary>
        /// <param name="pNewQuareNet"></param>
        /// <param name="sTFBH"></param>
        /// <param name="pEarth"></param>
        /// <param name="dAddY"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public static bool CreateSquareCrossWire(IFeatureClass pNewQuareNet, string sTFBH, EarthParams pEarth,
                                                          double dAddY, double dCentralMeridian, int iLength = 1000)
        {
            IFeatureCursor pInsertCursor = null;
            try
            {
                double dMinX, dMinY, dMaxX, dMaxY;
                double minLat = 0.0, minLon = 0.0, maxLat = 0.0, maxLon = 0.0;

                if (pNewQuareNet == null || !TFComm.CheckTFHIsValid(sTFBH))
                {
                    return false;
                }

                int iScale = TFComm.GetScaleByTFH(sTFBH);
                double dCrossWire = 0.005 * iScale;//十字丝长度的一半

                TFComm.GetFourDSFromTFH(sTFBH, ref minLon, ref minLat, ref maxLon, ref maxLat);
                IGeometry pAreaGeo = TFComm.CreateTFPolygon(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //创建边框线
                IGeometry pGeo = TFComm.CreateTFPolline(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //根据图幅框的外边距生成方里网
                IEnvelope pEnvelope = pAreaGeo.Envelope;
                dMinX = pEnvelope.XMin;
                dMinY = pEnvelope.YMin;
                dMaxX = pEnvelope.XMax;
                dMaxY = pEnvelope.YMax;

                double dSquareX = 0.0;//计算的方立网X坐标
                double dSquareY = 0.0;//计算的方里网Y坐标
                double dRemainderX = (double)((decimal)dMinX % (decimal)iLength);//方里网X范围最小值与方里网格长度的余数
                double dRemainderY = (double)((decimal)dMinY % (decimal)iLength);//方里网Y范围最小值与方里网格长度的余数

                //计算给定范围内方里网的最小X坐标
                if (dRemainderX == 0)
                {
                    dSquareX = dMinX;
                }
                else
                {
                    dSquareX = dMinX - dRemainderX + iLength;
                }

                //计算给定范围内方里网的最小Y坐标
                if (dRemainderY == 0)
                {
                    dSquareY = dMinY;
                }
                else
                {
                    dSquareY = dMinY - dRemainderY + iLength;
                }

                pInsertCursor = pNewQuareNet.Insert(true);
                IFeatureBuffer pFeatureBuffer = pNewQuareNet.CreateFeatureBuffer();

                //添加图幅框
                pFeatureBuffer.Shape = pGeo;
                pInsertCursor.InsertFeature(pFeatureBuffer);

                IPoint pPoint = new PointClass();
                int iCount = 0;
                //生成十字丝
                IPolyline pLine = null;
                while (dSquareX <= dMaxX)
                {
                    double dY = dSquareY;
                    while (dY <= dMaxY)
                    {
                        //画横线
                        pLine = CreateLineForTwoPoint(dSquareX - dCrossWire, dY, dSquareX + dCrossWire, dY);
                        pLine = GeometryComm.GetIntersectGeo(pLine, pAreaGeo, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                        pFeatureBuffer.Shape = pLine;
                        pInsertCursor.InsertFeature(pFeatureBuffer);

                        //画纵线
                        pLine = CreateLineForTwoPoint(dSquareX, dY - dCrossWire, dSquareX, dY + dCrossWire);
                        pLine = GeometryComm.GetIntersectGeo(pLine, pAreaGeo, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                        pFeatureBuffer.Shape = pLine;
                        pInsertCursor.InsertFeature(pFeatureBuffer);
                        //500条压缩一次
                        iCount++;
                        if (iCount % 500 == 0)
                        {
                            pInsertCursor.Flush();
                        }
                        dY += iLength;
                    }
                    dSquareX += iLength;
                }

                if (pInsertCursor != null)
                {
                    pInsertCursor.Flush();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pInsertCursor);
            }
        }

        /// <summary>
        /// 创建外图廓和方里网注记
        /// </summary>
        /// <param name="pNewOutTKFeatureClass">新的外图廓</param>
        /// <param name="pSquareFeatureClass">方里网</param>
        /// <param name="pNewAnoFeatureClass">新注记图层</param>
        /// <param name="sTFBH">图幅号</param>
        /// <param name="pEarth"></param>
        /// <param name="dAddY"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="iLength">方里网距离</param>
        /// <returns></returns>
        public static bool CreateOutTK(IFeatureClass pNewOutTKFeatureClass, IFeatureClass pSquareFeatureClass,
                                                IFeatureClass pNewAnoFeatureClass, string sTFBH,
                                                EarthParams pEarth, double dAddY, double dCentralMeridian, int iLength)
        {
            try
            {
                double minLat = 0.0, minLon = 0.0, maxLat = 0.0, maxLon = 0.0;

                if (pNewOutTKFeatureClass == null || !TFComm.CheckTFHIsValid(sTFBH))
                {
                    return false;
                }
                int iScale = TFComm.GetScaleByTFH(sTFBH);

                TFComm.GetFourDSFromTFH(sTFBH, ref minLon, ref minLat, ref maxLon, ref maxLat);

                IGeometry pAreaGeo = TFComm.CreateTFPolygon(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //生成外图廓线
                IPolyline pOutLine = TFComm.CreateOutTKLine(pAreaGeo as IPolygon, iScale);
                IFeature pNewFeature = pNewOutTKFeatureClass.CreateFeature();

                pNewFeature.Shape = pOutLine;
                pNewFeature.Store();

                TFComm.Create4CornerLine(pSquareFeatureClass, pNewAnoFeatureClass, minLat, minLon, maxLat, maxLon, iScale, pEarth, dCentralMeridian, dAddY, pOutLine);
                TFComm.CreateMeasuredGrid(pSquareFeatureClass, pNewAnoFeatureClass, pAreaGeo as IPolygon, iScale, iLength);
                TFComm.CreateTFHAnno(pNewAnoFeatureClass, sTFBH, pEarth, dCentralMeridian, dAddY);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// 创建图廓图层，并生成新的要素
        /// </summary>
        /// <param name="pNewInnnerTKPolygon"></param>
        /// <param name="sTFBH"></param>
        /// <param name="pEarth"></param>
        /// <param name="dAddY"></param>
        /// <param name="dCentralMeridian"></param>
        /// <returns></returns>
        public static bool CreateInnerTKPolygon(IFeatureClass pNewInnnerTKPolygon, IFeatureClass pSourceFeatureClass, string sTFBH,
                                                        EarthParams pEarth, double dAddY, double dCentralMeridian)
        {
            try
            {
                if (pNewInnnerTKPolygon == null)
                {
                    return false;
                }

                double minLat = 0.0, minLon = 0.0, maxLat = 0.0, maxLon = 0.0;//图幅经纬度范围

                TFComm.GetFourDSFromTFH(sTFBH, ref minLon, ref minLat, ref maxLon, ref maxLat);
                //创建图廓图形
                IGeometry pAreaGeo = TFComm.CreateTFPolygon(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                //创建新的要素
                IFeature pFeature = pNewInnnerTKPolygon.CreateFeature();
                pFeature.Shape = pAreaGeo;

                //将源图廓要素的属性复制到新的要素中
                List<IFeature> lst = FeatureComm.GetFeatures(pSourceFeatureClass, "");
                if (lst != null && lst.Count > 0)
                {
                    IFeature pSourceFeature = lst[0];
                    FeatureComm.CopyFieldValue(pSourceFeature, ref pFeature);
                }
                pFeature.Store();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 根据两点创建直线
        /// </summary>
        /// <param name="dFormPointX">起点X坐标</param>
        /// <param name="dFormPointY">起点Y坐标</param>
        /// <param name="dToPointX">终点X坐标</param>
        /// <param name="dToPontY">终点Y坐标</param>
        /// <returns></returns>
        public static IPolyline CreateLineForTwoPoint(double dFormPointX, double dFormPointY, double dToPointX, double dToPontY)
        {
            try
            {
                IPolyline pPolyLine = new PolylineClass();
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(dFormPointX, dFormPointY);
                pPolyLine.FromPoint = pPoint;//起点
                pPoint.PutCoords(dToPointX, dToPontY);
                pPolyLine.ToPoint = pPoint;//终点
                return pPolyLine;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建图幅短线
        /// </summary>
        /// <param name="dFormPointX"></param>
        /// <param name="dFormPointY"></param>
        /// <param name="dToPointX"></param>
        /// <param name="dToPontY"></param>
        /// <param name="pInnerPolyLine"></param>
        /// <param name="pLineDirection"></param>
        /// <returns></returns>
        public static IPolyline CreateShortLine(double dFormPointX, double dFormPointY, double dToPointX, double dToPontY, IPolyline pInnerPolyLine)
        {
            try
            {
                IPolyline pPolyLine = CreateLineForTwoPoint(dFormPointX, dFormPointY, dToPointX, dToPontY);
                IPoint pIntersectPoint = GeometryComm.GetIntersectPoint(pPolyLine, pInnerPolyLine);
                if (pIntersectPoint == null)
                {
                    return null;
                }
                pPolyLine.ToPoint = pIntersectPoint;
                return pPolyLine;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
