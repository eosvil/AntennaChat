using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Framework
{
    /// <summary>
    /// 桌面快捷方式操作
    /// </summary>
    public class DeskTopShortCut
    {
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
                LogHelper.WriteError("[CreateShortcut]:" + ex.Message + ex.StackTrace + ex.Source);
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
                bool isNeedCreatDesk = false;
                //获取公共桌面文件夹路径
                string publicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                string[] publicitems = Directory.GetFiles(publicDesktop, "*.lnk", SearchOption.AllDirectories);
                //遍历集合中的每个文件，如果名称包括“触角”则将其快捷方式删除。
                foreach (string item in publicitems)
                {
                    if (item.Contains("触角") || item.Contains("AntennaChat"))
                    {
                        isNeedCreatDesk = true;
                        System.IO.File.Delete(item);
                    }
                }
                string[] itemsPublicDestop = publicitems.Where(m => m.ToString().Contains("七讯")).ToArray();
                if (!itemsPublicDestop.Any()&& isNeedCreatDesk)
                {
                    CreateShortcut(publicDesktop, shortcutName, targetPath, description, iconLocation);
                    //刷新桌面
                    DeskRef();
                }
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
                LogHelper.WriteError("[CreateShortcutOnDesktop]:" + ex.Message + ex.StackTrace + ex.Source);
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
                    string currentVersion = publicMethod.xmlFind("version", System.Environment.CurrentDirectory + "\\version.xml");
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
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[SetAppInfo]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        #endregion
    }
    #endregion
}