using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AntennaChat.ViewModel.Talk;
using Microsoft.Expression.Interactivity.Core;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using static Antenna.Framework.GlobalVariable;
using AntennaChat.Views;

namespace AntennaChat.ViewModel.Contacts
{
    public class SessionInfoViewModel : PropertyNotifyObject
    {
        public GlobalVariable.BurnFlag IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
        public SessionType _SessionType;
        public string SessionId;
        public AntSdkContact_User AntSdkContact_User;
        public AntSdkGroupInfo GroupInfo;
        public List<AntSdkGroupMember> GroupMembers;
        public bool IsMouseClick = false;
        public string LastMsgTimeStamp;
        public string LastChatIndex;
        public bool IsRemoveLoaclSession = true;
        /// <summary>
        /// 群聊为false  单聊为true
        /// </summary>
        public bool isPointOrGroup = false;

        #region 构造器
        /// <summary>
        /// 群发助手
        /// </summary>
        public SessionInfoViewModel(AntSdkMassMsgCtt massMsg)
        {
            this.LastChatIndex = massMsg.chatIndex;
            this.SessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId,
                GlobalVariable.MassAssistantId);
            _SessionType = SessionType.MassAssistant;
            this.LastMessage = massMsg.content;
            this.LastMsgTimeStamp = DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000";
            this.LastTime = DataConverter.FormatTimeByTimeStamp(LastMsgTimeStamp);//从消息来的为时间戳  
            this.Photo = DefaultImage.MassAssistantDefaultImage;
            //var tempHeadPicBitmapImage = new BitmapImage();
            //tempHeadPicBitmapImage.BeginInit();
            //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
            //tempHeadPicBitmapImage.EndInit();
            //ContactPhoto = tempHeadPicBitmapImage;
            this.Name = "群发助手";
            this.ImageSendingVisibility = string.IsNullOrEmpty(massMsg.chatIndex) ? Visibility.Visible : Visibility.Collapsed;
            SetUnreadCount(0);
        }
        /// <summary>
        /// 考勤助手
        /// </summary>
        /// <param name="session"></param>
        /// <param name="type"></param>
        public SessionInfoViewModel(AntSdkTsession session, SessionType type)
        {
            if (type == SessionType.AttendanceAssistant)
            {
                this.LastChatIndex = session.LastChatIndex;
                this.SessionId = session.SessionId;
                _SessionType = type;
                if (session.UnreadCount > 0)
                    this.IsNewAttendance = true;
                this.LastMessage = session.LastMsg;
                this.LastMsgTimeStamp = session.LastMsgTimeStamp;
                this.LastTime = DataConverter.FormatTimeByTimeStamp(LastMsgTimeStamp); //从消息来的为时间戳   
                this.Photo = DefaultImage.AttendanceAssistantDefaultImage;
                this.Name = "考勤助手";
                //var tempHeadPicBitmapImage = new BitmapImage();
                //tempHeadPicBitmapImage.BeginInit();
                //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                //tempHeadPicBitmapImage.EndInit();
                //ContactPhoto = tempHeadPicBitmapImage;
                //this.ImageSendingVisibility = string.IsNullOrEmpty(session.LastChatIndex) ? Visibility.Visible : Visibility.Collapsed;
                this.TopIndex = session.TopIndex;
                SetUnreadCount(0);
            }
        }

        public SessionInfoViewModel(AntSdkTsession session)
        {
            if (session.UserId == GlobalVariable.MassAssistantId) //群发助手
            {
                this.LastChatIndex = session.LastChatIndex;
                this.SessionId = session.SessionId;
                _SessionType = SessionType.MassAssistant;
                this.LastMessage = session.LastMsg;
                this.LastMsgTimeStamp = session.LastMsgTimeStamp;
                this.LastTime = DataConverter.FormatTimeByTimeStamp(LastMsgTimeStamp); //从消息来的为时间戳   
                this.Photo = DefaultImage.MassAssistantDefaultImage;
                this.Name = "群发助手";
                //var tempHeadPicBitmapImage = new BitmapImage();
                //tempHeadPicBitmapImage.BeginInit();
                //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                //tempHeadPicBitmapImage.EndInit();
                //ContactPhoto = tempHeadPicBitmapImage;
                this.ImageSendingVisibility = string.IsNullOrEmpty(session.LastChatIndex) ? Visibility.Visible : Visibility.Collapsed;
                this.TopIndex = session.TopIndex;
                SetUnreadCount(0);
            }
        }

