
using ICSharpCode.SharpZipLib.Zip;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace AutoUpdate
{
    public class publicAutoUpdate
    {
        /// <summary>
        /// url+randomstring
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string getRandomUrl(string url)
        {
            Random random = new Random();
            string randomStr = random.Next(10).ToString() + random.Next(11).ToString() + random.Next(12).ToString();
            string randmoUrl = url.Trim() + "?" + randomStr;
            return randmoUrl;
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
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "AntennaChat.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
                else
                {

                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "AntennaChat.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
            }
            catch (Exception ex)
            {
                isStart = false;
            }
            return isStart;
        }
        /// <summary>
        /// 启动EXE
        /// </summary>
        /// <returns></returns>
        public static bool startApplicationExe()
        {
            bool isStart = false;
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {

                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\AntennaChat.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
                else
                {

                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\AntennaChat.exe";
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    isStart = true;
                }
            }
            catch (Exception ex)
            {
                isStart = false;
            }
            return isStart;
        }
        /// <summary>
        /// 杀死进程
        /// </summary>
        /// <returns></returns>
        public static bool killProcess()
        {
            bool kill = false;
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ("AntennaChat" == myProcess.ProcessName)
                    {
                        myProcess.Kill();
                    }
                }
                kill = true;
            }
            catch (Exception ee)
            {
                kill = false;
            }
            return kill;
        }
        /// <summary>
        /// 检查是否存在AntennaChat进程
        /// </summary>
        /// <returns></returns>
        public static bool IsHaveProcess()
        {
            bool isHave = false;
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ("AntennaChat" == myProcess.ProcessName)

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
                if (!TargetDirectory.EndsWith("\\")) { TargetDirectory = TargetDirectory + "\\"; }

                using (ZipInputStream zipfiles = new ZipInputStream(System.IO.File.OpenRead(ZipFile)))
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
                            if ((System.IO.File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) || (!System.IO.File.Exists(TargetDirectory + directoryName + fileName)))
                            {
                                using (FileStream streamWriter = System.IO.File.Create(TargetDirectory + directoryName + fileName))
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
                System.Windows.Forms.MessageBox.Show(ex.ToString() + ex.StackTrace);
                isUnZip = false; ;
            }
            return isUnZip;
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
                value = (node.SelectSingleNode(nodeName)).InnerText;
            }
            return value;
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
                modify = false;
            }
            return modify;
        }
        /// <summary>
        /// 获取文件MD5
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getFileMd5Value(string path)
        {
            FileStream file = null;
            string md5Value = "";
            try
            {
                file = new FileStream(path, FileMode.Open);
                MD5 fileMd5 = new MD5CryptoServiceProvider();
                byte[] getMd5 = fileMd5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                foreach (var list in getMd5)
                {
                    sb.Append(list.ToString("x2"));
                }
                md5Value = sb.ToString();
            }
            catch (Exception ex)
            {
                file.Close();
            }
            return md5Value;
        }
        static string IcoPath = System.Environment.CurrentDirectory + "\\Images\\七讯.ico";
        static string AppPath = System.Environment.CurrentDirectory + "\\AntennaChat.exe";
        static string Uninstall = System.Environment.CurrentDirectory + "\\unins000.exe";
        /// <summary>
        /// 程序重新命名
        /// </summary>
        public static void AppReName()
        {
            CreateShortcutOnDesktop("七讯", AppPath, "", IcoPath);
        }
        /// <summary>
        /// 创建快捷方式方法
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="shortcutName"></param>
        /// <param name="targetPath"></param>
        /// <param name="description"></param>
        /// <param name="iconLocation"></param>
        public static void CreateShortcut(string directory, string shortcutName, string targetPath,
             string description = null, string iconLocation = null)
        {
            try
            {
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
                shortcut.TargetPath = targetPath;//指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);//设置起始位置
                shortcut.WindowStyle = 1;//设置运行方式，默认为常规窗口
                shortcut.Description = description;//设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;//设置图标路径
                shortcut.Save();//保存快捷方式
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 创建快捷方式到桌面
        /// </summary>
        /// <param name="shortcutName">快捷方式名称</param>
        /// <param name="targetPath">应用程序目标路径</param>
        /// <param name="description">描述  一般为空</param>
        /// <param name="iconLocation">icon图标路径 一般为空</param>
        public static void CreateShortcutOnDesktop(string shortcutName, string targetPath,
             string description = null, string iconLocation = null)
        {
            try
            {
                //获取公共桌面文件夹路径
                string publicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                string[] publicitems = Directory.GetFiles(publicDesktop, "*.lnk", SearchOption.AllDirectories);
                //遍历集合中的每个文件，如果名称包括“触角”则将其快捷方式删除。
                foreach (string item in publicitems)
                {
                    if (item.Contains("触角") || item.Contains("AntennaChat"))
                    {
                        System.IO.File.Delete(item);
                    }
                }
                //创建桌面快捷方式
                CreateShortcut(publicDesktop, shortcutName, targetPath, description, iconLocation);
                //刷新桌面
                DeskRef();
                #region 删除旧启动目录 创建新启动目录
                //是否存在触角目录
                string StartDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                //旧程序启动路径
                string OldStartPath = StartDirectory + "\\Programs\\触角";
                //新程序启动路径
                string NewStartPath = StartDirectory + "\\Programs\\七讯";
                bool isExist = Directory.Exists(OldStartPath);
                if (isExist)
                {
                    //删除触角目录
                    Directory.Delete(OldStartPath, true);
                    //创建目录
                    Directory.CreateDirectory(NewStartPath);
                    //创建程序快捷目录
                    if (Directory.Exists(NewStartPath))
                    {
                        //创建程序快捷方式
                        CreateShortcut(NewStartPath, "七讯", AppPath, "", IcoPath);
                        if (Directory.Exists(Uninstall))
                        {
                            //创建卸载快捷方式
                            CreateShortcut(NewStartPath, "卸载 七讯", Uninstall, "", IcoPath);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {

            }
        }
        #region 桌面刷新
        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(HChangeNotifyEventID wEventId, HChangeNotifyFlags uFlags, IntPtr dwItem1, IntPtr dwItem2);
        public static void DeskRef()
        {
            SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
        #region public enum HChangeNotifyFlags
        [Flags]
        public enum HChangeNotifyFlags
        {
            SHCNF_DWORD = 0x0003,
            SHCNF_IDLIST = 0x0000,
            SHCNF_PATHA = 0x0001,
            SHCNF_PATHW = 0x0005,
            SHCNF_PRINTERA = 0x0002,
            SHCNF_PRINTERW = 0x0006,
            SHCNF_FLUSH = 0x1000,
            SHCNF_FLUSHNOWAIT = 0x2000
        }
        #endregion//enum HChangeNotifyFlags
        #region enum HChangeNotifyEventID
        [Flags]
        public enum HChangeNotifyEventID
        {
            SHCNE_ALLEVENTS = 0x7FFFFFFF,
            SHCNE_ASSOCCHANGED = 0x08000000,
            SHCNE_ATTRIBUTES = 0x00000800,
            SHCNE_CREATE = 0x00000002,
            SHCNE_DELETE = 0x00000004,
            SHCNE_DRIVEADD = 0x00000100,
            SHCNE_DRIVEADDGUI = 0x00010000,
            SHCNE_DRIVEREMOVED = 0x00000080,
            SHCNE_EXTENDED_EVENT = 0x04000000,
            SHCNE_FREESPACE = 0x00040000,
            SHCNE_MEDIAINSERTED = 0x00000020,
            SHCNE_MEDIAREMOVED = 0x00000040,
            SHCNE_MKDIR = 0x00000008,
            SHCNE_NETSHARE = 0x00000200,
            SHCNE_NETUNSHARE = 0x00000400,
            SHCNE_RENAMEFOLDER = 0x00020000,
            SHCNE_RENAMEITEM = 0x00000001,
            SHCNE_RMDIR = 0x00000010,
            SHCNE_SERVERDISCONNECT = 0x00004000,
            SHCNE_UPDATEDIR = 0x00001000,
            SHCNE_UPDATEIMAGE = 0x00008000,
        }
        #endregion
        #endregion
        #region 修改程序相关信息
        public static void SetAppInfo()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{F34E4FC6-8B37-4E45-A1B0-ADE256E152A6}_is1", true);
                if (software != null)
                {
                    string version = software.GetValue("DisplayVersion").ToString();
                    string currentVersion = publicAutoUpdate.xmlFind("version", System.Environment.CurrentDirectory + "\\version.xml");
                    if (version.ToLower() != currentVersion.ToLower())
                    {
                        //修改版本号
                        software.SetValue("DisplayVersion", currentVersion + "");
                        //修改ico
                        software.SetValue("Inno Setup: Icon Group", "七讯");
                        //修改DisplayName
                        software.SetValue("DisplayName", "七讯 " + currentVersion + "");
                        //修改公司信息
                        software.SetValue("Publisher", "深圳柒壹思诺科技有限公司");
                        //修改卸载地址（兼容之前旧版本）
                        software.SetValue("QuietUninstallString", ("\"") + Uninstall + ("\"") + " /SILENT");
                        software.SetValue("UninstallString", "\"" + Uninstall + "\"");
                        //添加ico新键值
                        software.SetValue("DisplayIcon", IcoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}