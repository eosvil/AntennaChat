using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Talk;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using Antenna.Model.PictureAndTextMix;
using SDK.AntSdk.DAL;
using SDK.AntSdk.BLL;

namespace AntennaChat.Helper
{
    public class PublicMessageFunction
    {
        static BaseBLL<AntSdkTsession, T_SessionDAL> t_sessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
        /// <summary>
        /// 格式化最后一条消息（会话列表中显示内容）
        /// </summary>
        public static string FormatLastMessageContent(AntSdkMsgType msgType, AntSdkChatMsg.ChatBase chatMsg, bool isGroupMsg, string startName = null)
        {
            var message = string.Empty;
            if (!isGroupMsg && chatMsg.flag == ((int)AntSdkBurnFlag.IsBurn)) //点对点聊天的阅后即焚消息（群聊的正常显示）
            {
                message = "无痕消息";
                if (!chatMsg.sessionId.StartsWith("G"))
                    message = "[阅后即焚消息]";
            }
            else if (msgType == AntSdkMsgType.ChatMsgAt)
            {
                var atMsg = chatMsg as AntSdkChatMsg.At;
                if (atMsg != null)
                {
                    message = FormatAtMsg(atMsg, startName);
                }
            }
            else
            {
                bool isIncludeAtMsg = false;
                switch (msgType)
                {
                    case AntSdkMsgType.ChatMsgText:
                    case AntSdkMsgType.Revocation:
                        message = chatMsg.sourceContent;
                        break;
                    case AntSdkMsgType.ChatMsgPicture:
                        message = "[图片]";
                        break;
                    case AntSdkMsgType.ChatMsgFile:
                        message = "[文件]";
                        break;
                    case AntSdkMsgType.ChatMsgVideo:
                        message = "[视频]";
                        break;
                    case AntSdkMsgType.PointAudioVideo:
                        message = "音频";
                        break;
                    case AntSdkMsgType.ChatMsgAudio:
                        message = "[语音]";
                        break;
                    case AntSdkMsgType.CreateVote:
                        message = "[投票]";
                        break;
                    case AntSdkMsgType.CreateActivity:
                        message = "[活动]";
                        break;
                    case AntSdkMsgType.ChatMsgMixMessage:
                        try
                        {
                            List<MixMessageObjDto> obj = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(chatMsg.sourceContent.ToString());
                            var strSB = new StringBuilder();
                            if (obj.Any())
                            {

                                foreach (var mixImageText in obj)
                                {
                                    if (mixImageText.type == "1001")
                                        strSB.Append(mixImageText.content);
                                    else if (mixImageText.type == "1002")
                                    {
                                        strSB.Append("[图片]");
                                    }
                                    else if (mixImageText.type == "1008")
                                    {
                                        if (mixImageText.content == null) break;
                                        try
                                        {
                                            var atMessage = JsonConvert.DeserializeObject<List<ATMessage>>(mixImageText.content.ToString());
                                            if (atMessage == null || atMessage.Count <= 0 ||
                                                string.IsNullOrEmpty(atMessage[0].names[0])) continue;
                                            isIncludeAtMsg = true;
                                            strSB.Append("@");
                                            strSB.Append(atMessage[0].names[0]);
                                        }
                                        catch (Exception ex)
                                        {
                                            message = "[混合消息]";
                                            LogHelper.WriteError("-------------------------------列表解析混合消息内容异常:chatMsg.os:----" + chatMsg.os + "----chatMsg.sourceContent：" + chatMsg.sourceContent + "mixImageText.content:" + mixImageText.content + ex.Message);
                                        }
                                    }
                                }
                                message = strSB.ToString();
                            }
                            else
                            {
                                message = "[混合消息]";
                            }
                        }
                        catch (Exception ex)
                        {
                            message = "[混合消息]";
                            LogHelper.WriteError("-------------------------------列表解析混合消息内容异常:chatMsg.os:----" + chatMsg.os + "----chatMsg.sourceContent：" + chatMsg.sourceContent + ex.Message);
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(startName))
                {
                    message = isIncludeAtMsg ? "[~!@]" + startName + ":" + message : startName + ":" + message;
                }
            }
            //返回信息
            return message;
        }

        /// <summary>
        /// 发送收到的聊天消息回执
        /// </summary>
        /// <param name="chatMsg"></param>
        public static void SendChatMsgReceipt(string sessionID, string chatIndex, AntSdkMsgType msgType, AntSdkReceiptType receiptType)
        {
            if (string.IsNullOrEmpty(chatIndex) || string.IsNullOrEmpty(sessionID)) return;
            //ChatMsgReceipt msgReceipt = new ChatMsgReceipt();
            //msgReceipt.ctt = new ChatMsgReceipt_Ctt();
            //msgReceipt.ctt.sendUserId = AntSdkService.AntSdkLoginOutput.userId;
            //msgReceipt.ctt.companyCode = GlobalVariable.CompanyCode;
            //msgReceipt.ctt.chatIndex = chatIndex;
            //msgReceipt.ctt.sessionId = sessionID;
            //msgReceipt.ctt.os = (int)GlobalVariable.OSType.PC;
            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkReceiptMsg receiptMsg = new AntSdkReceiptMsg();
            receiptMsg.chatIndex = chatIndex;
            receiptMsg.sessionId = sessionID;
            receiptMsg.userId = AntSdkService.AntSdkLoginOutput.userId;
            receiptMsg.MsgType = msgType;
            AntSdkService.SdkPublishReceiptMsg(receiptMsg, receiptType, ref errMsg);
            //MqttService.Instance.Publish<ChatMsgReceipt>(topic, msgReceipt, ref errMsg);
        }

        /// <summary>
        /// 用户A收到B已读阅后即焚的通知需要发送回执
        /// </summary>
        public static void SendBurnAfterReadReceipt(BurnAfterReadReceiptCtt msg, string messageID)
        {
            //BurnAfterReadReceipt receipt = new BurnAfterReadReceipt();
            //receipt.ctt = new BurnAfterReadReceiptCtt();
            //receipt.ctt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
            //receipt.ctt.companyCode = GlobalVariable.CompanyCode;
            //receipt.ctt.chatIndex = msg.chatIndex;
            //receipt.ctt.os = ((int)GlobalVariable.OSType.PC).ToString();
            //receipt.ctt.sessionId = msg.sessionId;
            //receipt.ctt.targetId = null;
            //receipt.ctt.content = null;
            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DNOE:AntSdk_Modify
            var burnRead = new AntSdkSendMsg.PointBurnReaded
            {
                targetId = msg.targetId,
                chatIndex = msg.chatIndex,
                sessionId = msg.sessionId,
                sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId,
                messageId = PublicTalkMothed.timeStampAndRandom(),
                chatType = (int)AntSdkchatType.Point,
                os = (int)GlobalVariable.OSType.PC,
                content = new AntSdkSendMsg.PointBurnReaded_content
                {
                    readIndex = int.Parse(msg.chatIndex),
                    //TODO://收到的那条阅后即焚消息的messageId
                    messageId = messageID
                }
            };
            AntSdkService.SdkPublishPointBurnReadReceiptMsg(burnRead, ref errMsg);
            //MqttService.Instance.Publish<BurnAfterReadReceipt>(GlobalVariable.TopicClass.MessageRead, receipt, ref errMsg, NullValueHandling.Ignore);
        }


        /// <summary>
        /// 发送用户通知回执
        /// </summary>
        /// 
        /// <param name="chatMsg"></param>
        public static void SendSysUserMsgReceipt(string chatIndex, string sessionId, AntSdkMsgType msgType)
        {
            if (string.IsNullOrEmpty(chatIndex) || string.IsNullOrEmpty(sessionId)) return;
            //SysUserMsgReceipt msgReceipt = new SysUserMsgReceipt();
            //msgReceipt.mtp = (int)GlobalVariable.MsgType.SysUserMsg;
            //msgReceipt.ctt = new SysUserMsgReceipt_Ctt();
            //msgReceipt.ctt.sendUserId = AntSdkService.AntSdkLoginOutput.userId;
            //msgReceipt.ctt.companyCode = GlobalVariable.CompanyCode;
            //msgReceipt.ctt.chatIndex = chatIndex;
            //msgReceipt.ctt.sessionId = sessionId;
            //msgReceipt.ctt.os = (int)GlobalVariable.OSType.PC;

            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkReceiptMsg receiptMsg = new AntSdkReceiptMsg();
            receiptMsg.chatIndex = chatIndex;
            receiptMsg.sessionId = sessionId;
            receiptMsg.userId = AntSdkService.AntSdkLoginOutput.userId;
            receiptMsg.MsgType = msgType;
            AntSdkService.SdkPublishReceiptMsg(receiptMsg, AntSdkReceiptType.ReadReceipt, ref errMsg);
            //MqttService.Instance.Publish<SysUserMsgReceipt>(GlobalVariable.TopicClass.MessageRead, msgReceipt, ref errMsg);
        }

        /// <summary>
        /// 解析AT消息
        /// </summary>
        private static string FormatAtMsg(AntSdkChatMsg.At atMsg, string startName)
        {
            string showContent = "";
            //TODO:AntSdk_Modify AT 消息
            //var message = atMsg.atcontent?.At_contents;
            if (!string.IsNullOrEmpty(startName))
            {
                showContent = startName + ":" + showContent;
            }
            //if (AntSdkService.AntSdkCurrentUserInfo.userId != atMsg.sendUserId)
            //{
            //    if (
            //        atMsg.atcontent?.At_users.Any(
            //            atuser =>
            //                (atuser.id == AntSdkService.AntSdkCurrentUserInfo.userId || atuser.id == atMsg.sessionId)) ??
            //        false)
            //    {
            //        message = "[~!@]" + message;
            //    }
            //}
            //return message;
            //string contents = "";
            //foreach(var msg in atMsg.content)
            //{
            //    contents += msg.content;
            //}
            //构造展示消息
            bool isShowAt = false;
            foreach (var str in atMsg.content)
            {
                switch (str.type)
                {
                    //文本
                    case "1001":
                        showContent += str.content;
                        continue;
                    //@全体成员
                    case "1111":
                        showContent += "@全体成员";
                        isShowAt = true;
                        continue;
                    //@个人
                    case "1112":
                        string strAt = "";
                        if (str.ids.Count() > 1)
                        {
                            foreach (var name in str.names)
                            {
                                strAt += "@" + name[0];
                                if (str.ids.FirstOrDefault(m => m == AntSdkService.AntSdkCurrentUserInfo.userId) != null)
                                {
                                    isShowAt = true;
                                }
                            }
                        }
                        else
                        {
                            strAt += "@" + str.names[0];
                            if (str.ids.FirstOrDefault(m => m == AntSdkService.AntSdkCurrentUserInfo.userId) != null)
                            {
                                isShowAt = true;
                            }
                        }
                        showContent += strAt;
                        continue;
                    //换行
                    case "0000":
                        showContent += "\r\n";
                        continue;
                    //图片
                    case "1002":
                        continue;
                }
                //if (AntSdkService.AntSdkCurrentUserInfo.userId != atMsg.sendUserId)
                //{
                //    if (
                //        atMsg.content?.At_users.Any(
                //            atuser =>
                //                (atuser.id == AntSdkService.AntSdkCurrentUserInfo.userId || atuser.id == atMsg.sessionId)) ??
                //        false)
                //    {
                //        message = "[~!@]" + message;
                //    }
                //}

            }
            if (isShowAt)
                return "[~!@]" + showContent;
            else
                return showContent;
        }
        /// <summary>
        /// 消息时间
        /// </summary>
        /// <param name="lastMsgTime">消息时间戳</param>
        /// <param name="burnLastMsgTime">阅后即焚消息时间戳</param>
        public static string LastMsgTime(string lastMsgTime, string burnLastMsgTime)
        {
            return string.IsNullOrEmpty(burnLastMsgTime) ? lastMsgTime : burnLastMsgTime;
        }
        /// <summary>
        /// 从网络查询消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <param name="chatType">聊天类型</param>
        /// <param name="first">第一次查询，传true；其他传false</param>
        /// <param name="msgCount">查询数量，范围在[1,20],超出的话，会报错</param>
        /// <param name="index">起始chatIndex，如果第一次查询的话，可以传0；如果不是第一次查询，那么查询出来的消息的index都是小于这个chatIndex的</param>
        /// <returns></returns>
        public static List<AntSdkChatMsg.ChatBase> QueryMessageFromServer(string sessionId, AntSdkchatType chatType, bool first, int msgCount, int index)
        {
            var input = new AntSdkSynchronusMsgInput
            {
                sessionId = sessionId,
                chatType = (int)chatType,
                flag = 0,
                userId = AntSdkService.AntSdkLoginOutput.userId,
                isFirst = first,
                count = msgCount,
                chatIndex = index,
            };
            var chatMsgList = new List<AntSdkChatMsg.ChatBase>();
            int errorCode = 0;
            string errorMsg = string.Empty;
            AntSdkService.SynchronusMsgs(input, ref chatMsgList, ref errorCode, ref errorMsg);
            if (chatMsgList == null || chatMsgList.Count == 0)
                return null;
            else
            {

                var tempChatMsg = chatMsgList[chatMsgList.Count - 1];
                var localMsgSession = t_sessionBll.GetModelByKey(tempChatMsg.sessionId);
                var chatIndex = 0;
                int.TryParse(tempChatMsg.chatIndex, out chatIndex);

                if (localMsgSession != null)
                {
                    var localChatIndex = 0;
                    var burnChatIndex = 0;
                    if (!string.IsNullOrEmpty(localMsgSession.LastChatIndex))
                        int.TryParse(localMsgSession.LastChatIndex, out localChatIndex);
                    if (!string.IsNullOrEmpty(localMsgSession.BurnLastChatIndex))
                        int.TryParse(localMsgSession.BurnLastChatIndex, out burnChatIndex);
                    if (localChatIndex < burnChatIndex)
                    {
                        localChatIndex = burnChatIndex;
                    }
                    if (chatIndex > localChatIndex)
                    {
                        var antSdkContactUser = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == tempChatMsg.sendUserId);
                        localMsgSession.LastChatIndex = tempChatMsg.chatIndex;
                        localMsgSession.LastMsg = antSdkContactUser != null ?
                            FormatLastMessageContent(tempChatMsg.MsgType, tempChatMsg, chatType == AntSdkchatType.Group,
                            antSdkContactUser.userNum != null ? antSdkContactUser.userNum + antSdkContactUser.userName : antSdkContactUser.userName) :
                            PublicMessageFunction.FormatLastMessageContent(tempChatMsg.MsgType, tempChatMsg, chatType == AntSdkchatType.Group);
                        localMsgSession.LastMsgTimeStamp = tempChatMsg.sendTime;
                        t_sessionBll.Update(localMsgSession);
                        tempChatMsg.chatType = (int)chatType;
                        SessionMonitor.AddSessionItemOnlineMsg(tempChatMsg.MsgType, tempChatMsg, false);
                    }
                }
                return chatMsgList;
            }
        }
        /// <summary>
        /// 从本地查询消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <param name="chatType">聊天类型</param>
        /// <param name="first">第一次查询，传true；其他传false</param>
        /// <param name="msgCount">查询数量，范围在[1,20],超出的话，会报错</param>
        /// <param name="index">起始chatIndex，如果第一次查询的话，可以传0；如果不是第一次查询，那么查询出来的消息的index都是小于这个chatIndex的</param>
        /// <returns></returns>
        public static List<AntSdkChatMsg.ChatBase> QueryMessageFromLocal(string sessionId, AntSdkchatType chatType, bool first, int msgCount, int index)
        {
            var input = new AntSdkSynchronusMsgInput
            {
                sessionId = sessionId,
                chatType = (int)chatType,
                flag = 0,
                userId = AntSdkService.AntSdkLoginOutput.userId,
                isFirst = first,
                count = msgCount,
                chatIndex = index,
            };
            var listChatdata = new List<AntSdkChatMsg.ChatBase>();
            var result = AntSdkService.GetLocalMsgData(input, ref listChatdata, false);
            return result ? listChatdata : null;
        }
    }
}
