using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.GIS.CommonMethod;
using ZJUGIS.GISModule.CommonMethod;

namespace ZJUGIS.CoordinateTrans
{
    public class TFComm
    {
        public static Int32 Int(Double d)
        {
            return (Int32)Math.Floor(d);
        }

        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }

        #region 计算图幅号
        /// <summary>
        /// 国标 根据坐标位置、比例尺算图幅号,张主任的代码
        /// </summary>
        /// <param name="Lat">纬度（秒）</param>
        /// <param name="Lon">经度（秒）</param>
        /// <param name="iScale">比例尺</param>
        /// <returns></returns>
        public static string GetTFHFromJWDAndScale(double Lat, double Lon, int iScale)
        {
            Int32 aa, bb, cc, dd;
            String A, B, C, D;

            //计算100万的行和列
            aa = Int(Lat / 14400) + 1;
            bb = Int(Lon / 21600) + 31;

            A = Chr(aa + 64);//A的ASCII码是：65
            B = bb.ToString();
            String TfhText = "";
            //1：100万图幅号  "A"
            if (iScale == 1000000)
                TfhText = A + "" + B;
            //1：50万图幅号  "B"
            if (iScale == 500000)
            {
                cc = 2 - Int((Lat % 14400) / 7200);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);
                dd = Int((Lon % 21600) / 10800) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "B" + "" + C + "" + D;
            }
            //1：25万图幅号 "C"
            if (iScale == 250000)
            {
                cc = 4 - Int((Lat % 14400) / 3600);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);
                dd = Int((Lon % 21600) / 5400) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "C" + "" + C + "" + D;
            }
            //1：10万图幅号 "D"
            if (iScale == 100000)
            {
                cc = 12 - Int((Lat % 14400) / 1200);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);
                dd = Int((Lon % 21600) / 1800) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "D" + "" + C + "" + D;
            }
            //1：5万图幅号 "E"
            if (iScale == 50000)
            {
                cc = 24 - Int((Lat % 14400) / 600);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);
                dd = Int((Lon % 21600) / 900) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "E" + "" + C + "" + D;
            }
            //1：2.5万图幅号 "F"
            if (iScale == 25000)
            {
                cc = 48 - Int((Lat % 14400) / 300);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);

                dd = Int((Lon % 21600) / 450) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "F" + "" + C + "" + D;
            }
            //1：1万图幅号 "G"
            if (iScale == 10000)
            {
                //计算图幅行号
                cc = 96 - Int((Lat % 14400) / 150);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);
                //计算图幅列号
                dd = Int((Lon % 21600) / 225) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);

                TfhText = A + "" + B + "" + "G" + "" + C + "" + D;
            }
            //1：5千图幅号 "H"
            if (iScale == 5000)
            {
                cc = 192 - Int((Lat % 14400) / 75);
                C = cc.ToString();
                C = "000" + C;
                C = C.Substring(C.Length - 3);

                dd = Int((Lon % 21600) / 112.5) + 1;
                D = dd.ToString();
                D = "000" + D;
                D = D.Substring(D.Length - 3);
                TfhText = A + "" + B + "" + "H" + "" + C + "" + D;
            }
            if (iScale < 5000)
            {
                TfhText = GetMapIndexNumber(Lat, Lon, iScale);
            }
            return TfhText;
        }

        /// <summary>
        /// 根据图幅的四角点，计算图幅号
        /// </summary>
        /// <param name="minLat">最小纬度（秒）</param>
        /// <param name="minLon">最小经度（秒）</param>
        /// <param name="maxLat">最大纬度（秒）</param>
        /// <param name="maxLon">最大经度（秒）</param>
        /// <returns></returns>
        public static string GetTFHFromSJD(double minLat, double minLon, double maxLat, double maxLon)
        {
            string sTFH = string.Empty;
            try
            {
                //获取比例尺
                int dScale = GetScaleByDeviation(maxLat - minLat, maxLon - minLon);
                sTFH = GetTFHFromJWDAndScale(minLat, minLon, dScale);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sTFH;
        }

        /// <summary>
        /// 计算图幅号
        /// </summary>
        /// <param name="dPointB">纬度(秒)</param>
        /// <param name="dPointL">经度(秒)</param>
        /// <param name="iScale">比例尺</param>
        /// <returns></returns>
        public static string GetMapIndexNumber(double dPointB, double dPointL, int iScale)
        {
            double dLat = 0, dLon = 0;
            GetDeviation(iScale, ref dLat, ref dLon);//获取经差和纬差
            ////将度分秒转换为秒以便于统一计算：
            double dXDeviation = TransFormMethod.DmsToSecond(dLat);
            double dYDeviation = TransFormMethod.DmsToSecond(dLon);

            //首先计算出1:100万分幅中的图幅号:
            string sRow100w = (Math.Truncate(Math.Truncate(dPointL / 3600) / 6) + 31).ToString();
            int iCode = (int)(Math.Truncate(dPointB / 3600) / 4) + 65;
            char cCol100w = Microsoft.VisualBasic.Strings.Chr(iCode);

            int sR = (int)(4 * 3600 / dXDeviation) - (int)(dPointB % (4 * 3600) / dXDeviation);
            int sC = (int)((dPointL % (6 * 3600)) / dYDeviation) + 1;

            string dRowScale = sR.ToString();
            string dColScale = sC.ToString();

            if (iScale < 2000)
            {
                while (dRowScale.Length < 4)
                {
                    dRowScale = '0' + dRowScale;
                }
                while (dColScale.Length < 4)
                {
                    dColScale = '0' + dColScale;
                }
            }
            else
            {
                while (dRowScale.Length < 3)
                {
                    dRowScale = '0' + dRowScale;
                }
                while (dColScale.Length < 3)
                {
                    dColScale = '0' + dColScale;
                }
            }
            string sCode = ScaleComm.GetScaleCode(iScale.ToString());
            return cCol100w + sRow100w + sCode + dRowScale + dColScale;
        }

        /// <summary>
        /// 判断图幅号是否有效，有效返回true
        /// </summary>
        /// <param name="sTFH"></param>
        /// <returns></returns>
        public static bool CheckTFHIsValid(string sTFH)
        {
            bool isRigth = false;
            try
            {
                if (string.IsNullOrEmpty(sTFH))
                {
                    return false;
                }
                int iScale = 0;
                iScale = GetScaleByTFH(sTFH);
                if (iScale == 0)
                {
                    return false;
                }
                double minLat = 0.0;
                double minLon = 0.0;
                double maxLat = 0.0;
                double maxLon = 0.0;
                GetFourDSFromTFH(sTFH, ref minLon, ref minLat, ref maxLon, ref maxLat);
                if (minLat == 0.0 && minLon == 0.0 && maxLat == 0.0 && maxLon == 0.0)
                {
                    return false;
                }
                isRigth = true;
            }
            catch (Exception)
            {
            }
            return isRigth;
        }
        #endregion

        /// <summary>
        /// 国标 根据图幅号算图廓四个角点坐标
        /// </summary>
        /// <param name="TFH">图幅号</param>
        /// <param name="dMinLon">左下角经度（秒）</param>
        /// <param name="dMinLat">左下角纬度（秒）</param>
        /// <param name="dMaxLon">右上角经度（秒）</param>
        /// <param name="dMaxLat">右上角纬度（秒）</param>
        /// <returns></returns>
        public static bool GetFourDSFromTFH(string TFH, ref double dMinLon, ref double dMinLat, ref double dMaxLon, ref double dMaxLat)
        {
            try
            {
                string BLC;
                double Dq = 0, Dp = 0;
                char[] Para;
                TFH = TFH.Trim();
                int i, Count;
                string ResultTFH;
                Count = TFH.Length;
                ResultTFH = "";
                if (Count < 10) return false;
                Para = TFH.ToCharArray();
                for (i = 0; i < Count; i++)
                    if (Para[i] != ' ')
                        ResultTFH = ResultTFH + Para[i].ToString();
                TFH = ResultTFH;
                if (TFH.Length < 10) return false;
                BLC = TFH.Substring(3, 1);

                GetDeviation(BLC.ToUpper(), ref Dq, ref Dp);
                Dq = TransFormMethod.DmsToSecond(Dq);
                Dp = TransFormMethod.DmsToSecond(Dp);

                ConvertFromTFH(TFH, ref dMinLon, ref dMinLat);
                dMaxLon = dMinLon + Dp;
                dMaxLat = dMinLat + Dq;
                return true;
            }
            catch
            { return false; }
        }

        /// <summary>
        /// 国标 根据图幅号算图廓左下角点坐标
        /// </summary>
        /// <param name="TFH">图幅号</param>
        /// <param name="JD">经度(秒)</param>
        /// <param name="WD">纬度(秒)</param>
        /// <returns></returns>
        public static bool ConvertFromTFH(string TFH, ref double JD, ref double WD)
        {
            int a, b, c, d;
            double Dq = 0.0, Dp = 0.0;
            char[] Para;
            TFH = TFH.Trim();
            int i, Count;
            string ResultTFH;
            Count = TFH.Length;
            ResultTFH = "";
            if (Count < 10) return false;
            Para = TFH.ToCharArray();
            for (i = 0; i < Count; i++)
                if (Para[i] != ' ')
                    ResultTFH = ResultTFH + Para[i].ToString();
            TFH = ResultTFH;
            if (TFH.Length < 10) return false;
            string RowH100W, ColH100W, BLC, RowHTF = string.Empty, ColHTF = string.Empty;
            RowH100W = TFH.Substring(0, 1);
            ColH100W = TFH.Substring(1, 2);
            BLC = TFH.Substring(3, 1);
            if (TFH.Length == 10)
            {
                RowHTF = TFH.Substring(4, 3);
                ColHTF = TFH.Substring(7, 3);
            }
            else if (TFH.Length == 12)
            {
                RowHTF = TFH.Substring(4, 4);
                ColHTF = TFH.Substring(8, 4);
            }
            switch (RowH100W.ToUpper())
            {
                case "A":
                    a = 1;
                    break;
                case "B":
                    a = 2;
                    break;
                case "C":
                    a = 3;
                    break;
                case "D":
                    a = 4;
                    break;
                case "E":
                    a = 5;
                    break;
                case "F":
                    a = 6;
                    break;
                case "G":
                    a = 7;
                    break;
                case "H":
                    a = 8;
                    break;
                case "I":
                    a = 9;
                    break;
                case "J":
                    a = 10;
                    break;
                case "K":
                    a = 11;
                    break;
                case "L":
                    a = 12;
                    break;
                case "M":
                    a = 13;
                    break;
                case "N":
                    a = 14;
                    break;
                default:
                    return false;
            }
            b = Convert.ToInt32(ColH100W);
            if (b > 60 || b < 1) return false;
            c = Convert.ToInt32(RowHTF);
            if (c > 999 || c < 1) return false;
            d = Convert.ToInt32(ColHTF);
            if (d > 999 || d < 1) return false;

            GetDeviation(BLC.ToUpper(), ref Dq, ref Dp);
            Dq = TransFormMethod.DmsToSecond(Dq);
            Dp = TransFormMethod.DmsToSecond(Dp);

            JD = (b - 31) * 6 * 3600 + (d - 1) * Dp;
            //WD=4*3600-c*Dq
            WD = (a - 1) * 4 * 3600 + (4 * 3600.0 / Dq - c) * Dq;
            return true;
        }

        /// <summary>
        /// 根据比例尺获取经纬度的误差值.
        /// </summary>
        /// <param name="sMapType">图幅类型</param>
        /// <param name="iScale">比例尺</param>
        /// <param name="dLat">纬差</param>
        /// <param name="dLon">经差</param>
        /// <remarks></remarks>
        public static void GetDeviation(int iScale, ref double dLat, ref double dLon)
        {
            try
            {
                switch (iScale)
                {
                    case 500:
                        dLat = 0.000625;       //// 9.375"
                        dLon = 0.0009375;        //// 6.25"                  
                        break;

                    case 1000:
                        dLat = 0.00125;         //// 12.5"
                        dLon = 0.001875;        //// 18.75"
                        break;

                    case 2000:
                        dLat = 0.0025;          //// 25"
                        dLon = 0.00375;         //// 37.5"
                        break;

                    case 5000:
                        dLat = 0.0115;          //// 1'15"
                        dLon = 0.01525;         //// 1'52.5"
                        break;

                    case 10000:
                        dLat = 0.023;           //// 2'30"
                        dLon = 0.0345;          //// 3'45"
                        break;

                    case 25000:
                        dLat = 0.05;            //// 5'
                        dLon = 0.073;           //// 7'30"
                        break;

                    case 50000:
                        dLat = 0.1;             //// 10'
                        dLon = 0.15;            //// 15'
                        break;

                    case 100000:
                        dLat = 0.2;             //// 20'
                        dLon = 0.3;             //// 30'
                        break;
                    case 250000:
                        dLat = 1;               ////1°
                        dLon = 1.3;             ////1°30′
                        break;
                    case 500000:
                        dLat = 2;               ////2°
                        dLon = 3;               ////3°
                        break;
                    case 1000000:
                        dLat = 4;               ////4°
                        dLon = 6;               ////6°
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据比例尺获取经纬度的误差值.
        /// </summary>
        /// <param name="sMapType">图幅类型</param>
        /// <param name="iScale">比例尺</param>
        /// <param name="dLat">纬差</param>
        /// <param name="dLon">经差</param>
        /// <remarks></remarks>
        public static void GetDeviation(string sScaleID, ref double dLat, ref double dLon)
        {
            try
            {
                switch (sScaleID.ToUpper())
                {
                    case "K":
                        dLat = 0.000625;       //// 9.375"
                        dLon = 0.0009375;        //// 6.25"                  
                        break;

                    case "J":
                        dLat = 0.00125;         //// 12.5"
                        dLon = 0.001875;        //// 18.75"
                        break;

                    case "I":
                        dLat = 0.0025;          //// 25"
                        dLon = 0.00375;         //// 37.5"
                        break;

                    case "H":
                        dLat = 0.0115;          //// 1'15"
                        dLon = 0.01525;         //// 1'52.5"
                        break;

                    case "G":
                        dLat = 0.023;           //// 2'30"
                        dLon = 0.0345;          //// 3'45"
                        break;

                    case "F":
                        dLat = 0.05;            //// 5'
                        dLon = 0.073;           //// 7'30"
                        break;

                    case "E":
                        dLat = 0.1;             //// 10'
                        dLon = 0.15;            //// 15'
                        break;

                    case "D":
                        dLat = 0.2;             //// 20'
                        dLon = 0.3;             //// 30'
                        break;
                    case "C":
                        dLat = 1;               ////1°
                        dLon = 1.3;             ////1°30′
                        break;
                    case "B":
                        dLat = 2;               ////2°
                        dLon = 3;               ////3°
                        break;
                    case "A":
                        dLat = 4;               ////4°
                        dLon = 6;               ////6°
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region 获取比例尺
        /// <summary>
        /// 根据经差和纬差获取图幅比例尺
        /// </summary>
        /// <param name="dLat">纬差（秒）</param>
        /// <param name="dLon">经差（秒）</param>
        /// <returns></returns>
        public static int GetScaleByDeviation(double dLat, double dLon)
        {
            int iScale = 0;
            try
            {
                //1:1000000比例次
                if (Math.Abs(dLat - 14400) < 0.001 && Math.Abs(dLon - 21600) < 0.001)
                {
                    iScale = 1000000;
                }
                //1:500000比例尺
                else if (Math.Abs(dLat - 7200) < 0.001 && Math.Abs(dLon - 10800) < 0.001)
                {
                    iScale = 500000;
                }

                //1:250000比例尺
                else if (Math.Abs(dLat - 3600) < 0.001 && Math.Abs(dLon - 5400) < 0.001)
                {
                    iScale = 250000;
                }

                //1:100000比例尺
                else if (Math.Abs(dLat - 1200) < 0.001 && Math.Abs(dLon - 1800) < 0.001)
                {
                    iScale = 100000;
                }

                //1:50000比例尺
                else if (Math.Abs(dLat - 600) < 0.001 && Math.Abs(dLon - 900) < 0.001)
                {
                    iScale = 50000;
                }

                //1:25000比例尺
                else if (Math.Abs(dLat - 300) < 0.001 && Math.Abs(dLon - 450) < 0.001)
                {
                    iScale = 25000;
                }

                //1:10000比例尺
                else if (Math.Abs(dLat - 150) < 0.001 && Math.Abs(dLon - 225) < 0.001)
                {
                    iScale = 10000;
                }

                //1:5000比例尺
                else if (Math.Abs(dLat - 75) < 0.001 && Math.Abs(dLon - 112.5) < 0.001)
                {
                    iScale = 5000;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iScale;
        }

        /// <summary>
        /// 根据图幅号获取比例尺
        /// </summary>
        /// <param name="sTFH">图幅号</param>
        /// <returns></returns>
        public static int GetScaleByTFH(string sTFH)
        {
            int iScale = 0;
            try
            {
                sTFH = sTFH.Trim();
                int Count = sTFH.Length;
                if (Count < 10)
                    return iScale;
                char[] Para = sTFH.ToCharArray();
                string ResultTFH = string.Empty;
                for (int i = 0; i < Count; i++)
                    if (Para[i] != ' ')
                        ResultTFH = ResultTFH + Para[i].ToString();
                sTFH = ResultTFH;
                if (sTFH.Length < 10)
                    return iScale;
                string sBLC = sTFH.Substring(3, 1);
                switch (sBLC.ToUpper())
                {
                    case "B":
                        iScale = 500000;
                        break;
                    case "C":
                        iScale = 250000;
                        break;
                    case "D":
                        iScale = 100000;
                        break;
                    case "E":
                        iScale = 50000;
                        break;
                    case "F":
                        iScale = 25000;
                        break;
                    case "G":
                        iScale = 10000;
                        break;
                    case "H":
                        iScale = 5000;
                        break;
                    case "I":
                        iScale = 2000;
                        break;
                    case "J":
                        iScale = 1000;
                        break;
                    case "K":
                        iScale = 500;
                        break;
                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iScale;
        }
        #endregion

        #region 计算矩形范围内图幅定点坐标
        /// <summary>
        /// 计算矩形范围内图幅定点坐标
        /// </summary>
        /// <param name="minLat">左下角纬度（秒）</param>
        /// <param name="minLon">左下角经度（秒）</param>
        /// <param name="maxLat">右上角纬度（秒）</param>
        /// <param name="maxLon">右上角经度（秒）</param>
        /// <param name="iScale">比例尺（秒）</param>
        /// <returns></returns>
        public static List<CoordinatePoint> GetCoordinatePointsFromRange(double minLat, double minLon, double maxLat, double maxLon, int iScale)
        {
            List<CoordinatePoint> pListCoordinatePoint = new List<CoordinatePoint>();
            try
            {
                double dDeviationLat = 0.0, dDeviationLon = 0.0;//图幅纬差、经差
                GetDeviation(iScale, ref dDeviationLat, ref dDeviationLon);
                ////将度分秒转换为秒以便于统一计算：
                dDeviationLat = TransFormMethod.DmsToSecond(dDeviationLat);
                dDeviationLon = TransFormMethod.DmsToSecond(dDeviationLon);

                double dStartLat = 0.0, dStartLon = 0.0;//范围内最小图幅纬度和经度
                double dRemainderLat = (double)((decimal)minLat % (decimal)dDeviationLat);//纬度余数
                double dRemainderLon = (double)((decimal)minLon % (decimal)dDeviationLon);//经度余数

                //计算范围内图幅最小纬度
                if (dRemainderLat == 0.0)
                {
                    dStartLat = minLat;
                }
                else
                {
                    dStartLat = minLat - dRemainderLat + dDeviationLat;
                }
                //计算范围内图幅最小经度
                if (dRemainderLon == 0.0)
                {
                    dStartLon = minLon;
                }
                else
                {
                    dStartLon = minLon - dRemainderLon + dDeviationLon;
                }
                //获取范围内图幅角点经纬度
                while (dStartLat <= maxLat)
                {

                    double dLon = dStartLon;
                    while (dLon <= maxLon)
                    {
                        CoordinatePoint pCoordinatePoint = new CoordinatePoint();
                        pCoordinatePoint.XorLat = TransFormMethod.SecondToDms(dStartLat);
                        pCoordinatePoint.YorLon = TransFormMethod.SecondToDms(dLon);
                        pListCoordinatePoint.Add(pCoordinatePoint);
                        dLon += dDeviationLon;
                    }
                    dStartLat += dDeviationLat;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pListCoordinatePoint;
        }
        #endregion

        #region 创建图幅几何
        /// <summary>
        /// 根据四角点创建图幅框面
        /// </summary>
        /// <param name="minLat">图幅左下角纬度（秒）</param>
        /// <param name="minLon">图幅左下角经度（秒）</param>
        /// <param name="maxLat">图幅右上角纬度（秒）</param>
        /// <param name="maxLon">图幅右上角经度（秒）</param>
        /// <param name="pEarth">坐标系</param>
        /// <param name="dCentralMeridian">中央经线</param>
        /// <param name="iScale">比例尺</param>
        /// <returns></returns>
        public static IPolygon CreateTFPolygon(double minLat, double minLon, double maxLat, double maxLon, EarthParams pEarth, double dAddY, double dCentralMeridian)
        {
            IPolygon pPolygon = new PolygonClass();
            try
            {
                IPointCollection pPointCol = pPolygon as IPointCollection;
                CreateTFPointCollection(pPointCol, minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pPolygon;
        }

        /// <summary>
        /// 根据四角点创建图幅框线
        /// </summary>
        /// <param name="minLat">图幅左下角纬度（秒）</param>
        /// <param name="minLon">图幅左下角经度（秒）</param>
        /// <param name="maxLat">图幅右上角纬度（秒）</param>
        /// <param name="maxLon">图幅右上角经度（秒）</param>
        /// <param name="pEarth">坐标系</param>
        /// <param name="dCentralMeridian">中央经线</param>
        /// <param name="iScale">比例尺</param>
        /// <returns></returns>
        public static IPolyline CreateTFPolline(double minLat, double minLon, double maxLat, double maxLon, EarthParams pEarth, double dAddy, double dCentralMeridian)
        {
            IPolyline pPolyLine = new PolylineClass();
            try
            {
                IPointCollection pPointCol = pPolyLine as IPointCollection;
                CreateTFPointCollection(pPointCol, minLat, minLon, maxLat, maxLon, pEarth, dAddy, dCentralMeridian);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pPolyLine;
        }

        /// <summary>
        /// 创建图幅坐标点
        /// </summary>
        /// <param name="pPointCol"></param>
        /// <param name="minLat"></param>
        /// <param name="minLon"></param>
        /// <param name="maxLat"></param>
        /// <param name="maxLon"></param>
        /// <param name="pEarth"></param>
        /// <param name="dAddY"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="iScale"></param>
        public static void CreateTFPointCollection(IPointCollection pPointCol, double minLat, double minLon, double maxLat, double maxLon, EarthParams pEarth, double dAddY, double dCentralMeridian)
        {
            try
            {
                IPoint pPoint = new PointClass();
                double dX = 0.0;
                double dY = 0.0;

                //从左上角开始,顺时针整秒处插入加密点
                ConvertDmsToSecond(pEarth, dCentralMeridian, minLon, maxLat, dAddY, out dX, out dY);
                pPoint.PutCoords(dX, dY);
                pPointCol.AddPoint(pPoint);
                double dTmpLon = 0.0, dTmpLat = 0.0;
                dTmpLon = Math.Ceiling(minLon);//向上取整
                if (dTmpLon == minLon)
                {
                    dTmpLon++;
                }
                while (dTmpLon < maxLon)
                {
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dTmpLon, maxLat, dAddY, out dX, out dY);
                    pPoint.PutCoords(dX, dY);
                    pPointCol.AddPoint(pPoint);
                    dTmpLon++;
                    //最后一个取整点如果大于最大经度，则取最大经度
                    if (dTmpLon >= maxLon)
                    {
                        ConvertDmsToSecond(pEarth, dCentralMeridian, maxLon, maxLat, dAddY, out dX, out dY);
                        pPoint.PutCoords(dX, dY);
                        pPointCol.AddPoint(pPoint);
                    }
                }

                //从右上角开始，顺时针整秒处插入加密点(不包含右上角点，右上角点已在上面循环中添加)
                dTmpLat = Math.Floor(maxLat);
                if (dTmpLat == maxLat)
                {
                    dTmpLat--;
                }
                while (dTmpLat > minLat)
                {
                    ConvertDmsToSecond(pEarth, dCentralMeridian, maxLon, dTmpLat, dAddY, out dX, out dY);
                    pPoint.PutCoords(dX, dY);
                    pPointCol.AddPoint(pPoint);
                    dTmpLat--;
                    //最后一个取整点如果大于最大经度，则取最大经度
                    if (dTmpLat <= minLat)
                    {
                        ConvertDmsToSecond(pEarth, dCentralMeridian, maxLon, minLat, dAddY, out dX, out dY);
                        pPoint.PutCoords(dX, dY);
                        pPointCol.AddPoint(pPoint);
                    }
                }

                //从右下角开始，顺时针整秒处插入加密点(不包含右下角点，右下角点已在上面循环中添加)
                dTmpLon = Math.Floor(maxLon);
                if (dTmpLon == maxLon)
                {
                    dTmpLon--;
                }
                while (dTmpLon > minLon)
                {
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dTmpLon, minLat, dAddY, out dX, out dY);
                    pPoint.PutCoords(dX, dY);
                    pPointCol.AddPoint(pPoint);
                    dTmpLon--;
                    //最后一个取整点如果大于最大经度，则取最大经度
                    if (dTmpLon <= minLon)
                    {
                        ConvertDmsToSecond(pEarth, dCentralMeridian, minLon, minLat, dAddY, out dX, out dY);
                        pPoint.PutCoords(dX, dY);
                        pPointCol.AddPoint(pPoint);
                    }
                }

                //从左下角开始，顺时针整秒处插入加密点(不包含左下角点，左下角点已在上面循环中添加)
                dTmpLat = Math.Ceiling(minLat);
                if (dTmpLat == minLat)
                {
                    dTmpLat++;
                }
                while (dTmpLat < maxLat)
                {
                    ConvertDmsToSecond(pEarth, dCentralMeridian, minLon, dTmpLat, dAddY, out dX, out dY);
                    pPoint.PutCoords(dX, dY);
                    pPointCol.AddPoint(pPoint);
                    dTmpLat++;
                    //最后一个取整点如果大于最大经度，则取最大经度
                    if (dTmpLat >= maxLat)
                    {
                        ConvertDmsToSecond(pEarth, dCentralMeridian, minLon, maxLat, dAddY, out dX, out dY);
                        pPoint.PutCoords(dX, dY);
                        pPointCol.AddPoint(pPoint);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 高斯正算X、Y坐标
        /// </summary>
        /// <param name="pSpr">坐标系</param>
        /// <param name="dCentralMeridian">中央经线</param>
        /// <param name="dLon">经度</param>
        /// <param name="dLat">纬度</param>
        /// <param name="x">输出X坐标</param>
        /// <param name="y">输出Y坐标</param>
        private static void ConvertDmsToSecond(EarthParams pEarth, double dCentralMeridian, double dLon, double dLat, double dAddY, out double x, out double y)
        {
            dLon = TransFormMethod.SecondToDegree(dLon);
            dLat = TransFormMethod.SecondToDegree(dLat);
            y = TransFormMethod.GaussPositiveX(dLat, dLon, dCentralMeridian, pEarth.A, pEarth.B);
            x = TransFormMethod.GaussPositiveY(dLat, dLon, dCentralMeridian, pEarth.A, pEarth.B, dAddY);
        }
        #endregion

        #region 生成外图框几何
        /// <summary>
        /// 根据内图廓和比例尺生成外图框线
        /// </summary>
        /// <param name="pInnerPolygn"></param>
        /// <param name="iScale"></param>
        /// <returns></returns>
        public static IPolyline CreateOutTKLine(IPolygon pInnerPolygn, int iScale)
        {
            IPolyline pPolyLine = null;
            try
            {
                IPolygon pOutPolygon = CreateOutTKPolygon(pInnerPolygn, iScale);
                pPolyLine = (pOutPolygon as ITopologicalOperator).Boundary as IPolyline;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pPolyLine;
        }

        /// <summary>
        /// 根据内图廓和比例尺生成外图框面
        /// </summary>
        /// <param name="pInnerPolygn"></param>
        /// <param name="iScale"></param>
        /// <returns></returns>
        public static IPolygon CreateOutTKPolygon(IPolygon pInnerPolygn, int iScale)
        {
            IPolygon pPolygon = null;
            try
            {
                object missing = Type.Missing;
                double dTKDistance = GetTKDistance(iScale);
                if (iScale >= 5000)
                {
                    IEnvelope pEnvelope = pInnerPolygn.Envelope;
                    double dMaxX = pEnvelope.XMax + dTKDistance;
                    double dMaxY = pEnvelope.YMax + dTKDistance;
                    double dMinX = pEnvelope.XMin - dTKDistance;
                    double dMinY = pEnvelope.YMin - dTKDistance;

                    pPolygon = new PolygonClass();
                    IPointCollection pPointCollection = pPolygon as IPointCollection;
                    IPoint pPoint = new PointClass();
                    //左上角点
                    pPoint.PutCoords(dMinX, dMaxY);
                    pPointCollection.AddPoint(pPoint, ref missing, ref missing);
                    //右上角点
                    pPoint.PutCoords(dMaxX, dMaxY);
                    pPointCollection.AddPoint(pPoint, ref missing, ref missing);
                    //右下角点
                    pPoint.PutCoords(dMaxX, dMinY);
                    pPointCollection.AddPoint(pPoint, ref missing, ref missing);
                    //左下角点
                    pPoint.PutCoords(dMinX, dMinY);
                    pPointCollection.AddPoint(pPoint, ref missing, ref missing);
                    //闭合
                    pPoint.PutCoords(dMinX, dMaxY);
                    pPointCollection.AddPoint(pPoint, ref missing, ref missing);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pPolygon;
        }

        /// <summary>
        /// 绘制图幅的四角短线和注记
        /// </summary>
        /// <param name="pFeatureclass"></param>
        /// <param name="pAnnoFeatureclass"></param>
        /// <param name="dMinLat"></param>
        /// <param name="dMinLon"></param>
        /// <param name="dMaxLat"></param>
        /// <param name="dMaxLon"></param>
        /// <param name="iScale"></param>
        /// <param name="pEarth"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="dAddY"></param>
        public static void Create4CornerLine(IFeatureClass pFeatureclass, IFeatureClass pAnnoFeatureclass,
                                            double dMinLat, double dMinLon, double dMaxLat, double dMaxLon,
                                            int iScale, EarthParams pEarth, double dCentralMeridian, double dAddY, IPolyline pOutTKLine)
        {
            IFeatureCursor pCreateCursor = null;
            try
            {
                decimal dFontSize = 8;//注记字体大小
                //注记要素集
                IElementCollection pElementColl = new ElementCollectionClass();
                IPoint pPoint = new PointClass();
                ITextElement pTextElement = null;
                string sText = string.Empty;
                if (pFeatureclass != null && pAnnoFeatureclass != null)
                {
                    ITextSymbol pTextSymbol = null;
                    pCreateCursor = pFeatureclass.Insert(true);
                    IFeatureBuffer pFeatureBuffer = pFeatureclass.CreateFeatureBuffer();

                    double dTKDistance = GetTKDistance(iScale);//获取内外图廓的距离
                    double dVerValue = dTKDistance / 2;//注记距离
                    double dX0 = 0.0, dY0 = 0.0, dHorX = 0.0, dHorY = 0.0, dVerX = 0.0, dVerY = 0.0;
                    IPolyline pHorLine = null, pVerLine = null;
                    #region 左上角
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dMinLon, dMaxLat, dAddY, out dX0, out dY0);
                    //横线
                    dHorX = dX0 - dTKDistance;
                    dHorY = dY0;
                    pHorLine = SquareNetComm.CreateShortLine(dX0, dY0, dHorX - dTKDistance, dHorY, pOutTKLine);
                    pFeatureBuffer.Shape = pHorLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);

                    //创建纬度注记
                    pPoint.PutCoords(dX0, dY0);
                    sText = ResolutionLat(dMaxLat, true);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVABottom);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    //pPoint.PutCoords(dX0, dY0);
                    sText = ResolutionLat(dMaxLat, false);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVATop);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    //竖线
                    dVerX = dX0;
                    dVerY = dY0 + dTKDistance;
                    pVerLine = SquareNetComm.CreateShortLine(dX0, dY0, dVerX, dVerY + dTKDistance, pOutTKLine);
                    pFeatureBuffer.Shape = pVerLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);

                    //创建经度注记
                    pPoint.PutCoords(dX0, dY0 + dVerValue);
                    sText = ResolutionLon(dMinLon);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHACenter, esriTextVerticalAlignment.esriTVACenter);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);
                    #endregion
                    #region 右上角
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dMaxLon, dMaxLat, dAddY, out dX0, out dY0);
                    //横线
                    dHorX = dX0 + dTKDistance;
                    dHorY = dY0;
                    pHorLine = SquareNetComm.CreateShortLine(dX0, dY0, dHorX + dTKDistance, dHorY, pOutTKLine);
                    pFeatureBuffer.Shape = pHorLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);

                    //创建纬度注记
                    pPoint.PutCoords(dX0, dY0);
                    sText = ResolutionLat(dMaxLat, true);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVABottom);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    sText = ResolutionLat(dMaxLat, false);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVATop);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    //竖线
                    dVerX = dX0;
                    dVerY = dY0 + dTKDistance;
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHACenter, esriTextVerticalAlignment.esriTVACenter);
                    pVerLine = SquareNetComm.CreateShortLine(dX0, dY0, dVerX, dVerY + dTKDistance, pOutTKLine);
                    pFeatureBuffer.Shape = pVerLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);

                    //创建经度注记
                    pPoint.PutCoords(dX0, dY0 + dVerValue);
                    sText = ResolutionLon(dMaxLon);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);
                    #endregion
                    #region 右下角
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dMaxLon, dMinLat, dAddY, out dX0, out dY0);
                    //横线
                    dHorX = dX0 + dTKDistance;
                    dHorY = dY0;
                    pHorLine = SquareNetComm.CreateShortLine(dX0, dY0, dHorX + dTKDistance, dHorY, pOutTKLine);
                    pFeatureBuffer.Shape = pHorLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);
                    //创建纬度注记
                    pPoint.PutCoords(dX0, dY0);
                    sText = ResolutionLat(dMinLat, true);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVABottom);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    sText = ResolutionLat(dMinLat, false);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVATop);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    //竖线
                    dVerX = dX0;
                    dVerY = dY0 - dTKDistance;
                    pVerLine = SquareNetComm.CreateShortLine(dX0, dY0, dVerX, dVerY - dTKDistance, pOutTKLine);
                    pFeatureBuffer.Shape = pVerLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);
                    //创建经度注记
                    pPoint.PutCoords(dX0, dY0 - dVerValue);
                    sText = ResolutionLon(dMaxLon);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHACenter, esriTextVerticalAlignment.esriTVACenter);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);
                    #endregion
                    #region 左下角
                    ConvertDmsToSecond(pEarth, dCentralMeridian, dMinLon, dMinLat, dAddY, out dX0, out dY0);
                    //横线
                    dHorX = dX0 - dTKDistance;
                    dHorY = dY0;
                    pHorLine = SquareNetComm.CreateShortLine(dX0, dY0, dHorX - dTKDistance, dHorY, pOutTKLine);
                    pFeatureBuffer.Shape = pHorLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);
                    //创建纬度注记
                    pPoint.PutCoords(dX0, dY0);
                    sText = ResolutionLat(dMinLat, true);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVABottom);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);

                    sText = ResolutionLat(dMinLat, false);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVATop);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);
                    //竖线
                    dVerX = dX0;
                    dVerY = dY0 - dTKDistance;
                    pVerLine = SquareNetComm.CreateShortLine(dX0, dY0, dVerX, dVerY - dTKDistance, pOutTKLine);
                    pFeatureBuffer.Shape = pVerLine;
                    pCreateCursor.InsertFeature(pFeatureBuffer);
                    //创建经度注记
                    pPoint.PutCoords(dX0, dY0 - dVerValue);
                    sText = ResolutionLon(dMinLon);
                    pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHACenter, esriTextVerticalAlignment.esriTVACenter);
                    pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                    pElementColl.Add(pTextElement as IElement);
                    #endregion
                    pCreateCursor.Flush();
                    AnnoComm.AddElementToFeatureclass(pAnnoFeatureclass, pElementColl);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pCreateCursor);
            }
        }

        /// <summary>
        /// 创建公里格网和图框间短线和注记
        /// </summary>
        /// <param name="pFeatureclass"></param>
        /// <param name="pAnnoFeatureclass"></param>
        /// <param name="pInnerPolygon"></param>
        /// <param name="iScale"></param>
        /// <param name="pEarth"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="dAddY"></param>
        /// <param name="iLength"></param>
        public static void CreateMeasuredGrid(IFeatureClass pFeatureclass, IFeatureClass pAnnoFeatureclass, IPolygon pInnerPolygon, int iScale, int iLength = 1000)
        {
            IFeatureCursor pCreateCursor = null;
            try
            {
                decimal dMaxSize = 8, dMinsize = 4;
                //注记要素集X
                IElementCollection pElementColl = new ElementCollectionClass();
                IPoint pPoint = new PointClass();
                ITextElement pTextElement = null;
                string sText = string.Empty;
                if (pFeatureclass != null && pAnnoFeatureclass != null)
                {
                    ITextSymbol pTextSymbol = null;
                    pCreateCursor = pFeatureclass.Insert(true);
                    IFeatureBuffer pFeatureBuffer = pFeatureclass.CreateFeatureBuffer();
                    IPolyline pInnerLine = (pInnerPolygon as ITopologicalOperator).Boundary as IPolyline;//内图廓线
                    double dMinX = 0.0, dMinY = 0.0, dMaxX = 0.0, dMaxY = 0.0;//图幅最大和最小坐标
                    dMinX = pInnerPolygon.Envelope.XMin;
                    dMinY = pInnerPolygon.Envelope.YMin;
                    dMaxX = pInnerPolygon.Envelope.XMax;
                    dMaxY = pInnerPolygon.Envelope.YMax;

                    double dTKDistance = GetTKDistance(iScale);//获取内外图廓的距离
                    double dValue = dTKDistance / 6;//注记距离
                    double dShortX = 0.0, dShortY = 0.0;
                    IPolyline pShortLine = null;
                    #region 图幅上下短线和注记
                    double dRemainderX = dMinX % iLength;
                    double dStartX = dMinX - dRemainderX + iLength;
                    double dNowX = dStartX;
                    while (dNowX < dMaxX)
                    {
                        //上短线
                        dShortY = dMaxY + dTKDistance;
                        pShortLine = SquareNetComm.CreateShortLine(dNowX, dShortY, dNowX, dMaxY - dTKDistance, pInnerLine);
                        pFeatureBuffer.Shape = pShortLine;
                        pCreateCursor.InsertFeature(pFeatureBuffer);
                        //方里网注记
                        pPoint.PutCoords(dNowX, dShortY - dValue);
                        if (dStartX == dNowX || (dNowX + iLength) >= dMaxX)
                        {
                            sText = ResolutionXYMax(dNowX);
                            pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMinsize, esriTextHorizontalAlignment.esriTHARight);
                            pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                            pElementColl.Add(pTextElement as IElement);
                        }

                        sText = ResolutionXYMin(dNowX);
                        pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMaxSize, esriTextHorizontalAlignment.esriTHALeft);
                        pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                        pElementColl.Add(pTextElement as IElement);

                        //下短线
                        dShortY = dMinY - dTKDistance;
                        pShortLine = SquareNetComm.CreateShortLine(dNowX, dShortY, dNowX, dMinY + dTKDistance, pInnerLine);
                        pFeatureBuffer.Shape = pShortLine;
                        pCreateCursor.InsertFeature(pFeatureBuffer);
                        //方里网注记
                        pPoint.PutCoords(dNowX, dShortY + dValue);
                        if (dStartX == dNowX || (dNowX + iLength) >= dMaxX)
                        {
                            sText = ResolutionXYMax(dNowX);
                            pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMinsize, esriTextHorizontalAlignment.esriTHARight);
                            pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                            pElementColl.Add(pTextElement as IElement);
                        }
                        sText = ResolutionXYMin(dNowX);
                        pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMaxSize, esriTextHorizontalAlignment.esriTHALeft);
                        pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                        pElementColl.Add(pTextElement as IElement);

                        dNowX += iLength;
                    }

                    #endregion

                    #region 图幅左右短线和注记
                    double dRemainderY = dMinY % iLength;
                    double dStartY = dMinY - dRemainderY + iLength;
                    double dNowY = dStartY;
                    while (dNowY < dMaxY)
                    {
                        //左短线
                        dShortX = dMinX - dTKDistance;
                        pShortLine = SquareNetComm.CreateShortLine(dShortX, dNowY, dMinX + dTKDistance, dNowY, pInnerLine);
                        pFeatureBuffer.Shape = pShortLine;
                        pCreateCursor.InsertFeature(pFeatureBuffer);
                        //方里网注记
                        pPoint.PutCoords(dShortX + dValue, dNowY);
                        if (dNowY == dStartY || (dNowY + iLength) >= dMaxY)
                        {
                            sText = ResolutionXYMax(dNowY);
                            pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMinsize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVABottom);
                            pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                            pElementColl.Add(pTextElement as IElement);
                        }
                        sText = ResolutionXYMin(dNowY);
                        pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMaxSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVABottom);
                        pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                        pElementColl.Add(pTextElement as IElement);
                        //右短线
                        dShortX = dMaxX + dTKDistance;
                        pShortLine = SquareNetComm.CreateShortLine(dShortX, dNowY, dMaxX - dTKDistance, dNowY, pInnerLine);
                        pFeatureBuffer.Shape = pShortLine;
                        pCreateCursor.InsertFeature(pFeatureBuffer);
                        //方里网注记
                        pPoint.PutCoords(dShortX - dValue - dValue, dNowY);
                        if (dNowY == dStartY || (dNowY + iLength) >= dMaxY)
                        {
                            sText = ResolutionXYMax(dNowY);
                            pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMinsize, esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment.esriTVABottom);
                            pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                            pElementColl.Add(pTextElement as IElement);
                        }
                        sText = ResolutionXYMin(dNowY);
                        pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dMaxSize, esriTextHorizontalAlignment.esriTHALeft, esriTextVerticalAlignment.esriTVABottom);
                        pTextElement = CreateTextElement(pPoint, sText, pTextSymbol);
                        pElementColl.Add(pTextElement as IElement);

                        dNowY += iLength;
                    }
                    AnnoComm.AddElementToFeatureclass(pAnnoFeatureclass, pElementColl);
                    #endregion

                }
            }
            catch (Exception ex)
            {
                throw new Exception("生成公里格网失败，" + ex.Message);
            }
            finally
            {
                AEComm.ReleaseCOMObject(pCreateCursor);
            }
        }

        /// <summary>
        /// 注记图层添加图幅号注记
        /// </summary>
        /// <param name="pAnnoFeatureclass"></param>
        /// <param name="sTFH"></param>
        /// <param name="pEarth"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="dAddY"></param>
        /// <param name="pOutTKLine"></param>
        public static void CreateTFHAnno(IFeatureClass pAnnoFeatureclass,string sTFH,EarthParams pEarth, 
                                        double dCentralMeridian, double dAddY)
        {
            IFeatureCursor pCreateCursor = null;
            try
            {
                decimal dFontSize = 11;//注记字体大小
                //注记要素集
                IElementCollection pElementColl = new ElementCollectionClass();
                ITextElement pTextElement = null;
                if (pAnnoFeatureclass != null && !string.IsNullOrWhiteSpace(sTFH))
                {
                    ITextSymbol pTextSymbol = null;
                    int iScale = TFComm.GetScaleByTFH(sTFH);
                    double minLat=0.0, minLon=0.0, maxLat=0.0, maxLon=0.0;
                    if (TFComm.GetFourDSFromTFH(sTFH, ref minLon, ref minLat, ref maxLon, ref maxLat))
                    {
                       IPolygon pInnerPolygon= CreateTFPolygon(minLat, minLon, maxLat, maxLon, pEarth, dAddY, dCentralMeridian);
                       IPolygon pOutPlygon = CreateOutTKPolygon(pInnerPolygon, iScale);
                       if (pOutPlygon!=null)
                       {
                           IEnvelope pEnvelope = pOutPlygon.Envelope;
                           if (pEnvelope!=null)
                           {
                               IPoint pPoint = new PointClass();
                               pPoint.PutCoords((pEnvelope.XMin+pEnvelope.XMax)/2,pEnvelope.YMax+GetTKDistance(iScale));
                               pTextSymbol = CreateAnnoSymbol(pAnnoFeatureclass, dFontSize, esriTextHorizontalAlignment.esriTHACenter, esriTextVerticalAlignment.esriTVACenter);
                               pTextElement = CreateTextElement(pPoint, sTFH, pTextSymbol);
                               pElementColl.Add(pTextElement as IElement);

                               AnnoComm.AddElementToFeatureclass(pAnnoFeatureclass, pElementColl);
                           }
                       }
                    }
                    
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                AEComm.ReleaseCOMObject(pCreateCursor);
            }
        }

        /// <summary>
        /// 创建文本元素
        /// </summary>
        /// <param name="pGC"></param>
        /// <param name="pGeo"></param>
        /// <param name="sText"></param>
        public static ITextElement CreateTextElement(IGeometry pGeo, string sText, ITextSymbol pTextSymbol)
        {
            try
            {
                ITextElement textElement = new TextElementClass();
                textElement.ScaleText = true;
                textElement.Symbol = pTextSymbol;
                textElement.Text = sText;
                IElement element = textElement as IElement;
                element.Geometry = pGeo;
                return textElement;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析纬度
        /// </summary>
        /// <param name="dLat"></param>
        /// <param name="isUp"></param>
        /// <returns></returns>
        public static string ResolutionLat(double dLat, bool isUp)
        {
            try
            {
                if (isUp)
                {
                    double dGree = TransFormMethod.SecondToDegree(dLat);
                    return Math.Floor(dGree).ToString() + "°";
                }
                else
                {
                    double dDMS = TransFormMethod.SecondToDms(dLat);
                    decimal num = (int)dDMS;
                    decimal num2 = (int)(((dDMS - ((int)dDMS)) * 100.0) + 0.001);
                    decimal num5 = (decimal)(dDMS * 100.0);
                    string str = string.IsNullOrEmpty(((decimal)num5).ToString("#.###")) ? "0.0" : ((decimal)num5).ToString("#.###");
                    decimal num4 = (decimal)(dDMS * 100.0);
                    int num6 = (int)double.Parse(str);
                    decimal num3 = (decimal)((num4 - num6) * 100.0m);
                    num3 = Math.Round(num3, 0);
                    return num2.ToString() + "′" + num3.ToString() + "″";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析经度
        /// </summary>
        /// <param name="dLat"></param>
        /// <param name="isUp"></param>
        /// <returns></returns>
        public static string ResolutionLon(double dLon)
        {
            try
            {
                double dDMS = TransFormMethod.SecondToDms(dLon);
                decimal num = (int)dDMS;
                decimal num2 = (int)(((dDMS - ((int)dDMS)) * 100.0) + 0.001);
                decimal num5 = (decimal)(dDMS * 100.0);
                string str = string.IsNullOrEmpty(((decimal)num5).ToString("#.###")) ? "0.0" : ((decimal)num5).ToString("#.###");
                decimal num4 = (decimal)(dDMS * 100.0);
                int num6 = (int)double.Parse(str);
                decimal num3 = (decimal)((num4 - num6) * 100.0m);
                num3 = Math.Round(num3, 0);
                return num.ToString() + "°" + num2.ToString() + "′" + num3.ToString() + "″";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析公里网右侧注记值
        /// </summary>
        /// <param name="dXorY"></param>
        /// <returns></returns>
        public static string ResolutionXYMin(double dXorY)
        {
            try
            {
                string sMax = Math.Round((dXorY / 1000.0), 1).ToString("0.0");
                return sMax.Substring(sMax.Length - 4);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析公里网左侧注记值
        /// </summary>
        /// <param name="dXorY"></param>
        /// <returns></returns>
        public static string ResolutionXYMax(double dXorY)
        {
            try
            {
                string sMax = Math.Round((dXorY / 1000.0), 1).ToString("0.0");
                return sMax.Substring(0, sMax.Length - 4);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 内外图框间距
        /// </summary>
        /// <param name="iScale">比例尺</param>
        /// <returns></returns>
        public static double GetTKDistance(int iScale)
        {
            if (iScale < 5000)
            {
                return 0.015 * iScale;
            }
            else if (iScale == 5000)
            {
                return 0.0095 * iScale;
            }
            else
            {
                return 0.014 * iScale;
            }
        }

        /// <summary>
        /// 根据注记图层的样式创建注记样式
        /// </summary>
        /// <param name="pAnnoFeatureClass"></param>
        /// <param name="dFontSize"></param>
        /// <returns></returns>
        public static ITextSymbol CreateAnnoSymbol(IFeatureClass pAnnoFeatureClass, decimal dFontSize = 8, esriTextHorizontalAlignment pHA = esriTextHorizontalAlignment.esriTHARight, esriTextVerticalAlignment pVA = esriTextVerticalAlignment.esriTVACenter)
        {
            ITextSymbol symbol = null;
            try
            {
                IAnnoClass pAnnoClass = pAnnoFeatureClass.Extension as IAnnoClass;
                if (pAnnoClass == null)
                {
                    symbol = new TextSymbolClass();
                }
                else
                {
                    symbol = pAnnoClass.Symbol[0] as ITextSymbol;
                }
                stdole.IFontDisp pFont = new stdole.StdFontClass() as stdole.IFontDisp;
                pFont.Size = dFontSize;
                //symbol.Font.Size = dFontSize;
                symbol.Font = pFont;
                symbol.HorizontalAlignment = pHA;
                symbol.VerticalAlignment = pVA;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return symbol;
        }
        #endregion
    }
}
