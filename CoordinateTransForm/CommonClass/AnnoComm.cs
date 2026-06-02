using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans
{
    /// <summary>
    /// 注记要素公共方法
    /// </summary>
    public class AnnoComm
    {
        /// <summary>
        /// 添加注记要素
        /// </summary>
        /// <param name="pAnnoFeatureClass"></param>
        /// <param name="pElementColl"></param>
        public static void AddElementToFeatureclass(IFeatureClass pAnnoFeatureClass, IElementCollection pElementColl)
        {
            try
            {
                if (pAnnoFeatureClass == null||pElementColl==null||pElementColl.Count==0)
                {
                    return;
                }
                IDataset pDataset = pAnnoFeatureClass as IDataset;
                ITransactions pTransactions = pDataset.Workspace as ITransactions;
                pTransactions.StartTransaction();
                IFDOGraphicsLayerFactory pFDOGLFactory = new FDOGraphicsLayerFactoryClass();
                ILayer tmpLayer = pFDOGLFactory.OpenGraphicsLayer(pDataset.Workspace as IFeatureWorkspace, pAnnoFeatureClass.FeatureDataset, pDataset.Name);
                IFDOGraphicsLayer pFDOGLayer = tmpLayer as IFDOGraphicsLayer;
                //IElementCollection pElementColl = new ElementCollectionClass();
                pFDOGLayer.BeginAddElements();

                if (pElementColl.Count > 0)
                    pFDOGLayer.DoAddElements(pElementColl, 0);
                pFDOGLayer.EndAddElements();
                pElementColl.Clear();
                pTransactions.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 复制要素的注记信息
        /// </summary>
        /// <param name="pTargetFeature">新注记要素</param>
        /// <param name="pSourceFeature">原注记要素</param>
        /// <param name="pGeo">新注记的几何</param>
        /// <returns></returns>
        public static bool CopyAnnotationFeature(IFeature pTargetFeature, IFeature pSourceFeature)
        {
            try
            {
                bool isSuccess = false;
                IAnnotationFeature pTargetAnno = pTargetFeature as IAnnotationFeature;
                IAnnotationFeature pSourceAnnp = pSourceFeature as IAnnotationFeature;
                if (pTargetAnno == null || pSourceAnnp == null)
                {
                    return isSuccess;
                }
                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 复制要素的注记信息
        /// </summary>
        /// <param name="pTargetFeature">新注记要素</param>
        /// <param name="pSourceFeature">原注记要素</param>
        /// <param name="pGeo">新注记的几何</param>
        /// <returns></returns>
        public static bool CopyAnnotationFeature(IFeatureBuffer pTargetFeatureBuff, IFeature pSourceFeature)
        {
            try
            {
                bool isSuccess = false;
                IAnnotationFeature pTargetAnno = pTargetFeatureBuff as IAnnotationFeature;
                IAnnotationFeature pSourceAnnp = pSourceFeature as IAnnotationFeature;
                if (pTargetAnno == null || pSourceAnnp == null)
                {
                    return isSuccess;
                }
                //将原注记的Element赋值给新的要素
                pTargetAnno.Annotation = pSourceAnnp.Annotation;
                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
