using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 坐标点对
    /// </summary>
    public class CoordinatePointPair
    {
        private string isValid = "";
        private double x_lat_1 = 0.0;
        private double y_lon_1 = 0.0;
        private double h_z_1 = 0.0;
        private double x_lat_2 = 0.0;
        private double y_lon_2 = 0.0;
        private double h_z_2 = 0.0;
        private string guid = "";
        private decimal residualX = 0.0m;
        private decimal residualY = 0.0m;
        private decimal residualZ = 0.0m;

        /// <summary>
        /// 是否采用
        /// </summary>
        public string IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                isValid = value;
            }
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID
        {
            get { return guid; }
            set { guid = value; }
        }

        /// <summary>
        /// x坐标或者纬度坐标
        /// </summary>
        public double XorLat1
        {
            get
            {
                return x_lat_1;
            }
            set
            {
                x_lat_1 = value;
            }
        }

        /// <summary>
        /// x坐标或者纬度坐标
        /// </summary>
        public double XorLat2
        {
            get
            {
                return x_lat_2;
            }
            set
            {
                x_lat_2 = value;
            }
        }

        /// <summary>
        /// y坐标或者经度坐标
        /// </summary>
        public double YorLon1
        {
            get
            {
                return y_lon_1;
            }
            set
            {
                y_lon_1 = value;
            }
        }

        /// <summary>
        /// y坐标或者经度坐标
        /// </summary>
        public double YorLon2
        {
            get
            {
                return y_lon_2;
            }
            set
            {
                y_lon_2 = value;
            }
        }

        /// <summary>
        /// 大地高或者空间坐标Z
        /// </summary>
        public double HorZ1
        {
            get
            {
                return h_z_1;
            }
            set
            {
                h_z_1 = value;
            }
        }

        /// <summary>
        /// 大地高或者空间坐标Z
        /// </summary>
        public double HorZ2
        {
            get
            {
                return h_z_2;
            }
            set
            {
                h_z_2 = value;
            }
        }

        /// <summary>
        /// 残差X
        /// </summary>
        public decimal ResidualX
        {
            get { return residualX; }
            set { residualX = value; }
        }

        /// <summary>
        /// 残差Y
        /// </summary>
        public decimal ResidualY
        {
            get { return residualY; }
            set { residualY = value; }
        }

        /// <summary>
        /// 残差Z
        /// </summary>
        public decimal ResidualZ
        {
            get { return residualZ; }
            set { residualZ = value; }
        }

        public CoordinatePointPair()
        {

        }

        public CoordinatePointPair(double dX1, double dY1, double dX2, double dY2)
        {
            x_lat_1 = dX1;
            x_lat_2 = dX2;
            y_lon_1 = dY1;
            y_lon_2 = dY2;
        }

        public CoordinatePointPair(double dX1, double dY1, double dH1, double dX2, double dY2, double dH2)
        {
            x_lat_1 = dX1;
            x_lat_2 = dX2;
            y_lon_1 = dY1;
            y_lon_2 = dY2;
            h_z_1 = dH1;
            h_z_2 = dH2;
        }
    }

    /// <summary>
    /// 坐标转换坐标点类
    /// </summary>
    public class CoordinatePoint
    {
        private string dh = "";
        private double x_lat = 0.0;
        private double y_lon = 0.0;
        private double h_z = 0.0;

        /// <summary>
        /// 点号
        /// </summary>
        public string DH
        {
            get
            {
                return dh;
            }
            set
            {
                dh = value;
            }
        }

        /// <summary>
        /// x坐标或者纬度坐标
        /// </summary>
        public double XorLat
        {
            get
            {
                return x_lat;
            }
            set
            {
                x_lat = value;
            }
        }

        /// <summary>
        /// y坐标或者经度坐标
        /// </summary>
        public double YorLon
        {
            get
            {
                return y_lon;
            }
            set
            {
                y_lon = value;
            }
        }

        /// <summary>
        /// 大地高或者空间坐标Z
        /// </summary>
        public double HorZ
        {
            get
            {
                return h_z;
            }
            set
            {
                h_z = value;
            }
        }

        public CoordinatePoint()
        {

        }

        public CoordinatePoint(string sDH, double dX, double dY, double dH)
        {
            dh = sDH;
            x_lat = dX;
            y_lon = dY;
            h_z = dH;
        }
    }

    #region 转换模型参数

    public class TParams
    {

    }

    /// <summary>
    /// 七参数
    /// </summary>
    public class SevenParams : TParams
    {
        /// <summary>
        /// 平移量X
        /// </summary>
        public double DX { get; set; }
        /// <summary>
        /// 平移量Y
        /// </summary>
        public double DY { get; set; }
        /// <summary>
        /// 平移量Z
        /// </summary>
        public double DZ { get; set; }
        /// <summary>
        /// X轴旋转角
        /// </summary>
        public double AngleX { get; set; }
        /// <summary>
        /// Y轴旋转角
        /// </summary>
        public double AngleY { get; set; }
        /// <summary>
        /// Z轴旋转角
        /// </summary>
        public double AngleZ { get; set; }
        /// <summary>
        /// 尺度因子K
        /// </summary>
        public double ScaleK { get; set; }
    }

    /// <summary>
    /// 四参数
    /// </summary>
    public class FourParams2D : TParams
    {
        /// <summary>
        /// 平移量X
        /// </summary>
        public double DX { get; set; }
        /// <summary>
        /// 平移量Y
        /// </summary>
        public double DY { get; set; }
        /// <summary>
        /// X轴旋转角
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// 尺度因子K
        /// </summary>
        public double ScaleK { get; set; }
    }

    /// <summary>
    /// 三维四参数
    /// </summary>
    public class FourParams3D : TParams
    {
        /// <summary>
        /// 平移量X
        /// </summary>
        public double DX { get; set; }
        /// <summary>
        /// 平移量Y
        /// </summary>
        public double DY { get; set; }
        /// <summary>
        /// 平移量Z
        /// </summary>
        public double DZ { get; set; }
        /// <summary>
        /// 旋转角
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// 区域中心纬度
        /// </summary>
        public double B0 { get; set; }
        /// <summary>
        /// 区域中心经度
        /// </summary>
        public double L0 { get; set; }
    }

    /// <summary>
    /// 平面多项式拟合参数
    /// </summary>
    public class Params2DN : TParams
    {
        public double A0 { get; set; }
        public double B0 { get; set; }
        public double A1 { get; set; }
        public double B1 { get; set; }
        public double A2 { get; set; }
        public double B2 { get; set; }
        public double A3 { get; set; }
        public double B3 { get; set; }
        public double A4 { get; set; }
        public double B4 { get; set; }
        public double A5 { get; set; }
        public double B5 { get; set; }
    }

    /// <summary>
    /// 椭球面多项式拟合参数
    /// </summary>
    public class Params3DN : TParams
    {
        public double A0 { get; set; }
        public double B0 { get; set; }
        public double A1 { get; set; }
        public double B1 { get; set; }
        public double A2 { get; set; }
        public double B2 { get; set; }
        public double A3 { get; set; }
        public double B3 { get; set; }
        public double A4 { get; set; }
        public double B4 { get; set; }
        public double A5 { get; set; }
        public double B5 { get; set; }
    }
    #endregion

    /// <summary>
    /// 椭球参数
    /// </summary>
    public class EarthParams
    {
        /// <summary>
        /// 参考椭球名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 地理坐标系名称
        /// </summary>
        public string GeoName { get; set; }
        /// <summary>
        /// 长半轴
        /// </summary>
        public double A { get; set; }
        /// <summary>
        /// 扁率
        /// </summary>
        public double F { get; set; }

        /// <summary>
        /// 短半轴
        /// </summary>        
        private double b;
        public double B
        {
            get
            {
                if (b == 0 || Name == "自定义")
                {
                    b = TransFormMethod.GetB(A, F);
                }
                return b;
            }
            set
            {
                b = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

    }

    /// <summary>
    /// 莫洛金斯基七参数模型
    /// </summary>
    public class MolodenskyParams : SevenParams
    {
        public double X0 { get; set; }
        public double Y0 { get; set; }
        public double Z0 { get; set; }
    }

    /// <summary>
    /// 四角点和图幅改正量
    /// </summary>
    public class MapCorrection
    {
        private string _tfbh = "";
        public string TFBH
        {
            get { return _tfbh; }
            set { _tfbh = value; }
        }
        public double LeftTopdB { get; set; }
        public double LeftTopdL { get; set; }
        public double LeftBottomdB { get; set; }
        public double LeftBottomdL { get; set; }
        public double RightTopdB { get; set; }
        public double RightTopdL { get; set; }
        public double RightBottomdB { get; set; }
        public double RightBottomdL { get; set; }
        private double _minB = 0.0;
        public double minB
        {
            get { return _minB; }
            set { _minB = value; }
        }
        private double _minL = 0.0;
        public double minL
        {
            get { return _minL; }
            set { _minL = value; }
        }
        private double _maxB = 0.0;
        public double maxB
        {
            get { return _maxB; }
            set { _maxB = value; }
        }
        private double _maxL = 0.0;
        public double maxL
        {
            get { return _maxL; }
            set { _maxL = value; }
        }
        private int _scale = 0;
        public int Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        /// <summary>
        /// 完善属性值
        /// </summary>
        public void ComputeBL()
        {
            if (!string.IsNullOrEmpty(_tfbh) && _minB.Equals(0.0) && _minL.Equals(0.0) && _maxB.Equals(0.0) && _maxL.Equals(0.0))
            {
                TFComm.GetFourDSFromTFH(_tfbh, ref _minL, ref _minB, ref _maxL, ref _maxB);
                _scale = TFComm.GetScaleByTFH(_tfbh);
            }
            else if (string.IsNullOrEmpty(_tfbh) && !_minB.Equals(0.0) && !_minL.Equals(0.0) && !_maxB.Equals(0.0) && !_maxL.Equals(0.0))
            {
                _scale = TFComm.GetScaleByDeviation(_maxB - _minB, _maxL - _minL);
                _tfbh = TFComm.GetTFHFromJWDAndScale(_minB, _minL, _scale);
            }
        }


    }

    #region 枚举类

    /// <summary>
    /// 坐标类型
    /// </summary>
    public enum EnumCoordinateType
    {
        BL,
        BLH,
        xy,
        xyH
    }

    /// <summary>
    /// 转换类型
    /// </summary>
    public enum EnumTransFormType
    {
        BLtoXY,
        XYtoBL,
        XYtoXY,
        BLHtoBLH,
        BLHtoXYH,
        XYHtoXYH,
        XYHtoBLH,
        XYtoXYby4
    }

    /// <summary>
    /// 坐标格式
    /// </summary>
    public enum EnumCoordinateFormat
    {
        Du,
        ddmmss
    }

    /// <summary>
    /// 转换模型
    /// </summary>
    public enum EnumTransFormModel
    {
        Bursa = 0,
        Molodensky = 1,
        _2D7Params = 2,
        _3D7Params = 3,
        _2D4Params = 4,
        _3D4Params = 5
    }
    #endregion
}
