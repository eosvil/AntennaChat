using Antenna.Framework;
using System;
using System.Configuration;
using System.Windows;
using Antenna.Model;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using AntennaChat.Views.Update;
using AntennaChat.ViewModel.Update;

namespace AntennaChat
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            bool catchGlobalExceptions = ConfigurationManager.AppSettings["CatchGlobalExceptions"] == null ? true : bool.Parse(ConfigurationManager.AppSettings["CatchGlobalExceptions"]);
            if (catchGlobalExceptions)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            this.Startup += App_Startup;
        }

        public static void DeleteUpdateFile()
        {
            #region 更新自动更新程序
            try
            {
                bool isHave = publicMethod.IsHaveUpdataProcess();
                Task<bool> isTask;
                if (isHave == true)
                {
                    isTask = Task.Factory.StartNew(() => publicMethod.killUpdataProcess());
                }
                else
                {
                    isTask = Task.Factory.StartNew(() => true);
                }
                if (isTask.Result == true)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "update.zip"))
                    {
                        //string fileMd5 = publicMethod.xmlFind("upDataVersion",AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                        //if (fileMd5.Trim() == "")
                        //{
                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        if (File.Exists(path + "//update.zip"))
                        {
                            bool b = publicMethod.UnZip(path + "//update.zip", path, true);
                            if (b == true)
                            {
                                //publicMethod.xmlModify("upDataVersion", "1.0",AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                                File.Delete(path + "//update.zip");
                                LogHelper.WriteDebug("自动更新程序更新成功!");
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[App_Startup]:" + ex.Source + ex.Message + ex.StackTrace);
            }
            #endregion
        }
        string wherefrom = "";

        private void App_Startup(object sender, StartupEventArgs e)
        {
            #region 触角更名
            //判断桌面是否有七讯快捷方式  有就不更新 没有就更新
            DeskTopShortCut.AppReName();
            #endregion

            #region 删除日志
            RemoveLog();
            #endregion
            //获取配置文件信息
            var temperrorCode = 0;
            var temperrorMsg = string.Empty;
            if (!GetConfigInfo(ref temperrorMsg))
            {
                return;
            }
            string[] str = e.Args;
            if (str.Length > 0)
            {
                wherefrom = "source";
            }

            //判断是否有更新文件 有则删除 没有就算了
            ThreadPool.QueueUserWorkItem(m => DeleteUpdateFile());

            #region 临时删除数据库

            try
            {
                //是否删除数据库
                string isDelete = publicMethod.xmlFind("isDeleteDataBase",
                    AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                if (isDelete == "0")
                {
                    if (Directory.Exists(publicMethod.localDataPath()))
                    {
                        Directory.Delete(publicMethod.localDataPath(), true);
                        publicMethod.xmlModify("isDeleteDataBase", "1",
                            AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteDebug("[初始化数据库失败：]" + ex.Message);
            }

            #endregion

            try
            {
                #region 启动SDK并检查版本更新
                //更新检测
                var outData = SDK.AntSdk.AntSdkService.AntSdkCheckUpgrade(ref temperrorCode, ref temperrorMsg);
                bool result = outData != null;
                if (result == true)
                {
                    //本地版本和服务器版本比较
                    string localVersion = publicMethod.xmlFind("version",
                        AppDomain.CurrentDomain.BaseDirectory + "version.xml");
                    if (Convert.ToInt32(localVersion.Trim().Replace(".", "")) <
                        Convert.ToInt32(outData.version.Trim().Replace(".", "")))
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile"))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile");
                        }
                        string path = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml";
                        if (!File.Exists(path))
                        {
                            if (publicMethod.createXml(path))
                            {
                                publicMethod.xmlModify("title", "UpdateVersion",
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                                publicMethod.xmlModify("version", outData.version,
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                                publicMethod.xmlModify("describe", outData.describe,
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                                publicMethod.xmlModify("url", outData.url,
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                                publicMethod.xmlModify("updateType", outData.updateType,
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                                publicMethod.xmlModify("fileMd5Value", (outData.md5Str + "") == "" ? "" : outData.md5Str,
                                    AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            }
                        }
                        else
                        {
                            publicMethod.xmlModify("title", "UpdateVersion",
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            publicMethod.xmlModify("version", outData.version,
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            publicMethod.xmlModify("describe", outData.describe,
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            publicMethod.xmlModify("url", outData.url,
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            publicMethod.xmlModify("updateType", outData.updateType,
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                            publicMethod.xmlModify("fileMd5Value", (outData.md5Str + "") == "" ? "" : outData.md5Str,
                                AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                        }
                        switch (outData.updateType)
                        {
                            //软更新
                            case "1":
                                UpdateWindowView updateWin = new UpdateWindowView();
                                UpdateWindowViewModel updateWinModel = new UpdateWindowViewModel(outData.describe, false);
                                //updateWin.ShowInTaskbar = false;
                                //updateWin.Owner = Antenna.Framework.Win32.GetTopWindow();
                                updateWin.DataContext = updateWinModel;
                                updateWin.ShowDialog();
                                break;
                            //硬更新
                            case "2":
                                UpdateWindowView update = new UpdateWindowView();
                                UpdateWindowViewModel updateModel = new UpdateWindowViewModel(outData.describe, true);
                                //update.ShowInTaskbar = false;
                                update.DataContext = updateModel;
                                //update.Owner = Antenna.Framework.Win32.GetTopWindow();
                                update.ShowDialog();
                                break;
                        }
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("最新版");
                    }
                }

                #endregion
            }
            catch (Exception exc)
            {

            }

        }
        /// <summary>
        /// 删除七天前的日志
        /// </summary>
        private void RemoveLog()
        {
            #region 删除日志
            if (Directory.Exists(System.Environment.CurrentDirectory + "\\current\\Logs\\Warn\\"))
            {
                var files = Directory.GetFiles(System.Environment.CurrentDirectory + "\\current\\Logs\\Warn\\");

                foreach (var file in files)
                {
                    try
                    {
                        if (!File.Exists(file)) continue;
                        var info = new FileInfo(file);
                        var index = info.Name.LastIndexOf(".", StringComparison.Ordinal);
                        var filename = info.Name.Substring(0, index);
                        var fileDate = Convert.ToDateTime(filename);
                        var endDate = DateTime.Now.AddDays(-7);
                        if (fileDate.Date < endDate.Date)
                            File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (Directory.Exists(System.Environment.CurrentDirectory + "\\current\\Logs\\Error\\"))
            {
                var files = Directory.GetFiles(System.Environment.CurrentDirectory + "\\current\\Logs\\Error\\");

                foreach (var file in files)
                {
                    try
                    {
                        if (!File.Exists(file)) continue;
                        var info = new FileInfo(file);
                        var index = info.Name.LastIndexOf(".", StringComparison.Ordinal);
                        var filename = info.Name.Substring(0, index);
                        var fileDate = Convert.ToDateTime(filename);
                        var endDate = DateTime.Now.AddDays(-7);
                        if (fileDate.Date < endDate.Date)
                            File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (Directory.Exists(System.Environment.CurrentDirectory + "\\Logs\\Warn\\"))
            {
                var tempFiles = Directory.GetFiles(System.Environment.CurrentDirectory + "\\Logs\\Warn\\");

                foreach (var file in tempFiles)
                {
                    try
                    {
                        if (!File.Exists(file)) continue;
                        var info = new FileInfo(file);
                        var index = info.Name.LastIndexOf(".", StringComparison.Ordinal);
                        var filename = info.Name.Substring(0, index);
                        var fileDate = Convert.ToDateTime(filename);
                        var endDate = DateTime.Now.AddDays(-7); ;
                        if (fileDate.Date < endDate.Date)
                            File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (Directory.Exists(System.Environment.CurrentDirectory + "\\current\\SDK_Logs\\Error\\"))
            {
                var tempFiles = Directory.GetFiles(System.Environment.CurrentDirectory + "\\current\\SDK_Logs\\Error\\");

                foreach (var file in tempFiles)
                {
                    try
                    {
                        if (!File.Exists(file)) continue;
                        var info = new FileInfo(file);
                        var index = info.Name.LastIndexOf(".", StringComparison.Ordinal);
                        var filename = info.Name.Substring(0, index);
                        var fileDate = Convert.ToDateTime(filename);
                        var endDate = DateTime.Now.AddDays(-7); ;
                        if (fileDate.Date < endDate.Date)
                            File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (Directory.Exists(System.Environment.CurrentDirectory + "\\current\\SDK_Logs\\Info\\"))
            {
                var tempFiles = Directory.GetFiles(System.Environment.CurrentDirectory + "\\current\\SDK_Logs\\Info\\");

                foreach (var file in tempFiles)
                {
                    try
                    {
                        if (!File.Exists(file)) continue;
                        var info = new FileInfo(file);
                        var index = info.Name.LastIndexOf(".", StringComparison.Ordinal);
                        var filename = info.Name.Substring(0, index);
                        var fileDate = Convert.ToDateTime(filename);
                        var endDate = DateTime.Now.AddDays(-7); ;
                        if (fileDate.Date < endDate.Date)
                            File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            #endregion
        }
        /// <summary>
        /// 获取配置文件信息
        /// </summary>
        private bool GetConfigInfo(ref string errorMsg)
        {
            System.Collections.Generic.List<string> allkeyList = null;
            if (ConfigurationManager.AppSettings.AllKeys.Length > 0)
            {
                allkeyList = new System.Collections.Generic.List<string>(ConfigurationManager.AppSettings.AllKeys);
            }
            //判断配置文件信息[先检查Http前缀（触角服务及客户系统接入）]
            if (allkeyList == null || !allkeyList.Contains("HttpPrdfixService") || !allkeyList.Contains("HttpPrdfixCustoms") ||
                string.IsNullOrEmpty(ConfigurationManager.AppSettings["HttpPrdfixService"]) ||
                string.IsNullOrEmpty(ConfigurationManager.AppSettings["HttpPrdfixCustoms"]))
            {
                errorMsg += "配置文件错误！";
                return false;
            }
            var antsdkconfig = new SDK.AntSdk.AntModels.AntSdkConfig
            {
                AntServiceHttpPrdfix = ConfigurationManager.AppSettings["HttpPrdfixService"],
                CustomersHttpPrdfix = ConfigurationManager.AppSettings["HttpPrdfixCustoms"],
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            //日志配置
            var logconfigInfo = new LogConfigInfo();
            var logenabled = false;
            bool.TryParse(ConfigurationManager.AppSettings["DebugLogEnable"], out logenabled);
            if (logenabled)
                antsdkconfig.AntSdkLogMode |= SDK.AntSdk.AntSdkLogLevel.DebugLogEnable;
            logconfigInfo.DebugLogEnable = logenabled;
            logenabled = false;
            bool.TryParse(ConfigurationManager.AppSettings["InfoLogEnable"], out logenabled);
            if (logenabled)
                antsdkconfig.AntSdkLogMode |= SDK.AntSdk.AntSdkLogLevel.InfoLogEnable;
            logconfigInfo.InfoLogEnable = logenabled;
            logenabled = false;
            bool.TryParse(ConfigurationManager.AppSettings["WarnLogEnable"], out logenabled);
            if (logenabled)
                antsdkconfig.AntSdkLogMode |= SDK.AntSdk.AntSdkLogLevel.WarnLogEnable;
            logconfigInfo.WarnLogEnable = logenabled;
            logenabled = false;
            bool.TryParse(ConfigurationManager.AppSettings["ErrorLogEnable"], out logenabled);
            if (logenabled)
                antsdkconfig.AntSdkLogMode |= SDK.AntSdk.AntSdkLogLevel.ErrorLogEnable;
            logconfigInfo.ErrorLogEnable = logenabled;
            logenabled = false;
            bool.TryParse(ConfigurationManager.AppSettings["FatalLogEnable"], out logenabled);
            if (logenabled)
                antsdkconfig.AntSdkLogMode |= SDK.AntSdk.AntSdkLogLevel.FatalLogEnable;
            logconfigInfo.FatalLogEnable = logenabled;
            //启动触角SDK
            var temperrorMsg = string.Empty;
            if (!SDK.AntSdk.AntSdkService.StartAntSdk(antsdkconfig, ref temperrorMsg))
            {
                errorMsg += $"启动触角SDK错误{temperrorMsg}";
                return false;
            }

            LogHelper.LoadConfig(logconfigInfo);
            //返回
            return true;
        }
        /// <summary>
        /// 处理UI线程未处理异常
        /// </summary>
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            try
            {
                LogHelper.WriteError("[Current_DispatcherUnhandledException_UI线程全局异常]:" + e.Exception.Message + "," + e.Exception.StackTrace);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[Current_DispatcherUnhandledException_不可恢复的UI线程全局异常]:" + e.Exception.Message + "," + e.Exception.StackTrace);
                MessageBox.Show("应用程序发生不可恢复的异常，将要退出！");
            }

        }
        /// <summary>
        /// 处理其他线程未处理异常
        /// </summary>
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    LogHelper.WriteError("[CurrentDomain_UnhandledException非UI线程全局异常]" + e.ToString() + e.ExceptionObject.ToString() + exception.Message + exception.Source + exception.InnerException + exception.TargetSite);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[CurrentDomain_UnhandledException不可恢复的非UI线程全局异常]" + e.ToString() + e.ExceptionObject.ToString() + ex.Message + ex.Source + ex.InnerException + ex.TargetSite);
                //MessageBox.Show("应用程序发生不可恢复的异常，将要退出！");
            }

        }
    }
}
