using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 平面四参数转换
    /// </summary>
    public class TCoordinate2D_4Params : TCoordinate
    {
        private FourParams2D _Params;
        private List<CoordinatePoint> _points;
        private EarthParams _Earth = null;
        private double _dCentralMeridian = 0.0;
        private double _dAddY = 0.0;

        public TCoordinate2D_4Params(FourParams2D Params)
        {
            _Params = Params;
        }

        public TCoordinate2D_4Params(FourParams2D Params, List<CoordinatePoint> points)
        {
            _Params = Params;
            _points = points;
        }

        public TCoordinate2D_4Params(FourParams2D Params, EarthParams earth, List<CoordinatePoint> points, double dCentralMeridian, double dAddY)
        {
            _Params = Params;
            _points = points;
            _Earth = earth;
            _dCentralMeridian = dCentralMeridian;
            _dAddY = dAddY;
        }

        public CoordinatePoint Compute(CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                NewPoint.DH = point.DH;
                NewPoint.HorZ = point.HorZ;

                //// 四参数矩阵
                double dAngle = TransFormMethod.DegreeToArc(_Params.Angle / 3600.0);
                double c = _Params.ScaleK * Math.Sin(dAngle) * (-1.0);
                double d = _Params.ScaleK * Math.Cos(dAngle) - 1;
                double[,] MatrixX = new double[4, 1] { { _Params.DX }, { _Params.DY }, { c }, { d } };
                //// 坐标矩阵
                double[,] MatrixP = new double[2, 4] { { 1, 0, point.YorLon, point.XorLat }, { 0, 1, -point.XorLat, point.YorLon } };
                //// 新旧坐标差值矩阵
                double[,] MatrixL = TransFormMethod.MatrixMultiply(MatrixP, MatrixX);

                NewPoint.XorLat = MatrixL[0, 0] + point.XorLat;
                NewPoint.YorLon = MatrixL[1, 0] + point.YorLon;
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
        /// 坐标点转换计算
        /// </summary>
        /// <returns></returns>
        public List<CoordinatePoint> Compute(List<CoordinatePoint> lstPoint)
        {
            List<CoordinatePoint> lstNew = new List<CoordinatePoint>();
            try
            {
                if (lstPoint == null || lstPoint.Count == 0)
                {
                    return lstNew;
                }
                foreach (CoordinatePoint point in lstPoint)
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

                //// 四参数矩阵
                double dAngle = TransFormMethod.DegreeToArc(_Params.Angle / 3600.0);
                double c = _Params.ScaleK * Math.Sin(dAngle) * (-1.0);
                double d = _Params.ScaleK * Math.Cos(dAngle) - 1;
                double[,] MatrixX = new double[4, 1] { { _Params.DX }, { _Params.DY }, { c }, { d } };
                //// 坐标矩阵
                double[,] MatrixP = new double[2, 4] { { 1, 0, p.X, p.Y }, { 0, 1, -p.Y, p.X } };
                //// 新旧坐标差值矩阵
                double[,] MatrixL = TransFormMethod.MatrixMultiply(MatrixP, MatrixX);

                NewPoint.Y = MatrixL[0, 0] + p.Y;
                NewPoint.X = MatrixL[1, 0] + p.X;

                double dZ = double.IsNaN(p.Z) ? 0.0 : p.Z;
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
                    flag = true;
                }
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
                IGeoDatasetSchemaEdit pGeoE = pGeoDataset as IGeoDatasetSchemaEdit;

                //// 创建未知坐标系
                ISpatialReference spatialReference = new UnknownCoordinateSystemClass();
                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
                spatialReferenceResolution.ConstructFromHorizon();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
                spatialReferenceTolerance.SetDefaultXYTolerance();

                //// 修改要素类空间参考
                if (pGeoE.CanAlterSpatialReference)
                {
                    pGeoE.AlterSpatialReference(spatialReference);
                    flag = true;
                }

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
        /// 获取新建的空间参考
        /// </summary>
        /// <returns></returns>
        public ISpatialReference GetSpatialReference()
        {
            ISpatialReference pSpr = null;
            try
            {
                if (_Earth == null)
                {
                    //// 创建未知坐标系
                    pSpr = new UnknownCoordinateSystemClass();
                    ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)pSpr;
                    spatialReferenceResolution.ConstructFromHorizon();
                    ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)pSpr;
                    spatialReferenceTolerance.SetDefaultXYTolerance();
                }
                else
                {
                    pSpr = GISComm.CreateProjectCoordinateSystem(_Earth, _dCentralMeridian, _dAddY);
                }
            }
            catch (Exception ex)
            {

            }
            return pSpr;
        }
    }
}
