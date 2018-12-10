using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.Contacts;
using AntennaChat.ViewModel.FileUpload;
using AntennaChat.Views;
using AntennaChat.Views.FileUpload;
using CefSharp.Wpf;
using CSharpWin_JD.CaptureImage;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using static Antenna.Model.SendMessageDto;
using System.Windows.Threading;
using AntennaChat.Views.Talk;
using AntennaChat.Views.Notice;
using AntennaChat.ViewModel.Notice;
using AntennaChat.Views.Contacts;
using CefSharp;
using System.Threading.Tasks;
using AntennaChat.Helper;
using AntennaChat.Resource;
using Newtonsoft.Json.Linq;
using static AntennaChat.ViewModel.Talk.TalkGroupViewModel.callbackId;
using AntennaChat.Helper.IHelper;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;
using Antenna.Model.OnceSendMessage;
using AntennaChat.ViewModel.Activity;
using AntennaChat.ViewModel.Vote;
using AntennaChat.Views.Vote;
using static AntennaChat.OnceSendMessage.GroupToGroup;
using Antenna.Model.PictureAndTextMix;
using AntennaChat.CefSealedHelper.OneAndGroupCommon;
using AntennaChat.CefSealedHelper.OneToGroup.PictureAndTextMix;
using static Antenna.Model.ModelEnum;
using AntennaChat.CefSealedHelper.OneToGroup.vote;
using AntennaChat.CefSealedHelper.OneToGroup.Activity;
using SDK.Service.Models;
using AntennaChat.CefSealedHelper;
using static AntennaChat.ViewModel.Talk.TalkGroupViewModel.CallbackValueById;

