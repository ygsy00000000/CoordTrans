using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.GISModule.CommonModule;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// VCT数据转换公共方法
    /// </summary>
    public class TransVCTMathComm
    {
        /// <summary>
        /// VCT文件数据转换
        /// </summary>
        /// <param name="sSourceVCTPath"></param>
        /// <param name="sTargetVCTPath"></param>
        /// <param name="TClass"></param>
        /// <param name="sListError"></param>
        /// <returns></returns>
        public static bool TransVCT(string sSourceVCTPath, string sTargetVCTPath, TCoordinate TClass, ref List<string> sListError)
        {
            StreamWriter swVCT = null;
            bool isSuccess = false;
            try
            {
                if (sListError == null)
                {
                    sListError = new List<string>();
                }
                VCTHead pVCTHead = ReadVCTHead(sSourceVCTPath, ref sListError);
                if (pVCTHead == null)
                {
                    string sErr = "VCT格式有误！";
                    sListError.Add(sErr);
                    return isSuccess;
                }
                swVCT = new StreamWriter(sTargetVCTPath, false);
                if (swVCT == null)
                {
                    string sErr = "创建vct文件失败！";
                    sListError.Add(sErr);
                    return isSuccess;
                }
                char cSpheroid = pVCTHead.Separator.ToCharArray(0, 1)[0];
                //写入头文件
                pVCTHead.SetSpatialReferenceParam(TClass.GetSpatialReference());
                string sHeadErr = string.Empty;
                pVCTHead.Write(swVCT, ref sHeadErr);
                if (!string.IsNullOrWhiteSpace(sHeadErr))
                {
                    sListError.Add(sHeadErr);
                }

                bool isGeometryPart = false;//判断是否是图形部分

                //写入其他部分
                IPoint pPoint = new PointClass();
                IPoint pNewPoint = new PointClass();
                using (StreamReader sr = new StreamReader(sSourceVCTPath))
                {
                    if (sr == null)
                    {
                        return isSuccess;
                    }

                    double X = 0.0, Y = 0.0;
                    int iCount = 0;
                    while (sr.Peek() > -1)
                    {
                        string sRead = sr.ReadLine();
                        //原VCT头文件不写入
                        if (sRead.Equals(VCTSevenPart.HeadBegin))
                        {
                            sRead = sr.ReadLine();
                            while (!sRead.Equals(VCTSevenPart.HeadEnd))
                            {
                                sRead = sr.ReadLine();
                            }
                        }
                        else
                        {
                            //几何部分开始
                            if (sRead.Equals(VCTSevenPart.PointBegin)||sRead.Equals(VCTSevenPart.LineBegin)
                                ||sRead.Equals(VCTSevenPart.PolygonBegin)||sRead.Equals(VCTSevenPart.AnnotationBegin))
                            {
                                isGeometryPart = true;
                            }
                            //几何部分结束
                            if (sRead.Equals(VCTSevenPart.PointEnd)||sRead.Equals(VCTSevenPart.LineEnd)
                                ||sRead.Equals(VCTSevenPart.PolygonEnd)||sRead.Equals(VCTSevenPart.AnnotationEnd))
                            {
                                isGeometryPart = false;
                            }
                            if (isGeometryPart)
                            {
                                bool isCoordinate = IsCoordinate(sRead, ref X, ref Y, cSpheroid);
                                //对坐标数据进行坐标转换
                                if (isCoordinate)
                                {
                                    pPoint.PutCoords(X, Y);
                                    pNewPoint = TClass.PointCompute(pPoint, pNewPoint);
                                    if (pNewPoint != null)
                                    {
                                        sRead = pNewPoint.X.ToString() + pVCTHead.Separator + pNewPoint.Y.ToString();
                                    }

                                }
                            }
                            
                            
                            swVCT.WriteLine(sRead);
                            iCount++;
                            //每1000条压缩一次
                            if (iCount%1000==0)
                            {
                                swVCT.Flush();
                            }
                        }
                    }

                    if (sr != null)
                    {
                        sr.Close();
                    }
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                string sErr = ex.Message;
                sListError.Add(sErr);
            }
            finally
            {
                if (swVCT!=null)
                {
                    swVCT.Flush();
                    swVCT.Close();
                }
            }
            return isSuccess;
        }

        /// <summary>
        /// 读取VCT文件的头文件
        /// </summary>
        /// <param name="sVCTPath"></param>
        /// <param name="pVCTHead"></param>
        /// <param name="cSeparator"></param>
        /// <param name="sListError"></param>
        public static VCTHead ReadVCTHead(string sVCTPath, ref List<string> sListError)
        {
            VCTHead pVCTHead = null;
            try
            {
                if (!File.Exists(sVCTPath))
                {
                    string sErr = "VCT文件不存在！";
                    sListError.Add(sErr);
                    return pVCTHead;
                }
                string sVErr = string.Empty;
                using (StreamReader sr = new StreamReader(sVCTPath))
                {
                    if (sr == null)
                    {
                        return pVCTHead;
                    }
                    bool bHaveReadHead = false;
                    while (sr.Peek() > -1)
                    {
                        string sRead = sr.ReadLine();
                        switch (sRead)
                        {
                            case VCTSevenPart.HeadBegin:
                                pVCTHead = VCTHead.Read(sr, ref sVErr);
                                bHaveReadHead = true;
                                break;
                            default:
                                break;
                        }
                        if (bHaveReadHead)
                        {
                            break;
                        }
                    }
                }
            }
            catch { }
            return pVCTHead;
        }

        #region 未用到代码
        /// <summary>
        /// 复制VCT中的非几何部分要素
        /// </summary>
        /// <param name="sSourceSR"></param>
        /// <param name="sTargetSW"></param>
        /// <param name="sStartValue"></param>
        /// <param name="sEndValue"></param>
        /// <returns></returns>
        public static bool CopyValue(StreamReader sSourceSR, StreamWriter sTargetSW, string sStartValue, string sEndValue)
        {
            bool isSuccess = false;
            try
            {
                if (sSourceSR == null || sTargetSW == null)
                {
                    return isSuccess;
                }
                sTargetSW.WriteLine(sStartValue);
                if (sSourceSR.Peek() > -1)
                {
                    string sLastLine = sSourceSR.ReadLine();
                    sTargetSW.WriteLine(sLastLine);
                    //循环复制
                    while (!sLastLine.Equals(sEndValue))
                    {
                        sLastLine = sSourceSR.ReadLine();
                        sTargetSW.WriteLine(sLastLine);
                    }
                }
                isSuccess = true;
            }
            catch (Exception)
            {
            }
            return isSuccess;
        }
        #endregion
        /// <summary>
        /// 判断VCT一行数据是否是坐标串，如果是返回X、Y坐标
        /// </summary>
        /// <param name="sValue"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cSeparator"></param>
        /// <returns></returns>
        public static bool IsCoordinate(string sValue, ref double X, ref double Y, char cSeparator)
        {
            bool isXY = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(sValue))
                {
                    string[] aValue = sValue.Split(cSeparator);
                    //根据分隔符分裂后等于2
                    if (aValue.Length == 2)
                    {
                        string sX = aValue[0];
                        string sY = aValue[1];
                        //包含小数点
                        if (!string.IsNullOrWhiteSpace(sX) && !string.IsNullOrWhiteSpace(sY)
                            && (sX.Split('.').Length == 2 || sY.Split('.').Length == 2))
                        {
                            //是数字型
                            if (double.TryParse(sX, out X) && double.TryParse(sY, out Y))
                            {
                                isXY = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return isXY;
        }
    }

    #region VCT第一部分头文件

    /// <summary>
    /// VCT编写的常量
    /// </summary>
    public class VCTSevenPart
    {
        /// <summary>
        /// 换行符
        /// </summary>
        public const string Wrap = "\r\n";

        /// <summary>
        /// 标识码字段名称
        /// </summary>
        public const string BSMFieldName = "BSM";

        /// <summary>
        /// 要素代码名称
        /// </summary>
        public const string YSDMFieldName = "YSDM";

        /// <summary>
        /// 第一部分头文件开始标识
        /// </summary>
        public const string HeadBegin = "HeadBegin";

        /// <summary>
        /// 第一部分头文件结束标识
        /// </summary>
        public const string HeadEnd = "HeadEnd";


        /// <summary>
        /// 第二部分要素参数描述开始标识
        /// </summary>
        public const string FeatureCodeBegin = "FeatureCodeBegin";

        /// <summary>
        /// 第二部分要素参数描述结束标识
        /// </summary>
        public const string FeatureCodeEnd = "FeatureCodeEnd";

        /// <summary>
        /// 第三部分属性数据结构开始标识
        /// </summary>
        public const string TableStructureBegin = "TableStructureBegin";

        /// <summary>
        /// 第三部分属性数据结构结束标识
        /// </summary>
        public const string TableStructureEnd = "TableStructureEnd";


        /// <summary>
        /// 点状要素开始标识
        /// </summary>
        public const string PointBegin = "PointBegin";

        /// <summary>
        /// 点状要素结束标识
        /// </summary>
        public const string PointEnd = "PointEnd";

        /// <summary>
        /// 线状要素开始标识
        /// </summary>
        public const string LineBegin = "LineBegin";

        /// <summary>
        /// 线状要素结束标识
        /// </summary>
        public const string LineEnd = "LineEnd";


        /// <summary>
        /// 面状要素开始标识
        /// </summary>
        public const string PolygonBegin = "PolygonBegin";

        /// <summary>
        /// 面状要素结束标识
        /// </summary>
        public const string PolygonEnd = "PolygonEnd";


        /// <summary>
        /// 注记开始标识
        /// </summary>
        public const string AnnotationBegin = "AnnotationBegin";

        /// <summary>
        /// 注记结束标识
        /// </summary>
        public const string AnnotationEnd = "AnnotationEnd";


        /// <summary>
        /// 拓扑数据开始标识
        /// </summary>
        public const string TopologyBegin = "TopologyBegin";

        /// <summary>
        /// 拓扑数据结束标识
        /// </summary>
        public const string TopologyEnd = "TopologyEnd";


        /// <summary>
        /// 属性数据开始标识
        /// </summary>
        public const string AttributeBegin = "AttributeBegin";

        /// <summary>
        /// 属性数据结束标识
        /// </summary>
        public const string AttributeEnd = "AttributeEnd";
    }

    ///// <summary>
    ///// VCT第一部分头文件，三调文档标准
    ///// </summary>
    //public class VCTHead
    //{

    //    private string sDatamark = "LANDUSE-VCT";
    //    /// <summary>
    //    /// 交换格式标志
    //    /// </summary>
    //    public string DataMark
    //    {
    //        get
    //        {
    //            return this.sDatamark.Trim();
    //        }
    //        set
    //        {
    //            this.sDatamark = value;
    //        }
    //    }


    //    private string sVersion = "GB/T 17798-2007";
    //    /// <summary>
    //    /// 版本号
    //    /// </summary>
    //    public string Version
    //    {
    //        get
    //        {
    //            return this.sVersion.Trim();
    //        }
    //        set
    //        {
    //            this.sVersion = value;
    //        }
    //    }

    //    private string sCoordinateSystemType = string.Empty;
    //    /// <summary>
    //    /// 坐标系统类型
    //    /// </summary>
    //    public string CoordinateSystemType
    //    {
    //        get { return sCoordinateSystemType; }
    //        set { this.sCoordinateSystemType = value; }
    //    }

    //    private int sDim = 2;
    //    /// <summary>
    //    /// 坐标维数
    //    /// </summary>
    //    public int Dim
    //    {
    //        get
    //        {
    //            return this.sDim;
    //        }
    //        set
    //        {
    //            this.sDim = value;
    //        }
    //    }

    //    private string sXAxisDirection = "E";
    //    /// <summary>
    //    /// X坐标轴方向
    //    /// </summary>
    //    public string XAxisDirection
    //    {
    //        get { return this.sXAxisDirection; }
    //        set { this.sXAxisDirection = value; }
    //    }

    //    private string sYAxisDirection = "N";
    //    /// <summary>
    //    /// Y坐标轴方向
    //    /// </summary>
    //    public string YAxisDirection
    //    {
    //        get { return this.sYAxisDirection; }
    //        set { this.sYAxisDirection = value; }
    //    }

    //    private string sXYUnit = "M";
    //    /// <summary>
    //    /// 平面坐标单位
    //    /// </summary>
    //    public string XYUnit
    //    {
    //        get
    //        {
    //            return this.sXYUnit.Trim();
    //        }
    //        set
    //        {
    //            this.sXYUnit = value;
    //        }
    //    }


    //    private string sSpheroid = string.Empty;
    //    /// <summary>
    //    /// 参考椭球
    //    /// </summary>
    //    public string Spheroid
    //    {
    //        get { return this.sSpheroid; }
    //        set { this.sSpheroid = value; }
    //    }

    //    private string sPrimeMeridian = string.Empty;
    //    /// <summary>
    //    /// 首子午线
    //    /// </summary>
    //    public string PrimeMeridian
    //    {
    //        get { return this.sPrimeMeridian; }
    //        set { this.sPrimeMeridian = value; }
    //    }

    //    private string sProjection = string.Empty;
    //    /// <summary>
    //    /// 投影类型
    //    /// </summary>
    //    public string Projection
    //    {
    //        set
    //        {
    //            this.sProjection = value;
    //        }
    //        get
    //        {
    //            return this.sProjection.Trim();
    //        }
    //    }

    //    private string sParameters = string.Empty;
    //    /// <summary>
    //    /// 投影参数
    //    /// </summary>
    //    public string Parameters
    //    {
    //        get { return this.sParameters; }
    //        set { this.sParameters = value; }
    //    }

    //    private string sVerticalDatum = string.Empty;
    //    /// <summary>
    //    /// 高程基准
    //    /// </summary>
    //    public string VerticalDatum
    //    {
    //        get { return this.sVerticalDatum; }
    //        set { this.sVerticalDatum = value; }
    //    }


    //    private string sTemporalReferenceSystem = "北京时间,+0800";
    //    /// <summary>
    //    /// 时间参照系
    //    /// </summary>
    //    public string TemporalReferenceSystem
    //    {
    //        get { return this.sTemporalReferenceSystem; }
    //        set { this.sTemporalReferenceSystem = value; }
    //    }

    //    private string sExtentMin = string.Empty;
    //    /// <summary>
    //    /// 最小坐标
    //    /// </summary>
    //    public string ExtentMin
    //    {
    //        get { return this.sExtentMin; }
    //        set { this.sExtentMin = value; }
    //    }

    //    private string sExtentMax = string.Empty;
    //    /// <summary>
    //    /// 最大坐标
    //    /// </summary>
    //    public string ExtentMax
    //    {
    //        get { return this.sExtentMax; }
    //        set { this.sExtentMax = value; }
    //    }

    //    private int sMapScale = 5000;
    //    /// <summary>
    //    /// 比例尺分母
    //    /// </summary>
    //    public int MapScale
    //    {
    //        get
    //        {
    //            return this.sMapScale;
    //        }
    //        set
    //        {
    //            this.sMapScale = value;
    //        }
    //    }

    //    private double dOffset = 0.0;
    //    /// <summary>
    //    /// 坐标偏移量
    //    /// </summary>
    //    public double Offset
    //    {
    //        get { return this.dOffset; }
    //        set { this.dOffset = value; }
    //    }

    //    private System.DateTime pDate = System.DateTime.Today;
    //    /// <summary>
    //    /// 数据生成日期
    //    /// </summary>
    //    public System.DateTime Date
    //    {
    //        get
    //        {
    //            return this.pDate;
    //        }
    //        set
    //        {
    //            this.pDate = value;
    //        }
    //    }

    //    private string sSeparator = ",";
    //    /// <summary>
    //    /// 属性字段分隔符
    //    /// </summary>
    //    public string Separator
    //    {
    //        get { return this.sSeparator; }
    //        set { this.sSeparator = value; }
    //    }

    //    /// <summary>
    //    /// 创建地理坐标系
    //    /// </summary>
    //    /// <returns></returns>
    //    private ISpatialReference CreateGeographicCoordinateSystem()
    //    {
    //        ISpatialReference pSpatialReference = null;

    //        try
    //        {
    //            ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
    //            string[] sSpheroids = this.Spheroid.Split(',');
    //            string sName = sSpheroids[0];
    //            string sSpheroidOther = sSpheroids[1] + "," + sSpheroids[2];

    //            string sGEOGCS = string.Format("GCS_{0}", sName);
    //            string sDATUM = string.Format("D_{0}", sName);
    //            string sPRJ = string.Format("GEOGCS[\"{0}\",DATUM[\"{1}\",SPHEROID[\"{2}\",{3}]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]]", sGEOGCS, sDATUM, sName, sSpheroidOther);
    //            string sPRJFile = AppFileComm.SysConfigPath + @"Coordinate Systems\" + sGEOGCS + ".prj";
    //            if (FileComm.WriteFile(sPRJ, sPRJFile))
    //            {
    //                pSpatialReference = pSpatialReferenceFactory.CreateESRISpatialReferenceFromPRJFile(sPRJFile);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            LogComm.WriteLog("CreateProjectCoordinateSystem", ex);
    //        }

    //        return pSpatialReference;
    //    }

    //    /// <summary>
    //    /// 创建投影坐标系
    //    /// </summary>
    //    /// <returns></returns>
    //    private ISpatialReference CreateProjectCoordinateSystem()
    //    {
    //        ISpatialReference pSpatialReference = null;

    //        try
    //        {

    //            ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
    //            string[] sSpheroids = this.Spheroid.Split(',');
    //            string sName = sSpheroids[0];
    //            string sSpheroidOther = sSpheroids[1] + "," + sSpheroids[2];

    //            string[] sParameters = this.Parameters.Split(',');
    //            int sMeridian = Convert.ToInt32(sParameters[0].Substring(0, 3));
    //            string sZoneParam = sParameters[5];
    //            string sZone = string.Empty;
    //            if (!sZoneParam.Equals("500000"))
    //            {
    //                sZone = sZoneParam.Substring(0, 2);
    //            }

    //            string sPROJCSName = string.Empty;
    //            if (sZone.Length.Equals(2))
    //            {
    //                sPROJCSName = string.Format("{0}_3_Degree_GK_Zone_{1}", sName, sZone);
    //            }
    //            else
    //            {
    //                sPROJCSName = string.Format("{0}_3_Degree_GK_CM_{1}E", sName, sMeridian);
    //            }
    //            string sPRJFile = AppFileComm.SysConfigPath + @"Coordinate Systems\" + sPROJCSName + ".prj";
    //            string sGEOGCS = string.Format("GCS_{0}", sName);
    //            string sDATUM = string.Format("D_{0}", sName);
    //            if (this.Projection.Equals("高斯-克吕格"))
    //            {
    //                this.Projection = "Gauss_Kruger";
    //            }
    //            string sPRJ = string.Format("PROJCS[\"{0}\",GEOGCS[\"{1}\",DATUM[\"{2}\",SPHEROID[\"{3}\",{4}]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"{5}\"],PARAMETER[\"False_Easting\",{6}],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",{7}],PARAMETER[\"Scale_Factor\",1.0],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]", sPROJCSName, sGEOGCS, sDATUM, sName, sSpheroidOther, this.Projection, sZoneParam, sMeridian);
    //            //pSpatialReference = pSpatialReferenceFactory.CreateESRISpatialReferenceFromPRJFile(sPRJFile);
    //            if (FileComm.WriteFile(sPRJ, sPRJFile))
    //            {
    //                pSpatialReference = pSpatialReferenceFactory.CreateESRISpatialReferenceFromPRJFile(sPRJFile);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            LogComm.WriteLog("CreateProjectCoordinateSystem", ex);
    //        }

    //        return pSpatialReference;
    //    }

    //    /// <summary>
    //    /// 根据参数创建ISpatialReference
    //    /// </summary>
    //    /// <returns></returns>
    //    public ISpatialReference CreateSpatialReference()
    //    {
    //        ISpatialReference pSpatialReference = null;
    //        if (this.CoordinateSystemType.Equals("D"))
    //        {
    //            pSpatialReference = CreateGeographicCoordinateSystem();
    //        }
    //        else if (this.CoordinateSystemType.Equals("P"))
    //        {
    //            pSpatialReference = CreateProjectCoordinateSystem();
    //        }

    //        ISpatialReferenceTolerance pSpatialReferenceTolerance = pSpatialReference as ISpatialReferenceTolerance;
    //        pSpatialReferenceTolerance.SetDefaultXYTolerance();
    //        ISpatialReferenceResolution pSpatialReferenceResolution = pSpatialReference as ISpatialReferenceResolution;
    //        pSpatialReferenceResolution.SetDefaultXYResolution();

    //        return pSpatialReference;
    //    }

    //    public void SetSpatialReferenceParam(ISpatialReference spatialReference)
    //    {
    //        try
    //        {
    //            if (spatialReference != null)
    //            {
    //                if (spatialReference is IGeographicCoordinateSystem)
    //                {
    //                    IGeographicCoordinateSystem pGeographicCoordinateSystem = spatialReference as IGeographicCoordinateSystem;
    //                    this.CoordinateSystemType = "D";
    //                    this.XAxisDirection = string.Empty;
    //                    this.YAxisDirection = string.Empty;
    //                    this.XYUnit = "D";
    //                    double dSemiMajorAxis = pGeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;//长半轴
    //                    double dSemiMinorAxis = pGeographicCoordinateSystem.Datum.Spheroid.SemiMinorAxis;//短半轴;
    //                    double dPL = dSemiMajorAxis / (dSemiMajorAxis - dSemiMinorAxis);//扁率的倒数
    //                    this.Spheroid = pGeographicCoordinateSystem.Datum.Spheroid.Name + "," + dSemiMajorAxis.ToString() + "," + dPL.ToString();
    //                    this.PrimeMeridian = pGeographicCoordinateSystem.PrimeMeridian.Longitude.ToString();
    //                    //this.Projection = pGeographicCoordinateSystem.Projection.Name;
    //                }
    //                else
    //                {
    //                    IProjectedCoordinateSystem pProjectedCoordinateSystem = spatialReference as IProjectedCoordinateSystem;
    //                    this.CoordinateSystemType = "P";
    //                    //this.XYUnit = pProjectedCoordinateSystem.CoordinateUnit.Name;
    //                    double dSemiMajorAxis = pProjectedCoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis;//长半轴
    //                    double dSemiMinorAxis = pProjectedCoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.SemiMinorAxis;//短半轴;
    //                    double dPL = MathComm.Rounding(dSemiMajorAxis / (dSemiMajorAxis - dSemiMinorAxis), 9);//扁率的倒数
    //                    this.Spheroid = pProjectedCoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.Name + "," + dSemiMajorAxis.ToString() + "," + dPL.ToString();
    //                    this.PrimeMeridian = pProjectedCoordinateSystem.GeographicCoordinateSystem.PrimeMeridian.Longitude.ToString();

    //                    //this.Projection = pProjectedCoordinateSystem.Projection.Name;
    //                    this.Projection = "高斯-克吕格";

    //                    double dMeridian = pProjectedCoordinateSystem.get_CentralMeridian(false);//原点精度
    //                    double dLatitude = 0;//原点纬度
    //                    string sFalse_Easting = pProjectedCoordinateSystem.FalseEasting.ToString();
    //                    string sZone = string.Empty;
    //                    if (!sFalse_Easting.Equals("500000"))
    //                    {
    //                        sZone = sFalse_Easting.Substring(0, 2);
    //                    }

    //                    this.Parameters = string.Format("{0},,,,1.0,{1},0.0,3,{2}", dMeridian, sFalse_Easting, sZone);

    //                }
    //                //this.Dim = value.HasZPrecision() ? 3 : 2;
    //                this.Dim = 2;
    //                this.PrimeMeridian = "0";
    //                double MinX, MinY, MaxX, MaxY;
    //                spatialReference.GetDomain(out MinX, out MaxX, out MinY, out MaxY);
    //                this.ExtentMax = MaxX.ToString() + this.Separator + MaxY.ToString();
    //                this.ExtentMin = MinX.ToString() + this.Separator + MinY.ToString();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            LogComm.WriteLog("SpatialReferenceSet", ex);
    //            throw ex;
    //        }
    //    }

    //    /// <summary>
    //    /// 写入头文件
    //    /// </summary>
    //    /// <param name="sw"></param>
    //    public void Write(StreamWriter sw, ref string sErr)
    //    {
    //        try
    //        {
    //            sw.WriteLine(VCTSevenPart.HeadBegin);

    //            //遍历头文件对象，并写入对象的信息
    //            System.Reflection.PropertyInfo[] pSourcePropertyInfos = this.GetType().GetProperties();
    //            foreach (System.Reflection.PropertyInfo pSourcePropertyInfo in pSourcePropertyInfos)
    //            {
    //                string sSourceName = pSourcePropertyInfo.Name;
    //                object oSourceValue = pSourcePropertyInfo.GetValue(this, null);
    //                if (oSourceValue == null || string.IsNullOrWhiteSpace(oSourceValue.ToString()))
    //                {
    //                    continue;
    //                }
    //                if (oSourceValue.GetType() == typeof(DateTime))
    //                {
    //                    DateTime dSourceValue = DateTime.Today;
    //                    bool isS = DateTime.TryParse(oSourceValue.ToString(), out dSourceValue);
    //                    if (isS)
    //                    {
    //                        oSourceValue = dSourceValue.ToString("yyyyMMdd");
    //                    }

    //                }
    //                string sRowVlue = sSourceName + ":" + oSourceValue.ToString();
    //                sw.WriteLine(sRowVlue);
    //            }
    //            sw.WriteLine(VCTSevenPart.HeadEnd);
    //        }
    //        catch (Exception ex)
    //        {
    //            LogComm.WriteLog("WriterVCTHead", ex);
    //            sErr += ex.Message + VCTSevenPart.Wrap;
    //        }
    //        finally
    //        {
    //            if (sw != null)
    //            {
    //                sw.Flush();
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 读取vct文件中的头文件信息
    //    /// </summary>
    //    /// <param name="sr"></param>
    //    /// <param name="sLastLine">返回结束标志</param>
    //    /// <returns></returns>
    //    public static VCTHead Read(StreamReader sr)
    //    {
    //        VCTHead pVCTHead = new VCTHead();
    //        try
    //        {
    //            //判断是否已读到文件末尾
    //            if (sr.Peek() > -1)
    //            {
    //                string sLastLine = sr.ReadLine();
    //                while (!sLastLine.Equals(VCTSevenPart.HeadEnd))
    //                {
    //                    if (!string.IsNullOrWhiteSpace(sLastLine))
    //                    {
    //                        string[] p = sLastLine.Split(':');
    //                        if (p.Length >= 2 && !string.IsNullOrWhiteSpace(p[0]) && !string.IsNullOrWhiteSpace(p[1]))
    //                        {
    //                            System.Reflection.PropertyInfo[] pSourcePropertyInfos = pVCTHead.GetType().GetProperties();

    //                            //将读取的数据存放到头文件对象中
    //                            foreach (System.Reflection.PropertyInfo pSourcePropertyInfo in pSourcePropertyInfos)
    //                            {
    //                                string sSourceName = pSourcePropertyInfo.Name;
    //                                if (string.IsNullOrWhiteSpace(sSourceName))
    //                                {
    //                                    continue;
    //                                }
    //                                if (sSourceName.Equals(p[0]))
    //                                {
    //                                    object oSourceValue = pSourcePropertyInfo.GetValue(pVCTHead, null);
    //                                    if (oSourceValue == null)
    //                                    {
    //                                        pSourcePropertyInfo.SetValue(pVCTHead, p[1], null);
    //                                    }
    //                                    else
    //                                    {
    //                                        if (oSourceValue.GetType() == typeof(DateTime))
    //                                        {
    //                                            DateTime dSourceValue = DateTime.Today;
    //                                            bool isS = DateTime.TryParse(oSourceValue.ToString(), out dSourceValue);
    //                                            if (isS)
    //                                            {
    //                                                pSourcePropertyInfo.SetValue(pVCTHead, dSourceValue, null);
    //                                            }
    //                                        }
    //                                        else if (oSourceValue.GetType() == typeof(int))
    //                                        {
    //                                            int iValue = 0;
    //                                            if (int.TryParse(p[1], out iValue))
    //                                            {
    //                                                pSourcePropertyInfo.SetValue(pVCTHead, iValue, null);
    //                                            }
    //                                        }
    //                                        else if (oSourceValue.GetType() == typeof(double))
    //                                        {
    //                                            double dValue = 0.0;
    //                                            if (double.TryParse(p[1], out dValue))
    //                                            {
    //                                                pSourcePropertyInfo.SetValue(pVCTHead, dValue, null);
    //                                            }
    //                                        }
    //                                        else
    //                                        {
    //                                            pSourcePropertyInfo.SetValue(pVCTHead, p[1], null);
    //                                        }
    //                                    }
    //                                    //匹配到后跳出匹配循环
    //                                    break;
    //                                }
    //                            }
    //                        }
    //                    }
    //                    if (sr.Peek() < 0)
    //                    {
    //                        sLastLine = string.Empty;
    //                        break;
    //                    }
    //                    sLastLine = sr.ReadLine();
    //                }
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //            LogComm.WriteLog("", ex);
    //            throw ex;
    //        }
    //        return pVCTHead;
    //    }

    //}
    #endregion
}
