using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using SDK.AntSdk;
using System.Configuration;

namespace Antenna.Framework
{
    public class publicMethod
    {
        ///// <summary>
        ///// 联系人头像保存路径
        ///// </summary>
        //public static  string UserHeadImageFilePath = userHeadImageFilePath();
        ///// <summary>
        ///// 下载文件的保存路径
        ///// </summary>
        //public static  string DownloadFilePath = downloadFilePath();

        public static string UserHeadImageFilePath()
        {
            return publicMethod.localDataPath() + AntSdkService.AntSdkCurrentUserInfo.companyCode + "\\"
                   + AntSdkService.AntSdkCurrentUserInfo.userId + "\\UserHeadImage\\";
        }
        public static string xmlPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "BurnStates.xml";
        }

        public static string DownloadFilePath()
        {
            return publicMethod.localDataPath() + AntSdkService.AntSdkCurrentUserInfo.companyCode + "\\" +
                               AntSdkService.AntSdkCurrentUserInfo.userId;
        }
        /// <summary>
        /// 验证文件下载地址
        /// </summary>
        /// <returns></returns>
        public static bool IsUrlRegex(string url)
        {
            string Pattern = @"^(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex r = new Regex(Pattern);
            Match m = r.Match(url);
            return m.Success;
        }

        /// <summary>
        /// 查找Xml节点的值
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static string xmlFind(string nodeName, string path)
        {
            string value = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlElt = xmlDoc.DocumentElement;
            XmlNode node = xmlElt.SelectSingleNode("//updateAntenna");
            if (node != null)
            {
                value = (node.SelectSingleNode(nodeName))?.InnerText;
            }
            return value;
        }

