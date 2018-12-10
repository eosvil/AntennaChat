using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Diagnostics;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Practices.Prism.Commands;
using CSharpWin_JD.CaptureImage;
using System.IO;
using static Antenna.Model.SendMessageDto;
using Antenna.Model;
using CefSharp.Wpf;
using Antenna.Framework;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.ComponentModel;
using AntennaChat.ViewModel.FileUpload;
using AntennaChat.Views.FileUpload;
using AntennaChat.Command;
using AntennaChat.ViewModel.Contacts;
using System.Configuration;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AntennaChat.Views.Contacts;
using AntennaChat.Views.Talk;
using CefSharp;
using System.Threading.Tasks;
using System.Xaml;
using System.Xml;
using AntennaChat.Views;
using Clipboard = System.Windows.Clipboard;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;
using AntennaChat.Helper;
using AntennaChat.Helper.IHelper;
using AntennaChat.Resource;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Application;
using Antenna.Model.OnceSendMessage;
using Antenna.Model.PictureAndTextMix;
using AntennaChat.CefSealedHelper.OneAndGroupCommon;
using System.Net.Http;
using System.Net.Http.Headers;
using AntennaChat.CefSealedHelper.OneToOne.PictureAndTextMix;
using static Antenna.Model.ModelEnum;
using AntennaChat.ViewModel.AudioVideo;
using AntennaChat.Views.AudioVideo;
using AntennaChat.CefSealedHelper;
using static AntennaChat.ViewModel.Talk.TalkViewModel.CallbackValueById;
namespace AntennaChat.ViewModel.Talk
{
    public class TalkViewModel : PropertyNotifyObject
    {
        /// <summary>
        /// 第一屏数据
        /// </summary>
        IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> FirstPageData = null;
        /// <summary>
        /// 消息监控集合 
        /// </summary>
       // public List<SendMsgStateMonitor> MessageStateMonitorList = new List<SendMsgStateMonitor>();
        public GlobalVariable.IsRun IsRun = GlobalVariable.IsRun.NotIsRun;
        public List<AddImgUrlDto> listDictImgUrls = new List<AddImgUrlDto>();
        public List<string> listChatIndex = new List<string>();
        //private Dictionary<string, string> MsgIdAndImgSendingId = new Dictionary<string, string>();
        public delegate void SentBurnAfterReadDelegate(AntSdkChatMsg.ChatBase receipt, AntSdkMsgType smType, bool isFlag = true);
        public static event SentBurnAfterReadDelegate SentBurnAfterReadEvent;
        private MsgEditAssistant msgEditAssistant;
        //public static Action<>
        System.Windows.Controls.ContextMenu cms;
        SendMessage_ctt s_ctt;
        SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr = null;
        public AntSdkContact_User user { get; set; }
        int unreadCount = 0;
        string lastShowTime = "";
        string preTime = "";
        string firstTime = "";
        private string imageSuffix = ".png";
        string userIMImageSavePath = string.Empty;
        string format = "EditMsgList";
        private List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantMsgList = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
        private DateTime lastReceivedMsgTime = DateTime.MinValue;
        private DateTime autoReplyMsgTime = DateTime.MinValue;
        private bool IsRobot = false;
        private bool _isChanageWindow = false;
        private const int pageSize = 10;//每一页显示的消息条数
        private List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> OnlineReceiveMessageList = null;
        /// <summary>
        /// 类内窗体控制器的实例
        /// </summary>
        protected IWindowHelper _windowHelper;
        private List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> ChatMsgLst = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
        public TalkViewModel(SendMessage_ctt ctt, AntSdkContact_User user, int unreadCount)
        {
            LogHelper.WriteDebug("TalkViewModel:" + "时间:" + DateTime.Now + "SendMessage_ctt:" + JsonConvert.SerializeObject(ctt) + "AntSdkContact_User:" + JsonConvert.SerializeObject(user) + "unreadCount:" + unreadCount);
            try
            {
                _richTextBox.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                _richTextBox.IsReadOnly = true;
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.userNum))
                    {
                        UserName = user.userName;
                    }
                    else
                    {
                        UserName = user.userNum + user.userName;
                    }
                    if (user.status == 0 && user.state == 0)
                        UserName = UserName + "（停用）";
                    if (string.IsNullOrEmpty(user.picture))
                    {
                        Picture = "pack://application:,,,/AntennaChat;Component/Images/36-头像.png";
                    }
                    else
                    {
                        Picture = user.picture;
                    }
                    this.s_ctt = ctt;
                    this.user = user;
                    this.unreadCount = unreadCount;
                    userIMImageSavePath = publicMethod.localDataPath() + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\personal\\cutImg\\";

                    System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();

                    System.Windows.Controls.MenuItem miCut = new System.Windows.Controls.MenuItem() { Header = "剪切" };
                    cm.Items.Add(miCut);
                    miCut.Click += MiCut_Click;
                    System.Windows.Controls.MenuItem miCopy = new System.Windows.Controls.MenuItem() { Header = "复制" };
                    cm.Items.Add(miCopy);
                    miCopy.Click += MiCopy_Click;
                    System.Windows.Controls.MenuItem miPaste = new System.Windows.Controls.MenuItem() { Header = "粘贴" };
                    cm.Items.Add(miPaste);
                    miPaste.Click += MiPaste_Click;
                    _richTextBox.ContextMenu = cm;
                    _richTextBox.AllowDrop = true;
                    _richTextBox.PreviewDragOver += richTextBox_PreviewDragOver;
                    _richTextBox.LostFocus += _richTextBox_LostFocus;
                    _richTextBox.GotFocus += _richTextBox_GotFocus;
                    msgEditAssistant = new MsgEditAssistant(_richTextBox, userIMImageSavePath);
                    if (publicMethod.isBurnMode(s_ctt.sessionId))
                    {
                        LogHelper.WriteDebug("TalkViewModel:阅后即焚模式");
                        BurnMode();
                    }
                    else
                    {
                        LogHelper.WriteDebug("TalkViewModel:非阅后即焚模式");
                        NotBurnMode();
                    }
                    InitTalkChromiumWebBrowser();

