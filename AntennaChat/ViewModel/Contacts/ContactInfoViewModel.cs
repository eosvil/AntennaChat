using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class ContactInfoViewModel : PropertyNotifyObject
    {
        public bool IsMouseClick = false;
        public AntSdkContact_User User;
        public GroupInfoViewModel GroupInfoVM;
        public GlobalVariable.ContactInfoViewContainer Container;
        /// <summary>
        /// 用于处理多音字，将匹配的首字母组合传入
        /// 高亮
        /// </summary>
        private string pinYin = string.Empty;
        /// <summary>
        /// 搜索关键字
        /// </summary>
        private string keyWord = "";
        #region 构造器
        public ContactInfoViewModel(AntSdkContact_User user, GlobalVariable.ContactInfoViewContainer container)
        {
            //if (!string.IsNullOrWhiteSpace(user?.picture))
            //{
            //    this.Photo = user.picture;
            //}
            //else
            //{
            //    this.Photo = "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";
            //}
            this.Name = user?.userNum + user?.userName;
            this.Position = user?.position;
            if (user == null)
                user = new AntSdkContact_User();
            this.User = user;
            this.Container = container;
            if (container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft || Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft)
            {
                StateImageVisibility = Visibility.Visible;
                StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/默认.png";
                ContextMenuVisibility = Visibility.Collapsed;
            }
            else if (container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight || Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight)
            {
                StateImageVisibility = Visibility.Visible;
                StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/人员删除-1.png";
                ContextMenuVisibility = Visibility.Collapsed;
            }
            else
            {
                StateImageVisibility = Visibility.Collapsed;
                ContextMenuVisibility = Visibility.Visible;
            }
            if (user.userId == AntSdkService.AntSdkLoginOutput.userId)
            {
                SendMsgVisibility = Visibility.Collapsed;
            }
            //if (!string.IsNullOrEmpty(user.state))
            //{
            //    var state = int.Parse(user.state);
            if (AntSdkService.AntSdkCurrentUserInfo.robotId == user.userId)
                user.state = (int)GlobalVariable.OnLineStatus.OnLine;
            if (!AntSdkService.AntSdkIsConnected)
            {
                UserOnlineStateIcon = "";
            }
            else if (user.state != (int)GlobalVariable.OnLineStatus.OffLine)
            {
                if (GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic.ContainsKey(user.state))
                {
                    UserOnlineStateIcon = GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic[user.state];
                }
            }
            //}
            SetContactPhoto();


        }
        /// <summary>
        /// 设置联系人头像
        /// </summary>
        private void SetContactPhoto(bool isGroup = false)
        {
            //AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            //{
            if (!isGroup)
            {
                try
                {

                    if (!string.IsNullOrWhiteSpace(User?.picture) && publicMethod.IsUrlRegex(User.picture))
                    {

                        //var index = User.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                        //var fileNameIndex = User.picture.LastIndexOf(".", StringComparison.Ordinal);
                        //var fileName = User.picture.Substring(index, fileNameIndex - index);
                        //string strUrl = User.picture.Replace(fileName, fileName + "_35x35");
                        //this.Photo = publicMethod.IsUrlRegex(strUrl) ? strUrl : GlobalVariable.DefaultImage.UserHeadDefaultImage;

                        if (publicMethod.IsUrlRegex(User.picture))
                        {
                            var userImage =
                                GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                    m => m.UserID == User.userId);
                            this.Photo = string.IsNullOrEmpty(userImage?.Url) ? User.picture : userImage.Url;
                        }
                        else
                        {
                            this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                        }
                        //DownloadImage(User.picture);
                    }
                    else
                    {
                        this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    }
                    //var tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;
                }
                catch (Exception)
                {
                    this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    //var tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;
                }
                if (AntSdkService.AntSdkCurrentUserInfo.robotId == User?.userId)
                {
                    IsOfflineState = !AntSdkService.AntSdkIsConnected;
                }
                else
                {
                    if (!AntSdkService.AntSdkIsConnected)
                    {
                        IsOfflineState = true;
                    }
                    else
                    {
                        IsOfflineState = User?.state == (int)GlobalVariable.OnLineStatus.OffLine;
                    }
                }

            }
            else
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(GroupInfoVM?.GroupInfo?.groupPicture))
                    {
                        this.Photo = GroupInfoVM?.GroupInfo?.groupPicture;
                    }
                    else
                    {
                        this.Photo = GlobalVariable.DefaultImage.GroupHeadDefaultImage;
                    }
                    //var tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;
                }
                catch (Exception)
                {
                    this.Photo = GlobalVariable.DefaultImage.GroupHeadDefaultImage;
                    //var tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;
                }
            }
            //});
        }


        public ContactInfoViewModel(AntSdkContact_User user, GlobalVariable.ContactInfoViewContainer container, string word) : this(user, container)
        {
            this.keyWord = word;
        }

        public ContactInfoViewModel(AntSdkContact_User user, GlobalVariable.ContactInfoViewContainer container, string word,
            string pinyin) : this(user, container, word)
        {
            pinYin = pinyin;
        }
        /// <summary>
        /// 主界面上的直接查询（群组部分的实例化）
        /// </summary>
        /// <param name="groupInfo"></param>
        /// <param name="container"></param>
        /// <param name="word"></param>
        public ContactInfoViewModel(GroupInfoViewModel groupInfoVM, GlobalVariable.ContactInfoViewContainer container, string word)
        {
            this.keyWord = word;
            this.Name = groupInfoVM.GroupInfo.groupName;
            //this.Photo = !string.IsNullOrWhiteSpace(groupInfoVM.GroupInfo.groupPicture) ? groupInfoVM.GroupInfo.groupPicture : "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";

            this.Container = container;
            this.IsGroupContact = true;
            GroupInfoVM = groupInfoVM;
            SetContactPhoto(true);
            ColumnNameWidth = double.NaN;
            StateImageVisibility = Visibility.Collapsed;
            ContextMenuVisibility = Visibility.Visible;
        }

        public ContactInfoViewModel(GroupInfoViewModel groupInfoVM, GlobalVariable.ContactInfoViewContainer container,
            string word, string pinyin) : this(groupInfoVM, container, word)
        {
            this.pinYin = pinyin;
        }
        /// <summary>
        /// 高亮关键字
        /// </summary>
        private void HighLightKeyWord(object obj)
        {
            if (string.IsNullOrEmpty(this.keyWord)) return;
            int starIndex = -1;
            if (!DataConverter.InputIsChinese(keyWord) && !DataConverter.InputIsNum(keyWord))
            {
                if (!string.IsNullOrEmpty(pinYin))
                    starIndex = pinYin.IndexOf(keyWord);
            }
            else
                starIndex = Name.ToLower().IndexOf(keyWord);
            if (starIndex != -1)
            {
                TextBlock tb = (TextBlock)obj;
                TextEffect tfe = new TextEffect();
                tfe.Foreground = new SolidColorBrush(Colors.Red);
                tfe.PositionStart = starIndex;
                tfe.PositionCount = keyWord.Length;
                tb.TextEffects = new TextEffectCollection();
                tb.TextEffects.Add(tfe);
            }
        }
        #endregion

        #region 属性
        private string _Photo;
        /// <summary>
        /// 头像
        /// </summary>
        public string Photo
        {
            get { return this._Photo; }
            set
            {
                this._Photo = value;
                RaisePropertyChanged(() => Photo);
            }
        }

        private BitmapImage _contactPhoto;
        /// <summary>
        /// 头像
        /// </summary>
        public BitmapImage ContactPhoto
        {
            get { return _contactPhoto; }
            set
            {
                _contactPhoto = value;
                RaisePropertyChanged(() => ContactPhoto);
            }
        }

        private bool _isOfflineState;
        /// <summary>
        /// 是否是离线状态
        /// </summary>
        public bool IsOfflineState
        {
            get
            {
                return _isOfflineState;
            }
            set
            {
                _isOfflineState = value;
                RaisePropertyChanged(() => IsOfflineState);
            }
        }

        private string _Name;
        /// <summary>
        /// 头像
        /// </summary>
        public string Name
        {
            get { return this._Name; }
            set
            {
                this._Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private string _Position;
        /// <summary>
        /// 头像
        /// </summary>
        public string Position
        {
            get { return this._Position; }
            set
            {
                this._Position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        private Brush _Background;
        /// <summary>
        /// 背景色
        /// </summary>
        public Brush Background
        {
            get { return this._Background; }
            set
            {
                this._Background = value;
                RaisePropertyChanged(() => Background);
            }
        }

        private double _PlaceholderWidth = 20;
        /// <summary>
        /// 占位符宽度
        /// </summary>
        public double PlaceholderWidth
        {
            get { return this._PlaceholderWidth; }
            set
            {
                this._PlaceholderWidth = value;
                RaisePropertyChanged(() => PlaceholderWidth);
            }
        }

        private Visibility _ContextMenuVisibility = Visibility.Visible;
        /// <summary>
        /// 占位符宽度
        /// </summary>
        public Visibility ContextMenuVisibility
        {
            get { return this._ContextMenuVisibility; }
            set
            {
                this._ContextMenuVisibility = value;
                RaisePropertyChanged(() => ContextMenuVisibility);
            }
        }

        private Visibility _StateImageVisibility = Visibility.Collapsed;
        /// <summary>
        /// 状态图的可见性
        /// </summary>
        public Visibility StateImageVisibility
        {
            get { return this._StateImageVisibility; }
            set
            {
                this._StateImageVisibility = value;
                RaisePropertyChanged(() => StateImageVisibility);
            }
        }


        private Visibility _SendMsgVisibility = Visibility.Visible;
        /// <summary>
        /// 右键发送消息的可见性
        /// </summary>
        public Visibility SendMsgVisibility
        {
            get { return this._SendMsgVisibility; }
            set
            {
                this._SendMsgVisibility = value;
                RaisePropertyChanged(() => SendMsgVisibility);
            }
        }

        private string _StateImageSource;
        /// <summary>
        /// 状态图的地址
        /// </summary>
        public string StateImageSource
        {
            get { return this._StateImageSource; }
            set
            {
                this._StateImageSource = value;
                RaisePropertyChanged(() => StateImageSource);
            }
        }

        private bool isGroupContact;
        /// <summary>
        /// 是否是群组会话
        /// </summary>
        public bool IsGroupContact
        {
            get
            {
                return isGroupContact;
            }
            set
            {
                if (value != isGroupContact)
                {
                    isGroupContact = value;
                    if (isGroupContact)
                    {
                        GetUserInfoVisibility = Visibility.Collapsed;
                        RaisePropertyChanged(() => GetUserInfoVisibility);
                    }
                }
            }
        }

        private double _columnNameWidth = 50;
        /// <summary>
        /// 名字的列宽度（当前会话为群组时，列宽度变宽）
        /// </summary>
        public double ColumnNameWidth
        {
            get { return _columnNameWidth; }
            set
            {
                _columnNameWidth = value;
                RaisePropertyChanged(() => ColumnNameWidth);
            }
        }


        private bool _isFocused = false;
        /// <summary>
        /// 是否获得当前焦点
        /// </summary>
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                //if (_isFocused)
                //{
                //    if (!this.IsMouseClick)
                //    {
                //        Background = (Brush) (new BrushConverter()).ConvertFromString("#e4f2fb");
                //    }
                //}
                //else
                //{
                //    if (this.IsMouseClick)
                //    {
                //        Background = (Brush) (new BrushConverter()).ConvertFromString("#cae9fc");
                //    }
                //    else
                //    {
                //        Background = (Brush) (new BrushConverter()).ConvertFromString("#FFFFFF");
                //    }
                //}
            }
        }

        public Visibility GetUserInfoVisibility { get; set; } = Visibility.Visible;
        #endregion

        #region 命令
        /// <summary>
        /// 加载
        /// </summary>
        private ICommand _LoadedCommand;
        public ICommand LoadedCommand
        {
            get
            {
                if (this._LoadedCommand == null)
                {
                    this._LoadedCommand = new DefaultCommand(o =>
                    {
                        if (o != null)
                        {
                            HighLightKeyWord(o);
                        }
                    });
                }
                return this._LoadedCommand;
            }
        }

        private string _userOnlineStateIcon;
        /// <summary>
        /// 用户在线状态
        /// </summary>
        public string UserOnlineStateIcon
        {
            get { return this._userOnlineStateIcon; }
            set
            {
                this._userOnlineStateIcon = value;
                RaisePropertyChanged(() => UserOnlineStateIcon);
            }
        }

        /// <summary>
        /// 鼠标进入颜色变化
        /// </summary>
        private ICommand _MouseEnter;
        public ICommand MouseEnter
        {
            get
            {
                if (this._MouseEnter == null)
                {
                    this._MouseEnter = new DefaultCommand(o =>
                    {
                        if (!this.IsMouseClick)
                        {
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#F5F5F5");
                        }
                    });
                }
                return this._MouseEnter;
            }
            set { _MouseEnter = value; }
        }
        /// <summary>
        /// 鼠标移出颜色变化
        /// </summary>
        private ICommand _MouseLeave;
        public ICommand MouseLeave
        {
            get
            {
                if (this._MouseLeave == null)
                {
                    this._MouseLeave = new DefaultCommand(o =>
                    {
                        if (this.IsMouseClick)
                        {
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#f0f0f0");
                        }
                        else
                        {
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                        }
                    });
                }
                return this._MouseLeave;
            }
            set { _MouseLeave = value; }
        }

        /// <summary>
        /// 状态图标鼠标进入颜色变化
        /// </summary>
        private ICommand _StateImageMouseEnter;
        public ICommand StateImageMouseEnter
        {
            get
            {
                if (this._StateImageMouseEnter == null)
                {
                    this._StateImageMouseEnter = new DefaultCommand(o =>
                    {
                        if (Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight || Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight)
                        {
                            StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/人员删除-2.png";
                        }
                    });
                }
                return this._StateImageMouseEnter;
            }
        }
        /// <summary>
        /// 状态图标鼠标离开颜色变化
        /// </summary>
        private ICommand _StateImageMouseLeave;
        public ICommand StateImageMouseLeave
        {
            get
            {
                if (this._StateImageMouseLeave == null)
                {
                    this._StateImageMouseLeave = new DefaultCommand(o =>
                    {
                        if (Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight || Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight)
                        {
                            StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/人员删除-1.png";
                        }
                    });
                }
                return this._StateImageMouseLeave;
            }
        }

        public delegate void StateImageClickDelegate(object sender, bool? isSelected);
        public static event StateImageClickDelegate StateImageClickEvent;

        /// <summary>
        /// 鼠标点击右侧图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isSelected">True表示在创建讨论组左侧选中，False表示在左侧取消选择，Null表示在右侧点击</param>
        public void OnStateImageClickEventEvent(object sender, bool? isSelected)
        {
            SetStateImageOnClick(isSelected);
            //if (Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight)
            //{
            //    OnStateImageClickEventEvent((o as ContactInfoView).DataContext, null);
            //}
            if (StateImageClickEvent != null)
            {
                StateImageClickEvent(sender, isSelected);
            }
        }

        public void SetStateImageOnClick(bool? isSelected)
        {
            if (isSelected == true)
            {
                if (this.User.userId == AntSdkService.AntSdkLoginOutput.userId)
                {
                    StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/选中-灰色.png";
                }
                else if (StateImageSource != "pack://application:,,,/AntennaChat;Component/Images/选中-灰色.png")
                {
                    StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/选中.png";
                }
                this.IsMouseClick = true;
                Background = (Brush)(new BrushConverter()).ConvertFromString("#f0f0f0");
                //if(StateImageClickEvent!=null)
                //{
                //    StateImageClickEvent(sender, isSelected);
                //}
                //OnStateImageClickEventEvent((o as ContactInfoView).DataContext, true);
            }
            else if (isSelected == false)
            {
                StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/默认.png";
                Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                this.IsMouseClick = false;
                //OnStateImageClickEventEvent((o as ContactInfoView).DataContext, false);
            }
        }

        public void SetExistGroupMember()
        {
            StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/选中-灰色.png";
            this.IsMouseClick = true;
            Background = (Brush)(new BrushConverter()).ConvertFromString("#f0f0f0");
        }

        /// <summary>
        /// 控件状态图变更
        /// </summary>
        private ICommand _StateImageMouseLeftButtonDown;
        public ICommand StateImageMouseLeftButtonDown
        {
            get
            {
                if (this._StateImageMouseLeftButtonDown == null)
                {
                    this._StateImageMouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        if (Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft &&
                            this.User.userId == AntSdkService.AntSdkLoginOutput.userId)
                        {
                            return;
                        }
                        bool? isSelected = null;
                        if (Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft || Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft)
                        {
                            if (this.User.userId == AntSdkService.AntSdkLoginOutput.userId || StateImageSource == "pack://application:,,,/AntennaChat;Component/Images/选中-灰色.png")
                            {
                                isSelected = true;
                                //StateImageSource = "pack://application:,,,/AntennaChat;Component/Images/选中-灰色.png";
                            }
                            else if (StateImageSource == "pack://application:,,,/AntennaChat;Component/Images/默认.png")
                            {
                                isSelected = true;
                            }
                            else if (StateImageSource == "pack://application:,,,/AntennaChat;Component/Images/选中.png")
                            {
                                isSelected = false;
                            }
                        }
                        OnStateImageClickEventEvent((o as ContactInfoView).DataContext, isSelected);
                    });
                }
                return this._StateImageMouseLeftButtonDown;
            }
        }

        //public delegate void MouseClickDelegate(object contactInfoViewModel);
        public static event EventHandler MouseDoubleClickEvent;
        public void OnMouseDoubleClickEvent()
        {
            if (!IsGroupContact)
            {
                if (User.userId == AntSdkService.AntSdkLoginOutput.userId) return;
                MouseDoubleClickEvent?.Invoke(User.userId, EventArgs.Empty);
            }
            else
            {
                MouseDoubleClickEvent?.Invoke(GroupInfoVM, EventArgs.Empty);
            }
        }
        private ICommand _MouseDoubleClick;
        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        public ICommand MouseDoubleClick
        {
            get
            {
                if (this._MouseDoubleClick == null)
                {
                    this._MouseDoubleClick = new DefaultCommand(o =>
                    {
                        if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
                        {

                            OnMouseDoubleClickEvent();
                        }
                    });
                }
                return this._MouseDoubleClick;
            }
        }

        /// <summary>
        /// 查看联系人信息
        /// </summary>
        private ICommand _GetUserInfo;
        public ICommand GetUserInfo
        {
            get
            {
                if (this._GetUserInfo == null)
                {
                    this._GetUserInfo = new DefaultCommand(o =>
                    {
                        if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
                        {
                            object dataContext = (o as ContactInfoView).DataContext;
                            Win_UserInfoView win = new Win_UserInfoView();
                            win.ShowInTaskbar = false;
                            Win_UserInfoViewModel model = new Win_UserInfoViewModel((dataContext as ContactInfoViewModel).User.userId);
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.ShowDialog();
                        }
                    });
                }
                return this._GetUserInfo;
            }
        }

        public event EventHandler MouseLeftButtonDownEvent;
        private void OnMouseLeftButtonDown(object contactInfoView)
        {
            MouseLeftButtonDownEvent?.Invoke((contactInfoView as ContactInfoView).DataContext, null);
        }

        /// <summary>
        /// 获取焦点时发生
        /// </summary>
        public static event EventHandler GetFocusedEvent;

        private void OnGetFocused()
        {
            GetFocusedEvent?.Invoke(this, EventArgs.Empty);
        }


        private ICommand _MouseLeftButtonDown;


        /// <summary>
        /// 鼠标单击事件
        /// </summary>
        public ICommand MouseLeftButtonDown
        {
            get
            {
                if (this._MouseLeftButtonDown == null)
                {
                    this._MouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        OnMouseLeftButtonDown(o);
                        //if (!string.IsNullOrEmpty(this.keyWord)) return;
                        //var vm = new StructureDetailsViewModel();
                        //var view = new StructureDetailsView {DataContext = vm};
                        //view.DetailType = DetailType.Personal;
                        //MainWindowViewModel.GoStructureDetail(view);
                        //vm.InitDetails(DetailType.Personal, this);
                    });
                }
                return this._MouseLeftButtonDown;
            }
        }

        #endregion
    }
}
