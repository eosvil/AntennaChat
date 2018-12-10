using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Resource;
using AntennaChat.ViewModel.Contacts;
using AntennaChat.ViewModel.Setting;
using AntennaChat.ViewModel.Talk;
using AntennaChat.ViewModel.Update;
using AntennaChat.Views;
using AntennaChat.Views.Talk;
using AntennaChat.Views.Update;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AntennaChat.Helper;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using Image = System.Windows.Controls.Image;
using SDK.AntSdk.AntSdkDAL;
using AntennaChat.Views.AudioVideo;
using AntennaChat.ViewModel.AudioVideo;

namespace AntennaChat.ViewModel
{
    public class MainWindowViewModel : WindowBaseViewModel
    {
        public Window win;

        /// <summary>
        /// 主展示容器
        /// </summary>
        public static Grid MainExhibitControl;

        private SessionListViewModel _SessionListViewModel;
        public GroupListViewModel _GroupListViewModel;
        private ContactListViewModel _ContactListViewModel;
        private QueryContactListViewModel _QueryContactList;
        readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private DispatcherTimer searchTimer = new DispatcherTimer();
        //bool IsCloseByTaskbar = true;//是否是通过任务栏图标右键关闭
        private KeyboardHookLib _keyboardHook = null;
        public List<AntSdkGroupInfo> groupList = null;
        
        public bool IsStopSdkService;
        //public bool IsStopSdkServiceForced;
        #region 构造器/窗体加载

        public MainWindowViewModel()
        {





            //searchTimer.Interval = new TimeSpan(0, 0, 0, 0, 600);
            //searchTimer.Tick += searchTimer_Tick;
            //GlobalSearchCommand=new DefaultCommand(GlobalSearchOperation);
        }