        /// <summary>
        /// 添加xml节点
        /// </summary>
        /// <param name="dict">键</param>
        /// <param name="value">值</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool xmlAdd(string dict, string value, string path)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                XmlNode root = xml.SelectSingleNode("//updateAntenna");
                XmlElement xmlElement = xml.CreateElement(dict);
                xmlElement.InnerText = value;
                root.AppendChild(xmlElement);
                xml.Save(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 修改Xml节点数据
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="newValue">新值</param>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static bool xmlModify(string nodeName, string newValue, string path)
        {
            bool modify = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xmlElt = xmlDoc.DocumentElement;
                XmlNode node = xmlElt.SelectSingleNode("//updateAntenna");
                if (node != null)
                {
                    (node.SelectSingleNode(nodeName)).InnerText = newValue;
                    xmlDoc.Save(path);
                    modify = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[publicMethod_xmlModify]:" + ex.Message + ex.Source);
                modify = false;
            }
            return modify;
        }

        /// <summary>
        /// 判断是否阅后即焚模式
        /// </summary>
        /// <param name="sessionid"></param>
        /// <returns></returns>
        public static bool isBurnMode(string sessionid)
        {
            bool b = false;
            string value = publicMethod.xmlFind("burn" + sessionid, xmlPath());
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Trim() == "1")
                {
                    b = true;
                }
                else
                {
                    b = false;
                }
            }
            return b;
        }

        /// <summary>
        /// 启动程序
        /// </summary>
        /// <returns></returns>
        public static bool startApplication()
        {
            bool isStart = false;
            try
            {
                System.Security.Principal.WindowsIdentity identity =
                    System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal =
                    new System.Security.Principal.WindowsPrincipal(identity);
                string path= AppDomain.CurrentDomain.BaseDirectory;
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {

                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = path+ "AutoUpdate.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
                else
                {

                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "AutoUpdate.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[publicMethod_startApplication]:" + ex.Message + ex.Source);
                isStart = false;
            }
            return isStart;
        }

        /// <summary>
        /// 创建xml
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool createXml(string path)
        {
            bool createIsSucess = false;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);

            XmlNode root = xmlDoc.CreateElement("updateAntenna");
            xmlDoc.AppendChild(root);

            CreateNode(xmlDoc, root, "title", "");
            CreateNode(xmlDoc, root, "version", "");
            CreateNode(xmlDoc, root, "describe", "");
            CreateNode(xmlDoc, root, "url", "");
            CreateNode(xmlDoc, root, "updateType", "");
            CreateNode(xmlDoc, root, "fileMd5Value", "");
            CreateNode(xmlDoc, root, "isDownFileSucess", "");

            try
            {
                xmlDoc.Save(path);
                createIsSucess = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[publicMethod_createXml]:" + ex.Message + ex.Source);
                createIsSucess = false;
            }
            return createIsSucess;
        }

        /// <summary>
        /// 创建xmlNode
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="parentNode"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        /// <summary>
        /// ZIP:解压一个zip文件
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="Password">解压密码</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static bool UnZip(string ZipFile, string TargetDirectory, bool OverWrite = true)
        {
            bool isUnZip = false;
            try
            {
                if (!System.IO.Directory.Exists(TargetDirectory))
                {

                }
                if (!TargetDirectory.EndsWith("\\"))
                {
                    TargetDirectory = TargetDirectory + "\\";
                }

                using (ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(ZipFile)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = zipfiles.GetNextEntry()) != null)
                    {
                        string directoryName = "";
                        string pathToZip = "";
                        pathToZip = theEntry.Name;

                        if (pathToZip != "")
                            directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                        string fileName = Path.GetFileName(pathToZip);

                        Directory.CreateDirectory(TargetDirectory + directoryName);

                        if (fileName != "")
                        {
                            if ((File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) ||
                                (!File.Exists(TargetDirectory + directoryName + fileName)))
                            {
                                using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName)
                                    )
                                {
                                    int size = 2048;
                                    byte[] data = new byte[size];
                                    while (true)
                                    {
                                        size = zipfiles.Read(data, 0, data.Length);

                                        if (size > 0)
                                            streamWriter.Write(data, 0, size);
                                        else
                                            break;
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                    }

                    zipfiles.Close();
                }
                isUnZip = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[UnZip]:" + ex.Message + ex.Source + ex.StackTrace);
                isUnZip = false;
                ;
            }
            return isUnZip;
        }

        /// <summary>
        /// 杀死更新程序
        /// </summary>
        /// <returns></returns>
        public static bool killUpdataProcess()
        {
            bool kill = false;
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ("AutoUpdate" == myProcess.ProcessName)
                    {
                        myProcess.Kill();
                        LogHelper.WriteDebug("kill:true");
                    }
                }
                kill = true;
            }
            catch (Exception ee)
            {
                LogHelper.WriteError("[killUpdataProcess]:" + ee.StackTrace + ee.Message + ee.TargetSite);
                kill = false;
            }
            return kill;
        }
        /// <summary>
        /// 查看是否有更新程序进程
        /// </summary>
        /// <returns></returns>
        public static bool IsHaveUpdataProcess()
        {
            bool isHave = false;
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ("AutoUpdate" == myProcess.ProcessName)
                        isHave = true;
                }
            }
            catch (Exception ee)
            {
                isHave = false;
            }
            return isHave;
        }
        /// <summary>
        /// 检测是否开启预发布
        /// </summary>
        /// <returns></returns>
        public static bool IsPreRelease()
        {
            bool result = false;
            string config = ConfigurationManager.AppSettings["SwitchDataBase"]+"".ToString();
            if (config == "" || config.ToLower() == "false")
            {
                result = false;
            }
            else if (config.ToLower() == "true")
            {
                result = true;
            }
            return result;
        }
        /// <summary>
        /// 聊天记录保存文件
        /// </summary>
        /// <returns></returns>
        public static string localDataPath()
        {
            string path = "";
            if(IsPreRelease())
            {
                //程序运行目录
                path = AppDomain.CurrentDomain.BaseDirectory + "AntennaChat\\localData\\";
            }
            else
            {
                //公共目录
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AntennaChat\\localData\\";
            }
            return path;
        }
        /// <summary>
        /// 拷贝oldlab的文件到newlab下面
        /// </summary>
        /// <param name="sourcePath">lab文件所在目录(@"~\labs\oldlab")</param>
        /// <param name="savePath">保存的目标目录(@"~\labs\newlab")</param>
        /// <returns>返回:true-拷贝成功;false:拷贝失败</returns>
        public static bool CopyOldLabFilesToNewLab(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            #region //拷贝labs文件夹到savePath下
            try
            {
                string[] labDirs = Directory.GetDirectories(sourcePath);//目录
                string[] labFiles = Directory.GetFiles(sourcePath);//文件
                if (labFiles.Length > 0)
                {
                    for (int i = 0; i < labFiles.Length; i++)
                    {
                        if (Path.GetFileName(labFiles[i]) != ".lab")//排除.lab文件
                        {
                            File.Copy(sourcePath + "\\" + Path.GetFileName(labFiles[i]), savePath + "\\" + Path.GetFileName(labFiles[i]), true);
                        }
                    }
                }
                if (labDirs.Length > 0)
                {
                    for (int j = 0; j < labDirs.Length; j++)
                    {
                        Directory.GetDirectories(sourcePath + "\\" + Path.GetFileName(labDirs[j]));

                        //递归调用
                        CopyOldLabFilesToNewLab(sourcePath + "\\" + Path.GetFileName(labDirs[j]), savePath + "\\" + Path.GetFileName(labDirs[j]));
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            #endregion
            return true;
        }
    }
}
