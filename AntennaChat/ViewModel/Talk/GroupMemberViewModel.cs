using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.Contacts;
using AntennaChat.Views;
using AntennaChat.Views.Contacts;
using AntennaChat.Views.Talk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Talk
{
    public class GroupMemberViewModel : PropertyNotifyObject
    {
        public string GroupId;
        public AntSdkGroupMember Member;
        private GroupMemberListViewModel Owner;
        private string keyword;
        private string pinYin;
        private AntSdkContact_User contactUser;
        #region 构造器
        public GroupMemberViewModel(AntSdkGroupMember user, string GroupAdminId, string groupId, GroupMemberListViewModel owner, string strKey, int adminCount = 0)
        {
            TalkGroupViewModel.isShowTransferAdminMenu += TalkGroupViewModel_isShowTransferAdminMenu;
            this.Owner = owner;
            this.Member = user;
            this.GroupId = groupId;
            AdminCount = adminCount;
            keyword = strKey;
            //if (!string.IsNullOrWhiteSpace(user.picture))
            //{
            //    this.Photo = user.picture;
            //}
            //else
            //{
            //    this.Photo = "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";
            //}
            if (string.IsNullOrEmpty(user.userNum))
            {
                this.Name = user.userName;
            }
            else
            {
                this.Name = user.userNum + user.userName;
            }
            var cuss = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(m => m.userId == user.userId);
            if (cuss != null && cuss.status == 0 && cuss.state == 0)
            {
                this.Name = this.Name + "（停用）";
            }
            else if (cuss == null)
            {
                this.Name = "离职人员";
            }
            this.Position = user.position;
            this.PromptToolTip = string.Format("{0}({1})", this.Name, this.Position);
            if (GroupAdminId == AntSdkService.AntSdkLoginOutput.userId && user.userId != AntSdkService.AntSdkLoginOutput.userId)
            {
                if (cuss != null && cuss.status == 2)
                {
                    ChangeMangerVisibility = KickoutGroupVisibility = Visibility.Visible;
                    IsSetGroupManager = true;
                }

            }
            if (GroupAdminId == user.userId && cuss != null && cuss.status == 2)
            {
                AdminImageVisibility = Visibility.Visible;
                IsSetGroupManager = false;
            }
            if (user.userId == AntSdkService.AntSdkLoginOutput.userId)
            {
                SendMsgVisibility = Visibility.Collapsed;
            }
            if (Member.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin)
            {
                GroupMenuContent = "解除管理员";
                IsGroupAdminImage = true;
                if (cuss != null && cuss.status == 0)
                    IsSetGroupManager = true;
            }
            if (AdminCount >= 4 && Member.roleLevel != (int)GlobalVariable.GroupRoleLevel.Admin)
            {
                IsSetGroupManager = false;
            }
            //if (AntSdkService.AntSdkCurrentUserInfo.robotId == Member?.userId)
            //{
            //    IsSetGroupManager = false;
            //    ChangeMangerVisibility = KickoutGroupVisibility = Visibility.Collapsed;

            //}
            //if (Member.roleLevel== (int) GlobalVariable.GroupRoleLevel.Admin &&
            //    user.userId != AntSdkService.AntSdkLoginOutput.userId)
            //{
            //    KickoutGroupVisibility = Visibility.Visible;
            //}
            var userinfo = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == user.userId);
            if (userinfo != null)
            {
                contactUser = userinfo;
                if (AntSdkService.AntSdkCurrentUserInfo.robotId == userinfo.userId)
                {
                    IsSetGroupManager = false;
                    ChangeMangerVisibility = KickoutGroupVisibility = Visibility.Collapsed;
                    userinfo.state = (int)GlobalVariable.OnLineStatus.OnLine;
                    Name = userinfo.userName;
                    Position = userinfo.position;
                    this.PromptToolTip = string.Format("{0}({1})", this.Name, this.Position);
                    //    IsOfflineState = !AntSdkService.AntSdkIsConnected;
                }
                SetContactPhoto();
                //else
                //{
                //    if (!AntSdkService.AntSdkIsConnected)
                //    {
                //        IsOfflineState = true;
                //    }
                //    else
                //    {
                //        IsOfflineState = userinfo.state == (int)GlobalVariable.OnLineStatus.OffLine;
                //    }

                //}
                //if (!string.IsNullOrEmpty(userinfo?.state))
                //{
                //    var state = int.Parse(userinfo.state);
                if (!AntSdkService.AntSdkIsConnected)
                {
                    UserOnlineStateIcon = "";
                }
                else if (!IsOfflineState)
                {
                    if (GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic.ContainsKey(userinfo.state))
                    {
                        UserOnlineStateIcon = GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic[userinfo.state];
                    }
                }


                if (AntSdkService.AntSdkCurrentUserInfo.robotId == contactUser?.userId)
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
                        IsOfflineState = contactUser?.state == (int)GlobalVariable.OnLineStatus.OffLine;
                    }
                }
                //}
            }
        }
        /// <summary>
        /// 设置联系人头像
        /// </summary>
        public void SetContactPhoto()
        {

            //AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            //{
            try
            {

                if (!string.IsNullOrWhiteSpace(contactUser?.picture))
                {
                    var index = contactUser.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                    var fileNameIndex = contactUser.picture.LastIndexOf(".", StringComparison.Ordinal);
                    var fileName = contactUser.picture.Substring(index, fileNameIndex - index);
                    string strUrl = contactUser.picture.Replace(fileName, fileName + "_35x35");
                    this.Photo = publicMethod.IsUrlRegex(strUrl) ? strUrl : GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    //if (publicMethod.IsUrlRegex(contactUser.picture))
                    //{
                    //    var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                    //            m => m.UserID == contactUser.userId);
                    //    this.Photo = string.IsNullOrEmpty(userImage?.Url) ? contactUser.picture : userImage.Url;
                    //}
                    //else
                    //{
                    //    this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    //}
                    // DownloadImage(Member.picture);
                }
                else
                {
                    this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    //BinaryReader binReader = new BinaryReader(File.Open(this.Photo, FileMode.Open));
                    //FileInfo fileInfo = new FileInfo(this.Photo);
                    //byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                    //binReader.Close();
                    //BitmapImage tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.StreamSource = new MemoryStream(bytes);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;

                }

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

        }
        public GroupMemberViewModel(AntSdkGroupMember user, string GroupAdminId, string groupId,
            GroupMemberListViewModel owner, string strKey, string PinYin)
            : this(user, GroupAdminId, groupId, owner, strKey)
        {
            pinYin = PinYin;
        }

        private void TalkGroupViewModel_isShowTransferAdminMenu(object sender, EventArgs e)
        {
            bool b = (bool)sender;
            if (b)
            {
                ChangeMangerVisibility = Visibility.Visible;
                IsSetGroupManager = true;
            }
            else
            {
                ChangeMangerVisibility = Visibility.Collapsed;
                IsSetGroupManager = false;
            }
        }
        #endregion
        //管理员转让成功阅后即焚图标显示
        public static event EventHandler SwitchBurnImage;

        private void switchImage()
        {
            if (SwitchBurnImage != null)
            {
                SwitchBurnImage(true, null);
            }
        }
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
        private string _Name;
        /// <summary>
        /// 姓名
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
        /// 岗位
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

        private string _PromptToolTip;
        /// <summary>
        /// 岗位
        /// </summary>
        public string PromptToolTip
        {
            get { return this._PromptToolTip; }
            set
            {
                this._PromptToolTip = value;
                RaisePropertyChanged(() => PromptToolTip);
            }
        }

        private string _groupMenuContent = "设置为管理员";
        /// <summary>
        /// 管理员设置菜单Header
        /// </summary>
        public string GroupMenuContent
        {
            get { return this._groupMenuContent; }
            set
            {
                _groupMenuContent = value;
                RaisePropertyChanged(() => GroupMenuContent);
            }
        }

        private bool _isSetGroupManager;
        /// <summary>
        /// 是否可设置为管理员
        /// </summary>
        public bool IsSetGroupManager
        {
            get { return _isSetGroupManager; }
            set
            {
                _isSetGroupManager = value;
                RaisePropertyChanged(() => IsSetGroupManager);
            }
        }


        private Brush _Background;
        /// 背景色
        public Brush Background
        {
            get { return this._Background; }
            set
            {
                this._Background = value;
                RaisePropertyChanged(() => Background);
            }
        }

        private Visibility _KickoutGroupVisibility = Visibility.Collapsed;
        /// <summary>
        /// 踢出讨论组按钮是否可见（只有管理员可见）
        /// </summary>
        public Visibility KickoutGroupVisibility
        {
            get { return this._KickoutGroupVisibility; }
            set
            {
                this._KickoutGroupVisibility = value;
                RaisePropertyChanged(() => KickoutGroupVisibility);
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

        private Visibility _changeMangerVisibility = Visibility.Collapsed;
        /// <summary>
        /// 右键转交管理员权限的可见性(与右键踢出讨论组的可见性一致)
        /// </summary>
        public Visibility ChangeMangerVisibility
        {
            get { return this._changeMangerVisibility; }
            set
            {
                this._changeMangerVisibility = value;
                RaisePropertyChanged(() => ChangeMangerVisibility);
            }
        }

        private Visibility _adminImageVisibility = Visibility.Hidden;
        /// <summary>
        /// 群主图标显示的可见性
        /// </summary>
        public Visibility AdminImageVisibility
        {
            get { return this._adminImageVisibility; }
            set
            {
                this._adminImageVisibility = value;
                RaisePropertyChanged(() => AdminImageVisibility);
            }
        }

        private bool _isGroupAdminImage;
        /// <summary>
        /// 是否是群管理员
        /// </summary>
        public bool IsGroupAdminImage
        {
            get { return _isGroupAdminImage; }
            set
            {
                this._isGroupAdminImage = value;
                RaisePropertyChanged(() => IsGroupAdminImage);
            }
        }
        /// <summary>
        /// 管理员数量
        /// </summary>
        public int AdminCount { get; set; }

        #endregion

        #region 命令
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
                        object dataContext = (o as GroupMemberView).DataContext;
                        Win_UserInfoView win = new Win_UserInfoView();
                        win.ShowInTaskbar = false;
                        Win_UserInfoViewModel model = new Win_UserInfoViewModel((dataContext as GroupMemberViewModel).Member.userId);
                        win.DataContext = model;
                        win.Owner = Antenna.Framework.Win32.GetTopWindow();
                        win.ShowDialog();
                    });
                }
                return this._GetUserInfo;
            }
        }
        /// <summary>
        /// 鼠标进入事件
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
                        Background = (Brush)(new BrushConverter()).ConvertFromString("#f0f0f0");
                    });
                }
                return this._MouseEnter;
            }
        }
        /// <summary>
        /// 鼠标离开事件
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
                        Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                    });
                }
                return this._MouseLeave;
            }
        }
        /// <summary>
        /// 踢出讨论组
        /// </summary>
        private ICommand _KickoutGroup;
        public ICommand KickoutGroup
        {
            get
            {
                if (this._KickoutGroup == null)
                {
                    this._KickoutGroup = new DefaultCommand(o =>
                    {
                        //UpdateGroupInput input = new UpdateGroupInput();
                        //input.token = AntSdkService.AntSdkLoginOutput.token;
                        //input.version = GlobalVariable.Version;
                        //input.userId = AntSdkService.AntSdkLoginOutput.userId;
                        //input.groupId = this.GroupId;
                        //input.deleteUserIds = this.Member.userId;
                        //input.delUserNames = this.Member.userName;

                        //BaseOutput output = new BaseOutput();

                        AntSdkUpdateGroupInput input = new AntSdkUpdateGroupInput();
                        input.userId = AntSdkService.AntSdkLoginOutput.userId;
                        input.groupId = this.GroupId;
                        if (input.delUserNames == null)
                            input.delUserNames = new List<string>();
                        input.delUserNames.Add(this.Member.userName);
                        if (input.deleteUserIds == null)
                            input.deleteUserIds = new List<string>();
                        input.deleteUserIds.Add(this.Member.userId);
                        var errCode = 0;
                        string errMsg = string.Empty;
                        //TODO:AntSdk_Modify
                        //DONE:AntSdk_Modify
                        var isResult = AntSdkService.UpdateGroup(input, ref errCode, ref errMsg);
                        if (isResult)
                        {
                            List<string> pictureList = Owner.GroupMemberControlList.Where(c => c.Member.userId != this.Member.userId).Select(c => c.Photo).ToList();
                            input.groupPicture = ImageHandle.GetGroupPicture(pictureList);
                            string[] ThreadParams = new string[3];
                            ThreadParams[0] = this.GroupId;
                            ThreadParams[1] = input.groupPicture;
                            ThreadParams[2] = string.IsNullOrEmpty(input.groupName) ? "" : input.groupName;
                            Thread UpdateGroupPictureThread = new Thread(GroupPublicFunction.UpdateGroupPicture);
                            UpdateGroupPictureThread.Start(ThreadParams);
                        }
                        else
                        {
                            MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                        }
                        //if ((new HttpService()).UpdateGroup(input, ref output, ref errMsg))
                        //{
                        //    List<string> pictureList = Owner.GroupMemberControlList.Where(c => c.Member.userId != this.Member.userId).Select(c => c.Photo).ToList();
                        //    input.groupPicture = ImageHandle.GetGroupPicture(pictureList);
                        //    string[] ThreadParams = new string[2];
                        //    ThreadParams[0] = this.GroupId;
                        //    ThreadParams[1] = input.groupPicture;
                        //    Thread UpdateGroupPictureThread = new Thread(UpdateGroupPicture);
                        //    UpdateGroupPictureThread.Start(ThreadParams);
                        //    //OnKickoutGroupEvent(this.GroupId, this.Member.userId, input.groupPicture);
                        //}
                        //else
                        //{
                        //    if (output.errorCode != "1004")
                        //    {
                        //        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                        //    }
                        //}
                    });
                }
                return this._KickoutGroup;
            }
        }

        /// <summary>
        /// 管理员转让
        /// </summary>
        public ICommand ChangeManagerCommand
        {
            get
            {
                return new DefaultCommand(o =>
                {
                    if (
                        MessageBoxWindow.Show("提示", string.Format("把群主转让给\"{0}\"吗？", Name), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) ==
                        GlobalVariable.ShowDialogResult.Ok)
                    {
                        string errMsg = string.Empty;
                        if (ChangeAdmin(ref errMsg))
                        {
                            ChangeManagerCompletedEvent?.Invoke(GroupId);
                            DismissGroupHandlerHidden?.Invoke(GroupId, null);
                        }
                        else
                        {
                            LogHelper.WriteError(errMsg);
                            MessageBoxWindow.Show("提示", errMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        }
                    }
                });
            }
        }
        /// <summary>
        /// 设置群组管理员
        /// </summary>
        public ICommand SetGroupManagerCommand
        {
            get
            {
                return new DefaultCommand(o =>
                {
                    var errCode = 0;
                    var errMsg = string.Empty;
                    var changeResult = false;
                    AntSdkGroupManagerChangeInput groupManagerChangeInput = new AntSdkGroupManagerChangeInput();
                    groupManagerChangeInput.groupId = GroupId;
                    groupManagerChangeInput.newOwnerId = Member.userId;
                    groupManagerChangeInput.userId = AntSdkService.AntSdkLoginOutput.userId;
                    if (Member.roleLevel == (int)GlobalVariable.GroupRoleLevel.Ordinary)
                    {

                        if (MessageBoxWindow.Show("提示", $"确定要设置\"{Name}\"为管理员吗？",
                            MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) ==
                            GlobalVariable.ShowDialogResult.Ok)
                        {
                            groupManagerChangeInput.roleLevel = (int)GlobalVariable.GroupRoleLevel.Admin;
                            changeResult = AntSdkService.GroupManagerSet(groupManagerChangeInput, ref errCode, ref errMsg);
                            if (changeResult)
                            {

                            }
                            else
                            {
                                LogHelper.WriteError(errMsg);
                                MessageBoxWindow.Show("提示", $"设置\"{Name}\"为管理员失败！", MessageBoxButton.OK,
                                    GlobalVariable.WarnOrSuccess.Warn);
                            }
                        }
                    }
                    else
                    {
                        if (MessageBoxWindow.Show("提示", $"确定要取消\"{Name}\"的管理员资格吗？",
                            MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) ==
                            GlobalVariable.ShowDialogResult.Ok)
                        {
                            groupManagerChangeInput.roleLevel = (int)GlobalVariable.GroupRoleLevel.Ordinary;
                            changeResult = AntSdkService.GroupManagerSet(groupManagerChangeInput, ref errCode, ref errMsg);
                            if (changeResult)
                            {

                            }
                            else
                            {
                                LogHelper.WriteError(errMsg);
                                MessageBoxWindow.Show("提示", $"取消\"{Name}\"的管理员资格失败！", MessageBoxButton.OK,
                                    GlobalVariable.WarnOrSuccess.Warn);
                            }
                        }
                    }

                });
            }
        }
        public static event EventHandler DismissGroupHandlerHidden;

        private void DismissGroupHandler(string sessionid)
        {
            DismissGroupHandlerHidden?.Invoke(sessionid, null);
        }



        public delegate void KickoutGroupDelegate(string groupId, string userId, string picture);
        public static event KickoutGroupDelegate KickoutGroupEvent;
        private void OnKickoutGroupEvent(string groupId, string userId, string picture)
        {
            if (KickoutGroupEvent != null)
            {
                KickoutGroupEvent(this.GroupId, this.Member.userId, picture);
            }
        }

        public delegate void ChangeManagerCompletedDelegate(string groupId);

        public static event ChangeManagerCompletedDelegate ChangeManagerCompletedEvent;
        /// <summary>
        /// 管理员转让
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool ChangeAdmin(ref string errMsg)
        {
            bool changeResult = false;
            try
            {
                //ChangeGroupAdminIn input = new ChangeGroupAdminIn()
                //{
                //    groupId = GroupId,
                //    newManagerId = Member.userId,
                //    token = AntSdkService.AntSdkLoginOutput.token,
                //    userId = AntSdkService.AntSdkLoginOutput.userId,
                //    version = GlobalVariable.Version
                //};
                //ChangeGroupAdminOut output = new ChangeGroupAdminOut();
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                AntSdkGroupOwnerChangeInput groupManageChangeInput = new AntSdkGroupOwnerChangeInput();
                groupManageChangeInput.groupId = GroupId;
                groupManageChangeInput.newOwnerId = Member.userId;
                groupManageChangeInput.userId = AntSdkService.AntSdkLoginOutput.userId;
                var errorCode = 0;
                changeResult = AntSdkService.GroupOwnerChange(groupManageChangeInput, ref errorCode, ref errMsg);
                //changeResult=(new HttpService()).ChangeGroupAdmin(input, ref output, ref errMsg);
                //转让管理员成功显示阅后即焚图标
                if (changeResult == true)
                {
                    switchImage();
                }
            }
            catch (Exception ex)
            {
                changeResult = false;
                errMsg = $"变更管理员时发生异常：{ex.Message}";
            }
            return changeResult;
        }


        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        //public delegate void MouseClickDelegate(object contactInfoViewModel);
        public static event EventHandler MouseDoubleClickEvent;
        public void OnMouseDoubleClickEvent(object sender)
        {
            object dataContext = null;
            if (sender is GroupMemberView)
                dataContext = (sender as GroupMemberView).DataContext;
            else
                dataContext = sender;
            if ((dataContext as GroupMemberViewModel).Member.userId == AntSdkService.AntSdkLoginOutput.userId) return;
            if (MouseDoubleClickEvent != null)
            {
                MouseDoubleClickEvent(dataContext, null);
            }
        }
        private ICommand _MouseDoubleClick;
        public ICommand MouseDoubleClick
        {
            get
            {
                if (this._MouseDoubleClick == null)
                {
                    this._MouseDoubleClick = new DefaultCommand(o =>
                    {
                        OnMouseDoubleClickEvent(o);
                    });
                }
                return this._MouseDoubleClick;
            }
        }
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
        /// <summary>
        /// 高亮关键字
        /// </summary>
        private void HighLightKeyWord(object obj)
        {
            if (string.IsNullOrEmpty(this.keyword)) return;

            int starIndex = -1;
            if (!DataConverter.InputIsChinese(keyword) && !DataConverter.InputIsNum(keyword))
            {
                if (!string.IsNullOrEmpty(this.pinYin))
                    starIndex = pinYin.IndexOf(keyword);
            }
            else
                starIndex = Name.ToLower().IndexOf(keyword);
            if (starIndex != -1)
            {
                TextBlock tb = (TextBlock)obj;
                TextEffect tfe = new TextEffect();
                tfe.Foreground = new SolidColorBrush(Colors.Red);
                tfe.PositionStart = starIndex;
                tfe.PositionCount = keyword.Length;
                tb.TextEffects = new TextEffectCollection();
                tb.TextEffects.Add(tfe);
            }
        }
        #endregion
    }
}
