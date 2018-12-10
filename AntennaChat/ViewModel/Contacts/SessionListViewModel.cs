using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.Notice;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using AntennaChat.Views.Contacts;
using AntennaChat.Views.Notice;
using AntennaChat.Views.Talk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AntennaChat.Helper;
using static Antenna.Framework.GlobalVariable;
using static Antenna.Model.SendMessageDto;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;
using static SDK.AntSdk.AntModels.AntSdkReceivedGroupMsg;
using talkGroupModel = AntennaChat.ViewModel.Talk.TalkGroupViewModel;
using talkPersonModel = AntennaChat.ViewModel.Talk.TalkViewModel;
using System.IO;
using AntennaChat.Resource;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel.Contacts
{
    public class SessionListViewModel : PropertyNotifyObject
    {
        private MassMsgListView _massMsgListView;
        private CheckInListView _checkInListView;
        public SessionListView sessionListView;
        public bool curSessionUnreadMsg = false;//当前会话有未读消息
        double scrollHeight = 0;
        private bool isFirst;
        MainWindowViewModel mainWindowViewModel;
        List<Notice_content> UnreadNoticeList = new List<Notice_content>();//未读通知列表
        private BaseBLL<AntSdkMassMsgCtt, T_MassMsgDAL> t_massMsgBll;
        BaseBLL<AntSdkTsession, T_SessionDAL> t_sessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
        //BaseBLL<T_NoRemindGroup, T_NoRemindGroupDAL> t_NoRemindGroupBll = new BaseBLL<T_NoRemindGroup, T_NoRemindGroupDAL>();
        Dispatcher dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        private Dictionary<string, object> dicTalkViewModel = new Dictionary<string, object>();//key为目标联系人ID或讨论组ID
        private Dictionary<string, TalkWindowView> dictWindows = new Dictionary<string, TalkWindowView>();
        private Dictionary<string, TalkGroupWindowView> dictGroupWindows = new Dictionary<string, TalkGroupWindowView>();
        public ObservableCollection<SessionInfoViewModel> _SessionControlList = new ObservableCollection<SessionInfoViewModel>();
        private T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
        T_Chat_Message_GroupDAL t_groupChat = new T_Chat_Message_GroupDAL();
        public int? MaxTopIndex = null;
        private IList<AntSdkTsession> t_SessionList;
        private List<string> BurnSessionList = new List<string>();

        private ObservableCollection<SessionInfoViewModel> tempSessionInfoViewModels =
            new ObservableCollection<SessionInfoViewModel>();
        /// <summary>
        /// 会话列表
        /// </summary>
        public ObservableCollection<SessionInfoViewModel> SessionControlList
        {
            get { return this._SessionControlList; }
            set
            {
                this._SessionControlList = value;
                RaisePropertyChanged(() => SessionControlList);
            }
        }
        #region 构造器
        public SessionListViewModel()
        {

        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mainWindowViewModel"></param>
        public void IninSessionVM(MainWindowViewModel mainWindowViewModel)
        {
            //注册事件
            TalkViewModel.updateFailMessageEventHandler += TalkViewModel_updateFailMessageEventHandler;
            TalkGroupViewModel.updateFailMessageEventHandler += TalkGroupViewModel_updateFailMessageEventHandler;
            TalkGroupViewModel.callbackId.CreateSessionEvent += ContactInfoViewMouseDoubleClick;
            TalkGroupViewModel.PopupCreateSessionEventHandler += TalkGroupViewModel_PopupCreateSessionEventHandler;
            ContactInfoViewModel.MouseDoubleClickEvent += ContactInfoViewMouseDoubleClick;
            GroupInfoViewModel.MouseDoubleClickEvent += GroupInfoViewMouseDoubleClick;
            GroupMemberViewModel.MouseDoubleClickEvent += GroupMemberViewMouseClickEvent;
            GroupInfoViewModel.DropOutGroupEvent += DropOutGroup;
            TalkGroupViewModel.ExitGroupEvent += DropOutGroup;
            TalkGroupViewModel.InviteToGroupEvent += AddNewMember;
            GroupMemberViewModel.KickoutGroupEvent += KickOutGroup;
            TalkGroupViewModel.OnAtMsgChanged += TalkGroupViewModel_OnAtMsgChanged;
            GroupInfoViewModel.SetMsgRemindEvent += SetMsgRemind;
            MassMsgSentViewModel.SendMassMsgEvent += SendMassMsg;
            MassMsgViewModel.SendMassMsgEvent += SendMassMsg;
            TalkViewModel.SentBurnAfterReadEvent += HandleMsessageDestroyReceipt;

            //ContactInfoViewModel.MouseDownEvent += ModifyColorOnMouseClick;
            this.mainWindowViewModel = mainWindowViewModel;
            //ClearMainViewRightPart();
            //InitSessionList();
            StartWaitingTimer();
        }

        private void StructureDetailsViewModel_SendMsgHandler(string arg1, string userid)
        {
            if (!string.IsNullOrEmpty(userid))
            {
                AddContactSession(userid, true);
            }
        }

        private void TalkGroupViewModel_PopupCreateSessionEventHandler(object sender, EventArgs e)
        {
            if (sender is string)
            {
                string userid = (sender as string);
                if (!string.IsNullOrEmpty(userid))
                {
                    AddContactSession(userid, true);
                }
            }
            else if (sender is GroupInfoViewModel)
            {
                var groupInfoVm = sender as GroupInfoViewModel;
                AddGroupSession(groupInfoVm.GroupInfo, true);
            }
        }

        /// <summary>
        /// 是否显示@
        /// </summary>
        /// <param name="sessionID">消息ID</param>
        private void TalkGroupViewModel_OnAtMsgChanged(string sessionID)
        {
            var sessionViewModel = GetSessionInfoViewModelById(sessionID);
            if (sessionViewModel != null)
                sessionViewModel.ImageAtVisibility = Visibility.Collapsed;
        }

        public void UnsubscribeOnClosingWindow()
        {
            TalkGroupViewModel.callbackId.CreateSessionEvent -= ContactInfoViewMouseDoubleClick;
            ContactInfoViewModel.MouseDoubleClickEvent -= ContactInfoViewMouseDoubleClick;
            GroupInfoViewModel.MouseDoubleClickEvent -= GroupInfoViewMouseDoubleClick;
            GroupMemberViewModel.MouseDoubleClickEvent -= GroupMemberViewMouseClickEvent;
            GroupInfoViewModel.DropOutGroupEvent -= DropOutGroup;
            TalkGroupViewModel.ExitGroupEvent -= DropOutGroup;
            TalkGroupViewModel.InviteToGroupEvent -= AddNewMember;
            GroupMemberViewModel.KickoutGroupEvent -= KickOutGroup;
            TalkGroupViewModel.OnAtMsgChanged -= TalkGroupViewModel_OnAtMsgChanged;
            GroupInfoViewModel.SetMsgRemindEvent -= SetMsgRemind;
            MassMsgSentViewModel.SendMassMsgEvent -= SendMassMsg;
            MassMsgViewModel.SendMassMsgEvent -= SendMassMsg;
            TalkViewModel.SentBurnAfterReadEvent -= HandleMsessageDestroyReceipt;
            //StructureDetailsViewModel.SendMsgHandler -= StructureDetailsViewModel_SendMsgHandler;
        }

        /// <summary>
        /// 初始化会话列表（从本地Sqlite获取数据）
        /// </summary>
        public void InitSessionList()
        {
            try
            {
                //AsyncHandler.CallFuncWithUI<IList<AntSdkTsession>>(System.Windows.Application.Current.Dispatcher,
                //    () =>
                //    {
                t_SessionList = t_sessionBll.GetList();
                //if (t_SessionList == null || t_SessionList.Count == 0) return null;
                if (t_SessionList != null)
                {
                    t_SessionList =
                        t_SessionList.OrderByDescending(p => p.TopIndex)
                            .ThenByDescending(p => PublicMessageFunction.LastMsgTime(p.LastMsgTimeStamp, p.BurnLastMsgTimeStamp))
                            .ToList();
                    //IList<T_NoRemindGroup> t_NoRemindGroupList = t_NoRemindGroupBll.GetList();
                    MaxTopIndex = t_SessionList[0].TopIndex;
                }
                //return t_SessionList;
                //}, (ex, datas) =>
                //{
                string sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.robotId, AntSdkService.AntSdkCurrentUserInfo.userId);
                if (t_SessionList == null || t_SessionList.Count == 0)
                {
                    AddRobotSession();
                    return;
                }
                SessionInfoViewModel tempVM = null;
                var sessionList = t_SessionList.ToList();
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                if (GlobalVariable.CurrentUserIsFirstLogin)
                {
                    AddRobotSession();
                }
                List<SessionInfoViewModel> sessionInfoList = new List<SessionInfoViewModel>(sessionList.Count);
                int i = 0;
                foreach (var session in sessionList)
                {

                    var tempSession = session;
                    //t_sessionBll.GetModelByKey(session.SessionId);
                    //if (tempSession.SessionId != sessionId)
                    //    continue;
                    if (string.IsNullOrEmpty(tempSession.BurnLastMsgTimeStamp) && string.IsNullOrEmpty(tempSession.LastMsgTimeStamp))
                        continue;
                    if (i > 99)
                    {
                        t_sessionBll.Delete(tempSession);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(tempSession.UserId))
                    {
                        #region 点对点聊天或群发助手

                        if (tempSession.UserId == GlobalVariable.MassAssistantId) //群发助手
                        {
                            //dispatcher.Invoke(new Action(() =>
                            //{
                            tempVM = new SessionInfoViewModel(tempSession);
                            tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                            tempVM.DeleteSessionEvent += DeleteSession;
                            tempVM.PostTopSessionEvent += PostTopSession;
                            tempVM.CancelTopSessionEvent += CacelTopSession;
                            //SessionControlList.Add(tempVM);
                            SessionControlList.Remove(tempVM);
                            AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
                            //}));
                        }
                        else if (tempSession.UserId == GlobalVariable.AttendAssistantId)//考勤助手
                        {
                            var sessionInfo = GetSessionInfoViewModelById(tempSession.SessionId);
                            if (sessionInfo != null) continue;
                            tempVM = new SessionInfoViewModel(tempSession, SessionType.AttendanceAssistant);
                            tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                            tempVM.DeleteSessionEvent += DeleteSession;
                            tempVM.PostTopSessionEvent += PostTopSession;
                            tempVM.CancelTopSessionEvent += CacelTopSession;
                            //SessionControlList.Add(tempVM);
                            SessionControlList.Remove(tempVM);
                            AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
                        }
                        else //点对点聊天
                        {
                            var sessionInfo = GetSessionInfoViewModelById(tempSession.SessionId);
                            if (sessionInfo != null) continue;
                            AntSdkContact_User AntSdkContact_User =
                                AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                                    c => c.userId == tempSession.UserId);
                            if (AntSdkContact_User == null)
                            {
                                t_sessionBll.Delete(tempSession);
                                continue;
                            }

                            SessionInfoModel model = new SessionInfoModel();
                            model.SessionId = tempSession.SessionId;
                            //model.lastMessage = FormatLastMessageContent(mtp, chatMsg.content);
                            var chatIndex = 0;
                            var burnChatIndex = 0;
                            if (!string.IsNullOrEmpty(tempSession.LastChatIndex))
                                int.TryParse(tempSession.LastChatIndex, out chatIndex);
                            if (!string.IsNullOrEmpty(tempSession.BurnLastChatIndex))
                                int.TryParse(tempSession.BurnLastChatIndex, out burnChatIndex);
                            if (burnChatIndex > chatIndex)
                            {
                                model.lastMessage = tempSession.BurnLastMsg;
                                model.lastTime = tempSession.BurnLastMsgTimeStamp;
                                model.lastChatIndex = tempSession.BurnLastChatIndex;
                            }
                            else
                            {
                                model.lastMessage = tempSession.LastMsg;
                                model.lastTime = tempSession.LastMsgTimeStamp;
                                model.lastChatIndex = tempSession.LastChatIndex;
                            }
                            model.photo = AntSdkContact_User.picture;
                            //model.unreadCount = SessionMonitor.MessageCount(model.SessionId, model.lastChatIndex, 0);
                            model.unreadCount = tempSession.UnreadCount;
                            LogHelper.WriteDebug("---------------------------SessionListViewModel个人:" +
                                                tempSession.SessionId + " 本地未读消息数：" + tempSession.UnreadCount);

                            model.topIndex = tempSession.TopIndex;
                            if (string.IsNullOrEmpty(AntSdkContact_User.userNum))
                            {
                                model.name = AntSdkContact_User.userName;

                            }
                            else
                            {
                                model.name = AntSdkContact_User.userNum + AntSdkContact_User.userName;
                            }

                            //model.MyselfMsgCount = session.MyselfMsgCount;
                            //dispatcher.Invoke(new Action(() =>
                            //{
                            tempVM = new SessionInfoViewModel(AntSdkContact_User, model, AntSdkMsgType.ChatMsgText);
                            tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                            tempVM.DeleteSessionEvent += DeleteSession;
                            tempVM.PostTopSessionEvent += PostTopSession;
                            tempVM.CancelTopSessionEvent += CacelTopSession;
                            //if (tempSession.SessionId == sessionId)
                            //{
                            //SessionControlList.Add(tempVM);
                            SessionControlList.Remove(tempVM);
                            AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
                            //}
                            //SessionControlList.Add(tempVM);
                            //}));
                        }

                        #endregion
                    }
                    else if (!string.IsNullOrEmpty(tempSession.GroupId))
                    {
                        var sessionInfo = GetSessionInfoViewModelById(tempSession.SessionId);
                        if (sessionInfo != null) continue;
                        #region 群聊

                        var grouplist = SessionMonitor.GroupListViewModel?.GroupInfos;
                        var groupInfo = grouplist != null && grouplist.Any() ? grouplist.FirstOrDefault(
                                c => c.groupId == tempSession.GroupId) : null;
                        if (groupInfo == null)
                        {
                            t_sessionBll.Delete(tempSession);
                            continue;
                        }
                        SessionInfoModel model = new SessionInfoModel();
                        model.SessionId = tempSession.SessionId;
                        model.name = groupInfo.groupName;
                        model.photo = groupInfo.groupPicture;
                        model.topIndex = tempSession.TopIndex;
                        if (tempSession.IsBurnMode == (int)BurnFlag.IsBurn)
                        {
                            model.lastMessage = tempSession.BurnLastMsg;
                            model.lastChatIndex = tempSession.BurnLastChatIndex;
                            model.lastTime = tempSession.BurnLastMsgTimeStamp;
                            model.unreadCount = tempSession.BurnUnreadCount;
                            //if (session.IsBurnMode == 0)
                            //{
                            //    session.IsBurnMode = 1;
                            //    t_sessionBll.Update(session);
                            //}
                        }
                        else
                        {

                            model.lastMessage = tempSession.LastMsg;
                            model.lastChatIndex = tempSession.LastChatIndex;
                            model.lastTime = tempSession.LastMsgTimeStamp;
                            model.unreadCount = tempSession.UnreadCount;
                        }

                        //dispatcher.Invoke(new Action(() =>
                        //{
                        var groupInfoLst = SessionMonitor.GroupListViewModel?.GroupInfoList;
                        var groupInfoVM = groupInfoLst != null && groupInfoLst.Count > 0 ? groupInfoLst.FirstOrDefault(
                              m => m.GroupInfo != null && m.GroupInfo.groupId == groupInfo.groupId) : null;
                        var groupMembers = groupInfoVM?.Members != null && groupInfoVM.Members.Any()
                            ? groupInfoVM.Members
                            : new List<AntSdkGroupMember>();
                        tempVM = new SessionInfoViewModel(groupInfo,
                            tempSession.IsBurnMode == (int)BurnFlag.IsBurn
                                ? BurnFlag.IsBurn
                                : BurnFlag.NotIsBurn, groupMembers, model);
                        tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                        tempVM.DeleteSessionEvent += DeleteSession;
                        tempVM.PostTopSessionEvent += PostTopSession;
                        tempVM.CancelTopSessionEvent += CacelTopSession;
                        //SessionControlList.Add(tempVM);
                        //if ((tempSession.IsBurnMode == (int)BurnFlag.IsBurn &&
                        //     tempSession.BurnUnreadCount == 0) ||
                        //    (tempSession.IsBurnMode == (int)BurnFlag.NotIsBurn &&
                        //     tempSession.UnreadCount == 0))
                        //{
                        //SessionControlList.Add(tempVM);
                        SessionControlList.Remove(tempVM);
                        AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
                        //}
                        //}), DispatcherPriority.Background);
                        //if (session.UnreadCount + session.BurnUnreadCount > 0 && groupInfoVM.GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                        //{
                        //    T_NoRemindGroup temp = t_NoRemindGroupBll.GetModelByKey(session.GroupId) ?? new T_NoRemindGroup();
                        //    temp.BurnLastMsg = session.BurnLastMsg;
                        //    temp.BurnLastMsgTimeStamp = session.BurnLastMsgTimeStamp;
                        //    temp.BurnUnreadCount = session.BurnUnreadCount;
                        //    temp.BurnLastChatIndex = session.BurnLastChatIndex;
                        //    temp.LastMsg = session.LastMsg;
                        //    temp.LastMsgTimeStamp = session.LastMsgTimeStamp;
                        //    temp.UnreadCount = session.UnreadCount;
                        //    temp.LastChatIndex = session.LastChatIndex;
                        //    if (string.IsNullOrEmpty(temp.GroupId))
                        //    {
                        //        temp.GroupId = session.GroupId;
                        //        t_NoRemindGroupBll.Insert(temp);
                        //    }
                        //    else
                        //    {
                        //        t_NoRemindGroupBll.Update(temp);
                        //    }
                        //    groupInfoVM.SetUnreadCount(session.UnreadCount);
                        //}

                        #endregion
                    }
                    i++;
                }
                //});
                //});
            }
            catch (Exception e)
            {
                LogHelper.WriteError(e.Message + "," + e.StackTrace);
            }
        }
        /// <summary>
        /// 首次登陆添加消息列表机器人
        /// </summary>
        /// <param name="sessionId"></param>
        private void AddRobotSession()
        {
            // string robotSessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.robotId, AntSdkService.AntSdkCurrentUserInfo.userId);
            string attendSessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, GlobalVariable.AttendAssistantId);
            if (GlobalVariable.CurrentUserIsFirstLogin)
            {
                //var tempRobotSession = t_sessionBll.GetModelByKey(robotSessionId);
                //var sessionInfo = GetSessionInfoViewModelById(robotSessionId);
                //if (tempRobotSession == null && sessionInfo == null)//机器人
                //{
                //    AntSdkContact_User AntSdkContact_User =
                //        AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                //            c => c.userId == AntSdkService.AntSdkCurrentUserInfo.robotId);
                //    if (AntSdkContact_User != null)
                //    {
                //        tempRobotSession = new AntSdkTsession();
                //        SessionInfoModel model = new SessionInfoModel();
                //        model.SessionId = robotSessionId;

                //        //model.lastMessage = FormatLastMessageContent(mtp, chatMsg.content);
                //        model.lastMessage = GlobalVariable.RevocationPrompt.RobotFirstMsg;
                //        model.lastTime = DataConverter.ConvertDateTimeInt(DateTime.Now) + "000";
                //        model.lastChatIndex = "0";
                //        model.photo = AntSdkContact_User.picture;
                //        model.unreadCount = 1;

                //        if (string.IsNullOrEmpty(AntSdkContact_User.userNum))
                //        {
                //            model.name = AntSdkContact_User.userName;

                //        }
                //        else
                //        {
                //            model.name = AntSdkContact_User.userNum + AntSdkContact_User.userName;
                //        }

                //        //model.MyselfMsgCount = session.MyselfMsgCount;
                //        //dispatcher.Invoke(new Action(() =>
                //        //{
                //        var tempInfoVM = new SessionInfoViewModel(AntSdkContact_User, model,
                //            AntSdkMsgType.ChatMsgText);
                //        tempInfoVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                //        tempInfoVM.DeleteSessionEvent += DeleteSession;
                //        tempInfoVM.PostTopSessionEvent += PostTopSession;
                //        tempInfoVM.CancelTopSessionEvent += CacelTopSession;
                //        SessionControlList.Remove(tempInfoVM);
                //        AddSessionControl(tempInfoVM.LastMsgTimeStamp, tempInfoVM);
                //        tempRobotSession.SessionId = robotSessionId;
                //        tempRobotSession.LastMsg = model.lastMessage;
                //        tempRobotSession.LastMsgTimeStamp = model.lastTime;
                //        tempRobotSession.LastChatIndex = model.lastChatIndex;
                //        tempRobotSession.UserId = AntSdkService.AntSdkCurrentUserInfo.robotId; ;
                //        tempRobotSession.UnreadCount = 1;
                //        t_sessionBll.Insert(tempRobotSession);
                //    }


                var tempAttendSession = t_sessionBll.GetModelByKey(attendSessionId);
                var attendSessionInfo = GetSessionInfoViewModelById(attendSessionId);
                if (tempAttendSession == null && attendSessionInfo == null)//考勤
                {
                    tempAttendSession = new AntSdkTsession();
                    SessionInfoModel model = new SessionInfoModel();
                    model.SessionId = attendSessionId;

                    //model.lastMessage = FormatLastMessageContent(mtp, chatMsg.content);
                    model.lastMessage = "打卡记录";
                    model.lastTime = DataConverter.ConvertDateTimeInt(DateTime.Now) + "000";
                    model.lastChatIndex = "0";
                    //model.photo = AntSdkContact_User.picture;
                    //model.unreadCount = 1;
                    tempAttendSession.SessionId = attendSessionId;
                    tempAttendSession.LastMsg = model.lastMessage;
                    tempAttendSession.LastMsgTimeStamp = model.lastTime;
                    tempAttendSession.LastChatIndex = model.lastChatIndex;
                    tempAttendSession.UserId = GlobalVariable.AttendAssistantId;
                    //tempRobotSession.UnreadCount = 1;

                    var tempInfoVM = new SessionInfoViewModel(tempAttendSession, SessionType.AttendanceAssistant); ;
                    tempInfoVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                    tempInfoVM.DeleteSessionEvent += DeleteSession;
                    tempInfoVM.PostTopSessionEvent += PostTopSession;
                    tempInfoVM.CancelTopSessionEvent += CacelTopSession;
                    AddSessionControl(tempInfoVM.LastMsgTimeStamp, tempInfoVM);
                    t_sessionBll.Insert(tempAttendSession);
                }
            }

        }

        #endregion

        #region 命令/方法
        private ICommand _Loaded;
        /// <summary>
        /// 加载窗体命令
        /// </summary>
        public ICommand LoadedCommand
        {
            get
            {
                if (this._Loaded == null)
                {
                    this._Loaded = new DefaultCommand(o =>
                    {

                        sessionListView = o as SessionListView; //RefreshSource();
                        sessionListView.scrollViewer.ScrollToVerticalOffset(scrollHeight);
                        sessionListView.scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                    });
                }
                return this._Loaded;
            }
        }
        /// <summary>
        /// 设置滚动条位置
        /// </summary>
        public void ScrollToVerticalOffset()
        {
            sessionListView?.scrollViewer?.ScrollToVerticalOffset(scrollHeight);
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!isFirst)
                scrollHeight = e.VerticalOffset;
            else
                isFirst = false;
        }

        private DateTime currentDay = DateTime.Now;
        DispatcherTimer waitingTimer;
        /// <summary>
        /// 每隔30分钟刷新一次消息列表时间
        /// </summary>
        /// 作者：赵雪峰 20160528
        private void StartWaitingTimer()
        {
            waitingTimer = new DispatcherTimer();
            waitingTimer.Tick += waitingTimer_Tick;
            waitingTimer.Interval = TimeSpan.FromMinutes(30);
            waitingTimer.Start();
        }
        private void waitingTimer_Tick(object sender, EventArgs e)
        {
            //如果没跨天不刷新
            if (currentDay.Date < DateTime.Now.Date)
                currentDay = DateTime.Now;
            else
                return;
            if (SessionControlList == null || SessionControlList.Count == 0) return;
            foreach (SessionInfoViewModel vm in SessionControlList)
            {
                if (!string.IsNullOrEmpty(vm.LastMsgTimeStamp))
                {
                    vm.LastTime = DataConverter.FormatTimeByTimeStamp(vm.LastMsgTimeStamp);
                }
            }
        }

        private SessionInfoViewModel sessionInfoViewModel;
        /// <summary>
        /// 列表控件点击事件（刷新控件颜色）
        /// </summary>
        /// <param name="ID"></param>
        public void SessionViewMouseLeftButtonDown(object sender, EventArgs e)
        {
            if (SessionControlList == null) return;
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                try
                {
                    for (int i = 0; i < SessionControlList.Count; i++)
                    {
                        var control = SessionControlList[i];
                        if (sender == control)
                        {
                            ScrollToVerticalOffset(i);
                            //sessionListView.ScrollViewer.ScrollToVerticalOffset(i * 64);
                            sessionInfoViewModel = control as SessionInfoViewModel;
                            if (sessionInfoViewModel != null)
                            {
                                if (sessionInfoViewModel.IsMouseClick)
                                    return;
                                sessionInfoViewModel.Background =
                                    (Brush)(new BrushConverter()).ConvertFromString("#e0e0e0");
                                sessionInfoViewModel.IsMouseClick = true;
                                //if (sessionInfoViewModel.GroupInfo != null && sessionInfoViewModel.GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                                //{
                                //    T_NoRemindGroup noRemindGroup = new T_NoRemindGroup();
                                //    noRemindGroup.GroupId = sessionInfoViewModel.SessionId;
                                //    t_NoRemindGroupBll.Delete(noRemindGroup);
                                //    GroupInfoViewModel groupInfoVM = this.mainWindowViewModel._GroupListViewModel.GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == noRemindGroup.GroupId);
                                //    if (groupInfoVM != null)
                                //    {
                                //        groupInfoVM.SetUnreadCount(0);
                                //    }
                                //}

                                AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionInfoViewModel.SessionId);
                                int unReadCount = 0;
                                int burnUnreadCount = 0;
                                if (tSession != null)
                                {
                                    unReadCount = tSession.UnreadCount;
                                    burnUnreadCount = tSession.BurnUnreadCount;
                                }
                                int count = sessionInfoViewModel.UnreadCount;

                                if (sessionInfoViewModel._SessionType == SessionType.GroupChat) //群聊
                                {
                                    var memberList = SessionMonitor.GroupListViewModel?.GroupInfoList;
                                    List<AntSdkGroupMember> groupMembers = null;
                                    if (memberList != null && memberList.Count > 0)
                                    {
                                        var groupInfo =
                                            memberList.FirstOrDefault(
                                                m => m.GroupInfo?.groupId == sessionInfoViewModel.SessionId);
                                        if (groupInfo != null && groupInfo.Members?.Count > 0)
                                            sessionInfoViewModel.GroupMembers = groupInfo.Members;
                                    }
                                    groupMembers = sessionInfoViewModel.GroupMembers;
                                    LogHelper.WriteWarn(
                                 "---------------------------[SessionViewMouseLeftButtonDown:]" +
                                 sessionInfoViewModel.SessionId + "未读消息消息列表计数个数---------------------" +
                                 sessionInfoViewModel.UnreadCount);
                                    //PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, AntSdkReceiptType.ReadReceipt);
                                    sessionInfoViewModel.SetUnreadCount(0);
                                    RefreshMainViewRightPart(sessionInfoViewModel.GroupInfo,
                                        groupMembers, sessionInfoViewModel.IsBurnMode,
                                        unReadCount, burnUnreadCount);


                                    ShowLastUnreadNotice(sessionInfoViewModel.SessionId);
                                }
                                else if (sessionInfoViewModel._SessionType == SessionType.SingleChat) //单聊
                                {
                                    LogHelper.WriteWarn(
                                      "---------------------------[SessionViewMouseLeftButtonDown:]" +
                                      sessionInfoViewModel.SessionId + "未读消息消息列表计数个数---------------------" +
                                      sessionInfoViewModel.UnreadCount);
                                    //PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, AntSdkReceiptType.ReadReceipt);
                                    sessionInfoViewModel.SetUnreadCount(0);
                                    RefreshMainViewRightPart(sessionInfoViewModel.AntSdkContact_User, count,
                                        sessionInfoViewModel.LastMsgTimeStamp);
                                }
                                else if (sessionInfoViewModel._SessionType == SessionType.MassAssistant) //群发助手
                                {
                                    WindowMonitor.ChanageWindowHelper(null);
                                    RefreshMainViewRightPart();
                                }
                                else if (sessionInfoViewModel._SessionType == SessionType.AttendanceAssistant)//考勤助手
                                {
                                    WindowMonitor.ChanageWindowHelper(null);
                                    sessionInfoViewModel.IsNewAttendance = false;
                                    RefreshAttendanceRecordViewRightPart();
                                }
                                if (count > 0 || unReadCount > 0 || burnUnreadCount > 0)
                                {

                                    if (tSession != null)
                                    {
                                        if (tSession.IsBurnMode == (int)BurnFlag.IsBurn)
                                        {
                                            tSession.BurnUnreadCount = 0;
                                        }
                                        else
                                        {
                                            tSession.UnreadCount = 0;
                                        }
                                        t_sessionBll.Update(tSession);
                                    }
                                }
                            }
                        }
                        else
                        {
                            SessionInfoViewModel sessionInfoViewModel = control as SessionInfoViewModel;
                            if (sessionInfoViewModel != null)
                            {
                                if (!sessionInfoViewModel.TopIndex.HasValue)
                                    sessionInfoViewModel.Background =
                                        (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                                else
                                    sessionInfoViewModel.Background =
                                        (Brush)(new BrushConverter()).ConvertFromString("#F5F5F5");
                                sessionInfoViewModel.IsMouseClick = false;
                            }
                        }

                    }
                    //var winShowCount = dicTalkViewModel.Count;
                    //if (winShowCount > 2)
                    //{
                    //    var tempSession = SessionControlList.Where(m => !m.IsMouseClick).Skip(2);
                    //    foreach (var sessionInfo in tempSession)
                    //    {
                    //        if (sessionInfo._SessionType == SessionType.GroupChat)
                    //        {
                    //            if (dictGroupWindows.ContainsKey(sessionInfo.SessionId))
                    //            {
                    //                var talkGroupVM = dicTalkViewModel[sessionInfo.SessionId] as TalkGroupViewModel;
                    //                if (talkGroupVM == null) return;
                    //                talkGroupVM.chromiumWebBrowser?.Dispose();
                    //                talkGroupVM.chromiumWebBrowserburn?.Dispose();
                    //                talkGroupVM.chromiumWebBrowser = null;
                    //                talkGroupVM.chromiumWebBrowserburn = null;
                    //                //dicTalkViewModel.Remove(sessionInfo.SessionId);
                    //                //dictGroupWindows.Remove(sessionInfo.SessionId);
                    //                //WindowMonitor.RemoveWindowHelper(sessionInfo.SessionId);
                    //                //MessageMonitor.RemoveMessageHelper(sessionInfo.SessionId);
                    //                MainWindowViewModel.MainExhibitControl.Children.Clear();
                    //            }
                    //        }
                    //        else if (sessionInfo._SessionType == SessionType.SingleChat)
                    //        {
                    //            if (sessionInfo.AntSdkContact_User == null) continue;
                    //            if (dictWindows.ContainsKey(sessionInfo.AntSdkContact_User.userId))
                    //            {
                    //                var talkVM = dicTalkViewModel[sessionInfo.AntSdkContact_User.userId] as TalkViewModel;
                    //                if (talkVM == null) return;
                    //                talkVM.chromiumWebBrowser?.Dispose();
                    //                talkVM.chromiumWebBrowser = null;
                    //                //dicTalkViewModel.Remove(sessionInfo.AntSdkContact_User.userId);
                    //                //dictWindows.Remove(sessionInfo.AntSdkContact_User.userId);
                    //                //WindowMonitor.RemoveWindowHelper(sessionInfo.SessionId);
                    //                //MessageMonitor.RemoveMessageHelper(sessionInfo.SessionId);
                    //                MainWindowViewModel.MainExhibitControl.Children.Clear();
                    //            }
                    //        }
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("SessionViewMouseLeftButtonDown:" + ex.Message + "," + ex.StackTrace);
                }
            });
        }
        private void ScrollToVerticalOffset(int i)
        {
            if (sessionListView?.scrollViewer != null)
            {
                scrollHeight = sessionListView.scrollViewer.VerticalOffset;
                if (sessionListView != null)
                {
                    if (sessionListView.scrollViewer.VerticalOffset >= i * 64
                        || sessionListView.scrollViewer.VerticalOffset + sessionListView.ActualHeight < i * 64)
                    {
                        scrollHeight = i * 64;
                    }
                }
            }
        }

        /// <summary>
        /// 设置列表勿打扰模式
        /// </summary>
        public void SetMsgRemind(GroupInfoViewModel sender, GlobalVariable.MsgRemind msgRemind, int unreadCount)
        {
            SessionInfoViewModel sessionVm = SessionControlList.FirstOrDefault(c => c.SessionId == sender.GroupInfo.groupId);
            if (msgRemind == GlobalVariable.MsgRemind.NoRemind)
            {
                if (sessionVm != null)
                {
                    sessionVm.GroupInfo.state = (int)GlobalVariable.MsgRemind.NoRemind;
                    sessionVm.SetUnreadCount(sessionVm.UnreadCount);
                }
            }
            else
            {
                if (sessionVm != null)
                {
                    sessionVm.GroupInfo.state = (int)GlobalVariable.MsgRemind.Remind;
                    sessionVm.SetUnreadCount(sessionVm.UnreadCount);
                }
            }
        }
        /// <summary>
        /// 更新列表勿打扰模式
        /// </summary>
        public void UpdateMsgRemind(string groupId, int state)
        {
            SessionInfoViewModel sessionVm = SessionControlList.FirstOrDefault(c => c.SessionId == groupId);
            if (state == (int)GlobalVariable.MsgRemind.NoRemind)
            {
                if (sessionVm != null)
                {
                    sessionVm.GroupInfo.state = (int)GlobalVariable.MsgRemind.NoRemind;
                    sessionVm.SetUnreadCount(sessionVm.UnreadCount);
                }
            }
            else
            {
                if (sessionVm != null)
                {
                    sessionVm.GroupInfo.state = (int)GlobalVariable.MsgRemind.Remind;
                    sessionVm.SetUnreadCount(sessionVm.UnreadCount);
                }
            }
        }

        /// <summary>
        /// 刷新右侧聊天框（群发助手）
        /// </summary>
        private void RefreshMainViewRightPart()
        {
            if (_massMsgListView == null)
            {
                _massMsgListView = new MassMsgListView();
                MassMsgListViewModel vm = new MassMsgListViewModel();
                _massMsgListView.DataContext = vm;
                MainWindowViewModel.MainExhibitControl.Children.Clear();
                MainWindowViewModel.MainExhibitControl.Children.Add(_massMsgListView);
            }
            else
            {
                MainWindowViewModel.MainExhibitControl.Children.Clear();
                MainWindowViewModel.MainExhibitControl.Children.Add(_massMsgListView);
            }
        }



        /// <summary>
        /// 刷新右侧聊天框（点对点聊天）
        /// </summary>
        private void RefreshMainViewRightPart(AntSdkContact_User user, int unreadCount, string lastTime = "")
        {
            if (user == null) return;
            if (dicTalkViewModel.ContainsKey(user.userId))
            {
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                TalkWindowView talkWindows = dictWindows[user.userId] as TalkWindowView;
                if (!MainWindowViewModel.MainExhibitControl.Children.Contains(talkWindows))
                {
                    foreach (var list in dicTalkViewModel)
                    {
                        var dictPerson = list.Value as TalkViewModel;
                        if (dictPerson != null)
                        {
                            //dictPerson
                            talkPersonModel.callbackId.ShowUserInfoEvent -= dictPerson.CallbackId_ShowUserInfoEvent;
                        }
                        else
                        {
                            TalkGroupViewModel dictGroups = list.Value as TalkGroupViewModel;
                            if (dictGroups != null)
                                talkGroupModel.callbackId.ShowUserInfoEvent -= dictGroups.CallbackId_ShowUserInfoEvent;
                        }
                    }
                    MainWindowViewModel.MainExhibitControl.Children.Clear();
                    MainWindowViewModel.MainExhibitControl.Children.Add(talkWindows);
                    var vm = (dicTalkViewModel[user.userId] as TalkViewModel);
                    if (vm != null)
                    {
                        vm.richTextBox.Focus();
                        talkPersonModel.callbackId.ShowUserInfoEvent += vm.CallbackId_ShowUserInfoEvent;
                        vm.LoadMsgData();
                        if (vm.chromiumWebBrowser == null)
                        {
                            vm.InitTalkChromiumWebBrowser();
                        }
                        else
                        {
                            vm.ShowMsgData();
                        }
                        //if (unreadCount > 0)
                        //    PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType, AntSdkReceiptType.ReadReceipt);
                    }
                }
                else
                {
                    var talkViewModel = dicTalkViewModel[user.userId] as TalkViewModel;
                    if (talkViewModel == null) return;
                    talkViewModel.richTextBox.Focus();
                    if (unreadCount > 0)
                    {
                        talkViewModel.LoadMsgData();
                        if (talkViewModel.chromiumWebBrowser == null)
                        {
                            talkViewModel.InitTalkChromiumWebBrowser();
                        }
                        else
                        {
                            talkViewModel.ShowMsgData();
                        }
                    }
                }
                //}, DispatcherPriority.Background);
            }
            else
            {
                SendMessage_ctt ctt = new SendMessage_ctt
                {
                    sendUserId = AntSdkService.AntSdkLoginOutput.userId,
                    targetId = user.userId,
                    companyCode = AntSdkService.AntSdkLoginOutput.companyCode,
                    content = user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId ? lastTime : ""
                };
                ctt.sessionId = DataConverter.GetSessionID(ctt.sendUserId, ctt.targetId);
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                foreach (var list in dicTalkViewModel)
                {
                    var dictPerson = list.Value as TalkViewModel;
                    if (dictPerson != null)
                    {
                        //dictPerson
                        talkPersonModel.callbackId.ShowUserInfoEvent -= dictPerson.CallbackId_ShowUserInfoEvent;
                    }
                    else
                    {
                        TalkGroupViewModel dictGroups = list.Value as TalkGroupViewModel;
                        if (dictGroups != null)
                            talkGroupModel.callbackId.ShowUserInfoEvent -= dictGroups.CallbackId_ShowUserInfoEvent;
                    }
                }
                TalkWindowView talk = new TalkWindowView();
                var talkViewModel = new TalkViewModel(ctt, user, unreadCount);
                talkViewModel.AudioClickEvent += TalkViewModel_AudioClickEvent;
                talk.DataContext = talkViewModel;
                talkViewModel.LoadMsgData();
                //if (unreadCount > 0)
                //    PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType, AntSdkReceiptType.ReadReceipt);
                if (MainWindowViewModel.MainExhibitControl == null)
                {
                    if (GlobalVariable.isCutShow)
                    {
                        mainWindowViewModel?.LoginOutMethod(false);
                        return;
                    }
                    else
                    {
                        var hwnd = Win32.GetForegroundWindow();
                        if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized) || mainWindowViewModel.win == null || mainWindowViewModel.win.ShowInTaskbar)
                        {
                            // var result = MessageBoxWindow.Show("提醒", msg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                            MessageBoxWindow.Show("提醒", "软件发生意外异常，导致无法正常使用，将为您返回重新登录。", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn, true);
                        }
                        else
                        {
                            MessageBoxWindow.Show("提醒", "软件发生意外异常，导致无法正常使用，将为您返回重新登录。", MessageBoxButton.OK, mainWindowViewModel.win, GlobalVariable.WarnOrSuccess.Warn);
                        }
                        //MessageBoxWindow.Show("软件发生意外异常，导致无法正常使用，将为您返回重新登录。", GlobalVariable.WarnOrSuccess.Warn);
                        mainWindowViewModel?.LoginOutMethod(false);
                        return;
                    }
                }
                MainWindowViewModel.MainExhibitControl.Children.Clear();
                MainWindowViewModel.MainExhibitControl.Children.Add(talk);
                if (!dictWindows.ContainsKey(user.userId))
                    dictWindows.Add(user.userId, talk);
                if (!dicTalkViewModel.ContainsKey(user.userId))
                    dicTalkViewModel.Add(user.userId, talkViewModel);
                PublicTalkMothed.ClearMemory();
                //}, DispatcherPriority.Background);
            }
        }

        #region 语音电话
        /// <summary>
        /// 处理语音电话窗体位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TalkViewModel_AudioClickEvent(object sender, EventArgs e)
        {
            if (mainWindowViewModel == null) return;
            var audioWin = sender as Window;
            DataConverter.SetWindowLocation(mainWindowViewModel.win, audioWin);
        }

        #endregion
        /// <summary>
        /// 刷新右侧聊天框（讨论组聊天）
        /// </summary>
        private void RefreshMainViewRightPart(AntSdkGroupInfo groupInfo, List<AntSdkGroupMember> GroupMembers, BurnFlag isBurnMode, int unreadCount, int burnUnreadcount)
        {
            if (groupInfo == null) return;
            if (dicTalkViewModel.ContainsKey(groupInfo.groupId))
            {
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                TalkGroupWindowView talkView = dictGroupWindows[groupInfo.groupId] as TalkGroupWindowView;
                if (!MainWindowViewModel.MainExhibitControl.Children.Contains(talkView))
                {
                    foreach (var list in dicTalkViewModel)
                    {
                        var dictPerson = list.Value as TalkViewModel;
                        if (dictPerson != null)
                        {
                            //dictPerson
                            talkPersonModel.callbackId.ShowUserInfoEvent -= dictPerson.CallbackId_ShowUserInfoEvent;
                        }
                        else
                        {
                            TalkGroupViewModel dictGroups = list.Value as TalkGroupViewModel;
                            if (dictGroups != null)
                                talkGroupModel.callbackId.ShowUserInfoEvent -= dictGroups.CallbackId_ShowUserInfoEvent;
                        }
                    }
                    MainWindowViewModel.MainExhibitControl.Children.Clear();
                    MainWindowViewModel.MainExhibitControl.Children.Add(talkView);
                    var groupVM = (dicTalkViewModel[groupInfo.groupId] as TalkGroupViewModel);
                    if (groupVM != null)
                    {
                        talkGroupModel.callbackId.ShowUserInfoEvent += groupVM.CallbackId_ShowUserInfoEvent;
                        groupVM.LoadMsgData(isBurnMode);

                        if (isBurnMode == BurnFlag.IsBurn)
                        {
                            if (groupVM.chromiumWebBrowserburn == null)
                            {
                                groupVM.InitTalkBurn();
                            }
                            else
                            {
                                if (groupVM.chromiumWebBrowserburn.IsBrowserInitialized)
                                    groupVM.ShowMsgData();
                            }
                            //if (burnUnreadcount > 0)
                            //    PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType, AntSdkReceiptType.ReadReceipt);
                        }
                        else
                        {

                            if (groupVM.chromiumWebBrowser == null)
                            {
                                groupVM.InitTalk();
                            }
                            else
                            {
                                if (groupVM.chromiumWebBrowser.IsBrowserInitialized)
                                    groupVM.ShowMsgData();
                            }
                            //if (unreadCount > 0)
                            //{
                            //    LogHelper.WriteWarn(
                            //        "---------------------------[TalkViewModel-ShowMsgData:]SessionViewMouseLeftButtonDown" +
                            //        sessionInfoViewModel.SessionId + "发送已读回执---------------------" +
                            //        sessionInfoViewModel.LastMessage);
                            //    PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId,
                            //        sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType,
                            //        AntSdkReceiptType.ReadReceipt);
                            //}
                        }

                    }
                }
                else
                {
                    var groupVm = (dicTalkViewModel[groupInfo.groupId] as TalkGroupViewModel);
                    if (groupVm == null) return;
                    groupVm._richTextBox.Focus();

                    if (isBurnMode == BurnFlag.IsBurn && burnUnreadcount > 0)
                    {
                        groupVm.LoadMsgData(isBurnMode);
                        if (groupVm.chromiumWebBrowserburn == null)
                        {
                            groupVm.InitTalkBurn();
                        }
                        else
                        {
                            if (groupVm.chromiumWebBrowserburn.IsBrowserInitialized)
                                groupVm.ShowMsgData();
                        }
                    }
                    else if (unreadCount > 0)
                    {
                        groupVm.LoadMsgData(isBurnMode);
                        if (groupVm.chromiumWebBrowser == null)
                        {
                            groupVm.InitTalk();
                        }
                        else
                        {
                            if (groupVm.chromiumWebBrowser.IsBrowserInitialized)
                                groupVm.ShowMsgData();
                        }
                    }
                }
                //}, DispatcherPriority.Background);
            }
            else
            {

                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                try
                {
                    SendMessage_ctt ctt = new SendMessage_ctt();
                    ctt.sendUserId = AntSdkService.AntSdkLoginOutput.userId;
                    ctt.targetId = groupInfo.groupId;
                    ctt.companyCode = AntSdkService.AntSdkLoginOutput.companyCode;
                    ctt.sessionId = groupInfo.groupId;
                    TalkGroupWindowView talk = new TalkGroupWindowView();
                    TalkGroupViewModel talkViewModel = new TalkGroupViewModel(ctt, groupInfo, GroupMembers, isBurnMode, unreadCount, burnUnreadcount);
                    talk.DataContext = talkViewModel;
                    //talkViewModel.LoadMsgData(isBurnMode);
                    //if (isBurnMode == BurnFlag.IsBurn)
                    //{
                    //    if (burnUnreadcount > 0)
                    //        PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId, sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType, AntSdkReceiptType.ReadReceipt);
                    //}
                    //else
                    //{
                    //    if (unreadCount > 0)
                    //    {
                    //        LogHelper.WriteWarn(
                    //               "---------------------------[TalkViewModel-ShowMsgData:]SessionViewMouseLeftButtonDown" +
                    //               sessionInfoViewModel.SessionId + "发送已读回执---------------------" +
                    //               sessionInfoViewModel.LastMessage);
                    //        PublicMessageFunction.SendChatMsgReceipt(sessionInfoViewModel.SessionId,
                    //            sessionInfoViewModel.LastChatIndex, sessionInfoViewModel.MsgType,
                    //            AntSdkReceiptType.ReadReceipt);
                    //    }
                    //}
                    if (MainWindowViewModel.MainExhibitControl == null)
                    {
                        MessageBoxWindow.Show("软件发生意外异常，导致无法正常使用，将为您返回重新登录。", GlobalVariable.WarnOrSuccess.Warn);
                        mainWindowViewModel?.LoginOutMethod(false);
                        return;
                    }
                    MainWindowViewModel.MainExhibitControl.Children.Clear();
                    MainWindowViewModel.MainExhibitControl.Children.Add(talk);

                    if (GroupMembers == null || GroupMembers.Count == 0)
                    {
                        var memberList = SessionMonitor.GroupListViewModel?.GroupInfoList;
                        List<AntSdkGroupMember> groupMembers = null;
                        if (memberList != null && memberList.Count > 0)
                        {
                            var tempGroupInfo =
                                memberList.FirstOrDefault(
                                    m => m.GroupInfo?.groupId == sessionInfoViewModel.SessionId);
                            if (groupInfo != null && tempGroupInfo.Members?.Count > 0)
                                sessionInfoViewModel.GroupMembers = tempGroupInfo.Members;
                        }
                        groupMembers = sessionInfoViewModel.GroupMembers;
                        if (groupMembers == null || groupMembers.Count == 0)
                        {
                            AsyncHandler.CallFuncWithUI<List<AntSdkGroupMember>>(
                                System.Windows.Application.Current.Dispatcher,
                                () =>
                                {
                                    return groupMembers = GroupPublicFunction.GetMembers(sessionInfoViewModel.GroupInfo.groupId);
                                },
                                (ex, datas) =>
                                {
                                    if (datas == null)
                                        return;
                                    sessionInfoViewModel.GroupMembers = groupMembers;
                                    talkViewModel.GroupMembersLoad(groupMembers);
                                });
                        }
                        else
                        {
                            talkViewModel.GroupMembersLoad(groupMembers);
                        }
                    }
                    else
                    {
                        talkViewModel.GroupMembersLoad(GroupMembers);
                    }
                    if (!dicTalkViewModel.ContainsKey(groupInfo.groupId))
                        dicTalkViewModel.Add(groupInfo.groupId, talkViewModel);
                    if (!dictGroupWindows.ContainsKey(groupInfo.groupId))
                        dictGroupWindows.Add(groupInfo.groupId, talk);
                    PublicTalkMothed.ClearMemory();
                }
                catch (Exception ex)
                {
                    throw;
                }
                //}, DispatcherPriority.Background);
            }
        }

        public void ContactInfoViewMouseDoubleClick(object sender, EventArgs e)
        {
            //if (SessionControlList.Contains(sender)) return;
            //ContactInfoViewModel vm = sender as ContactInfoViewModel;
            //if (vm != null)
            //{
            //    AddContactSession(vm.User.userId, true);
            //}
            //else
            //{
            if (sender is string)
            {
                string userid = (sender as string);
                if (!string.IsNullOrEmpty(userid))
                {
                    AddContactSession(userid, true);
                }
            }
            else if (sender is GroupInfoViewModel)
            {
                GroupInfoViewModel vm = sender as GroupInfoViewModel;
                AddGroupSession(vm.GroupInfo, true);
            }
            //}
        }

        public void GroupInfoViewMouseDoubleClick(object sender, EventArgs e)
        {
            GroupInfoViewModel vm = sender as GroupInfoViewModel;
            if (vm == null) return;

            AddGroupSession(vm.GroupInfo, true);
        }

        public void GroupMemberViewMouseClickEvent(object sender, EventArgs e)
        {
            //if (SessionControlList.Contains(sender)) return;
            GroupMemberViewModel vm = sender as GroupMemberViewModel;
            if (vm == null) return;
            //判断是否已存在
            foreach (var control in SessionControlList)
            {
                SessionInfoViewModel sessionInfoViewModel = control as SessionInfoViewModel;
                if (sessionInfoViewModel != null && sessionInfoViewModel.SessionId == DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, vm.Member.userId))
                {
                    SessionViewMouseLeftButtonDown(sessionInfoViewModel, null);
                    return;
                }
            }
            AntSdkContact_User antsdkcontact_User = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == vm.Member.userId);
            if (antsdkcontact_User == null) return;
            dispatcher.Invoke(new Action(() =>
             {
                 SessionInfoModel model = new SessionInfoModel();
                 model.SessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, vm.Member.userId);
                 //model.name = AntSdkContact_User.userName;
                 if (string.IsNullOrEmpty(antsdkcontact_User.userNum))
                 {
                     model.name = antsdkcontact_User.userName;
                 }
                 else
                 {
                     model.name = antsdkcontact_User.userNum + antsdkcontact_User.userName;
                 }
                 model.photo = antsdkcontact_User.picture;
                 model.unreadCount = 0;
                 model.lastTime = DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000";
                 //model.MyselfMsgCount = 0;
                 SessionInfoViewModel tempVM = new SessionInfoViewModel(antsdkcontact_User, model);
                 tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                 tempVM.DeleteSessionEvent += DeleteSession;
                 tempVM.PostTopSessionEvent += PostTopSession;
                 tempVM.CancelTopSessionEvent += CacelTopSession;
                 SessionControlList.Remove(tempVM);
                 AddSessionControl(tempVM.LastMsgTimeStamp, tempVM);
                 SessionViewMouseLeftButtonDown(tempVM, null);
             }));
        }

        /// <summary>
        /// 刷新会话列表（保存到SQLite）
        /// </summary>
        /// <param name="lastMessage">最后一条消息</param>
        /// <param name="AntSdkContact_User">消息发送者</param>
        /// <param name="chatMsg">聊天内容</param>
        /// <param name="msgCount">消息数（在线消息为1，离线消息>=1）</param>
        /// <param name="isOffline">是否是离线消息</param>
        public void RefreshSessionInfoList(string lastMessage, AntSdkContact_User AntSdkContact_User, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg, int msgCount, AntSdkMsgType msgType, bool isOffline = false)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                string sessionID = string.Empty;
                sessionID = chatMsg == null ? lastMessage : chatMsg.sessionId;
                SessionInfoViewModel existVM = GetSessionInfoViewModelById(sessionID);
                var hwnd = Win32.GetForegroundWindow();
                if (existVM == null)
                {
                    #region 点对点聊天

                    if (chatMsg == null) return;
                    if (chatMsg.chatType == (int)AntSdkchatType.Point)//点对点聊天(发言者可能是本人)
                    {
                        //dispatcher.Invoke(() =>
                        //{
                        if (AntSdkContact_User == null) return;
                        if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)//如果发送者是本人
                        {
                            AntSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.targetId);
                            if (AntSdkContact_User == null) return;
                            msgCount = 0;
                        }
                        SessionInfoModel model = new SessionInfoModel();
                        //model.SessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, chatMsg.sendUserId);
                        //model.lastMessage = FormatLastMessageContent(mtp, chatMsg.content);
                        model.SessionId = sessionID;
                        model.lastMessage = lastMessage;
                        model.lastTime = chatMsg.sendTime;
                        model.lastChatIndex = chatMsg.chatIndex;

                        //model.name = AntSdkContact_User.userName;
                        if (string.IsNullOrEmpty(AntSdkContact_User.userNum))
                        {
                            model.name = AntSdkContact_User.userName;
                        }
                        else
                        {
                            model.name = AntSdkContact_User.userNum + AntSdkContact_User.userName;
                        }
                        model.photo = AntSdkContact_User.picture;
                        var userSession = t_sessionBll.GetModelByKey(sessionID);
                        string sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.robotId, AntSdkService.AntSdkCurrentUserInfo.userId);
                        if (userSession.UnreadCount > 0 && userSession.SessionId != sessionId)
                            model.unreadCount = userSession.UnreadCount;
                        model.topIndex = userSession.TopIndex;

                        var tempVM = new SessionInfoViewModel(AntSdkContact_User, model, msgType);
                        tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                        tempVM.DeleteSessionEvent += DeleteSession;
                        tempVM.PostTopSessionEvent += PostTopSession;
                        tempVM.CancelTopSessionEvent += CacelTopSession;
                        SessionControlList.Remove(tempVM);
                        AddSessionControl(chatMsg.sendTime, tempVM);
                        existVM = tempVM;
                        //});
                    }
                    #endregion

                    #region 群聊
                    else if (chatMsg.chatType == (int)AntSdkchatType.Group) //群聊(发言者可能是本人)
                    {
                        dispatcher.Invoke(() =>
                           {
                               var groupInfoVm = SessionMonitor.GroupListViewModel?.GroupInfos?.FirstOrDefault(
                                c => c.groupId == sessionID);
                               if (groupInfoVm == null) return;
                               if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)//如果发送者是本人
                               {
                                   msgCount = 0;
                               }
                               #region 处理消息免打扰的情况
                               //if (groupInfoVM.GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                               //{
                               //    
                               //    T_NoRemindGroup temp = t_NoRemindGroupBll.GetModelByKey(chatMsg.sessionId);
                               //    if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId) //如果发送者是本人，不计未读数
                               //    {
                               //        msgCount = 0;
                               //    }
                               //    if (temp == null)
                               //    {
                               //        temp = new T_NoRemindGroup();
                               //    }
                               //    if (chatMsg.flag == ((int)GlobalVariable.BurnFlag.IsBurn).ToString())
                               //    {
                               //        temp.BurnLastMsg = lastMessage;
                               //        temp.BurnLastMsgTimeStamp = chatMsg.sendTime;
                               //        temp.BurnUnreadCount = temp.BurnUnreadCount + msgCount;
                               //        temp.BurnLastChatIndex = chatMsg.chatIndex;
                               //    }
                               //    else
                               //    {
                               //        temp.LastMsg = lastMessage;
                               //        temp.LastMsgTimeStamp = chatMsg.sendTime;
                               //        temp.UnreadCount = temp.UnreadCount + msgCount;
                               //        temp.LastChatIndex = chatMsg.chatIndex;
                               //    }
                               //    if (string.IsNullOrEmpty(temp.GroupId))
                               //    {
                               //        temp.GroupId = chatMsg.sessionId;
                               //        t_NoRemindGroupBll.Insert(temp);
                               //    }
                               //    else
                               //    {
                               //        t_NoRemindGroupBll.Update(temp);
                               //    }
                               //    groupInfoVM.SetUnreadCount(temp.UnreadCount);
                               //   
                               //    return;
                               //}
                               #endregion
                               var userSession = t_sessionBll.GetModelByKey(sessionID);
                               if (userSession.IsBurnMode == 1 && userSession.IsBurnMode != chatMsg.flag)
                               {
                                   msgCount = 0;
                                   chatMsg.flag = 1;
                                   lastMessage = userSession.BurnLastMsg;
                               }

                               SessionInfoModel model = new SessionInfoModel();
                               model.SessionId = sessionID;
                               model.lastMessage = lastMessage;
                               model.lastTime = chatMsg.sendTime;
                               model.name = groupInfoVm.groupName;
                               model.photo = groupInfoVm.groupPicture;
                               model.unreadCount = msgCount;
                               model.lastChatIndex = chatMsg.chatIndex;
                               model.topIndex = userSession.TopIndex;

                               var groupInfoVM = SessionMonitor.GroupListViewModel?.GroupInfoList?.FirstOrDefault(
                                   c => c.GroupInfo?.groupId == sessionID);
                               if (groupInfoVM != null)
                               {
                                   if (chatMsg.flag == 1)
                                       groupInfoVM.IsBurnMode = GlobalVariable.BurnFlag.IsBurn;
                                   else if (chatMsg.flag == 0)
                                       groupInfoVM.IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
                               }
                               SessionInfoViewModel tempVM = new SessionInfoViewModel(groupInfoVm,
                                   chatMsg.flag == 1 ? GlobalVariable.BurnFlag.IsBurn : GlobalVariable.BurnFlag.NotIsBurn, null, model);
                               tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                               tempVM.DeleteSessionEvent += DeleteSession;
                               tempVM.PostTopSessionEvent += PostTopSession;
                               tempVM.CancelTopSessionEvent += CacelTopSession;
                               AddSessionControl(chatMsg.sendTime, tempVM);
                               existVM = tempVM;
                           });
                    }
                    #endregion
                    if (!isOffline && ((mainWindowViewModel != null && !mainWindowViewModel.SessionSelected)
                        || (GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized)))
                    {
                        isFirst = true;
                        scrollHeight = 0;
                    }
                }
                else
                {
                    if (!isOffline && ((mainWindowViewModel != null && !mainWindowViewModel.SessionSelected)
                        || (GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized))
                        && existVM.UnreadCount == 0 && !existVM.IsMouseClick)
                        scrollHeight = 0;
                    //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                    //{
                    //    if (!existVM.IsShow)
                    //        existVM.IsShow = true;
                    //});
                    //特殊情况处理

                    if (chatMsg == null)
                    {
                        if (!string.IsNullOrEmpty(lastMessage))
                        {
                            //dispatcher.Invoke(new Action(() =>
                            //{
                            AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionID);
                            existVM.LastMessage = string.Empty;
                            existVM.LastTime = DataConverter.FormatTimeByTimeStamp(tSession.LastMsgTimeStamp);
                            existVM.MsgDatetime = DataConverter.GetTimeByTimeStamp(tSession.LastMsgTimeStamp);
                            existVM.LastMsgTimeStamp = tSession.LastMsgTimeStamp;
                            existVM.LastChatIndex = tSession.LastChatIndex;
                            existVM.ImageSendingVisibility = Visibility.Collapsed;
                            existVM.ImageFailingVisibility = Visibility.Collapsed;

                            if (!existVM.IsMouseClick)
                            {
                                existVM.SetUnreadCount(msgCount);
                            }
                            else if (!mainWindowViewModel.win.ShowInTaskbar)
                            {
                                existVM.UnreadCount += 1;
                            }
                            SessionControlList.Remove(existVM);
                            AddSessionControl(tSession.LastMsgTimeStamp, existVM);
                            //}));
                        }
                        return;
                    }

                    var groupInfoVM =
                        SessionMonitor.GroupListViewModel?.GroupInfoList.FirstOrDefault(
                            c => c.GroupInfo != null && c.GroupInfo.groupId == existVM.SessionId);

                    #region 更新界面会话

                    if (chatMsg.flag < 0) chatMsg.flag = 0;
                    var userSession = t_sessionBll.GetModelByKey(existVM.SessionId);
                    if (userSession.IsBurnMode == 1 && userSession.IsBurnMode != chatMsg.flag)
                    {
                        msgCount = 0;
                        chatMsg.flag = 1;
                        lastMessage = userSession.BurnLastMsg;
                    }
                    if (groupInfoVM != null)
                    {
                        if (chatMsg.flag == 1)
                            groupInfoVM.IsBurnMode = GlobalVariable.BurnFlag.IsBurn;
                        else if (chatMsg.flag == 0)
                            groupInfoVM.IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
                    }
                    //单聊，或者群聊且群模式和收到的消息模式一致才更新会话
                    if (groupInfoVM == null || (int)groupInfoVM.IsBurnMode == chatMsg.flag)
                    {
                        if (chatMsg.MsgType == AntSdkMsgType.ChatMsgAt && existVM.IsMouseClick)
                        {
                            if (existVM.ImageNoticeVisibility == Visibility.Visible)
                            {
                                if (string.IsNullOrEmpty(userSession.LastMsgTimeStamp) ||
                                    string.CompareOrdinal(existVM.LastMsgTimeStamp, chatMsg.sendTime) < 0)
                                    existVM.LastMessage = lastMessage;
                            }
                            else
                            {
                                existVM.LastMessage = lastMessage;
                            }

                        }
                        else
                        {
                            if (existVM.ImageNoticeVisibility == Visibility.Visible)
                            {
                                if (string.IsNullOrEmpty(userSession.LastMsgTimeStamp) ||
                                    string.CompareOrdinal(existVM.LastMsgTimeStamp, chatMsg.sendTime) < 0)
                                    existVM.LastMessage = lastMessage;
                            }
                            else
                            {
                                existVM.LastMessage = lastMessage;
                            }
                        }
                        if (groupInfoVM != null && existVM.IsBurnMode != groupInfoVM.IsBurnMode)
                        {
                            existVM.IsBurnMode = groupInfoVM.IsBurnMode;
                            existVM.ImageBurnVisibility = groupInfoVM.IsBurnMode == BurnFlag.IsBurn
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }
                        if (existVM.ImageNoticeVisibility == Visibility.Visible)
                        {
                            if (string.CompareOrdinal(existVM.LastMsgTimeStamp, chatMsg.sendTime) < 0 ||
                                string.IsNullOrEmpty(existVM.LastMsgTimeStamp))
                            {
                                existVM.LastTime = DataConverter.FormatTimeByTimeStamp(chatMsg.sendTime);
                                existVM.MsgDatetime = DataConverter.GetTimeByTimeStamp(chatMsg.sendTime);
                                existVM.LastMsgTimeStamp = chatMsg.sendTime;
                            }
                        }
                        else
                        {
                            existVM.LastTime = DataConverter.FormatTimeByTimeStamp(chatMsg.sendTime);
                            existVM.MsgDatetime = DataConverter.GetTimeByTimeStamp(chatMsg.sendTime);
                            existVM.LastMsgTimeStamp = chatMsg.sendTime;
                        }
                        existVM.LastChatIndex = chatMsg.chatIndex;
                        existVM.ImageSendingVisibility = Visibility.Collapsed;
                        existVM.ImageFailingVisibility = Visibility.Collapsed;

                        //if(chatMsg.os!=(int)GlobalVariable.OSType.PC &&chatMsg.sessionId)
                        if (!existVM.IsMouseClick && chatMsg.sendUserId != AntSdkService.AntSdkCurrentUserInfo.userId)
                        {
                            existVM.SetUnreadCount(msgCount);
                        }
                        else if (!mainWindowViewModel.win.ShowInTaskbar)
                        {
                            existVM.UnreadCount += 1;
                        }
                        //收到离线消息时，会话窗体已打开（场景：服务断开重连）
                        if (existVM.IsMouseClick && isOffline)
                        {
                            if (existVM.isPointOrGroup)
                            {
                                if (!dicTalkViewModel.ContainsKey(existVM.AntSdkContact_User.userId)) return;
                                var vm = dicTalkViewModel[existVM.AntSdkContact_User.userId] as talkPersonModel;
                                if (vm != null)
                                {
                                    vm.LoadMsgData(BurnFlag.NotIsBurn, false);
                                    vm.ShowMsgData(null, BurnFlag.NotIsBurn, true);
                                }
                            }
                            else
                            {
                                if (!dicTalkViewModel.ContainsKey(existVM.GroupInfo.groupId)) return;
                                var vm = dicTalkViewModel[existVM.GroupInfo.groupId] as talkGroupModel;
                                if (vm != null)
                                {
                                    vm.LoadMsgData(existVM.IsBurnMode, false);
                                    vm.ShowMsgData(null, true);
                                }
                            }
                        }
                        SessionControlList.Remove(existVM);
                        AddSessionControl(chatMsg.sendTime, existVM);
                    }
                    #endregion

                }


                if (chatMsg != null && chatMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId)//非本人发的消息
                {
                    if (existVM != null && (existVM.GroupInfo == null || existVM.GroupInfo.state == (int)GlobalVariable.MsgRemind.Remind))
                    {

                        if ((GlobalVariable.winHandle != hwnd || GlobalVariable.isMinimized) && chatMsg.MsgType != AntSdkMsgType.Revocation)
                        {
                            //if (GetTotelUnreadCount() > 0)
                            if (mainWindowViewModel != null && mainWindowViewModel.win != null && mainWindowViewModel.win.ShowInTaskbar)
                                ToolbarFlash.Flash(GlobalVariable.winHandle, 2);
                        }
                        //dispatcher.Invoke(new Action(() =>
                        //{
                        if (mainWindowViewModel != null && mainWindowViewModel.win != null)
                        {
                            if (!mainWindowViewModel.win.ShowInTaskbar && existVM.IsMouseClick)
                            {
                                if (chatMsg.MsgType != AntSdkMsgType.Revocation)
                                    curSessionUnreadMsg = true;
                                else if (GetTotelUnreadCount() == 0)
                                {
                                    curSessionUnreadMsg = false;
                                }
                                //if (chatMsg.MsgType == AntSdkMsgType.Revocation)
                                //    curSessionUnreadMsg = false;
                            }
                        }
                        //}));
                    }
                }
            });

            #region 更新T_NoRemindGroup表
            //if (existVM.UnreadCount > 0 && chatMsg.sessionId == chatMsg.targetId && existVM.GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
            //{
            //    T_NoRemindGroup temp = t_NoRemindGroupBll.GetModelByKey(chatMsg.sessionId);
            //    if (temp == null)
            //    {
            //        temp = new T_NoRemindGroup();
            //    }
            //    if (chatMsg.flag == ((int)GlobalVariable.BurnFlag.IsBurn).ToString())
            //    {
            //        temp.BurnLastMsg = lastMessage;
            //        temp.BurnLastMsgTimeStamp = chatMsg.sendTime;
            //        temp.BurnLastChatIndex = chatMsg.chatIndex;
            //        temp.BurnUnreadCount = temp.BurnUnreadCount + msgCount;
            //    }
            //    else
            //    {
            //        temp.LastMsg = lastMessage;
            //        temp.LastMsgTimeStamp = chatMsg.sendTime;
            //        temp.LastChatIndex = chatMsg.chatIndex;
            //        temp.UnreadCount = temp.UnreadCount + msgCount;
            //    }
            //    if (string.IsNullOrEmpty(temp.GroupId))
            //    {
            //        temp.GroupId = chatMsg.sessionId;
            //        t_NoRemindGroupBll.Insert(temp);
            //    }
            //    else
            //    {
            //        t_NoRemindGroupBll.Update(temp);
            //    }
            //    GroupInfoViewModel groupInfoVm = this.mainWindowViewModel._GroupListViewModel.GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == chatMsg.sessionId);
            //    groupInfoVm?.SetUnreadCount(temp.UnreadCount);
            //}
            #endregion

        }
        /// <summary>
        /// 根据sessionID删除消息列表对应的项
        /// </summary>
        /// <param name="sessionID"></param>
        public void RemoveSessionInfoList(string sessionID, GlobalVariable.BurnFlag isBurnFlag)
        {

            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                if (SessionControlList != null && SessionControlList.Count > 0)
                {
                    var sessionInfo = SessionControlList.OfType<SessionInfoViewModel>().FirstOrDefault(m => m.SessionId == sessionID && m.IsBurnMode == isBurnFlag);
                    if (sessionInfo != null && !sessionInfo.IsMouseClick)
                    {
                        SessionControlList.Remove(sessionInfo);
                    }
                }
            });


        }

        /// <summary>
        /// 点对点会话更新最近消息列表
        /// </summary>
        /// <param name="msgReceipt"></param>
        public void RefreshSessionInfoList(AntSdkChatMsg.ChatBase msgReceipt, bool isGroup)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                SessionInfoViewModel sessionInfo = GetSessionInfoViewModelById(msgReceipt.sessionId);
                if (sessionInfo != null)
                {
                    AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionInfoViewModel.SessionId);
                    if (tSession != null)
                    {
                        var chatIndex = 0;
                        var burnLastChatIndex = 0;
                        if (!string.IsNullOrEmpty(tSession.LastChatIndex))
                            int.TryParse(tSession.LastChatIndex, out chatIndex);
                        if (!string.IsNullOrEmpty(tSession.BurnLastChatIndex))
                            int.TryParse(tSession.BurnLastChatIndex, out burnLastChatIndex);
                        var msgChatIndex = 0;
                        if (!string.IsNullOrEmpty(msgReceipt.chatIndex))
                            int.TryParse(msgReceipt.chatIndex, out msgChatIndex);
                        if (msgReceipt.flag == 1 && msgChatIndex < burnLastChatIndex)
                            return;
                        if (msgReceipt.flag == 0 && msgChatIndex < chatIndex)
                            return;
                    }
                    sessionInfo.ImageFailingVisibility = Visibility.Collapsed;
                    sessionInfo.ImageSendingVisibility = Visibility.Collapsed;
                    sessionInfo.LastTime = DataConverter.FormatTimeByTimeStamp(msgReceipt.sendTime);
                    sessionInfo.MsgDatetime = DataConverter.GetTimeByTimeStamp(msgReceipt.sendTime);
                    sessionInfo.LastChatIndex = msgReceipt.chatIndex;
                    sessionInfo.LastMsgTimeStamp = msgReceipt.sendTime;
                    if (!string.IsNullOrEmpty(msgReceipt.sourceContent))
                        sessionInfo.LastMessage = msgReceipt.sourceContent;

                    if (tSession != null)
                    {
                        if (msgReceipt.flag == 0)
                        {
                            tSession.LastChatIndex = msgReceipt.chatIndex;
                            tSession.LastMsgTimeStamp = msgReceipt.sendTime;
                            tSession.LastMsg = sessionInfo.LastMessage;
                        }
                        else
                        {
                            tSession.BurnLastChatIndex = msgReceipt.chatIndex;
                            tSession.BurnLastMsgTimeStamp = msgReceipt.sendTime;
                            tSession.BurnLastMsg = sessionInfo.LastMessage;
                        }
                        if (tSession.TopIndex != null && MaxTopIndex != null && tSession.TopIndex.Value < MaxTopIndex.Value)
                        {
                            tSession.TopIndex = MaxTopIndex.Value + 1;
                            MaxTopIndex = tSession.TopIndex;
                        }
                        t_sessionBll.Update(tSession);
                    }
                    else
                    {
                        tSession = new AntSdkTsession
                        {
                            SessionId = msgReceipt.sessionId,
                            GroupId = isGroup ? msgReceipt.sessionId : "",
                            UserId = isGroup ? "" : msgReceipt.targetId
                        };
                        if (msgReceipt.flag == 0)
                        {
                            tSession.LastChatIndex = msgReceipt.chatIndex;
                            tSession.LastMsgTimeStamp = msgReceipt.sendTime;
                            tSession.LastMsg = sessionInfo.LastMessage;
                        }
                        else
                        {
                            tSession.BurnLastChatIndex = msgReceipt.chatIndex;
                            tSession.BurnLastMsgTimeStamp = msgReceipt.sendTime;
                            tSession.BurnLastMsg = sessionInfo.LastMessage;
                        }
                        t_sessionBll.Insert(tSession);
                    }
                    var tempSession = SessionControlList[0];
                    if (tempSession.SessionId == sessionInfo.SessionId) return;
                    SessionControlList.Remove(sessionInfo);
                    AddSessionControl(msgReceipt.sendTime, sessionInfo);
                }
            }, DispatcherPriority.Background);
        }


        /// <summary>
        /// 新增一个会话（,有设置置顶的为第一优先级，最后一条消息时间为空的为第二优先级，剩下的需要按最后一条消息时间排序,从近到远）
        /// </summary>
        /// <param name="lastMsgTimeStamp"></param>
        /// <param name="sessionInfoVM"></param>
        private void AddSessionControl(string lastMsgTimeStamp, SessionInfoViewModel sessionInfoVM, bool isLoadData = false)
        {
            int sessionIndex = 0;
            //如果设置为置顶的
            if (sessionInfoVM.TopIndex.HasValue)
            {
                if (isLoadData)
                {
                    sessionIndex = SessionControlList.Count(p => p.TopIndex > sessionInfoVM.TopIndex);
                }
                else
                {
                    sessionIndex = SessionControlList.Count(p => p.TopIndex > sessionInfoVM.TopIndex);
                    if (sessionIndex != 0)
                        sessionIndex = 0;
                }

            }
            //如果不为置顶，但属于新建的会话
            else if (string.IsNullOrEmpty(sessionInfoVM.LastMsgTimeStamp))
            {
                sessionIndex = SessionControlList.Count(p => p.TopIndex.HasValue);
            }
            //如果既不属于置顶，也不属于新建会话
            else
            {
                sessionIndex = SessionControlList.Count(
                    p =>
                        p.TopIndex.HasValue ||
                        (!p.TopIndex.HasValue && string.Compare(p.LastMsgTimeStamp, sessionInfoVM.LastMsgTimeStamp,
                            StringComparison.Ordinal) > 0)
                    );
            }
            var tempSessionList = SessionControlList.Where(m => m.SessionId == sessionInfoVM.SessionId);
            var infoViewModels = tempSessionList as IList<SessionInfoViewModel> ?? tempSessionList.ToList();
            if (infoViewModels.Count > 0)
            {
                var sessionInfoViewModels = tempSessionList as IList<SessionInfoViewModel> ?? infoViewModels.ToList();
                foreach (SessionInfoViewModel sessionInfo in sessionInfoViewModels)
                {
                    SessionControlList.Remove(sessionInfo);
                }
            }
            if (sessionIndex > SessionControlList.Count)
                sessionIndex = SessionControlList.Count;
            SessionControlList.Insert(SessionControlList.Count == 0 ? 0 : sessionIndex, sessionInfoVM);
            RemoveRepeatSession();
        }
        /// <summary>
        /// 截图
        /// </summary>
        public void TalkViewCutImg()
        {
            var currentOpenSession = SessionControlList.FirstOrDefault(m => m.IsMouseClick);
            if (currentOpenSession != null)
            {
                if (currentOpenSession._SessionType == SessionType.GroupChat && dictGroupWindows.ContainsKey(currentOpenSession.SessionId))
                {
                    var talkGroupVm = dicTalkViewModel[currentOpenSession.SessionId] as talkGroupModel;
                    talkGroupVm?.TalkViewCutImage();
                    return;
                }
                else if (currentOpenSession._SessionType == SessionType.SingleChat && dicTalkViewModel.ContainsKey(currentOpenSession.AntSdkContact_User.userId))
                {
                    var talkViewModel = dicTalkViewModel[currentOpenSession.AntSdkContact_User.userId] as talkPersonModel;
                    talkViewModel?.TalkViewCutImage();
                    return;
                }
            }
            else
            {
                ImageHandle.CutImg();
            }
        }
        /// <summary>
        /// 语音电话中间提示信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        public void TalkViewAudio(string userId, string msg)
        {
            var talkViewModel = dicTalkViewModel[userId] as talkPersonModel;
            talkViewModel?.AudioTips(msg);
        }
        /// <summary>
        /// 语音电话右边消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        public void TalkVieAudioMessage(string userId, string msg)
        {
            var talkViewModel = dicTalkViewModel[userId] as talkPersonModel;
            talkViewModel?.AudioMessage(msg);
        }
        /// <summary>
        /// 删除消息列表选中项
        /// </summary>
        public void DeleteSession(object sender, EventArgs e)
        {
            if (SessionControlList == null) return;
            for (int i = 0; i < SessionControlList.Count; i++)
            {
                var control = SessionControlList[i];
                if (sender == control)
                {
                    SessionInfoViewModel sessionInfoViewModel = sender as SessionInfoViewModel;
                    var currentOpenSession = SessionControlList.FirstOrDefault(m => m.IsMouseClick);
                    if (sessionInfoViewModel.IsRemoveLoaclSession && sessionInfoViewModel.IsBurnMode == BurnFlag.IsBurn && !BurnSessionList.Contains(sessionInfoViewModel.SessionId))
                        BurnSessionList.Add(sessionInfoViewModel.SessionId);
                    SessionControlList.Remove(sessionInfoViewModel);
                    var tempSesssionInfo =
                        tempSessionInfoViewModels.FirstOrDefault(m => m.SessionId == sessionInfoViewModel.SessionId);
                    if (tempSesssionInfo != null)
                        tempSessionInfoViewModels.Remove(tempSesssionInfo);
                    if (!string.IsNullOrEmpty(sessionInfoViewModel.SessionId) && sessionInfoViewModel.IsRemoveLoaclSession)
                    {
                        AntSdkTsession session = new AntSdkTsession();
                        session.SessionId = sessionInfoViewModel.SessionId;
                        t_sessionBll.Delete(session);
                    }
                    if (control._SessionType == SessionType.GroupChat)
                    {
                        if (dictGroupWindows.ContainsKey(sessionInfoViewModel.SessionId))
                        {
                            var talkGroupVM = (dicTalkViewModel[sessionInfoViewModel.SessionId] as TalkGroupViewModel);
                            if (talkGroupVM?.chromiumWebBrowser != null)
                                talkGroupVM.EndNotBurnEvent();
                            //talkGroupVM.chromiumWebBrowser.Dispose();
                            if (talkGroupVM?.chromiumWebBrowserburn != null)
                                talkGroupVM.EndBurnEvent();
                            //talkGroupVM.chromiumWebBrowserburn.Dispose();
                            WindowMonitor.RemoveWindowHelper(sessionInfoViewModel.SessionId);
                            MessageMonitor.RemoveMessageHelper(sessionInfoViewModel.SessionId);
                            dicTalkViewModel.Remove(sessionInfoViewModel.SessionId);
                            dictGroupWindows.Remove(sessionInfoViewModel.SessionId);
                            talkGroupVM.timer.Stop();
                            talkGroupVM.timerBurn.Stop();
                            //mainWindowViewModel.three.Children.Clear();
                            if (currentOpenSession == null || (currentOpenSession.SessionId == sessionInfoViewModel.SessionId))
                                MainWindowViewModel.ClearMainViewRightPart();

                        }
                    }
                    else if (control._SessionType == SessionType.SingleChat)
                    {
                        if (dictWindows.ContainsKey(control.AntSdkContact_User.userId))
                        {
                            var talkViewModel = dicTalkViewModel[control.AntSdkContact_User.userId] as TalkViewModel;
                            if (talkViewModel?.chromiumWebBrowser != null)
                                talkViewModel.EndEvent();
                            //(dicTalkViewModel[control.AntSdkContact_User.userId] as TalkViewModel).chromiumWebBrowser.Dispose();
                            dicTalkViewModel.Remove(control.AntSdkContact_User.userId);
                            dictWindows.Remove(control.AntSdkContact_User.userId);
                            WindowMonitor.RemoveWindowHelper(sessionInfoViewModel.SessionId);
                            MessageMonitor.RemoveMessageHelper(sessionInfoViewModel.SessionId);
                            //mainWindowViewModel.three.Children.Clear();
                            talkViewModel.timer.Stop();
                            if (currentOpenSession == null || (currentOpenSession.SessionId == sessionInfoViewModel.SessionId))
                                MainWindowViewModel.ClearMainViewRightPart();
                        }
                    }
                    else if (control._SessionType == SessionType.MassAssistant)//群发助手
                    {

                    }
                    break;
                }
            }
            if (SessionControlList.Count > 0)
            {
                var isMouseClickCount = SessionControlList.Count(m => m.IsMouseClick);
                if (isMouseClickCount == 0)
                    SessionViewMouseLeftButtonDown(SessionControlList[0], null);
            }
            else
            {
                //this.mainWindowViewModel.ThirdPartViewModel = null;
            }
        }

        /// <summary>
        /// 将会话置顶
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostTopSession(object sender, EventArgs e)
        {
            if (SessionControlList == null) return;
            try
            {

                for (int i = 0; i < SessionControlList.Count; i++)
                {
                    var control = SessionControlList[i];
                    if (sender == control)
                    {
                        SessionInfoViewModel sessionInfoViewModel = control as SessionInfoViewModel;
                        if (!string.IsNullOrEmpty(sessionInfoViewModel.SessionId))
                        {
                            if (MaxTopIndex.HasValue)
                                MaxTopIndex = MaxTopIndex.Value + 1;
                            else
                                MaxTopIndex = 1;
                            AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionInfoViewModel.SessionId);
                            T_SessionDAL sessionDal = new T_SessionDAL();
                            if (tSession != null)
                                sessionDal.UpdateTopIndex(sessionInfoViewModel.SessionId, MaxTopIndex.Value);
                            sessionInfoViewModel.TopIndex = MaxTopIndex;
                            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                            {
                                SessionControlList.Remove(sessionInfoViewModel);
                                AddSessionControl(sessionInfoViewModel.LastMsgTimeStamp, sessionInfoViewModel);
                            }, DispatcherPriority.Background);

                            if (tSession == null)
                            {
                                tSession = new AntSdkTsession
                                {
                                    TopIndex = sessionInfoViewModel.TopIndex,
                                    SessionId = sessionInfoViewModel.SessionId,
                                    UserId = sessionInfoViewModel.AntSdkContact_User != null ? sessionInfoViewModel.AntSdkContact_User.userId : string.Empty,
                                    GroupId = sessionInfoViewModel.GroupInfo != null ? sessionInfoViewModel.GroupInfo.groupId : string.Empty
                                };
                                t_sessionBll.Insert(tSession);
                            }
                        }
                        break;
                    }
                }
                if (SessionControlList.Count > 0)
                {
                    SessionViewMouseLeftButtonDown(SessionControlList[0], null);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("将会话置顶时发生异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 取消会话置顶
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacelTopSession(object sender, EventArgs e)
        {
            try
            {
                if (SessionControlList == null) return;
                for (int i = 0; i < SessionControlList.Count; i++)
                {
                    var control = SessionControlList[i];
                    if (sender == control)
                    {
                        SessionInfoViewModel sessionInfoViewModel = control as SessionInfoViewModel;
                        if (!string.IsNullOrEmpty(sessionInfoViewModel.SessionId))
                        {
                            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                            {
                                if (MaxTopIndex.HasValue)
                                    MaxTopIndex = MaxTopIndex.Value + 1;
                                else
                                    MaxTopIndex = 0;

                                T_SessionDAL sessionDal = new T_SessionDAL();
                                sessionDal.UpdateTopIndex(sessionInfoViewModel.SessionId, new int?());


                                sessionInfoViewModel.TopIndex = null;

                                SessionControlList.Remove(sessionInfoViewModel);
                                AddSessionControl(sessionInfoViewModel.LastMsgTimeStamp, sessionInfoViewModel);
                            }, DispatcherPriority.Background);

                            AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionInfoViewModel.SessionId);
                            if (tSession != null && string.IsNullOrEmpty(tSession.LastMsgTimeStamp) &&
                                string.IsNullOrEmpty(tSession.BurnLastMsgTimeStamp))
                            {
                                t_sessionBll.Delete(tSession);
                            }
                        }
                        break;
                    }
                }
                if (SessionControlList.Count > 0)
                {
                    SessionViewMouseLeftButtonDown(SessionControlList[0], null);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("将会话取消置顶时发生异常:" + ex.Message);
            }
        }



        /// <summary>
        /// 解析AT消息
        /// </summary>
        private string FormatAtMsg(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, string startName)
        {
            string message = string.Empty;
            AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            message = atDto.ctt.content;
            if (!string.IsNullOrEmpty(startName))
            {
                message = startName + ":" + message;
            }
            if (AntSdkService.AntSdkCurrentUserInfo.userId != msg.sendUserId)
            {
                foreach (var ids in atDto.ctt.ids)
                {
                    AtIdsName at = JsonConvert.DeserializeObject<AtIdsName>(ids.ToString());
                    if (at.id == AntSdkService.AntSdkCurrentUserInfo.userId || at.id == msg.sessionId)
                    {
                        message = "[~!@]" + message;
                        break;
                    }
                }
            }
            return message;
        }
        public SessionInfoViewModel GetSessionInfoViewModelById(string sessionId)
        {
            try
            {
                if (SessionControlList != null && SessionControlList.Count > 0)
                    return SessionControlList.OfType<SessionInfoViewModel>().FirstOrDefault(m => m.SessionId == sessionId);
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 手机端发送的已读回执
        /// </summary>
        /// <param name="msg"></param>
        public void HandlePhoneReadSession(AntSdkReceivedOtherMsg.MultiTerminalSynch msg)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                if (msg.status == (int)GlobalVariable.IsRead.Read && msg.os != (int)GlobalVariable.OSType.PC)
                {
                    SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == msg.sessionId);
                    if (vm == null) return;
                    int count = vm.UnreadCount;
                    vm.SetUnreadCount(0);
                    #region 消息免打扰模式下的本地数据处理（已注释）
                    //if (vm.GroupInfo != null && vm.GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                    //{
                    //    GroupInfoViewModel groupInfoVM = this.mainWindowViewModel._GroupListViewModel.GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == msg.sessionId);
                    //    if (groupInfoVM != null)
                    //    {
                    //        groupInfoVM.SetUnreadCount(0);
                    //        T_NoRemindGroup noRemindGroup = t_NoRemindGroupBll.GetModelByKey(vm.SessionId);
                    //        if (noRemindGroup != null)
                    //        {
                    //            noRemindGroup.GroupId = vm.SessionId;
                    //            if (groupInfoVM.IsBurnMode == GlobalVariable.BurnFlag.IsBurn)
                    //            {
                    //                noRemindGroup.BurnUnreadCount = 0;
                    //            }
                    //            else
                    //            {
                    //                noRemindGroup.UnreadCount = 0;
                    //            }
                    //            if (noRemindGroup.UnreadCount + noRemindGroup.BurnUnreadCount == 0)
                    //            {
                    //                t_NoRemindGroupBll.Delete(noRemindGroup);
                    //            }
                    //            else
                    //            {
                    //                t_NoRemindGroupBll.Update(noRemindGroup);
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                    AntSdkTsession t_session = t_sessionBll.GetModelByKey(vm.SessionId);
                    if (t_session != null)
                    {
                        if (t_session.IsBurnMode == (int)BurnFlag.IsBurn)
                        {
                            t_session.BurnUnreadCount = 0;
                        }
                        else
                        {
                            t_session.UnreadCount = 0;
                        }
                        t_sessionBll.Update(t_session);
                        //查询未读消息
                        T_SessionDAL tDal = new T_SessionDAL();
                        if (tDal.NoReadCount() == 0)
                        {
                            IsShowRedCricle(false);
                        }
                        if (msg.flag == (int)GlobalVariable.BurnFlag.IsBurn)
                        {
                            AntSdkChatMsg.ChatBase burnMsg = new AntSdkChatMsg.ChatBase();
                            burnMsg.sendUserId = msg.sendUserId;
                            burnMsg.targetId = t_session.UserId;
                            burnMsg.chatIndex = msg.chatIndex;
                            burnMsg.os = msg.os;
                            burnMsg.sessionId = msg.sessionId;
                            burnMsg.messageId = msg.messageId;
                            HandleBurnAfterReadReceipt(burnMsg, msg.MsgType, true);
                        }
                    }
                }
            });

        }
        //多终端更新未读红点
        public static event EventHandler IsShowRedCricleEventHandler;

        private static void IsShowRedCricle(bool b)
        {
            if (IsShowRedCricleEventHandler != null)
            {
                IsShowRedCricleEventHandler(b, null);
            }
        }
        public void DropOutGroup(string groupId)
        {
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == groupId);
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                DeleteSession(vm, null);
            });
        }
        private void DropOutGroup(object viewModel, EventArgs e)
        {
            GroupInfoViewModel vm = viewModel as GroupInfoViewModel;
            if (vm == null) return;
            SessionInfoViewModel sessionInfoVm = SessionControlList.FirstOrDefault(c => c.SessionId == vm.GroupInfo.groupId);
            DeleteSession(sessionInfoVm, null);
        }

        private void AddNewMember(string groupId, string picture, List<AntSdkContact_User> newGroupMemberList)
        {
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == groupId);
            if (vm != null)
            {
                vm.GroupInfo.groupPicture = picture;
                vm.Photo = picture;
                vm.SetContactPhoto(true);

            }
        }

        private void KickOutGroup(string groupId, string userID, string picture)
        {
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == groupId);
            if (vm != null)
            {
                vm.GroupInfo.groupPicture = picture;
                vm.Photo = picture;
                vm.SetContactPhoto(true);
            }
        }
        /// <summary>
        /// 更新群组成员
        /// </summary>
        public void UpdateGroupMemeber(string groupId, List<AntSdkGroupMember> members)
        {
            var sessionInfo = GetSessionInfoViewModelById(groupId);
            if (sessionInfo == null) return;
            sessionInfo.GroupMembers = members;
            var isUserMember =
                   sessionInfo.GroupMembers.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            if (isUserMember?.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner)
            {
                sessionInfo.DeleteGroupVisibility = Visibility.Visible;
            }
            else if (sessionInfo.DeleteGroupVisibility == Visibility.Visible)
            {
                sessionInfo.DeleteGroupVisibility = Visibility.Collapsed;
            }
            if (!dicTalkViewModel.ContainsKey(groupId)) return;
            talkGroupModel vm = dicTalkViewModel[groupId] as TalkGroupViewModel;
            vm?.UpdateGroupMembers(members);
        }

        public void UpdateGroupInfo(AntSdkReceivedGroupMsg.Modify sysMsg)
        {
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == sysMsg.sessionId);
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(sysMsg.content?.groupPicture))
                {
                    vm.GroupInfo.groupPicture = sysMsg.content?.groupPicture;
                    vm.Photo = sysMsg.content?.groupPicture;
                    vm.SetContactPhoto(true);
                }
                if (!string.IsNullOrEmpty(sysMsg.content?.groupName))
                {
                    vm.Name = sysMsg.content?.groupName;
                }

                if (dicTalkViewModel.ContainsKey(sysMsg.sessionId))
                {
                    TalkGroupViewModel talkGroupViewModel = dicTalkViewModel[sysMsg.sessionId] as TalkGroupViewModel;
                    if (talkGroupViewModel == null) return;
                    if (!string.IsNullOrEmpty(sysMsg.content?.groupPicture))
                    {
                        talkGroupViewModel.Picture = sysMsg.content?.groupPicture;
                    }
                    if (!string.IsNullOrEmpty(sysMsg.content?.groupName))
                    {
                        talkGroupViewModel.GroupName = sysMsg.content?.groupName;
                    }
                }
            }
        }
        /// <summary>
        /// 更新消息列表用户的在线状态
        /// </summary>
        /// <param name="userId"></param>
        ///  <param name="onLineSate"></param>
        public void UpdateUserOnlineSate(string userId, int onLineSate)
        {
            if (SessionControlList == null) return;
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.AntSdkContact_User != null && c.AntSdkContact_User.userId == userId);
            int state = onLineSate;
            if (state > 0 && GlobalVariable.UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(state))
            {
                if (vm != null)
                {
                    vm.UserOnlineStateIcon = GlobalVariable.UserOnlineSataeInfo.UserOnlineStateIconDic[state];
                    vm.IsOfflineState = false;
                    vm.AntSdkContact_User.state = state;
                }
                foreach (object item in dicTalkViewModel.Values)
                {
                    if (item is talkGroupModel)
                    {
                        talkGroupModel talkGroupViewModel = item as talkGroupModel;
                        AntSdkGroupMember user = talkGroupViewModel.GroupMembers?.Find(c => c.userId == userId);
                        if (user != null)
                        {
                            GroupMemberViewModel groupMemberViewModel =
                                talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.FirstOrDefault(
                                    c => c.Member.userId == userId);
                            if (groupMemberViewModel != null)
                            {
                                groupMemberViewModel.IsOfflineState = false;
                                if (GlobalVariable.UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(state))
                                    groupMemberViewModel.UserOnlineStateIcon =
                                        GlobalVariable.UserOnlineSataeInfo.UserOnlineStateMinIconDic[state];
                                break;
                            }
                        }
                    }
                    else
                    {
                        var talkPersonViewModel = item as talkPersonModel;
                        if (talkPersonViewModel?.user != null && talkPersonViewModel.user.userId == userId)
                        {
                            talkPersonViewModel.user.state = state;
                            break;
                        }
                    }
                }

            }
            else
            {
                if (vm != null)
                {
                    vm.UserOnlineStateIcon = "";
                    vm.IsOfflineState = true;
                    vm.AntSdkContact_User.state = state;
                }
                foreach (object item in dicTalkViewModel.Values)
                {
                    if (item is talkGroupModel)
                    {
                        TalkGroupViewModel talkGroupViewModel = item as TalkGroupViewModel;
                        AntSdkGroupMember user = talkGroupViewModel?.GroupMembers?.Find(c => c.userId == userId);
                        if (user != null)
                        {
                            GroupMemberViewModel groupMemberViewModel =
                                talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.FirstOrDefault(
                                    c => c.Member.userId == userId);
                            if (groupMemberViewModel != null)
                            {
                                groupMemberViewModel.UserOnlineStateIcon = "";
                                groupMemberViewModel.IsOfflineState = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var talkPersonViewModel = item as talkPersonModel;
                        if (talkPersonViewModel?.user != null && talkPersonViewModel.user.userId == userId)
                        {
                            talkPersonViewModel.user.state = state;
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateUserInfo(string userId, string picture, string pictureUrl)
        {
            if (SessionControlList == null) return;
            if (dicTalkViewModel.ContainsKey(userId))
            {
                TalkViewModel talkViewModel = dicTalkViewModel[userId] as TalkViewModel;
                if (talkViewModel == null) return;
                if (!string.IsNullOrEmpty(picture))
                {
                    talkViewModel.Picture = picture;
                }
            }
            if (!string.IsNullOrEmpty(picture))
            {
                SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.AntSdkContact_User != null && c.AntSdkContact_User.userId == userId);
                if (vm != null)
                {
                    vm.AntSdkContact_User.picture = pictureUrl;
                    vm.Photo = picture;
                    vm.SetContactPhoto();
                }
                foreach (object item in dicTalkViewModel.Values)
                {
                    TalkGroupViewModel talkGroupViewModel = item as TalkGroupViewModel;
                    AntSdkGroupMember user = talkGroupViewModel?.GroupMembers?.Find(c => c.userId == userId);
                    if (user != null)
                    {
                        user.picture = pictureUrl;
                        GroupMemberViewModel groupMemberViewModel = talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.FirstOrDefault(c => c.Member.userId == userId);
                        if (groupMemberViewModel != null)
                        {
                            groupMemberViewModel.Member.picture = pictureUrl;
                            groupMemberViewModel.Photo = picture;
                            groupMemberViewModel.SetContactPhoto();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUserInfo(AntSdkContact_User user, bool isUpdateUserpicture = true)
        {
            if (SessionControlList == null) return;
            #region 更新头像
            if (isUpdateUserpicture)
            {
                if (!string.IsNullOrEmpty(user.picture))
                {
                    var imageFilePath = string.Empty;
                    if (publicMethod.IsUrlRegex(user.picture))
                    {
                        var index = user.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                        var fileName = user.picture.Substring(index, user.picture.Length - index);
                        if (File.Exists(publicMethod.UserHeadImageFilePath() + fileName)) return;
                        var downloadUserHeadImagePath = publicMethod.DownloadFilePath() +
                                                        "\\DownloadUserHeadImage\\";
                        if (!Directory.Exists(downloadUserHeadImagePath))
                            Directory.CreateDirectory(downloadUserHeadImagePath);
                        if (!Directory.Exists(publicMethod.UserHeadImageFilePath()))
                            Directory.CreateDirectory(publicMethod.UserHeadImageFilePath());
                        imageFilePath = FileHelper.DownloadPicture(user.picture,
                            downloadUserHeadImagePath + fileName, -1,
                            publicMethod.UserHeadImageFilePath(), user.userId);
                        if (!string.IsNullOrEmpty(imageFilePath))
                        {
                            //user.picture = imageFilePath;
                            var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                m => m.UserID == user.userId);
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
                                    UserID = user.userId,
                                    Url = imageFilePath
                                });
                            }
                        }
                    }
                    var vm = SessionControlList.FirstOrDefault(
                        c => c.AntSdkContact_User != null && c.AntSdkContact_User.userId == user.userId);
                    if (vm != null)
                    {
                        vm.AntSdkContact_User.picture = user.picture;
                        vm.Photo = string.IsNullOrEmpty(imageFilePath) ? user.picture : imageFilePath;
                        vm.SetContactPhoto(true);
                    }
                    foreach (var item in dicTalkViewModel.Values)
                    {
                        var talkPerson = item as TalkViewModel;
                        //个人
                        if (talkPerson != null)
                        {
                        }
                        //群组
                        else
                        {
                            var talkGroupViewModel = item as TalkGroupViewModel;
                            var member = talkGroupViewModel?.GroupMembers?.Find(c => c.userId == user.userId);
                            if (member == null) continue;
                            {
                                member.picture = user.picture;
                                var groupMemberViewModel =
                                    talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.FirstOrDefault(
                                        c => c.Member.userId == user.userId);
                                if (groupMemberViewModel == null) continue;
                                groupMemberViewModel.Member.picture = user.picture;
                                groupMemberViewModel.Photo = string.IsNullOrEmpty(imageFilePath)
                                    ? user.picture
                                    : imageFilePath;
                                ;
                            }
                        }
                    }
                }


            }
            #endregion
            #region 更新名字
            if (!string.IsNullOrEmpty(user.userName))
            {
                //更新会话列表里的名称
                var vm =
                    SessionControlList.FirstOrDefault(
                        c => c.AntSdkContact_User != null && c.AntSdkContact_User.userId == user.userId);
                if (vm != null)
                {
                    vm.AntSdkContact_User.userName = user.userName;
                    vm.Name = string.IsNullOrEmpty(user.userNum) ? user.userName : user.userNum + user.userName;
                    if (user.status == 0 && user.state == 0)
                        vm.Name = vm.Name + "（停用）";
                }
                //更新聊天框名称
                foreach (var item in dicTalkViewModel.Values)
                {
                    var talkPerson = item as TalkViewModel;
                    //个人
                    if (talkPerson != null)
                    {
                        if (talkPerson.user.userId == user.userId)
                        {
                            talkPerson.UserName = string.IsNullOrEmpty(user.userNum)
                                ? user.userName
                                : user.userNum + user.userName;
                            if (user.status == 0 && user.state == 0)
                                talkPerson.UserName = talkPerson.UserName + "（停用）";
                        }
                    }
                    //群组
                    else
                    {
                        var talkGroupViewModel = item as TalkGroupViewModel;
                        var member = talkGroupViewModel?.GroupMembers?.FirstOrDefault(c => c.userId == user.userId);
                        if (member == null) continue;
                        {
                            member.userName = user.userName;
                            member.userNum = user.userNum;
                            var groupMemberViewModel =
                                talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.FirstOrDefault(
                                    c => c.Member.userId == user.userId);
                            if (groupMemberViewModel == null) continue;
                            groupMemberViewModel.Member.userName = user.userName;
                            groupMemberViewModel.Member.userNum = user.userNum;
                            groupMemberViewModel.Name = string.IsNullOrEmpty(user.userNum)
                                ? user.userName
                                : user.userNum + user.userName;
                            if (user.status == 0 && user.state == 0)
                                groupMemberViewModel.Name = groupMemberViewModel.Name + "（停用）";
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary> 
        /// 增加一个新的讨论组会话
        /// </summary>
        /// <param name="vm"></param>
        public void AddGroupSession(AntSdkGroupInfo groupInfo, bool isClick, string lastMsgTimeStamp = "")
        {
            dispatcher.Invoke(new Action(() =>
            {
                //判断是否已存在
                SessionInfoViewModel sessionInfoViewModel = SessionControlList.FirstOrDefault(c => c.SessionId == groupInfo.groupId);
                if (sessionInfoViewModel != null)
                {
                    if (isClick)
                    {
                        SessionViewMouseLeftButtonDown(sessionInfoViewModel, null);
                    }
                    return;
                }
                SessionInfoModel model = new SessionInfoModel();
                //model.SessionId = DataConverter.GetSessionID(vm.GroupInfo.groupId);
                model.SessionId = groupInfo.groupId;
                model.name = groupInfo.groupName;
                model.photo = groupInfo.groupPicture;
                model.unreadCount = 0;

                model.lastTime = string.IsNullOrEmpty(lastMsgTimeStamp) ? DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000" : lastMsgTimeStamp;

                //IList<T_NoRemindGroup> t_NoRemindGroupList = t_NoRemindGroupBll.GetList();
                #region 消息免打扰（已注释）
                //T_NoRemindGroup temp = t_NoRemindGroupBll.GetModelByKey(model.SessionId);
                //if (temp != null)
                //{
                //    model.unreadCount = temp.UnreadCount;
                //    model.lastTime = temp.LastMsgTimeStamp;
                //    model.lastMessage = temp.LastMsg;
                //    model.lastChatIndex = temp.LastChatIndex;
                //    T_Session t_session = new T_Session();
                //    t_session.GroupId = t_session.SessionId = temp.GroupId;
                //    t_session.UnreadCount = temp.UnreadCount;
                //    t_session.LastMsg = temp.LastMsg;
                //    t_session.LastMsgTimeStamp = temp.LastMsgTimeStamp;
                //    t_session.LastChatIndex = temp.LastChatIndex;
                //    t_session.BurnUnreadCount = temp.BurnUnreadCount;
                //    t_session.BurnLastMsg = temp.BurnLastMsg;
                //    t_session.BurnLastMsgTimeStamp = temp.BurnLastMsgTimeStamp;
                //    t_session.BurnLastChatIndex = temp.BurnLastChatIndex;
                //    if (vm.IsBurnMode == BurnFlag.IsBurn)
                //    {
                //        t_session.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                //    }
                //    else
                //    {
                //        t_session.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                //    }
                //    t_sessionBll.Insert(t_session);
                //}
                #endregion

                var burnMode = BurnFlag.NotIsBurn;
                if (BurnSessionList.Contains(groupInfo.groupId))
                    burnMode = BurnFlag.IsBurn;
                var contanctSession = t_sessionBll.GetModelByKey(model.SessionId);
                if (contanctSession != null)
                {
                    if (burnMode == BurnFlag.NotIsBurn)
                        model.lastMessage = contanctSession.LastMsg;
                    else
                    {
                        model.lastMessage = contanctSession.BurnLastMsg;
                    }
                }
                SessionInfoViewModel tempVM = new SessionInfoViewModel(groupInfo, burnMode, null, model);
                tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                tempVM.DeleteSessionEvent += DeleteSession;
                tempVM.PostTopSessionEvent += PostTopSession;
                tempVM.CancelTopSessionEvent += CacelTopSession;
                AddSessionControl(tempVM.LastMsgTimeStamp, tempVM);
                if (isClick)
                {
                    SessionViewMouseLeftButtonDown(tempVM, null);
                }
            }));
        }

        /// <summary>
        /// 新增一个单人会话
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isClick"></param>
        public void AddContactSession(string userId, bool isClick)
        {
            //判断是否已存在
            SessionInfoViewModel sessionInfoViewModel = SessionControlList.FirstOrDefault(c => c.AntSdkContact_User != null && c.AntSdkContact_User.userId == userId);
            if (sessionInfoViewModel != null)
            {
                if (isClick)
                {
                    SessionViewMouseLeftButtonDown(sessionInfoViewModel, null);
                }
                return;
            }
            AntSdkContact_User AntSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == userId);
            if (AntSdkContact_User == null) return;
            SessionInfoModel model = new SessionInfoModel();
            model.SessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, userId);
            //model.name = AntSdkContact_User.userName;
            if (string.IsNullOrEmpty(AntSdkContact_User.userNum))
            {
                model.name = AntSdkContact_User.userName;
            }
            else
            {
                model.name = AntSdkContact_User.userNum + AntSdkContact_User.userName;
            }
            var contanctSession = t_sessionBll.GetModelByKey(model.SessionId);
            if (contanctSession != null)
            {
                model.lastMessage = contanctSession.LastMsg;
            }
            model.photo = AntSdkContact_User.picture;
            model.unreadCount = 0;
            model.lastTime = DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000";
            //model.MyselfMsgCount = 0;
            dispatcher.Invoke(new Action(() =>
            {
                SessionInfoViewModel tempVM = new SessionInfoViewModel(AntSdkContact_User, model);
                tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                tempVM.DeleteSessionEvent += DeleteSession;
                tempVM.PostTopSessionEvent += PostTopSession;
                tempVM.CancelTopSessionEvent += CacelTopSession;
                AddSessionControl(tempVM.LastMsgTimeStamp, tempVM);
                if (isClick)
                {
                    SessionViewMouseLeftButtonDown(tempVM, null);
                }
            }));
        }
        /// <summary>
        /// 获取总的未读消息数（讨论组消息免打扰的除外）
        /// </summary>
        /// <returns></returns>
        public int GetTotelUnreadCount()
        {
            int unreadCount = 0;
            if (SessionControlList == null || SessionControlList.Count == 0) return unreadCount;
            var tempSessionList = SessionControlList.ToList();
            var tempList = tempSessionList.Where(c => (c.GroupInfo == null || c.GroupInfo.state == (int)GlobalVariable.MsgRemind.Remind) && (c.UnreadCount) > 0 && c.UnreadCountVisibility == Visibility.Visible).ToList();
            if (tempList == null || tempList.Count == 0) return unreadCount;
            unreadCount = tempList.Sum(c => c.UnreadCount);
            return unreadCount;
        }

        private NoticeRead read;
        private NoticeReadViewModel readModel;
        private bool isExit = true;

        /// <summary>
        /// 讨论组收到新的通知，显示通知图标
        /// </summary>
        /// <param name="groupInfoVM"></param>
        /// <param name="groupId"></param>
        public void AddNotice(AntSdkGroupInfo groupInfo, List<Notice_content> noticeList, bool isLast = true)
        {
            foreach (Notice_content notice in noticeList)
            {
                if (!UnreadNoticeList.Select(c => c.notificationId).Contains(notice.notificationId))
                {
                    UnreadNoticeList.Add(notice);
                    isExit = false;
                }
            }
            if (isExit)
                return;
            if (!isLast) return;
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                SessionInfoViewModel vm =
                    SessionControlList.FirstOrDefault(c => c.SessionId == groupInfo.groupId);
                if (vm == null)
                {
                    AddGroupSession(groupInfo, false, noticeList.Last().createTime);
                    vm = SessionControlList.FirstOrDefault(c => c.SessionId == groupInfo.groupId);
                }
                if (vm != null)
                {
                    vm.ImageNoticeVisibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(vm.LastMsgTimeStamp)
                        || string.CompareOrdinal(vm.LastMsgTimeStamp, noticeList.Last().createTime) < 0)
                    {
                        vm.LastMsgTimeStamp = noticeList.Last().createTime;
                        vm.LastMessage = "[公告]" + noticeList.Last().title;
                        vm.LastTime = DataConverter.FormatTimeByTimeStamp(vm.LastMsgTimeStamp);
                        //if (!vm.IsMouseClick)
                        //{
                        //    vm.SetUnreadCount(vm.UnreadCount + noticeList.Count);
                        //}
                    }
                    else if (string.IsNullOrEmpty(vm.LastMessage))
                    {
                        vm.LastMessage = "[公告]" + noticeList.Last().title;
                    }
                    SessionControlList.Remove(vm);
                    AddSessionControl(vm.LastMsgTimeStamp, vm);

                    ;

                    #region 更新数据库T_Session表

                    AntSdkTsession tSession = t_sessionBll.GetModelByKey(groupInfo.groupId) ??
                                              new AntSdkTsession();
                    ;
                    if (string.CompareOrdinal(tSession.LastMsgTimeStamp, noticeList.Last().createTime) < 0)
                        tSession.LastMsgTimeStamp = vm.LastMsgTimeStamp;
                    tSession.LastMsg = vm.LastMessage;
                    if (vm.UnreadCount > 0)
                        tSession.UnreadCount = vm.UnreadCount;
                    if (string.IsNullOrEmpty(tSession.LastChatIndex))
                    {
                        tSession.LastChatIndex = "0";
                    }
                    if (string.IsNullOrEmpty(tSession.SessionId))
                    {
                        tSession.SessionId = tSession.GroupId = groupInfo.groupId;
                        t_sessionBll.Insert(tSession);
                    }
                    else
                    {
                        t_sessionBll.Update(tSession);
                    }

                    #endregion

                    //}
                }
                if (vm == null || !vm.IsMouseClick) return;
                vm.ImageNoticeVisibility = Visibility.Collapsed;
                if (read == null || !read.IsLoaded)
                {
                    read = new NoticeRead();
                    readModel = new NoticeReadViewModel();
                    read.DataContext = readModel;
                    readModel.LoadNoticeData(noticeList);
                    //read.Owner = Antenna.Framework.Win32.GetTopWindow();
                    read.Owner = Window.GetWindow(sessionListView);
                    read.ShowDialog();
                }
                else
                    readModel.LoadNoticeData(noticeList);


            }, DispatcherPriority.Background);
        }
        #region 打卡
        /// <summary>
        /// 根据本地数据添加Session
        /// </summary>
        /// <param name="sessionId"></param>
        public void AddAssistantSession(string sessionId)
        {
            var tempSession = t_sessionBll.GetModelByKey(sessionId);
            if (tempSession == null) return;
            SessionInfoViewModel tempVM = null;
            if (tempSession.UserId == GlobalVariable.MassAssistantId) //群发助手
            {
                //dispatcher.Invoke(new Action(() =>
                //{
                tempVM = new SessionInfoViewModel(tempSession);
                tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                tempVM.DeleteSessionEvent += DeleteSession;
                tempVM.PostTopSessionEvent += PostTopSession;
                tempVM.CancelTopSessionEvent += CacelTopSession;
                //SessionControlList.Add(tempVM);
                SessionControlList.Remove(tempVM);
                AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
                //}));
            }
            if (tempSession.UserId == GlobalVariable.AttendAssistantId)//考勤助手
            {
                tempVM = new SessionInfoViewModel(tempSession, SessionType.AttendanceAssistant);
                tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                tempVM.DeleteSessionEvent += DeleteSession;
                tempVM.PostTopSessionEvent += PostTopSession;
                tempVM.CancelTopSessionEvent += CacelTopSession;
                //SessionControlList.Add(tempVM);
                SessionControlList.Remove(tempVM);
                AddSessionControl(tempVM.LastMsgTimeStamp, tempVM, true);
            }
        }
        /// <summary>
        /// 考勤记录
        /// </summary>
        public void AddAttendanceRecords(AntSdkReceivedOtherMsg.AttendanceRecordVerify msg)
        {
            var attendAssistantSession =
                    SessionControlList.FirstOrDefault(c => c._SessionType == GlobalVariable.SessionType.AttendanceAssistant);
            if (msg.MsgType == AntSdkMsgType.CheckInVerify)
            {
                var sessionId = DataConverter.GetSessionID(msg.sessionId,
                    GlobalVariable.AttendAssistantId);
                var isNewSession = false;
                var tSession = t_sessionBll.GetModelByKey(sessionId);

                if (tSession == null)
                {
                    isNewSession = true;
                    tSession = new AntSdkTsession();
                }
                tSession.SessionId = DataConverter.GetSessionID(msg.sessionId,
                    GlobalVariable.AttendAssistantId); ;
                tSession.UserId = GlobalVariable.AttendAssistantId;
                tSession.GroupId = string.Empty;
                tSession.UnreadCount = 1;
                tSession.LastMsg = "打卡记录";
                tSession.LastMsgTimeStamp = msg.sendTime;
                tSession.LastChatIndex = msg.chatIndex;

                if (attendAssistantSession == null)
                {
                    dispatcher.Invoke(new Action(() =>
                    {
                        attendAssistantSession = new SessionInfoViewModel(tSession, SessionType.AttendanceAssistant);
                        attendAssistantSession.IsNewAttendance = true;
                        attendAssistantSession.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                        attendAssistantSession.DeleteSessionEvent += DeleteSession;
                        attendAssistantSession.PostTopSessionEvent += PostTopSession;
                        attendAssistantSession.CancelTopSessionEvent += CacelTopSession;
                        AddSessionControl(attendAssistantSession.LastMsgTimeStamp, attendAssistantSession);
                    }));
                }
                else
                {
                    dispatcher.Invoke(new Action(() =>
                    {
                        AntSdkChatMsg.ChatBase massMsgBase = new AntSdkChatMsg.ChatBase();
                        massMsgBase.chatIndex = msg.chatIndex;
                        massMsgBase.sourceContent = "打卡记录";
                        massMsgBase.sessionId = msg.sessionId;
                        massMsgBase.sendTime = msg.sendTime;
                        attendAssistantSession.RefreshMassAssistantSession(massMsgBase);
                        if (!attendAssistantSession.IsMouseClick)
                            attendAssistantSession.IsNewAttendance = true;
                        SessionControlList.Remove(attendAssistantSession);
                        AddSessionControl(attendAssistantSession.LastMsgTimeStamp, attendAssistantSession);
                    }));
                }
                if (isNewSession)
                {
                    //更新SQLite:新增会话
                    ThreadPool.QueueUserWorkItem(m => t_sessionBll.Insert(tSession));
                }
                else
                {
                    //更新SQLite:更新会话
                    ThreadPool.QueueUserWorkItem(m => t_sessionBll.Update(tSession));
                }
            }
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                mainWindowViewModel.CloseExitVerify();
            }));
            if (checkInListVm != null && attendAssistantSession != null && attendAssistantSession.IsMouseClick)
            {
                checkInListVm.GetServicePunchClocksData(true);
            }
        }
        CheckInListViewModel checkInListVm;
        bool firstAttendanceRecord;
        /// <summary>
        /// 打卡考勤助手
        /// </summary>
        private void RefreshAttendanceRecordViewRightPart()
        {
            firstAttendanceRecord = true;
            if (_checkInListView == null)
            {
                _checkInListView = new Views.CheckInListView();
                checkInListVm = new CheckInListViewModel();
                _checkInListView.DataContext = checkInListVm;
                MainWindowViewModel.MainExhibitControl.Children.Clear();
                MainWindowViewModel.MainExhibitControl.Children.Add(_checkInListView);
                if (_checkInListView != null)
                {
                    checkInListVm.GetServicePunchClocksData(true);
                    _checkInListView.attendanceRecordsScroll.ScrollChanged += AttendanceRecordsScroll_ScrollChanged;
                    _checkInListView.attendanceRecordsScroll?.ScrollToEnd();
                }
            }
            else
            {
                if (checkInListVm != null)
                {
                    checkInListVm.timer?.Stop();
                    _checkInListView.DataContext = null;
                    checkInListVm = null;
                }
                checkInListVm = new CheckInListViewModel();
                _checkInListView.DataContext = checkInListVm;
                MainWindowViewModel.MainExhibitControl.Children.Clear();
                MainWindowViewModel.MainExhibitControl.Children.Add(_checkInListView);
                checkInListVm.GetServicePunchClocksData(true);
                _checkInListView.attendanceRecordsScroll?.ScrollToEnd();
            }
        }

        private void AttendanceRecordsScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_checkInListView.attendanceRecordsScroll.VerticalOffset == 0 && !firstAttendanceRecord)
            {
                if (checkInListVm != null)
                {
                    checkInListVm.GetServicePunchClocksData();
                }
            }
            else
            {
                firstAttendanceRecord = false;
            }
        }
        /// <summary>
        /// 滚动条是否滚动到最底部
        /// </summary>
        public bool IsVerticalScrollBarAtButtom
        {
            get
            {
                bool isAtButtom = false;

                // get the vertical scroll position  
                double dVer = _checkInListView.attendanceRecordsScroll.VerticalOffset;

                //get the vertical size of the scrollable content area  
                double dViewport = _checkInListView.attendanceRecordsScroll.ViewportHeight;

                //get the vertical size of the visible content area  
                double dExtent = _checkInListView.attendanceRecordsScroll.ExtentHeight;

                if (dVer != 0)
                {
                    if (dVer + dViewport == dExtent)
                    {
                        isAtButtom = true;
                    }
                    else
                    {
                        isAtButtom = false;
                    }
                }
                else
                {
                    isAtButtom = false;
                }

                if (_checkInListView.attendanceRecordsScroll.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled
                    || _checkInListView.attendanceRecordsScroll.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                {
                    isAtButtom = true;
                }

                return isAtButtom;
            }
        }
        #endregion
        /// <summary>
        /// 通知已读或者群主刪除通知
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <param name="isDelete">true：群主刪除通知，false：通知已读</param>
        public void RemoveNotice(string notificationId, bool isDelete)
        {
            Notice_content content = UnreadNoticeList.Find(c => c.notificationId == notificationId);
            if (content == null) return;
            UnreadNoticeList.Remove(content);
            var tempUnreadNoticeList = UnreadNoticeList.Where(c => c.targetId == content.targetId);
            if (tempUnreadNoticeList.Any()) return;
            {
                SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == content.targetId);
                if (vm != null)
                {
                    vm.ImageNoticeVisibility = Visibility.Collapsed;
                    if (!vm.IsMouseClick || !isDelete) return;
                    if (!dicTalkViewModel.ContainsKey(content.targetId)) return;
                    var talkGroup = dicTalkViewModel[content.targetId] as talkGroupModel;
                    if (talkGroup?.NoticeWindowListsViewModel == null) return;
                    var model = talkGroup.NoticeWindowListsViewModel as NoticeWindowListsViewModel;
                    var noticeInfo = model?.NoticeList
                        .FirstOrDefault(
                            m => m.NotificationId == notificationId);
                    if (noticeInfo != null)
                        model.NoticeList.Remove(
                            noticeInfo);
                }
            }
        }

        /// <summary>
        /// 讨论组聊天框弹出未读通知
        /// </summary>
        private void ShowLastUnreadNotice(string groupId)
        {
            if (UnreadNoticeList == null || UnreadNoticeList.Count == 0) return;
            IEnumerable<Notice_content> noticeListE = UnreadNoticeList.OrderByDescending(c => c.createTime).Where(c => c.targetId == groupId);
            var noticeContents = noticeListE as Notice_content[] ?? noticeListE.ToArray();

            List<Notice_content> lastNoticeList = noticeContents.ToList();
            if (lastNoticeList.Count == 0) return;
            NoticeReadViewModel readModel = new NoticeReadViewModel();
            NoticeRead read = new NoticeRead();
            read.DataContext = readModel;
            readModel.LoadNoticeData(lastNoticeList);
            read.Owner = Antenna.Framework.Win32.GetTopWindow();
            read.ShowDialog();
        }

        public void SelectUploadFiles(string[] fileNames)
        {
            TalkWindowView view = MainWindowViewModel.MainExhibitControl.Children[0] as TalkWindowView;
            if (view != null)
            {
                TalkViewModel vm = view.DataContext as TalkViewModel;
                vm.SelectUploadFiles(fileNames);
                return;
            }
            else
            {
                TalkGroupWindowView groupView = MainWindowViewModel.MainExhibitControl.Children[0] as TalkGroupWindowView;
                if (groupView != null)
                {
                    TalkGroupViewModel vm = groupView.DataContext as TalkGroupViewModel;
                    vm.SelectUploadFiles(fileNames);
                    return;
                }
            }
        }

        object SendMassMsgLock = new object();
        /// <summary>
        /// 发送群发消息(可能是失败之后重发)
        /// </summary>
        /// <param name="msg"></param>
        public void SendMassMsg(AntSdkMassMsgCtt msg)
        {
            lock (SendMassMsgLock)
            {
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                AntSdkTsession tSession = new AntSdkTsession();
                tSession.SessionId = msg.sessionId;
                tSession.UserId = GlobalVariable.MassAssistantId;
                tSession.GroupId = string.Empty;
                tSession.UnreadCount = 0;
                tSession.LastMsg = msg.content;
                tSession.LastMsgTimeStamp = DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000";
                tSession.LastChatIndex = string.Empty;
                SessionInfoViewModel massAssistantSession =
                    SessionControlList.FirstOrDefault(c => c._SessionType == GlobalVariable.SessionType.MassAssistant);
                if (massAssistantSession == null)
                {

                    massAssistantSession = new SessionInfoViewModel(msg);
                    massAssistantSession.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                    massAssistantSession.DeleteSessionEvent += DeleteSession;
                    massAssistantSession.PostTopSessionEvent += PostTopSession;
                    massAssistantSession.CancelTopSessionEvent += CacelTopSession;
                    AddSessionControl(massAssistantSession.LastMsgTimeStamp, massAssistantSession);

                    //更新SQLite:新增会话
                    ThreadPool.QueueUserWorkItem(m => t_sessionBll.Insert(tSession));
                }
                else
                {

                    AntSdkChatMsg.ChatBase massMsgBase = new AntSdkChatMsg.ChatBase();
                    massMsgBase.chatIndex = msg.chatIndex;
                    massMsgBase.sourceContent = msg.content;
                    massMsgBase.sessionId = msg.sessionId;
                    massMsgBase.sendTime = msg.sendTime;
                    massAssistantSession.RefreshMassAssistantSession(massMsgBase);

                    SessionControlList.Remove(massAssistantSession);
                    AddSessionControl(massAssistantSession.LastMsgTimeStamp, massAssistantSession);

                    //更新SQLite:更新会话
                    ThreadPool.QueueUserWorkItem(m => t_sessionBll.Update(tSession));
                }
                if (massAssistantSession?.IsMouseClick == false)
                {
                    SessionViewMouseLeftButtonDown(massAssistantSession, null);
                }
                if (_massMsgListView == null)
                    RefreshMainViewRightPart();
                MassMsgListViewModel massMsgListViewModel = _massMsgListView.DataContext as MassMsgListViewModel;
                if (massMsgListViewModel == null) return;
                if (massMsgListViewModel.MassMsgControlList.FirstOrDefault(c => c.MessageId == msg.messageId) != null) return;
                massMsgListViewModel.AddNewMassMsg(msg);
                //更新SQLite:新增群发消息
                ThreadPool.QueueUserWorkItem(m =>
                {
                    if (t_massMsgBll == null)
                    {
                        t_massMsgBll = new BaseBLL<AntSdkMassMsgCtt, T_MassMsgDAL>();
                    }
                    t_massMsgBll.Insert(msg);
                });
                //});
            }
        }

        /// <summary>
        /// 处理群发消息回执
        /// </summary>
        /// <param name="massMsgReceipt"></param>
        public void HandleMassMsgReceipt(AntSdkChatMsg.ChatBase massMsgReceipt)
        {
            lock (SendMassMsgLock)
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    SessionInfoViewModel massAssistantSession =
                        SessionControlList.FirstOrDefault(c => c._SessionType == GlobalVariable.SessionType.MassAssistant);
                    if (massAssistantSession == null) //设置一秒延时，防止先收到回执，后收到发消息的通知
                    {
                        massAssistantSession =
                            SessionControlList.FirstOrDefault(
                                c => c._SessionType == GlobalVariable.SessionType.MassAssistant);
                    }
                    massMsgReceipt.sourceContent = massAssistantSession?.LastMessage;
                    //Thread.Sleep(10000);
                    if (massAssistantSession != null)
                    {
                        massAssistantSession.RefreshMassAssistantSession(massMsgReceipt);
                        SessionControlList.Remove(massAssistantSession);
                        AddSessionControl(massAssistantSession.LastMsgTimeStamp, massAssistantSession);
                        (_massMsgListView?.DataContext as MassMsgListViewModel)?.RefreshMassMsg(massMsgReceipt);
                    }
                    //更新SQLite:更新会话和群发消息
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        AntSdkTsession tSession = new AntSdkTsession();
                        tSession.SessionId = massMsgReceipt.sessionId;
                        tSession.UserId = GlobalVariable.MassAssistantId;
                        tSession.GroupId = string.Empty;
                        tSession.UnreadCount = 0;
                        tSession.LastMsg = massMsgReceipt.sourceContent;
                        tSession.LastMsgTimeStamp = massMsgReceipt.sendTime;
                        tSession.LastChatIndex = massMsgReceipt.chatIndex;
                        t_sessionBll.Update(tSession);
                        if (t_massMsgBll == null)
                        {
                            t_massMsgBll = new BaseBLL<AntSdkMassMsgCtt, T_MassMsgDAL>();
                        }
                        AntSdkMassMsgCtt massMsgCtt = new AntSdkMassMsgCtt();
                        massMsgCtt.messageId = massMsgReceipt.messageId;
                        massMsgCtt.sessionId = massMsgReceipt.sessionId;
                        massMsgCtt.chatIndex = massMsgReceipt.chatIndex;
                        massMsgCtt.sendTime = massMsgReceipt.sendTime;
                        t_massMsgBll.Update(massMsgCtt);
                    });
                }));

            }
        }

        public void HandleMsessageDestroyReceipt(AntSdkChatMsg.ChatBase msg, AntSdkMsgType msgType, bool isFlag)
        {
            if (msgType == AntSdkMsgType.Revocation)
            {
                HandleRevocationReceipt(msg, false);
            }
            else
            {
                HandleBurnAfterReadReceipt(msg, msgType, isFlag);
            }
        }

        /// <summary>
        /// 用户A收到消息服务转发的B已读阅后即焚消息的通知
        /// 或者用户B收到A的阅后即焚消息，发送已读回执给消息服务
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgType"></param>
        public void HandleBurnAfterReadReceipt(AntSdkChatMsg.ChatBase msg, AntSdkMsgType msgType, bool isFlag)
        {
            try
            {
                SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == msg.sessionId);
                #region 获取已读的chatIndex
                string readIndex;
                //用户A收到消息服务转发的B已读阅后即焚消息的通知
                if (isFlag)
                {
                    if (dicTalkViewModel.ContainsKey(msg.sendUserId))
                    {
                        if (string.IsNullOrEmpty(msg.sourceContent)) return;
                        readIndex = msg.sourceContent;
                        //string errMsg = string.Empty;
                        //if (!DataConverter.GetValueByJsonKey("readIndex", msg.content, ref readIndex, ref errMsg))
                        //{
                        //    return;
                        //}
                        //do something

                        if (dicTalkViewModel != null && dicTalkViewModel.ContainsKey(msg.sendUserId))
                            (dicTalkViewModel[msg.sendUserId] as TalkViewModel)?.HideMsgAndShowRecallMsg(msg.messageId);

                        //设置图片无效
                        if (dicTalkViewModel != null && dicTalkViewModel.ContainsKey(msg.sendUserId))
                        {
                            var img = (dicTalkViewModel[msg.sendUserId] as TalkViewModel)?.listDictImgUrls.LastOrDefault
                                (
                                    m => m.ChatIndex == readIndex);
                            if (img != null)
                            {
                                img.IsEffective = burnMsg.IsEffective.NotEffective;
                            }
                        }
                    }
                    else //用户B收到A的阅后即焚消息，发送已读回执给消息服务
                    {
                        readIndex = msg.chatIndex;
                        if (msg.os != (int)OSType.PC)
                        {
                            //B在其他端读了该条消息
                            if (dicTalkViewModel != null && dicTalkViewModel.Keys.Contains(msg.targetId))
                                (dicTalkViewModel[msg.targetId] as TalkViewModel)?.HideMsgAndShowRecallMsg(msg.messageId);
                        }
                    }



                    //删除本地记录

                    this.t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                        AntSdkService.AntSdkCurrentUserInfo.userId, msg.messageId);
                }
                else
                {
                    readIndex = msg.chatIndex;
                }
                #endregion
                AntSdkTsession tSession = t_sessionBll.GetModelByKey(msg.sessionId);
                if (tSession == null)
                    return;
                //如果读到的阅后即焚消息不是最后一条消息则无需处理
                int chatIndex = -1;
                int.TryParse(readIndex, out chatIndex);
                if (chatIndex < int.Parse(tSession.LastChatIndex))
                {
                    return;
                }
                SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg = t_chat.GetBeforeRecordByChatIndex(readIndex, tSession.SessionId);
                if (chatMsg == null)
                {
                    if (vm != null)
                    {
                        vm.LastChatIndex = string.Empty;
                        vm.LastMessage = string.Empty;
                        vm.LastMsgTimeStamp = string.Empty;
                        vm.LastTime = string.Empty;
                    }
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        AntSdkTsession t_session = new AntSdkTsession();
                        t_session.SessionId = tSession.SessionId;
                        t_session.LastMsg = string.Empty;
                        t_session.LastMsgTimeStamp = tSession.LastMsgTimeStamp;
                        t_session.LastChatIndex = tSession.LastChatIndex;
                        t_sessionBll.Update(t_session);
                        //AntSdkTsession session = new AntSdkTsession();
                        //session.SessionId = vm.SessionId;
                        //t_sessionBll.Delete(session);
                    });
                }
                else
                {
                    string lastMsg = PublicMessageFunction.FormatLastMessageContent(chatMsg.MsgType, chatMsg, false);
                    if (vm != null)
                    {
                        vm.LastChatIndex = chatMsg.chatIndex;
                        vm.LastMessage = lastMsg;
                        vm.LastMsgTimeStamp = chatMsg.sendTime;
                        vm.LastTime = DataConverter.FormatTimeByTimeStamp(vm.LastMsgTimeStamp);
                    }
                    //ThreadPool.QueueUserWorkItem(m =>
                    //{
                    AntSdkTsession t_session = new AntSdkTsession();
                    t_session.SessionId = chatMsg.sessionId;
                    t_session.LastMsg = lastMsg;
                    t_session.LastMsgTimeStamp = chatMsg.sendTime;
                    t_session.LastChatIndex = chatMsg.chatIndex;
                    if (vm != null)
                        SessionInfoListSort(vm, t_session);
                    else
                    {
                        //tSession.TopIndex!=null
                        t_sessionBll.Update(t_session);
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[HandleBurnAfterReadReceipt]:" + e.Message + "," + e.StackTrace);
            }
        }


        /// <summary>
        /// 消息撤回通知
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isFlag"></param>
        public void HandleRevocationReceipt(AntSdkChatMsg.ChatBase msg, bool isFlag)
        {
            try
            {
                SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == msg.sessionId);
                if (isFlag)
                {
                    string messageId = "";
                    if (msg is AntSdkChatMsg.Revocation)
                    {
                        var tempChatMsg = (AntSdkChatMsg.Revocation)msg;
                        messageId = tempChatMsg.content?.messageId;
                    }
                    else if (msg is AntSdkChatMsg.DeteleVoteMsg)
                    {
                        var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)msg;
                        messageId = tempChatMsg.messageId;
                    }

                    if (msg.chatType == (int)AntSdkchatType.Point)
                    {
                        if (dicTalkViewModel.ContainsKey(msg.sendUserId))
                        {
                            if (dicTalkViewModel != null && dicTalkViewModel.ContainsKey(msg.sendUserId))
                                (dicTalkViewModel[msg.sendUserId] as TalkViewModel)?.HideMsgAndShowRecallMsg(messageId);
                        }
                        else
                        {
                            if (dicTalkViewModel != null && dicTalkViewModel.Keys.Contains(msg.targetId))
                                (dicTalkViewModel[msg.targetId] as TalkViewModel)?.HideMsgAndShowRecallMsg(messageId);
                        }
                    }
                    else
                    {
                        if (dicTalkViewModel.ContainsKey(msg.sessionId))
                        {
                            (dicTalkViewModel[msg.sessionId] as TalkGroupViewModel)?.HideMsgAndShowRecallMsg(messageId);
                        }
                    }


                }
                else
                {
                    AntSdkTsession tSession = t_sessionBll.GetModelByKey(msg.sessionId);
                    if (tSession == null)
                    {
                        AntSdkTsession newSession = new AntSdkTsession();
                        newSession.SessionId = msg.sessionId;
                        if (msg.chatType == (int)AntSdkchatType.Group)
                            newSession.GroupId = msg.sessionId;
                        else
                        {
                            newSession.UserId = msg.sendUserId == AntSdkService.AntSdkCurrentUserInfo?.userId
                                ? msg.targetId
                                : msg.sendUserId;
                            ;
                        }

                        newSession.LastMsg = msg.sourceContent;
                        newSession.LastMsgTimeStamp = msg.sendTime;
                        newSession.LastChatIndex = msg.chatIndex;
                        t_sessionBll.Insert(newSession);
                        return;
                    }
                    //如果读到的阅后即焚消息不是最后一条消息则无需处理
                    //int chatIndex = -1;
                    //int.TryParse(msg.chatIndex, out chatIndex);
                    //if (chatIndex < int.Parse(tSession.LastChatIndex))
                    //{
                    //    return;
                    //}

                    //ThreadPool.QueueUserWorkItem(m =>
                    //{
                    if (vm != null)
                    {
                        vm.LastChatIndex = msg.chatIndex;
                        vm.LastMessage = msg.sourceContent;
                        vm.LastMsgTimeStamp = msg.sendTime;
                        vm.LastTime = DataConverter.FormatTimeByTimeStamp(vm.LastMsgTimeStamp);
                        vm.MsgType = msg.MsgType;
                    }
                    AntSdkTsession t_session = new AntSdkTsession();
                    t_session.SessionId = msg.sessionId;
                    t_session.LastMsg = msg.sourceContent;
                    t_session.LastMsgTimeStamp = msg.sendTime;
                    t_session.LastChatIndex = msg.chatIndex;
                    if (vm != null)
                        SessionInfoListSort(vm, t_session);
                    else
                    {
                        t_sessionBll.Update(t_session);
                    }

                }

                //}
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[HandleRevocationReceipt]:" + e.Message + "," + e.StackTrace);
            }
        }

        /// <summary>
        /// 阅后即焚发生改变时，刷新会话列表
        /// </summary>
        private void SessionInfoListSort(SessionInfoViewModel vm, AntSdkTsession t_session)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                SessionInfoViewModel sessionInfoModel = null;
                int index = 0;
                //阅后即焚发生改变时，刷新会话列表
                if (vm.TopIndex != null && vm.TopIndex > 0)
                {
                    sessionInfoModel = SessionControlList.LastOrDefault(n => DataConverter.GetTimeByTimeStamp(
                        n.LastMsgTimeStamp) > DataConverter.GetTimeByTimeStamp(vm.LastMsgTimeStamp) && n.TopIndex != null);
                    if (sessionInfoModel?.TopIndex > 1)
                        t_session.TopIndex = sessionInfoModel.TopIndex - 1;
                    t_sessionBll.Update(t_session);
                }
                else
                {
                    var tempSessionCount = SessionControlList.Count(n => DataConverter.GetTimeByTimeStamp(
                        n.LastMsgTimeStamp) > DataConverter.GetTimeByTimeStamp(vm.LastMsgTimeStamp) || n.TopIndex != null);

                    sessionInfoModel = SessionControlList[tempSessionCount];
                }
                if (sessionInfoModel != null)
                {
                    index = SessionControlList.IndexOf(sessionInfoModel);
                    var tempSessionList = SessionControlList.Where(m => m.SessionId == vm.SessionId);
                    var sessionInfoViewModels = tempSessionList as IList<SessionInfoViewModel> ?? tempSessionList.ToList();
                    if (sessionInfoViewModels.Count > 0)
                    {
                        foreach (SessionInfoViewModel sessionInfo in sessionInfoViewModels)
                        {
                            SessionControlList.Remove(sessionInfo);
                        }
                    }
                    SessionControlList.Insert(index, vm);
                }
                else
                {
                    index = SessionControlList.Count(m => m.TopIndex != null && DataConverter.GetTimeByTimeStamp(
                    m.LastMsgTimeStamp) > DataConverter.GetTimeByTimeStamp(vm.LastMsgTimeStamp));
                    var tempSessionList = SessionControlList.Where(m => m.SessionId == vm.SessionId);
                    var sessionInfoViewModels = tempSessionList as IList<SessionInfoViewModel> ?? tempSessionList.ToList();
                    if (sessionInfoViewModels.Count > 0)
                    {
                        foreach (SessionInfoViewModel sessionInfo in sessionInfoViewModels)
                        {
                            SessionControlList.Remove(sessionInfo);
                        }
                    }
                    SessionControlList.Insert(index, vm);
                }
            });
        }
        DispatcherTimer timerPageIsShow = new DispatcherTimer();
        /// <summary>
        /// 讨论组阅后即焚模式切换
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <param name="msgType"></param>
        /// <param name="sessionId"></param>
        public void HandleGroupBurnAfterReadMode(GroupBase sysMsg, AntSdkMsgType msgType, string sessionId)
        {
            #region 处理会话和T_Session表

            int maxIndex = -1;
            SessionInfoViewModel vm = SessionControlList.FirstOrDefault(c => c.SessionId == sessionId);
            AntSdkTsession tSession = t_sessionBll.GetModelByKey(sessionId);
            TalkGroupViewModel talkGroupVM = null;
            var sendTime = AntSdkDataConverter.ConvertDateTimeToIntLong(DateTime.Now).ToString();
            //if (vm != null)
            //{
            if (dicTalkViewModel.ContainsKey(sessionId))
                talkGroupVM = (dicTalkViewModel[sessionId] as TalkGroupViewModel);
            if (msgType == AntSdkMsgType.GroupOwnerBurnMode)
            {
                var groupMsg = (OwnerBurnMode)sysMsg;
                if (groupMsg.content == null)
                    return;
                maxIndex = groupMsg.content.maxIndex;
                if (!BurnSessionList.Contains(sessionId))
                    BurnSessionList.Add(sessionId);
                if (vm?.IsBurnMode == GlobalVariable.BurnFlag.NotIsBurn
                    || string.IsNullOrEmpty(tSession?.BurnLastChatIndex)
                    || !string.IsNullOrEmpty(tSession.BurnLastChatIndex) && int.Parse(tSession.BurnLastChatIndex) < maxIndex
                    || string.IsNullOrEmpty(tSession?.BurnLastMsgTimeStamp))
                {
                    if (vm != null)
                    {
                        vm.ImageBurnVisibility = Visibility.Visible;
                        vm.IsBurnMode = GlobalVariable.BurnFlag.IsBurn;
                        vm.LastMessage = "[无痕]模式开启";
                        vm.LastMsgTimeStamp = sendTime;
                        vm.LastChatIndex = maxIndex.ToString();
                        vm.MsgDatetime = DataConverter.GetTimeByTimeStamp(sendTime);
                        vm.LastTime = DataConverter.FormatTimeByTimeStamp(sendTime);
                        vm.SetUnreadCount(0);
                        //SessionInfoListSort(vm, tSession);
                    }
                    if (tSession != null && (string.IsNullOrEmpty(tSession.BurnLastChatIndex) || int.Parse(tSession.BurnLastChatIndex) < maxIndex))
                    {
                        tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                        tSession.BurnLastMsg = "[无痕]模式开启";
                        tSession.BurnLastMsgTimeStamp = sendTime;
                        tSession.BurnLastChatIndex = maxIndex.ToString();
                        tSession.BurnUnreadCount = 0;
                        if (string.IsNullOrEmpty(tSession.GroupId))
                            tSession.GroupId = sessionId;
                        if (vm?.TopIndex != null && vm.TopIndex > 0)
                            SessionInfoListSort(vm, tSession);
                        else
                        {
                            if (vm != null)
                                SessionInfoListSort(vm, tSession);
                            t_sessionBll.Update(tSession);
                        }


                    }
                }
                if (tSession == null)
                {
                    tSession = new AntSdkTsession();
                    tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                    tSession.BurnLastMsg = "[无痕]模式开启";
                    tSession.BurnLastMsgTimeStamp = sendTime;
                    tSession.BurnLastChatIndex = maxIndex.ToString();
                    tSession.BurnUnreadCount = 0;
                    //if (vm != null)
                    //    SessionInfoListSort(vm, tSession);
                    tSession.SessionId = sessionId;
                    tSession.GroupId = sessionId;
                    t_sessionBll.Insert(tSession);
                }


                //切换即焚切换
                //if (dicTalkViewModel.ContainsKey(sysMsg.sessionId))
                //{
                if (talkGroupVM != null)
                {
                    if (talkGroupVM._richTextBox != null)
                    {
                        talkGroupVM._richTextBox.Document.Blocks.Clear();
                        talkGroupVM._richTextBox.isBurnMode = true;
                    }
                    talkGroupVM.chromiumWebBrowserburn.Visibility = Visibility.Visible;
                    talkGroupVM.chromiumWebBrowser.Visibility = Visibility.Collapsed;
                    talkGroupVM.SwitchBurnMode();

                    var burn = talkGroupVM.chromiumWebBrowserburn;
                    if (burn.Address != null)
                    {
                        if (burn.ActualHeight != 0 || burn.ActualWidth != 0)
                        {
                            timerPageIsShow.Interval = TimeSpan.FromMilliseconds(50);
                            timerPageIsShow.Tick += TimerPageIsShow_Tick;
                            timerPageIsShow.Tag = burn;
                            timerPageIsShow.Start();
                        }
                    }

                    talkGroupVM.LoadMsgData(GlobalVariable.BurnFlag.IsBurn, vm?.IsMouseClick ?? false);
                    talkGroupVM.IsbtnTipShow = "Hidden";
                }
                //}
            }
            else if (msgType == AntSdkMsgType.GroupOwnerBurnDelete)
            {
                var groupMsg = (OwnerBurnDelete)sysMsg;
                if (groupMsg.content == null)
                    return;
                maxIndex = groupMsg.content.maxIndex;
                if (!BurnSessionList.Contains(sessionId))
                    BurnSessionList.Add(sessionId);
                string burnDeleteMsg = "[无痕模式]消息已清空";
                if (vm?.IsBurnMode == GlobalVariable.BurnFlag.NotIsBurn || (!string.IsNullOrEmpty(tSession?.BurnLastChatIndex) &&
                    int.Parse(tSession.BurnLastChatIndex) <= maxIndex))
                {

                    //SessionMonitor.RemoveWaitingToReceiveOfflineMsgRecord(sessionId, GlobalVariable.BurnFlag.IsBurn);
                    //SessionMonitor.GetWaitingToReceiveOnlineMessage(sessionId, (int)GlobalVariable.BurnFlag.IsBurn, AntSdkchatType.Group);
                    if (tSession?.BurnLastMsg != burnDeleteMsg)
                    {
                        if (vm != null)
                        {
                            vm.ImageBurnVisibility = Visibility.Visible;
                            vm.IsBurnMode = GlobalVariable.BurnFlag.IsBurn;
                            vm.LastMessage = burnDeleteMsg;
                            vm.LastMsgTimeStamp = sendTime;
                            vm.MsgDatetime = DataConverter.GetTimeByTimeStamp(sendTime);
                            vm.LastChatIndex = maxIndex.ToString();
                            vm.LastTime = DataConverter.FormatTimeByTimeStamp(sendTime);
                            vm.SetUnreadCount(0);

                        }
                        if (tSession != null &&
                            (string.IsNullOrEmpty(tSession.BurnLastChatIndex) ||
                             int.Parse(tSession.BurnLastChatIndex) <= maxIndex))
                        {
                            tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                            tSession.BurnLastMsg = burnDeleteMsg;
                            tSession.BurnLastMsgTimeStamp = sendTime;
                            tSession.BurnLastChatIndex = maxIndex.ToString();
                            tSession.BurnUnreadCount = 0;
                            if (string.IsNullOrEmpty(tSession.GroupId))
                                tSession.GroupId = sessionId;
                            if (vm?.TopIndex != null && vm.TopIndex > 0)
                                SessionInfoListSort(vm, tSession);
                            else
                            {
                                if (vm != null)
                                    SessionInfoListSort(vm, tSession);
                                t_sessionBll.Update(tSession);
                            }

                        }
                    }
                }
                if (tSession == null)
                {
                    tSession = new AntSdkTsession();
                    tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.IsBurn;
                    tSession.BurnLastMsg = burnDeleteMsg;
                    tSession.BurnLastMsgTimeStamp = sendTime;
                    tSession.BurnLastChatIndex = maxIndex.ToString();
                    tSession.BurnUnreadCount = 0;
                    //if (vm != null)
                    //    SessionInfoListSort(vm, tSession);
                    tSession.SessionId = sessionId;
                    tSession.GroupId = sessionId;
                    t_sessionBll.Insert(tSession);
                }
                //if (dicTalkViewModel.ContainsKey(sysMsg.sessionId))
                //{
                var burn = talkGroupVM?.chromiumWebBrowserburn;
                if (burn?.Address != null)
                {
                    burn.ExecuteScriptAsync(PublicTalkMothed.clearHtml());
                    PublicTalkMothed.addContentTips(burn);
                    talkGroupVM.TextShowReceiveMsg = "";
                    talkGroupVM.TextShowRowHeight = "0";
                }
                //talkGroupVM?.chromiumWebBrowserburn?.ExecuteScriptAsync(PublicTalkMothed.clearHtml());
                //}
            }
            else if (msgType == AntSdkMsgType.GroupOwnerNormal)
            {
                if (BurnSessionList.Contains(sessionId))
                    BurnSessionList.Remove(sessionId);
                maxIndex = 0;
                //SessionMonitor.RemoveWaitingToReceiveOfflineMsgRecord(sessionId, GlobalVariable.BurnFlag.IsBurn);

                if (vm != null)
                {
                    vm.ImageBurnVisibility = Visibility.Collapsed;
                    vm.IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
                    if (tSession != null)
                    {
                        //    tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.NotIsBurn;
                        //    tSession.BurnLastMsg = string.Empty;
                        //    tSession.BurnLastMsgTimeStamp = string.Empty;
                        //    tSession.BurnLastChatIndex = String.Empty;
                        //    tSession.BurnUnreadCount = 0;
                        if (vm.IsMouseClick)
                            vm.SetUnreadCount(0);
                        else
                            vm.SetUnreadCount(tSession.UnreadCount);
                        vm.LastMessage = tSession.LastMsg;
                        vm.MsgDatetime = DataConverter.GetTimeByTimeStamp(sendTime);
                        vm.LastMsgTimeStamp = tSession.LastMsgTimeStamp;
                        vm.LastChatIndex = tSession.LastChatIndex;
                        vm.LastTime = DataConverter.FormatTimeByTimeStamp(tSession.LastMsgTimeStamp);

                        SessionInfoListSort(vm, tSession);
                    }
                    //t_sessionBll.Update(tSession); 
                }
                else
                {
                    var session = tSession;
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        var count = SessionMonitor.MessageCount(sessionId, "", (int)GlobalVariable.BurnFlag.NotIsBurn);
                        if (count > 0)
                        {
                            var grouplist = SessionMonitor.GroupListViewModel?.GroupInfos;
                            var groupInfo = grouplist != null && grouplist.Any()
                                ? grouplist.FirstOrDefault(
                                    c => c.groupId == sessionId)
                                : null;
                            if (groupInfo == null && session != null)
                            {
                                t_sessionBll.Delete(session);
                                return;
                            }
                            SessionInfoModel model = new SessionInfoModel();
                            model.SessionId = sessionId;
                            model.name = groupInfo?.groupName;
                            model.photo = groupInfo?.groupPicture;
                            model.topIndex = session?.TopIndex;
                            model.lastMessage = session?.LastMsg;
                            model.lastChatIndex = session?.LastChatIndex;
                            model.lastTime = session?.LastMsgTimeStamp;

                            model.unreadCount = SessionMonitor.MessageCount(model.SessionId, model.lastChatIndex, 0);
                            var groupInfoLst = SessionMonitor.GroupListViewModel?.GroupInfoList;
                            var groupInfoVM = groupInfoLst != null && groupInfoLst.Count > 0
                                ? groupInfoLst.FirstOrDefault(
                                    m => m.GroupInfo != null && m.GroupInfo.groupId == groupInfo?.groupId)
                                : null;
                            var groupMembers = groupInfoVM?.Members != null && groupInfoVM.Members.Any()
                                ? groupInfoVM.Members
                                : new List<AntSdkGroupMember>();
                            var tempVM = new SessionInfoViewModel(groupInfo, BurnFlag.NotIsBurn, groupMembers, model);
                            tempVM.MouseLeftButtonDownEvent += SessionViewMouseLeftButtonDown;
                            tempVM.DeleteSessionEvent += DeleteSession;
                            tempVM.PostTopSessionEvent += PostTopSession;
                            tempVM.CancelTopSessionEvent += CacelTopSession;
                            AddSessionControl(tempVM.LastMsgTimeStamp, tempVM);
                        }
                    }));
                    //dispatcher.Invoke(new Action(() =>
                    //{
                }
                if (tSession != null)
                {
                    tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.NotIsBurn;
                    tSession.BurnLastMsg = string.Empty;
                    tSession.BurnLastMsgTimeStamp = string.Empty;
                    tSession.BurnLastChatIndex = String.Empty;
                    tSession.BurnUnreadCount = 0;
                    t_sessionBll.Update(tSession);
                }
                else
                {
                    tSession = new AntSdkTsession();
                    tSession.IsBurnMode = (int)GlobalVariable.BurnFlag.NotIsBurn;
                    tSession.BurnLastMsg = string.Empty;
                    tSession.BurnLastMsgTimeStamp = string.Empty;
                    tSession.BurnLastChatIndex = String.Empty;
                    tSession.BurnUnreadCount = 0;
                    tSession.SessionId = sessionId;
                    tSession.GroupId = sessionId;
                    t_sessionBll.Insert(tSession);
                }
                //切换正常模式
                //if (dicTalkViewModel.ContainsKey(sysMsg.sessionId))
                //{
                if (talkGroupVM != null)
                {
                    if (talkGroupVM._richTextBox != null)
                    {
                        talkGroupVM._richTextBox.Document.Blocks.Clear();
                        talkGroupVM._richTextBox.isBurnMode = false;
                    }
                    talkGroupVM.SwitchNotBurnMode();
                    if (talkGroupVM.chromiumWebBrowserburn != null)
                    {
                        if (talkGroupVM.chromiumWebBrowserburn.WebBrowser != null)
                        {
                            var burn = talkGroupVM.chromiumWebBrowserburn;
                            if (burn.IsBrowserInitialized != false && burn.Visibility == Visibility.Visible)
                            {
                                burn.ExecuteScriptAsync(PublicTalkMothed.clearHtml());
                                //PublicTalkMothed.addContentTips(burn);
                            }
                        }
                        talkGroupVM.chromiumWebBrowser.Visibility = Visibility.Visible;
                        if (vm != null && vm.IsMouseClick)
                        {

                            talkGroupVM.LoadMsgData(GlobalVariable.BurnFlag.NotIsBurn, (bool)vm?.IsMouseClick);
                            if (talkGroupVM.chromiumWebBrowser.IsBrowserInitialized)
                                talkGroupVM.ShowMsgData();
                        }
                        //if (talkGroupVM.chromiumWebBrowserburn != null)
                        //{
                        //    talkGroupVM.chromiumWebBrowserburn.Visibility = Visibility.Collapsed;
                        //    talkGroupVM.chromiumWebBrowserburn.Dispose();
                        //}
                    }
                }
            }


            //}

            //}
            #endregion
            #region 处理消息免打扰 (已注释)
            //T_NoRemindGroup tNoRemindGroup = t_NoRemindGroupBll.GetModelByKey(sysMsg.groupId);
            //if (tNoRemindGroup != null)
            //{
            //    if (int.Parse(sysMsg.type) == (int)GlobalVariable.SysUserMsgType.GroupBurnAfterReadMode
            //        || int.Parse(sysMsg.type) == (int)GlobalVariable.SysUserMsgType.DeleteMsgBurnMode)
            //    {
            //        if (!string.IsNullOrEmpty(tNoRemindGroup.BurnLastChatIndex) && int.Parse(tNoRemindGroup.BurnLastChatIndex) < int.Parse(sysMsg.maxIndex))
            //        {
            //            tNoRemindGroup.BurnLastChatIndex = string.Empty;
            //            tNoRemindGroup.BurnLastMsg = string.Empty;
            //            tNoRemindGroup.BurnLastMsgTimeStamp = string.Empty;
            //            tNoRemindGroup.BurnUnreadCount = 0;
            //        }
            //        if (tNoRemindGroup.BurnUnreadCount + tNoRemindGroup.UnreadCount > 0)
            //        {
            //            t_NoRemindGroupBll.Update(tNoRemindGroup);
            //        }
            //        else
            //        {
            //            t_NoRemindGroupBll.Delete(tNoRemindGroup);
            //        }
            //    }
            //    else if (int.Parse(sysMsg.type) == (int)GlobalVariable.SysUserMsgType.GroupNoBurnAfterReadMode)
            //    {
            //        tNoRemindGroup.BurnLastChatIndex = string.Empty;
            //        tNoRemindGroup.BurnLastMsg = string.Empty;
            //        tNoRemindGroup.BurnLastMsgTimeStamp = string.Empty;
            //        tNoRemindGroup.BurnUnreadCount = 0;
            //        if (tNoRemindGroup.BurnUnreadCount + tNoRemindGroup.UnreadCount > 0)
            //        {
            //            t_NoRemindGroupBll.Update(tNoRemindGroup);
            //        }
            //        else
            //        {
            //            t_NoRemindGroupBll.Delete(tNoRemindGroup);
            //        }
            //    }
            //}
            #endregion

            #region 处理501、502、503删除数据问题

            if (maxIndex >= 0)
            {
                T_Chat_Message_GroupBurnDAL tBurnDal = new T_Chat_Message_GroupBurnDAL();
                switch (msgType)
                {
                    case AntSdkMsgType.GroupOwnerBurnMode:
                    case AntSdkMsgType.GroupOwnerBurnDelete:
                        //删除阅后即焚对应会话内容
                        var flag = tBurnDal.DeleteBurnDataByChatIndex(sessionId, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, maxIndex.ToString());
                        if (flag <= 0) return;
                        MessageMonitor.GetOfflineMessageStatisticList(sessionId, GlobalVariable.BurnFlag.IsBurn, maxIndex.ToString());
                        SessionMonitor.GetWaitingToReceiveOnlineMessage(sessionId, (int)GlobalVariable.BurnFlag.IsBurn, AntSdkchatType.Group);
                        break;
                    case AntSdkMsgType.GroupOwnerNormal:
                        //删除阅后即焚对应会话内容
                        var flagDeleteBurn = tBurnDal.DeleteBurnData(sessionId, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                        if (flagDeleteBurn <= 0) return;
                        MessageMonitor.GetOfflineMessageStatisticList(sessionId, GlobalVariable.BurnFlag.IsBurn);
                        SessionMonitor.GetWaitingToReceiveOnlineMessage(sessionId, (int)GlobalVariable.BurnFlag.IsBurn, AntSdkchatType.Group);
                        break;
                }

            }

            #endregion
        }
        /// <summary>
        /// 监控页面是否显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerPageIsShow_Tick(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            var burn = timer.Tag as ChromiumWebBrowsers;
            if (burn.IsBrowserInitialized == true)
            {
                Task<JavascriptResponse> task = burn.EvaluateScriptAsync(PublicTalkMothed.add());
                task.Wait();
                if (task.Result.Success)
                {
                    timerPageIsShow.Stop();
                    burn.ExecuteScriptAsync(PublicTalkMothed.clearHtml());
                    PublicTalkMothed.addContentTips(burn);
                }
            }
        }

        /// <summary>
        /// 服务断开或重连之后的状态改变
        /// </summary>
        /// <param name="isConnected">是否连接成功</param>
        public void ChangeSessionListUserState(bool isConnected)
        {
            if (!isConnected)
            {
                //如果连接断开Session列表所有在线用户改为离线状态
                var onLineSessionList = SessionControlList.Where(m => !m.IsOfflineState && m.isPointOrGroup);
                foreach (var onLineSession in onLineSessionList)
                {
                    onLineSession.IsOfflineState = true;
                    onLineSession.UserOnlineStateIcon = "";
                }
                //群成员在线改为离线状态
                foreach (var item in dicTalkViewModel.Values)
                {
                    if (!(item is TalkGroupViewModel)) continue;
                    var talkGroupViewModel = item as TalkGroupViewModel;
                    var groupMemberList = talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList.Where(
                            m => !m.IsOfflineState);
                    if (groupMemberList == null) continue;
                    foreach (var groupMemberVm in groupMemberList)
                    {
                        groupMemberVm.IsOfflineState = true;
                        groupMemberVm.UserOnlineStateIcon = "";
                    }
                }

            }
            else
            {
                //如果连接重连成功Session列表所有个人恢复状态
                var onLineSessionList = SessionControlList.Where(m => m.isPointOrGroup);
                foreach (var onLineSession in onLineSessionList)
                {
                    var userInfo =
                        AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                            m => m.userId == onLineSession.AntSdkContact_User.userId && m.state != (int)GlobalVariable.OnLineStatus.OffLine);
                    if (userInfo == null) continue;
                    onLineSession.IsOfflineState = userInfo.state == (int)GlobalVariable.OnLineStatus.OffLine;
                    if (UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(userInfo.state))
                    {
                        onLineSession.UserOnlineStateIcon = UserOnlineSataeInfo.UserOnlineStateIconDic[userInfo.state];
                    }
                }
                //群成员恢复状态
                foreach (var item in dicTalkViewModel.Values)
                {
                    if (!(item is TalkGroupViewModel)) continue;
                    var talkGroupViewModel = item as TalkGroupViewModel;
                    var groupMemberList = talkGroupViewModel.GroupMemberListViewModel?.GroupMemberControlList;
                    if (groupMemberList == null) continue;
                    foreach (var groupMemberVm in groupMemberList)
                    {
                        var userInfo =
                         AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                             m => m.userId == groupMemberVm.Member.userId && m.state != (int)GlobalVariable.OnLineStatus.OffLine);
                        if (userInfo == null) continue;
                        groupMemberVm.IsOfflineState = userInfo.state == (int)GlobalVariable.OnLineStatus.OffLine;
                        if (UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(userInfo.state))
                        {
                            groupMemberVm.UserOnlineStateIcon = UserOnlineSataeInfo.UserOnlineStateIconDic[userInfo.state];
                        }
                    }
                }
            }
        }

        private void TalkGroupViewModel_updateFailMessageEventHandler(object sender, EventArgs e)
        {
            AntSdkFailOrSucessMessageDto failMessage = sender as AntSdkFailOrSucessMessageDto;
            if (failMessage != null)
            {
                RefreshFailMessage(failMessage, "");
            }
        }
        private void TalkViewModel_updateFailMessageEventHandler(AntSdkFailOrSucessMessageDto failMessage, string targetId)
        {
            if (failMessage != null)
            {
                RefreshFailMessage(failMessage, targetId);
            }
        }
        /// <summary>
        /// 删除重复session
        /// </summary>
        public void RemoveRepeatSession()
        {
            if (SessionControlList.Count > 0)
            {
                //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                //{
                var resultSession = from s in SessionControlList
                                    group s by s.SessionId into g
                                    where g.Count() > 1
                                    select g;
                var resultSessionList = resultSession as IList<IGrouping<string, SessionInfoViewModel>> ??
                                        resultSession.ToList();
                if (!resultSessionList.Any()) return;
                {
                    //var tempSessionList = SessionControlList.GroupBy(m => m).Select(m => m.Key.SessionId).ToList();
                    LogHelper.WriteDebug("[SessionList]:-----------------消息列表有重复数据列表个数：" + resultSessionList.Count);
                    //foreach (var info in resultSessionList)
                    //{
                    //    var sessionInfoList = SessionControlList?.OrderBy(m => m.LastMsgTimeStamp);
                    //    var sessionInfo= sessionInfoList.FirstOrDefault(m => m.SessionId == info.Key);
                    //    if (sessionInfo != null)
                    //    {
                    //        SessionControlList.Remove(sessionInfo);
                    //        var tempSessionInfo = sessionInfoList.LastOrDefault(m => m.SessionId == info.Key);
                    //        if (tempSessionInfo != null && !tempSessionInfoViewModels.Contains(tempSessionInfo))
                    //            tempSessionInfoViewModels.Add(tempSessionInfo);
                    //    }
                    //}
                    //var tempSessionList = SessionControlList.GroupBy(m => m).Select(m => m.Key.SessionId).ToList();
                    // LogHelper.WriteDebug("[SessionList]:-----------------消息列表重复数据删除后列表个数：" + SessionControlList.Count);
                    //for (int i = 0; i < tempSessionList.Count; i++)
                    //{
                    //    var sessionId = tempSessionList[i];
                    //    var sessionInfoList = SessionControlList.Where(m => m.SessionId == sessionId);
                    //    if (sessionInfoList.Count() > 1)
                    //    {
                    //        var sessionInfo = SessionControlList.FirstOrDefault(m => m.SessionId == sessionId);
                    //        if (sessionInfo != null)
                    //            SessionControlList.Remove(sessionInfo);
                    //        continue;
                    //    }
                    //}
                }
                //foreach (var tempSessionInfo in tempSessionInfoViewModels)
                //{
                //    var sessionInfo = SessionControlList?.FirstOrDefault(m => m.SessionId == tempSessionInfo.SessionId);
                //    if (sessionInfo == null)
                //        AddSessionControl(tempSessionInfo.LastMsgTimeStamp, tempSessionInfo);
                //}


                //}, DispatcherPriority.Background);

            }
        }

        /// <summary>
        /// 发送进行中、失败、阅后即焚更新
        /// </summary>
        /// <param name="failMessage"></param>
        public void RefreshFailMessage(AntSdkFailOrSucessMessageDto failMessage, string targetId)
        {
            try
            {
                SessionInfoViewModel sessionInfo =
                    _SessionControlList.SingleOrDefault(m => m.SessionId == failMessage.sessionid);
                if (sessionInfo == null)
                {
                    return;
                }
                sessionInfo.TargetId = targetId;
                switch (failMessage.IsSendSucessOrFail)
                {
                    case AntSdkburnMsg.isSendSucessOrFail.sending:
                        sessionInfo.ImageSendingVisibility = Visibility.Visible;
                        break;
                    case AntSdkburnMsg.isSendSucessOrFail.sucess:
                        break;
                    case AntSdkburnMsg.isSendSucessOrFail.fail:
                        break;
                }
                #region 2017-08-19 重发机制修改
                //sessionInfo.ImageFailingVisibility = Visibility.Collapsed;
                //sessionInfo.ImageSendingVisibility = Visibility.Visible;
                //if (failMessage.IsSendSucessOrFail == AntSdkburnMsg.isSendSucessOrFail.fail)
                //{
                //    sessionInfo.ImageSendingVisibility = Visibility.Collapsed;
                //    sessionInfo.ImageFailingVisibility = Visibility.Visible;
                //}
                //else
                //{
                //    sessionInfo.ImageSendingVisibility = Visibility.Visible;
                //}
                #endregion
                if (failMessage.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                {
                    sessionInfo.LastMessage = "无痕消息";
                    if (!failMessage.sessionid.StartsWith("G"))
                        sessionInfo.LastMessage = "[阅后即焚消息]";
                }
                else
                {
                    switch (failMessage.mtp)
                    {
                        case (int)GlobalVariable.MsgType.Text:
                        case (int)AntSdkMsgType.ChatMsgText:
                            sessionInfo.LastMessage = failMessage.content;
                            break;
                        case (int)GlobalVariable.MsgType.Picture:
                        case (int)AntSdkMsgType.ChatMsgPicture:
                            sessionInfo.LastMessage = "[图片]";
                            break;
                        case (int)GlobalVariable.MsgType.File:
                        case (int)AntSdkMsgType.ChatMsgFile:
                            sessionInfo.LastMessage = "[文件]";
                            break;
                        case (int)GlobalVariable.MsgType.Voice:
                        case (int)AntSdkMsgType.ChatMsgAudio:
                            sessionInfo.LastMessage = "[语音]";
                            break;

                        case (int)AntSdkMsgType.ChatMsgMixMessage:
                            sessionInfo.LastMessage = failMessage.content;
                            break;
                    }
                }
                sessionInfo.LastTime =
                    DataConverter.FormatTimeByTimeStamp(
                        PublicTalkMothed.ConvertDateTimeToIntLong(Convert.ToDateTime(failMessage.lastDatetime))
                            .ToString());
                sessionInfo.MsgDatetime = Convert.ToDateTime(failMessage.lastDatetime);
            }
            catch (Exception ex)
            {

            }
            //TalkViewModel.updateFailMessageEventHandler -= TalkViewModel_updateFailMessageEventHandler;
        }
        #endregion
    }
}
