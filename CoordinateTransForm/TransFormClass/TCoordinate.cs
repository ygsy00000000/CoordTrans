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
    /// 坐标转换基类
    /// </summary>
    public interface TCoordinate
    {
        CoordinatePoint Compute(CoordinatePoint p);

        List<CoordinatePoint> Compute();

        IPoint PointCompute(IPoint p, IPoint newPoint);

        bool JudgeSpatialReference(IFeatureClass pSourceFeatureClass);

        bool JudgeSpatialReference(ISpatialReference pSpatialReference);

        bool SetSpatialReference(IFeatureClass pFeatureClass);

        bool SetSpatialReference(IGeoDataset pGeoDataset);

        ISpatialReference GetSpatialReference();
    }
}