        /// <summary>
        /// 点对点聊天消息
        /// </summary>
        public SessionInfoViewModel(AntSdkContact_User antsdkcontact_User, SessionInfoModel model, AntSdkMsgType msgType = AntSdkMsgType.ChatMsgText)
        {
            isPointOrGroup = true;
            _SessionType = SessionType.SingleChat;
            this.AntSdkContact_User = antsdkcontact_User;
            this.SessionId = model.SessionId;
            this.MsgType = msgType;
            //if (!string.IsNullOrWhiteSpace(model.photo))
            //{
            //    this.Photo = model.photo;
            //}
            //else
            //{
            //    this.Photo = "pack://application:,,,/AntennaChat;Component/Images/27-头像.png";
            //}
            this.Name = model.name;
            if (AntSdkContact_User.status == 0 && AntSdkContact_User.state == 0)
                this.Name = model.name + "（停用）";
            this.LastMessage = model.lastMessage;
            this.LastChatIndex = model.lastChatIndex;
            SetUnreadCount(model.unreadCount);
            this.TopIndex = model.topIndex;
            if (string.IsNullOrWhiteSpace(model.lastTime))
            {
                this.LastTime = string.Empty;
            }
            else
            {
                try
                {
                    this.LastTime = DataConverter.FormatTimeByTimeStamp(model.lastTime);//从消息来的为时间戳
                    this.LastMsgTimeStamp = model.lastTime;
                }
                catch
                {
                    //this.LastTime = model.lastTime;//从Sqlite来的为时间格式
                }
            }
            //if (!string.IsNullOrEmpty(antsdkcontact_User.state))
            //{
            //    var state = int.Parse(antsdkcontact_User.state);
            if (AntSdkService.AntSdkCurrentUserInfo.robotId == AntSdkContact_User?.userId)
                antsdkcontact_User.state = (int)GlobalVariable.OnLineStatus.OnLine;
            if (!AntSdkService.AntSdkIsConnected)
            {
                UserOnlineStateIcon = "";
            }
            else if (antsdkcontact_User.state != (int)GlobalVariable.OnLineStatus.OffLine)
            {
                if (UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(antsdkcontact_User.state))
                {
                    UserOnlineStateIcon = UserOnlineSataeInfo.UserOnlineStateIconDic[antsdkcontact_User.state];
                }
            }
            //}
            SetContactPhoto();
        }
        /// <summary>
        /// 设置联系人头像
        /// </summary>
        public void SetContactPhoto(bool isGroup = false)
        {
            //AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            //{
            if (!isGroup)
            {

                try
                {
                    if (!string.IsNullOrWhiteSpace(AntSdkContact_User?.picture))
                    {
                        var index = AntSdkContact_User.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                        var fileNameIndex = AntSdkContact_User.picture.LastIndexOf(".", StringComparison.Ordinal);
                        var fileName = AntSdkContact_User.picture.Substring(index, fileNameIndex - index);
                        string strUrl = AntSdkContact_User.picture.Replace(fileName, fileName + "_100x100");
                        this.Photo = publicMethod.IsUrlRegex(strUrl) ? strUrl : GlobalVariable.DefaultImage.UserHeadDefaultImage;
                        //if (publicMethod.IsUrlRegex(AntSdkContact_User.picture))
                        //{
                        //    var userImage =GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                        //            m => m.UserID == AntSdkContact_User.userId);
                        //    this.Photo = string.IsNullOrEmpty(userImage?.Url) ? AntSdkContact_User.picture : userImage.Url;
                        //}
                        //else
                        //{
                        //    this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                        //}
                    }
                    else
                    {
                        //if (!string.IsNullOrWhiteSpace(AntSdkContact_User?.picture) && File.Exists(AntSdkContact_User?.picture))
                        //    this.Photo = AntSdkContact_User.picture;
                        //else
                        //{
                        this.Photo = DefaultImage.SessionUserHeadDefaultImage;
                        //}

                        //var tempHeadPicBitmapImage = new BitmapImage();
                        //tempHeadPicBitmapImage.BeginInit();
                        //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                        //tempHeadPicBitmapImage.EndInit();
                        //ContactPhoto = tempHeadPicBitmapImage;


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
                    this.Photo = DefaultImage.UserHeadDefaultImage;
                    //var tempHeadPicBitmapImage = new BitmapImage();
                    //tempHeadPicBitmapImage.BeginInit();
                    //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                    //tempHeadPicBitmapImage.EndInit();
                    //ContactPhoto = tempHeadPicBitmapImage;
                }
                if (AntSdkService.AntSdkCurrentUserInfo.robotId == AntSdkContact_User?.userId)
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
                        IsOfflineState = AntSdkContact_User?.state == (int)GlobalVariable.OnLineStatus.OffLine;
                    }
                }

            }
            else
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(GroupInfo.groupPicture) && GroupInfo.groupPicture != DefaultImage.GroupHeadDefaultImage)
                    {
                        this.Photo = GroupInfo.groupPicture;
                        //DownloadImage(GroupInfo.groupPicture,true);
                    }
                    else
                    {
                        this.Photo = DefaultImage.SessionGroupHeadDefaultImage;
                        //var tempHeadPicBitmapImage = new BitmapImage();
                        //tempHeadPicBitmapImage.BeginInit();
                        //tempHeadPicBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        //tempHeadPicBitmapImage.UriSource = new Uri(this.Photo);
                        //tempHeadPicBitmapImage.EndInit();
                        //ContactPhoto = tempHeadPicBitmapImage;
                    }

                }
                catch (Exception)
                {
                    this.Photo = DefaultImage.SessionGroupHeadDefaultImage;
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
        /// <summary>
        /// 下载头像
        /// </summary>
        /// <param name="imgUrl"></param>
        async void DownloadImage(string imgUrl, bool isGroup = false)
        {
            var request = WebRequest.Create(imgUrl);
            using (var response = await request.GetResponseAsync())
            using (var destStream = new MemoryStream())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return;
                var downloadTask = responseStream.CopyToAsync(destStream);
                RefreshUI(downloadTask, destStream, isGroup);
                await downloadTask;
            }
        }
        /// <summary>
        /// 头像下载完成时，更新界面
        /// </summary>
        /// <param name="downloadTask"></param>
        /// <param name="stream"></param>
        async void RefreshUI(Task downloadTask, MemoryStream stream, bool isGroup = false)
        {
            await Task.WhenAny(downloadTask, Task.Delay(1000));            //每隔一秒刷新一次

            var data = stream.ToArray();
            var tmpStream = new MemoryStream(data);        //TODO 当图片的头没有下载到时，这儿可能抛异常
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = tmpStream;
            bmp.EndInit();

            ContactPhoto = bmp;        //刷新图片

            if (!downloadTask.IsCompleted)
            {
                RefreshUI(downloadTask, stream);
            }
            else
            {
                if (!isGroup)
                {
                    if (AntSdkService.AntSdkCurrentUserInfo.robotId == AntSdkContact_User?.userId)
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
                            IsOfflineState = AntSdkContact_User?.state == (int)GlobalVariable.OnLineStatus.OffLine;
                        }
                    }
                }
            }


        }

        /// <summary>
        /// 讨论组聊天消息
        /// </summary>
        public SessionInfoViewModel(AntSdkGroupInfo groupInfo, BurnFlag isBurnMode, List<AntSdkGroupMember> groupMembers, SessionInfoModel model)
        {
            this.IsBurnMode = isBurnMode;
            this.ImageBurnVisibility = isBurnMode == BurnFlag.IsBurn ? Visibility.Visible : Visibility.Collapsed;
            _SessionType = SessionType.GroupChat;
            IsGroup = true;

            this.GroupInfo = groupInfo;
            if (groupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
            {
                MessageNoticeIsChecked = false;
                MessageHideIsChecked = true;
                ImageNoRemindVisibility = Visibility.Visible;
            }
            else
            {
                MessageNoticeIsChecked = true;
                MessageHideIsChecked = false;
                ImageNoRemindVisibility = Visibility.Collapsed;
            }
            if (groupInfo.groupOwnerId == AntSdkService.AntSdkCurrentUserInfo.userId)
            {
                DeleteGroupVisibility = Visibility.Visible;
            }
            if (groupMembers != null && groupMembers.Any())
            {

                this.GroupMembers = groupMembers;
            }
            else
            {
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                //AsyncHandler.Call(Application.Current.Dispatcher, () =>
                //{
                //    this.GroupMembers = GetMembers();
                //},DispatcherPriority.Background);
                stopWatch.Stop();
                LogHelper.WriteDebug(string.Format("[SessionInfoViewModel_GetMembers({0}毫秒)]", stopWatch.Elapsed.TotalMilliseconds));
            }
            this.SessionId = model.SessionId;
            //if (!string.IsNullOrWhiteSpace(model.photo))
            //{
            //    this.Photo = model.photo;
            //}
            //else
            //{
            //    this.Photo = "pack://application:,,,/AntennaChat;Component/Images/44-头像.png";
            //}
            SetContactPhoto(true);
            this.Name = model.name;
            this.LastMessage = model.lastMessage;
            this.LastChatIndex = model.lastChatIndex;
            this.TopIndex = model.topIndex;
            SetUnreadCount(model.unreadCount);
            if (string.IsNullOrWhiteSpace(model.lastTime))
            {
                this.LastTime = string.Empty;
            }
            else
            {
                try
                {
                    this.LastTime = DataConverter.FormatTimeByTimeStamp(model.lastTime);//从消息来的为时间戳
                    this.LastMsgTimeStamp = model.lastTime;
                }
                catch
                {
                    //this.LastTime = model.lastTime;//从Sqlite来的为时间格式
                }
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

        private string _targetId;
        /// <summary>
        /// 点对点目标ID
        /// </summary>
        public string TargetId
        {
            get { return this._targetId; }
            set
            {
                this._targetId = value;
                RaisePropertyChanged(() => TargetId);
            }
        }


        private AntSdkMsgType _msgType = AntSdkMsgType.ChatMsgText;
        /// <summary>
        /// 最后一天消息的消息类型
        /// </summary>
        public AntSdkMsgType MsgType
        {
            get { return this._msgType; }
            set
            {
                this._msgType = value;
                RaisePropertyChanged(() => MsgType);
            }
        }
        private string _Name;
        /// <summary>
        /// 名字
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

        private string _LastMessage;
        /// <summary>
        /// 最后一条消息内容
        /// </summary>
        public string LastMessage
        {
            get { return this._LastMessage; }
            set
            {
                this._LastMessage = value;
                if (value != null && value.StartsWith("[~!@]")) //处理AT消息
                {
                    ImageAtVisibility = Visibility.Visible;
                    this.LastMessageDisplay = value.Substring(5);
                }
                else
                {
                    ImageAtVisibility = Visibility.Collapsed;
                    this.LastMessageDisplay = value;
                }
                RaisePropertyChanged(() => LastMessage);
            }
        }

        private string _LastMessageDisplay;
        /// <summary>
        /// 最后一条消息内容(用于界面展示)
        /// </summary>
        public string LastMessageDisplay
        {
            get { return this._LastMessageDisplay; }
            set
            {
                this._LastMessageDisplay = value;
                RaisePropertyChanged(() => LastMessageDisplay);
            }
        }

        //public int MyselfMsgCount = 0;//由自己账号从其他终端发送过来的消息数（点击打开聊天框时显示的消息数为UnreadCount+MyselfMsgCount）
        private int _UnreadCount;
        /// <summary>
        /// 未读消息数
        /// </summary>
        public int UnreadCount
        {
            get { return this._UnreadCount; }
            set
            {
                this._UnreadCount = value;
                RaisePropertyChanged(() => UnreadCount);
            }
        }

        private int _UnreadCountWidth = 16;
        /// <summary>
        /// 未读消息数显示宽度
        /// </summary>
        public int UnreadCountWidth
        {
            get { return this._UnreadCountWidth; }
            set
            {
                this._UnreadCountWidth = value;
                RaisePropertyChanged(() => UnreadCountWidth);
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
        private int _UnreadCountRectangleWidth = 0;
        /// <summary>
        /// 未读消息数显示宽度
        /// </summary>
        public int UnreadCountRectangleWidth
        {
            get { return this._UnreadCountRectangleWidth; }
            set
            {
                this._UnreadCountRectangleWidth = value;
                RaisePropertyChanged(() => UnreadCountRectangleWidth);
            }
        }

        private Visibility _ImageBurnVisibility = Visibility.Collapsed;
        /// <summary>
        /// 阅后即焚图标可见性
        /// </summary>
        public Visibility ImageBurnVisibility
        {
            get { return this._ImageBurnVisibility; }
            set
            {
                this._ImageBurnVisibility = value;
                RaisePropertyChanged(() => ImageBurnVisibility);
            }
        }

        private Visibility _UnreadCountVisibility;
        /// <summary>
        /// 未读消息数可见性 
        /// </summary>
        public Visibility UnreadCountVisibility
        {
            get { return this._UnreadCountVisibility; }
            set
            {
                this._UnreadCountVisibility = value;
                RaisePropertyChanged(() => UnreadCountVisibility);
            }
        }

        private Visibility _ImageNoRemindVisibility = Visibility.Collapsed;
        /// <summary>
        /// 消息免打扰图标
        /// </summary>
        public Visibility ImageNoRemindVisibility
        {
            get { return this._ImageNoRemindVisibility; }
            set
            {
                this._ImageNoRemindVisibility = value;
                RaisePropertyChanged(() => ImageNoRemindVisibility);
            }
        }

        private Visibility _ImageNoticeVisibility = Visibility.Collapsed;
        /// <summary>
        /// 群通知图标
        /// </summary>
        public Visibility ImageNoticeVisibility
        {
            get { return this._ImageNoticeVisibility; }
            set
            {
                this._ImageNoticeVisibility = value;
                RaisePropertyChanged(() => ImageNoticeVisibility);
            }
        }

        private Visibility _ImageSendingVisibility = Visibility.Collapsed;
        /// <summary>
        /// 群发消息图标
        /// </summary>
        public Visibility ImageSendingVisibility
        {
            get { return this._ImageSendingVisibility; }
            set
            {
                this._ImageSendingVisibility = value;
                RaisePropertyChanged(() => ImageSendingVisibility);
            }
        }
        private Visibility _ImageFailingVisibility = Visibility.Collapsed;
        /// <summary>
        /// 失败消息图标
        /// </summary>
        public Visibility ImageFailingVisibility
        {
            get { return this._ImageFailingVisibility; }
            set
            {
                this._ImageFailingVisibility = value;
                RaisePropertyChanged(() => ImageFailingVisibility);
            }
        }
        private Visibility _ImageAtVisibility = Visibility.Collapsed;
        /// <summary>
        /// AT消息图标
        /// </summary>
        public Visibility ImageAtVisibility
        {
            get { return this._ImageAtVisibility; }
            set
            {
                this._ImageAtVisibility = value;
                RaisePropertyChanged(() => ImageAtVisibility);
            }
        }

        private Brush _UnreadCountBackground = (Brush)(new BrushConverter()).ConvertFromString("#F55D5B");
        /// <summary>
        /// 未读消息数可见性 
        /// </summary>
        public Brush UnreadCountBackground
        {
            get { return this._UnreadCountBackground; }
            set
            {
                this._UnreadCountBackground = value;
                RaisePropertyChanged(() => UnreadCountBackground);
            }
        }

        private string _LastTime;
        /// <summary>
        /// 收到最后一条消息时间
        /// </summary>
        public string LastTime
        {
            get { return this._LastTime; }
            set
            {
                this._LastTime = value;
                RaisePropertyChanged(() => LastTime);
            }
        }

        private DateTime _msgDatetime;
        /// <summary>
        /// 收到消息的时间日期
        /// </summary>
        public DateTime MsgDatetime
        {
            get { return _msgDatetime; }
            set
            {
                this._msgDatetime = value;
                RaisePropertyChanged(() => MsgDatetime);
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

        private int? _topIndex;
        public int? TopIndex
        {
            get { return _topIndex; }
            set
            {
                _topIndex = value;
                if (_topIndex.HasValue)
                {
                    PostTopCommandVisibility = Visibility.Collapsed;
                    CancelTopCommandVisibility = Visibility.Visible;
                    Background = (Brush)(new BrushConverter()).ConvertFromString("#F5F5F5");
                    RaisePropertyChanged(() => PostTopCommandVisibility);
                    RaisePropertyChanged(() => CancelTopCommandVisibility);
                }
                else
                {
                    PostTopCommandVisibility = Visibility.Visible;
                    CancelTopCommandVisibility = Visibility.Collapsed;
                    Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                    RaisePropertyChanged(() => PostTopCommandVisibility);
                    RaisePropertyChanged(() => CancelTopCommandVisibility);
                }
            }
        }
        /// <summary>
        /// 置顶菜单可见性
        /// </summary>
        public Visibility PostTopCommandVisibility { get; set; } = Visibility.Visible;
        /// <summary>
        /// 取消置顶菜单可见性
        /// </summary>
        public Visibility CancelTopCommandVisibility { get; set; } = Visibility.Collapsed;

        private bool _isShow = true;
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                RaisePropertyChanged(() => IsShow);
            }
        }
        private bool _isGroup;
        public bool IsGroup
        {
            get { return _isGroup; }
            set
            {
                _isGroup = value;
                RaisePropertyChanged(() => IsGroup);
            }
        }
        private Visibility _DeleteGroupVisibility = Visibility.Collapsed;
        /// <summary>
        /// 解散讨论组的可见性
        /// </summary>
        public Visibility DeleteGroupVisibility
        {
            get { return this._DeleteGroupVisibility; }
            set
            {
                this._DeleteGroupVisibility = value;
                RaisePropertyChanged(() => DeleteGroupVisibility);
            }
        }
        private bool _MessageNoticeIsChecked = true;
        /// <summary>
        /// 接收消息并提醒是否选中
        /// </summary>
        public bool MessageNoticeIsChecked
        {
            get { return this._MessageNoticeIsChecked; }
            set
            {
                this._MessageNoticeIsChecked = value;
                RaisePropertyChanged(() => MessageNoticeIsChecked);
            }
        }

        private bool _MessageHideIsChecked = false;
        /// <summary>
        /// 消息不提醒是否选中
        /// </summary>
        public bool MessageHideIsChecked
        {
            get { return this._MessageHideIsChecked; }
            set
            {
                this._MessageHideIsChecked = value;
                RaisePropertyChanged(() => MessageHideIsChecked);
            }
        }

        private bool _isNewAttendance = false;
        /// <summary>
        /// 是否有新记录
        /// </summary>
        public bool IsNewAttendance
        {
            get
            {
                return _isNewAttendance;
            }
            set
            {
                _isNewAttendance = value;
                RaisePropertyChanged(() => IsNewAttendance);
            }
        }
        #endregion

        #region 命令
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
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#F0F0F0");
                        }
                    });
                }
                return this._MouseEnter;
            }
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
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#E0E0E0");
                        }
                        else if (this.TopIndex.HasValue)
                        {
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#F5F5F5");
                        }
                        else
                        {
                            Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                        }
                    });
                }
                return this._MouseLeave;
            }
        }

        //public delegate void MouseClickDelegate(object sessionInfoViewModel);
        public event EventHandler MouseLeftButtonDownEvent;
        private void OnMouseLeftButtonDown(object sessionInfoView)
        {
            if (MouseLeftButtonDownEvent != null)
            {
                SessionInfoView view = sessionInfoView as SessionInfoView;
                //if (_SessionType == SessionType.GroupChat)
                //{
                //    if (this.GroupMembers==null || this.GroupMembers.Count==0)
                //    {
                //        AsyncHandler.Call(Application.Current.Dispatcher, () =>
                //        {
                //            this.GroupMembers = GetMembers();
                //        });
                //    }
                //}
                MouseLeftButtonDownEvent(this, null);
            }
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
                    });
                }
                return this._MouseLeftButtonDown;
            }
        }

        public event EventHandler DeleteSessionEvent;
        private void OnDeleteSession(object sessionInfoView)
        {
            if (DeleteSessionEvent != null)
            {
                DeleteSessionEvent((sessionInfoView as SessionInfoView).DataContext, null);
            }
        }
        private ICommand _DeleteSession;
        /// <summary>
        /// 删除会话
        /// </summary>
        public ICommand DeleteSession
        {
            get
            {
                if (this._DeleteSession == null)
                {
                    this._DeleteSession = new DefaultCommand(o =>
                    {

                        OnDeleteSession(o);
                    });
                }
                return this._DeleteSession;
            }
        }

        /// <summary>
        /// 置顶事件
        /// </summary>
        public event EventHandler PostTopSessionEvent;
        private void OnPostTopSession(object sessionInfoView)
        {
            PostTopSessionEvent?.Invoke((sessionInfoView as SessionInfoView).DataContext, null);
        }

        /// <summary>
        /// 取消置顶事件
        /// </summary>
        public event EventHandler CancelTopSessionEvent;
        private void OnCancelTopSession(object sessionInfoView)
        {
            CancelTopSessionEvent?.Invoke((sessionInfoView as SessionInfoView).DataContext, null);
        }
        /// <summary>
        /// 退出讨论组
        /// </summary>
        private ICommand _DropOutGroup;
        public ICommand DropOutGroup
        {
            get
            {
                if (this._DropOutGroup == null)
                {
                    this._DropOutGroup = new DefaultCommand(o =>
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要退出{0}吗？", GroupInfo.groupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Ok)
                        {
                            GroupPublicFunction.ExitGroup(this.GroupInfo.groupId, this.GroupInfo.groupName, this.GroupMembers);
                        }
                    });
                }
                return this._DropOutGroup;
            }
        }

        /// <summary>
        /// 解散讨论组
        /// </summary>
        private ICommand _DeleteGroup;
        public ICommand DeleteGroup
        {
            get
            {
                if (this._DeleteGroup == null)
                {
                    this._DeleteGroup = new DefaultCommand(o =>
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要解散{0}吗？", GroupInfo.groupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Ok)
                        {
                            GroupPublicFunction.DismissGroup(GroupInfo.groupId);
                        }
                    });
                }
                return this._DeleteGroup;
            }
        }

        /// <summary>
        /// 消息提醒
        /// </summary>
        private ICommand _MessageNoticeCommand;
        public ICommand MessageNoticeCommand
        {
            get
            {
                if (this._MessageNoticeCommand == null)
                {
                    this._MessageNoticeCommand = new DefaultCommand(o =>
                    {
                        MessageNoticeIsChecked = true;
                        if (MessageHideIsChecked)
                        {
                            MessageHideIsChecked = false;
                            ImageNoRemindVisibility = Visibility.Collapsed;
                            SetMsgRemind();
                        }
                    });
                }
                return this._MessageNoticeCommand;
            }
        }

        /// <summary>
        /// 消息不提醒
        /// </summary>
        private ICommand _MessageHideCommand;
        public ICommand MessageHideCommand
        {
            get
            {
                if (this._MessageHideCommand == null)
                {
                    this._MessageHideCommand = new DefaultCommand(o =>
                    {
                        MessageHideIsChecked = true;
                        if (MessageNoticeIsChecked)
                        {
                            MessageNoticeIsChecked = false;
                            ImageNoRemindVisibility = Visibility.Visible;
                            SetMsgRemind();
                        }
                    });
                }
                return this._MessageHideCommand;
            }
        }


        /// <summary>
        /// 置顶
        /// </summary>
        public ICommand PostTopCommand => new ActionCommand(OnPostTopSession);

        /// <summary>
        /// 取消置顶
        /// </summary>
        public ICommand CancelTopCommand => new ActionCommand(OnCancelTopSession);

        #endregion

        #region 其他方法

        /// <summary>
        /// 设置消息提醒方式
        /// </summary>
        public void SetMsgRemind(GlobalVariable.MsgRemind? remind = null)
        {
            AntSdkUpdateGroupConfigInput input = new AntSdkUpdateGroupConfigInput();
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.groupId = this.GroupInfo.groupId;
            BaseOutput output = new BaseOutput();
            var errCode = 0;
            string errMsg = string.Empty;

            if (remind == null)
                input.state = MessageNoticeIsChecked
                    ? ((int)GlobalVariable.MsgRemind.Remind).ToString()
                    : ((int)GlobalVariable.MsgRemind.NoRemind).ToString();
            else
                input.state = ((int)remind.Value).ToString();
            var isResult = AntSdkService.UpdateGroupConfig(input, ref errCode, ref errMsg);
            if (isResult)
            {
                GroupInfo.state = int.Parse(input.state);
                if (GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                {
                    MessageNoticeIsChecked = false;
                    MessageHideIsChecked = true;
                    ImageNoRemindVisibility = Visibility.Visible;
                }
                else
                {
                    MessageNoticeIsChecked = true;
                    MessageHideIsChecked = false;
                    ImageNoRemindVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            }
        }
        public void SetUnreadCount(int count)
        {
            if (GroupInfo != null && GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
            {
                ImageNoRemindVisibility = Visibility.Visible;
            }
            else
            {
                ImageNoRemindVisibility = Visibility.Collapsed;
            }
            if (count >= 0)
                this.UnreadCount = count;
            if (count == 0)
            {
                UnreadCountVisibility = Visibility.Collapsed;
            }
            else
            {
                UnreadCountVisibility = Visibility.Visible;
                if (GroupInfo != null && GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                {
                    this.UnreadCountBackground = (Brush)(new BrushConverter()).ConvertFromString("#22AEFF");
                }
                else
                {
                    this.UnreadCountBackground = (Brush)(new BrushConverter()).ConvertFromString("#F55D5B");
                }
                if (count > 9)
                {
                    UnreadCountWidth = 22;
                    UnreadCountRectangleWidth = 6;
                }
                else
                {
                    UnreadCountWidth = 16;
                    UnreadCountRectangleWidth = 0;
                }
            }
        }

        /// <summary>
        /// 刷新群发助手
        /// </summary>
        /// <param name="massMsgReceipt"></param>
        public void RefreshMassAssistantSession(AntSdkChatMsg.ChatBase massMsgReceipt)
        {
            if (string.IsNullOrEmpty(massMsgReceipt.chatIndex))//新发消息
            {
                this.LastMessage = massMsgReceipt.sourceContent;
                this.LastMsgTimeStamp = DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000";
                this.ImageSendingVisibility = Visibility.Visible;
            }
            else //回执
            {
                this.LastChatIndex = massMsgReceipt.chatIndex;
                this.LastMsgTimeStamp = massMsgReceipt.sendTime;
                this.ImageSendingVisibility = Visibility.Collapsed;
            }
            this.LastTime = DataConverter.FormatTimeByTimeStamp(LastMsgTimeStamp);//从消息来的为时间戳  
        }
        #endregion
    }
}