        /// <summary>
        /// 初始化主窗体
        /// </summary>
        public void InitMainVM()
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                switch (GlobalVariable.UserCurrentOnlineState)
                {
                    case (int)GlobalVariable.OnLineStatus.OnLine:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconOnLine;
                        break;
                    case (int)GlobalVariable.OnLineStatus.Busy:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconBusy;
                        break;
                    case (int)GlobalVariable.OnLineStatus.Leave:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconLeave;
                        break;
                }
                if (!string.IsNullOrWhiteSpace(AntSdkService.AntSdkCurrentUserInfo.picture))
                {
                    var index = AntSdkService.AntSdkCurrentUserInfo.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                    var fileNameIndex = AntSdkService.AntSdkCurrentUserInfo.picture.LastIndexOf(".", StringComparison.Ordinal);
                    var fileName = AntSdkService.AntSdkCurrentUserInfo.picture.Substring(index, fileNameIndex - index);
                    string strUrl = AntSdkService.AntSdkCurrentUserInfo.picture.Replace(fileName, fileName + "_80x80");
                    // this.Photo = publicMethod.IsUrlRegex(strUrl) ? strUrl : GlobalVariable.DefaultImage.UserHeadDefaultImage;

                    HeadBitmapImage(strUrl, AntSdkService.AntSdkCurrentUserInfo.picture);
                    //var tempUserHeadImages = GlobalVariable.ContactHeadImage.UserHeadImages.ToList();
                    //var userImage = tempUserHeadImages.FirstOrDefault(
                    //        m => m.UserID == AntSdkService.AntSdkCurrentUserInfo.userId);
                    //HeadBitmapImage(userImage != null && File.Exists(userImage.Url)
                    //        ? userImage.Url
                    //        : AntSdkService.AntSdkCurrentUserInfo.picture, AntSdkService.AntSdkCurrentUserInfo.picture);
                    //this.HeadPic = AntSdkService.AntSdkCurrentUserInfo.picture;
                }
                else
                {
                    HeadBitmapImage("pack://application:,,,/AntennaChat;Component/Images/198-头像.png", "");
                    //this.HeadPic = "pack://application:,,,/AntennaChat;Component/Images/198-头像.png";
                }
            });


            NotifyToolTip = "七讯：" + AntSdkService.AntSdkCurrentUserInfo.userName;
            //群组列表实例化
            _GroupListViewModel.InitGroupVM();
            //最近消息列表实例化
            _SessionListViewModel.IninSessionVM(this);
            if (InitMqttService())
            {
                //AsyncHandler.CallFuncWithUI<List<AntSdkGroupInfo>>(
                //    System.Windows.Application.Current.Dispatcher,
                //    () =>
                //    {
                _GroupListViewModel.ResetGroupList();

                //    return _GroupListViewModel.GroupInfos;
                //},
                //(ex, datas) =>
                //{
                //});
                ClearMainViewRightPart();
                manualResetEvent.Set();
                StartWaitingTimer();
            }
            SessionListViewModel.IsShowRedCricleEventHandler += SessionListViewModel_IsShowRedCricleEventHandler;
            _ContactListViewModel = new ContactListViewModel(GlobalVariable.ContactInfoViewContainer.ContactListView,
                this);
            //_SessionListViewModel.ButtonClickEvent += _SessionListViewModel_ButtonClickEvent;
            ContactInfoViewModel.MouseDoubleClickEvent += _SessionListViewModel_ButtonClickEvent;
            GroupInfoViewModel.MouseDoubleClickEvent += _SessionListViewModel_ButtonClickEvent;
            GroupMemberViewModel.MouseDoubleClickEvent += _SessionListViewModel_ButtonClickEvent;
            //StructureDetailsViewModel.SendMsgHandler += StructureDetailsViewModel_SendMsgHandler;
            MassMsgSentViewModel.SendMassMsgEvent += SendMassMsgEvent;


        }

        #region 任务栏通知

        private string _iconUrl;

        /// <summary>
        /// 任务图标
        /// </summary>
        public string IconUrl
        {
            get { return _iconUrl; }
            set
            {
                _iconUrl = value;
                RaisePropertyChanged(() => IconUrl);
            }
        }

        private string _notifyToolTip;

        /// <summary>
        /// 任务栏鼠标移入提示
        /// </summary>
        public string NotifyToolTip
        {
            get { return _notifyToolTip; }
            set
            {
                _notifyToolTip = value;
                RaisePropertyChanged(() => NotifyToolTip);
            }
        }

        private ICommand _commandOpenMainWindow;

        /// <summary>
        /// 打开主窗体
        /// </summary>
        public ICommand CommandOpenMainWindow
        {
            get
            {
                _commandOpenMainWindow = new DefaultCommand(o =>
                {
                    LogHelper.WriteError("OpenMainWindow消息未读数=======================" + _SessionListViewModel.GetTotelUnreadCount());
                    QPan_OpenFromTuoPan();
                });
                return _commandOpenMainWindow;
            }
        }

        private ICommand _commandLogoutSystem;

        /// <summary>
        /// 注销
        /// </summary>
        public ICommand CommandLogoutSystem
        {
            get
            {
                _commandLogoutSystem = new DefaultCommand(o =>
                {
                    LoginOutMethod(false);
                });
                return _commandLogoutSystem;
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
                    LoginOutMethod(true);
                });
                return _commandExitMainWindow;
            }
        }

        #endregion

        private void StructureDetailsViewModel_SendMsgHandler(string arg1, string userId)
        {
            SessionSelected = true;
            _SessionListViewModel.AddContactSession(userId, true);
        }

        /// <summary>
        /// 版本更新
        /// </summary>
        /// <param name="versionUpdate"></param>
        private void MessageMonitor_VersionUpdatedToReceive(AntSdkReceivedOtherMsg.VersionHardUpdate versionUpdate)
        {
            update(versionUpdate);
        }

        private void SessionListViewModel_IsShowRedCricleEventHandler(object sender, EventArgs e)
        {
            IsUnreadMsg = false;
        }

        /// <summary>
        /// 头像图片处理
        /// </summary>
        /// <param name="url">图片路径</param>
        private void HeadBitmapImage(string imagePath, string imageUrl)
        {
            this.HeadPic = string.IsNullOrWhiteSpace(imagePath)
                ? "pack://application:,,,/AntennaChat;Component/Images/198-头像.png"
                : imagePath;
            AntSdkService.AntSdkCurrentUserInfo.picture = string.IsNullOrEmpty(imageUrl) ? this.HeadPic : imageUrl;
        }

        #endregion


        #region 属性

        private string _HeadPic;

        /// <summary>
        /// 头像地址
        /// </summary>
        public string HeadPic
        {
            get { return this._HeadPic; }
            set
            {
                this._HeadPic = value;
                RaisePropertyChanged(() => HeadPic);
            }
        }

        private BitmapImage _headPicBitmapImage = new BitmapImage();

        /// <summary>
        /// 头像
        /// </summary>
        public BitmapImage HeadPicBitmapImage
        {
            get { return _headPicBitmapImage; }
            set
            {
                this._headPicBitmapImage = value;
                RaisePropertyChanged(() => HeadPicBitmapImage);
            }
        }

        private bool _PopSetOpen = false;

        /// <summary>
        /// 设置弹窗是否打开
        /// </summary>
        public bool PopSetOpen
        {
            get { return this._PopSetOpen; }
            set
            {
                this._PopSetOpen = value;
                RaisePropertyChanged(() => PopSetOpen);
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get { return AntSdkService.AntSdkCurrentUserInfo.userName; }
            set
            {
                AntSdkService.AntSdkCurrentUserInfo.userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserId
        {
            get { return AntSdkService.AntSdkCurrentUserInfo.loginName; }
            set
            {
                AntSdkService.AntSdkCurrentUserInfo.loginName = value;
                RaisePropertyChanged(() => UserId);
            }
        }

        /// <summary>
        /// 在线状态图片
        /// </summary>
        private string _StatusImage = "pack://application:,,,/AntennaChat;Component/Images/OnLineStatus/在线.png";

        public string StatusImage
        {
            get { return _StatusImage; }
            set
            {
                _StatusImage = value;
                RaisePropertyChanged(() => StatusImage);
            }
        }

        /// <summary>
        /// 在线状态文字
        /// </summary>
        private string _StatusText = "在线";

        public string StatusText
        {
            get { return _StatusText; }
            set
            {
                _StatusText = value;
                RaisePropertyChanged(() => StatusText);
            }
        }

        /// <summary>
        /// 断网提示显示
        /// </summary>
        private string _isShowNetwork = "Hidden";

        public string isShowNetwork
        {
            get { return _isShowNetwork; }
            set
            {
                _isShowNetwork = value;
                RaisePropertyChanged(() => isShowNetwork);
            }
        }

        private bool _isUnreadMsg;

        /// <summary>
        /// 是否有未读消息
        /// </summary>
        public bool IsUnreadMsg
        {
            get { return _isUnreadMsg; }
            set
            {
                _isUnreadMsg = value;
                RaisePropertyChanged(() => IsUnreadMsg);
            }
        }

        //private bool _MenuIsOpen = false;
        //public bool MenuIsOpen
        //{
        //    get { return _MenuIsOpen; }
        //    set
        //    {
        //        _MenuIsOpen = value;
        //        RaisePropertyChanged(() => MenuIsOpen);
        //    }
        //}

        private string _searchContactName;

        /// <summary>
        /// 讨论组成员搜索关键字
        /// </summary>
        public string SearchContactName
        {
            get { return _searchContactName; }
            set
            {
                this._searchContactName = value;
                RaisePropertyChanged(() => SearchContactName);
            }
        }

        private bool _isQueryTxtFocus;

        /// <summary>
        /// 查询列表获得焦点
        /// </summary>
        public bool IsQueryTxtFocus
        {
            get { return _isQueryTxtFocus; }
            set
            {
                _isQueryTxtFocus = value;
                RaisePropertyChanged(() => IsQueryTxtFocus);

            }
        }

        /// <summary>
        /// 消息选中
        /// </summary>    
        private bool _SessionSelected = true;

        public bool SessionSelected
        {
            get { return _SessionSelected; }
            set
            {
                _SessionSelected = value;
                if (_SessionSelected)
                {
                    this.SecondPartViewModel = _SessionListViewModel;
                    if (IsUnreadMsg)
                        IsUnreadMsg = false;
                    SearchContactName = string.Empty;
                }
                RaisePropertyChanged(() => SessionSelected);
            }
        }

        /// <summary>
        /// 讨论组选中
        /// </summary>
        private bool _GroupSelected = false;

        public bool GroupSelected
        {
            get { return _GroupSelected; }
            set
            {
                _GroupSelected = value;
                if (_GroupSelected)
                {
                    if (_GroupListViewModel == null)
                    {
                        _GroupListViewModel = new GroupListViewModel();
                    }
                    else
                    {
                        if (_GroupListViewModel.GroupInfoList.Count != 0)
                        {
                            _GroupListViewModel.LoadGroupListData();
                        }
                    }

                    this.SecondPartViewModel = _GroupListViewModel;
                }
                RaisePropertyChanged(() => GroupSelected);
            }
        }

        /// <summary>
        /// 联系人选中
        /// </summary>
        private bool _ContactSelected = false;

        public bool ContactSelected
        {
            get { return _ContactSelected; }
            set
            {
                _ContactSelected = value;
                if (_ContactSelected)
                    this.SecondPartViewModel = _ContactListViewModel;
                RaisePropertyChanged(() => ContactSelected);
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
            get { return _userSelectedState; }
            set
            {

                _userSelectedState = value;
                RaisePropertyChanged(() => UserSelectedState);
            }
        }

        private ICommand _stateChangedCommand;

        /// <summary>
        /// 用户在线状态切换
        /// </summary>
        public ICommand StateChangedCommand
        {
            get
            {
                _stateChangedCommand = new DefaultCommand(o =>
                {
                    int state = 0;
                    state = o == null ? UserSelectedState.OnlineState : int.Parse(o.ToString());
                    if (AntSdkService.AntSdkIsConnected)
                    {

                        var errCode = 0;
                        string errMsg = string.Empty;
                        var isResult = AntSdkService.AntSdkUpdateCurrentUserState(state,
                            ref errCode, ref errMsg);
                        if (!isResult)
                        {
                            return;
                        }
                        UserSelectedState = UserOnlineStates.FirstOrDefault(m => m.OnlineState == state);
                        GlobalVariable.UserCurrentOnlineState = state;
                    }
                    else
                    {
                        UserSelectedState = new UserOnlineState
                        {
                            OnlineState = (int)GlobalVariable.OnLineStatus.OffLine,
                            StateContent = "",
                            StateImage = ""
                        };
                    }
                });
                return _stateChangedCommand;
            }
        }

        //private ICommand _ContactCommand;
        //public ICommand ContactCommand
        //{
        //    get
        //    {
        //        if (this._ContactCommand == null)
        //        {
        //            this._ContactCommand = new DefaultCommand(
        //                      o =>
        //                      {
        //                          this.ViewModel = _ContactListViewModel;
        //                      });
        //        }
        //        return this._ContactCommand;
        //    }
        //}
        private object _SecondPartViewModel;

        /// <summary>
        /// 要绑定和切换的第二部分ViewModel
        /// </summary>
        public object SecondPartViewModel
        {
            get { return _SecondPartViewModel; }
            set
            {
                if (_SecondPartViewModel == value)
                {
                    return;
                }
                _SecondPartViewModel = value;
                RaisePropertyChanged(() => SecondPartViewModel);
            }
        }

        #endregion

        #region 命令

        private ActionCommand<Window> _Loaded;

        public ActionCommand<Window> LoadedCommand
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
        /// 窗体加载Action
        /// </summary>
        /// <param name="w"></param>
        private void LoadAction(Window w)
        {
            InitMainVM();
            GlobalVariable.isLoginOut = false;
            win = w;
            WindowInteropHelper wndHelper = new WindowInteropHelper(w);
            GlobalVariable.winHandle = wndHelper.Handle;
            win.Drop += Window_Drop;
            //安装勾子
            _keyboardHook = new KeyboardHookLib();
            _keyboardHook.InstallHook(OnKeyPress);
        }

        private ActionCommand<Window> _WindowStateChanged;

        public ActionCommand<Window> WindowStateChanged
        {
            get
            {
                if (this._WindowStateChanged == null)
                {
                    this._WindowStateChanged = new ActionCommand<Window>(o =>
                    {
                        if (o != null)
                        {
                            if (o.WindowState == WindowState.Minimized)
                            {
                                GlobalVariable.isMinimized = true;
                                if (_SessionListViewModel != null && _SessionListViewModel.GetTotelUnreadCount() > 0)
                                    ToolbarFlash.Flash(GlobalVariable.winHandle, 2);
                            }
                            else
                            {
                                if (_SessionListViewModel != null)
                                {
                                    _SessionListViewModel.ScrollToVerticalOffset();
                                    _SessionListViewModel.curSessionUnreadMsg = false;
                                }
                                if (GlobalVariable.isMinimized && _SessionListViewModel != null)
                                {
                                    SessionInfoViewModel vm =
                                        _SessionListViewModel.SessionControlList.FirstOrDefault(c => c.IsMouseClick);
                                    if (vm?.UnreadCount > 0)
                                    {
                                        PublicMessageFunction.SendChatMsgReceipt(vm.SessionId, vm.LastChatIndex,
                                            vm.MsgType, AntSdkReceiptType.ReadReceipt);
                                        vm.UnreadCount = 0;
                                    }
                                }
                                GlobalVariable.isMinimized = false;
                                base.IsMaxWin = false;
                            }
                            if (o.WindowState == WindowState.Maximized)
                            {
                                base.IsMaxWin = true;
                                MainWindowMargin = 0;
                            }
                        }
                    });
                }
                return this._WindowStateChanged;
            }
        }

        public bool isLoad = false;
        private ActionCommand<Grid> _GridLoadedCommand;

        public ActionCommand<Grid> GridLoadedCommand
        {
            get
            {
                if (this._GridLoadedCommand == null)
                {
                    this._GridLoadedCommand = new ActionCommand<Grid>(o =>
                    {

                        if (o != null)
                        {
                            if (MainExhibitControl == null)
                            {
                                MainExhibitControl = o;
                            }
                            else
                            {
                                //查找当前会话
                                var currentSession =
                                    _SessionListViewModel._SessionControlList.FirstOrDefault(m => m.IsMouseClick);
                                if (currentSession != null)
                                {
                                    //删除当前会话
                                    currentSession.IsRemoveLoaclSession = false;
                                    _SessionListViewModel.DeleteSession(currentSession, null);
                                    //群聊为false  单聊为true
                                    switch (currentSession._SessionType)
                                    {
                                        case GlobalVariable.SessionType.SingleChat:
                                            var userId = currentSession.AntSdkContact_User.userId;
                                            _SessionListViewModel.AddContactSession(userId, false);
                                            break;
                                        case GlobalVariable.SessionType.GroupChat:
                                            var groupInfo = currentSession.GroupInfo;
                                            _SessionListViewModel.AddGroupSession(groupInfo, false);
                                            break;
                                        case GlobalVariable.SessionType.MassAssistant:
                                        case GlobalVariable.SessionType.AttendanceAssistant:
                                            _SessionListViewModel.AddAssistantSession(currentSession.SessionId);
                                            break;
                                    }
                                }
                            }

                            if (isLoad == false)
                            {
                                ClearMainViewRightPart();
                                isLoad = true;
                            }
                        }
                    });
                }
                return this._GridLoadedCommand;
            }
        }

        private ICommand _globalSearchCommand;
        private int _flag = 0;

        /// <summary>
        /// 全局搜索命令
        /// </summary>
        public ICommand GlobalSearchCommand
        {
            get
            {

                this._globalSearchCommand = new DefaultCommand(o =>
                {
                    if (o is Key)
                    {
                        var key = (Key)o;
                        if (key != Key.Up && key != Key.Down && key != Key.Enter)
                        {
                            if (_QueryContactList != null)
                                _QueryContactList.IsQueryListFocus = false;
                        }
                    }
                });
                return _globalSearchCommand;
            }
        }

        /// <summary>
        /// 搜索条件变更
        /// </summary>   
        private ICommand _queryConditionChanged;

        public ICommand QueryConditionChanged
        {
            get
            {
                if (this._queryConditionChanged == null)
                {
                    this._queryConditionChanged = new DefaultCommand(
                        o =>
                        {

                            if (string.IsNullOrEmpty(SearchContactName))
                            {
                                if (_QueryContactList != null && _QueryContactList.QueryContactList?.Count > 0)
                                {
                                    _QueryContactList.QueryContactList.Clear();
                                    _QueryContactList.QueryContactList = null;
                                    _QueryContactList = null;
                                }

                                if (SessionSelected)
                                {
                                    this.SecondPartViewModel = _SessionListViewModel;
                                }
                                else if (GroupSelected)
                                {
                                    this.SecondPartViewModel = _GroupListViewModel;
                                }
                                else if (ContactSelected)
                                {
                                    this.SecondPartViewModel = _ContactListViewModel;
                                }
                            }
                            else
                            {
                                if (_QueryContactList == null)
                                {
                                    _QueryContactList = new QueryContactListViewModel(SearchContactName,
                                        GlobalVariable.ContactInfoViewContainer.ContactListView, _GroupListViewModel);
                                }
                                else
                                {

                                    _QueryContactList.QueryCondition(SearchContactName);
                                }
                                //if (_QueryContactList != null && _QueryContactList.QueryContactList.Any())
                                //    _QueryContactList.IsQueryListFocus = true;
                                this.SecondPartViewModel = _QueryContactList;
                                _QueryContactList.SelectIndex = 0;

                            }
                        });
                }
                return this._queryConditionChanged;
            }
        }

        /// <summary>
        ///主展示区显示各（公司、部分、群组、个人）详情
        /// </summary>
        public static void GoStructureDetail(ContentControl control)
        {
            if (control != null)
            {

                MainExhibitControl.Children.Clear();
                MainExhibitControl.Children.Add(control);
            }
        }

        /// <summary>
        /// 清空右侧对话框
        /// </summary>
        public static void ClearMainViewRightPart()
        {
            if (MainExhibitControl == null) return;
            MainExhibitControl.Children.Clear();
            Image backGroundimg = new Image()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 80,
                Height = 80
            };
            backGroundimg.Source =
                new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Images/logoIcon/空白页-默认图标.png",
                    UriKind.RelativeOrAbsolute));
            MainExhibitControl.Children.Add(backGroundimg);
        }

        /// <summary>
        /// 搜索框上下移动选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TxtSearch_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                _QueryContactList?.ChangeSelectIndex(e.Key);
            }
            if (e.Key == Key.Enter)
            {
                _QueryContactList?.OpenContactCommand.Execute(null);
            }
        }

        public void MainWindowView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 && e.Key == Key.A)
            //{
            //    ImageHandle.CutImg();
            //    e.Handled = true;
            //}
        }

        public void OnKeyPress(KeyboardHookLib.HookStruct hookStruct, out bool handle)
        {
            handle = false; //预设不拦截任何键
            if (GlobalVariable.isCutShow) return;
            System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)hookStruct.vkCode;
            //默认
            if (GlobalVariable.systemSetting == null || string.IsNullOrEmpty(GlobalVariable.systemSetting.KeyShortcuts))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 &&
                    key == System.Windows.Forms.Keys.Q)
                {
                    _SessionListViewModel?.TalkViewCutImg();
                }
            }
            //自定义
            else
            {
                int keyNum = Convert.ToInt32(GlobalVariable.systemSetting.KeyShortcuts);
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 &&
                    hookStruct.vkCode == keyNum)
                {
                    _SessionListViewModel?.TalkViewCutImg();
                }
            }
        }

        /// <summary>
        /// 头像点击
        /// </summary>   
        private ICommand _HeadCommand;

        public ICommand HeadCommand
        {
            get
            {
                if (this._HeadCommand == null)
                {
                    this._HeadCommand = new DefaultCommand(
                        o =>
                        {
                            Views.Setting.Win_ProfileView win = new Views.Setting.Win_ProfileView();
                            win.ShowInTaskbar = false;
                            Win_ProfileViewModel model = new Win_ProfileViewModel(win.Close);
                            model.CloseWinEvent += Model_CloseWinEvent;
                            model.QueryInfoHandler += Model_QueryInfoHandler;
                            model.ChangePasswordSuccessEvent += Model_ChangePasswordSuccessEvent;
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.ShowDialog();
                        });
                }
                return this._HeadCommand;
            }
        }

        private void Model_QueryInfoHandler(AntSdkReceivedUserMsg.Modify modify)
        {
            HandleSysUerMsg_UpdateUserInfo(modify);
        }

        /// <summary>
        /// 修改密码成功 注销登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_ChangePasswordSuccessEvent(object sender, EventArgs e)
        {
            CloseExitVerify();
            LoginOutMethod(false, true);
        }

        /// <summary>
        /// 点击创建讨论组
        /// </summary>   
        private ICommand _CreateGroupCommand;

        public ICommand CreateGroupCommand
        {
            get
            {
                if (this._CreateGroupCommand == null)
                {
                    this._CreateGroupCommand = new DefaultCommand(
                        o =>
                        {
                            Views.Contacts.GroupEditWindowView win = new Views.Contacts.GroupEditWindowView();
                            win.ShowInTaskbar = false;
                            List<string> memberIds = new List<string>();
                            memberIds.Add(AntSdkService.AntSdkLoginOutput.userId);
                            if (
                                AntSdkService.AntSdkListContactsEntity.users.Exists(
                                    m => m.userId == AntSdkService.AntSdkCurrentUserInfo.robotId))
                                memberIds.Add(AntSdkService.AntSdkCurrentUserInfo.robotId);
                            GroupEditWindowViewModel model = new GroupEditWindowViewModel(win.Close, memberIds);
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.ShowDialog();
                            if (_GroupListViewModel != null && model.CreateGroupOutput != null)
                            {
                                _GroupListViewModel.AddGroupAndClick(model.CreateGroupOutput);
                            }
                        });
                }
                return this._CreateGroupCommand;
            }
        }

        /// <summary>
        /// 清空搜索框
        /// </summary>
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
                            SearchContactName = string.Empty;
                        });
                }
                return this._DeleteCommand;
            }
        }

        private void Model_CloseWinEvent()
        {
            //HeadBitmapImage(AntSdkService.AntSdkCurrentUserInfo.picture, "");
        }



        /// <summary>
        /// 打开设置弹窗
        /// </summary>
        private ICommand _OpenSettingCommand;

        public ICommand OpenSettingCommand
        {
            get
            {
                if (this._OpenSettingCommand == null)
                {
                    this._OpenSettingCommand = new DefaultCommand(
                        o =>
                        {
                            PopSetOpen = true;
                        });
                }
                return this._OpenSettingCommand;
            }
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        private ActionCommand<Window> _LoginOut;

        public ActionCommand<Window> LoginOut
        {
            get
            {
                if (this._LoginOut == null)
                {
                    this._LoginOut = new ActionCommand<Window>(
                        o =>
                        {
                            LoginOutMethod(false);
                        });
                }
                return this._LoginOut;
            }
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        private ICommand _settingCommand;

        public ICommand SettingCommand
        {
            get
            {
                if (this._settingCommand == null)
                {
                    this._settingCommand = new DefaultCommand(
                        o =>
                        {
                            var profileView = new Views.Setting.Win_ProfileView { ShowInTaskbar = false };
                            var model = new Win_ProfileViewModel(profileView.Close);
                            model.CloseWinEvent += Model_CloseWinEvent;
                            model.ChangePasswordSuccessEvent += Model_ChangePasswordSuccessEvent;
                            model.QueryInfoHandler += Model_QueryInfoHandler;
                            profileView.DataContext = model;
                            profileView.Owner = Win32.GetTopWindow();
                            profileView.ShowDialog();
                        });
                }
                return this._settingCommand;
            }
        }

        /// <summary>
        /// 意见反馈
        /// </summary>
        private ICommand _SuggestionCommand;

        public ICommand SuggestionCommand
        {
            get
            {
                if (this._SuggestionCommand == null)
                {
                    this._SuggestionCommand = new DefaultCommand(
                        o =>
                        {
                            PictureViewerView win = new PictureViewerView();
                            PictureViewerViewModel model = new PictureViewerViewModel();
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.Show();
                        });
                }
                return this._SuggestionCommand;
            }
        }

        /// <summary>
        /// 菜单点击
        /// </summary>
        private ICommand _MenuItemClick;

        public ICommand MenuItemClick
        {
            get
            {
                if (this._MenuItemClick == null)
                {
                    this._MenuItemClick = new DefaultCommand(o =>
                    {
                        string status = (string)o;
                        StatusText = status;
                        switch (status)
                        {
                            #region

                            case "在线":
                                {
                                    UpdateOnLineStatus(GlobalVariable.OnLineStatus.OnLine);
                                    StatusImage = "pack://application:,,,/AntennaChat;Component/Images/OnLineStatus/在线.png";
                                    break;
                                }
                            case "忙碌":
                                {
                                    UpdateOnLineStatus(GlobalVariable.OnLineStatus.Busy);
                                    StatusImage = "pack://application:,,,/AntennaChat;Component/Images/OnLineStatus/忙碌.png";
                                    break;
                                }
                            case "离开":
                                {
                                    UpdateOnLineStatus(GlobalVariable.OnLineStatus.Leave);
                                    StatusImage = "pack://application:,,,/AntennaChat;Component/Images/OnLineStatus/离开.png";
                                    break;
                                }
                            case "离线":
                                {
                                    UpdateOnLineStatus(GlobalVariable.OnLineStatus.OffLine);
                                    StatusImage = "pack://application:,,,/AntennaChat;Component/Images/OnLineStatus/离线.png";
                                    break;
                                }
                            default:
                                break;

                                #endregion
                        }
                    });
                }
                return this._MenuItemClick;
            }
        }

        private StackPanel panel;
        private ActionCommand<StackPanel> _StackPanelInitialized;

        public ActionCommand<StackPanel> StackPanelInitialized
        {
            get
            {
                if (this._StackPanelInitialized == null)
                {
                    this._StackPanelInitialized = new ActionCommand<StackPanel>(
                        o =>
                        {
                            panel = o;
                            o.ContextMenu = null;
                        });
                }
                return this._StackPanelInitialized;
            }
        }

        private ActionCommand<ContextMenu> _ContextMenuCommand;

        public ActionCommand<ContextMenu> ContextMenuCommand
        {
            get
            {
                if (this._ContextMenuCommand == null)
                {
                    this._ContextMenuCommand = new ActionCommand<ContextMenu>(
                        o =>
                        {
                            //目标
                            o.PlacementTarget = panel;
                            //位置
                            o.Placement = PlacementMode.Bottom;
                            //显示菜单
                            o.IsOpen = true;
                        });
                }
                return this._ContextMenuCommand;
            }
        }

        #endregion

        #region 方法

        #region 处理MQTT接口相关的方法

        /// <summary>
        /// 初始化Mqtt服务
        /// </summary>
        /// <param name="put"></param>
        private bool InitMqttService()
        {
            AntSdkService.ReconnectedMqtt += ReconnectedMqtt;
            AntSdkService.TokenErrorEvent += HandleTokenError;
            AntSdkService.DisconnectedMqtt += AntSdkService_DisconnectedMqtt;
            //HttpService.TokenErrorEvent += HandleTokenError;
            //GetOfflineChatMsg();
            return AntSdkService.AntSdkIsConnected;
        }



        private void AntSdkService_DisconnectedMqtt(object sender, EventArgs e)
        {
            if (IsStopSdkService) return;
            isShowNetwork = "Visible";
            if (GlobalVariable.isAudioShow)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _SessionListViewModel?.TalkViewAudio(AudioChat.targetUid, "网络中断，请检查您的网络");
                    HandlerSessionHangupRes(0);
                    //网络连接异常提示
                    AudioChat.End();
                }));
            }
            waitingTimer?.Stop();
            MessageMonitor.OffLineMsgSession?.Clear();
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                UserSelectedState = new UserOnlineState
                {
                    OnlineState = (int)GlobalVariable.OnLineStatus.OffLine,
                    StateContent = "",
                    StateImage = ""
                };
                IconUrl = GlobalVariable.NotifyIcon.NotifyIconOffline;
                //NotifyIconControl.Instance.notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/ant_offLine.ico");
                _SessionListViewModel.ChangeSessionListUserState(false);
                _ContactListViewModel.ChangeContactListUserState(false);
            });
        }

        /// <summary>
        /// MQTT断开后重连的状态推送（每5秒推送一次重连状态，直到成功）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReconnectedMqtt(object sender, EventArgs e)
        {
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            if (AntSdkService.AntSdkIsConnected) //重连成功
            {
                //GetOfflineChatMsg();//获取单聊离线消息
                isShowNetwork = "Collapsed";
                UserSelectedState = UserOnlineStates.FirstOrDefault(m => m.OnlineState == GlobalVariable.UserCurrentOnlineState);
                switch (UserSelectedState?.OnlineState)
                {
                    case (int)GlobalVariable.OnLineStatus.OnLine:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconOnLine;
                        //NotifyIconControl.Instance.notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/七讯.ico");
                        break;
                    case (int)GlobalVariable.OnLineStatus.Busy:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconBusy;
                        //NotifyIconControl.Instance.notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/ant_busy.ico");
                        break;
                    case (int)GlobalVariable.OnLineStatus.Leave:
                        IconUrl = GlobalVariable.NotifyIcon.NotifyIconLeave;
                        //NotifyIconControl.Instance.notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/ant_leave.ico");
                        break;
                }
                GetOfflineChatMsg(); //获取群聊离线消息
                ChangeContactState(); //MQTT断开重连后其他联系人的在线状态变化
                HandleOrganizationModify(); //重连后组织结构变更
                waitingTimer.Start();
            }
        }

        /// <summary>
        /// 重连后组织结构变更
        /// </summary>
        private void HandleOrganizationModify()
        {
            var currentVersion = string.IsNullOrEmpty(AntSdkService.AntSdkListContactsEntity.dataVersion)
                ? 0
                : Convert.ToInt32(AntSdkService.AntSdkListContactsEntity.dataVersion);
            var errCode = 0;
            var errMsg = string.Empty;
            var input = new AntSdkAddListContactsInput { dataVersion = currentVersion.ToString() };
            var addListContactsOutput = AntSdkService.AntSdkGetAddContacts(input, ref errCode, ref errMsg);
            if (addListContactsOutput != null) //查询成功
            {
                UpdateOrganizationModify(addListContactsOutput);
            }
        }

        /// <summary>
        ///  MQTT断开重连后其他联系人的在线状态变化
        /// </summary>
        private void ChangeContactState()
        {

            var errCode = 0;
            string errMsg = string.Empty;
            var isResult = AntSdkService.AntSdkUpdateCurrentUserState(GlobalVariable.UserCurrentOnlineState,
                ref errCode, ref errMsg);
            if (!isResult)
            {
                if (errCode == 1004)
                {
                    if (GlobalVariable.isCutShow)
                    {
                        LoginOutMethod(false);
                    }
                    else
                    {
                        MessageBoxWindow.Show(errMsg + ",返回登录界面。", GlobalVariable.WarnOrSuccess.Warn);
                        LoginOutMethod(false);
                    }
                    return;
                }
                //if (!string.IsNullOrEmpty(errMsg))
                //{
                //    var msg = errMsg;
                //    Application.Current.Dispatcher.Invoke(
                //        new Action(() => MessageBoxWindow.Show("更新在线状态失败！", GlobalVariable.WarnOrSuccess.Warn)));
                //}
            }
            AsyncHandler.CallFuncWithUI<List<AntSdkUserStateOutput>>(Application.Current.Dispatcher, () =>
            {
                List<AntSdkUserStateOutput> userStateList = null;
                isResult = AntSdkService.AntSdkGetUserStateList(ref userStateList, null, ref errCode, ref errMsg);
                return userStateList;
            }, (ex, datas) =>
             {
                 if (isResult || (datas != null && datas.Count > 0))
                 {
                     AntSdkService.AntSdkListContactsEntity.users = AntSdkService.AntSdkListContactsEntity.users?.Select(
                         m =>
                         {
                             var userstate = datas.FirstOrDefault(n => n.userId == m.userId);
                             if (userstate != null)
                                 m.state = string.IsNullOrEmpty(userstate.state) ? 0 : int.Parse(userstate.state);
                             return m;
                         }).ToList();
                 }
                 _SessionListViewModel.ChangeSessionListUserState(true);
                 _ContactListViewModel.ChangeContactListUserState(true);

             });
        }

        private void HandleTokenError(object sender, EventArgs e)
        {
            ForcedLoginOut("服务连接已失效，需重新登录!");
        }

        /// <summary>
        /// 推送user_online主题，获取单聊的离线消息
        /// </summary>
        //private void GetOfflineChatMsg()
        //{
        //    SendUserOnline online = new SendUserOnline();
        //    online.ctt = new SendUserOnline_Ctt();
        //    online.ctt.os = (int)GlobalVariable.OSType.PC;
        //    online.ctt.targetId = AntSdkService.AntSdkLoginOutput.userId;
        //    online.ctt.companycode = GlobalVariable.CompanyCode;
        //    string errMsg = string.Empty;
        //    MqttService.Instance.Publish("$queue/user_online", online, ref errMsg);
        //}

        /// <summary>
        /// 获取群聊离线消息
        /// </summary>
        public void GetOfflineChatMsg()
        {
            var errCode = 0;
            string errMsg = string.Empty;
            //if (_GroupListViewModel.GroupInfos == null || _GroupListViewModel.GroupInfos.Count == 0)
            //    _GroupListViewModel.GetGroupList();
            var tempAddGroupList = new List<AntSdkGroupInfo>();
            var tempDelGroupList = new List<AntSdkGroupInfo>();
            //重连之后重新获取群组列表（防止在服务断开期间有新的群组加入）
            var groups = AntSdkService.GetGroupList(AntSdkService.AntSdkLoginOutput.userId, ref errCode, ref errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                LogHelper.WriteError("[GetGroupList接口]" + errMsg);
                var hwnd = Win32.GetForegroundWindow();
                LoginOutMethodForced(false);
                if (errCode == 1004)
                {
                    if (GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized)
                    {
                        // var result = MessageBoxWindow.Show("提醒", msg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                        MessageBoxWindow.Show("提醒", errMsg + "，返回登录界面。", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                    }
                    else
                    {
                        MessageBoxWindow.Show("提醒", errMsg + "，返回登录界面。", MessageBoxButton.OK, win, GlobalVariable.WarnOrSuccess.Warn);
                    }

                }
                else
                {
                    // Application.Current.Dispatcher.Invoke(
                    //new Action(() =>
                    if (GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized)
                    {
                        MessageBoxWindow.Show("提醒", "群组数据请求失败，群相关功能无法正常使用，返回登录界面", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                    }
                    else
                    {
                        MessageBoxWindow.Show("提醒", "群组数据请求失败，群相关功能无法正常使用，返回登录界面", MessageBoxButton.OK, win, GlobalVariable.WarnOrSuccess.Warn);
                    }
                    //));
                }
                LoginOutMethod(false, true);
                return;
            }
            if (groups != null && groups.Length > 0)
            {
                groupList = new List<AntSdkGroupInfo>(groups);
                tempAddGroupList =
                    groupList.Where(m => !_GroupListViewModel.GroupInfos.Exists(n => n.groupId == m.groupId)).ToList();
                tempDelGroupList =
                    _GroupListViewModel.GroupInfos.Where(m => !groupList.Exists(n => n.groupId == m.groupId)).ToList();
                _GroupListViewModel.GroupInfos = groupList;
            }
            List<string> topics = new List<string>();
            AntSdkSendMsg.Terminal.QuestOffLine msgOffline = new AntSdkSendMsg.Terminal.QuestOffLine();
            topics.Add("-1");
            if (_GroupListViewModel.GroupInfos != null && _GroupListViewModel.GroupInfos.Count > 0)
            {
                topics.AddRange(_GroupListViewModel.GroupInfos.Select(m => m.groupId).ToArray());
            }
            msgOffline.attr = topics.ToArray();
            msgOffline.userId = AntSdkService.AntSdkLoginOutput.userId;
            var isUserResult = AntSdkService.SdkPublishTerminalMsg(msgOffline, ref errMsg);
            if (!isUserResult)
            {
                LogHelper.WriteError("[请求离线消息SdkPublishTerminalMsg接口]" + errMsg);
                Application.Current.Dispatcher.Invoke(
                    new Action(() => MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn)));
            }

            #region 更新群组、消息列表

            //服务断开时新加群组
            if (tempAddGroupList.Count > 0)
            {
                _GroupListViewModel.AddGroups(tempAddGroupList);
            }
            //服务断开时被删除的群组
            if (tempDelGroupList.Count > 0)
            {
                foreach (var groupInfo in tempDelGroupList)
                {
                    _GroupListViewModel.DropOutGroup(groupInfo.groupId);
                    _SessionListViewModel.DropOutGroup(groupInfo.groupId);
                }
            }
            //服务断开时群组有变更，比如：转让群主、设置管理员
            if (_GroupListViewModel.GroupInfos != null && _GroupListViewModel.GroupInfos.Count > 0)
            {
                _GroupListViewModel.AddGroups(_GroupListViewModel.GroupInfos);
                //如果群成员有增删
                foreach (var groupInfo in _GroupListViewModel.GroupInfos)
                {
                    var tempGroupInfoVm =
                        _GroupListViewModel.GroupInfoList.FirstOrDefault(m => m.GroupInfo.groupId == groupInfo.groupId);
                    if (tempGroupInfoVm == null) continue;
                    var count = tempGroupInfoVm.Members?.Count;
                    if (string.IsNullOrEmpty(groupInfo.memberCount) || count == int.Parse(groupInfo.memberCount))
                        continue;
                    tempGroupInfoVm.GroupMemberCount = $"({groupInfo.memberCount}人)";
                    AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                        () =>
                        {
                            var groupMembers = GroupPublicFunction.GetMembers(groupInfo.groupId);
                            if (groupMembers != null && groupMembers.Count > 0)
                                tempGroupInfoVm.Members = GroupPublicFunction.GetMembers(groupInfo.groupId);
                            return groupMembers;
                        },
                        (ex, datas) =>
                        {
                            if (datas == null || datas.Count == 0)
                                return;
                            _SessionListViewModel.UpdateGroupMemeber(groupInfo.groupId, tempGroupInfoVm.Members);
                        });
                }

            }

            #endregion

            //if (!(new HttpService()).QueryMsg(queryMsgInput, ref queryMsgOutPut, ref errMsg))
            //{
            //    if (queryMsgOutPut.errorCode != "1004")
            //    {
            //        App.Current.Dispatcher.Invoke(new Action(() => MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn)));
            //    }
            //    return;
            //}
        }

        /// <summary>
        /// 获取群组
        /// </summary>
        public bool GetGroupList()
        {
            var temperrorCode = 0;
            var temperrorMsg = string.Empty;
            _GroupListViewModel = new GroupListViewModel(); //登录就需要订阅讨论组ID，因此需要在这里初始化
            SessionMonitor.GroupListViewModel = _GroupListViewModel;
            _SessionListViewModel = new SessionListViewModel();
            SessionMonitor.SessionListViewModel = _SessionListViewModel;
            SessionMonitor.MainWindowVM = this;
            AudioChat.MainWindowVm = this;

            SecondPartViewModel = _SessionListViewModel;
            WindowMonitor.Start();
            MessageMonitor.VersionUpdatedToReceive += MessageMonitor_VersionUpdatedToReceive;
            var groups = AntSdkService.GetGroupList(AntSdkService.AntSdkLoginOutput.userId, ref temperrorCode,
                ref temperrorMsg);
            if (groups == null && !string.IsNullOrEmpty(temperrorMsg.Trim()))
            {
                //MessageBoxWindow.Show("群数据请求失败，群相关功能无法正常使用，将为您尝试重新获取数据。", GlobalVariable.WarnOrSuccess.Warn);
                if (temperrorCode == 1004)
                {
                    MessageBoxWindow.Show(temperrorMsg + ",返回登录界面。", GlobalVariable.WarnOrSuccess.Warn);
                    LoginOutMethod(false);
                }
                else
                {
                    return false;
                }
            }
            if (groups != null && groups.Length > 0)
            {
                groupList = new List<AntSdkGroupInfo>(groups);
                _GroupListViewModel.GroupInfos = groupList;
            }
            var errCode = 0;
            string errMsg = string.Empty;
            _SessionListViewModel.InitSessionList();
            //订阅默认主题
            AntSdkService.AntSdkDefaultTopicSubscribe(ref errCode, ref errMsg);
            //if (_GroupListViewModel.GroupInfos == null || _GroupListViewModel.GroupInfos.Count == 0)
            //    _GroupListViewModel.GetGroupList();
            List<string> topics = new List<string>();
            topics.Add("-1");
            if (_GroupListViewModel.GroupInfos != null && _GroupListViewModel.GroupInfos.Count > 0)
            {
                topics.AddRange(_GroupListViewModel.GroupInfos.Select(m => m.groupId).ToArray());
            }

            AntSdkSendMsg.Terminal.QuestOffLine msgOffline = new AntSdkSendMsg.Terminal.QuestOffLine();
            //if (groupList != null && groupList.Count > 0)
            //{
            //    topics.AddRange(groupList.Select(m => m.groupId).ToArray());
            //}
            msgOffline.attr = topics.ToArray();
            msgOffline.userId = AntSdkService.AntSdkLoginOutput.userId;
            //请求离线消息
            var isUserResult = AntSdkService.SdkPublishTerminalMsg(msgOffline, ref errMsg);
            if (!isUserResult)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() => MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn)));
            }
            MessageMonitor.LoadLocalUnReadMsgData();
            UserSelectedState =
                UserOnlineStates.FirstOrDefault(m => m.OnlineState == GlobalVariable.UserCurrentOnlineState);
            return true;
            //SessionMonitor.GroupListViewModel = _GroupListViewModel;
            //SessionMonitor.SessionListViewModel = _SessionListViewModel;
            //SessionMonitor.MainWindowVm = this;
        }



        /// <summary>
        /// 下载头像
        /// </summary>
        /// <param name="imgUrl"></param>
        async void DownloadImage(string imgUrl, string userId)
        {
            var request = WebRequest.Create(imgUrl);
            using (var response = await request.GetResponseAsync())
            using (var destStream = new MemoryStream())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var downloadTask = responseStream.CopyToAsync(destStream);
                    RefreshUI(downloadTask, destStream, userId);
                    await downloadTask;
                }
            }
        }

        /// <summary>
        /// 头像下载完成时，更新界面
        /// </summary>
        /// <param name="downloadTask"></param>
        /// <param name="stream"></param>
        private async void RefreshUI(Task downloadTask, MemoryStream stream, string userId)
        {
            await Task.WhenAny(downloadTask, Task.Delay(1000)); //每隔一秒刷新一次
            var data = stream.ToArray();
            var tmpStream = new MemoryStream(data); //TODO 当图片的头没有下载到时，这儿可能抛异常
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = tmpStream;
            bmp.EndInit();
            if (!downloadTask.IsCompleted)
            {
                RefreshUI(downloadTask, stream, userId);
            }
            else
            {

                downloadTask.Dispose();
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="versionUpdate"></param>
        public void update(AntSdkReceivedOtherMsg.VersionHardUpdate versionUpdate)
        {

            //版本更新（硬更新）
            //本地版本和服务器版本比较
            string localVersion = publicMethod.xmlFind("version",
                AppDomain.CurrentDomain.BaseDirectory + "version.xml");
            if (Convert.ToInt32(localVersion.Trim().Replace(".", "")) <
                Convert.ToInt32(versionUpdate.version.Trim().Replace(".", "")))
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
                        publicMethod.xmlModify("version", versionUpdate.version,
                            AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                        publicMethod.xmlModify("describe", versionUpdate.attr?.describe,
                            AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                        publicMethod.xmlModify("url", versionUpdate.attr?.url,
                            AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                        publicMethod.xmlModify("updateType", versionUpdate.attr?.updateType,
                            AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                        publicMethod.xmlModify("fileMd5Value", versionUpdate.attr?.updateType,
                            AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    }
                }
                else
                {
                    publicMethod.xmlModify("title", "UpdateVersion",
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    publicMethod.xmlModify("version", versionUpdate.version,
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    publicMethod.xmlModify("describe", versionUpdate.attr?.describe,
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    publicMethod.xmlModify("url", versionUpdate.attr?.url,
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    publicMethod.xmlModify("updateType", versionUpdate.attr?.updateType,
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                    publicMethod.xmlModify("fileMd5Value", versionUpdate.attr?.updateType,
                        AppDomain.CurrentDomain.BaseDirectory + "downUpdateFile//update.xml");
                }
                //Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateWindowView update = new UpdateWindowView();
                    UpdateWindowViewModel updateModel = new UpdateWindowViewModel(versionUpdate.attr?.describe, true);
                    update.ShowInTaskbar = false;
                    update.DataContext = updateModel;
                    update.Owner = win;
                    update.ShowDialog();
                }));
            }
        }

        protected object objct = new object();

        /// <summary>
        /// 讨论组成员更新
        /// </summary>
        /// <param name="sysMsg"></param>
        public void HandleSysUerMsg_UpdateUserInfo(AntSdkMsBase sysMsg)
        {
            #region 修改头像

            AntSdkContact_User user = null;
            var msg = sysMsg as AntSdkReceivedUserMsg.Modify;
            if (msg != null)
            {
                //AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
                //{
                var userSysMsg = msg;
                if (userSysMsg.attr == null) return;
                user = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                    c => c.userId == userSysMsg.userId);
                if (user == null) return;

                if (!string.IsNullOrEmpty(userSysMsg.attr.picture) && user.picture != userSysMsg.attr.picture)
                {
                    string imageFilePath = string.Empty;
                    user.picture = userSysMsg.attr.picture;
                    if (publicMethod.IsUrlRegex(userSysMsg.attr.picture))
                    {
                        var index = userSysMsg.attr.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                        var fileName = userSysMsg.attr?.picture.Substring(index,
                            userSysMsg.attr.picture.Length - index);
                        if (!File.Exists(publicMethod.UserHeadImageFilePath() + fileName))
                        {
                            var downloadUserHeadImagePath = publicMethod.DownloadFilePath() +
                                                      "\\DownloadUserHeadImage\\";
                            if (!Directory.Exists(downloadUserHeadImagePath))
                                Directory.CreateDirectory(downloadUserHeadImagePath);
                            if (!Directory.Exists(publicMethod.UserHeadImageFilePath()))
                                Directory.CreateDirectory(publicMethod.UserHeadImageFilePath());
                            imageFilePath = FileHelper.DownloadPicture(userSysMsg.attr.picture,
                                downloadUserHeadImagePath + fileName, -1,
                                publicMethod.UserHeadImageFilePath(), userSysMsg.userId);
                            if (!string.IsNullOrEmpty(imageFilePath))
                            {
                                //userSysMsg.attr.picture = imageFilePath;
                                var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                    m => m.UserID == userSysMsg.userId);
                                if (userImage != null)
                                {
                                    //if (!string.IsNullOrEmpty(userImage.Url) && File.Exists(userImage.Url))
                                    //    File.Delete(userImage.Url);//图片正在使用 无法删除
                                    userImage.Url = imageFilePath;
                                }
                                else
                                {
                                    GlobalVariable.ContactHeadImage.UserHeadImages.Add(new ContactUserImage
                                    {
                                        UserID = userSysMsg.userId,
                                        Url = imageFilePath
                                    });
                                }
                            }
                        }
                        else
                        {
                            var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                  m => m.UserID == userSysMsg.userId);
                            if (userImage != null)
                            {
                                userImage.Url = publicMethod.UserHeadImageFilePath() + fileName;
                            }
                        }

                    }
                    var tempImageFilePath = !string.IsNullOrEmpty(imageFilePath)
                        ? imageFilePath
                        : userSysMsg.attr.picture;
                    if (userSysMsg.userId == AntSdkService.AntSdkLoginOutput.userId)
                    {
                        //HeadPic = sysMsg.picture;
                        HeadBitmapImage(tempImageFilePath, userSysMsg.attr.picture);
                        AntSdkService.AntSdkCurrentUserInfo.picture = userSysMsg.attr.picture;
                    }
                    var contactInfoVm =
                        _ContactListViewModel.ContactInfoViewModelList?.FirstOrDefault(
                            c => c.User.userId == userSysMsg.userId);
                    if (contactInfoVm != null)
                    {
                        contactInfoVm.Photo = tempImageFilePath;
                        contactInfoVm.User.picture = tempImageFilePath;
                    }

                    if (_GroupListViewModel?.GroupInfoList != null)
                    {
                        foreach (GroupInfoViewModel vm in _GroupListViewModel.GroupInfoList)
                        {
                            if (vm.Members == null) return;
                            AntSdkGroupMember member = vm.Members.Find(c => c.userId == userSysMsg.userId);
                            if (member == null) continue;
                            member.picture = userSysMsg.attr.picture;
                        }
                    }
                    _SessionListViewModel?.UpdateUserInfo(userSysMsg.userId, tempImageFilePath, userSysMsg.attr.picture);
                }
                if (user.departmentId != userSysMsg.attr.departmentId)
                {
                    var tempDepartmentId = user.departmentId;
                    user.departmentId = userSysMsg.attr.departmentId;
                    user.position = userSysMsg.attr.position;
                    _ContactListViewModel?.UserChangeDepartemnt(tempDepartmentId, userSysMsg.attr.departmentId);
                }
                else
                {
                    if (user.position != userSysMsg.attr.position || user.userName != userSysMsg.attr.userName || user.userNum != userSysMsg.attr.userNum)
                    {
                        if (!string.IsNullOrEmpty(userSysMsg.attr.position))
                            user.position = userSysMsg.attr.position;
                        if (!string.IsNullOrEmpty(userSysMsg.attr.userName))
                            user.userName = userSysMsg.attr.userName;
                        if (!string.IsNullOrEmpty(userSysMsg.attr.userNum))
                            user.userNum = userSysMsg.attr.userNum;
                        _ContactListViewModel?.DepartmentMemberUpdate(userSysMsg.attr.departmentId);
                        AntSdkContact_User contactUser = new AntSdkContact_User
                        {
                            position = userSysMsg.attr.position,
                            userName = userSysMsg.attr.userName,
                            userNum = userSysMsg.attr.userNum,
                            userId = userSysMsg.userId
                        };
                        _SessionListViewModel?.UpdateUserInfo(contactUser, false);

                    }
                }


                #region 信息入库

                ThreadPool.QueueUserWorkItem(m =>
                    AntSdkService._cUserInfoDal.Update(userSysMsg));

                #endregion

                //});

            }
            else
            {

                var userSysMsg = sysMsg as AntSdkReceivedUserMsg.State;

                if (userSysMsg != null)
                {

                    var stateType = userSysMsg.MsgType;
                    #region 用户是否被停用
                    if (stateType == AntSdkMsgType.Disable)
                    {
                        if (userSysMsg.userId == AntSdkService.AntSdkCurrentUserInfo.userId)
                        {
                            ForcedLoginOut("您的账号已被禁用，无法继续登录！");
                            return;
                        }
                        var cuss = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(m => m.userId == userSysMsg.userId);
                        if (cuss != null)
                        {
                            cuss.status = 0;
                            AntSdkContact_User contactUser = new AntSdkContact_User
                            {
                                position = cuss.position,
                                userName = cuss.userName,
                                userNum = cuss.userNum,
                                userId = cuss.userId,
                                status = cuss.status
                            };
                            ThreadPool.QueueUserWorkItem(m =>
                  AntSdkService._cUserInfoDal.Update(cuss.userId, cuss.status));
                            _SessionListViewModel?.UpdateUserInfo(contactUser, false);
                            _ContactListViewModel?.DepartmentMemberUpdate(cuss.departmentId);
                        }
                        stateType = AntSdkMsgType.OffLine;
                    }
                    #endregion
                    #region 用户在线状态
                    var state = -1;
                    switch (stateType)
                    {
                        case AntSdkMsgType.OnLine:
                            state = (int)GlobalVariable.OnLineStatus.OnLine;
                            break;
                        case AntSdkMsgType.Leave:
                            state = (int)GlobalVariable.OnLineStatus.Leave;
                            break;
                        case AntSdkMsgType.Busy:
                            state = (int)GlobalVariable.OnLineStatus.Busy;
                            break;
                        case AntSdkMsgType.PhoneLine:
                            state = (int)GlobalVariable.OnLineStatus.OtherOnLine;
                            break;
                        case AntSdkMsgType.OffLine:
                            state = (int)GlobalVariable.OnLineStatus.OffLine;
                            break;

                    }
                    user =
                        AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == userSysMsg.userId);
                    //LogHelper.WriteWarn("用户状态变化用户名：" + user?.userName + "  用户ID：" + user?.userId + "   state：" + state);
                    if (user != null && user.state == state ||
                        userSysMsg.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                        return;
                    ContactInfoViewModel contactInfoVM =
                        _ContactListViewModel.ContactInfoViewModelList?.FirstOrDefault(
                            c => c.User.userId == userSysMsg.userId);
                    if (contactInfoVM != null)
                    {
                        if (GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic.ContainsKey(state))
                        {
                            contactInfoVM.IsOfflineState = false;
                            contactInfoVM.User.state = state;
                            contactInfoVM.UserOnlineStateIcon =
                                GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic[state];
                        }
                        else
                        {
                            contactInfoVM.UserOnlineStateIcon = "";
                            contactInfoVM.User.state = state;
                            contactInfoVM.IsOfflineState = true;
                        }
                    }

                    if (user != null)
                    {
                        if ((state == 0 || state == 4) && userSysMsg.userId == AntSdkService.AntSdkCurrentUserInfo.userId)
                            return;
                        user.state = state;
                    }
                    if (userSysMsg.userId == AntSdkService.AntSdkCurrentUserInfo.userId)
                    {
                        if (user != null)
                        {
                            if (user.state == 0 || user.state == 4)
                                return;
                            switch (user.state)
                            {
                                case (int)GlobalVariable.OnLineStatus.OnLine:
                                    IconUrl = GlobalVariable.NotifyIcon.NotifyIconOnLine;
                                    break;
                                case (int)GlobalVariable.OnLineStatus.Busy:
                                    IconUrl = GlobalVariable.NotifyIcon.NotifyIconBusy;
                                    break;
                                case (int)GlobalVariable.OnLineStatus.Leave:
                                    IconUrl = GlobalVariable.NotifyIcon.NotifyIconLeave;
                                    break;
                            }
                            UserSelectedState = UserOnlineStates.FirstOrDefault(m => m.OnlineState == user.state);
                            if (!isAutoState)
                                GlobalVariable.UserCurrentOnlineState = state;
                        }
                    }

                    _SessionListViewModel?.UpdateUserOnlineSate(userSysMsg.userId, state);
                    #endregion
                }

            }




            #endregion
        }

        /// <summary>
        /// 组织架构更新
        /// </summary>
        public void OrganizationModify()
        {
            _ContactListViewModel.RefreshSource();
        }

        /// <summary>
        /// 组织架构更新
        /// </summary>
        /// <param name="addListContacts"></param>
        public void UpdateOrganizationModify(AntSdkAddListContactsOutput addListContacts)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (addListContacts.users?.update != null && addListContacts.users.update.Count > 0)
                {
                    _ContactListViewModel?.RefreshSource();
                    //处理本人的信息更新
                    var info = addListContacts.users.update.FirstOrDefault(
                        v => v.userId == AntSdkService.AntSdkCurrentUserInfo.userId);
                    if (info != null)
                    {
                        if (!string.IsNullOrEmpty(info.picture))
                        {
                            if (publicMethod.IsUrlRegex(info.picture))
                            {
                                var index = info.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                                var fileName = info.picture.Substring(index, info.picture.Length - index);
                                if (!File.Exists(publicMethod.UserHeadImageFilePath() + fileName))
                                {
                                    var downloadUserHeadImagePath = publicMethod.DownloadFilePath() +
                                                                    "\\DownloadUserHeadImage\\";
                                    if (!Directory.Exists(downloadUserHeadImagePath))
                                        Directory.CreateDirectory(downloadUserHeadImagePath);
                                    if (!Directory.Exists(publicMethod.UserHeadImageFilePath()))
                                        Directory.CreateDirectory(publicMethod.UserHeadImageFilePath());
                                    var imageFilePath = FileHelper.DownloadPicture(info.picture,
                                        downloadUserHeadImagePath + fileName, -1,
                                        publicMethod.UserHeadImageFilePath(), info.userId);
                                    if (!string.IsNullOrEmpty(imageFilePath))
                                    {

                                        var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                            m => m.UserID == info.userId);
                                        if (userImage != null)
                                        {
                                            if (!string.IsNullOrEmpty(userImage.Url) && File.Exists(userImage.Url))
                                                File.Delete(userImage.Url);
                                            userImage.Url = imageFilePath;
                                        }
                                        else
                                        {
                                            GlobalVariable.ContactHeadImage.UserHeadImages.Add(new ContactUserImage
                                            {
                                                UserID = info.userId,
                                                Url = imageFilePath
                                            });
                                        }
                                    }
                                    var tempindex = AntSdkService.AntSdkCurrentUserInfo.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                                    var fileNameIndex = AntSdkService.AntSdkCurrentUserInfo.picture.LastIndexOf(".", StringComparison.Ordinal);
                                    var tempfileName = AntSdkService.AntSdkCurrentUserInfo.picture.Substring(index, fileNameIndex - index);
                                    string strUrl = AntSdkService.AntSdkCurrentUserInfo.picture.Replace(fileName, fileName + "_80x80");
                                    HeadBitmapImage(tempfileName, info.picture);
                                }
                            }
                        }

                        if (info.userName != null)
                        {
                            AntSdkService.AntSdkCurrentUserInfo.userName = info.userName;
                            UserName = info.userName;
                        }
                    }

                    AsyncHandler.AsyncCall(Dispatcher.CurrentDispatcher, () =>
                    {
                        foreach (var user in addListContacts.users.update)
                        {
                            _SessionListViewModel?.UpdateUserInfo(user,user?.userId==AntSdkService.AntSdkCurrentUserInfo.userId);
                        }
                    });
                    return;
                }
                if (addListContacts.departs != null)
                {
                    _ContactListViewModel?.DepartmentUpdate(addListContacts.departs);
                }
                if (addListContacts.users != null)
                {
                    if (addListContacts.users.add != null && addListContacts.users.add.Count > 0)
                    {
                        string tempDepartmentId = string.Empty;
                        foreach (var user in addListContacts.users.add)
                        {
                            if (tempDepartmentId == user.departmentId) continue;
                            _ContactListViewModel?.DepartmentMemberUpdate(user.departmentId);
                            tempDepartmentId = user.departmentId;
                        }
                    }

                    if (addListContacts.users.delete != null && addListContacts.users.delete.Count > 0)
                    {
                        string tempDepartmentId = string.Empty;
                        foreach (var user in addListContacts.users.delete)
                        {
                            if (tempDepartmentId == user.departmentId) continue;
                            _ContactListViewModel?.DepartmentMemberUpdate(user.departmentId);
                            tempDepartmentId = user.departmentId;
                        }
                    }

                }
            });
            //_ContactListViewModel.RefreshSource();
            if (addListContacts?.users?.update != null && addListContacts.users.update.Count > 0)
            {

            }

        }

        #endregion

        /// <summary>
        /// 退出程序
        /// </summary>
        private void ItemExitClick(object sender, EventArgs e)
        {
            //PostResult logout = new PostResult();
            //string errMsg = string.Empty;
            //if (!httpService.Logout(mainWindowParams.companyCode, mainWindowParams.userInfo.userId, mainWindowParams.token, ref logout, ref errMsg))
            //{
            //    if (MessageBoxWindow.Show("错误", "账号退出失败：" + errMsg + "！是否继续退出程序？", MessageBoxButton.OKCancel) != ShowDialogResult.Ok)
            //    {
            //        return;
            //    }
            //}
            //IsCloseBytTaskbar = false;
            //this.Close();
            //NotifyIconControl.GetInstance().notifyIcon.Visible = false;
            //System.Windows.Application.Current.Shutdown();
            //win.Close();
            LoginOutMethod(true);
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        private void ItemLogoutClick(object sender, EventArgs e)
        {
            LoginOutMethod(false);
        }

        private void OnNotifyIconMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                QPan_OpenFromTuoPan();
            }
        }

        public void QPan_OpenFromTuoPan()
        {
            if (win == null) return;
            win.Show();
            win.WindowState = WindowState.Normal;
            win.Activate();
            win.ShowInTaskbar = true;
        }

        private void _SessionListViewModel_ButtonClickEvent(object sender, EventArgs args)
        {
            SessionSelected = true;
        }

        private void SendMassMsgEvent(AntSdkMassMsgCtt massMsg)
        {
            SessionSelected = true;
        }

        /// <summary>
        /// 获取组织结构
        /// </summary>
        private void GetListContacts()
        {
            //ListContactsInput input = new ListContactsInput();
            //input.token = AntSdkService.AntSdkLoginOutput.token;
            //input.version = GlobalVariable.Version;
            //input.userId = AntSdkService.AntSdkLoginOutput.userId;
            //input.dataVersion = string.Empty;
            //AntSdkService.AntSdkListContactsEntity = new ListContactsOutput();
            //string errMsg = string.Empty;
            ////Debug.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.sss") + "]" + "步骤1");
            //if (!(new HttpService()).ListContacts(input, ref AntSdkService.AntSdkListContactsEntity, ref errMsg))
            //{
            //    MessageBox.Show("获取联系人失败:" + errMsg);
            //    return;
            //}
        }


        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="isExitApp">是否直接点击退出</param>
        /// <param name="isForced">是否被迫下线（被抢登、修改密码重新登录）</param>
        public void LoginOutMethod(bool isExitApp, bool isForced = false)
        {
            GlobalVariable.isLoginOut = true;
            //IsCloseByTaskbar = false;
            var errorCode = 0;
            var errorMsg = string.Empty;
            if (!isForced)
            {
                if (GlobalVariable.isAudioShow)
                {
                    if (
                        MessageBoxWindow.Show("提示", "正在语音通话中，注销或退出程序会结束语音通话，是否退出？", MessageBoxButton.YesNo,
                            GlobalVariable.WarnOrSuccess.Warn) != GlobalVariable.ShowDialogResult.Yes)
                        return;
                    AudioChat.End();
                }
                #region 注销登录
                UnsubscribeOnClosingWindow();

                bool isResult = false;
                isResult = AntSdkService.AntSdkUpdateCurrentUserState(((int)GlobalVariable.OnLineStatus.OffLine),
                    ref errorCode, ref errorMsg);
                if (!isResult)
                {

                }
                IsStopSdkService = true;
                if (!SDK.AntSdk.AntSdkService.StopAntSdk(ref errorCode, ref errorMsg))
                {
                    LogHelper.WriteError("[StopAntSdk接口]" + errorMsg);
                    if (MessageBoxWindow.Show("提示", "服务器停止失败，可能会影响正常使用，是否强制退出？", MessageBoxButton.YesNo,
                        GlobalVariable.WarnOrSuccess.Warn) != GlobalVariable.ShowDialogResult.Yes)
                    {
                        isResult = AntSdkService.AntSdkUpdateCurrentUserState(GlobalVariable.UserCurrentOnlineState,
                            ref errorCode, ref errorMsg);
                        if (!isResult)
                        {
                            LogHelper.WriteError("[AntSdkUpdateCurrentUserState接口]" + errorMsg);
                        }
                        IsStopSdkService = false;
                        return;
                    }
                }
            }

            #region 保存发送消息快捷键配置

            if (GlobalVariable.systemSetting == null)
            {
                GlobalVariable.systemSetting = new SystemSetting
                {
                    SendKeyType = 0,
                    KeyShortcuts = "81" //对应截图快捷键Q
                };
            }
            else if (string.IsNullOrEmpty(GlobalVariable.systemSetting.KeyShortcuts))
            {
                GlobalVariable.systemSetting.KeyShortcuts = "81"; //对应截图快捷键Q
            }
            DataConverter.EntityToXml<SystemSetting>(System.Environment.CurrentDirectory + "/SysSettingCache.xml",
                GlobalVariable.systemSetting, ref errorMsg);

            #endregion
            AudioChat.ExitClearApi();

            #region 注销新

            //修改登录信息 取消自动登录

            if (!isExitApp)
            {
                var listAccountInfo = new List<AccountInfo>();
                DataConverter.XmlToEntity(Environment.CurrentDirectory + "/AccountCache.xml", ref listAccountInfo,
                    ref errorMsg);
                if (listAccountInfo != null)
                {
                    foreach (var info in listAccountInfo)
                    {
                        info.AutoLogin = false;
                    }
                }
                var accountInfo = listAccountInfo?.FirstOrDefault(
                  m => m.ID == AntSdkService.AntSdkCurrentUserInfo.phone ||
                       m.ID == AntSdkService.AntSdkCurrentUserInfo.email);
                if (accountInfo != null)
                    accountInfo.LastLoginTime = DateTime.Now.AddSeconds(-30);
                DataConverter.EntityToXml<List<AccountInfo>>(System.Environment.CurrentDirectory + "/AccountCache.xml",
                    listAccountInfo, ref errorMsg);
            }
            else
            {
                var listAccountInfo = new List<AccountInfo>();
                DataConverter.XmlToEntity(Environment.CurrentDirectory + "/AccountCache.xml", ref listAccountInfo,
                    ref errorMsg);
                var accountInfo = listAccountInfo?.FirstOrDefault(
                    m => m.ID == AntSdkService.AntSdkCurrentUserInfo.phone ||
                         m.ID == AntSdkService.AntSdkCurrentUserInfo.email);
                if (accountInfo != null)
                    accountInfo.LastLoginTime = DateTime.Now.AddSeconds(-30);
                DataConverter.EntityToXml<List<AccountInfo>>(System.Environment.CurrentDirectory + "/AccountCache.xml",
                    listAccountInfo, ref errorMsg);
            }
            if (!isExitApp)
            {
                bool startApp =
                    CommonMethods.StartApplication(System.Windows.Forms.Application.StartupPath + "\\AntennaChat.exe");
            }
            //NotifyIconControl.Instance.notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();

            #endregion

            #region 注销旧

            //if (isExitApp)
            //{
            //    NotifyIconControl.Instance.notifyIcon.Visible = false;
            //    win.Close();
            //    System.Windows.Application.Current.Shutdown();
            //}
            //else
            //{
            //    NotifyIconControl.Instance.SetNotifyIconText(string.Empty);
            //    App.Current.Dispatcher.Invoke((Action)(() =>
            //    {
            //        LoginWindowView login = new LoginWindowView();

            //        win.Hide();//隐藏主窗体
            //        login.Show();
            //        win.Close();//关闭
            //    }));
            //}

            #endregion

            #endregion

            GC.Collect();
            cancel(true);
        }

        /// <summary>
        /// 被强制退出
        /// </summary>
        public void LoginOutMethodForced(bool isStopAntSdk = true)
        {
            GlobalVariable.isLoginOut = true;
            //IsCloseByTaskbar = false;
            //TODO:AntSdk_Modify
            if (GlobalVariable.isAudioShow)
            {
                AudioChat.End();

            }
            UnsubscribeOnClosingWindow();
            if (isStopAntSdk)
            {
                var errorCode = 0;
                var errorMsg = string.Empty;
                bool isResult = false;
                //IsStopSdkService = true;
                if (!SDK.AntSdk.AntSdkService.StopAntSdk(ref errorCode, ref errorMsg))
                {
                    LogHelper.WriteError("[StopAntSdk接口]" + errorMsg);
                    if (MessageBoxWindow.Show("提示", "服务器停止失败，可能会影响正常使用，是否强制退出？", MessageBoxButton.YesNo,
                            GlobalVariable.WarnOrSuccess.Warn) != GlobalVariable.ShowDialogResult.Yes)
                    {
                        isResult = AntSdkService.AntSdkUpdateCurrentUserState(GlobalVariable.UserCurrentOnlineState,
                            ref errorCode, ref errorMsg);
                        if (!isResult)
                        {
                            LogHelper.WriteError("[AntSdkUpdateCurrentUserState接口]" + errorMsg);
                        }
                    }
                }
                else
                {
                    isShowNetwork = "Collapsed";
                }
            }
            else
            {
                isShowNetwork = "Collapsed";
            }
        }

        public static event EventHandler CancelEventHandler;

        private static void cancel(bool b)
        {
            if (CancelEventHandler != null)
            {
                CancelEventHandler(true, null);
            }
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        public void Window_Closing(object sender, CancelEventArgs e)
        {
            GlobalVariable.MainWinIsMinimized = true;
            //关闭窗口之前，没有调用过注销登录接口（从任务栏关闭会有这种情况）
            if (GlobalVariable.isLoginOut == false)
            {
                win.WindowState = WindowState.Minimized;
                win.ShowInTaskbar = false;
                e.Cancel = true;
            }
        }

        //private ICommand _ClosingWindow;
        //public ICommand ClosingWindow
        //{
        //    get
        //    {
        //        if (this._ClosingWindow == null)
        //        {
        //            this._ClosingWindow = new DefaultCommand(o =>
        //            {
        //                if(GlobalVariable.isLoginOut ==false )//关闭状态之前，没有调用过注销登录接口（从任务栏关闭会有这种情况）
        //                {

        //                }
        //                //if (GlobalVariable.isLoginOut == false)
        //                //{
        //                //    #region 注销登录
        //                //    LoginOutInput input = new LoginOutInput();
        //                //    BaseOutput output = new BaseOutput();
        //                //    input.token = AntSdkService.AntSdkLoginOutput.token;
        //                //    input.version = GlobalVariable.Version;
        //                //    input.userId = AntSdkService.AntSdkLoginOutput.userId;
        //                //    string errMsg = string.Empty;
        //                //    if (!(new HttpService()).LoginOut(input, ref output, ref errMsg))
        //                //    {
        //                //        return;
        //                //    }
        //                //    NotifyIconControl.Instance.SetNotifyIconText(string.Empty);
        //                //    UnsubscribeOnClosingWindow();
        //                //    #endregion
        //                //}
        //            });
        //        }
        //        return this._ClosingWindow;
        //    }
        //}



        /// <summary>
        /// 关闭窗体时取消订阅事件
        /// </summary>
        private void UnsubscribeOnClosingWindow()
        {
            try
            {
                if (_keyboardHook != null) _keyboardHook.UninstallHook(); //取消钩子
                ContactInfoViewModel.MouseDoubleClickEvent -= _SessionListViewModel_ButtonClickEvent;
                GroupInfoViewModel.MouseDoubleClickEvent -= _SessionListViewModel_ButtonClickEvent;
                GroupMemberViewModel.MouseDoubleClickEvent -= _SessionListViewModel_ButtonClickEvent;
                MassMsgSentViewModel.SendMassMsgEvent -= SendMassMsgEvent;
                //TODO:AntSdk_Modify
                //MqttService.Instance.ReconnectedMqtt -= ReconnectedMqtt;
                //HttpService.TokenErrorEvent -= HandleTokenError;
                //NotifyIconControl.Instance.notifyIcon.ContextMenu.MenuItems["ExitApp"].Click -= ItemExitClick;
                //NotifyIconControl.Instance.notifyIcon.ContextMenu.MenuItems["Logout"].Click -= ItemLogoutClick;
                //NotifyIconControl.Instance.notifyIcon.MouseClick -= OnNotifyIconMouseClick;
                if (_SessionListViewModel != null)
                {
                    _SessionListViewModel.UnsubscribeOnClosingWindow();
                }
                WindowMonitor.End();
                //TODO:AntSdk_Modify
                //MqttService.Instance.DisConnect();
                this.UserId = string.Empty;
                waitingTimer?.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[UnsubscribeOnClosingWindow]" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 更新用户在线状态信息
        /// </summary>
        /// <param name="onLine"></param>
        private void UpdateOnLineStatus(GlobalVariable.OnLineStatus onLine)
        {

        }

        /// <summary>
        /// 被迫离线（挤掉或者被踢出）
        /// </summary>
        public void ForcedLoginOut(string msg)
        {
            try
            {
                //MqttService.Instance.MessageReceived -= MqttMessageReceived;
                //MqttService.Instance.DisConnect();
                //UnsubscribeOnClosingWindow();
                //该账户在其他地方登陆，则强制退出
                Application.Current.Dispatcher.Invoke((Action)(() =>
               {
                   CloseExitVerify();
                   if (GlobalVariable.isCutShow)
                   {
                       LoginOutMethod(false, true);
                   }
                   else
                   {
                       LoginOutMethodForced();
                       var hwnd = Win32.GetForegroundWindow();
                       if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized) || win.ShowInTaskbar)
                       {
                           // var result = MessageBoxWindow.Show("提醒", msg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                           MessageBoxWindow.Show("提醒", msg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                       }
                       else
                       {
                           MessageBoxWindow.Show("提醒", msg, MessageBoxButton.OK, win, GlobalVariable.WarnOrSuccess.Warn);
                       }

                       LoginOutMethod(false, true);

                   }
               }));
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[ForcedLoginOut]:" + e.Message + "," + e.StackTrace);
            }
        }


        //private Grid _ThirdPartViewModel = new Grid();
        ///// <summary>
        ///// 要绑定和切换的第三部分ViewModel
        ///// </summary>
        //public Grid ThirdPartViewModel
        //{
        //    get { return _ThirdPartViewModel; }
        //    set
        //    {
        //        if (_ThirdPartViewModel == value)
        //        {
        //            return;
        //        }
        //        _ThirdPartViewModel = value;
        //        RaisePropertyChanged(() => ThirdPartViewModel);
        //    }
        //}
        /// <summary>
        /// 重写主窗口点击触发方法
        /// </summary>
        public override void MainClick()
        {
            base.MainClick();
            if (MainExhibitControl.Children.Count == 0) return;
            var child = MainExhibitControl.Children[0];
            if (child is TalkGroupWindowView)
            {
                string newName = (child as TalkGroupWindowView).txtName.Text;
                var dataContext = (child as TalkGroupWindowView).DataContext;
                var talkGroupViewModel = dataContext as TalkGroupViewModel;
                talkGroupViewModel?.UpdateName(newName);
            }
        }

        #region 有未读消息时托盘图标闪烁功能 

        DispatcherTimer waitingTimer;

        /// <summary>
        /// 每隔一秒刷新一次等待时间
        /// </summary>
        /// 作者：赵雪峰 20160528
        private void StartWaitingTimer()
        {
            waitingTimer = new DispatcherTimer();
            waitingTimer.Tick += waitingTimer_Tick;
            waitingTimer.Interval = TimeSpan.FromMilliseconds(500);
            waitingTimer.Start();
        }

        private bool blink = false;
        private bool isAutoState;
        /// <summary>
        /// 刷新等待时间事件
        /// </summary>
        /// 作者：赵雪峰 20160528
        private void waitingTimer_Tick(object sender, EventArgs e)
        {
            var minute = GetLastInputTime();
            //电脑15分钟没任何操作，状态自动设置为离开状态
            if (minute >= 10)
            {
                if (UserSelectedState == null || UserSelectedState.OnlineState != (int)GlobalVariable.OnLineStatus.Leave)
                {
                    isAutoState = true;
                    var errCode = 0;
                    string errMsg = string.Empty;
                    var isResult = AntSdkService.AntSdkUpdateCurrentUserState((int)GlobalVariable.OnLineStatus.Leave,
                        ref errCode, ref errMsg);
                    if (!isResult)
                    {
                        return;
                    }
                    UserSelectedState =
                        UserOnlineStates.FirstOrDefault(m => m.OnlineState == (int)GlobalVariable.OnLineStatus.Leave);
                    //GlobalVariable.CurrentUserIsIdleState = true;
                }
            }
            else if (GlobalVariable.UserCurrentOnlineState != (int)GlobalVariable.OnLineStatus.Leave && UserSelectedState != null &&
                     GlobalVariable.UserCurrentOnlineState != UserSelectedState.OnlineState && isAutoState)
            {
                var errCode = 0;
                string errMsg = string.Empty;
                var isResult = AntSdkService.AntSdkUpdateCurrentUserState(GlobalVariable.UserCurrentOnlineState,
                    ref errCode, ref errMsg);
                if (!isResult)
                {
                    return;
                }
                UserSelectedState =
                    UserOnlineStates.FirstOrDefault(m => m.OnlineState == GlobalVariable.UserCurrentOnlineState);
                isAutoState = false;
                //GlobalVariable.CurrentUserIsIdleState = false;
            }
            if (_SessionListViewModel == null) return;
            try
            {
                if (!win.ShowInTaskbar &&
                    (_SessionListViewModel.GetTotelUnreadCount() > 0 || _SessionListViewModel.curSessionUnreadMsg))
                {
                    //500毫秒切换一次图标
                    if (!blink)
                    {
                        switch (UserSelectedState?.OnlineState)
                        {
                            case (int)GlobalVariable.OnLineStatus.OnLine:
                                IconUrl = GlobalVariable.NotifyIcon.NotifyIconOnLine;
                                break;
                            case (int)GlobalVariable.OnLineStatus.Busy:
                                IconUrl = GlobalVariable.NotifyIcon.NotifyIconBusy;
                                break;
                            case (int)GlobalVariable.OnLineStatus.Leave:
                                IconUrl = GlobalVariable.NotifyIcon.NotifyIconLeave;
                                break;
                        }

                    }
                    else
                    {
                        IconUrl = "/AntennaChat;component/blank.ico";
                    }
                }
                else
                {
                    switch (UserSelectedState?.OnlineState)
                    {
                        case (int)GlobalVariable.OnLineStatus.OnLine:
                            IconUrl = GlobalVariable.NotifyIcon.NotifyIconOnLine;
                            break;
                        case (int)GlobalVariable.OnLineStatus.Busy:
                            IconUrl = GlobalVariable.NotifyIcon.NotifyIconBusy;
                            break;
                        case (int)GlobalVariable.OnLineStatus.Leave:
                            IconUrl = GlobalVariable.NotifyIcon.NotifyIconLeave;
                            break;
                    }
                }
            }
            catch
            {
            }
            blink = !blink;
            //未读消息标识
            if (_SessionListViewModel.GetTotelUnreadCount() > 0 && !IsUnreadMsg && !SessionSelected)
                IsUnreadMsg = true;
        }

        #endregion

        #region 图片/文件拖拽功能

        /// <summary>
        /// 拖拽文件
        /// </summary>
        public void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            //msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            var s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            var fileNames = s?.Where(File.Exists).ToList();
            if (fileNames?.Count > 0)
            {
                _SessionListViewModel.SelectUploadFiles(fileNames.ToArray());
            }
        }

        public void SearchTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
            //Window_Drop(sender, e);
            //Debug.Print("SearchTextBox_PreviewDragOver");
        }

        public void SearchTextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            Window_Drop(sender, e);
        }

        #endregion

        #endregion
        #region 确认打卡
        private CheckInVerifyResultView resultView;
        public static CheckInVerifyView VerifyView;
        private CheckInVerifyViewModel verifyVm;
        public void AttendanceRecordVerify(string msgTime, AntSdkReceivedOtherMsg.AttendanceRecord_content content, AntSdkMsgType msgType)
        {

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (msgType == AntSdkMsgType.CheckInVerify)
                {
                    var errCode = 0;
                    var errMsg = string.Empty;
                    bool isPassword;
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                    DateTime serverDateTime = DateTime.Now;
                    if (serverResult != null)
                    {
                        serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                    }
                    var checkInDataTime = PublicTalkMothed.ConvertStringToDateTime(content.attendTime);

                    var diffMinute = serverDateTime - checkInDataTime;
                    if (checkInDataTime.ToShortDateString() != serverDateTime.ToShortDateString())
                        return;
                    if (diffMinute.Hours > 6)
                        return;
                    var hwnd = Win32.GetForegroundWindow();
                    if (diffMinute.Days > 0 || diffMinute.TotalMinutes > 20)
                    {
                        CloseExitVerify();
                        resultView = new CheckInVerifyResultView();
                        resultView.DataContext = new CheckInVerifyViewModel(content, CheckInVerifyResultState.TimeError, false, false);
                        //if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized))
                        //{
                        resultView.Topmost = true;

                        //}
                        //else
                        //{
                        //    resultView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        //    if (win != null)
                        //    {
                        //        resultView.Owner = win as MainWindowView;
                        //    }
                        //}
                        resultView.Show();
                        return;
                    }
                    var msgDateTime = PublicTalkMothed.ConvertStringToDateTime(msgTime);
                    var diff = serverDateTime - GlobalVariable.LastLoginDatetime;
                    isPassword = diff.TotalMinutes > 5;
                    CloseExitVerify();
                    VerifyView = new CheckInVerifyView(isPassword);
                    verifyVm = new CheckInVerifyViewModel(content, null, isPassword);
                    VerifyView.DataContext = verifyVm;
                    verifyVm.VerifyView = VerifyView;
                    verifyVm.VerifySucceed += VerifyVm_VerifySucceed;

                    //if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized))
                    //{
                    VerifyView.Topmost = true;
                    //}
                    //else
                    //{
                    //    VerifyView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    //    if (win != null)
                    //    {
                    //        VerifyView.Owner = win as MainWindowView;
                    //    }
                    //}
                    VerifyView.Show();

                }
                else
                {
                    CloseExitVerify();
                    VerifyResultView view = new VerifyResultView();
                    var hwnd = Win32.GetForegroundWindow();
                    //if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized))
                    //{
                    view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (win != null)
                    {
                        view.Owner = win as MainWindowView;
                    }
                    //}
                    //view.Topmost = true;
                    view.ShowDialog();
                }
            }));
        }
        /// <summary>
        /// 关闭需进行打卡的窗体
        /// </summary>
        public void CloseExitVerify()
        {
            if (verifyVm != null)
            {
                verifyVm.timer?.Stop();
                verifyVm = null;
            }
            if (resultView != null)
            {
                resultView.DataContext = null;
                resultView.Close();
                resultView = null;
            }
            if (VerifyView != null)
            {
                VerifyView.DataContext = null;
                VerifyView.Close();
                VerifyView = null;
            }
        }
        /// <summary>
        /// 验证成功提示
        /// </summary>
        private void VerifyVm_VerifySucceed()
        {
            //VerifyResultView view = new Views.VerifyResultView();
            //view.Topmost = true;
            //view.Show();
        }
        #endregion

        #region 音视频通话相关

        private RequestViewModel requestModel;
        /// <summary>
        /// 根据userId查找用户
        /// </summary>
        /// <param name="accid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private AntSdkContact_User FindTargetUser(string userId)
        {
            var user = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(v => v.userId == userId);
            if (user == null)
            {
                LogHelper.WriteError($"没有找到Userid为{userId}的用户");
            }
            return user;
        }
        /// <summary>
        /// 发起结果处理
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <param name="code">错误码</param>
        public void HandlerSessionStartRes(long channel_id, int code)
        {
            string errMsg = string.Empty;
            if (code == 408)
            {
                errMsg = "请求超时";
                if (AudioChat.audioViewModel != null && GlobalVariable.isAudioShow)
                    AudioChat.audioViewModel.CloseAudio();
                //提示
            }
            if (code == 200)//发送成功
            {
                var targetId = GlobalVariable.currentAudioUserId;
                //通过MQTT发送一条语音电话消息
            }
        }

        /// <summary>
        /// 邀请会话通知处理
        /// </summary>
        /// <param name="channel_id">频道ID</param>
        /// <param name="targetId">目标id</param>
        /// <param name="model">会话类型</param>
        public void HandlerSessionInviteNotify(long channel_id, string targetId, int model)
        {
            var user = FindTargetUser((targetId.ToUpper()));
            if (user == null) return;
            var requestView = new RequestView();
            requestModel = new RequestViewModel(requestView.Close, user, channel_id);
            requestModel.HandlerAcceptRequest += RequestModel_HandlerAcceptRequest;
            requestView.DataContext = requestModel;
            requestView.Show();
            _SessionListViewModel?.AddContactSession(targetId.ToUpper(), true);
            //查找或创建一个会话并打开
        }

        /// <summary>
        /// 接听语音电话请求
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channel_id"></param>
        private void RequestModel_HandlerAcceptRequest(AntSdkContact_User user, long channel_id)
        {
            if (!AntSdkService.AntSdkIsConnected)
            {
                MessageBoxWindow.Show("提示", "网络连接已断开,无法进行语音电话！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            string errMsg = string.Empty;
            if (!AudioChat.CheckDevices(ref errMsg))
            {
                MessageBoxWindow.Show("提示", errMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                AudioChat.AudioResult(channel_id, false);
                return;
            }
            AudioChat.AudioResult(channel_id, true);
            ShowAudioView(user, GlobalVariable.AudioSendOrReceive.Receive);
        }
        /// <summary>
        /// 显示通话页面
        /// </summary>
        /// <param name="user">目标用户</param>
        /// <param name="sendOrReceive">发送or接收</param>
        private void ShowAudioView(AntSdkContact_User user, GlobalVariable.AudioSendOrReceive sendOrReceive)
        {
            if (!AntSdkService.AntSdkIsConnected)
            {
                MessageBoxWindow.Show("提示", "网络连接已断开,无法发送语音电话！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            var audioView = new AudioChatView();
            AudioChat.audioViewModel = new AudioChatViewModel(user, sendOrReceive, audioView.Close);
            audioView.DataContext = AudioChat.audioViewModel;
            audioView.Show();
            GlobalVariable.isAudioShow = true;
            GlobalVariable.currentAudioUserId = user.userId;
            DataConverter.SetWindowLocation(win, audioView);
        }
        /// <summary>
        /// 发起后对方响应通知处理
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="mode"></param>
        /// <param name="accept"></param>
        public void HandlerSessionCalleeAckNotify(long channel_id, string uid, int mode, bool accept)
        {
            //语音电话
            if (mode == (int)NIM.NIMVideoChatMode.kNIMVideoChatModeAudio)
            {
                if (accept)
                {
                    AudioChat.audioViewModel?.SetStanckPanel(false);
                    AudioChat.audioViewModel?.UpdateAudioInfo();
                }
                else
                {
                    AudioChat.audioViewModel?.CloseAudio();
                    _SessionListViewModel?.TalkVieAudioMessage(uid.ToUpper(), "已拒绝");
                    //对方拒绝提示
                }
            }
            //视频
            else
            {

            }
        }
        /// <summary>
        /// 连接通知处理
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="msg"></param>
        public void HandlerSessionConnectNotify(long channel_id, string msg)
        {
        }
        /// <summary>
        /// 主动挂断结果处理
        /// </summary>
        /// <param name="channel_id"></param>
        public void HandlerSessionHangupRes(long channel_id)
        {
            if (AudioChat.audioViewModel != null && GlobalVariable.isAudioShow)
                AudioChat.audioViewModel.CloseAudio();
            if (AudioChat.audioViewModel == null || AudioChat.audioViewModel._targetUser == null) return;
            if (AudioChat._currentChannelId == 0) return;
            var targetId = AudioChat.audioViewModel._targetUser.userId;
            var strMsg = AudioChat.audioViewModel.IsTalking ? "通话已结束" : "已取消";
            _SessionListViewModel?.TalkVieAudioMessage(targetId, strMsg);
            AudioChat._currentChannelId = 0;
            //聊天框消息提示
        }
        /// <summary>
        /// 对方挂断通知处理
        /// </summary>
        /// <param name="channel_id"></param>
        public void HandlerSessionHangupNotify(long channel_id)
        {
            if (requestModel != null && requestModel.ModelId == channel_id)
            {
                requestModel.CloseWin();
                if (requestModel._targetUser == null) return;
                var targetId = requestModel._targetUser.userId;
                _SessionListViewModel?.TalkVieAudioMessage(targetId, "对方已取消");
            }
            if (AudioChat.audioViewModel != null && GlobalVariable.isAudioShow)
            {
                AudioChat.audioViewModel.CloseAudio();
                if (AudioChat.audioViewModel._targetUser == null) return;
                var targetId = AudioChat.audioViewModel._targetUser.userId;
                _SessionListViewModel?.TalkVieAudioMessage(targetId, "通话已结束");
            }
        }
        /// <summary>
        /// 处理控制操作通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="type"></param>
        public void HandlerSessionControlNotify(long channel_id, string uid, int type)
        {
            switch (type)
            {
                case (int)NIM.NIMVChatControlType.kNIMTagControlBusyLine://对方占线
                    {
                        if (AudioChat.audioViewModel != null && GlobalVariable.isAudioShow)
                            AudioChat.audioViewModel.CloseAudio();
                        _SessionListViewModel?.TalkViewAudio(uid.ToUpper(), "对方正在语音通话中");
                        AudioChat.End();
                        //对方占线提示
                        break;
                    }
            }
        }

        /// <summary>
        /// 其他端处理结果
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        /// <param name="uid"></param>
        /// <param name="mode"></param>
        /// <param name="accept"></param>
        /// <param name="time"></param>
        /// <param name="client"></param>
        public void HandlerSessionSyncAckNotify(long channel_id, int code, string uid, int mode, bool accept, long time,
            int client)
        {
            if (mode == (int)NIM.NIMVideoChatMode.kNIMVideoChatModeAudio)
            {
                if (requestModel != null && requestModel.ModelId == channel_id)
                    requestModel.CloseWin();
                if (string.IsNullOrEmpty(uid)) return;
                if (accept)
                    _SessionListViewModel?.TalkViewAudio(uid.ToUpper(), "已在其他设备接听");
                else
                {
                    _SessionListViewModel?.TalkVieAudioMessage(uid.ToUpper(), "已拒绝");
                }
                AudioChat._currentChannelId = 0;
            }
            else
            {
                //视频
            }
        }

        /// <summary>
        /// 处理网络连接状态
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="status"></param>
        /// <param name="uid"></param>
        public void HandlerSessionNetStatus(long channel_id, int status, string uid)
        {
            switch (status)
            {
                case (int)NIM.NIMVideoChatSessionNetStat.kNIMVideoChatSessionNetStatVeryBad: //网络状态极差
                    {
                        AudioChat._currentChannelId = 0;
                        _SessionListViewModel?.TalkViewAudio(uid.ToUpper(), "网络中断，请检查您的网络");
                        HandlerSessionHangupRes(channel_id);
                        //网络连接异常提示
                        AudioChat.End();
                        break;
                    }
            }
        }

        #endregion
    }
}
