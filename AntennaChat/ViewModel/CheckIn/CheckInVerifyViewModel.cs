using Antenna.Framework;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AntennaChat.ViewModel
{
    public class CheckInVerifyViewModel : PropertyNotifyObject
    {
        public DispatcherTimer timer;

        private ProcessCount processCount;
        private string hostIp;
        private string attendId = "2";
        private DateTime checkInDataTime = DateTime.Now.AddMinutes(-19);
        private AntSdkReceivedOtherMsg.AttendanceRecord_content _attendanceContent;
        public CheckInVerifyView VerifyView { get; set; }
        public event Action VerifySucceed;
        private List<string> listIp;
        public CheckInVerifyViewModel(AntSdkReceivedOtherMsg.AttendanceRecord_content content, CheckInVerifyResultState? state, bool IsPassword = true, bool isVerify = true)
        {
            _attendanceContent = content;
            attendId = _attendanceContent.attendId;
            if (isVerify)
            {
                IsDisplayPassword = IsPassword;
                CheckInAddress = content.address;
                checkInDataTime = PublicTalkMothed.ConvertStringToDateTime(content.attendTime);
                LoadData();
            }
            else
            {
                VerifyResultShow(false, state);
            }
        }
        #region 属性
        private string _checkInAddress;
        /// <summary>
        /// 打卡地址
        /// </summary>
        public string CheckInAddress
        {
            get
            {
                return _checkInAddress;
            }
            set
            {
                _checkInAddress = value;
                RaisePropertyChanged(() => CheckInAddress);
            }
        }
        private string _chcekInTimer;
        /// <summary>
        /// 打卡时间
        /// </summary>
        public string ChcekInTimer
        {
            get { return _chcekInTimer; }
            set
            {
                _chcekInTimer = value;
                RaisePropertyChanged(() => ChcekInTimer);
            }
        }
        private bool _isDisplayPassword;
        /// <summary>
        /// 是否要输入密码打卡
        /// </summary>
        public bool IsDisplayPassword
        {
            get { return _isDisplayPassword; }
            set
            {
                _isDisplayPassword = value;
                RaisePropertyChanged(() => IsDisplayPassword);
            }
        }
        private string _loginPwd = string.Empty;
        /// <summary>
        /// 用户登录密码
        /// </summary>
        public string LoginPwd
        {
            get { return _loginPwd; }
            set
            {
                _loginPwd = value;
                RaisePropertyChanged(() => LoginPwd);
            }
        }

        private string _verifyResultIcon;
        /// <summary>
        /// 打卡状态图标
        /// </summary>
        public string VerifyResultIcon
        {
            get { return _verifyResultIcon; }
            set
            {
                _verifyResultIcon = value;
                RaisePropertyChanged(() => VerifyResultIcon);
            }
        }
        private string _verifyResult;
        /// <summary>
        /// 验证结果
        /// </summary>
        public string VerifyResult
        {
            get { return _verifyResult; }
            set
            {
                _verifyResult = value;
                RaisePropertyChanged(() => VerifyResult);
            }
        }
        private string _verifyDescription;
        /// <summary>
        /// 验证结果描述
        /// </summary>
        public string VerifyDescription
        {
            get { return _verifyDescription; }
            set
            {
                _verifyDescription = value;
                RaisePropertyChanged(() => VerifyDescription);
            }
        }

        private bool _TipsPopuIsOpen;
        /// <summary>
        /// 提示框显示状态
        /// </summary>
        public bool TipsPopuIsOpen
        {
            get { return this._TipsPopuIsOpen; }
            set
            {
                this._TipsPopuIsOpen = value;
                RaisePropertyChanged(() => TipsPopuIsOpen);

            }
        }

        private string _TipsLabelText;
        /// <summary>
        /// 提示框显示内容
        /// </summary>
        public string TipsLabelText
        {
            get { return this._TipsLabelText; }
            set
            {
                this._TipsLabelText = value;
                RaisePropertyChanged(() => TipsLabelText);
            }
        }
        #endregion
        #region 命令
        /// <summary>
        /// 打卡验证命令
        /// </summary>
        public ICommand ConfirmVerifyCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var errorCode = 0;
                    var errorMsg = string.Empty;
                    if (IsDisplayPassword && string.IsNullOrEmpty(LoginPwd))
                    {
                        TipsPopuIsOpen = true;
                        TipsLabelText = "密码不能为空";
                        return;
                    }
                    var isResult = AntSdkService.ConfirmVerify(attendId, hostIp, AntSdkService.AntSdkCurrentUserInfo.userId, LoginPwd, ref errorCode, ref errorMsg);
                    if (isResult)
                    {
                        VerifySucceed?.Invoke();
                        timer?.Stop();
                        VerifyView.Close();
                        MainWindowViewModel.VerifyView = null;
                        //MainWindowViewModel.CloseExitVerify();
                    }
                    else if (errorCode == 9023)
                    {
                        VerifyResultShow(true, CheckInVerifyResultState.TimeError);
                        timer?.Stop();
                        VerifyView.Close();
                        MainWindowViewModel.VerifyView = null;
                        //MainWindowViewModel.CloseExitVerify();
                    }
                    else if (errorCode == 9024)
                    {
                        VerifyResultShow(true, CheckInVerifyResultState.IPVerifyError);
                        timer?.Stop();
                        VerifyView.Close();
                        MainWindowViewModel.VerifyView = null;
                        //MainWindowViewModel.CloseExitVerify();
                    }
                    else if (errorCode == 9025)
                    {
                        timer?.Stop();
                        VerifyView.Close();
                        MainWindowViewModel.VerifyView = null;
                        //MainWindowViewModel.CloseExitVerify();
                    }
                    else if (errorCode == 10014)
                    {
                        TipsPopuIsOpen = true;
                        TipsLabelText = "密码有误";
                    }
                    else if (errorCode == 10013)
                    {
                        TipsPopuIsOpen = true;
                        TipsLabelText = "密码格式不对";
                    }
                    else
                    {
                        TipsPopuIsOpen = true;
                        TipsLabelText = errorMsg;
                    }

                });
            }
        }
        #endregion
        #region 方法
        /// <summary>
        /// 验证结果
        /// </summary>
        /// <param name="isShowWin"></param>
        /// <param name="state"></param>
        private void VerifyResultShow(bool isShowWin, CheckInVerifyResultState? state)
        {
            if (isShowWin)
            {
                CheckInVerifyResultView resultView = new CheckInVerifyResultView();
                resultView.DataContext = new CheckInVerifyViewModel(_attendanceContent, state, false, false);
                resultView.Topmost = true;
                resultView.Show();
                return;
            }
            switch (state)
            {
                case CheckInVerifyResultState.TimeError:
                    VerifyResultIcon = GlobalVariable.AttendanceImage.AttendValidationFailsIcon;
                    VerifyResult = "打卡验证失败";
                    var checkInDataTime = PublicTalkMothed.ConvertStringToDateTime(_attendanceContent.attendTime);
                    VerifyDescription = "由于未按时在PC端上完成验证，您在" + checkInDataTime.ToString("HH:mm") + "的打卡失效";
                    break;
                case CheckInVerifyResultState.IPVerifyError:
                    VerifyResultIcon = GlobalVariable.AttendanceImage.AttendVerificationFailedIcon;
                    VerifyResult = "打卡验证未通过";
                    VerifyDescription = "请使用公司的网络登录七讯PC版以完成验证";
                    break;
            }

        }
        /// <summary>
        /// Timer触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (OnCountDown())
            {
                //HourArea.Text = processCount.GetHour();
                //MinuteArea.Text = processCount.GetMinute();
                //SecondArea.Text = processCount.GetSecond();
                ChcekInTimer = processCount.GetMinute() + "分" + processCount.GetSecond() + "秒";
                ThreadPool.QueueUserWorkItem(m =>
                {
                    if (processCount.timerTotalSecond >= 20)
                    {
                        var errCode = 0;
                        var errMsg = string.Empty;
                        AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                        DateTime serverDateTime = DateTime.Now;
                        if (serverResult != null)
                        {
                            serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                        }
                        var diffMinute = serverDateTime - checkInDataTime;
                        if (checkInDataTime.ToShortDateString() != serverDateTime.ToShortDateString())
                            return;
                        if (diffMinute.Days > 0 || diffMinute.TotalMinutes > 20)
                            return;
                        var minuteChangeSecond = (20 - diffMinute.TotalMinutes) * 60;
                        processCount.ResetTotalSecond((int)minuteChangeSecond);
                    }
                });
            }
            else
            {
                if (VerifyView != null)
                {
                    VerifyView.Close();
                    //MainWindowViewModel.CloseExitVerify();
                }
                VerifyResultShow(true, CheckInVerifyResultState.TimeError);
                timer.Stop();
            }
        }

        //private bool CmdPing(string strIp)
        ////通过CMD中的ping命令去得电脑上网IP
        //{
        //    bool returnvalue = false;
        //    Process p = new Process(); p.StartInfo.FileName = "cmd.exe";//设定程序名
        //    p.StartInfo.UseShellExecute = false; //关闭Shell的使用
        //    p.StartInfo.RedirectStandardInput = true;//重定向标准输入
        //    p.StartInfo.RedirectStandardOutput = true;//重定向标准输出
        //    p.StartInfo.RedirectStandardError = true;//重定向错误输出
        //    p.StartInfo.CreateNoWindow = true;//设置不显示窗口
        //    p.Start(); p.StandardInput.WriteLine("ping -n 2 -w 1 -S " + strIp + " " + "GuangWang");
        //    p.StandardInput.WriteLine("exit");
        //    string strRst = p.StandardOutput.ReadToEnd();
        //    if (strRst.IndexOf("(100% 丢失)") != -1 || strRst.IndexOf("(100% loss)") != -1)
        //    {
        //        returnvalue = false;
        //    }
        //    else
        //    {
        //        returnvalue = true;
        //    }
        //    p.Close();
        //    return returnvalue;
        //}

   
        /// <summary>
        /// 加载初始化数据
        /// </summary>
        private void LoadData()
        {
            string _ComputName = System.Net.Dns.GetHostName();
            listIp= NetworkHelper.GetPhysicsNetworkCardIP();
            if (listIp.Count > 0)
                hostIp = listIp[0];
            //System.Net.IPAddress[] _IPList = System.Net.Dns.GetHostAddresses(_ComputName);
            //for (int i = 0; i != _IPList.Length; i++)
            //{
            //    if (_IPList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    {

                //            hostIp = _IPList[i].ToString();
                //            MessageBoxWindow.Show(hostIp, GlobalVariable.WarnOrSuccess.Success);
                //            break;

                //    }
                //}
            var errCode = 0;
            var errMsg = string.Empty;
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            var diffMinute = serverDateTime - checkInDataTime;
            if (checkInDataTime.ToShortDateString() != serverDateTime.ToShortDateString())
                return;
            if (diffMinute.Days > 0 || diffMinute.TotalMinutes > 20)
                return;
            //设置定时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 1);    //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
            var minuteChangeSecond = 0d;
            if (diffMinute.TotalMinutes < 0)
                minuteChangeSecond = (20 - 0.01) * 60;
            else
            {
                minuteChangeSecond = (20 - diffMinute.TotalMinutes) * 60;
            }

            //处理倒计时的类
            processCount = new ProcessCount((int)minuteChangeSecond);
            ChcekInTimer = processCount.GetMinute() + "分" + processCount.GetSecond() + "秒";
            CountDown += new CountDownHandler(processCount.ProcessCountDown);

            //开启定时器
            timer.Start();
        }
        /// <summary>
        /// 处理事件
        /// </summary>
        public event CountDownHandler CountDown;
        public bool OnCountDown()
        {
            if (CountDown != null)
                return CountDown();
            return false;
        }
        #endregion

    }
    /// <summary>
    /// 处理倒计时的委托
    /// </summary>
    /// <returns></returns>
    public delegate bool CountDownHandler();
    public enum CheckInVerifyResultState
    {
        TimeError = 0,
        IPVerifyError = 1,
    }
    /// <summary>
    /// 实现倒计时功能的类
    /// </summary>
    public class ProcessCount
    {
        private int _TotalSecond;
        public int TotalSecond
        {
            get { return _TotalSecond; }
            set { _TotalSecond = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcessCount(int totalSecond)
        {
            this.TotalSecond = totalSecond;
        }
        public void ResetTotalSecond(int totalSecond)
        {
            timerTotalSecond = 0;
            this.TotalSecond = totalSecond;
        }
        public int timerTotalSecond = 0;
        /// <summary>
        /// 减秒
        /// </summary>
        /// <returns></returns>
        public bool ProcessCountDown()
        {
            if (TotalSecond == 0)
                return false;
            else
            {
                timerTotalSecond++;
                TotalSecond--;
                return true;
            }
        }

        /// <summary>
        /// 获取小时显示值
        /// </summary>
        /// <returns></returns>
        public string GetHour()
        {
            return string.Format("{0:D2}", (TotalSecond / 3600));
        }

        /// <summary>
        /// 获取分钟显示值
        /// </summary>
        /// <returns></returns>
        public string GetMinute()
        {
            return string.Format("{0:D2}", (TotalSecond % 3600) / 60);
        }

        /// <summary>
        /// 获取秒显示值
        /// </summary>
        /// <returns></returns>
        public string GetSecond()
        {
            return string.Format("{0:D2}", TotalSecond % 60);
        }

    }
    public class NetworkHelper
    {
        /// <summary></summary> 
        /// 显示本机各网卡的详细信息 
        /// <summary></summary> 
        public static void ShowNetworkInterfaceMessage()
        {
            NetworkInterface[] fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in fNetworkInterfaces)
            {
                #region " 网卡类型 "
                string fCardType = "未知网卡";
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                if (rk != null)
                {
                    // 区分 PnpInstanceID  
                    // 如果前面有 PCI 就是本机的真实网卡 
                    // MediaSubType 为 01 则是常见网卡，02为无线网卡。 
                    string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();
                    int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));
                    if (fPnpInstanceID.Length > 3 &&
                        fPnpInstanceID.Substring(0, 3) == "PCI")
                        fCardType = "物理网卡";
                    else if (fMediaSubType == 1)
                        fCardType = "虚拟网卡";
                    else if (fMediaSubType == 2)
                        fCardType = "无线网卡";
                }
                #endregion
                IPInterfaceProperties fIPInterfaceProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = fIPInterfaceProperties.UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
                {
                    if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //if(fCardType)
                        //var ip = UnicastIPAddressInformation.Address.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 获得本机真实物理网卡IP
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPhysicsNetworkCardIP()
        {
            var networkCardIPs = new List<string>();

            NetworkInterface[] fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in fNetworkInterfaces)
            {
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                if (rk != null)
                {
                    // 区分 PnpInstanceID  
                    // 如果前面有 PCI 就是本机的真实网卡 
                    string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();
                    int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));
                    if (fPnpInstanceID.Length > 3 && fPnpInstanceID.Substring(0, 3) == "PCI")
                    {
                        IPInterfaceProperties fIPInterfaceProperties = adapter.GetIPProperties();
                        UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = fIPInterfaceProperties.UnicastAddresses;
                        foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
                        {
                            if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                networkCardIPs.Add(UnicastIPAddressInformation.Address.ToString()); //Ip 地址
                            }
                        }
                    }
                }
            }
            return networkCardIPs;
        }
    }


}
