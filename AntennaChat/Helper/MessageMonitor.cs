/*
Author: tanqiyan
Crate date: 2017-06-14
Description：消息控制类
--------------------------------------------------------------------------------------------------------
Versions：
V1.00 2017-06-14 tanqiyan 描述：消息接收入口
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Helper.IHelper;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;
using static Antenna.Model.SendMessageDto;

namespace AntennaChat.Helper
{
    public class MessageMonitor
    {
        //static readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        static object objct = new object();
        static readonly T_Chat_Message_GroupDAL t_groupChat = new T_Chat_Message_GroupDAL();
        static readonly T_Chat_Message_GroupBurnDAL t_groupBurnChat = new T_Chat_Message_GroupBurnDAL();
        static readonly T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
        static readonly BaseBLL<AntSdkTsession, T_SessionDAL> t_sessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
        private static bool isFlag = false;
        static StringBuilder messageIds = new StringBuilder();

        private static List<AntSdkMsBase> _offlinePointBurnReadeds =
            new List<AntSdkMsBase>();
        #region 静态私有变量
        /// <summary>
        /// 消息控制器集合
        /// </summary>
        private static List<IMessageHelper> _lstMessageHelper;

        public static List<string> OffLineMsgSession = new List<string>();
        private static List<AntSdkMsBase> GroupBurnModeList = new List<AntSdkMsBase>();
        /// <summary>
        /// 本地未读消息缓存集合
        /// </summary>
        private static Dictionary<string, GlobalVariable.BurnFlag, List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>> LocalUnreadMsgList;
        /// <summary>
        /// 所有的离线消息缓存集合
        /// </summary>
        private static Dictionary<string, GlobalVariable.BurnFlag, List<AntSdkChatMsg.ChatBase>> OfflineMsgs;
        #endregion
        /// <summary>
        /// 信息到达事件(未被窗体接收)
        /// </summary>
        public static event Action<AntSdkMsgType, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> InstantMessageWaitingToReceive;


        /// <summary>
        /// 系统信息到达事件(未被窗体接收)
        /// </summary>
        public static event Action<AntSdkMsBase, AntSdkMsgType> InstantSystemMessageWaitingToReceive;

        /// <summary>
        /// 消息助手信息
        /// </summary>
        public static event Action<AntSdkChatMsg.ChatBase> InstantMassMsgWaitingToReceive;

        /// <summary>
        /// 收到消息服务转发的消息销毁的通知
        /// </summary>
        public static event Action<AntSdkChatMsg.ChatBase, AntSdkMsgType> InstatntMsessageDestroyToReceive;

        /// <summary>
        /// 手机端发送的已读回调
        /// </summary>
        public static event Action<AntSdkReceivedOtherMsg.MultiTerminalSynch> InstatantPhoneReadSessionToReceive;

        /// <summary>
        /// 版本更新事件
        /// </summary>
        public static event Action<AntSdkReceivedOtherMsg.VersionHardUpdate> VersionUpdatedToReceive;

        static MessageMonitor()
        {

        }

        /// <summary>
        /// MQTT消息接收，触角SDK的消息类型：SDK.AntSdk.AntSdkMsgType
        /// </summary>
        /*消息类型枚举：
         * UnDefineMsg:未定义消息类型[初始化类型使用]
         * AllMessage :所有类型消息[注册回调时不区分消息类型进行回调接收]
         * MultiTerminalSynch:多终端同步已读回执,例如：用户A（android端）发送已读回执时，sdkmessage收到这条消息后，会同时向用户A的ID发送已读回执的消息，用来做多终端同步，用户A的其他端收到同步消息后 ,1）如果OS和自己一样，那么不需要处理 ;2）如果OS和自己不一样，那么表示其他端已读该条消息，自己也要将该条消息标识为已读;
         * MsgReceipt:收到消息服务回执:目前只有自己发的聊天消息会收到该回执
         * PointFileAccepted:点对点文件消息的已接受的回执
         * PointBurnReaded:点对点阅后即焚消息已读的回执,适用场景：用户A发送阅后即焚消息给用户B，用户B发送点对点阅后即焚已读回执，用户A收到该条消息之后，解析content中的readIndex，删除对应的消息，并且需要向服务端发送已读回执
         * UnvarnishedMsg:透传自定义消息（目前乐盈通使用，SDK后续定义4000-9999的消息类型为自定义类型, 暂时保留乐盈通的1998透传消息）
         * VersionHardUpdate:硬更新
         * OrganizationModify:组织架构变更
         * OffLine:用户离线
         * OnLine:用户在线
         * Leave:用户离开
         * Busy:用户忙碌
         * Disable:用户账号被停用
         * ModifyInfo:用户信息变更（包括头像、名称、个性签名等信息的变更）
         * PaswordChangeKickOut:用户修改密码，踢出登录
         * KickOut:用户被踢出登录
         * ChatMsgText:收到的聊天消息(文本)（需要发送回执）
         * ChatMsgPicture:收到的聊天消息(图片)（需要发送回执）
         * ChatMsgAudio:收到的聊天消息(音频)（需要发送回执）
         * ChatMsgVideo:收到的聊天消息(视频)（需要发送回执）
         * ChatMsgFile:收到的聊天消息(文件)（需要发送回执）
         * ChatMsgMapLocation:收到的聊天消息(地理位置)（需要发送回执）
         * ChatMsgMixImageText:收到的聊天消息(图文混合)（需要发送回执）
         * ChatMsgAt:收到的聊天消息(@消息)（需要发送回执）
         * ChatMsgMultiAudioVideo:收到的聊天消息(多人视频)（需要发送回执）
         * CreateChatRoom:用户新增聊天室(需要发送已读回执)
         * DismissChatRoom:解散聊天室（需要发送已读回执）
         * DeleteChatRoomMember:聊天室删除成员（需发送已读回执）
         * QuitChatRoomMember:成员退出聊天室（需发送已读回执）
         * AddChatRoomMember:聊天室添加成员（需要发送已读回执）
         * ModifyChatRoomMember:聊天室更新成员信息（需要发送已读回执）
         * ModifyChatRoom:聊天室信息变更（需要发送已读回执）
         * CreateGroup:用户新增讨论组(需要发送已读回执)
         * DissolveGroup:讨论组解散（需要发送已读回执）
         * AddGroupMember:讨论组新增成员（需要发送已读回执）
         * DeleteGroupMember:讨论组删除成员（需要发送已读回执）
         * QuitGroupMember:成员退出讨论组（需要发送已读回执）
         * ModifyGroupMember:讨论组成员信息变更(需要发送已读回执)
         * ModifyGroup:讨论组信息变更(需要发送已读回执)
         * GroupOwnerBurnMode:群主切换为阅后即焚
         * GroupOwnerBurnDelete:群主在阅后即焚模式下清空消息
         * GroupOwnerNormal:群主切换为普通消息模式
         * UnReadNotifications:用户登录时获取讨论组公告列表（用户未读的）
         * AddNotification:新增讨论组公告
         * DeleteNotification:删除讨论组公告
         * ModifyNotificationState:修改公告状态为已读（多终端同步）
         * CustomMessage:自定义消息[4000,9999]
         * OffLineMessage:离线消息
         * MultiUserSend:群发助手消息[消息发送群发时必须传，需要使用特殊sessionId = id,id2点对点聊天sessionId的生成 id为用户自己的ID id2固定为900000]
         * EnterLine:回车换行消息类型
         * HeartBeat:表示心跳消息，终端没有收发消息时，每隔60秒发送一次心跳
         * OffLineMsgRequest:请求离线消息，终端上线发送
         * SingleAudioVideo:单人（点对点聊天）音频视频(需要发送回执)
         * InviteJoin:邀请加入会话
         * HandleInvite:处理邀请会话(同意或拒绝邀请)
         */
        public static void Start()
        {
            _lstMessageHelper = new List<IMessageHelper>();
            OfflineMsgs = new Dictionary<string, GlobalVariable.BurnFlag, List<AntSdkChatMsg.ChatBase>>();
            LocalUnreadMsgList = new Dictionary<string, GlobalVariable.BurnFlag, List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>>();
            //即时聊天消息接收：Text[文本消息]、Picture[图片消息]、Audio[音频消息]、Video[视频消息]、File[文件消息]、MapLocation[地理位置]、MixImageText[图文混合]、At[@消息]、MultiAudioVideo[多人音/视频消息]
            //实体访问方式，例如文本消息：SDK.AntSdk.AntModels.AntSdkChatMsg.Text
            //                  图片消息：SDK.AntSdk.AntModels.AntSdkChatMsg.Picture ....

            //其他信息接：MsgReceipt[消息服务回执]、MultiTerminalSynch[多终端同步的已读回执]、PointFileAccepted[点对点文件消息的已接收的回执]、PointBurnReaded[点对点阅后即焚消息已读的回执]、VersionHardUpdate[硬更新通知]、OrganizationModify[组织结构变更通知]、UnvarnishedMsg[透传暂时不管]
            //讨论组公告：UnRead[未读公告]、Add[添加公告]、State[修改公告状态为已读（多终端同步）]、Delete[删除讨论组公告]
            //自动以信息：Custom[自定义消息]
            //实体访问方式，例如回执消息：SDK.AntSdk.AntModels.AntSdkReceivedOtherMsg.MsgReceipt
            //              未读公告消息：SDK.AntSdk.AntModels.AntSdkReceivedOtherMsg.Notifications.UnRead
            //              添加公告消息：SDK.AntSdk.AntModels.AntSdkReceivedOtherMsg.Notifications.Add
            AntSdkService.MsOtherReceived += AntSdkService_MsOtherReceived;

            //讨论组通知信息接收:Create[创建]、Modify[修改]、Delete[解散]、AddMembers[添加成员]、QuitMember[成员退出]、ModifyMember[修改成员信息]、OwnerBurnMode[群主切换为阅后即焚]、OwnerBurnDelete[群主阅后即焚下删除消息]、OwnerNormal[群主切换为普通模式]
            //实体访问方式，例如创建消息：SDK.AntSdk.AntModels.AntSdkReceivedGroupMsg.Create
            //                  删除消息：SDK.AntSdk.AntModels.AntSdkReceivedGroupMsg.Delete
            AntSdkService.MsGroupReceived += AntSdkService_MsGroupReceived;

            //用户通知信息接收：State[用户状态变化：AntSdkMsgType.OffLine[离线]|OnLine[在线]|Leave[离开]|Busy[忙碌]|Disable[账号被停用]|PaswordChangeKickOut[户修改密码，踢出登录]|KickOut[用户被踢出登录]]、Modify[用户信息修改]
            AntSdkService.MsUsersReceived += AntSdkService_MsUsersReceived;

            AntSdkService.MsChatsReceived += AntSdkService_MsChatsReceived;

            //离线消息接收[所有聊天消息]：[所有聊天消息基类数组]
            AntSdkService.MsOfflineReceivedChatMsg += AntSdkService_MsOfflineReceived;

            //离线消息接收[所有聊天消息之外的消息]：[所有消息基类数组]
            AntSdkService.MsOfflineReceivedOtherMsg += AntSdkService_MsOfflineReceived;
        }

        //读取本地离线数据
        public static void LoadLocalUnReadMsgData()
        {
            //读取本地离线数据
            //ThreadPool.QueueUserWorkItem(m => SearchLocalUnreadMsg());
            SearchLocalUnreadMsg();
        }

        /// <summary>
        /// 方法说明：在线消息接收
        /// </summary>
        /// <param name="msgType">消息类型</param>
        /// <param name="contentObject">消息内容[已转换为实体信息]</param>
        private static void AntSdkService_MsChatsReceived(AntSdkMsgType msgType, object contentObject)
        {
            var msgBase = (AntSdkChatMsg.ChatBase)contentObject;
            if (IsRepeatChatMsg(msgBase)) return;
            HandleOnlineMessage(msgType, contentObject);
        }
        /// <summary>
        /// 讨论组系统信息接收
        /// </summary>
        private static void AntSdkService_MsGroupReceived(AntSdkMsgType msgType, object contentObject)
        {
            if (!(contentObject is AntSdkMsBase)) return;
            var msgBase = (AntSdkReceivedGroupMsg.GroupBase)contentObject;
            if (msgBase.MsgType != AntSdkMsgType.GroupOwnerNormal
                && msgBase.MsgType != AntSdkMsgType.GroupOwnerBurnMode
                && msgBase.MsgType != AntSdkMsgType.GroupOwnerBurnDelete)
            {
                if (IsRepeatSysMsg(msgBase)) return;
                PublicMessageFunction.SendSysUserMsgReceipt(msgBase.chatIndex, msgBase.sessionId, msgBase.MsgType);
            }
            else
            {
                if (GroupBurnModeList.Exists(m => m.sessionId == msgBase.sessionId))
                {
                    GroupBurnModeList.Remove(GroupBurnModeList.FirstOrDefault(m => m.sessionId == msgBase.sessionId));
                }
                GroupBurnModeList.Add(msgBase);
            }
            InstantSystemMessageWaitingToReceive?.Invoke(msgBase, msgType);
        }
        /// <summary>
        /// 用户系统信息接收
        /// </summary>
        private static void AntSdkService_MsUsersReceived(AntSdkMsgType msgType, object contentObject)
        {
            if (!(contentObject is AntSdkMsBase)) return;
            var msgBase = (AntSdkMsBase)contentObject;
            InstantSystemMessageWaitingToReceive?.Invoke(msgBase, msgType);

        }
        /// <summary>
        /// 其它系统消息接收
        /// </summary>
        private static void AntSdkService_MsOtherReceived(AntSdkMsgType msgType, object contentObject)
        {
            if (!(contentObject is AntSdkMsBase)) return;
            var sdkMsgBase = contentObject as AntSdkMsBase;
            if (!string.IsNullOrEmpty(sdkMsgBase.sessionId) && !string.IsNullOrEmpty(sdkMsgBase.chatIndex))
            {
                var isResend = false;
                if (sdkMsgBase.MsgType == AntSdkMsgType.MsgReceipt)
                {
                    var recievedMsg = contentObject as AntSdkReceivedOtherMsg.MsgReceipt;
                    if (recievedMsg != null)
                    {
                        AttrDto attrDto = JsonConvert.DeserializeObject<AttrDto>(recievedMsg.attr);
                        if (attrDto?.isResend == "1")
                        {
                            isResend = true;
                        }
                    }
                }

                if (!isResend)
                    if (IsRepeatSysMsg(sdkMsgBase)) return;
                if (sdkMsgBase.MsgType == AntSdkMsgType.CheckInVerify || sdkMsgBase.MsgType == AntSdkMsgType.CheckInResult)
                {
                    var checkInVerifyMsg = sdkMsgBase as AntSdkReceivedOtherMsg.AttendanceRecordVerify;
                    if (checkInVerifyMsg != null)
                        SessionMonitor.HandleSysMsg_AttendanceRecordVerify(checkInVerifyMsg);
                }
                if (sdkMsgBase.MsgType != AntSdkMsgType.MsgReceipt
                    && sdkMsgBase.MsgType != AntSdkMsgType.PointBurnReaded
                    && sdkMsgBase.MsgType != AntSdkMsgType.MultiTerminalSynch
                    && sdkMsgBase.MsgType != AntSdkMsgType.VersionHardUpdate
                    && sdkMsgBase.MsgType != AntSdkMsgType.OrganizationModify
                    && sdkMsgBase.MsgType != AntSdkMsgType.PaswordChangeKickOut
                    && sdkMsgBase.MsgType != AntSdkMsgType.KickOut
                    )
                    PublicMessageFunction.SendSysUserMsgReceipt(sdkMsgBase.chatIndex, sdkMsgBase.sessionId,
                        sdkMsgBase.MsgType);
            }
            switch (msgType)
            {
                case AntSdkMsgType.MsgReceipt:
                    HandleOnlineMessage(msgType, contentObject);
                    break;
                case AntSdkMsgType.Revocation:
                    var revocationMsg = contentObject as AntSdkChatMsg.Revocation;
                    if (revocationMsg != null)
                    {
                        AntSdkChatMsg.ChatBase msg = new AntSdkChatMsg.ChatBase();
                        msg.sendUserId = revocationMsg.sendUserId;
                        msg.messageId = revocationMsg.content?.messageId;
                        msg.targetId = revocationMsg.targetId;
                        //msg.companyCode = /*ctt.companyCode*/ AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode;
                        msg.chatIndex = revocationMsg.chatIndex;
                        msg.sessionId = revocationMsg.sessionId;
                        msg.chatType = revocationMsg.chatType;
                        msg.sendTime = revocationMsg.sendTime;
                        msg.sourceContent = /*ctt.content*/ revocationMsg.messageId;
                        InstatntMsessageDestroyToReceive?.Invoke(msg, msgType);
                    }
                    break;
                case AntSdkMsgType.PointBurnReaded:
                    var receivedMsg = contentObject as AntSdkReceivedOtherMsg.PointBurnReaded;
                    if (receivedMsg != null)
                    {
                        AntSdkChatMsg.ChatBase msg = new AntSdkChatMsg.ChatBase();
                        msg.sendUserId = receivedMsg.sendUserId;
                        msg.messageId = receivedMsg.content?.messageId;
                        msg.targetId = receivedMsg.targetId;
                        //msg.companyCode = /*ctt.companyCode*/ AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode;
                        msg.chatIndex = receivedMsg.chatIndex;
                        msg.sessionId = receivedMsg.sessionId;
                        msg.os = receivedMsg.os;
                        msg.sourceContent = /*ctt.content*/ receivedMsg.content?.readIndex.ToString();
                        InstatntMsessageDestroyToReceive?.Invoke(msg, msgType);
                    }
                    break;
                case AntSdkMsgType.MultiTerminalSynch:
                    var multiTerminalSynch = contentObject as AntSdkReceivedOtherMsg.MultiTerminalSynch;
                    InstatantPhoneReadSessionToReceive?.Invoke(multiTerminalSynch);
                    break;
                case AntSdkMsgType.VersionHardUpdate:
                    var versionHardUpdate = contentObject as AntSdkReceivedOtherMsg.VersionHardUpdate;
                    VersionUpdatedToReceive?.Invoke(versionHardUpdate);
                    break;
                default:
                    var msgBase = contentObject as AntSdkMsBase;
                    InstantSystemMessageWaitingToReceive?.Invoke(msgBase, msgType);
                    break;
            }
        }
        /// <summary>
        /// //离线消息接收：[所有消息基类数组]
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="contentObject"></param>
        private static void AntSdkService_MsOfflineReceived(AntSdkMsgType msgType, object contentObject)
        {

            if (msgType == AntSdkMsgType.OffLineMessageOtherMsg)
            {
                var othermsgList = contentObject as Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>;
                if (othermsgList != null && othermsgList.Values != null && othermsgList.Values.Count > 0)
                {
                    int i = 0;
                    _offlinePointBurnReadeds = othermsgList.Values.Where(
                        offlineMsg => offlineMsg.MsgType == AntSdkMsgType.PointBurnReaded).ToList();


                    //var tempOtherMsgList = othermsgList.Where(offlineMsg => offlineMsg.Key != null);
                    var checkInVerifyMsgList = othermsgList.Values.Where(m => m.MsgType == AntSdkMsgType.CheckInVerify && m.sessionId == AntSdkService.AntSdkCurrentUserInfo.userId).ToList();
                    var checkInResultMsgList = othermsgList.Values.Where(m => m.MsgType == AntSdkMsgType.CheckInResult && m.sessionId == AntSdkService.AntSdkCurrentUserInfo.userId).ToList();
                    if (checkInVerifyMsgList != null && checkInVerifyMsgList.Count > 0)
                    {
                        var checkInVerifyMsg = checkInVerifyMsgList[checkInVerifyMsgList.Count - 1] as AntSdkReceivedOtherMsg.AttendanceRecordVerify;
                        var checkInResult = checkInResultMsgList.Count > 0 ? checkInResultMsgList[checkInResultMsgList.Count - 1] as AntSdkReceivedOtherMsg.AttendanceRecordVerify : null;
                        if (checkInVerifyMsg != null)
                            SessionMonitor.HandleSysMsg_AttendanceRecordVerify(checkInVerifyMsg, checkInResult?.content?.attendId != checkInVerifyMsg.content?.attendId);

                    }
                    //其他离线消息处理[普通聊天消息外的消息]
                    foreach (var offlineMsg in othermsgList.Where(offlineMsg => offlineMsg.Key != null))
                    {
                        switch (offlineMsg.Key.MsgType)
                        {
                            case AntSdkMsgType.PointBurnReaded:
                                var receivedMsg = offlineMsg.Value as AntSdkReceivedOtherMsg.PointBurnReaded;
                                if (receivedMsg == null) return;
                                if (IsRepeatSysMsg(receivedMsg)) return;
                                if (i == 0)
                                    SessionMonitor.RemoveSessionInfoList(receivedMsg.sessionId, GlobalVariable.BurnFlag.NotIsBurn);

                                HandleBurnAfterReadReceipt(receivedMsg, i == othermsgList.Count - 1);
                                i++;
                                break;
                            case AntSdkMsgType.UnReadNotifications:
                                var unReadNotification = offlineMsg.Value as AntSdkReceivedOtherMsg.Notifications.UnRead;
                                if (unReadNotification != null)
                                    SessionMonitor.HandleSysUerMsg_NoticeLogin(unReadNotification, i == othermsgList.Count - 1);
                                i++;
                                break;
                            //case AntSdkMsgType.Revocation:
                            //    var revocationMsg = offlineMsg.Value as AntSdkChatMsg.Revocation;
                            //    if (revocationMsg != null)
                            //        if (IsRepeatSysMsg(revocationMsg)) return;
                            //    PublicMessageFunction.SendSysUserMsgReceipt(offlineMsg.Key.chatIndex,
                            //        offlineMsg.Key.sessionId, offlineMsg.Key.MsgType);
                            //    RevocationMsgChanageLocalData(revocationMsg, i == othermsgList.Count - 1);
                            //    i++;
                            //    break;
                            default:
                                if (offlineMsg.Key.MsgType != AntSdkMsgType.MsgReceipt)
                                    PublicMessageFunction.SendSysUserMsgReceipt(offlineMsg.Key.chatIndex,
                                        offlineMsg.Key.sessionId, offlineMsg.Key.MsgType);
                                break;
                        }
                    }

                }
            }
            else if (msgType == AntSdkMsgType.OffLineMessageChatMsg)
            {
                var offlineMsgList = contentObject as List<AntSdkChatMsg.ChatBase>;
                //List < AntSdkChatMsg.ChatBase > chatMsgList = offlineMsgList.Where(c => c.MsgType == AntSdkMsgType.ChatMsgText 
                // || c.MsgType == AntSdkMsgType.ChatMsgPicture
                // || c.MsgType == AntSdkMsgType.ChatMsgFile
                // || c.MsgType == AntSdkMsgType.ChatMsgAudio 
                // || c.MsgType == AntSdkMsgType.ChatMsgAt 
                // || c.MsgType == AntSdkMsgType.ChatMsgVideo).ToList();
                if (offlineMsgList == null) return;
                //离线聊天消息处理
                if (offlineMsgList.Any())
                    HandleOfflineMqttMessage(offlineMsgList);
            }
            else
            {
                //TODO:ERROR Log
            }
        }


        /// <summary>
        /// 方法说明：在线聊天消息接收
        /// </summary>
        /// <param name="msgType">消息类型</param>
        /// <param name="contentObject">消息内容[已转换为实体信息]</param>
        private static void HandleOnlineMessage(AntSdkMsgType msgType, object contentObject)
        {
            var chatMsg = contentObject as AntSdkChatMsg.ChatBase;
            if (chatMsg == null)
            {
                if ((msgType & AntSdkMsgType.MsgReceipt) != 0)
                {
                    var recievedMsg = contentObject as AntSdkReceivedOtherMsg.MsgReceipt;
                    if (recievedMsg != null)
                    {
                        if (string.IsNullOrEmpty(recievedMsg.messageId)) return;
                        AntSdkChatMsg.ChatBase tempChatBase = new AntSdkChatMsg.ChatBase();
                        tempChatBase.messageId = recievedMsg.messageId;
                        tempChatBase.sessionId = recievedMsg.sessionId;
                        tempChatBase.chatIndex = recievedMsg.chatIndex;
                        tempChatBase.sendTime = recievedMsg.sendTime;
                        tempChatBase.attr = recievedMsg.attr;
                        tempChatBase.os = (int)GlobalVariable.OSType.PC;
                        var messageHelper = GetMessageHelper(recievedMsg.sessionId);
                        if (messageHelper != null)
                        {
                            tempChatBase.sendUserId = AntSdkService.AntSdkLoginOutput.userId;
                            SessionMonitor.UpdateSeesionItemOnLineMsg(msgType, recievedMsg);
                            messageHelper.TriggerInstantMessageHasBeenReceivedEvent((int)msgType, tempChatBase);
                        }
                        else
                        {

                            InstantMassMsgWaitingToReceive?.Invoke(tempChatBase);
                        }

                    }
                }
                return;
            }
            try
            {
                if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId &&
                    chatMsg.os == (int)GlobalVariable.OSType.PC)
                {
                    return;
                }
                var antSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.sendUserId);
                if (antSdkContact_User == null) return;
                var messageHelper = GetMessageHelper(chatMsg.sessionId);
                if (msgType == AntSdkMsgType.Revocation)
                {
                    if (chatMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId)
                    {
                        chatMsg.sourceContent = "你" + GlobalVariable.RevocationPrompt.Msessage;
                    }
                    else
                    {

                        if (chatMsg.chatType == (int)AntSdkchatType.Point)
                        {
                            chatMsg.sourceContent = "对方" + GlobalVariable.RevocationPrompt.Msessage;
                        }
                        else
                        {
                            var antSdkContactUser = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.sendUserId);
                            chatMsg.sourceContent = antSdkContactUser?.userNum + antSdkContactUser?.userName +
                                      GlobalVariable.RevocationPrompt.Msessage;
                        }

                    }

                }
                else if (chatMsg.MsgType == AntSdkMsgType.DeleteVote)
                {
                    var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)chatMsg;
                    if (tempChatMsg.content != null)
                    {
                        var baseChatMsg =
                            t_groupChat.GetGroupMsgByVoteOrActivityId(tempChatMsg.content.id.ToString(),
                                tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateVote).ToString(),
                                AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                        if (baseChatMsg != null)
                            chatMsg.messageId = baseChatMsg.messageId;
                    }
                }
                else if (chatMsg.MsgType == AntSdkMsgType.DeleteActivity)
                {
                    var tempChatMsg = (AntSdkChatMsg.DeleteActivityMsg)chatMsg;
                    if (tempChatMsg.content != null)
                    {
                        var baseChatMsg =
                            t_groupChat.GetGroupMsgByVoteOrActivityId(tempChatMsg.content.activityId.ToString(),
                                tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateActivity).ToString(),
                                AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                        if (baseChatMsg != null)
                            chatMsg.messageId = baseChatMsg.messageId;
                    }
                }
                //如果信息辅助类为null，则证明没有任何窗体可接收此信息，触发即时信息等待接收事件
                if (messageHelper == null || !messageHelper.LocalWindowHelper.IsCurrentTalkWin)
                {
                    InstantMessageWaitingToReceive?.Invoke(msgType, chatMsg);
                }
                else
                {
                    SessionMonitor.AddSessionItemOnlineMsg(msgType, chatMsg);
                    messageHelper.TriggerInstantMessageHasBeenReceivedEvent((int)msgType, chatMsg);
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[HandleChatMsg]:" + e.Message + "," + e.StackTrace);
            }
        }


        //消息控制结束
        public static void End()
        {
            //即时聊天消息接收
            AntSdkService.MsChatsReceived -= AntSdkService_MsChatsReceived;
            //讨论组通知信息接收
            AntSdkService.MsGroupReceived -= AntSdkService_MsGroupReceived;
            //用户信息接收：
            AntSdkService.MsUsersReceived -= AntSdkService_MsUsersReceived;
            //其他信息接收:
            AntSdkService.MsOtherReceived -= AntSdkService_MsOtherReceived;
            //离线消息接收[所有聊天消息]
            AntSdkService.MsOfflineReceivedChatMsg -= AntSdkService_MsOfflineReceived;
            //离线消息接收
            AntSdkService.MsOfflineReceivedOtherMsg -= AntSdkService_MsOfflineReceived;
            OfflineMsgs = null;
            LocalUnreadMsgList = null;
            _lstMessageHelper = null;
        }

        /// <summary>
        /// 接收SDK离线消息
        /// </summary>
        /// <param name="offlineMsgList"></param>
        private static void HandleOfflineMqttMessage(List<AntSdkChatMsg.ChatBase> offlineMsgList)
        {
            ThreadPool.QueueUserWorkItem(m => ReceiveOfflineMessage(offlineMsgList));
        }

        /// <summary>
        /// 缓存离线消息
        /// </summary>
        /// <param name="chatMsgList"></param>
        private static void ReceiveOfflineMessage(List<AntSdkChatMsg.ChatBase> chatMsgList)
        {
            lock (objct)
            {
                //if (OffLineMsgSession.Contains(chatMsgList[0].sessionId))
                //{
                //    var lastChatMsg = chatMsgList[chatMsgList.Count - 1];
                //    PublicMessageFunction.SendChatMsgReceipt(lastChatMsg.sessionId, lastChatMsg.chatIndex, lastChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                //    LogHelper.WriteDebug("[重复离线消息发送最后一条消息的已收回执]--------------sessionId：" + lastChatMsg.sessionId + "---------chatIndex：" + lastChatMsg.chatIndex + "-----------sendUserId" + lastChatMsg.sendUserId);
                //    return;
                //}
                OffLineMsgSession.Add(chatMsgList[0].sessionId);

                Tuple<string, GlobalVariable.BurnFlag> tuple2 = null;
                int maxChatIndex, offlineMaxChatIndex = 0;
                //SessionMonitor.RemoveSessionInfoList(chatMsgList[0].sessionId, chatMsgList[chatMsgList.Count-1].flag==1? GlobalVariable.BurnFlag.IsBurn: GlobalVariable.BurnFlag.NotIsBurn);
                if (chatMsgList[0].chatType == (int)AntSdkchatType.Group) //讨论组消息
                {
                    var chatBrunMsgLst = chatMsgList.Where(m => m.flag == (int)GlobalVariable.BurnFlag.IsBurn);
                    var offlineBrunMsgReceives = chatBrunMsgLst as List<AntSdkChatMsg.ChatBase> ?? chatBrunMsgLst.ToList();
                    var brunCount = offlineBrunMsgReceives.Count;
                    if (brunCount > 0)
                    {
                        var maxLocalChatIndex = t_groupBurnChat.getQueryZeroChatIndex(offlineBrunMsgReceives[0].sessionId,
                             AntSdkService.AntSdkLoginOutput.companyCode, AntSdkService.AntSdkLoginOutput.userId);
                        var tempOfflineMaxChatIndex = offlineBrunMsgReceives[offlineBrunMsgReceives.Count - 1].chatIndex;
                        int.TryParse(maxLocalChatIndex, out maxChatIndex);
                        int.TryParse(tempOfflineMaxChatIndex, out offlineMaxChatIndex);
                        tuple2 = Tuple.Create(offlineBrunMsgReceives[0].sessionId, GlobalVariable.BurnFlag.IsBurn);
                        if (!OfflineMsgs.ContainsKey(tuple2) && offlineMaxChatIndex > maxChatIndex)
                        {
                            //SessionMonitor.RemoveSessionInfoList(offlineBrunMsgReceives[0].sessionId, GlobalVariable.BurnFlag.IsBurn);
                            //缓存离线消息
                            OfflineMsgs.Add(tuple2, offlineBrunMsgReceives);
                            var unReadOfflineMsgList = offlineBrunMsgReceives.Where(m => m.status == 0 && (m.MsgType != AntSdkMsgType.Revocation) && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                            //记录离线消息数量
                            RecordOffLineMsgCount(offlineBrunMsgReceives, unReadOfflineMsgList.Count, GlobalVariable.BurnFlag.IsBurn);
                            isFlag = true;
                        }
                        else
                        {
                            if (OfflineMsgs.ContainsKey(tuple2))
                            {
                                var repeatCount = 0;
                                foreach (var offlineBrunMsg in offlineBrunMsgReceives)
                                {
                                    var tempOfflineMsg =
                                        OfflineMsgs[tuple2].FirstOrDefault(m => m.messageId == offlineBrunMsg.messageId);
                                    if (tempOfflineMsg == null)
                                    {
                                        OfflineMsgs[tuple2].Add(offlineBrunMsg);
                                        if (offlineBrunMsg.status == 0 && offlineBrunMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId)
                                            repeatCount++;
                                    }
                                }
                                //var unReadOfflineMsgList = offlineBrunMsgReceives.Where(m => m.status == 0 && m.MsgType != AntSdkMsgType.Revocation && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                                //记录离线消息数量
                                if (repeatCount == 0) return;
                                RecordOffLineMsgCount(offlineBrunMsgReceives, repeatCount, GlobalVariable.BurnFlag.IsBurn);
                                isFlag = true;
                            }
                            else
                            {
                                isFlag = false;
                            }

                        }
                    }

                    var chatMsgLst = chatMsgList.Where(m => m.flag != (int)GlobalVariable.BurnFlag.IsBurn);
                    var offlineMsgReceives = chatMsgLst as List<AntSdkChatMsg.ChatBase> ?? chatMsgLst.ToList();
                    var count = offlineMsgReceives.Count;
                    if (count > 0)
                    {
                        var maxLocalChatIndex = t_groupChat.getQueryZeroChatIndex(offlineMsgReceives[0].sessionId,
                          AntSdkService.AntSdkLoginOutput.companyCode, AntSdkService.AntSdkLoginOutput.userId);
                        var tempOfflineMaxChatIndex = offlineMsgReceives[offlineMsgReceives.Count - 1].chatIndex;
                        int.TryParse(maxLocalChatIndex, out maxChatIndex);
                        int.TryParse(tempOfflineMaxChatIndex, out offlineMaxChatIndex);
                        tuple2 = Tuple.Create(offlineMsgReceives[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                        if (!OfflineMsgs.ContainsKey(tuple2) && offlineMaxChatIndex > maxChatIndex)
                        {
                            //SessionMonitor.RemoveSessionInfoList(offlineMsgReceives[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                            OfflineMsgs.Add(tuple2, offlineMsgReceives);
                            var unReadOfflineMsgList = offlineMsgReceives.Where(m => m.status == 0 && m.MsgType != AntSdkMsgType.Revocation
                            && m.MsgType != AntSdkMsgType.DeleteVote
                            && m.MsgType != AntSdkMsgType.DeleteActivity
                            && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                            RecordOffLineMsgCount(offlineMsgReceives, unReadOfflineMsgList.Count, GlobalVariable.BurnFlag.NotIsBurn);
                            isFlag = true;
                        }
                        else
                        {
                            if (OfflineMsgs.ContainsKey(tuple2))
                            {
                                var repeatCount = 0;
                                foreach (var offlineMsg in offlineMsgReceives)
                                {
                                    var tempOfflineMsg =
                                        OfflineMsgs[tuple2].FirstOrDefault(m => m.messageId == offlineMsg.messageId);
                                    if (tempOfflineMsg == null)
                                    {
                                        OfflineMsgs[tuple2].Add(offlineMsg);
                                        if (offlineMsg.status == 0
                                            && offlineMsg.MsgType != AntSdkMsgType.Revocation
                                            && offlineMsg.MsgType != AntSdkMsgType.DeleteVote
                                            && offlineMsg.MsgType != AntSdkMsgType.DeleteActivity
                                            && offlineMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId)
                                            repeatCount++;
                                    }
                                }
                                //var unReadOfflineMsgList = offlineMsgReceives.Where(m => m.status == 0 && m.MsgType != AntSdkMsgType.Revocation
                                //&& m.MsgType != AntSdkMsgType.DeleteVote
                                //&& m.MsgType != AntSdkMsgType.DeleteActivity
                                //&& m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                                if (repeatCount == 0) return;
                                RecordOffLineMsgCount(offlineMsgReceives, repeatCount, GlobalVariable.BurnFlag.NotIsBurn);
                                isFlag = true;
                            }
                            else
                            {
                                isFlag = false;
                            }

                        }
                    }
                    //防止重复写入本地
                    if (!isFlag)
                    {
                        var repetitionChatMsg = chatMsgList[chatMsgList.Count - 1];
                        LogHelper.WriteDebug("[非正常群组离线消息入库成功]:-----------------chatIndex" + chatMsgList[0].chatIndex + " 至 " + repetitionChatMsg.chatIndex);
                        WriteLocalHistory(chatMsgList, false);
                        PublicMessageFunction.SendChatMsgReceipt(repetitionChatMsg.sessionId, repetitionChatMsg.chatIndex, repetitionChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                        LogHelper.WriteDebug("[非正常群组离线消息发送已收回执]--------------sessionId：" + repetitionChatMsg.sessionId + "---------chatIndex：" + repetitionChatMsg.chatIndex);
                        return;
                    }
                    //群组离线消息写入本地成功之后发已接收回执
                    if (!WriteLocalHistory(chatMsgList)) return;
                    var chatMsg = chatMsgList[chatMsgList.Count - 1];
                    LogHelper.WriteDebug("[离线群组消息入库成功]:-----------------chatIndex" + chatMsgList[0].chatIndex + " 至 " + chatMsg.chatIndex);
                    PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                    LogHelper.WriteDebug("[离线群组消息发送已收回执]--------------sessionId：" + chatMsg.sessionId + "---------chatIndex：" + chatMsg.chatIndex);
                }
                else
                {
                    tuple2 = Tuple.Create(chatMsgList[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                    var maxLocalChatIndex = t_chat.getQueryZeroChatIndex(chatMsgList[0].sessionId,
                        AntSdkService.AntSdkLoginOutput.companyCode, AntSdkService.AntSdkLoginOutput.userId);
                    var tempOfflineMaxChatIndex = chatMsgList[chatMsgList.Count - 1].chatIndex;
                    int.TryParse(maxLocalChatIndex, out maxChatIndex);
                    int.TryParse(tempOfflineMaxChatIndex, out offlineMaxChatIndex);
                    //防止重复添加数据到缓存中
                    if (OfflineMsgs.ContainsKey(tuple2) && offlineMaxChatIndex <= maxChatIndex)
                    {
                        var repetitionChatMsg = chatMsgList[chatMsgList.Count - 1];
                        LogHelper.WriteDebug("[非正常个人离线消息入库成功]:-----------------chatIndex" + chatMsgList[0].chatIndex + " 至 " + repetitionChatMsg.chatIndex);
                        WriteLocalHistory(chatMsgList, false);
                        PublicMessageFunction.SendChatMsgReceipt(repetitionChatMsg.sessionId, repetitionChatMsg.chatIndex, repetitionChatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                        LogHelper.WriteDebug("[非正常个人离线消息发送已收回执]--------------sessionId：" + repetitionChatMsg.sessionId + "---------chatIndex：" + repetitionChatMsg.chatIndex);
                        return;
                    }
                    var repeatCount = 0;
                    if (OfflineMsgs.ContainsKey(tuple2))
                    {
                        foreach (var offlineMsg in chatMsgList)
                        {
                            var tempOfflineMsg =
                                OfflineMsgs[tuple2].FirstOrDefault(m => m.messageId == offlineMsg.messageId);
                            if (tempOfflineMsg == null)
                            {
                                if (offlineMsg.status == 0
                                           && offlineMsg.MsgType != AntSdkMsgType.Revocation
                                           && offlineMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId)
                                    repeatCount++;
                                OfflineMsgs[tuple2].Add(offlineMsg);
                            }
                        }
                        if (repeatCount == 0)
                            isFlag = false;
                    }
                    else
                    {
                        OfflineMsgs.Add(tuple2, chatMsgList);
                        var unReadOfflineMsgList = chatMsgList.Where(m => m.status == 0 && m.MsgType != AntSdkMsgType.Revocation && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                        repeatCount = unReadOfflineMsgList.Count;
                    }
                    //SessionMonitor.RemoveSessionInfoList(chatMsgList[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);


                    //个人离线消息写入本地成功之后发已接收回执
                    if (!WriteLocalHistory(chatMsgList)) return;
                    var chatMsg = chatMsgList[chatMsgList.Count - 1];
                    RecordOffLineMsgCount(chatMsgList, repeatCount, GlobalVariable.BurnFlag.NotIsBurn);
                    LogHelper.WriteDebug("[离线单人消息入库成功]:-----------------chatIndex" + chatMsgList[0].chatIndex + " 至 " + chatMsgList[chatMsgList.Count - 1].chatIndex);
                    PublicMessageFunction.SendChatMsgReceipt(chatMsg.sessionId, chatMsg.chatIndex, chatMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                    LogHelper.WriteDebug("[离线个人消息发送已收回执]-----------------sessionId：" + chatMsg.sessionId + "----------chatIndex：" + chatMsg.chatIndex);
                }
                var lastMsg = chatMsgList[chatMsgList.Count - 1];
                var contactUser = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == lastMsg.sendUserId);
                if (lastMsg.flag >= 0)
                {
                    SessionMonitor.AddSessionItemOfflineMsg(GetLastMsg(lastMsg.MsgType, lastMsg), contactUser, lastMsg, true);
                }
            }
        }

        static readonly object objSysUserMsgLock = new object();

        /// <summary>
        /// 判断系统用户通知的重复性
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <returns>true表示重复，false表示没有重复</returns>
        private static bool IsRepeatSysMsg(AntSdkMsBase sysMsg)
        {
            if (sysMsg.sessionId == null || sysMsg.chatIndex == null) return false;
            lock (objSysUserMsgLock)
            {
                if (AntSdkService.SysUserMsgList == null)
                {
                    AntSdkService.SysUserMsgList = new List<AntSdkMsBase>();
                    AntSdkService.SysUserMsgList.Add(sysMsg);
                    return false;
                }
                if (AntSdkService.SysUserMsgList.FirstOrDefault(c => c.chatIndex == sysMsg.chatIndex && c.sessionId == sysMsg.sessionId && c.MsgType == sysMsg.MsgType) == null)
                {
                    AntSdkService.SysUserMsgList.Add(sysMsg);
                    return false;
                }
                return true;
            }
        }
        /// <summary>
        /// 判断系统用户通知的重复性
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <returns>true表示重复，false表示没有重复</returns>
        private static bool IsRepeatSendMsg(AntSdkMsBase sysMsg)
        {
            if (sysMsg.sessionId == null || sysMsg.chatIndex == null) return false;
            lock (objSysUserMsgLock)
            {
                if (AntSdkService.SysUserMsgList == null)
                {
                    AntSdkService.SysUserMsgList = new List<AntSdkMsBase>();
                    AntSdkService.SysUserMsgList.Add(sysMsg);
                    return false;
                }
                if (AntSdkService.SysUserMsgList.FirstOrDefault(c => c.chatIndex == sysMsg.chatIndex && c.sessionId == sysMsg.sessionId && c.MsgType == sysMsg.MsgType) == null)
                {
                    AntSdkService.SysUserMsgList.Add(sysMsg);
                    return false;
                }
                return true;
            }
        }
        static readonly object objChatMsgLock = new object();

        /// <summary>
        /// 判断系统用户通知的重复性
        /// </summary>
        /// <param name="sysMsg"></param>
        /// <returns>true表示重复，false表示没有重复</returns>
        private static bool IsRepeatChatMsg(AntSdkChatMsg.ChatBase chatMsg)
        {
            lock (objChatMsgLock)
            {
                if (AntSdkService.ChaMsgList == null)
                {
                    AntSdkService.ChaMsgList = new List<AntSdkChatMsg.ChatBase>();
                    AntSdkService.ChaMsgList.Add(chatMsg);
                    return false;
                }
                if (AntSdkService.ChaMsgList.FirstOrDefault(c => c.sessionId == chatMsg.sessionId && c.MsgType == chatMsg.MsgType && c.messageId == chatMsg.messageId) == null)
                {
                    AntSdkService.ChaMsgList.Add(chatMsg);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 记录离线消息个数
        /// </summary>
        /// <param name="chatMsgLst"></param>
        /// <param name="count"></param>
        /// <param name="isBurnFlag">是否阅后即焚</param>
        /// <param name="sessionID">会话ID，可为不传值</param>
        /// <param name="chatMsgIndex"></param>
        public static void RecordOffLineMsgCount(List<AntSdkChatMsg.ChatBase> chatMsgLst, int count, GlobalVariable.BurnFlag isBurnFlag, string sessionID = "", string chatMsgIndex = "")
        {
            Tuple<string, GlobalVariable.BurnFlag> tuple2 = null;
            string tempSessionId;
            string tempChatIndex;
            if (chatMsgLst != null && string.IsNullOrEmpty(sessionID))
            {
                var tempLastMsg = chatMsgLst[chatMsgLst.Count - 1];
                tempSessionId = tempLastMsg.sessionId;
                tuple2 = Tuple.Create(tempSessionId, isBurnFlag);
                tempChatIndex = tempLastMsg.chatIndex;
            }
            else
            {
                tempSessionId = sessionID;
                tuple2 = Tuple.Create(tempSessionId, isBurnFlag);
                tempChatIndex = chatMsgIndex;
            }
            if (!SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
            {
                SessionMonitor.WaitingToReceiveOfflineMsgNum.Add(tuple2.Item1, tuple2.Item2, new MessageInfo() { IsBurnFlag = tuple2.Item2, Count = count, ChatIndex = tempChatIndex });
            }
            else
            {
                var info = SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2];
                int chatIndex;
                int lastChatIndex;
                int.TryParse(info.ChatIndex, out chatIndex);
                int.TryParse(tempChatIndex, out lastChatIndex);
                if (chatIndex >= lastChatIndex)
                {
                    if (info.Count < count)
                    {
                        SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count = count;
                    }
                    return;
                }
                var msgInfo = new MessageInfo
                {
                    ChatIndex = chatIndex >= lastChatIndex ? tempChatIndex : info.ChatIndex,
                    IsBurnFlag = info.IsBurnFlag,
                    Count = count + info.Count
                };
                SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2] = msgInfo;
            }
        }

        /// <summary>
        /// 获取离线消息
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        public static List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> GetOfflineMessageStatisticList(string sessionID, GlobalVariable.BurnFlag isBurnFlag, string chatIndex = "")
        {
            if (SessionMonitor.WaitingToReceiveOfflineMsgNum == null || SessionMonitor.WaitingToReceiveOfflineMsgNum.Count < 0) return new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            var tuple2 = Tuple.Create(sessionID, isBurnFlag);
            MessageInfo msgInfo = null;
            if (SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
            {
                if (string.IsNullOrEmpty(chatIndex))
                    msgInfo = SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2];
                else
                {
                    //阅后即焚切换，删除消息删除响应的缓存
                    int index = 0;
                    if (!string.IsNullOrEmpty(chatIndex))
                        int.TryParse(chatIndex, out index);
                    msgInfo = SessionMonitor.WaitingToReceiveOfflineMsgNum.FirstOrDefault(m => Equals(m.Key, tuple2) && int.Parse(m.Value.ChatIndex) <= index).Value;
                }
            }
            //if (msgInfo == null) return new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            var tempOfflineMsgList = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            if (OfflineMsgs != null && OfflineMsgs.Count > 0)
            {

                if (OfflineMsgs.ContainsKey(tuple2))
                {
                    var offlineMsgList = OfflineMsgs[tuple2];
                    var offlineCount = offlineMsgList.Count(m => m.MsgType != AntSdkMsgType.Revocation);
                    if (msgInfo != null && msgInfo.Count > 0)
                    {
                        if (offlineCount != msgInfo.Count)
                        {
                            var count = msgInfo.Count - offlineCount;
                            if (count > 0)
                            {
                                if (LocalUnreadMsgList.ContainsKey(tuple2))
                                    tempOfflineMsgList.AddRange(LocalUnreadMsgList[tuple2]);
                            }
                        }
                    }
                    if (offlineMsgList.Count > 0)
                    {
                        foreach (var offlineMsg in offlineMsgList)
                        {
                            var isExists = tempOfflineMsgList.Exists(m => m.messageId == offlineMsg.messageId);
                            if (!isExists)
                                tempOfflineMsgList.Add(offlineMsg);
                        }
                    }
                    tempOfflineMsgList = tempOfflineMsgList.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                }
                else
                {
                    //是否有本地未读消息
                    if (LocalUnreadMsgList.ContainsKey(tuple2))
                        tempOfflineMsgList = LocalUnreadMsgList[tuple2];
                }
            }
            else
            {
                //是否有本地未读消息
                if (LocalUnreadMsgList.ContainsKey(tuple2))
                    tempOfflineMsgList = LocalUnreadMsgList[tuple2];
            }
            if (tempOfflineMsgList.Count > 0)
            {
                LocalUnreadMsgList.Remove(tuple2);
                OfflineMsgs?.Remove(tuple2);
                SessionMonitor.WaitingToReceiveOfflineMsgNum.Remove(tuple2);
                return tempOfflineMsgList;
            }
            return new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
        }

        /// <summary>
        /// 获取本地只存未读消息
        /// </summary>
        private static void SearchLocalUnreadMsg()
        {
            var tSessionList = t_sessionBll.GetList();
            if (tSessionList == null) return;
            tSessionList =
                           tSessionList.OrderByDescending(p => p.TopIndex)
                               .ThenByDescending(p => PublicMessageFunction.LastMsgTime(p.LastMsgTimeStamp, p.BurnLastMsgTimeStamp))
                               .ToList();
            var receiveCount = 0;
            List<AntSdkChatMsg.ChatBase> tempOfflineMsg = null;
            foreach (var session in tSessionList)
            {
                if (session.BurnUnreadCount == 0 && session.UnreadCount == 0)
                    continue;
                var burnFlag = session.IsBurnMode == 1 ? GlobalVariable.BurnFlag.IsBurn : GlobalVariable.BurnFlag.NotIsBurn;
                var tuple2 = Tuple.Create(session.SessionId, burnFlag);
                var ctt = new SendMessage_ctt
                {
                    sendUserId = AntSdkService.AntSdkLoginOutput.userId,
                    targetId = session.GroupId,
                    companyCode = AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                    sessionId = session.SessionId
                };

                var unreadCount = 0;
                if (session.SessionId.StartsWith("G"))
                {
                    var groupInfoVm = SessionMonitor.GroupListViewModel?.GroupInfos?.FirstOrDefault(
                         c => c.groupId == session.SessionId);
                    if (groupInfoVm == null)
                    {
                        continue;
                    }
                    if (session.BurnUnreadCount > 0)
                    {
                        var localMsgList = SearchLocalData(ctt, GlobalVariable.BurnFlag.IsBurn, session.BurnUnreadCount);
                        if (localMsgList.Count == 0) continue;
                        tuple2 = Tuple.Create(session.SessionId, GlobalVariable.BurnFlag.IsBurn);
                        if (!LocalUnreadMsgList.ContainsKey(tuple2))
                            LocalUnreadMsgList.Add(tuple2, localMsgList);
                        if (OfflineMsgs.ContainsKey(tuple2))
                        {
                            tempOfflineMsg = OfflineMsgs[tuple2];
                            receiveCount = tempOfflineMsg.Count;
                        }
                        var tempLocalUnreadMsgList = LocalUnreadMsgList[tuple2];
                        unreadCount = receiveCount + tempLocalUnreadMsgList.Count;
                        LogHelper.WriteDebug("---------------------------本地群组:" + session.SessionId + " 无痕模式未读消息数：" + LocalUnreadMsgList.Count + "----------离线消息未读数：" + receiveCount);
                        RecordOffLineMsgCount(null, tempLocalUnreadMsgList.Count, GlobalVariable.BurnFlag.IsBurn, session.SessionId, session.BurnLastChatIndex);
                        var isExitGroupModeChanged = GroupBurnModeList.Count > 0 && GroupBurnModeList.Exists(m => m.sessionId == session.SessionId &&
                        (m.MsgType == AntSdkMsgType.GroupOwnerNormal || m.MsgType == AntSdkMsgType.GroupOwnerBurnDelete));
                        if (session.IsBurnMode == 1 || isExitGroupModeChanged)
                        {

                            var lastMsg = tempLocalUnreadMsgList[tempLocalUnreadMsgList.Count - 1];
                            if (receiveCount > 0)
                            {
                                if (tempOfflineMsg != null)
                                    lastMsg = tempOfflineMsg[tempOfflineMsg.Count - 1];
                                session.BurnLastMsg = GetLastMsg(lastMsg.MsgType, lastMsg);
                            }
                            if (isExitGroupModeChanged)
                            {
                                session.BurnLastMsg = session.LastMsg;
                                LocalUnreadMsgList[tuple2].Clear();
                                SessionMonitor.WaitingToReceiveOfflineMsgNum.Remove(tuple2);
                                lastMsg.chatIndex = session.LastChatIndex;
                                lastMsg.sendTime = session.LastMsgTimeStamp;
                                lastMsg.flag = 0;
                            }
                            var contactUser = AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(c => c.userId == lastMsg.sendUserId);
                            if (string.IsNullOrEmpty(lastMsg.flag.ToString()))
                                lastMsg.flag = 1;

                            //if (!string.IsNullOrEmpty(lastMsg.flag.ToString()) && contactUser != null)
                            //{

                            //    SessionMonitor.AddSessionItemOfflineMsg(session.BurnLastMsg, contactUser, lastMsg);
                            //}
                        }
                    }
                    if (session.UnreadCount <= 0) continue;
                    {
                        var localMsgList = SearchLocalData(ctt, GlobalVariable.BurnFlag.NotIsBurn, session.UnreadCount);
                        //本地消息获取成功之后，再添加到缓存和记录数量并显示消息列表中
                        if (localMsgList.Count == 0) continue;
                        tuple2 = Tuple.Create(session.SessionId, GlobalVariable.BurnFlag.NotIsBurn);
                        if (!LocalUnreadMsgList.ContainsKey(tuple2))
                            LocalUnreadMsgList.Add(tuple2, localMsgList);
                        var tempLocalUnreadMsgList = LocalUnreadMsgList[tuple2];
                        if (OfflineMsgs.ContainsKey(tuple2))
                        {
                            tempOfflineMsg = OfflineMsgs[tuple2];
                            receiveCount = tempOfflineMsg.Count(m => m.MsgType != AntSdkMsgType.Revocation);
                            var tempOfflineRevocationMsg = tempOfflineMsg.Where(m => m.MsgType == AntSdkMsgType.Revocation);
                            foreach (var offlineMsg in tempOfflineRevocationMsg)
                            {
                                var tempLocalUnreadMsg =
                                    LocalUnreadMsgList[tuple2].FirstOrDefault(m => m.messageId == offlineMsg.messageId);
                                if (tempLocalUnreadMsg != null)
                                    LocalUnreadMsgList[tuple2].Remove(tempLocalUnreadMsg);
                            }
                        }
                        unreadCount = receiveCount + tempLocalUnreadMsgList.Count;
                        LogHelper.WriteDebug("---------------------------本地群组:" + session.SessionId + " 正常模式未读消息数：" + LocalUnreadMsgList[tuple2].Count + "----------离线消息未读数：" + receiveCount);
                        RecordOffLineMsgCount(null, tempLocalUnreadMsgList.Count, GlobalVariable.BurnFlag.NotIsBurn, session.SessionId, session.LastChatIndex);
                        var isExitGroupModeChanged = GroupBurnModeList.Count > 0 && GroupBurnModeList.Exists(
                            m => m.sessionId == session.SessionId && m.MsgType == AntSdkMsgType.GroupOwnerNormal);
                        if (session.IsBurnMode == 0 || isExitGroupModeChanged)
                        {
                            var lastMsg = tempLocalUnreadMsgList[tempLocalUnreadMsgList.Count - 1];
                            if (receiveCount > 0)
                            {
                                if (tempOfflineMsg != null)
                                    lastMsg = tempOfflineMsg[tempOfflineMsg.Count - 1];
                                session.LastMsg = GetLastMsg(lastMsg.MsgType, lastMsg);
                            }
                            var contactUser =
                                AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                                    c => c.userId == lastMsg.sendUserId);
                            if (string.IsNullOrEmpty(lastMsg.flag.ToString()))
                                lastMsg.flag = 0;

                            if (!string.IsNullOrEmpty(lastMsg.flag.ToString()) && contactUser != null)
                            {
                                //SessionMonitor.AddSessionItemOfflineMsg(session.LastMsg, contactUser, lastMsg);
                            }
                        }
                    }
                }
                else
                {
                    if (session.UnreadCount <= 0) continue;
                    var localMsgList = SearchLocalData(ctt, GlobalVariable.BurnFlag.NotIsBurn, session.UnreadCount);
                    if (localMsgList.Count == session.UnreadCount && _offlinePointBurnReadeds.Count > 0)
                    {
                        localMsgList = localMsgList.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                        foreach (var pointBurnMsg in _offlinePointBurnReadeds.Select(baseMsg => baseMsg as AntSdkReceivedOtherMsg.PointBurnReaded))
                        {
                            if (pointBurnMsg == null) return;
                            var localMsg =
                                localMsgList.FirstOrDefault(m => m.messageId == pointBurnMsg.content?.messageId);
                            localMsgList.Remove(localMsg ?? localMsgList[0]);
                        }
                        var lastLocalMsg = localMsgList[localMsgList.Count - 1];
                        session.LastMsg = GetLastMsg(lastLocalMsg.MsgType, lastLocalMsg);
                    }
                    //本地消息获取成功之后，再添加到缓存和记录数量并显示消息列表中
                    if (localMsgList.Count == 0) continue;
                    if (!LocalUnreadMsgList.ContainsKey(tuple2))
                        LocalUnreadMsgList.Add(tuple2, localMsgList);
                    var tempLocalUnreadMsgList = LocalUnreadMsgList[tuple2];
                    if (OfflineMsgs.ContainsKey(tuple2))
                    {
                        tempOfflineMsg = OfflineMsgs[tuple2];
                        receiveCount = tempOfflineMsg.Count;
                        var tempOfflineRevocationMsg = tempOfflineMsg.Where(m => m.MsgType == AntSdkMsgType.Revocation);
                        foreach (var offlineMsg in tempOfflineRevocationMsg)
                        {
                            var tempLocalUnreadMsg =
                                LocalUnreadMsgList[tuple2].FirstOrDefault(m => m.messageId == offlineMsg.messageId);
                            if (tempLocalUnreadMsg != null)
                                LocalUnreadMsgList[tuple2].Remove(tempLocalUnreadMsg);
                        }
                    }
                    unreadCount = receiveCount + tempLocalUnreadMsgList.Count;
                    LogHelper.WriteDebug("---------------------------本地个人:" + session.SessionId + " 未读消息数：" + LocalUnreadMsgList[tuple2].Count + "----------离线消息未读数：" + receiveCount);
                    RecordOffLineMsgCount(null, tempLocalUnreadMsgList.Count, GlobalVariable.BurnFlag.NotIsBurn, session.SessionId, session.LastChatIndex);
                    var lastMsg = tempLocalUnreadMsgList[tempLocalUnreadMsgList.Count - 1];
                    if (receiveCount > 0)
                    {
                        if (tempOfflineMsg != null)
                            lastMsg = tempOfflineMsg[tempOfflineMsg.Count - 1];
                        session.LastMsg = GetLastMsg(lastMsg.MsgType, lastMsg);
                    }
                    var contactUser = AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(c => c.userId == lastMsg.sendUserId);
                    if (string.IsNullOrEmpty(lastMsg.flag.ToString()))
                        lastMsg.flag = 0;

                    if (!string.IsNullOrEmpty(lastMsg.flag.ToString()) && contactUser != null)
                    {
                        //SessionMonitor.AddSessionItemOfflineMsg(session.LastMsg, contactUser, lastMsg);
                    }
                }

            }
        }

        /// <summary>
        /// 查询本地库数据
        /// </summary>
        /// <param name="ctt"></param>
        /// <param name="isBurnFlag"></param>
        /// <param name="count"></param>
        private static List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> SearchLocalData(SendMessage_ctt ctt, GlobalVariable.BurnFlag isBurnFlag, int count)
        {
            var chatMsgList = new List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>();
            if (!string.IsNullOrEmpty(ctt.targetId))
            {
                if (isBurnFlag == GlobalVariable.BurnFlag.NotIsBurn)
                {
                    var groupChat = t_groupChat.GetDataTable(ctt.sessionId, AntSdkService.AntSdkCurrentUserInfo.userId, ctt.targetId, ctt.companyCode, 0, count);
                    if (groupChat != null)
                    {
                        var listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(groupChat);
                        if (listChatdata != null && listChatdata.Count > 0)
                            chatMsgList = listChatdata.Select(m =>
                            {
                                m.chatType = (int)AntSdkchatType.Group;
                                m.flag = 0; return m;
                            }).ToList();
                    }
                }
                else if (isBurnFlag == GlobalVariable.BurnFlag.IsBurn)
                {
                    var groupBurnChat = t_groupBurnChat.GetDataTable(ctt.sessionId, ctt.sendUserId, ctt.targetId, ctt.companyCode, 0, count);
                    if (groupBurnChat != null)
                    {
                        var listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_groupBurnChat.GetDataTable(ctt.sessionId, ctt.sendUserId, ctt.targetId, ctt.companyCode, 0, count));
                        if (listChatdata != null && listChatdata.Count > 0)
                            chatMsgList = listChatdata.Select(m =>
                            {
                                m.chatType = (int)AntSdkchatType.Group;
                                m.flag = 1; return m;
                            }).ToList();
                    }
                }
            }
            else
            {
                var chat = t_chat.GetDataTable(ctt.sessionId, ctt.sendUserId, ctt.targetId, ctt.companyCode, 0, count);
                if (chat != null)
                {
                    var listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(chat);
                    if (listChatdata != null && listChatdata.Count > 0)
                        chatMsgList = listChatdata.Select(m =>
                        { m.chatType = (int)AntSdkchatType.Point; return m; }).ToList();
                }
            }
            return chatMsgList;
        }

        /// <summary>
        /// 方法说明：离线消息写入本地
        /// </summary>
        /// <param name="offlinemsgList">离线消息集合</param>
        /// <returns>是否成功写入本地</returns>
        private static bool WriteLocalHistory(IReadOnlyList<AntSdkChatMsg.ChatBase> offlinemsgList, bool isSaveSession = true)
        {
            try
            {
                var tSessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
                var isMsgRecord = false;
                //群组聊天离线
                var offlinegroupmsgList = offlinemsgList.Where(c => c.chatType == (int)AntSdkchatType.Group).ToList().OrderBy(c => c.chatIndex).ToList();
                //原来会话信息
                var oldSession = new AntSdkTsession();
                //新的会话信息
                var newTSeesion = new AntSdkTsession();
                if (offlinegroupmsgList.Count > 0)
                {
                    //原会话信息
                    oldSession = tSessionBll.GetModelByKey(offlinegroupmsgList[0].sessionId);
                    var offlineMsgListBurnMode = offlinegroupmsgList.Where(c => c.flag == (int)AntSdkBurnFlag.IsBurn).ToList();
                    var offlineMsgListNormalMode = offlinegroupmsgList.Where(c => c.flag != (int)AntSdkBurnFlag.IsBurn).ToList();
                    var sbSql = new StringBuilder();

                    if (offlineMsgListNormalMode.Count > 0) //群组正常离线消息记录本地库
                    {
                        var tempofflineMsgListNormalMode = offlineMsgListNormalMode.Where(m => m.MsgType != AntSdkMsgType.DeleteActivity && m.MsgType != AntSdkMsgType.DeleteVote);
                        sbSql.Append("INSERT OR REPLACE INTO T_Chat_Message_Group(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL,SPARE1) values");
                        foreach (var list in tempofflineMsgListNormalMode)
                        {
                            if (list.MsgType == AntSdkMsgType.Revocation)
                            {
                                string sourceContent;
                                if (list.sendUserId == AntSdkService.AntSdkLoginOutput.userId)
                                {
                                    sourceContent = "你" + GlobalVariable.RevocationPrompt.Msessage;
                                }
                                else
                                {
                                    var userInfo =
                                        AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                                            m => m.userId == list.sendUserId);
                                    string userName = string.Empty;
                                    if (userInfo != null)
                                        userName = userInfo.userNum + userInfo.userName;
                                    sourceContent = userName + GlobalVariable.RevocationPrompt.Msessage;
                                }
                                list.sourceContent = sourceContent;
                                var tempChatMsg = (AntSdkChatMsg.Revocation)list;
                                if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                                {
                                    var isResult = t_groupChat.DeleteGroupMsgByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                          AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                                    if (isResult > 0 && oldSession.UnreadCount > 0 && isSaveSession)
                                    {
                                        oldSession.UnreadCount -= 1;
                                        if (LocalUnreadMsgList != null && LocalUnreadMsgList.Count > 0)
                                        {

                                            Tuple<string, GlobalVariable.BurnFlag> tuple2 = Tuple.Create(list.sessionId,
                                                GlobalVariable.BurnFlag.NotIsBurn);
                                            if (LocalUnreadMsgList.ContainsKey(tuple2))
                                            {
                                                var unReadMessage =
                                                    LocalUnreadMsgList[tuple2].FirstOrDefault(
                                                        m => m.messageId == tempChatMsg.content?.messageId);
                                                if (unReadMessage != null)
                                                    LocalUnreadMsgList[tuple2].Remove(unReadMessage);
                                                if (LocalUnreadMsgList[tuple2].Count == 0)
                                                    LocalUnreadMsgList.Remove(tuple2);
                                                if (unReadMessage != null && SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                                                    SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                                                    SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                                            }
                                        }
                                    }

                                }
                            }

                            list.SENDORRECEIVE = "0";

                            string content = string.IsNullOrEmpty(list.sourceContent) ? "" : list.sourceContent.Replace("'", "''");
                            if (list.MsgType == AntSdkMsgType.CreateVote)
                            {
                                var tempChatMsg = (AntSdkChatMsg.CreateVoteMsg)list;
                                if (tempChatMsg.content != null)
                                    list.VoteOrActivityID = tempChatMsg.content.id.ToString();
                            }
                            else if (list.MsgType == AntSdkMsgType.CreateActivity)
                            {
                                var tempChatMsg = (AntSdkChatMsg.ActivityMsg)list;
                                if (tempChatMsg.content != null)
                                    list.VoteOrActivityID = tempChatMsg.content.activityId.ToString();
                            }
                            sbSql.Append("('" + (int)list.MsgType + "','" + list.chatIndex + "','" + content + "','" + list.messageId + "','" + list.sendTime + "','" + list.sendUserId + "','" + list.sessionId + "','" + list.targetId + "','" + list.SENDORRECEIVE + "','" + 1 + "','" + list.VoteOrActivityID + "'),");
                        }
                        var str = sbSql.ToString().Substring(0, sbSql.ToString().Length - 1);
                        isMsgRecord = AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                        LogHelper.WriteDebug("[HandleOfflineMqttMessageByTopic_T_Chat_Message_Group:]" + str);

                        var tempOfflineLastMsg = offlineMsgListNormalMode[offlineMsgListNormalMode.Count - 1];
                        var unReadOfflineList = offlineMsgListNormalMode.Where(m => m.status == 0
                        && m.MsgType != AntSdkMsgType.Revocation
                        && m.MsgType != AntSdkMsgType.DeleteActivity
                        && m.MsgType != AntSdkMsgType.DeleteVote
                        && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                        if (isSaveSession)
                        {
                            if (oldSession != null)
                            {
                                LogHelper.WriteDebug("---------------------------OfflineMqttMessage群组:" +
                                                     oldSession.SessionId + " 正常模式本地未读消息数：" + oldSession.UnreadCount +
                                                     "----------离线消息未读数：" + unReadOfflineList.Count);
                                oldSession.UnreadCount = oldSession.UnreadCount + unReadOfflineList.Count;
                                oldSession.LastChatIndex = tempOfflineLastMsg.chatIndex;
                                oldSession.LastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                                if (string.CompareOrdinal(oldSession.LastMsgTimeStamp, tempOfflineLastMsg.sendTime) < 0)
                                    oldSession.LastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                                //Tuple<string, GlobalVariable.BurnFlag> tuple2 = Tuple.Create(offlineMsgListNormalMode[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                                //if (SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                                //    SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2) && oldSession.UnreadCount > 0)
                                //    SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count = oldSession.UnreadCount;
                            }
                            else
                            {
                                newTSeesion.UnreadCount = unReadOfflineList.Count;
                                newTSeesion.LastChatIndex = tempOfflineLastMsg.chatIndex;
                                newTSeesion.LastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                                newTSeesion.LastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                            }
                        }
                        #region 活动或投票被删除的离线消息
                        var tempofflineMsgListDelete = offlineMsgListNormalMode.Where(m => m.MsgType == AntSdkMsgType.DeleteActivity || m.MsgType == AntSdkMsgType.DeleteVote);
                        foreach (var offlineMsg in tempofflineMsgListDelete)
                        {
                            var strID = "";
                            var baseChatMsg = offlineMsg;
                            if (offlineMsg.MsgType == AntSdkMsgType.DeleteVote)
                            {
                                var tempChatMsg = (AntSdkChatMsg.DeteleVoteMsg)offlineMsg;
                                if (tempChatMsg.content != null)
                                {
                                    strID = tempChatMsg.content.id.ToString();
                                }
                                baseChatMsg = t_groupChat.GetGroupMsgByVoteOrActivityId(strID,
                                offlineMsg.sessionId, ((int)AntSdkMsgType.CreateVote).ToString(),
                                AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                            }
                            else if (offlineMsg.MsgType == AntSdkMsgType.DeleteActivity)
                            {
                                var tempChatMsg = (AntSdkChatMsg.DeleteActivityMsg)offlineMsg;
                                if (tempChatMsg.content != null)
                                {
                                    strID = tempChatMsg.content.activityId.ToString();
                                }
                                baseChatMsg = t_groupChat.GetGroupMsgByVoteOrActivityId(strID,
                                offlineMsg.sessionId, ((int)AntSdkMsgType.CreateActivity).ToString(),
                                AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                            }


                            if (baseChatMsg != null)
                            {
                                var result = t_groupChat.DeleteByMessageId(
                                     AntSdkService.AntSdkLoginOutput.companyCode,
                                     AntSdkService.AntSdkCurrentUserInfo.userId,
                                     baseChatMsg.messageId);
                                offlineMsg.messageId = baseChatMsg.messageId;
                                if (result > 0)
                                {
                                    switch (offlineMsg.MsgType)
                                    {
                                        case AntSdkMsgType.DeleteVote:
                                            PublicMessageFunction.SendChatMsgReceipt(offlineMsg.sessionId, offlineMsg.chatIndex,
                                                offlineMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                                            if (!string.IsNullOrEmpty(strID))
                                                LogHelper.WriteDebug("[离线群投票被删除消息发送收回执]--------------sessionId：" +
                                                                     offlineMsg.sessionId + "---------chatIndex：" +
                                                                     offlineMsg.chatIndex + "--------voteId" +
                                                                     strID);
                                            break;
                                        case AntSdkMsgType.DeleteActivity:
                                            PublicMessageFunction.SendChatMsgReceipt(offlineMsg.sessionId, offlineMsg.chatIndex, offlineMsg.MsgType, AntSdkReceiptType.ReceiveReceipt);
                                            if (!string.IsNullOrEmpty(strID))
                                                LogHelper.WriteDebug("[离线群活动被删除消息发送收回执]--------------sessionId：" + offlineMsg.sessionId + "---------chatIndex：" + offlineMsg.chatIndex + "---------activityId" + strID);
                                            break;
                                    }
                                }
                            }
                            if (oldSession == null ||
                                (baseChatMsg == null || oldSession.UnreadCount <= 0 || !isSaveSession)) continue;
                            oldSession.UnreadCount -= 1;
                            if (LocalUnreadMsgList != null && LocalUnreadMsgList.Count > 0)
                            {
                                var tuple2 = Tuple.Create(offlineMsg.sessionId,
                                    GlobalVariable.BurnFlag.NotIsBurn);
                                if (LocalUnreadMsgList.ContainsKey(tuple2))
                                {
                                    var unReadMessage =
                                        LocalUnreadMsgList[tuple2].FirstOrDefault(
                                            m => m.messageId == offlineMsg.messageId);
                                    if (unReadMessage != null)
                                        LocalUnreadMsgList[tuple2].Remove(unReadMessage);
                                    if (LocalUnreadMsgList[tuple2].Count == 0)
                                        LocalUnreadMsgList.Remove(tuple2);
                                    if (SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                                        SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                                        SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                                }
                            }
                            if (OfflineMsgs != null && OfflineMsgs.Count > 0)
                            {
                                var tuple2 = Tuple.Create(offlineMsg.sessionId,
                                    GlobalVariable.BurnFlag.NotIsBurn);
                                if (OfflineMsgs.ContainsKey(tuple2))
                                {
                                    var unReadMessage =
                                        OfflineMsgs[tuple2].FirstOrDefault(
                                            m => m.messageId == offlineMsg.messageId);
                                    if (unReadMessage != null)
                                        OfflineMsgs[tuple2].Remove(unReadMessage);
                                    if (OfflineMsgs[tuple2].Count == 0)
                                        OfflineMsgs.Remove(tuple2);
                                    if (SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                                        SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                                        SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                                }
                            }
                        }
                        #endregion
                    }
                    var sbSqlBurn = new StringBuilder();
                    if (offlineMsgListBurnMode.Count > 0) //群组阅后即焚离线消息记录本地库
                    {
                        sbSqlBurn.Append("INSERT OR REPLACE INTO t_chat_message_groupburn(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL) values");
                        foreach (var list in offlineMsgListBurnMode)
                        {
                            list.SENDORRECEIVE = "0";
                            string content = string.IsNullOrEmpty(list.sourceContent) ? "" : list.sourceContent.Replace("'", "''");
                            sbSqlBurn.Append("('" + (int)list.MsgType + "','" + list.chatIndex + "','" + content + "','" + list.messageId + "','" + list.sendTime + "','" + list.sendUserId + "','" + list.sessionId + "','" + list.targetId + "','" + list.SENDORRECEIVE + "','" + 1 + "'),");
                        }
                        var str = sbSqlBurn.ToString().Substring(0, sbSqlBurn.ToString().Length - 1);
                        isMsgRecord = AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                        var tempOfflineLastMsg = offlineMsgListBurnMode[offlineMsgListBurnMode.Count - 1];
                        var unReadOfflineList = offlineMsgListBurnMode.Where(m => m.status == 0 && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                        if (isSaveSession)
                        {
                            if (oldSession != null)
                            {
                                LogHelper.WriteDebug("---------------------------OfflineMqttMessage群组:" +
                                                     oldSession.SessionId + " 无痕模式本地未读消息数：" + oldSession.BurnUnreadCount +
                                                     "----------离线消息未读数：" + unReadOfflineList.Count);
                                oldSession.BurnUnreadCount = oldSession.BurnUnreadCount + unReadOfflineList.Count;
                                oldSession.BurnLastChatIndex = tempOfflineLastMsg.chatIndex;
                                oldSession.BurnLastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                                if (
                                    string.CompareOrdinal(oldSession.BurnLastMsgTimeStamp, tempOfflineLastMsg.sendTime) <
                                    0)
                                    oldSession.BurnLastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                            }
                            else
                            {
                                newTSeesion.BurnUnreadCount = unReadOfflineList.Count;
                                newTSeesion.BurnLastChatIndex = tempOfflineLastMsg.chatIndex;
                                newTSeesion.BurnLastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                                newTSeesion.BurnLastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                            }
                        }
                        LogHelper.WriteDebug("[HandleOfflineMqttMessageByTopic_T_Chat_Message_Group:]" + str);
                    }
                }
                //个人聊天离线:个人离线消息记录本地库
                var offlinepointmsgList = offlinemsgList.Where(c => c.chatType == (int)AntSdkchatType.Point).ToList().OrderBy(c => c.chatIndex).ToList();
                if (offlinepointmsgList.Count > 0)
                {
                    //原会话信息
                    oldSession = tSessionBll.GetModelByKey(offlinepointmsgList[0].sessionId);
                    var sbSql = new StringBuilder();
                    sbSql.Append("INSERT OR REPLACE INTO T_Chat_Message(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL,flag) values");
                    foreach (var list in offlinepointmsgList)
                    {
                        if (list.MsgType == AntSdkMsgType.Revocation)
                        {
                            string sourceContent;
                            if (list.sendUserId == AntSdkService.AntSdkLoginOutput.userId)
                            {
                                sourceContent = "你" + GlobalVariable.RevocationPrompt.Msessage;
                            }
                            else
                            {
                                //var userInfo =
                                //    AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                                //        m => m.userId == list.sendUserId);
                                //string userName = string.Empty;
                                //if (userInfo != null)
                                //    userName =userInfo.userNum+ userInfo.userName;
                                sourceContent = "对方" + GlobalVariable.RevocationPrompt.Msessage;
                            }
                            list.sourceContent = sourceContent;
                            var tempChatMsg = (AntSdkChatMsg.Revocation)list;
                            if (!string.IsNullOrEmpty(tempChatMsg.content?.messageId))
                            {
                                var isResult = t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                    AntSdkService.AntSdkCurrentUserInfo.userId, tempChatMsg.content?.messageId);
                                if (isResult > 0 && oldSession.UnreadCount > 0 && isSaveSession)
                                {
                                    oldSession.UnreadCount -= 1;
                                    if (LocalUnreadMsgList != null && LocalUnreadMsgList.Count > 0)
                                    {
                                        Tuple<string, GlobalVariable.BurnFlag> tuple2 = Tuple.Create(list.sessionId,
                                            GlobalVariable.BurnFlag.NotIsBurn);
                                        if (LocalUnreadMsgList.ContainsKey(tuple2))
                                        {
                                            var unReadMessage =
                                                LocalUnreadMsgList[tuple2].FirstOrDefault(
                                                    m => m.messageId == tempChatMsg.content?.messageId);
                                            if (unReadMessage != null)
                                                LocalUnreadMsgList[tuple2].Remove(unReadMessage);
                                            if (LocalUnreadMsgList[tuple2].Count == 0)
                                                LocalUnreadMsgList.Remove(tuple2);
                                            if (unReadMessage != null && SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                                                SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                                                SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count -= 1;
                                        }
                                    }
                                }
                            }
                        }
                        list.SENDORRECEIVE = "0";
                        string content = string.IsNullOrEmpty(list.sourceContent) ? "" : list.sourceContent.Replace("'", "''");
                        sbSql.Append("('" + (int)list.MsgType + "','" + list.chatIndex + "','" + content + "','" + list.messageId + "','" + list.sendTime + "','" + list.sendUserId + "','" + list.sessionId + "','" + list.targetId + "','" + list.SENDORRECEIVE + "','" + 1 + "','" + list.flag + "'),");
                    }
                    isMsgRecord = AntSdkSqliteHelper.InsertBigData(sbSql.ToString().Substring(0, sbSql.ToString().Length - 1), AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                    var tempOfflineLastMsg = offlinemsgList[offlinemsgList.Count - 1];
                    var unReadOfflineList = offlinemsgList.Where(m => m.status == 0 && m.MsgType != AntSdkMsgType.Revocation && m.sendUserId != AntSdkService.AntSdkLoginOutput.userId).ToList();
                    if (isSaveSession)
                    {
                        if (oldSession != null)
                        {
                            LogHelper.WriteDebug("---------------------------OfflineMqttMessage个人:" +
                                                 oldSession.SessionId + " 本地未读消息数：" + oldSession.UnreadCount +
                                                 "----------离线消息未读数：" + unReadOfflineList.Count);
                            string sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.robotId,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                            if (sessionId == oldSession.SessionId && oldSession.UnreadCount > 0)
                                oldSession.UnreadCount = 0;
                            else
                            {
                                oldSession.UnreadCount = oldSession.UnreadCount + unReadOfflineList.Count;
                            }
                            oldSession.LastChatIndex = tempOfflineLastMsg.chatIndex;
                            oldSession.LastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                            if (string.CompareOrdinal(oldSession.LastMsgTimeStamp, tempOfflineLastMsg.sendTime) < 0)
                                oldSession.LastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                            //Tuple<string, GlobalVariable.BurnFlag> tuple2 = Tuple.Create(offlinemsgList[0].sessionId, GlobalVariable.BurnFlag.NotIsBurn);
                            //if (SessionMonitor.WaitingToReceiveOfflineMsgNum != null &&
                            //    SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2) && oldSession.UnreadCount > 0)
                            //    SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count = oldSession.UnreadCount;
                        }
                        else
                        {
                            newTSeesion.UnreadCount = unReadOfflineList.Count;
                            newTSeesion.LastChatIndex = tempOfflineLastMsg.chatIndex;
                            newTSeesion.LastMsg = GetLastMsg(tempOfflineLastMsg.MsgType, tempOfflineLastMsg);
                            newTSeesion.LastMsgTimeStamp = tempOfflineLastMsg.sendTime;
                        }
                    }
                }
                //如果数据已存库，更新Seesion表
                if (isMsgRecord && isSaveSession)
                {
                    if (oldSession != null)
                    {
                        tSessionBll.Update(oldSession);
                    }
                    else
                    {
                        var tempOfflineLastMsg = offlinemsgList[offlinemsgList.Count - 1];
                        newTSeesion.SessionId = tempOfflineLastMsg.sessionId;
                        if (tempOfflineLastMsg.sessionId == tempOfflineLastMsg.targetId) //群聊
                        {
                            newTSeesion.GroupId = tempOfflineLastMsg.sessionId;
                        }
                        else if (tempOfflineLastMsg.sendUserId == AntSdkService.AntSdkCurrentUserInfo.userId) //如果发送者是本人
                        {
                            newTSeesion.UserId = tempOfflineLastMsg.targetId;
                        }
                        else
                        {
                            newTSeesion.UserId = tempOfflineLastMsg.sendUserId;
                        }
                        tSessionBll.Insert(newTSeesion);
                    }
                }
                return isMsgRecord;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("离线消息入库:" + ex.Message);
                return false;

            }

        }

        private static void DeleteActivityAndVote()
        {

        }

        /// <summary>
        /// 获取最后一条消息内容
        /// </summary>
        /// <param name="lastMsg"></param>
        /// <returns></returns>
        private static string GetLastMsg(AntSdkMsgType msgType, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase lastMsg)
        {
            string lastMessage = string.Empty;
            var antSdkContactUser = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == lastMsg.sendUserId);
            AntSdkContact_User AntSdkContact_User = null;
            if (lastMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId && lastMsg.os != (int)GlobalVariable.OSType.PC)
            {
                AntSdkContact_User = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                        c => c.userId == AntSdkService.AntSdkLoginOutput.userId);
                if (AntSdkContact_User == null) return string.Empty;
                lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, lastMsg.chatType == (int)AntSdkchatType.Group);
            }
            else if (lastMsg.chatType == (int)AntSdkchatType.Point && lastMsg.sendUserId != AntSdkService.AntSdkLoginOutput.userId) //点对点聊天，且发言者不是本人
            {
                lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, false);
            }
            else if (lastMsg.sessionId == lastMsg.targetId && lastMsg.chatType == (int)AntSdkchatType.Group) //群聊，且发言者不是本人 
            {
                if (lastMsg.sendUserId == AntSdkService.AntSdkLoginOutput.userId && lastMsg.os == ((int)GlobalVariable.OSType.PC)) return string.Empty; //过滤掉本人发的群消息（本人发的消息不可能是离线消息）
                if (antSdkContactUser == null)
                {
                    lastMessage = lastMsg.flag != ((int)GlobalVariable.BurnFlag.IsBurn) ? PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, true, "离职人员") : PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, true);
                }
                else
                {
                    //lastMessage = FormatLastMessageContent(lastMsg.mtp, lastMsg.ctt, true);
                    if (lastMsg.MsgType == AntSdkMsgType.Revocation)
                    {
                        lastMessage = PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, true);
                    }
                    else
                    {
                        lastMessage = lastMsg.flag != ((int)GlobalVariable.BurnFlag.IsBurn) ? PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, true, antSdkContactUser.userNum != null ? antSdkContactUser.userNum + antSdkContactUser.userName : antSdkContactUser.userName) : PublicMessageFunction.FormatLastMessageContent(msgType, lastMsg, true);
                    }

                }
            }
            return lastMessage;
        }


        /// <summary>
        /// 用户A收到消息服务转发的B已读阅后即焚消息的通知（离线消息）
        /// </summary>
        /// <param name="ctt"></param>
        private static void HandleBurnAfterReadReceipt(AntSdkReceivedOtherMsg.PointBurnReaded ctt, bool isLast)
        {
            try
            {
                BurnAfterReadReceiptCtt msg = new BurnAfterReadReceiptCtt();
                msg.sendUserId = ctt.sendUserId;
                msg.targetId = ctt.targetId;
                msg.companyCode = /*ctt.companyCode*/ AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode;
                msg.chatIndex = ctt.chatIndex;
                msg.os = ctt.os.ToString();
                msg.sessionId = ctt.sessionId;
                msg.messageId = ctt.content?.messageId;
                msg.content = ctt.chatIndex;
                var messageHelper = GetMessageHelper(ctt.sessionId);
                if (messageHelper != null && messageHelper.LocalWindowHelper.IsCurrentTalkWin)
                {
                    AntSdkChatMsg.ChatBase chatMsg = new AntSdkChatMsg.ChatBase();
                    chatMsg.sendUserId = ctt.sendUserId;
                    chatMsg.messageId = ctt.content?.messageId;
                    chatMsg.targetId = ctt.targetId;
                    //msg.companyCode = /*ctt.companyCode*/ AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode;
                    chatMsg.chatIndex = ctt.chatIndex;
                    chatMsg.sessionId = ctt.sessionId;
                    chatMsg.os = ctt.os;
                    chatMsg.sourceContent = /*ctt.content*/ ctt.content?.readIndex.ToString();
                    SessionMonitor.OfflineMsgBurnAfterReadReceipt(chatMsg, AntSdkMsgType.PointBurnReaded);
                }
                BurnAfterReadReceiptLocalData(msg, ctt.MsgType, isLast);
                //PublicMessageFunction.SendBurnAfterReadReceipt(msg, ctt.content?.messageId);
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[HandleBurnAfterReadReceipt]:" + e.Message + "," + e.StackTrace);
            }
        }

        /// <summary>
        /// 点对点已读阅后即焚消息本地库处理
        /// </summary>
        public static void BurnAfterReadReceiptLocalData(BurnAfterReadReceiptCtt msg, AntSdkMsgType msgType, bool isLast)
        {

            var readIndex = string.Empty;
            var tSession = t_sessionBll.GetModelByKey(msg.sessionId);
            var isExit = t_chat.MsgExit(msg.messageId);
            //if (msg.sendUserId != AntSdkService.AntSdkCurrentUserInfo.userId)
            //    PublicMessageFunction.SendSysUserMsgReceipt(msg.chatIndex, msg.sessionId, msgType);
            if (isExit)
            {

                //用户A收到消息服务转发的B已读阅后即焚消息的通知
                if (msgType == AntSdkMsgType.PointBurnReaded)
                {
                    if (string.IsNullOrEmpty(msg.content)) return;
                    readIndex = msg.content;

                    //删除本地记录
                    //t_chat.DeleteByMessageId(msg.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, msg.messageId);
                    messageIds.Append("'" + msg.messageId + "'" + ",");
                }
                else //用户B收到A的阅后即焚消息，发送已读回执给消息服务
                {
                    readIndex = msg.chatIndex;
                    if (msg.os != ((int)GlobalVariable.OSType.PC).ToString())
                    {
                        //删除本地记录
                        //t_chat.DeleteByMessageId(msg.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId, msg.messageId);
                        messageIds.Append("'" + msg.messageId + "'" + ",");
                    }
                }
                if (!string.IsNullOrEmpty(msg.messageId))
                {
                    if (LocalUnreadMsgList != null && LocalUnreadMsgList.Count > 0)
                    {
                        Tuple<string, GlobalVariable.BurnFlag> tuple = Tuple.Create(msg.sessionId,
                            GlobalVariable.BurnFlag.NotIsBurn);
                        if (LocalUnreadMsgList.ContainsKey(tuple))
                        {
                            var unReadMessage =
                                LocalUnreadMsgList[tuple].FirstOrDefault(m => m.messageId == msg.messageId);
                            if (unReadMessage != null)
                                LocalUnreadMsgList[tuple].Remove(unReadMessage);
                            if (SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple))
                            {
                                if (SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple].Count > 0)
                                {
                                    SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple].Count -= 1;
                                }
                                else
                                {
                                    SessionMonitor.WaitingToReceiveOfflineMsgNum.Remove(tuple);
                                }
                            }
                        }
                    }
                }
            }
            //ThreadPool.QueueUserWorkItem(n =>
            //{
            if (isLast)
            {

                var messageids = messageIds.ToString();
                if (!string.IsNullOrEmpty(messageids))
                {
                    messageids = messageids.TrimEnd(',');
                    t_chat.DeleteByMessageIds(msg.companyCode, AntSdkService.AntSdkCurrentUserInfo.userId,
                        messageids);
                    messageIds.Clear();
                }

            }
            Tuple<string, GlobalVariable.BurnFlag> tuple2 = Tuple.Create(msg.sessionId,
                       GlobalVariable.BurnFlag.NotIsBurn);
            //如果读到的阅后即焚消息不是最后一条消息则无需处理
            if (tSession == null)
            {
                if (!isLast) return;
                SessionMonitor.AddSessionItemOfflineMsg(msg.sessionId, null, null);
                return;
            }
            if (!string.IsNullOrEmpty(readIndex) && int.Parse(readIndex) < int.Parse(tSession.LastChatIndex))
            {
                if (!isLast) return;
                var tempChatMsg = t_chat.GetBeforeRecordByChatIndex(readIndex, msg.sessionId);
                var contactUser =
                    AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                        c => c.userId == tempChatMsg?.sendUserId);
                if (tempChatMsg != null)
                    tempChatMsg.chatType = (int)AntSdkchatType.Point;
                if (SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    tSession.UnreadCount = SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count > 0 ? SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count : 0;
                }
                else
                {
                    tSession.UnreadCount = 0;
                }
                t_sessionBll.Update(tSession);
                SessionMonitor.AddSessionItemOfflineMsg(tSession.LastMsg, contactUser, tempChatMsg);
                return;
            }

            var chatMsg = t_chat.GetBeforeRecordByChatIndex(readIndex, msg.sessionId);
            if (chatMsg == null)
            {
                //ThreadPool.QueueUserWorkItem(m =>
                //{

                tSession.SessionId = tSession.SessionId;
                tSession.LastMsg = string.Empty;
                tSession.LastMsgTimeStamp = tSession.LastMsgTimeStamp;
                tSession.LastChatIndex = tSession.LastChatIndex;
                if (SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    if (SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count > 0)
                    {
                        tSession.UnreadCount = SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count;
                    }
                    else
                    {
                        tSession.UnreadCount = 0;
                    }
                }
                else
                {
                    tSession.UnreadCount = 0;
                }
                t_sessionBll.Update(tSession);
                //特殊处理
                if (!isLast) return;

                SessionMonitor.AddSessionItemOfflineMsg(tSession.SessionId, null, null);
                //});
            }
            else
            {
                //ThreadPool.QueueUserWorkItem(m =>
                //{
                var contactUser =
                AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == chatMsg.sendUserId);
                string lastMsg = PublicMessageFunction.FormatLastMessageContent(chatMsg.MsgType, chatMsg, false);
                tSession.SessionId = chatMsg.sessionId;
                tSession.LastMsg = lastMsg;
                tSession.LastMsgTimeStamp = chatMsg.sendTime;
                tSession.LastChatIndex = chatMsg.chatIndex;
                if (SessionMonitor.WaitingToReceiveOfflineMsgNum.ContainsKey(tuple2))
                {
                    tSession.UnreadCount = SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count > 0 ? SessionMonitor.WaitingToReceiveOfflineMsgNum[tuple2].Count : 0;
                }
                else
                {
                    tSession.UnreadCount = 0;
                }
                t_sessionBll.Update(tSession);
                if (!isLast) return;
                chatMsg.chatType = (int)AntSdkchatType.Point;
                SessionMonitor.AddSessionItemOfflineMsg(lastMsg, contactUser, chatMsg);
                //});
            }
            //});

        }

        /// <summary>
        /// 添加新的消息控制器
        /// </summary>
        /// <param name="messagehelper"></param>
        public static void AddMessageHelper(IMessageHelper messagehelper)
        {
            if (_lstMessageHelper == null)
            {
                _lstMessageHelper = new List<IMessageHelper>();
            }
            lock (typeof(MessageHelper))
            {
                if (!CheckMessageHelperIsHad(messagehelper.LocalWindowHelper.WindowID))
                {
                    _lstMessageHelper.Add(messagehelper);
                }
            }
        }


        /// <summary>
        /// 按Window标识释放资源
        /// </summary>
        /// <param name="strWindowID">Window标识</param>
        public static void DisposeMessageHelper(string strWindowID)
        {
            RemoveMessageHelper(strWindowID);
        }

        /// <summary>
        /// 按窗体ID移除消息控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID)</param>
        public static void RemoveMessageHelper(string strID)
        {
            if (CheckMessageHelperIsHad(strID))
            {
                IMessageHelper messageHelper = GetMessageHelper(strID);
                _lstMessageHelper.Remove(messageHelper);
            }
        }

        /// <summary>
        /// 按窗体ID获取消息控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID)</param>
        /// <returns></returns>
        public static IMessageHelper GetMessageHelper(string strID)
        {
            if (CheckMessageHelperIsHad(strID))
            {
                return _lstMessageHelper.Find(m => m.LocalWindowHelper.WindowID == strID);
            }
            return null;
        }

        /// <summary>
        /// 按窗体ID检查是否有符合的消息控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID)</param>
        /// <returns></returns>
        public static bool CheckMessageHelperIsHad(string strID)
        {
            if (_lstMessageHelper != null)
                return _lstMessageHelper.Exists(m => m.LocalWindowHelper != null && m.LocalWindowHelper.WindowID == strID);
            return false;
        }
    }
}
