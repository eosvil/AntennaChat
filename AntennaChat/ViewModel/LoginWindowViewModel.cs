using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Resource;
using AntennaChat.ViewModel.Update;
using AntennaChat.Views;
using AntennaChat.Views.Update;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AntennaChat.ViewModel.Talk;
using SDK.AntSdk;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk.AntModels.Http;
using System.Net;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel
{
    public class LoginWindowViewModel : WindowBaseViewModel
    {
        private LoginModel loginModel = new LoginModel();
        private List<AccountInfo> ListAccountInfo = new List<AccountInfo>();
        private List<InstallLoginInfo> InstallLoginInfoLst = new List<InstallLoginInfo>();
        Window LoginWindow;
        object loginLock = new object();
        private bool IsLoginSuccess = false;
        //private LoginOutput output;
        public delegate void LoginSuccess();
        public event LoginSuccess LoginSuccessEvent;
        private int loginCount = 0;
        private string checkCode = string.Empty;
        private DateTime LoginPwdErrorDateTime = DateTime.Now.AddMinutes(-1);
        private int LoginPwdErrorCount;
        private Dictionary<string, DateTime> loginIDList = new Dictionary<string, DateTime>();
        protected virtual void OnLoginSuccess()
        {
            //NotifyIconControl.Instance.notifyIcon.ContextMenu.MenuItems["ExitApp"].Click -= ItemExitClick;
            //NotifyIconControl.Instance.notifyIcon.MouseClick -= OnNotifyIconMouseClick;
            //NotifyIconControl.Instance.notifyIcon.Visible = false;
            //this.LoginSuccessEvent?.Invoke();
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindowViewModel model = new MainWindowViewModel();
                var result = model.GetGroupList();
                if (!result)
                {
                    loginCount++;
                    //如果尝试3获取数据失败，返回登录页面
                    if (loginCount >= 3)
                    {
                        MessageBoxWindow.Show("尝试多次获取群数据失败，请联系管理员。", GlobalVariable.WarnOrSuccess.Warn);
                        if (IsLoginSuccess)
                        {
                            var errorCode = 0;
                            var errorMsg = string.Empty;
                            //发送状态
                            AntSdkService.AntSdkUpdateCurrentUserState((int)GlobalVariable.OnLineStatus.OffLine,
                     ref errorCode, ref errorMsg);
                            //停止SDK
                            SDK.AntSdk.AntSdkService.StopAntSdk(ref errorCode, ref errorMsg);
                            AudioChat.ExitClearApi();
                        }
                        System.Windows.Application.Current.Shutdown();
                        CommonMethods.StartApplication(System.Windows.Forms.Application.StartupPath + "/AntennaChat.exe");
                        return;
                    }
                    OnLoginSuccess();
                    return;
                }
                //model.DownloadUserHeadImage();
                MainWindowView mainWindow = new MainWindowView { DataContext = model };
                //model.InitMainVM();
                if (this.LoginWindow != null)
                {
                    var loginWindow = LoginWindow as LoginWindowView;
                    loginWindow?.taskbarIcon.Dispose();
                }
                loginCount = 0;
                mainWindow.Show();
                GlobalVariable.LastLoginDatetime = DateTime.Now;
                this.LoginWindow?.Close();
            });
            stopWatch.Stop();
            Antenna.Framework.LogHelper.WriteDebug($"[Model_LoginSuccessEvent({stopWatch.Elapsed.TotalMilliseconds}毫秒)]");
        }

        public LoginWindowViewModel()
        {
            //ThreadPool.QueueUserWorkItem(m => GetUpgradeVersion());
            //NotifyIconControl.Instance.CreateNotifyIcon();
            //NotifyIconControl.Instance.notifyIcon.ContextMenu.MenuItems["ExitApp"].Click += ItemExitClick;
            //NotifyIconControl.Instance.notifyIcon.ContextMenu.MenuItems["Logout"].Visible = false;
            //NotifyIconControl.Instance.notifyIcon.MouseClick += OnNotifyIconMouseClick;
            //NotifyIconControl.Instance.notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/ant_offLine.ico");

            IsLoginSuccess = false;

        }

        private void OnNotifyIconMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                QPan_OpenFromTuoPan();
            }
        }

        public void QPan_OpenFromTuoPan()
        {
            if (LoginWindow == null) return;
            LoginWindow.Visibility = Visibility.Visible;
            LoginWindow.ShowInTaskbar = true;
            LoginWindow.WindowState = WindowState.Normal;
            LoginWindow.Topmost = true;
            LoginWindow.Topmost = false;
        }

        private void ItemExitClick(object sender, EventArgs e)
        {
            //NotifyIconControl.Instance.notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }


        /// <summary>
        /// 登录账号
        /// </summary>
        public string LoginID
        {
            get { return this.loginModel.LoginID; }
            set
            {
                this.loginModel.LoginID = value;
                AccountInfo info = ListAccountInfo.Find(o => o.ID == loginModel.LoginID);
                var loginID = loginIDList.FirstOrDefault(m => m.Key == loginModel.LoginID);
                if ((info != null && info.IsIdentifyingCode) || !string.IsNullOrEmpty(loginID.Key))
                {
                    var lastLoginDatetime = DateTime.Now;
                    if (info == null && loginIDList.ContainsKey(loginModel.LoginID))
                    {
                        lastLoginDatetime = Convert.ToDateTime(loginID.Value);
                    }
                    else
                    {
                        lastLoginDatetime = Convert.ToDateTime(info.LastLoginTime);
                    }
                    if (DateTime.Now.Year - lastLoginDatetime.Year == 0 && DateTime.Now.Month - lastLoginDatetime.Month == 0 && lastLoginDatetime.Day - DateTime.Now.Day <= 0)
                    {
                        IsIdentifyingCode = true;
                        SetVerifyCodeImage();
                        IdentifyingCode = string.Empty;
                    }
                }
                else
                {
                    IsIdentifyingCode = false;
                }
                RaisePropertyChanged(() => LoginID);
            }
        }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd
        {
            get { return this.loginModel.Password; }
            set
            {
                this.loginModel.Password = value;
                RaisePropertyChanged(() => LoginPwd);
            }
        }
        private bool _IsRememberPwd;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsRememberPwd
        {
            get { return this._IsRememberPwd; }
            set
            {
                this._IsRememberPwd = value;
                if (value == false)
                    IsAutoLogin = false;
                RaisePropertyChanged(() => IsRememberPwd);
            }
        }

        private bool _IsAutoLogin;
        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool IsAutoLogin
        {
            get { return this._IsAutoLogin; }
            set
            {
                this._IsAutoLogin = value;
                if (value == true)
                    IsRememberPwd = true;
                RaisePropertyChanged(() => IsAutoLogin);
            }
        }

        private ObservableCollection<string> _ListID = new ObservableCollection<string>();
        /// <summary>
        /// 用户名数据源
        /// </summary>
        public ObservableCollection<string> ListID
        {
            get { return this._ListID; }
            set
            {
                this._ListID = value;
                RaisePropertyChanged(() => ListID);
            }
        }
        /// <summary>
        /// 在线状态信息
        /// </summary>
        public List<UserOnlineState> UserOnlineStates => GlobalVariable.UserOnlineSataeInfo.UserOnlineStates;

        private UserOnlineState _userSelectedState;
        /// <summary>
        /// 用户选中状态
        /// </summary>
        public UserOnlineState UserSelectedState
        {
            get
            {
                return _userSelectedState;
            }
            set
            {

                _userSelectedState = value;
                RaisePropertyChanged(() => UserSelectedState);
            }
        }

        private string _selectedData;
        /// <summary>
        /// 列表中选中行的对象
        /// </summary>
        public string SelectedData
        {
            get { return _selectedData; }
            set
            {
                if (_selectedData != value)
                {
                    _selectedData = value;
                    AccountInfo info = ListAccountInfo.Find(o => o.ID == _selectedData);
                    if (info != null)
                    {
                        if (info.Password.Trim() == "")
                        {
                            LoginPwd = "";
                        }
                        else
                        {
                            LoginPwd = DataConverter.MD5Decrypt(info.Password, "");
                        }
                        IsRememberPwd = info.RememberPwd;
                        IsAutoLogin = info.AutoLogin;
                        if (info.OnLine == (int)GlobalVariable.OnLineStatus.OffLine)
                            info.OnLine = (int)GlobalVariable.OnLineStatus.OnLine;
                        UserSelectedState = UserOnlineStates.FirstOrDefault(m => m.OnlineState == info.OnLine);
                        //var loginID = loginIDList.FirstOrDefault(m => m.Key == SelectedData);
                        //if ((info != null && info.IsIdentifyingCode) || !string.IsNullOrEmpty(loginID.Key))
                        //{
                        //    var lastLoginDatetime = Convert.ToDateTime(info.LastLoginTime);
                        //    if (DateTime.Now.Year - lastLoginDatetime.Year == 0 && DateTime.Now.Month - lastLoginDatetime.Month == 0 && lastLoginDatetime.Day - DateTime.Now.Day <= 0)
                        //    {
                        //        IsIdentifyingCode = true;
                        //        SetVerifyCodeImage();
                        //        IdentifyingCode = string.Empty;
                        //    }
                        //}
                        //else
                        //{
                        //    IsIdentifyingCode = false;
                        //}
                    }
                    else
                    {
                        LoginPwd = "";
                        IsRememberPwd = false;
                        IsAutoLogin = false;
                        UserSelectedState = UserOnlineStates[0];
                    }
                    RaisePropertyChanged("SelectedData");
                }
            }
        }
        private Visibility _IsLogining = Visibility.Collapsed;
        /// <summary>
        /// Loading界面是否显示
        /// </summary>
        public Visibility IsLogining
        {
            get { return this._IsLogining; }
            set
            {
                this._IsLogining = value;
                RaisePropertyChanged(() => IsLogining);
            }
        }
        private Visibility _LoginVisibility = Visibility.Visible;
        /// <summary>
        /// 常规登陆的界面是否显示
        /// </summary>
        public Visibility LoginVisibility
        {
            get { return this._LoginVisibility; }
            set
            {
                this._LoginVisibility = value;
                RaisePropertyChanged(() => LoginVisibility);
            }
        }
        private bool _IDPopuIsOpen;
        /// <summary>
        /// 用户名提示窗是否打开
        /// </summary>
        public bool IDPopuIsOpen
        {
            get { return this._IDPopuIsOpen; }
            set
            {
                this._IDPopuIsOpen = value;
                RaisePropertyChanged(() => IDPopuIsOpen);
            }
        }

        private bool _PwdPopuIsOpen;
        /// <summary>
        /// 密码提示窗是否打开
        /// </summary>
        public bool PwdPopuIsOpen
        {
            get { return this._PwdPopuIsOpen; }
            set
            {
                this._PwdPopuIsOpen = value;
                RaisePropertyChanged(() => PwdPopuIsOpen);
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
        #region 计时处理
        DispatcherTimer myTimer = null;
        private void Timing()
        {
            if (TipsPopuIsOpen)
            {
                if (myTimer == null)
                    myTimer = new DispatcherTimer();
                myTimer.Interval = new TimeSpan(0, 0, 3);
                myTimer.Tick += new EventHandler(Timer_Tick);
                myTimer.Start();
            }
            else
            {
                if (myTimer != null)
                {
                    myTimer.Stop();
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (myTimer != null)
            {
                myTimer.Stop();
            }
            TipsPopuIsOpen = false;
        }
        #endregion
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

        private ProcessParam param = new ProcessParam();
        public ProcessParam Param
        {
            get { return this.param; }
            set
            {
                this.param = value;
                RaisePropertyChanged(() => Param);
            }
        }
        #region Command

        private ICommand _commandOpenMainWindow;

        /// <summary>
        /// 打开登录窗体
        /// </summary>
        public ICommand CommandOpenMainWindow
        {
            get
            {
                _commandOpenMainWindow = new DefaultCommand(o =>
                {
                    QPan_OpenFromTuoPan();
                });
                return _commandOpenMainWindow;
            }
        }

        private ICommand _commandExitMainWindow;
        /// <summary>
        /// 退出
        /// </summary>
        public ICommand CommandExitMainWindow
        {
            get
            {
                _commandExitMainWindow = new DefaultCommand(o =>
                {
                    System.Windows.Application.Current.Shutdown();
                });
                return _commandExitMainWindow;
            }
        }
        private ICommand _DeleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (this._DeleteCommand == null)
                {
                    this._DeleteCommand = new DefaultCommand(
                              o =>
                              {
                                  string errMsg = string.Empty;
                                  string deleteID = o.ToString();
                                  ListID.Remove(deleteID);
                                  AccountInfo info = ListAccountInfo.FirstOrDefault(c => c.ID == deleteID);
                                  ListAccountInfo.Remove(info);
                                  SelectedData = ListID.Count == 0 ? "" : ListID[0];
                                  DataConverter.EntityToXml<List<AccountInfo>>(System.Environment.CurrentDirectory + "/AccountCache.xml", ListAccountInfo, ref errMsg);
                              });
                }
                return this._DeleteCommand;
            }
        }

        private ICommand _LoginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (this._LoginCommand == null)
                {
                    this._LoginCommand = new DefaultCommand(
                        o =>
                        {

                            RaisePropertyChanged(() => IdentifyingCode);
                            LoginAction(o);

                        });
                }
                return this._LoginCommand;
            }
        }

        /// <summary>
        /// 登录Action
        /// </summary>
        /// <param name="parameter"></param>
        private void LoginAction(object parameter)
        {
            //无论修改是否成功 都执行登录
            if (!DataConverter.RegisterFileMove()) return;
            Param.IsLoginValidate = true;
            string id = this.LoginID;
            string pwd = this.LoginPwd;
            if (!DataConverter.EmailCheck(id) && !DataConverter.NumberCheck(id))
            {
                IDPopuIsOpen = true;
                Param.IsLoginValidate = false;
                return;
            }
            if (string.IsNullOrEmpty(pwd))
            {
                PwdPopuIsOpen = true;
                Param.IsLoginValidate = false;
                return;
            }
            if (DataConverter.NumberCheck(id) && id.Length != 11)
            {
                this.TipsLabelText = string.Format("手机号长度不正确，请重新输入");
                this.TipsPopuIsOpen = true;
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    Timing();
                }));
                Param.IsLoginValidate = false;
                return;
            }
            if (string.IsNullOrEmpty(IdentifyingCode) && IsIdentifyingCode)
            {
                this.TipsLabelText = string.Format("请输入验证码");
                this.TipsPopuIsOpen = true;
                Param.IsLoginValidate = false;
                return;
            }
            LoginVisibility = Visibility.Collapsed;
            IsLogining = Visibility.Visible;
            if (Param.IsLoginValidate)
            {
                Param.ReceiveProcessCount = 0;
                Param.SendProcessCount = Win32.SendMsg("七讯", id);
                totalMiSeconds = 0;
                Countdown();
            }
        }
        private void Login()
        {
            if (!Param.IsLoginValidate)
            {
                LoginVisibility = Visibility.Visible;
                IsLogining = Visibility.Collapsed;
                this.TipsLabelText = string.Format("该账号已经登录，不能重复登录");
                this.TipsPopuIsOpen = true;
                return;
            }
            string errMsg = string.Empty;
            ThreadPool.QueueUserWorkItem(o =>
            {
                lock (loginLock)
                {
                    if (IsLoginSuccess == true) return;
                    if (Login(LoginID, LoginPwd, ref errMsg))
                    {
                        IsLoginSuccess = true;
                        //初始化网易云信SDK
                        if (!AudioChat.InitApi()) return;
                        ThreadPool.QueueUserWorkItem(m => FileHelper.DownloadUserHeadImage());
                        OnLoginSuccess();
                    }
                    else
                    {
                        LoginVisibility = Visibility.Visible;
                        IsLogining = Visibility.Collapsed;
                        this.TipsLabelText = string.Format("登录失败:{0}", errMsg);
                        this.TipsPopuIsOpen = true;
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            Timing();
                        }));
                    }
                }
            });
        }
        #region 进程间通信两秒等待
        private int totalMiSeconds = 0;//总计时
        DispatcherTimer countdownTimer = null;
        private void Countdown()
        {
            countdownTimer?.Stop();
            countdownTimer = null;
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            countdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            countdownTimer.Start();
        }
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (!Param.IsLoginValidate)
            {
                countdownTimer.Stop();
                Login();
            }
            if (Param.ReceiveProcessCount == Param.SendProcessCount)
            {
                countdownTimer.Stop();
                Login();
            }
            if (totalMiSeconds >= 2000)
            {
                countdownTimer.Stop();
                Login();
            }
            else
            {
                totalMiSeconds += 100;
            }
        }
        #endregion
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pwd"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private bool Login(string id, string pwd, ref string errMsg)
        {
            var errCode = 0;
            AccountInfo info = ListAccountInfo.FirstOrDefault(c => c.ID == this.LoginID);
            if (info != null)
            {
                TimeSpan span = DateTime.Now - info.LastLoginTime;
                if (span.TotalSeconds < 10)
                {
                    var loginWindow = LoginWindow as LoginWindowView;
                    loginWindow?.LoginFault();
                    return false;
                }
                else
                {
                    //    info.IsLogin = true;
                    info.LastLoginTime = DateTime.Now;
                    DataConverter.EntityToXml(
                        System.Environment.CurrentDirectory + "/AccountCache.xml", ListAccountInfo, ref errMsg);
                }
            }
            Stopwatch stopWatch = new Stopwatch();
            if (!AntSdkService.AntSdkLogin(id, pwd, UserSelectedState.OnlineState, IdentifyingCode, ref errCode, ref errMsg))
            {
                if (info != null)
                    info.LastLoginTime = DateTime.Now.AddMinutes(-1);
                //TimeSpan time = DateTime.Now - LoginPwdErrorDateTime;
                if (errCode == 10022)
                {
                    if (info != null && !info.IsIdentifyingCode)
                        info.IsIdentifyingCode = true;
                    LoginPwd = string.Empty;
                    if (!IsIdentifyingCode)
                        IsIdentifyingCode = true;
                    IdentifyingCode = string.Empty;
                    SetVerifyCodeImage();
                    if (!loginIDList.ContainsKey(id))
                        loginIDList.Add(id, DateTime.Now.AddMinutes(-1));
                }
                else if (errCode == 10014 && IsIdentifyingCode)
                {
                    IdentifyingCode = string.Empty;
                    SetVerifyCodeImage();
                }
                else if (errCode == 10023)
                {
                    IdentifyingCode = string.Empty;
                    if (!IsIdentifyingCode)
                    {
                        if (!loginIDList.ContainsKey(id))
                            loginIDList.Add(id, DateTime.Now.AddMinutes(-1));
                        IsIdentifyingCode = true;
                    }
                    SetVerifyCodeImage();
                    //this.TipsLabelText = errMsg;
                }
                DataConverter.EntityToXml(
                   System.Environment.CurrentDirectory + "/AccountCache.xml", ListAccountInfo, ref errMsg);
                return false;
            }
            else
            {
                if (info != null && info.IsIdentifyingCode)
                {
                    info.IsIdentifyingCode = false;
                    DataConverter.EntityToXml(
                       System.Environment.CurrentDirectory + "/AccountCache.xml", ListAccountInfo, ref errMsg);
                }
                loginIDList.Remove(id);
                checkCode = string.Empty;
                IdentifyingCode = string.Empty;
                LoginPwdErrorCount = 0;
                //LoginPwdErrorDateTime = DateTime.Now.AddMinutes(-1);
            }
            SaveAccountInfo();
            stopWatch.Stop();
            LogHelper.WriteDebug($"[LoadAction_Login({stopWatch.Elapsed.TotalMilliseconds}毫秒)]");
            return true;
        }

        private ICommand _Loaded;
        public ICommand LoadedCommand
        {
            get
            {
                if (this._Loaded == null)
                {
                    this._Loaded = new ActionCommand<Window>(LoadAction);
                }
                return this._Loaded;
            }
        }
        /// <summary>
        /// 设置验证码图片
        /// </summary>
        private void SetVerifyCodeImage()
        {
            //AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
            //    () =>
            //    {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                var errMessage = string.Empty;
                var errCode = 0;
                HttpWebRequest request = null;
                HttpWebResponse ress = null;
                Stream sstreamRes = null;
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(AntSdkService.GetVerifyCodeImage(string.IsNullOrEmpty(this.LoginID) ? _selectedData : this.LoginID, ref errCode, ref errMessage));
                    request.Method = "GET";
                    ress = (HttpWebResponse)request.GetResponse();
                    sstreamRes = ress.GetResponseStream();
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    //bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = sstreamRes;
                    bmp.EndInit();
                    Thread.Sleep(50);
                    IdentifyingCodeBitmap = bmp;
                    //if (sstreamRes != null)
                    //    return sstreamRes;
                }
                catch (System.Net.WebException x)
                {

                }
                finally
                {
                    request.Abort();
                    ress?.Close();
                    sstreamRes?.Close();

                }
            }));
            //return null;
            //},
            //(ex, datas) =>
            //{
            // if (datas == null) return;
            //var bmp = new BitmapImage();
            //bmp.BeginInit();
            ////bmp.CacheOption = BitmapCacheOption.OnLoad;
            //bmp.StreamSource = sstreamRes;
            //bmp.EndInit();
            //IdentifyingCodeBitmap = bmp;
            //});
            //IdentifyingCodeBitmap = AntSdkService.GetVerifyCodeImage(string.IsNullOrEmpty(this.LoginID) ? _selectedData : this.LoginID, ref errCode, ref errMessage);
        }

        /// <summary>
        /// 窗体加载Action
        /// </summary>
        /// <param name="obj"></param>
        private void LoadAction(Window loginWindowView)
        {
            loginWindowView.Topmost = true;
            loginWindowView.Topmost = false;
            this.LoginWindow = loginWindowView;

            //清理头像缓存文件夹
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Cache";
            if (Directory.Exists(filePath) == true)//如果存在就删除
            {
                DataConverter.DeleteDirectory(filePath);
            }
            string errMsg = string.Empty;
            DataConverter.XmlToEntity(Environment.CurrentDirectory + "\\InstallLogin.xml", ref InstallLoginInfoLst, ref errMsg);
            DataConverter.XmlToEntity(Environment.CurrentDirectory + "\\AccountCache.xml", ref ListAccountInfo, ref errMsg);
            if (ListAccountInfo == null || ListAccountInfo.Count == 0)
            {
                UserSelectedState = UserOnlineStates[0];
                return;
            }

            IEnumerable<string> ie = ListAccountInfo.OrderByDescending(c => c.LastLoginTime).Select(c => c.ID).Distinct();
            ListID = DataConverter.ConvertToObservableCollection(ie);
            if (ListID != null && ListID.Count > 0)
            {
                SelectedData = ListID[0];
            }
            else
            {
                UserSelectedState = UserOnlineStates[0];
            }

            FindAutoLoginUser();
            //加载系统设置配置信息
            DataConverter.XmlToEntity(Environment.CurrentDirectory + "/SysSettingCache.xml", ref GlobalVariable.systemSetting, ref errMsg);
        }
        /// <summary>
        /// 查询自动登录的用户信息
        /// </summary>
        private void FindAutoLoginUser()
        {
            AccountInfo info = ListAccountInfo.Find(o => o.AutoLogin == true);
            if (GlobalVariable.wherefrom.Length == 0)
            {
                if (info != null)
                {
                    SelectedData = info.ID;
                    IsRememberPwd = info.RememberPwd;
                    IsAutoLogin = info.AutoLogin;
                    if (info.OnLine == (int)GlobalVariable.OnLineStatus.OffLine)
                        info.OnLine = (int)GlobalVariable.OnLineStatus.OnLine;
                    UserSelectedState = UserOnlineStates.FirstOrDefault(m => m.OnlineState == info.OnLine);
                    if (!GlobalVariable.isLoginOut)
                        LoginAction(new object());
                }
            }
        }

        /// <summary>
        /// 保存登录信息
        /// </summary>
        private void SaveAccountInfo()
        {
            string errMsg = string.Empty;
            AccountInfo info = ListAccountInfo.FirstOrDefault(c => c.ID == this.LoginID);
            if (info == null)
            {
                info = new AccountInfo();
                info.IsLogin = true;
                info.ID = this.LoginID;
                info.Password = LoginPwd;
                info.LastLoginTime = DateTime.Now;
                //if (UserSelectedState != null && UserSelectedState.OnlineState != (int)GlobalVariable.OnLineStatus.OnLine)
                //    AntSdkService.AntSdkUpdateCurrentUserState(UserSelectedState.OnlineState, ref errMsg);
                info.OnLine = UserSelectedState?.OnlineState ?? (int)GlobalVariable.OnLineStatus.OnLine;
                GlobalVariable.UserCurrentOnlineState = UserSelectedState?.OnlineState ?? (int)GlobalVariable.OnLineStatus.OnLine;
                info.RememberPwd = IsRememberPwd;
                info.AutoLogin = IsAutoLogin;

                if (info.RememberPwd)
                {
                    //info.Password = pwd.Password;
                    info.Password = DataConverter.MD5Encrypt(LoginPwd, "");
                }
                else
                {
                    info.Password = string.Empty;
                }
                ListAccountInfo.Add(info);
            }
            else
            {
                info.LastLoginTime = DateTime.Now;
                //if (UserSelectedState != null && UserSelectedState.OnlineState != (int)GlobalVariable.OnLineStatus.OnLine)
                //{
                //    AntSdkService.AntSdkUpdateCurrentUserState(UserSelectedState.OnlineState, ref errMsg);
                //}
                GlobalVariable.UserCurrentOnlineState = UserSelectedState?.OnlineState ?? (int)GlobalVariable.OnLineStatus.OnLine;
                info.OnLine = UserSelectedState?.OnlineState ?? (int)GlobalVariable.OnLineStatus.OnLine;
                info.RememberPwd = IsRememberPwd;
                info.AutoLogin = IsAutoLogin;
                info.Password = LoginPwd;
                info.IsLogin = true;
                if (info.RememberPwd)
                {
                    //info.Password = pwd.Password;
                    info.Password = DataConverter.MD5Encrypt(LoginPwd, "");
                }
                else
                {
                    info.Password = string.Empty;
                }
            }
            var installLoginInfo = InstallLoginInfoLst.FirstOrDefault(m => m.ID == this.LoginID);
            if (installLoginInfo == null)
            {
                installLoginInfo = new InstallLoginInfo
                {
                    ID = this.LoginID,
                    IsFirstLogin = false
                };
                GlobalVariable.CurrentUserIsFirstLogin = true;
                InstallLoginInfoLst.Add(installLoginInfo);
                DataConverter.EntityToXml<List<InstallLoginInfo>>(System.Environment.CurrentDirectory + "/InstallLogin.xml", InstallLoginInfoLst, ref errMsg);
            }
            else
            {
                GlobalVariable.CurrentUserIsFirstLogin = installLoginInfo.IsFirstLogin;
            }
            GlobalVariable.CompanyCode = AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode;
            DataConverter.EntityToXml<List<AccountInfo>>(System.Environment.CurrentDirectory + "/AccountCache.xml", ListAccountInfo, ref errMsg);
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        private ActionCommand<Window> _ExitApp;
        public ActionCommand<Window> ExitApp
        {
            get
            {
                if (this._ExitApp == null)
                {
                    this._ExitApp = new ActionCommand<Window>(
                           o =>
                           {
                               ItemExitClick(null, null);
                           });
                }
                return this._ExitApp;
            }
        }
        #endregion
        #region 修改密码新功能
        DispatcherTimer timer = null;

        /// <summary>
        /// 手机号码
        /// </summary>
        private string _textMobileNum = "";
        public string textMobileNum
        {
            get { return _textMobileNum; }
            set
            {
                this._textMobileNum = value;
                RaisePropertyChanged(() => textMobileNum);
            }
        }
        private string _setBtnText = "获取验证码";
        public string setBtnText
        {
            get { return _setBtnText; }
            set
            {
                this._setBtnText = value;
                RaisePropertyChanged(() => setBtnText);
            }
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        private string _textCode = "";
        public string textCode
        {
            get { return _textCode; }
            set
            {
                this._textCode = value;
                RaisePropertyChanged(() => textCode);
            }
        }
        private Visibility _IsForgetPwdUi = Visibility.Hidden;
        /// <summary>
        /// 忘记密码界面
        /// </summary>
        public Visibility IsForgetPwdUi
        {
            get { return this._IsForgetPwdUi; }
            set
            {
                this._IsForgetPwdUi = value;
                RaisePropertyChanged(() => IsForgetPwdUi);
            }
        }
        private Visibility _IsSetPwdUi = Visibility.Hidden;
        /// <summary>
        /// 设置密码界面
        /// </summary>
        public Visibility IsSetPwdUi
        {
            get { return this._IsSetPwdUi; }
            set
            {
                this._IsSetPwdUi = value;
                RaisePropertyChanged(() => IsSetPwdUi);
            }
        }
        public ICommand btnCommandForget
        {
            get
            {

                return new DelegateCommand(() =>
                {
                    IsForgetPwdUi = Visibility.Visible;
                });
            }
        }

        public ICommand btnCommandBackFirst
        {
            get
            {
                return new DelegateCommand<System.Windows.Controls.Button>((obj) =>
                {
                    isShowTextBlock = Visibility.Hidden;
                    System.Windows.Controls.Button button = (obj as System.Windows.Controls.Button);
                    if (button.Name == "secondBack")
                    {
                        IsForgetPwdUi = Visibility.Hidden;
                        IsSetPwdUi = Visibility.Hidden;
                    }
                    else
                    {
                        IsForgetPwdUi = Visibility.Hidden;
                    }
                });
            }
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        public ICommand btnCommandGetCodeValue
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (string.IsNullOrEmpty(textMobileNum.Trim()))
                    {
                        //提示输入手机号
                        showToastMethod("请输入手机号!");
                        return;
                    }
                    if (!RegexHelper.RegexHelper.IsMobileNum(textMobileNum))
                    {
                        //提示手机号码格式不正确
                        showToastMethod("手机号码不正确!");
                        return;
                    }
                    int errorCode = 0;
                    string errorMsg = "";
                    if (isClick)
                    {
                        bool result = SDK.AntSdk.AntSdkService.GetVerifyCodeMethod(textMobileNum, ref errorCode, ref errorMsg);
                        if (result)
                        {
                            isClick = false;
                            timer = new DispatcherTimer();
                            timer.Interval = TimeSpan.FromMilliseconds(1000);
                            timer.Tick += Timer_Tick1;
                            timer.Start();
                        }
                        else
                        {
                            if (errorCode == 10020)
                            {
                                showToastMethod("该用户不存在!");
                                return;
                            }
                            if (errorCode == 1012)
                            {
                                showToastMethod("同一手机号短信发送不能超过5条短信!");
                            }
                            else
                            {
                                showToastMethod(errorMsg);
                            }
                        }
                    }
                });
            }
        }
        private int count = 60;
        private bool isClick = true;
        private void Timer_Tick1(object sender, EventArgs e)
        {
            --count;
            if (count > 0)
            {
                setBtnText = count + "s";
            }
            else
            {
                isClick = true;
                count = 60;
                timer.Stop();
                setBtnText = "获取验证码";
            }

        }
        private string _firstPwd = "";
        /// <summary>
        /// 密码1
        /// </summary>
        public string firstPwd
        {
            get { return this._firstPwd; }
            set
            {
                this._firstPwd = value;
                RaisePropertyChanged(() => firstPwd);
            }
        }
        private string _secondPwd = "";
        /// <summary>
        /// 密码2
        /// </summary>
        public string secondPwd
        {
            get { return this._secondPwd; }
            set
            {
                this._secondPwd = value;
                RaisePropertyChanged(() => secondPwd);
            }
        }
        string data = "";
        /// <summary>
        /// 下一步
        /// </summary>
        public ICommand btnCommandNext
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (string.IsNullOrEmpty(textMobileNum.Trim()))
                    {
                        //提示输入手机号
                        showToastMethod("请输入手机号!");
                        return;
                    }
                    if (!RegexHelper.RegexHelper.IsMobileNum(textMobileNum))
                    {
                        //提示手机号码格式不正确
                        showToastMethod("手机号码不正确!");
                        return;
                    }
                    if (string.IsNullOrEmpty(textCode.Trim()))
                    {
                        //提示输入验证码
                        showToastMethod("请输入验证码!");
                        return;
                    }
                    int errorCode = 0;
                    string errorMsg = "";
                    //调用获取验证码接口
                    AntSdkSendVerifyCodeInput input = new AntSdkSendVerifyCodeInput();

                    input.mobile = textMobileNum.Trim();
                    input.validateCode = textCode.Trim();
                    bool resultSendCode = SDK.AntSdk.AntSdkService.SentVerifyCodeMethod(input, ref errorCode, ref errorMsg, ref data);
                    if (resultSendCode)
                    {
                        //设置密码
                        IsSetPwdUi = Visibility.Visible;
                    }
                    else
                    {
                        showToastMethod(errorMsg);
                    }
                });
            }
        }
        /// <summary>
        /// 进入七讯
        /// </summary>
        public ICommand btnEnterCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (string.IsNullOrEmpty(firstPwd.Trim()) || string.IsNullOrEmpty(secondPwd.Trim()))
                    {
                        showToastMethod("请输入密码!");
                        return;
                    }
                    if (firstPwd.Trim() != secondPwd.Trim())
                    {
                        showToastMethod("两次输入的密码不一致!");
                        return;
                    }
                    //重置密码
                    int errorCode = 0;
                    string errorMsg = "";
                    AntSdkResetPassWoldInput input = new AntSdkResetPassWoldInput();
                    input.mobileKey = data;
                    input.password = secondPwd.Trim();
                    bool result = SDK.AntSdk.AntSdkService.ResetPassWordMethod(input, ref errorCode, ref errorMsg);
                    if (result)
                    {
                        //影藏界面
                        IsSetPwdUi = Visibility.Collapsed;
                        IsForgetPwdUi = Visibility.Collapsed;


                        this.LoginID = this.textMobileNum;
                        this.LoginPwd = this.secondPwd;
                        //重置 定时器、倒计时文字、释放点击锁、清除手机号、验证码、设置密码1、2清除
                        timer?.Stop();
                        setBtnText = "获取验证码";
                        count = 60;
                        textMobileNum = "";
                        textCode = "";
                        isClick = true;
                        firstPwd = "";
                        secondPwd = "";
                        LoginAction(new object());
                    }
                    else
                    {
                        showToastMethod(errorMsg);
                    }
                });
            }
        }
        private Visibility _isShowTextBlock = Visibility.Hidden;
        /// <summary>
        /// 设置密码界面
        /// </summary>
        public Visibility isShowTextBlock
        {
            get { return this._isShowTextBlock; }
            set
            {
                this._isShowTextBlock = value;
                RaisePropertyChanged(() => isShowTextBlock);
            }
        }
        #region 防刷验证码
        private string _identifyingCode = string.Empty;
        /// <summary>
        /// 防刷验证码
        /// </summary>
        public string IdentifyingCode
        {
            get { return this._identifyingCode; }
            set
            {
                this._identifyingCode = value;
                RaisePropertyChanged(() => IdentifyingCode);
            }
        }

        private bool _isIdentifyingCode;
        /// <summary>
        /// 是否需要进行防刷验证
        /// </summary>
        public bool IsIdentifyingCode
        {
            get { return _isIdentifyingCode; }
            set
            {
                _isIdentifyingCode = value;
                RaisePropertyChanged(() => IsIdentifyingCode);
            }
        }
        private BitmapImage _identifyingCodeBitmap;
        /// <summary>
        /// 验证码生成图
        /// </summary>
        public BitmapImage IdentifyingCodeBitmap
        {
            get { return _identifyingCodeBitmap; }
            set
            {
                _identifyingCodeBitmap = value;
                RaisePropertyChanged(() => IdentifyingCodeBitmap);
            }
        }
        /// <summary>
        /// 刷新验证码图片
        /// </summary>
        public ICommand IdentifyingImageCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    SetVerifyCodeImage();
                });
            }
        }
        #endregion
        /// <summary>
        /// tip提示文字
        /// </summary>
        private string _showText = "";
        public string showText
        {
            get { return _showText; }
            set
            {
                this._showText = value;
                RaisePropertyChanged(() => showText);
            }
        }
        DispatcherTimer IsShowTip = new DispatcherTimer();
        /// <summary>
        /// toast提示
        /// </summary>
        /// <param name="tipsMsg">提示内容</param>
        public void showToastMethod(string tipsMsg)
        {
            if (string.IsNullOrEmpty(tipsMsg))
            {
                return;
            }
            showText = tipsMsg;
            isShowTextBlock = Visibility.Visible;
            IsShowTip.Interval = TimeSpan.FromMilliseconds(2000);
            IsShowTip.Tick += IsShowTip_Tick; ;
            IsShowTip.Start();

        }
        private void IsShowTip_Tick(object sender, EventArgs e)
        {
            IsShowTip.Stop();
            isShowTextBlock = Visibility.Hidden;
        }
        #endregion
    }
}
public class ProcessParam
{
    public volatile bool IsLoginValidate = true;
    public int SendProcessCount = 0;
    public volatile int ReceiveProcessCount = 0;
}