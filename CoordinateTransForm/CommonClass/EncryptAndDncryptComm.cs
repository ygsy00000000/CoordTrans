using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZJUGIS.Framework.CommonMethod;
using ZJUGIS.Framework.Dev;

namespace ZJUGIS.CoordinateTrans.CommonClass.EncryptionAndDecrypt
{
    /// <summary>
    /// 加密解密公共类
    /// </summary>
    public class EncryptAndDncryptComm
    {
        private const string _sLog = "加密解密文件";

        #region 外部调用
        /// <summary>
        /// 形成一个XML，并且将其形成加密文件
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="sFilePath"></param>
        public static bool CreateXMLFromDicAndEncrpyXML(Dictionary<string, MapCorrection> dic, string sFilePath)
        {
            bool bIsOk = false;
            string sXMLPath = AppFileComm.AppPath + "Temp\\" + "temp.xml";
            //sFilePath = AppFileComm.AppPath + "Temp\\" + "\\坐标改正量文件.dlic";
            if (dic == null || dic.Count.Equals(0) || string.IsNullOrWhiteSpace(sFilePath))
            {
                return false;
            }
            try
            {
                if (File.Exists(sXMLPath))
                {
                    System.IO.File.Delete(sXMLPath);
                }

                XmlDocument xmlDoc = new XmlDocument();
                //创建类型声明节点  
                XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "GB2312", "");
                xmlDoc.AppendChild(node);
                XmlElement pRoot = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(pRoot);

                foreach (KeyValuePair<string, MapCorrection> entry in dic)
                {
                    //图幅号获取
                    string sTFH = entry.Key;
                    //创建根节点  
                    XmlElement pEachTFHElement = xmlDoc.CreateElement("TFH");
                    pEachTFHElement.SetAttribute("TFH", sTFH);
                    xmlDoc.SelectSingleNode("root").AppendChild(pEachTFHElement);

                    #region 子节点添加
                    ///子节点添加
                    XmlElement pXleEleMent = null;
                    ///d_LeftBottomdB
                    double d_LeftBottomdB = entry.Value.LeftBottomdB;
                    pXleEleMent = xmlDoc.CreateElement("LeftBottomdB");
                    pXleEleMent.SetAttribute("LeftBottomdB", d_LeftBottomdB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///LeftBottomdL
                    double d_LeftBottomdL = entry.Value.LeftBottomdL;
                    pXleEleMent = xmlDoc.CreateElement("LeftBottomdL");
                    pXleEleMent.SetAttribute("LeftBottomdL", d_LeftBottomdL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///LeftTopdB
                    double d_LeftTopdB = entry.Value.LeftTopdB;
                    pXleEleMent = xmlDoc.CreateElement("LeftTopdB");
                    pXleEleMent.SetAttribute("LeftTopdB", d_LeftTopdB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///LeftTopdL
                    double d_LeftTopdL = entry.Value.LeftTopdL;
                    pXleEleMent = xmlDoc.CreateElement("LeftTopdL");
                    pXleEleMent.SetAttribute("LeftTopdL", d_LeftTopdL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///maxB
                    double d_maxB = entry.Value.maxB;
                    pXleEleMent = xmlDoc.CreateElement("maxB");
                    pXleEleMent.SetAttribute("maxB", d_maxB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///maxL
                    double d_maxL = entry.Value.maxL;
                    pXleEleMent = xmlDoc.CreateElement("maxL");
                    pXleEleMent.SetAttribute("maxL", d_maxL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///minB
                    double d_minB = entry.Value.minB;
                    pXleEleMent = xmlDoc.CreateElement("minB");
                    pXleEleMent.SetAttribute("minB", d_minB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///minL
                    double d_minL = entry.Value.minL;
                    pXleEleMent = xmlDoc.CreateElement("minL");
                    pXleEleMent.SetAttribute("minL", d_minL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///RightBottomdB
                    double d_RightBottomdB = entry.Value.RightBottomdB;
                    pXleEleMent = xmlDoc.CreateElement("RightBottomdB");
                    pXleEleMent.SetAttribute("RightBottomdB", d_RightBottomdB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///RightBottomdL
                    double d_RightBottomdL = entry.Value.RightBottomdL;
                    pXleEleMent = xmlDoc.CreateElement("RightBottomdL");
                    pXleEleMent.SetAttribute("RightBottomdL", d_RightBottomdL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///RightTopdB
                    double d_RightTopdB = entry.Value.RightTopdB;
                    pXleEleMent = xmlDoc.CreateElement("RightTopdB");
                    pXleEleMent.SetAttribute("RightTopdB", d_RightTopdB.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///RightTopdL
                    double d_RightTopdL = entry.Value.RightTopdL;
                    pXleEleMent = xmlDoc.CreateElement("RightTopdL");
                    pXleEleMent.SetAttribute("RightTopdL", d_RightTopdL.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    ///Scale
                    int i_Scale = entry.Value.Scale;
                    pXleEleMent = xmlDoc.CreateElement("Scale");
                    pXleEleMent.SetAttribute("Scale", i_Scale.ToString());
                    pEachTFHElement.AppendChild(pXleEleMent);

                    #endregion 子节点添加
                }
                ///保存xml
                xmlDoc.Save(sXMLPath);

                ///将XML加密并保存到相应文件夹
                bIsOk = EncrpyXmlToSelectFolder(sFilePath, xmlDoc);

            }
            catch (Exception ex)
            {
                LogComm.WriteLog(_sLog, ex.Message, false);
            }
            return bIsOk;
        }

        /// <summary>
        /// 将加密后文件转换成一个Dictionary<string, MapCorrection>
        /// </summary>
        /// <param name="sFile">加密文件路径</param>
        /// <returns></returns>
        public static Dictionary<string, MapCorrection> DecrpyFile(string sFile)
        {
            //sFile = @"E:\项目\坐标转换工具\02Code\bin\Temp\坐标改正量文件.dlic";
            if (string.IsNullOrWhiteSpace(sFile) || !File.Exists(sFile))
            {
                DevMessageBox.ShowInformation("改正量文件不存在，或不正确，请选择正确的文件！");
                return null;
            }
            Dictionary<string, MapCorrection> dic = new Dictionary<string, MapCorrection>();
            try
            {
                //获取待解密字符串
                string sDecryptDES = string.Empty;
                using (StreamReader sr = new StreamReader(sFile))
                {
                    sDecryptDES = sr.ReadLine();
                    sr.Close();
                }
                string sDCode = DESComm.DecryptDES(sDecryptDES);
                //解密字符串放入XML
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.InnerXml = sDCode;
                if (xmldoc != null && xmldoc.HasChildNodes)
                {
                    XmlNode parNode = XmlComm.GetNode(xmldoc, "/root");
                    if (parNode != null && parNode.HasChildNodes)
                    {
                        MapCorrection pMapCorrection = null;
                        foreach (XmlNode node in parNode.ChildNodes)
                        {
                            XmlElement xe = node as XmlElement;
                            if (xe == null)
                            {
                                continue;
                            }
                            pMapCorrection = new MapCorrection();
                            string sTFH = xe.GetAttribute("TFH");//图幅号
                            pMapCorrection.TFBH = sTFH;

                            if (node.HasChildNodes)
                            {
                                XmlElement pChildXml = null;
                                foreach (XmlNode pChildNode in node.ChildNodes)
                                {
                                    pChildXml = pChildNode as XmlElement;
                                    if (pChildXml == null)
                                    {
                                        continue;
                                    }

                                    #region 子节点获取
                                    ///LeftBottomdB
                                    if (pChildXml.HasAttribute("LeftBottomdB"))
                                    {
                                        double d_LeftBottomdB = -1;
                                        string sLeftBottomdB = pChildXml.GetAttribute("LeftBottomdB");
                                        double.TryParse(sLeftBottomdB, out d_LeftBottomdB);
                                        pMapCorrection.LeftBottomdB = d_LeftBottomdB;
                                    }

                                    ///LeftBottomdL
                                    if (pChildXml.HasAttribute("LeftBottomdL"))
                                    {
                                        double d_LeftBottomdL = -1;
                                        string sLeftBottomdL = pChildXml.GetAttribute("LeftBottomdL");
                                        double.TryParse(sLeftBottomdL, out d_LeftBottomdL);
                                        pMapCorrection.LeftBottomdL = d_LeftBottomdL;
                                    }

                                    ///LeftTopdB
                                    if (pChildXml.HasAttribute("LeftTopdB"))
                                    {
                                        double d_LeftTopdB = -1;
                                        string sLeftTopdB = pChildXml.GetAttribute("LeftTopdB");
                                        double.TryParse(sLeftTopdB, out d_LeftTopdB);
                                        pMapCorrection.LeftTopdB = d_LeftTopdB;
                                    }

                                    ///LeftTopdL
                                    if (pChildXml.HasAttribute("LeftTopdL"))
                                    {
                                        double d_LeftTopdL = -1;
                                        string sLeftTopdL = pChildXml.GetAttribute("LeftTopdL");
                                        double.TryParse(sLeftTopdL, out d_LeftTopdL);
                                        pMapCorrection.LeftTopdL = d_LeftTopdL;
                                    }

                                    ///maxB
                                    if (pChildXml.HasAttribute("maxB"))
                                    {
                                        double d_maxB = -1;
                                        string smaxB = pChildXml.GetAttribute("maxB");
                                        double.TryParse(smaxB, out d_maxB);
                                        pMapCorrection.maxB = d_maxB;
                                    }
                                    ///maxL
                                    if (pChildXml.HasAttribute("maxL"))
                                    {
                                        double d_maxL = -1;
                                        string smaxL = pChildXml.GetAttribute("maxL");
                                        double.TryParse(smaxL, out d_maxL);
                                        pMapCorrection.maxL = d_maxL;
                                    }

                                    ///minB
                                    if (pChildXml.HasAttribute("minB"))
                                    {
                                        double d_minB = -1;
                                        string sminB = pChildXml.GetAttribute("minB");
                                        double.TryParse(sminB, out d_minB);
                                        pMapCorrection.minB = d_minB;
                                    }
                                    ///minL
                                    if (pChildXml.HasAttribute("minL"))
                                    {
                                        double d_minL = -1;
                                        string sminL = pChildXml.GetAttribute("minL");
                                        double.TryParse(sminL, out d_minL);
                                        pMapCorrection.minL = d_minL;
                                    }

                                    ///RightBottomdB
                                    if (pChildXml.HasAttribute("RightBottomdB"))
                                    {
                                        double d_RightBottomdB = -1;
                                        string sRightBottomdBL = pChildXml.GetAttribute("RightBottomdB");
                                        double.TryParse(sRightBottomdBL, out d_RightBottomdB);
                                        pMapCorrection.RightBottomdB = d_RightBottomdB;
                                    }

                                    ///RightBottomdL
                                    if (pChildXml.HasAttribute("RightBottomdL"))
                                    {
                                        double d_RightBottomdL = -1;
                                        string sRightBottomdL = pChildXml.GetAttribute("RightBottomdL");
                                        double.TryParse(sRightBottomdL, out d_RightBottomdL);
                                        pMapCorrection.RightBottomdL = d_RightBottomdL;
                                    }

                                    ///RightTopdB
                                    if (pChildXml.HasAttribute("RightTopdB"))
                                    {
                                        double d_RightTopdB = -1;
                                        string sRightTopdB = pChildXml.GetAttribute("RightTopdB");
                                        double.TryParse(sRightTopdB, out d_RightTopdB);
                                        pMapCorrection.RightTopdB = d_RightTopdB;
                                    }

                                    ///RightTopdL
                                    if (pChildXml.HasAttribute("RightTopdL"))
                                    {
                                        double d_RightTopdL = -1;
                                        string sRightTopdL = pChildXml.GetAttribute("RightTopdL");
                                        double.TryParse(sRightTopdL, out d_RightTopdL);
                                        pMapCorrection.RightTopdL = d_RightTopdL;
                                    }

                                    ///Scale
                                    if (pChildXml.HasAttribute("Scale"))
                                    {
                                        int i_Scale = -1;
                                        string sScale = pChildXml.GetAttribute("Scale");
                                        int.TryParse(sScale, out i_Scale);
                                        pMapCorrection.Scale = i_Scale;
                                    }
                                    #endregion 子节点获取
                                }
                                dic.Add(sTFH, pMapCorrection);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(_sLog, ex.Message, false);
            }
            return dic;
        }

        #endregion 外部调用

        #region 内部方法
        /// <summary>
        /// 根据XML形成一个加密文件到指定文件
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <param name="xmlDoc"></param>
        private static bool EncrpyXmlToSelectFolder(string sFilePath, XmlDocument xmlDoc)
        {
            bool bIsOK = false;
            try
            {
                //获取XML文档中内容
                string sInnerXml = xmlDoc.InnerXml;
                if (string.IsNullOrWhiteSpace(sInnerXml))
                {
                    //return;
                }
                //加密后字符串
                string sEncryptDES = DESComm.EncryptDES(sInnerXml);
                if (string.IsNullOrWhiteSpace(sEncryptDES))
                {
                    //return;
                }
                //将加密后的字符串写入导出文件中
                using (StreamWriter sw = new StreamWriter(sFilePath, false, Encoding.Default))
                {
                    sw.WriteLine(sEncryptDES);
                    sw.Close();
                }
                bIsOK = true;
            }
            catch (Exception ex)
            {
                LogComm.WriteLog(_sLog, ex.Message, false);
            }
            return bIsOK;
        }
        #endregion 内部方法

        #region 七参数四参数加密解密
        /// <summary>
        /// 解密七参数存储文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static SevenParams DecryptSevenParams(string file, string sModel)
        {
            SevenParams pSevenParams = null;
            try
            {
                if (!File.Exists(file))
                {
                    throw new Exception("文件不存在，请检查！");
                }

                sModel = IniFileComm.IniReadValue("转换模型", "转换模型", file);
                string sDX = IniFileComm.IniReadValue("七参数", "平移量X(米)", file);
                string sDY = IniFileComm.IniReadValue("七参数", "平移量Y(米)", file);
                string sDZ = IniFileComm.IniReadValue("七参数", "平移量Z(米)", file);
                string sAngelX = IniFileComm.IniReadValue("七参数", "X轴旋转角(秒)", file);
                string sAngelY = IniFileComm.IniReadValue("七参数", "Y轴旋转角(秒)", file);
                string sAngelZ = IniFileComm.IniReadValue("七参数", "Z轴旋转角(秒)", file);
                string sScaleK = IniFileComm.IniReadValue("七参数", "尺度因子K(ppm)", file);
                string sX0 = IniFileComm.IniReadValue("七参数", "中心点X0(米)", file);
                string sY0 = IniFileComm.IniReadValue("七参数", "中心点Y0(米)", file);
                string sZ0 = IniFileComm.IniReadValue("七参数", "中心点Z0(米)", file);

                if (sModel == "莫洛金斯基模型")
                {
                    pSevenParams = new MolodenskyParams();
                }
                else
                {
                    pSevenParams = new SevenParams();
                }

                try
                {
                    double.Parse(sDX);
                }
                catch
                {
                    sDX = DESComm.DecryptDES(sDX);
                    sDY = DESComm.DecryptDES(sDY);
                    sDZ = DESComm.DecryptDES(sDZ);
                    sAngelX = DESComm.DecryptDES(sAngelX);
                    sAngelY = DESComm.DecryptDES(sAngelY);
                    sAngelZ = DESComm.DecryptDES(sAngelZ);
                    sScaleK = DESComm.DecryptDES(sScaleK);
                    sX0 = DESComm.DecryptDES(sX0);
                    sY0 = DESComm.DecryptDES(sY0);
                    sZ0 = DESComm.DecryptDES(sZ0);
                }

                pSevenParams.DX = string.IsNullOrEmpty(sDX) ? 0.0 : double.Parse(sDX);
                pSevenParams.DY = string.IsNullOrEmpty(sDY) ? 0.0 : double.Parse(sDY);
                pSevenParams.DZ = string.IsNullOrEmpty(sDZ) ? 0.0 : double.Parse(sDZ);
                pSevenParams.AngleX = string.IsNullOrEmpty(sAngelX) ? 0.0 : double.Parse(sAngelX);
                pSevenParams.AngleY = string.IsNullOrEmpty(sAngelY) ? 0.0 : double.Parse(sAngelY);
                pSevenParams.AngleZ = string.IsNullOrEmpty(sAngelZ) ? 0.0 : double.Parse(sAngelZ);
                pSevenParams.ScaleK = string.IsNullOrEmpty(sScaleK) ? 0.0 : double.Parse(sScaleK);
                if (pSevenParams is MolodenskyParams)
                {
                    MolodenskyParams obj = pSevenParams as MolodenskyParams;
                    obj.X0 = string.IsNullOrEmpty(sX0) ? 0.0 : double.Parse(sX0);
                    obj.Y0 = string.IsNullOrEmpty(sY0) ? 0.0 : double.Parse(sY0);
                    obj.Z0 = string.IsNullOrEmpty(sZ0) ? 0.0 : double.Parse(sZ0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pSevenParams;
        }

        /// <summary>
        /// 解密四参数存储文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FourParams2D DecryptFourParams(string file)
        {
            FourParams2D pFourParams = null;
            try
            {
                if (!File.Exists(file))
                {
                    throw new Exception("文件不存在，请检查！");
                }

                pFourParams = new FourParams2D();
                string sDX = IniFileComm.IniReadValue("四参数", "平移量X(米)", file);
                string sDY = IniFileComm.IniReadValue("四参数", "平移量Y(米)", file);
                string sAngle = IniFileComm.IniReadValue("四参数", "旋转角(秒)", file);
                string sScaleK = IniFileComm.IniReadValue("四参数", "尺度因子K", file);
                try
                {
                    double.Parse(sDX);
                }
                catch
                {
                    sDX = DESComm.DecryptDES(sDX);
                    sDY = DESComm.DecryptDES(sDY);
                    sAngle = DESComm.DecryptDES(sAngle);
                    sScaleK = DESComm.DecryptDES(sScaleK);
                }
                pFourParams.DX = string.IsNullOrEmpty(sDX) ? 0.0 : double.Parse(sDX);
                pFourParams.DY = string.IsNullOrEmpty(sDY) ? 0.0 : double.Parse(sDY);
                pFourParams.Angle = string.IsNullOrEmpty(sAngle) ? 0.0 : double.Parse(sAngle);
                pFourParams.ScaleK = string.IsNullOrEmpty(sScaleK) ? 0.0 : double.Parse(sScaleK);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return pFourParams;
        }

        /// <summary>
        /// 将七参数加密写入文件中
        /// </summary>
        /// <param name="pSevenParams"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool EncryptSevenParams(SevenParams pSevenParams, string sModel, string file, bool bDES)
        {
            bool flag = false;

            try
            {
                string sDX = ((decimal)(pSevenParams.DX)).ToString();
                string sDY = ((decimal)(pSevenParams.DY)).ToString();
                string sDZ = ((decimal)(pSevenParams.DZ)).ToString();
                string sAngleX = ((decimal)(pSevenParams.AngleX)).ToString();
                string sAngleY = ((decimal)(pSevenParams.AngleY)).ToString();
                string sAngleZ = ((decimal)(pSevenParams.AngleZ)).ToString();
                string sScaleK = ((decimal)(pSevenParams.ScaleK)).ToString();
                if (bDES)
                {
                    sDX = DESComm.EncryptDES(sDX);
                    sDY = DESComm.EncryptDES(sDY);
                    sDZ = DESComm.EncryptDES(sDZ);
                    sAngleX = DESComm.EncryptDES(sAngleX);
                    sAngleY = DESComm.EncryptDES(sAngleY);
                    sAngleZ = DESComm.EncryptDES(sAngleZ);
                    sScaleK = DESComm.EncryptDES(sScaleK);
                }

                IniFileComm.IniWriteValue("转换模型", "转换模型", sModel, file);
                IniFileComm.IniWriteValue("七参数", "平移量X(米)", sDX, file);
                IniFileComm.IniWriteValue("七参数", "平移量Y(米)", sDY, file);
                IniFileComm.IniWriteValue("七参数", "平移量Z(米)", sDZ, file);
                IniFileComm.IniWriteValue("七参数", "X轴旋转角(秒)", sAngleX, file);
                IniFileComm.IniWriteValue("七参数", "Y轴旋转角(秒)", sAngleY, file);
                IniFileComm.IniWriteValue("七参数", "Z轴旋转角(秒)", sAngleZ, file);
                IniFileComm.IniWriteValue("七参数", "尺度因子K(ppm)", sScaleK, file);
                if (pSevenParams is MolodenskyParams)
                {
                    string sX0 = ((decimal)(pSevenParams as MolodenskyParams).X0).ToString();
                    string sY0 = ((decimal)(pSevenParams as MolodenskyParams).Y0).ToString();
                    string sZ0 = ((decimal)(pSevenParams as MolodenskyParams).Z0).ToString();
                    if (bDES)
                    {
                        sX0 = DESComm.EncryptDES(sX0);
                        sY0 = DESComm.EncryptDES(sY0);
                        sZ0 = DESComm.EncryptDES(sZ0);
                    }

                    IniFileComm.IniWriteValue("七参数", "中心点X0(米)", sX0, file);
                    IniFileComm.IniWriteValue("七参数", "中心点Y0(米)", sY0, file);
                    IniFileComm.IniWriteValue("七参数", "中心点Z0(米)", sZ0, file);
                }

                flag = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }

        /// <summary>
        /// 将四参数加密写入文件中
        /// </summary>
        /// <param name="pFourParams"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool EncryptFourParams(FourParams2D pFourParams, string file, bool bDES)
        {
            bool flag = false;
            try
            {
                string sDX = ((decimal)(pFourParams.DX)).ToString();
                string sDY = ((decimal)(pFourParams.DY)).ToString();
                string sAngle = ((decimal)(pFourParams.Angle)).ToString();
                string sScaleK = ((decimal)(pFourParams.ScaleK)).ToString();
                if (bDES)
                {
                    sDX = DESComm.EncryptDES(sDX);
                    sDY = DESComm.EncryptDES(sDY);
                    sAngle = DESComm.EncryptDES(sAngle);
                    sScaleK = DESComm.EncryptDES(sScaleK);
                }

                IniFileComm.IniWriteValue("转换模型", "转换模型", "二维四参数模型", file);
                IniFileComm.IniWriteValue("四参数", "平移量X(米)", sDX, file);
                IniFileComm.IniWriteValue("四参数", "平移量Y(米)", sDY, file);
                IniFileComm.IniWriteValue("四参数", "旋转角(秒)", sAngle, file);
                IniFileComm.IniWriteValue("四参数", "尺度因子K", sScaleK, file);

                flag = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }

        #endregion
    }
}
