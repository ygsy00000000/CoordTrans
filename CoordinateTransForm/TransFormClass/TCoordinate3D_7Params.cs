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
    /// 三维七参数转换
    /// </summary>
    public class TCoordinate3D_7Params : TCoordinate
    {
        private EnumTransFormModel _Model;
        private SevenParams _Params;
        private List<CoordinatePoint> _points;
        private EnumTransFormType _type;
        private EnumCoordinateFormat _format;
        private EarthParams _earth_old;
        private EarthParams _earth_new;
        private double _dOldCentralMeridian;
        private double _dOldAddY;
        private double _dNewCentralMeridian;
        private double _dNewAddY;


        public TCoordinate3D_7Params(EnumTransFormType type, EnumCoordinateFormat format, EarthParams earth_old, EarthParams earth_new, EnumTransFormModel model, SevenParams Params, List<CoordinatePoint> points, double dOldCentralMeridian, double dOldAddY, double dNewCentralMeridian, double dNewAddY)
        {
            _type = type;
            _format = format;
            _earth_old = earth_old;
            _earth_new = earth_new;
            _Model = model;
            _Params = Params;
            _points = points;
            _dOldCentralMeridian = dOldCentralMeridian;
            _dOldAddY = dOldAddY;
            _dNewCentralMeridian = dNewCentralMeridian;
            _dNewAddY = dNewAddY;
        }


        public CoordinatePoint Compute(CoordinatePoint point)
        {
            CoordinatePoint NewPnt = new CoordinatePoint();
            try
            {
                TCoordinate TClass = null;

                //// 高斯反算转换 
                if (_type == EnumTransFormType.XYHtoBLH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(_type, _format, _earth_old, null, _dOldCentralMeridian, _dOldAddY);
                    point = TClass.Compute(point);
                }

                double[,] MatrixB = null, MatrixL = null;
                CoordinatePoint PointXYZ = null;

                if (_Model == EnumTransFormModel.Bursa || _Model == EnumTransFormModel.Molodensky)
                {
                    #region 布尔莎和莫洛金斯基
                    //// 大地坐标转换成空间直角坐标
                    PointXYZ = TransFormMethod.TransFormXYZ(_format, _earth_old, point);

                    //// 七参数矩阵
                    double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) },
                                                               { TransFormMethod.DegreeToArc(_Params.AngleX / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleY / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleZ / 3600.0)} };

                    //// 坐标矩阵
                    MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                    //// 新旧坐标差值矩阵
                    MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                    //// 新坐标系下的空间直角坐标
                    CoordinatePoint NewPointXYZ = new CoordinatePoint(point.DH, MatrixL[0, 0] + PointXYZ.XorLat, MatrixL[1, 0] + PointXYZ.YorLon, MatrixL[2, 0] + PointXYZ.HorZ);
                    //// 空间直角坐标转换成大地坐标
                    NewPnt = TransFormMethod.TransFormBLH(_format, _earth_new, NewPointXYZ);
                    NewPnt.DH = point.DH;
                    #endregion
                }
                else if (_Model == EnumTransFormModel._2D7Params || _Model == EnumTransFormModel._3D7Params)
                {
                    #region 二维七参数和三维七参数
                    #region 经纬度坐标转弧度
                    if (_format == EnumCoordinateFormat.Du)
                    {
                        PointXYZ = new CoordinatePoint("", TransFormMethod.DegreeToArc(point.XorLat), TransFormMethod.DegreeToArc(point.YorLon), point.HorZ);
                    }
                    else if (_format == EnumCoordinateFormat.ddmmss)
                    {
                        PointXYZ = new CoordinatePoint("", TransFormMethod.DmsToArc(point.XorLat), TransFormMethod.DmsToArc(point.YorLon), point.HorZ);
                    }
                    #endregion

                    //// 七参数矩阵
                    double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) }, { _Params.AngleX }, { _Params.AngleY }, { _Params.AngleZ } };

                    //// 坐标矩阵
                    MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                    //// 新旧坐标差值矩阵
                    MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                    //// 新坐标系下的弧度坐标
                    CoordinatePoint NewPointXYZ = GetNewPoint(MatrixL, PointXYZ); ;

                    #region 弧度转经纬度坐标
                    if (_format == EnumCoordinateFormat.Du)
                    {
                        NewPnt = new CoordinatePoint(point.DH, TransFormMethod.ArcToDegree(NewPointXYZ.XorLat), TransFormMethod.ArcToDegree(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                    }
                    else if (_format == EnumCoordinateFormat.ddmmss)
                    {
                        NewPnt = new CoordinatePoint(point.DH, TransFormMethod.ArcToDms(NewPointXYZ.XorLat), TransFormMethod.ArcToDms(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                    }

                    #endregion

                    #endregion

                }
                //// 高斯正算
                if (_type == EnumTransFormType.BLHtoXYH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(EnumTransFormType.BLHtoXYH, _format, _earth_new, null, _dNewCentralMeridian, _dNewAddY);
                    NewPnt = TClass.Compute(NewPnt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return NewPnt;
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
                TCoordinate TClass = null;

                //// 高斯反算转换 
                if (_type == EnumTransFormType.XYHtoBLH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(_type, _format, _earth_old, _points, _dOldCentralMeridian, _dOldAddY);
                    _points = TClass.Compute();
                }

                double[,] MatrixB = null, MatrixL = null;
                CoordinatePoint PointXYZ = null;

                foreach (CoordinatePoint point in _points)
                {
                    if (_Model == EnumTransFormModel.Bursa || _Model == EnumTransFormModel.Molodensky)
                    {
                        #region 布尔莎和莫洛金斯基
                        //// 大地坐标转换成空间直角坐标
                        PointXYZ = TransFormMethod.TransFormXYZ(_format, _earth_old, point);

                        //// 七参数矩阵
                        double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) },
                                                               { TransFormMethod.DegreeToArc(_Params.AngleX / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleY / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleZ / 3600.0)} };

                        //// 坐标矩阵
                        MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                        //// 新旧坐标差值矩阵
                        MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                        //// 新坐标系下的空间直角坐标
                        CoordinatePoint NewPointXYZ = new CoordinatePoint(point.DH, MatrixL[0, 0] + PointXYZ.XorLat, MatrixL[1, 0] + PointXYZ.YorLon, MatrixL[2, 0] + PointXYZ.HorZ);
                        //// 空间直角坐标转换成大地坐标
                        CoordinatePoint NewPoint = TransFormMethod.TransFormBLH(_format, _earth_new, NewPointXYZ);
                        NewPoint.DH = point.DH;
                        lstNew.Add(NewPoint);
                        #endregion
                    }
                    else if (_Model == EnumTransFormModel._2D7Params || _Model == EnumTransFormModel._3D7Params)
                    {
                        #region 二维七参数和三维七参数
                        #region 经纬度坐标转弧度
                        if (_format == EnumCoordinateFormat.Du)
                        {
                            PointXYZ = new CoordinatePoint("", TransFormMethod.DegreeToArc(point.XorLat), TransFormMethod.DegreeToArc(point.YorLon), point.HorZ);
                        }
                        else if (_format == EnumCoordinateFormat.ddmmss)
                        {
                            PointXYZ = new CoordinatePoint("", TransFormMethod.DmsToArc(point.XorLat), TransFormMethod.DmsToArc(point.YorLon), point.HorZ);
                        }
                        #endregion

                        //// 七参数矩阵
                        double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) }, { _Params.AngleX }, { _Params.AngleY }, { _Params.AngleZ } };

                        //// 坐标矩阵
                        MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                        //// 新旧坐标差值矩阵
                        MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                        //// 新坐标系下的弧度坐标
                        CoordinatePoint NewPointXYZ = GetNewPoint(MatrixL, PointXYZ); ;

                        #region 弧度转经纬度坐标
                        CoordinatePoint NewPoint = null;
                        if (_format == EnumCoordinateFormat.Du)
                        {
                            NewPoint = new CoordinatePoint(point.DH, TransFormMethod.ArcToDegree(NewPointXYZ.XorLat), TransFormMethod.ArcToDegree(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                        }
                        else if (_format == EnumCoordinateFormat.ddmmss)
                        {
                            NewPoint = new CoordinatePoint(point.DH, TransFormMethod.ArcToDms(NewPointXYZ.XorLat), TransFormMethod.ArcToDms(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                        }

                        lstNew.Add(NewPoint);
                        #endregion

                        #endregion
                    }
                }
                //// 高斯正算
                if (_type == EnumTransFormType.BLHtoXYH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(EnumTransFormType.BLHtoXYH, _format, _earth_new, lstNew, _dNewCentralMeridian, _dNewAddY);
                    lstNew = TClass.Compute();
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
                TCoordinate TClass = null;

                //// 高斯反算转换 
                if (_type == EnumTransFormType.XYHtoBLH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(_type, _format, _earth_old, lstPoint, _dOldCentralMeridian, _dOldAddY);
                    lstPoint = TClass.Compute();
                }

                double[,] MatrixB = null, MatrixL = null;
                CoordinatePoint PointXYZ = null;

                foreach (CoordinatePoint point in lstPoint)
                {
                    if (_Model == EnumTransFormModel.Bursa || _Model == EnumTransFormModel.Molodensky)
                    {
                        #region 布尔莎和莫洛金斯基
                        //// 大地坐标转换成空间直角坐标
                        PointXYZ = TransFormMethod.TransFormXYZ(_format, _earth_old, point);

                        //// 七参数矩阵
                        double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) },
                                                               { TransFormMethod.DegreeToArc(_Params.AngleX / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleY / 3600.0)}, 
                                                               { TransFormMethod.DegreeToArc(_Params.AngleZ / 3600.0)} };

                        //// 坐标矩阵
                        MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                        //// 新旧坐标差值矩阵
                        MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                        //// 新坐标系下的空间直角坐标
                        CoordinatePoint NewPointXYZ = new CoordinatePoint(point.DH, MatrixL[0, 0] + PointXYZ.XorLat, MatrixL[1, 0] + PointXYZ.YorLon, MatrixL[2, 0] + PointXYZ.HorZ);
                        //// 空间直角坐标转换成大地坐标
                        CoordinatePoint NewPoint = TransFormMethod.TransFormBLH(_format, _earth_new, NewPointXYZ);
                        NewPoint.DH = point.DH;
                        lstNew.Add(NewPoint);
                        #endregion
                    }
                    else if (_Model == EnumTransFormModel._2D7Params || _Model == EnumTransFormModel._3D7Params)
                    {
                        #region 二维七参数和三维七参数
                        #region 经纬度坐标转弧度
                        if (_format == EnumCoordinateFormat.Du)
                        {
                            PointXYZ = new CoordinatePoint("", TransFormMethod.DegreeToArc(point.XorLat), TransFormMethod.DegreeToArc(point.YorLon), point.HorZ);
                        }
                        else if (_format == EnumCoordinateFormat.ddmmss)
                        {
                            PointXYZ = new CoordinatePoint("", TransFormMethod.DmsToArc(point.XorLat), TransFormMethod.DmsToArc(point.YorLon), point.HorZ);
                        }
                        #endregion

                        //// 七参数矩阵
                        double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK / Math.Pow(10, 6) }, { _Params.AngleX }, { _Params.AngleY }, { _Params.AngleZ } };

                        //// 坐标矩阵
                        MatrixB = TransFormMethod.GetMatrixB(_earth_old, _earth_new, PointXYZ, ref _Params, _Model);

                        //// 新旧坐标差值矩阵
                        MatrixL = TransFormMethod.MatrixMultiply(MatrixB, MatrixX);
                        //// 新坐标系下的弧度坐标
                        CoordinatePoint NewPointXYZ = GetNewPoint(MatrixL, PointXYZ); ;

                        #region 弧度转经纬度坐标
                        CoordinatePoint NewPoint = null;
                        if (_format == EnumCoordinateFormat.Du)
                        {
                            NewPoint = new CoordinatePoint(point.DH, TransFormMethod.ArcToDegree(NewPointXYZ.XorLat), TransFormMethod.ArcToDegree(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                        }
                        else if (_format == EnumCoordinateFormat.ddmmss)
                        {
                            NewPoint = new CoordinatePoint(point.DH, TransFormMethod.ArcToDms(NewPointXYZ.XorLat), TransFormMethod.ArcToDms(NewPointXYZ.YorLon), NewPointXYZ.HorZ);
                        }

                        lstNew.Add(NewPoint);
                        #endregion

                        #endregion
                    }
                }
                //// 高斯正算
                if (_type == EnumTransFormType.BLHtoXYH || _type == EnumTransFormType.XYHtoXYH)
                {
                    TClass = new GaussTrans(EnumTransFormType.BLHtoXYH, _format, _earth_new, lstNew, _dNewCentralMeridian, _dNewAddY);
                    lstNew = TClass.Compute();
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
                double dZ = double.IsNaN(p.Z) ? 0.0 : p.Z;
                CoordinatePoint point = new CoordinatePoint("", p.Y, p.X, dZ);
                //if (p.SpatialReference is IProjectedCoordinateSystem)
                //{
                //    _type = TransFormType.XYHtoXYH;
                //}
                //else
                //{
                //    _type = TransFormType.BLHtoBLH;
                //}

                _format = EnumCoordinateFormat.Du;
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

        public IPointCollection PointCompute(IPointCollection p)
        {
            IPointCollection PntCol = null;
            try
            {
                if (p == null || p.PointCount == 0)
                {
                    return PntCol;
                }

                //// 七参数矩阵
                double[,] MatrixX = new double[7, 1] { { _Params.DX }, { _Params.DY }, { _Params.DZ }, { _Params.ScaleK }, { _Params.AngleX }, { _Params.AngleY }, { _Params.AngleZ } };
                double[,] MatrixP = new double[3 * p.PointCount, 7];
                for (int j = 0; j < p.PointCount; j++)
                {
                    IPoint point = p.get_Point(j);
                    double dZ = double.IsNaN(point.Z) ? 0.0 : point.Z;
                    CoordinatePoint pnt = new CoordinatePoint("", point.Y, point.X, dZ);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PntCol;
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
                if (_type == EnumTransFormType.BLHtoBLH || _type == EnumTransFormType.BLHtoXYH)
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
        public bool SetSpatialReference(IGeoDataset pGeoDataset)
        {
            bool flag = false;
            try
            {
                ISpatialReference pSpr = null;
                IGeoDatasetSchemaEdit pGeoE = pGeoDataset as IGeoDatasetSchemaEdit;
                if (_type == EnumTransFormType.XYHtoXYH || _type == EnumTransFormType.BLHtoXYH)
                {
                    pSpr = GISComm.CreateProjectCoordinateSystem(_earth_new, _dNewCentralMeridian, _dNewAddY);
                }
                else
                {
                    pSpr = GISComm.CreateGeographicCoordinateSystem(_earth_new);
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

        /// <summary>
        /// 根据矩阵L，获取转换后的新点
        /// </summary>
        /// <param name="MatrixL"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private CoordinatePoint GetNewPoint(double[,] MatrixL, CoordinatePoint point)
        {
            CoordinatePoint NewPoint = new CoordinatePoint();
            try
            {
                double p = 180.0 * 3600 / TransFormMethod.L_AREACAL_PI;             //// 弧度秒 1.0   180.0 * 3600 / L_AREACAL_PI    七参数是弧度时(三维七参数除外)，p=1，如果是秒时，p=180.0 * 3600 / L_AREACAL_PI
                double dA = _earth_new.A - _earth_old.A;
                double dF = (1 / _earth_new.F) - (1 / _earth_old.F);
                double E2 = TransFormMethod.GetE2(_earth_old.A, _earth_old.B);
                double Lat1 = point.XorLat;

                double W = Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(Lat1), 2));
                double N = _earth_old.A / W;                //// A/根号下（1-e2*sinB*sinB）   卯酉圈曲率半径N
                double M = _earth_old.A * (1 - E2) / Math.Pow(W, 3);                       //// 子午圈曲率半径M

                double num1 = N * E2 * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dA / (M * _earth_old.A)
                    + (2 - E2 * Math.Pow(Math.Sin(Lat1), 2)) * Math.Sin(Lat1) * Math.Cos(Lat1) * p * dF / (1 - 1 / _earth_old.F);

                double num2 = -W * dA + _earth_old.A * (1 - E2) * Math.Pow(Math.Sin(Lat1), 2) * dF / ((1 - _earth_old.A) * W);

                NewPoint.XorLat = point.XorLat + (num1 + MatrixL[1, 0]) / p;
                NewPoint.YorLon = point.YorLon + MatrixL[0, 0] / p;

                if (_Model == EnumTransFormModel._2D7Params)
                {
                    NewPoint.HorZ = point.HorZ;
                }
                else if (_Model == EnumTransFormModel._3D7Params)
                {
                    NewPoint.HorZ = point.HorZ + num2 + MatrixL[2, 0];
                }
            }
            catch (Exception ex)
            {

            }
            return NewPoint;
        }

    }
}
