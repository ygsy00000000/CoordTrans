using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 转换算法
    /// </summary>
    public class TransFormMethod
    {
        /// <summary>
        /// PI
        /// </summary>
        public const double L_AREACAL_PI = 3.14159265358979;

        #region 度、度分秒、弧度之间的转化
        /// <summary>
        /// 度转弧度
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static double DegreeToArc(double D)
        {
            return D / 180 * L_AREACAL_PI;
        }

        /// <summary>
        /// 弧度转度
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double ArcToDegree(double A)
        {
            return A * 180 / L_AREACAL_PI;
        }

        /// <summary>
        /// 度转度分秒
        /// </summary>
        /// <param name="Degree"></param>
        /// <returns></returns>
        public static double DegreeToDms(double Degree)
        {
            decimal num = (int)Degree;
            decimal num2 = (int)(((decimal)Degree - num) * 60.0m);
            decimal num3 = (decimal)(((((decimal)Degree - num) * 60.0m) - ((int)(((decimal)Degree - num) * 60.0m))) * 60.0m);
            return (double)((num + (num2 / 100.0m)) + (num3 / 10000.0m));
        }

        /// <summary>
        /// 度分秒转度
        /// </summary>
        /// <param name="Dms"></param>
        /// <returns></returns>
        public static double DmsToDegree(double Dms)
        {
            return (double)(DmsToSecond(Dms) / 3600.0);
        }

        /// <summary>
        /// 度分秒转秒
        /// </summary>
        /// <param name="dms"></param>
        /// <returns></returns>
        public static double DmsToSecond(double dms)
        {
            decimal num = (int)dms;
            decimal num2 = (int)(((dms - ((int)dms)) * 100.0) + 0.001);
            decimal num5 = (decimal)(dms * 100.0);
            string str = string.IsNullOrEmpty(((decimal)num5).ToString("#.###")) ? "0.0" : ((decimal)num5).ToString("#.###");
            decimal num4 = (decimal)(dms * 100.0);
            int num6 = (int)double.Parse(str);
            decimal num3 = (decimal)((num4 - num6) * 100.0m);
            return (double)(((num * 3600.0m) + (num2 * 60.0m)) + num3);
        }

        /// <summary>
        /// 度转秒
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static double DegreeToSecond(double D)
        {
            return D * 3600.0;
        }

        /// <summary>
        /// 秒转度
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static double SecondToDegree(double S)
        {
            return S / 3600.0;
        }

        /// <summary>
        /// 度分秒转弧度
        /// </summary>
        /// <param name="Dms"></param>
        /// <returns></returns>
        public static double DmsToArc(double Dms)
        {
            double D = DmsToDegree(Dms);
            return DegreeToArc(D);
        }

        /// <summary>
        /// 弧度转度分秒
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double ArcToDms(double A)
        {
            double D = ArcToDegree(A);
            return DegreeToDms(D);
        }

        /// <summary>
        /// 秒转度分秒
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double SecondToDms(double S)
        {
            double D = SecondToDegree(S);
            return DegreeToDms(D);
        }
        #endregion

        #region 椭球参数计算
        /// <summary>
        /// 获取椭球参数短半轴B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static double GetB(double A, double F)
        {
            double B = 0.0;
            try
            {
                if (A.Equals(0) || F.Equals(0))
                {
                    return B;
                }
                B = A * (1 - 1.0 / F);
            }
            catch (Exception ex)
            {
                return 0.0;
            }
            return B;
        }

        /// <summary>
        /// 椭球第一偏心率的平方
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static double GetE2(double A, double B)
        {
            double E2 = 0.0;
            try
            {
                if (A.Equals(0) || B.Equals(0))
                {
                    return E2;
                }
                E2 = (Math.Pow(A, 2.0) - Math.Pow(B, 2.0)) / Math.Pow(A, 2.0);
            }
            catch (Exception ex)
            {
                return 0.0;
            }
            return E2;
        }

        #region 计算弧长计算的四参数
        public static double GetK1(double A, double E2)
        {
            double dValue = 0.0;
            try
            {
                if (A.Equals(0.0))
                {
                    return dValue;
                }
                double num = A * (1 - E2);
                double ParamA = GetParamA(E2);
                dValue = num * ParamA;
                double dd = dValue * L_AREACAL_PI / 180.0;
            }
            catch (Exception ex)
            {
                return 0.0;
            }
            return dValue;
        }

        public static double GetK2(double A, double E2)
        {
            double dValue = 0.0;
            try
            {
                if (A.Equals(0.0))
                {
                    return dValue;
                }
                double num = A * (1 - E2);
                dValue = num * (GetParamC(E2) - GetParamB(E2) - GetParamD(E2));
            }
            catch (Exception ex)
            {

            }
            return dValue;
        }

        public static double GetK3(double A, double E2)
        {
            double dValue = 0.0;
            try
            {
                if (A.Equals(0.0))
                {
                    return dValue;
                }
                double num = A * (1 - E2);
                dValue = num * ((16.0 / 3.0) * GetParamD(E2) - 2.0 * GetParamC(E2));
            }
            catch (Exception ex)
            {

            }
            return dValue;
        }

        public static double GetK4(double A, double E2)
        {
            double dValue = 0.0;
            try
            {
                if (A.Equals(0))
                {
                    return dValue;
                }
                double num = A * (1 - E2);
                dValue = num * (-(16.0 / 3.0) * GetParamD(E2));
            }
            catch (Exception ex)
            {

            }
            return dValue;
        }

        public static double GetParamA(double E2)
        {
            double dValue = 1 + (3.0 / 4.0) * E2 + (45.0 / 64.0) * Math.Pow(E2, 2.0) + (175.0 / 256.0) * Math.Pow(E2, 3.0)
                              + (11025.0 / 16384.0) * Math.Pow(E2, 4.0) + (43659.0 / 65536.0) * Math.Pow(E2, 5.0)
                              + (693693.0 / 1048576.0) * Math.Pow(E2, 6.0);
            return dValue;
        }
        public static double GetParamB(double E2)
        {
            double dValue = (3.0 / 4.0) * E2 + (15.0 / 16.0) * Math.Pow(E2, 2.0) + (525.0 / 512.0) * Math.Pow(E2, 3.0)
                              + (2205.0 / 2048.0) * Math.Pow(E2, 4.0) + (72765.0 / 65536.0) * Math.Pow(E2, 5.0);
            //+ (297297.0 / 262144.0) * Math.Pow(E2, 6.0);
            return dValue;
        }
        public static double GetParamC(double E2)
        {
            double dValue = (15.0 / 64.0) * Math.Pow(E2, 2.0) + (105.0 / 256.0) * Math.Pow(E2, 3.0)
                              + (2205.0 / 4096.0) * Math.Pow(E2, 4.0) + (10359.0 / 16384.0) * Math.Pow(E2, 5.0);
            return dValue;
        }
        public static double GetParamD(double E2)
        {
            double dValue = (35.0 / 512.0) * Math.Pow(E2, 3.0) + (315.0 / 2048.0) * Math.Pow(E2, 4.0) + (31185.0 / 13072.0) * Math.Pow(E2, 5.0); ;
            return dValue;
        }
        #endregion
        #endregion

        #region 高斯正算
        /// <summary>
        /// 高斯正算y坐标值(坐标形式：度)
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Lon"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static double GaussPositiveX(double Lat, double Lon, double dCentralMeridian, double A, double B)
        {
            double dValue = 0.0;
            try
            {
                double E2 = GetE2(A, B);                                            //// 第一偏心率的平方
                double R_Lat = DegreeToArc(Lat);                                     //// 纬度弧度
                double R_Lon = DegreeToArc(Lon);                                     //// 经度弧度
                double R_CentralMeridian = DegreeToArc(dCentralMeridian);            //// 中央经线弧度

                //// 经线弧长
                double x0 = GetK1(A, E2) * R_Lat + Math.Cos(R_Lat) * (GetK2(A, E2) * Math.Sin(R_Lat)
                          + GetK3(A, E2) * Math.Pow(Math.Sin(R_Lat), 3) + GetK4(A, E2) * Math.Pow(Math.Sin(R_Lat), 5));

                //double x1 = A * (1 - E2) * (NewTransForm.GetA1(E2) * R_Lat - NewTransForm.GetB1(E2) * Math.Sin(2 * R_Lat)
                //            + NewTransForm.GetC1(E2) * Math.Sin(4 * R_Lat) - NewTransForm.GetD1(E2) * Math.Sin(6 * R_Lat)
                //            + NewTransForm.GetE1(E2) * Math.Sin(8 * R_Lat) - NewTransForm.GetF1(E2) * Math.Sin(10 * R_Lat)
                //            + NewTransForm.GetG1(E2) * Math.Sin(12 * R_Lat));

                double l = R_Lon - R_CentralMeridian;                            //// 经差
                double t = Math.Sin(R_Lat) / Math.Cos(R_Lat);                    //// tanB
                double m = l * Math.Cos(R_Lat);                                  //// lcosB
                double g2 = E2 / (1 - E2) * Math.Pow(Math.Cos(R_Lat), 2);         //// e2/(1-e2)*cosB*cosB
                double N = A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(R_Lat), 2));    //// A/根号下（1-e2*sinB*sinB）

                dValue = x0 + N * t * (1.0 / 2.0 * Math.Pow(m, 2.0) + 1 / 24.0 * (5.0 - t * t + 9 * g2 + 4 * g2 * g2) * Math.Pow(m, 4.0)
                            + 1.0 / 720.0 * (61.0 - 58.0 * t * t + Math.Pow(t, 4.0)) * Math.Pow(m, 6.0)
                            + 1.0 / 24.0 * (9 - 11 * t * t) * g2 * Math.Pow(m, 6.0));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dValue;
        }

        /// <summary>
        /// 高斯正算X坐标值(坐标形式：度)
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Lon"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="dFalseEasting"></param>
        /// <returns></returns>
        public static double GaussPositiveY(double Lat, double Lon, double dCentralMeridian, double A, double B, double dFalseEasting)
        {
            double dValue = 0.0;
            try
            {
                double E2 = GetE2(A, B);                                                        //// 第一偏心率的平方
                double R_Lat = DegreeToArc(Lat);                                     //// 纬度弧度
                double R_Lon = DegreeToArc(Lon);                                     //// 经度弧度
                double R_CentralMeridian = DegreeToArc(dCentralMeridian);            //// 中央经线弧度

                double l = R_Lon - R_CentralMeridian;                                           //// 经差
                double t = Math.Sin(R_Lat) / Math.Cos(R_Lat);                                   //// tanB
                double m = l * Math.Cos(R_Lat);                                                 //// lcosB
                double g2 = E2 / (1 - E2) * Math.Pow(Math.Cos(R_Lat), 2);                       //// e2/(1-e2)*cosB*cosB
                double N = A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(R_Lat), 2));                //// A/根号下（1-e2*sinB*sinB）

                dValue = N * m * (1 + 1 / 6.0 * (1.0 - t * t + g2) * m * m
                                 + 1 / 120.0 * (5.0 - 18.0 * t * t + Math.Pow(t, 4.0) + 14.0 * g2 - 58.0 * g2 * t * t) * Math.Pow(m, 4.0)
                                 + 1 / 5040.0 * (61 - 479 * t * t + 179 * Math.Pow(t, 4.0) - Math.Pow(t, 6.0)) * Math.Pow(m, 6.0));
                dValue = dValue + dFalseEasting;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dValue;
        }
        #endregion

        #region 高斯反算
        /// <summary>
        /// 高斯反算纬度(坐标形式：度)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="dFalseEasting"></param>
        /// <returns></returns>
        public static double GaussNegativeB(double x, double y, double dCentralMeridian, double A, double B, double dFalseEasting)
        {
            double dValue = 0.0;
            try
            {
                double E2 = GetE2(A, B);                                                        //// 第一偏心率的平方
                double R_CentralMeridian = DegreeToArc(dCentralMeridian);                       //// 中央经线弧度
                double y0 = y - dFalseEasting;                                                  //// 公式中对应的y
                double x0 = x;                                                                  //// 公式中对应的x

                double K1 = GetK1(A, E2);
                double K2 = GetK2(A, E2);
                double K3 = GetK3(A, E2);
                double K4 = GetK4(A, E2);

                double Bf0 = x0 / K1;                                                 //// Bf0
                double Bf = 0.0;

                //// 经线弧长   4.8E-11M,1E-09
                while (Math.Abs(Bf - Bf0) > 1E-09)
                {
                    Bf = Bf0;
                    Bf0 = (x0 - (Math.Cos(Bf) *
                        (K2 * Math.Sin(Bf) + K3 * Math.Pow(Math.Sin(Bf), 3.0) + K4 * Math.Pow(Math.Sin(Bf), 5.0)
                        ))) / K1;
                }

                Bf = Bf0;
                double tf = Math.Sin(Bf) / Math.Cos(Bf);                                   //// tanB
                double gf2 = E2 / (1 - E2) * Math.Pow(Math.Cos(Bf), 2);                       //// e2/(1-e2)*cosB*cosB
                double Nf = A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Bf), 2));                //// A/根号下（1-e2*sinB*sinB）
                double Vf2 = 1 + gf2;

                dValue = Bf - Vf2 * tf * Math.Pow(y0 / Nf, 2.0) / 2.0 + Vf2 * tf * Math.Pow(y0 / Nf, 4.0) * (5.0 + 3.0 * tf * tf + gf2 - 9.0 * gf2 * tf * tf) / 24.0
                            - Vf2 * tf * Math.Pow(y0 / Nf, 6.0) * (61.0 + 90.0 * tf * tf + 45.0 * Math.Pow(tf, 4.0)) / 720.0;
                //// 弧度转为度
                dValue = ArcToDegree(dValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dValue;
        }

        /// <summary>
        /// 高斯反算经度(坐标形式：度)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dCentralMeridian"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="dFalseEasting"></param>
        /// <returns></returns>
        public static double GaussNegativeL(double x, double y, double dCentralMeridian, double A, double B, double dFalseEasting)
        {
            double dValue = 0.0;
            try
            {
                double E2 = GetE2(A, B);                                                        //// 第一偏心率的平方
                double R_CentralMeridian = DegreeToArc(dCentralMeridian);            //// 中央经线弧度
                double y0 = y - dFalseEasting;                                                  //// 公式中对应的y
                double x0 = x;                                                                  //// 公式中对应的x

                double K1 = GetK1(A, E2);
                double K2 = GetK2(A, E2);
                double K3 = GetK3(A, E2);
                double K4 = GetK4(A, E2);

                double Bf0 = x0 / K1;                                                 //// Bf0
                double Bf = 0.0;

                //// 经线弧长   4.8E-11M,1E-09
                while (Math.Abs(Bf - Bf0) > 1E-09)
                {
                    Bf = Bf0;
                    Bf0 = (x0 - (Math.Cos(Bf) *
                        (K2 * Math.Sin(Bf) + K3 * Math.Pow(Math.Sin(Bf), 3.0) + K4 * Math.Pow(Math.Sin(Bf), 5.0)
                        ))) / K1;
                }

                Bf = Bf0;
                double tf = Math.Sin(Bf) / Math.Cos(Bf);                                   //// tanB
                double gf2 = E2 / (1 - E2) * Math.Pow(Math.Cos(Bf), 2);                       //// e2/(1-e2)*cosB*cosB
                double Nf = A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Bf), 2));                //// A/根号下（1-e2*sinB*sinB）

                dValue = (y0 / Nf - (1 + 2 * tf * tf + gf2) * Math.Pow(y0 / Nf, 3.0) / 6.0
                          + (5.0 + 28.0 * tf * tf + 24.0 * Math.Pow(tf, 4.0) + 6.0 * gf2 + 8.0 * tf * tf * gf2) * Math.Pow(y0 / Nf, 5.0) / 120.0) / Math.Cos(Bf);
                dValue += R_CentralMeridian;
                //// 弧度转为度
                dValue = ArcToDegree(dValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dValue;
        }
        #endregion

        #region 计算四参数
        /// <summary>
        /// 计算四参数
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public static FourParams2D Compute4Params(ref List<CoordinatePointPair> pairs)
        {
            FourParams2D pFourParams = new FourParams2D();
            try
            {
                int count = pairs.Count;                           //// 坐标点对个数

                double[,] L = new double[count * 2, 1];            //// 矩阵L   2Count X 1 的矩阵
                double[,] B = new double[count * 2, 4];            //// 矩阵B   2Count X 4 的矩阵

                #region 矩阵赋值
                //// 矩阵赋值
                for (int i = 0; i < count; i++)
                {
                    CoordinatePointPair pair = pairs[i];
                    //// L矩阵赋值
                    L[2 * i, 0] = pair.XorLat2 - pair.XorLat1;           //// 新旧X值之差
                    L[2 * i + 1, 0] = pair.YorLon2 - pair.YorLon1;       //// 新旧Y值之差

                    //// B矩阵赋值
                    //// 第一行
                    B[2 * i, 0] = 1;
                    B[2 * i, 1] = 0;
                    B[2 * i, 2] = pair.YorLon1;                          //// 旧值Y
                    B[2 * i, 3] = pair.XorLat1;                          //// 旧值X
                    //// 第二行
                    B[2 * i + 1, 0] = 0;
                    B[2 * i + 1, 1] = 1;
                    B[2 * i + 1, 2] = -pair.XorLat1;                     //// 旧值-X
                    B[2 * i + 1, 3] = pair.YorLon1;                      //// 旧值Y
                }
                #endregion

                //// 最小二乘法求得四参数矩阵
                double[,] BT = MatrixTranspose(B);                 //// 转置矩阵
                double[,] BTL = MatrixMultiply(BT, L);             //// BT*L
                double[,] BTB = MatrixMultiply(BT, B);             //// BT*B
                double[,] VBTB = Inverse(BTB);               //// BT*B的逆矩阵
                if (VBTB == null)
                {
                    return null;
                }

                double[,] dValue = MatrixMultiply(VBTB, BTL);
                pFourParams.DX = dValue[0, 0];
                pFourParams.DY = dValue[1, 0];

                double c = dValue[2, 0];                        //// 根据公式 c = (1+k)*sinA 
                double d = dValue[3, 0];                        //// 根据公式 1 + d = (1+k)*cosA

                pFourParams.Angle = ArcToDegree(-Math.Atan2(c, 1 + d)) * 3600;       //// 弧度转秒
                pFourParams.ScaleK = Math.Sqrt(c * c + (1 + d) * (1 + d));          //// 1 + k

                ////// 设置残差
                //double[,] V = MatrixPlus(MatrixMultiply(B, dValue), MatrixPly(L, -1.0));
                //for (int i = 0; i < count; i++)
                //{
                //    CoordinatePointPair pair = pairs[i];
                //    pair.ResidualX = V[2 * i, 0];
                //    pair.ResidualY = V[2 * i + 1, 0];
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pFourParams;
        }

        #endregion

        #region 计算七参数
        /// <summary>
        /// 计算七参数（X,Y,Z）
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public static SevenParams Compute7Params(EarthParams earth_old, EarthParams earth_new, List<CoordinatePointPair> pairs, EnumTransFormModel model = EnumTransFormModel.Bursa)
        {
            SevenParams pSevenParams = new SevenParams();
            try
            {
                if (model == EnumTransFormModel.Molodensky)
                {
                    pSevenParams = new MolodenskyParams();
                }

                int count = pairs.Count;                           //// 坐标点对个数
                if (pairs == null || pairs.Count == 0)
                {
                    return null;
                }

                double[,] L = GetMatrixL(earth_old, earth_new, pairs, model);                              //// 矩阵L   3Count X 1 的矩阵
                double[,] B = GetMatrixB(earth_old, earth_new, pairs, ref pSevenParams, model);            //// 矩阵B   3Count X 7 的矩阵

                double[,] dValue = LeastSquare(B, L);
                pSevenParams.DX = dValue[0, 0];
                pSevenParams.DY = dValue[1, 0];
                pSevenParams.DZ = dValue[2, 0];
                pSevenParams.ScaleK = dValue[3, 0] * Math.Pow(10, 6);                     //// 单位ppm
                //// 单位：秒
                pSevenParams.AngleX = dValue[4, 0];
                pSevenParams.AngleY = dValue[5, 0];
                pSevenParams.AngleZ = dValue[6, 0];
                if (model != EnumTransFormModel._2D7Params && model != EnumTransFormModel._3D7Params)
                {
                    pSevenParams.AngleX = ArcToDegree(dValue[4, 0]) * 3600;
                    pSevenParams.AngleY = ArcToDegree(dValue[5, 0]) * 3600;
                    pSevenParams.AngleZ = ArcToDegree(dValue[6, 0]) * 3600;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pSevenParams;
        }

        /// <summary>
        /// 根据公共坐标点对和转换模型获取转换矩阵B
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static double[,] GetMatrixB(EarthParams earth_old, EarthParams earth_new, List<CoordinatePointPair> pairs, ref SevenParams pSevenParams, EnumTransFormModel model = EnumTransFormModel.Bursa)
        {
            if (pairs == null || pairs.Count == 0)
            {
                return null;
            }
            double[,] B = null;            //// 矩阵B   3Count X 7 的矩阵
            try
            {
                double p = 180.0 * 3600 / L_AREACAL_PI;            //// 弧度秒   180.0 * 3600 / L_AREACAL_PI   p = 1.0; 
                double E2 = GetE2(earth_old.A, earth_old.B);
                double W = 0.0, N = 0.0, M = 0.0;
                double Lat1 = 0.0, Lon1 = 0.0, H1 = 0.0;

                switch (model)
                {
                    case EnumTransFormModel._3D7Params:
                        #region 三维七参数
                        B = new double[pairs.Count * 3, 7];
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            Lat1 = pair.XorLat1;
                            Lon1 = pair.YorLon1;
                            H1 = pair.HorZ1;

                            W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                            N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                            M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M
                            //// L矩阵赋值
                            //// 第一行
                            B[3 * i, 0] = -Math.Sin(Lon1) * p / ((N + H1) * Math.Cos(Lat1));
                            B[3 * i, 1] = Math.Cos(Lon1) * p / ((N + H1) * Math.Cos(Lat1));
                            B[3 * i, 2] = 0;
                            B[3 * i, 3] = 0;
                            B[3 * i, 4] = ((N * (1 - E2) + H1) / (N + H1)) * Math.Tan(Lat1) * Math.Cos(Lon1);
                            B[3 * i, 5] = ((N * (1 - E2) + H1) / (N + H1)) * Math.Tan(Lat1) * Math.Sin(Lon1);
                            B[3 * i, 6] = -1;
                            //// 第二行
                            B[3 * i + 1, 0] = -Math.Sin(Lat1) * Math.Cos(Lon1) * p / (M + H1);
                            B[3 * i + 1, 1] = -Math.Sin(Lat1) * Math.Sin(Lon1) * p / (M + H1);
                            B[3 * i + 1, 2] = Math.Cos(Lat1) * p / (M + H1);
                            B[3 * i + 1, 3] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p / M;
                            B[3 * i + 1, 4] = -Math.Sin(Lon1) * (N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2)) / (M + H1);
                            B[3 * i + 1, 5] = Math.Cos(Lon1) * (N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2)) / (M + H1);
                            B[3 * i + 1, 6] = 0;
                            //// 第三行
                            B[3 * i + 2, 0] = Math.Cos(Lat1) * Math.Cos(Lon1);
                            B[3 * i + 2, 1] = Math.Sin(Lat1) * Math.Sin(Lon1);
                            B[3 * i + 2, 2] = Math.Sin(Lat1);
                            B[3 * i + 2, 3] = N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2);
                            B[3 * i + 2, 4] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * Math.Sin(Lon1) / p;
                            B[3 * i + 2, 5] = N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * Math.Cos(Lon1) / p;
                            B[3 * i + 2, 6] = 0;
                        }
                        #endregion
                        break;

                    case EnumTransFormModel._2D7Params:
                        #region 二维七参数
                        B = new double[pairs.Count * 2, 7];
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            Lat1 = pair.XorLat1;
                            Lon1 = pair.YorLon1;

                            W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                            N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                            M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M
                            //// L矩阵赋值
                            //// 第一行
                            B[2 * i, 0] = -Math.Sin(Lon1) * p / (N * Math.Cos(Lat1));
                            B[2 * i, 1] = Math.Cos(Lon1) * p / (N * Math.Cos(Lat1));
                            B[2 * i, 2] = 0;
                            B[2 * i, 3] = 0;
                            B[2 * i, 4] = Math.Tan(Lat1) * Math.Cos(Lon1);
                            B[2 * i, 5] = Math.Tan(Lat1) * Math.Sin(Lon1);
                            B[2 * i, 6] = -1;
                            //// 第二行
                            B[2 * i + 1, 0] = -Math.Sin(Lat1) * Math.Cos(Lon1) * p / M;
                            B[2 * i + 1, 1] = -Math.Sin(Lat1) * Math.Sin(Lon1) * p / M;
                            B[2 * i + 1, 2] = Math.Cos(Lat1) * p / M;
                            B[2 * i + 1, 3] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p / M;
                            B[2 * i + 1, 4] = -Math.Sin(Lon1);
                            B[2 * i + 1, 5] = Math.Cos(Lon1);
                            B[2 * i + 1, 6] = 0;
                        }
                        #endregion
                        break;

                    case EnumTransFormModel.Molodensky:
                        #region 莫洛金斯基模型
                        B = new double[pairs.Count * 3, 7];
                        //// 旋转中心坐标
                        double x0 = 0.0;
                        double y0 = 0.0;
                        double z0 = 0.0;
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            x0 += pair.XorLat1;
                            y0 += pair.YorLon1;
                            z0 += pair.HorZ1;
                        }
                        x0 = x0 / pairs.Count;
                        y0 = y0 / pairs.Count;
                        z0 = z0 / pairs.Count;
                        //x0 = pairs[0].XorLat1;
                        //y0 = pairs[0].YorLon1;
                        //z0 = pairs[0].HorZ1;

                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];

                            //// B矩阵赋值
                            //// 第一行
                            B[3 * i, 0] = 1;
                            B[3 * i, 1] = 0;
                            B[3 * i, 2] = 0;
                            B[3 * i, 3] = pair.XorLat1 - x0;
                            B[3 * i, 4] = 0;
                            B[3 * i, 5] = -(pair.HorZ1 - z0);
                            B[3 * i, 6] = pair.YorLon1 - y0;
                            //// 第二行
                            B[3 * i + 1, 0] = 0;
                            B[3 * i + 1, 1] = 1;
                            B[3 * i + 1, 2] = 0;
                            B[3 * i + 1, 3] = pair.YorLon1 - y0;
                            B[3 * i + 1, 4] = pair.HorZ1 - z0;
                            B[3 * i + 1, 5] = 0;
                            B[3 * i + 1, 6] = -(pair.XorLat1 - x0);
                            //// 第三行
                            B[3 * i + 2, 0] = 0;
                            B[3 * i + 2, 1] = 0;
                            B[3 * i + 2, 2] = 1;
                            B[3 * i + 2, 3] = pair.HorZ1 - z0;
                            B[3 * i + 2, 4] = -(pair.YorLon1 - y0);
                            B[3 * i + 2, 5] = pair.XorLat1 - x0;
                            B[3 * i + 2, 6] = 0;
                        }
                        if (pSevenParams is MolodenskyParams)
                        {
                            MolodenskyParams obj = pSevenParams as MolodenskyParams;
                            obj.X0 = x0;
                            obj.Y0 = y0;
                            obj.Z0 = z0;
                        }
                        #endregion
                        break;

                    default:
                        #region 布尔莎模型
                        B = new double[pairs.Count * 3, 7];
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];

                            //// B矩阵赋值
                            //// 第一行
                            B[3 * i, 0] = 1;
                            B[3 * i, 1] = 0;
                            B[3 * i, 2] = 0;
                            B[3 * i, 3] = pair.XorLat1;
                            B[3 * i, 4] = 0;
                            B[3 * i, 5] = -pair.HorZ1;
                            B[3 * i, 6] = pair.YorLon1;
                            //// 第二行
                            B[3 * i + 1, 0] = 0;
                            B[3 * i + 1, 1] = 1;
                            B[3 * i + 1, 2] = 0;
                            B[3 * i + 1, 3] = pair.YorLon1;
                            B[3 * i + 1, 4] = pair.HorZ1;
                            B[3 * i + 1, 5] = 0;
                            B[3 * i + 1, 6] = -pair.XorLat1;
                            //// 第三行
                            B[3 * i + 2, 0] = 0;
                            B[3 * i + 2, 1] = 0;
                            B[3 * i + 2, 2] = 1;
                            B[3 * i + 2, 3] = pair.HorZ1;
                            B[3 * i + 2, 4] = -pair.YorLon1;
                            B[3 * i + 2, 5] = pair.XorLat1;
                            B[3 * i + 2, 6] = 0;
                        }

                        #endregion
                        break;
                }

                return B;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据公共坐标点对和转换模型获取转换矩阵L(新旧差值矩阵)
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static double[,] GetMatrixL(EarthParams earth_old, EarthParams earth_new, List<CoordinatePointPair> pairs, EnumTransFormModel model = EnumTransFormModel.Bursa)
        {
            if (pairs == null || pairs.Count == 0)
            {
                return null;
            }
            double[,] L = null;            //// 矩阵B   3Count X 7 的矩阵
            try
            {
                double p = 180.0 * 3600 / L_AREACAL_PI;       //// 弧度秒   180.0 * 3600 / L_AREACAL_PI   p = 1.0;      
                double dA = earth_new.A - earth_old.A;
                double dF = (1 / earth_new.F) - (1 / earth_old.F);
                double E2 = GetE2(earth_old.A, earth_old.B);
                double W = 0.0, N = 0.0, M = 0.0;
                double Lat1 = 0.0;
                double num1 = 0.0, num2 = 0.0;

                switch (model)
                {
                    case EnumTransFormModel._3D7Params:
                        #region 三维七参数
                        L = new double[pairs.Count * 3, 1];
                        //// 矩阵赋值
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            Lat1 = pair.XorLat1;

                            W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                            N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                            M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M

                            num1 = N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dA / (M * earth_old.A)
                                + (2 - E2 * Math.Pow(Math.Sin(Lat1), 2)) * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dF / (1 - 1 / earth_old.F);

                            num2 = -W * dA + earth_old.A * (1 - E2) * Math.Pow(Math.Sin(Lat1), 2) * dF / ((1 - earth_old.A) * W);

                            //// L矩阵赋值
                            L[3 * i, 0] = (pair.YorLon2 - pair.YorLon1) * p;              //// 新旧经度值之差
                            L[3 * i + 1, 0] = (pair.XorLat2 - Lat1) * p - num1;          //// 新旧纬度值之差-
                            L[3 * i + 2, 0] = pair.HorZ2 - pair.HorZ1 - num2;          //// △H-
                        }
                        #endregion
                        break;

                    case EnumTransFormModel._2D7Params:
                        #region 二维七参数
                        L = new double[pairs.Count * 2, 1];
                        //// 矩阵赋值
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            Lat1 = pair.XorLat1;

                            W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                            N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                            M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M

                            num1 = N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dA / (M * earth_old.A)
                                + (2 - E2 * Math.Pow(Math.Sin(Lat1), 2)) * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dF / (1 - 1 / earth_old.F);

                            //// L矩阵赋值
                            L[2 * i, 0] = (pair.YorLon2 - pair.YorLon1) * p;              //// 新旧经度值之差
                            L[2 * i + 1, 0] = (pair.XorLat2 - Lat1) * p - num1;          //// 新旧纬度值之差-
                        }
                        #endregion
                        break;

                    default:
                        #region 布尔莎和莫洛金斯基模型
                        L = new double[pairs.Count * 3, 1];
                        //// 矩阵赋值
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            CoordinatePointPair pair = pairs[i];
                            //// L矩阵赋值
                            L[3 * i, 0] = pair.XorLat2 - pair.XorLat1;          //// 新旧X值之差
                            L[3 * i + 1, 0] = pair.YorLon2 - pair.YorLon1;      //// 新旧Y值之差
                            L[3 * i + 2, 0] = pair.HorZ2 - pair.HorZ1;          //// 新旧Z值之差
                        }
                        #endregion
                        break;
                }
                return L;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据原数据点获取矩阵B
        /// </summary>
        /// <param name="earth_old"></param>
        /// <param name="earth_new"></param>
        /// <param name="point"></param>
        /// <param name="pSevenParams"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static double[,] GetMatrixB(EarthParams earth_old, EarthParams earth_new, CoordinatePoint point, ref SevenParams pSevenParams, EnumTransFormModel model = EnumTransFormModel.Bursa)
        {
            if (point == null)
            {
                return null;
            }
            double[,] B = null;            //// 矩阵B   3Count X 7 的矩阵
            try
            {
                double p = 180.0 * 3600 / L_AREACAL_PI;             //// 弧度秒   180.0 * 3600 / L_AREACAL_PI  p = 1.0; 
                double E2 = GetE2(earth_old.A, earth_old.B);
                double W = 0.0, N = 0.0, M = 0.0;
                double Lat1 = 0.0, Lon1 = 0.0, H1 = 0.0;

                switch (model)
                {
                    case EnumTransFormModel._3D7Params:
                        #region 三维七参数
                        B = new double[3, 7];
                        Lat1 = point.XorLat;
                        Lon1 = point.YorLon;
                        H1 = point.HorZ;

                        W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                        N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                        M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M
                        //// L矩阵赋值
                        //// 第一行
                        B[0, 0] = -Math.Sin(Lon1) * p / ((N + H1) * Math.Cos(Lat1));
                        B[0, 1] = Math.Cos(Lon1) * p / ((N + H1) * Math.Cos(Lat1));
                        B[0, 2] = 0;
                        B[0, 3] = 0;
                        B[0, 4] = ((N * (1 - E2) + H1) / (N + H1)) * Math.Tan(Lat1) * Math.Cos(Lon1);
                        B[0, 5] = ((N * (1 - E2) + H1) / (N + H1)) * Math.Tan(Lat1) * Math.Sin(Lon1);
                        B[0, 6] = -1;
                        //// 第二行
                        B[1, 0] = -Math.Sin(Lat1) * Math.Cos(Lon1) * p / (M + H1);
                        B[1, 1] = -Math.Sin(Lat1) * Math.Sin(Lon1) * p / (M + H1);
                        B[1, 2] = Math.Cos(Lat1) * p / (M + H1);
                        B[1, 3] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p / M;
                        B[1, 4] = -Math.Sin(Lon1) * (N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2)) / (M + H1);
                        B[1, 5] = Math.Cos(Lon1) * (N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2)) / (M + H1);
                        B[1, 6] = 0;
                        //// 第三行
                        B[2, 0] = Math.Cos(Lat1) * Math.Cos(Lon1);
                        B[2, 1] = Math.Sin(Lat1) * Math.Sin(Lon1);
                        B[2, 2] = Math.Sin(Lat1);
                        B[2, 3] = N + H1 - N * E2 * Math.Pow(Math.Sin(Lat1), 2);
                        B[2, 4] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * Math.Sin(Lon1) / p;
                        B[2, 5] = N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * Math.Cos(Lon1) / p;
                        B[2, 6] = 0;

                        #endregion
                        break;

                    case EnumTransFormModel._2D7Params:
                        #region 二维七参数
                        B = new double[2, 7];
                        Lat1 = point.XorLat;
                        Lon1 = point.YorLon;

                        W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                        N = earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                        M = earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M
                        //// L矩阵赋值
                        //// 第一行
                        B[0, 0] = -Math.Sin(Lon1) * p / (N * Math.Cos(Lat1));
                        B[0, 1] = Math.Cos(Lon1) * p / (N * Math.Cos(Lat1));
                        B[0, 2] = 0;
                        B[0, 3] = 0;
                        B[0, 4] = Math.Tan(Lat1) * Math.Cos(Lon1);
                        B[0, 5] = Math.Tan(Lat1) * Math.Sin(Lon1);
                        B[0, 6] = -1;
                        //// 第二行
                        B[1, 0] = -Math.Sin(Lat1) * Math.Cos(Lon1) * p / M;
                        B[1, 1] = -Math.Sin(Lat1) * Math.Sin(Lon1) * p / M;
                        B[1, 2] = Math.Cos(Lat1) * p / M;
                        B[1, 3] = -N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p / M;
                        B[1, 4] = -Math.Sin(Lon1);
                        B[1, 5] = Math.Cos(Lon1);
                        B[1, 6] = 0;

                        #endregion
                        break;

                    case EnumTransFormModel.Molodensky:
                        #region 莫洛金斯基模型
                        MolodenskyParams obj = pSevenParams as MolodenskyParams;
                        B = new double[3, 7] { { 1, 0, 0, point.XorLat-obj.X0, 0, -(point.HorZ-obj.Z0), point.YorLon-obj.Y0 }
                                              ,{ 0, 1, 0, point.YorLon-obj.Y0, point.HorZ-obj.Z0, 0 , -(point.XorLat-obj.X0)}
                                              ,{ 0, 0, 1, point.HorZ-obj.Z0, -(point.YorLon-obj.Y0), point.XorLat-obj.X0, 0}};
                        #endregion
                        break;

                    default:
                        #region 布尔莎模型
                        B = new double[3, 7];

                        //// B矩阵赋值
                        //// 第一行
                        B[0, 0] = 1;
                        B[0, 1] = 0;
                        B[0, 2] = 0;
                        B[0, 3] = point.XorLat;
                        B[0, 4] = 0;
                        B[0, 5] = -point.HorZ;
                        B[0, 6] = point.YorLon;
                        //// 第二行
                        B[1, 0] = 0;
                        B[1, 1] = 1;
                        B[1, 2] = 0;
                        B[1, 3] = point.YorLon;
                        B[1, 4] = point.HorZ;
                        B[1, 5] = 0;
                        B[1, 6] = -point.XorLat;
                        //// 第三行
                        B[2, 0] = 0;
                        B[2, 1] = 0;
                        B[2, 2] = 1;
                        B[2, 3] = point.HorZ;
                        B[2, 4] = -point.YorLon;
                        B[2, 5] = point.XorLat;
                        B[2, 6] = 0;

                        #endregion
                        break;
                }

                return B;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 矩阵运算
        /// <summary>
        /// 矩阵加法
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static double[,] MatrixPlus(double[,] X, double[,] Y)
        {
            try
            {
                int RowX = X.GetLength(0);
                int ColX = X.GetLength(1);
                int RowY = Y.GetLength(0);
                int ColY = Y.GetLength(1);
                if (RowX != RowY || ColX != ColY)
                {
                    return null;
                }
                double[,] target = new double[RowX, ColX];
                for (int i = 0; i < RowX; i++)
                {
                    for (int j = 0; j < ColX; j++)
                    {
                        target[i, j] = X[i, j] + Y[i, j];
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 矩阵转置运算
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] MatrixTranspose(double[,] source)
        {
            try
            {
                int Row = source.GetLength(0);
                int Col = source.GetLength(1);
                double[,] target = new double[Col, Row];
                for (int i = 0; i < Col; i++)
                {
                    for (int j = 0; j < Row; j++)
                    {
                        target[i, j] = source[j, i];
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 矩阵相乘
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static double[,] MatrixMultiply(double[,] X, double[,] Y)
        {
            try
            {
                //// 不符合矩阵相乘的规则
                if (X.GetLength(1) != Y.GetLength(0))
                {
                    return null;
                }

                int row = X.GetLength(0);
                int col = Y.GetLength(1);
                int mid = X.GetLength(1);
                double[,] target = new double[row, col];
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        for (int k = 0; k < mid; k++)
                        {
                            target[i, j] += X[i, k] * Y[k, j];
                        }
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 数乘
        /// </summary>
        /// <param name="source"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double[,] MatrixPly(double[,] source, double k)
        {
            try
            {
                int row = source.GetLength(0);
                int col = source.GetLength(1);
                double[,] target = new double[row, col];
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        target[i, j] = source[i, j] * k;
                    }
                }
                return target;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 求逆矩阵
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] Inverse(double[,] source)
        {
            int row = source.GetLength(0);
            int col = source.GetLength(1);
            if (row != col)
            {
                return null;
            }
            //// 复制数组 
            double[,] copy = new double[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    copy[i, j] = source[i, j];
                }
            }

            //// 单位矩阵
            double[,] E = new double[row, col];
            for (int i = 0; i < row; i++)
            {
                E[i, i] = 1;
            }

            //i表示第几行，j表示第几列  
            for (int j = 0; j < row; j++)
            {
                bool flag = false;
                for (int i = j; i < row; i++)
                {
                    if (copy[i, j] != 0)
                    {
                        flag = true;
                        double temp;
                        //交换i,j,两行  
                        if (i != j)
                        {
                            for (int k = 0; k < row; k++)
                            {
                                temp = copy[j, k];
                                copy[j, k] = copy[i, k];
                                copy[i, k] = temp;

                                temp = E[j, k];
                                E[j, k] = E[i, k];
                                E[i, k] = temp;
                            }
                        }
                        //第j行标准化  
                        double d = copy[j, j];
                        for (int k = 0; k < row; k++)
                        {
                            copy[j, k] = copy[j, k] / d;
                            E[j, k] = E[j, k] / d;
                        }
                        //消去其他行的第j列  
                        d = copy[j, j];
                        for (int k = 0; k < row; k++)
                        {
                            if (k != j)
                            {
                                double t = copy[k, j];
                                for (int n = 0; n < row; n++)
                                {
                                    copy[k, n] -= (t / d) * copy[j, n];
                                    E[k, n] -= (t / d) * E[j, n];
                                }
                            }
                        }
                    }
                }
                if (!flag) return null;
            }
            return E;
        }

        /// <summary>
        /// 矩阵求逆(高斯法)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] MatrixInv(double[,] source)
        {
            int m = source.GetLength(0);
            int n = source.GetLength(1);
            if (m != n)
            {
                Exception myException = new Exception("数组维数不匹配");
                throw myException;
            }
            double[,] a = (double[,])source.Clone();
            double[,] b = new double[m, n];

            int i, j, row, k;
            double max, temp;

            //单位矩阵
            for (i = 0; i < n; i++)
            {
                b[i, i] = 1;
            }
            for (k = 0; k < n; k++)
            {
                max = 0; row = k;
                //找最大元，其所在行为row
                for (i = k; i < n; i++)
                {
                    temp = Math.Abs(a[i, k]);
                    if (max < temp)
                    {
                        max = temp;
                        row = i;
                    }

                }
                if (max == 0)
                {
                    Exception myException = new Exception("没有逆矩阵");
                    throw myException;
                }
                //交换k与row行
                if (row != k)
                {
                    for (j = 0; j < n; j++)
                    {
                        temp = a[row, j];
                        a[row, j] = a[k, j];
                        a[k, j] = temp;

                        temp = b[row, j];
                        b[row, j] = b[k, j];
                        b[k, j] = temp;
                    }

                }

                //首元化为1
                for (j = k + 1; j < n; j++) a[k, j] /= a[k, k];
                for (j = 0; j < n; j++) b[k, j] /= a[k, k];

                a[k, k] = 1;

                //k列化为0
                //对a
                for (j = k + 1; j < n; j++)
                {
                    for (i = 0; i < k; i++) a[i, j] -= a[i, k] * a[k, j];
                    for (i = k + 1; i < n; i++) a[i, j] -= a[i, k] * a[k, j];
                }
                //对b
                for (j = 0; j < n; j++)
                {
                    for (i = 0; i < k; i++) b[i, j] -= a[i, k] * b[k, j];
                    for (i = k + 1; i < n; i++) b[i, j] -= a[i, k] * b[k, j];
                }
                for (i = 0; i < n; i++) a[i, k] = 0;
                a[k, k] = 1;
            }

            return b;
        }

        /// <summary>
        /// 矩阵求逆(返回值如果为null，说明矩阵不可逆、矩阵数据有问题、或者矩阵不是方阵等)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] MatrixInverse(double[,] source)
        {
            try
            {
                //// 判断是否是方阵
                int RowNum = source.GetLength(0);
                int ColNum = source.GetLength(1);
                if (RowNum != ColNum)
                {
                    return null;
                }

                //// N阶方阵可逆的充分必要条件是|A| ！= 0
                decimal DetValue = MatrixValue(source);          //// 矩阵行列式值
                if (DetValue == 0)
                {
                    return null;
                }

                //// 获取伴随矩阵
                double[,] adjoint = new double[RowNum, RowNum];
                for (int i = 0; i < RowNum; i++)
                {
                    for (int j = 0; j < RowNum; j++)
                    {
                        adjoint[i, j] = double.Parse((MatrixValue(MatrixCofactor(source, j, i)) / DetValue).ToString());
                    }
                }

                //// 获取逆矩阵
                //double[,] target = MatrixPly(adjoint, 1.0 / DetValue);
                decimal dd = MatrixValue(adjoint);
                decimal cc = dd * DetValue;
                return adjoint;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 求行列式的值(返回值如果是-1，表示矩阵有问题)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static decimal MatrixValue(double[,] source)
        {
            try
            {
                decimal dValue = 0.0m;
                int row = source.GetLength(0);
                int col = source.GetLength(1);
                //// 判断是否是方阵
                if (row != col)
                {
                    return -1;
                }
                //// 1阶矩阵直接返回该值
                if (row == 1)
                {
                    return (decimal)source[0, 0];
                }

                //// 行列式展开
                for (int i = 0; i < col; i++)
                {
                    decimal a = (decimal)source[0, i];
                    decimal b = MatrixValue(MatrixCofactor(source, 0, i));
                    dValue += (decimal)source[0, i] * MatrixValue(MatrixCofactor(source, 0, i));
                }
                return dValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 计算代数余子式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static double[,] MatrixCofactor(double[,] source, int row, int col)
        {
            try
            {
                int RowNum = source.GetLength(0);
                int ColNum = source.GetLength(1);
                double[,] target = new double[RowNum - 1, ColNum - 1];
                //// 矩阵赋值
                for (int i = 0; i < RowNum - 1; i++)
                {
                    for (int j = 0; j < ColNum - 1; j++)
                    {
                        if (i < row && j < col)
                        {
                            target[i, j] = source[i, j];
                        }
                        else if (i < row && j >= col)
                        {
                            target[i, j] = source[i, j + 1];
                        }
                        else if (i >= row && j < col)
                        {
                            target[i, j] = source[i + 1, j];
                        }
                        else
                        {
                            target[i, j] = source[i + 1, j + 1];
                        }
                    }
                }

                for (int i = 0; i < RowNum - 1; i++)
                {
                    target[i, 0] = Math.Pow(-1, row + col) * target[i, 0];
                }


                return target;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 最小二乘法
        /// </summary>
        /// <param name="B"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        public static double[,] LeastSquare(double[,] B, double[,] L)
        {
            try
            {
                double[,] BT = MatrixTranspose(B);                 //// 转置矩阵
                double[,] BTL = MatrixMultiply(BT, L);             //// BT*L
                double[,] BTB = MatrixMultiply(BT, B);             //// BT*B
                double[,] VBTB = Inverse(BTB);               //// BT*B的逆矩阵
                if (VBTB == null)
                {
                    return null;
                }
                double[,] dValue = MatrixMultiply(VBTB, BTL);
                return dValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region BLH <=> XYZ
        /// <summary>
        /// BLH => XYZ
        /// </summary>
        /// <param name="format">度或者度分秒</param>
        /// <param name="earth"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static CoordinatePoint TransFormXYZ(EnumCoordinateFormat format, EarthParams earth, CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                double B = DegreeToArc(point.XorLat);
                double L = DegreeToArc(point.YorLon);
                double H = point.HorZ;
                if (format == EnumCoordinateFormat.ddmmss)
                {
                    B = DegreeToArc(DmsToDegree(point.XorLat));
                    L = DegreeToArc(DmsToDegree(point.YorLon));
                }

                double E2 = GetE2(earth.A, earth.B);
                double N = earth.A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(B), 2.0));

                NewPoint.XorLat = (N + H) * Math.Cos(B) * Math.Cos(L);
                NewPoint.YorLon = (N + H) * Math.Cos(B) * Math.Sin(L);
                NewPoint.HorZ = Math.Sin(B) * (N * (1 - E2) + H);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }

        /// <summary>
        /// XYZ => BLH
        /// </summary>
        /// <param name="earth"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static CoordinatePoint TransFormBLH(EnumCoordinateFormat format, EarthParams earth, CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                double A = earth.A;
                double B = earth.B;
                double X = point.XorLat;
                double Y = point.YorLon;
                double Z = point.HorZ;
                double E2 = GetE2(earth.A, earth.B);
                double E21 = (Math.Pow(A, 2.0) - Math.Pow(B, 2.0)) / Math.Pow(B, 2.0);

                double L = Math.Atan2(Y, X);

                double angel = Math.Atan(Z * A / (B * Math.Sqrt(X * X + Y * Y)));

                double Lat = Math.Atan((Z + E21 * B * Math.Pow(Math.Sin(angel), 3.0)) / (Math.Sqrt(X * X + Y * Y) - E2 * A * Math.Pow(Math.Cos(angel), 3.0)));

                double N = earth.A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat), 2.0));
                double H = Math.Sqrt(X * X + Y * Y) / Math.Cos(Lat) - N;

                NewPoint.XorLat = ArcToDegree(Lat);
                NewPoint.YorLon = ArcToDegree(L);
                NewPoint.HorZ = H;
                if (format == EnumCoordinateFormat.ddmmss)
                {
                    NewPoint.XorLat = DegreeToDms(NewPoint.XorLat);
                    NewPoint.YorLon = DegreeToDms(NewPoint.YorLon);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }

        /// <summary>
        /// XYZ => BLH(迭代法求解)
        /// </summary>
        /// <param name="earth"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static CoordinatePoint TransFormBLH1(EnumCoordinateFormat format, EarthParams earth, CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                double X = point.XorLat;
                double Y = point.YorLon;
                double Z = point.HorZ;
                double E2 = GetE2(earth.A, earth.B);

                double L = Math.Atan2(Y, X);
                double N = 0.0;
                double Lat = 0.0;
                double Lat0 = Math.Atan(Z / Math.Sqrt(X * X + Y * Y));
                while ((Lat0 - Lat) > 1E-15)
                {
                    Lat = Lat0;
                    N = earth.A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat), 2.0));
                    Lat0 = Math.Atan((Z + E2 * N * Math.Sin(Lat)) / Math.Sqrt(X * X + Y * Y));
                }

                Lat = Lat0;
                double H = Math.Sqrt(X * X + Y * Y) / Math.Cos(Lat) - N;

                NewPoint.XorLat = ArcToDegree(Lat);
                NewPoint.YorLon = ArcToDegree(L);
                NewPoint.HorZ = H;
                if (format == EnumCoordinateFormat.ddmmss)
                {
                    NewPoint.XorLat = DegreeToDms(NewPoint.XorLat);
                    NewPoint.YorLon = DegreeToDms(NewPoint.YorLon);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }


        #endregion

        public static void ExportMatrix(double[,] A, string name)
        {
            string file = @"E:\项目\坐标系坐标转换工具\测试数据\七参数转换\4\" + name + ".txt";
            if (!Directory.Exists(Path.GetDirectoryName(file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            StreamWriter writer = new StreamWriter(file, false, Encoding.Default);
            for (int i = 0; i < A.GetLength(0); i++)
            {
                string str = "";
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    str = string.IsNullOrEmpty(str) ? A[i, j].ToString() : str + "," + A[i, j].ToString();
                }
                writer.WriteLine(str);
            }
            writer.Close();

        }

        #region 双线性内插法计算改正量

        /// <summary>
        /// 双线性内插法计算改正量
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="B"></param>
        /// <param name="L"></param>
        /// <param name="dB"></param>
        /// <param name="dL"></param>
        public static void BilinearInterpolation(MapCorrection obj, double B, double L, out double dB, out double dL)
        {
            dB = 0.0;
            dL = 0.0;
            try
            {
                double dxmB = obj.LeftBottomdB + (obj.RightBottomdB - obj.LeftBottomdB) * ((obj.maxB - B) / (obj.maxB - obj.minB));
                double dxnB = obj.LeftTopdB + (obj.RightTopdB - obj.LeftTopdB) * ((obj.maxB - B) / (obj.maxB - obj.minB));
                dB = dxmB + (dxnB - dxmB) * ((obj.maxL - L) / (obj.maxL - obj.minL));
                double dxmL = obj.LeftBottomdL + (obj.RightBottomdL - obj.LeftBottomdL) * ((obj.maxB - B) / (obj.maxB - obj.minB));
                double dxnL = obj.LeftTopdL + (obj.RightTopdL - obj.LeftTopdL) * ((obj.maxB - B) / (obj.maxB - obj.minB));
                dL = dxmL + (dxnL - dxmL) * ((obj.maxL - L) / (obj.maxL - obj.minL));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 大地坐标改正量计算公式
        /// <summary>
        /// 计算改正量dL
        /// </summary>
        /// <param name="dLat"></param>
        /// <param name="dLon"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static double ComputeCorrectionL(double Lat, double Lon, double A, double B)
        {
            double dL = 0.0;
            try
            {
                double dx = 0.0;
                double dy = 0.0;
                double E2 = GetE2(A, B);                                                        //// 第一偏心率的平方
                double R_Lat = DegreeToArc(Lat);                                     //// 纬度弧度
                double R_Lon = DegreeToArc(Lon);
                double N = A / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(R_Lat), 2));                //// A/根号下（1-e2*sinB*sinB）
                dL = (-1.0 / (N * Math.Cos(R_Lat))) * (dx * Math.Sin(R_Lon) - dy * Math.Cos(R_Lon));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dL;
        }

        /// <summary>
        /// 计算改正量dB
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Lon"></param>
        /// <param name="A80"></param>
        /// <param name="B80"></param>
        /// <returns></returns>
        public static double ComputeCorrectionB(double Lat, double Lon, double A80, double B80, double A2000, double B2000)
        {
            double dB = 0.0;
            try
            {
                double dx = 0.0;
                double dy = 0.0;
                double dz = 0.0;

                double E2_80 = GetE2(A80, B80);                                                        //// 第一偏心率的平方
                double E2_2000 = GetE2(A2000, B2000);
                double R_Lat = DegreeToArc(Lat);                                     //// 纬度弧度
                double R_Lon = DegreeToArc(Lon);

                double W = Math.Sqrt(1 - E2_80 * Math.Pow(Math.Sin(R_Lat), 2));
                double N = A80 / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                double M = A80 * (1 - E2_80) / Math.Pow(W, 3);                       //// 子午圈曲率半径M

                dB = (-dx * Math.Sin(R_Lat) * Math.Cos(R_Lon)) / M
                    - (dy * Math.Sin(R_Lat) * Math.Sin(R_Lon)) / M
                    + dz * Math.Cos(R_Lat) / M
                    + (Math.Sin(R_Lat) * Math.Cos(R_Lat) / M) * ((E2_80 * (A2000 - A80) / W) + N * (2 - E2_80 * Math.Pow(Math.Sin(R_Lat), 2)) * (E2_2000 - E2_80) / (2 * W * W));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dB;
        }
        #endregion

    }
}
