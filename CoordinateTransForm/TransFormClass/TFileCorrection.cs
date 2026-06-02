using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    public class TFileCorrection : TCoordinate
    {
        private EnumTransFormType _type;
        private EarthParams _earth_old;
        private EarthParams _earth_new;
        private double _dOldCentralMeridian;
        private double _dOldAddY;
        private double _dNewCentralMeridian;
        private double _dNewAddY;
        private Dictionary<string, MapCorrection> _dicCorrection;
        private int _iScale;
        private double dLastdB = 0.0;
        private double dLastdL = 0.0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TFileCorrection(EnumTransFormType type, Dictionary<string, MapCorrection> dicCorrection, int iScale,
                                 EarthParams earth_old, EarthParams earth_new,
                                 double dOldCentralMeridian, double dOldAddY,
                                 double dNewCentralMeridian, double dNewAddY)
        {
            _type = type;
            _iScale = iScale;
            _dicCorrection = dicCorrection;
            _earth_old = earth_old;
            _earth_new = earth_new;
            _dOldCentralMeridian = dOldCentralMeridian;
            _dOldAddY = dOldAddY;
            _dNewCentralMeridian = dNewCentralMeridian;
            _dNewAddY = dNewAddY;
        }


        public CoordinatePoint Compute(CoordinatePoint p)
        {
            CoordinatePoint NewPnt = new CoordinatePoint();
            try
            {
                MapCorrection obj = null;
                TCoordinate TClass = null;
                NewPnt = p;

                //// 高斯反算转换 
                if (_type == EnumTransFormType.XYHtoBLH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(_type, EnumCoordinateFormat.Du, _earth_old, null, _dOldCentralMeridian, _dOldAddY);
                    NewPnt = TClass.Compute(p);
                }
                //// 度转秒
                double Lats = TransFormMethod.DegreeToSecond(NewPnt.XorLat);
                double Lons = TransFormMethod.DegreeToSecond(NewPnt.YorLon);
                //// 获取所在图幅的改正量
                string sTFH = TFComm.GetTFHFromJWDAndScale(Lats, Lons, _iScale);
                if (_dicCorrection.ContainsKey(sTFH))
                {
                    obj = _dicCorrection[sTFH];
                }
                else if (dLastdB == 0.0 && dLastdL == 0.0)
                {
                    obj = _dicCorrection.First().Value;
                }
                else
                {
                        
                }

                if (_iScale < 10000)
                {
                    double dX, dY;
                    if (obj != null)
                    {
                        TransFormMethod.BilinearInterpolation(obj, Lats, Lons, out dX, out dY);
                        dLastdB = dX;
                        dLastdL = dY;
                    }
                    else
                    {
                        dX = dLastdB;
                        dY = dLastdL;
                    }

                    //// 新坐标系下的xy坐标         可能逻辑不对需要修改
                    NewPnt.XorLat = p.XorLat + dX;
                    NewPnt.YorLon = p.YorLon + dY;
                    //// 高斯反算转换，平面坐标转经纬度
                    if (_type == EnumTransFormType.XYHtoBLH || _type == EnumTransFormType.BLHtoBLH)
                    {
                        TClass = new GaussTrans(EnumTransFormType.XYHtoBLH, EnumCoordinateFormat.Du, _earth_new, null, _dNewCentralMeridian, _dNewAddY);
                        NewPnt = TClass.Compute(NewPnt);
                    }
                }
                else
                {
                    double dB, dL;
                    if (obj != null)
                    {
                        TransFormMethod.BilinearInterpolation(obj, Lats, Lons, out dB, out dL);
                        dLastdB = dB;
                        dLastdL = dL;
                    }
                    else
                    {
                        dB = dLastdB;
                        dL = dLastdL;
                    }

                    //// 新坐标系下的经纬度（秒）
                    Lats = Lats + dB;
                    Lons = Lons + dL;
                    //// 秒转度
                    NewPnt.XorLat = TransFormMethod.SecondToDegree(Lats);
                    NewPnt.YorLon = TransFormMethod.SecondToDegree(Lons);
                    //// 经纬度转平面坐标
                    if (_type == EnumTransFormType.BLHtoXYH || _type == EnumTransFormType.XYHtoXYH)
                    {
                        TClass = new GaussTrans(EnumTransFormType.BLHtoXYH, EnumCoordinateFormat.Du, _earth_new, null, _dNewCentralMeridian, _dNewAddY);
                        NewPnt = TClass.Compute(NewPnt);
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPnt;
        }

        public List<CoordinatePoint> Compute()
        {
            throw new NotImplementedException();
        }

        public ESRI.ArcGIS.Geometry.IPoint PointCompute(IPoint p, IPoint NewPoint)
        {
            try
            {
                if (p == null)
                {
                    return NewPoint;
                }
                double dZ = double.IsNaN(p.Z) ? 0.0 : p.Z;
                CoordinatePoint point = new CoordinatePoint("", p.Y, p.X, dZ);
                CoordinatePoint NewPnt = Compute(point);
                NewPoint.X = NewPnt.YorLon;
                NewPoint.Y = NewPnt.XorLat;
                NewPoint.Z = NewPnt.HorZ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }

        public bool JudgeSpatialReference(ESRI.ArcGIS.Geodatabase.IFeatureClass pSourceFeatureClass)
        {
            bool flag = false;
            try
            {
                ISpatialReference pSpr = (pSourceFeatureClass as IGeoDataset).SpatialReference;
                flag = JudgeSpatialReference(pSpr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }

        public bool JudgeSpatialReference(ESRI.ArcGIS.Geometry.ISpatialReference pSpatialReference)
        {
            bool flag = false;
            try
            {
                if (_type == EnumTransFormType.BLHtoBLH || _type == EnumTransFormType.BLHtoXYH)
                {
                    if (pSpatialReference is IGeographicCoordinateSystem)
                    {
                        IGeographicCoordinateSystem sysType = pSpatialReference as IGeographicCoordinateSystem;
                        double A = sysType.Datum.Spheroid.SemiMajorAxis;            //// 长半轴
                        double F = 1.0 / sysType.Datum.Spheroid.Flattening;               //// 扁率
                        if (CheckGeoZB(A, F))
                        {
                            flag = true;
                        }
                    }
                }
                else
                {
                    if (pSpatialReference is IProjectedCoordinateSystem)
                    {
                        IProjectedCoordinateSystem sysType = pSpatialReference as IProjectedCoordinateSystem;
                        double dCentralMeridian = sysType.get_CentralMeridian(false);                                  //// 中央经线
                        double dFalseEastint = sysType.FalseEasting;
                        double A = sysType.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;            //// 长半轴
                        double F = 1.0 / sysType.GeographicCoordinateSystem.Datum.Spheroid.Flattening;               //// 扁率
                        if (CheckPrjZB(A, F, dCentralMeridian, dFalseEastint))
                        {
                            flag = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }

        public bool SetSpatialReference(ESRI.ArcGIS.Geodatabase.IFeatureClass pFeatureClass)
        {
            bool flag = false;
            try
            {
                flag = SetSpatialReference(pFeatureClass as IGeoDataset);
            }
            catch (Exception ex)
            {

            }
            return flag;
        }

        public bool SetSpatialReference(ESRI.ArcGIS.Geodatabase.IGeoDataset pGeoDataset)
        {
            bool flag = false;
            try
            {
                IGeoDatasetSchemaEdit pGeoE = pGeoDataset as IGeoDatasetSchemaEdit;
                ISpatialReference pSpr = GetSpatialReference();
                if (pSpr != null && pGeoE.CanAlterSpatialReference)
                {
                    pGeoE.AlterSpatialReference(pSpr);
                    flag = true;
                }
            }
            catch (Exception ex)
            {

            }
            return flag;
        }

        public ESRI.ArcGIS.Geometry.ISpatialReference GetSpatialReference()
        {
            ISpatialReference pSpr = null;
            try
            {
                if (_type == EnumTransFormType.XYHtoXYH || _type == EnumTransFormType.BLHtoXYH)
                {
                    pSpr = GISComm.CreateProjectCoordinateSystem(_earth_new, _dNewCentralMeridian, _dNewAddY);
                }
                else
                {
                    pSpr = GISComm.CreateGeographicCoordinateSystem(_earth_new);
                }
            }
            catch (Exception ex)
            {

            }
            return pSpr;
        }

        /// <summary>
        /// 检查投影坐标系空间参考参数值一致性
        /// </summary>
        /// <param name="A"></param>
        /// <param name="F"></param>
        private bool CheckPrjZB(double A, double F, double dCentralMeridian, double dFalseEastint)
        {
            if (Math.Abs(_earth_old.A - A) > 1E-09)
            {
                string str = string.Format("原数据坐标系“长半轴”值【{0}】与设置值【{1}】不一致", A, _earth_old.A);
                throw new Exception(str);
            }
            if (Math.Abs(_earth_old.F - F) > 1E-09)
            {
                string str = string.Format("原数据坐标系“扁率的倒数”值【{0}】与设置值【{1}】不一致", F, _earth_old.F);
                throw new Exception(str);
            }
            if (Math.Abs(_dOldCentralMeridian - dCentralMeridian) > 1E-09)
            {
                string str = string.Format("原数据坐标系“中央子午线”值【{0}】与设置值【{1}】不一致", dCentralMeridian, _dOldCentralMeridian);
                throw new Exception(str);
            }
            if (Math.Abs(_dOldAddY - dFalseEastint) > 1E-09)
            {
                string str = string.Format("原数据坐标系“东偏”值【{0}】与设置“Y坐标加常数”值【{1}】不一致", dFalseEastint, _dOldAddY);
                throw new Exception(str);
            }
            return true;
        }

        /// <summary>
        /// 检查地理坐标系空间参考参数值一致性
        /// </summary>
        /// <param name="A"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        private bool CheckGeoZB(double A, double F)
        {
            if (Math.Abs(_earth_old.A - A) > 1E-09)
            {
                string str = string.Format("原数据坐标系“长半轴”值【{0}】与设置值【{1}】不一致", A, _earth_old.A);
                throw new Exception(str);
            }
            if (Math.Abs(_earth_old.F - F) > 1E-09)
            {
                string str = string.Format("原数据坐标系“扁率的倒数”值【{0}】与设置值【{1}】不一致", F, _earth_old.F);
                throw new Exception(str);
            }
            return true;
        }
    }


}
