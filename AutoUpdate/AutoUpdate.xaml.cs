using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AutoUpdate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public bool IsUnizping = false;
        /// <summary>
        /// 是否更新中
        /// </summary>
        public bool isUpdateSucOFail = true;
        DispatcherTimer timer = new DispatcherTimer();
        WebClient webClient = new WebClient();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(m => DownStart());
        }

        public void DownStart()
        {
            bool IsHave = false;
            //检测触角程序是否运行
            IsHave = publicAutoUpdate.killProcess();
            if (IsHave == true)
            {
                timer.Interval = TimeSpan.FromMilliseconds(1000);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("检测到七讯正在运行，请点击确定按钮关闭七讯完成更新！", "温馨提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    bool result = publicAutoUpdate.killProcess();
                    timer.Interval = TimeSpan.FromMilliseconds(1000);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        string url = "";
        string path = "";
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Stop();
                url = publicAutoUpdate.xmlFind("url", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                //url = "http://ftp.71chat.com/chujiaoSetup/client/4.1.3.2.zip?=7800";
                if (url.Substring(url.Length - 3, 3).ToLower().Contains("exe"))
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\" + "AntennaChat.exe";
                }
                else
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\" + "AntennaChat.zip";
                }
                webClient.DownloadFileAsync(new Uri(publicAutoUpdate.getRandomUrl(url)), path);
                //webClient.Proxy = null;
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WebClient_DownloadProgressChanged);
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            }
            catch (Exception ex)
            {
                isUpdateSucOFail = false;
                webClient.Dispose();
                //MessageBox.Show(ex.Message+ex.Source+ex.StackTrace);
                //Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }
        DispatcherTimer timerUpdata = new DispatcherTimer();
        bool isHaving = true;
        bool isUnzip = false;
        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    if (e.Error.Message.Contains("404"))
                    {
                        isUpdateSucOFail = false;
                        MessageBox.Show("远程服务器返回错误");
                        Environment.Exit(0);
                        return;
                    }
                    else
                    {
                        isUpdateSucOFail = false;
                        MessageBox.Show("连接超时");
                        Environment.Exit(0);
                        return;
                    }
                }
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//AntennaChat.zip";
                //md5 校验
                //string md5Value=publicAutoUpdate.getFileMd5Value(filePath);
                //string serverMd5Value = publicAutoUpdate.xmlFind("fileMd5Value", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                //if ((md5Value + "").Trim() != (serverMd5Value + "").Trim())
                //{
                //    isUpdateSucOFail = false;
                //    MessageBox.Show("文件校验失败,请联系供应商!");
                //    Environment.Exit(0);
                //    return;
                //}
                if (e.Cancelled)
                {
                    isUpdateSucOFail = false;
                    MessageBox.Show("下载被取消！");
                    webClient.Dispose();
                }
                else
                {
                    //下载完成
                    publicAutoUpdate.xmlModify("isDownFileSucess", "1", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    webClient.Dispose();
                    bool IsHave = publicAutoUpdate.IsHaveProcess();
                    if (IsHave == true)
                    {
                        MessageBox.Show("检测到七讯正在运行,请关闭该程序以便完成更新操作!");
                        timerUpdata.Interval = TimeSpan.FromMilliseconds(1000);
                        timerUpdata.Tick += TimerUpdata_Tick;
                        timerUpdata.Start();
                    }
                    else
                    {
                        if (url.Substring(url.Length - 3, 3).ToLower().Contains("exe"))
                        {
                            bool b = publicAutoUpdate.startApplicationExe();
                            if (b == true)
                            {
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            //Task<bool> bTask =Task.Factory.StartNew(() => publicAutoUpdate.UnZip(path + "downUpdateFile\\AntennaChat.zip", path, true));
                            //isUnzip = bTask.Result;
                            IsUnizping = true;
                            isUnzip = publicAutoUpdate.UnZip(path + "downUpdateFile\\AntennaChat.zip", path, true);
                            if (isUnzip == true)
                            {
                                string version = publicAutoUpdate.xmlFind("version", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                bool modify = publicAutoUpdate.xmlModify("version", version, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string title = publicAutoUpdate.xmlFind("title", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                publicAutoUpdate.xmlModify("title", title, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string describe = publicAutoUpdate.xmlFind("describe", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                publicAutoUpdate.xmlModify("describe", describe, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string url = publicAutoUpdate.xmlFind("url", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                publicAutoUpdate.xmlModify("url", url, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string isDownFileSucess = publicAutoUpdate.xmlFind("isDownFileSucess", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                publicAutoUpdate.xmlModify("isDownFileSucess", isDownFileSucess, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string fileMd5 = publicAutoUpdate.xmlFind("fileMd5Value", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                publicAutoUpdate.xmlModify("fileMd5Value", fileMd5, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                string updateType = publicAutoUpdate.xmlFind("updateType", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                                bool modifyUpdateType = publicAutoUpdate.xmlModify("updateType", updateType, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                                if (modifyUpdateType == true)
                                {
                                    DirectoryInfo dir = new DirectoryInfo(path + "downUpdateFile");
                                    FileInfo[] files = dir.GetFiles();
                                    foreach (var file in files)
                                    {
                                        File.Delete(file.FullName);
                                    }
                                }
                                this.Hide();
                                //更新名字
                                publicAutoUpdate.AppReName();
                                //更新卸载列表相关数据
                                publicAutoUpdate.SetAppInfo();
                                MessageBoxResult dr = MessageBox.Show("更新成功，是否打开七讯程序？", "温馨提示", MessageBoxButton.YesNo);
                                if (dr.ToString() == "Yes")
                                {
                                    bool isStart = publicAutoUpdate.startApplication();
                                    //if (isStart == true)
                                    //{
                                    //Application.Current.Shutdown();
                                    Environment.Exit(0);
                                    //}
                                }
                                else
                                {
                                    //Application.Current.Shutdown();
                                    Environment.Exit(0);
                                }
                            }

                            else
                            {
                                IsUnizping = false;
                                isUpdateSucOFail = false;
                                MessageBox.Show("解压覆盖失败!");
                                //Application.Current.Shutdown();
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IsUnizping = false;
                //MessageBox.Show(ex.ToString() + ex.StackTrace);
                publicAutoUpdate.xmlModify("isDownFileSucess", "0", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                Application.Current.Shutdown();
            }
        }

        private void TimerUpdata_Tick(object sender, EventArgs e)
        {
            if (url.Substring(url.Length - 3, 3).ToLower().Contains("exe"))
            {
                timerUpdata.Stop();
                bool b = publicAutoUpdate.startApplicationExe();
                if (b == true)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                isHaving = publicAutoUpdate.IsHaveProcess();
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (isHaving == false)
                {
                    timerUpdata.Stop();
                    isUnzip = publicAutoUpdate.UnZip(path + "downUpdateFile\\AntennaChat.zip", path, true);
                    if (isUnzip == true)
                    {
                        string version = publicAutoUpdate.xmlFind("version", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        bool modify = publicAutoUpdate.xmlModify("version", version, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string title = publicAutoUpdate.xmlFind("title", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        publicAutoUpdate.xmlModify("title", title, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string describe = publicAutoUpdate.xmlFind("describe", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        publicAutoUpdate.xmlModify("describe", describe, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string url = publicAutoUpdate.xmlFind("url", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        publicAutoUpdate.xmlModify("url", url, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string fileMd5 = publicAutoUpdate.xmlFind("fileMd5Value", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        publicAutoUpdate.xmlModify("fileMd5Value", fileMd5, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string isDownFileSucess = publicAutoUpdate.xmlFind("isDownFileSucess", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        publicAutoUpdate.xmlModify("isDownFileSucess", isDownFileSucess, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        string updateType = publicAutoUpdate.xmlFind("updateType", AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile\\update.xml");
                        bool modifyUpdateType = publicAutoUpdate.xmlModify("updateType", updateType, AppDomain.CurrentDomain.BaseDirectory + "version.xml");

                        if (modifyUpdateType == true)
                        {
                            DirectoryInfo dir = new DirectoryInfo(path + "downUpdateFile");
                            FileInfo[] files = dir.GetFiles();
                            foreach (var file in files)
                            {
                                File.Delete(file.FullName);
                            }
                        }
                        this.Hide();

                        MessageBoxResult dr = MessageBox.Show("更新成功，是否打开七讯程序！", "温馨提示", MessageBoxButton.YesNo);
                        if (dr.ToString() == "Yes")
                        {
                            bool isStart = publicAutoUpdate.startApplication();
                            if (isStart == true)
                            {
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                else
                {
                    IsUnizping = false;
                    MessageBox.Show("解压覆盖失败!");
                    Environment.Exit(0);
                }
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                this.progress.Value = e.ProgressPercentage;
                this.lbProgress.Content = e.ProgressPercentage.ToString() + "%";
                this.labDetail.Content = string.Format("正在下载文件，完成进度{0}/{1}(MB)"
                                    , e.BytesReceived / 1024 / 1024
                                    , e.TotalBytesToReceive / 1024 / 1024);
                if (e.BytesReceived == e.TotalBytesToReceive)
                {
                    labDetail.Content = "下载完成,正在配置中,请稍候......";
                }
            }
            catch (Exception ex)
            {
                isUpdateSucOFail = false;
                MessageBox.Show("下载过程中出现错误!");
                Environment.Exit(0);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(IsUnizping)
            {
                MessageBoxResult messageBox = MessageBox.Show("正在解压更新文件，不能退出！", "温馨提示", MessageBoxButton.YesNo);
                e.Cancel = true;

            }
            MessageBoxResult dr = MessageBox.Show("是否终止更新？", "温馨提示", MessageBoxButton.YesNo);
            if (dr.ToString() == "No")
            {
                this.Activate();
                e.Cancel = true;
            }
        }
    }
}