using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    public class GaussTrans : TCoordinate
    {
        private EnumTransFormType _Type;
        private EarthParams _Earth;
        private List<CoordinatePoint> _points;
        private double _dCentralMeridian;
        private double _dAddY;
        private EnumCoordinateFormat _format;

        public GaussTrans(EnumTransFormType type, EnumCoordinateFormat format, EarthParams earth, List<CoordinatePoint> points, double dCentralMeridian, double dAddY)
        {
            _Type = type;
            _format = format;
            _Earth = earth;
            _points = points;
            _dCentralMeridian = dCentralMeridian;
            _dAddY = dAddY;
        }

        public EnumTransFormType Type
        {
            get { return _Type; }
        }

        public CoordinatePoint Compute(CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                NewPoint.DH = point.DH;
                NewPoint.HorZ = point.HorZ;
                double dX = string.IsNullOrEmpty(point.XorLat.ToString().Trim()) ? 0.0 : double.Parse(point.XorLat.ToString().Trim());
                double dY = string.IsNullOrEmpty(point.YorLon.ToString().Trim()) ? 0.0 : double.Parse(point.YorLon.ToString().Trim());

                if (_Type == EnumTransFormType.BLtoXY || _Type == EnumTransFormType.BLHtoXYH)
                {
                    if (_format == EnumCoordinateFormat.ddmmss)
                    {
                        dX = TransFormMethod.DmsToDegree(dX);
                        dY = TransFormMethod.DmsToDegree(dY);
                    }
                    ////高斯正算
                    NewPoint.XorLat = TransFormMethod.GaussPositiveX(dX, dY, _dCentralMeridian, _Earth.A, _Earth.B);
                    NewPoint.YorLon = TransFormMethod.GaussPositiveY(dX, dY, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                }
                else
                {
                    //// 高斯反算
                    double dValue_X = TransFormMethod.GaussNegativeB(dX, dY, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                    double dValue_Y = TransFormMethod.GaussNegativeL(dX, dY, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                    if (_format == EnumCoordinateFormat.ddmmss)
                    {
                        dValue_X = TransFormMethod.DegreeToDms(dValue_X);
                        dValue_Y = TransFormMethod.DegreeToDms(dValue_Y);
                    }
                    NewPoint.XorLat = dValue_X;
                    NewPoint.YorLon = dValue_Y;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }

        /// <summary>
        /// 坐标点转换计算
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<CoordinatePoint> Compute()
        {
            List<CoordinatePoint> lstNew = new List<CoordinatePoint>();
            try
            {
                if (_points == null || _points.Count == 0)
                {
                    return lstNew;
                }
                foreach (CoordinatePoint point in _points)
                {
                    CoordinatePoint NewPoint = Compute(point);

                    lstNew.Add(NewPoint);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstNew;
        }

        /// <summary>
        /// 空间数据点转换
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public IPoint PointCompute(IPoint p, IPoint NewPoint)
        {
            try
            {
                if (p == null)
                {
                    return NewPoint;
                }
                double x = p.Y;
                double y = p.X;
                if (_Type == EnumTransFormType.BLtoXY || _Type == EnumTransFormType.BLHtoXYH)
                {
                    ////高斯正算
                    NewPoint.X = TransFormMethod.GaussPositiveY(x, y, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                    NewPoint.Y = TransFormMethod.GaussPositiveX(x, y, _dCentralMeridian, _Earth.A, _Earth.B);
                }
                else
                {
                    //// 高斯反算
                    NewPoint.Y = TransFormMethod.GaussNegativeB(x, y, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                    NewPoint.X = TransFormMethod.GaussNegativeL(x, y, _dCentralMeridian, _Earth.A, _Earth.B, _dAddY);
                }

                NewPoint.Z = p.Z;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPoint;
        }

        /// <summary>
        /// 判断空间参考是否一致
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <returns></returns>
        public bool JudgeSpatialReference(IFeatureClass pSourceFeatureClass)
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

        /// <summary>
        /// 判断空间参考是否一致
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <returns></returns>
        public bool JudgeSpatialReference(ISpatialReference pSpr)
        {
            bool flag = false;
            try
            {
                if (_Type == EnumTransFormType.BLtoXY || _Type == EnumTransFormType.BLHtoXYH)
                {
                    if (pSpr is IGeographicCoordinateSystem)
                    {
                        IGeographicCoordinateSystem sysType = pSpr as IGeographicCoordinateSystem;
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
                    if (pSpr is IProjectedCoordinateSystem)
                    {
                        IProjectedCoordinateSystem sysType = pSpr as IProjectedCoordinateSystem;
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

        /// <summary>
        /// 设置空间参考
        /// </summary>
        /// <param name="pFeatureClass"></param>
        public bool SetSpatialReference(IFeatureClass pFeatureClass)
        {
            bool flag = false;
            try
            {
                IGeoDataset pGeo = pFeatureClass as IGeoDataset;
                flag = SetSpatialReference(pGeo);
            }
            catch (Exception ex)
            {

            }
            return flag;
        }

        /// <summary>
        /// 设置空间参考
        /// </summary>
        /// <param name="pFeatureClass"></param>
        public bool SetSpatialReference(IGeoDataset pGeoDataset)
        {
            bool flag = false;
            try
            {
                ISpatialReference pSpr = null;

                IGeoDatasetSchemaEdit pGeoE = pGeoDataset as IGeoDatasetSchemaEdit;
                if (_Type == EnumTransFormType.BLtoXY || _Type == EnumTransFormType.BLHtoXYH)
                {
                    pSpr = GISComm.CreateProjectCoordinateSystem(_Earth, _dCentralMeridian, _dAddY);
                }
                else
                {
                    pSpr = GISComm.CreateGeographicCoordinateSystem(_Earth);
                }
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

        /// <summary>
        /// 获取新建的空间参考
        /// </summary>
        /// <returns></returns>
        public ISpatialReference GetSpatialReference()
        {
            ISpatialReference pSpr = null;
            try
            {
                if (_Type == EnumTransFormType.BLtoXY || _Type == EnumTransFormType.BLHtoXYH)
                {
                    pSpr = GISComm.CreateProjectCoordinateSystem(_Earth, _dCentralMeridian, _dAddY);
                }
                else
                {
                    pSpr = GISComm.CreateGeographicCoordinateSystem(_Earth);
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
            if (Math.Abs(_Earth.A - A) > 1E-09)
            {
                string str = string.Format("原数据坐标系“长半轴”值【{0}】与设置值【{1}】不一致", A, _Earth.A);
                throw new Exception(str);
            }
            if (Math.Abs(_Earth.F - F) > 1E-09)
            {
                string str = string.Format("原数据坐标系“扁率的倒数”值【{0}】与设置值【{1}】不一致", F, _Earth.F);
                throw new Exception(str);
            }
            if (Math.Abs(_dCentralMeridian - dCentralMeridian) > 1E-09)
            {
                string str = string.Format("原数据坐标系“中央子午线”值【{0}】与设置值【{1}】不一致", dCentralMeridian, _dCentralMeridian);
                throw new Exception(str);
            }
            if (Math.Abs(_dAddY - dFalseEastint) > 1E-09)
            {
                string str = string.Format("原数据坐标系“东偏”值【{0}】与设置“Y坐标加常数”值【{1}】不一致", dFalseEastint, _dAddY);
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
            if (Math.Abs(_Earth.A - A) > 1E-09)
            {
                string str = string.Format("原数据坐标系“长半轴”值【{0}】与设置值【{1}】不一致", A, _Earth.A);
                throw new Exception(str);
            }
            if (Math.Abs(_Earth.F - F) > 1E-09)
            {
                string str = string.Format("原数据坐标系“扁率的倒数”值【{0}】与设置值【{1}】不一致", F, _Earth.F);
                throw new Exception(str);
            }
            return true;
        }

    }
}