namespace AntennaChat.ViewModel.Talk
{
    /// <summary>
    /// 讨论组
    /// </summary>
    public class TalkGroupViewModel : PropertyNotifyObject
    {
        /// <summary>
        /// 消息字典
        /// </summary>
        Dictionary<string, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> dictionaryMsg = new Dictionary<string, AntSdkChatMsg.ChatBase>();
        /// <summary>
        /// 第一屏数据
        /// </summary>
        IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> FirstPageData = null;
        /// <summary>
        /// 第一屏阅后即焚数据
        /// </summary>
        IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> FirstPageDataBurn = null;
        /// <summary>
        /// 消息监控集合
        /// </summary>
       // public List<SendMsgStateMonitor> MessageStateMonitorList = new List<SendMsgStateMonitor>();
        /// <summary>
        /// 正常消息初始化
        /// </summary>
        public GlobalVariable.IsRun IsRun = GlobalVariable.IsRun.NotIsRun;
        /// <summary>
        /// 阅后即焚消息初始化
        /// </summary>
        public GlobalVariable.BurnRun BurnIsRun = GlobalVariable.BurnRun.NotIsRun;
        public List<AddImgUrlDto> listDictImgUrls = new List<AddImgUrlDto>();
        public List<AddImgUrlDto> listDictImgUrlsBurn = new List<AddImgUrlDto>();
        public bool isRegister = false;
        //private Dictionary<string, string> MsgIdAndImgSendingId = new Dictionary<string, string>();
        /// <summary>
        /// 正常聊天listChatindex
        /// </summary>
        public List<string> listChatIndex = new List<string>();
        /// <summary>
        /// 阅后即焚burnListChatindex
        /// </summary>
        public List<string> BurnListChatIndex = new List<string>();
        System.Windows.Controls.ContextMenu cms;
        #region 2017-03-23 屏蔽
        System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
        #endregion
        string lastShowTime = "";
        string preTime = "";
        string firstTime = "";
        private string rememberName;
        public AntSdkGroupInfo GroupInfo;
        private MsgEditAssistant msgEditAssistant;
        public List<AntSdkGroupMember> GroupMembers { get; set; }
        BaseBLL<AntSdkTsession, T_SessionDAL> t_sessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
        SendMessage_ctt s_ctt;
        public int _unreadCount = 0;
        public int _burnUnreadCount = 0;
        GlobalVariable.BurnFlag BurnFlag;
        string userIMImageSavePath = string.Empty;
        private string imageSuffix = ".png";
        string format = "EditMsgList";
        private bool isSearch = false;//是否处于@搜索状态
        private bool isAdminId = false;
        private bool _isChanageWindow = false;
        private GlobalVariable.BurnFlag _isBurnMode;
        private const int pageSize = 10;//每一页显示的消息条数
        private List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> ChatMsgLst = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
        private List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> OnlineReceiveMessageList = null;
        /// <summary>
        /// 类内窗体控制器的实例
        /// </summary>
        protected IWindowHelper _windowHelper;
        public TalkGroupViewModel(SendMessage_ctt ctt, AntSdkGroupInfo groupInfo, List<AntSdkGroupMember> groupMembers, GlobalVariable.BurnFlag isBurnMode, int unreadCount, int burnUnreadCount)
        {
            try
            {
                LogHelper.WriteWarn("TalkGroupViewModel:" + "时间:" + DateTime.Now + "unreadCount:" + unreadCount + "burnUnreadCount:" + burnUnreadCount);
                //this.GroupMembers = groupMembers;
                this.GroupInfo = groupInfo;
                this.GroupName = groupInfo.groupName;
                this.s_ctt = ctt;
                this._unreadCount = unreadCount;
                this._burnUnreadCount = burnUnreadCount;
                this.BurnFlag = isBurnMode;
                rememberName = groupInfo.groupName;
                _isBurnMode = isBurnMode;
                InitTalkGroupView(groupMembers);

                //if (isBurnMode == GlobalVariable.BurnFlag.NotIsBurn)
                //{
                //if (ChatMsgLst.Count > 0 || FirstPageData?.Count() > 0)
                //{
                //timerBurn?.Stop();
                //if (isBurnMode == GlobalVariable.BurnFlag.NotIsBurn)
                //{
                timer.Interval = TimeSpan.FromMilliseconds(50);
                timer.Tick += Timer_Tick;
                timer.Start();
                //}
                //} 
                //}
                //else
                //{
                //if (ChatMsgLst.Count > 0 || FirstPageDataBurn.Count()>0)
                //{
                //timer?.Stop();
                //if (isBurnMode == GlobalVariable.BurnFlag.IsBurn)
                //{
                timerBurn.Interval = TimeSpan.FromMilliseconds(1500);
                timerBurn.Tick += TimerBurn_Tick;
                timerBurn.Start();
                //}
                //}
                //}

            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel]:" + ex.Source + ex.StackTrace + ex.Message);
            }
        }
        /// <summary>
        /// 无痕模式消息框初始化
        /// </summary>
        public void InitTalkBurn()
        {
            //BurnIsRun = GlobalVariable.BurnRun.NotIsRun;
            //timerBurn?.Stop();
            //timer?.Stop();
            //this.chromiumWebBrowserburn = new ChromiumWebBrowsers();

            chromiumWebBrowserburn.sessionid = s_ctt.sessionId;
            chromiumWebBrowserburn.s_ctt = s_ctt;
            #region 注册2
            BurnAndNotBurnImgList add = new BurnAndNotBurnImgList();
            add.NotBurn = listDictImgUrls;
            add.YesBurn = listDictImgUrlsBurn;
            // CallbackValueById.upLoadImageMsgOnce += CallbackValueById_upLoadImageMsgOnce;
            callbackOnmousewheelBackBurn.AddImgUrlEventHandler += CallbackOnmousewheelBack_AddImgUrlEventHandler;
            this.chromiumWebBrowserburn.RegisterJsObject("callbackUserId", new callbackId(this.chromiumWebBrowserburn));
            this.chromiumWebBrowserburn.RegisterJsObject("callbackObj", new CallbackObjectForJs(add, null, false, null, null));
            this.chromiumWebBrowserburn.RegisterJsObject("callbackUrl", new CallbackObjectUrlJs(this.chromiumWebBrowserburn, "", s_ctt));
            this.chromiumWebBrowserburn.RegisterJsObject("callbackFilePath", new CallbackObjectFilePathJs(this.chromiumWebBrowserburn,s_ctt));
            this.chromiumWebBrowserburn.RegisterJsObject("callbackValue", new CallbackValueById(this.chromiumWebBrowserburn, SendMsgListMonitor.MessageStateMonitorList));
            this.chromiumWebBrowserburn.RegisterJsObject("callbackOpenDirectory", new CallBackSelectDirecoryFile(this.chromiumWebBrowserburn));
            //2017-03-07 注册事件 滚轮事件
            this.chromiumWebBrowserburn.RegisterJsObject("callbackOnmousewheelburn", new callbackOnmousewheelBackBurn(this.chromiumWebBrowserburn, GroupMembers, s_ctt, BurnListChatIndex, GroupInfo, this));
            #endregion
            chromiumWebBrowserburn.MenuHandler = new MenuHandler();

            #region 2017-03-07 添加
            //如果unreadCount为0则查询该会话的最大的chatindex 如果大于0 则查询unreadCount+1条消息的chatindex
            //unreadCount为0情况 查询语句为：select max(chatindex) from t_chat_message;
            //unreadCount不为0情况 查询语句为：select chatindex from t_chat_message order by chatindex desc limit 10,1;
            //if (isBurnMode == GlobalVariable.BurnFlag.IsBurn)
            //{
            if (_burnUnreadCount == 0)
            {
                T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                string currentChatIndex = "0";
                var chatIndex = t_chat.getQueryZeroChatIndex(s_ctt.sessionId, s_ctt.companyCode,
                    AntSdkService.AntSdkCurrentUserInfo.userId);
                chromiumWebBrowserburn.scrollChatIndex = !string.IsNullOrEmpty(chatIndex) ? chatIndex : currentChatIndex;
                //if (chatIndex != "0")
                //{
                FirstPageDataBurn = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, 0, 10));
                //}
            }
            else
            {
                string currentChatIndex = "0";
                T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                var chatIndex = t_chat.getQueryNotZeroChatIndex(s_ctt.sessionId, s_ctt.companyCode,
                    AntSdkService.AntSdkCurrentUserInfo.userId, _burnUnreadCount.ToString());
                chromiumWebBrowserburn.scrollChatIndex = !string.IsNullOrEmpty(chatIndex) ? chatIndex : "0";
            }
            if (FirstPageDataBurn == null || (FirstPageDataBurn != null && FirstPageDataBurn.Count == 0))
                chromiumWebBrowserburn.scrollChatIndex = "0";
            else
            {
                var index = FirstPageDataBurn[0].chatIndex;
                chromiumWebBrowserburn.scrollChatIndex = !string.IsNullOrEmpty(index) ? index : "0";
            }
            //}
            #endregion
        }
        /// <summary>
        /// 正常模式消息框初始化
        /// </summary>
        public void InitTalk()
        {
            #region 注册
            chromiumWebBrowser.sessionid = s_ctt.sessionId;
            chromiumWebBrowser.s_ctt = s_ctt;
            BurnAndNotBurnImgList add = new BurnAndNotBurnImgList();
            add.NotBurn = listDictImgUrls;
            add.YesBurn = listDictImgUrlsBurn;
            this.chromiumWebBrowser.RegisterJsObject("callbackUserId", new callbackId(this.chromiumWebBrowser));
            this.chromiumWebBrowser.RegisterJsObject("callbackObj", new CallbackObjectForJs(add, s_ctt, IsIncognitoModelState, GroupInfo, chromiumWebBrowser, this));
            this.chromiumWebBrowser.RegisterJsObject("callbackUrl", new CallbackObjectUrlJs(this.chromiumWebBrowser, "", s_ctt));
            this.chromiumWebBrowser.RegisterJsObject("callbackFilePath", new CallbackObjectFilePathJs(this.chromiumWebBrowser,s_ctt));
            this.chromiumWebBrowser.RegisterJsObject("callbackValue", new CallbackValueById(this.chromiumWebBrowser, SendMsgListMonitor.MessageStateMonitorList));
            this.chromiumWebBrowser.RegisterJsObject("callbackOpenDirectory", new CallBackSelectDirecoryFile(this.chromiumWebBrowser));
            #endregion
            chromiumWebBrowser.MenuHandler = new MenuHandler();
            chromiumWebBrowser.AllowDrop = true;
            chromiumWebBrowser.PreviewDrop += _chromiumWebBrowser_PreviewDrop;
            #region 消息漫游 2017-12-8
            //第一次加载，判断是否有网络，如果有，从服务器拉取最新的pageSize条消息；如果没有，提示网络异常，并从本地数据库拉取最新pageSize条消息
            if (!AntSdkService.AntSdkIsConnected)
            {
                //网络异常，从本地数据库拉取最新pageSize条消息
                FirstPageData = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Group, true, pageSize, 0);
            }
            else
            {
                //从网络拉取pageSize条消息，如果请求失败，从本地数据库拉取最新pageSize条消息，如果请求成功，则在页面显示pageSize条消息
                FirstPageData = PublicMessageFunction.QueryMessageFromServer(s_ctt.sessionId, AntSdkchatType.Group, true, pageSize, 0);
                if (FirstPageData == null)
                {
                    FirstPageData = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Group, true, pageSize, 0);
                }
            }
            if (FirstPageData == null || (FirstPageData != null && FirstPageData.Count == 0))
                _chromiumWebBrowser.scrollChatIndex = "0";
            else
            {
                var index = FirstPageData[0].chatIndex;
                _chromiumWebBrowser.scrollChatIndex = !string.IsNullOrEmpty(index) ? index : "0";
            }
            //2017-03-07 注册事件 滚轮事件
            this.chromiumWebBrowser.RegisterJsObject("callbackOnmousewheel", new callbackOnmousewheelBack(this.chromiumWebBrowser, GroupMembers, s_ctt, listChatIndex, FirstPageData, GroupInfo, this));
            #endregion
        }
        /// <summary>
        /// 群成员加载
        /// </summary>
        /// <param name="groupMembers"></param>
        public void GroupMembersLoad(List<AntSdkGroupMember> groupMembers)
        {
            GroupMembers = groupMembers;
            this.chromiumWebBrowser.GroupMembers = GroupMembers;
            this.chromiumWebBrowserburn.GroupMembers = GroupMembers;
            if (_richTextBox != null && (_richTextBox.RichTextSource == null || _richTextBox.RichTextSource.Count == 0))
                _richTextBox.RichTextSource = GroupMembers;
        }

        /// <summary>
        /// 即时消息推送
        /// </summary>
        /// <param name="mtp"></param>
        /// <param name="chatMsg"></param>
        private void LocalMessageHelper_InstantMessageHasBeenReceived(int mtp, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg)
        {
            var user = GroupMembers.FirstOrDefault(m => m.userId == chatMsg.sendUserId);
            if (chatMsg.MsgType == AntSdkMsgType.Revocation)
            {
                var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                    HideMsgAndShowRecallMsg(tempChatMsg.content?.messageId);
            }
            else if (chatMsg.MsgType == AntSdkMsgType.DeleteVote || chatMsg.MsgType == AntSdkMsgType.DeleteActivity)
            {
                HideMsgAndShowRecallMsg(chatMsg.messageId);
                #region 2017-11-09 屏蔽 禅道优化ID 5208
                //if (chatMsg.MsgType == AntSdkMsgType.DeleteVote)
                //{
                //    var voteContent = JsonConvert.DeserializeObject<AntSdkChatMsg.DeteleVote_content>(chatMsg.sourceContent);
                //    chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.InsertUIRecallMsg("[投票]：&quot;" + voteContent.title + "&quot;已删除"));
                //}
                //else
                //{
                //    var ActivityContent = JsonConvert.DeserializeObject<ReleaseGroupActivityInput>(chatMsg.sourceContent);
                //    chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.InsertUIRecallMsg("[活动]：&quot;" + ActivityContent.theme + "&quot;已删除"));
                //}
                #endregion
                return;
            }

            if (chatMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId) //本人发的消息，系统给的回执
            {
                LogHelper.WriteDebug("[群组聊天回执已经回了]:" + /*TODO:AntSdk_Modify:chatMsg.content*/chatMsg.sourceContent);
                //var contactUser = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == AntSdkService.AntSdkLoginOutput.userId);
                //if (contactUser == null) return;
                if (chatMsg.os == ((int)GlobalVariable.OSType.PC)) //PC端发的消息
                {
                    #region 重发消息处理
                    AttrDto attrDto = JsonConvert.DeserializeObject<AttrDto>(chatMsg.attr);
                    if (attrDto?.isResend == "1")
                    {
                        //更新消息状态
                        var result = SendOrReceiveMsgStateOperate.UpdateSendMsgGroup(IsIncognitoModelState == true ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn, chatMsg.messageId, chatMsg.chatIndex);
                        if (result == 1)
                        {
                            if (!SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(chatMsg.messageId)) return;
                            //更新界面状态
                            SendOrReceiveMsgStateOperate.UpdateUiState(AntSdkburnMsg.isSendSucessOrFail.sucess, IsIncognitoModelState == true ? this.chromiumWebBrowserburn : this.chromiumWebBrowser, SendMsgListMonitor.MsgIdAndImgSendingId[chatMsg.messageId]);
                            Console.WriteLine("================" + SendMsgListMonitor.MsgIdAndImgSendingId[chatMsg.messageId] + "================");
                            //移除
                            SendMsgListMonitor.MsgIdAndImgSendingId.Remove(chatMsg.messageId);

                            #region 停止并移除消息监控
                            var MsgState = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == chatMsg.messageId);
                            if (MsgState != null)
                            {
                                MsgState.dispatcherTimer.Stop();
                                SendMsgListMonitor.MessageStateMonitorList.Remove(MsgState);
                            }

                            #endregion
                        }
                    }
                    #endregion
                    else
                    {
                        if (!SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(chatMsg.messageId)) return;
                        //更新界面状态
                        SendOrReceiveMsgStateOperate.UpdateUiState(AntSdkburnMsg.isSendSucessOrFail.sucess, IsIncognitoModelState == true ? this.chromiumWebBrowserburn : this.chromiumWebBrowser, SendMsgListMonitor.MsgIdAndImgSendingId[chatMsg.messageId]);
                        //移除
                        SendMsgListMonitor.MsgIdAndImgSendingId.Remove(chatMsg.messageId);

                        #region 停止并移除消息监控
                        var MsgState = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == chatMsg.messageId);
                        if (MsgState != null)
                        {
                            MsgState.dispatcherTimer.Stop();
                            SendMsgListMonitor.MessageStateMonitorList.Remove(MsgState);
                        }

                        #endregion
                    }
                }
                else
                {

                    if (chatMsg.flag == 1)
                    {
                        LoadMsgData(GlobalVariable.BurnFlag.IsBurn, false);
                        if (ChatMsgLst.Count > 0)
                        {
                            ShowMsgData(ChatMsgLst);
                        }
                        if (!BurnListChatIndex.Contains(chatMsg.messageId))
                        {
                            receiveMsg(mtp, chatMsg);
                        }
                    }
                    else
                    {
                        LoadMsgData(GlobalVariable.BurnFlag.NotIsBurn, false);
                        if (ChatMsgLst.Count > 0)
                        {

                            ShowMsgData(ChatMsgLst);
                        }
                        if (!listChatIndex.Contains(chatMsg.messageId))
                        {
                            receiveMsg(mtp, chatMsg);
                        }
                    }
                }
            }
            else
            {
                if (chatMsg.flag == 1)
                {
                    //获取当前滚动条位置
                    var task = chromiumWebBrowserburn.EvaluateScriptAsync("getScroolPosition();");
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
                                    TextShowReceiveMsg = chatMsg.sourceContent;
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgFile:
                                    #region 文件
                                    TextShowReceiveMsg = "[文件]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgAudio:
                                    #region 语音
                                    TextShowReceiveMsg = "[语音]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgPicture:
                                    #region 图片
                                    TextShowReceiveMsg = "[图片]";
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
                    //如果未读消息还没全展示，在打开会话展示界面过程中的即时消息先缓存。
                    LoadMsgData(GlobalVariable.BurnFlag.IsBurn, false);
                    if (ChatMsgLst.Count > 0)
                    {
                        ShowMsgData(ChatMsgLst);
                    }
                    if (!BurnListChatIndex.Contains(chatMsg.messageId))
                    {
                        receiveMsg(mtp, chatMsg);
                    }
                }
                else
                {
                    //获取当前滚动条位置
                    var task = chromiumWebBrowser.EvaluateScriptAsync("getScroolPosition();");
                    task.Wait();
                    string startStr = "【" + user?.userNum + user?.userName + "】: ";
                    if (task.Result.Success)
                    {
                        if ((bool)task.Result.Result == false)
                        {
                            TextShowRowHeight = "32";
                            switch (chatMsg.MsgType)
                            {
                                case AntSdkMsgType.ChatMsgText:
                                    #region 文本
                                    TextShowReceiveMsg = startStr + chatMsg.sourceContent;
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgFile:
                                    #region 文件
                                    TextShowReceiveMsg = startStr + "[文件]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgAudio:
                                    #region 语音
                                    TextShowReceiveMsg = startStr + "[语音]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgPicture:
                                    #region 图片
                                    TextShowReceiveMsg = startStr + "[图片]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.ChatMsgMixMessage:
                                    #region 混合消息
                                    string startContent = "";
                                    //显示内容解析
                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(chatMsg.sourceContent);
                                    foreach (var content in receive)
                                    {
                                        switch (content.type)
                                        {
                                            case "1001":
                                                startContent += content.content;
                                                break;
                                            case "1002":
                                                startContent += "[图片]";
                                                break;
                                            case "1008":
                                                List<AntSdkChatMsg.At_content> at = JsonConvert.DeserializeObject<List<AntSdkChatMsg.At_content>>(content.content?.ToString());
                                                foreach (var atList in at)
                                                {
                                                    if (atList.type == "1112")
                                                    {
                                                        foreach (var atName in atList.names)
                                                        {
                                                            startContent += "@" + atName;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        #region @全体成员
                                                        startContent += "@全体成员";
                                                        #endregion
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    TextShowReceiveMsg = startStr + startContent;
                                    #endregion
                                    break;
                                case AntSdkMsgType.CreateActivity:
                                    #region 活动
                                    TextShowReceiveMsg = startStr + "[活动]";
                                    #endregion
                                    break;
                                case AntSdkMsgType.CreateVote:
                                    #region 投票
                                    TextShowReceiveMsg = startStr + "[投票]";
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
                    if (ChatMsgLst.Count > 0)
                    {
                        ShowMsgData(ChatMsgLst);
                    }
                    if (!listChatIndex.Contains(chatMsg.messageId))
                    {
                        receiveMsg(mtp, chatMsg);
                    }
                }

            }

        }
        /// <summary>
        /// 加载所有未读消息
        /// </summary>
        /// <param name="ctt"></param>
        /// <param name="isBurnMode">是否是阅后即焚标记</param>
        /// <param name="isChanageWindow">是否改变会话窗体为当前会话</param>
        public void LoadMsgData(GlobalVariable.BurnFlag isBurnMode, bool isChanageWindow = true)
        {
            _isBurnMode = isBurnMode;
            _isChanageWindow = isChanageWindow;
            if (_isChanageWindow && _windowHelper != null)
                WindowMonitor.ChanageWindowHelper(_windowHelper);
            var offlineMessageList = MessageMonitor.GetOfflineMessageStatisticList(s_ctt.sessionId, isBurnMode) ?? new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-LoadMsgData:]" + s_ctt.sessionId + "离线消息条数-------------" + offlineMessageList.Count);
            var onlineMessageList = SessionMonitor.GetWaitingToReceiveOnlineMessage(s_ctt.sessionId, (int)isBurnMode, AntSdkchatType.Group);
            LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-LoadMsgData:]" + s_ctt.sessionId + "在线消息条数-------------" + onlineMessageList.Count);
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
                    "---------------------------[TalkGroupViewModel-LoadMsgData:]SessionViewMouseLeftButtonDown" +
                    lastMsg.sessionId + "发送已读回执---------------------msessageId:" +
                    lastMsg.messageId + "------chatIndex:" + lastMsg.chatIndex);
            }
            LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-ShowMsgData:]LoadMsgData" +
                                ChatMsgLst[0].sessionId + "消息展示消息个数---------------------" + ChatMsgLst.Count);
        }
        /// <summary>
        /// 消息数据展示到界面
        /// </summary>
        /// <param name="isBurnMode"></param>
        /// <param name="isConversation">是否正在会话</param>
        public void ShowMsgData(List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> chatMsgList = null, bool isConversation = false)
        {
            if (chatMsgList == null)
            {
                chatMsgList = ChatMsgLst;
            }
            if (ChatMsgLst.Count > 0)
                LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-ShowMsgData:]ShowMsgData" + ChatMsgLst[0].sessionId + "未读消息个数---------------------" + ChatMsgLst.Count);
            if (chatMsgList.Count == 0)
            {
                if (_richTextBox != null)
                {
                    _richTextBox.IsReadOnly = false;
                    _richTextBox.Focus();
                }

                return;
            }

            switch (_isBurnMode)
            {
                case GlobalVariable.BurnFlag.NotIsBurn:
                    if (chromiumWebBrowser.IsBrowserInitialized == false || chromiumWebBrowser.Visibility != Visibility.Visible) return;
                    StringBuilder sbLeftNotIsBurn = new StringBuilder();
                    string topStrs = PublicTalkMothed.topString();
                    sbLeftNotIsBurn.AppendLine(topStrs);
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

                            #region 2016-06-14更换 old
                            //list.typeBurnMsg = 0;
                            //if (!listChatIndex.Contains(list.chatIndex))
                            //{
                            //    if ((int)GlobalVariable.MsgType.Picture == Convert.ToInt32(list.MTP))
                            //    {
                            //        AddImgUrlDto addImg = new AddImgUrlDto();
                            //        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(list.content);
                            //        addImg.ChatIndex = list.chatIndex;
                            //        addImg.ImageUrl = rimgDto.picUrl;
                            //        listDictImgUrls.Add(addImg);
                            //    }
                            //    listChatIndex.Add(list.chatIndex);
                            //    receiveMsg(Convert.ToInt32(list.MTP), list);
                            //    LogHelper.WriteDebug("TalkGroupViewModel-Timer_Tick:" + JsonConvert.SerializeObject(list));
                            //} 
                            #endregion

                            #region 2017-06-14修改 new
                            List<string> imageId = new List<string>();
                            imageId.Clear();
                            switch (Convert.ToInt32(/*//TODO:AntSdk_Modify:list.MTP*/list.MsgType))
                            {
                                case (int)AntSdkMsgType.ChatMsgMixMessage:
                                    #region 图文混合
                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                    foreach (var ilist in receive)
                                    {
                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                        imageId.Add(imgId);
                                        PictureAndTextMixContentDto contents = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, contents.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    }
                                    #endregion
                                    break;
                                case (int)AntSdkMsgType.ChatMsgPicture:
                                    AddImgUrlDto addImg = new AddImgUrlDto();
                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                    addImg.ChatIndex = list.chatIndex;
                                    addImg.ImageUrl = rimgDto.picUrl;
                                    listDictImgUrls.Add(addImg);
                                    break;
                                case (int)AntSdkMsgType.ChatMsgAt:
                                    #region 2017-07-09屏蔽
                                    //AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                    //foreach (var ids in atDto.ctt.ids)
                                    //{
                                    //    AtIdsName at = JsonConvert.DeserializeObject<AtIdsName>(ids.ToString());

                                    //    if (at.id == AntSdkService.AntSdkCurrentUserInfo.userId)
                                    //    {

                                    //        var person = GroupMembers.SingleOrDefault(m => m.userId == list.sendUserId);
                                    //        if (person != null)
                                    //        {
                                    //            IsbtnTipShow = "Visible";
                                    //            SetContents =
                                    //           person.userName +
                                    //           "@了我";
                                    //            scrollPostion = list.chatIndex;
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        if (at.name == "全体成员")
                                    //        {
                                    //            var person = GroupMembers.SingleOrDefault(m => m.userId == list.sendUserId);
                                    //            if (person != null)
                                    //            {
                                    //                IsbtnTipShow = "Visible";
                                    //                SetContents = person.
                                    //                userName +
                                    //                "@了我";
                                    //                scrollPostion = list.chatIndex;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                    #region 新
                                    #region 图文混合
                                    List<MsChatMsgAt_content> receiveat = JsonConvert.DeserializeObject<List<MsChatMsgAt_content>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                    foreach (var ilist in receiveat)
                                    {
                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                        imageId.Add(imgId);
                                        PictureAndTextMixContentDto contents = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content);
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, contents.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    }
                                    #endregion
                                    //1、先看type=1111的消息  如果有 直接显示@提示  
                                    //2、看type=1112的消息 如若有 显示@提示
                                    //3、如果没有就正常显示
                                    var setlist = list as AntSdkChatMsg.At;
                                    var isHaveAtAll = setlist.content.FirstOrDefault(m => m.type == "1111");
                                    if (isHaveAtAll != null)
                                    {
                                        var person = GroupMembers.FirstOrDefault(m => m.userId == list.sendUserId);
                                        if (person != null)
                                        {
                                            IsbtnTipShow = "Visible";
                                            SetContents = person.
                                            userName +
                                            "@了我";
                                            scrollPostion = list.chatIndex;
                                        }
                                    }
                                    else
                                    {
                                        var listAtPerson = setlist.content.Where(m => m.type == "1112").ToList();
                                        foreach (var lists in listAtPerson)
                                        {
                                            if (lists.ids.Count() > 1)
                                            {
                                                foreach (var arry in lists.ids)
                                                {
                                                    if (/*TODO:AntSdk_Modify:ids.id*/arry.ToString() == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                    {
                                                        var person = GroupMembers.SingleOrDefault(m => m.userId == list.sendUserId);
                                                        if (person != null)
                                                        {
                                                            IsbtnTipShow = "Visible";
                                                            SetContents =
                                                           person.userName +
                                                           "@了我";
                                                            scrollPostion = list.chatIndex;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (/*TODO:AntSdk_Modify:ids.id*/lists.ids[0].ToString() == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    var person = GroupMembers.SingleOrDefault(m => m.userId == list.sendUserId);
                                                    if (person != null)
                                                    {
                                                        IsbtnTipShow = "Visible";
                                                        SetContents =
                                                       person.userName +
                                                       "@了我";
                                                        scrollPostion = list.chatIndex;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    break;
                            }
                            listChatIndex.Add(list.messageId);
                            string groupStrs = PublicTalkMothed.LeftGroupToGroupShowMessage(list, GroupMembers, imageId);
                            sbLeftNotIsBurn.AppendLine(groupStrs);
                            #endregion
                        }
                    }
                    string endStrs = PublicTalkMothed.endString();
                    sbLeftNotIsBurn.AppendLine(endStrs);
                    string content = sbLeftNotIsBurn.ToString();
                    bool IsSucess = false;
                    Task<JavascriptResponse> results = _chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(content);
                    results.Wait();
                    if (results.Result.Success == true)
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
                        Task<JavascriptResponse> resultWhile = _chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(content);
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
                        #region 2017-12-03 屏蔽
                        //Task<JavascriptResponse> results2 = _chromiumWebBrowser.EvaluateScriptAsync(content);
                        //results2.Wait();
                        //if (results2.Result.Success)
                        //{
                        //    LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_NotBurn:第二次执行成功---------------------");
                        //}
                        //else
                        //{
                        //    Thread.Sleep(100);
                        //    LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_NotBurn:第二次执行失败---------------------");
                        //    Task<JavascriptResponse> results3 = _chromiumWebBrowser.EvaluateScriptAsync(content);
                        //    if (results3.Result.Success)
                        //    {
                        //        LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_NotBurn:第三次执行成功---------------------");
                        //    }
                        //    else
                        //    {
                        //        LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_NotBurn:第三次执行失败---------------------");
                        //    }
                        //}
                        #endregion
                    }
                    ChatMsgLst.Clear();
                    this._chromiumWebBrowser.ExecuteScriptAsync("setscross();");
                    break;
                case GlobalVariable.BurnFlag.IsBurn:
                    if (_chromiumWebBrowserburn.IsBrowserInitialized == false || _chromiumWebBrowserburn.Visibility != Visibility.Visible) return;
                    StringBuilder sbLeft = new StringBuilder();
                    string topStr = PublicTalkMothed.topString();
                    sbLeft.AppendLine(topStr);
                    string checkBurnId = "";
                    foreach (var list in chatMsgList)
                    {
                        checkBurnId = list.messageId;
                        #region 2017-06-14更换 old
                        //list.typeBurnMsg = 1;
                        //if (!BurnListChatIndex.Contains(list.chatIndex))
                        //{
                        //    BurnListChatIndex.Add(list.chatIndex);
                        //    receiveMsg(Convert.ToInt32(list.MTP), list);
                        //    LogHelper.WriteDebug("TalkGroupViewModel-TimerBurn_Tick:" + JsonConvert.SerializeObject(list));
                        //}
                        #endregion

                        #region 2017-06-14修改 New
                        switch (Convert.ToInt32(/*TODO:AntSdk_Modify:list.MTP*/list.MsgType))
                        {
                            case (int)AntSdkMsgType.ChatMsgPicture:
                                SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn,
                                   list.chatIndex, rimgDto.picUrl, "", burnMsg.IsReadImg.read,
                                   burnMsg.IsEffective.effective);
                                break;
                        }
                        BurnListChatIndex.Add(list.messageId);
                        string groupBurnStr = PublicTalkMothed.LeftGroupToGroupShowBurnMessage(list, GroupMembers);
                        sbLeft.AppendLine(groupBurnStr);
                        #endregion
                    }
                    string endStr = PublicTalkMothed.endString();
                    sbLeft.AppendLine(endStr);
                    bool IsSucessBurn = false;
                    Task<JavascriptResponse> result = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
                    result.Wait();
                    if (result.Result.Success == true)
                    {
                        var isExist = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkBurnId + "');");
                        isExist.Wait();
                        if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                        {
                            IsSucessBurn = false;
                        }
                        else
                        {
                            IsSucessBurn = true;
                        }
                    }
                    while (IsSucessBurn == false)
                    {
                        Task<JavascriptResponse> resultBurnWhile = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
                        resultBurnWhile.Wait();
                        if (resultBurnWhile.Result.Success)
                        {
                            var isExist = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkBurnId + "');");
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
                    }
                    #region 2017-12-03 屏蔽
                    //if (!result.Result.Success)
                    //{
                    //    Thread.Sleep(50);
                    //    LogHelper.WriteWarn("[TalkGroupViewModel_ShowMsgData_Burn]" + sbLeft.ToString());
                    //    Task<JavascriptResponse> results2 = _chromiumWebBrowserburn.EvaluateScriptAsync(sbLeft.ToString());
                    //    if (results2.Result.Success)
                    //    {
                    //        LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_Burn:第二次执行成功---------------------");
                    //    }
                    //    else
                    //    {
                    //        Thread.Sleep(100);
                    //        LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_Burn:第二次执行失败---------------------");
                    //        Task<JavascriptResponse> results3 = _chromiumWebBrowserburn.EvaluateScriptAsync(sbLeft.ToString());
                    //        if (results3.Result.Success)
                    //        {
                    //            LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_Burn:第三次次执行成功---------------------");
                    //        }
                    //        else
                    //        {
                    //            LogHelper.WriteWarn("---------------------------TalkGroupViewModel_ShowMsgData_Burn:第三次次执行失败---------------------");
                    //        }
                    //    }
                    //}
                    #endregion
                    ChatMsgLst.Clear();
                    this._chromiumWebBrowserburn.ExecuteScriptAsync("setscross();");
                    break;
            }

            if (_richTextBox != null)
            {
                _richTextBox.IsReadOnly = false;
                _richTextBox.Focus();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitTalkGroupView(List<AntSdkGroupMember> groupMembers)
        {
            try
            {
                //this.GroupMembers = groupMembers;
                GroupMemberViewModel.SwitchBurnImage += GroupMemberViewModel_SwitchBurnImage;

                //2017-03-09 判断是否为管理员
                var isUserMember = GroupInfo.groupOwnerId == AntSdkService.AntSdkCurrentUserInfo.userId || (GroupInfo.managerIds.Count > 0 && GroupInfo.managerIds.Contains(AntSdkService.AntSdkCurrentUserInfo.userId));
                //  GroupMembers.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));

                //if (isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.GroupOwner && isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.Admin)
                if (!isUserMember)
                {
                    LogHelper.WriteDebug("[正常模式_TalkGroupViewModel]:非管理员");
                    isShowBurn = "Collapsed";
                    isShowExit = "Collapsed";
                    isShowDelete = "Collapsed";
                }
                else
                {
                    LogHelper.WriteDebug("[正常模式_TalkGroupViewModel]:管理员");
                    isAdminId = true;
                }

                //if (isUserMember?.roleLevel == (int) GlobalVariable.GroupRoleLevel.Admin)
                //{
                //    isAdminId = true;
                //}
                if (_richTextBox != null)
                {
                    userIMImageSavePath = publicMethod.localDataPath() + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\group\\cutImg\\";
                    msgEditAssistant = new MsgEditAssistant(_richTextBox, userIMImageSavePath);
                    if (GroupInfo != null)
                        _richTextBox.GroupId = GroupInfo.groupId;
                }

                InitTalk();
                InitTalkBurn();
                if (_isBurnMode == GlobalVariable.BurnFlag.IsBurn)
                {
                    isShowBurn = "Collapsed";
                    isShowEmoji = "Collapsed";
                    isShowSound = Visibility.Collapsed;
                    isShowCutImage = "Collapsed";
                    NoticeVisibility = Visibility.Collapsed;
                    //if (isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.GroupOwner && isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.Admin)
                    if (!isUserMember)
                    {
                        LogHelper.WriteDebug("[阅后即焚_TalkGroupViewModel]");
                        isShowExit = "Collapsed";
                        isShowDelete = "Collapsed";
                    }
                    else
                    {
                        LogHelper.WriteDebug("[阅后即焚_TalkGroupViewModel]");
                        isShowExit = "Visible";
                        isShowDelete = "Visible";
                    }
                    IsIncognitoModelState = true;
                    isShowWinMsg = "Collapsed";
                    isShowBurnWinMsg = "Visible";
                    chromiumWebBrowserburn.Visibility = Visibility.Visible;
                    chromiumWebBrowser.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LogHelper.WriteDebug("[正常模式_TalkGroupViewModel]");
                    //InitTalk();
                    chromiumWebBrowserburn.Visibility = Visibility.Collapsed;
                    chromiumWebBrowser.Visibility = Visibility.Visible;
                }
                if (string.IsNullOrWhiteSpace(GroupInfo.groupPicture))
                {
                    this.Picture = "pack://application:,,,/AntennaChat;Component/Images/36-头像.png";
                }
                else
                {
                    this.Picture = GroupInfo.groupPicture;
                }
                if (PublicTalkMothed.isRegester == false)
                {
                    CallbackValueById.upLoadImageMsgOnce += CallbackValueById_upLoadImageMsgOnce;
                    CallbackValueById.sendMixPicAndTextOnce += CallbackValueById_sendMixPicAndTextOnce;
                    CallbackValueById.sendMixMessageOnce += CallbackValueById_sendMixMessageOnce;
                    CallbackValueById.upLoadFileMsgOnce += CallbackValueById_upLoadFileMsgOnce;
                    CallbackValueById.sendTextMsgOnce += CallbackValueById_sendTextMsgOnce;
                    CallbackValueById.sendAtTextMsgOnce += CallbackValueById_sendAtTextMsgOnce;
                    CallbackValueById.upLoadVoiceMsgOnce += CallbackValueById_upLoadVoiceMsgOnce;
                    callbackOnmousewheelBack.AddImgUrlEventHandler += CallbackOnmousewheelBack_AddImgUrlEventHandler;

                    PublicTalkMothed.isRegester = true;
                }
                ShowUserInfoEvent += CallbackId_ShowUserInfoEvent;
                GroupMemberViewModel.KickoutGroupEvent += KickoutGroup;
                System.Windows.Controls.MenuItem miCut = new System.Windows.Controls.MenuItem() { Header = "剪切" };
                cm.Items.Add(miCut);
                miCut.Click += MiCut_Click;
                System.Windows.Controls.MenuItem miCopy = new System.Windows.Controls.MenuItem() { Header = "复制" };
                cm.Items.Add(miCopy);
                miCopy.Click += MiCopy_Click;
                System.Windows.Controls.MenuItem miPaste = new System.Windows.Controls.MenuItem() { Header = "粘贴" };
                cm.Items.Add(miPaste);
                miPaste.Click += MiPaste_Click;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_TalkGroupViewModel]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        private void CallbackValueById_sendMixMessageOnce(object sender, EventArgs e)
        {
            var send = sender as MixPicAndSenderArgs;
            //sendMixMsg(send.type, send.picAndTxtMix, send.currentChat, send.listPicAndText, send.arg);

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
                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, "R" + list.PreGuid, list.Path, "", burnMsg.IsReadImg.notRead, burnMsg.IsEffective.effective);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallbackObjectForJs_ReCallMsgEventHandler(object sender, EventArgs e)
        {
            string messageId = sender as string;
            //ThreadPool.QueueUserWorkItem(m => HandlerRecallMsg(messageId));
            HandlerRecallMsg(messageId);
        }

        private void HandlerRecallMsg(string messageId)
        {
            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
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
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                string errMsg = "";
                AntSdkChatMsg.Revocation ReCall = new AntSdkChatMsg.Revocation();
                string newMessageId = PublicTalkMothed.timeStampAndRandom();
                ReCall.messageId = newMessageId;
                AntSdkChatMsg.Revocation_content ReContent = new AntSdkChatMsg.Revocation_content();
                ReCall.chatType = (int)AntSdkchatType.Group;
                ReCall.MsgType = AntSdkMsgType.Revocation;
                ReCall.os = (int)GlobalVariable.OSType.PC;
                ReContent.messageId = messageId;
                ReCall.content = ReContent;
                ReCall.sendUserId = this.s_ctt.sendUserId;
                ReCall.targetId = this.s_ctt.targetId;
                ReCall.sessionId = this.s_ctt.sessionId;

                int maxChatIndex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, this.s_ctt.sessionId);
                //插入消息
                SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase tempChatMsg = new AntSdkChatMsg.ChatBase();
                tempChatMsg.MsgType = AntSdkMsgType.Revocation;
                tempChatMsg.messageId = newMessageId;
                tempChatMsg.SENDORRECEIVE = "1";
                tempChatMsg.chatType = (int)AntSdkchatType.Group;
                tempChatMsg.flag = IsIncognitoModelState ? 1 : 0;
                tempChatMsg.os = (int)(int)GlobalVariable.OSType.PC;
                tempChatMsg.sendUserId = this.s_ctt.sendUserId;
                tempChatMsg.chatIndex = maxChatIndex.ToString();
                tempChatMsg.targetId = GroupInfo?.groupId;
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
                    }
                    else
                    {
                        t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, newMessageId);
                    }
                }
            }
        }
        public void EndNotBurnEvent()
        {
            OnceSendMessage.GroupToGroup.CefList?.Remove(s_ctt.sessionId);
            if (_windowHelper != null)
            {
                _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived -= LocalMessageHelper_InstantMessageHasBeenReceived;
            }
            ShowUserInfoEvent -= CallbackId_ShowUserInfoEvent;
            chromiumWebBrowser.PreviewDrop -= _chromiumWebBrowser_PreviewDrop;
            GroupMemberViewModel.SwitchBurnImage -= GroupMemberViewModel_SwitchBurnImage;
            CallbackValueById.upLoadImageMsgOnce -= CallbackValueById_upLoadImageMsgOnce;
            CallbackValueById.sendMixMessageOnce -= CallbackValueById_sendMixMessageOnce;
            CallbackValueById.sendMixPicAndTextOnce -= CallbackValueById_sendMixPicAndTextOnce;
            CallbackValueById.upLoadFileMsgOnce -= CallbackValueById_upLoadFileMsgOnce;
            CallbackValueById.sendTextMsgOnce -= CallbackValueById_sendTextMsgOnce;
            CallbackValueById.sendAtTextMsgOnce -= CallbackValueById_sendAtTextMsgOnce;
            CallbackValueById.upLoadVoiceMsgOnce -= CallbackValueById_upLoadVoiceMsgOnce;
            callbackOnmousewheelBack.AddImgUrlEventHandler -= CallbackOnmousewheelBack_AddImgUrlEventHandler;
            GroupMemberViewModel.KickoutGroupEvent -= KickoutGroup;
            //CallbackObjectForJs.ReCallMsgEventHandler -= CallbackObjectForJs_ReCallMsgEventHandler;
            chromiumWebBrowser.PreviewDrop -= _chromiumWebBrowser_PreviewDrop;
            foreach (var list in SendMsgListMonitor.MessageStateMonitorList)
            {
                list._dispatcherTimer.Stop();
            }
            chromiumWebBrowser.Dispose();
            PublicTalkMothed.isRegester = false;
        }
        public void EndBurnEvent()
        {
            OnceSendMessage.GroupToGroup.CefList?.Remove(s_ctt.sessionId);
            if (_windowHelper != null)
            {
                _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived -= LocalMessageHelper_InstantMessageHasBeenReceived;
            }
            callbackOnmousewheelBackBurn.AddImgUrlEventHandler -= CallbackOnmousewheelBack_AddImgUrlEventHandler;
            CallbackValueById.upLoadImageMsgOnce -= CallbackValueById_upLoadImageMsgOnce;
            CallbackValueById.upLoadFileMsgOnce -= CallbackValueById_upLoadFileMsgOnce;
            CallbackValueById.sendTextMsgOnce -= CallbackValueById_sendTextMsgOnce;
            CallbackValueById.sendAtTextMsgOnce -= CallbackValueById_sendAtTextMsgOnce;
            CallbackValueById.upLoadVoiceMsgOnce -= CallbackValueById_upLoadVoiceMsgOnce;
            callbackOnmousewheelBack.AddImgUrlEventHandler -= CallbackOnmousewheelBack_AddImgUrlEventHandler;

            GroupMemberViewModel.KickoutGroupEvent -= KickoutGroup;
            //CallbackObjectForJs.ReCallMsgEventHandler -= CallbackObjectForJs_ReCallMsgEventHandler;
            foreach (var list in SendMsgListMonitor.MessageStateMonitorList)
            {
                list._dispatcherTimer.Stop();
            }
            chromiumWebBrowserburn.Dispose();
        }
        private int count = 0;
        public void CallbackId_ShowUserInfoEvent(object sender, EventArgs e)
        {
            var isBurn = IsIncognitoModelState
                ? GlobalVariable.BurnFlag.IsBurn
                : GlobalVariable.BurnFlag.NotIsBurn;
            var userID = sender as string;
            if (UserInfoControl == null)
            {
                UserInfoControl = new UserInfoViewModel(userID, isBurn);
                UserInfoControl.SendOrAtEvent += UserInfoControl_SendOrAtEvent;
            }
            else
                UserInfoControl.InitUserInfo(userID, isBurn);
            PopUserInfoIsOpen = false;
            PopUserInfoIsOpen = true;
            count++;
            LogHelper.WriteDebug("触发次数" + count);
        }
        /// <summary>
        /// 发送消息或者@对象
        /// </summary>
        /// <param name="methodType"></param>
        /// <param name="id"></param>
        private void UserInfoControl_SendOrAtEvent(string methodType, string id)
        {
            if (!string.IsNullOrEmpty(methodType))
            {
                switch (methodType)
                {
                    case "1":
                        PopupCreateSessionEven(id);
                        break;
                    case "2":
                        PopUserInfoIsOpen = false;
                        AntSdkGroupMember userinfo = chromiumWebBrowser.GroupMembers.SingleOrDefault(m => m.userId == PublicTalkMothed.SubString(id));
                        PublicTalkMothed.InsertAtBlock(chromiumWebBrowser.richTextBox, userinfo.userName, userinfo.userId);
                        chromiumWebBrowser.richTextBox.Focus();
                        break;
                }
            }
        }
        public static event EventHandler PopupCreateSessionEventHandler;
        private void PopupCreateSessionEven(string userId)
        {
            if (PopupCreateSessionEventHandler != null)
            {
                PopupCreateSessionEventHandler(userId, null);
            }
        }

        private void CallbackOnmousewheelBack_AddImgUrlEventHandler(object sender, EventArgs e)
        {
            AddImgUrlDto addImg = sender as AddImgUrlDto;
            if (addImg != null)
            {
                AddDictImgUrl(addImg.PreOrEnd, addImg.IsBurn, addImg.ChatIndex, addImg.ImageUrl, "", addImg.IsRead, addImg.IsEffective);
            }
        }

        private void GroupMemberViewModel_SwitchBurnImage(object sender, EventArgs e)
        {
            isShowBurn = "Collapsed";
            isAdminId = false;
        }

        private void CallbackValueById_sendAtTextMsgOnce(object sender, EventArgs e)
        {
            sendTextDto sendText = sender as sendTextDto;

            List<AntSdkChatMsg.At_content> obj = new List<AntSdkChatMsg.At_content>();
            List<object> th = JsonConvert.DeserializeObject<List<object>>(sendText.AtArray.ToString());
            //foreach (var list in th)
            //{
            //    AntSdkChatMsg.At_content aneName = JsonConvert.DeserializeObject<AntSdkChatMsg.At_content>(list.ToString());
            //    if (aneName.ids == null)
            //    {
            //        AtIds atIds = new AtIds();
            //        //TODO:AntSdk_Modify
            //        //atIds.name = aneName.names;
            //        //obj.Add(atIds);
            //    }
            //    else
            //    {
            //        obj.Add(aneName);
            //    }
            //}
            sendAtTextMethod(sendText.msgStr, sendText.messageid, sendText.imageTipId, sendText.imageSendingId, sendText.FailOrSucess, null, th);
        }

        private void CallbackValueById_sendTextMsgOnce(object sender, EventArgs e)
        {
            #region 重发 同步方法
            //sendTextDto sendTextDto = sender as sendTextDto;
            //sendTextMethod(sendTextDto.msgStr, sendTextDto.messageid, sendTextDto.imageTipId, sendTextDto.imageSendingId, sendTextDto.FailOrSucess, null);
            #endregion
            #region 重发 异步 修改：姚伦海
            var sendTextDto = sender as sendTextDto;
            if (sendTextDto != null)
            {
                ThreadPool.QueueUserWorkItem(
                    m =>
                        sendTextMethod(sendTextDto.msgStr, sendTextDto.messageid, sendTextDto.imageTipId, sendTextDto.imageSendingId, sendTextDto.FailOrSucess, null, sendTextDto.isOnceSendMsg));
                #region 滚动条置底
                StringBuilder sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                if (IsIncognitoModelState)
                {
                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                }
                else
                {
                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                }

                #endregion
            }
            #endregion
        }

        public static event EventHandler updateFailMessageEventHandler;

        public static void updateFailMessage(AntSdkFailOrSucessMessageDto failMessage)
        {
            if (updateFailMessageEventHandler != null)
            {
                updateFailMessageEventHandler(failMessage, null);
            }
        }
        private void CallbackValueById_upLoadFileMsgOnce(object sender, EventArgs e)
        {
            var upLoadFilesDto = sender as UpLoadFilesDto;
            if (upLoadFilesDto != null)
            {
                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(upLoadFilesDto.messageId))
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId[upLoadFilesDto.messageId] =
                        upLoadFilesDto.imageSendingId;
                }
                else
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(upLoadFilesDto.messageId,
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

            if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(sendMsg.messageId))
            {
                SendMsgListMonitor.MsgIdAndImgSendingId[sendMsg.messageId] = sendMsg.imageSendingId;
            }
            else
            {
                SendMsgListMonitor.MsgIdAndImgSendingId.Add(sendMsg.messageId, sendMsg.imageSendingId);
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

        /// <summary>
        /// 2017-03-07 滚轮事件
        /// </summary>
        public class callbackOnmousewheelBack
        {
            /// <summary>
            /// 存放sessionid和当前chatindex
            /// </summary>
            Dictionary<string, string> dictId = new Dictionary<string, string>();
            Dictionary<string, string> isFirstShow = new Dictionary<string, string>();
            public ChromiumWebBrowsers chromiumWebBrowser;
            private List<AntSdkGroupMember> GroupMembers;
            SendMessage_ctt s_ctt;
            public bool isScrolls = false;
            private List<string> listChatIndex;
            public AntSdkGroupInfo groupInfo;
            private IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> FirstPageData;
            private TalkGroupViewModel talkViewModel;
            public callbackOnmousewheelBack(ChromiumWebBrowsers chromiumWebBrowser, List<AntSdkGroupMember> GroupMembers, SendMessage_ctt s_ctt, List<string> listChatIndex, IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> firstPageData, AntSdkGroupInfo groupInfo, TalkGroupViewModel talkViewModel)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.GroupMembers = GroupMembers;
                    this.s_ctt = s_ctt;
                    FirstPageData = firstPageData;
                    this.listChatIndex = GetChatIndex(firstPageData); ;
                    this.groupInfo = groupInfo;
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
                                IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = null;
                                //向上查询数据
                                var index = Convert.ToInt32(chatindex);
                                if (!AntSdkService.AntSdkIsConnected)
                                {
                                    //网络异常，从本地数据库拉取最新pageSize条消息
                                    listChatdata = PublicMessageFunction.QueryMessageFromLocal(s_ctt.sessionId, AntSdkchatType.Group, false, pageSize, index);
                                }
                                else
                                {
                                    listChatdata = PublicMessageFunction.QueryMessageFromServer(s_ctt.sessionId, AntSdkchatType.Group, false, pageSize, index);
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
                                        if (!listChatIndex.Contains(list.messageId))
                                        {
                                            listChatIndex.Add(list.messageId);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        switch (Convert.ToInt32(/*TODO:AntSdk_Modify:list.MTP*/list.MsgType))
                                        {
                                            #region 消息处理
                                            case (int)AntSdkMsgType.CreateActivity:
                                                #region 活动
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    GroupActivity.RightGroupScrollActivity(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    GroupActivity.LeftGroupScrollActivity(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                #endregion
                                                break;
                                            case (int)AntSdkMsgType.CreateVote:
                                                #region 投票
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    GroupVote.RightGroupScrollVote(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    GroupVote.LeftGroupScrollVote(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                #endregion
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgMixMessage:
                                                List<string> imageId = new List<string>();
                                                imageId.Clear();
                                                List<MixMessageObjDto> receive = null;
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    var sourceData = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent);
                                                    foreach (var listpic in sourceData)
                                                    {
                                                        if (listpic.type == "1002")
                                                        {
                                                            PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(listpic.content?.ToString());
                                                            string guid = Guid.NewGuid().ToString();
                                                            string imgId = "RL" + guid;
                                                            imageId.Add(imgId);
                                                        }
                                                    }
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sourceData);
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        break;
                                                    }
                                                    foreach (var ilist in receive)
                                                    {
                                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                                        imageId.Add(imgId);
                                                        PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                                        AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.notBurn,
                                                        imgId, content.picUrl, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    }
                                                }
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    GroupSendPicAndText.RightGroupScrollPicAndTextMix(chromiumWebBrowser, list, imageId);
                                                }
                                                else
                                                {
                                                    GroupSendPicAndText.LeftGroupScrollPicAndTextMix(chromiumWebBrowser, list, GroupMembers, imageId);
                                                }
                                                receive.Reverse();
                                                imageId.Reverse();
                                                int j = 0;
                                                foreach (var imgInsert in receive)
                                                {
                                                    PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(imgInsert.content?.ToString());
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.notBurn,
                                                    imageId[j], content.picUrl, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    j++;
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgText:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, list);
                                                    }
                                                    PublicTalkMothed.RightGroupShowScrollText(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    PublicTalkMothed.LeftGroupShowScrollText(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgPicture:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendImage sendImage = new OnceSendImage();
                                                        sendImage.GroupInfo = groupInfo;
                                                        sendImage.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendImage);
                                                    }
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                    PublicTalkMothed.RightGroupShowScrollImage(chromiumWebBrowser, list);
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.notBurn,
                                                        list.messageId, rimgDto.picUrl, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                }
                                                else
                                                {
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                    PublicTalkMothed.LeftGroupShowScrollImage(chromiumWebBrowser, list, GroupMembers);
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.notBurn,
                                                       list.messageId, rimgDto.picUrl, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgFile:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendFile sendFile = new OnceSendFile();
                                                        sendFile.GroupInfo = groupInfo;
                                                        sendFile.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendFile);
                                                    }
                                                    if (list.uploadOrDownPath == "" || list.uploadOrDownPath == null)
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                    }
                                                    PublicTalkMothed.RightGroupShowScrollFile(chromiumWebBrowser,
                                                        list);
                                                }
                                                else
                                                {
                                                    if (list.uploadOrDownPath == "" || list.uploadOrDownPath == null)
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                    }
                                                    else
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                    }
                                                    PublicTalkMothed.LeftGroupShowScrollFile(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgAt:
                                                List<PictureAndTextMixDto> listAtPicAndText = new List<PictureAndTextMixDto>();
                                                string showContent = "";
                                                //构造展示消息
                                                var setlist = list as AntSdkChatMsg.At;
                                                if (setlist != null)
                                                {
                                                    SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
                                                    sortedDictionary.Clear();
                                                    Dictionary<int, Dictionary<string, string>> dictImg = new Dictionary<int, Dictionary<string, string>>();
                                                    dictImg.Clear();
                                                    int i = 0;
                                                    foreach (var str in setlist.content)
                                                    {
                                                        switch (str.type)
                                                        {
                                                            //文本
                                                            case "1001":
                                                                showContent += PublicTalkMothed.talkContentReplace(str.content);
                                                                break;
                                                            //@全体成员
                                                            case "1111":
                                                                showContent += "@全体成员";
                                                                break;
                                                            //@个人
                                                            case "1112":
                                                                string strAt = "";
                                                                if (str.ids.Count() > 1)
                                                                {
                                                                    foreach (var name in str.names)
                                                                    {
                                                                        strAt += "@" + name[0];
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    strAt += "@" + str.names[0];
                                                                }
                                                                showContent += strAt;
                                                                break;
                                                            //换行
                                                            case "0000":
                                                                showContent += "<br/>";
                                                                break;
                                                            //图片
                                                            case "1002":
                                                                PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(str.content);
                                                                string ImgUrl = pictureAndTextMix.picUrl;
                                                                string ImgId = "RL" + Guid.NewGuid().ToString();
                                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                                {
                                                                    showContent += "<img id=\"" + ImgId + "\" src=\"" + ImgUrl + "\" class=\"imgRightProportion\" ondblclick=\"myFunctions(event)\"/>";
                                                                }
                                                                else
                                                                {
                                                                    showContent += "<img id=\"" + ImgId + "\" src=\"" + ImgUrl + "\" class=\"imgLeftProportion\" ondblclick=\"myFunctions(event)\"/>";
                                                                }
                                                                i++;
                                                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                                                dictionary.Add(ImgId, ImgUrl);
                                                                dictImg.Add(i, dictionary);
                                                                break;
                                                        }
                                                    }
                                                    var dicSort = from sort in dictImg orderby sort.Key descending select sort;
                                                    foreach (var item in dicSort)
                                                    {
                                                        var InItem = item.Value;
                                                        foreach (var items in InItem)
                                                        {
                                                            AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.notBurn,
                                                            items.Key, items.Value, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                        }
                                                    }
                                                    if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                    {
                                                        List<PictureAndTextMixDto> picMixAt = null;
                                                        if (list.sendsucessorfail == 0)
                                                        {
                                                            list.chatType = (int)AntSdkchatType.Group;
                                                            picMixAt = JsonConvert.DeserializeObject<List<PictureAndTextMixDto>>(list.sourceContent);
                                                            foreach (var listat in picMixAt)
                                                            {
                                                                if (listat.type == PictureAndTextMixEnum.Image)
                                                                {
                                                                    PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(listat.content);
                                                                    string guid = Guid.NewGuid().ToString();
                                                                    string imgId = "RL" + guid;
                                                                    //imageId.Add(imgId);
                                                                    listat.ImgGuid = guid;
                                                                    listat.ImgId = imgId;
                                                                    listat.ImgPath = content.picUrl.Substring(8, content.picUrl.Length - 8);
                                                                    listat.content = content.picUrl;
                                                                }

                                                            }
                                                            OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, picMixAt);
                                                        }
                                                        //contnets atDto = JsonConvert.DeserializeObject<contnets>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        PublicTalkMothed.RightGroupShowAtScrollText(chromiumWebBrowser, list,
                                                                GroupMembers, showContent);
                                                    }
                                                    else
                                                    {
                                                        //AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                        PublicTalkMothed.leftGroupShowAtScrollText(chromiumWebBrowser, list, GroupMembers, showContent);
                                                    }
                                                }
                                                break;
                                            case (int)AntSdkMsgType.Revocation:
                                                chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.ScrollUIRecallMsg(list.sourceContent));
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgAudio:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    PublicTalkMothed.RightGroupShowScrollVoice(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    PublicTalkMothed.LeftGroupShowScrollVoice(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                                #endregion
                                        }
                                    }
                                    var indexList = listChatdata.Select(m => int.Parse(m.chatIndex)).Min();
                                    chromiumWebBrowser.scrollChatIndex = indexList.ToString();
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
            public void updateReadTime(string chatIndex)
            {
                string getChatIndex = chatIndex.Substring(43, chatIndex.Length - 43);
                int time = PublicTalkMothed.ConvertDateTimeInt(DateTime.Now);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.UpdateAfterReadBurnTime(s_ctt.sessionId, s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, getChatIndex, time.ToString());

            }
            public void deleteMessageByChatIndex(string chatIndex)
            {
                string getChatIndex = chatIndex.Substring(38, chatIndex.Length - 38);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, getChatIndex);
            }
            public static event EventHandler AddImgUrlEventHandler;

            private void AddImgUrl(AddImgUrlDto.InsertPreOrEnd preOrEnd, burnMsg.isBurnMsg isBurn, string chatIndex, string imgUrl, burnMsg.IsReadImg isRead, burnMsg.IsEffective isEffective)
            {
                if (AddImgUrlEventHandler != null)
                {
                    AddImgUrlDto addImg = new AddImgUrlDto();
                    addImg.PreOrEnd = preOrEnd;
                    addImg.IsBurn = isBurn;
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imgUrl;
                    addImg.IsRead = isRead;
                    addImg.IsEffective = isEffective;
                    AddImgUrlEventHandler(addImg, null);
                }
            }
        }
        /// <summary>
        /// 2017-03-08 阅后即焚滚轮事件
        /// </summary>
        public class callbackOnmousewheelBackBurn
        {
            /// <summary>
            /// 存放sessionid和当前chatindex
            /// </summary>
            Dictionary<string, string> dictId = new Dictionary<string, string>();
            Dictionary<string, string> isFirstShow = new Dictionary<string, string>();
            public ChromiumWebBrowsers chromiumWebBrowser;
            private List<AntSdkGroupMember> GroupMembers;
            SendMessage_ctt s_ctt;
            public bool isScrolls = false;
            private List<string> BurnListChatIndex;
            public AntSdkGroupInfo groupInfo;
            private TalkGroupViewModel TalkGroupView;
            public callbackOnmousewheelBackBurn(ChromiumWebBrowsers chromiumWebBrowser, List<AntSdkGroupMember> GroupMembers, SendMessage_ctt s_ctt, List<string> BurnListChatIndex, AntSdkGroupInfo groupInfo, TalkGroupViewModel talkGroupView)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.GroupMembers = GroupMembers;
                    this.s_ctt = s_ctt;
                    this.BurnListChatIndex = BurnListChatIndex;
                    this.groupInfo = groupInfo;
                    this.TalkGroupView = talkGroupView;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkViewModel_CallBackSelectDirecoryFile]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            public void isToEndWheel(bool isToEnd)
            {
                if (isToEnd == true)
                {
                    TalkGroupView.TextShowRowHeight = "0";
                    TalkGroupView.TextShowReceiveMsg = "";
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

                            if (!dictId.Keys.Contains(s_ctt.sessionId))
                            {
                                dictId.Add(s_ctt.sessionId, chatindex);
                                IList<AntSdkChatMsg.ChatBase> listChatdata = null;
                                T_Chat_Message_GroupBurnDAL groupchat = new T_Chat_Message_GroupBurnDAL();
                                listChatdata = AntSdkSqliteHelper.ModelConvertHelper<AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(groupchat.getDataByMoreThanScroll(s_ctt.sessionId, s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, chatindex, 10));

                                if (!isFirstShow.Keys.Contains(s_ctt.sessionId))
                                {
                                    isFirstShow.Add(s_ctt.sessionId, chatindex);
                                }
                                if (listChatdata.Count() > 0)
                                {
                                    foreach (var list in listChatdata)
                                    {
                                        list.flag = 1;
                                        if (!BurnListChatIndex.Contains(list.messageId))
                                        {
                                            BurnListChatIndex.Add(list.messageId);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        switch (Convert.ToInt32(list.MsgType))
                                        {
                                            case (int)AntSdkMsgType.ChatMsgText:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, list);
                                                    }
                                                    PublicTalkMothed.RightGroupShowScrollText(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    PublicTalkMothed.LeftGroupBurnShowScrollText(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgPicture:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendImage sendImage = new OnceSendImage();
                                                        sendImage.GroupInfo = groupInfo;
                                                        sendImage.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendImage);
                                                    }
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                    PublicTalkMothed.RightGroupShowScrollImage(chromiumWebBrowser, list);
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.yesBurn,
                                                       list.messageId, rimgDto.picUrl);
                                                }
                                                else
                                                {
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:list.content*/list.sourceContent);
                                                    PublicTalkMothed.LeftGroupBurnShowScrollImage(chromiumWebBrowser, list, GroupMembers);
                                                    AddImgUrl(AddImgUrlDto.InsertPreOrEnd.Pre, burnMsg.isBurnMsg.yesBurn,
                                                     list.messageId, rimgDto.picUrl);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgFile:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendFile sendFile = new OnceSendFile();
                                                        sendFile.GroupInfo = groupInfo;
                                                        sendFile.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendFile);
                                                    }
                                                    if (list.uploadOrDownPath == "" || list.uploadOrDownPath == null)
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;                                                     
                                                    }
                                                    PublicTalkMothed.RightGroupShowScrollFile(chromiumWebBrowser,
                                                         list);
                                                }
                                                else
                                                {
                                                    if (list.uploadOrDownPath == "" || list.uploadOrDownPath == null)
                                                    {
                                                        list.flag = 1;
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                    }
                                                    else
                                                    {
                                                        chromiumWebBrowser.sessionid = s_ctt.sessionId;
                                                    }
                                                    PublicTalkMothed.LeftGroupBurnShowScrollFile(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                            case (int)AntSdkMsgType.ChatMsgAudio:
                                                if (list.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    PublicTalkMothed.RightGroupShowScrollVoice(chromiumWebBrowser, list);
                                                }
                                                else
                                                {
                                                    PublicTalkMothed.LeftGroupBurnShowScrollVoice(chromiumWebBrowser, list, GroupMembers);
                                                }
                                                break;
                                        }
                                    }
                                    chromiumWebBrowser.scrollChatIndex = listChatdata[listChatdata.Count - 1].chatIndex;
                                    chromiumWebBrowser.ExecuteScriptAsync("setScrollToPosition('" + listChatdata[0].messageId + "');");
                                }
                                if (listChatdata.Count() > 0)
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
            public void updateReadTime(string chatIndex)
            {
                string getChatIndex = chatIndex.Substring(43, chatIndex.Length - 43);
                int time = PublicTalkMothed.ConvertDateTimeInt(DateTime.Now);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.UpdateAfterReadBurnTime(s_ctt.sessionId, s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, getChatIndex, time.ToString());

            }
            public void deleteMessageByChatIndex(string chatIndex)
            {
                string getChatIndex = chatIndex.Substring(38, chatIndex.Length - 38);
                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, getChatIndex);
            }
            public static event EventHandler AddImgUrlEventHandler;

            private void AddImgUrl(AddImgUrlDto.InsertPreOrEnd preOrEnd, burnMsg.isBurnMsg isBurn, string chatIndex, string imgUrl)
            {
                if (AddImgUrlEventHandler != null)
                {
                    AddImgUrlDto addImg = new AddImgUrlDto();
                    addImg.PreOrEnd = preOrEnd;
                    addImg.IsBurn = isBurn;
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imgUrl;
                    AddImgUrlEventHandler(addImg, null);
                }
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
                LogHelper.WriteError("MiCut_Click：" + ex.Message);
            }
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
                LogHelper.WriteError("MiCopy_Click：" + ex.Message);
            }

        }
        private void MiPaste_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                LogHelper.WriteError("MiPaste_Click：" + ex.Message);
            }
        }

        private void _richTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_isBurnMode == GlobalVariable.BurnFlag.IsBurn)
            {
                _richTextBox.isBurnMode = true;
            }
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
                            if (counts.Count() == 0)
                            {
                                hOffset = -22;
                                showTextMethod("发送消息不能为空，请重新输入！");
                                if (e != null)
                                {
                                    e.Handled = true;
                                }
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
                            if (counts.Count() == 0)
                            {
                                hOffset = -22;
                                showTextMethod("发送消息不能为空，请重新输入！");
                                if (e != null)
                                {
                                    e.Handled = true;
                                }
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
                        TextRange textRange = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End);
                        textRange.Text = "";
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[_richTextBox_PreviewKeyDown]" + ex.Message + ex.StackTrace);
            }
        }
        #region 属性
        /// <summary>
        /// 是否显示正常聊天展示窗口
        /// </summary>
        private string _isShowWinMsg = "Visible";
        public string isShowWinMsg
        {
            set
            {
                this._isShowWinMsg = value;
                RaisePropertyChanged(() => isShowWinMsg);
            }
            get { return this._isShowWinMsg; }
        }
        /// <summary>
        /// @消息展示
        /// </summary>
        private string _IsbtnTipShow = "Collapsed";
        public string IsbtnTipShow
        {
            set
            {
                this._IsbtnTipShow = value;
                RaisePropertyChanged(() => IsbtnTipShow);
            }
            get { return this._IsbtnTipShow; }
        }
        /// <summary>
        /// 是否显示阅后即焚聊天窗口
        /// </summary>
        private string _isShowBurnWinMsg = "Collapsed";
        public string isShowBurnWinMsg
        {
            set
            {
                this._isShowBurnWinMsg = value;
                RaisePropertyChanged(() => isShowBurnWinMsg);
            }
            get { return this._isShowBurnWinMsg; }
        }
        /// <summary>
        /// 是否显示Emoji按钮
        /// </summary>
        private string _isShowDelete = "Collapsed";
        public string isShowDelete
        {
            set
            {
                this._isShowDelete = value;
                RaisePropertyChanged(() => isShowDelete);
            }
            get { return this._isShowDelete; }
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
        /// 公告图标是否显示
        /// </summary>
        private Visibility _NoticeVisibility = Visibility.Visible;
        public Visibility NoticeVisibility
        {
            set
            {
                this._NoticeVisibility = value;
                RaisePropertyChanged(() => NoticeVisibility);
            }
            get { return this._NoticeVisibility; }
        }

        /// <summary>
        /// 是否显示提示内容
        /// </summary>
        private string _SetContents = "";
        public string SetContents
        {
            set
            {
                this._SetContents = value;
                RaisePropertyChanged(() => SetContents);
            }
            get { return this._SetContents; }
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

        private bool _isIncognitoModelState;
        /// <summary>
        /// 当前会话窗体的模式状态
        /// </summary>
        public bool IsIncognitoModelState
        {
            get { return _isIncognitoModelState; }
            set
            {
                _isIncognitoModelState = value;
                RaisePropertyChanged(() => IsIncognitoModelState);
            }
        }
        private string _GroupName;
        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string GroupName
        {
            get { return this._GroupName; }
            set
            {
                this._GroupName = value;
                RaisePropertyChanged(() => GroupName);
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

        private Visibility _txtNameVisibility = Visibility.Hidden;
        /// <summary>
        /// Textbox是否可见
        /// </summary>
        public Visibility txtNameVisibility
        {
            get { return this._txtNameVisibility; }
            set
            {
                this._txtNameVisibility = value;
                RaisePropertyChanged(() => txtNameVisibility);
            }
        }
        private Visibility _labNameVisibility = Visibility.Visible;
        /// <summary>
        /// Label是否可见
        /// </summary>
        public Visibility labNameVisibility
        {
            get { return this._labNameVisibility; }
            set
            {
                this._labNameVisibility = value;
                RaisePropertyChanged(() => labNameVisibility);
            }
        }
        private Visibility _borderVisibility = Visibility.Hidden;
        /// <summary>
        /// border是否可见
        /// </summary>
        public Visibility borderVisibility
        {
            get { return this._borderVisibility; }
            set
            {
                this._borderVisibility = value;
                RaisePropertyChanged(() => borderVisibility);
            }
        }

        private Visibility _GroupMemberVisibility = Visibility.Collapsed;
        /// <summary>
        /// 群成员列表是否可见
        /// </summary>
        public Visibility GroupMemberVisibility
        {
            get { return this._GroupMemberVisibility; }
            set
            {
                this._GroupMemberVisibility = value;
                RaisePropertyChanged(() => GroupMemberVisibility);
            }
        }

        private bool _isShowGroupMember = false;
        /// <summary>
        /// 群成员列表是否可见
        /// </summary>
        public bool IsShowGroupMember
        {
            get { return this._isShowGroupMember; }
            set
            {
                this._isShowGroupMember = value;
                if (!_isShowGroupMember)
                {
                    if (GroupMemberListViewModel != null &&
                        GroupMemberListViewModel.GroupMemberControlList?.Count > 0)
                    {
                        GroupMemberListViewModel.Dispose();
                        GroupMemberListViewModel = null;
                        //GC.Collect();
                    }
                }
                RaisePropertyChanged(() => IsShowGroupMember);
            }
        }
        private Brush _GroupNameBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8D8C89"));
        /// <summary>
        /// 讨论组名称边框颜色
        /// </summary>
        public Brush GroupNameBorderBrush
        {
            get { return _GroupNameBorderBrush; }
            set
            {
                if (_GroupNameBorderBrush == value)
                {
                    return;
                }
                _GroupNameBorderBrush = value;
                RaisePropertyChanged(() => GroupNameBorderBrush);
            }
        }

        /// <summary>
        /// At tip
        /// </summary>
        private string _ScrollPosition = "0,10,30,10";
        public string ScrollPosition
        {
            get { return this._ScrollPosition; }
            set
            {
                this._ScrollPosition = value;
                RaisePropertyChanged(() => ScrollPosition);
            }
        }
        private GroupMemberListViewModel _GroupMemberListViewModel;
        /// <summary>
        /// 头像
        /// </summary>
        public GroupMemberListViewModel GroupMemberListViewModel
        {
            get { return this._GroupMemberListViewModel; }
            set
            {
                this._GroupMemberListViewModel = value;
                RaisePropertyChanged(() => GroupMemberListViewModel);
            }
        }

        public void UpdateGroupMembers(List<AntSdkGroupMember> members)
        {
            this.GroupMembers = members;
            _richTextBox.RichTextSource = members;
            _richTextBox.GroupId = GroupInfo.groupId;
            var userOwner = members.FirstOrDefault(m => m.userId == AntSdkService.AntSdkLoginOutput.userId);
            //如果当前用户是群主
            if (userOwner?.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner ||
                userOwner?.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin)
            {
                if (_isBurnMode == GlobalVariable.BurnFlag.NotIsBurn)
                {
                    isShowBurn = "Visible";
                    isShowExit = "Collapsed";
                    isShowDelete = "Collapsed";
                    IsIncognitoModelState = false;
                    isAdminId = true;
                }
                else
                {
                    isShowBurn = "Collapsed";
                    isShowExit = "Visible";
                    isShowDelete = "Visible";
                    IsIncognitoModelState = true;
                    isAdminId = false;
                }
            }
            else
            {
                isShowBurn = "Collapsed";
                isShowExit = "Collapsed";
                isShowDelete = "Collapsed";
                IsIncognitoModelState = false;
                isAdminId = false;
            }
            //var userAdmin = members.FirstOrDefault(m => m.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin &&m.userId==AntSdkService.AntSdkLoginOutput.userId);
            ////如果当前用户是管理员
            //if (userAdmin != null)
            //{
            //    isAdminId = _isBurnMode == GlobalVariable.BurnFlag.NotIsBurn;
            //}
            //else
            //{
            //    isAdminId = false;
            //}

            //else
            //{
            //    if (_isBurnMode == GlobalVariable.BurnFlag.IsBurn)
            //    {
            //        isShowBurn = "Visible";
            //    }
            //    else
            //    {

            //    }
            //}


            //else
            //{
            //    IsIncognitoModelState = true;
            //}
            GroupMemberListViewModel?.UpdateGroupMembers(members);
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
        #endregion

        #region 命令/其他方法
        public void labNamePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //当前用户在讨论组中是不是管理员
                var user = GroupMembers.FirstOrDefault(v => v.userId == AntSdkService.AntSdkCurrentUserInfo.userId);
                if (user == null || user.roleLevel == (int)GlobalVariable.GroupRoleLevel.Ordinary) return;
                labNameVisibility = Visibility.Hidden;
                txtNameVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_labNamePreviewMouseDown]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 更新讨论组名
        /// </summary>
        public void UpdateName(string newName)
        {
            try
            {
                if (txtNameVisibility == Visibility.Hidden) return;
                if (string.IsNullOrEmpty(newName))
                {
                    MessageBoxWindow.Show("提示", "讨论组名不能为空！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                    GroupName = rememberName;
                    return;
                    //讨论组名字为空  提示
                }
                if (rememberName == newName)
                {
                    labNameVisibility = Visibility.Visible;
                    txtNameVisibility = Visibility.Hidden;
                    return;
                }
                else
                {
                    GroupName = newName;
                    rememberName = GroupName;
                    //UpdateGroupInput input = new UpdateGroupInput();
                    //input.token = AntSdkService.AntSdkLoginOutput.token;
                    //input.version = GlobalVariable.Version;
                    //input.userId = AntSdkService.AntSdkLoginOutput.userId;
                    //input.groupId = this.GroupInfo.groupId;
                    //input.groupName = GroupName;
                    //BaseOutput output = new BaseOutput();
                    var errCode = 0;
                    string errMsg = string.Empty;
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkUpdateGroupInput input = new AntSdkUpdateGroupInput();
                    input.userId = AntSdkService.AntSdkLoginOutput.userId;
                    input.groupId = this.GroupInfo.groupId;
                    input.groupName = GroupName;
                    var isResult = AntSdkService.UpdateGroup(input, ref errCode, ref errMsg);
                    if (isResult)
                    {

                    }
                    else
                    {
                        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                    }
                    //if ((new HttpService()).UpdateGroup(input, ref output, ref errMsg))
                    //{
                    //    //更新成功
                    //}
                    //else
                    //{
                    //    if (output.errorCode != "1004")
                    //    {
                    //        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                    //    }
                    //}
                }
                if (labNameVisibility == Visibility.Hidden)
                {
                    labNameVisibility = Visibility.Visible;
                    txtNameVisibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_UpdateName]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
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
                                TalkHistoryViewModel = new TalkHistoryViewModel(s_ctt, GroupInfo, GroupMembers);
                            }
                            if (TalkHistoryVisibility == Visibility.Visible)
                            {
                                TalkHistoryViewModel.cwm.Dispose();
                            }
                            TalkHistoryVisibility = TalkHistoryVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkGroupViewModel_HistoryCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._HistoryCommand;
            }
        }
        #region 公告
        private object _NoticeWindowListsViewModel;
        /// <summary>
        /// 公告列表
        /// </summary>
        public object NoticeWindowListsViewModel
        {
            get { return this._NoticeWindowListsViewModel; }
            set
            {
                this._NoticeWindowListsViewModel = value;
                RaisePropertyChanged(() => NoticeWindowListsViewModel);
            }
        }

        private object _noticeCreateVM;
        /// <summary>
        /// 创建公告
        /// </summary>
        public object NoticeCreateVM
        {
            get { return _noticeCreateVM; }
            set
            {
                _noticeCreateVM = value;
                RaisePropertyChanged(() => NoticeCreateVM);
            }
        }
        public Visibility _NoticeWindowListsViewModelVisibility = Visibility.Collapsed;
        /// <summary>
        /// 聊天记录是否可见
        /// </summary>
        public Visibility NoticeWindowListsViewModelVisibility
        {
            get { return this._NoticeWindowListsViewModelVisibility; }
            set
            {
                this._NoticeWindowListsViewModelVisibility = value;
                RaisePropertyChanged(() => NoticeWindowListsViewModelVisibility);
            }
        }

        private bool _isShowNoticeList;

        public bool IsShowNoticeList
        {
            get { return _isShowNoticeList; }
            set
            {
                _isShowNoticeList = value;
                RaisePropertyChanged(() => IsShowNoticeList);
            }
        }

        /// <summary>
        /// 通知列表
        /// </summary>
        private ICommand _NoticeListIsShow;
        public ICommand NoticeListIsShow
        {
            get
            {
                if (this._NoticeListIsShow == null)
                {
                    this._NoticeListIsShow = new DefaultCommand(o =>
                    {
                        try
                        {
                            //if (NoticeWindowListsViewModelVisibility == Visibility.Collapsed)
                            //{
                            NoticeWindowListsViewModel = new NoticeWindowListsViewModel(isAdminId, GroupInfo.groupId);
                            ((NoticeWindowListsViewModel)NoticeWindowListsViewModel).CreateNoticeEvent += NoticeWindowListsViewModel_CreateNoticeEvent;
                            //}
                            NoticeWindowListsViewModelVisibility = Visibility.Visible;
                            IsShowNoticeList = true;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkGroupViewModel_HistoryCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._NoticeListIsShow;
            }
        }


        /// <summary>
        /// 点击创建公告时触发
        /// </summary>
        private void NoticeWindowListsViewModel_CreateNoticeEvent(bool isCreateNotice)
        {
            if (isCreateNotice)
            {
                NoticeCreateVM = new NoticeAddWindowViewModel(GroupInfo.groupId);
                ((NoticeAddWindowViewModel)NoticeCreateVM).CreatedNoticeEvent += NoticeCreateVM_CreatedNoticeEvent;
                NoticeWindowListsViewModel = null;
            }
            else
            {
                NoticeWindowListsViewModel = null;
                NoticeWindowListsViewModelVisibility = Visibility.Collapsed;
                IsShowNoticeList = false;
            }
        }
        /// <summary>
        /// 创建公告之后触发
        /// </summary>
        private void NoticeCreateVM_CreatedNoticeEvent()
        {
            NoticeCreateVM = null;
            IsShowNoticeList = false;
            NoticeWindowListsViewModelVisibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 投票@方法
        /// </summary>
        /// <param name="atMsgList"></param>
        private void NoticeCreateVM_SendAtMsgEvent(List<object> atMsgList)
        {
            int maxChatindex = 0;

            //查询数据库最大chatindex
            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
            maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);

            //发送中提示
            AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
            failMessage.preChatIndex = maxChatindex;
            failMessage.mtp = (int)AntSdkMsgType.ChatMsgText;
            string content = "@全体成员";
            foreach (var list in atMsgList)
            {
                var contents = list as AntSdkChatMsg.contentText;

                if (contents != null)
                {
                    content += contents.content;
                }
            }
            failMessage.content = content;
            failMessage.sessionid = s_ctt.sessionId;
            DateTime dt = DateTime.Now;
            failMessage.lastDatetime = dt.ToString();

            string messageid = PublicTalkMothed.timeStampAndRandom();
            string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
            string imageSendingId = "sending" + imageTipId;

            if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
            {
                SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
            }
            else
            {
                SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
            }

            #region 插入数据

            //smt.content = JsonConvert.SerializeObject(outCtt);
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = new SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase();
            sm_ctt.MsgType = AntSdkMsgType.ChatMsgAt;
            /*TODO:AntSdk_Modify:list.content*/
            sm_ctt.sourceContent = JsonConvert.SerializeObject(atMsgList);
            //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
            sm_ctt.messageId = messageid;
            sm_ctt.sendUserId = s_ctt.sendUserId;
            sm_ctt.sessionId = s_ctt.sessionId;
            sm_ctt.targetId = GroupInfo.groupId;
            //sm_ctt.companyCode = s_ctt.companyCode;
            //sm_ctt.MTP = ((int)GlobalVariable.MsgType.At).ToString();
            sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
            sm_ctt.sendsucessorfail = 0;
            sm_ctt.SENDORRECEIVE = "1";
            ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));

            #endregion
            failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
            string atMsgjson = JsonConvert.SerializeObject(atMsgList);
            string base64Str = PublicTalkMothed.ConvertToBase64(atMsgjson);
            PublicTalkMothed.RightGroupShowSendAtText(chromiumWebBrowser, content, null, messageid, imageTipId, imageSendingId, dt, content, base64Str);

            #region 消息状态监控

            MessageStateArg arg = new MessageStateArg();
            arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
            arg.isGroup = true;
            arg.MessageId = messageid;
            arg.SessionId = s_ctt.sessionId;
            arg.WebBrowser = chromiumWebBrowser;
            arg.SendIngId = imageSendingId;
            arg.RepeatId = imageTipId;
            var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
            if (IsHave != null)
            {
                SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
            }
            SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

            #endregion

            sendAtTextMethod(content, messageid, imageTipId, imageSendingId, failMessage, null, atMsgList);

        }

        #endregion
        #region 投票

        private VoteListViewModel _voteListVm;
        /// <summary>
        /// 投票列表对象
        /// </summary>
        public VoteListViewModel VoteListVM
        {
            get { return _voteListVm; }
            set
            {
                _voteListVm = value;
                RaisePropertyChanged(() => VoteListVM);
            }
        }

        private CreateVoteViewModel _voteCreateVM;
        /// <summary>
        /// 创建投票
        /// </summary>
        public CreateVoteViewModel VoteCreateVM
        {
            get { return _voteCreateVM; }
            set
            {
                _voteCreateVM = value;
                RaisePropertyChanged(() => VoteCreateVM);
            }
        }
        //public Visibility _voteWindowListsViewModelVisibility = Visibility.Collapsed;
        ///// <summary>
        ///// 聊天记录是否可见
        ///// </summary>
        //public Visibility VoteWindowListsViewModelVisibility
        //{
        //    get { return this.VoteWindowListsViewModelVisibility; }
        //    set
        //    {
        //        this.VoteWindowListsViewModelVisibility = value;
        //        RaisePropertyChanged(() => VoteWindowListsViewModelVisibility);
        //    }
        //}

        private bool _isShowVoteList;

        public bool IsShowVoteList
        {
            get { return _isShowVoteList; }
            set
            {
                _isShowVoteList = value;
                RaisePropertyChanged(() => IsShowVoteList);
            }
        }

        /// <summary>
        /// 投票列表
        /// </summary>
        public ICommand VoteListShowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    NoticeWindowListsViewModel = new VoteListViewModel(GroupInfo.groupId);

                    ((VoteListViewModel)NoticeWindowListsViewModel).GoVoteOperationEvent += VoteListViewModel_CrateNoticeEvent;
                    ((VoteListViewModel)NoticeWindowListsViewModel).SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    if (_isBurnMode == GlobalVariable.BurnFlag.IsBurn)
                        ((VoteListViewModel)NoticeWindowListsViewModel).IsShowBtnAddVote = false;
                    //}
                    NoticeWindowListsViewModelVisibility = Visibility.Visible;
                    IsShowNoticeList = true;
                });
            }
        }

        private bool _isVoteVisibility = true;
        /// <summary>
        /// 是否要投票功能
        /// </summary>
        public bool IsVoteVisibility
        {
            get { return _isVoteVisibility; }
            set
            {
                _isVoteVisibility = value;
                RaisePropertyChanged(() => IsVoteVisibility);
            }
        }
        /// <summary>
        /// 通过投票列表跳转
        /// </summary>
        /// <param name="isVote"></param>
        /// <param name="type"></param>
        /// <param name="voteId"></param>
        private void VoteListViewModel_CrateNoticeEvent(bool isVote, VoteViewType type, int voteId)
        {
            if (!isVote)
            {
                NoticeWindowListsViewModel = null;
                NoticeWindowListsViewModelVisibility = Visibility.Collapsed;
                IsShowNoticeList = false;
                return;
            }
            var groupMembersCount = GroupMembers != null && GroupMembers.Count > 0 ? GroupMembers.Count(m => m.userId != AntSdkService.AntSdkCurrentUserInfo.robotId) : 0;
            switch (type)
            {
                case VoteViewType.CreateVote:

                    NoticeCreateVM = new CreateVoteViewModel(GroupInfo.groupId);
                    ((CreateVoteViewModel)NoticeCreateVM).CreatedVoteEvent += NoticeCreateVM_CreatedNoticeEvent;
                    ((CreateVoteViewModel)NoticeCreateVM).GoVoteEvent += VoteViewModel_GoVoteEvent;
                    NoticeWindowListsViewModel = null;

                    break;
                case VoteViewType.VoteDetail:
                    NoticeCreateVM = new VoteDetailView();
                    var voteDetatl = new VoteDetailViewModel(type, voteId, groupMembersCount);
                    ((VoteDetailView)NoticeCreateVM).DataContext = voteDetatl;
                    voteDetatl.CloseVoteViewEvent += NoticeCreateVM_CreatedNoticeEvent;
                    voteDetatl.SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    voteDetatl.GoVoteEvent += VoteViewModel_GoVoteEvent;
                    NoticeWindowListsViewModel = null;
                    break;

                case VoteViewType.VoteResult:
                    NoticeCreateVM = new VoteResultView();
                    var voteResultDetatl = new VoteDetailViewModel(type, voteId, groupMembersCount);
                    ((VoteResultView)NoticeCreateVM).DataContext = voteResultDetatl;
                    voteResultDetatl.CloseVoteViewEvent += NoticeCreateVM_CreatedNoticeEvent;
                    voteResultDetatl.SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    NoticeWindowListsViewModel = null;
                    break;
            }

        }
        /// <summary>
        /// 消息框跳转到投票详情
        /// </summary>
        /// <param name="voteId"></param>
        /// <param name="voteEndDateTime"></param>
        private void GoVote(int voteId)
        {
            var groupMembersCount = GroupMembers != null && GroupMembers.Count > 0 ? GroupMembers.Count(m => m.userId != AntSdkService.AntSdkCurrentUserInfo.robotId) : 0;
            var errCode = 0;
            var errMsg = string.Empty;
            var output = AntSdkService.GetVoteInfo(voteId, AntSdkService.AntSdkCurrentUserInfo.userId, ref errCode, ref errMsg);
            if (output == null) return;
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            var isVoteEnd = DateTime.Compare(Convert.ToDateTime(output.expiryTime), serverDateTime) < 0;
            if (output.voted || isVoteEnd)
            {
                AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                {
                    NoticeWindowListsViewModelVisibility = Visibility.Visible;
                    NoticeCreateVM = new VoteResultView();
                    var voteResultDetatil = new VoteDetailViewModel(output, VoteViewType.VoteResult, groupMembersCount,
                        serverDateTime);
                    ((VoteResultView)NoticeCreateVM).DataContext = voteResultDetatil;
                    voteResultDetatil.CloseVoteViewEvent += NoticeCreateVM_CreatedNoticeEvent;
                    voteResultDetatil.SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    NoticeWindowListsViewModel = null;
                });
            }
            else
            {
                AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                {
                    NoticeWindowListsViewModelVisibility = Visibility.Visible;
                    NoticeCreateVM = new VoteDetailView();
                    var voteDetail = new VoteDetailViewModel(output, VoteViewType.VoteDetail, groupMembersCount, serverDateTime);
                    ((VoteDetailView)NoticeCreateVM).DataContext = voteDetail;
                    voteDetail.CloseVoteViewEvent += NoticeCreateVM_CreatedNoticeEvent;
                    voteDetail.SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    voteDetail.GoVoteEvent += VoteViewModel_GoVoteEvent;
                    NoticeWindowListsViewModel = null;
                });

            }

        }

        /// <summary>
        /// 投票步骤
        /// </summary>
        /// <param name="voteId">投票标识</param>
        /// <param name="type"></param>
        private void VoteViewModel_GoVoteEvent(int voteId, VoteViewType type)
        {
            VoteListViewModel_CrateNoticeEvent(true, type, voteId);
        }

        #endregion

        #region 活动
        private ActivityListViewModel _activityListVm;
        /// <summary>
        /// 活动列表对象
        /// </summary>
        public ActivityListViewModel ActivityListVM
        {
            get { return _activityListVm; }
            set
            {
                _activityListVm = value;
                RaisePropertyChanged(() => ActivityListVM);
            }
        }

        private ReleaseActivityViewModel _releaseActivityVM;
        /// <summary>
        /// 发布活动
        /// </summary>
        public ReleaseActivityViewModel ReleaseActivityVM
        {
            get { return _releaseActivityVM; }
            set
            {
                _releaseActivityVM = value;
                RaisePropertyChanged(() => ReleaseActivityVM);
            }
        }
        //public Visibility _voteWindowListsViewModelVisibility = Visibility.Collapsed;
        ///// <summary>
        ///// 聊天记录是否可见
        ///// </summary>
        //public Visibility VoteWindowListsViewModelVisibility
        //{
        //    get { return this.VoteWindowListsViewModelVisibility; }
        //    set
        //    {
        //        this.VoteWindowListsViewModelVisibility = value;
        //        RaisePropertyChanged(() => VoteWindowListsViewModelVisibility);
        //    }
        //}

        private bool _isShowActivityList;

        public bool IsShowActivityList
        {
            get { return _isShowActivityList; }
            set
            {
                _isShowActivityList = value;
                RaisePropertyChanged(() => IsShowActivityList);
            }
        }

        /// <summary>
        /// 活动列表
        /// </summary>
        public ICommand ActivityListShowCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    NoticeWindowListsViewModel = new ActivityListViewModel(isAdminId, GroupInfo.groupId);

                    ((ActivityListViewModel)NoticeWindowListsViewModel).GoActivityOperationEvent += ActivityListViewModel_CrateNoticeEvent;
                    ((ActivityListViewModel)NoticeWindowListsViewModel).SendAtMsgEvent += NoticeCreateVM_SendAtMsgEvent;
                    //if (_isBurnMode == GlobalVariable.BurnFlag.IsBurn)
                    //    ((VoteListViewModel)NoticeWindowListsViewModel).IsShowBtnAddVote = false;
                    //}
                    NoticeWindowListsViewModelVisibility = Visibility.Visible;
                    IsShowNoticeList = true;
                });
            }
        }

        private bool _isActivityVisibility = true;
        /// <summary>
        /// 是否要活动功能
        /// </summary>
        public bool IsActivityVisibility
        {
            get { return _isActivityVisibility; }
            set
            {
                _isActivityVisibility = value;
                RaisePropertyChanged(() => IsActivityVisibility);
            }
        }
        /// <summary>
        /// 活动页面跳转
        /// </summary>
        /// <param name="isVote"></param>
        /// <param name="type"></param>
        /// <param name="activityId"></param>
        private void ActivityListViewModel_CrateNoticeEvent(bool isVote, ActivityViewType type, int activityId)
        {
            if (!isVote)
            {
                NoticeWindowListsViewModel = null;
                NoticeWindowListsViewModelVisibility = Visibility.Collapsed;
                return;
            }
            switch (type)
            {
                case ActivityViewType.ReleaseActivity:
                    NoticeCreateVM = new ReleaseActivityViewModel(GroupInfo.groupId);
                    ((ReleaseActivityViewModel)NoticeCreateVM).ReleasedActivityEvent += NoticeCreateVM_CreatedNoticeEvent;
                    NoticeWindowListsViewModel = null;

                    break;
                case ActivityViewType.ActivityDetail:
                case ActivityViewType.ActivityResult:
                    if (NoticeWindowListsViewModelVisibility == Visibility.Collapsed)
                        NoticeWindowListsViewModelVisibility = Visibility.Visible;
                    NoticeCreateVM = new ActivityDetailsViewModel(activityId, GroupInfo.groupId);
                    NoticeWindowListsViewModel = null;
                    ((ActivityDetailsViewModel)NoticeCreateVM).CloseActivityEvent += NoticeCreateVM_CreatedNoticeEvent;
                    break;
            }

        }
        #endregion

        bool isGroupMemeberImageCLick = false;

        /// <summary>
        /// 群成员图标鼠标左键点击事件
        /// </summary>
        private ICommand _GroupMemeberImageMouseLeftButtonDown;

        public ICommand GroupMemeberImageMouseLeftButtonDown
        {
            get
            {
                if (this._GroupMemeberImageMouseLeftButtonDown == null)
                {
                    this._GroupMemeberImageMouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        IsShowGroupMember = true;
                        try
                        {
                            if (IsIncognitoModelState)
                            {
                                isShowAdminMenu(false);
                            }

                            if (GroupMemberListViewModel == null)
                            {
                                if (GroupMembers == null || GroupMembers.Count == 0)
                                    GetGroupMembers();
                                if (GroupMembers != null && GroupMembers.Count > 0)
                                {
                                    GroupMemberListViewModel = new GroupMemberListViewModel(GroupMembers, this.GroupInfo);
                                    GroupMemberListViewModel.CloseGroupListEvent += GroupMemberListViewModel_CloseGroupListEvent;
                                }
                            }
                            else
                            {
                                GroupMemberListViewModel.CloseGroupListEvent -= GroupMemberListViewModel_CloseGroupListEvent;
                                GroupMemberListViewModel.CloseGroupListEvent += GroupMemberListViewModel_CloseGroupListEvent;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkGroupViewModel_GroupMemeberImageMouseLeftButtonDown]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._GroupMemeberImageMouseLeftButtonDown;
            }
        }

        private void GroupMemberListViewModel_CloseGroupListEvent()
        {
            IsShowGroupMember = false;
            //if (GroupMemberListViewModel != null && GroupMemberListViewModel.GroupMemberControlList?.Count > 0)
            //{
            //    GroupMemberListViewModel.GroupMemberControlList.Clear();
            //    GroupMemberListViewModel.GroupMemberControlList = null;
            //    if (GroupMemberListViewModel.SearchGroupMemberControlList != null)
            //        GroupMemberListViewModel.SearchGroupMemberControlList = null;

            //    GroupMemberListViewModel = null;
            //}
        }

        private void GetGroupMembers()
        {
            try
            {
                //GetGroupMembersInput input = new GetGroupMembersInput();
                //input.token = AntSdkService.AntSdkLoginOutput.token;
                //input.version = GlobalVariable.Version;
                //input.userId = AntSdkService.AntSdkLoginOutput.userId;
                //input.groupId = this.GroupInfo.groupId;
                //GetGroupMembersOutput output = new GetGroupMembersOutput();
                //string errMsg = string.Empty;
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                var menbers = GroupPublicFunction.GetMembers(this.GroupInfo.groupId);
                if (menbers != null && menbers.Count > 0)
                    GroupMembers = menbers;
                //if ((new HttpService()).GetGroupMembers(input, ref output, ref errMsg))
                //{
                //    GroupMembers = output.users;
                //}
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_GetGroupMembers]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        #region 退出讨论组

        public delegate void ExitGroupDelegate(string groupId);

        public static event ExitGroupDelegate ExitGroupEvent;

        private void OnExitGroupEvent(string groupId)
        {
            try
            {
                if (ExitGroupEvent != null)
                {
                    ExitGroupEvent(groupId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_OnExitGroupEvent]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        /// <summary>
        /// 退出讨论组命令
        /// </summary>
        private ICommand _ExitGroupMouseLeftButtonDown;

        public ICommand ExitGroupMouseLeftButtonDown
        {
            get
            {
                if (this._ExitGroupMouseLeftButtonDown == null)
                {
                    this._ExitGroupMouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        try
                        {
                            if (MessageBoxWindow.Show("提醒", string.Format("确定要退出{0}吗？", GroupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Ok)
                            {
                                ExitGroup();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkGroupViewModel_ExitGroupMouseLeftButtonDown]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._ExitGroupMouseLeftButtonDown;
            }
        }

        private void ExitGroup()
        {
            //退出讨论组之前需要先更新讨论组头像
            //UpdateGroupInput updateInput = new UpdateGroupInput();
            //updateInput.groupId = this.GroupInfo.groupId;
            //updateInput.token = AntSdkService.AntSdkLoginOutput.token;
            //updateInput.userId = AntSdkService.AntSdkLoginOutput.userId;
            //updateInput.version = GlobalVariable.Version;
            //updateInput.groupPicture = ImageHandle.GetGroupPicture(GroupMembers.Where(c => c.userId != updateInput.userId).Select(c => c.picture).ToList());
            //BaseOutput updateOutput = new BaseOutput();
            //string errMsg = string.Empty;
            //(new HttpService()).UpdateGroup(updateInput, ref updateOutput, ref errMsg);

            //ExitGroupInput input = new ExitGroupInput();
            //input.groupId = this.GroupInfo.groupId;
            //input.token = AntSdkService.AntSdkLoginOutput.token;
            //input.userId = AntSdkService.AntSdkLoginOutput.userId;
            //input.version = GlobalVariable.Version;
            //BaseOutput output = new BaseOutput();
            //string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            if (this.GroupInfo == null) return;
            var isResult = GroupPublicFunction.ExitGroup(this.GroupInfo.groupId, this.GroupInfo.groupName, GroupMembers);
            if (isResult)
            {
                OnExitGroupEvent(this.GroupInfo.groupId);
            }
            //if ((new HttpService()).ExitGroup(input, ref output, ref errMsg))
            //{
            //    string[] ThreadParams = new string[2];
            //    ThreadParams[0] = this.GroupInfo.groupId;
            //    ThreadParams[1] = ImageHandle.GetGroupPicture(GroupMembers.Where(c => c.userId != AntSdkService.AntSdkLoginOutput.userId).Select(c => c.picture).ToList());
            //    Thread UpdateGroupPictureThread = new Thread(UpdateGroupPicture);
            //    UpdateGroupPictureThread.Start(ThreadParams);

            //    OnExitGroupEvent(this.GroupInfo.groupId);
            //}
            //else
            //{
            //    if (output.errorCode != "1004")
            //    {
            //        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            //    }
            //}
        }


        /// <summary>
        /// 失去焦点
        /// </summary>
        private ICommand _EditNameLostFocus;

        public ICommand EditNameLostFocus
        {
            get
            {
                if (this._EditNameLostFocus == null)
                {
                    this._EditNameLostFocus = new DefaultCommand(o =>
                    {
                        //if (string.IsNullOrEmpty(GroupName))
                        //{
                        //    //讨论组名字为空  提示
                        //    GroupName = rememberName;
                        //}
                        //else
                        //{
                        //    rememberName = GroupName;
                        //    UpdateGroupInput input = new UpdateGroupInput();
                        //    input.token = AntSdkService.AntSdkLoginOutput.token;
                        //    input.version = GlobalVariable.Version;
                        //    input.userId = AntSdkService.AntSdkLoginOutput.userId;
                        //    input.groupId = this.GroupInfo.groupId;
                        //    input.groupName = GroupName;
                        //    BaseOutput output = new BaseOutput();
                        //    string errMsg = string.Empty;
                        //    if ((new HttpService()).UpdateGroup(input, ref output, ref errMsg))
                        //    {
                        //        //更新成功
                        //    }
                        //    else
                        //    {
                        //        MessageBoxWindow.Show(errMsg);
                        //    }
                        //}
                    });
                }
                return this._EditNameLostFocus;
            }
        }

        #endregion

        /// <summary>
        /// 邀请加入讨论组事件
        /// </summary>
        private ICommand _InviteToGroupMouseLeftButtonDown;

        public ICommand InviteToGroupMouseLeftButtonDown
        {
            get
            {
                if (this._InviteToGroupMouseLeftButtonDown == null)
                {
                    this._InviteToGroupMouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        try
                        {
                            Views.Contacts.GroupEditWindowView win = new Views.Contacts.GroupEditWindowView();
                            win.ShowInTaskbar = false;
                            List<string> userIds = GroupMembers.Select(c => c.userId).ToList();
                            GroupEditWindowViewModel model = new GroupEditWindowViewModel(win.Close, userIds, this.GroupInfo);
                            win.DataContext = model;
                            win.Owner = Antenna.Framework.Win32.GetTopWindow();
                            win.ShowDialog();
                            //if (model.NewGroupMemberList != null && model.NewGroupMemberList.Count > 0)
                            //{
                            //    GroupMemberListViewModel.AddNewMember(model.NewGroupMemberList);
                            //    this.Picture = model.NewGroupPicture;
                            //    AddGroupMembers(model.NewGroupMemberList);
                            //    OnInviteToGroupEvent(this.GroupInfo.groupId, this.Picture, model.NewGroupMemberList);
                            //}
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[TalkGroupViewModel_InviteToGroupMouseLeftButtonDown]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._InviteToGroupMouseLeftButtonDown;
            }
        }

        private void AddGroupMembers(List<AntSdkContact_User> userList)
        {
            try
            {
                foreach (AntSdkContact_User user in userList)
                {
                    AntSdkGroupMember member = new AntSdkGroupMember();
                    member.picture = user.picture;
                    member.position = user.position;
                    member.roleLevel = (int)GlobalVariable.GroupRoleLevel.Ordinary;
                    member.userId = user.userId;
                    member.userName = user.userName;
                    GroupMembers.Add(member);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_AddGroupMembers]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        public delegate void InviteToGroupDelegate(string groupId, string picture, List<AntSdkContact_User> newGroupMemberList);

        public static event InviteToGroupDelegate InviteToGroupEvent;

        private void OnInviteToGroupEvent(string groupId, string picture, List<AntSdkContact_User> newGroupMemberList)
        {
            if (InviteToGroupEvent != null)
            {
                InviteToGroupEvent(groupId, picture, newGroupMemberList);
            }
        }

        private void KickoutGroup(string groupId, string userId, string picture)
        {
            if (this.GroupInfo.groupId != groupId) return;
            AntSdkGroupMember user = GroupMembers.FirstOrDefault(c => c.userId == userId);
            GroupMembers.Remove(user);
            this.Picture = picture;
        }

        #endregion

        /// <summary>
        /// 触发关键词
        /// </summary>
        private List<char> _ContentAssistTriggers = new List<char>();

        public List<char> ContentAssistTriggers
        {
            set
            {
                _ContentAssistTriggers = value;
                RaisePropertyChanged(() => ContentAssistTriggers);
            }
            get { return _ContentAssistTriggers; }
        }

        public RichTextBoxEx _richTextBox;

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitRichTextBoxEx()
        {
            this.chromiumWebBrowser.richTextBox = _richTextBox;
            if (s_ctt != null)
            {
                userIMImageSavePath = publicMethod.localDataPath() + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\group\\cutImg\\";
                msgEditAssistant = new MsgEditAssistant(_richTextBox, userIMImageSavePath);
            }
            _richTextBox.PreviewKeyDown += _richTextBox_PreviewKeyDown;
            _richTextBox.Document.LineHeight = 2;
            _richTextBox.Document.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            _richTextBox.Document.FontSize = 13;
            _richTextBox.ContextMenu = cm;
            //_richTextBox.Focus();
            ContentAssistTriggers.Add('@');
            if (GroupMembers?.Count > 0 && (_richTextBox.RichTextSource == null || _richTextBox.RichTextSource.Count == 0))
                _richTextBox.RichTextSource = GroupMembers;
            if (GroupInfo != null)
                _richTextBox.GroupId = GroupInfo.groupId;
        }

        private ActionCommand<RichTextBoxEx> _RichTextBoxCommand;

        public ActionCommand<RichTextBoxEx> RichTextBoxCommand
        {
            get
            {
                if (this._RichTextBoxCommand == null)
                {
                    this._RichTextBoxCommand = new ActionCommand<RichTextBoxEx>(o =>
                    {
                        _richTextBox = o;
                        //_richTextBox.IsReadOnly = true;
                        InitRichTextBoxEx();
                        _richTextBox.PreviewDragOver += richTextBox_PreviewDragOver;
                        _richTextBox.PreviewDrop += richTextBox_PreviewDrop;
                    });
                }
                return this._RichTextBoxCommand;
            }
        }

        static string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
        static string indexPath = "file:///" + pathHtml + "web_content/group.html";
        public ChromiumWebBrowsers _chromiumWebBrowser = new ChromiumWebBrowsers() { Address = indexPath };

        public ChromiumWebBrowsers chromiumWebBrowser
        {
            get { return _chromiumWebBrowser; }
            set
            {
                _chromiumWebBrowser = value;
                RaisePropertyChanged(() => chromiumWebBrowser);
            }

        }
        public DispatcherTimer timer = new DispatcherTimer();
        int defaultPage = 0;
        int counter = 0;
        private object obj = new object();

        private void Timer_Tick(object sender, EventArgs e)
        {
            string add = "";
            counter++;
            try
            {
                if (chromiumWebBrowser.Visibility != Visibility.Visible || chromiumWebBrowser.IsInitialized != true || chromiumWebBrowser.IsBrowserInitialized != true || chromiumWebBrowser.GetMainFrame().Url == "")
                {
                    return;
                }
                //判断js是否可以执行
                Task<JavascriptResponse> task = this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.add());
                task.Wait();
                if (task.Result.Success && task.Result.Result.ToString() == "2")
                {
                    timer.Stop();
                    LogHelper.WriteWarn("TalkGroupViewModel_Timer_Tick_LoadMsgData..................");
                    LoadMsgData(GlobalVariable.BurnFlag.NotIsBurn);
                    var cefBool = CefList.ContainsKey(this.GroupInfo.groupId);
                    if (!cefBool)
                    {
                        GroupCef groupCef = new GroupCef();
                        groupCef.Cef = _chromiumWebBrowser;
                        CefList.Add(this.GroupInfo.groupId, groupCef);
                    }
                    else
                    {
                        var list = CefList.SingleOrDefault(m => m.Key == this.GroupInfo.groupId).Value as GroupCef;
                        list.Cef = _chromiumWebBrowser;
                    }
                    if (_richTextBox != null)
                    {
                        _richTextBox.IsReadOnly = false;
                        _richTextBox.Focus();
                    }
                    counter = 0;
                    add = task.Result.Result.ToString();
                    LogHelper.WriteDebug("TalkGroupViewModel_Timer_Tick时间Success:" + DateTime.Now.ToLongTimeString());
                    #region 未读消息解析
                    {
                        string checkId = "";
                        #region 默认显示一页消息逻辑

                        if (defaultPage == 0)
                        {
                            //if (_unreadCount == 0)
                            //{
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
                                    lock (obj)
                                    {
                                        #region 2014-06-14修改 New
                                        List<string> imageId = new List<string>();
                                        imageId.Clear();
                                        listChatIndex.Add(list.messageId);
                                        switch (list.MsgType)
                                        {
                                            case AntSdkMsgType.ChatMsgMixMessage:
                                                #region 图文混合
                                                List<MixMessageObjDto> picMix = null;
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    picMix = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent.ToString());
                                                    foreach (var listpic in picMix)
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
                                                    #region 图文混合
                                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    int j = 0;
                                                    foreach (var ilist in receive)
                                                    {
                                                        PictureAndTextMixContentDto contents = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imageId[j], contents.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                        j++;
                                                    }
                                                    #endregion
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, picMix);
                                                }
                                                else
                                                {

                                                    List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    foreach (var ilist in receive)
                                                    {
                                                        string imgId = "RL" + Guid.NewGuid().ToString();
                                                        imageId.Add(imgId);
                                                        PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, content.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    }
                                                }
                                                #endregion
                                                break;
                                            case AntSdkMsgType.ChatMsgText:
                                                #region 文本
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Group;
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, list);
                                                }
                                                #endregion
                                                break;
                                            case AntSdkMsgType.ChatMsgPicture:
                                                #region 图片
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Group;
                                                    OnceSendImage sendImage = new OnceSendImage();
                                                    sendImage.GroupInfo = this.GroupInfo;
                                                    sendImage.ctt = this.s_ctt;
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendImage);
                                                }
                                                if (list.MsgType == AntSdkMsgType.ChatMsgPicture)
                                                {
                                                    list.chatIndex = list.messageId;
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:msg.content*/list.sourceContent);
                                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, list.messageId, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                }
                                                #endregion
                                                break;
                                            case AntSdkMsgType.ChatMsgFile:
                                                #region 文件
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Group;
                                                    OnceSendFile sendFile = new OnceSendFile();
                                                    sendFile.GroupInfo = this.GroupInfo;
                                                    sendFile.ctt = this.s_ctt;
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendFile);
                                                }
                                                #endregion
                                                break;
                                            case AntSdkMsgType.ChatMsgAt:
                                                #region AT
                                                List<PictureAndTextMixDto> picMixAt = null;
                                                if (list.sendsucessorfail == 0)
                                                {
                                                    list.chatType = (int)AntSdkchatType.Group;
                                                    picMixAt = JsonConvert.DeserializeObject<List<PictureAndTextMixDto>>(list.sourceContent);
                                                    foreach (var listat in picMixAt)
                                                    {
                                                        if (listat.type == PictureAndTextMixEnum.Image)
                                                        {
                                                            PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(listat.content);
                                                            string guid = Guid.NewGuid().ToString();
                                                            string imgId = "RL" + guid;
                                                            imageId.Add(imgId);
                                                            listat.ImgGuid = guid;
                                                            listat.ImgId = imgId;
                                                            listat.ImgPath = content.picUrl.Substring(8, content.picUrl.Length - 8);
                                                            listat.content = content.picUrl;
                                                        }

                                                    }
                                                    #region 图文混合
                                                    List<AntSdkChatMsg.MixMessage_content> receive = JsonConvert.DeserializeObject<List<AntSdkChatMsg.MixMessage_content>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                    int j = 0;
                                                    foreach (var ilist in receive)
                                                    {
                                                        PictureAndTextMixContentDto contents = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imageId[j], contents.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                        j++;
                                                    }
                                                    #endregion
                                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, picMixAt);

                                                }
                                                List<AntSdkChatMsg.MixMessage_content> receiveAt = JsonConvert.DeserializeObject<List<AntSdkChatMsg.MixMessage_content>>(list.sourceContent).Where(m => m.type == "1002").ToList();
                                                foreach (var ilist in receiveAt)
                                                {
                                                    string imgId = "RL" + Guid.NewGuid().ToString();
                                                    imageId.Add(imgId);
                                                    PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, content.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                }
                                                #endregion
                                                break;
                                        }
                                        string oneStr = PublicTalkMothed.LeftGroupToGroupShowMessage(list, GroupMembers, imageId);
                                        sbLeft.AppendLine(oneStr);

                                        #endregion
                                    }
                                }
                                string endStr = PublicTalkMothed.endString();
                                sbLeft.AppendLine(endStr);
                                bool IsSucess = false;
                                Task<JavascriptResponse> result = _chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
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
                                }
                                #region 2017-12-03 屏蔽
                                //Task<JavascriptResponse> result = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                //result.Wait();
                                //if (!result.Result.Success)
                                //{
                                //    Thread.Sleep(50);
                                //    LogHelper.WriteWarn("[TalkViewModel-FirstPageData:]" + sbLeft.ToString());
                                //    Task<JavascriptResponse> results2 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                //    results2.Wait();
                                //    if (results2.Result.Success)
                                //    {
                                //        LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第二次执行成功---------------------");
                                //    }
                                //    else
                                //    {
                                //        LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第二次执行失败---------------------");
                                //        Task<JavascriptResponse> results3 = _chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                //        results3.Wait();
                                //        Thread.Sleep(100);
                                //        if (results3.Result.Success)
                                //        {
                                //            LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第三次次执行成功---------------------");
                                //        }
                                //        else
                                //        {
                                //            LogHelper.WriteWarn("---------------------------[TalkViewModel-FirstPageData:]第三次执行失败---------------------");
                                //        }
                                //    }
                                //}
                                #endregion
                                defaultPage = 1;
                                FirstPageData = null;
                                ChatMsgLst.Clear();
                                result.Dispose();
                                this._chromiumWebBrowser.EvaluateScriptAsync("setscross();");
                            }
                            else
                            {
                                if (ChatMsgLst?.Count > 0)
                                    ShowMsgData();
                            }
                        }
                    }

                    #endregion
                    if (_richTextBox != null)
                    {
                        _richTextBox.IsReadOnly = false;
                        _richTextBox.Focus();
                    }
                }
                #endregion
                //注册在线消息接收事件
                if (_windowHelper == null)
                {
                    _windowHelper = new WindowHelper(this.s_ctt.sessionId);
                    _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived += LocalMessageHelper_InstantMessageHasBeenReceived;
                    if (_isChanageWindow)
                        WindowMonitor.ChanageWindowHelper(_windowHelper);
                }
                if (counter == 10)
                {
                    if (add != "2")
                    {
                        timer.Stop();
                        counter = 0;
                    }
                    LogHelper.WriteDebug("TalkGroupViewModel_Timer_Tick超时:" + DateTime.Now.ToLongTimeString());
                }
            }
            catch (Exception ex)
            {
                //注册在线消息接收事件
                if (_windowHelper == null)
                {
                    _windowHelper = new WindowHelper(this.s_ctt.sessionId);
                    _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived += LocalMessageHelper_InstantMessageHasBeenReceived;
                    if (_isChanageWindow)
                        WindowMonitor.ChanageWindowHelper(_windowHelper);
                }
                LogHelper.WriteError("TalkGroupViewModel_Timer_Tick错误:" + ex.Source + ex.Message + ex.StackTrace);
            }
        }

        static string pathHtmlBurn = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
        static string indexPathBurn = "file:///" + pathHtml + "web_content/groupBurn.html";

        /// <summary>
        /// 阅后即焚显示
        /// </summary>
        public ChromiumWebBrowsers _chromiumWebBrowserburn = new ChromiumWebBrowsers() { Address = indexPathBurn };

        public ChromiumWebBrowsers chromiumWebBrowserburn
        {
            set
            {
                _chromiumWebBrowserburn = value;
                RaisePropertyChanged(() => chromiumWebBrowserburn);
            }
            get { return _chromiumWebBrowserburn; }
        }
        public DispatcherTimer timerBurn = new DispatcherTimer();

        int defaultPageBurn = 0;
        int counterBurn = 0;

        private void TimerBurn_Tick(object sender, EventArgs e)
        {
            // DispatcherTimer timer = sender as DispatcherTimer;
            string add = "";
            counterBurn++;
            try
            {
                //LogHelper.WriteWarn("Timer_Tick_chromiumWebBrowserburn.IsBrowserInitialized:---------"+chromiumWebBrowserburn.IsBrowserInitialized);
                if (chromiumWebBrowserburn.Visibility != Visibility.Visible || chromiumWebBrowserburn.IsInitialized != true || chromiumWebBrowserburn.IsBrowserInitialized != true || chromiumWebBrowserburn.GetMainFrame().Url == "")
                {
                    return;
                }
                Task<JavascriptResponse> task = this.chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync(PublicTalkMothed.add());
                if (task.Result.Success && task.Result.Result.ToString() == "2")
                {
                    timerBurn.Stop();
                    LogHelper.WriteWarn("TalkGroupViewModel_TimerBurn_Tick_LoadMsgData..................");
                    LoadMsgData(GlobalVariable.BurnFlag.IsBurn);
                    var cefBool = CefList.ContainsKey(this.GroupInfo.groupId);
                    if (!cefBool)
                    {
                        GroupCef groupCef = new GroupCef();
                        groupCef.CefBurn = _chromiumWebBrowserburn;
                        CefList.Add(this.GroupInfo.groupId, groupCef);
                    }
                    else
                    {
                        var list = CefList.SingleOrDefault(m => m.Key == this.GroupInfo.groupId).Value as GroupCef;
                        list.CefBurn = _chromiumWebBrowserburn;
                    }
                    counterBurn = 0;
                    add = task.Result.Result.ToString();
                    LogHelper.WriteDebug("TalkGroupViewModel_TimerBurn_Tick时间Success:" + DateTime.Now.ToLongTimeString());

                    #region 未读消息解析
                    #region old
                    #endregion
                    _burnUnreadCount = 0;
                    if (ChatMsgLst?.Count > 0)
                    {
                        var index = ChatMsgLst[0].chatIndex;
                        chromiumWebBrowserburn.scrollChatIndex = !string.IsNullOrEmpty(index) ? index : "0";
                        ShowMsgData();
                    }
                    else
                    {
                        string checkId = "";
                        #region 默认显示一页消息逻辑
                        if (defaultPageBurn == 0)
                        {
                            if (_burnUnreadCount == 0)
                            {
                                if (FirstPageDataBurn?.Count() > 0)
                                {
                                    StringBuilder sbLeft = new StringBuilder();
                                    string topStr = PublicTalkMothed.topString();
                                    sbLeft.AppendLine(topStr);
                                    foreach (var list in FirstPageDataBurn)
                                    {
                                        checkId = list.messageId;
                                        lock (obj)
                                        {
                                            #region 2014-06-14修改 New

                                            BurnListChatIndex.Add(list.messageId);
                                            switch (list.MsgType)
                                            {
                                                case AntSdkMsgType.ChatMsgText:
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, list);
                                                    }
                                                    break;
                                                case AntSdkMsgType.ChatMsgPicture:
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendImage sendImage = new OnceSendImage();
                                                        sendImage.GroupInfo = this.GroupInfo;
                                                        sendImage.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendImage);
                                                    }
                                                    list.chatIndex = list.messageId;
                                                    SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:msg.content*/list.sourceContent);
                                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn, list.messageId, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                                    break;
                                                case AntSdkMsgType.ChatMsgFile:
                                                    if (list.sendsucessorfail == 0)
                                                    {
                                                        list.chatType = (int)AntSdkchatType.Group;
                                                        OnceSendFile sendFile = new OnceSendFile();
                                                        sendFile.GroupInfo = this.GroupInfo;
                                                        sendFile.ctt = this.s_ctt;
                                                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(list.messageId, sendFile);
                                                    }
                                                    break;
                                            }
                                            string oneStr = PublicTalkMothed.LeftGroupToGroupShowBurnMessage(list, GroupMembers);
                                            sbLeft.AppendLine(oneStr);

                                            #endregion
                                        }
                                    }
                                    string endStr = PublicTalkMothed.endString();
                                    sbLeft.AppendLine(endStr);
                                    bool IsScuessBurn = false;
                                    Task<JavascriptResponse> result = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
                                    result.Wait();
                                    if (result.Result.Success)
                                    {
                                        var isExist = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                                        isExist.Wait();
                                        if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                                        {
                                            IsScuessBurn = false;
                                        }
                                        else
                                        {
                                            IsScuessBurn = true;
                                        }
                                    }
                                    while (IsScuessBurn == false)
                                    {
                                        Task<JavascriptResponse> resultBurnWhile = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync(sbLeft.ToString());
                                        resultBurnWhile.Wait();
                                        if (resultBurnWhile.Result.Success)
                                        {
                                            var isExist = _chromiumWebBrowserburn.GetMainFrame().EvaluateScriptAsync("isExistId('" + checkId + "');");
                                            isExist.Wait();
                                            if ((bool)isExist.Result.Result == false || isExist.Result.Result == null)
                                            {
                                                IsScuessBurn = false;
                                            }
                                            else
                                            {
                                                IsScuessBurn = true;
                                            }
                                        }
                                    }
                                    #region 2017-12-03 屏蔽
                                    //Task<JavascriptResponse> result = _chromiumWebBrowserburn.EvaluateScriptAsync(sbLeft.ToString());
                                    //result.Wait();
                                    //if (!result.Result.Success)
                                    //{
                                    //    Thread.Sleep(50);
                                    //    LogHelper.WriteWarn("[TalkGroupViewModel-FirstPageDataBurn:]" + sbLeft.ToString());
                                    //    Task<JavascriptResponse> results2 = _chromiumWebBrowserburn.EvaluateScriptAsync(sbLeft.ToString());
                                    //    results2.Wait();
                                    //    if (results2.Result.Success)
                                    //    {
                                    //        LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-FirstPageDataBurn:]第二次执行成功---------------------");
                                    //    }
                                    //    else
                                    //    {
                                    //        LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-FirstPageDataBurn:]第二次执行失败---------------------");
                                    //        Task<JavascriptResponse> results3 = _chromiumWebBrowserburn.EvaluateScriptAsync(sbLeft.ToString());
                                    //        results3.Wait();
                                    //        Thread.Sleep(100);
                                    //        if (results3.Result.Success)
                                    //        {
                                    //            LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-FirstPageDataBurn:]第三次次执行成功---------------------");
                                    //        }
                                    //        else
                                    //        {
                                    //            LogHelper.WriteWarn("---------------------------[TalkGroupViewModel-FirstPageDataBurn:]第三次执行失败---------------------");
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                    defaultPageBurn = 1;
                                    FirstPageDataBurn = null;
                                    result.Dispose();
                                    _chromiumWebBrowserburn.EvaluateScriptAsync("setscross();");
                                }
                            }
                        }

                        #endregion

                        if (_richTextBox != null)
                        {
                            _richTextBox.IsReadOnly = false;
                            _richTextBox.Focus();
                        }
                    }
                    //注册在线消息接收事件
                    if (_windowHelper == null)
                    {
                        _windowHelper = new WindowHelper(this.s_ctt.sessionId);
                        _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived += LocalMessageHelper_InstantMessageHasBeenReceived;
                        if (_isChanageWindow)
                            WindowMonitor.ChanageWindowHelper(_windowHelper);
                    }
                    #endregion
                }
                if (counterBurn == 10)
                {
                    if (add != "2")
                    {
                        timerBurn.Stop();
                        counter = 0;
                    }
                    LogHelper.WriteDebug("TalkGroupViewModel_Timer_Tick超时:" + DateTime.Now.ToLongTimeString());
                }
            }
            catch (Exception ex)
            {
                if (_windowHelper == null)
                {
                    _windowHelper = new WindowHelper(this.s_ctt.sessionId);
                    _windowHelper.LocalMessageHelper.InstantMessageHasBeenReceived += LocalMessageHelper_InstantMessageHasBeenReceived;
                    if (_isChanageWindow)
                        WindowMonitor.ChanageWindowHelper(_windowHelper);
                }
                //timer.Stop();
                //counter = 0;
                LogHelper.WriteError("TalkGroupViewModel_Timer_Tick错误:" + ex.Source + ex.Message + ex.StackTrace);
            }
        }

        bool IsFirst = true;

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
                        LogHelper.WriteError("[TalkGroupViewModel_btnShowPopupCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }

        #endregion

        //BackgroundWorker back = null;

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
                        #region 修改前

                        //System.Windows.Controls.Button btn = obj as System.Windows.Controls.Button;

                        //CaptureImageTool CapImg = new CaptureImageTool();

                        //if (CapImg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        //{
                        //    string imgName = Guid.NewGuid().ToString();
                        //    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "localData\\" + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\group\\cutImg"))
                        //    {
                        //        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "localData\\" + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\group\\cutImg");
                        //    }
                        //    string path = AppDomain.CurrentDomain.BaseDirectory + "localData\\" + s_ctt.companyCode + "\\" + s_ctt.sendUserId + "\\group\\cutImg\\" + imgName;
                        //    SaveImg(CapImg.Image, path);

                        //    CapImg.Dispose();

                        //    SendCutImageDto scid = new SendCutImageDto();
                        //    scid.cmpcd = s_ctt.companyCode;
                        //    scid.seId = s_ctt.sessionId;
                        //    scid.file = path + ".Jpeg";
                        //    scid.fileFileName = imgName;

                        //    System.Drawing.Bitmap pic = new System.Drawing.Bitmap(scid.file);

                        //    scid.imageWidth = pic.Width.ToString();
                        //    scid.imageHeight = pic.Height.ToString();

                        //    pic.Dispose();

                        //    back = new BackgroundWorker();

                        //    back.RunWorkerCompleted += Back_RunWorkerCompleted;
                        //    back.DoWork += Back_DoWork;
                        //    back.RunWorkerAsync(scid);

                        //    #region 构造时间差
                        //    if (lastShowTime == "")
                        //    {
                        //        preTime = DateTime.Now.ToString();
                        //        lastShowTime = preTime;
                        //        firstTime = preTime;
                        //        this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime());
                        //        Thread.Sleep(50);
                        //    }
                        //    else
                        //    {
                        //        DateTime dt = DateTime.Now;
                        //        if (PublicTalkMothed.showTimeSend(lastShowTime, preTime, dt))
                        //        {
                        //            this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime());
                        //            Thread.Sleep(50);
                        //        }
                        //        preTime = DateTime.Now.ToString();
                        //    }
                        //    #endregion

                        //    //显示图片
                        //    #region 圆形图片Right
                        //    StringBuilder sbRight = new StringBuilder();
                        //    sbRight.AppendLine("function myFunction()");

                        //    sbRight.AppendLine("{ var first=document.createElement('div');");
                        //    sbRight.AppendLine("first.className='rightd';");

                        //    sbRight.AppendLine("var second=document.createElement('div');");
                        //    sbRight.AppendLine("second.className='rightimg';");

                        //    string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                        //    if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/头像-个人资料.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                        //    {
                        //        imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/头像-个人资料.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //    }

                        //    sbRight.AppendLine("var img = document.createElement('img');");
                        //    sbRight.AppendLine("img.src='" + imgUrl + "';");


                        //    sbRight.AppendLine("img.className='divcss5';");
                        //    sbRight.AppendLine("second.appendChild(img);");
                        //    sbRight.AppendLine("first.appendChild(second);");

                        //    sbRight.AppendLine("var three=document.createElement('div');");
                        //    sbRight.AppendLine("three.className='speech right';");

                        //    sbRight.AppendLine("var img1 = document.createElement('img');");

                        //    string imgLocalPath = "file:///" + scid.file.Replace(@"\", @"/");
                        //    sbRight.AppendLine("img1.src='" + imgLocalPath + "';");


                        //    //if (Convert.ToInt32(scid.imageWidth) > 300)
                        //    //{
                        //    //    sbRight.AppendLine("img1.style.width='300px';");
                        //    //}
                        //    //sbRight.AppendLine("img1.style.height='" + scid.imageHeight + "px';");
                        //    sbRight.AppendLine("img1.style.width='100%';");
                        //    sbRight.AppendLine("img1.style.height='100%';");

                        //    sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                        //    sbRight.AppendLine("img1.title='双击查看原图';");
                        //    sbRight.AppendLine("three.appendChild(img1);");

                        //    sbRight.AppendLine("first.appendChild(three);");
                        //    sbRight.AppendLine("document.body.appendChild(first);");

                        //    sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                        //    sbRight.AppendLine("}");

                        //    sbRight.AppendLine("myFunction();");

                        //    var task = this.cwm.EvaluateScriptAsync(sbRight.ToString());

                        //    StringBuilder sbEnds = new StringBuilder();
                        //    sbEnds.AppendLine("setscross();");
                        //    this.cwm.EvaluateScriptAsync(sbEnds.ToString());

                        //    #endregion

                        //} 

                        #endregion

                        #region 修改后

                        msgEditAssistant.CutImage();

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkGroupViewModel_btnCutImageCommand]:" + ex.Message + ex.StackTrace + ex.Source);
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
                    if (IsIncognitoModelState)
                    {
                        chromiumWebBrowserburn.ExecuteScriptAsync("setscross();");
                    }
                    else
                    {
                        chromiumWebBrowser.EvaluateScriptAsync("setscross();");
                    }

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
                //var fileOutput = AntSdkService.FileUpload(sendmsg, ref strError);
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
                    rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
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
                    rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
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
                rcidReturn.isOnceSendMsg = sendmsg.isOnceSendMsg;
                e.Result = rcidReturn;
                LogHelper.WriteError("[TalkGroupViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }

        private void Back_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var rcidReturn = e.Result as ReturnCutImageDto;
                if (rcidReturn.isState.ToString() == "0")
                {
                    // System.Windows.Forms.MessageBox.Show("上传失败!");
                    if (IsIncognitoModelState)
                    {
                        rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                        updateFailMessage(rcidReturn.FailOrSucess);
                        PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                        PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imagedTipId);
                    }
                    else
                    {
                        rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                        updateFailMessage(rcidReturn.FailOrSucess);
                        PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                        PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                    }
                }
                else
                {
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
                    //smt.messageId = Guid.NewGuid().ToString();
                    //2017-04-06 屏蔽
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.messageId = rcidReturn.messageId;
                    OnceSendImage sendImage = null;
                    if (rcidReturn.isOnceSendMsg)
                    {
                        sendImage = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == rcidReturn.messageId).Value as OnceSendImage;
                        smt.sendUserId = sendImage.ctt.sendUserId;
                        smt.sessionId = sendImage.ctt.sessionId;
                        smt.targetId = sendImage.GroupInfo.groupId;
                    }
                    else
                    {
                        smt.sendUserId = s_ctt.sendUserId;
                        smt.sessionId = s_ctt.sessionId;
                        smt.targetId = GroupInfo.groupId;
                    }
                    smt.flag = IsIncognitoModelState ? 1 : 0;
                    smg.ctt = smt;


                    string imgError = "";
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.Picture imgMsg = new AntSdkChatMsg.Picture();
                    AntSdkChatMsg.Picture_content pictureContent = new AntSdkChatMsg.Picture_content();
                    pictureContent.picUrl = rcidReturn.fileUrl;
                    imgMsg.content = pictureContent;
                    imgMsg.MsgType = AntSdkMsgType.ChatMsgPicture;
                    imgMsg.flag = IsIncognitoModelState ? 1 : 0;
                    if (rcidReturn.isOnceSendMsg)
                    {
                        imgMsg.sendUserId = sendImage.ctt.sendUserId;
                        imgMsg.sessionId = sendImage.ctt.sessionId;
                        imgMsg.targetId = sendImage.GroupInfo.groupId;
                    }
                    else
                    {
                        imgMsg.sendUserId = s_ctt.sendUserId;
                        imgMsg.sessionId = s_ctt.sessionId;
                        imgMsg.targetId = GroupInfo.groupId;
                    }
                    imgMsg.messageId = rcidReturn.messageId;
                    imgMsg.chatType = (int)AntSdkchatType.Group;
                    SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(rcidReturn.FailOrSucess.obj as MessageStateArg));
                    var result = false;
                    var isResult = false;
                    if (rcidReturn.isOnceSendMsg == false)
                    {
                        #region 发送成功 插入数据 2017-09-03 屏蔽  插入方法提前

                        //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                        ////TODO:AntSdk_Modify
                        ////sm_ctt.MTP = "2";
                        //sm_ctt.sourceContent = smt.content;
                        //sm_ctt.chatIndex = rcidReturn.preChatIndex.ToString();
                        //sm_ctt.sendsucessorfail = 0;
                        //sm_ctt.SENDORRECEIVE = "1";
                        //sm_ctt.uploadOrDownPath = rcidReturn.prePath;
                        //if (IsIncognitoModelState)
                        //{
                        //    result = ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
                        //}
                        //else
                        //{
                        //    result = ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                        //}
                        //if (result)
                        //{
                        result = AntSdkService.SdkPublishChatMsg(imgMsg, ref imgError);
                        //}

                        #endregion
                    }
                    else
                    {
                        result = AntSdkService.SdkRePublishChatMsg(imgMsg, ref imgError);
                    }

                    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref imgError);
                    if (result)
                    {
                        if (IsIncognitoModelState)
                        {
                            ////更新状态
                            //T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                            //t_chat.UpdateSendMsgState(rcidReturn.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                            //updateFailMessage(rcidReturn.FailOrSucess);
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.yesBurn, PointOrGroupFrom.PointOrGroup.Group, rcidReturn.messageId, imgJson);
                        }
                        else
                        {
                            ////更新状态
                            //T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                            //t_chat.UpdateSendMsgState(rcidReturn.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                            //updateFailMessage(rcidReturn.FailOrSucess);
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.notBurn, PointOrGroupFrom.PointOrGroup.Group, rcidReturn.messageId, imgJson);
                        }
                    }
                    else
                    {
                        if (IsIncognitoModelState)
                        {
                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            //updateFailMessage(rcidReturn.FailOrSucess);
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                            //PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imagedTipId);
                        }
                        else
                        {
                            //rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            //updateFailMessage(rcidReturn.FailOrSucess);
                            //PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                            //PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                        }
                    }
                    //System.Windows.Forms.MessageBox.Show("上传成功!" + rcidReturn.imgSize);
                    //back.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_Back_RunWorkerCompleted]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        #endregion
        /// <summary>
        /// 截图
        /// </summary>
        public void TalkViewCutImage()
        {
            msgEditAssistant.CutImage();
        }
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
                        SoundHelper.StartRecord(true);
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
        private ChromiumWebBrowser Cef;
        /// <summary>
        /// 发送语音方法
        /// filePath==Null 则表示为消息上传失败，需要重新上传再发送
        /// 否则为第一次发送
        /// </summary>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="sendDto">语音文件构造</param>
        private void SendSound(string filePath, SendCutImageDto sendDto)
        {
            var isSendAgain = string.IsNullOrEmpty(filePath);
            if (isSendAgain)
                Cef = sendDto.Cef;
            else
                Cef = null;
            AudioBack?.CancelAsync();
            var maxChatindex = 0;
            if (IsIncognitoModelState)
            {
                //查询数据库最大chatindex
                var t_chat = new T_Chat_Message_GroupBurnDAL();
                maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
            }
            else
            {
                //查询数据库最大chatindex
                var t_chat = new T_Chat_Message_GroupDAL();
                maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
            }
            var file = isSendAgain ? sendDto.prePaths : filePath;
            var duration = int.Parse(SoundHelper.GetMp3Time(file));
            duration = duration > 60 ? 60 : duration;
            var fileInput = new AntSdkSendFileInput
            {
                cmpcd = s_ctt.companyCode,
                messageId = isSendAgain ? sendDto.messageId : PublicTalkMothed.timeStampAndRandom(),
                file = file,
                preChatIndex = maxChatindex,
                filesize = duration.ToString(),
                seId = isSendAgain ? sendDto.Dto.sessionId : s_ctt.sessionId,
                fileFileName = Path.GetFileNameWithoutExtension(file),
                targetId = isSendAgain ? sendDto.Dto.targetId : s_ctt.targetId,
                sendUserId = s_ctt.sendUserId,
                isOnceSend = isSendAgain
            };
            var failMessage = new AntSdkFailOrSucessMessageDto
            {
                mtp = (int)AntSdkMsgType.ChatMsgAudio,
                content = "[语音]",
                preChatIndex = maxChatindex,
                sessionid = isSendAgain ? sendDto.Dto.sessionId : s_ctt.sessionId,
                lastDatetime = DateTime.Now.ToString(),
                IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending,
                IsBurnMsg = IsIncognitoModelState ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn
            };
            fileInput.FailOrSucess = failMessage;
            updateFailMessage(fileInput.FailOrSucess);
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
                audioMsg.flag = IsIncognitoModelState ? 1 : 0;
                audioMsg.sendUserId = s_ctt.sendUserId;
                audioMsg.sessionId = s_ctt.sessionId;
                audioMsg.targetId = s_ctt.targetId;
                audioMsg.chatType = (int)AntSdkchatType.Group;
                AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(audioMsg));
                sm_ctt.sourceContent = JsonConvert.SerializeObject(audioContent);
                sm_ctt.chatIndex = maxChatindex.ToString();
                sm_ctt.sendsucessorfail = 0;
                sm_ctt.SENDORRECEIVE = "1";
                sm_ctt.uploadOrDownPath = file;
                sm_ctt.flag = audioMsg.flag;
                if (IsIncognitoModelState)
                {
                    ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                }
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
            if (AudioBack.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            var rcidReturn = new ReturnCutImageDto();
            var sendmsg = e.Argument as AntSdkSendFileInput;
            if (sendmsg == null)
                return;
            try
            {
                var duration = Convert.ToInt32(sendmsg.filesize); //音频时长 
                rcidReturn.FailOrSucess = sendmsg.FailOrSucess;
                rcidReturn.prePath = sendmsg.file;
                rcidReturn.messageId = sendmsg.messageId;
                rcidReturn.imagedTipId = sendmsg.imgeTipId;
                rcidReturn.imageSendingId = sendmsg.imageSendingId;
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                rcidReturn.imagedTipId = imageTipId;
                rcidReturn.imageSendingId = "sending" + imageTipId;
                rcidReturn.targetId = sendmsg.targetId;
                rcidReturn.sendUserId = sendmsg.sendUserId;
                rcidReturn.sessionid = sendmsg.seId;
                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(rcidReturn.messageId))
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId[rcidReturn.messageId] = rcidReturn.imageSendingId;
                }
                else
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(rcidReturn.messageId, rcidReturn.imageSendingId);
                }

                #region 消息状态监控
                var arg = new MessageStateArg
                {
                    isBurn = IsIncognitoModelState ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn,
                    isGroup = true,
                    MessageId = rcidReturn.messageId,
                    SessionId = rcidReturn.sessionid,
                    SendIngId = rcidReturn.imageSendingId,
                    RepeatId = rcidReturn.imagedTipId
                };

                if (Cef != null)
                {
                    arg.WebBrowser = Cef;
                }
                else
                {
                    arg.WebBrowser = IsIncognitoModelState ? chromiumWebBrowserburn : chromiumWebBrowser;
                }
                var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == rcidReturn.messageId);
                if (IsHave != null)
                {
                    SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                }
                SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                #endregion
                if (Cef == null)
                {
                    var dt = DateTime.Now;
                    PublicTalkMothed.RightGroupShowSendVoice(IsIncognitoModelState ? chromiumWebBrowserburn : chromiumWebBrowser, dt, rcidReturn, duration);
                }
                else
                {
                    var dt = DateTime.Now;
                    PublicTalkMothed.RightGroupShowSendVoice(Cef, dt, rcidReturn, duration);
                }

                #region 滚动条置底

                var sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                if (Cef == null)
                {
                    if (IsIncognitoModelState)
                    {
                        var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                    }
                    else
                    {
                        var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }
                }
                else
                {
                    Cef.EvaluateScriptAsync(sbEnd.ToString());
                }
                #endregion

                var errorCode = 0;
                string strError = string.Empty;
                var fileOutput = AntSdkService.FileUpload(sendmsg, ref errorCode, ref strError);
                if (fileOutput == null) return;
                rcidReturn.isState = "1";
                rcidReturn.fileUrl = fileOutput.dowmnloadUrl;
                rcidReturn.imgSize = fileOutput.fileSize;
                e.Result = rcidReturn;
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
                if (rcidReturn == null)
                {
                    return;
                }
                if (rcidReturn.isState == "0") //失败
                {
                    if (IsIncognitoModelState)
                    {
                        rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                        updateFailMessage(rcidReturn.FailOrSucess);
                        PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                        PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imagedTipId);
                    }
                    else
                    {
                        rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                        updateFailMessage(rcidReturn.FailOrSucess);
                        PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                        PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                    }
                }
                else //成功
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
                    audioMsg.flag = IsIncognitoModelState ? 1 : 0;
                    audioMsg.sendUserId = rcidReturn.sendUserId;
                    audioMsg.sessionId = rcidReturn.sessionid;
                    audioMsg.targetId = rcidReturn.targetId;
                    audioMsg.chatType = (int)AntSdkchatType.Group;

                    #region 异步发送方法

                    ThreadPool.QueueUserWorkItem(m => SendVoiceMethod(audioMsg, rcidReturn));

                    #endregion

                    AudioBack.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_Back_RunWorkerCompleted]:" + ex.Message + ex.StackTrace + ex.Source);
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
                var sourceContent = JsonConvert.SerializeObject(audioMsg.content);
                if (IsIncognitoModelState)
                {
                    //更新状态                
                    var t_chat = new T_Chat_Message_GroupBurnDAL();
                    t_chat.UpdateSendVoiceMsgState(rcidReturn.messageId, sourceContent, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                    updateFailMessage(rcidReturn.FailOrSucess);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imagedTipId);
                }
                else
                {
                    //更新状态
                    var t_chat = new T_Chat_Message_GroupDAL();
                    t_chat.UpdateSendVoiceMsgState(rcidReturn.messageId, sourceContent, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                    updateFailMessage(rcidReturn.FailOrSucess);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                }
            }
            else
            {
                if (IsIncognitoModelState)
                {
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                    updateFailMessage(rcidReturn.FailOrSucess);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowserburn, rcidReturn.imagedTipId);
                }
                else
                {
                    rcidReturn.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                    updateFailMessage(rcidReturn.FailOrSucess);
                    PublicTalkMothed.HiddenMsgDiv(this.chromiumWebBrowser, rcidReturn.imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, rcidReturn.imagedTipId);
                }
            }
        }
        #endregion

        #region 发送消息

        public ICommand btnSendMsgCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (!AntSdkService.AntSdkIsConnected) //重连成功
                    {
                        showTextMethod("网络连接已断开，不能发送消息！");
                        //hOffset = -135;
                        return;
                    }

                    var counts = _richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                    if (counts.Count() == 0)
                    {
                        //hOffset = -22;
                        showTextMethod("发送消息不能为空，请重新输入！");
                        this._richTextBox.Focus();
                        return;
                    }
                    else
                    {
                        sendMsg(null);
                        this._richTextBox.Focus();
                    }
                });
            }
        }

        /// <summary>
        /// @是否已读事件
        /// </summary>
        public static event Action<string> OnAtMsgChanged;

        /// <summary>
        /// 滚动位置
        /// </summary>
        public ICommand btnScrollPosition
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                   {
                       Task task = chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.scrollPosition(scrollPostion));
                       task.Wait();
                       AntSdkTsession tSession = t_sessionBll.GetModelByKey(this.s_ctt.sessionId);
                       tSession.LastMsg = tSession.LastMsg.Replace("[~!@]", "");
                       t_sessionBll.Update(tSession);
                       if (OnAtMsgChanged != null)
                           OnAtMsgChanged(this.s_ctt.sessionId);
                       IsbtnTipShow = "Hidden";
                   }));
                });
            }
        }

        DateTime timeSplit = new DateTime();
        DispatcherTimer IsShowTip = new DispatcherTimer();
        int typeString = 0;
        int typeImage = 0;
        int typeAt = 0;

        #region sendMsg

        private void sendMsg(KeyEventArgs e)
        {
            string listShow = "";
            typeString = 0;
            typeImage = 0;
            typeAt = 0;
            List<MixMessageObjDto> mixMsg = new List<MixMessageObjDto>();
            List<PictureAndTextMixDto> picAndTxtMix = new List<PictureAndTextMixDto>();
            picAndTxtMix.Clear();
            List<BatchImage> listImage = new List<BatchImage>();

            List<AntSdkChatMsg.At_content> liststr = new List<AntSdkChatMsg.At_content>();
            List<object> sendContent = new List<object>();
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

                var blockCount = this._richTextBox.Document.Blocks.Count(m => m != null);
                var counts = this._richTextBox.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
                string msgStr = "";
                string SendStr = "";
                int addCount = 0;

                foreach (var ss in counts)
                {
                    int lineCount = 0;
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
                                SendStr += tt.Text.Replace(" ", "&#160;").Replace("<", "&lt;").Replace("'", "&#39;").Replace(" ", "&nbsp;").Replace(@"\", @"\\");
                                if (contents.Length > 0)
                                {
                                    AntSdkChatMsg.contentText text = new AntSdkChatMsg.contentText();
                                    text.type = AntSdkAtMsgType.Text;
                                    text.content = tt.Text;
                                    sendContent.Add(text);
                                    //AntSdkChatMsg.At_content atIds = new AntSdkChatMsg.At_content();
                                    //atIds.content = tt.Text;
                                    //liststr.Add(atIds);
                                }
                                MixMessageObjDto mixtext = new MixMessageObjDto();
                                mixtext.type = "1001";
                                mixtext.content = contents;
                                mixMsg.Add(mixtext);

                                picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Text, contents));
                                //SendStr += tt.Text.Replace(" ", "&#160;").Replace("<", "&lt;").Replace(@"""", "&quot;&quot;").Replace("'", "&#39");
                                listShow += tt.Text;
                            }
                        }
                        else if (ite is LineBreak)
                        {
                            msgStr += "\r\n";
                            SendStr += "<br/>";
                            AntSdkChatMsg.contentNewLine newLine = new AntSdkChatMsg.contentNewLine();
                            newLine.type = AntSdkAtMsgType.Enter;
                            sendContent.Add(newLine);

                            MixMessageObjDto mixtext = new MixMessageObjDto();
                            mixtext.type = "0000";
                            mixtext.content = "";
                            mixMsg.Add(mixtext);
                            picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.LineBreak));
                        }

                        InlineUIContainer iu = ite as InlineUIContainer;
                        if (iu != null)
                        {
                            var tb = iu.Child as TextBlock;
                            if (tb != null)
                            {
                                var atPerson = tb.Text;
                                var userId = "";
                                //AntSdkChatMsg.At_content atIdsName = new AntSdkChatMsg.At_content();

                                if (atPerson.Substring(1, atPerson.Length - 1) != "全体成员")
                                {
                                    //AntSdkChatMsg.contentText atText = new AntSdkChatMsg.contentText();
                                    //atText.type = "1008";

                                    AntSdkChatMsg.contentAtOrdinary atOrdinary = new AntSdkChatMsg.contentAtOrdinary();
                                    atOrdinary.type = "1112";
                                    userId = tb.Tag.ToString();
                                    //TODO:AntSdk_Modify
                                    atOrdinary.ids = new List<string>();
                                    atOrdinary.ids.Add(userId);
                                    atOrdinary.names = new List<string>();
                                    atOrdinary.names.Add(atPerson.Substring(1, atPerson.Length - 1));

                                    //sendContent.Add(atText);

                                    MixMessageObjDto mixtext = new MixMessageObjDto();
                                    mixtext.type = "1008";
                                    mixtext.content = atOrdinary;
                                    mixMsg.Add(mixtext);


                                    picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(PictureAndTextMixEnum.AtPerson, "", "", "", "", atOrdinary));
                                    listShow += atPerson;
                                }
                                else
                                {


                                    AntSdkChatMsg.contentAtOrdinary atOrdinary = new AntSdkChatMsg.contentAtOrdinary();
                                    atOrdinary.type = "1111";

                                    atOrdinary.ids = new List<string>();
                                    atOrdinary.ids.Add("8888888888");
                                    atOrdinary.names = new List<string>();
                                    atOrdinary.names.Add("全体成员");
                                    /// atText.content = JsonConvert.SerializeObject(atOrdinary);

                                    MixMessageObjDto mixtext = new MixMessageObjDto();
                                    mixtext.type = "1008";
                                    mixtext.content = atOrdinary;
                                    mixMsg.Add(mixtext);


                                    AntSdkChatMsg.contentAtAll atAll = new AntSdkChatMsg.contentAtAll();

                                    userId = tb.Tag.ToString();
                                    //TODO:AntSdk_Modify
                                    //atIdsName.names = "全体成员";
                                    atAll.type = AntSdkAtMsgType.AtAll;
                                    //atAll
                                    sendContent.Add(atAll);
                                    picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(PictureAndTextMixEnum.AtAll, "", "", "", "", atAll));
                                    listShow += "@全体成员";
                                }

                                //atIdsName.id = userId;

                                //liststr.Add(atIdsName);

                                msgStr += atPerson;
                                SendStr += atPerson;
                                typeAt = 1;
                            }
                            var oo = iu.Child as Image;
                            if (oo != null)
                            {
                                if (oo.Tag != null && oo.Tag.ToString() == "cut")
                                {
                                    typeImage = 1;
                                    BatchImage batchImage = new BatchImage();
                                    batchImage.image = oo;
                                    listImage.Add(batchImage);
                                    string path = oo.Source.ToString().Substring(8, oo.Source.ToString().Length - 8);
                                    string guid = "b" + Guid.NewGuid().ToString();
                                    string imgId = "i" + Guid.NewGuid().ToString();
                                    picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Image, oo.Source.ToString(), guid, path, imgId));

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
                                    var img = new Image { Source = oo.Source, Width = oo.Width, Height = oo.Height, Stretch = Stretch.Fill };

                                    InlineUIContainer iuImage = new InlineUIContainer();
                                    iuImage.Child = img;

                                    var imgUrls = oo.Source;

                                    //表情构造

                                    string imgSubStr = "[" + imgUrls.ToString().Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) + "]";
                                    msgStr += imgSubStr;
                                    //网络Emoji
                                    //string nn = "http://192.168.10.229:8080/scsf/visitor/images/emotions/" + imgUrls.ToString().Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) + ".png";
                                    //本地Emoji
                                    string nn = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Emoji/" + imgUrls.ToString().Substring(imgUrls.ToString().LastIndexOf("/") + 1, 4) + ".png").Replace(@"\", @"/").Replace(" ", "%20");
                                    SendStr += "<img src=" + nn + "></img>";
                                    picAndTxtMix.Add(PictureAndTextMixMethod.PictureAndTextStruct(ModelEnum.PictureAndTextMixEnum.Text, imgSubStr));
                                    AntSdkChatMsg.contentText text = new AntSdkChatMsg.contentText();
                                    text.type = AntSdkAtMsgType.Text;
                                    text.content = imgSubStr;
                                    sendContent.Add(text);


                                    MixMessageObjDto mixtext = new MixMessageObjDto();
                                    mixtext.type = "1001";

                                    mixtext.content = imgSubStr;
                                    mixMsg.Add(mixtext);
                                    listShow += imgSubStr;
                                }
                            }
                        }
                    }
                    if (addCount < counts.Count())
                    {
                        msgStr += "\r\n";
                        SendStr += "<br/>";
                        AntSdkChatMsg.contentNewLine newLine = new AntSdkChatMsg.contentNewLine();
                        newLine.type = AntSdkAtMsgType.Enter;
                        sendContent.Add(newLine);
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
                //        //if (IsShowTip.IsEnabled)
                //        //{
                //        //    isShowPopup = false;
                //        //}
                //        //else
                //        //{
                //        //    IsShowTip.Interval = TimeSpan.FromMilliseconds(1000);
                //        //    IsShowTip.Tick += IsShowTip_Tick; ;
                //        //    IsShowTip.Start();
                //        //    isShowPopup = true;
                //        //}
                //        hOffset = -65;
                //        showTextMethod("暂不支持图文混合发送");
                //        e.Handled = true;
                //    }
                //    else
                //    {
                //        //if (IsShowTip.IsEnabled)
                //        //{
                //        //    isShowPopup = false;
                //        //}
                //        //else
                //        //{
                //        //    IsShowTip.Interval = TimeSpan.FromMilliseconds(1000);
                //        //    IsShowTip.Tick += IsShowTip_Tick;
                //        //    IsShowTip.Start();
                //        //    isShowPopup = true;
                //        //}
                //        hOffset = -65;
                //        showTextMethod("暂不支持图文混合发送");
                //    }
                //    return;
                //}
                #endregion

                if (msgStr.Trim() == "" && listImage.Count() == 0)
                {
                    this._richTextBox.Document.Blocks.Clear();
                    if (e != null)
                    {
                        showTextMethod("发送消息不能为空，请重新输入！");
                        e.Handled = true;
                    }
                }
                else
                {
                    if (PublicTalkMothed.textLength(_richTextBox))
                    {
                        //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showTips());
                        showTextMethod("发送消息内容超长，请分条发送。");
                        if (e != null)
                        {
                            e.Handled = true;
                        }
                        //this.chromiumWebBrowser.EvaluateScriptAsync("setscross();");

                        return;
                    }
                    if (timeSplit.Year == 1)
                    {
                        timeSplit = DateTime.Now;
                    }
                    else
                    {
                        double sd = DateTime.Now.Subtract(timeSplit).TotalSeconds;
                        if (DateTime.Now.Subtract(timeSplit).TotalMilliseconds < 500)
                        {
                            showTextMethod("客官，你发消息太快了，悠着点奥！^_^");
                            hOffset = -176;
                            if (e != null)
                            {
                                e.Handled = true;
                            }
                            //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.quickly());
                            //this.chromiumWebBrowser.EvaluateScriptAsync("setscross();");

                            return;
                        }
                    }
                    timeSplit = DateTime.Now;
                    //文本发送
                    if (typeString == 1 && typeImage == 0 && typeAt == 0)
                    {
                        #region 2017-03-09 构造时间差

                        //if (lastShowTime == "")
                        //{
                        //    preTime = DateTime.Now.ToString();
                        //    lastShowTime = preTime;
                        //    firstTime = preTime;
                        //    this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime());
                        //    Thread.Sleep(50);
                        //}
                        //else
                        //{
                        //    DateTime dt = DateTime.Now;
                        //    if (PublicTalkMothed.showTimeSend(lastShowTime, preTime, dt))
                        //    {
                        //        this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime());
                        //        Thread.Sleep(50);
                        //    }
                        //    preTime = DateTime.Now.ToString();
                        //}

                        #endregion

                        //查询数据库最大chatindex
                        int maxChatindex = 0;
                        if (IsIncognitoModelState)
                        {
                            T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                            maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                        }
                        else
                        {
                            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                            maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                        }


                        //发送中提示
                        AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                        failMessage.preChatIndex = maxChatindex;
                        failMessage.mtp = (int)AntSdkMsgType.ChatMsgText;
                        failMessage.content = msgStr.TrimEnd('\n').TrimEnd('\r');
                        failMessage.sessionid = s_ctt.sessionId;
                        DateTime dt = DateTime.Now;
                        failMessage.lastDatetime = dt.ToString();

                        string messageid = PublicTalkMothed.timeStampAndRandom();
                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                        string imageSendingId = "sending" + imageTipId;
                        if (IsIncognitoModelState)
                        {
                            failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                            if (!BurnListChatIndex.Contains(messageid))
                            {
                                BurnListChatIndex.Add(messageid);
                                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                                {
                                    SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                                }
                                else
                                {
                                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                                }
                                updateFailMessage(failMessage);
                                PublicTalkMothed.RightGroupBurnShowSendText(chromiumWebBrowserburn, SendStr, e, messageid, imageTipId, imageSendingId, dt, msgStr);
                                #region 滚动条置底

                                StringBuilder sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");
                                if (IsIncognitoModelState)
                                {
                                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                                }
                                else
                                {
                                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                }

                                #endregion
                                #region 消息状态监控

                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = true;
                                arg.MessageId = messageid;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowserburn;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
                                if (IsHave != null)
                                {
                                    SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                                #endregion

                                // 同步发送方法
                                //sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId, failMessage, e);

                                #region 异步发送方法

                                ThreadPool.QueueUserWorkItem(m => sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId, failMessage, e, false));



                                if (msgStr.Trim() != "")
                                {
                                    _richTextBox.Document.Blocks.Clear();
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                            if (!listChatIndex.Contains(messageid))
                            {
                                listChatIndex.Add(messageid);
                                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                                {
                                    SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                                }
                                else
                                {
                                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                                }
                                updateFailMessage(failMessage);
                                PublicTalkMothed.RightGroupShowSendText(chromiumWebBrowser, SendStr, e, messageid, imageTipId, imageSendingId, dt, msgStr);
                                #region 滚动条置底

                                StringBuilder sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");
                                if (IsIncognitoModelState)
                                {
                                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                                }
                                else
                                {
                                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                }

                                #endregion

                                #region 消息状态监控

                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = true;
                                arg.MessageId = messageid;
                                arg.SessionId = s_ctt.sessionId;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
                                if (IsHave != null)
                                {
                                    SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                }
                                SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                                #endregion


                                // 同步发送方法
                                //sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId,
                                //    failMessage, e);

                                #region 异步发送方法

                                ThreadPool.QueueUserWorkItem(m => sendTextMethod(msgStr.TrimEnd('\n').TrimEnd('\r'), messageid, imageTipId, imageSendingId, failMessage, e, false));



                                if (msgStr.Trim() != "")
                                {
                                    _richTextBox.Document.Blocks.Clear();
                                }

                                #endregion
                            }
                        }
                    }
                    //图片发送
                    else if ((typeString == 0 || string.IsNullOrEmpty(msgStr.Trim())) && typeImage == 1)
                    {
                        int maxChatindex = 0;
                        T_Chat_Message_GroupBurnDAL t_chatBurn = null;
                        T_Chat_Message_GroupDAL t_chat = null;
                        List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listsImg = new List<AntSdkChatMsg.ChatBase>();
                        if (IsIncognitoModelState)
                        {
                            //查询数据库最大chatindex
                            t_chatBurn = new T_Chat_Message_GroupBurnDAL();
                            maxChatindex = t_chatBurn.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                        }
                        else
                        {
                            //查询数据库最大chatindex
                            t_chat = new T_Chat_Message_GroupDAL();
                            maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                        }
                        //批量插入数据
                        foreach (var lists in listImage)
                        {
                            SendImageDto sid = new SendImageDto();
                            sid.picUrl = lists.image.Source.ToString();
                            sid.thumbnailUrl = null;
                            ;
                            sid.imgSize = null;
                            string imgJson = JsonConvert.SerializeObject(sid);

                            SendMessage_ctt smt = new SendMessage_ctt();

                            SendMessageDto smg = new SendMessageDto();
                            smt.MsgType = AntSdkMsgType.ChatMsgPicture;
                            smt.content = imgJson;
                            smt.companyCode = s_ctt.companyCode;
                            //smt.messageId = Guid.NewGuid().ToString();
                            //2017-04-06 屏蔽
                            //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                            lists.messageId = PublicTalkMothed.timeStampAndRandom();
                            smt.messageId = lists.messageId;
                            smt.sendUserId = s_ctt.sendUserId;
                            smt.sessionId = s_ctt.sessionId;
                            smt.targetId = GroupInfo.groupId;
                            smt.flag = IsIncognitoModelState ? 1 : 0;
                            smg.ctt = smt;


                            string imgError = "";
                            //TODO:AntSdk_Modify
                            //DONE:AntSdk_Modify
                            AntSdkChatMsg.Picture imgMsg = new AntSdkChatMsg.Picture();
                            AntSdkChatMsg.Picture_content pictureContent = new AntSdkChatMsg.Picture_content();
                            pictureContent.picUrl = lists.image.Source.ToString();
                            imgMsg.content = pictureContent;
                            imgMsg.MsgType = AntSdkMsgType.ChatMsgPicture;
                            imgMsg.flag = IsIncognitoModelState ? 1 : 0;
                            imgMsg.sendUserId = s_ctt.sendUserId;
                            imgMsg.sessionId = s_ctt.sessionId;
                            imgMsg.targetId = GroupInfo.groupId;
                            imgMsg.messageId = smt.messageId;
                            imgMsg.chatType = (int)AntSdkchatType.Group;

                            #region 发送成功 插入数据

                            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                            //TODO:AntSdk_Modify
                            //sm_ctt.MTP = "2";
                            sm_ctt.sourceContent = smt.content;
                            sm_ctt.chatIndex = maxChatindex.ToString();
                            sm_ctt.sendsucessorfail = 0;
                            sm_ctt.SENDORRECEIVE = "1";
                            sm_ctt.uploadOrDownPath = lists.image.Source.ToString();
                            listsImg.Add(sm_ctt);

                            #endregion

                            OnceSendImage sendImage = new OnceSendImage();
                            sendImage.ctt = this.s_ctt;
                            sendImage.GroupInfo = this.GroupInfo;
                            OnceSendMessage.GroupToGroup.OnceMsgList.Add(lists.messageId, sendImage);
                        }
                        var resultImg = false;
                        if (IsIncognitoModelState)
                        {
                            resultImg = t_chatBurn.InsertBig(listsImg);
                        }
                        else
                        {
                            resultImg = t_chat.InsertBig(listsImg);
                        }
                        if (resultImg)
                        {
                            foreach (var lists in listImage)
                            {
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.preChatIndex = maxChatindex;
                                failMessage.mtp = (int)AntSdkMsgType.ChatMsgPicture;
                                failMessage.content = "";
                                failMessage.sessionid = s_ctt.sessionId;
                                DateTime dt = DateTime.Now;
                                failMessage.lastDatetime = dt.ToString();

                                //2017-04-06 添加
                                string messageId = lists.messageId;
                                //2017-04-06添加
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string prePath = lists.image.Source.ToString().Replace(@"\", @"/");
                                string prePaths = prePath.Substring(8, prePath.Length - 8);
                                string fileFileName = System.IO.Path.GetFileNameWithoutExtension(prePaths);
                                string imageSendingId = "sending" + imageTipId;

                                if (IsIncognitoModelState)
                                {
                                    if (!BurnListChatIndex.Contains(messageId))
                                    {
                                        BurnListChatIndex.Add(messageId);
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn, messageId, prePaths, "wlc" + fileFileName, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;

                                        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                                        }
                                        updateFailMessage(failMessage);

                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                        arg.isGroup = true;
                                        arg.MessageId = messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowserburn;
                                        arg.SendIngId = imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }
                                        //SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                        failMessage.obj = arg;
                                        uploadImageSegment(prePaths, fileFileName, messageId, imageTipId, imageSendingId, failMessage);

                                        #endregion

                                        PublicTalkMothed.RightGroupBurnShowSendImage(chromiumWebBrowserburn, lists.image, e, _richTextBox, fileFileName, messageId, imageTipId, imageSendingId, dt);
                                    }
                                }
                                else
                                {
                                    if (!listChatIndex.Contains(messageId))
                                    {
                                        listChatIndex.Add(messageId);
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, messageId, prePaths, "wlc" + fileFileName, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;

                                        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                                        }
                                        updateFailMessage(failMessage);

                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                        arg.isGroup = true;
                                        arg.MessageId = messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowser;
                                        arg.SendIngId = imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }
                                        //SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                                        failMessage.obj = arg;
                                        uploadImageSegment(prePaths, fileFileName, messageId, imageTipId, imageSendingId, failMessage);

                                        #endregion

                                        PublicTalkMothed.RightGroupShowSendImage(chromiumWebBrowser, lists.image, e, _richTextBox, fileFileName, messageId, imageTipId, imageSendingId, dt);
                                    }
                                }
                            }
                        }
                    }
                    #region 屏蔽
                    ////@消息发送
                    //if ((typeString == 1 && typeAt == 1) && typeImage != 1)
                    //{
                    //    int maxChatindex = 0;

                    //    //查询数据库最大chatindex
                    //    T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                    //    maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);

                    //    //发送中提示
                    //    AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                    //    failMessage.preChatIndex = maxChatindex;
                    //    failMessage.mtp = (int)AntSdkMsgType.ChatMsgText;
                    //    failMessage.content = msgStr.TrimEnd('\n').TrimEnd('\r');
                    //    failMessage.sessionid = s_ctt.sessionId;
                    //    DateTime dt = DateTime.Now;
                    //    failMessage.lastDatetime = dt.ToString();

                    //    string messageid = PublicTalkMothed.timeStampAndRandom();
                    //    string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                    //    string imageSendingId = "sending" + imageTipId;

                    //    if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                    //    {
                    //        SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                    //    }
                    //    else
                    //    {
                    //        SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                    //    }
                    //    if (IsIncognitoModelState)
                    //    {
                    //        if (!listChatIndex.Contains(messageid))
                    //        {
                    //            listChatIndex.Add(messageid);
                    //            failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                    //            PublicTalkMothed.RightGroupBurnShowSendText(chromiumWebBrowserburn, SendStr, e, messageid, imageTipId, imageSendingId, dt, msgStr);
                    //            sendAtTextMethod(msgStr, messageid, imageTipId, imageSendingId, failMessage, e, liststr);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (!listChatIndex.Contains(messageid))
                    //        {
                    //            #region 插入数据

                    //            //smt.content = JsonConvert.SerializeObject(outCtt);
                    //            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = new SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase();
                    //            sm_ctt.MsgType = AntSdkMsgType.ChatMsgAt;
                    //            /*TODO:AntSdk_Modify:list.content*/
                    //            sm_ctt.sourceContent = JsonConvert.SerializeObject(sendContent);
                    //            //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                    //            sm_ctt.messageId = messageid;
                    //            sm_ctt.sendUserId = s_ctt.sendUserId;
                    //            sm_ctt.sessionId = s_ctt.sessionId;
                    //            sm_ctt.targetId = GroupInfo.groupId;
                    //            //sm_ctt.companyCode = s_ctt.companyCode;
                    //            //sm_ctt.MTP = ((int)GlobalVariable.MsgType.At).ToString();
                    //            sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
                    //            sm_ctt.sendsucessorfail = 0;
                    //            sm_ctt.SENDORRECEIVE = "1";
                    //            if (IsIncognitoModelState)
                    //            {
                    //                //添加阅后即焚数据到阅后即焚表
                    //                ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
                    //            }
                    //            else
                    //            {
                    //                ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                    //            }

                    //            #endregion

                    //            listChatIndex.Add(messageid);
                    //            failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                    //            string atMsgjson = JsonConvert.SerializeObject(sendContent);
                    //            string base64Str = PublicTalkMothed.ConvertToBase64(atMsgjson);
                    //            PublicTalkMothed.RightGroupShowSendAtText(chromiumWebBrowser, SendStr, e, messageid, imageTipId, imageSendingId, dt, msgStr, base64Str);

                    //            #region 消息状态监控

                    //            MessageStateArg arg = new MessageStateArg();
                    //            arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                    //            arg.isGroup = true;
                    //            arg.MessageId = messageid;
                    //            arg.SessionId = s_ctt.sessionId;
                    //            arg.WebBrowser = chromiumWebBrowser;
                    //            arg.SendIngId = imageSendingId;
                    //            arg.RepeatId = imageTipId;
                    //            var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageid);
                    //            if (IsHave != null)
                    //            {
                    //                SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                    //            }
                    //            SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                    //            #endregion

                    //            sendAtTextMethod(msgStr, messageid, imageTipId, imageSendingId, failMessage, e, sendContent);
                    //        }
                    //    }
                    //}
                    ////图文混合发送
                    //if (typeString + typeImage == 2 && !string.IsNullOrEmpty((msgStr).Trim()) && typeAt != 1)
                    //{
                    //    if (IsIncognitoModelState)
                    //    {
                    //        showTextMethod("无痕模式不支持图文混合消息发送");
                    //        return;
                    //    }
                    //    int maxChatindex = 0;

                    //    //查询数据库最大chatindex
                    //    T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                    //    maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);

                    //    AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                    //    failMessage.preChatIndex = maxChatindex;
                    //    failMessage.mtp = (int)GlobalVariable.MsgType.Text;
                    //    failMessage.content = "[图文混排]";
                    //    failMessage.sessionid = s_ctt.sessionId;
                    //    DateTime dt = DateTime.Now;
                    //    failMessage.lastDatetime = dt.ToString();
                    //    failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                    //    failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                    //    updateFailMessage(failMessage);

                    //    string messageId = PublicTalkMothed.timeStampAndRandom();
                    //    string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                    //    string imageSendingId = "sending" + imageTipId;

                    //    #region 消息状态监控
                    //    MessageStateArg arg = new MessageStateArg();
                    //    arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                    //    arg.isGroup = false;
                    //    arg.MessageId = messageId;
                    //    arg.SessionId = s_ctt.sessionId;
                    //    arg.WebBrowser = chromiumWebBrowser;
                    //    arg.SendIngId = imageSendingId;
                    //    arg.RepeatId = imageTipId;
                    //    #endregion

                    //    #region content数据构造
                    //    List<object> mixData = new List<object>();
                    //    mixData.Clear();
                    //    foreach (var list in picAndTxtMix)
                    //    {
                    //        switch (list.type)
                    //        {
                    //            case ModelEnum.PictureAndTextMixEnum.Text:
                    //                PictureAndTextMixStringDto text = new PictureAndTextMixStringDto();
                    //                text.type = Convert.ToInt32(ModelEnum.PictureAndTextMixEnum.Text).ToString();
                    //                text.content = list.content;
                    //                mixData.Add(text);
                    //                break;
                    //            case ModelEnum.PictureAndTextMixEnum.Image:
                    //                PictureAndTextMixContentDto imgContent = new PictureAndTextMixContentDto();
                    //                PictureAndTextMixStringDto img = new PictureAndTextMixStringDto();
                    //                img.type = Convert.ToInt32(ModelEnum.PictureAndTextMixEnum.Image).ToString();
                    //                imgContent.picUrl = list.content;
                    //                img.content = JsonConvert.SerializeObject(imgContent);
                    //                mixData.Add(img);
                    //                AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, "R" + list.ImgId, list.content, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                    //                break;
                    //            case ModelEnum.PictureAndTextMixEnum.LineBreak:
                    //                PictureAndTextMixTypeBaseDto line = new PictureAndTextMixTypeBaseDto();
                    //                line.type = Convert.ToInt32(ModelEnum.PictureAndTextMixEnum.LineBreak).ToString() + "000";
                    //                mixData.Add(line);
                    //                break;
                    //        }
                    //    }
                    //    string content = JsonConvert.SerializeObject(mixData);
                    //    #endregion
                    //    #region 消息插入数据库构造
                    //    SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase Mix = new AntSdkChatMsg.ChatBase();
                    //    Mix.MsgType = AntSdkMsgType.ChatMsgMixImageText;
                    //    Mix.chatIndex = maxChatindex.ToString();
                    //    Mix.sourceContent = content;
                    //    //string messageId = PublicTalkMothed.timeStampAndRandom();
                    //    Mix.messageId = messageId;
                    //    Mix.sendUserId = s_ctt.sendUserId;
                    //    Mix.sessionId = s_ctt.sessionId;
                    //    Mix.targetId = GroupInfo?.groupId;
                    //    Mix.SENDORRECEIVE = "0";
                    //    Mix.sendsucessorfail = 0;
                    //    Mix.flag = 0;
                    //    var resultMix = t_chat.Insert(Mix);
                    //    #endregion
                    //    //图文混合展示
                    //    if (resultMix == 1)
                    //    {

                    //    }
                    //    if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                    //    {
                    //        SendMsgListMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                    //    }
                    //    else
                    //    {
                    //        SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                    //    }
                    //    OnceSendMessage.GroupToGroup.OnceMsgList.Add(messageId, picAndTxtMix);
                    //    GroupSendPicAndText.RightGroupSendPicAndTextMix(chromiumWebBrowser, messageId, picAndTxtMix, arg);
                    //    //图片上传
                    //    List<PictureAndTextMixDto> listPicAndText = picAndTxtMix.Where(m => m.type == PictureAndTextMixEnum.Image).ToList();
                    //    var url = AntSdkService.AntSdkConfigInfo.AntSdkMultiFileUpload;
                    //    //var url = "http://ftp.71chat.com/platform/file/v1/file/batch/upload";
                    //    CurrentChatDto currentChat = new CurrentChatDto();
                    //    currentChat.type = AntSdkchatType.Group;
                    //    currentChat.messageId = messageId;
                    //    currentChat.sendUserId = s_ctt.sendUserId;
                    //    currentChat.sessionId = s_ctt.sessionId; ;
                    //    currentChat.targetId = GroupInfo?.groupId;
                    //    sendMixPicAndText(AntSdkMsgType.ChatMsgMixImageText, listPicAndText, currentChat, picAndTxtMix, arg);
                    //    //new MultiFileUpload().MultiFileHttpClientUpLoad(AntSdkMsgType.ChatMsgMixImageText,  listPicAndText, currentChat, picAndTxtMix,null);
                    //    #region 滚动条置底
                    //    StringBuilder sbEnd = new StringBuilder();
                    //    sbEnd.AppendLine("setscross();");
                    //    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    //    #endregion

                    //    _richTextBox.Document.Blocks.Clear();
                    //    if (e != null)
                    //    {
                    //        e.Handled = true;
                    //    }
                    //}
                    //带AT的图文混合
                    #endregion
                    else
                    {

                        if (IsIncognitoModelState)
                        {
                            showTextMethod("无痕模式不支持图文混合消息发送");
                            return;
                        }

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

                        //string ss = JsonConvert.SerializeObject(picAndTxtMix);
                        int maxChatindex = 0;

                        //查询数据库最大chatindex
                        T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                        maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);

                        AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                        failMessage.preChatIndex = maxChatindex;
                        //混合消息结构
                        failMessage.mtp = (int)AntSdkMsgType.ChatMsgMixMessage;
                        failMessage.content = listShow;
                        failMessage.sessionid = s_ctt.sessionId;
                        DateTime dt = DateTime.Now;
                        failMessage.lastDatetime = dt.ToString();
                        failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                        updateFailMessage(failMessage);

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
                        List<object> mixAtData = new List<object>();
                        mixAtData.Clear();
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
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, guidImgId, contentImg.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    break;
                            }
                        }
                        mixMsgClass.TagDto = listTagDto;
                        #endregion
                        string content = JsonConvert.SerializeObject(ListMixMsg);
                        #region 消息插入数据库构造
                        SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase Mix = new AntSdkChatMsg.ChatBase();
                        Mix.MsgType = AntSdkMsgType.ChatMsgMixMessage;
                        Mix.chatIndex = maxChatindex.ToString();
                        Mix.sourceContent = content;
                        //string messageId = PublicTalkMothed.timeStampAndRandom();
                        Mix.messageId = messageId;
                        Mix.sendUserId = s_ctt.sendUserId;
                        Mix.sessionId = s_ctt.sessionId;
                        Mix.targetId = GroupInfo?.groupId;
                        Mix.SENDORRECEIVE = "0";
                        Mix.sendsucessorfail = 0;
                        Mix.flag = 0;
                        #endregion
                        var resultMix = t_chat.Insert(Mix);
                        if (resultMix == 1)
                        {

                        }
                        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageId))
                        {
                            SendMsgListMonitor.MsgIdAndImgSendingId[messageId] = imageSendingId;
                        }
                        else
                        {
                            SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageId, imageSendingId);
                        }
                        OnceSendMessage.GroupToGroup.OnceMsgList.Add(messageId, mixMsg);
                        chromiumWebBrowser.ExecuteScriptAsync("setscross();");
                        GroupSendPicAndText.RightGroupSendAtPicAndTextMix(chromiumWebBrowser, messageId, mixMsg, arg, mixMsgClass);
                        #region 滚动条置底

                        StringBuilder sbEnd = new StringBuilder();
                        sbEnd.AppendLine("setscross();");

                        var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());


                        #endregion
                        //图片上传
                        //List<PictureAndTextMixDto> listAtPicAndText = picAndTxtMix.Where(m => m.type == PictureAndTextMixEnum.Image).ToList();
                        var url = AntSdkService.AntSdkConfigInfo.AntSdkMultiFileUpload;
                        //var url = "http://ftp.71chat.com/platform/file/v1/file/batch/upload";
                        CurrentChatDto currentChat = new CurrentChatDto();
                        currentChat.type = AntSdkchatType.Group;
                        currentChat.messageId = messageId;
                        currentChat.sendUserId = s_ctt.sendUserId;
                        currentChat.sessionId = s_ctt.sessionId; ;
                        currentChat.targetId = GroupInfo?.groupId;
                        sendMixMsg(AntSdkMsgType.ChatMsgAt, currentChat, arg, mixMsgClass, mixMsg);


                        _richTextBox.Document.Blocks.Clear();
                        //if (e != null)
                        //{
                        //    e.Handled = true;
                        //}
                    }
                }
                IsFirst = false;
            }
            catch (Exception ex)
            {
                IsFirst = false;
                LogHelper.WriteError("[TalkGroupViewModel_sendMsg]" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        public void sendMixMsg(AntSdkMsgType type, CurrentChatDto currentChat, MessageStateArg arg, MixMsg mixMsg, List<MixMessageObjDto> obj)
        {
            new MultiFileUpload().MultiFileHttpClientUpLoad(AntSdkMsgType.ChatMsgAt, currentChat, arg, mixMsg, obj);
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
            new MultiFileUpload().MultiFileHttpClientUpLoad(AntSdkMsgType.ChatMsgMixMessage, currentChat, arg, mixMsg, obj);
        }

        /// <summary>
        /// 插入图片地址
        /// </summary>
        /// <param name="preOrEnd"></param>
        /// <param name="chatIndex"></param>
        /// <param name="imageUrl"></param>
        public void AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd preOrEnd, burnMsg.isBurnMsg isBurnMsg, string chatIndex, string imageUrl, string imageId, burnMsg.IsReadImg isRead, burnMsg.IsEffective isEffective)
        {
            AddImgUrlDto addImg = new AddImgUrlDto();
            switch (preOrEnd)
            {
                case AddImgUrlDto.InsertPreOrEnd.End:
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imageUrl;
                    addImg.ImageId = imageId;
                    addImg.messageId = chatIndex;
                    addImg.PreOrEnd = AddImgUrlDto.InsertPreOrEnd.End;
                    if (isBurnMsg == burnMsg.isBurnMsg.yesBurn)
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.yesBurn;
                        addImg.IsEffective = isEffective;
                        var ishave = listDictImgUrlsBurn.SingleOrDefault(m => m.ChatIndex == chatIndex);
                        if (ishave == null)
                        {
                            listDictImgUrlsBurn.Add(addImg);
                        }
                    }
                    else
                    {
                        addImg.IsEffective = isEffective;
                        addImg.IsBurn = burnMsg.isBurnMsg.notBurn;
                        var ishave = listDictImgUrls.SingleOrDefault(m => m.ChatIndex == chatIndex);
                        if (ishave == null)
                        {
                            listDictImgUrls.Add(addImg);
                        }
                    }
                    break;
                case AddImgUrlDto.InsertPreOrEnd.Pre:
                    addImg.ChatIndex = chatIndex;
                    addImg.ImageUrl = imageUrl;
                    addImg.PreOrEnd = AddImgUrlDto.InsertPreOrEnd.Pre;
                    addImg.messageId = chatIndex;
                    if (isBurnMsg == burnMsg.isBurnMsg.yesBurn)
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.yesBurn;
                        addImg.IsEffective = isEffective;
                        var ishave = listDictImgUrlsBurn.SingleOrDefault(m => m.ChatIndex == chatIndex);
                        if (ishave == null)
                        {
                            listDictImgUrlsBurn.Insert(0, addImg);
                        }
                    }
                    else
                    {
                        addImg.IsBurn = burnMsg.isBurnMsg.notBurn;
                        addImg.IsEffective = isEffective;
                        var ishave = listDictImgUrls.SingleOrDefault(m => m.ChatIndex == chatIndex);
                        if (ishave == null)
                        {
                            listDictImgUrls.Insert(0, addImg);
                        }
                    }

                    break;
            }
        }

        //Dictionary<string,object>  dictAtMsg=new Dictionary<string, object>();
        public void sendAtTextMethod(string msgStr, string messageid, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failMessage, System.Windows.Input.KeyEventArgs e, object sendContent)
        {
            #region 消息发送

            //string messageID = PublicTalkMothed.timeStampAndRandom();
            //SendMessageAtOut_ctt outCtt = new SendMessageAtOut_ctt();
            //outCtt.mtp = (int)GlobalVariable.MsgType.At;


            //SendMessageAt_ctt smt = new SendMessageAt_ctt();
            //smt.messageId = messageid;
            //smt.sendUserId = s_ctt.sendUserId;
            //smt.targetId = GroupInfo.groupId;
            //smt.companyCode = s_ctt.companyCode;
            //smt.os = 1;
            //smt.sessionId = s_ctt.sessionId;


            //ContentAtOut atOut = new ContentAtOut();
            //atOut.mtp = ((int)GlobalVariable.MsgType.Text).ToString();


            //ContentAtIn atIn = new ContentAtIn();
            //atIn.content = msgStr;
            //atIn.ids = liststr;

            //atOut.ctt = atIn;
            //smt.content = atOut;
            //outCtt.ctt = smt;


            //string err = JsonConvert.SerializeObject(outCtt);
            var isRobot = false; //是否为@机器人 调用接口不同
            string error = "";
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkChatMsg.At atMsg = new AntSdkChatMsg.At();
            AntSdkChatMsg.At_content atContent = new AntSdkChatMsg.At_content();
            atMsg.chatType = (int)AntSdkchatType.Group;
            //atContent.At_contents = msgStr;
            //atContent.At_users = liststr;
            //atMsg.atcontent = atContent;
            atMsg.MsgType = AntSdkMsgType.ChatMsgAt;
            atMsg.sendUserId = s_ctt.sendUserId;
            atMsg.sessionId = s_ctt.sessionId;
            atMsg.targetId = GroupInfo.groupId;
            atMsg.messageId = messageid;
            atMsg.sourceContent = JsonConvert.SerializeObject(sendContent);
            var contentList = sendContent as List<object>;
            if (contentList != null)
            {
                var robotId = AntSdkService.AntSdkCurrentUserInfo.robotId;
                if (!string.IsNullOrEmpty(robotId))
                {
                    if (contentList.OfType<AntSdkChatMsg.contentAtOrdinary>().Any(atcontent => atcontent.ids.Contains(robotId)))
                    {
                        isRobot = true;
                    }
                }
            }
            bool isResult = false;
            if (isRobot)
            {
                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                }
                else
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                }
                if (failMessage.isOnceSendMsg)
                {
                    var content = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == messageid).Value as AntSdkChatMsg.At;
                    if (content != null)
                    {
                        atMsg = content;
                    }
                }
                else
                {
                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(messageid, atMsg);
                }
                isResult = failMessage.isOnceSendMsg == true ? AntSdkService.SdkReRobotPublishChatMsg(atMsg, ref error) : AntSdkService.SdkRobotPublishChatMsg(atMsg, ref error);
            }
            else
            {
                if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageid))
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId[messageid] = imageSendingId;
                }
                else
                {
                    SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageid, imageSendingId);
                }
                if (failMessage.isOnceSendMsg)
                {
                    var content = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == messageid).Value as AntSdkChatMsg.At;
                    if (content != null)
                    {
                        atMsg = content;
                    }
                }
                else
                {
                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(messageid, atMsg);
                }
                isResult = failMessage.isOnceSendMsg == true ? AntSdkService.SdkRePublishChatMsg(atMsg, ref error) : AntSdkService.SdkPublishChatMsg(atMsg, ref error);
            }
            if (isResult)
            {
                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                updateFailMessage(failMessage);
                //PublicTalkMothed.HiddenMsgDiv(IsIncognitoModelState ? chromiumWebBrowserburn : chromiumWebBrowser,
                //    imageSendingId);
            }
            else
            {
                failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
                PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, imageTipId);
                //发送失败回调方法
                updateFailMessage(failMessage);
            }

            #endregion

            #region 滚动条置底

            StringBuilder sbEnd = new StringBuilder();
            sbEnd.AppendLine("setscross();");
            if (IsIncognitoModelState)
            {
                var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
            }
            else
            {
                var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
            }

            #endregion

            if (msgStr.Trim() != "")
            {
                _richTextBox.Document.Blocks.Clear();
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }

        public void sendTextMethod(string msgStr, string messageid, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failMessage, System.Windows.Input.KeyEventArgs e, bool isOnceSendMsg)
        {
            //TODO:AntSdk_Modify:Example

            #region 消息发送同步方法

            //#region 消息发送

            //var sendText = new SDK.AntSdk.AntModels.AntSdkChatMsg.Text
            //{
            //    chatType = (int) AntSdkchatType.Group,
            //    content = msgStr.TrimEnd('\n').TrimEnd('\r'),
            //    messageId = messageid,
            //    sendUserId = s_ctt.sendUserId,
            //    sessionId = s_ctt.sessionId,
            //    os = (int) GlobalVariable.OSType.PC,
            //    targetId = GroupInfo?.groupId,
            //    MsgType = AntSdkMsgType.ChatMsgText,
            //    flag = IsIncognitoModelState ? 1 : 0
            //};

            //#region 发送成功 插入数据

            ////TODO:AntSdk_Modify
            //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt =
            //    JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(
            //        JsonConvert.SerializeObject(sendText));
            ////sm_ctt.MTP = "1";
            //sm_ctt.sourceContent = msgStr.TrimEnd('\n').TrimEnd('\r');
            //sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
            //sm_ctt.sendsucessorfail = 0;
            //sm_ctt.SENDORRECEIVE = "1";
            //if (IsIncognitoModelState)
            //{
            //    //添加阅后即焚数据到阅后即焚表
            //    ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
            //}
            //else
            //{
            //    ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
            //}

            //#endregion

            //var errorMsg = string.Empty;
            //var isResult = AntSdkService.SdkPublishChatMsg(sendText, ref errorMsg);
            ////bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);
            //if (isResult)
            //{
            //    failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
            //    if (IsIncognitoModelState)
            //    {
            //        //更新状态
            //        T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
            //        t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
            //            AntSdkService.AntSdkCurrentUserInfo.userId);
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, imageSendingId);
            //    }
            //    else
            //    {
            //        //更新状态
            //        T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
            //        t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
            //            AntSdkService.AntSdkCurrentUserInfo.userId);
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
            //    }
            //    //updateFailMessage(failMessage);
            //}
            //else
            //{
            //    failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
            //    if (IsIncognitoModelState)
            //    {
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, imageSendingId);
            //        PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowserburn, imageTipId);
            //        //updateFailMessage(failMessage);
            //    }
            //    else
            //    {
            //        PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
            //        PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, imageTipId);
            //        //发送失败回调方法
            //        updateFailMessage(failMessage);
            //    }
            //}

            //#endregion

            //#region 滚动条置底

            //StringBuilder sbEnd = new StringBuilder();
            //sbEnd.AppendLine("setscross();");
            //if (IsIncognitoModelState)
            //{
            //    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
            //}
            //else
            //{
            //    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
            //}

            //#endregion

            //if (msgStr.Trim() != "")
            //{
            //    _richTextBox.Document.Blocks.Clear();
            //}
            //if (e != null)
            //{
            //    e.Handled = true;
            //}

            #endregion

            bool result = false;
            var errorMsg = string.Empty;

            #region 消息发送异步方法 修改姚伦海

            #region 消息发送

            var sendText = new AntSdkChatMsg.Text();
            if (isOnceSendMsg == false)
            {
                sendText.chatType = (int)AntSdkchatType.Group;
                sendText.content = msgStr.TrimEnd('\n').TrimEnd('\r');
                sendText.messageId = messageid;
                sendText.sendUserId = s_ctt.sendUserId;
                sendText.sessionId = s_ctt.sessionId;
                sendText.os = (int)GlobalVariable.OSType.PC;
                sendText.targetId = GroupInfo?.groupId;
                sendText.MsgType = AntSdkMsgType.ChatMsgText;
                sendText.flag = IsIncognitoModelState ? 1 : 0;
            }
            else
            {
                sendText.chatType = (int)AntSdkchatType.Group;
                sendText.content = msgStr.TrimEnd('\n').TrimEnd('\r');
                sendText.messageId = messageid;
                sendText.sendUserId = s_ctt.sendUserId;
                sendText.sessionId = failMessage.sessionid;
                sendText.os = (int)GlobalVariable.OSType.PC;
                sendText.targetId = failMessage.targetId;
                sendText.MsgType = AntSdkMsgType.ChatMsgText;
                sendText.flag = IsIncognitoModelState ? 1 : 0;
            }

            #region 发送成功 插入数据

            //TODO:AntSdk_Modify
            AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(sendText));
            sm_ctt.sourceContent = msgStr.TrimEnd('\n').TrimEnd('\r');
            sm_ctt.chatIndex = failMessage.preChatIndex.ToString();
            sm_ctt.sendsucessorfail = 0;
            if (isOnceSendMsg == false)
            {
                sm_ctt.SENDORRECEIVE = "1";
                if (IsIncognitoModelState)
                {
                    //添加阅后即焚数据到阅后即焚表
                    addGroupBurnData(sm_ctt);
                }
                else
                {
                    addData(sm_ctt);
                }
                OnceSendMessage.GroupToGroup.OnceMsgList.Add(messageid, sendText);
                result = AntSdkService.SdkPublishChatMsg(sendText, ref errorMsg);
            }
            else
            {
                var content = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == messageid).Value as AntSdkChatMsg.Text;
                if (content != null)
                {
                    sendText = content;
                }
                result = AntSdkService.SdkRePublishChatMsg(sendText, ref errorMsg);
            }

            #endregion

            if (result)
            {
                //failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                if (IsIncognitoModelState)
                {
                    ////更新状态
                    //T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                    //t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                    //    AntSdkService.AntSdkCurrentUserInfo.userId);
                    //PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, imageSendingId);
                }
                else
                {
                    ////更新状态
                    //T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                    //t_chat.UpdateSendMsgState(messageid, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                    //    AntSdkService.AntSdkCurrentUserInfo.userId);
                    //PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
                }
            }
            else
            {
                //failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                if (IsIncognitoModelState)
                {
                    PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowserburn, imageTipId);
                }
                else
                {
                    PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, imageSendingId);
                    PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, imageTipId);
                    //发送失败回调方法
                    updateFailMessage(failMessage);
                }
            }

            #endregion

            if (e != null)
            {
                e.Handled = true;
            }

            #endregion
        }

        List<BackgroundWorkers> listBackWork = new List<BackgroundWorkers>();

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
            //System.Drawing.Bitmap pic = new System.Drawing.Bitmap(prePaths);
          
            //fileInput.imageWidth = pic.Width.ToString();
            //fileInput.imageHeight = pic.Height.ToString();
            fileInput.isOnceSendMsg = failOrSucess.isOnceSendMsg;
            //pic.Dispose();
            var resultBackWork = listBackWork.SingleOrDefault(m => m.messageId == messageId);
            if (resultBackWork == null)
            {
                BackgroundWorkers back = new BackgroundWorkers();
                back.WorkerSupportsCancellation = true;
                back.messageId = messageId;
                listBackWork.Add(back);
                back.DoWork += Back_DoWork;
                back.RunWorkerCompleted += Back_RunWorkerCompleted;
                back.RunWorkerAsync(fileInput);
            }
            else
            {
                resultBackWork.CancelAsync();
                resultBackWork.Dispose();
                listBackWork.Remove(resultBackWork);
                BackgroundWorkers back = new BackgroundWorkers();
                back.WorkerSupportsCancellation = true;
                back.messageId = messageId;
                listBackWork.Add(back);
                back.DoWork += Back_DoWork;
                back.RunWorkerCompleted += Back_RunWorkerCompleted;
                back.RunWorkerAsync(fileInput);
            }
        }

        private void IsShowTip_Tick(object sender, EventArgs e)
        {
            IsRevocationShowPopup = false;
            isShowPopup = false;
            IsShowTip.Stop();
        }

        #endregion

        public ICommand btnImagesUploadCommand
        {
            get { return new DelegateCommand(() => { msgEditAssistant.ImgImagesUpload(); }); }
        }

        #endregion

        //public RowDefinition showFile = null;
        //BackgroundWorker backFileUpload = null;
        List<UpLoadFilesDto> upFileList = new List<UpLoadFilesDto>();

        #region 上传文件事件

        public ICommand btnFileUploadCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    try
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
                            //showFile = obj as RowDefinition;
                            SelectUploadFiles(openFile.FileNames);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[TalkGroupViewModel_btnFileUploadCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }

        /// <summary>
        /// 删除阅后即焚消息记录
        /// </summary>
        public ICommand btnDeleteCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    //SendBurnDto sendBurn = new SendBurnDto();
                    //sendBurn.sendUserId = s_ctt.sendUserId;
                    //sendBurn.companyCode = GlobalVariable.CompanyCode;
                    //sendBurn.sessionId = s_ctt.sessionId;
                    //sendBurn.targetId = GroupInfo.groupId;

                    //BurnContent burn = new BurnContent();
                    //burn.type = ((int)GlobalVariable.IsBurnMode.IsBurn).ToString();
                    //burn.groupId = GroupInfo.groupId;
                    //sendBurn.ctt = burn;
                    //string error = "";
                    //string ss = JsonConvert.SerializeObject(sendBurn);

                    SendMessageDto smg = new SendMessageDto();
                    SendMessage_ctt smt = new SendMessage_ctt();
                    smg.mtp = (int)GlobalVariable.MsgType.SysUserMsg;
                    BurnContent burn = new BurnContent();
                    burn.type = ((int)GlobalVariable.SysUserMsgType.DeleteMsgBurnMode).ToString();
                    ;
                    burn.groupId = GroupInfo.groupId;
                    smt.content = JsonConvert.SerializeObject(burn);

                    smt.companyCode = s_ctt.companyCode;
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.sendUserId = s_ctt.sendUserId;
                    smt.sessionId = s_ctt.sessionId;
                    smt.targetId = GroupInfo.groupId;
                    smg.ctt = smt;
                    string error = "";
                    //string ss = JsonConvert.SerializeObject(smg);
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.ChatBase chatMsg = new AntSdkChatMsg.ChatBase();
                    chatMsg.chatType = (int)AntSdkchatType.Group;
                    chatMsg.MsgType = AntSdkMsgType.GroupOwnerBurnDelete;
                    chatMsg.sendUserId = s_ctt.sendUserId;
                    chatMsg.sessionId = s_ctt.sessionId;
                    chatMsg.targetId = GroupInfo.groupId;
                    chatMsg.messageId = PublicTalkMothed.timeStampAndRandom();
                    bool isResult = AntSdkService.SdkPublishGpOwnerChangeMode(AntSdkGroupChangeMode.BurnModeDelete, chatMsg, ref error);

                    //清屏幕数据
                    chromiumWebBrowserburn.ExecuteScriptAsync((PublicTalkMothed.clearHtml()));
                    TextShowRowHeight = "0";
                    TextShowReceiveMsg = "";
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
                    if (!AntSdkService.AntSdkIsConnected)
                        return;
                    //System.Windows.Controls.Button btnBrunSwitch = obj as System.Windows.Controls.Button;
                    //AfterReadBrunWindow afterReadBurn = new AfterReadBrunWindow();
                    //afterReadBurn.Owner = Window.GetWindow(btnBrunSwitch);
                    //afterReadBurn.ShowDialog();
                    isShowBurn = "Collapsed";
                    isShowEmoji = "Collapsed";
                    isShowSound = Visibility.Collapsed;
                    isShowCutImage = "Collapsed";
                    NoticeVisibility = Visibility.Collapsed;
                    isShowExit = "Visible";
                    IsIncognitoModelState = true;
                    isShowDelete = "Visible";
                    isShowWinMsg = "Collapsed";
                    isShowBurnWinMsg = "Visible";

                    //切换阅后即焚模式

                    #region 消息发送

                    SendMessageDto smg = new SendMessageDto();
                    SendMessage_ctt smt = new SendMessage_ctt();
                    smg.mtp = (int)GlobalVariable.MsgType.SysUserMsg;
                    BurnContent burn = new BurnContent();
                    burn.type = "501";
                    burn.groupId = GroupInfo.groupId;
                    smt.content = JsonConvert.SerializeObject(burn);

                    smt.companyCode = s_ctt.companyCode;
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.sendUserId = s_ctt.sendUserId;
                    smt.sessionId = s_ctt.sessionId;
                    smt.targetId = GroupInfo.groupId;
                    smg.ctt = smt;
                    string error = "";
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.ChatBase chatMsg = new AntSdkChatMsg.ChatBase();
                    chatMsg.chatType = (int)AntSdkchatType.Group;
                    chatMsg.MsgType = AntSdkMsgType.GroupOwnerBurnMode;
                    chatMsg.sendUserId = s_ctt.sendUserId;
                    chatMsg.sessionId = s_ctt.sessionId;
                    chatMsg.messageId = PublicTalkMothed.timeStampAndRandom();

                    chatMsg.targetId = GroupInfo.groupId;
                    bool isResult = AntSdkService.SdkPublishGpOwnerChangeMode(AntSdkGroupChangeMode.BurnMode, chatMsg, ref error);
                    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);

                    #endregion

                    //PublicTalkMothed.showBurnTips(_chromiumWebBrowserburn);
                });
            }
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
                    if (!AntSdkService.AntSdkIsConnected)
                        return;
                    TextShowRowHeight = "0";
                    TextShowReceiveMsg = "";
                    System.Windows.Controls.Button btn = obj as System.Windows.Controls.Button;
                    isShowBurn = "Visible";
                    isShowEmoji = "Visible";
                    isShowCutImage = "Visible";
                    isShowSound = Visibility.Visible;
                    NoticeVisibility = Visibility.Visible;
                    isShowExit = "Collapsed";
                    IsIncognitoModelState = false;
                    isShowDelete = "Collapsed";
                    isShowWinMsg = "Visible";
                    isShowBurnWinMsg = "Collapsed";

                    //切换非阅后即焚模式

                    #region 消息发送

                    SendMessageDto smg = new SendMessageDto();
                    SendMessage_ctt smt = new SendMessage_ctt();
                    smg.mtp = (int)GlobalVariable.MsgType.SysUserMsg;
                    BurnContent burn = new BurnContent();
                    burn.type = "502";
                    burn.groupId = GroupInfo.groupId;
                    smt.content = JsonConvert.SerializeObject(burn);

                    smt.companyCode = s_ctt.companyCode;
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.sendUserId = s_ctt.sendUserId;
                    smt.sessionId = s_ctt.sessionId;
                    smt.targetId = GroupInfo.groupId;
                    smg.ctt = smt;
                    string error = "";
                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.ChatBase chatMsg = new AntSdkChatMsg.ChatBase();
                    chatMsg.chatType = (int)AntSdkchatType.Group;
                    chatMsg.MsgType = AntSdkMsgType.GroupOwnerNormal;
                    chatMsg.sendUserId = s_ctt.sendUserId;
                    chatMsg.sessionId = s_ctt.sessionId;
                    chatMsg.messageId = PublicTalkMothed.timeStampAndRandom();
                    chatMsg.targetId = GroupInfo.groupId;
                    bool isResult = AntSdkService.SdkPublishGpOwnerChangeMode(AntSdkGroupChangeMode.NormalMode, chatMsg, ref error);
                    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);

                    #endregion

                    //删除阅后即焚对应会话内容
                    //T_Chat_Message_GroupBurnDAL tGroupBurnDal = new T_Chat_Message_GroupBurnDAL();
                    //tGroupBurnDal.DeleteBurnData(s_ctt.sessionId, GlobalVariable.CompanyCode, s_ctt.sendUserId);

                    //清除页面数据
                    //if (chromiumWebBrowser != null && chromiumWebBrowser.IsBrowserInitialized)
                    //    chromiumWebBrowserburn?.ExecuteScriptAsync(PublicTalkMothed.clearHtml());
                });
            }
        }

        /// <summary>
        /// 选择需上传文件
        /// </summary>
        /// <param name="fileNames"></param>
        public void SelectUploadFiles(string[] fileAndDirNames)
        {
            //文件是否占用
            bool isFileUse = false;
            List<string> fileNames = new List<string>();
            bool isContainDirectory = false;
            bool isContainDuplicateFile = false;
            for (int i = 0; i < fileAndDirNames.Length; i++)
            {
                if (File.Exists(fileAndDirNames[i])) // 是文件
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
                else if (Directory.Exists(fileAndDirNames[i])) // 是文件夹
                {
                    isContainDirectory = true;
                }
            }

            if (fileNames.Count() > 5)
            {
                KeyboardHookLib.isHook = true;
                //System.Windows.Forms.MessageBox.Show("上传文件不能超过10个!", "温馨提示");
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
                        //System.Windows.Forms.MessageBox.Show(msg + "", "温馨提示");
                        MessageBoxWindow.Show("温馨提示", msg, GlobalVariable.WarnOrSuccess.Warn);
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

                    fUserControl.img.Source = new BitmapImage(new Uri(fileShowImage.showImageHtmlPath(list.fileExtendName, ""), UriKind.RelativeOrAbsolute));
                    fUserControl.fileName.ToolTip = list.fileName;
                    fUserControl.btnClose.Tag = fUserControl;
                    _wrapPanel.Children.Add(fUserControl);
                }
            }
            if (isFileUse == true)
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

        public void richTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
            //Window_Drop(sender, e);
            //Debug.Print("SearchTextBox_PreviewDragOver");
        }

        public void richTextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            _chromiumWebBrowser_PreviewDrop(sender, e);
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_BtnClose_Click]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        private void ThreadUploadFile(UpLoadFilesDto upDtos)
        {
            //updateFailMessage(upDtos.FailOrSucess);
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

            //2017-04-06 添加
            scid.imgeTipId = upDto.imageTipId;
            scid.FailOrSucess = upDto.FailOrSucess;
            scid.imageSendingId = upDto.imageSendingId;
            scid.from = upDto.from;
            scid.ImgOrFileArg = upDto.ImgOrFileArg;
            FileUploadMutis(scid);
        }

        private void BackFileUpload_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker background = sender as BackgroundWorker;
            try
            {
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
                //2017-03-17 添加
                scid.back = background;
                FileUploadMutis(scid);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_BackFileUpload_DoWork]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }

        #endregion

        #region UrlEncode转化

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }

        #endregion

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
                    if (errorMsg.Length > 0)
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
                        if (scid.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            sendFile = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == scid.messageId).Value as OnceSendFile;
                            smt.sendUserId = sendFile.ctt.sendUserId;
                            smt.sessionId = sendFile.ctt.sessionId;
                            smt.targetId = sendFile.GroupInfo.groupId;
                        }
                        else
                        {
                            smt.sendUserId = s_ctt.sendUserId;
                            smt.sessionId = s_ctt.sessionId;
                            smt.targetId = GroupInfo.groupId;
                        }
                        if (_isShowBurn == "Collapsed")
                        {
                            smt.flag = 1;
                        }
                        smt.MsgType = AntSdkMsgType.ChatMsgFile;
                        string name = fileOutput.fileName;
                        fDto.fileUrl = fileOutput.dowmnloadUrl;
                        fDto.fileName = name;
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
                        fileMsg.flag = _isBurnMode == GlobalVariable.BurnFlag.IsBurn ? 1 : 0;
                        AntSdkChatMsg.File_content fileContent = new AntSdkChatMsg.File_content();
                        fileContent.fileName = scid.fileFileName;
                        fileContent.size = scid.filesize;
                        fileContent.fileUrl = fileOutput.dowmnloadUrl;
                        fileMsg.content = fileContent;
                        fileMsg.MsgType = AntSdkMsgType.ChatMsgFile;
                        fileMsg.chatType = (int)AntSdkchatType.Group;
                        fileMsg.messageId = scid.messageId;
                        if (scid.from == AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            fileMsg.sendUserId = sendFile.ctt.sendUserId;
                            fileMsg.sessionId = sendFile.ctt.sessionId;
                            fileMsg.targetId = sendFile.GroupInfo.groupId;
                        }
                        else
                        {
                            fileMsg.sendUserId = s_ctt.sendUserId;
                            fileMsg.sessionId = s_ctt.sessionId;
                            fileMsg.targetId = GroupInfo.groupId;
                        }

                        #region 发送成功 插入数据

                        //SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                        ////TODO:AntSdk_Modify
                        ////sm_ctt.MTP = "3";
                        //sm_ctt.chatIndex = scid.FailOrSucess.preChatIndex.ToString();
                        //sm_ctt.sendsucessorfail = 0;
                        //sm_ctt.SENDORRECEIVE = "1";
                        ////2017-03-07 添加
                        //sm_ctt.uploadOrDownPath = scid.file;
                        //if (IsIncognitoModelState)
                        //{
                        //    ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
                        //}
                        //else
                        //{
                        //    ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                        //}

                        #endregion

                        #region 消息状态监控

                        //MessageStateArg arg = new MessageStateArg();
                        //arg.isBurn = fileMsg.flag == 1 ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn;
                        //arg.isGroup = false;
                        //arg.MessageId = scid.messageId;
                        //arg.SessionId = s_ctt.sessionId;
                        //arg.WebBrowser = fileMsg.flag == 1 ? chromiumWebBrowserburn : chromiumWebBrowser;
                        //arg.SendIngId = scid.imageSendingId;
                        //arg.RepeatId = scid.imgeTipId;
                        //var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == scid.messageId);
                        //if (IsHave != null)
                        //{
                        //    MessageStateMonitorList.Remove(IsHave);
                        //}
                        //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                        #endregion

                        bool isResult = false;
                        if (scid.from != AntSdkSendFrom.SendFrom.OnceSend)
                        {
                            SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(scid.ImgOrFileArg));
                            isResult = AntSdkService.SdkPublishChatMsg(fileMsg, ref error);
                        }
                        else
                        {
                            SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(scid.ImgOrFileArg));
                            isResult = AntSdkService.SdkRePublishChatMsg(fileMsg, ref error);
                        }
                        if (isResult)
                        {
                            StringBuilder sbFileimg = new StringBuilder();
                            sbFileimg.AppendLine("setFileImg('" + scid.imgguid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                            if (IsIncognitoModelState)
                            {
                                //更改上传成功路径
                                T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                                t_chat.UpdateContent(scid.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, smt.sourceContent);

                                this.chromiumWebBrowserburn.ExecuteScriptAsync(sbFileimg.ToString());

                                string mstr = "setProcess('" + scid.progressId + "','" + 100 + "%');";
                                this.chromiumWebBrowserburn.ExecuteScriptAsync(mstr);
                                //progress.Wait();

                                StringBuilder sbFileText = new StringBuilder();
                                sbFileText.AppendLine("setFileText('" + scid.textguid + "','上传成功');");
                                this.chromiumWebBrowserburn.ExecuteScriptAsync(sbFileText.ToString());
                                //task.Wait();
                                PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, scid.imageSendingId);
                            }
                            else
                            {
                                //更新状态
                                T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                                t_chat.UpdateContent(scid.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, smt.sourceContent);

                                this.chromiumWebBrowser.ExecuteScriptAsync(sbFileimg.ToString());

                                string mstr = "setProcess('" + scid.progressId + "','" + 100 + "%');";
                                ;
                                this.chromiumWebBrowser.ExecuteScriptAsync(mstr);
                                //progress.Wait();

                                StringBuilder sbFileText = new StringBuilder();
                                sbFileText.AppendLine("setFileText('" + scid.textguid + "','上传成功');");
                                this.chromiumWebBrowser.ExecuteScriptAsync(sbFileText.ToString());
                                //task.Wait();
                                PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, scid.imageSendingId);
                            }
                        }

                        #endregion

                        if (IsIncognitoModelState)
                        {
                            #region 滚动条置底

                            StringBuilder sbEnd = new StringBuilder();
                            sbEnd.AppendLine("setscross();");
                            this.chromiumWebBrowserburn.ExecuteScriptAsync(sbEnd.ToString());

                            #endregion
                        }
                        else
                        {
                            #region 滚动条置底

                            StringBuilder sbEnd = new StringBuilder();
                            sbEnd.AppendLine("setscross();");
                            this.chromiumWebBrowser.ExecuteScriptAsync(sbEnd.ToString());

                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_FileUploadMutis]:" + ex.Message + ex.StackTrace);
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
                ////Thread.Sleep(100);
                HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
                StringBuilder sbEnd = new StringBuilder();
                SendCutImageDto dtos = hbc.obj as SendCutImageDto;
                string proId = dtos.progressId.ToString();
                sbEnd.AppendLine("setProcess('" + proId + "','" + e.ProgressPercentage + "%');");
                if (IsIncognitoModelState)
                {
                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                }
                else
                {
                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_Client_UploadProgressChanged]:" + ex.Message + ex.StackTrace);
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
                        if (IsIncognitoModelState)
                        {
                            var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        else
                        {
                            var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        }

                        #endregion
                    }

                    StringBuilder sbFileimg = new StringBuilder();
                    sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                    if (IsIncognitoModelState)
                    {
                        this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileimg.ToString());
                    }
                    else
                    {
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                    }


                    StringBuilder sbFileText = new StringBuilder();
                    sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                    if (IsIncognitoModelState)
                    {
                        this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileText.ToString());
                    }
                    else
                    {
                        this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                    }

                    string str = Encoding.UTF8.GetString(e.Result);
                    //UpLoadFilesDto rct = JsonConvert.DeserializeObject<UpLoadFilesDto>(Encoding.UTF8.GetString(e.Result));

                    //backFileUpload.Dispose();
                }
                else
                {
                    #region 滚动条置底

                    StringBuilder sbEnd = new StringBuilder();
                    sbEnd.AppendLine("setscross();");
                    if (IsIncognitoModelState)
                    {
                        var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                    }
                    else
                    {
                        var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    }

                    #endregion

                    // HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
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
                    smt.MsgType = AntSdkMsgType.ChatMsgFile;
                    fDto.fileUrl = upoutput.dowmnloadUrl;
                    fDto.thumbnailUrl = upoutput.thumbnailUrl;
                    fDto.fileName = sucess.fileFileName;
                    fDto.size = sucess.filesize;
                    fDto.fileExtendName = sucess.fileFileExtendName;
                    smt.content = JsonConvert.SerializeObject(fDto);
                    smt.companyCode = s_ctt.companyCode;
                    smt.sourceContent = JsonConvert.SerializeObject(fDto);
                    //smt.messageId = Guid.NewGuid().ToString();

                    //2017-04-06 修改
                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                    smt.messageId = sucess.messageId;
                    OnceSendFile sendFile = null;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        sendFile = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == sucess.messageId).Value as OnceSendFile;
                        smt.sendUserId = sendFile.ctt.sendUserId;
                        smt.sessionId = sendFile.ctt.sessionId;
                        smt.targetId = sendFile.GroupInfo.groupId;
                    }
                    else
                    {
                        smt.sendUserId = s_ctt.sendUserId;
                        smt.sessionId = s_ctt.sessionId;
                        smt.targetId = s_ctt.targetId;
                    }

                    //TODO:AntSdk_Modify
                    //DONE:AntSdk_Modify
                    AntSdkChatMsg.File fileMsg = new AntSdkChatMsg.File();
                    AntSdkChatMsg.File_content fileContent = new AntSdkChatMsg.File_content();
                    if (IsIncognitoModelState)
                    {
                        smt.flag = 1;
                        fileMsg.flag = 1;
                    }
                    else
                    {
                        smt.flag = 0;
                        fileMsg.flag = 0;
                    }
                    smg.ctt = smt;
                    string error = "";

                    fileContent.fileName = sucess.fileFileName;
                    fileContent.size = sucess.filesize;
                    fileContent.fileUrl = upoutput.dowmnloadUrl;
                    fileMsg.content = fileContent;
                    fileMsg.MsgType = AntSdkMsgType.ChatMsgFile;
                    fileMsg.chatType = (int)AntSdkchatType.Group;

                    fileMsg.messageId = sucess.messageId;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        fileMsg.sendUserId = sendFile.ctt.sendUserId;
                        fileMsg.sessionId = sendFile.ctt.sessionId;
                        fileMsg.targetId = sendFile.GroupInfo.groupId;
                    }
                    else
                    {
                        fileMsg.sendUserId = s_ctt.sendUserId;
                        fileMsg.sessionId = s_ctt.sessionId;
                        fileMsg.targetId = s_ctt.targetId;
                    }

                    #region 消息状态监控 2017-09-08 16:27

                    //MessageStateArg arg = new MessageStateArg();
                    //arg.isBurn = fileMsg.flag == 1 ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn;
                    //arg.isGroup = false;
                    //arg.MessageId = sucess.messageId;
                    //arg.SessionId = s_ctt.sessionId;
                    //arg.WebBrowser = fileMsg.flag == 1 ? chromiumWebBrowserburn : chromiumWebBrowser;
                    //arg.SendIngId = sucess.imageSendingId;
                    //arg.RepeatId = sucess.imgeTipId;
                    //var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == sucess.messageId);
                    //if (IsHave != null)
                    //{
                    //    MessageStateMonitorList.Remove(IsHave);
                    //}
                    //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                    #endregion

                    bool isResult = false;
                    if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                    {
                        SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(sucess.ImgOrFileArg));
                        isResult = AntSdkService.SdkRePublishChatMsg(fileMsg, ref error);
                    }
                    else
                    {
                        SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(sucess.ImgOrFileArg));
                        isResult = AntSdkService.SdkPublishChatMsg(fileMsg, ref error);
                    }
                    int isSendSucess = 0;
                    //bool b = false;//MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, smg, ref error);
                    if (isResult)
                    {
                        isSendSucess = 1;
                        StringBuilder sbFileimg = new StringBuilder();
                        sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("success", "") + "');");
                        sbEnd.AppendLine("setProcess('" + sucess.progressId + "','" + 100 + "%');");
                        StringBuilder sbFileText = new StringBuilder();
                        sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传成功');");

                        if (IsIncognitoModelState)
                        {
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.yesBurn, PointOrGroupFrom.PointOrGroup.Group, sucess.messageId, smt.content);
                            var cef = OnceSendMessage.GroupToGroup.CefList.SingleOrDefault(m => m.Key == sucess.FailOrSucess.sessionid).Value as GroupCef;
                            if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                            {
                                cef.CefBurn.EvaluateScriptAsync(sbFileimg.ToString());
                                cef.CefBurn.EvaluateScriptAsync(sbEnd.ToString());
                                cef.CefBurn.EvaluateScriptAsync(sbFileText.ToString());
                            }
                            else
                            {
                                this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileimg.ToString());
                                this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                                this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileText.ToString());
                                //PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, sucess.imageSendingId);
                            }
                        }
                        else
                        {
                            SendOrReceiveMsgStateOperate.UpdateContent(AntSdkburnMsg.isBurnMsg.notBurn, PointOrGroupFrom.PointOrGroup.Group, sucess.messageId, smt.content);
                            var cef = OnceSendMessage.GroupToGroup.CefList.SingleOrDefault(m => m.Key == sucess.FailOrSucess.sessionid).Value as GroupCef;
                            if (sucess.from == AntSdkSendFrom.SendFrom.OnceSend)
                            {
                                cef.Cef.EvaluateScriptAsync(sbFileimg.ToString());
                                cef.Cef.EvaluateScriptAsync(sbEnd.ToString());
                                cef.Cef.EvaluateScriptAsync(sbFileText.ToString());
                            }
                            else
                            {
                                this._chromiumWebBrowser.ExecuteScriptAsync(sbFileimg.ToString());
                                this._chromiumWebBrowser.ExecuteScriptAsync(sbEnd.ToString());
                                this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                            }
                        }
                        //if (IsIncognitoModelState)
                        //{
                        //    this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileText.ToString());
                        //    sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                        //    PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, sucess.imageSendingId);
                        //}
                        //else
                        //{
                        //    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                        //    sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sucess;
                        //    PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, sucess.imageSendingId);
                        //}
                    }
                    else
                    {
                        isSendSucess = 0;
                        StringBuilder sbFileimg = new StringBuilder();
                        sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                        if (IsIncognitoModelState)
                        {
                            this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileimg.ToString());
                        }
                        else
                        {
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                        }


                        sbEnd.AppendLine("setProcess('" + sucess.progressId + "','" + 0 + "%');");
                        if (IsIncognitoModelState)
                        {
                            var progress = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        else
                        {
                            var progress = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        StringBuilder sbFileText = new StringBuilder();
                        sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                        if (IsIncognitoModelState)
                        {
                            this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileText.ToString());
                        }
                        else
                        {
                            this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                        }
                        if (IsIncognitoModelState)
                        {
                            sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowserburn, sucess.imageSendingId);
                            PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowserburn, sucess.imgeTipId);
                            updateFailMessage(sucess.FailOrSucess);
                        }
                        else
                        {
                            sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                            PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, sucess.imageSendingId);
                            PublicTalkMothed.VisibleMsgDiv(chromiumWebBrowser, sucess.imgeTipId);
                            updateFailMessage(sucess.FailOrSucess);
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region 滚动条置底

                StringBuilder sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                if (IsIncognitoModelState)
                {
                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                }
                else
                {
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, hbc.obj.imgeTipId);
                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                }

                #endregion

                if (IsIncognitoModelState)
                {
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowserburn, hbc.obj.imgeTipId);
                }
                else
                {
                    PublicTalkMothed.VisibleMsgDiv(this.chromiumWebBrowser, hbc.obj.imgeTipId);
                }
                //HttpWebClient<SendCutImageDto> hbc = sender as HttpWebClient<SendCutImageDto>;
                SendCutImageDto sucess = hbc.obj as SendCutImageDto;

                StringBuilder sbFileimg = new StringBuilder();
                sbFileimg.AppendLine("setFileImg('" + sucess.imgguid + "','" + fileShowImage.showImageHtmlPath("fail", "") + "');");
                if (IsIncognitoModelState)
                {
                    this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileimg.ToString());
                }
                else
                {
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileimg.ToString());
                }


                StringBuilder sbFileText = new StringBuilder();
                sbFileText.AppendLine("setFileText('" + sucess.textguid + "','上传失败');");
                if (IsIncognitoModelState)
                {
                    this.chromiumWebBrowserburn.EvaluateScriptAsync(sbFileText.ToString());
                }
                else
                {
                    this.chromiumWebBrowser.EvaluateScriptAsync(sbFileText.ToString());
                }

                //上传回调
                sucess.FailOrSucess.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.fail;
                updateFailMessage(sucess.FailOrSucess);
                //backFileUpload.Dispose();

                LogHelper.WriteError("[TalkGroupViewModel_Client_UploadFileCompleted]:" + ex.Message + ex.StackTrace);
            }
        }

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
                            int maxChatindex = 0;
                            if (IsIncognitoModelState)
                            {
                                //查询数据库最大chatindex
                                T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                                maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                            }
                            else
                            {
                                //查询数据库最大chatindex
                                T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                                maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, s_ctt.sessionId);
                            }
                            List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> lists = new List<AntSdkChatMsg.ChatBase>();
                            List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listsBurn = new List<AntSdkChatMsg.ChatBase>();
                            //构造入库
                            foreach (var listfile in upFileList)
                            {
                                if (!PublicTalkMothed.IsFileInUsing(listfile.localOrServerPath))
                                {
                                    FileDto fDto = new FileDto();
                                    SendMessageDto smg = new SendMessageDto();
                                    SendMessage_ctt smt = new SendMessage_ctt();
                                    if (_isShowBurn == "Collapsed")
                                    {
                                        smt.flag = 1;
                                    }
                                    smt.MsgType = AntSdkMsgType.ChatMsgFile;
                                    string name = listfile.fileName;
                                    fDto.fileUrl = listfile.localOrServerPath.Replace(@"\", "/");
                                    fDto.fileName = name;
                                    fDto.size = listfile.fileSize;
                                    fDto.fileExtendName = name.Substring(name.LastIndexOf("."), name.Length - name.LastIndexOf("."));
                                    smt.content = JsonConvert.SerializeObject(fDto);
                                    smt.companyCode = s_ctt.companyCode;
                                    smt.sourceContent = JsonConvert.SerializeObject(fDto);
                                    //smt.messageId = Guid.NewGuid().ToString();

                                    //2017-03-16 修改
                                    //smt.messageId = PublicTalkMothed.timeStampAndRandom();
                                    smt.messageId = listfile.messageId;

                                    smt.sendUserId = s_ctt.sendUserId;
                                    smt.sessionId = s_ctt.sessionId;
                                    smt.targetId = s_ctt.targetId;
                                    smg.ctt = smt;

                                    #region 发送成功 插入数据

                                    SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(smt));
                                    //TODO:AntSdk_Modify
                                    //sm_ctt.MTP = "3";
                                    sm_ctt.chatIndex = maxChatindex.ToString();
                                    sm_ctt.sendsucessorfail = 0;
                                    sm_ctt.SENDORRECEIVE = "1";
                                    //2017-03-07 添加
                                    sm_ctt.uploadOrDownPath = listfile.localOrServerPath.Replace(@"\", "/");
                                    if (IsIncognitoModelState)
                                    {
                                        listsBurn.Add(sm_ctt);
                                        //ThreadPool.QueueUserWorkItem(m => addGroupBurnData(sm_ctt));
                                    }
                                    else
                                    {
                                        lists.Add(sm_ctt);
                                        //ThreadPool.QueueUserWorkItem(m => addData(sm_ctt));
                                    }

                                    #endregion
                                }
                            }
                            var result = false;
                            if (IsIncognitoModelState)
                            {
                                T_Chat_Message_GroupBurnDAL t_chatBurn = new T_Chat_Message_GroupBurnDAL();
                                result = t_chatBurn.InsertBig(listsBurn);
                            }
                            else
                            {
                                T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                                result = t_chat.InsertBig(lists);
                            }
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
                                    sendFile.GroupInfo = this.GroupInfo;
                                    OnceSendMessage.GroupToGroup.OnceMsgList.Add(listfile.messageId, sendFile);
                                    if (IsIncognitoModelState)
                                    {
                                        //listfile.messageId = PublicTalkMothed.timeStampAndRandom();
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
                                        failMessage.mtp = (int)AntSdkMsgType.ChatMsgFile;
                                        failMessage.content = "";
                                        failMessage.sessionid = s_ctt.sessionId;
                                        //DateTime dt = DateTime.Now;
                                        failMessage.lastDatetime = dt.ToString();
                                        failMessage.IsBurnMsg = _isBurnMode == GlobalVariable.BurnFlag.IsBurn ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn;
                                        listfile.FailOrSucess = failMessage;

                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;

                                        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(listfile.messageId))
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId[listfile.messageId] = listfile.imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId.Add(listfile.messageId, listfile.imageSendingId);
                                        }
                                        updateFailMessage(failMessage);

                                        PublicTalkMothed.RightGroupBurnShwoSendFile(chromiumWebBrowserburn, listfile);

                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                                        arg.isGroup = false;
                                        arg.MessageId = listfile.messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowserburn;
                                        arg.SendIngId = listfile.imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == listfile.messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }
                                        listfile.ImgOrFileArg = arg;
                                        //SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                                        #endregion
                                    }
                                    else
                                    {
                                        //listfile.messageId = PublicTalkMothed.timeStampAndRandom();
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
                                        failMessage.mtp = (int)AntSdkMsgType.ChatMsgFile;
                                        failMessage.content = "";
                                        failMessage.sessionid = s_ctt.sessionId;
                                        //DateTime dt = DateTime.Now;
                                        failMessage.lastDatetime = dt.ToString();
                                        failMessage.IsBurnMsg = _isBurnMode == GlobalVariable.BurnFlag.IsBurn ? AntSdkburnMsg.isBurnMsg.yesBurn : AntSdkburnMsg.isBurnMsg.notBurn;
                                        listfile.FailOrSucess = failMessage;

                                        failMessage.IsSendSucessOrFail = AntSdkburnMsg.isSendSucessOrFail.sending;
                                        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(listfile.messageId))
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId[listfile.messageId] = listfile.imageSendingId;
                                        }
                                        else
                                        {
                                            SendMsgListMonitor.MsgIdAndImgSendingId.Add(listfile.messageId, listfile.imageSendingId);
                                        }
                                        updateFailMessage(failMessage);
                                        PublicTalkMothed.RightGroupShowSendFile(chromiumWebBrowser, listfile);

                                        #region 消息状态监控

                                        MessageStateArg arg = new MessageStateArg();
                                        arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                        arg.isGroup = false;
                                        arg.MessageId = listfile.messageId;
                                        arg.SessionId = s_ctt.sessionId;
                                        arg.WebBrowser = chromiumWebBrowser;
                                        arg.SendIngId = listfile.imageSendingId;
                                        arg.RepeatId = imageTipId;
                                        var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == listfile.messageId);
                                        if (IsHave != null)
                                        {
                                            SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                                        }
                                        listfile.ImgOrFileArg = arg;
                                        //SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                                        #endregion
                                    }

                                    #region 异步上传

                                    ThreadPool.QueueUserWorkItem(m => ThreadUploadFile(listfile));

                                    #endregion
                                }

                                FileUploadShowHeight = 0;
                                _wrapPanel.Children.Clear();
                                upFileList.Clear();
                                StringBuilder sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");
                                if (IsIncognitoModelState)
                                {
                                    var taskEnd = this.chromiumWebBrowserburn.EvaluateScriptAsync(sbEnd.ToString());
                                }
                                else
                                {
                                    var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                }
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
                        LogHelper.WriteError("[TalkGroupViewModel_btnSendUploadFile]:" + ex.Message + ex.StackTrace);
                    }
                });
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="cmr"></param>
        public void addData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase receive = cmr;
            BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL> t_chat = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL>();
            if (t_chat.Insert(cmr) == 1)
            {
            }
            else
            {
            }
        }

        /// <summary>
        /// 插入群聊阅后即焚数据
        /// </summary>
        /// <param name="cmr"></param>
        public void addGroupBurnData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase receive = cmr;
            BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupBurnDAL> t_chat = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupBurnDAL>();
            if (t_chat.Insert(cmr) == 1)
            {
            }
            else
            {
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
                        LogHelper.WriteError("[TalkGroupViewModel_btnCanelUpLoadFile]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }
        }

        #region js

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
                    LogHelper.WriteError("[TalkGroupViewModel_CallBackSelectDirecoryFile]:" + ex.Message + ex.Source + ex.Source);
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
                    LogHelper.WriteError("[TalkGroupViewModel_openDirecrory]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
        }

        public class CallbackValueById
        {
            public ChromiumWebBrowsers chromiumWebBrowser;
            private List<SendMsgStateMonitor> MessageStateMonitorList;

            public CallbackValueById(ChromiumWebBrowsers chromiumWebBrowser, List<SendMsgStateMonitor> MessageStateMonitorList)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.MessageStateMonitorList = MessageStateMonitorList;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_CallbackValueById]:" + ex.Message + ex.StackTrace + ex.Source);
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
                        string args = string.Format("/Select, {0}", fileToSelect);
                        ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                        System.Diagnostics.Process.Start(pfi);
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_GetValueById]:" + ex.Message + ex.StackTrace + ex.Source);
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
            /// <param name="isBurn"></param>
            /// <param name="isLeftOrRight">1为左边，0为右边</param>
            public void getVoiceParameter(bool isRead, string audioUrl, string isShowGifId, string messageid, bool isBurn, string isLeftOrRight)
            {
                if (string.IsNullOrEmpty(audioUrl)) return;
                //更新数据库语音状态 0未读 1已读
                if (!isRead)
                {
                    if (isBurn)
                    {
                        if (isLeftOrRight == "0")
                        {
                            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                            t_chat.UpdateVoiceState(messageid);
                        }
                    }
                    else
                    {
                        if (isLeftOrRight == "0")
                        {
                            T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                            t_chat.UpdateVoiceState(messageid);
                        }
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
                        player.Open(new Uri(audioUrl));
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
            public void sendMsgagen(string method, string messageId, string pathOrValue, string preValue, string atPreValues)
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
                               PublicTalkMothed.RightGroupShowSendText(chromiumWebBrowser, pathOrValue, null, messageIds, imageTipId, imageSendingId, dt, preValue);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                               arg.isGroup = true;
                               arg.MessageId = messageIds;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = imageSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                               if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                               }
                               else
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                               }

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)GlobalVariable.MsgType.Text;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               failMessage.isOnceSendMsg = true;
                               failMessage.targetId = chromiumWebBrowser.s_ctt.targetId;
                               updateFailMessage(failMessage);
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
                               PublicTalkMothed.RightGroupShowSendText(chromiumWebBrowser, pathOrValue, null, messageIds, imageTipId, imageSendingId, dt, preValue);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                               arg.isGroup = true;
                               arg.MessageId = messageIds;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = imageSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                               if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                               }
                               else
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                               }

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgText;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               updateFailMessage(failMessage);
                               //发送
                               sendTextMsg(preValue.Replace("<br/>", "\r\n"), messageIds, imageTipId, imageSendingId, failMessage, true);
                           }));

                            #endregion

                            break;
                        case "sendAtText":

                            #region At消息重发

                            App.Current.Dispatcher.Invoke((Action)(() =>
                           {
                               //移除
                               PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                               //显示
                               string messageIds = messageId;
                               string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                               string imageSendingId = "sending" + imageTipId;
                               DateTime dt = DateTime.Now;

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                               arg.isGroup = true;
                               arg.MessageId = messageIds;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = imageSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                               if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                               }
                               else
                               {
                                   SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                               }

                               #endregion

                               PublicTalkMothed.RightGroupShowSendAtText(chromiumWebBrowser, pathOrValue, null, messageIds, imageTipId, imageSendingId, dt, preValue, atPreValues);

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgText;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                               failMessage.lastDatetime = dt.ToString();
                               failMessage.isOnceSendMsg = true;
                               updateFailMessage(failMessage);
                               //发送
                               //object obj = PublicTalkMothed.Base64ToString(preValue);
                               object obj = null;
                               if (!IsBase64(atPreValues))
                               {
                                   obj = preValue;
                               }
                               else
                               {
                                   obj = PublicTalkMothed.Base64ToString(atPreValues);
                               }

                               sendAtTextMsg(preValue.Replace("<br/>", "\r\n"), messageId, imageTipId, imageSendingId, failMessage, "", obj);
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
                               string prePath = lists.Source.ToString().Replace(@"\", @"/");
                               string prePaths = prePath.Substring(8, prePath.Length - 8);
                               string fileFileName = System.IO.Path.GetFileNameWithoutExtension(prePaths);
                               string messageIds = messageId;
                               string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                               string imageSendingId = "sending" + imageTipId;
                               DateTime dt = DateTime.Now;
                               PublicTalkMothed.RightGroupShowSendImage(chromiumWebBrowser, lists, null, null, fileFileName, messageIds, imageTipId, imageSendingId, dt);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                               arg.isGroup = true;
                               arg.MessageId = messageIds;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = imageSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageIds);
                               if (IsHave != null)
                               {
                                   IsHave.dispatcherTimer.Stop();
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                               //if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(messageIds))
                               //{
                               //    SendMsgListMonitor.MsgIdAndImgSendingId[messageIds] = imageSendingId;
                               //}
                               //else
                               //{
                               //    SendMsgListMonitor.MsgIdAndImgSendingId.Add(messageIds, imageSendingId);
                               //}

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgPicture;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               failMessage.isOnceSendMsg = true;
                               failMessage.isSucessOrFail = preValue;
                               failMessage.obj = arg;
                               updateFailMessage(failMessage);
                               //上传
                               upLoadImageMsg(prePaths, fileFileName, messageIds, imageTipId, imageSendingId, failMessage);
                           }));

                            #endregion

                            break;
                        case "sendVoice":
                            #region 正常语音重发
                            CurrentChatDto currentChatVoice = new CurrentChatDto();
                            currentChatVoice.type = AntSdkchatType.Group;
                            currentChatVoice.messageId = messageId;
                            currentChatVoice.sendUserId = chromiumWebBrowser.s_ctt.sendUserId;
                            currentChatVoice.sessionId = chromiumWebBrowser.sessionid; ;
                            currentChatVoice.targetId = chromiumWebBrowser.s_ctt.targetId;
                            currentChatVoice.isOnceSend = true;

                            Application.Current.Dispatcher.Invoke((Action)(() =>
                           {
                               //移除
                               PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                               string sessionid = chromiumWebBrowser.sessionid;
                               MessageStateArg arg = new MessageStateArg();
                               arg.SessionId = sessionid;
                               upLoadVoiceMsg(pathOrValue, messageId, preValue, arg, chromiumWebBrowser, currentChatVoice);
                           }));

                            #endregion
                            break;
                        case "1":

                            #region 正常文件重发

                            App.Current.Dispatcher.BeginInvoke((Action)(() =>
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
                               PublicTalkMothed.RightGroupShowSendFile(chromiumWebBrowser, upFileDto);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                               arg.isGroup = false;
                               arg.MessageId = messageId;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = fileSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgFile;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               upFileDto.FailOrSucess = failMessage;
                               upFileDto.from = AntSdkSendFrom.SendFrom.OnceSend;
                               upFileDto.ImgOrFileArg = arg;
                               updateFailMessage(failMessage);
                               //重新上传文件
                               ThreadPool.QueueUserWorkItem(m => upLoadFileMsg(upFileDto));
                           }));

                            #endregion

                            break;
                        case "2":

                            #region 阅后即焚重发图片

                            App.Current.Dispatcher.Invoke((Action)(() =>
                           {
                               #region 阅后即焚图片重发

                               //移除
                               PublicTalkMothed.RemoveMsgDiv(chromiumWebBrowser, messageId);
                               //显示
                               Image listburn = new Image();
                               listburn.Source = new BitmapImage(new Uri(pathOrValue.Replace("file:///", "")));
                               string burnprePath = listburn.Source.ToString().Replace(@"\", @"/");
                               string burnprePaths = burnprePath.Substring(8, burnprePath.Length - 8);
                               string burnfileFileName = System.IO.Path.GetFileNameWithoutExtension(burnprePaths);
                               string burnmessageIds = messageId;
                               string burnimageTipId = Guid.NewGuid().ToString().Replace("-", "");
                               string burnimageSendingId = "sending" + burnimageTipId;
                               DateTime dt = DateTime.Now;
                               PublicTalkMothed.RightGroupBurnShowSendImage(chromiumWebBrowser, listburn, null, null, burnfileFileName, burnmessageIds, burnimageTipId, burnimageSendingId, dt);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                               arg.isGroup = true;
                               arg.MessageId = burnmessageIds;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = burnimageSendingId;
                               arg.RepeatId = burnimageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == burnmessageIds);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                               #endregion

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgPicture;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               failMessage.isOnceSendMsg = true;
                               failMessage.obj = arg;
                               updateFailMessage(failMessage);
                               //上传
                               upLoadImageMsg(burnprePaths, burnfileFileName, burnmessageIds, burnimageTipId, burnimageSendingId, failMessage);
                           }));

                            #endregion

                            break;
                        case "3":

                            #region 阅后即焚文件重发

                            App.Current.Dispatcher.BeginInvoke((Action)(() =>
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
                               PublicTalkMothed.RightGroupBurnShwoSendFile(chromiumWebBrowser, upFileDto);

                               #region 消息状态监控

                               MessageStateArg arg = new MessageStateArg();
                               arg.isBurn = AntSdkburnMsg.isBurnMsg.yesBurn;
                               arg.isGroup = false;
                               arg.MessageId = messageId;
                               arg.SessionId = chromiumWebBrowser.sessionid;
                               arg.WebBrowser = chromiumWebBrowser;
                               arg.SendIngId = fileSendingId;
                               arg.RepeatId = imageTipId;
                               var IsHave = MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == messageId);
                               if (IsHave != null)
                               {
                                   MessageStateMonitorList.Remove(IsHave);
                               }
                               //MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));

                               #endregion

                               //构造
                               AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                               failMessage.mtp = (int)AntSdkMsgType.ChatMsgFile;
                               failMessage.content = preValue;
                               failMessage.sessionid = chromiumWebBrowser.sessionid;
                               failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.yesBurn;
                               DateTime dtNow = dt;
                               failMessage.lastDatetime = dt.ToString();
                               failMessage.isOnceSendMsg = true;
                               upFileDto.FailOrSucess = failMessage;
                               upFileDto.from = AntSdkSendFrom.SendFrom.OnceSend;
                               upFileDto.ImgOrFileArg = arg;
                               updateFailMessage(failMessage);
                               //重新上传文件
                               ThreadPool.QueueUserWorkItem(m => upLoadFileMsg(upFileDto));
                           }));

                            #endregion

                            break;
                        case "sendMixPic":
                            #region 图文混排
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                //移除
                                // PublicTalkMothed.HiddenMsgDiv(chromiumWebBrowser, messageId);

                                //获取重发数据
                                var mixData = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == messageId);
                                List<MixMessageObjDto> obj = mixData.Value as List<MixMessageObjDto>;
                                var sss = JsonConvert.SerializeObject(obj);

                                MixMsg mixMsgClass = new MixMsg();
                                mixMsgClass.MessageId = messageId;
                                List<MixMessageTagDto> listTagDto = new List<MixMessageTagDto>();
                                string listShow = "";
                                foreach (var list in obj)
                                {
                                    switch (list.type)
                                    {
                                        case "1001":
                                            listShow += list.content.ToString();
                                            break;
                                        case "1002":
                                            MixMessageTagDto tagDto = new MixMessageTagDto();

                                            var contentImg = JsonConvert.DeserializeObject<PictureDto>(list.content?.ToString());
                                            var guidImgId = Guid.NewGuid().ToString();
                                            tagDto.PreGuid = guidImgId;
                                            tagDto.Path = contentImg.picUrl;
                                            listTagDto.Add(tagDto);
                                            //AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, guidImgId, contentImg.picUrl, "", burnMsg.isBurnMsg.notBurn, burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                            break;
                                        case "1008":
                                            var content = JsonConvert.DeserializeObject<List<AntSdkChatMsg.contentAtOrdinary>>(list.content.ToString());
                                            foreach (var listc in content)

                                                if (listc.type == "1112")
                                                {
                                                    #region @普通成员

                                                    foreach (var name in listc.names)
                                                    {
                                                        listShow += "@" + name;
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region @全体成员
                                                    listShow += "@全体成员";
                                                    #endregion
                                                }
                                            break;
                                    }
                                }
                                mixMsgClass.TagDto = listTagDto;
                                //string newMsgId = PublicTalkMothed.timeStampAndRandom();
                                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                                string imageSendingId = "sending" + imageTipId;
                                #region 消息状态监控
                                MessageStateArg arg = new MessageStateArg();
                                arg.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                                arg.isGroup = false;
                                arg.MessageId = messageId;
                                arg.SessionId = chromiumWebBrowser.sessionid;
                                arg.WebBrowser = chromiumWebBrowser;
                                arg.SendIngId = imageSendingId;
                                arg.RepeatId = imageTipId;
                                #endregion
                                CurrentChatDto currentChat = new CurrentChatDto();
                                currentChat.type = AntSdkchatType.Group;
                                currentChat.messageId = messageId;
                                currentChat.sendUserId = chromiumWebBrowser.s_ctt.sendUserId;
                                currentChat.sessionId = chromiumWebBrowser.sessionid; ;
                                currentChat.targetId = chromiumWebBrowser.s_ctt.targetId;
                                currentChat.isOnceSend = true;
                                GroupSendPicAndText.RightGroupSendPicAndTextMix(chromiumWebBrowser, messageId, obj, arg, mixMsgClass);
                                //构造
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)AntSdkMsgType.ChatMsgMixMessage;
                                failMessage.content = listShow;
                                failMessage.sessionid = chromiumWebBrowser.sessionid;
                                failMessage.IsBurnMsg = AntSdkburnMsg.isBurnMsg.notBurn;
                                string dtNow = DateTime.Now.ToString("HH:mm:ss");
                                failMessage.lastDatetime = dtNow;
                                failMessage.isOnceSendMsg = true;
                                failMessage.targetId = chromiumWebBrowser.s_ctt.targetId;
                                updateFailMessage(failMessage);
                                //滚动条置地
                                var sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");
                                chromiumWebBrowser.ExecuteScriptAsync(sbEnd.ToString());
                                sendMixPicAndText(AntSdkMsgType.ChatMsgMixMessage, currentChat, obj, arg, mixMsgClass);
                            }));
                            #endregion
                            break;
                        case "sendAtMixPic":
                            #region At图文混合消息
                            //获取重发数据
                            var mixAtData = OnceSendMessage.GroupToGroup.OnceMsgList.SingleOrDefault(m => m.Key == messageId);
                            List<PictureAndTextMixDto> listAtPicAndText = mixAtData.Value as List<PictureAndTextMixDto>;
                            List<PictureAndTextMixDto> picAtAndTxtMix = listAtPicAndText.Where(m => m.type == PictureAndTextMixEnum.Image).ToList();
                            //string newMsgId = PublicTalkMothed.timeStampAndRandom();
                            string imageAtTipId = Guid.NewGuid().ToString().Replace("-", "");
                            string imageAtSendingId = "sending" + imageAtTipId;
                            #region 消息状态监控
                            MessageStateArg argAt = new MessageStateArg();
                            argAt.isBurn = AntSdkburnMsg.isBurnMsg.notBurn;
                            argAt.isGroup = false;
                            argAt.MessageId = messageId;
                            argAt.SessionId = chromiumWebBrowser.sessionid;
                            argAt.WebBrowser = chromiumWebBrowser;
                            argAt.SendIngId = imageAtSendingId;
                            argAt.RepeatId = imageAtTipId;
                            #endregion
                            CurrentChatDto currentChatAt = new CurrentChatDto();
                            currentChatAt.type = AntSdkchatType.Group;
                            currentChatAt.messageId = messageId;
                            currentChatAt.sendUserId = chromiumWebBrowser.s_ctt.sendUserId;
                            currentChatAt.sessionId = chromiumWebBrowser.sessionid; ;
                            currentChatAt.targetId = chromiumWebBrowser.s_ctt.targetId;
                            currentChatAt.isOnceSend = true;
                            //GroupSendPicAndText.RightGroupSendAtPicAndTextMix(chromiumWebBrowser, messageId, listAtPicAndText, argAt);
                            //sendMixMessage(AntSdkMsgType.ChatMsgMixMessage, listAtPicAndText, currentChatAt, picAtAndTxtMix, argAt);
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            private static readonly HashSet<char> h = new HashSet<char>()
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/', '='
            };

            public static bool IsBase64(string s)
            {
                if (string.IsNullOrEmpty(s))
                    return false;
                else if (s.Any(c => !h.Contains(c)))
                    return false;

                try
                {
                    Convert.FromBase64String(s);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            public static event EventHandler sendMixMessageOnce;
            private void sendMixMessage(AntSdkMsgType type, CurrentChatDto currentChat, List<MixMessageObjDto> obj, MessageStateArg arg, MixMsg mixMsg)
            {
                if (sendMixMessageOnce != null)
                {
                    MixPicAndSenderArgs senderArgs = new MixPicAndSenderArgs();
                    senderArgs.type = type;
                    senderArgs.obj = obj;
                    senderArgs.currentChat = currentChat;
                    senderArgs.mixMsg = mixMsg;
                    senderArgs.arg = arg;
                    sendMixMessageOnce(senderArgs, null);
                }
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

            public static event EventHandler sendAtTextMsgOnce;

            private void sendAtTextMsg(string msgStr, string messageid, string imageTipId, string imageSendingId, AntSdkFailOrSucessMessageDto failMessage, string nullstr, object liststr)
            {
                if (sendAtTextMsgOnce != null)
                {
                    sendTextDto AtText = new sendTextDto();
                    AtText.msgStr = msgStr;
                    AtText.messageid = messageid;
                    AtText.imageTipId = imageTipId;
                    AtText.imageSendingId = imageSendingId;
                    AtText.FailOrSucess = failMessage;
                    AtText.AtArray = liststr;
                    sendAtTextMsgOnce(AtText, null);
                }
            }


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
                    send.imageSendingId = imageSendingId;
                    send.FailOrSucess = failOrSucess;
                    send.FailOrSucess.obj = failOrSucess.obj;
                    upLoadImageMsgOnce(send, null);
                }
            }

            /// <summary>
            /// 重发语音事件
            /// </summary>
            public static event EventHandler upLoadVoiceMsgOnce;

            private void upLoadVoiceMsg(string prePaths, string messageId, string preValue, MessageStateArg arg, ChromiumWebBrowsers cef, CurrentChatDto dto)
            {
                if (upLoadVoiceMsgOnce == null) return;
                var send = new SendCutImageDto
                {
                    prePaths = preValue,
                    messageId = messageId,
                    file = preValue,//本地地址
                    ImgOrFileArg = arg,
                    Cef = cef,
                    Dto = dto
                };
                upLoadVoiceMsgOnce(send, null);
            }

            /// <summary>
            /// 重新上传文件事件
            /// </summary>
            public static event EventHandler upLoadFileMsgOnce;

            private void upLoadFileMsg(UpLoadFilesDto file)
            {
                if (upLoadFileMsgOnce != null)
                {
                    upLoadFileMsgOnce(file, null);
                }
            }
        }

        public class CallbackObjectFilePathJs
        {
            public ChromiumWebBrowsers chromiumWebBrowser;
            SendMessage_ctt send;
            public CallbackObjectFilePathJs(ChromiumWebBrowsers chromiumWebBrowser, SendMessage_ctt s_ctt)
            {
                try
                {
                    this.chromiumWebBrowser = chromiumWebBrowser;
                    this.send = s_ctt;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_CallbackObjectFilePathJs]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            /// <summary>
            /// 打开文件夹 文件另存为
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="btnText"></param>
            public void GetFilePath(string obj, string btnText)
            {
                try
                {
                    ReceiveOrUploadFileDto roufd = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(obj);
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
                                    System.Windows.Forms.SaveFileDialog openFile = new System.Windows.Forms.SaveFileDialog();
                                    openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe ";
                                    if (string.IsNullOrEmpty(GlobalVariable.fileSavePath) || !Directory.Exists(GlobalVariable.fileSavePath))
                                        openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                    else
                                        openFile.InitialDirectory = GlobalVariable.fileSavePath;
                                    openFile.FilterIndex = 0;
                                    openFile.FileName = roufd.fileName;
                                    if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                    LogHelper.WriteError("[TalkGroupViewModel_GetFilePath]:" + ex.Message + ex.StackTrace + ex.Source);
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
                    if(e.ProgressPercentage==100)
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
                    if (dtos.flag == "1")
                    {
                        T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                        t_chat.updateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, downPath);
                    }
                    else
                    {
                        T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                        t_chat.updateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, downPath);
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
            private List<AddImgUrlDto> notBurnList;
            private List<AddImgUrlDto> yesBurnList;
            private ChromiumWebBrowsers notBurn;
            private SendMessage_ctt s_ctt;
            private bool IsIncognitoModelState;
            private AntSdkGroupInfo GroupInfo;
            private ChromiumWebBrowsers chromiumWebBrowser;
            private TalkGroupViewModel _talkGroupViewModel;

            public CallbackObjectForJs(BurnAndNotBurnImgList isBurnList, SendMessage_ctt s_ctt, bool IsIncognitoModelState, AntSdkGroupInfo groupInfo, ChromiumWebBrowsers chromiumWebBrowser, TalkGroupViewModel talkGroupViewModel = null)
            {
                _talkGroupViewModel = talkGroupViewModel;
                this.notBurnList = isBurnList.NotBurn;
                this.yesBurnList = isBurnList.YesBurn;
                this.s_ctt = s_ctt;
                this.IsIncognitoModelState = IsIncognitoModelState;
                this.GroupInfo = groupInfo;
                this.chromiumWebBrowser = chromiumWebBrowser;
            }
            /// <summary>
            /// 投票显示
            /// </summary>
            public void ShowVote(string voteId)
            {
                if (!string.IsNullOrEmpty(voteId))
                    _talkGroupViewModel?.GoVote(int.Parse(voteId));
            }
            /// <summary>
            /// 活动展示
            /// </summary>
            /// <param name="activityId"></param>
            public void showActivity(string activityId)
            {
                if (!string.IsNullOrEmpty(activityId))
                    _talkGroupViewModel?.ActivityListViewModel_CrateNoticeEvent(true, ActivityViewType.ActivityDetail, int.Parse(activityId));
            }
            public void ShowMessage(string id, string src, string title, string value, string sid, string mid)
            {
                try
                {
                    string index = "0";
                    if (value == "1")
                    {
                        if (yesBurnList != null)
                        {
                            AddImgUrlDto addImg = null;
                            if (!string.IsNullOrEmpty(mid))
                            {
                                addImg = yesBurnList.SingleOrDefault(m => m.ChatIndex == mid.Substring(1, mid.Length - 1));
                            }
                            else
                            {
                                addImg = yesBurnList.SingleOrDefault(m => m.ChatIndex == sid);
                            }
                            //AddImgUrlDto addImg = yesBurnList.SingleOrDefault(m => m.ChatIndex == sid);
                            //foreach (var item in yesBurnList)
                            //{
                            if (addImg != null)
                            {
                                index = addImg.ChatIndex;
                            }
                            //}
                        }
                    }
                    else
                    {
                        if (notBurnList != null)
                        {
                            AddImgUrlDto addImg = null;
                            if (!string.IsNullOrEmpty(mid))
                            {
                                addImg = notBurnList.SingleOrDefault(m => m.ChatIndex == mid.Substring(1, mid.Length - 1));
                            }
                            else
                            {
                                addImg = notBurnList.SingleOrDefault(m => m.ChatIndex == sid);
                            }
                            //AddImgUrlDto addImg = notBurnList.SingleOrDefault(m => m.ChatIndex == sid);
                            //foreach (var item in notBurnList)
                            //{
                            if (addImg != null)
                            {
                                index = addImg.ChatIndex;
                            }
                            //}
                        }
                    }
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
                               ShowImage(imagePath, GlobalVariable.BurnFlag.IsBurn, index, yesBurnList);
                           }
                           else
                           {
                               ShowImage(imagePath, GlobalVariable.BurnFlag.NotIsBurn, index, notBurnList);
                           }
                       }));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_ShowMessage]:" + ex.Message + ex.StackTrace);
                }
            }

            private void ShowImage(string path, GlobalVariable.BurnFlag flag, string currentIndex, List<AddImgUrlDto> imgUrl)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) return;
                    var index = imgUrl.FirstOrDefault(m => m.messageId == currentIndex);
                    if (index == null)
                    {
                        ImageHandle.PicView = new PictureViewerView();
                        ImageHandle.PicViewModel = new PictureViewerViewModel(path);
                        ImageHandle.PicView.DataContext = ImageHandle.PicViewModel;
                        ImageHandle.PicView.Show();
                        return;
                    }
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
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_ShowImage]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }

            public void imageMenuMethod(string caseMethod, string imageUrl)
            {
                if (string.IsNullOrEmpty(caseMethod))
                {
                    return;
                }
                else
                {
                    switch (caseMethod)
                    {
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
                        case "3":
                            ReCallMsg(imageUrl);
                            break;
                    }
                }
            }

            public void copydivContent(string caseMethod, string divid)
            {
                if (string.IsNullOrEmpty(caseMethod))
                {
                    return;
                }
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
                                   //PublicTalkMothed.CopyHtmlToClipBoard(divid);
                                   //PublicTalkMothed.SetDataToClipBoard(divid);
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
                            //OnReCallMsgClickEvent(divid);
                            ThreadPool.QueueUserWorkItem(m => ReCallMsg(divid));
                            break;
                    }
                }
            }

            public void ReCallMsg(string messageId)
            {
                T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
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
                                _talkGroupViewModel?.showTextMethod("", false);
                                return;
                            }
                        }
                    }
                    string errMsg = "";
                    AntSdkChatMsg.Revocation ReCall = new AntSdkChatMsg.Revocation();
                    string newMessageId = PublicTalkMothed.timeStampAndRandom();
                    ReCall.messageId = newMessageId;
                    AntSdkChatMsg.Revocation_content ReContent = new AntSdkChatMsg.Revocation_content();
                    ReCall.chatType = (int)AntSdkchatType.Group;
                    ReCall.MsgType = AntSdkMsgType.Revocation;
                    ReCall.os = (int)GlobalVariable.OSType.PC;
                    ReContent.messageId = messageId;
                    ReCall.content = ReContent;
                    ReCall.sendUserId = this.s_ctt.sendUserId;
                    ReCall.targetId = this.s_ctt.targetId;
                    ReCall.sessionId = this.s_ctt.sessionId;

                    int maxChatIndex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, this.s_ctt.sessionId);
                    //插入消息
                    SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase tempChatMsg = new AntSdkChatMsg.ChatBase();
                    tempChatMsg.MsgType = AntSdkMsgType.Revocation;
                    tempChatMsg.messageId = newMessageId;
                    tempChatMsg.SENDORRECEIVE = "1";
                    tempChatMsg.chatType = (int)AntSdkchatType.Group;
                    tempChatMsg.flag = IsIncognitoModelState ? 1 : 0;
                    tempChatMsg.os = (int)(int)GlobalVariable.OSType.PC;
                    tempChatMsg.sendUserId = this.s_ctt.sendUserId;
                    tempChatMsg.chatIndex = maxChatIndex.ToString();
                    tempChatMsg.targetId = GroupInfo?.groupId;
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
                            StringBuilder sbEnd = new StringBuilder();
                            sbEnd.AppendLine("setscross();");
                            var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        else
                        {
                            t_chat.DeleteByMessageId(s_ctt.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, newMessageId);
                        }
                    }
                }
            }

            /// <summary>
            /// 插入数据
            /// </summary>
            /// <param name="cmr"></param>
            public void addData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
            {
                SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase receive = cmr;
                BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL> t_chat = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL>();
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
                    App.Current.Dispatcher.Invoke((Action)(() => { ShowImage(id); }));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_ShowMessage]:" + ex.Message + ex.StackTrace);
                }
            }

            private void ShowImage(string id)
            {
                try
                {
                    //Win_UserInfoView win = new Win_UserInfoView();
                    //win.ShowInTaskbar = false;
                    //Win_UserInfoViewModel model = new Win_UserInfoViewModel(PublicTalkMothed.SubString(id));
                    //win.DataContext = model;
                    //win.Owner = Antenna.Framework.Win32.GetTopWindow();
                    //win.ShowDialog();
                    //PopUserInfoIsOpen = false;
                    //PopUserInfoIsOpen = true;

                    //string mm = PublicTalkMothed.SubString(id);

                    OnShowUserInfoEvent(PublicTalkMothed.SubString(id));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_ShowImage]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }

            public void executeMethod(string caseMethod, string userId)
            {
                if (string.IsNullOrEmpty(caseMethod))
                {
                    return;
                }
                else
                {
                    switch (caseMethod)
                    {
                        case "1":
                            // MessageBox.Show("发送消息");
                            App.Current.Dispatcher.Invoke((Action)(() => { OnMouseDoubleClickEvent(userId); }));
                            break;
                        case "2":
                            App.Current.Dispatcher.Invoke((Action)(() => { insertAt(userId); }));
                            break;
                    }
                }
            }

            /// <summary>
            /// 插入@
            /// </summary>
            /// <param name="userId"></param>
            public void insertAt(string userId)
            {
                AntSdkGroupMember userinfo = ceBrowser.GroupMembers.SingleOrDefault(m => m.userId == PublicTalkMothed.SubString(userId));
                PublicTalkMothed.InsertAtBlock(ceBrowser.richTextBox, userinfo.userName, userinfo.userId);
            }

            public static event EventHandler CreateSessionEvent;

            private void OnMouseDoubleClickEvent(string userId)
            {
                if (CreateSessionEvent != null)
                {
                    CreateSessionEvent(PublicTalkMothed.SubString(userId), null);
                }
            }
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
                    LogHelper.WriteError("[TalkGroupViewModel_CallbackObjectUrlJs]:" + ex.Message + ex.StackTrace + ex.Source);
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
                    LogHelper.WriteError("[TalkGroupViewModel_CallbackObjectUrlJs]:" + ex.Message + ex.StackTrace);
                }
            }
            public void DownUrl(string obj, string btnText)
            {
                try
                {
                    switch (btnText)
                    {
                        case "接收":
                            #region 接收
                            if (!Directory.Exists(publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download"))
                            {
                                Directory.CreateDirectory(publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download");
                            }
                            ReceiveOrUploadFileDto roufd = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(obj);
                            //判断是否有相同文件名的文件
                            string preString = publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download\\";
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
                                    path = publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download\\" + roufds.fileName;
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
                    LogHelper.WriteError("[TalkGroupViewModel_DownUrl]:" + ex.Message + ex.StackTrace);
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
                    webclient.DownloadFileAsync(new Uri(receive.fileUrl), publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download\\" + receive.fileName);
                    webclient.DownloadProgressChanged += Webclient_DownloadProgressChanged;
                    webclient.DownloadFileCompleted += Webclient_DownloadFileCompleted;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_downFile]:" + ex.Message + ex.StackTrace);
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
                    LogHelper.WriteError("[TalkGroupViewModel_Webclient_DownloadProgressChanged]:" + ex.Message + ex.StackTrace);
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
                    string downPath = (publicMethod.localDataPath() + send.companyCode + "\\" + send.sendUserId + "\\group\\download\\" + dtos.fileName).Replace(@"\", @"/");
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
                    if (dtos.flag == "1")
                    {
                        T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
                        t_chat.updateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, downPath);
                    }
                    else
                    {
                        T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                        t_chat.updateFilePathAndFlag(chromiumWebBrowser.sessionid, GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, dtos.messageId, downPath);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[TalkGroupViewModel_Webclient_DownloadFileCompleted]:" + ex.Message + ex.StackTrace);
                }
            }
        }

        #endregion

        private string scrollPostion = "";
        private object objs = new object();

        #region 接收消息

        /// <summary>
        /// 隐藏阅后即焚消息和在界面插入一条新撤回消息
        /// </summary>
        /// <param name="messageid"></param>
        public void HideMsgAndShowRecallMsg(string messageid)
        {
            var taskHide = chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.hideDivById(messageid));
            taskHide.Wait();
            StringBuilder sbEnd = new StringBuilder();
            sbEnd.AppendLine("setscross();");
            var taskEnd = this.chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
            //var taskShow = chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.InsertUIRecallMsg(name));
            //taskShow.Wait();
        }

        public void receiveMsg(int mtp, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            try
            {
                Task<JavascriptResponse> task = null;
                lock (objs)
                {
                    if (msg.flag == 1)
                    {
                        task = chromiumWebBrowserburn.EvaluateScriptAsync("getScroolPosition();");
                        task.Wait();
                        if (task.Result.Success)
                        {
                            if ((bool)task.Result.Result == true)
                            {
                                msg.IsSetImgLoadComplete = true;
                            }
                        }
                        BurnListChatIndex.Add(msg.messageId);
                    }
                    else
                    {
                        task = chromiumWebBrowser.EvaluateScriptAsync("getScroolPosition();");
                        task.Wait();
                        if (task.Result.Success)
                        {
                            if ((bool)task.Result.Result == true)
                            {
                                msg.IsSetImgLoadComplete = true;
                            }
                        }
                        listChatIndex.Add(msg.messageId);
                    }
                    //获取接收者头像
                    string pathImages = "";
                    //获取接收者头像
                    var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
                    AntSdkGroupMember user = GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId);
                    if (listUser == null)
                    {
                        if (user == null)
                        {
                            AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
                            if (cus == null)
                            {
                                user = new AntSdkGroupMember();
                                pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                                user.picture = pathImages;
                                user.userName = "离职人员";
                            }
                            else
                            {
                                user = new AntSdkGroupMember();
                                user.userNum = cus.userNum;
                                user.userName = cus.userName;
                                user.userId = cus.userId;
                                user.position = cus.position;
                                user.picture = cus.picture;
                            }
                        }
                    }
                    switch (mtp)
                    {
                        case (int)AntSdkMsgType.ChatMsgMixMessage:

                            #region 图文混合消息
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                List<string> imageId = new List<string>();
                                imageId.Clear();
                                #region 图文混合
                                List<MixMessageObjDto> receives = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent).Where(m => m.type == "1002").ToList();
                                foreach (var ilist in receives)
                                {
                                    string imgId = "RL" + Guid.NewGuid().ToString();
                                    imageId.Add(imgId);
                                    PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, content.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                }
                                #endregion
                                GroupSendPicAndText.RightGroupShowPicAndTextMix(chromiumWebBrowser, msg, imageId);
                            }
                            else
                            {
                                List<string> imageId = new List<string>();
                                imageId.Clear();
                                #region 图文混合
                                List<MixMessageObjDto> receives = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent).Where(m => m.type == "1002").ToList();
                                foreach (var ilist in receives)
                                {
                                    string imgId = "RL" + Guid.NewGuid().ToString();
                                    imageId.Add(imgId);
                                    PictureAndTextMixContentDto content = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(ilist.content?.ToString());
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, content.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                }
                                #endregion
                                GroupSendPicAndText.LeftGroupShowPicAndTextMix(chromiumWebBrowser, msg, GroupMembers, imageId);
                            }
                            #endregion

                            break;
                        case (int)AntSdkMsgType.ChatMsgText:

                            #region  文本消息

                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                if (msg.flag == 1)
                                {
                                    PublicTalkMothed.RightGroupBurnShowText(chromiumWebBrowserburn, msg);
                                    LogHelper.WriteDebug("[RightGroupBurnShowText:]" + /*TODO:AntSdk_Modify:list.content*/ msg.sourceContent + "msg.flag:" + msg.flag + "msg.typeBurnMsg:" + msg.typeBurnMsg);
                                }
                                else
                                {
                                    PublicTalkMothed.rightShowText(chromiumWebBrowser, msg);
                                    LogHelper.WriteDebug("[rightShowText:]" + /*TODO:AntSdk_Modify:list.content*/ msg.sourceContent);
                                }
                            }
                            else
                            {
                                if (msg.typeBurnMsg == 0)
                                {
                                    if (msg.flag == 1)
                                    {
                                        PublicTalkMothed.leftGroupBurnShowText(chromiumWebBrowserburn, msg, GroupMembers);
                                        LogHelper.WriteDebug("[leftGroupBurnShowText:]" + /*TODO:AntSdk_Modify:list.content*/ msg.sourceContent + "msg.flag:" + msg.flag + "msg.typeBurnMsg:" + msg.typeBurnMsg);
                                    }
                                    else
                                    {
                                        PublicTalkMothed.leftGroupShowText(chromiumWebBrowser, msg, GroupMembers);
                                        LogHelper.WriteDebug("[leftGroupShowText:]" + /*TODO:AntSdk_Modify:list.content*/ msg.sourceContent);
                                    }
                                }
                                else
                                {
                                    PublicTalkMothed.leftGroupBurnShowText(chromiumWebBrowserburn, msg, GroupMembers);
                                    LogHelper.WriteDebug("[leftGroupBurnShowText:]" + /*TODO:AntSdk_Modify:list.content*/ msg.sourceContent);
                                }
                            }

                            #endregion

                            break;
                        case (int)AntSdkMsgType.ChatMsgPicture:

                            #region 图片消息

                            #region 2013-07-09 构造时间差

                            //if (dtR.Year == 1)
                            //{
                            //    dtR = DataConverter.GetTimeByTimeStamp(msg.sendTime);
                            //    lastShowTime = preTime;
                            //    firstTime = preTime;
                            //    //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime(Convert.ToDateTime(dtR)));
                            //    this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.newShowCenterTime(DataConverter.FormatTimeByTimeStamp(msg.sendTime)));
                            //    //// Thread.Sleep(50);
                            //}
                            //else
                            //{
                            //    DateTime dt = DataConverter.GetTimeByTimeStamp(msg.sendTime);
                            //    if (PublicTalkMothed.showTimeReceive(dt, dtR))
                            //    {
                            //        //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime(dt));
                            //        this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.newShowCenterTime(DataConverter.FormatTimeByTimeStamp(msg.sendTime)));
                            //        //// Thread.Sleep(50);
                            //    }
                            //    dtR = dt;
                            //}

                            #endregion

                            #region 插入数据

                            //插入数据之前构造
                            //msg.MTP = "2";
                            //msg.SENDORRECEIVE = "0";
                            //msg.sendUserId = s_ctt.sendUserId;
                            //插入数据  
                            //ThreadPool.QueueUserWorkItem(m => addData(msg));

                            #endregion

                            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:list.content*/msg.sourceContent);
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                if (msg.flag == 1)
                                {
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn, msg.messageId, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    PublicTalkMothed.RightGroupBurnShowImage(chromiumWebBrowserburn, msg);
                                }
                                else
                                {
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, msg.messageId, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    PublicTalkMothed.rightShowImage(chromiumWebBrowser, msg);
                                }
                            }
                            else
                            {
                                if (msg.typeBurnMsg == 0)
                                {
                                    if (msg.flag == 1)
                                    {
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn, msg.chatIndex, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                        PublicTalkMothed.LeftGroupBurnShowImage(chromiumWebBrowserburn, msg, GroupMembers);
                                    }
                                    else
                                    {
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, msg.messageId, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                        PublicTalkMothed.LeftGroupShowImage(chromiumWebBrowser, msg, GroupMembers);
                                    }
                                }
                                else
                                {
                                    AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.yesBurn, msg.chatIndex, rimgDto.picUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                    PublicTalkMothed.LeftGroupBurnShowImage(chromiumWebBrowserburn, msg, GroupMembers);
                                }
                            }

                            #endregion

                            break;
                        case (int)AntSdkMsgType.ChatMsgFile:

                            #region 文件消息

                            #region 2017-03-09 构造时间差

                            //if (dtR.Year == 1)
                            //{
                            //    dtR = DataConverter.GetTimeByTimeStamp(msg.sendTime);
                            //    lastShowTime = preTime;
                            //    firstTime = preTime;
                            //    //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime(Convert.ToDateTime(dtR)));
                            //    this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.newShowCenterTime(DataConverter.FormatTimeByTimeStamp(msg.sendTime)));
                            //    //// Thread.Sleep(50);
                            //}
                            //else
                            //{
                            //    DateTime dt = DataConverter.GetTimeByTimeStamp(msg.sendTime);
                            //    if (PublicTalkMothed.showTimeReceive(dt, dtR))
                            //    {
                            //        //this.chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showCenterTime(dt));
                            //        this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.newShowCenterTime(DataConverter.FormatTimeByTimeStamp(msg.sendTime)));
                            //        ////Thread.Sleep(50);
                            //    }
                            //    dtR = dt;
                            //}

                            #endregion

                            #region 插入数据

                            //插入数据之前构造
                            //msg.MTP = "3";
                            //msg.SENDORRECEIVE = "0";
                            //msg.sendUserId = s_ctt.sendUserId;
                            //插入数据
                            //ThreadPool.QueueUserWorkItem(m => addData(msg));

                            #endregion

                            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>( /*TODO:AntSdk_Modify:list.content*/msg.sourceContent);
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                if (msg.flag == 1)
                                {
                                    PublicTalkMothed.RightGroupBurnShowFile(chromiumWebBrowserburn, msg);
                                }
                                else
                                {
                                    PublicTalkMothed.rightShowFile(chromiumWebBrowser, msg);
                                }
                            }
                            else
                            {
                                if (msg.typeBurnMsg == 0)
                                {
                                    if (msg.flag == 1)
                                    {
                                        PublicTalkMothed.LeftGroupBurnShowFile(chromiumWebBrowserburn, msg, GroupMembers);
                                    }
                                    else
                                    {
                                        PublicTalkMothed.LeftGroupShowFile(chromiumWebBrowser, msg, GroupMembers);
                                    }
                                }
                                else
                                {
                                    PublicTalkMothed.LeftGroupBurnShowFile(chromiumWebBrowserburn, msg, GroupMembers);
                                }
                            }

                            #endregion

                            break;
                        case (int)AntSdkMsgType.ChatMsgAudio:

                            #region  MP3

                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                if (msg.typeBurnMsg == 0)
                                {
                                    if (msg.flag == 1)
                                    {
                                        PublicTalkMothed.RightGroupBurnShowVoice(chromiumWebBrowserburn, msg);
                                    }
                                    else
                                    {
                                        PublicTalkMothed.rightShowVoice(this.chromiumWebBrowser, msg);
                                    }
                                }
                                else
                                {
                                    PublicTalkMothed.RightGroupBurnShowVoice(chromiumWebBrowserburn, msg);
                                }
                            }
                            else
                            {
                                if (msg.typeBurnMsg == 0)
                                {
                                    if (msg.flag == 1)
                                    {
                                        PublicTalkMothed.LeftGroupBurnShowVoice(this.chromiumWebBrowserburn, msg, GroupMembers);
                                    }
                                    else
                                    {
                                        PublicTalkMothed.LeftGroupShowVioce(this.chromiumWebBrowser, msg, GroupMembers);
                                    }
                                }
                                else
                                {
                                    PublicTalkMothed.LeftGroupBurnShowVoice(this.chromiumWebBrowserburn, msg, GroupMembers);
                                }
                            }

                            #endregion

                            break;
                        case (int)AntSdkMsgType.ChatMsgAt:

                            #region @消息

                            var atDto = /*TODO:AntSdk_Modify:list.content*/ msg as AntSdkChatMsg.At;
                            string showContent = "";
                            //构造展示消息
                            foreach (var str in atDto.content)
                            {
                                switch (str.type)
                                {
                                    //文本
                                    case "1001":
                                        showContent += PublicTalkMothed.talkContentReplace(str.content);
                                        break;
                                    //@全体成员
                                    case "1111":
                                        showContent += "@全体成员";
                                        break;
                                    //@个人
                                    case "1112":
                                        string strAt = "";
                                        if (str.ids.Count() > 1)
                                        {
                                            foreach (var name in str.names)
                                            {
                                                strAt += "@" + name[0];
                                            }
                                        }
                                        else
                                        {
                                            strAt += "@" + str.names[0];
                                        }
                                        showContent += strAt;
                                        break;
                                    //换行
                                    case "0000":
                                        showContent += "<br/>";
                                        break;
                                    //图片
                                    case "1002":
                                        PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(str.content);
                                        string ImgUrl = pictureAndTextMix.picUrl;
                                        string imgId = "L" + Guid.NewGuid().ToString();
                                        AddDictImgUrl(AddImgUrlDto.InsertPreOrEnd.End, burnMsg.isBurnMsg.notBurn, imgId, ImgUrl, "", burnMsg.IsReadImg.read, burnMsg.IsEffective.effective);
                                        showContent += "<img id=\"" + imgId + "\" src=\"" + ImgUrl + "\" class=\"imgLeftProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>";
                                        break;
                                }
                            }
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                //AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(list.content);
                                //TODO: AntSdk_Modify AT 消息
                                PublicTalkMothed.RightGroupShowAtext(chromiumWebBrowser, msg, GroupMembers, showContent);
                            }
                            else
                            {
                                //1、先看type=1111的消息  如果有 直接显示@提示  
                                //2、看type=1112的消息 如若有 显示@提示
                                //3、如果没有就正常显示
                                var isHaveAtAll = atDto.content.SingleOrDefault(m => m.type == "1111");
                                if (isHaveAtAll != null)
                                {
                                    var person = GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId);
                                    if (person != null)
                                    {
                                        IsbtnTipShow = "Visible";
                                        SetContents = person.userName + "@了我";
                                        scrollPostion = msg.chatIndex;
                                    }
                                }
                                else
                                {
                                    var listAtPerson = atDto.content.Where(m => m.type == "1112").ToList();
                                    foreach (var list in listAtPerson)
                                    {
                                        if (list.ids.Count() > 1)
                                        {
                                            foreach (var arry in list.ids)
                                            {
                                                if ( /*TODO:AntSdk_Modify:ids.id*/arry.ToString() == AntSdkService.AntSdkCurrentUserInfo.userId)
                                                {
                                                    var person = GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId);
                                                    if (person != null)
                                                    {
                                                        IsbtnTipShow = "Visible";
                                                        SetContents = person.userName + "@了我";
                                                        scrollPostion = msg.chatIndex;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if ( /*TODO:AntSdk_Modify:ids.id*/list.ids[0].ToString() == AntSdkService.AntSdkCurrentUserInfo.userId)
                                            {
                                                var person = GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId);
                                                if (person != null)
                                                {
                                                    IsbtnTipShow = "Visible";
                                                    SetContents = person.userName + "@了我";
                                                    scrollPostion = msg.chatIndex;
                                                }
                                            }
                                        }
                                    }
                                }
                                PublicTalkMothed.leftGroupShowAtText(chromiumWebBrowser, msg, GroupMembers, showContent);
                            }

                            #endregion

                            break;
                        case (int)AntSdkMsgType.Revocation:

                            #region 撤销消息提示

                            chromiumWebBrowser.ExecuteScriptAsync(PublicTalkMothed.InsertUIRecallMsg(msg.sourceContent));

                            #endregion

                            break;
                        case (int)AntSdkMsgType.CreateVote:

                            #region 投票
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                GroupVote.RightGroupSendVote(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                GroupVote.LeftGroupShowVote(chromiumWebBrowser, msg, GroupMembers);
                            }
                            #endregion

                            break;
                        case (int)AntSdkMsgType.CreateActivity:
                            #region 活动
                            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                            {
                                GroupActivity.RightGroupSendActivity(chromiumWebBrowser, msg);
                            }
                            else
                            {
                                GroupActivity.LeftGroupShowActivity(chromiumWebBrowser, msg, GroupMembers);
                            }
                            #endregion
                            break;
                    }

                    #region 滚动条置底

                    StringBuilder sbEnd = new StringBuilder();
                    sbEnd.AppendLine("setscross();");
                    if (IsIncognitoModelState)
                    {
                        if (task.Result.Success)
                        {
                            if ((bool)task.Result.Result == true)
                            {
                                chromiumWebBrowserburn.EvaluateScriptAsync("setscross();");
                            }
                        }
                    }
                    else
                    {
                        if (task.Result.Success)
                        {
                            if ((bool)task.Result.Result == true)
                            {
                                chromiumWebBrowser.EvaluateScriptAsync("setscross();");
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkGroupViewModel_receiveMsg]" + ex.Message + ex.StackTrace);
            }
        }

        #endregion

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
                        LogHelper.WriteError("[TalkGroupViewModel_btnSetShortCuts]:" + ex.Message + ex.StackTrace);
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
                LogHelper.WriteError("[TalkGroupViewModel_Btn_MouseLeftButtonDown]:" + ex.Message + ex.StackTrace);
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

        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="tipsMsg"></param>
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

        private UserInfoViewModel _UserInfoControl;

        public UserInfoViewModel UserInfoControl
        {
            get { return _UserInfoControl; }
            set
            {
                _UserInfoControl = value;
                RaisePropertyChanged(() => UserInfoControl);
            }
        }

        //处理阅后即焚切换
        public void SwitchBurnMode()
        {
            TextShowRowHeight = "0";
            TextShowReceiveMsg = "";
            AntSdkGroupMember isUserMember = GroupMembers.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            if (isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.GroupOwner && isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.Admin)
            {
                isShowExit = "Collapsed";
                isShowDelete = "Collapsed";
            }
            else
            {
                isShowExit = "Visible";
                isShowDelete = "Visible";
                isShowAdminMenu(false);
            }
            _isBurnMode = GlobalVariable.BurnFlag.IsBurn;
            isShowBurn = "Collapsed";
            isShowEmoji = "Collapsed";
            isShowSound = Visibility.Collapsed;
            isShowCutImage = "Collapsed";
            NoticeVisibility = Visibility.Collapsed;
            //if (this.chromiumWebBrowser != null)
            //{
            //    this.chromiumWebBrowser.Dispose();
            //    this.chromiumWebBrowser = null;
            //}
            //InitTalkBurn();
            if (chromiumWebBrowserburn != null)
                chromiumWebBrowserburn.Visibility = Visibility.Visible;
            if (chromiumWebBrowser != null)
                chromiumWebBrowser.Visibility = Visibility.Collapsed;
            isShowWinMsg = "Collapsed";
            IsIncognitoModelState = true;
            isShowBurnWinMsg = "Visible";
            //timerBurn.Interval = TimeSpan.FromMilliseconds(50);
            //timerBurn.Tick += TimerBurn_Tick; ;
            //timerBurn.Start();
        }

        /// <summary>
        /// 切换正常模式
        /// </summary>
        public void SwitchNotBurnMode()
        {
            var isUserMember = GroupMembers.FirstOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            if (isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.GroupOwner && isUserMember?.roleLevel != (int)GlobalVariable.GroupRoleLevel.Admin)
            {
                isShowBurn = "Collapsed";
            }
            else
            {
                isShowBurn = "Visible";
            }
            TextShowRowHeight = "0";
            TextShowReceiveMsg = "";
            //if (this.chromiumWebBrowserburn != null)
            //{
            //    this.chromiumWebBrowserburn.Dispose();
            //    this.chromiumWebBrowserburn = null;
            //}
            //InitTalk();
            _isBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
            isShowAdminMenu(true);
            isShowEmoji = "Visible";
            isShowSound = Visibility.Visible;
            isShowCutImage = "Visible";
            NoticeVisibility = Visibility.Visible;
            isShowExit = "Collapsed";
            IsIncognitoModelState = false;
            isShowDelete = "Collapsed";
            isShowWinMsg = "Visible";
            if (chromiumWebBrowserburn != null)
                chromiumWebBrowserburn.Visibility = Visibility.Collapsed;
            if (chromiumWebBrowser != null)
                chromiumWebBrowser.Visibility = Visibility.Visible;
            isShowBurnWinMsg = "Collapsed";
        }

        public static event EventHandler isShowTransferAdminMenu;

        private void isShowAdminMenu(bool b)
        {
            isShowTransferAdminMenu?.Invoke(b, null);
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