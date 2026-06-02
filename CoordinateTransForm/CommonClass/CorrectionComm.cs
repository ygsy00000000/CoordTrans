using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.Framework.CommonModule;

namespace ZJUGIS.CoordinateTrans.CommonClass
{
    /// <summary>
    /// 计算改正量公共方法
    /// </summary>
    public class CorrectionComm
    {
        /// <summary>
        /// 计算坐标改正量
        /// </summary>
        /// <param name="earth_old"></param>
        /// <param name="earth_new"></param>
        /// <param name="iScale"></param>
        /// <param name="model"></param>
        /// <param name="pSevenParams"></param>
        /// <param name="minB"></param>
        /// <param name="minL"></param>
        /// <param name="maxB"></param>
        /// <param name="maxL"></param>
        /// <param name="dOldCentralMeridian"></param>
        /// <param name="dOldAddY"></param>
        /// <param name="dNewCentralMeridian"></param>
        /// <param name="dNewAddY"></param>
        public static Dictionary<string, MapCorrection> ExcuteComputeCorrection(EarthParams earth_old, EarthParams earth_new, int iScale,
                                                   EnumTransFormModel model, SevenParams pSevenParams, List<CoordinatePoint> points,
                                                   double dOldCentralMeridian, double dOldAddY,
                                                   double dNewCentralMeridian, double dNewAddY,
                                                   IProgressDialog pProgressDialog = null)
        {
            Dictionary<string, MapCorrection> dic = null;
            try
            {
                //// 获取格网点列表（度分秒）
                if (points == null || points.Count == 0)
                {
                    return null;
                }

                if (iScale < 10000)
                {
                    TCoordinate TClass = new TCoordinate3D_7Params(EnumTransFormType.BLHtoXYH, EnumCoordinateFormat.ddmmss,
                                                                   earth_old, earth_new, model, pSevenParams, points,
                                                                   dOldCentralMeridian, dOldAddY, dNewCentralMeridian, dNewAddY);
                    //// 七参数转换得到新坐标系的平面坐标
                    List<CoordinatePoint> NewPnts = TClass.Compute();

                    //// 高斯正算就坐标系下的平面坐标
                    TCoordinate TGauss = new GaussTrans(EnumTransFormType.BLtoXY, EnumCoordinateFormat.ddmmss, earth_old, points, dOldCentralMeridian, dOldAddY);
                    List<CoordinatePoint> oldPnts = TGauss.Compute();

                    dic = ComputeCorrection3(iScale, points, NewPnts, pProgressDialog, oldPnts);

                }
                else
                {
                    TCoordinate TClass = new TCoordinate3D_7Params(EnumTransFormType.BLHtoBLH, EnumCoordinateFormat.ddmmss,
                                                                   earth_old, earth_new, model, pSevenParams, points,
                                                                   dOldCentralMeridian, dOldAddY, dNewCentralMeridian, dNewAddY);
                    //// 七参数转换得到新坐标系的大地坐标
                    List<CoordinatePoint> NewPnts = TClass.Compute();
                    dic = ComputeCorrection3(iScale, points, NewPnts, pProgressDialog, null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dic;
        }

        /// <summary>
        /// 计算改正量
        /// </summary>
        /// <param name="iScale">比例尺</param>
        /// <param name="SourcePnts">原坐标系下的图幅格网点(度分秒)</param>
        /// <param name="NewPnts">新坐标系下的格网点坐标(度)</param>
        /// <param name="oldPnts">原坐标系下的图幅格网点平面坐标(米)</param>
        /// <returns></returns>
        public static Dictionary<string, MapCorrection> ComputeCorrection(int iScale, List<CoordinatePoint> SourcePnts, List<CoordinatePoint> NewPnts, IProgressDialog pProgressDialog = null, List<CoordinatePoint> oldPnts = null)
        {
            Dictionary<string, MapCorrection> dic = null;
            try
            {
                double dB = 0.0, dL = 0.0;
                TFComm.GetDeviation(iScale, ref dB, ref dL);
                dB = TransFormMethod.DmsToSecond(dB);
                dL = TransFormMethod.DmsToSecond(dL);

                dic = new Dictionary<string, MapCorrection>();
                MapCorrection obj = null;
                List<CoordinatePoint> top = null;
                List<CoordinatePoint> face = null;
                List<CoordinatePoint> right = null;
                CoordinatePoint current = null;
                int index = 0;


                if (pProgressDialog != null)
                {
                    pProgressDialog.Position = 1;
                    pProgressDialog.Description = "正在生成图幅格网点字典表";
                    pProgressDialog.Step(1);

                    pProgressDialog.Max = SourcePnts.Count;
                    pProgressDialog.Position = 0;
                    pProgressDialog.Message = "正在生成图幅格网点字典表";
                }

                if (iScale < 10000)
                {
                    #region 大比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }
                        current = SourcePnts[i];
                        obj = new MapCorrection();
                        obj.Scale = iScale;
                        obj.minB = TransFormMethod.DmsToSecond(current.XorLat);
                        obj.minL = TransFormMethod.DmsToSecond(current.YorLon);

                        face = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB - dB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL - dL) < 1E-09 select t).ToList();
                        if (face == null || face.Count == 0)
                        {
                            continue;
                        }

                        top = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB - dB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL) < 1E-09 select t).ToList();
                        if (top == null || top.Count == 0)
                        {
                            continue;
                        }

                        right = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL - dL) < 1E-09 select t).ToList();
                        if (right == null || right.Count == 0)
                        {
                            continue;
                        }