                    //对方是不是机器人
                    if (AntSdkService.AntSdkCurrentUserInfo.robotId == user.userId)
                    {
                        isShowBurn = "Collapsed";
                        IsRobot = true;
                        isShowAudio = Visibility.Collapsed;
                    }
                    timer.Interval = TimeSpan.FromMilliseconds(50);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_TalkViewModel]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 消息展示框初始化
        /// </summary>
        public void InitTalkChromiumWebBrowser()
        {
            //IsRun = GlobalVariable.IsRun.NotIsRun;
            //timer?.Stop();
            #region 注册
            //chromiumWebBrowser = new ChromiumWebBrowsers();
            if (PublicTalkMothed.isOneToOneRegester == false)
            {
                CallbackValueById.upLoadImageMsgOnce += CallbackValueById_upLoadImageMsgOnce;
                CallbackValueById.upLoadFileMsgOnce += CallbackValueById_upLoadFileMsgOnce;
                CallbackValueById.sendTextMsgOnce += CallbackValueById_sendTextMsgOnce;
                CallbackValueById.upLoadVoiceMsgOnce += CallbackValueById_upLoadVoiceMsgOnce;
                MainWindowViewModel.CancelEventHandler += MainWindowViewModel_CancelEventHandler;
                callbackOnmousewheelBack.AddImgUrlEventHandler += CallbackOnmousewheelBack_AddImgUrlEventHandler;
                callbackOnmousewheelBack.UpdateReadImgEventHandler += CallbackOnmousewheelBack_UpdateReadImgEventHandler;
                callbackOnmousewheelBack.EffictiveImgUrlEventHandler += CallbackOnmousewheelBack_EffictiveImgUrlEventHandler;
                sendMixPicAndTextOnce += CallbackValueById_sendMixPicAndTextOnce;

                PublicTalkMothed.isOneToOneRegester = true;
            }
            callbackId.ShowUserInfoEvent += CallbackId_ShowUserInfoEvent;
            _chromiumWebBrowser.sessionid = s_ctt.sessionId;
            this._chromiumWebBrowser.s_ctt = s_ctt;
            this._chromiumWebBrowser.userId = this.user.userId;
            this._chromiumWebBrowser.RegisterJsObject("callbackUserId", new callbackId(this.chromiumWebBrowser));
            this._chromiumWebBrowser.RegisterJsObject("callbackObj", new CallbackObjectForJs(listDictImgUrls, this.chromiumWebBrowser, s_ctt, _isShowBurn, this));
            this._chromiumWebBrowser.RegisterJsObject("callbackUrl", new CallbackObjectUrlJs(this._chromiumWebBrowser, "", s_ctt));
            this._chromiumWebBrowser.RegisterJsObject("callbackFilePath", new CallbackObjectFilePathJs(this._chromiumWebBrowser, s_ctt));
            this._chromiumWebBrowser.RegisterJsObject("callbackValue", new CallbackValueById(this._chromiumWebBrowser, s_ctt, SendMsgListPointMonitor.MessageStateMonitorList));
            this._chromiumWebBrowser.RegisterJsObject("callbackOpenDirectory", new CallBackSelectDirecoryFile(this._chromiumWebBrowser));

            #endregion
            //取消
            _chromiumWebBrowser.MenuHandler = new MenuHandler();
            cms = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem miCopys = new System.Windows.Controls.MenuItem() { Header = "复制" };
            miCopys.Click += MiCopys_Click;
            cms.Items.Add(miCopys);
            _chromiumWebBrowser.ContextMenu = cms;

            _chromiumWebBrowser.MouseLeftButtonDown += _chromiumWebBrowser_MouseLeftButtonDown;
            _chromiumWebBrowser.MouseRightButtonDown += _chromiumWebBrowser_MouseRightButtonDown;
            _richTextBox.PreviewKeyDown += _richTextBox_PreviewKeyDown;
            _chromiumWebBrowser.AllowDrop = true;
            _chromiumWebBrowser.PreviewDrop += _chromiumWebBrowser_PreviewDrop;
            #region 2017-02-27 添加
            //如果unreadCount为0则查询该会话的最大的chatindex 如果大于0 则查询unreadCount + 1条消息的chatindex
            //  unreadCount为0情况 查询语句为：select max(chatindex)from t_chat_message;
            //unreadCount不为0情况 查询语句为：select chatindex from t_chat_message order by chatindex desc limit 10,1;
            //if (unreadCount == 0)
            //{
            //    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            //    string currentChatIndex = "0";
            //    var chatIndex = t_chat.getQueryZeroChatIndex(s_ctt.sessionId, s_ctt.companyCode,
            //        AntSdkService.AntSdkCurrentUserInfo.userId);
            //    _chromiumWebBrowser.scrollChatIndex = !string.IsNullOrEmpty(chatIndex) ? chatIndex : currentChatIndex;
            //    FirstPageData = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, 0, 10));
            //}
            //else
            //{
            //    string currentChatIndex = "0";
            //    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            //    var chatIndex = t_chat.getQueryNotZeroChatIndex(s_ctt.sessionId, s_ctt.companyCode,
            //        AntSdkService.AntSdkCurrentUserInfo.userId, unreadCount.ToString());
            //    _chromiumWebBrowser.scrollChatIndex = !string.IsNullOrEmpty(chatIndex) ? chatIndex : currentChatIndex;
            //}
            #endregion
            #region 2017-12-5 消息漫游
            //第一次加载，判断是否有网络，如果有，从服务器拉取最新的pageSize条消息；如果没有，提示网络异常，并从本地数据库拉取最新pageSize条消息
            if (!AntSdkService.AntSdkIsConnected)
            {
                //网络异常，从本地数据库拉取最新pageSize条消息
                FirstPageData = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Point, true, pageSize, 0);
            }
            else
            {
                //从网络拉取pageSize条消息，如果请求失败，从本地数据库拉取最新pageSize条消息，如果请求成功，则在页面显示pageSize条消息
                FirstPageData = PublicMessageFunction.QueryMessageFromServer(s_ctt.sessionId, AntSdkchatType.Point, true, pageSize, 0);
                if (FirstPageData == null)
                {
                    FirstPageData = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Point, true, pageSize, 0);
                }
            }
            if (FirstPageData == null || (FirstPageData != null && FirstPageData.Count == 0))
                _chromiumWebBrowser.scrollChatIndex = "0";
            else
            {
                var index = FirstPageData[0].chatIndex;
                _chromiumWebBrowser.scrollChatIndex = !string.IsNullOrEmpty(index) ? index : "0";
            }
            #endregion
            //2017-02-22 注册事件 滚轮事件
            this._chromiumWebBrowser.RegisterJsObject("callbackOnmousewheel", new callbackOnmousewheelBack(this._chromiumWebBrowser, user, s_ctt, listChatIndex, this, FirstPageData));
        }
        private void CallbackValueById_sendMixPicAndTextOnce(object sender, EventArgs e)
        {
            var send = sender as MixPicAndSenderArgs;
            if (send != null)
            {
                sendMixPicAndText(send.type, send.currentChat, send.arg, send.mixMsg, send.obj);
                //插入图片列表
                foreach (var list in send.mixMsg.TagDto)
                {
                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, list.PreGuid, list.Path, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.notRead, burnMsg.IsEffective.NotEffective);
                }
            }
        }

        public void CallbackObjectForJs_ReCallMsgEventHandler(object sender, EventArgs e)
        {
            string messageId = sender as string;
            if (!string.IsNullOrEmpty(messageId))
            {
                //ThreadPool.QueueUserWorkItem(m => HandlerRecallMsg(messageId));
                HandlerRecallMsg(messageId);
            }
        }

        private void HandlerRecallMsg(string messageId)
        {
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            if (!string.IsNullOrEmpty(messageId))
            {
                //获取服务器时间
                var errorCode = 0;
                string errorMsg = "";
                AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errorCode, ref errorMsg);
                if (serverResult == null)
                {
                    return;
                }
                string serverStamp = serverResult.systemCurrentTime;
                if (!string.IsNullOrEmpty(serverStamp))
                {
                    var recalMsg = AntSdkSqliteHelper.ModelConvertHelper<ChatInfo>.ConvertToModel(t_chat.getRecallData(messageId));
                    if (!recalMsg.Any())
                    {
                        return;
                    }
                    string localTime = "";
                    if (recalMsg[0].sendsucessorfail == "1")
                    {
                        localTime = recalMsg[0].sendtime;
                        DateTime dtServer = PublicTalkMothed.ConvertStringToDateTime(serverStamp);
                        DateTime dtLocal = PublicTalkMothed.ConvertStringToDateTime(localTime);
                        double differ = dtServer.Subtract(dtLocal).TotalMinutes;
                        if (differ > 2)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
            //1、先发送
            //2、发送成功删除该消息
            //3、发送成功插入撤回消息
            string errMsg = "";
            AntSdkChatMsg.Revocation ReCall = new AntSdkChatMsg.Revocation();
            string newMessageId = PublicTalkMothed.timeStampAndRandom();
            ReCall.messageId = newMessageId;
            AntSdkChatMsg.Revocation_content ReContent = new AntSdkChatMsg.Revocation_content();
            ReCall.chatType = (int)AntSdkchatType.Point;
            ReCall.MsgType = AntSdkMsgType.Revocation;
            ReCall.os = (int)GlobalVariable.OSType.PC;
            ReContent.messageId = messageId;
            ReCall.content = ReContent;
            ReCall.sendUserId = this.s_ctt.sendUserId;
            ReCall.targetId = this.s_ctt.targetId;
            ReCall.sessionId = this.s_ctt.sessionId;
            int maxChatIndex = t_chat.GetPreOneChatIndex(this.s_ctt.sessionId, AntSdkService.AntSdkCurrentUserInfo.userId);
            //插入消息
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase tempChatMsg = new AntSdkChatMsg.ChatBase();
            tempChatMsg.MsgType = AntSdkMsgType.Revocation;
            tempChatMsg.messageId = newMessageId;
            tempChatMsg.SENDORRECEIVE = "1";
            tempChatMsg.chatType = (int)AntSdkchatType.Point;
            tempChatMsg.flag = _isShowBurn == "Collapsed" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId ? 1 : 0;
            tempChatMsg.os = (int)(int)GlobalVariable.OSType.PC;
            tempChatMsg.sendUserId = this.s_ctt.sendUserId;
            tempChatMsg.chatIndex = maxChatIndex.ToString();
            tempChatMsg.targetId = this.s_ctt.targetId;
            tempChatMsg.sessionId = this.s_ctt.sessionId;
            tempChatMsg.sourceContent = "你" + GlobalVariable.RevocationPrompt.Msessage;
            bool result = ThreadPool.QueueUserWorkItem(m => addData(tempChatMsg));
            if (result)
            {
                var sendMsg = AntSdkService.SdkPublishChatMsg(ReCall, ref errMsg);
                if (sendMsg)
                {
                    //删除本地消息
                    t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, messageId);
                    //隐藏界面显示
                    PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, messageId);
                    //插入界面一条消息 "我撤销了一条消息"
                    this.chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.InsertUIRecallMsg("你" + GlobalVariable.RevocationPrompt.Msessage));
                    //更改发送状态
                    t_chat.UpdateSendMsgState(newMessageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                    tempChatMsg.sendTime = AntSdkDataConverter.ConvertDateTimeToIntLong(DateTime.Now).ToString();
                    SentBurnAfterReadEvent?.Invoke(tempChatMsg, AntSdkMsgType.Revocation, false);
                }
                else
                {
                    t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, newMessageId);
                }
                //AntSdkChatMsg.ChatBase receipt = new AntSdkChatMsg.ChatBase();
                //receipt.sessionId = this.s_ctt.sessionId;
                //receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                //receipt.chatIndex = maxChatIndex.ToString();
                //receipt.targetId = this.user.userId;
                //receipt.messageId = messageId;
                //receipt.sourceContent=

            }
        }
        public void EndEvent()
        {
            OnceSendMessage.OneToOne.CefList.Remove(s_ctt.sessionId);
            if (_windowHelper != null)
            {
                _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived -= LocalMessageHelper_InstantMessageHasBeenReceived;
            }
            callbackOnmousewheelBack.AddImgUrlEventHandler -= CallbackOnmousewheelBack_AddImgUrlEventHandler;
            CallbackValueById.upLoadImageMsgOnce -= CallbackValueById_upLoadImageMsgOnce;
            sendMixPicAndTextOnce -= CallbackValueById_sendMixPicAndTextOnce;
            CallbackValueById.upLoadFileMsgOnce -= CallbackValueById_upLoadFileMsgOnce;
            CallbackValueById.sendTextMsgOnce -= CallbackValueById_sendTextMsgOnce;
            CallbackValueById.upLoadVoiceMsgOnce -= CallbackValueById_upLoadVoiceMsgOnce;
            MainWindowViewModel.CancelEventHandler -= MainWindowViewModel_CancelEventHandler;
            callbackOnmousewheelBack.UpdateReadImgEventHandler -= CallbackOnmousewheelBack_UpdateReadImgEventHandler;
            callbackOnmousewheelBack.EffictiveImgUrlEventHandler -= CallbackOnmousewheelBack_EffictiveImgUrlEventHandler;
            foreach (var list in SendMsgListPointMonitor.MessageStateMonitorList)
            {
                list._dispatcherTimer.Stop();
            }
            _chromiumWebBrowser.Dispose();
            PublicTalkMothed.isOneToOneRegester = false;
        }

        /// <summary>
        /// 即时消息推送
        /// </summary>
        /// <param name="mtp"></param>
        /// <param name="chatMsg"></param>
        private void LocalMessageHelper_InstantMessageHasBeenReceived(int mtp, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg)
        {
            if (chatMsg.MsgType == AntSdkMsgType.Revocation)
            {
                var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                    HideMsgAndShowRecallMsg(tempChatMsg.content?.messageId);
            }
            if (chatMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId) //本人发的消息，系统给的回执
            {
                //var contactUser = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == AntSdkService.AntSdkLoginOutput.userId);
                //if (contactUser == null) return;
                if (chatMsg.os == ((int)GlobalVariable.OSType.PC)) //PC端发的消息
                {
                    #region 重发消息处理
                    AttrDto attrDto = JsonConvert.DeserializeObject<AttrDto>(chatMsg.attr);
                    if (attrDto?.isResend == "1")
                    {
                        //更新消息状态
                        var result = SendOrReceiveMsgStateOperate.UpdateSendMsgPointStatus(chatMsg.messageId, chatMsg.chatIndex);
                        if (result == 1)
                        {
                            if (!SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(chatMsg.messageId)) return;
                            //更新界面状态
                            SendOrReceiveMsgStateOperate.UpdateUiState(AntSdkburnMsg.isSendSucessOrFail.sucess, this.chromiumWebBrowser, SendMsgListPointMonitor.MsgIdAndImgSendingId[chatMsg.messageId]);
                            //移除
                            SendMsgListPointMonitor.MsgIdAndImgSendingId.Remove(chatMsg.messageId);
                            LogHelper.WriteDebug("[TalkViewModel_LocalMessageHelper_InstantMessageHasBeenReceived]:[sucess]" + chatMsg.messageId);
                        }
                        else
                        {
                            LogHelper.WriteDebug("[TalkViewModel_LocalMessageHelper_InstantMessageHasBeenReceived]:[fail]" + chatMsg.messageId);
                        }
                        #region 停止并移除消息监控
                        var MsgState = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == chatMsg.messageId);
                        MsgState.dispatcherTimer.Stop();
                        MsgState.Dispose();
                        SendMsgListPointMonitor.MessageStateMonitorList.Remove(MsgState);
                        #endregion
                    }
                    else
                    {
                        if (!SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(chatMsg.messageId)) return;
                        //更新界面状态
                        SendOrReceiveMsgStateOperate.UpdateUiState(AntSdkburnMsg.isSendSucessOrFail.sucess,
                            this.chromiumWebBrowser, SendMsgListPointMonitor.MsgIdAndImgSendingId[chatMsg.messageId]);
                        //移除
                        SendMsgListPointMonitor.MsgIdAndImgSendingId.Remove(chatMsg.messageId);
                        LogHelper.WriteDebug(
                            "[TalkViewModel_LocalMessageHelper_InstantMessageHasBeenReceived]:[sucess]" +
                            chatMsg.messageId);

                        #region 停止并移除消息监控
                        var MsgState = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == chatMsg.messageId);

                        MsgState.dispatcherTimer.Stop();
                        MsgState.Dispose();
                        SendMsgListPointMonitor.MessageStateMonitorList.Remove(MsgState);

                        #endregion
                    }
                    #endregion
                }
                else
                {
                    //TODO:AntSdk_Modify
                    //chatMsg.MTP = mtp.ToString();
                    if (chatMsg.flag != 1)
                    {
                        chatMsg.readtime = "read";
                    }
                    LoadMsgData(GlobalVariable.BurnFlag.NotIsBurn, false);
                    //如果未读消息还没全展示，在打开会话展示界面过程中的即时消息先缓存。
                    if (ChatMsgLst.Count > 0)
                    {
                        ShowMsgData(ChatMsgLst);
                    }
                    if (!listChatIndex.Contains(chatMsg.messageId))
                    {
                        receiveMsg(chatMsg);
                    }
                    LogHelper.WriteDebug("[其他终端发的消息]:" + mtp + "----------" + JsonConvert.SerializeObject(chatMsg));
                }
            }
            else
            {
                //获取当前滚动条位置
                var task = chromiumWebBrowser.EvaluateScriptAsync("getScroolPosition();");
                task.Wait();
                if (task.Result.Success)
                {
                    if ((bool)task.Result.Result == false)
                    {
                        TextShowRowHeight = "32";
                        switch (chatMsg.MsgType)
                        {
                            case AntSdkMsgType.ChatMsgText:
                                #region 文本
                                if (chatMsg.flag == 1)
                                {
                                    TextShowReceiveMsg = "[阅后即焚消息]";
                                }
                                else
                                {
                                    TextShowReceiveMsg = chatMsg.sourceContent;
                                }
                                #endregion
                                break;
                            case AntSdkMsgType.ChatMsgFile:
                                #region 文件
                                if (chatMsg.flag == 1)
                                {
                                    TextShowReceiveMsg = "[阅后即焚消息]";
                                }
                                else
                                {
                                    TextShowReceiveMsg = "[文件]";
                                }
                                #endregion
                                break;
                            case AntSdkMsgType.ChatMsgAudio:
                                #region 语音
                                if (chatMsg.flag == 1)
                                {
                                    TextShowReceiveMsg = "[阅后即焚消息]";
                                }
                                else
                                {
                                    TextShowReceiveMsg = "[语音]";
                                }
                                #endregion
                                break;
                            case AntSdkMsgType.ChatMsgPicture:
                                if (chatMsg.flag == 1)
                                {
                                    TextShowReceiveMsg = "[阅后即焚消息]";
                                }
                                else
                                {
                                    TextShowReceiveMsg = "[图片]";
                                }
                                break;
                            case AntSdkMsgType.ChatMsgMixMessage:
                                #region 混合消息
                                string startStr = "";
                                //显示内容解析
                                List<AntSdkChatMsg.MixMessage_content> receive = JsonConvert.DeserializeObject<List<AntSdkChatMsg.MixMessage_content>>(chatMsg.sourceContent);
                                foreach (var content in receive)
                                {
                                    switch (content.type)
                                    {
                                        case "1001":
                                            startStr += content.content;
                                            break;
                                        default:
                                            startStr += "[图片]";
                                            break;
                                    }
                                }
                                TextShowReceiveMsg = startStr;
                                #endregion
                                break;
                        }
                    }
                    else
                    {
                        TextShowRowHeight = "0";
                        TextShowReceiveMsg = "";
                    }
                }
                LoadMsgData(GlobalVariable.BurnFlag.NotIsBurn, false);
                //如果未读消息还没全展示，在打开会话展示界面过程中的即时消息先缓存。
                if (ChatMsgLst.Count > 0)
                {
                    ShowMsgData(ChatMsgLst);
                }
                if (!listChatIndex.Contains(chatMsg.messageId))
                {
                    receiveMsg(chatMsg);
                    lastReceivedMsgTime = DateTime.Now;
                }
            }

        }

        /// <summary>
        /// 加载所有未读消息
        /// </summary>
        /// <param name="ctt"></param>
        /// <param name="isBurnMode">是否是阅后即焚标记</param>
        public void LoadMsgData(GlobalVariable.BurnFlag isBurnMode = GlobalVariable.BurnFlag.NotIsBurn, bool isChanageWindow = true)
        {
            _isChanageWindow = isChanageWindow;
            if (_isChanageWindow && _windowHelper != null)
                WindowMonitor.ChanageWindowHelper(_windowHelper);
            var offlineMessageList = MessageMonitor.GetOfflineMessageStatisticList(s_ctt.sessionId, isBurnMode) ?? new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            LogHelper.WriteWarn("---------------------------[TalkViewModel-LoadMsgData:]" + s_ctt.sessionId + "离线消息条数-------------" + offlineMessageList.Count);
            var onlineMessageList = SessionMonitor.GetWaitingToReceiveOnlineMessage(s_ctt.sessionId, (int)isBurnMode, AntSdkchatType.Point);
            LogHelper.WriteWarn("---------------------------[TalkViewModel-LoadMsgData:]" + s_ctt.sessionId + "在线消息条数-------------" + offlineMessageList.Count);
            var unReadMessageList = offlineMessageList;
            if (onlineMessageList != null && onlineMessageList.Count > 0)
            {
                unReadMessageList.AddRange(onlineMessageList);
                OnlineReceiveMessageList = onlineMessageList;
            }
            if (unReadMessageList.Count == 0)
                return;
            ChatMsgLst = unReadMessageList.OrderBy(m => int.Parse(m.chatIndex)).ToList();
            var lastMsg = ChatMsgLst[ChatMsgLst.Count - 1];
            if (lastMsg.status == 0)
            {
                PublicMessageFunction.SendChatMsgReceipt(lastMsg.sessionId, lastMsg.chatIndex, lastMsg.MsgType,
                    AntSdkReceiptType.ReadReceipt);

                LogHelper.WriteWarn(
                    "---------------------------[TalkViewModel-LoadMsgData:]SessionViewMouseLeftButtonDown" +
                    lastMsg.sessionId + "发送已读回执---------------------msessageId:" +
                    lastMsg.messageId + "------chatIndex:" + lastMsg.chatIndex);
            }
            LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]LoadMsgData" +
                                    ChatMsgLst[0].sessionId + "消息展示消息个数---------------------" + ChatMsgLst.Count);
        }

        /// <summary>
        /// 消息数据展示到界面
        /// </summary>
        /// <param name="isBurnMode"></param>
        /// <param name="isConversation">是否正在会话</param>
        public void ShowMsgData(List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> chatMsgList = null, GlobalVariable.BurnFlag isBurnMode = GlobalVariable.BurnFlag.NotIsBurn, bool isConversation = false)
        {
            if (chatMsgList == null)
            {
                chatMsgList = ChatMsgLst;
            }
            if (ChatMsgLst.Count > 0)
                LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]ShowMsgData" + ChatMsgLst[0].sessionId + "未读消息个数---------------------" + ChatMsgLst.Count);
            if (chatMsgList.Count == 0)
            {
                if (this._richTextBox != null)
                {
                    this._richTextBox.IsReadOnly = false;
                    _richTextBox.Focus();
                }
                return;
            }
            StringBuilder sbLeft = new StringBuilder();
            string topStr = PublicTalkMothed.topString();
            sbLeft.AppendLine(topStr);
            string checkId = "";
            foreach (var list in chatMsgList)
            {
                checkId = list.messageId;
                list.sendsucessorfail = 1;
                lock (obj)
                {
                    if (isConversation && list.MsgType == AntSdkMsgType.Revocation)
                    {
                        var tempChatMsg = (AntSdkChatMsg.Revocation)list;
                        if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                            HideMsgAndShowRecallMsg(tempChatMsg.content?.messageId);
                    }
                    #region 2014-06-14修改 New
                    List<string> imageId = new List<string>();
                    imageId.Clear();
                    switch (list.MsgType)
                    {
                        case AntSdkMsgType.ChatMsgPicture:
                            #region 图片消息
                            if (list.flag == 1)
                            {
                                burnMsg.IsReadImg bImg;
                                burnMsg.IsEffective isEffective;
                                if (list.readtime != "")
                                {
                                    bImg = burnMsg.IsReadImg.read;
                                    isEffective = burnMsg.IsEffective.effective;
                                }
                                else
                                {
                                    bImg = burnMsg.IsReadImg.notRead;
                                    isEffective = burnMsg.IsEffective.NotEffective;
                                }
                                SendImageDto rimgDto =
                                    JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:list.content*/
                                        list.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, list.messageId, rimgDto.picUrl, "",
                                    burnMsg.isBurnMsg.yesBurn, bImg, isEffective);
                            }
                            else
                            {
                                burnMsg.IsReadImg bImg;
                                if (list.readtime != "")
                                {
                                    bImg = burnMsg.IsReadImg.read;
                                }
                                else
                                {
                                    bImg = burnMsg.IsReadImg.notRead;
                                }
                                SendImageDto rimgDto =
                                    JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:list.content*/
                                        list.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, list.messageId, rimgDto.picUrl, "",
                                    burnMsg.isBurnMsg.notBurn, bImg, burnMsg.IsEffective.UnKnow);
                            }
                            break;
                        #endregion
                        case AntSdkMsgType.ChatMsgMixMessage:
                            #region 图文混合消息
                            List<AntSdkChatMsg.MixMessage_content> receives = JsonConvert.DeserializeObject<List<AntSdkChatMsg.MixMessage_content>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                            foreach (var ilist in receives)
                            {
                                string imgId = "RL" + Guid.NewGuid().ToString();
                                imageId.Add(imgId);
                                PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, imgId, content.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                            }
                            #endregion
                            break;
                    }
                    listChatIndex.Add(list.messageId);
                    string oneStr = PublicTalkMothed.LeftOneToOneShowMessage(list, user, imageId);
                    sbLeft.AppendLine(oneStr);

                    #endregion
                }
            }
            string endStr = PublicTalkMothed.endString();
            sbLeft.AppendLine(endStr);
            bool IsSucess = false;
            Task<JavascriptResponse> result = _chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
            result.Wait();
            if (result.Result.Success == true)
            {
                var isExist = chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                isExist.Wait();
                if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                {
                    IsSucess = false;
                }
                else
                {
                    IsSucess = true;
                }
            }
            while (IsSucess == false)
            {
                Task<JavascriptResponse> resultWhile = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                resultWhile.Wait();
                if (resultWhile.Result.Success)
                {
                    var isExist = chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                    isExist.Wait();
                    if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                    {
                        IsSucess = false;
                    }
                    else
                    {
                        IsSucess = true;
                    }
                }
                #region 2017-12-08 屏蔽
                //Thread.Sleep(50);
                //LogHelper.WriteWarn("[TalkViewModel-ShowMsgData:]" + sbLeft.ToString());
                //Task<JavascriptResponse> results2 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                //results2.Wait();
                //if (results2.Result.Success)
                //{
                //    LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]第二次执行成功---------------------");
                //}
                //else
                //{
                //    LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]第二次执行失败---------------------");
                //    Task<JavascriptResponse> results3 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                //    results3.Wait();
                //    Thread.Sleep(100);
                //    if (results3.Result.Success)
                //    {
                //        LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]第三次次执行成功---------------------");
                //    }
                //    else
                //    {
                //        LogHelper.WriteWarn("---------------------------[TalkViewModel-ShowMsgData:]第三次执行失败---------------------");
                //    }
                //}
                #endregion
                result.Dispose();
            }
            if (this._richTextBox != null)
            {
                this._richTextBox.IsReadOnly = false;
                this._richTextBox.Focus();
            }
            ChatMsgLst.Clear();
            _chromiumWebBrowser.ExecuteScriptAsync("setscross();");
        }

        public void CallbackId_ShowUserInfoEvent(object sender, EventArgs e)
        {
            var userID = sender as string;
            if (UserInfoControl == null)
            {
                UserInfoControl = new UserInfoViewModel(userID);
            }
            else
                UserInfoControl.InitUserInfo(userID);
            PopUserInfoIsOpen = false;
            PopUserInfoIsOpen = true;
        }

        private void _richTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //KeyboardHookLib.isHook = false;
            _richTextBox.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void _richTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            KeyboardHookLib.isHook = true;
            _richTextBox.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
        }

        private void ChromiumWebBrowser_Drop(object sender, System.Windows.DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CallbackOnmousewheelBack_EffictiveImgUrlEventHandler(object sender, EventArgs e)
        {
            string index = sender as string;
            AddImgUrlDto update = listDictImgUrls.SingleOrDefault(m => m.ChatIndex == index);
            if (update != null)
            {
                update.IsEffective = burnMsg.IsEffective.NotEffective;
            }
        }

        private void CallbackOnmousewheelBack_UpdateReadImgEventHandler(object sender, EventArgs e)
        {
            AddImgUrlDto update = listDictImgUrls.SingleOrDefault(m => m.ChatIndex == (sender as string));
            if (update != null)
            {
                update.IsRead = burnMsg.IsReadImg.read;
                update.IsEffective = burnMsg.IsEffective.effective;
            }
        }
        /// <summary>
        /// 获取图片集合事件
        /// </summary>
        public static event EventHandler GetImgUrlListEventHandler;

        private static void GetImgUrlList(List<AddImgUrlDto> listDictImgUrls)
        {
            if (GetImgUrlListEventHandler != null)
            {
                GetImgUrlListEventHandler(listDictImgUrls, null);
            }
        }
        private void CallbackOnmousewheelBack_AddImgUrlEventHandler(object sender, EventArgs e)
        {
            AddImgUrlDto addImg = sender as AddImgUrlDto;
            if (addImg != null)
            {
                AddDictImgUrl(addImg.PreOrEnd, addImg.ChatIndex, addImg.ImageUrl, "", addImg.IsBurn, addImg.IsRead, addImg.IsEffective);
            }
        }

        private void MainWindowViewModel_CancelEventHandler(object sender, EventArgs e)
        {
            if ((bool)sender)
            {
                CallbackValueById.sendTextMsgOnce -= CallbackValueById_sendTextMsgOnce;
            }
        }

        private void CallbackValueById_sendTextMsgOnce(object sender, EventArgs e)
        {
            #region 重发 同步方法

            //sendTextDto sendTextDto = sender as sendTextDto;
            //sendTextMethod(sendTextDto.msgStr, sendTextDto.messageid, sendTextDto.imageTipId, sendTextDto.imageSendingId,
            //    sendTextDto.FailOrSucess, null);

            #endregion

            #region 重发 异步 修改：姚伦海

            var sendTextDto = sender as sendTextDto;
            if (sendTextDto != null)
            {
                ThreadPool.QueueUserWorkItem(
                    m =>
                        sendTextMethod(sendTextDto.msgStr, sendTextDto.messageid, sendTextDto.imageTipId,
                            sendTextDto.imageSendingId, sendTextDto.FailOrSucess, null, sendTextDto.isOnceSendMsg));
            }

            #endregion
        }

        private void CallbackValueById_upLoadFileMsgOnce(object sender, EventArgs e)
        {
            var upLoadFilesDto = sender as UpLoadFilesDto;
            if (upLoadFilesDto != null)
            {
                if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(upLoadFilesDto.messageId))
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId[upLoadFilesDto.messageId] =
                        upLoadFilesDto.imageSendingId;
                }
                else
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(upLoadFilesDto.messageId,
                        upLoadFilesDto.imageSendingId);
                }
            }
            ThreadUploadFile(upLoadFilesDto);
        }

        private void CallbackValueById_upLoadImageMsgOnce(object sender, EventArgs e)
        {
            SendCutImageDto sendMsg = sender as SendCutImageDto;
            if (sendMsg == null) return;
            uploadImageSegment(sendMsg.prePaths, sendMsg.fileFileName, sendMsg.messageId, sendMsg.imgeTipId, sendMsg.imageSendingId, sendMsg.FailOrSucess);

            if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(sendMsg.messageId))
            {
                SendMsgListPointMonitor.MsgIdAndImgSendingId[sendMsg.messageId] = sendMsg.imageSendingId;
            }
            else
            {
                SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(sendMsg.messageId, sendMsg.imageSendingId);
            }
        }
        /// <summary>
        /// 语音重发处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallbackValueById_upLoadVoiceMsgOnce(object sender, EventArgs e)
        {
            var sendMsg = sender as SendCutImageDto;
            if (sendMsg == null) return;
            //if (sendMsg.prePaths.StartsWith("http://"))//语音已经上传成功，发送消息失败，重新发送
            //{
            //    SendVoiceAgain(sendMsg);
            //}
            //else//语音上传失败，重新上传再发送
            //{
            //    SendSound(null, sendMsg);
            //}
            SendSound(null, sendMsg);
        }
        public class callbackId
        {
            public static event EventHandler ShowUserInfoEvent;
            private void OnShowUserInfoEvent(string userId)
            {
                ShowUserInfoEvent?.Invoke(userId, null);
            }

            private ChromiumWebBrowsers ceBrowser;
            public callbackId(ChromiumWebBrowsers cefBrowsers)
            {
                this.ceBrowser = cefBrowsers;
            }
            public void showUserId(string id)
            {
                try
                {
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        //个人信息弹窗
                        OnShowUserInfoEvent(id);
                        //OnShowUserInfoEvent(id);
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_ShowMessage]:" + ex.Message + ex.StackTrace);
                }
            }
        }
        int ss = 0;
        /// <summary>
        /// 2017-02-22 滚轮事件
        /// </summary>
        public class callbackOnmousewheelBack
        {
            /// <summary>
            /// 存放sessionid和当前chatindex
            /// </summary>
            Dictionary<string, string> dictId = new Dictionary<string, string>();
            Dictionary<string, string> isFirstShow = new Dictionary<string, string>();
            public ChromiumWebBrowsers chromiumWebBrowser;
            AntSdkContact_User user;
            SendMessage_ctt s_ctt;
            public bool isScrolls = false;
            private List<string> listChatIndex;
            private IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> FirstPageData;
            private TalkViewModel talkViewModel;
            public callbackOnmousewheelBack(ChromiumWebBrowsers chromiumWebBrowser, AntSdkContact_User user, SendMessage_ctt s_ctt, List<string> chatIndex, TalkViewModel talkViewModel, IList<AntSdkChatMsg.ChatBase> firstPageData)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.user = user;
                    this.s_ctt = s_ctt;
                    listChatIndex = GetChatIndex(firstPageData);
                    FirstPageData = firstPageData;
                    this.talkViewModel = talkViewModel;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallBackSelectDirecoryFile]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            /// <summary>
            /// 获取第一屏messageId
            /// </summary>
            /// <param name="firstPageData"></param>
            /// <returns></returns>
            private List<string> GetChatIndex(IList<AntSdkChatMsg.ChatBase> firstPageData)
            {
                if (firstPageData != null && firstPageData.Count > 0)
                {
                    var idList = firstPageData.Select(v => v.messageId).ToList();
                    return idList;
                }
                return new List<string>();
            }

            public void isToEndWheel(bool isToEnd)
            {
                if (isToEnd == true)
                {
                    talkViewModel.TextShowRowHeight = "0";
                    talkViewModel.TextShowReceiveMsg = "";
                }
            }
            public int getValueWheel(string values)
            {
                try
                {
                    if (Convert.ToInt32(values.Split(',')[0]) > 0)
                    {
                        if (values.Split(',')[1] == "0")
                        {
                            string chatindex = chromiumWebBrowser.scrollChatIndex;
                            var _index = Convert.ToInt32(chatindex);
                            if (!dictId.Keys.Contains(s_ctt.sessionId) && _index > 1)
                            {
                                dictId.Add(s_ctt.sessionId, chatindex);
                                IList<AntSdkChatMsg.ChatBase> listChatdata = null;
                                //向上查询数据
                                var index = Convert.ToInt32(chatindex);
                                if (!AntSdkService.AntSdkIsConnected)
                                {
                                    //网络异常，从本地数据库拉取最新pageSize条消息
                                    listChatdata = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Point, false, pageSize, index);
                                }
                                else
                                {
                                    listChatdata = PublicMessageFunction.QueryMessageFromServer(s_ctt.sessionId, AntSdkchatType.Point, false, pageSize, index);
                                }
                                if (!isFirstShow.Keys.Contains(s_ctt.sessionId))
                                {
                                    isFirstShow.Add(s_ctt.sessionId, chatindex);
                                }
                                if (listChatdata != null && listChatdata.Count() > 0)
                                {
                                    for (int n = listChatdata.Count - 1; n >= 0; n--)
                                    {
                                        var list = listChatdata[n];
                                        if (AntSdkService.AntSdkCurrentUserInfo.robotId == user.userId)
                                        {
                                            list.IsRobot = true;
                                        }
                                        if (!listChatIndex.Contains(list.messageId))
                                        {
                                            listChatIndex.Add(list.messageId);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        if (AntSdkService.AntSdkCurrentUserInfo.userId != list.sendUserId)
                                        {
                                            #region 获取接收者头像 New
                                            string pathImage = "";
                                            //获取接收者头像
                                            var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == list.sendUserId);
                                            if (listUser == null)
                                            {
                                                AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == list.sendUserId);
                                                if (cus == null)
                                                {
                                                    user = new AntSdkContact_User();
                                                    pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                                                    user.copyPicture = pathImage;
                                                    user.userName = "离职人员";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(cus.picture + ""))
                                                    {
                                                        pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                                                        user.copyPicture = pathImage;
                                                    }
                                                    else
                                                    {
                                                        pathImage = cus.picture;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(listUser.Url + ""))
                                                {
                                                    pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                                                    user.copyPicture = pathImage;
                                                }
                                                else
                                                {
                                                    pathImage = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
                                                    user.copyPicture = pathImage;
                                                }
                                            }
                                            #endregion
                                        }
                                        switch (Convert.ToInt32(/*TODO:AntSdk_Modify:list.MTP*/list.MsgType))
                                        {
                                            #region 消息
                                            case (int)AntSdkMsgType.ChatMsgMixMessage:
                                                List<string> imageId = new List<string>();
                                                imageId.Clear();
                                                List<MixMessageObjDto> receive = null;
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    var sourceData = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent);
                                                    foreach (var listpic in receive)
                                                    {
                                                        if (listpic.type == "1002")
                                                        {
                                                            PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(listpic.content?.ToString());
                                                            string guid = Guid.NewGuid().ToString();
                                                            string imgId = "RL" + guid;
                                                            imageId.Add(imgId);
                                                            //listpic.ImgGuid = guid;
                                                            //listpic.ImgId = imgId;
                                                            //listpic.ImgPath = content.picUrl.Substring(8, content.picUrl.Length - 8);
                                                            //listpic.content = content.picUrl;
                                                        }
                                                    }
                                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                        OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, sourceData);
                                                }
                                                else
                                                {
                                                    receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    foreach (var ilist in receive)
                                                    {
                                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                                        imageId.Add(imgId);
                                                    }
                                                }

                                                #region 图文混合消息
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    #region 右边展示
                                                    OneRightSendPicAndText.RightScrollPicAndTextMix(chromiumWebBrowser, list, imageId);
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region 左边消息
                                                    OneRightSendPicAndText.LeftScrollPicAndTextMix(chromiumWebBrowser, list, user, imageId);
                                                    #endregion
                                                }
                                                #endregion
                                                receive.Reverse();
                                                imageId.Reverse();
                                                int j = 0;
                                                foreach (var imgInsert in receive)
                                                {
                                                    PictureDto content = JsonConvert.DeserializeObject<PictureDto>(imgInsert.content?.ToString());
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre,
                                                    imageId[j], content.picUrl, burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    j++;
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgText:
                                                //文本显示
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Point;
                                                        if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                            OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, list);
                                                    }
                                                    if (list.flag == 1)
                                                    {
                                                        PublicTalkMothed.rightAfterBurnScrollText(chromiumWebBrowser, list);
                                                    }
                                                    else
                                                    {
                                                        PublicTalkMothed.rightShowScrollText(chromiumWebBrowser, list);
                                                    }
                                                }
                                                else
                                                {
                                                    if (list.flag == 1)
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                        bool deleteSuccess = false;
                                                        PublicTalkMothed.leftAfterBurnScrollText(chromiumWebBrowser,
                                                            list, user, ref deleteSuccess);
                                                        if (deleteSuccess)
                                                        {
                                                            AntSdkChatMsg.ChatBase receipt =
                                                                new AntSdkChatMsg.ChatBase();
                                                            receipt.sessionId = s_ctt.sessionId;
                                                            receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                                                            receipt.chatIndex = list.chatIndex;
                                                            receipt.messageId = list.messageId;
                                                            receipt.targetId = this.user.userId;
                                                            SentBurnAfterReadEvent?.Invoke(receipt, list.MsgType);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        PublicTalkMothed.leftShowScrollText(chromiumWebBrowser, list, user);
                                                    }
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgPicture:
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        OnceSendImage sendImage = new OnceSendImage();
                                                        sendImage.ctt = this.s_ctt;
                                                        if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                            OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, sendImage);
                                                    }
                                                    if (list.flag == 1)
                                                    {
                                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, list.messageId, rimgDto.picUrl, burnMsg.isBurnMsg.yesBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                        PublicTalkMothed.rightAfterBurnScollImage(chromiumWebBrowser, list);
                                                    }
                                                    else
                                                    {
                                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, list.messageId, rimgDto.picUrl, burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.UnKnow);
                                                        PublicTalkMothed.rightShowScrollImage(chromiumWebBrowser, list);
                                                    }
                                                }
                                                else
                                                {
                                                    if (list.flag == 1)
                                                    {
                                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        burnMsg.IsReadImg bImg;
                                                        burnMsg.IsEffective isEffective;
                                                        if (list.readtime != null)
                                                        {
                                                            bImg = burnMsg.IsReadImg.read;
                                                            isEffective = burnMsg.IsEffective.effective;
                                                        }
                                                        else
                                                        {
                                                            bImg = burnMsg.IsReadImg.notRead;
                                                            isEffective = burnMsg.IsEffective.NotEffective;
                                                        }
                                                        AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, list.messageId, rimgDto.picUrl, burnMsg.isBurnMsg.yesBurn, bImg, isEffective);
                                                        PublicTalkMothed.leftAfterBurnScrollImage(chromiumWebBrowser, list, user);
                                                    }
                                                    else
                                                    {
                                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, list.messageId, rimgDto.picUrl, burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.UnKnow);
                                                        PublicTalkMothed.leftShowScrollImage(chromiumWebBrowser, list, user);
                                                    }
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgFile:
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Point;
                                                        OnceSendFile sendFile = new OnceSendFile();
                                                        sendFile.ctt = this.s_ctt;
                                                        if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                            OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, sendFile);
                                                    }
                                                    //阅后即焚
                                                    if (list.flag == 1)
                                                    {
                                                        PublicTalkMothed.rightAfterBurnScollFile(chromiumWebBrowser, list);
                                                    }
                                                    else
                                                    {
                                                        if (list.uploadOrDownPath == "" || list.uploadOrDownPath == null)
                                                        {
                                                            chromiumWebBrowser.sessionid = list.sessionId;
                                                            chromiumWebBrowser.currentChatIndex = list.chatIndex;
                                                        }
                                                        PublicTalkMothed.rightShowScrollFile(chromiumWebBrowser, list);
                                                    }
                                                }
                                                else
                                                {
                                                    //阅后即焚
                                                    if (list.flag == 1)
                                                    {
                                                        //未下载过的文件展示
                                                        //if(list.uploadOrDownPath==""||list.uploadOrDownPath==null)
                                                        //{
                                                        chromiumWebBrowser.flag = "1";
                                                        chromiumWebBrowser.currentChatIndex = list.chatIndex;
                                                        chromiumWebBrowser.sessionid = list.sessionId;
                                                        PublicTalkMothed.leftAterBurnScrollFile(chromiumWebBrowser, list, user);
                                                        //}
                                                        //else
                                                        //{

                                                        //}
                                                    }
                                                    else
                                                    {
                                                        //if(list.uploadOrDownPath==""||list.uploadOrDownPath==null)
                                                        //{
                                                        //    chromiumWebBrowser.sessionid = list.sessionId;
                                                        //    chromiumWebBrowser.currentChatIndex = list.chatIndex;
                                                        PublicTalkMothed.leftShowScrollFile(chromiumWebBrowser, list, user);
                                                        //}
                                                        //else
                                                        //{
                                                        //    PublicTalkMothed.leftShowScrollFileDown(chromiumWebBrowser, list, user);
                                                        //}
                                                    }
                                                }
                                                break;
                                            case (int)AntSdkMsgType.Revocation:
                                                chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.ScrollUIRecallMsg(list.sourceContent));
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgAudio:
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    if (list.flag == 1)
                                                    {
                                                        PublicTalkMothed.RightAfterBurnShowScrollVoice(chromiumWebBrowser, list);
                                                    }
                                                    else
                                                    {
                                                        PublicTalkMothed.RightShowScrollVoice(chromiumWebBrowser, list);
                                                    }
                                                }
                                                else
                                                {
                                                    if (list.flag == 1)
                                                    {
                                                        PublicTalkMothed.LeftAfterBurnShowScrollVoice(chromiumWebBrowser, list, user);
                                                    }
                                                    else
                                                    {
                                                        PublicTalkMothed.leftShowScrollVoice(chromiumWebBrowser, list, user);
                                                    }
                                                }
                                                break;
                                            case (int)AntSdkMsgType.PointAudioVideo:
                                                if (AntSdkService.AntSdkCurrentUserInfo.userId == list.sendUserId)
                                                {
                                                    PublicTalkMothed.rightShowScrollAudio(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    PublicTalkMothed.leftShowScrollAudio(chromiumWebBrowser, list, user);
                                                }
                                                break;
                                        }
                                        #endregion
                                    }
                                    var indexList = listChatdata.Select(m => int.Parse(m.chatIndex)).Min();
                                    chromiumWebBrowser.scrollChatIndex = indexList.ToString();//listChatdata[listChatdata.Count - 1].chatIndex;
                                    chromiumWebBrowser.ExecuteScriptAsync("setScrollToPosition('" + listChatdata[listChatdata.Count - 1].messageId + "');");
                                }
                                if (listChatdata != null && listChatdata.Count() > 0)
                                {
                                    dictId.Remove(s_ctt.sessionId);
                                }
                            }
                            else
                            {
                                PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, "history");
                            }
                        }
                    }
                    return 123;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_openDirecrory]:" + ex.Message + ex.StackTrace + ex.Source);
                    return 0;
                }
            }
            public void updateReadTime(string chatIndex, string messageid)
            {
                string getChatIndex = chatIndex.Substring(43, chatIndex.Length - 43);
                int time = PublicTalkMothed.ConvertDateTimeInt(DateTime.Now);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.UpdateAfterReadBurnTime(s_ctt.sessionId, s_ctt.companyCode,
                    AntSdkService.AntSdkCurrentUserInfo.userId, messageid, time.ToString());
                SentBurnAfterReadReceipt(getChatIndex, messageid);
                //更新阅后即焚是否已经读了
                UpdateReadImg(messageid);
            }
            public static event EventHandler UpdateReadImgEventHandler;

            private void UpdateReadImg(string chatIndex)
            {
                if (UpdateReadImgEventHandler != null)
                {
                    UpdateReadImgEventHandler(chatIndex, null);
                }
            }

            public void deleteMessageByChatIndex(string chatIndex, string messageId)
            {
                //string getChatIndex = chatIndex.Substring(38, chatIndex.Length - 38);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, messageId);
                AntSdkChatMsg.ChatBase receipt = new AntSdkChatMsg.ChatBase();
                receipt.sessionId = this.s_ctt.sessionId;
                receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                receipt.chatIndex = chatIndex;
                receipt.targetId = this.user.userId;
                receipt.messageId = messageId;
                SentBurnAfterReadEvent?.Invoke(receipt, AntSdkMsgType.PointBurnReaded, false);
                //var nns=listChatIndex.SingleOrDefault(m => m.Equals(getChatIndex));
                //listChatIndex.Remove(listChatIndex.SingleOrDefault(m => m.Equals(getChatIndex)));
                EffictiveImgUrl(messageId);
            }

            /// <summary>
            ///更改阅后即焚图片为无效
            /// </summary>
            public static event EventHandler EffictiveImgUrlEventHandler;

            private void EffictiveImgUrl(string chatIndex)
            {
                if (EffictiveImgUrlEventHandler != null)
                {
                    EffictiveImgUrlEventHandler(chatIndex, null);
                }
            }
            /// <summary>
            /// 用户B收到A的阅后即焚消息，发送已读回执给消息服务
            /// </summary>
            public void SentBurnAfterReadReceipt(string chatIndex, string messageId)
            {
                if (this.s_ctt == null) return;
                BurnAfterReadReceipt receipt = new BurnAfterReadReceipt();
                receipt.ctt = new BurnAfterReadReceiptCtt();
                receipt.ctt.sendUserId = this.s_ctt.sendUserId;
                receipt.ctt.targetId = this.s_ctt.targetId;
                receipt.ctt.companyCode = GlobalVariable.CompanyCode;
                receipt.ctt.chatIndex = chatIndex;
                receipt.ctt.os = ((int)GlobalVariable.OSType.PC).ToString();
                receipt.ctt.sessionId = this.s_ctt.sessionId;
                receipt.ctt.content = null;
                string errMsg = string.Empty;
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                var burnRead = new AntSdkSendMsg.PointBurnReaded
                {
                    sendUserId = this.s_ctt.sendUserId,
                    targetId = this.s_ctt.targetId,
                    chatIndex = chatIndex,
                    os = (int)GlobalVariable.OSType.PC,
                    sessionId = this.s_ctt.sessionId,
                    MsgType = AntSdkMsgType.PointBurnReaded,
                    chatType = (int)AntSdkchatType.Point,
                    messageId = PublicTalkMothed.timeStampAndRandom(),
                    content = new AntSdkSendMsg.PointBurnReaded_content
                    {
                        readIndex = int.Parse(chatIndex),
                        //TODO:收到的那条阅后即焚消息的messageId
                        messageId = messageId
                    }
                };
                AntSdkService.SdkPublishPointBurnReadReceiptMsg(burnRead, ref errMsg);
                //MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageBurn, receipt, ref errMsg,
                //    NullValueHandling.Ignore);
            }

            public static event EventHandler AddImgUrlEventHandler;

            private void AddImgUrl(AddImgUrlDto.InsertPreOrEnd preOrEnd, string chatIndex, string imgUrl, burnMsg.isBurnMsg isBurn, burnMsg.IsReadImg isRead, burnMsg.IsEffective isEffective)
            {
                if (AddImgUrlEventHandler != null)
                {
                    AddImgUrlDto addImg = new AddImgUrlDto();
                    addImg.PreOrEnd = preOrEnd;
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imgUrl;
                    addImg.IsBurn = isBurn;
                    addImg.IsRead = isRead;
                    addImg.IsEffective = isEffective;
                    AddImgUrlEventHandler(addImg, null);
                }
            }
            /// <summary>
            ///滚动去重复
            /// </summary>
            public static event EventHandler AddChatIndexEventHandler;

            private void AddChatIndex(string listChatIndex)
            {
                if (AddChatIndexEventHandler != null)
                {
                    AddChatIndexEventHandler(listChatIndex, null);
                }
            }
        }
        private void _chromiumWebBrowser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (cms.IsOpen == true)
            //{
            //    cms.IsOpen = false;
            //}
        }
        private void MiCopys_Click(object sender, RoutedEventArgs e)
        {
            this.chromiumWebBrowser.Copy();
        }

        private void _chromiumWebBrowser_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            cms.IsOpen = true;
        }

        private void MiCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                msgEditAssistant.CopyMsgContent();
                if (!Clipboard.ContainsData(format))
                {
                    var imageLst = Clipboard.GetFileDropList();
                    if (imageLst.Count == 0)
                    {
                        Clipboard.Clear();
                        Clipboard.SetDataObject(_richTextBox.Selection.Text);
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("MiPaste_Click：" + ex.Message);
            }
        }

        private void MiCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                msgEditAssistant.CopyMsgContent();
                if (!Clipboard.ContainsData(format))
                {
                    var imageLst = Clipboard.GetFileDropList();
                    if (imageLst.Count == 0)
                    {
                        Clipboard.Clear();
                        Clipboard.SetDataObject(_richTextBox.Selection.Text);
                    }
                }
                if (!_richTextBox.Selection.IsEmpty)
                {
                    var textRange = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End)
                    {
                        Text = ""
                    };
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("MiPaste_Click：" + ex.Message);
            }
        }

        private void MiPaste_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsData(format))
            {
                msgEditAssistant.PasteMsgContent();
            }
            else
            {
                msgEditAssistant.PasteExternalMsgContent();
            }
            e.Handled = true;
        }
        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _richTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                var type = GlobalVariable.systemSetting == null ? 0 : GlobalVariable.systemSetting.SendKeyType;
                switch (type)
                {
                    case 0:
                        if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                        {
                            if (!AntSdkService.AntSdkIsConnected)//重连成功
                            {
                                showTextMethod("网络连接已断开，不能发送消息！");
                                hOffset = -135;
                                e.Handled = true;
                                return;
                            }
                            var counts = _richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                            if (!counts.Any())
                            {
                                hOffset = -22;
                                showTextMethod("发送消息不能为空，请重新输入！");
                                e.Handled = true;
                            }
                            else
                            {
                                sendMsg(e);
                                e.Handled = true;
                            }
                        }
                        break;
                    case 1:
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
                        {
                            if (!AntSdkService.AntSdkIsConnected)//重连成功
                            {
                                showTextMethod("网络连接已断开，不能发送消息！");
                                hOffset = -135;
                                e.Handled = true;
                                return;
                            }
                            var counts = _richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                            if (!counts.Any())
                            {
                                hOffset = -22;
                                showTextMethod("发送消息不能为空，请重新输入！");
                                e.Handled = true;
                            }
                            else
                            {
                                sendMsg(e);
                                e.Handled = true;
                            }
                        }
                        break;
                }

                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                {
                    try
                    {
                        msgEditAssistant.CopyMsgContent();
                        if (!Clipboard.ContainsData(format))
                        {
                            var imageLst = Clipboard.GetFileDropList();
                            if (imageLst.Count == 0)
                            {
                                Clipboard.Clear();
                                Clipboard.SetDataObject(_richTextBox.Selection.Text);
                            }
                        }
                        e.Handled = true;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("MiPaste_Click：" + ex.Message);
                    }
                }
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                {
                    if (Clipboard.ContainsData(format))
                    {
                        msgEditAssistant.PasteMsgContent();
                    }
                    else
                    {

                        msgEditAssistant.PasteExternalMsgContent();
                    }
                    e.Handled = true;
                }
                //默认截图按键
                //if (GlobalVariable.systemSetting == null ||
                //    string.IsNullOrEmpty(GlobalVariable.systemSetting.KeyShortcuts))
                //{
                //    if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 &&
                //        e.Key == Key.Q)
                //    {
                //        msgEditAssistant.CutImage();
                //        e.Handled = true;
                //    }
                //}
                //自定义
                //else
                //{
                //    int keyNum = Convert.ToInt32(GlobalVariable.systemSetting.KeyShortcuts);
                //    var keyChar = (System.Windows.Forms.Keys)keyNum;
                //    if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 &&
                //        e.Key.ToString() == keyChar.ToString())
                //    {
                //        msgEditAssistant.CutImage();
                //        e.Handled = true;
                //    }
                //}
                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0 && e.Key == Key.Enter)
                {
                    TextPointer textPointer = _richTextBox.CaretPosition.InsertLineBreak();
                    _richTextBox.CaretPosition = textPointer;
                    e.Handled = true;
                }
                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.X)
                {
                    msgEditAssistant.CopyMsgContent();
                    if (Clipboard.ContainsData(format))
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        Clipboard.Clear();
                        Clipboard.SetDataObject(_richTextBox.Selection.Text);
                    }
                    if (!_richTextBox.Selection.IsEmpty)
                    {
                        var textRange = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End)
                        {
                            Text = ""
                        };
                    }
                    e.Handled = true;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[_richTextBox_PreviewKeyDown]" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 截图
        /// </summary>
        public void TalkViewCutImage()
        {
            msgEditAssistant.CutImage();
        }
        /// <summary>
        /// 语音电话中间提示
        /// </summary>
        /// <param name="msg"></param>
        public void AudioTips(string msg)
        {
            PublicTalkMothed.showCenterMsg(_chromiumWebBrowser, msg);
        }
        /// <summary>
        /// 语音电话气泡消息
        /// </summary>
        /// <param name="msg"></param>
        public void AudioMessage(string msg)
        {
            var dt = DateTime.Now;
            PublicTalkMothed.RightShowSendAudio(_chromiumWebBrowser, msg, dt, 0);
        }
        #region 属性
        public string _isShowTimeOutMsg = "Visible";
        /// <summary>
        /// 超过两分钟撤销提示
        /// </summary>
        public string isShowTimeOutMsg
        {
            set
            {
                this._isShowTimeOutMsg = value;
                RaisePropertyChanged(() => isShowTimeOutMsg);
            }
            get { return this._isShowTimeOutMsg; }
        }
        /// <summary>
        /// 是否显示Emoji按钮
        /// </summary>
        private string _isShowEmoji = "Visible";
        public string isShowEmoji
        {
            set
            {
                this._isShowEmoji = value;
                RaisePropertyChanged(() => isShowEmoji);
            }
            get { return this._isShowEmoji; }
        }
        /// <summary>
        /// 是否显示截图按钮
        /// </summary>
        private string _isShowCutImage = "Visible";
        public string isShowCutImage
        {
            set
            {
                this._isShowCutImage = value;
                RaisePropertyChanged(() => isShowCutImage);
            }
            get { return this._isShowCutImage; }
        }
        /// <summary>
        /// 阅后即焚图标是否显示
        /// </summary>
        private string _isShowBurn = "Visible";
        public string isShowBurn
        {
            set
            {
                this._isShowBurn = value;
                RaisePropertyChanged(() => isShowBurn);
            }
            get { return this._isShowBurn; }
        }
        /// <summary>
        /// 语音电话按钮是否可用
        /// </summary>
        private Visibility _isShowAudio = Visibility.Visible;
        public Visibility isShowAudio
        {
            get { return this._isShowAudio; }
            set
            {
                this._isShowAudio = value;
                RaisePropertyChanged(() => isShowAudio);
            }
        }
        /// <summary>
        /// 是否显示阅后即焚退出按钮
        /// </summary>
        private string _isShowExit = "Collapsed";
        public string isShowExit
        {
            set
            {
                this._isShowExit = value;
                RaisePropertyChanged(() => isShowExit);
            }
            get { return this._isShowExit; }
        }

        private bool _isBurnState;
        /// <summary>
        /// 当前会话窗体的模式状态
        /// </summary>
        public bool IsBurnState
        {
            get { return _isBurnState; }
            set
            {
                _isBurnState = value;
                RaisePropertyChanged(() => IsBurnState);
            }
        }

        private string _UserName;
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName
        {
            get { return this._UserName; }
            set
            {
                this._UserName = value;
                RaisePropertyChanged(() => UserName);
            }
        }
        private string _Picture;
        /// <summary>
        /// 头像
        /// </summary>
        public string Picture
        {
            get { return this._Picture; }
            set
            {
                this._Picture = value;
                RaisePropertyChanged(() => Picture);
            }
        }

        private string _SigNature = "个性签名!";
        /// <summary>
        /// 个性签名
        /// </summary>
        public string SigNature
        {
            get { return this._SigNature; }
            set
            {
                this._SigNature = value;
                RaisePropertyChanged(() => SigNature);
            }
        }
        private Visibility _TalkHistoryVisibility = Visibility.Collapsed;
        /// <summary>
        /// 聊天记录是否可见
        /// </summary>
        public Visibility TalkHistoryVisibility
        {
            get { return this._TalkHistoryVisibility; }
            set
            {
                this._TalkHistoryVisibility = value;
                RaisePropertyChanged(() => TalkHistoryVisibility);
            }
        }

        private TalkHistoryViewModel _TalkHistoryViewModel;
        /// <summary>
        /// 聊天记录
        /// </summary>
        public TalkHistoryViewModel TalkHistoryViewModel
        {
            get { return this._TalkHistoryViewModel; }
            set
            {
                this._TalkHistoryViewModel = value;
                RaisePropertyChanged(() => TalkHistoryViewModel);
            }
        }
        private bool _PopUserInfoIsOpen;
        public bool PopUserInfoIsOpen
        {
            get { return _PopUserInfoIsOpen; }
            set
            {
                this._PopUserInfoIsOpen = value;
                RaisePropertyChanged(() => PopUserInfoIsOpen);
            }
        }
        private UserInfoViewModel _UserInfoControl;
        public UserInfoViewModel UserInfoControl
        {
            get
            {
                return _UserInfoControl;
            }
            set
            {
                _UserInfoControl = value;
                RaisePropertyChanged(() => UserInfoControl);
            }
        }
        /// <summary>
        /// 语音按钮是否显示
        /// </summary>
        private Visibility _isShowSound = Visibility.Visible;
        public Visibility isShowSound
        {
            set
            {
                this._isShowSound = value;
                RaisePropertyChanged(() => isShowSound);
            }
            get { return this._isShowSound; }
        }
        #endregion

        #region 创建讨论组
        public delegate void MouseClickDelegate(AntSdkCreateGroupOutput group);
        public static event MouseClickDelegate CreateGroupEvent;
        private void OnCreateGroupEvent(AntSdkCreateGroupOutput group)
        {
            if (CreateGroupEvent != null)
            {
                try
                {
                    CreateGroupEvent(group);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_OnCreateGroupEvent]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
        }
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
                                  try
                                  {
                                      Views.Contacts.GroupEditWindowView win = new Views.Contacts.GroupEditWindowView();
                                      win.ShowInTaskbar = false;
                                      List<string> memberIds = new List<string>();
                                      memberIds.Add(AntSdkService.AntSdkLoginOutput.userId);
                                      memberIds.Add(user.userId);
                                      if (user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId
                                      && AntSdkService.AntSdkListContactsEntity.users.Exists(m => m.userId == AntSdkService.AntSdkCurrentUserInfo.robotId))
                                          memberIds.Add(AntSdkService.AntSdkCurrentUserInfo.robotId);
                                      GroupEditWindowViewModel model = new GroupEditWindowViewModel(win.Close, memberIds);
                                      win.DataContext = model;
                                      win.Owner = Antenna.Framework.Win32.GetTopWindow();
                                      win.ShowDialog();
                                      if (model.CreateGroupOutput != null)
                                      {
                                          CreateGroupEvent?.Invoke(model.CreateGroupOutput);
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      LogHelper.WriteError("[TalkViewModel_CreateGroupCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                                  }
                              });
                }
                return this._CreateGroupCommand;
            }
        }
        #endregion

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private AntSdkUserInfo GetUserInfo(string userId)
        {
            try
            {
                string errMsg = string.Empty;
                UserOutput userOutput = new UserOutput();
                UserInput input = new UserInput();
                input.token = AntSdkService.AntSdkLoginOutput.token;
                input.version = GlobalVariable.Version;
                input.userId = AntSdkService.AntSdkLoginOutput.userId;
                input.targetUserId = userId;
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                return GroupPublicFunction.QueryUserInfo(userId);
                //if (!(new HttpService()).GetUserInfo(input, ref userOutput, ref errMsg))
                //{
                //    return null;
                //}
            }
            catch (Exception ex)
            {

                LogHelper.WriteError("[TalkViewModel_GetUserInfo]:" + ex.Message + ex.StackTrace + ex.Source);
                return null;
            }
        }

        #region 命令
        /// <summary>
        /// 聊天记录
        /// </summary>
        private ICommand _HistoryCommand;
        public ICommand HistoryCommand
        {
            get
            {
                if (this._HistoryCommand == null)
                {
                    this._HistoryCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            if (TalkHistoryVisibility == Visibility.Collapsed)
                            {
                                TalkHistoryViewModel = new TalkHistoryViewModel(s_ctt, user);
                            }
                            if (TalkHistoryVisibility == Visibility.Visible)
                            {
                                TalkHistoryViewModel.cwm.Dispose();
                            }
                            TalkHistoryVisibility = TalkHistoryVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkViewModel_HistoryCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._HistoryCommand;
            }
        }
        /// <summary>
        /// 查看联系人资料
        /// </summary>
        private ICommand _HeadCommand;
        public ICommand HeadCommand
        {
            get
            {
                if (this._HeadCommand == null)
                {
                    this._HeadCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            Win_UserInfoView win = new Win_UserInfoView();
                            win.ShowInTaskbar = false;
                            Win_UserInfoViewModel model = new Win_UserInfoViewModel(user.userId);
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkViewModel_HeadCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._HeadCommand;
            }
        }

        #endregion



        bool IsFirst = false;

        private FileMultiUpload _fileMultiUpload = new FileMultiUpload();

        public FileMultiUpload fileMultiUpload
        {
            set
            {
                _fileMultiUpload = value;
                RaisePropertyChanged(() => fileMultiUpload);
            }
            get { return _fileMultiUpload; }
        }
        private System.Windows.Controls.RichTextBox _richTextBox = new System.Windows.Controls.RichTextBox() { BorderThickness = new Thickness(0), VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(20, 10, 20, 0), Document = new WFdExtend.WFdExtend() { LineHeight = 2, Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)), FontFamily = new FontFamily("微软雅黑"), FontSize = 13 } };
        public System.Windows.Controls.RichTextBox richTextBox
        {
            set
            {
                _richTextBox = value;
                RaisePropertyChanged(() => richTextBox);
            }
            get { return _richTextBox; }
        }
        static string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
        static string indexPath = "file:///" + pathHtml + "web_content/index.html";
        public ChromiumWebBrowsers _chromiumWebBrowser = new ChromiumWebBrowsers() { Address = indexPath };
        public ChromiumWebBrowsers chromiumWebBrowser
        {
            set
            {
                _chromiumWebBrowser = value;
                RaisePropertyChanged(() => chromiumWebBrowser);
            }
            get { return _chromiumWebBrowser; }
        }
        public ICommand btnImagesUploadCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    msgEditAssistant.ImgImagesUpload();
                });
            }
        }
        List<UpLoadFilesDto> upFileList = new List<UpLoadFilesDto>();
        #region 上传文件事件
        public ICommand btnFileUploadCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
                    //openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|Rar文件(*.rar)|*.rar|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|mp3文件(*.mp3)|*.mp3";
                    //openFile.Filter = "所有文件(*.*)|*.*|所有文件(*.txt;*.xlsx;*.xls;*.docx;*.doc;*.rar;*.pdf;*.html;*.htm;*.mp3|*.txt;*.xlsx;*.xls;*.docx;*.doc;*.rar;*.pdf;*.html;*.htm;*.mp3|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|Rar文件(*.rar)|*.rar|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|mp3文件(*.mp3)|*.mp3";
                    openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe";
                    openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    openFile.FilterIndex = 0;
                    openFile.Multiselect = true;
                    if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SelectUploadFiles(openFile.FileNames);
                    }
                });
            }
        }
        /// <summary>
        /// 进入阅后即焚模式
        /// </summary>
        public ICommand btnReadBurnAfterCommand
        {
            get
            {
                return new DelegateCommand<System.Windows.Controls.Button>((obj) =>
                {
                    string isShow = publicMethod.xmlFind("isFirstShowBurnWin",
                        AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                    if (isShow == "0")
                    {
                        System.Windows.Controls.Button btnBrunSwitch = obj as System.Windows.Controls.Button;
                        AfterReadBrunWindow afterReadBurn = new AfterReadBrunWindow();
                        afterReadBurn.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        afterReadBurn.Owner = Window.GetWindow(btnBrunSwitch);
                        afterReadBurn.ShowDialog();

                        publicMethod.xmlModify("isFirstShowBurnWin", "1",
                            AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                        string value = publicMethod.xmlFind("burn" + s_ctt.sessionId, publicMethod.xmlPath());
                        if (string.IsNullOrEmpty(value))
                        {
                            publicMethod.xmlAdd("burn" + s_ctt.sessionId, "1", publicMethod.xmlPath());
                        }
                    }
                    else
                    {
                        string value = publicMethod.xmlFind("burn" + s_ctt.sessionId, publicMethod.xmlPath());
                        if (string.IsNullOrEmpty(value))
                        {
                            publicMethod.xmlAdd("burn" + s_ctt.sessionId, "1", publicMethod.xmlPath());
                        }
                        else
                        {
                            publicMethod.xmlModify("burn" + s_ctt.sessionId, "1", publicMethod.xmlPath());
                        }
                    }
                    publicMethod.xmlModify("isBurnMode", "1",
                            AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                    BurnMode();
                    _richTextBox.IsReadOnly = false;
                    _richTextBox.Focus();
                });
            }
        }
        /// <summary>
        /// 阅后即焚模式图片切换
        /// </summary>
        private void BurnMode()
        {
            isShowBurn = "Collapsed";
            isShowEmoji = "Collapsed";
            isShowSound = Visibility.Collapsed;
            isShowAudio = Visibility.Collapsed;
            isShowCutImage = "Collapsed";
            isShowExit = "Visible";
            IsBurnState = true;
        }
        /// <summary>
        /// 退出阅后即焚模式
        /// </summary>
        public ICommand btnBurnExitCommand
        {
            get
            {
                return new DelegateCommand<System.Windows.Controls.Button>((obj) =>
                {
                    System.Windows.Controls.Button btn = obj as System.Windows.Controls.Button;
                    publicMethod.xmlModify("isBurnMode", "0",
                           AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                    publicMethod.xmlModify("burn" + s_ctt.sessionId, "0", publicMethod.xmlPath());
                    NotBurnMode();
                    _richTextBox.IsReadOnly = false;
                    _richTextBox.Focus();
                });
            }
        }
        /// <summary>
        /// 退出阅后即焚模式
        /// </summary>
        private void NotBurnMode()
        {
            isShowBurn = "Visible";
            isShowEmoji = "Visible";
            isShowSound = Visibility.Visible;
            isShowCutImage = "Visible";
            isShowExit = "Collapsed";
            IsBurnState = false;
            isShowAudio = Visibility.Visible;
        }


        /// <summary>
        /// 选择需上传文件
        /// </summary>
        /// <param name="fileNames"></param>
        public void SelectUploadFiles(string[] fileAndDirNames)
        {
            //文件是否占用
            bool isFileUse = false;
            //过滤掉文件夹和重复文件
            List<string> fileNames = new List<string>();
            bool isContainDirectory = false;
            bool isContainDuplicateFile = false;
            for (int i = 0; i < fileAndDirNames.Length; i++)
            {
                if (File.Exists(fileAndDirNames[i]))// 是文件
                {
                    if (upFileList.Exists(c => c.localOrServerPath == fileAndDirNames[i]))
                    {
                        isContainDuplicateFile = true;
                    }
                    else
                    {
                        if (PublicTalkMothed.IsFileInUsing(fileAndDirNames[i]))
                        {
                            isFileUse = true;
                        }
                        else
                        {
                            fileNames.Add(fileAndDirNames[i]);
                        }
                    }
                }
                else if (Directory.Exists(fileAndDirNames[i]))// 是文件夹
                {
                    isContainDirectory = true;
                }
            }

            //判断文件上传个数
            if (fileNames.Count() > 5)
            {
                KeyboardHookLib.isHook = true;
                MessageBoxWindow.Show("温馨提示", "上传文件不能超过5个!", GlobalVariable.WarnOrSuccess.Warn);
                //if (_richTextBox.IsFocused)
                //    KeyboardHookLib.isHook = false;
                return;
            }
            if (fileNames.Count() >= 1)
            {
                foreach (var listFile in fileNames)
                {
                    var msg = string.Empty;
                    System.IO.FileInfo fileInfo = new FileInfo(listFile);
                    int type = 0;
                    var fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                    if (fileSize > 30)
                    {
                        msg = "单个上传文件不能超过30MB" + "\r\n";
                        type = 1;
                        msg += fileInfo.Name + "\r\n";
                    }
                    else if (fileSize == 0)
                    {
                        msg = "上传文件不能空！" + "\r\n";
                        type = 1;
                        msg += fileInfo.Name + "\r\n";
                    }
                    if (type == 1)
                    {
                        KeyboardHookLib.isHook = true;
                        MessageBoxWindow.Show("温馨提示", msg + "", GlobalVariable.WarnOrSuccess.Warn);
                        //if (_richTextBox.IsFocused)
                        //    KeyboardHookLib.isHook = false;
                        return;
                    }
                }
            }

            foreach (var listFile in fileNames)
            {
                if (upFileList.Count() > 5)
                {
                    KeyboardHookLib.isHook = true;
                    //System.Windows.Forms.MessageBox.Show("上传文件不能超过10个!", "温馨提示");
                    MessageBoxWindow.Show("温馨提示", "上传文件不能超过5个!", GlobalVariable.WarnOrSuccess.Warn);
                    //if (_richTextBox.IsFocused)
                    //    KeyboardHookLib.isHook = false;
                    return;
                }
                UpLoadFilesDto upFileDto = new UpLoadFilesDto();
                upFileDto.messageId = PublicTalkMothed.timeStampAndRandom();
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                upFileDto.imageTipId = imageTipId;

                upFileDto.fileGuid = "wlc" + Guid.NewGuid().ToString().Replace("-", "");
                upFileDto.fileName = listFile.Substring((listFile.LastIndexOf('\\') + 1), listFile.Length - 1 - listFile.LastIndexOf('\\'));

                System.IO.FileInfo fileInfo = new FileInfo(listFile);
                //if (fileInfo.Length < 1024)
                //{
                //    upFileDto.fileSize = fileInfo.Length + "B";
                //}
                //if (fileInfo.Length > 1024)
                //{
                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024, 2) + "KB";
                //}
                //if (fileInfo.Length > 1024 * 1024)
                //{
                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 2) + "MB";
                //}
                upFileDto.fileSize = fileInfo.Length.ToString();
                upFileDto.localOrServerPath = listFile;
                upFileDto.fileExtendName = listFile.Substring((listFile.LastIndexOf('.') + 1), listFile.Length - 1 - listFile.LastIndexOf('.'));
                upFileDto.cmpcd = s_ctt.companyCode;
                upFileDto.seId = s_ctt.sessionId;
                upFileList.Add(upFileDto);
            }

            if (upFileList.Count() > 0)
            {
                //showFile.Height = new GridLength(102);
                FileUploadShowHeight = 102;
                _wrapPanel.Children.Clear();
                foreach (var list in upFileList)
                {
                    FileUserControl fUserControl = new FileUserControl();
                    fUserControl.btnClose.Click += BtnClose_Click;

                    fUserControl.Tag = list.fileGuid;
                    fUserControl.fileName.Text = list.fileName.Length > 8 ? list.fileName.Substring(0, 8) + "..." : list.fileName;

                    fUserControl.Tag = list.fileGuid;
                    fUserControl.fileName.Text = list.fileName.Length > 4 ? list.fileName.Substring(0, 4) : list.fileName;

                    fUserControl.img.Source = new BitmapImage(new Uri(fileShowImage.showImageHtmlPath(list.fileExtendName, ""), UriKind.RelativeOrAbsolute));
                    fUserControl.fileName.ToolTip = list.fileName;
                    fUserControl.btnClose.Tag = fUserControl;
                    _wrapPanel.Children.Add(fUserControl);
                }
            }
            if (isFileUse)
            {
                KeyboardHookLib.isHook = true;
                MessageBoxWindow.Show("温馨提示", "    文件被其他程序占用，无法上传，请检查并关闭相应的程序。", GlobalVariable.WarnOrSuccess.Warn);
                //if (_richTextBox.IsFocused)
                //    KeyboardHookLib.isHook = false;
                return;
            }
            if (isContainDirectory && isContainDuplicateFile)
            {
                KeyboardHookLib.isHook = true;
                MessageBoxWindow.Show("温馨提示", "不能上传文件夹!" + Environment.NewLine + "不能上传重复文件!", GlobalVariable.WarnOrSuccess.Warn);
                //if (_richTextBox.IsFocused)
                //    KeyboardHookLib.isHook = false;
            }
            else if (isContainDirectory)
            {
                KeyboardHookLib.isHook = true;
                MessageBoxWindow.Show("温馨提示", "不能上传文件夹!", GlobalVariable.WarnOrSuccess.Warn);
                //if (_richTextBox.IsFocused)
                //    KeyboardHookLib.isHook = false;
            }
            else if (isContainDuplicateFile)
            {
                KeyboardHookLib.isHook = true;
                MessageBoxWindow.Show("温馨提示", "不能上传重复文件!", GlobalVariable.WarnOrSuccess.Warn);
                //if (_richTextBox.IsFocused)
                //    KeyboardHookLib.isHook = false;
            }
        }

        private void _chromiumWebBrowser_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Move;
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                string[] s = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);
                SelectUploadFiles(s);
            }
            e.Handled = true;
        }

        private void richTextBox_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Move;
            e.Handled = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button fileControl = sender as System.Windows.Controls.Button;
            FileUserControl fuc = fileControl.Tag as FileUserControl;

            _wrapPanel.Children.Remove(fuc);
            UpLoadFilesDto ulfd = upFileList.SingleOrDefault(m => m.fileGuid == fuc.Tag.ToString());
            if (ulfd != null)
            {
                upFileList.Remove(ulfd);
            }
        }

        private void ThreadUploadFile(UpLoadFilesDto upDtos)
        {
            updateFailMessage(upDtos.FailOrSucess, "");
            //BackgroundWorker backGround = sender as BackgroundWorker;
            UpLoadFilesDto upDto = upDtos;
            SendCutImageDto scid = new SendCutImageDto();
            scid.cmpcd = s_ctt.companyCode;
            scid.seId = s_ctt.sessionId;
            scid.file = upDto.localOrServerPath;
            scid.fileFileName = upDto.fileName;
            scid.progressId = upDto.progressId;
            scid.imgguid = upDto.fileImgGuid;
            scid.textguid = upDto.fileTextGuid;
            scid.filesize = upDto.fileSize;
            scid.fileFileExtendName = upDto.fileExtendName;
            //2017-03-06 添加
            scid.messageId = upDto.messageId;
            //2017-03-06 添加
            //scid.back = backGround;
            scid.imgeTipId = upDto.imageTipId;
            scid.FailOrSucess = upDto.FailOrSucess;
            scid.imageSendingId = upDto.imageSendingId;
            scid.from = upDto.from;
            scid.ImgOrFileArg = upDto.ImgOrFileArg;
            FileUploadMutis(scid);
        }
        private void BackFileUpload_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backGround = sender as BackgroundWorker;
            UpLoadFilesDto upDto = e.Argument as UpLoadFilesDto;
            SendCutImageDto scid = new SendCutImageDto();
            scid.cmpcd = s_ctt.companyCode;
            scid.seId = s_ctt.sessionId;
            scid.file = upDto.localOrServerPath;
            scid.fileFileName = upDto.fileName;
            scid.progressId = upDto.progressId;
            scid.imgguid = upDto.fileImgGuid;
            scid.textguid = upDto.fileTextGuid;
            scid.filesize = upDto.fileSize;
            scid.fileFileExtendName = upDto.fileExtendName;
            //2017-03-06 添加
            scid.messageId = upDto.messageId;
            //2017-03-06 添加
            scid.back = backGround;
            FileUploadMutis(scid);
        }

        private void BackFileUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        #endregion
        #region 截图事件
        /// <summary>
        /// 
        /// </summary>
        public ICommand btnCutImageCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    try
                    {
                        #region 修改后
                        msgEditAssistant.CutImage();
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkViewModel_btnCutImageCommand]" + ex.Message + ex.StackTrace);
                    }
                });
            }
        }
        public ICommand textBlockCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    TextShowReceiveMsg = "";
                    TextShowRowHeight = "0";
                    //滚动条置底
                    chromiumWebBrowser.EvaluateScriptAsync("setscross();");
                });
            }
        }

        private void Back_DoWork(object sender, DoWorkEventArgs e)
        {
            ReturnCutImageDto rcidReturn = null;
            var sendmsg = e.Argument as AntSdkSendFileInput;
            if (sendmsg == null)
                return;
            try
            {
                var errorCode = 0;
                string strError = string.Empty;
                rcidReturn = new ReturnCutImageDto();
                //TODO:AntSdk_Modify
                //DODO:AntSdk_Modify
                var fileOutput = AntSdkService.FileUpload(sendmsg, ref errorCode, ref strError);
                if (fileOutput != null)
                {
                    //rcidReturn = (new HttpService()).FileUpload<ReturnCutImageDto>(sendmsg);
                    rcidReturn.FailOrSucess = sendmsg.FailOrSucess;
                    rcidReturn.prePath = sendmsg.file;
                    rcidReturn.messageId = sendmsg.messageId;
                    rcidReturn.imagedTipId = sendmsg.imgeTipId;
                    rcidReturn.imageSendingId = sendmsg.imageSendingId;

                    rcidReturn.isState = "1";
                    rcidReturn.fileUrl = fileOutput.dowmnloadUrl;
                    rcidReturn.imgSize = fileOutput.fileSize;
                    rcidReturn.preChatIndex = sendmsg.preChatIndex;
                    rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
                    rcidReturn.isSucessOrFail = sendmsg.FailOrSucess.isSucessOrFail;
                    e.Result = rcidReturn;
                }
                else
                {
                    rcidReturn = new ReturnCutImageDto();
                    rcidReturn.FailOrSucess = sendmsg.FailOrSucess;
                    rcidReturn.prePath = sendmsg.file;
                    rcidReturn.messageId = sendmsg.messageId;
                    rcidReturn.imagedTipId = sendmsg.imgeTipId;
                    rcidReturn.imageSendingId = sendmsg.imageSendingId;
                    rcidReturn.isState = "0";
                    rcidReturn.preChatIndex = sendmsg.preChatIndex;
                    rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
                    rcidReturn.isSucessOrFail = sendmsg.FailOrSucess.isSucessOrFail;
                    e.Result = rcidReturn;
                }

            }
            catch (Exception ex)
            {
                rcidReturn = new ReturnCutImageDto();
                rcidReturn.FailOrSucess = sendmsg.FailOrSucess;
                rcidReturn.prePath = sendmsg.file;
                rcidReturn.messageId = sendmsg.messageId;
                rcidReturn.imagedTipId = sendmsg.imgeTipId;
                rcidReturn.imageSendingId = sendmsg.imageSendingId;
                rcidReturn.isState = "0";
                rcidReturn.preChatIndex = sendmsg.preChatIndex;
                rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
                rcidReturn.isSucessOrFail = sendmsg.FailOrSucess.isSucessOrFail;
                e.Result = rcidReturn;
                LogHelper.WriteError("[TalkViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }

        private void Back_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var rcidReturn = e.Result as ReturnCutImageDto;
                if (rcidReturn.isState.ToString() == "0")
                {
                    //状态显示
                    //this.chromiumWebBrowser.ExecuteScriptAsync("","");
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                    //发送失败回调sessionList提示显示
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                    updateFailMessage(rcidReturn.FailOrSucess, "");
                    //System.Windows.Forms.MessageBox.Show("上传失败!");
                    //#region 发送成功 插入数据
                    //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                    //sm_ctt.MTP = "2";
                    //sm_ctt.chatIndex = Guid.NewGuid().ToString().Replace("-", "");
                    //sm_ctt.sendsucessorfail = 1;
                    //ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                    //#endregion
                }
                else
                {

                    var message = sender as BackgroundWorker;
                    //var rcidReturn = e.Result as ReturnCutImageDto;
                    SendImageDto sid = new SendImageDto();
                    sid.picUrl = rcidReturn.fileUrl;
                    sid.thumbnailUrl = rcidReturn.thumbUrl;
                    sid.imgSize = rcidReturn.imgSize;
                    string imgJson = JsonConvert.SerializeObject(sid);

                    SendMessage_ctt smt = new SendMessage_ctt();

                    SendMessageDto smg = new SendMessageDto();

                    smt.MsgType = AntSdkMsgType.ChatMsgPicture;
                    smt.content = imgJson;
                    smt.companyCode = s_ctt.companyCode;
                    smt.messageId = rcidReturn.messageId;


                    OnceSendImage sendImage = null;
                    if (rcidReturn.isOnceSendMsg)
                    {
                        if (rcidReturn.FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                        {
                            smt.flag = 1;
                        }
                        else
                        {
                            smt.flag = 0;
                        }
                        sendImage = OnceSendMessage.OneToOne.OnceMsgList.SingleOrDefault(m => m.Key == rcidReturn.messageId).Value as OnceSendImage;
                        smt.sendUserId = sendImage.ctt.sendUserId;
                        smt.sessionId = sendImage.ctt.sessionId;
                        smt.targetId = sendImage.ctt.targetId;
                    }
                    else
                    {
                        smt.sendUserId = s_ctt.sendUserId;
                        smt.sessionId = s_ctt.sessionId;
                        smt.targetId = s_ctt.targetId;
                    }
                    if (rcidReturn.FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                    {
                        rcidReturn.FailOrSucess.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                        smt.flag = 1;
                    }
                    else
                    {
                        rcidReturn.FailOrSucess.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                    }
                    smg.ctt = smt;


                    string imgError = "";
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.Picture imgMsg = new AntSdkChatMsg.Picture();
                    AntSdkChatMsg.Picture_content pictureContent = new AntSdkChatMsg.Picture_content();
                    pictureContent.picUrl = rcidReturn.fileUrl;
                    imgMsg.messageId = rcidReturn.messageId;
                    imgMsg.content = pictureContent;
                    imgMsg.MsgType = AntSdkMsgType.ChatMsgPicture;

                    if (user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                    {
                        imgMsg.flag = 0;
                    }
                    else
                    {
                        imgMsg.flag = smt.flag;
                    }
                    if (rcidReturn.isOnceSendMsg)
                    {
                        imgMsg.sendUserId = sendImage.ctt.sendUserId;
                        imgMsg.sessionId = sendImage.ctt.sessionId;
                        imgMsg.targetId = sendImage.ctt.targetId;
                    }
                    else
                    {
                        imgMsg.sendUserId = s_ctt.sendUserId;
                        imgMsg.sessionId = s_ctt.sessionId;
                        imgMsg.targetId = s_ctt.targetId;
                    }
                    imgMsg.chatType = (int)AntSdkchatType.Point;
                    SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(rcidReturn.FailOrSucess.obj as MessageStateArg));
                    //var result = false;
                    var isResult = false;
                    if (rcidReturn.isOnceSendMsg == false)
                    {
                        #region 发送成功 插入数据
                        //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(imgMsg));
                        ////TODO:AntSdk_Modify
                        ////sm_ctt.MTP = "2";
                        //sm_ctt.sourceContent = JsonConvert.SerializeObject(pictureContent);
                        //sm_ctt.chatIndex = rcidReturn.preChatIndex.ToString();
                        //sm_ctt.sendsucessorfail = 0;
                        //sm_ctt.SENDORRECEIVE = "1";
                        //sm_ctt.uploadOrDownPath = rcidReturn.prePath;
                        //if (_isShowBurn == "Collapsed")
                        //{
                        //    sm_ctt.flag = 1;
                        //}
                        #endregion
                        //result = ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                        //if (result)
                        //{

                        isResult = AntSdkService.SdkPublishChatMsg(imgMsg, ref imgError);
                        if (isResult)
                        {
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.notBurn, PointOrGroupFrom.PointOrGroup.Point, rcidReturn.messageId, imgJson);
                            //发送自动回复消息
                            SendAutoReplyMessage();
                            ////更改发送状态
                            //T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                            //t_chat.UpdateSendMsgState(rcidReturn.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                            //updateFailMessage(rcidReturn.FailOrSucess, s_ctt.targetId);
                        }
                        else
                        {
                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            //updateFailMessage(rcidReturn.FailOrSucess, s_ctt.targetId);
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                            //PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                        }
                        //}
                    }
                    else
                    {
                        if (rcidReturn.isOnceSendMsg == true && rcidReturn.FailOrSucess.isSucessOrFail == "1")
                        {
                            isResult = AntSdkService.SdkPublishChatMsg(imgMsg, ref imgError);
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.notBurn, PointOrGroupFrom.PointOrGroup.Point, rcidReturn.messageId, imgJson);
                        }
                        else
                        {
                            isResult = AntSdkService.SdkRePublishChatMsg(imgMsg, ref imgError);
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.notBurn, PointOrGroupFrom.PointOrGroup.Point, rcidReturn.messageId, imgJson);
                        }
                    }
                    //back.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_Back_RunWorkerCompleted]" + ex.Message + ex.StackTrace);
            }
        }
        #endregion
        #region 表情显示

        public ICommand btnShowPopupCommand
        {
            get
            {
                return new DelegateCommand<Popup>((popup) =>
                {
                    try
                    {
                        msgEditAssistant.ShowPopupWin(popup);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkViewModel_btnShowPopupCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }
        #endregion

        #region 录制语音
        /// <summary>
        /// 打开录制语音
        /// </summary>
        public ICommand BtnSoundCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    if (!AntSdkService.AntSdkIsConnected)
                    {
                        MessageBoxWindow.Show("提示", "网络连接已断开,无法发送语音！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    if (SoundRecordShowHeight == "0")
                    {
                        //检测音频设备
                        var errMsg = string.Empty;
                        if (!SoundHelper.CheckDeviceAvailable(ref errMsg))
                        {
                            MessageBoxWindow.Show("提示", errMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                            return;
                        }
                        SoundProValue = 0;
                        SoundRecordTiming();
                        SoundRecordShowHeight = "32";
                        SoundHelper.StartRecord(false);
                    }
                    else
                    {
                        var soundFilePath = SoundHelper.StopRecord();
                        if (!string.IsNullOrEmpty(soundFilePath))
                        {
                            if (File.Exists(soundFilePath))
                            {
                                File.Delete(soundFilePath);
                            }
                        }
                        SoundRecordShowHeight = "0";
                        _soundRecordTimer?.Stop();
                        SoundProValue = 0;
                    }
                });
            }
        }
        /// <summary>
        /// 发送语音
        /// </summary>
        public ICommand SendSoundCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    var soundFilePath = SoundHelper.StopRecord();
                    if (!AntSdkService.AntSdkIsConnected)
                    {
                        if (!string.IsNullOrEmpty(soundFilePath))
                        {
                            if (File.Exists(soundFilePath))
                            {
                                File.Delete(soundFilePath);
                            }
                        }
                        SoundRecordShowHeight = "0";
                        _soundRecordTimer?.Stop();
                        SoundProValue = 0;
                        MessageBoxWindow.Show("提示", "网络连接已断开,无法发送语音！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    if (!string.IsNullOrEmpty(soundFilePath))
                    {
                        SendSound(soundFilePath, null);
                    }
                    SoundRecordShowHeight = "0";
                    _soundRecordTimer?.Stop();
                    SoundProValue = 0;
                });
            }
        }
        /// <summary>
        /// 取消发送语音
        /// </summary>
        public ICommand CancelSoundCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    var soundFilePath = SoundHelper.StopRecord();
                    if (!string.IsNullOrEmpty(soundFilePath))
                    {
                        if (File.Exists(soundFilePath))
                        {
                            File.Delete(soundFilePath);
                        }
                    }
                    SoundRecordShowHeight = "0";
                    _soundRecordTimer?.Stop();
                    SoundProValue = 0;
                });
            }
        }
        #region 录音计时
        DispatcherTimer _soundRecordTimer = null;
        private void SoundRecordTiming()
        {
            _soundRecordTimer = null;
            _soundRecordTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _soundRecordTimer.Tick += soundRecordTimer_Tick;
            _soundRecordTimer.Start();
        }
        private void soundRecordTimer_Tick(object sender, EventArgs e)
        {
            SoundProValue += 6;
            if (SoundProValue == 360)
            {
                _soundRecordTimer?.Stop();
                var soundFilePath = SoundHelper.StopRecord();
                if (!string.IsNullOrEmpty(soundFilePath))
                {
                    SendSound(soundFilePath, null);
                }
                SoundRecordShowHeight = "0";
                SoundProValue = 0;
            }

        }
        #endregion

        private BackgroundWorker AudioBack;

        /// <summary>
        /// 发送语音方法
        /// filePath==Null 则表示为消息上传失败，需要重新上传再发送
        /// 否则为第一次发送
        /// </summary>
        /// <param name="filePath">本地音频文件</param>
        /// <param name="sendDto">语音文件构造</param>
        private void SendSound(string filePath, SendCutImageDto sendDto)
        {
            var isSendAgain = string.IsNullOrEmpty(filePath);
            AudioBack?.CancelAsync();
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId,
                this.s_ctt.sessionId);
            var file = isSendAgain ? sendDto.prePaths : filePath;
            var duration = int.Parse(SoundHelper.GetMp3Time(file));
            duration = duration > 60 ? 60 : duration;
            var fileInput = new AntSdkSendFileInput
            {
                cmpcd = s_ctt.companyCode,
                messageId = isSendAgain ? sendDto.messageId : PublicTalkMothed.timeStampAndRandom(),
                file = file,
                preChatIndex = maxChatindex,
                seId = s_ctt.sessionId,
                filesize = duration.ToString(),
                fileFileName = Path.GetFileNameWithoutExtension(file)
            };
            var failMessage = new AntSdkFailOrSucessMessageDto
            {
                mtp = (int)AntSdkMsgType.ChatMsgAudio,
                content = "[语音]",
                preChatIndex = maxChatindex,
                sessionid = s_ctt.sessionId,
                lastDatetime = DateTime.Now.ToString(),
                IsBurnMsg =
                    _isShowBurn == "Visible" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId
                        ? AntSdkburnMsg.isBurnMsg.notBurn
                        : AntSdkburnMsg.isBurnMsg.yesBurn,
                IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending
            };
            fileInput.FailOrSucess = failMessage;
            updateFailMessage(fileInput.FailOrSucess, s_ctt.targetId);

            #region 插入数据 此时保留的是本地路径 用于发送失败重发

            if (isSendAgain) //重发 更新数据库
            {

            }
            else //第一次发送 写入数据库
            {
                AntSdkChatMsg.Audio audioMsg = new AntSdkChatMsg.Audio();
                AntSdkChatMsg.Audio_content audioContent = new AntSdkChatMsg.Audio_content
                {
                    duration = duration,
                    audioUrl = file
                };
                audioMsg.messageId = fileInput.messageId;
                audioMsg.content = audioContent;
                audioMsg.MsgType = AntSdkMsgType.ChatMsgAudio;
                audioMsg.flag = _isShowBurn == "Visible" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId
                    ? 0
                    : 1;
                audioMsg.sendUserId = s_ctt.sendUserId;
                audioMsg.sessionId = s_ctt.sessionId;
                audioMsg.targetId = s_ctt.targetId;
                audioMsg.chatType = (int)AntSdkchatType.Point;
                AntSdkChatMsg.ChatBase sm_ctt =
                    JsonConvert.DeserializeObject<AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(audioMsg));
                sm_ctt.sourceContent = JsonConvert.SerializeObject(audioContent);
                sm_ctt.chatIndex = maxChatindex.ToString();
                sm_ctt.sendsucessorfail = 0;
                sm_ctt.SENDORRECEIVE = "1";
                sm_ctt.uploadOrDownPath = file;
                sm_ctt.flag = audioMsg.flag;
                ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
            }

            #endregion

            AudioBack = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            AudioBack.RunWorkerCompleted += AudioBack_RunWorkerCompleted;
            ;
            AudioBack.DoWork += AudioBack_DoWork;
            ;
            AudioBack.RunWorkerAsync(fileInput);
        }

        /// <summary>
        /// 上传语音Work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioBack_DoWork(object sender, DoWorkEventArgs e)
        {
            if (AudioBack.CancellationPending) { e.Cancel = true; return; }
            ReturnCutImageDto rcidReturn = new ReturnCutImageDto();
            var sendmsg = e.Argument as AntSdkSendFileInput;
            if (sendmsg == null)
                return;
            try
            {
                var duration = Convert.ToInt32(sendmsg.filesize);//音频时长 
                rcidReturn.FailOrSucess = sendmsg.FailOrSucess;
                rcidReturn.prePath = sendmsg.file;
                rcidReturn.messageId = sendmsg.messageId;
                rcidReturn.imagedTipId = sendmsg.imgeTipId;
                rcidReturn.imageSendingId = sendmsg.imageSendingId;
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                rcidReturn.imagedTipId = imageTipId;
                rcidReturn.imageSendingId = "sending" + imageTipId;
                if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(rcidReturn.messageId))
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId[rcidReturn.messageId] = rcidReturn.imageSendingId;
                }
                else
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(rcidReturn.messageId, rcidReturn.imageSendingId);
                }
                #region 消息状态监控
                var arg = new MessageStateArg
                {
                    isBurn = _isShowBurn == "Visible" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId ? AntSdkburnMsg.isBurnMsg.notBurn : AntSdkburnMsg.isBurnMsg.yesBurn,
                    isGroup = false,
                    MessageId = rcidReturn.messageId,
                    SessionId = s_ctt.sessionId,
                    WebBrowser = chromiumWebBrowser,
                    SendIngId = rcidReturn.imageSendingId,
                    RepeatId = rcidReturn.imagedTipId
                };
                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == rcidReturn.messageId);
                if (IsHave != null)
                {
                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                }
                SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                #endregion
                var dt = DateTime.Now;
                if (IsBurnState)
                    PublicTalkMothed.rightSendVoiceBurn(chromiumWebBrowser, dt, rcidReturn, duration);
                else
                {
                    PublicTalkMothed.RightShowSendVoice(chromiumWebBrowser, dt, rcidReturn, duration);
                }
                #region 滚动条置底
                var sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                #endregion

                var errorCode = 0;
                string strError = string.Empty;
                var fileOutput = AntSdkService.FileUpload(sendmsg, ref errorCode, ref strError);
                if (fileOutput != null)
                {
                    rcidReturn.isState = "1";
                    rcidReturn.fileUrl = fileOutput.dowmnloadUrl;
                    rcidReturn.imgSize = fileOutput.fileSize;//音频大小
                    e.Result = rcidReturn;
                }
            }
            catch (Exception ex)
            {
                rcidReturn = new ReturnCutImageDto
                {
                    FailOrSucess = sendmsg.FailOrSucess,
                    prePath = sendmsg.file,
                    messageId = sendmsg.messageId,
                    isState = "0"
                };
                e.Result = rcidReturn;
                LogHelper.WriteError("[TalkGroupViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 上传语音完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioBack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var rcidReturn = e.Result as ReturnCutImageDto;
                if (rcidReturn == null) return;
                if (rcidReturn.isState == "0")//上传失败
                {
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                    updateFailMessage(rcidReturn.FailOrSucess, "");
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                }
                else
                {
                    var duration = int.Parse(SoundHelper.GetMp3Time(rcidReturn.prePath));
                    duration = duration > 60 ? 60 : duration;
                    AntSdkChatMsg.Audio audioMsg = new AntSdkChatMsg.Audio();
                    AntSdkChatMsg.Audio_content audioContent = new AntSdkChatMsg.Audio_content
                    {
                        duration = duration,
                        audioUrl = rcidReturn.fileUrl
                    };
                    audioMsg.messageId = rcidReturn.messageId;
                    audioMsg.content = audioContent;
                    audioMsg.MsgType = AntSdkMsgType.ChatMsgAudio;
                    audioMsg.flag = _isShowBurn == "Visible" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId ? 0 : 1;
                    audioMsg.sendUserId = s_ctt.sendUserId;
                    audioMsg.sessionId = s_ctt.sessionId;
                    audioMsg.targetId = s_ctt.targetId;
                    audioMsg.chatType = (int)AntSdkchatType.Point;
                    #region 异步发送方法
                    ThreadPool.QueueUserWorkItem(
                        m =>
                            SendVoiceMethod(audioMsg, rcidReturn));
                    #endregion
                    AudioBack.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_Back_RunWorkerCompleted]" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 异步发送语音方法
        /// </summary>
        /// <param name="audioMsg"></param>
        /// <param name="rcidReturn"></param>
        private void SendVoiceMethod(AntSdkChatMsg.Audio audioMsg, ReturnCutImageDto rcidReturn)
        {
            var imgError = "";
            var isResult = AntSdkService.SdkPublishChatMsg(audioMsg, ref imgError);
            if (isResult)
            {
                //发送自动回复消息
                SendAutoReplyMessage();
                if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(rcidReturn.messageId))
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId[rcidReturn.messageId] = rcidReturn.imageSendingId;
                }
                else
                {
                    SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(rcidReturn.messageId, rcidReturn.imageSendingId);
                }
                #region 发送成功 更新Content
                var sourceContent = JsonConvert.SerializeObject(audioMsg.content);
                var t_chat = new T_Chat_MessageDAL();
                t_chat.UpdateSendVoiceMsgState(rcidReturn.messageId, sourceContent,
                    AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                #endregion
                rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
            }
            else
            {
                rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                updateFailMessage(rcidReturn.FailOrSucess, s_ctt.targetId);
                PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
            }
        }
        /// <summary>
        /// 重新发送语音方法
        /// </summary>
        /// <param name="dto"></param>
        private void SendVoiceAgain(SendCutImageDto dto)
        {
            var maxChatindex = 0;
            //查询数据库最大chatindex
            var t_chat = new T_Chat_MessageDAL();
            maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
            var failMessage = new AntSdkFailOrSucessMessageDto
            {
                mtp = (int)AntSdkMsgType.ChatMsgAudio,
                content = "[语音]",
                preChatIndex = maxChatindex,
                sessionid = s_ctt.sessionId,
                lastDatetime = DateTime.Now.ToString(),
                IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending,
                IsBurnMsg = dto.isBurn ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn
            };
            var duration = int.Parse(SoundHelper.GetMp3Time(dto.file));
            duration = duration > 60 ? 60 : duration;
            AntSdkChatMsg.Audio audioMsg = new AntSdkChatMsg.Audio();
            AntSdkChatMsg.Audio_content audioContent = new AntSdkChatMsg.Audio_content
            {
                duration = duration,
                audioUrl = dto.prePaths
            };
            audioMsg.messageId = dto.messageId;
            audioMsg.content = audioContent;
            audioMsg.MsgType = AntSdkMsgType.ChatMsgAudio;
            audioMsg.flag = dto.isBurn ? 1 : 0;
            audioMsg.sendUserId = s_ctt.sendUserId;
            audioMsg.sessionId = s_ctt.sessionId;
            audioMsg.targetId = s_ctt.targetId;
            audioMsg.chatType = (int)AntSdkchatType.Point;
            var rcidReturn = new ReturnCutImageDto
            {
                FailOrSucess = failMessage,
                messageId = dto.messageId,
                isState = "1",
                fileUrl = dto.prePaths,
                prePath = dto.file
            };
            var imageTipId = Guid.NewGuid().ToString().Replace("-", "");
            rcidReturn.imagedTipId = imageTipId;
            rcidReturn.imageSendingId = "sending" + imageTipId;
            var dt = DateTime.Now;
            if (dto.isBurn)
                PublicTalkMothed.rightSendVoiceBurn(chromiumWebBrowser, dt, rcidReturn, duration);
            else
            {
                PublicTalkMothed.RightShowSendVoice(chromiumWebBrowser, dt, rcidReturn, duration);
            }
            #region 消息状态监控
            var arg = new MessageStateArg
            {
                isBurn = AntSdkburnMsg.isBurnMsg.notBurn,
                isGroup = false,
                MessageId = rcidReturn.messageId,
                SessionId = s_ctt.sessionId,
                WebBrowser = chromiumWebBrowser,
                SendIngId = rcidReturn.imageSendingId,
                RepeatId = rcidReturn.imagedTipId
            };
            var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == rcidReturn.messageId);
            if (IsHave != null)
            {
                SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
            }
            SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
            #endregion
            #region 异步发送方法
            ThreadPool.QueueUserWorkItem(
                m =>
                    SendVoiceMethod(audioMsg, rcidReturn));
            #endregion
        }

        #endregion


        #region 语音电话
        /// <summary>
        /// 用来处理窗体位置
        /// </summary>
        public event EventHandler AudioClickEvent;
        public void OnMouseDoubleClickEvent(object audioView)
        {
            AudioClickEvent?.Invoke(audioView, EventArgs.Empty);
        }
        /// <summary>
        /// 发起语音电话邀请
        /// </summary>
        public ICommand BtnAudioCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    if (GlobalVariable.isAudioShow || GlobalVariable.isRequestShow) return;
                    if (!AntSdkService.AntSdkIsConnected)
                    {
                        MessageBoxWindow.Show("提示", "网络连接已断开,无法发送语音电话！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    //if (!AudioChat.IsInit)
                    //{
                    //    AudioChat.InitApi();
                    //    //重连一次
                    //}
                    if (!AudioChat.IsInit)
                    {
                        MessageBoxWindow.Show("提示", "初始化失败，语音电话功能不可用！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    //检查设备
                    string errMsg = string.Empty;
                    if (!AudioChat.CheckDevices(ref errMsg))
                    {
                        MessageBoxWindow.Show("提示", errMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    ShowAudioView(user, GlobalVariable.AudioSendOrReceive.Send);
                });
            }
        }
        /// <summary>
        /// 显示通话页面
        /// </summary>
        /// <param name="user">目标用户</param>
        /// <param name="sendOrReceive">发送or接收</param>
        private void ShowAudioView(AntSdkContact_User user, GlobalVariable.AudioSendOrReceive sendOrReceive)
        {
            var audioView = new AudioChatView();
            AudioChat.audioViewModel = new AudioChatViewModel(user, sendOrReceive, audioView.Close);
            AudioChat.audioViewModel.sessionId = this.s_ctt.sessionId;
            audioView.DataContext = AudioChat.audioViewModel;
            audioView.Show();
            GlobalVariable.isAudioShow = true;
            GlobalVariable.currentAudioUserId = user.userId;
            OnMouseDoubleClickEvent(audioView);
            //
        }
        #endregion

        public DispatcherTimer timer = new DispatcherTimer();
        /// <summary>
        /// 是否加载过 1加载过 0未加载过
        /// </summary>
        int defaultPage = 0;
        int counter = 0;
        private object obj = new object();
        private object timeObj = new object();
        private void Timer_Tick(object sender, EventArgs e)
        {
            lock (timeObj)
            {
                string add = "";
                counter++;
                try
                {
                    if (chromiumWebBrowser.Visibility != Visibility.Visible || chromiumWebBrowser.IsInitialized != true || chromiumWebBrowser.IsBrowserInitialized != true || chromiumWebBrowser.GetMainFrame().Url == "")
                    {
                        LogHelper.WriteWarn("初始化失败:");
                        return;
                    }
                    Task<JavascriptResponse> task = this._chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(PublicTalkMothed.add());
                    task.Wait();
                    if (task.Result.Success && task.Result.Result.ToString() == "2")
                    {
                        timer.Stop();
                        if (OnceSendMessage.OneToOne.CefList.ContainsKey(s_ctt.sessionId))
                        {
                            OnceSendMessage.OneToOne.CefList.Remove(s_ctt.sessionId);
                        }
                        OnceSendMessage.OneToOne.CefList.Add(s_ctt.sessionId, _chromiumWebBrowser);

                        counter = 0;
                        add = task.Result.Result.ToString();
                        LogHelper.WriteDebug("TalkViewModel_Timer_Tick时间Success:" + DateTime.Now.ToLongTimeString());
                        #region 未读消息解析
                        string checkId = "";
                        if (this.user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId && (FirstPageData == null || !FirstPageData.Any()))
                            PublicTalkMothed.AutoReplyMessage(this.chromiumWebBrowser, this.user, GlobalVariable.RevocationPrompt.RobotFirstMsg, this.s_ctt.content);
                        #region 默认显示一页消息逻辑
                        if (defaultPage == 0)
                        {
                            var tempFirstPageData = FirstPageData?.Count > 0 ? FirstPageData.ToList() : null;
                            if (OnlineReceiveMessageList?.Count > 0 && tempFirstPageData?.Count > 0)
                            {
                                var loseChatMsgLst = OnlineReceiveMessageList.Where(m => !tempFirstPageData.Exists(n => n.messageId == m.messageId && n.chatIndex == m.chatIndex)).ToList();
                                if (loseChatMsgLst.Count > 0)
                                {
                                    tempFirstPageData.AddRange(loseChatMsgLst);
                                    tempFirstPageData = tempFirstPageData.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                                }
                            }
                            if (tempFirstPageData?.Count > 0)
                            {
                                StringBuilder sbLeft = new StringBuilder();
                                string topStr = PublicTalkMothed.topString();
                                sbLeft.AppendLine(topStr);
                                foreach (var list in tempFirstPageData)
                                {
                                    checkId = list.messageId;
                                    if (IsRobot == true)
                                    {
                                        list.IsRobot = true;
                                    }
                                    lock (obj)
                                    {
                                        #region 2014-06-14修改 New
                                        List<string> imageId = new List<string>();
                                        imageId.Clear();
                                        switch (list.MsgType)
                                        {
                                            case AntSdkMsgType.ChatMsgMixMessage:
                                                List<MixMessageObjDto> picMix = null;
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    picMix = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent);
                                                    foreach (var listpic in picMix)
                                                    {
                                                        if (listpic.type == "1002")
                                                        {
                                                            PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(listpic.content?.ToString());
                                                            string guid = Guid.NewGuid().ToString();
                                                            string imgId = "RL" + guid;
                                                            imageId.Add(imgId);
                                                        }
                                                    }
                                                    #region 图文混合
                                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    int j = 0;
                                                    foreach (var ilist in receive)
                                                    {
                                                        PictureDto contents = JsonConvert.DeserializeObject<PictureDto>(ilist.content?.ToString());
                                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, imageId[j], contents.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                        j++;
                                                    }
                                                    #endregion
                                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                        OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, picMix);
                                                }
                                                else
                                                {
                                                    #region 图文混合
                                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    foreach (var ilist in receive)
                                                    {
                                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                                        imageId.Add(imgId);
                                                        PictureDto content = JsonConvert.DeserializeObject<PictureDto>(ilist.content?.ToString());
                                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, imgId, content.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    }
                                                    #endregion
                                                }
                                                break;
                                            case AntSdkMsgType.ChatMsgText:
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Point;
                                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                        OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, list);
                                                }
                                                break;
                                            case AntSdkMsgType.ChatMsgPicture:
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    OnceSendImage sendImage = new OnceSendImage();
                                                    sendImage.ctt = this.s_ctt;
                                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                        OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, sendImage);
                                                }
                                                if (list.flag == 1)
                                                {
                                                    burnMsg.IsReadImg bImg;
                                                    burnMsg.IsEffective isEffective;
                                                    if (list.readtime != "")
                                                    {
                                                        bImg = burnMsg.IsReadImg.read;
                                                        isEffective = burnMsg.IsEffective.effective;
                                                    }
                                                    else
                                                    {
                                                        bImg = burnMsg.IsReadImg.notRead;
                                                        isEffective = burnMsg.IsEffective.NotEffective;
                                                    }
                                                    SendImageDto rimgDto =
                                                        JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:list.content*/
                                                            list.sourceContent);
                                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, list.messageId, rimgDto.picUrl, "",
                                                        burnMsg.isBurnMsg.yesBurn, bImg, isEffective);
                                                }
                                                else
                                                {
                                                    burnMsg.IsReadImg bImg;
                                                    if (list.readtime != "")
                                                    {
                                                        bImg = burnMsg.IsReadImg.read;
                                                    }
                                                    else
                                                    {
                                                        bImg = burnMsg.IsReadImg.notRead;
                                                    }
                                                    SendImageDto rimgDto =
                                                        JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:list.content*/
                                                            list.sourceContent);
                                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, list.messageId, rimgDto.picUrl, "",
                                                        burnMsg.isBurnMsg.notBurn, bImg, burnMsg.IsEffective.UnKnow);
                                                }
                                                break;
                                            case AntSdkMsgType.ChatMsgFile:
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Point;
                                                    OnceSendFile sendFile = new OnceSendFile();
                                                    sendFile.ctt = this.s_ctt;
                                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(list.messageId))
                                                        OnceSendMessage.OneToOne.OnceMsgList.Add(list.messageId, sendFile);
                                                }
                                                break;
                                        }
                                        listChatIndex.Add(list.messageId);
                                        string oneStr = PublicTalkMothed.LeftOneToOneShowMessage(list, user, imageId);
                                        sbLeft.AppendLine(oneStr);

                                        #endregion
                                    }
                                }
                                string endStr = PublicTalkMothed.endString();
                                sbLeft.AppendLine(endStr);
                                bool IsSucess = false;
                                Task<JavascriptResponse> result = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                result.Wait();
                                if (result.Result.Success)
                                {
                                    var isExist = chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                                    isExist.Wait();
                                    if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                                    {
                                        IsSucess = false;
                                    }
                                    else
                                    {
                                        IsSucess = true;
                                    }
                                }
                                while (IsSucess == false)
                                {
                                    Task<JavascriptResponse> resultWhile = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                    resultWhile.Wait();
                                    if (resultWhile.Result.Success)
                                    {
                                        var isExist = chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                                        isExist.Wait();
                                        if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                                        {
                                            IsSucess = false;
                                        }
                                        else
                                        {
                                            IsSucess = true;
                                        }
                                    }
                                    #region 2017-12-04 屏蔽
                                    //Thread.Sleep(50);
                                    //LogHelper.WriteWarn("[TalkViewModel-FirstPageData:]" + sbLeft.ToString());
                                    //Task<JavascriptResponse> results2 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                    //results2.Wait();
                                    //if (results2.Result.Success)
                                    //{
                                    //    LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第二次执行成功---------------------");
                                    //}
                                    //else
                                    //{
                                    //    LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第二次执行失败---------------------");
                                    //    Task<JavascriptResponse> results3 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                    //    results3.Wait();
                                    //    Thread.Sleep(100);
                                    //    if (results3.Result.Success)
                                    //    {
                                    //        LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第三次次执行成功---------------------");
                                    //    }
                                    //    else
                                    //    {
                                    //        LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第三次执行失败---------------------");
                                    //    }
                                    //}
                                    #endregion
                                }
                                defaultPage = 1;
                                FirstPageData = null;
                                ChatMsgLst.Clear();
                                result.Dispose();
                                this._chromiumWebBrowser.ExecuteScriptAsync("setscross();");
                            }
                            else
                            {
                                if (ChatMsgLst?.Count > 0)
                                    ShowMsgData();
                            }
                        }

                    }
                    #endregion
                    unreadCount = 0;
                    if (_windowHelper == null)
                    {
                        _windowHelper = new WindowHelper(s_ctt.sessionId);
                        _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived +=
                            LocalMessageHelper_InstantMessageHasBeenReceived;
                        if (_isChanageWindow)
                            WindowMonitor.ChanageWindowHelper(_windowHelper);
                    }
                    #endregion
                    if (counter == 10)
                    {
                        if (add != "2")
                        {
                            timer.Stop();
                            counter = 0;
                        }
                        LogHelper.WriteDebug("TalkViewModel_Timer_Tick超时:" + DateTime.Now.ToLongTimeString());
                    }
                }
                catch (Exception ex)
                {
                    if (this._richTextBox != null)
                    {
                        this._richTextBox.IsReadOnly = false;
                        this._richTextBox.Focus();
                    }
                    if (_windowHelper == null)
                    {
                        _windowHelper = new WindowHelper(s_ctt.sessionId);
                        _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived +=
                            LocalMessageHelper_InstantMessageHasBeenReceived;
                        if (_isChanageWindow)
                            WindowMonitor.ChanageWindowHelper(_windowHelper);
                    }
                    LogHelper.WriteError("TalkViewModel[Timer_Tick]" + ex.Message + ex.StackTrace);
                }
                if (this._richTextBox != null)
                {
                    this._richTextBox.IsReadOnly = false;
                    this._richTextBox.Focus();
                }
            }
        }
        public class CallBackSelectDirecoryFile
        {
            public ChromiumWebBrowsers chromiumWebBrowser;
            public CallBackSelectDirecoryFile(ChromiumWebBrowsers chromiumWebBrowser)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallBackSelectDirecoryFile]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            public void openDirecrory(string obj)
            {
                try
                {
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        if (string.IsNullOrEmpty(obj) || !File.Exists(obj))
                        {
                            MessageBoxWindow.Show("文件不存在！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                            return;
                        }
                        string first = obj.Replace(@"/", "\\");
                        string fileToSelect = first;
                        string args = string.Format("/Select, {0}", fileToSelect);
                        ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                        System.Diagnostics.Process.Start(pfi);
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_openDirecrory]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
        }
        public class CallbackValueById
        {
            public ChromiumWebBrowsers chromiumWebBrowser;
            public SendMessage_ctt s_ctt;
            public List<SendMsgStateMonitor> MessageStateMonitorList;
            public CallbackValueById(ChromiumWebBrowsers chromiumWebBrowser, SendMessage_ctt s_ctt, List<SendMsgStateMonitor> MessageStateMonitorList)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.s_ctt = s_ctt;
                    this.MessageStateMonitorList = MessageStateMonitorList;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallbackValueById]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            public void GetValueById(string obj)
            {
                try
                {
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        if (string.IsNullOrEmpty(obj) || !File.Exists(obj))
                        {
                            MessageBoxWindow.Show("文件不存在！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                            return;
                        }
                        string first = obj.Replace(@"/", "\\");
                        string fileToSelect = first;
                        System.Diagnostics.Process.Start(fileToSelect);
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_GetValueById]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private MediaPlayers player;
            /// <summary>
            /// 单人播放语音
            /// </summary>
            /// <param name="isRead"></param>
            /// <param name="audioUrl"></param>
            /// <param name="isShowGifId"></param>
            /// <param name="messageid"></param>
            /// <param name="isLeftOrRight">1为左边，0为右边</param>
            public void getVoiceParameter(bool isRead, string audioUrl, string isShowGifId, string messageid, string isLeftOrRight, string chatindex, string isBurn)
            {
                if (string.IsNullOrEmpty(audioUrl)) return;
                if (isBurn == "0")
                {
                    //删除
                    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                    t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode, AntSdkService.AntSdkLoginOutput.userId, messageid);
                    if (chatindex + "" != "")
                    {
                        SentBurnAfterReadReceipt(chatindex, messageid);
                    }
                }
                //更新数据库语音状态 0未读 1已读
                if (!isRead)
                {
                    if (isLeftOrRight == "0")
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.UpdateVoiceState(messageid);
                    }
                }
                try
                {
                    //语音播放和图片变化
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        player?.Stop();
                        if (player != null)
                        {
                            if (messageid != player.messageId)
                            {
                                PublicTalkMothed.swithStaticJpg(this.chromiumWebBrowser, player.IsImgPlayId, player.isLeftOrRight);
                            }
                        }
                        player = new MediaPlayers();
                        player.Open(new Uri(audioUrl, UriKind.RelativeOrAbsolute));
                        player.IsImgPlayId = isShowGifId;
                        player.messageId = messageid;
                        player.isLeftOrRight = isLeftOrRight;
                        player.Play();
                        player.MediaOpened += playerMediaOpened;
                        player.MediaEnded += playerMediaEnded;
                    }, DispatcherPriority.Background);

                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("语音播放失败:" + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
            public void SentBurnAfterReadReceipt(string chatIndex, string messageId)
            {
                if (this.s_ctt == null) return;
                BurnAfterReadReceipt receipt = new BurnAfterReadReceipt();
                receipt.ctt = new BurnAfterReadReceiptCtt();
                receipt.ctt.sendUserId = this.s_ctt.sendUserId;
                receipt.ctt.targetId = this.s_ctt.targetId;
                receipt.ctt.companyCode = GlobalVariable.CompanyCode;
                receipt.ctt.chatIndex = chatIndex;
                receipt.ctt.os = ((int)GlobalVariable.OSType.PC).ToString();
                receipt.ctt.sessionId = this.s_ctt.sessionId;
                receipt.ctt.content = null;
                string errMsg = string.Empty;
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                var burnRead = new AntSdkSendMsg.PointBurnReaded
                {
                    sendUserId = this.s_ctt.sendUserId,
                    targetId = this.s_ctt.targetId,
                    chatIndex = chatIndex,
                    os = (int)GlobalVariable.OSType.PC,
                    sessionId = this.s_ctt.sessionId,
                    MsgType = AntSdkMsgType.PointBurnReaded,
                    chatType = (int)AntSdkchatType.Point,
                    messageId = PublicTalkMothed.timeStampAndRandom(),
                    content = new AntSdkSendMsg.PointBurnReaded_content
                    {
                        readIndex = int.Parse(chatIndex),
                        //TODO:收到的那条阅后即焚消息的messageId
                        messageId = messageId
                    }
                };
                AntSdkService.SdkPublishPointBurnReadReceiptMsg(burnRead, ref errMsg);
            }
            /// <summary>
            /// 开始播放事件
            /// </summary>
            private void playerMediaOpened(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// 播放结束时间
            /// </summary>
            private void playerMediaEnded(object sender, EventArgs e)
            {
                MediaPlayers media = sender as MediaPlayers;
                PublicTalkMothed.swithStaticJpg(this.chromiumWebBrowser, media.IsImgPlayId, media.isLeftOrRight);
                player?.Close();
            }
            /// <summary>
            /// 重发、移除
            /// </summary>
            /// <param name="method"></param>
            /// <param name="messageId"></param>
            /// <param name="path"></param>
            public void sendMsgagen(string method, string messageId, string pathOrValue, string preValue)
            {
                try
                {
                    switch (method)
                    {
                        case "sendText":
                            #region 正常文本重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                //显示
                                string messageIds = messageId;
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string imageSendingId = "sending" + imageTipId;
                                DateTime dt = DateTime.Now;
                                PublicTalkMothed.rightSendText(chromiumWebBrowser, pathOrValue, messageId, imageTipId, imageSendingId, dt, preValue, false);

                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageIds;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                                if (IsHave != null)
                                {
                                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                                {
                                    SendMsgListPointMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                                }
                                else
                                {
                                    SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                                }
                                SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                #endregion

                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)GlobalVariable.MsgType.Text;
                                failMessage.content = preValue;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                DateTime dtNow = dt;
                                failMessage.lastDatetime = dt.ToString();
                                updateFailMessage(failMessage, "");
                                //发送
                                sendTextMsg(preValue.Replace("<br/>", "\r\n"), messageIds, imageTipId, imageSendingId, failMessage, true);
                            }));
                            #endregion
                            break;
                        case "sendBurnText":
                            #region 阅后即焚文本重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                //显示
                                string messageIds = messageId;
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string imageSendingId = "sending" + imageTipId;
                                DateTime dt = DateTime.Now;
                                PublicTalkMothed.rightSendTextBurn(chromiumWebBrowser, pathOrValue, messageIds, imageTipId, imageSendingId, dt, preValue);

                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageIds;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                                if (IsHave != null)
                                {
                                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                                {
                                    SendMsgListPointMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                                }
                                else
                                {
                                    SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                                }
                                SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                #endregion

                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)GlobalVariable.MsgType.Text;
                                failMessage.content = preValue;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                                DateTime dtNow = dt;
                                failMessage.lastDatetime = dt.ToString();
                                updateFailMessage(failMessage, "");
                                //发送
                                sendTextMsg(pathOrValue, messageIds, imageTipId, imageSendingId, failMessage, true);
                            }));
                            #endregion
                            break;
                        case "0":
                            #region 正常图片重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                                               {
                                                   //移除
                                                   PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);

                                                   //显示
                                                   Image lists = new Image();
                                                   lists.Source = new BitmapImage(new Uri(pathOrValue.Replace("file:///", "")));
                                                   string prePath = lists.Source.ToString().Replace(@"\", @"/").Replace("file:///", "");
                                                   string prePaths = prePath;
                                                   string fileFileName = System.IO.Path.GetFileNameWithoutExtension(prePaths);
                                                   string messageIds = messageId;
                                                   string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                                   string imageSendingId = "sending" + imageTipId;
                                                   DateTime dt = DateTime.Now;
                                                   PublicTalkMothed.rightSendImage(chromiumWebBrowser, lists, fileFileName, null, null, messageIds, imageTipId, imageSendingId, dt, false);

                                                   #region 消息状态监控
                                                   var result = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                                                   if (result != null)
                                                   {
                                                       SendMsgListPointMonitor.MessageStateMonitorList.Remove(result);
                                                   }
                                                   MessageStateArg arg = new MessageStateArg();
                                                   arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                                   arg.isGroup = false;
                                                   arg.MessageId = messageIds;
                                                   arg.SessionId = s_ctt.sessionId;
                                                   arg.WebBrowser = chromiumWebBrowser;
                                                   arg.SendIngId = imageSendingId;
                                                   arg.RepeatId = imageTipId;
                                                   //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                                   #endregion

                                                   //构造
                                                   AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                                   failMessage.mtp = (int)GlobalVariable.MsgType.Picture;
                                                   failMessage.content = preValue;
                                                   failMessage.sessionid = chromiumWebBrowser.sessionid;
                                                   failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                                   DateTime dtNow = dt;
                                                   failMessage.lastDatetime = dt.ToString();
                                                   failMessage.isOnceSendMsg = true;
                                                   failMessage.isSucessOrFail = preValue;
                                                   failMessage.obj = arg;
                                                   updateFailMessage(failMessage, "");
                                                   //上传
                                                   upLoadImageMsg(prePaths, fileFileName, messageIds, imageTipId, imageSendingId, failMessage);
                                               }));
                            #endregion
                            break;
                        case "sendVoice":
                            #region 正常语音重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                upLoadVoiceMsg(pathOrValue, messageId, preValue, false);
                            }));
                            #endregion
                            break;
                        case "sendBurnVoice":
                            #region 阅后即焚语音重发
                            Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                upLoadVoiceMsg(pathOrValue, messageId, preValue, true);
                            }));
                            #endregion
                            break;
                        case "2":
                            #region 阅后即焚图片重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                //消息构造
                                Image list = new Image();
                                list.Source = new BitmapImage(new Uri(pathOrValue.Replace("file:///", "")));
                                string burnprePath = list.Source.ToString().Replace(@"\", @"/");
                                string burnprePaths = burnprePath.Substring(8, burnprePath.Length - 8);
                                string burnfileFileName = System.IO.Path.GetFileNameWithoutExtension(burnprePath);
                                string burnmessageIds = messageId;
                                string burnimageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string burnimageSendingId = "sending" + burnimageTipId;
                                DateTime dt = DateTime.Now;
                                //显示
                                PublicTalkMothed.rightSendImageBurn(chromiumWebBrowser, list, burnfileFileName, null, null, burnmessageIds, burnimageTipId, burnimageSendingId, dt);
                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                arg.isGroup = false;
                                arg.MessageId = burnmessageIds;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = burnimageSendingId;
                                arg.RepeatId = burnimageTipId;
                                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == burnmessageIds);
                                if (IsHave != null)
                                {
                                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                #endregion
                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)GlobalVariable.MsgType.Picture;
                                failMessage.content = preValue;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                                DateTime dtNow = dt;
                                failMessage.lastDatetime = dt.ToString();
                                failMessage.isOnceSendMsg = true;
                                failMessage.obj = arg;
                                updateFailMessage(failMessage, "");
                                //上传
                                upLoadImageMsg(burnprePaths, burnfileFileName, burnmessageIds, burnimageTipId, burnimageSendingId, failMessage);
                            }));
                            #endregion
                            break;
                        case "1":
                            #region 正常文件上传重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                if (string.IsNullOrEmpty(pathOrValue))
                                {
                                    MessageBoxWindow.Show("文件不存在！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                    return;
                                }
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                //构造文件上传信息
                                UpLoadFilesDto upFileDto = new UpLoadFilesDto();

                                upFileDto.fileGuid = "wlc" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileName = pathOrValue.Substring((pathOrValue.LastIndexOf('/') + 1), pathOrValue.Length - 1 - pathOrValue.LastIndexOf('/'));

                                System.IO.FileInfo fileInfo = new FileInfo(pathOrValue);
                                //if (fileInfo.Length < 1024)
                                //{
                                //    upFileDto.fileSize = fileInfo.Length + "B";
                                //}
                                //if (fileInfo.Length > 1024)
                                //{
                                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024, 2) + "KB";
                                //}
                                //if (fileInfo.Length > 1024 * 1024)
                                //{
                                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 2) + "MB";
                                //}
                                upFileDto.fileSize = fileInfo.Length.ToString();
                                upFileDto.localOrServerPath = pathOrValue;
                                upFileDto.fileExtendName = pathOrValue.Substring((pathOrValue.LastIndexOf('.') + 1), pathOrValue.Length - 1 - pathOrValue.LastIndexOf('.'));

                                upFileDto.messageId = messageId;
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.imageTipId = imageTipId;

                                string fileSendingId = "sending" + imageTipId;
                                upFileDto.imageSendingId = fileSendingId;

                                DateTime dt = DateTime.Now;
                                upFileDto.DtTime = dt;

                                string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.progressId = progressId;

                                string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileImgGuid = showImgGuid;

                                string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileTextGuid = fileshowText;

                                string fileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                upFileDto.fileOpenguid = fileOpenguid;

                                string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                upFileDto.fileOpenDirectory = fileOpenDirectory;
                                upFileDto.cmpcd = GlobalVariable.CompanyCode;
                                upFileDto.seId = chromiumWebBrowser.sessionid;

                                //显示
                                PublicTalkMothed.rightSendFile(chromiumWebBrowser, upFileDto);

                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageId;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = fileSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                if (IsHave != null)
                                {
                                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                                #endregion
                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)GlobalVariable.MsgType.File;
                                failMessage.content = preValue;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                DateTime dtNow = dt;
                                failMessage.lastDatetime = dt.ToString();
                                upFileDto.FailOrSucess = failMessage;
                                upFileDto.from = AntSdkSendFrom.SendFrom.OnceSend;
                                upFileDto.imageSendingId = fileSendingId;
                                upFileDto.ImgOrFileArg = arg;
                                updateFailMessage(failMessage, "");
                                //重新上传文件
                                upLoadFileMsg(upFileDto);
                            }));
                            #endregion
                            break;
                        case "3":
                            #region 阅后即焚文件重发
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                                //构造文件上传信息
                                UpLoadFilesDto upFileDto = new UpLoadFilesDto();

                                upFileDto.fileGuid = "wlc" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileName = pathOrValue.Substring((pathOrValue.LastIndexOf('/') + 1), pathOrValue.Length - 1 - pathOrValue.LastIndexOf('/'));

                                System.IO.FileInfo fileInfo = new FileInfo(pathOrValue);
                                //if (fileInfo.Length < 1024)
                                //{
                                //    upFileDto.fileSize = fileInfo.Length + "B";
                                //}
                                //if (fileInfo.Length > 1024)
                                //{
                                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024, 2) + "KB";
                                //}
                                //if (fileInfo.Length > 1024 * 1024)
                                //{
                                //    upFileDto.fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 2) + "MB";
                                //}
                                upFileDto.fileSize = fileInfo.Length.ToString();
                                upFileDto.localOrServerPath = pathOrValue;
                                upFileDto.fileExtendName = pathOrValue.Substring((pathOrValue.LastIndexOf('.') + 1), pathOrValue.Length - 1 - pathOrValue.LastIndexOf('.'));

                                upFileDto.messageId = messageId;
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.imageTipId = imageTipId;

                                string fileSendingId = "sending" + imageTipId;
                                upFileDto.imageSendingId = fileSendingId;

                                DateTime dt = DateTime.Now;
                                upFileDto.DtTime = dt;

                                string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.progressId = progressId;

                                string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileImgGuid = showImgGuid;

                                string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                upFileDto.fileTextGuid = fileshowText;

                                string fileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                upFileDto.fileOpenguid = fileOpenguid;

                                string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                upFileDto.fileOpenDirectory = fileOpenDirectory;
                                upFileDto.cmpcd = GlobalVariable.CompanyCode;
                                upFileDto.seId = chromiumWebBrowser.sessionid;
                                //显示
                                PublicTalkMothed.rightSendFileBurn(chromiumWebBrowser, upFileDto);
                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageId;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = fileSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                if (IsHave != null)
                                {
                                    SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                #endregion
                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)GlobalVariable.MsgType.File;
                                failMessage.content = preValue;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                                DateTime dtNow = dt;
                                failMessage.lastDatetime = dt.ToString();
                                upFileDto.FailOrSucess = failMessage;
                                upFileDto.from = AntSdkSendFrom.SendFrom.OnceSend;
                                upFileDto.imageSendingId = fileSendingId;
                                upFileDto.ImgOrFileArg = arg;
                                updateFailMessage(failMessage, "");
                                //重新上传文件
                                upLoadFileMsg(upFileDto);
                            }));
                            #endregion
                            break;
                        case "sendMixPic":
                            #region 图文混合发送
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                // PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, messageId);

                                //获取重发数据
                                var mixData = OnceSendMessage.OneToOne.OnceMsgList.SingleOrDefault(m => m.Key == messageId);
                                List<MixMessageObjDto> obj = mixData.Value as List<MixMessageObjDto>;

                                MixMsg mixMsgClass = new MixMsg();
                                mixMsgClass.MessageId = messageId;
                                List<MixMessageTagDto> listTagDto = new List<MixMessageTagDto>();
                                foreach (var list in obj)
                                {
                                    switch (list.type)
                                    {
                                        case "1002":
                                            MixMessageTagDto tagDto = new MixMessageTagDto();

                                            var contentImg = JsonConvert.DeserializeObject<PictureDto>(list.content?.ToString());
                                            var guidImgId = "R" + Guid.NewGuid().ToString();
                                            tagDto.PreGuid = guidImgId;
                                            tagDto.Path = contentImg.picUrl;
                                            listTagDto.Add(tagDto);
                                            //AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, guidImgId, contentImg.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                            break;
                                    }
                                }
                                mixMsgClass.TagDto = listTagDto;
                                //List<PictureAndTextMixDto> picAndTxtMix = listPicAndText.Where(m => m.type == "1002").ToList();
                                //string newMsgId = PublicTalkMothed.timeStampAndRandom();
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string imageSendingId = "sending" + imageTipId;
                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageId;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                #endregion
                                CurrentChatDto currentChat = new CurrentChatDto();
                                currentChat.type = AntSdkchatType.Point;
                                currentChat.messageId = messageId;
                                currentChat.sendUserId = s_ctt.sendUserId;
                                currentChat.sessionId = s_ctt.sessionId; ;
                                currentChat.targetId = s_ctt.targetId;
                                currentChat.isOnceSend = true;

                                //sendMixPicAndText(AntSdkMsgType.ChatMsgMixMessage, currentChat, arg, mixData, mixMsg);
                                OneRightSendPicAndText.RightSendPicAndTextMix(chromiumWebBrowser, messageId, obj, arg, mixMsgClass);
                                sendMixPicAndText(AntSdkMsgType.ChatMsgMixMessage, currentChat, obj, arg, mixMsgClass);
                            }));
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_sendMsgagen]" + ex.Message + ex.Source + ex.StackTrace);
                }

            }
            /// <summary>
            /// 重发文本事件
            /// </summary>
            public static event EventHandler sendTextMsgOnce;

            private void sendTextMsg(string msgStr, string messageid, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failMessage, bool isOnceSendMsg)
            {
                if (sendTextMsgOnce != null)
                {
                    sendTextDto sendText = new sendTextDto();
                    sendText.msgStr = msgStr;
                    sendText.messageid = messageid;
                    sendText.imageTipId = imageTipId;
                    sendText.imageSendingId = imageSendingId;
                    sendText.FailOrSucess = failMessage;
                    sendText.isOnceSendMsg = isOnceSendMsg;
                    sendTextMsgOnce(sendText, null);
                }
            }
            /// <summary>
            /// 重发图片事件
            /// </summary>
            public static event EventHandler upLoadImageMsgOnce;

            private void upLoadImageMsg(string prePaths, string fileFileName, string messageId, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failOrSucess)
            {
                if (upLoadImageMsgOnce != null)
                {
                    SendCutImageDto send = new SendCutImageDto();
                    send.prePaths = prePaths;
                    send.fileFileName = fileFileName;
                    send.messageId = messageId;
                    send.imgeTipId = imageTipId;
                    send.FailOrSucess = failOrSucess;
                    send.imageSendingId = imageSendingId;
                    send.FailOrSucess.obj = failOrSucess.obj;
                    upLoadImageMsgOnce(send, null);
                }
            }
            /// <summary>
            /// 重发文件事件
            /// </summary>
            public static event EventHandler upLoadFileMsgOnce;

            private void upLoadFileMsg(UpLoadFilesDto file)
            {
                if (upLoadFileMsgOnce != null)
                {
                    upLoadFileMsgOnce(file, null);
                }
            }
            /// <summary>
            /// 重发语音事件
            /// </summary>
            public static event EventHandler upLoadVoiceMsgOnce;
            private void upLoadVoiceMsg(string prePaths, string messageId, string preValue, bool isBurnMsg)
            {
                if (upLoadVoiceMsgOnce == null) return;
                var send = new SendCutImageDto
                {
                    prePaths = preValue,
                    messageId = messageId,
                    file = preValue,//本地地址
                    isBurn = isBurnMsg//是否为阅后即焚语音重发
                };
                upLoadVoiceMsgOnce(send, null);
            }

            public static event EventHandler sendMixPicAndTextOnce;
            private void sendMixPicAndText(AntSdkMsgType type, CurrentChatDto currentChat, List<MixMessageObjDto> obj, MessageStateArg arg, MixMsg mixMsg)
            {
                if (sendMixPicAndTextOnce != null)
                {
                    MixPicAndSenderArgs senderArgs = new MixPicAndSenderArgs();
                    senderArgs.type = type;
                    senderArgs.obj = obj;
                    senderArgs.currentChat = currentChat;
                    senderArgs.mixMsg = mixMsg;
                    senderArgs.arg = arg;
                    sendMixPicAndTextOnce(senderArgs, null);
                }
            }
            public class MixPicAndSenderArgs
            {
                public AntSdkMsgType type { set; get; }
                public List<MixMessageObjDto> obj { set; get; }
                public CurrentChatDto currentChat { set; get; }
                public MixMsg mixMsg { set; get; }
                public MessageStateArg arg { set; get; }
            }
        }
        public class CallbackObjectFilePathJs
        {
            private SendMessage_ctt send;
            public ChromiumWebBrowsers chromiumWebBrowser;
            public CallbackObjectFilePathJs(ChromiumWebBrowsers chromiumWebBrowser, SendMessage_ctt s_ctt)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.send = s_ctt;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallbackObjectFilePathJs]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            /// <summary>
            /// 打开文件夹、另存为
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="btnText"></param>
            /// <param name="id"></param>
            public void GetFilePath(string obj, string btnText, string id)
            {
                try
                {
                    ReceiveOrUploadFileDto roufd = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(obj);
                    roufd.guid = id.StartsWith("dct") ? id.Substring(3) : id;//阅后即焚消息以dct开头
                    switch (btnText)
                    {
                        case "打开文件夹":
                            {
                                #region 打开文件夹
                                var path = "";
                                if (!string.IsNullOrEmpty(roufd.downloadPath))
                                    path = roufd.downloadPath;
                                else
                                    path = roufd.localOrServerPath;
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    string first = path.Replace(@"/", "\\");
                                    string fileToSelect = first;
                                    if (File.Exists(fileToSelect))
                                    {
                                        string args = string.Format("/Select, {0}", fileToSelect);
                                        ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                                        Process.Start(pfi);
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(roufd.fileUrl) || !roufd.fileUrl.StartsWith("http"))
                                        {
                                            MessageBoxWindow.Show("文件不存在或未下载！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                        }
                                        else
                                        {
                                            MessageBoxWindow.Show("文件不存在或未下载，请重新下载文件！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                            ResetStatus(roufd);
                                        }
                                    }
                                }));
                                #endregion
                            }
                            break;
                        case "另存为":
                            {
                                #region 另存为
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    SaveFileDialog openFile = new SaveFileDialog();
                                    openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe ";
                                    if (string.IsNullOrEmpty(GlobalVariable.fileSavePath) || !Directory.Exists(GlobalVariable.fileSavePath))
                                        openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                    else
                                        openFile.InitialDirectory = GlobalVariable.fileSavePath;
                                    openFile.FilterIndex = 0;
                                    openFile.FileName = roufd.fileName;
                                    if (openFile.ShowDialog() == DialogResult.OK)
                                    {
                                        var tmpPath = openFile.FileName;
                                        var index = tmpPath.LastIndexOf(@"\");
                                        roufd.downloadPath = tmpPath;
                                        GlobalVariable.fileSavePath = tmpPath.Substring(0, index);
                                        downFile(roufd);
                                    }
                                }));
                                #endregion
                            }
                            break;
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_GetFilePath]:" + ex.Message + ex.Source + ex.StackTrace);
                }
            }
            /// <summary>
            /// 打开文件不存在  重置状态
            /// </summary>
            /// <param name="dtos"></param>
            private void ResetStatus(ReceiveOrUploadFileDto dtos)
            {
                dtos.downloadPath = "";
                dtos.localOrServerPath = "";
                dtos.haveDownFile = "";
                var setValues = JsonConvert.SerializeObject(dtos);
                //设置进度条
                string proId = dtos.progressId.ToString();
                this.chromiumWebBrowser.EvaluateScriptAsync("setProcess('" + proId + "','" + 0 + "%');");
                //设置下载状态图片
                StringBuilder sbFileimg = new StringBuilder();
                sbFileimg.AppendLine("setFileImg('" + dtos.fileImgGuid + "','" + fileShowImage.showImageHtmlPath("reveiving", "") + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                //设置下载状态文字
                StringBuilder sbFileText = new StringBuilder();
                sbFileText.AppendLine("setFileText('" + dtos.fileTextGuid + "','未下载');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                //设置打开、接收
                StringBuilder sbFileOpen = new StringBuilder();
                sbFileOpen.AppendLine("setFileText('" + dtos.fileOpenGuid + "','接收');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpen.ToString());
                //设置打开文件路径
                StringBuilder sbFileOpenPath = new StringBuilder();
                sbFileOpenPath.AppendLine("setFilePath('" + dtos.fileOpenGuid + "','" + setValues + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpenPath.ToString());
                //设置打开文件夹、另存为
                StringBuilder sbFolderOpen = new StringBuilder();
                sbFolderOpen.AppendLine("setFileText('" + dtos.fileDirectoryGuid + "','另存为');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpen.ToString());
                //设置打开文件夹路径
                StringBuilder sbFolderOpenPath = new StringBuilder();
                sbFolderOpenPath.AppendLine("setFilePath('" + dtos.fileDirectoryGuid + "','" + setValues + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpenPath.ToString());
                //更新数据库
                if (dtos.flag == null)
                {
                    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                    t_chat.UpdateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, dtos.downloadPath);
                }
            }
            private void downFile(ReceiveOrUploadFileDto roufd)
            {
                try
                {
                    ReceiveOrUploadFileDto receive = roufd as ReceiveOrUploadFileDto;
                    HttpWebClient<ReceiveOrUploadFileDto> webclient = new HttpWebClient<ReceiveOrUploadFileDto>();
                    webclient.obj = roufd;
                    webclient.DownloadFileAsync(new Uri(receive.fileUrl), receive.downloadPath);
                    webclient.DownloadProgressChanged += Webclient_DownloadProgressChanged;
                    webclient.DownloadFileCompleted += Webclient_DownloadFileCompleted;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_downFile]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void Webclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                try
                {
                    if (e.ProgressPercentage <= 100)
                    {
                        HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                        StringBuilder sbEnd = new StringBuilder();
                        ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                        string proId = dtos.progressId.ToString();
                        sbEnd.AppendLine("setProcess('" + proId + "','" + e.ProgressPercentage + "%');");
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }
                    if (e.ProgressPercentage == 100)
                    {
                        HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                        StringBuilder sbEnd = new StringBuilder();
                        ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                        string proId = dtos.progressId.ToString();
                        sbEnd.AppendLine("setProcess('" + proId + "','" + 100 + "%');");
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_Webclient_DownloadProgressChanged]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void Webclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                try
                {
                    if (e.Error != null)
                    {
                        LogHelper.WriteError("[下载文件失败]>>>" + e.Error.Message + "," + e.Error.StackTrace + "," + e.Error.Source);
                        return;
                    }
                    HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                    ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                    var downPath = dtos.downloadPath.Replace(@"\", @"/");
                    dtos.localOrServerPath = downPath;
                    dtos.downloadPath = downPath;
                    var setValues = JsonConvert.SerializeObject(dtos);
                    //设置进度条
                    string proId = dtos.progressId.ToString();
                    this.chromiumWebBrowser.EvaluateScriptAsync("setProcess('" + proId + "','" + 100 + "%');");
                    //设置下载状态图片
                    StringBuilder sbFileimg = new StringBuilder();
                    sbFileimg.AppendLine("setFileImg('" + dtos.fileImgGuid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                    //设置下载状态文字
                    StringBuilder sbFileText = new StringBuilder();
                    sbFileText.AppendLine("setFileText('" + dtos.fileTextGuid + "','下载完成');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                    //设置打开、接收
                    StringBuilder sbFileOpen = new StringBuilder();
                    sbFileOpen.AppendLine("setFileText('" + dtos.fileOpenGuid + "','打开');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpen.ToString());
                    //设置打开文件路径
                    StringBuilder sbFileOpenPath = new StringBuilder();
                    sbFileOpenPath.AppendLine("setFilePath('" + dtos.fileOpenGuid + "','" + setValues + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpenPath.ToString());
                    //设置打开文件夹、另存为
                    StringBuilder sbFolderOpen = new StringBuilder();
                    sbFolderOpen.AppendLine("setFileText('" + dtos.fileDirectoryGuid + "','打开文件夹');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpen.ToString());
                    //设置打开文件夹路径
                    StringBuilder sbFolderOpenPath = new StringBuilder();
                    sbFolderOpenPath.AppendLine("setFilePath('" + dtos.fileDirectoryGuid + "','" + setValues + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpenPath.ToString());
                    //倒计时
                    if (!dtos.guid.Contains("gxc"))
                    {
                        SendMessage_ctt s_ctt = chromiumWebBrowser.s_ctt as SendMessage_ctt;
                        chromiumWebBrowser.EvaluateScriptAsync("fileClickshow('" + dtos.guid + "','" + dtos.messageId + "');");
                        string errMsg = string.Empty;
                        var burnRead = new AntSdkSendMsg.PointBurnReaded
                        {
                            sendUserId = s_ctt.sendUserId,
                            targetId = s_ctt.targetId,
                            chatIndex = dtos.chatIndex,
                            os = (int)GlobalVariable.OSType.PC,
                            sessionId = s_ctt.sessionId,
                            MsgType = AntSdkMsgType.PointBurnReaded,
                            chatType = (int)AntSdkchatType.Point,
                            messageId = PublicTalkMothed.timeStampAndRandom(),
                            content = new AntSdkSendMsg.PointBurnReaded_content
                            {
                                readIndex = int.Parse(dtos.chatIndex),
                                //TODO://收到的那条阅后即焚消息的messageId
                                messageId = dtos.messageId
                            }
                        };
                        AntSdkService.SdkPublishPointBurnReadReceiptMsg(burnRead, ref errMsg);
                    }
                    //2017-03-04 下载成功 文本路径更新到数据库  阅后即焚除外
                    if (dtos.flag == null)
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.UpdateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, dtos.downloadPath);
                    }
                    else
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.DeleteByMessageId(GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId);
                        AntSdkChatMsg.ChatBase receipt = new AntSdkChatMsg.ChatBase();
                        receipt.sessionId = chromiumWebBrowser.sessionid;
                        receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                        receipt.chatIndex = dtos.chatIndex;
                        receipt.targetId = hbc.userId;
                        SentBurnAfterReadEvent?.Invoke(receipt, AntSdkMsgType.PointBurnReaded);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_Webclient_DownloadFileCompleted]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
        }
        /// <summary>
        /// 双击查看图片
        /// </summary>
        public class CallbackObjectForJs
        {
            private List<AddImgUrlDto> imgUrl;
            private ChromiumWebBrowsers chromiumWebBrowser;
            private SendMessage_ctt s_ctt;
            private string _isShowBurn = "";
            private TalkViewModel _talkViewModel;
            public CallbackObjectForJs(List<AddImgUrlDto> imgUrlDtos, ChromiumWebBrowsers cef, SendMessage_ctt s_ctt, string _isShowBurn, TalkViewModel talkViewModel)
            {
                _talkViewModel = talkViewModel;
                imgUrl = imgUrlDtos;
                this.chromiumWebBrowser = cef;
                this.s_ctt = s_ctt;
                this._isShowBurn = _isShowBurn;
            }
            public void ShowMessage(string id, string src, string title, string value, string sid, string mid)
            {
                try
                {
                    string index = "0";
                    if (imgUrl != null)
                    {
                        AddImgUrlDto addImg = null;
                        if (!string.IsNullOrEmpty(mid))
                        {
                            addImg = imgUrl.SingleOrDefault(m => m.ChatIndex == mid.Substring(1, mid.Length - 1));
                        }
                        else
                        {
                            addImg = imgUrl.SingleOrDefault(m => m.ChatIndex == sid);
                        }
                        //foreach (var item in imgUrl)
                        //{
                        if (addImg != null)
                        {
                            index = addImg.ChatIndex;
                        }
                        //}
                    }
                    //System.Windows.MessageBox.Show(id);
                    string imagePath = "";
                    if (src != null)
                    {
                        if (src.StartsWith("http://"))
                        {
                            imagePath = src;
                        }
                        else
                        {
                            imagePath = PublicTalkMothed.strUrlDecode(src.Substring(8, src.Length - 8).Replace(@"/", @"\").Replace("%20", " "));
                        }
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (value == "1")
                            {
                                ShowImage(imagePath, GlobalVariable.BurnFlag.IsBurn, index, imgUrl);
                            }
                            else
                            {
                                ShowImage(imagePath, GlobalVariable.BurnFlag.NotIsBurn, index, imgUrl);
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_ShowMessage]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void ShowImage(string path, GlobalVariable.BurnFlag flag, string currentIndex, List<AddImgUrlDto> imgUrl)
            {
                if (string.IsNullOrEmpty(path)) return;
                //PictureViewerView win = new PictureViewerView();
                //PictureViewerViewModel model = new PictureViewerViewModel(path, flag, currentIndex, imgUrl);
                //win.DataContext = model;
                //win.Show();
                if (ImageHandle.PicView == null)
                {
                    ImageHandle.PicView = new PictureViewerView();
                    ImageHandle.PicViewModel = new PictureViewerViewModel(path, flag, currentIndex, imgUrl);
                    ImageHandle.PicView.DataContext = ImageHandle.PicViewModel;
                    ImageHandle.PicView.Show();
                }
                else
                {
                    ImageHandle.PicView.Close();
                    ImageHandle.PicView = null;
                    ImageHandle.PicViewModel = null;
                    ImageHandle.PicView = new PictureViewerView();
                    ImageHandle.PicViewModel = new PictureViewerViewModel(path, flag, currentIndex, imgUrl);
                    ImageHandle.PicView.DataContext = ImageHandle.PicViewModel;
                    ImageHandle.PicView.Show();
                }
            }

            public void imageMenuMethod(string caseMethod, string imageUrl)
            {
                if (string.IsNullOrEmpty(caseMethod)) { return; }
                else
                {
                    switch (caseMethod)
                    {
                        //复制
                        case "1":
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                using (var webClient = new System.Net.WebClient())
                                {
                                    var imgData = webClient.DownloadData(imageUrl);
                                    using (var stream = new MemoryStream(imgData))
                                    {
                                        BitmapImage bitImg = new BitmapImage();
                                        bitImg.BeginInit();
                                        bitImg.StreamSource = stream;
                                        bitImg.EndInit();
                                        System.Windows.Clipboard.SetImage(bitImg);
                                    }
                                }
                            }));
                            break;
                        //另存为
                        case "2":
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                                sfd.Filter = "JPEG Files(*.jpg)|*.jpg|BMP Files (*.bmp)|*.bmp|PNG Files(*.png)|*.png";
                                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                                string getExtendName = imageUrl.Substring(imageUrl.LastIndexOf('.'), imageUrl.Length - (imageUrl.LastIndexOf('.')));
                                sfd.FileName = DateTime.Now.ToString("yyyyMMddhhmmssffff") + getExtendName;
                                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    WebClient wc = new WebClient();
                                    wc.DownloadFileAsync(new Uri(imageUrl), sfd.FileName);
                                }
                            }));
                            break;
                        //撤回
                        case "3":
                            ReCallMsg(imageUrl);
                            break;
                    }

                }
            }
            public void copydivContent(string caseMethod, string divid)
            {
                if (string.IsNullOrEmpty(caseMethod)) { return; }
                else
                {
                    switch (caseMethod)
                    {
                        //复制
                        case "1":
                            try
                            {
                                App.Current.Dispatcher.Invoke((Action)(() =>
                                 {
                                     try
                                     {

                                         Analogkeyboard.CtrlAndC();
                                     }
                                     catch (Exception ex)
                                     {

                                     }

                                 }));
                            }
                            catch (Exception ex)
                            {

                                LogHelper.WriteError(ex.Message + ex.Source + ex.StackTrace);
                            }
                            break;
                        //撤回
                        case "2":
                            ThreadPool.QueueUserWorkItem(m => ReCallMsg(divid));
                            break;
                    }
                }
            }
            public void ReCallMsg(string messageId)
            {
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                if (!string.IsNullOrEmpty(messageId))
                {
                    //获取服务器时间
                    var errorCode = 0;
                    string errorMsg = "";
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errorCode, ref errorMsg);
                    if (serverResult == null)
                    {
                        return;
                    }
                    string serverStamp = serverResult.systemCurrentTime;
                    if (!string.IsNullOrEmpty(serverStamp))
                    {
                        var recalMsg = AntSdkSqliteHelper.ModelConvertHelper<ChatInfo>.ConvertToModel(t_chat.getRecallData(messageId));
                        if (recalMsg.Count() == 0)
                        {
                            return;
                        }
                        string localTime = "";
                        if (recalMsg[0].sendsucessorfail == "1")
                        {
                            localTime = recalMsg[0].sendtime;
                            DateTime dtServer = PublicTalkMothed.ConvertStringToDateTime(serverStamp);
                            DateTime dtLocal = PublicTalkMothed.ConvertStringToDateTime(localTime);
                            double differ = dtServer.Subtract(dtLocal).TotalMinutes;
                            if (differ > 2)
                            {
                                _talkViewModel.showTextMethod("", false);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
                //1、先发送
                //2、发送成功删除该消息
                //3、发送成功插入撤回消息
                string errMsg = "";
                AntSdkChatMsg.Revocation ReCall = new AntSdkChatMsg.Revocation();
                string newMessageId = PublicTalkMothed.timeStampAndRandom();
                ReCall.messageId = newMessageId;
                AntSdkChatMsg.Revocation_content ReContent = new AntSdkChatMsg.Revocation_content();
                ReCall.chatType = (int)AntSdkchatType.Point;
                ReCall.MsgType = AntSdkMsgType.Revocation;
                ReCall.os = (int)GlobalVariable.OSType.PC;
                ReContent.messageId = messageId;
                ReCall.content = ReContent;
                ReCall.sendUserId = this.s_ctt.sendUserId;
                ReCall.targetId = this.s_ctt.targetId;
                ReCall.sessionId = this.s_ctt.sessionId;
                int maxChatIndex = t_chat.GetPreOneChatIndex(this.s_ctt.sessionId, AntSdkService.AntSdkCurrentUserInfo.userId);
                //插入消息
                SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase tempChatMsg = new AntSdkChatMsg.ChatBase();
                tempChatMsg.MsgType = AntSdkMsgType.Revocation;
                tempChatMsg.messageId = newMessageId;
                tempChatMsg.SENDORRECEIVE = "1";
                tempChatMsg.chatType = (int)AntSdkchatType.Point;
                tempChatMsg.flag = _isShowBurn == "Collapsed" ? 1 : 0;
                tempChatMsg.os = (int)GlobalVariable.OSType.PC;
                tempChatMsg.sendUserId = this.s_ctt.sendUserId;
                tempChatMsg.chatIndex = maxChatIndex.ToString();
                tempChatMsg.targetId = this.s_ctt.targetId;
                tempChatMsg.sessionId = this.s_ctt.sessionId;
                tempChatMsg.sourceContent = "你" + GlobalVariable.RevocationPrompt.Msessage;
                bool result = ThreadPool.QueueUserWorkItem(m => addData(tempChatMsg));
                if (result)
                {
                    var sendMsg = AntSdkService.SdkPublishChatMsg(ReCall, ref errMsg);
                    if (sendMsg)
                    {
                        //删除本地消息
                        t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, messageId);

                        //隐藏界面显示
                        PublicTalkMothed.ReCallMsgDiv(chromiumWebBrowser, messageId);
                        //插入界面一条消息 "我撤销了一条消息"
                        this.chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.InsertUIRecallMsg("你" + GlobalVariable.RevocationPrompt.Msessage));
                        //更改发送状态
                        t_chat.UpdateSendMsgState(newMessageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                        tempChatMsg.sendTime = AntSdkDataConverter.ConvertDateTimeToIntLong(DateTime.Now).ToString();
                        //SentBurnAfterReadEvent?.Invoke(tempChatMsg, AntSdkMsgType.Revocation, false);
                    }
                    else
                    {
                        t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, newMessageId);
                    }
                    //AntSdkChatMsg.ChatBase receipt = new AntSdkChatMsg.ChatBase();
                    //receipt.sessionId = this.s_ctt.sessionId;
                    //receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                    //receipt.chatIndex = maxChatIndex.ToString();
                    //receipt.targetId = this.user.userId;
                    //receipt.messageId = messageId;
                    //receipt.sourceContent=

                }
            }
            /// <summary>
            /// 插入数据
            /// </summary>
            /// <param name="cmr"></param>
            public void addData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
            {
                SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase receive = cmr;
                BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_MessageDAL> t_chat = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_MessageDAL>();
                if (t_chat.Insert(cmr) == 1)
                {

                }
                else
                {

                }
            }
            //public static event EventHandler ReCallMsgEventHandler;
            //private void OnReCallMsgClickEvent(string messageId)
            //{
            //    if (ReCallMsgEventHandler != null)
            //    {
            //        ReCallMsgEventHandler(messageId, null);
            //    }
            //}
        }
        public static BackgroundWorker downBack = null;
        public class CallbackObjectUrlJs
        {
            public ChromiumWebBrowsers chromiumWebBrowser;
            public CallbackObjectUrlJs(ChromiumWebBrowsers chromiumWebBrowser)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallbackObjectUrlJs]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            string path = "";
            SendMessage_ctt send;
            public CallbackObjectUrlJs(ChromiumWebBrowsers chromiumWebBrowser, string path, SendMessage_ctt send)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.path = path;
                    this.send = send;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallbackObjectUrlJs]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            public void DownUrl(string obj, string btnText, string id)
            {
                try
                {
                    switch (btnText)
                    {
                        case "接收":
                            #region 接收
                            if (!Directory.Exists(publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download"))
                            {
                                Directory.CreateDirectory(publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download");
                            }
                            ReceiveOrUploadFileDto roufd = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(obj);
                            roufd.guid = id;
                            //判断是否有相同文件名的文件
                            string preString = publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download\\";
                            string endString = roufd.fileName;
                            string paths = preString + endString;
                            string newName = paths;
                            int i = 0;
                            while (File.Exists(newName))
                            {
                                string noExtendname = Path.GetFileNameWithoutExtension(paths);
                                string yesExtendname = Path.GetExtension(paths);
                                newName = preString + noExtendname + "(" + i + ")" + yesExtendname;
                                i++;
                            }
                            if (roufd.fileName != newName)
                            {
                                string yesExtendname = Path.GetExtension(newName);
                                string noExtendname = Path.GetFileNameWithoutExtension(newName);
                                roufd.fileName = noExtendname + yesExtendname;
                            }
                            downFile(roufd);
                            #endregion
                            break;
                        case "打开":
                            #region 打开
                            ReceiveOrUploadFileDto roufds = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(obj);
                            string path = "";
                            string path2 = "";
                            string path3 = "";
                            if (string.IsNullOrEmpty(roufds.haveDownFile))
                            {
                                if (string.IsNullOrEmpty(roufds.localOrServerPath))
                                    path = publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download\\" + roufds.fileName;
                                else
                                    path = roufds.localOrServerPath;
                            }
                            else
                            {
                                path = roufds.haveDownFile;
                            }
                            path2 = roufds.localOrServerPath;
                            path3 = roufds.fileUrl;
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                try
                                {
                                    if (File.Exists(path))
                                    {
                                        System.Diagnostics.Process.Start(path);
                                    }
                                    else
                                    {
                                        if (File.Exists(path2))
                                        {
                                            System.Diagnostics.Process.Start(path2);
                                        }
                                        else
                                        {
                                            if (File.Exists(path3))
                                            {
                                                System.Diagnostics.Process.Start(path3);
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(roufds.fileUrl) || !roufds.fileUrl.StartsWith("http"))
                                                {
                                                    MessageBoxWindow.Show("文件不存在或未下载！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                                }
                                                else
                                                {
                                                    MessageBoxWindow.Show("文件不存在或未下载，请重新下载文件！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                                    ResetStatus(roufds);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message.Contains("没有应用程序与此操作的指定文件有关联"))
                                    {
                                        MessageBoxWindow.Show("没有找到能打开此文件的程序", MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn);
                                    }
                                }
                            }));
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_DownUrl]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            /// <summary>
            /// 打开文件不存在  重置状态
            /// </summary>
            /// <param name="dtos"></param>
            private void ResetStatus(ReceiveOrUploadFileDto dtos)
            {
                dtos.downloadPath = "";
                dtos.localOrServerPath = "";
                dtos.haveDownFile = "";
                var setValues = JsonConvert.SerializeObject(dtos);
                //设置进度条
                string proId = dtos.progressId.ToString();
                this.chromiumWebBrowser.EvaluateScriptAsync("setProcess('" + proId + "','" + 0 + "%');");
                //设置下载状态图片
                StringBuilder sbFileimg = new StringBuilder();
                sbFileimg.AppendLine("setFileImg('" + dtos.fileImgGuid + "','" + fileShowImage.showImageHtmlPath("reveiving", "") + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                //设置下载状态文字
                StringBuilder sbFileText = new StringBuilder();
                sbFileText.AppendLine("setFileText('" + dtos.fileTextGuid + "','未下载');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                //设置打开、接收
                StringBuilder sbFileOpen = new StringBuilder();
                sbFileOpen.AppendLine("setFileText('" + dtos.fileOpenGuid + "','接收');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpen.ToString());
                //设置打开文件路径
                StringBuilder sbFileOpenPath = new StringBuilder();
                sbFileOpenPath.AppendLine("setFilePath('" + dtos.fileOpenGuid + "','" + setValues + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpenPath.ToString());
                //设置打开文件夹、另存为
                StringBuilder sbFolderOpen = new StringBuilder();
                sbFolderOpen.AppendLine("setFileText('" + dtos.fileDirectoryGuid + "','另存为');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpen.ToString());
                //设置打开文件夹路径
                StringBuilder sbFolderOpenPath = new StringBuilder();
                sbFolderOpenPath.AppendLine("setFilePath('" + dtos.fileDirectoryGuid + "','" + setValues + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpenPath.ToString());
                //更新数据库
                if (dtos.flag == null)
                {
                    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                    t_chat.UpdateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, dtos.downloadPath);
                }
            }
            private void downFile(ReceiveOrUploadFileDto roufd)
            {
                try
                {
                    ReceiveOrUploadFileDto receive = roufd as ReceiveOrUploadFileDto;
                    HttpWebClient<ReceiveOrUploadFileDto> webclient = new HttpWebClient<ReceiveOrUploadFileDto>();
                    webclient.obj = roufd;
                    webclient.DownloadFileAsync(new Uri(receive.fileUrl), publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download\\" + receive.fileName);
                    webclient.DownloadProgressChanged += Webclient_DownloadProgressChanged;
                    webclient.DownloadFileCompleted += Webclient_DownloadFileCompleted;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_downFile]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void Webclient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                try
                {
                    if (e.ProgressPercentage <= 100)
                    {
                        HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                        StringBuilder sbEnd = new StringBuilder();
                        ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                        string proId = dtos.progressId.ToString();
                        sbEnd.AppendLine("setProcess('" + proId + "','" + e.ProgressPercentage + "%');");
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }
                    if (e.ProgressPercentage == 100)
                    {
                        HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                        StringBuilder sbEnd = new StringBuilder();
                        ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                        string proId = dtos.progressId.ToString();
                        sbEnd.AppendLine("setProcess('" + proId + "','" + 100 + "%');");
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_Webclient_DownloadProgressChanged]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void Webclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                try
                {
                    if (e.Error != null)
                    {
                        LogHelper.WriteError("[下载文件失败]>>>" + e.Error.Message + "," + e.Error.StackTrace + "," + e.Error.Source);
                        return;
                    }
                    HttpWebClient<ReceiveOrUploadFileDto> hbc = sender as HttpWebClient<ReceiveOrUploadFileDto>;
                    ReceiveOrUploadFileDto dtos = hbc.obj as ReceiveOrUploadFileDto;
                    string downPath = (publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\personal\\download\\" + dtos.fileName).Replace(@"\", @"/");
                    dtos.downloadPath = downPath;
                    dtos.localOrServerPath = downPath;
                    var setValues = JsonConvert.SerializeObject(dtos);
                    //设置进度条
                    string proId = dtos.progressId.ToString();
                    this.chromiumWebBrowser.EvaluateScriptAsync("setProcess('" + proId + "','" + 100 + "%');");
                    //设置下载状态图片
                    StringBuilder sbFileimg = new StringBuilder();
                    sbFileimg.AppendLine("setFileImg('" + dtos.fileImgGuid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                    //设置下载状态文字
                    StringBuilder sbFileText = new StringBuilder();
                    sbFileText.AppendLine("setFileText('" + dtos.fileTextGuid + "','下载完成');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                    //设置打开、接收
                    StringBuilder sbFileOpen = new StringBuilder();
                    sbFileOpen.AppendLine("setFileText('" + dtos.fileOpenGuid + "','打开');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpen.ToString());
                    //设置打开文件路径
                    StringBuilder sbFileOpenPath = new StringBuilder();
                    sbFileOpenPath.AppendLine("setFilePath('" + dtos.fileOpenGuid + "','" + setValues + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileOpenPath.ToString());
                    //设置打开文件夹、另存为
                    StringBuilder sbFolderOpen = new StringBuilder();
                    sbFolderOpen.AppendLine("setFileText('" + dtos.fileDirectoryGuid + "','打开文件夹');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpen.ToString());
                    //设置打开文件夹路径
                    StringBuilder sbFolderOpenPath = new StringBuilder();
                    sbFolderOpenPath.AppendLine("setFilePath('" + dtos.fileDirectoryGuid + "','" + setValues + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFolderOpenPath.ToString());
                    //2017-02-24 添加新方法 倒计时
                    if (!dtos.guid.Contains("gxc"))
                    {
                        SendMessage_ctt s_ctt = chromiumWebBrowser.s_ctt as SendMessage_ctt;
                        chromiumWebBrowser.EvaluateScriptAsync("fileClickshow('" + dtos.guid + "','" + dtos.messageId + "');");
                        string errMsg = string.Empty;
                        var burnRead = new AntSdkSendMsg.PointBurnReaded
                        {
                            sendUserId = s_ctt.sendUserId,
                            targetId = s_ctt.targetId,
                            chatIndex = dtos.chatIndex,
                            os = (int)GlobalVariable.OSType.PC,
                            sessionId = s_ctt.sessionId,
                            MsgType = AntSdkMsgType.PointBurnReaded,
                            chatType = (int)AntSdkchatType.Point,
                            messageId = PublicTalkMothed.timeStampAndRandom(),
                            content = new AntSdkSendMsg.PointBurnReaded_content
                            {
                                readIndex = int.Parse(dtos.chatIndex),
                                //TODO://收到的那条阅后即焚消息的messageId
                                messageId = dtos.messageId
                            }
                        };
                        AntSdkService.SdkPublishPointBurnReadReceiptMsg(burnRead, ref errMsg);
                    }
                    //下载成功 文本路径更新到数据库  阅后即焚除外
                    if (dtos.flag == null)
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.UpdateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, downPath);
                    }
                    else
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.DeleteByMessageId(GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId);
                        AntSdkChatMsg.ChatBase receipt = new AntSdkChatMsg.ChatBase();
                        receipt.sessionId = chromiumWebBrowser.sessionid;
                        receipt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
                        receipt.chatIndex = dtos.chatIndex;
                        receipt.targetId = hbc.userId;
                        SentBurnAfterReadEvent?.Invoke(receipt, AntSdkMsgType.PointBurnReaded);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_Webclient_DownloadFileCompleted]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
        }
        DispatcherTimer IsShowTip = new DispatcherTimer();
        #region 发送消息
        public ICommand btnSendMsgCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (!AntSdkService.AntSdkIsConnected)//重连成功
                    {
                        showTextMethod("网络连接已断开，不能发送消息！");
                        hOffset = -135;
                        return;
                    }
                    var counts = _richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                    if (counts.Count() == 0)
                    {
                        hOffset = -22;
                        showTextMethod("发送消息不能为空，请重新输入！");
                        this.richTextBox.Focus();
                        return;
                    }
                    else
                    {
                        sendMsg(null);
                        this.richTextBox.Focus();
                    }

                });
            }
        }
        DateTime timeSplit = new DateTime();
        int typeString = 0;
        int typeImage = 0;
        #region 发送消息方法
        private void sendMsg(System.Windows.Input.KeyEventArgs e)
        {
            string listShow = "";
            List<MixMessageObjDto> mixMsg = new List<MixMessageObjDto>();
            List<PictureAndTextMixDto> picAndTxtMix = new List<PictureAndTextMixDto>();
            picAndTxtMix.Clear();
            List<BatchImage> listImage = new List<BatchImage>();
            typeString = 0;
            typeImage = 0;
            SendMessage_ctt smt = null;
            try
            {
                var sendCounts = this._richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                if (sendCounts.Count == 0)
                {
                    return;
                }
                if (sendCounts[0].FirstInline == null || sendCounts[0].LastInline == null)
                {
                    return;
                }

                #region 消息构造

                var blockCount = this.richTextBox.Document.Blocks.Count(m => m != null);
                var counts = this.richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                string msgStr = "";
                string SendStr = "";
                int addCount = 0;
                foreach (var ss in counts)
                {
                    addCount++;
                    var ll = ss.OfType<Inline>();
                    foreach (var ite in ll)
                    {
                        if (ite is Run)
                        {
                            Run tt = ite as Run;
                            if (tt != null)
                            {
                                typeString = 1;
                                string contents = tt.Text;
                                msgStr += tt.Text;
                                SendStr +=
                                    tt.Text.Replace(" ", "&#160;")
                                        .Replace("<", "&lt;")
                                        .Replace("'", "&#39;")
                                        .Replace(" ", "&nbsp;")
                                        .Replace(@"\", @"\\");
                                picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Text, contents));
                                //SendStr += tt.Text.Replace(" ", "&#160;").Replace("<", "&lt;").Replace(@"""", "&quot;&quot;").Replace("'", "&#39");

                                MixMessageObjDto mixtext = new MixMessageObjDto();
                                mixtext.type = "1001";
                                mixtext.content = contents;
                                mixMsg.Add(mixtext);

                                listShow += tt.Text;
                            }
                        }
                        else if (ite is LineBreak)
                        {
                            msgStr += "\r\n";
                            SendStr += "<br/>";
                            picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.LineBreak));
                            MixMessageObjDto mixtext = new MixMessageObjDto();
                            mixtext.type = "0000";
                            mixtext.content = "";
                            mixMsg.Add(mixtext);
                        }
                        InlineUIContainer iu = ite as InlineUIContainer;
                        if (iu != null)
                        {
                            var oo = iu.Child as Image;
                            if (oo.Tag != null && oo.Tag.ToString() == "cut")
                            {
                                typeImage = 1;
                                BatchImage batchImage = new BatchImage();
                                batchImage.image = oo;
                                listImage.Add(batchImage);
                                string path = oo.Source.ToString().Substring(8, oo.Source.ToString().Length - 8);
                                string imgMd5 = PublicTalkMothed.getFileMd5Value(path);
                                string imgId = "i" + Guid.NewGuid().ToString();
                                picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Image, oo.Source.ToString(), imgMd5, path, imgId));

                                MixMessageObjDto mixtext = new MixMessageObjDto();
                                mixtext.type = "1002";
                                PictureDto pictureDto = new PictureDto();
                                pictureDto.picUrl = path;
                                mixtext.content = JsonConvert.SerializeObject(pictureDto);
                                mixMsg.Add(mixtext);
                                listShow += "[图片]";
                            }
                            else
                            {
                                typeString = 1;
                                var img = new Image
                                {
                                    Source = oo.Source,
                                    Width = oo.Width,
                                    Height = oo.Height,
                                    Stretch = Stretch.Fill
                                };

                                InlineUIContainer iuImage = new InlineUIContainer();
                                iuImage.Child = img;

                                var imgUrls = oo.Source;

                                //表情构造

                                string imgSubStr = "[" +
                                                   imgUrls.ToString()
                                                       .Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) + "]";
                                msgStr += imgSubStr;
                                //网络Emoji
                                //string nn = "http://192.168.10.229:8080/scsf/visitor/images/emotions/" + imgUrls.ToString().Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) + ".png";
                                //本地Emoji
                                string nn = "file:///" +
                                            (AppDomain.CurrentDomain.BaseDirectory + "Emoji/" +
                                             imgUrls.ToString().Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) +
                                             ".png").Replace(@"\", @"/").Replace(" ", "%20");
                                SendStr += "<img src=" + nn + "></img>";
                                picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Text, imgSubStr));

                                MixMessageObjDto mixtext = new MixMessageObjDto();
                                mixtext.type = "1001";

                                mixtext.content = imgSubStr;
                                mixMsg.Add(mixtext);
                                listShow += imgSubStr;
                            }
                        }
                    }
                    if (addCount < counts.Count())
                    {
                        msgStr += "\r\n";
                        SendStr += "<br/>";
                        picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.LineBreak));


                        MixMessageObjDto mixtext = new MixMessageObjDto();
                        mixtext.type = "0000";
                        mixtext.content = "";
                        mixMsg.Add(mixtext);
                    }
                }

                #endregion
                #region 屏蔽图文混合提示
                //if (typeString + typeImage == 2 && !string.IsNullOrEmpty((msgStr).Trim()))
                //{
                //    if (e != null)
                //    {
                //        hOffset = -65;
                //        showTextMethod("暂不支持图文混合发送");
                //        e.Handled = true;
                //    }
                //    else
                //    {
                //        hOffset = -65;
                //        showTextMethod("暂不支持图文混合发送");
                //    }
                //    return;
                //}
                #endregion
                if (string.IsNullOrEmpty(msgStr) && listImage.Count() == 0)
                {
                    this.richTextBox.Document.Blocks.Clear();
                    if (e != null)
                    {
                        showTextMethod("发送消息不能为空，请重新输入！");
                        e.Handled = true;
                    }
                }
                else
                {
                    if (PublicTalkMothed.textLength(richTextBox))
                    {
                        showTextMethod("发送消息内容超长，请分条发送。");
                        if (e != null)
                        {
                            e.Handled = true;
                        }
                        return;
                    }
                    if (timeSplit.Year == 1)
                    {
                        timeSplit = DateTime.Now;
                    }
                    else
                    {

                        double sd = DateTime.Now.Subtract(timeSplit).TotalMilliseconds;
                        if (sd < 500)
                        {
                            showTextMethod("客官，你发消息太快了，悠着点奥！^_^");
                            hOffset = -176;
                            if (e != null)
                            {
                                e.Handled = true;
                            }
                            return;
                        }

                    }
                    timeSplit = DateTime.Now;
                    //文本发送
                    if (typeString == 1 && typeImage == 0)
                    {
                        //文本消息
                        TextMessage(msgStr, s_ctt.sessionId, e, SendStr, _isShowBurn);
                        if (msgStr.Trim() != "")
                        {
                            _richTextBox.Document.Blocks.Clear();
                        }
                    }
                    //图片发送
                    if ((typeString == 0 || string.IsNullOrEmpty(msgStr.Trim())) && typeImage == 1)
                    {
                        //查询数据库最大chatindex
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId,
                            this.s_ctt.sessionId);
                        List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listsImg = new List<AntSdkChatMsg.ChatBase>();
                        foreach (var lists in listImage)
                        {
                            ImageDto imageDto = new ImageDto();
                            SendMessageDto smgImg = new SendMessageDto();
                            SendMessage_ctt smtImg = new SendMessage_ctt();

                            imageDto.picUrl = lists.image.Source.ToString();
                            smtImg.MsgType = AntSdkMsgType.ChatMsgPicture;
                            smtImg.sourceContent = JsonConvert.SerializeObject(imageDto);
                            smtImg.messageId = PublicTalkMothed.timeStampAndRandom();
                            lists.messageId = smtImg.messageId.ToString();
                            smtImg.sendUserId = s_ctt.sendUserId;
                            smtImg.sessionId = s_ctt.sessionId;
                            smtImg.targetId = s_ctt.targetId;

                            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt =
                                JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(
                                    JsonConvert.SerializeObject(smtImg));
                            //TODO:AntSdk_Modify
                            //sm_ctt.MTP = "2";
                            sm_ctt.sourceContent = JsonConvert.SerializeObject(imageDto);
                            sm_ctt.chatIndex = maxChatindex.ToString();
                            sm_ctt.sendsucessorfail = 0;
                            sm_ctt.SENDORRECEIVE = "1";
                            sm_ctt.uploadOrDownPath = lists.image.Source.ToString();
                            if (_isShowBurn == "Collapsed" && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
                            {
                                sm_ctt.flag = 1;
                            }
                            listsImg.Add(sm_ctt);
                            OnceSendImage sendImage = new OnceSendImage();
                            sendImage.ctt = this.s_ctt;
                            if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(lists.messageId))
                                OnceSendMessage.OneToOne.OnceMsgList.Add(lists.messageId, sendImage);
                        }
                        var resultImg = t_chat.InsertBig(listsImg);
                        if (resultImg)
                        {
                            foreach (var lists in listImage)
                            {
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.preChatIndex = maxChatindex;
                                failMessage.mtp = (int)GlobalVariable.MsgType.Picture;
                                failMessage.content = "";
                                failMessage.sessionid = s_ctt.sessionId;
                                DateTime dt = DateTime.Now;
                                failMessage.lastDatetime = dt.ToString();

                                //2017-03-16 添加
                                string messageId = lists.messageId.ToString();
                                //2017-04-01添加
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string prePath = lists.image.Source.ToString().Replace(@"\", @"/");
                                string prePaths = prePath.Substring(8, prePath.Length - 8);
                                string fileFileName = System.IO.Path.GetFileNameWithoutExtension(prePaths);
                                string imageSendingId = "sending" + imageTipId;
                                if (_isShowBurn == "Visible" || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                                {
                                    if (!listChatIndex.Contains(messageId))
                                    {
                                        listChatIndex.Add(messageId);
                                        if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                                        {
                                            SendMsgListPointMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                                        }
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                        updateFailMessage(failMessage, "");
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, messageId, prePaths,
                                            "wlc" + fileFileName, burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read,
                                            burnMsg.IsEffective.UnKnow);
                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;


                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                        arg.isGroup = false;
                                        arg.MessageId = messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowser;
                                        arg.SendIngId = imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }

                                        //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                        failMessage.obj = arg;
                                        #endregion
                                        uploadImageSegment(prePaths, fileFileName, messageId, imageTipId, imageSendingId,
                                           failMessage);
                                        PublicTalkMothed.rightSendImage(chromiumWebBrowser, lists.image, fileFileName, _richTextBox, e, messageId, imageTipId, imageSendingId, dt, IsRobot);
                                    }
                                }
                                else
                                {
                                    if (!listChatIndex.Contains(messageId))
                                    {
                                        listChatIndex.Add(messageId);
                                        if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                                        {
                                            SendMsgListPointMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                                        }
                                        updateFailMessage(failMessage, "");
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, messageId, prePaths,
                                            "wlc" + fileFileName, burnMsg.isBurnMsg.yesBurn, burnMsg.IsReadImg.read,
                                            burnMsg.IsEffective.effective);
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;


                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                        arg.isGroup = false;
                                        arg.MessageId = messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowser;
                                        arg.SendIngId = imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }
                                        //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                        failMessage.obj = arg;
                                        #endregion
                                        uploadImageSegment(prePaths, fileFileName, messageId, imageTipId, imageSendingId,
                                           failMessage);
                                        PublicTalkMothed.rightSendImageBurn(chromiumWebBrowser, lists.image, fileFileName, _richTextBox, e, messageId, imageTipId, imageSendingId, dt);
                                    }
                                }
                            }
                        }
                    }
                    //图文混合发送
                    if (typeString + typeImage == 2 && !string.IsNullOrEmpty((msgStr).Trim()))
                    {
                        if (IsRobot == true)
                        {
                            showTextMethod("机器人不支持图文混合消息发送！");
                            return;
                        }
                        if (isShowBurn != "Visible")
                        {
                            showTextMethod("阅后即焚模式不支持图文混合消息发送！");
                            return;
                        }

                        #region 查询数据库最大chatindex
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId,
                            this.s_ctt.sessionId);
                        #endregion

                        //发送中提示
                        AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                        failMessage.preChatIndex = maxChatindex;
                        failMessage.mtp = (int)GlobalVariable.MsgType.Text;
                        failMessage.content = listShow;
                        failMessage.sessionid = s_ctt.sessionId;
                        DateTime dt = DateTime.Now;
                        failMessage.lastDatetime = dt.ToString();
                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                        updateFailMessage(failMessage, "");

                        string messageId = PublicTalkMothed.timeStampAndRandom();
                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                        string imageSendingId = "sending" + imageTipId;

                        #region 消息状态监控
                        MessageStateArg arg = new MessageStateArg();
                        arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                        arg.isGroup = false;
                        arg.MessageId = messageId;
                        arg.SessionId = s_ctt.sessionId;
                        arg.WebBrowser = chromiumWebBrowser;
                        arg.SendIngId = imageSendingId;
                        arg.RepeatId = imageTipId;
                        #endregion


                        #region content数据构造
                        List<object> mixData = new List<object>();
                        mixData.Clear();
                        MixMsg mixMsgClass = new MixMsg();
                        mixMsgClass.MessageId = messageId;
                        List<MixMessageTagDto> listTagDto = new List<MixMessageTagDto>();
                        foreach (var list in mixMsg)
                        {
                            var type = list as MixMessageBase;
                            switch (type.type)
                            {
                                case "1002":
                                    MixMessageTagDto tagDto = new MixMessageTagDto();
                                    //var images = list as MixMessageDto;
                                    var contentImg = JsonConvert.DeserializeObject<PictureDto>(list.content?.ToString());
                                    var guidImgId = "R" + Guid.NewGuid().ToString();
                                    tagDto.PreGuid = guidImgId;
                                    tagDto.Path = contentImg.picUrl;
                                    listTagDto.Add(tagDto);
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, guidImgId, contentImg.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    break;
                            }
                        }
                        mixMsgClass.TagDto = listTagDto;
                        #region New
                        List<AntSdkChatMsg.MixMessage_content> ListMixMsg = new List<AntSdkChatMsg.MixMessage_content>();
                        AntSdkChatMsg.MixMessage mixImageText = new AntSdkChatMsg.MixMessage();

                        foreach (var list in mixMsg)
                        {
                            AntSdkChatMsg.MixMessage_content message_Content = new AntSdkChatMsg.MixMessage_content();
                            //var type = list as MixMessageBase;
                            switch (list.type)
                            {
                                case "1001":
                                    //var text = list as MixMessageDto;
                                    message_Content.type = list.type;
                                    message_Content.content = list.content;
                                    break;
                                case "1002":
                                    //var picDto = list as MixMessageDto;
                                    message_Content.type = list.type;
                                    message_Content.content = list.content;
                                    break;
                                case "1008":
                                    //var at = list as MixMessageObjDto;
                                    List<object> listObj = new List<object>();
                                    message_Content.type = list.type;
                                    listObj.Add(list.content);
                                    message_Content.content = listObj;
                                    break;
                                //换行
                                case "0000":
                                    message_Content.type = list.type;
                                    message_Content.content = "";
                                    break;
                            }
                            ListMixMsg.Add(message_Content);
                        }
                        #endregion
                        string contentMix = JsonConvert.SerializeObject(ListMixMsg);
                        //string content = JsonConvert.SerializeObject(mixData);
                        #endregion

                        #region 消息插入数据库构造
                        SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase Mix = new AntSdkChatMsg.ChatBase();
                        Mix.MsgType = AntSdkMsgType.ChatMsgMixMessage;
                        Mix.chatIndex = maxChatindex.ToString();
                        Mix.sourceContent = contentMix;

                        Mix.messageId = messageId;
                        Mix.sendUserId = s_ctt.sendUserId;
                        Mix.sessionId = s_ctt.sessionId;
                        Mix.targetId = s_ctt.targetId;
                        Mix.SENDORRECEIVE = "0";
                        Mix.sendsucessorfail = 0;
                        Mix.flag = 0;
                        var resultMix = t_chat.Insert(Mix);
                        #endregion
                        //图文混合展示
                        if (resultMix == 1)
                        {

                        }
                        if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                        {
                            SendMsgListPointMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                        }
                        else
                        {
                            SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                        }
                        if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(messageId))
                            OnceSendMessage.OneToOne.OnceMsgList.Add(messageId, mixMsg);
                        OneRightSendPicAndText.RightSendPicAndTextMix(chromiumWebBrowser, messageId, mixMsg, arg, mixMsgClass);
                        //图片上传
                        //List<PictureAndTextMixDto> listPicAndText = picAndTxtMix.Where(m => m.type == PictureAndTextMixEnum.Image).ToList();
                        var url = AntSdkService.AntSdkConfigInfo.AntSdkMultiFileUpload;
                        //var url = "http://ftp.71chat.com/platform/file/v1/file/batch/upload";
                        CurrentChatDto currentChat = new CurrentChatDto();
                        currentChat.type = AntSdkchatType.Point;
                        currentChat.messageId = messageId;
                        currentChat.sendUserId = s_ctt.sendUserId;
                        currentChat.sessionId = s_ctt.sessionId; ;
                        currentChat.targetId = s_ctt.targetId;
                        //new MultiFileUpload().MultiFileHttpClientUpLoad(AntSdkMsgType.ChatMsgMixImageText, url, listPicAndText, currentChat, picAndTxtMix, arg);
                        sendMixPicAndText(AntSdkMsgType.ChatMsgMixMessage, currentChat, arg, mixMsgClass, mixMsg);
                        _richTextBox.Document.Blocks.Clear();
                        if (e != null)
                        {
                            e.Handled = true;
                        }
                    }
                    IsFirst = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_sendMsg]" + ex.Message + ex.StackTrace + ex.Source);
                IsFirst = false;
            }
        }
        /// <summary>
        /// 图文混排重发机制
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listPicAndText"></param>
        /// <param name="currentChat"></param>
        /// <param name="picAndTxtMix"></param>
        /// <param name="arg"></param>
        public void sendMixPicAndText(AntSdkMsgType type, CurrentChatDto currentChat, MessageStateArg arg, MixMsg mixMsg, List<MixMessageObjDto> obj)
        {
            new MultiFileUpload().MultiFileHttpClientUpLoad(AntSdkMsgType.ChatMsgAt, currentChat, arg, mixMsg, obj);
        }
        /// <summary>
        /// 文本消息包装
        /// </summary>
        /// <param name="strMessage">消息内容</param>
        /// <param name="sessionId">会话ID</param>
        /// <param name="isAutoReply">是否是自动回复消息</param>
        public void TextMessage(string strMessage, string sessionId, System.Windows.Input.KeyEventArgs e, string SendStr, string isShowBurn, bool isAutoReply = false)
        {
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, sessionId);

            //发送中提示
            AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
            failMessage.preChatIndex = maxChatindex;
            failMessage.mtp = (int)GlobalVariable.MsgType.Text;
            failMessage.content = strMessage.TrimEnd('\n').TrimEnd('\r');
            failMessage.sessionid = sessionId;
            DateTime dt = DateTime.Now;
            failMessage.lastDatetime = dt.ToString();


            string messageid = PublicTalkMothed.timeStampAndRandom();
            string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
            string imageSendingId = "sending" + imageTipId;
            if (isShowBurn == "Visible" || isAutoReply || user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
            {
                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                if (!listChatIndex.Contains(messageid))
                {
                    listChatIndex.Add(messageid);
                    if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                    {
                        SendMsgListPointMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                    }
                    else
                    {
                        SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                    }
                    updateFailMessage(failMessage, "");
                    PublicTalkMothed.rightSendText(chromiumWebBrowser, SendStr.Replace("\r\n", "<br/>"),
                        messageid, imageTipId, imageSendingId, dt, strMessage, IsRobot);

                    #region 消息状态监控
                    MessageStateArg arg = new MessageStateArg();
                    arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                    arg.isGroup = false;
                    arg.MessageId = messageid;
                    arg.SessionId = sessionId;
                    arg.WebBrowser = chromiumWebBrowser;
                    arg.SendIngId = imageSendingId;
                    arg.RepeatId = imageTipId;
                    var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
                    if (IsHave != null)
                    {
                        SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                    }
                    SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                    #endregion

                    #region 2017-04-08 改写 同步方法
                    //sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId,
                    //    failMessage, e);
                    #endregion
                    #region  异步方法 修改：姚伦海
                    ThreadPool.QueueUserWorkItem(
                     m =>
                         sendTextMethod(strMessage.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId,
                             imageSendingId, failMessage, e, false));
                    #endregion
                }
            }
            else
            {
                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                if (!listChatIndex.Contains(messageid))
                {
                    listChatIndex.Add(messageid);
                    if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                    {
                        SendMsgListPointMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                    }
                    else
                    {
                        SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                    }
                    updateFailMessage(failMessage, "");
                    PublicTalkMothed.rightSendTextBurn(chromiumWebBrowser, SendStr.Replace("\r\n", "<br/>"), messageid, imageTipId, imageSendingId, dt, strMessage);
                    #region 消息状态监控
                    MessageStateArg arg = new MessageStateArg();
                    arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                    arg.isGroup = false;
                    arg.MessageId = messageid;
                    arg.SessionId = sessionId;
                    arg.WebBrowser = chromiumWebBrowser;
                    arg.SendIngId = imageSendingId;
                    arg.RepeatId = imageTipId;
                    var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
                    if (IsHave != null)
                    {
                        SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                    }
                    SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                    #endregion
                    #region 2017-04-08 改写 同步方法
                    //sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId, failMessage, e);
                    #endregion
                    #region  异步方法 修改：姚伦海
                    ThreadPool.QueueUserWorkItem(
                        m =>
                            sendTextMethod(strMessage.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId,
                                imageSendingId, failMessage, e, false));
                    #endregion
                }
            }
        }

        /// <summary>
        /// 发送Text消息
        /// </summary>
        /// <param name="msgStr"></param>
        /// <param name="messageid"></param>
        /// <param name="imageTipId"></param>
        /// <param name="imageSendingId"></param>
        /// <param name="failMessage"></param>
        /// <param name="sendMessage"></param>
        public void sendTextMethod(string msgStr, string messageid, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failMessage, System.Windows.Input.KeyEventArgs e, bool isOnceSendMsg)
        {
            //TODO:AntSdk_Modify:Example
            #region 消息发送 同步方法
            //var errorMsg = string.Empty;
            ////bool b = SDK.AntSdk.AntSdkService.SdkPublishChatMsg(sendText, ref errorMsg);
            ////TODO:AntSdk_Modify
            ////DONE:AntSdk_Modify
            //AntSdkChatMsg.Text sendAtText = new AntSdkChatMsg.Text();
            //sendAtText.content = msgStr.TrimEnd('\n').TrimEnd('\r');
            //sendAtText.MsgType = AntSdkMsgType.ChatMsgText;
            //sendAtText.messageId = messageid;
            //sendAtText.flag = _isShowBurn == "Collapsed" ? 1 : 0;
            //sendAtText.sendUserId = s_ctt.sendUserId;
            //sendAtText.sessionId = s_ctt.sessionId;
            //sendAtText.targetId = s_ctt.targetId;
            //sendAtText.chatType = (int)AntSdkchatType.Point;

            //#region 插入数据
            ////TODO:AntSdk_Modify
            //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(/*smt*/sendAtText));
            ////sm_ctt.MTP = "1";
            //sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
            //sm_ctt.sendsucessorfail = 0;
            //sm_ctt.SENDORRECEIVE = "1";
            //sm_ctt.sourceContent = sendAtText.content;
            ////2017-03-03 添加数据
            //if (_isShowBurn == "Collapsed")
            //{
            //    sm_ctt.flag = 1;
            //}
            //#endregion
            //bool result = ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
            //if (result)
            //{
            //    var isResult = AntSdkService.SdkPublishChatMsg(sendAtText, ref errorMsg);
            //    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);
            //    //发送成功
            //    if (isResult)
            //    {
            //        //更改发送状态
            //        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            //        t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

            //        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
            //        updateFailMessage(failMessage, s_ctt.targetId);
            //    }
            //    else
            //    {
            //        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
            //        PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, imageTipId);
            //        //发送失败回调方法
            //        updateFailMessage(failMessage, s_ctt.targetId);
            //    }
            //}
            //if (msgStr.Trim() != "")
            //{
            //    richTextBox.Document.Blocks.Clear();
            //}
            //if (e != null)
            //{
            //    e.Handled = true;
            //}
            #endregion

            #region 消息发送NEW 采用线程池方式  修改：姚伦海
            var errorMsg = string.Empty;
            //bool b = SDK.AntSdk.AntSdkService.SdkPublishChatMsg(sendText, ref errorMsg);
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            bool result = false;
            AntSdkChatMsg.Text sendAtText = new AntSdkChatMsg.Text();

            sendAtText.content = msgStr.TrimEnd('\n').TrimEnd('\r');
            sendAtText.MsgType = AntSdkMsgType.ChatMsgText;
            sendAtText.messageId = messageid;
            sendAtText.flag = _isShowBurn == "Collapsed" && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId ? 1 : 0;
            sendAtText.sendUserId = s_ctt.sendUserId;
            sendAtText.sessionId = s_ctt.sessionId;
            sendAtText.targetId = s_ctt.targetId;
            sendAtText.chatType = (int)AntSdkchatType.Point;

            #region 插入数据
            //TODO:AntSdk_Modify
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(/*smt*/sendAtText));
            //sm_ctt.MTP = "1";
            sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
            sm_ctt.sendsucessorfail = 0;
            sm_ctt.SENDORRECEIVE = "1";
            sm_ctt.sourceContent = sendAtText.content;
            //2017-03-03 添加数据
            if (failMessage.isOnceSendMsg)
            {
                if (failMessage.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                {
                    sm_ctt.flag = 1;
                }
                else
                {
                    sm_ctt.flag = 0;
                }
            }
            if (failMessage.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
            {
                sm_ctt.flag = 1;
            }
            if (isOnceSendMsg == false)
            {
                addData(sm_ctt);
                if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(messageid))
                    OnceSendMessage.OneToOne.OnceMsgList.Add(messageid, sendAtText);
                result = AntSdkService.SdkPublishChatMsg(sendAtText, ref errorMsg);
            }
            #endregion
            else
            {
                var content = OnceSendMessage.OneToOne.OnceMsgList.SingleOrDefault(m => m.Key == messageid).Value as AntSdkChatMsg.Text;
                if (content != null)
                {
                    sendAtText = content;
                }
                result = AntSdkService.SdkRePublishChatMsg(sendAtText, ref errorMsg);
            }

            //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);
            //发送成功
            if (result)
            {
                //发送自动回复消息
                SendAutoReplyMessage();
                //更改发送状态

                #region 2017-08-29 重发机制更改屏蔽

                //T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                //t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

                //failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                //PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
                //updateFailMessage(failMessage, s_ctt.targetId);

                #endregion
            }
            else
            {
                //failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                //PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
                //PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, imageTipId);
                ////发送失败回调方法
                //updateFailMessage(failMessage, s_ctt.targetId);
            }
            if (e != null)
            {
                e.Handled = true;
            }
            #endregion
        }
        /// <summary>
        /// 发送自动回复消息
        /// </summary>
        private void SendAutoReplyMessage()
        {
            //如果当前电脑是空闲状态，并且收到的消息未读，即发送一条自动回复
            TimeSpan time = lastReceivedMsgTime - autoReplyMsgTime;
            if ((autoReplyMsgTime == DateTime.MinValue || time.Minutes >= 10)
                && (user.state == (int)GlobalVariable.OnLineStatus.Leave || user.state == (int)GlobalVariable.OnLineStatus.Busy)
                && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
            {
                autoReplyMsgTime = DateTime.Now;
                LogHelper.WriteFatal("自动回复，当前用户：" + user.userNum + user.userName + " 在线状态：" + user.state + "最后收到消息时间：" + lastReceivedMsgTime + "自动回复时间：" + autoReplyMsgTime);
                PublicTalkMothed.AutoReplyMessage(this.chromiumWebBrowser, this.user, GlobalVariable.RevocationPrompt.AutoReplyMessage);
            }
        }

        public static Action<AntSdkFailOrSucessMessageDto, string> updateFailMessageEventHandler;

        private static void updateFailMessage(AntSdkFailOrSucessMessageDto failMessage, string targetId)
        {
            if (updateFailMessageEventHandler != null)
            {
                updateFailMessageEventHandler(failMessage, targetId);
            }
        }
        public void uploadImageSegment(string prePaths, string fileFileName, string messageId, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failOrSucess)
        {
            //SendCutImageDto scid = new SendCutImageDto();
            //scid.FailOrSucess = failOrSucess;
            //scid.cmpcd = s_ctt.companyCode;
            //scid.seId = s_ctt.sessionId;
            //scid.file = prePaths;

            //scid.fileFileName = fileFileName;


            //scid.messageId = messageId;


            //scid.imgeTipId = imageTipId;
            //scid.imageSendingId = imageSendingId;
            //System.Drawing.Bitmap pic = new System.Drawing.Bitmap(prePaths);

            //scid.imageWidth = pic.Width.ToString();
            //scid.imageHeight = pic.Height.ToString();

            AntSdkSendFileInput fileInput = new AntSdkSendFileInput();
            fileInput.preChatIndex = failOrSucess.preChatIndex;
            fileInput.messageId = messageId;
            fileInput.FailOrSucess = failOrSucess;
            fileInput.cmpcd = s_ctt.companyCode;
            fileInput.seId = s_ctt.sessionId;
            fileInput.file = prePaths;

            fileInput.fileFileName = fileFileName;


            fileInput.imgeTipId = imageTipId;
            fileInput.imageSendingId = imageSendingId;
            try
            {
                System.Drawing.Bitmap pic = new System.Drawing.Bitmap(prePaths);

                fileInput.imageWidth = pic.Width.ToString();
                fileInput.imageHeight = pic.Height.ToString();
                fileInput.isOnceSendMsg = failOrSucess.isOnceSendMsg;
                pic.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("图片消息发送异常uploadImageSegment---------------文件路径：" + prePaths);
            }

            BackgroundWorker back = new BackgroundWorker();

            back.RunWorkerCompleted += Back_RunWorkerCompleted;
            back.DoWork += Back_DoWork;
            back.RunWorkerAsync(fileInput);
        }
        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="tipsMsg"></param>
        /// <param name="isSendMsg">是否是发送消息提示语</param>
        public void showTextMethod(string tipsMsg, bool isSendMsg = true)
        {
            if (!string.IsNullOrEmpty(tipsMsg))
                showText = tipsMsg;
            if (IsShowTip.IsEnabled)
            {
                if (isSendMsg)
                    isShowPopup = false;
                else
                    IsRevocationShowPopup = false;
            }
            else
            {
                IsShowTip.Interval = TimeSpan.FromMilliseconds(1500);
                IsShowTip.Tick += IsShowTip_Tick;
                IsShowTip.Start();
                if (isSendMsg)
                    isShowPopup = true;
                else
                    IsRevocationShowPopup = true;
            }
        }

        private void IsShowTip_Tick(object sender, EventArgs e)
        {
            IsRevocationShowPopup = false;
            isShowPopup = false;
            IsShowTip.Stop();
        }
        #endregion
        #endregion
        #region 接收消息
        /// <summary>
        /// 收到阅后即焚回执消息逻辑处理
        /// </summary>
        /// <param name="mtp"></param>
        /// <param name="msg"></param>
        public void HideMsgAndShowRecallMsg(string messageid)
        {
            var task = chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.hideDivById(messageid));
            task.Wait();
            var tasks = chromiumWebBrowser.EvaluateScriptAsync("setscross();");
        }
        public class contents
        {
            public string readIndex { set; get; }
        }
        public void receiveMsg(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            try
            {
                var task = chromiumWebBrowser.EvaluateScriptAsync("getScroolPosition();");
                task.Wait();
                if (task.Result.Success)
                {
                    if ((bool)task.Result.Result == true)
                    {
                        msg.IsSetImgLoadComplete = true;
                    }
                }
                listChatIndex.Add(msg.messageId);
                //string pathImage = user.picture + "" == "" ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") : user.picture;
                #region 获取接收者头像 New
                string pathImage = "";
                //获取接收者头像
                var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
                if (listUser == null)
                {
                    AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
                    if (cus == null)
                    {
                        user = new AntSdkContact_User();
                        pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                        user.copyPicture = pathImage;
                        user.userName = "离职人员";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(cus.picture + ""))
                        {
                            pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                            user.copyPicture = pathImage;
                        }
                        else
                        {
                            pathImage = cus.picture;
                            user.copyPicture = pathImage;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(listUser.Url + ""))
                    {
                        pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                        user.copyPicture = pathImage;
                    }
                    else
                    {
                        pathImage = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
                        user.copyPicture = pathImage;
                    }
                }
                #endregion
                switch (msg.MsgType)
                {
                    case AntSdkMsgType.ChatMsgMixMessage:
                        #region 图文混合消息
                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            //目前为止没有此类消息
                        }
                        else
                        {
                            List<string> imageId = new List<string>();
                            imageId.Clear();
                            #region 图文混合
                            List<AntSdkChatMsg.MixMessage_content> receives = JsonConvert.DeserializeObject<List<AntSdkChatMsg.MixMessage_content>>(msg.sourceContent).Where(m => m.type == "1002").ToList();
                            foreach (var ilist in receives)
                            {
                                string imgId = "RL" + Guid.NewGuid().ToString();
                                imageId.Add(imgId);
                                PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, imgId, content.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                            }
                            #endregion
                            OneRightSendPicAndText.LeftShowPicAndTextMix(chromiumWebBrowser, msg, user, imageId);
                        }
                        #endregion
                        break;
                    case AntSdkMsgType.ChatMsgText:
                        #region  文本消息
                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.rightBurnAfterText(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                PublicTalkMothed.rightShowText(chromiumWebBrowser, msg);
                            }
                        }
                        else
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.leftBurnAfterText(chromiumWebBrowser, msg, user);
                            }
                            else
                            {
                                PublicTalkMothed.leftShowText(chromiumWebBrowser, msg, user);
                            }
                        }
                        #endregion
                        break;
                    case AntSdkMsgType.ChatMsgPicture:
                        #region 图片消息
                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            #endregion
                            if (msg.flag == 1)
                            {
                                SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, msg.messageId, rimgDto.picUrl, "", burnMsg.isBurnMsg.yesBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                PublicTalkMothed.rightBurnAfterImage(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, msg.messageId, rimgDto.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                PublicTalkMothed.rightShowImage(chromiumWebBrowser, msg);
                            }
                        }
                        else
                        {
                            if (msg.flag == 1)
                            {
                                burnMsg.IsReadImg bImg;
                                burnMsg.IsEffective isEffective;
                                if (msg.readtime != "")
                                {
                                    bImg = burnMsg.IsReadImg.read;
                                    isEffective = burnMsg.IsEffective.effective;
                                }
                                else
                                {
                                    bImg = burnMsg.IsReadImg.notRead;
                                    isEffective = burnMsg.IsEffective.NotEffective;
                                }
                                SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, msg.messageId, rimgDto.picUrl, "",
                                    burnMsg.isBurnMsg.yesBurn, bImg, isEffective);
                                PublicTalkMothed.leftBurnAfterImage(chromiumWebBrowser, msg, user);
                            }
                            else
                            {
                                burnMsg.IsReadImg bImg;
                                if (msg.readtime != "")
                                {
                                    bImg = burnMsg.IsReadImg.read;
                                }
                                else
                                {
                                    bImg = burnMsg.IsReadImg.notRead;
                                }
                                SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, msg.messageId, rimgDto.picUrl, "", burnMsg.isBurnMsg.notBurn, bImg, burnMsg.IsEffective.UnKnow);
                                PublicTalkMothed.leftShowImage(chromiumWebBrowser, msg, user);
                            }
                        }

                        break;
                    case AntSdkMsgType.ChatMsgFile:
                        #region 文件消息
                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.rightBurnAfterFile(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                PublicTalkMothed.rightShowFile(chromiumWebBrowser, msg);
                            }
                        }
                        else
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.leftBurnAfterFile(chromiumWebBrowser, msg, user);
                            }
                            else
                            {
                                PublicTalkMothed.leftShowFile(chromiumWebBrowser, msg, user);
                            }
                        }
                        #endregion
                        break;
                    case AntSdkMsgType.ChatMsgAudio:
                        #region  MP3
                        mp3Dto msgDto = JsonConvert.DeserializeObject<mp3Dto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.RightBurnAfterVoice(this.chromiumWebBrowser, msg);
                            }
                            else
                            {
                                PublicTalkMothed.rightShowVoice(this.chromiumWebBrowser, msg);
                            }
                        }
                        else
                        {
                            if (msg.flag == 1)
                            {
                                PublicTalkMothed.LeftBurnAfterVoice(this.chromiumWebBrowser, msg, user);
                            }
                            else
                            {
                                PublicTalkMothed.leftShowVoice(this.chromiumWebBrowser, msg, user);
                            }
                        }
                        #endregion
                        break;
                    case AntSdkMsgType.Revocation:
                        chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.InsertUIRecallMsg(msg.sourceContent));
                        break;
                    case AntSdkMsgType.PointAudioVideo:
                        {
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                PublicTalkMothed.rightShowAudio(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                PublicTalkMothed.LefttShowAudio(chromiumWebBrowser, msg, user);
                            }
                        }
                        break;
                }

                if (task.Result.Success)
                {
                    if ((bool)task.Result.Result == true)
                    {
                        chromiumWebBrowser.EvaluateScriptAsync("setscross();");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel]_[receiveMsg]" + ex.Message + ex.StackTrace + /*TODO:AntSdk_Modify:msg.content*/ msg.sourceContent == null ? "" : /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            }
        }
        #endregion
        /// <summary>
        /// 插入图片地址
        /// </summary>
        /// <param name="preOrEnd"></param>
        /// <param name="chatIndex"></param>
        /// <param name="imageUrl"></param>
        public void AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd preOrEnd, string chatIndex, string imageUrl, string imageId, burnMsg.isBurnMsg isBurn, burnMsg.IsReadImg isRead, burnMsg.IsEffective isEffective)
        {
            AddImgUrlDto addImg = new AddImgUrlDto();
            switch (preOrEnd)
            {
                case AddImgUrlDto.InsertPreOrEnd.End:
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imageUrl;
                    addImg.ImageId = imageId;
                    addImg.PreOrEnd = AddImgUrlDto.InsertPreOrEnd.End;
                    if (isBurn == burnMsg.isBurnMsg.yesBurn)
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.yesBurn;
                        addImg.IsEffective = isEffective;
                    }
                    else
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.notBurn;
                    }
                    if (isRead == burnMsg.IsReadImg.read)
                    {
                        addImg.IsRead = burnMsg.IsReadImg.read;
                    }
                    else
                    {
                        addImg.IsRead = burnMsg.IsReadImg.notRead;
                    }
                    listDictImgUrls.Add(addImg);
                    break;
                case AddImgUrlDto.InsertPreOrEnd.Pre:
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imageUrl;
                    addImg.PreOrEnd = AddImgUrlDto.InsertPreOrEnd.Pre;
                    if (isBurn == burnMsg.isBurnMsg.yesBurn)
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.yesBurn;
                        addImg.IsEffective = isEffective;
                    }
                    else
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.notBurn;
                    }
                    if (isRead == burnMsg.IsReadImg.read)
                    {
                        addImg.IsRead = burnMsg.IsReadImg.read;
                    }
                    else
                    {
                        addImg.IsRead = burnMsg.IsReadImg.notRead;
                    }
                    listDictImgUrls.Insert(0, addImg);
                    break;
            }
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="cmr"></param>
        public void addData(AntSdkChatMsg.ChatBase cmr)
        {
            AntSdkChatMsg.ChatBase receive = cmr;
            BaseBLL<AntSdkChatMsg.ChatBase, T_Chat_MessageDAL> t_chat = new BaseBLL<AntSdkChatMsg.ChatBase, T_Chat_MessageDAL>();
            if (t_chat.Insert(cmr) == 1)
            {

            }
            else
            {

            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="mainWindowParams"></param>
        /// <returns></returns>
        public void FileUploadMutis(SendCutImageDto scid)
        {
            try
            {
                string md5Value = PublicTalkMothed.getFileMd5Value(scid.file);
                if (!string.IsNullOrEmpty(md5Value))
                {
                    var errorCode = 0;
                    string errorMsg = string.Empty;
                    AntSdkFileUpLoadOutput fileOutput = AntSdkService.CompareFileMd5(md5Value, scid.fileFileName, ref errorCode, ref errorMsg);
                    if (errorCode == 10000)
                    {
                        var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == scid.messageId);
                        if (IsHave != null)
                        {
                            SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                        }
                        SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(scid.ImgOrFileArg));
                        return;
                    }
                    if ((errorMsg.Length > 0))
                    {
                        return;
                    }
                    if (fileOutput == null)
                    {
                        //string url = ConfigurationManager.AppSettings["UpLoadAddress"];
                        var url = AntSdkService.AntSdkConfigInfo.AntSdkFileUpload;
                        //string parm = string.Format("?&cmpcd={0}&seId={1}&fileFileName={2}", scid.cmpcd, scid.seId, scid.fileFileName);
                        string parm = $"?&key={20000}&requestTime={(DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000}&File={"file"}&token={AntSdkService.AntSdkToken}";
                        HttpWebClient<SendCutImageDto> client = new HttpWebClient<SendCutImageDto>();
                        client.Encoding = Encoding.UTF8;


                        client.UploadProgressChanged += Client_UploadProgressChanged;
                        client.UploadFileCompleted += Client_UploadFileCompleted;
                        client.UploadFileAsync(new Uri(url + parm), "POST", scid.file);
                        //client.backGround = scid.back;
                        client.obj = scid;
                    }
                    else
                    {
                        FileDto fDto = new FileDto();

                        #region 消息发送
                        SendMessageDto smg = new SendMessageDto();
                        SendMessage_ctt smt = new SendMessage_ctt();
                        OnceSendFile sendFile = null;
                        smt.flag = 0;
                        if (scid.FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                        {
                            smt.flag = 1;
                        }
                        else
                        {
                            smt.flag = 0;
                        }
                        if (scid.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            if (scid.FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                            {
                                smt.flag = 1;
                            }
                            else
                            {
                                smt.flag = 0;
                            }
                            sendFile = OnceSendMessage.OneToOne.OnceMsgList.SingleOrDefault(m => m.Key == scid.messageId).Value as OnceSendFile;
                            smt.sendUserId = sendFile.ctt.sendUserId;
                            smt.sessionId = sendFile.ctt.sessionId;
                            smt.targetId = sendFile.ctt.targetId;
                        }
                        else
                        {
                            smt.sendUserId = s_ctt.sendUserId;
                            smt.sessionId = s_ctt.sessionId;
                            smt.targetId = s_ctt.targetId;
                        }
                        if (user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                        {
                            smt.flag = 0;
                        }
                        smt.MsgType = AntSdkMsgType.ChatMsgFile;
                        string name = fileOutput.fileName;
                        fDto.fileName = name;
                        fDto.fileUrl = fileOutput.dowmnloadUrl;
                        fDto.size = fileOutput.fileSize;
                        fDto.fileExtendName = name.Substring(name.LastIndexOf("."), name.Length - name.LastIndexOf("."));
                        smt.content = JsonConvert.SerializeObject(fDto);
                        smt.companyCode = s_ctt.companyCode;
                        smt.sourceContent = JsonConvert.SerializeObject(fDto);
                        //smt.messageId = Guid.NewGuid().ToString();

                        //2017-03-16 修改
                        //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                        smt.messageId = scid.messageId;


                        smg.ctt = smt;
                        string error = "";
                        //TODO:AntSdk_Modify
                        //DONE:AntSdk_Modify
                        AntSdkChatMsg.File fileMsg = new AntSdkChatMsg.File();
                        //if (user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                        //{
                        fileMsg.flag = smt.flag;
                        //}
                        AntSdkChatMsg.File_content fileContent = new AntSdkChatMsg.File_content();
                        fileContent.fileName = scid.fileFileName;
                        fileContent.size = scid.filesize;
                        fileContent.fileUrl = fileOutput.dowmnloadUrl;
                        fileMsg.content = fileContent;
                        fileMsg.MsgType = AntSdkMsgType.ChatMsgFile;
                        fileMsg.chatType = (int)AntSdkchatType.Point;
                        fileMsg.messageId = scid.messageId;
                        if (scid.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            fileMsg.sendUserId = sendFile.ctt.sendUserId;
                            fileMsg.sessionId = sendFile.ctt.sessionId;
                            fileMsg.targetId = sendFile.ctt.targetId;
                        }
                        else
                        {
                            fileMsg.sendUserId = s_ctt.sendUserId;
                            fileMsg.sessionId = s_ctt.sessionId;
                            fileMsg.targetId = s_ctt.targetId;
                        }
                        bool isResult = false;
                        if (scid.from != AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(scid.ImgOrFileArg));
                            isResult = AntSdkService.SdkPublishChatMsg(fileMsg, ref error);
                        }
                        else
                        {
                            SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(scid.ImgOrFileArg));
                            isResult = AntSdkService.SdkRePublishChatMsg(fileMsg, ref error);
                        }
                        if (isResult)
                        {
                            //发送自动回复消息
                            SendAutoReplyMessage();
                            //更改上传成功路径
                            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                            t_chat.UpdateContent(scid.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, smt.sourceContent);

                            StringBuilder sbFileimg = new StringBuilder();
                            sbFileimg.AppendLine("setFileImg('" + scid.imgguid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                            this.chromiumWebBrowser.ExecuteScriptAsync(sbFileimg.ToString());
                            string mstr = "setProcess('" + scid.progressId + "','" + 100 + "%');";
                            //sbEnd.AppendLine();
                            this.chromiumWebBrowser.ExecuteScriptAsync(mstr);

                            StringBuilder sbFileText = new StringBuilder();
                            sbFileText.AppendLine("setFileText('" + scid.textguid + "','上传成功');");
                            this.chromiumWebBrowser.ExecuteScriptAsync(sbFileText.ToString());
                            //scid.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                            PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, scid.imageSendingId);
                            //updateFailMessage(scid.FailOrSucess, s_ctt.targetId);
                        }
                        #endregion
                        //}
                        #region 滚动条置底
                        StringBuilder sbEnd = new StringBuilder();
                        sbEnd.AppendLine("setscross();");
                        this.chromiumWebBrowser.ExecuteScriptAsync(sbEnd.ToString());
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_FileUploadMutis]:" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 方法说明：根据键值获取Json字符串中特定字段的值
        /// 完成时间：2016-05-20
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="json">json字符串</param>
        /// <param name="value">值</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否执行成功</returns>
        private static bool GetValueByJsonKey(string key, string json, ref string value, ref string errMsg)
        {
            try
            {
                var jObject = JObject.Parse(json);
                value = jObject[key].ToString();
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// 上传成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
            //BackgroundWorker backFileUpload = hbc.backGround;
            try
            {
                if (e.Result == null)
                {
                    SendCutImageDto sucess = hbc.obj as SendCutImageDto;
                    if (sucess.from != AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        #region 滚动条置底
                        StringBuilder sbEnd = new StringBuilder();
                        sbEnd.AppendLine("setscross();");
                        var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        #endregion
                    }

                    StringBuilder sbFileimg = new StringBuilder();
                    sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());


                    StringBuilder sbFileText = new StringBuilder();
                    sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                    string str = Encoding.UTF8.GetString(e.Result);
                    //UpLoadFilesDto rct = JsonConvert.DeserializeObject<UpLoadFilesDto>(Encoding.UTF8.GetString(e.Result));

                    //backFileUpload.Dispose();
                    LogHelper.WriteError("[TalkViewModel_Client_UploadFileCompleted]:null");
                }
                else
                {
                    #region 滚动条置底
                    StringBuilder sbEnd = new StringBuilder();
                    sbEnd.AppendLine("setscross();");
                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    #endregion

                    //HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
                    SendCutImageDto sucess = hbc.obj as SendCutImageDto;
                    string str = Encoding.UTF8.GetString(e.Result);
                    var dataStr = string.Empty;
                    var temperrorMsg = string.Empty;
                    if (!GetValueByJsonKey("data", str, ref dataStr, ref temperrorMsg))
                    {
                        LogHelper.WriteError($"[NoticeAddWindowViewModel_NoticeFileUploadReturnError data is null]:{temperrorMsg}");
                        return;
                    }
                    //处理
                    var upoutput = JsonConvert.DeserializeObject<AntSdkFileUpLoadOutput>(dataStr);

                    FileDto fDto = JsonConvert.DeserializeObject<FileDto>(str);

                    #region 消息发送
                    SendMessageDto smg = new SendMessageDto();
                    SendMessage_ctt smt = new SendMessage_ctt();
                    if (sucess.FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn)
                    {
                        smt.flag = 1;
                    }
                    smt.MsgType = AntSdkMsgType.ChatMsgFile;
                    fDto.fileUrl = upoutput.dowmnloadUrl;
                    fDto.fileName = sucess.fileFileName;
                    fDto.size = sucess.filesize;
                    fDto.fileExtendName = sucess.fileFileExtendName;
                    smt.content = JsonConvert.SerializeObject(fDto);
                    smt.companyCode = s_ctt.companyCode;
                    smt.sourceContent = JsonConvert.SerializeObject(fDto);
                    //smt.messageId = Guid.NewGuid().ToString();

                    //2017-03-16 修改
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.messageId = sucess.messageId;
                    OnceSendFile sendFile = null;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        sendFile = OnceSendMessage.OneToOne.OnceMsgList.SingleOrDefault(m => m.Key == sucess.messageId).Value as OnceSendFile;
                        smt.sendUserId = sendFile.ctt.sendUserId;
                        smt.sessionId = sendFile.ctt.sessionId;
                        smt.targetId = sendFile.ctt.targetId;
                    }
                    else
                    {
                        smt.sendUserId = s_ctt.sendUserId;
                        smt.sessionId = s_ctt.sessionId;
                        smt.targetId = s_ctt.targetId;
                    }
                    smg.ctt = smt;
                    string error = "";
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.File fileMsg = new AntSdkChatMsg.File();
                    if (user.userId == AntSdkService.AntSdkCurrentUserInfo.robotId)
                    {
                        fileMsg.flag = 0;
                    }
                    else
                    {
                        fileMsg.flag = smt.flag;
                    }
                    AntSdkChatMsg.File_content fileContent = new AntSdkChatMsg.File_content();
                    fileContent.fileName = sucess.fileFileName;
                    fileContent.size = sucess.filesize;
                    fileContent.fileUrl = upoutput.dowmnloadUrl;
                    fileMsg.content = fileContent;
                    fileMsg.MsgType = AntSdkMsgType.ChatMsgFile;
                    fileMsg.chatType = (int)AntSdkchatType.Point;
                    fileMsg.messageId = sucess.messageId;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        fileMsg.sendUserId = sendFile.ctt.sendUserId;
                        fileMsg.sessionId = sendFile.ctt.sessionId;
                        fileMsg.targetId = sendFile.ctt.targetId;
                    }
                    else
                    {
                        fileMsg.sendUserId = s_ctt.sendUserId;
                        fileMsg.sessionId = s_ctt.sessionId;
                        fileMsg.targetId = s_ctt.targetId;
                    }
                    bool isResult = false;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(sucess.ImgOrFileArg));
                        isResult = AntSdkService.SdkRePublishChatMsg(fileMsg, ref error);
                    }
                    else
                    {
                        SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(sucess.ImgOrFileArg));
                        isResult = AntSdkService.SdkPublishChatMsg(fileMsg, ref error);
                    }
                    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);
                    if (isResult)
                    {
                        T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                        t_chat.UpdateContent(sucess.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, smt.sourceContent);
                        //发送自动回复消息
                        SendAutoReplyMessage();
                        if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            var cef = OnceSendMessage.OneToOne.CefList.SingleOrDefault(m => m.Key == sucess.FailOrSucess.sessionid).Value as ChromiumWebBrowsers;
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, sucess.messageId);
                            StringBuilder sbFileimg = new StringBuilder();
                            sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                            cef.EvaluateScriptAsync(sbFileimg.ToString());
                            //this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());

                            sbEnd.AppendLine("setProcess('" + sucess.progressId + "','" + 100 + "%');");
                            cef.EvaluateScriptAsync(sbEnd.ToString());

                            StringBuilder sbFileText = new StringBuilder();
                            sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传成功');");
                            cef.EvaluateScriptAsync(sbFileText.ToString());
                        }
                        else
                        {
                            //更改发送状态
                            //T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                            //t_chat.UpdateSendMsgState(sucess.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

                            StringBuilder sbFileimg = new StringBuilder();
                            sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());

                            sbEnd.AppendLine("setProcess('" + sucess.progressId + "','" + 100 + "%');");
                            var progress = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());

                            StringBuilder sbFileText = new StringBuilder();
                            sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传成功');");
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                            sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                            PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, sucess.imageSendingId);
                            updateFailMessage(sucess.FailOrSucess, s_ctt.targetId);
                        }
                    }
                    else
                    {
                        if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {

                        }
                        else
                        {
                            StringBuilder sbFileimg = new StringBuilder();
                            sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());

                            sbEnd.AppendLine("setProcess('" + sucess.progressId + "','" + 0 + "%');");
                            var progressfail = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());

                            StringBuilder sbFileText = new StringBuilder();
                            sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());

                            sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, sucess.imageSendingId);
                            PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, sucess.imgeTipId);
                            updateFailMessage(sucess.FailOrSucess, s_ctt.targetId);
                        }
                    }
                    //}
                    #endregion
                    //}
                    //backFileUpload.Dispose();
                }
            }
            catch (Exception ex)
            {
                PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, hbc.obj.imgeTipId);

                #region 滚动条置底
                StringBuilder sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                #endregion

                //HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
                SendCutImageDto sucess = hbc.obj as SendCutImageDto;

                StringBuilder sbFileimg = new StringBuilder();
                sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());


                StringBuilder sbFileText = new StringBuilder();
                sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());

                //上传失败回调
                sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                updateFailMessage(sucess.FailOrSucess, "");

                //backFileUpload.Dispose();
                LogHelper.WriteError("[TalkViewModel_Client_UploadFileCompleted]:" + ex.Message + ex.StackTrace + ex.Source);
            }

        }
        /// <summary>
        /// 上传文件进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            try
            {
                //Thread.Sleep(100);
                HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
                StringBuilder sbEnd = new StringBuilder();
                SendCutImageDto dtos = hbc.obj as SendCutImageDto;
                string proId = dtos.progressId.ToString();
                sbEnd.AppendLine("setProcess('" + proId + "','" + e.ProgressPercentage + "%');");
                var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_Client_UploadProgressChanged]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 取消上传
        /// </summary>
        public ICommand btnCanelUpLoadFile
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    try
                    {
                        //showFile.Height = new GridLength(0);
                        FileUploadShowHeight = 0;
                        _wrapPanel.Children.Clear();
                        upFileList.Clear();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkViewModel_btnCanelUpLoadFile]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }
        public static object objupload = new object();
        /// <summary>
        /// 文件发送
        /// </summary>
        public ICommand btnSendUploadFile
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    try
                    {
                        if (upFileList.Count() > 0)
                        {
                            //查询数据库最大chatindex
                            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                            int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, this.s_ctt.sessionId);

                            List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> lists = new List<AntSdkChatMsg.ChatBase>();
                            //构造入库
                            foreach (var listfile in upFileList)
                            {
                                if (!PublicTalkMothed.IsFileInUsing(listfile.localOrServerPath))
                                {
                                    FileDto fDto = new FileDto();
                                    SendMessageDto smg = new SendMessageDto();
                                    SendMessage_ctt smt = new SendMessage_ctt();
                                    if (_isShowBurn == "Collapsed" && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
                                    {
                                        smt.flag = 1;
                                    }
                                    smt.MsgType = AntSdkMsgType.ChatMsgFile;
                                    string name = listfile.fileName;
                                    fDto.fileName = name;
                                    fDto.fileUrl = listfile.localOrServerPath.Replace(@"\", "/"); ;
                                    fDto.size = listfile.fileSize;
                                    fDto.fileExtendName = name.Substring(name.LastIndexOf("."), name.Length - name.LastIndexOf("."));
                                    smt.content = JsonConvert.SerializeObject(fDto);
                                    smt.companyCode = s_ctt.companyCode;
                                    smt.sourceContent = JsonConvert.SerializeObject(fDto);


                                    smt.messageId = listfile.messageId;

                                    smt.sendUserId = s_ctt.sendUserId;
                                    smt.sessionId = s_ctt.sessionId;
                                    smt.targetId = s_ctt.targetId;
                                    smg.ctt = smt;

                                    SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                                    sm_ctt.uploadOrDownPath = listfile.localOrServerPath.Replace(@"\", "/");
                                    sm_ctt.chatIndex = maxChatindex.ToString();
                                    sm_ctt.sendsucessorfail = 0;
                                    sm_ctt.SENDORRECEIVE = "1";
                                    if (_isShowBurn == "Collapsed" && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
                                    {
                                        sm_ctt.flag = 1;
                                    }
                                    lists.Add(sm_ctt);
                                }
                            }
                            T_Chat_MessageDAL t_chats = new T_Chat_MessageDAL();
                            var result = t_chats.InsertBig(lists);
                            if (result)
                            {
                                //文件是否占用
                                bool isFileUse = false;
                                foreach (var listfile in upFileList)
                                {

                                    if (PublicTalkMothed.IsFileInUsing(listfile.localOrServerPath))
                                    {
                                        isFileUse = true;
                                    }
                                    OnceSendFile sendFile = new OnceSendFile();
                                    sendFile.ctt = this.s_ctt;
                                    if (!OnceSendMessage.OneToOne.OnceMsgList.ContainsKey(listfile.messageId))
                                        OnceSendMessage.OneToOne.OnceMsgList.Add(listfile.messageId, sendFile);
                                    //阅后即焚
                                    if (_isShowBurn == "Collapsed" && user.userId != AntSdkService.AntSdkCurrentUserInfo.robotId)
                                    {
                                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.imageTipId = imageTipId;

                                        DateTime dt = DateTime.Now;
                                        listfile.DtTime = dt;

                                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.progressId = progressId;

                                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.fileImgGuid = showImgGuid;

                                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.fileTextGuid = fileshowText;

                                        string fileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        listfile.fileOpenguid = fileOpenguid;

                                        string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        listfile.fileOpenDirectory = fileOpenDirectory;

                                        listfile.imageSendingId = "sending" + imageTipId;

                                        //发送中提示
                                        AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                        failMessage.preChatIndex = maxChatindex;
                                        failMessage.mtp = (int)GlobalVariable.MsgType.File;
                                        failMessage.content = "";
                                        failMessage.sessionid = s_ctt.sessionId;
                                        //DateTime dt = DateTime.Now;
                                        failMessage.lastDatetime = dt.ToString();
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;

                                        listfile.FailOrSucess = failMessage;
                                        lock (objupload)
                                        {
                                            if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(listfile.messageId))
                                            {
                                                SendMsgListPointMonitor.MsgIdAndImgSendingId[listfile.messageId] = listfile.imageSendingId;
                                            }
                                            else
                                            {
                                                SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(listfile.messageId, listfile.imageSendingId);
                                            }
                                            updateFailMessage(failMessage, "");
                                            PublicTalkMothed.rightSendFileBurn(chromiumWebBrowser, listfile);
                                            #region 消息状态监控
                                            MessageStateArg arg = new MessageStateArg();
                                            arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                            arg.isGroup = false;
                                            arg.MessageId = listfile.messageId;
                                            arg.SessionId = s_ctt.sessionId;
                                            arg.WebBrowser = chromiumWebBrowser;
                                            arg.SendIngId = listfile.imageSendingId;
                                            arg.RepeatId = imageTipId;
                                            var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == listfile.messageId);
                                            if (IsHave != null)
                                            {
                                                SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                            }
                                            listfile.ImgOrFileArg = arg;
                                            //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                            #endregion
                                            ThreadPool.QueueUserWorkItem(m => ThreadUploadFile(listfile));
                                        }
                                    }
                                    else
                                    {
                                        //listfile.messageId = ;
                                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.imageTipId = imageTipId;

                                        DateTime dt = DateTime.Now;
                                        listfile.DtTime = dt;

                                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.progressId = progressId;

                                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.fileImgGuid = showImgGuid;

                                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        listfile.fileTextGuid = fileshowText;

                                        string fileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        listfile.fileOpenguid = fileOpenguid;

                                        string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        listfile.fileOpenDirectory = fileOpenDirectory;

                                        listfile.imageSendingId = "sending" + imageTipId;

                                        //发送中提示
                                        AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                        failMessage.preChatIndex = maxChatindex;
                                        failMessage.mtp = (int)GlobalVariable.MsgType.File;
                                        failMessage.content = "";
                                        failMessage.sessionid = s_ctt.sessionId;
                                        //DateTime dt = DateTime.Now;
                                        failMessage.lastDatetime = dt.ToString();
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                        listfile.FailOrSucess = failMessage;
                                        lock (objupload)
                                        {
                                            if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(listfile.messageId))
                                            {
                                                SendMsgListPointMonitor.MsgIdAndImgSendingId[listfile.messageId] = listfile.imageSendingId;
                                            }
                                            else
                                            {
                                                SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(listfile.messageId, listfile.imageSendingId);
                                            }
                                            updateFailMessage(failMessage, "");
                                            listfile.IsRobot = IsRobot;
                                            PublicTalkMothed.rightSendFile(chromiumWebBrowser, listfile);
                                            #region 消息状态监控
                                            MessageStateArg arg = new MessageStateArg();
                                            arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                            arg.isGroup = false;
                                            arg.MessageId = listfile.messageId;
                                            arg.SessionId = s_ctt.sessionId;
                                            arg.WebBrowser = chromiumWebBrowser;
                                            arg.SendIngId = listfile.imageSendingId;
                                            arg.RepeatId = imageTipId;
                                            var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == listfile.messageId);
                                            if (IsHave != null)
                                            {
                                                SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                                            }
                                            listfile.ImgOrFileArg = arg;
                                            //SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                            #endregion
                                            ThreadPool.QueueUserWorkItem(m => ThreadUploadFile(listfile));
                                        }
                                    }

                                    #region 异步上传文件
                                   //BackgroundWorker backFileUpload = new BackgroundWorker();
                                   //backFileUpload.RunWorkerAsync(listfile);
                                   //backFileUpload.DoWork += BackFileUpload_DoWork;
                                   //backFileUpload.RunWorkerCompleted += BackFileUpload_RunWorkerCompleted;

                                   //2017-03-18 修改
                                   //ThreadPool.QueueUserWorkItem(m => ThreadUploadFile(listfile));
                                   //Task isTask = Task.Factory.StartNew(()=>ThreadUploadFile(listfile));
                                   ;

                                    #endregion
                                }

                                FileUploadShowHeight = 0;
                                _wrapPanel.Children.Clear();
                                upFileList.Clear();
                                StringBuilder sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");

                                var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                if (isFileUse == true)
                                {
                                    KeyboardHookLib.isHook = true;
                                    MessageBoxWindow.Show("温馨提示", "    文件被其他程序占用，上传失败，请检查并关闭相应的程序。", GlobalVariable.WarnOrSuccess.Warn);
                                    //if (_richTextBox.IsFocused)
                                    //    KeyboardHookLib.isHook = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkViewModel_btnSendUploadFile]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }
        /// <summary>
        /// WrapPanel初始化
        /// </summary>
        private WrapPanel _wrapPanel = new WrapPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(15) };
        public WrapPanel wrapPanel
        {
            set
            {
                _wrapPanel = value;
                RaisePropertyChanged(() => wrapPanel);
            }
            get { return _wrapPanel; }
        }
        #region 发送快捷菜单
        System.Windows.Controls.Button btn;
        public ICommand btnSetShortCuts
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    try
                    {
                        btn = obj as System.Windows.Controls.Button;

                        btn.AddHandler(System.Windows.Controls.Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Btn_MouseLeftButtonDown), true);
                        btn.MouseLeftButtonDown += Btn_MouseLeftButtonDown;
                        Btn_MouseLeftButtonDown(null, null);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkViewModel_btnSetShortCuts]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }
        private void Btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var type = GlobalVariable.systemSetting == null ? 0 : GlobalVariable.systemSetting.SendKeyType;
                System.Windows.Controls.ContextMenu btnCm = new System.Windows.Controls.ContextMenu();
                btnCm.PlacementTarget = btn;
                System.Windows.Controls.MenuItem btnMi1 = new System.Windows.Controls.MenuItem();
                btnMi1.MouseLeftButtonDown += BtnMi1_MouseLeftButtonDown;
                btnMi1.AddHandler(System.Windows.Controls.MenuItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(BtnMi1_MouseLeftButtonDown), true);
                if (type == 0)
                {
                    Image image1 = new Image();
                    image1.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/images/勾选1.png"));
                    btnMi1.Icon = image1;
                }
                btnMi1.Header = "按Enter键发送消息";
                btnCm.Items.Add(btnMi1);


                System.Windows.Controls.MenuItem btnMi2 = new System.Windows.Controls.MenuItem();
                btnMi2.MouseLeftButtonDown += BtnMi2_MouseLeftButtonDown;
                btnMi2.AddHandler(System.Windows.Controls.MenuItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(BtnMi2_MouseLeftButtonDown), true);
                if (type == 1)
                {
                    Image image2 = new Image();
                    image2.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/images/勾选1.png"));
                    btnMi2.Icon = image2;
                }
                btnMi2.Header = "按Ctrl+Enter键发送消息";
                btnCm.Items.Add(btnMi2);
                btn.ContextMenu = btnCm;
                btnCm.IsOpen = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_Btn_MouseLeftButtonDown]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        private void BtnMi2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GlobalVariable.systemSetting.SendKeyType = 1;
        }

        private void BtnMi1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GlobalVariable.systemSetting.SendKeyType = 0;
        }
        #endregion
        private bool _isShowPopup;
        public bool isShowPopup
        {
            get { return _isShowPopup; }
            set
            {
                this._isShowPopup = value;
                RaisePropertyChanged(() => isShowPopup);
            }
        }
        private bool _isRevocationShowPopup;
        /// <summary>
        /// 撤销提示
        /// </summary>
        public bool IsRevocationShowPopup
        {
            get { return _isRevocationShowPopup; }
            set
            {
                _isRevocationShowPopup = value;
                RaisePropertyChanged(() => IsRevocationShowPopup);
            }
        }
        /// <summary>
        /// 提示信息
        /// </summary>
        private string _showText;
        public string showText
        {
            get { return _showText; }
            set
            {
                this._showText = value;
                RaisePropertyChanged(() => showText);
            }
        }
        private int _hOffset;
        public int hOffset
        {
            get { return _hOffset; }
            set
            {
                this._hOffset = value;
                RaisePropertyChanged(() => hOffset);
            }
        }
        private int _vOffset = -2;
        public int vOffset
        {
            get { return _vOffset; }
            set
            {
                this._vOffset = value;
                RaisePropertyChanged(() => vOffset);
            }
        }

        private double _FileUploadShowHeight;
        /// <summary>
        /// 发送文件控件是否可见
        /// </summary>
        public double FileUploadShowHeight
        {
            get { return this._FileUploadShowHeight; }
            set
            {
                this._FileUploadShowHeight = value;
                RaisePropertyChanged(() => FileUploadShowHeight);
            }
        }

        /// <summary>
        /// 录音控件是否显示
        /// </summary>
        private string _soundRecordShowHeight = "0";
        public string SoundRecordShowHeight
        {
            get { return this._soundRecordShowHeight; }
            set
            {
                this._soundRecordShowHeight = value;
                RaisePropertyChanged(() => SoundRecordShowHeight);
            }
        }
        /// <summary>
        /// 消息提示栏位高度
        /// </summary>
        private string _textShowRowHeight = "0";
        public string TextShowRowHeight
        {
            get { return this._textShowRowHeight; }
            set
            {
                this._textShowRowHeight = value;
                RaisePropertyChanged(() => TextShowRowHeight);
            }
        }
        /// <summary>
        /// 接收消息显示
        /// </summary>
        private string _textShowReceiveMsg;
        public string TextShowReceiveMsg
        {
            get { return this._textShowReceiveMsg; }
            set
            {
                this._textShowReceiveMsg = value;
                RaisePropertyChanged(() => TextShowReceiveMsg);
            }
        }
        /// <summary>
        /// 录音进度条
        /// </summary>
        private int _soundProValue = 0;
        public int SoundProValue
        {
            get { return this._soundProValue; }
            set
            {
                this._soundProValue = value;
                RaisePropertyChanged(() => SoundProValue);
            }
        }
    }
}
