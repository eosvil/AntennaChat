/*
Author: tanqiyan
Crate date: 2017-06-14
Description：消息Session控制类
--------------------------------------------------------------------------------------------------------
Versions：
V1.00 2017-06-14 tanqiyan 描述：会话未在进行中的其它会话消息处理（缓存、计数）
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel;
using AntennaChat.ViewModel.Contacts;
using AntennaChat.ViewModel.Talk;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;

namespace AntennaChat.Helper
{
    public class SessionMonitor
    {
        static readonly BaseBLL<AntSdkTsession, T_SessionDAL> t_sessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
        //static readonly BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupBurnDAL> t_chatGroupBurnMsg = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupBurnDAL>();
        //static readonly BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_MessageDAL> t_chatMsg = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_MessageDAL>();
        //static readonly BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL> t_chatGroupMsg = new BaseBLL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase, T_Chat_Message_GroupDAL>();
        static readonly T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
        static readonly T_Chat_Message_GroupBurnDAL t_chatGroupBurnMsg = new T_Chat_Message_GroupBurnDAL();
        static readonly T_Chat_MessageDAL t_chatMsg = new T_Chat_MessageDAL();
        static readonly T_Chat_Message_GroupDAL t_chatGroupMsg = new T_Chat_Message_GroupDAL();
        private static readonly T_Chat_Message_GroupDAL t_groupChat = new T_Chat_Message_GroupDAL();
        #region 静态开放属性：未处理消息列表

        private static Dictionary<string, GlobalVariable.BurnFlag, MessageInfo> _dicWaitingToReceiveOfflineMsgNum = new Dictionary<string, GlobalVariable.BurnFlag, MessageInfo>();

        /// <summary>
        /// 当前联系人尚未处理的所有离线消息的WindowID及未读消息数集合
        /// </summary>
        public static Dictionary<string, GlobalVariable.BurnFlag, MessageInfo> WaitingToReceiveOfflineMsgNum
        {
            get { return _dicWaitingToReceiveOfflineMsgNum; }
            set { _dicWaitingToReceiveOfflineMsgNum = value; }
        }

        private static SessionListViewModel _SessionListViewModel;

        public static SessionListViewModel SessionListViewModel
        {
            get { return _SessionListViewModel; }
            set { _SessionListViewModel = value; }
        }

        public static GroupListViewModel GroupListViewModel { get; set; }
        public static MainWindowViewModel MainWindowVM { get; set; }

        private static List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> _lstWaitingToReceiveOnlineMessageList;
        /// <summary>
        /// 系统中当前联系人尚未处理的所有错过的即时消息列表
        /// </summary>
        public static List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> WaitingToReceiveOnlineMessageList
        {
            get { return _lstWaitingToReceiveOnlineMessageList; }
            set { _lstWaitingToReceiveOnlineMessageList = value; }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// 静态构造方法
        /// </summary>
        static SessionMonitor()
        {

        }

        #endregion

        #region Function

        /// <summary>
        /// 开始监控
        /// </summary>
        public static void Start()
        {
            _dicWaitingToReceiveOfflineMsgNum = new Dictionary<string, GlobalVariable.BurnFlag, MessageInfo>();
            WaitingToReceiveOnlineMessageList = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            MessageMonitor.InstantMessageWaitingToReceive += MessageMonitor_InstantMessageWaitingToReceive;
            MessageMonitor.InstantSystemMessageWaitingToReceive += MessageMonitor_InstantSystemMessageWaitingToReceive;
            MessageMonitor.InstantMassMsgWaitingToReceive += MessageMonitor_InstantMassMsgWaitingToReceive;
            MessageMonitor.InstatantPhoneReadSessionToReceive += MessageMonitor_InstatantPhoneReadSessionToReceive;
            MessageMonitor.InstatntMsessageDestroyToReceive += MessageMonitor_InstatntMsessageDestroyToReceive;
        }

        /// <summary>
        /// 结束监控
        /// </summary>
        public static void End()
        {
            MessageMonitor.InstantMessageWaitingToReceive -= MessageMonitor_InstantMessageWaitingToReceive;
            MessageMonitor.InstantSystemMessageWaitingToReceive -= MessageMonitor_InstantSystemMessageWaitingToReceive;
            MessageMonitor.InstantMassMsgWaitingToReceive -= MessageMonitor_InstantMassMsgWaitingToReceive;
            MessageMonitor.InstatantPhoneReadSessionToReceive -= MessageMonitor_InstatantPhoneReadSessionToReceive;
            MessageMonitor.InstatntMsessageDestroyToReceive -= MessageMonitor_InstatntMsessageDestroyToReceive;
            _dicWaitingToReceiveOfflineMsgNum = null;
            WaitingToReceiveOnlineMessageList = null;
            GroupListViewModel = null;
            SessionListViewModel = null;
            MainWindowVM = null;
            GroupListViewModel = null;
        }

        /// <summary>
        /// 消息记录已销毁(阅后即焚)
        /// </summary>
        private static void MessageMonitor_InstatntMsessageDestroyToReceive(AntSdkChatMsg.ChatBase msg, AntSdkMsgType msgType)
        {
            string messageId = msg.messageId;
            var onlineMsg = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.messageId == messageId);
            if (onlineMsg != null)
                WaitingToReceiveOnlineMessageList.Remove(onlineMsg);
            _SessionListViewModel.HandleBurnAfterReadReceipt(msg, msgType, true);
        }
        /// <summary>
        /// 阅后即焚消息，离线之后已读（专用场景：服务断开重连）
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgType"></param>
        public static void OfflineMsgBurnAfterReadReceipt(AntSdkChatMsg.ChatBase msg, AntSdkMsgType msgType)
        {
            _SessionListViewModel.HandleBurnAfterReadReceipt(msg, msgType, true);
        }

        /// <summary>
        /// 手机已读消息
        /// </summary>
        /// <param name="obj"></param>
        private static void MessageMonitor_InstatantPhoneReadSessionToReceive(AntSdkReceivedOtherMsg.MultiTerminalSynch obj)
        {

            var burnFlag = obj.flag;
            if (!obj.sessionId.StartsWith("G"))
                burnFlag = 0;
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase onlineMsg = null;
            onlineMsg = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.chatIndex == obj.chatIndex && m.flag == obj.flag && m.messageId == obj.messageId);
            if (onlineMsg != null)
                WaitingToReceiveOnlineMessageList.Remove(onlineMsg);
            else
            {
                onlineMsg = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.chatIndex == obj.chatIndex && m.flag == obj.flag);
                if (onlineMsg != null)
                    onlineMsg.status = obj.status;
            }
            GlobalVariable.BurnFlag flag = GlobalVariable.BurnFlag.NotIsBurn;
            if (burnFlag == 1)
                flag = GlobalVariable.BurnFlag.IsBurn;
            RemoveWaitingToReceiveOfflineMsgRecord(obj.sessionId, flag);
            _SessionListViewModel.HandlePhoneReadSession(obj);

        }
        /// <summary>
        /// 消息助手信息
        /// </summary>
        private static void MessageMonitor_InstantMassMsgWaitingToReceive(AntSdkChatMsg.ChatBase massMsg)
        {
            LogHelper.WriteWarn("---------------------------消息助手群发消息发送成功的回执chatIndex:" + massMsg.chatIndex + "---------------------");
            _SessionListViewModel.HandleMassMsgReceipt(massMsg);
        }

        /// <summary>
        /// 处理没有窗体接收的到达信息
        /// </summary>
        /// <param name="chatMsg">活动的即时消息</param>
        private static void MessageMonitor_InstantMessageWaitingToReceive(AntSdkMsgType msgType, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg)
        {

            if (msgType == AntSdkMsgType.Revocation)
            {
                var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                var messageId = tempChatMsg.content?.messageId;
                var onlineMsg = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.messageId == messageId);
                if (onlineMsg != null)
                    WaitingToReceiveOnlineMessageList.Remove(onlineMsg);

                var tuple2 = Tuple.Create(chatMsg.sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2) && WaitingToReceiveOfflineMsgNum[tuple2].Count > 0)
                {
                    WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                }
                SessionListViewModel?.HandleRevocationReceipt(chatMsg, true);
            }
            else if (msgType == AntSdkMsgType.DeleteVote || msgType == AntSdkMsgType.DeleteActivity)
            {
                //var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)chatMsg;
                var messageId = chatMsg.messageId;
                var onlineMsg = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.messageId == messageId);
                if (onlineMsg != null)
                    WaitingToReceiveOnlineMessageList.Remove(onlineMsg);
                var tuple2 = Tuple.Create(chatMsg.sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2) && WaitingToReceiveOfflineMsgNum[tuple2].Count > 0)
                {
                    WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                }
                SessionListViewModel?.HandleRevocationReceipt(chatMsg, true);
            }
            //else if (msgType == AntSdkMsgType.DeleteActivity)
            //{

            //}

            var messsage = WaitingToReceiveOnlineMessageList.FirstOrDefault(m => m.messageId == chatMsg.messageId && m.chatIndex == chatMsg.chatIndex);
            if (messsage != null) return;
            chatMsg.sendsucessorfail = 1;
            WaitingToReceiveOnlineMessageList.Add(chatMsg);
            if (chatMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId && (msgType != AntSdkMsgType.Revocation
                && msgType != AntSdkMsgType.DeleteActivity && msgType != AntSdkMsgType.DeleteVote))
            {
                OnlineMessageCount(chatMsg);
            }
            AddSessionItemOnlineMsg(msgType, chatMsg);


        }

        private static void SendAutoReplyMessage(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg)
        {
            bool result = false;
            var errorMsg = string.Empty;
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            int maxChatindex = t_chat.GetPreOneChatIndex(AntSdkService.AntSdkCurrentUserInfo.userId, chatMsg.sessionId);
            AntSdkChatMsg.Text sendAtText = new AntSdkChatMsg.Text();
            string messageid = PublicTalkMothed.timeStampAndRandom();
            sendAtText.content = GlobalVariable.RevocationPrompt.AutoReplyMessage.TrimEnd('\n').TrimEnd('\r');
            sendAtText.MsgType = AntSdkMsgType.ChatMsgText;
            sendAtText.messageId = messageid;
            sendAtText.flag = 0;
            sendAtText.sendUserId = chatMsg.targetId;
            sendAtText.sessionId = chatMsg.sessionId;
            sendAtText.targetId = chatMsg.sendUserId;
            sendAtText.chatType = (int)AntSdkchatType.Point;

            //TODO:AntSdk_Modify
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase sm_ctt = JsonConvert.DeserializeObject<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>(JsonConvert.SerializeObject(/*smt*/sendAtText));
            sm_ctt.chatIndex = maxChatindex.ToString();
            sm_ctt.sendsucessorfail = 0;
            sm_ctt.SENDORRECEIVE = "1";
            sm_ctt.sourceContent = sendAtText.content;
            //2017-03-03 添加数据
            result = AntSdkService.SdkPublishChatMsg(sendAtText, ref errorMsg);
            WriteLocalHistory(sm_ctt.MsgType, sm_ctt, "");


        }

        /// <summary>
        /// 在线消息修改Session列表Item
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="chatMsg"></param>
        public static void AddSessionItemOnlineMsg(AntSdkMsgType msgType, AntSdkChatMsg.ChatBase chatMsg, bool isWriteLocalData = true)
        {
            string lastMsg = string.Empty;
            var isResult = GetLastMessageContent(msgType, chatMsg, out lastMsg);
            if (!isResult && isWriteLocalData) return;
            var contactUser = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.sendUserId);
            SessionListViewModel?.RefreshSessionInfoList(lastMsg, contactUser, chatMsg, MessageCount(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.flag), chatMsg.MsgType);
        }
        /// <summary>
        /// 在线消息修改Session列表Item(自己发送的消息)
        /// </summary>
        public static void UpdateSeesionItemOnLineMsg(AntSdkMsgType msgType, AntSdkReceivedOtherMsg.MsgReceipt msg)
        {
            string lastMsg = string.Empty;
            if (msg.sessionId.StartsWith("G"))
            {
                var chatMsg = new AntSdkChatMsg.ChatBase
                {
                    messageId = msg.messageId,
                    chatIndex = msg.chatIndex,
                    sendTime = msg.sendTime,
                    sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId
                };
                var chatGroupMsgData = AntSdkSqliteHelper
                        .ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                        (t_chatGroupMsg.GetChatMessageByMsessageID(msg.sessionId, msg.messageId));
                if (chatGroupMsgData?.Count > 0)
                {
                    updateGroupData(chatMsg);
                    if (chatGroupMsgData[0].MsgType == AntSdkMsgType.Revocation)
                        chatGroupMsgData[0].sourceContent = PublicMessageFunction.FormatLastMessageContent(chatGroupMsgData[0].MsgType, chatGroupMsgData[0], true);
                    else
                    {
                        chatGroupMsgData[0].sourceContent = "";
                    }
                    chatGroupMsgData[0].sendTime = msg.sendTime;
                    chatGroupMsgData[0].chatIndex = msg.chatIndex;
                    chatGroupMsgData[0].flag = 0;
                    SessionListViewModel?.RefreshSessionInfoList(chatGroupMsgData[0], true);
                }
                else
                {
                    var chatGroupBurnMsgData = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                    (t_chatGroupBurnMsg.GetChatMessageByMsessageID(msg.sessionId, msg.messageId));
                    if (chatGroupBurnMsgData?.Count > 0)
                    {
                        updateGroupBurnData(chatMsg);
                        if (chatGroupBurnMsgData[0].MsgType == AntSdkMsgType.Revocation)
                            chatGroupBurnMsgData[0].sourceContent = PublicMessageFunction.FormatLastMessageContent(chatGroupBurnMsgData[0].MsgType, chatGroupBurnMsgData[0], true);
                        else
                        {
                            chatGroupBurnMsgData[0].sourceContent = "";
                        }
                        chatGroupBurnMsgData[0].sendTime = msg.sendTime;
                        chatGroupBurnMsgData[0].chatIndex = msg.chatIndex;
                        chatGroupBurnMsgData[0].flag = 1;
                        SessionListViewModel?.RefreshSessionInfoList(chatGroupBurnMsgData[0], true);
                    }
                }
            }
            else
            {
                var chatMsg = new AntSdkChatMsg.ChatBase
                {
                    messageId = msg.messageId,
                    chatIndex = msg.chatIndex,
                    sendTime = msg.sendTime,
                    sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId
                };
                var chatMsgData = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                    (t_chatMsg.GetChatMessageByMsessageID(msg.sessionId, msg.messageId));
                if (chatMsgData?.Count > 0)
                {
                    t_chatMsg.Update(chatMsg);
                    if (chatMsgData[0].MsgType == AntSdkMsgType.Revocation)
                        chatMsgData[0].sourceContent = PublicMessageFunction.FormatLastMessageContent(chatMsgData[0].MsgType, chatMsgData[0], false);
                    else
                    {
                        chatMsgData[0].sourceContent = "";
                    }
                    chatMsgData[0].sendTime = msg.sendTime;
                    chatMsgData[0].chatIndex = msg.chatIndex;
                    SessionListViewModel?.RefreshSessionInfoList(chatMsgData[0], false);
                }
            }

        }

        /// <summary>
        /// 离线消息修改Session列表Item
        /// </summary>
        /// <param name="lastMessage">最一条的消息内容</param>
        /// <param name="AntSdkContact_User"></param>
        /// <param name="chatMsg"></param>
        /// <param name="msgCount">消息条数</param>
        /// <param name="isOffline">是否是离线消息</param>
        public static void AddSessionItemOfflineMsg(string lastMessage, AntSdkContact_User AntSdkContact_User, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg, bool isOffline = false)
        {
            int unReadCount = 0;
            if (chatMsg != null)
                unReadCount = MessageCount(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.flag);
            var msgType = AntSdkMsgType.UnDefineMsg;
            if (chatMsg != null)
                msgType = chatMsg.MsgType;
            SessionListViewModel?.RefreshSessionInfoList(lastMessage, AntSdkContact_User, chatMsg, unReadCount, msgType, isOffline);
        }

        /// <summary>
        /// 根据sessionID删除消息列表对应的项
        /// </summary>
        /// <param name="sessionID"></param>
        public static void RemoveSessionInfoList(string sessionID, GlobalVariable.BurnFlag isBurnFlag)
        {
            SessionListViewModel?.RemoveSessionInfoList(sessionID, isBurnFlag);
        }

        /// <summary>
        /// 在线消息没窗体接收的消息计数
        /// </summary>
        /// <param name="sessionId"></param>
        private static void OnlineMessageCount(AntSdkChatMsg.ChatBase chatMsg)
        {
            int burnFlag = chatMsg.flag;
            if (chatMsg.chatType == (int)AntSdkchatType.Point)
                burnFlag = 0;
            Tuple<string, GlobalVariable.BurnFlag> tuple2 = null;
            if (burnFlag == (int)GlobalVariable.BurnFlag.IsBurn)
            {
                tuple2 = Tuple.Create(chatMsg.sessionId, GlobalVariable.BurnFlag.IsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    var info = WaitingToReceiveOfflineMsgNum[tuple2];
                    info.Count += 1;
                    info.ChatIndex = chatMsg.chatIndex;
                    WaitingToReceiveOfflineMsgNum[tuple2] = info;
                }
                else
                {
                    WaitingToReceiveOfflineMsgNum.Add(tuple2,
                        new MessageInfo()
                        {
                            IsBurnFlag = GlobalVariable.BurnFlag.IsBurn,
                            ChatIndex = chatMsg.chatIndex,
                            Count = 1,
                            MessageId = chatMsg.messageId
                        });
                }
            }
            else if (burnFlag == (int)GlobalVariable.BurnFlag.NotIsBurn)
            {
                tuple2 = Tuple.Create(chatMsg.sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    var info = WaitingToReceiveOfflineMsgNum[tuple2];
                    info.Count += 1;
                    info.ChatIndex = chatMsg.chatIndex;
                    WaitingToReceiveOfflineMsgNum[tuple2] = info;
                }
                else
                {
                    WaitingToReceiveOfflineMsgNum.Add(tuple2,
                        new MessageInfo()
                        {
                            IsBurnFlag = GlobalVariable.BurnFlag.NotIsBurn,
                            ChatIndex = chatMsg.chatIndex,
                            Count = 1,
                            MessageId = chatMsg.messageId
                        });
                }
            }
        }
        /// <summary>
        /// 未查看消息的个数
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="chatIndex"></param>
        /// <param name="flag"></param>
        public static int MessageCount(string sessionId, string chatIndex, int flag)
        {
            if (WaitingToReceiveOfflineMsgNum == null) return 0;
            int burnFlag = flag;
            if (!sessionId.StartsWith("G"))
                burnFlag = 0;
            Tuple<string, GlobalVariable.BurnFlag> tuple2 = null;
            if (burnFlag == (int)GlobalVariable.BurnFlag.IsBurn)
            {
                tuple2 = Tuple.Create(sessionId, GlobalVariable.BurnFlag.IsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    return WaitingToReceiveOfflineMsgNum[tuple2].Count;
                }
            }
            else if (burnFlag == (int)GlobalVariable.BurnFlag.NotIsBurn)
            {
                tuple2 = Tuple.Create(sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    return WaitingToReceiveOfflineMsgNum[tuple2].Count;
                }
            }
            return 0;
        }
        /// <summary>
        /// 移除已被查看的消息记录
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="flag"></param>
        public static void RemoveWaitingToReceiveOfflineMsgRecord(string sessionId, GlobalVariable.BurnFlag flag)
        {
            int burnFlag = -1;
            if (flag == GlobalVariable.BurnFlag.IsBurn)
                burnFlag = 1;
            else if (flag == GlobalVariable.BurnFlag.NotIsBurn)
                burnFlag = 0;
            Tuple<string, GlobalVariable.BurnFlag> tuple2 = null;
            if (burnFlag == (int)GlobalVariable.BurnFlag.IsBurn)
            {
                tuple2 = Tuple.Create(sessionId, GlobalVariable.BurnFlag.IsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    WaitingToReceiveOfflineMsgNum.Remove(tuple2);
                }
            }
            else if (burnFlag == (int)GlobalVariable.BurnFlag.NotIsBurn)
            {
                tuple2 = Tuple.Create(sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                if (WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    WaitingToReceiveOfflineMsgNum.Remove(tuple2);
                }
            }
        }

        /// <summary>
        /// 根据窗体ID获取错过的即时消息列表
        /// </summary>
        /// <param name="sessionID">窗体ID</param>
        /// <returns>该窗体ID对应的错过即时文字消息列表</returns>
        public static List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> GetWaitingToReceiveOnlineMessage(string sessionID, int flag, AntSdkchatType chatType)
        {
            if (WaitingToReceiveOnlineMessageList.Count == 0) return new List<AntSdkChatMsg.ChatBase>();
            List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> lstResult;
            if (chatType == AntSdkchatType.Point)
            {
                lstResult = WaitingToReceiveOnlineMessageList.Where(m => m.sessionId == sessionID).ToList();
                WaitingToReceiveOnlineMessageList.RemoveAll(m => m.sessionId == sessionID);
                flag = (int)GlobalVariable.BurnFlag.NotIsBurn;
            }
            else
            {
                lstResult = WaitingToReceiveOnlineMessageList.Where(m => m.sessionId == sessionID && m.flag == flag).ToList();
                WaitingToReceiveOnlineMessageList.RemoveAll(m => m.sessionId == sessionID && m.flag == flag);
            }
            GlobalVariable.BurnFlag burnFlag = GlobalVariable.BurnFlag.NotIsBurn;
            if (flag == (int)GlobalVariable.BurnFlag.IsBurn)
                burnFlag = GlobalVariable.BurnFlag.IsBurn;
            RemoveWaitingToReceiveOfflineMsgRecord(sessionID, burnFlag);
            return lstResult;
        }

        /// <summary>
        /// 获取在线消息最后一条消息内容
        /// </summary>
        /// <param name="mtp"></param>
        /// <param name="chatMsg"></param>
        private static bool GetLastMessageContent(AntSdkMsgType msgType, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg, out string lastMessage)
        {
            AntSdkContact_User AntSdkContact_User = null;
            lastMessage = string.Empty;
            if (chatMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId) //本人发的消息，系统给的回执
            {
                AntSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                        c => c.userId == AntSdkService.AntSdkLoginOutput.userId);
                if (AntSdkContact_User == null) return false;
                if (msgType == AntSdkMsgType.Revocation)
                {
                    lastMessage = "你" + GlobalVariable.RevocationPrompt.Msessage;
                    chatMsg.sourceContent = lastMessage;
                    if (chatMsg.os != (int)GlobalVariable.OSType.PC)
                    {
                        var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                        if (chatMsg.chatType == (int)AntSdkchatType.Point)
                        {
                            if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                                t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                    AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                                t_groupChat.DeleteGroupMsgByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                  AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                        }
                    }
                }
                else if (msgType == AntSdkMsgType.DeleteVote)
                {
                    var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)chatMsg;

                    if (tempChatMsg.content != null)
                    {
                        t_groupChat.DeleteGroupMsgByVoteOrActivityId(tempChatMsg.content.id.ToString(),
                            tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateVote).ToString(),
                            AntSdkService.AntSdkLoginOutput.companyCode,
                            AntSdkService.AntSdkCurrentUserInfo.userId);
                    }
                    PublicMessageFunction.SendChatMsgReceipt(tempChatMsg.sessionId, tempChatMsg.chatIndex, tempChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                    if (tempChatMsg.content != null)
                        LogHelper.WriteDebug("[群投票被删除消息发送收回执]--------------sessionId：" + tempChatMsg.sessionId + "---------chatIndex：" + tempChatMsg.chatIndex + "--------voteId" + tempChatMsg.content.id);
                    return false;
                }
                else if (msgType == AntSdkMsgType.DeleteActivity)
                {
                    var tempChatMsg = (AntSdkChatMsg.DeleteActivityMsg)chatMsg;
                    if (tempChatMsg.content != null)
                    {
                        t_groupChat.DeleteGroupMsgByVoteOrActivityId(tempChatMsg.content.activityId.ToString(),
                            tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateActivity).ToString(),
                            AntSdkService.AntSdkLoginOutput.companyCode,
                            AntSdkService.AntSdkCurrentUserInfo.userId);
                    }
                    PublicMessageFunction.SendChatMsgReceipt(tempChatMsg.sessionId, tempChatMsg.chatIndex, tempChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                    if (tempChatMsg.content != null)
                        LogHelper.WriteDebug("[群活动被删除消息发送收回执]--------------sessionId：" + tempChatMsg.sessionId + "---------chatIndex：" + tempChatMsg.chatIndex + "---------activityId" + tempChatMsg.content.activityId);
                    return false;
                }
                else
                {
                    lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, chatMsg,
                        chatMsg.chatType == (int)AntSdkchatType.Group);
                }

                var isWrite = WriteLocalHistory(msgType, chatMsg, AntSdkContact_User.userName, true);
                if (isWrite)
                {
                    LogHelper.WriteDebug("[在线消息入库成功（本人发的消息，系统给的回执）]:" + msgType + "-----------------" +
                                         JsonConvert.SerializeObject(chatMsg));
                    return WriteLoaclSession(chatMsg, lastMessage, chatMsg.flag.ToString());
                }
            }
            else
            {
                AntSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.sendUserId);
                if (AntSdkContact_User == null) return false;
                if (chatMsg.chatType == (int)AntSdkchatType.Point)
                //点对点聊天，且发言者不是本人
                {
                    if (msgType == AntSdkMsgType.Revocation)
                    {
                        var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                        if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                            t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                        lastMessage = tempChatMsg.sourceContent;
                    }
                    else
                    {
                        lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, chatMsg, false);
                    }


                }
                else if (chatMsg.chatType == (int)AntSdkchatType.Group) //群聊，且发言者不是本人 
                {
                    if (chatMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId && chatMsg.os == ((int)GlobalVariable.OSType.PC)) return false; //过滤掉本人发的群消息
                    if (chatMsg.flag != ((int)GlobalVariable.BurnFlag.IsBurn))
                    {
                        if (msgType == AntSdkMsgType.Revocation)
                        {
                            var tempChatMsg = (AntSdkChatMsg.Revocation)chatMsg;
                            if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                                t_groupChat.DeleteGroupMsgByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                  AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                            lastMessage = chatMsg.sourceContent;
                        }
                        else if (msgType == AntSdkMsgType.DeleteVote)
                        {
                            var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)chatMsg;
                            if (tempChatMsg.content != null)
                            {
                                t_groupChat.DeleteGroupMsgByVoteOrActivityId(tempChatMsg.content.id.ToString(),
                                    tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateVote).ToString(),
                                    AntSdkService.AntSdkLoginOutput.companyCode,
                                    AntSdkService.AntSdkCurrentUserInfo.userId);
                            }
                            PublicMessageFunction.SendChatMsgReceipt(tempChatMsg.sessionId, tempChatMsg.chatIndex, tempChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                            LogHelper.WriteDebug("[群投票被删除消息发送收回执]--------------sessionId：" + tempChatMsg.sessionId + "---------chatIndex：" + tempChatMsg.chatIndex);
                            return false;
                        }
                        else if (msgType == AntSdkMsgType.DeleteActivity)
                        {
                            var tempChatMsg = (AntSdkChatMsg.DeleteActivityMsg)chatMsg;
                            if (tempChatMsg.content != null)
                            {
                                t_groupChat.DeleteGroupMsgByVoteOrActivityId(tempChatMsg.content.activityId.ToString(),
                                    tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateActivity).ToString(),
                                    AntSdkService.AntSdkLoginOutput.companyCode,
                                    AntSdkService.AntSdkCurrentUserInfo.userId);
                            }
                            PublicMessageFunction.SendChatMsgReceipt(tempChatMsg.sessionId, tempChatMsg.chatIndex, tempChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                            if (tempChatMsg.content != null)
                                LogHelper.WriteDebug("[群活动被删除消息发送收回执]--------------sessionId：" + tempChatMsg.sessionId + "---------chatIndex：" + tempChatMsg.chatIndex + "---------activityId" + tempChatMsg.content.activityId);
                            return false;
                        }
                        else
                        {
                            lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, chatMsg, true,
                                AntSdkContact_User.userName != null
                                    ? AntSdkContact_User.userNum + AntSdkContact_User.userName
                                    : AntSdkContact_User.userName);
                        }

                    }
                    else
                    {
                        lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, chatMsg, true);
                    }
                }
                var isWrite = WriteLocalHistory(msgType, chatMsg, AntSdkContact_User.userName);
                if (isWrite)
                {
                    LogHelper.WriteDebug("[在线消息入库成功]:" + msgType.ToString() + "-----------------" +
                                       JsonConvert.SerializeObject(chatMsg));
                    return WriteLoaclSession(chatMsg, lastMessage, chatMsg.flag.ToString());
                }
            }
            return false;
        }

        /// <summary>
        /// 在线消息写入库
        /// </summary>
        private static bool WriteLocalHistory(AntSdkMsgType msgType, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg, string userName, bool isCurrentUser = false)
        {
            if (isCurrentUser)
            {
                if (chatMsg.os == ((int)GlobalVariable.OSType.PC)) //PC端发的消息
                {
                    if (chatMsg.sessionId[0] == 'G') //群聊
                    {
                        return chatMsg.flag == 1 ? updateGroupBurnData(chatMsg) : updateGroupData(chatMsg);
                    }
                    else //点对点聊天
                    {
                        return updateOneToOneData(chatMsg);
                    }
                }
                else
                {
                    if (chatMsg.sessionId[0] == 'G') //群聊
                    {
                        //群聊信息入库
                        if (chatMsg.flag == 1)
                        {
                            //TODO:AntSdk_Modify
                            //chatMsg.MTP = msgType.ToString();
                            chatMsg.SENDORRECEIVE = "0";
                            //chatMsg.username = userName;
                            return PublicTalkMothed.addDataGroupBurn(chatMsg);
                        }
                        else
                        {
                            if (chatMsg.MsgType == AntSdkMsgType.CreateVote)
                            {
                                var tempChatMsg = (AntSdkChatMsg.CreateVoteMsg)chatMsg;
                                if (tempChatMsg.content != null)
                                    chatMsg.VoteOrActivityID = tempChatMsg.content.id.ToString();
                            }
                            else if (chatMsg.MsgType == AntSdkMsgType.CreateActivity)
                            {
                                var tempChatMsg = (AntSdkChatMsg.ActivityMsg)chatMsg;
                                if (tempChatMsg.content != null)
                                    chatMsg.VoteOrActivityID = tempChatMsg.content.activityId.ToString();
                            }
                            chatMsg.SENDORRECEIVE = "0";
                            //chatMsg.username = userName;
                            return PublicTalkMothed.addDataGroup(chatMsg);
                        }
                    }
                    else
                    {
                        //TODO:AntSdk_Modify
                        //单聊信息入库
                        //chatMsg.MTP = msgType.ToString();
                        chatMsg.SENDORRECEIVE = "1";
                        return PublicTalkMothed.addSelfData(chatMsg);
                    }
                }
            }
            else
            {
                if (chatMsg.chatType == (int)AntSdkchatType.Point)//点对点聊天，且发言者不是本人
                {
                    //单聊信息入库
                    //chatMsg.MTP = msgType.ToString();
                    chatMsg.SENDORRECEIVE = "0";
                    //if(msgType==AntSdkMsgType.Revocation)
                    //    chatMsg.messageId=chatMsg
                    return PublicTalkMothed.addDataOneToOne(chatMsg);
                }
                else if (chatMsg.chatType == (int)AntSdkchatType.Group)
                {
                    //群聊信息入库
                    if (chatMsg.flag == 1)
                    {
                        //chatMsg.MTP = msgType.ToString();
                        chatMsg.SENDORRECEIVE = "0";
                        //TODO:AntSdk_Modify
                        //chatMsg.username = userName;
                        return PublicTalkMothed.addDataGroupBurn(chatMsg);
                    }
                    else
                    {
                        if (chatMsg.MsgType == AntSdkMsgType.CreateVote)
                        {
                            var tempChatMsg = (AntSdkChatMsg.CreateVoteMsg)chatMsg;
                            if (tempChatMsg.content != null)
                                chatMsg.VoteOrActivityID = tempChatMsg.content.id.ToString();
                        }
                        else if (chatMsg.MsgType == AntSdkMsgType.CreateActivity)
                        {
                            var tempChatMsg = (AntSdkChatMsg.ActivityMsg)chatMsg;
                            if (tempChatMsg.content != null)
                                chatMsg.VoteOrActivityID = tempChatMsg.content.activityId.ToString();
                        }
                        chatMsg.SENDORRECEIVE = "0";
                        return PublicTalkMothed.addDataGroup(chatMsg);
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 在线消息最新消息记录
        /// </summary>
        /// <param name="chatMsg"></param>
        /// <param name="lastMessage"></param>
        /// <param name="burnFlag">阅后即焚标记</param>
        private static bool WriteLoaclSession(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase chatMsg, string lastMessage, string burnFlag)
        {
            if (chatMsg == null)
                return false;
            var tSession = t_sessionBll.GetModelByKey(chatMsg.sessionId);
            int tempBurnFlag = -1;
            if (!string.IsNullOrEmpty(burnFlag))
                int.TryParse(burnFlag, out tempBurnFlag);
            var burn = GlobalVariable.BurnFlag.NotIsBurn;
            if (tempBurnFlag == 1)
                burn = GlobalVariable.BurnFlag.IsBurn;
            if (chatMsg.chatType != (int)AntSdkchatType.Group)
                burn = GlobalVariable.BurnFlag.NotIsBurn;
            Tuple<string, GlobalVariable.BurnFlag> tuple = Tuple.Create(chatMsg.sessionId, burn);
            MessageInfo msgInfo = null;
            if (WaitingToReceiveOfflineMsgNum != null && WaitingToReceiveOfflineMsgNum.ContainsKey(tuple))
                msgInfo = WaitingToReceiveOfflineMsgNum[tuple];
            if (tSession == null)
            {
                AntSdkTsession t_session = null;
                if (chatMsg.chatType == (int)AntSdkchatType.Point)
                //点对点聊天(发言者可能是本人)
                {
                    t_session = new AntSdkTsession();
                    t_session.LastMsg = lastMessage;
                    t_session.LastMsgTimeStamp = chatMsg.sendTime;
                    //t_session.LastModifyTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss,fff");
                    t_session.SessionId = chatMsg.sessionId;
                    if (chatMsg.MsgType != AntSdkMsgType.Revocation)
                        t_session.UnreadCount = msgInfo?.Count ?? 1;
                    if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)//如果发送者是本人
                    {
                        var antsdkcontact_User = AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(c => c.userId == chatMsg.targetId);
                        if (antsdkcontact_User != null)
                            t_session.UnreadCount = 0;
                    }
                    t_session.LastChatIndex = chatMsg.chatIndex;
                    //t_session.MyselfMsgCount = model.MyselfMsgCount;
                    t_session.UserId = chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo?.userId ? chatMsg.targetId : chatMsg.sendUserId;
                    int result = t_sessionBll.Insert(t_session);
                    if (result == 1)
                    {
                        if (result == 1)
                        {
                            #region 发回执：非本人发的消息或者本人发的但不是PC端发的消息
                            if (chatMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId || chatMsg.os != ((int)GlobalVariable.OSType.PC))
                            {
                                PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                                LogHelper.WriteDebug("[在线消息发送已收回执]:" + chatMsg.MsgType.ToString() + "-----------------" +
                                     JsonConvert.SerializeObject(chatMsg));
                            }
                            #endregion
                            return true;
                        }
                    }
                }
                else if (chatMsg.chatType == (int)AntSdkchatType.Group) //群聊(发言者可能是本人)
                {
                    t_session = new AntSdkTsession();
                    var msgCount = msgInfo?.Count ?? 1;
                    if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)//如果发送者是本人
                    {
                        msgCount = 0;
                    }
                    if (tempBurnFlag == (int)GlobalVariable.BurnFlag.IsBurn)
                    {
                        t_session.BurnLastMsg = lastMessage;
                        t_session.BurnLastMsgTimeStamp = chatMsg.sendTime;
                        t_session.BurnUnreadCount = msgCount;
                        t_session.BurnLastChatIndex = chatMsg.chatIndex;
                        //t_session.IsBurnMode = chatMsg.flag;
                    }
                    else
                    {
                        t_session.LastMsg = lastMessage;
                        t_session.LastMsgTimeStamp = chatMsg.sendTime;
                        if (chatMsg.MsgType != AntSdkMsgType.Revocation)
                            t_session.UnreadCount = msgCount;
                        t_session.LastChatIndex = chatMsg.chatIndex;
                    }
                    t_session.SessionId = chatMsg.sessionId;
                    t_session.GroupId = chatMsg.sessionId;
                    int result = t_sessionBll.Insert(t_session);
                    if (result == 1)
                    {
                        #region 发回执：非本人发的消息或者本人发的但不是PC端发的消息
                        if (chatMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId || chatMsg.os != ((int)GlobalVariable.OSType.PC))
                        {
                            PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                            LogHelper.WriteDebug("[在线消息发送已收回执]:" + chatMsg.MsgType.ToString() + "-----------------" +
                                    JsonConvert.SerializeObject(chatMsg));
                        }
                        #endregion
                        return true;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(tSession.SessionId) && chatMsg.sessionId != tSession.SessionId)
                    tSession = t_sessionBll.GetModelByKey(chatMsg.sessionId);
                GroupInfoViewModel groupInfoVm = GroupListViewModel?.GroupInfoList?.FirstOrDefault(c => c.GroupInfo != null && c.GroupInfo.groupId == chatMsg.sessionId);
                var existVM = SessionListViewModel?.GetSessionInfoViewModelById(chatMsg.sessionId);
                if (chatMsg.sessionId == chatMsg.targetId && chatMsg.flag == ((int)GlobalVariable.BurnFlag.IsBurn))//讨论组阅后即焚消息
                {
                    if (existVM != null && (existVM.IsMouseClick && (int)existVM.IsBurnMode == chatMsg.flag))
                    {
                        tSession.BurnUnreadCount = 0;
                    }
                    else if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                    {
                        tSession.BurnUnreadCount = tSession.BurnUnreadCount;
                    }
                    else if (tSession.BurnLastChatIndex != chatMsg.chatIndex)
                    {
                        tSession.BurnUnreadCount = msgInfo?.Count ?? 1;
                    }
                    if (!string.IsNullOrEmpty(tSession.LastChatIndex) && !string.IsNullOrEmpty(chatMsg.chatIndex))
                    {
                        //var lastChartIndex = int.Parse(tSession.LastChatIndex);
                        //var chatIndex = int.Parse(chatMsg.chatIndex);
                        //if (chatIndex > lastChartIndex && tSession.IsBurnMode == 0)
                        //    tSession.IsBurnMode = 1;
                    }
                    tSession.BurnLastMsg = lastMessage;
                    tSession.BurnLastMsgTimeStamp = chatMsg.sendTime;
                    tSession.BurnLastChatIndex = chatMsg.chatIndex;
                    if (SessionListViewModel != null && tSession.TopIndex != null && SessionListViewModel.MaxTopIndex != null && tSession.TopIndex.Value < SessionListViewModel.MaxTopIndex.Value)
                    {
                        tSession.TopIndex = SessionListViewModel.MaxTopIndex + 1;
                        SessionListViewModel.MaxTopIndex = tSession.TopIndex;
                    }
                }
                else
                {
                    if (existVM != null && (existVM.IsMouseClick && (groupInfoVm == null || (int)existVM.IsBurnMode == (int)GlobalVariable.BurnFlag.NotIsBurn)))
                    {
                        tSession.UnreadCount = 0;
                    }
                    else if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                    {
                        tSession.UnreadCount = tSession.UnreadCount;
                    }
                    else if (tSession.LastChatIndex != chatMsg.chatIndex)
                    {
                        tSession.UnreadCount = msgInfo?.Count ?? 1;
                    }
                    //tSession.UnreadCount = existVM.UnreadCount;
                    if (tSession.SessionId == chatMsg.sessionId)
                    {
                        tSession.LastMsg = lastMessage;
                        tSession.LastMsgTimeStamp = chatMsg.sendTime;
                        tSession.LastChatIndex = chatMsg.chatIndex;
                    }
                    if (SessionListViewModel != null && tSession.TopIndex != null && SessionListViewModel.MaxTopIndex != null && tSession.TopIndex.Value < SessionListViewModel.MaxTopIndex.Value)
                    {
                        tSession.TopIndex = SessionListViewModel.MaxTopIndex.Value + 1;
                        SessionListViewModel.MaxTopIndex = tSession.TopIndex;
                    }
                    if (!string.IsNullOrEmpty(tSession.BurnLastChatIndex) && !string.IsNullOrEmpty(chatMsg.chatIndex))
                    {
                        //var burnLastChartIndex = int.Parse(tSession.BurnLastChatIndex);
                        //var chatIndex = int.Parse(chatMsg.chatIndex);
                        //if (chatIndex > burnLastChartIndex && tSession.IsBurnMode == 1)
                        //    tSession.IsBurnMode = 0;
                    }
                }
                if (string.IsNullOrEmpty(tSession.SessionId))
                {
                    tSession.SessionId = chatMsg.sessionId;
                    if (chatMsg.sessionId == chatMsg.targetId)//群聊
                    {
                        tSession.GroupId = chatMsg.sessionId;
                    }
                    else if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)//如果发送者是本人
                    {
                        tSession.UserId = chatMsg.targetId;
                    }
                    else
                    {
                        tSession.UserId = chatMsg.sendUserId;
                    }
                    //ThreadPool.QueueUserWorkItem(m => t_sessionBll.Insert(tSession));
                    int result = t_sessionBll.Insert(tSession);
                    if (result == 1)
                        return true;
                }
                else
                {
                    //ThreadPool.QueueUserWorkItem(m => t_sessionBll.Update(tSession));
                    int result = t_sessionBll.Update(tSession);
                    if (result == 1)
                    {
                        #region 发送已读或者已收回执
                        if (!GlobalVariable.isMinimized && existVM != null && existVM.IsMouseClick)
                        {
                            PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReadReceipt);
                            LogHelper.WriteDebug("[在线消息发送已读回执]:" + chatMsg.MsgType + "-----------------" +
                                    JsonConvert.SerializeObject(chatMsg));
                        }
                        else
                        {
                            PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                            LogHelper.WriteDebug("[在线消息发送已收回执]:" + chatMsg.MsgType + "-----------------" +
                                   JsonConvert.SerializeObject(chatMsg));
                        }
                        #endregion
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 更新群聊天阅后即焚数据
        /// </summary>
        /// <param name="cmr"></param>
        public static bool updateGroupBurnData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (t_chatGroupBurnMsg.Update(cmr) > 0)
                {
                    LogHelper.WriteDebug("[updateGroupBurnData-sucess]:" + cmr.messageId);
                }
                else
                {
                    LogHelper.WriteDebug("[updateGroupBurnData-fail]:" + cmr.messageId);
                }
            }));
            return true;
        }
        /// <summary>
        /// 插入一对一聊天数据
        /// </summary>
        /// <param name="cmr"></param>
        public static bool updateOneToOneData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (t_chatMsg.Update(cmr) > 0)
                {
                    LogHelper.WriteDebug("[updateOneToOneData-sucess]:" + cmr.messageId);
                }
                else
                {
                    LogHelper.WriteDebug("[updateOneToOneData-fail]:" + cmr.messageId);
                }
            }));
            return true;
        }

        //TODO:AntSdk_Modify
        /// <summary>
        /// 插入群聊天正常数据
        /// </summary>
        /// <param name="cmr"></param>
        public static bool updateGroupData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
           {

               if (t_chatGroupMsg.Update(cmr) > 0)
               {
                   LogHelper.WriteDebug("[updateGroupData-sucess]:" + cmr.messageId);
               }
               else
               {
                   LogHelper.WriteDebug("[updateGroupData-fail]:" + cmr.messageId);
               }
           }));
            return true;
        }

        /// <summary>
        /// 处理未被窗体接收的到达系统消息
        /// </summary>
        /// <param name="sysUserMsg">活动即时系统消息</param>
        private static
        void MessageMonitor_InstantSystemMessageWaitingToReceive(AntSdkMsBase msgBase, AntSdkMsgType msgType)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (msgType)
                {
                    case AntSdkMsgType.KickOut://用户被踢出登录
                        MainWindowVM.ForcedLoginOut("您的账号在另一个地点登录，您被迫下线。如果不是您本人操作，那么您的密码可能已泄露，建议您修改密码");
                        break;
                    case AntSdkMsgType.PaswordChangeKickOut://修改密码
                        MainWindowVM.ForcedLoginOut("您的密码已修改，请重新登录");
                        break;
                    case AntSdkMsgType.OnLine: //在线登录
                    case AntSdkMsgType.Leave: //离开登录
                    case AntSdkMsgType.Busy://忙碌登录
                    case AntSdkMsgType.PhoneLine:
                    case AntSdkMsgType.OffLine:
                    case AntSdkMsgType.Disable:
                    case AntSdkMsgType.ModifyInfo: //用户信息更新
                        //var modifyUserInfo = msgBase as AntSdkReceivedUserMsg.Modify;
                        //if (modifyUserInfo == null) break;
                        MainWindowVM.HandleSysUerMsg_UpdateUserInfo(msgBase);
                        break;
                    case AntSdkMsgType.CreateGroup: //讨论组新增
                        var createGroupInfo = msgBase as AntSdkReceivedGroupMsg.Create;
                        if (createGroupInfo == null) break;
                        HandleSysUerMsg_CreateGroup(createGroupInfo); break;
                    case AntSdkMsgType.DissolveGroup: //讨论组解散 
                        var deleteGroupInfo = msgBase as AntSdkReceivedGroupMsg.Delete;
                        if (deleteGroupInfo == null) break;
                        HandleSysUerMsg_DeleteGroup(deleteGroupInfo.sessionId); break;
                    case AntSdkMsgType.ModifyGroupMember://讨论组成员更新
                    case AntSdkMsgType.QuitGroupMember:
                    case AntSdkMsgType.GroupOwnerChanged:
                    case AntSdkMsgType.AddGroupMember:
                    case AntSdkMsgType.DeleteGroupMember:
                    case AntSdkMsgType.AdminSet:
                        var groupMember = msgBase as AntSdkReceivedGroupMsg.GroupBase;
                        if (groupMember == null) break;
                        HandleSysUerMsg_UpdateGroupMemeber(groupMember); break;
                    case AntSdkMsgType.ModifyGroup: //讨论组基本信息更新
                        var modifyGroupInfo = msgBase as AntSdkReceivedGroupMsg.Modify;
                        if (modifyGroupInfo == null) break;
                        HandleSysUerMsg_UpdateGroupInfo(modifyGroupInfo); break;
                    case AntSdkMsgType.UserIndividuation:
                        var individuation = msgBase as AntSdkReceivedOtherMsg.Individuation;
                        if (individuation == null) break;
                        HandleSysUserMsg_GroupState(individuation); break;
                    case AntSdkMsgType.OrganizationModify:
                        var modifyOrganization = msgBase as AntSdkReceivedOtherMsg.OrganizationModify;
                        if (modifyOrganization == null) break;
                        HandleOrganizationModify(modifyOrganization);
                        break;//组织架构更新
                    //case AntSdkMsgType.QuitGroupMember: break;
                    case AntSdkMsgType.UnReadNotifications:
                        //HandleSysUerMsg_NoticeLogin(ctt); 
                        break;//公告——登录推送
                    case AntSdkMsgType.AddNotification:
                        var addNotification = msgBase as AntSdkReceivedOtherMsg.Notifications.Add;
                        if (addNotification == null) return;
                        if (addNotification.sendUserId != AntSdkService.AntSdkLoginOutput.userId)
                            HandleSysUerMsg_NoticeAdd(addNotification);
                        break;//公告——添加推送
                    case AntSdkMsgType.DeleteNotification:
                        var deleteNotification = msgBase as AntSdkReceivedOtherMsg.Notifications.Delete;
                        if (deleteNotification == null) return;
                        HandleSysUerMsg_NoticeDelete(deleteNotification);
                        break;//公告——删除推送
                    case AntSdkMsgType.ModifyNotificationState:
                        var stateNotification = msgBase as AntSdkReceivedOtherMsg.Notifications.State;
                        if (stateNotification == null) return;
                        HandleSysUerMsg_NoticeRead(stateNotification);
                        break;//公告——修改已读状态推送
                    case AntSdkMsgType.GroupOwnerBurnMode://管理员将群由普通模式切换到阅后即焚模式
                    case AntSdkMsgType.GroupOwnerBurnDelete://删除阅后即焚消息
                    case AntSdkMsgType.GroupOwnerNormal:// 管理员将群由阅后即焚模式切换到普通模式
                        var groupBase = msgBase as AntSdkReceivedGroupMsg.GroupBase;
                        HandleGroupBurnAfterReadMode(groupBase, msgType);
                        break;
                    default: break;

                }
            }));
        }

        /// <summary>
        /// 讨论组新增
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_CreateGroup(AntSdkReceivedGroupMsg.Create sysMsg)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                if (GroupListViewModel == null)
                {
                    GroupListViewModel = new GroupListViewModel();
                }
                AntSdkCreateGroupOutput group = new AntSdkCreateGroupOutput();
                if (sysMsg.content == null) return;
                group.groupId = sysMsg.content.groupId;
                group.groupName = sysMsg.content.groupName;
                group.groupPicture = sysMsg.content.groupPicture;
                group.memberCount = sysMsg.content.memberCount.ToString();
                group.groupOwnerId = sysMsg.content.groupOwnerId;
                GroupListViewModel.AddGroup(group);
            });
        }
        /// <summary>
        /// 讨论组成员更新
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_UpdateGroupMemeber(AntSdkReceivedGroupMsg.GroupBase sysMsg)
        {
            var groupInfoVM = GroupListViewModel?.GroupInfoList?.FirstOrDefault(m => m.GroupInfo?.groupId == sysMsg.sessionId);
            if (groupInfoVM == null) return;

            if (sysMsg is AntSdkReceivedGroupMsg.OwnerChanged)//群主转让
            {
                var ownerGroup = (AntSdkReceivedGroupMsg.OwnerChanged)sysMsg;
                if (groupInfoVM.Members == null)
                {
                    groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                }
                else
                {
                    groupInfoVM.Members = groupInfoVM.Members.Select(m =>
                    {
                        if (m.userId == ownerGroup.content.newOwnerId)
                            m.roleLevel = (int)GlobalVariable.GroupRoleLevel.GroupOwner;
                        if (m.userId == ownerGroup.content.oldOwnerId)
                            m.roleLevel = (int)GlobalVariable.GroupRoleLevel.Ordinary;
                        return m;
                    }).ToList();
                }
                groupInfoVM.GroupInfo.groupOwnerId = ownerGroup?.content?.newOwnerId;

            }
            else if (sysMsg is AntSdkReceivedGroupMsg.QuitMember)//群组成员退出
            {
                var changedGroup = (AntSdkReceivedGroupMsg.QuitMember)sysMsg;
                if (!string.IsNullOrEmpty(changedGroup.content?.groupOwnerId) && groupInfoVM.GroupInfo.groupOwnerId != changedGroup.content?.groupOwnerId)
                {
                    if (groupInfoVM.Members == null)
                    {
                        groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                    }
                    else
                    {
                        groupInfoVM.Members = groupInfoVM.Members.Select(m =>
                        {
                            if (m.userId == changedGroup.content?.groupOwnerId)
                                m.roleLevel = (int)GlobalVariable.GroupRoleLevel.GroupOwner;
                            return m;
                        }).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(changedGroup.content?.userId))
                {
                    if (groupInfoVM.Members == null)
                    {
                        groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                    }
                    else
                    {
                        if (groupInfoVM.Members?.Count > 0)
                        {
                            if (changedGroup.content?.userId == AntSdkService.AntSdkLoginOutput.userId)//如果删除成员是当前登录用户
                            {
                                HandleSysUerMsg_DeleteGroup(sysMsg.sessionId);
                                return;
                            }
                            var groupMemeberInfo =
                                groupInfoVM.Members.FirstOrDefault(m => m.userId == changedGroup.content.userId);
                            if (groupMemeberInfo != null)
                                groupInfoVM.Members.Remove(groupMemeberInfo);
                        }
                    }
                }

            }
            else if (sysMsg is AntSdkReceivedGroupMsg.AdminSet)//设置管理员
            {
                var adminGroup = (AntSdkReceivedGroupMsg.AdminSet)sysMsg;
                if (groupInfoVM.Members == null)
                {
                    groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                }
                else
                {
                    groupInfoVM.Members = groupInfoVM.Members.Select(m =>
                    {
                        if (m.userId == adminGroup.content.manageId)
                            m.roleLevel = adminGroup.content.roleLevel;
                        return m;
                    }).ToList();
                }
                var groupManagerIds = groupInfoVM.GroupInfo.managerIds;
                if (groupManagerIds.Count == 0)
                    groupInfoVM.GroupInfo.managerIds.Add(adminGroup.content.manageId);
                else if(!groupManagerIds.Exists(m=>m== adminGroup.content.manageId))
                {
                    groupInfoVM.GroupInfo.managerIds.Add(adminGroup.content.manageId);
                }

            }
            else if (sysMsg is AntSdkReceivedGroupMsg.ModifyMember)
            {

            }
            else if (sysMsg is AntSdkReceivedGroupMsg.AddMembers)//增加群组成员
            {
                var addMemberGroup = (AntSdkReceivedGroupMsg.AddMembers)sysMsg;
                if (groupInfoVM.Members == null)
                {
                    groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                }
                else
                {
                    if (addMemberGroup.content?.members?.Count > 0)
                    {
                        var memberList = addMemberGroup.content.members;
                        foreach (var menber in memberList)
                        {
                            var groupMember = new AntSdkGroupMember();
                            var userInfo =
                                AntSdkService.AntSdkListContactsEntity.users?.FirstOrDefault(
                                    m => m.userId == menber.userId);
                            if (userInfo == null) return;
                            groupMember.userId = userInfo.userId;
                            groupMember.picture = userInfo.picture;
                            groupMember.position = userInfo.position;
                            groupMember.userName = userInfo.userName;
                            groupMember.userNum = userInfo.userNum;
                            if (!groupInfoVM.Members.Exists(m => m.userId == menber.userId))
                                groupInfoVM.Members.Add(groupMember);
                        }
                    }
                }
            }
            else
            {
                var deleteMemberGroup = sysMsg as AntSdkReceivedGroupMsg.DeleteMembers;
                if (groupInfoVM.Members == null)
                {
                    groupInfoVM.Members = GroupPublicFunction.GetMembers(sysMsg.sessionId);
                }
                else
                {
                    if (deleteMemberGroup?.content?.members?.Count > 0)
                    {
                        var memberList = deleteMemberGroup.content.members;
                        foreach (var menber in memberList)
                        {
                            if (menber.userId == AntSdkService.AntSdkLoginOutput.userId) //如果删除成员是当前登录用户
                            {
                                HandleSysUerMsg_DeleteGroup(sysMsg.sessionId);
                                return;
                            }
                            var groupMember = groupInfoVM.Members?.FirstOrDefault(m => m.userId == menber.userId);
                            if (groupMember != null)
                                groupInfoVM.Members.Remove(groupMember);
                        }
                    }
                }
            }

            GroupListViewModel?.UpdateGroupMemeber(sysMsg.sessionId, groupInfoVM.Members);
            _SessionListViewModel?.UpdateGroupMemeber(sysMsg.sessionId, groupInfoVM.Members);
        }
        /// <summary>
        /// 讨论组删除（被踢出讨论组）
        /// </summary>
        /// <param name="groupId"></param>
        private static void HandleSysUerMsg_DeleteGroup(string groupId)
        {
            GroupListViewModel?.DropOutGroup(groupId);
            _SessionListViewModel?.DropOutGroup(groupId);
        }
        /// <summary>
        /// 讨论组成员更新
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_UpdateGroupInfo(AntSdkReceivedGroupMsg.Modify sysMsg)
        {
            GroupListViewModel?.UpdateGroupInfo(sysMsg);
            _SessionListViewModel?.UpdateGroupInfo(sysMsg);
        }
        /// <summary>
        /// 组织架构更新
        /// </summary>
        private static void HandleOrganizationModify(AntSdkReceivedOtherMsg.OrganizationModify sysMsg)
        {
            if (string.IsNullOrEmpty(sysMsg.dataVersion)) return;
            //程序当前组织架构版本号
            var currentVersion = string.IsNullOrEmpty(AntSdkService.AntSdkListContactsEntity.dataVersion)
                ? 0
                : Convert.ToInt32(AntSdkService.AntSdkListContactsEntity.dataVersion);
            //推送过来的组织架构版本号
            var newVersion = Convert.ToInt32(sysMsg.dataVersion);
            //获取增量
            if (newVersion <= currentVersion) return;
            LogHelper.WriteWarn("组织结构增量数据当前版本：========" + currentVersion + "  最新版本：===========" + newVersion);
            var errCode = 0;
            var errMsg = string.Empty;
            var input = new AntSdkAddListContactsInput { dataVersion = newVersion.ToString() };
            //Thread.Sleep(100);
            var addListContactsOutput = AntSdkService.AntSdkGetAddContacts(input, ref errCode, ref errMsg);
            if (addListContactsOutput != null)//查询成功
            {

                MainWindowVM.UpdateOrganizationModify(addListContactsOutput);
                var departsAddCount = addListContactsOutput.departs.add?.Count ?? 0;
                var departsupdateCount = addListContactsOutput.departs.update?.Count ?? 0;
                var departsdeleteCount = addListContactsOutput.departs.delete?.Count ?? 0;

                var usersaddCount = addListContactsOutput.users.add?.Count ?? 0;
                var usersupdateCount = addListContactsOutput.users.update?.Count ?? 0;
                var usersdeleteCount = addListContactsOutput.users.delete?.Count ?? 0;

                LogHelper.WriteWarn("组织结构增量数据部门add：------" + departsAddCount + "  部门update：------" + departsupdateCount + "   部门delete：------" + departsdeleteCount);
                LogHelper.WriteWarn("组织结构增量数据用户add：------" + usersaddCount + "  用户update：------" + usersupdateCount + "   用户delete：------" + usersdeleteCount);
            }
            else//显示错误信息
            {

            }
        }
        /// <summary>
        /// 同步讨论组免打扰设置
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUserMsg_GroupState(AntSdkReceivedOtherMsg.Individuation sysMsg)
        {
            int state = 0;
            int.TryParse(sysMsg.content.state, out state);
            _SessionListViewModel?.UpdateMsgRemind(sysMsg.content?.targetId, state);
            GroupListViewModel?.UpdateMsgRemind(sysMsg.content?.targetId, state);
        }

        /// <summary>
        /// 通知——登录推送
        /// </summary>
        /// <param name="sysMsg"></param>
        public static void HandleSysUerMsg_NoticeLogin(AntSdkReceivedOtherMsg.Notifications.UnRead ctt, bool isLast)
        {
            string errMsg = string.Empty;
            //SysUserMsgReceive_Notice_Login sysMsg = new SysUserMsgReceive_Notice_Login();
            //if (!DataConverter.DeserializeJson(ctt, ref sysMsg, ref errMsg)) return;
            if (_SessionListViewModel != null)
            {
                if (ctt.content == null) return;
                //foreach (AntSdkReceivedOtherMsg.Notifications.Notification_content content in ctt.content)
                //{

                var vm = GroupListViewModel.GroupInfos.FirstOrDefault(c => c.groupId == ctt.targetId);
                if (vm == null) return;
                List<Notice_content> noticeContentList = new List<Notice_content>();
                foreach (var notification in ctt.content)
                {
                    Notice_content noticeContent = new Notice_content();
                    if (ctt.content == null) return;
                    noticeContent.createId = notification.createBy;
                    noticeContent.createTime = notification.createTime;
                    noticeContent.groupId = ctt.sessionId;
                    noticeContent.notificationId = notification.notificationId;
                    noticeContent.targetId = ctt.targetId;
                    noticeContent.title = notification.title;
                    noticeContent.hasAttach = string.IsNullOrEmpty(notification.hasAttach) ? 0 : int.Parse(notification.hasAttach);
                    noticeContentList.Add(noticeContent);
                }
                _SessionListViewModel?.AddNotice(vm, noticeContentList, isLast);
                //}
            }
        }
        /// <summary>
        /// 打卡推送消息处理
        /// </summary>
        /// <param name="msg"></param>
        public static void HandleSysMsg_AttendanceRecordVerify(AntSdkReceivedOtherMsg.AttendanceRecordVerify msg, bool isVerify = true)
        {
            _SessionListViewModel?.AddAttendanceRecords(msg);
            if (isVerify)
                MainWindowVM?.AttendanceRecordVerify(msg.sendTime, msg.content, msg.MsgType);
        }
        /// <summary>
        /// 通知——添加推送
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_NoticeAdd(AntSdkReceivedOtherMsg.Notifications.Add ctt)
        {
            string errMsg = string.Empty;
            //SysUserMsgReceive_Notice_Add sysMsg = new SysUserMsgReceive_Notice_Add();
            //if (!DataConverter.DeserializeJson(ctt, ref sysMsg, ref errMsg)) return;
            if (ctt.sendUserId == AntSdkService.AntSdkLoginOutput.userId) return;
            if (_SessionListViewModel != null)
            {
                var vm = GroupListViewModel.GroupInfos.FirstOrDefault(c => c.groupId == ctt.targetId);
                if (vm == null) return;
                List<Notice_content> noticeContentList = new List<Notice_content>();
                Notice_content noticeContent = new Notice_content();
                if (ctt.content == null) return;
                noticeContent.createId = ctt.sendUserId;
                noticeContent.createTime = ctt.sendTime;
                noticeContent.groupId = ctt.sessionId;
                noticeContent.notificationId = ctt.content.notificationId;
                noticeContent.targetId = ctt.targetId;
                noticeContent.title = ctt.content.title;
                noticeContent.hasAttach = string.IsNullOrEmpty(ctt.content.hasAttach) ? 0 : int.Parse(ctt.content.hasAttach);
                noticeContentList.Add(noticeContent);
                string errorMsg = string.Empty;

                _SessionListViewModel.AddNotice(vm, noticeContentList);
            }
        }
        /// <summary>
        /// 通知——删除推送
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_NoticeDelete(AntSdkReceivedOtherMsg.Notifications.Delete sysMsg)
        {
            _SessionListViewModel?.RemoveNotice(sysMsg.content?.notificationId, true);
        }

        /// <summary>
        /// 通知——修改已读状态推送
        /// </summary>
        /// <param name="sysMsg"></param>
        private static void HandleSysUerMsg_NoticeRead(AntSdkReceivedOtherMsg.Notifications.State sysMsg)
        {
            _SessionListViewModel?.RemoveNotice(sysMsg.content?.notificationId, false);
        }
        /// <summary>
        /// 管理员将群切换成普通模式或阅后即焚模式
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <param name="msgType"></param>
        private static void HandleGroupBurnAfterReadMode(AntSdkReceivedGroupMsg.GroupBase groupInfo, AntSdkMsgType msgType)
        {
            if (groupInfo == null)
                return;
            //var groupInfo = (AntSdkReceivedGroupMsg.GroupBase)sysMsg;
            _SessionListViewModel?.HandleGroupBurnAfterReadMode(groupInfo, msgType, groupInfo.sessionId);
            var vm = GroupListViewModel.GroupInfoList?.FirstOrDefault(c => c.GroupInfo.groupId == groupInfo.sessionId);
            if (vm != null)
            {
                if (msgType == AntSdkMsgType.GroupOwnerBurnMode
                    || msgType == AntSdkMsgType.GroupOwnerBurnDelete)
                {
                    vm.IsBurnMode = GlobalVariable.BurnFlag.IsBurn;
                }
                else
                {
                    vm.IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
                }
            }
        }



        #endregion
    }

    public class MessageInfo
    {
        public GlobalVariable.BurnFlag IsBurnFlag { get; set; }
        public string ChatIndex { get; set; }
        public int Count { get; set; }
        public string MessageId { get; set; }
    }

    public class Dictionary<TKey1, TKey2, TValue> : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue>
    {

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get { return base[Tuple.Create(key1, key2)]; }
            set { base[Tuple.Create(key1, key2)] = value; }
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            base.Add(Tuple.Create(key1, key2), value);
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return base.ContainsKey(Tuple.Create(key1, key2));
        }
    }
}