                        obj.maxB = TransFormMethod.DmsToSecond(face[0].XorLat);
                        obj.maxL = TransFormMethod.DmsToSecond(face[0].YorLon);
                        obj.LeftBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;
                        obj.LeftBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;
                        index = SourcePnts.IndexOf(top[0]);
                        obj.LeftTopdB = NewPnts[index].XorLat - oldPnts[index].XorLat;
                        obj.LeftTopdL = NewPnts[index].YorLon - oldPnts[index].YorLon;
                        index = SourcePnts.IndexOf(right[0]);
                        obj.RightBottomdB = NewPnts[index].XorLat - oldPnts[index].XorLat;
                        obj.RightBottomdL = NewPnts[index].YorLon - oldPnts[index].YorLon;
                        index = SourcePnts.IndexOf(face[0]);
                        obj.RightTopdB = NewPnts[index].XorLat - oldPnts[index].XorLat;
                        obj.RightTopdL = NewPnts[index].YorLon - oldPnts[index].YorLon;

                        obj.ComputeBL();

                        if (!dic.ContainsKey(obj.TFBH))
                        {
                            dic.Add(obj.TFBH, obj);
                        }
                        else
                        {

                        }
                    }
                    #endregion
                }
                else
                {
                    #region 小比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }

                        current = SourcePnts[i];
                        obj = new MapCorrection();
                        obj.Scale = iScale;
                        obj.minB = TransFormMethod.DmsToSecond(current.XorLat);
                        obj.minL = TransFormMethod.DmsToSecond(current.YorLon);
                        face = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB - dB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL - dL) < 1E-09 select t).ToList();
                        if (face == null || face.Count == 0)
                        {
                            continue;
                        }

                        top = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB - dB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL) < 1E-09 select t).ToList();
                        if (top == null || top.Count == 0)
                        {
                            continue;
                        }

                        right = (from t in SourcePnts where Math.Abs(TransFormMethod.DmsToSecond(t.XorLat) - obj.minB) < 1E-09 && Math.Abs(TransFormMethod.DmsToSecond(t.YorLon) - obj.minL - dL) < 1E-09 select t).ToList();
                        if (right == null || right.Count == 0)
                        {
                            continue;
                        }

                        obj.maxB = TransFormMethod.DmsToSecond(face[0].XorLat);
                        obj.maxL = TransFormMethod.DmsToSecond(face[0].YorLon);
                        obj.LeftBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                        obj.LeftBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);
                        index = SourcePnts.IndexOf(top[0]);
                        obj.LeftTopdB = TransFormMethod.DmsToSecond(NewPnts[index].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[index].XorLat);
                        obj.LeftTopdL = TransFormMethod.DmsToSecond(NewPnts[index].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[index].YorLon);
                        index = SourcePnts.IndexOf(right[0]);
                        obj.RightBottomdB = TransFormMethod.DmsToSecond(NewPnts[index].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[index].XorLat);
                        obj.RightBottomdL = TransFormMethod.DmsToSecond(NewPnts[index].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[index].YorLon);
                        index = SourcePnts.IndexOf(face[0]);
                        obj.RightTopdB = TransFormMethod.DmsToSecond(NewPnts[index].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[index].XorLat);
                        obj.RightTopdL = TransFormMethod.DmsToSecond(NewPnts[index].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[index].YorLon);

                        obj.ComputeBL();

                        if (!dic.ContainsKey(obj.TFBH))
                        {
                            dic.Add(obj.TFBH, obj);
                        }
                        else
                        {

                        }

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dic;
        }

        /// <summary>
        /// 度转度分秒
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<CoordinatePoint> PointsDmsToDegree(List<CoordinatePoint> points)
        {
            List<CoordinatePoint> NewPnts = new List<CoordinatePoint>();
            try
            {
                if (points == null || points.Count == 0)
                {
                    return NewPnts;
                }

                CoordinatePoint point = null;
                foreach (CoordinatePoint item in points)
                {
                    point = new CoordinatePoint();
                    point.DH = item.DH;
                    point.XorLat = TransFormMethod.DmsToDegree(item.XorLat);
                    point.YorLon = TransFormMethod.DmsToDegree(item.YorLon);
                    point.HorZ = item.HorZ;
                    NewPnts.Add(point);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPnts;
        }

        /// <summary>
        /// 计算改正量
        /// </summary>
        /// <param name="iScale">比例尺</param>
        /// <param name="SourcePnts">原坐标系下的图幅格网点(度分秒)</param>
        /// <param name="NewPnts">新坐标系下的格网点坐标(度)</param>
        /// <param name="oldPnts">原坐标系下的图幅格网点平面坐标(米)</param>
        /// <returns></returns>
        public static Dictionary<string, MapCorrection> ComputeCorrection2(int iScale, List<CoordinatePoint> SourcePnts, List<CoordinatePoint> NewPnts, IProgressDialog pProgressDialog = null, List<CoordinatePoint> oldPnts = null)
        {
            Dictionary<string, MapCorrection> dic = null;
            try
            {
                double dB = 0.0, dL = 0.0;
                TFComm.GetDeviation(iScale, ref dB, ref dL);
                dB = TransFormMethod.DmsToSecond(dB);
                dL = TransFormMethod.DmsToSecond(dL);

                dic = new Dictionary<string, MapCorrection>();
                MapCorrection obj = null;
                //List<CoordinatePoint> top = null;
                //List<CoordinatePoint> face = null;
                //List<CoordinatePoint> right = null;
                CoordinatePoint current = null;
                CoordinatePoint compare = null;
                int count = 0;
                double B = 0.0, L = 0.0;

                if (pProgressDialog != null)
                {
                    pProgressDialog.Position = 1;
                    pProgressDialog.Description = "正在生成图幅格网点字典表";
                    pProgressDialog.Step(1);

                    pProgressDialog.Max = SourcePnts.Count;
                    pProgressDialog.Position = 0;
                    pProgressDialog.Message = "正在生成图幅格网点字典表";
                }

                if (iScale < 10000)
                {
                    #region 大比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }
                        current = SourcePnts[i];
                        obj = new MapCorrection();
                        obj.Scale = iScale;
                        obj.minB = TransFormMethod.DmsToSecond(current.XorLat);
                        obj.minL = TransFormMethod.DmsToSecond(current.YorLon);

                        count = 0;
                        for (int j = 0; j < SourcePnts.Count; j++)
                        {
                            compare = SourcePnts[j];
                            B = TransFormMethod.DmsToSecond(compare.XorLat);
                            L = TransFormMethod.DmsToSecond(compare.YorLon);
                            if (Math.Abs(B - obj.minB - dB) < 1E-09 && Math.Abs(L - obj.minL - dL) < 1E-09)
                            {
                                count++;
                                obj.maxB = B;
                                obj.maxL = L;
                                obj.RightTopdB = NewPnts[j].XorLat - oldPnts[j].XorLat;
                                obj.RightTopdL = NewPnts[j].YorLon - oldPnts[j].YorLon;
                                continue;
                            }

                            if (Math.Abs(B - obj.minB - dB) < 1E-09 && Math.Abs(L - obj.minL) < 1E-09)
                            {
                                count++;
                                obj.LeftTopdB = NewPnts[j].XorLat - oldPnts[j].XorLat;
                                obj.LeftTopdL = NewPnts[j].YorLon - oldPnts[j].YorLon;
                                continue;
                            }

                            if (Math.Abs(B - obj.minB) < 1E-09 && Math.Abs(L - obj.minL - dL) < 1E-09)
                            {
                                count++;
                                obj.RightBottomdB = NewPnts[j].XorLat - oldPnts[j].XorLat;
                                obj.RightBottomdL = NewPnts[j].YorLon - oldPnts[j].YorLon;
                                continue;
                            }
                            if (count == 3)
                            {
                                break;
                            }
                        }

                        if (count != 3)
                        {
                            continue;
                        }

                        obj.LeftBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;
                        obj.LeftBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;

                        obj.ComputeBL();

                        if (!dic.ContainsKey(obj.TFBH))
                        {
                            dic.Add(obj.TFBH, obj);
                        }
                        else
                        {

                        }
                    }
                    #endregion
                }
                else
                {
                    #region 小比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }

                        current = SourcePnts[i];
                        obj = new MapCorrection();
                        obj.Scale = iScale;
                        obj.minB = TransFormMethod.DmsToSecond(current.XorLat);
                        obj.minL = TransFormMethod.DmsToSecond(current.YorLon);
                        count = 0;
                        for (int j = 0; j < SourcePnts.Count; j++)
                        {
                            compare = SourcePnts[j];
                            B = TransFormMethod.DmsToSecond(compare.XorLat);
                            L = TransFormMethod.DmsToSecond(compare.YorLon);
                            if (Math.Abs(B - obj.minB - dB) < 1E-09 && Math.Abs(L - obj.minL - dL) < 1E-09)
                            {
                                count++;
                                obj.maxB = B;
                                obj.maxL = L;
                                obj.RightTopdB = TransFormMethod.DmsToSecond(NewPnts[j].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[j].XorLat);
                                obj.RightTopdL = TransFormMethod.DmsToSecond(NewPnts[j].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[j].YorLon);
                                continue;
                            }

                            if (Math.Abs(B - obj.minB - dB) < 1E-09 && Math.Abs(L - obj.minL) < 1E-09)
                            {
                                count++;
                                obj.LeftTopdB = TransFormMethod.DmsToSecond(NewPnts[j].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[j].XorLat);
                                obj.LeftTopdL = TransFormMethod.DmsToSecond(NewPnts[j].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[j].YorLon);
                                continue;
                            }

                            if (Math.Abs(B - obj.minB) < 1E-09 && Math.Abs(L - obj.minL - dL) < 1E-09)
                            {
                                count++;
                                obj.RightBottomdB = TransFormMethod.DmsToSecond(NewPnts[j].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[j].XorLat);
                                obj.RightBottomdL = TransFormMethod.DmsToSecond(NewPnts[j].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[j].YorLon);
                                continue;
                            }
                            if (count == 3)
                            {
                                break;
                            }
                        }

                        if (count != 3)
                        {
                            continue;
                        }

                        obj.LeftBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                        obj.LeftBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);

                        obj.ComputeBL();

                        if (!dic.ContainsKey(obj.TFBH))
                        {
                            dic.Add(obj.TFBH, obj);
                        }
                        else
                        {

                        }

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dic;
        }

        /// <summary>
        /// 计算改正量
        /// </summary>
        /// <param name="iScale">比例尺</param>
        /// <param name="SourcePnts">原坐标系下的图幅格网点(度分秒)</param>
        /// <param name="NewPnts">新坐标系下的格网点坐标(度)</param>
        /// <param name="oldPnts">原坐标系下的图幅格网点平面坐标(米)</param>
        /// <returns></returns>
        public static Dictionary<string, MapCorrection> ComputeCorrection3(int iScale, List<CoordinatePoint> SourcePnts, List<CoordinatePoint> NewPnts, IProgressDialog pProgressDialog = null, List<CoordinatePoint> oldPnts = null)
        {
            Dictionary<string, MapCorrection> dic = null;
            try
            {
                double dB = 0.0, dL = 0.0;
                TFComm.GetDeviation(iScale, ref dB, ref dL);
                dB = TransFormMethod.DmsToSecond(dB);
                dL = TransFormMethod.DmsToSecond(dL);

                dic = new Dictionary<string, MapCorrection>();
                Dictionary<string, MapCorrection> dic1 = new Dictionary<string, MapCorrection>();
                MapCorrection obj = null;
                CoordinatePoint current = null;
                double B = 0.0, L = 0.0;
                string sTFH_LB = "";
                string sTFH_LT = "";
                string sTFH_RB = "";
                string sTFH_RT = "";

                if (pProgressDialog != null)
                {
                    pProgressDialog.Position = 1;
                    pProgressDialog.Description = "正在生成图幅格网点字典表";
                    pProgressDialog.Step(1);

                    pProgressDialog.Max = SourcePnts.Count;
                    pProgressDialog.Position = 0;
                    pProgressDialog.Message = "正在生成图幅格网点字典表";
                }

                if (iScale < 10000)
                {
                    #region 大比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }

                        current = SourcePnts[i];
                        B = TransFormMethod.DmsToSecond(current.XorLat);
                        L = TransFormMethod.DmsToSecond(current.YorLon);

                        #region 左下角点
                        //// 左下角点
                        sTFH_LB = TFComm.GetMapIndexNumber(B, L, iScale);
                        if (dic1.ContainsKey(sTFH_LB))
                        {
                            dic1[sTFH_LB].minB = B;
                            dic1[sTFH_LB].LeftBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            dic1[sTFH_LB].LeftBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_LB;
                            obj.Scale = iScale;
                            obj.minB = B;
                            obj.LeftBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            obj.LeftBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;
                            dic1.Add(sTFH_LB, obj);
                        }
                        #endregion

                        #region 右下角点
                        //// 右下角点
                        sTFH_RB = TFComm.GetMapIndexNumber(B + 1.0, L - 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_RB))
                        {
                            dic1[sTFH_RB].maxL = L;
                            dic1[sTFH_RB].RightBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            dic1[sTFH_RB].RightBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_RB;
                            obj.Scale = iScale;
                            obj.maxL = L;
                            obj.RightBottomdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            obj.RightBottomdL = NewPnts[i].YorLon - oldPnts[i].YorLon;
                            dic1.Add(sTFH_RB, obj);
                        }
                        #endregion

                        #region 右上角点
                        //// 右上角点
                        sTFH_RT = TFComm.GetMapIndexNumber(B - 1.0, L - 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_RT))
                        {
                            dic1[sTFH_RT].maxB = B;
                            dic1[sTFH_RT].RightTopdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            dic1[sTFH_RT].RightTopdL = NewPnts[i].YorLon - oldPnts[i].YorLon;

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_RT;
                            obj.Scale = iScale;
                            obj.maxB = B;
                            obj.RightTopdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            obj.RightTopdL = NewPnts[i].YorLon - oldPnts[i].YorLon;
                            dic1.Add(sTFH_RT, obj);
                        }
                        #endregion

                        #region 左上角点
                        //// 左上角点
                        sTFH_LT = TFComm.GetMapIndexNumber(B - 1.0, L + 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_LT))
                        {
                            dic1[sTFH_LT].minL = L;
                            dic1[sTFH_LT].LeftTopdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            dic1[sTFH_LT].LeftTopdL = NewPnts[i].YorLon - oldPnts[i].YorLon;

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_LT;
                            obj.Scale = iScale;
                            obj.minL = L;
                            obj.LeftTopdB = NewPnts[i].XorLat - oldPnts[i].XorLat;                //// 测试先减再转，和先转再减？
                            obj.LeftTopdL = NewPnts[i].YorLon - oldPnts[i].YorLon;
                            dic1.Add(sTFH_LT, obj);
                        }
                        #endregion
                    }

                    foreach (string key in dic1.Keys)
                    {
                        obj = dic1[key];
                        if (obj.minB != 0.0 && obj.maxB != 0.0 && obj.minL != 0.0 && obj.maxL != 0.0)
                        {
                            obj.ComputeBL();
                            dic.Add(key, obj);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 小比例尺
                    for (int i = 0; i < SourcePnts.Count; i++)
                    {
                        if (pProgressDialog != null)
                        {
                            pProgressDialog.Description = string.Format("正在生成图幅格网点字典表{0}/{1}", i + 1, SourcePnts.Count);
                            pProgressDialog.Step(1);
                        }

                        current = SourcePnts[i];
                        B = TransFormMethod.DmsToSecond(current.XorLat);
                        L = TransFormMethod.DmsToSecond(current.YorLon);

                        #region 左下角点
                        //// 左下角点
                        sTFH_LB = TFComm.GetTFHFromJWDAndScale(B, L, iScale);
                        if (dic1.ContainsKey(sTFH_LB))
                        {
                            dic1[sTFH_LB].minB = B;
                            dic1[sTFH_LB].LeftBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            dic1[sTFH_LB].LeftBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_LB;
                            obj.Scale = iScale;
                            obj.minB = B;
                            obj.LeftBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            obj.LeftBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);
                            dic1.Add(sTFH_LB, obj);
                        }
                        #endregion

                        #region 右下角点
                        //// 右下角点
                        sTFH_RB = TFComm.GetTFHFromJWDAndScale(B + 1.0, L - 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_RB))
                        {
                            dic1[sTFH_RB].maxL = L;
                            dic1[sTFH_RB].RightBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            dic1[sTFH_RB].RightBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_RB;
                            obj.Scale = iScale;
                            obj.maxL = L;
                            obj.RightBottomdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            obj.RightBottomdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);
                            dic1.Add(sTFH_RB, obj);
                        }
                        #endregion

                        #region 右上角点
                        //// 右上角点
                        sTFH_RT = TFComm.GetTFHFromJWDAndScale(B - 1.0, L - 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_RT))
                        {
                            dic1[sTFH_RT].maxB = B;
                            dic1[sTFH_RT].RightTopdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            dic1[sTFH_RT].RightTopdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_RT;
                            obj.Scale = iScale;
                            obj.maxB = B;
                            obj.RightTopdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            obj.RightTopdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);
                            dic1.Add(sTFH_RT, obj);
                        }
                        #endregion

                        #region 左上角点
                        //// 左上角点
                        sTFH_LT = TFComm.GetTFHFromJWDAndScale(B - 1.0, L + 1.0, iScale);
                        if (dic1.ContainsKey(sTFH_LT))
                        {
                            dic1[sTFH_LT].minL = L;
                            dic1[sTFH_LT].LeftTopdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            dic1[sTFH_LT].LeftTopdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);

                        }
                        else
                        {
                            obj = new MapCorrection();
                            obj.TFBH = sTFH_LT;
                            obj.Scale = iScale;
                            obj.minL = L;
                            obj.LeftTopdB = TransFormMethod.DmsToSecond(NewPnts[i].XorLat) - TransFormMethod.DmsToSecond(SourcePnts[i].XorLat);                //// 测试先减再转，和先转再减？
                            obj.LeftTopdL = TransFormMethod.DmsToSecond(NewPnts[i].YorLon) - TransFormMethod.DmsToSecond(SourcePnts[i].YorLon);
                            dic1.Add(sTFH_LT, obj);
                        }
                        #endregion
                    }

                    foreach (string key in dic1.Keys)
                    {
                        obj = dic1[key];
                        if (obj.minB != 0.0 && obj.maxB != 0.0 && obj.minL != 0.0 && obj.maxL != 0.0)
                        {
                            obj.ComputeBL();
                            dic.Add(key, obj);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dic;
        }

    }
}
