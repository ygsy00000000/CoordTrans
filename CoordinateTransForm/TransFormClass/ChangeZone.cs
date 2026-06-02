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
    /// <summary>
    /// 坐标换带
    /// </summary>
    public class ChangeZone : TCoordinate
    {
        private EarthParams _Earth;
        private List<CoordinatePoint> _points;
        private double _dOldCentralMeridian;
        private double _dOldAddY;
        private double _dNewCentralMeridian;
        private double _dNewAddY;

        public ChangeZone(EarthParams earth, List<CoordinatePoint> points, double dOldCentralMeridian, double dOldAddY, double dNewCentralMeridian, double dNewAddY)
        {
            _Earth = earth;
            _points = points;
            _dOldCentralMeridian = dOldCentralMeridian;
            _dOldAddY = dOldAddY;
            _dNewCentralMeridian = dNewCentralMeridian;
            _dNewAddY = dNewAddY;
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

                //// 高斯反算
                double dLat = TransFormMethod.GaussNegativeB(dX, dY, _dOldCentralMeridian, _Earth.A, _Earth.B, _dOldAddY);
                double dLon = TransFormMethod.GaussNegativeL(dX, dY, _dOldCentralMeridian, _Earth.A, _Earth.B, _dOldAddY);

                ////高斯正算
                NewPoint.XorLat = TransFormMethod.GaussPositiveX(dLat, dLon, _dNewCentralMeridian, _Earth.A, _Earth.B);
                NewPoint.YorLon = TransFormMethod.GaussPositiveY(dLat, dLon, _dNewCentralMeridian, _Earth.A, _Earth.B, _dNewAddY);
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
                double dZ = double.IsNaN(p.Z) ? 0.0 : p.Z;
                //// 高斯反算
                double dLat = TransFormMethod.GaussNegativeB(x, y, _dOldCentralMeridian, _Earth.A, _Earth.B, _dOldAddY);
                double dLon = TransFormMethod.GaussNegativeL(x, y, _dOldCentralMeridian, _Earth.A, _Earth.B, _dOldAddY);

                ////高斯正算
                NewPoint.Y = TransFormMethod.GaussPositiveX(dLat, dLon, _dNewCentralMeridian, _Earth.A, _Earth.B);
                NewPoint.X = TransFormMethod.GaussPositiveY(dLat, dLon, _dNewCentralMeridian, _Earth.A, _Earth.B, _dNewAddY);

                NewPoint.Z = dZ;
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
                if (pSpr is IProjectedCoordinateSystem)
                {
                    IProjectedCoordinateSystem sysType = pSpr as IProjectedCoordinateSystem;
                    double dCentralMeridian = sysType.get_CentralMeridian(false);                                  //// 中央经线
                    double dFalseEastint = sysType.FalseEasting;
                    double A = sysType.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;            //// 长半轴
                    double F = 1.0 / sysType.GeographicCoordinateSystem.Datum.Spheroid.Flattening;               //// 扁率
                    if (CheckZB(A, F, dCentralMeridian, dFalseEastint))
                    {
                        flag = true;
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
                flag = SetSpatialReference(pFeatureClass as IGeoDataset);
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
                //ISpatialReference pSpr = (pFeatureClass as IGeoDataset).SpatialReference;
                IGeoDatasetSchemaEdit pGeoEdit = pGeoDataset as IGeoDatasetSchemaEdit;
                ISpatialReference pSpr = GISComm.CreateProjectCoordinateSystem(_Earth, _dNewCentralMeridian, _dNewAddY);
                if (pSpr != null && pGeoEdit.CanAlterSpatialReference)
                {
                    pGeoEdit.AlterSpatialReference(pSpr);
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
                pSpr = GISComm.CreateProjectCoordinateSystem(_Earth, _dNewCentralMeridian, _dNewAddY);
            }
            catch (Exception ex)
            {

            }
            return pSpr;
        }

        /// <summary>
        /// 检查空间参考参数值一致性
        /// </summary>
        /// <param name="A"></param>
        /// <param name="F"></param>
        private bool CheckZB(double A, double F, double dCentralMeridian, double dFalseEastint)
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

    }
}
