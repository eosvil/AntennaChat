using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDK.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Properties;

namespace SDK.Service
{
    /// <summary>
    /// 类 名 称：SDK专用MsgConverter[消息转换器]
    /// 类 说 明：消息转换器
    /// 作    者：
    /// 完成日期：2017/4/15
    /// </summary>
    internal class MsgConverter
    {
        #region //反序列化转换接收到的消息(二维数组)

        #region   //SDK接收到聊天消息

        /// <summary>
        /// 方法说明：根据消息类型组织聊天消息信息
        /// 作    者：曹李刚
        /// 完成时间：2017-05-10
        /// </summary>
        /// <param name="msType">聊天信息类型</param>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="eventType">返回需要外部识别的消息事件类型</param>
        /// <param name="errorMsg">错误提示信息</param>
        /// <returns>聊天信息实体</returns>
        public static MsSdkMessageChat ReceiveChatMsgEntity(int msType, string[][] mqArray,
            ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                MsSdkMessageChat chatObj = null;
                //转换聊天内容信息
                switch (msType)
                {
                    case (int) SdkEnumCollection.ChatMsgType.Text: //收到聊天消息(文本)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgText>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgText;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.Picture: //收到聊天消息(图片)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgPicture>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgPicture;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.Audio: //收到聊天消息(音频)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgAudio>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgAudio;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.Video: //收到聊天消息(视频)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgVideo>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgVideo;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.File: //收到聊天消息(文件)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgFile>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgFile;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.MapLocation: //收到聊天消息(地理位置)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgMapLocation>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgMapLocation;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.MixImageText: //收到聊天消息(图文混合)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgMixMessage>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgMixMessage;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.At: //收到聊天消息(@消息)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgAt>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgAt;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.MultiAudioVideo: //收到聊天消息(多人视频)
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgMultiAudioVideo>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ChatMsgMultiAudioVideo;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatMsgType.PointAudioVideo: //单人（点对点）音频视频消息
                    {
                        chatObj = GetChatMsgEntity<MsChatMsgPointAudioVideo>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.PointAudioVideo;
                    }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.Revocation: //消息撤回
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgRevocation>(mqArray, ref errorMsg);
                            eventType = SdkMsgType.Revocation;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.CreateVote://创建投票
                        chatObj = GetChatMsgEntity<MsChatMsgCreateVote>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CreateVote;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.DeleteVote://删除投票
                        chatObj = GetChatMsgEntity<MsChatMsgDeteleVote>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DeleteVote;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.CreateActivity://创建活动
                        chatObj = GetChatMsgEntity<MsChatMsgActivity>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CreateActivity;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.DeleteActivity://删除活动
                        chatObj = GetChatMsgEntity<MsChatMsgDeleteActivity>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DeleteActivity;
                        break;
                    default:
                    {
                    }
                        break;
                }
                //返回
                return chatObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveChatMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取聊天室信息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <typeparam name="T">聊天室消息类型</typeparam>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static T GetChatMsgEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkMessageChat, new()
        {
            var logKey = typeof (T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    sourceContent = array[1][11]
                };
                var chatmsgtext = entity as MsChatMsgText;
                if (chatmsgtext != null)
                {
                    chatmsgtext.content = array[1][11];
                    return entity;
                }
                var chatmsgpicture = entity as MsChatMsgPicture;
                if (chatmsgpicture != null)
                {
                    chatmsgpicture.content = JsonCoder.DeserializeObject<MsChatMsgPicture_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatmsgaudio = entity as MsChatMsgAudio;
                if (chatmsgaudio != null)
                {
                    chatmsgaudio.content = JsonCoder.DeserializeObject<MsChatMsgAudio_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatmsgvideo = entity as MsChatMsgVideo;
                if (chatmsgvideo != null)
                {
                    chatmsgvideo.content = JsonCoder.DeserializeObject<MsChatMsgVideo_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatmsgfile = entity as MsChatMsgFile;
                if (chatmsgfile != null)
                {
                    chatmsgfile.content = JsonCoder.DeserializeObject<MsChatMsgFile_content>(array[1][11], ref errorMsg);
                    return entity;
                }
                var chatmsglocation = entity as MsChatMsgMapLocation;
                if (chatmsglocation != null)
                {
                    chatmsglocation.content = JsonCoder.DeserializeObject<MsChatMsgMapLocation_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatmsgmiximagetext = entity as MsChatMsgMixMessage;
                if (chatmsgmiximagetext != null)
                {
                    chatmsgmiximagetext.content = array[1][11];
                    return entity;
                }
                var chatmsgat = entity as MsChatMsgAt;
                if (chatmsgat != null)
                {
                    chatmsgat.content = JsonCoder.DeserializeObject<List<MsChatMsgAt_content>>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatmsgmultivideo = entity as MsChatMsgMultiAudioVideo;
                if (chatmsgmultivideo != null)
                {
                    chatmsgmultivideo.content =
                        JsonCoder.DeserializeObject<MsChatMsgMultiAudioVideo_content>(array[1][11], ref errorMsg);
                    return entity;
                }
                var chatmsgpointauvdeo = entity as MsChatMsgPointAudioVideo;
                if (chatmsgpointauvdeo != null)
                {
                    chatmsgpointauvdeo.content =
                        JsonCoder.DeserializeObject<MsChatMsgPointAudioVideo_content>(array[1][11], ref errorMsg);
                    return entity;
                }
                var chatrevocation = entity as MsChatMsgRevocation;
                if (chatrevocation != null)
                {
                    chatrevocation.content = JsonCoder.DeserializeObject<MsChatMsgRevocation_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatcreatevote = entity as MsChatMsgCreateVote;
                if (chatcreatevote != null)
                {
                    chatcreatevote.content = JsonCoder.DeserializeObject<MsChatMsgCreateVote_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatdeletevote = entity as MsChatMsgDeteleVote;
                if (chatdeletevote != null)
                {
                    chatdeletevote.content = JsonCoder.DeserializeObject<MsChatMsgDeteleVote_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatacitivity = entity as MsChatMsgActivity;
                if (chatacitivity != null)
                {
                    chatacitivity.content = JsonCoder.DeserializeObject<MsChatMsgActivity_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var chatadetelecitivity = entity as MsChatMsgDeleteActivity;
                if (chatadetelecitivity != null)
                {
                    chatadetelecitivity.content = JsonCoder.DeserializeObject<MsChatMsgDeleteActivity_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                //返回
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 漫游消息转换
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="eventType"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static MsSdkMessageChat ReceiveChatMsgEntity(SynchronusMsgOutput msg,
           ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                MsSdkMessageChat chatObj = null;
                int msType = 0;
                int.TryParse(msg.messageType, out msType);
                //转换聊天内容信息
                switch (msType)
                {
                    case (int)SdkEnumCollection.ChatMsgType.Text: //收到聊天消息(文本)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgText>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgText;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.Picture: //收到聊天消息(图片)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgPicture>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgPicture;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.Audio: //收到聊天消息(音频)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgAudio>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgAudio;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.Video: //收到聊天消息(视频)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgVideo>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgVideo;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.File: //收到聊天消息(文件)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgFile>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgFile;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.MapLocation: //收到聊天消息(地理位置)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgMapLocation>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgMapLocation;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.MixImageText: //收到聊天消息(图文混合)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgMixMessage>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgMixMessage;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.At: //收到聊天消息(@消息)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgAt>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgAt;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.MultiAudioVideo: //收到聊天消息(多人视频)
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgMultiAudioVideo>(msg, ref errorMsg);
                            eventType = SdkMsgType.ChatMsgMultiAudioVideo;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.PointAudioVideo: //单人（点对点）音频视频消息
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgPointAudioVideo>(msg, ref errorMsg);
                            eventType = SdkMsgType.PointAudioVideo;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.Revocation: //消息撤回
                        {
                            chatObj = GetChatMsgEntity<MsChatMsgRevocation>(msg, ref errorMsg);
                            eventType = SdkMsgType.Revocation;
                        }
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.CreateVote://创建投票
                        chatObj = GetChatMsgEntity<MsChatMsgCreateVote>(msg, ref errorMsg);
                        eventType = SdkMsgType.CreateVote;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.DeleteVote://删除投票
                        chatObj = GetChatMsgEntity<MsChatMsgDeteleVote>(msg, ref errorMsg);
                        eventType = SdkMsgType.DeleteVote;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.CreateActivity://创建活动
                        chatObj = GetChatMsgEntity<MsChatMsgActivity>(msg, ref errorMsg);
                        eventType = SdkMsgType.CreateActivity;
                        break;
                    case (int)SdkEnumCollection.ChatMsgType.DeleteActivity://删除活动
                        chatObj = GetChatMsgEntity<MsChatMsgDeleteActivity>(msg, ref errorMsg);
                        eventType = SdkMsgType.DeleteActivity;
                        break;
                    default:
                        {
                        }
                        break;
                }
                //返回
                return chatObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveChatMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }
        /// <summary>
        /// 设置漫游消息实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static T GetChatMsgEntity<T>(SynchronusMsgOutput msg, ref string errorMsg)
           where T : MsSdkMessageChat, new()
        {
            var logKey = typeof(T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = msg.messageType,
                    chatType = msg.chatType,
                    os = msg.os,
                    flag = msg.flag,
                    status = msg.status,
                    messageId = msg.messageId,
                    appKey = msg.appKey,
                    sendUserId = msg.sendUserId,
                    targetId = msg.targetId,
                    sessionId = msg.sessionId,
                    chatIndex = msg.chatIndex,
                    sendTime = msg.sendTime,
                    attr = msg.attr,
                    sourceContent = msg.content
                };
                var chatmsgtext = entity as MsChatMsgText;
                if (chatmsgtext != null)
                {
                    chatmsgtext.content = msg.content;
                    return entity;
                }
                var chatmsgpicture = entity as MsChatMsgPicture;
                if (chatmsgpicture != null)
                {
                    chatmsgpicture.content = JsonCoder.DeserializeObject<MsChatMsgPicture_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatmsgaudio = entity as MsChatMsgAudio;
                if (chatmsgaudio != null)
                {
                    chatmsgaudio.content = JsonCoder.DeserializeObject<MsChatMsgAudio_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatmsgvideo = entity as MsChatMsgVideo;
                if (chatmsgvideo != null)
                {
                    chatmsgvideo.content = JsonCoder.DeserializeObject<MsChatMsgVideo_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatmsgfile = entity as MsChatMsgFile;
                if (chatmsgfile != null)
                {
                    chatmsgfile.content = JsonCoder.DeserializeObject<MsChatMsgFile_content>(msg.content, ref errorMsg);
                    return entity;
                }
                var chatmsglocation = entity as MsChatMsgMapLocation;
                if (chatmsglocation != null)
                {
                    chatmsglocation.content = JsonCoder.DeserializeObject<MsChatMsgMapLocation_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatmsgmiximagetext = entity as MsChatMsgMixMessage;
                if (chatmsgmiximagetext != null)
                {
                    chatmsgmiximagetext.content = msg.content;
                    return entity;
                }
                var chatmsgat = entity as MsChatMsgAt;
                if (chatmsgat != null)
                {
                    chatmsgat.content = JsonCoder.DeserializeObject<List<MsChatMsgAt_content>>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatmsgmultivideo = entity as MsChatMsgMultiAudioVideo;
                if (chatmsgmultivideo != null)
                {
                    chatmsgmultivideo.content =
                        JsonCoder.DeserializeObject<MsChatMsgMultiAudioVideo_content>(msg.content, ref errorMsg);
                    return entity;
                }
                var chatmsgpointauvdeo = entity as MsChatMsgPointAudioVideo;
                if (chatmsgpointauvdeo != null)
                {
                    chatmsgpointauvdeo.content =
                        JsonCoder.DeserializeObject<MsChatMsgPointAudioVideo_content>(msg.content, ref errorMsg);
                    return entity;
                }
                var chatrevocation = entity as MsChatMsgRevocation;
                if (chatrevocation != null)
                {
                    chatrevocation.content = JsonCoder.DeserializeObject<MsChatMsgRevocation_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatcreatevote = entity as MsChatMsgCreateVote;
                if (chatcreatevote != null)
                {
                    chatcreatevote.content = JsonCoder.DeserializeObject<MsChatMsgCreateVote_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatdeletevote = entity as MsChatMsgDeteleVote;
                if (chatdeletevote != null)
                {
                    chatdeletevote.content = JsonCoder.DeserializeObject<MsChatMsgDeteleVote_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatacitivity = entity as MsChatMsgActivity;
                if (chatacitivity != null)
                {
                    chatacitivity.content = JsonCoder.DeserializeObject<MsChatMsgActivity_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                var chatadetelecitivity = entity as MsChatMsgDeleteActivity;
                if (chatadetelecitivity != null)
                {
                    chatadetelecitivity.content = JsonCoder.DeserializeObject<MsChatMsgDeleteActivity_content>(msg.content,
                        ref errorMsg);
                    return entity;
                }
                //返回
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取自定义消息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static MsSdkCustomEntity GetCustomMsgEntity(IReadOnlyList<string[]> array, ref string errorMsg)
        {
            try
            {
                var entity = new MsSdkCustomEntity
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    content = array[1][11],
                    sourceContent = array[1][11]
                };
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetCustomMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取自定义消息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static MsMultiTerminalSynch GetMultiTerminalEntity(IReadOnlyList<string[]> array, ref string errorMsg)
        {
            try
            {
                var entity = new MsMultiTerminalSynch
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    content = array[1][11],
                    sourceContent = array[1][11]
                };
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetCustomMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取自定义消息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static MsPointBurnReaded GetMsPointBurnReadedEntity(IReadOnlyList<string[]> array, ref string errorMsg)
        {
            try
            {
                var entity = new MsPointBurnReaded
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    content = JsonCoder.DeserializeObject<MsPointBurnReaded_content>(array[1][11], ref errorMsg),
                    sourceContent = array[1][11]
                };
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetCustomMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取自定义消息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static MsPointFileAccepted GetMsPointFileAcceptedEntity(IReadOnlyList<string[]> array, ref string errorMsg)
        {
            try
            {
                var entity = new MsPointFileAccepted
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    content = array[1][11],
                };
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetCustomMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        #endregion

        #region   //SDK收到用户信息通知消息

        /// <summary>
        /// 方法说明：根据消息类型组织用户消息信息
        /// 作    者：曹李刚
        /// 完成时间：2017-05-10
        /// </summary>
        /// <param name="msType">聊天信息类型</param>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="eventType">返回需要外部识别的消息事件类型</param>
        /// <param name="errorMsg">错误提示信息</param>
        /// <returns>聊天信息实体</returns>
        public static
            MsSdkUserBase ReceiveUserMsgEntity(int msType, string[][] mqArray,
                ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                MsSdkUserBase chatObj = null;
                //转换聊天内容信息
                switch (msType)
                {
                    case (int) SdkEnumCollection.UserStateNotify.OffLine:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.OffLine;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.OnLine:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.OnLine;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.Leave:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.Leave;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.Busy:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.Busy;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.Disable:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.Disable;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.PaswordChangeKickOut:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.PaswordChangeKickOut;
                    }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.KickOut:
                    {
                        chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.KickOut;
                    }
                        break;
                    case (int)SdkEnumCollection.UserStateNotify.PhoneLine:
                        {
                            chatObj = GetUserMsgEntity<MsUserStateChange>(mqArray, ref errorMsg);
                            eventType = SdkMsgType.PhoneLine;
                        }
                        break;
                    case (int) SdkEnumCollection.UserStateNotify.ModifyInfo:
                    {
                        chatObj = GetUserMsgEntity<MsUserInfoModify>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyInfo;
                    }
                        break;
                    default:
                    {
                    }
                        break;
                }
                //返回
                return chatObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveUserMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取用户信息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <typeparam name="T">聊天室消息类型</typeparam>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static T GetUserMsgEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkUserBase, new()
        {
            var logKey = typeof (T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    userId = array[1][0]
                };
                var userstate = entity as MsUserStateChange;
                if (userstate != null)
                {
                    userstate.attr = array[1][1];
                    return entity;
                }
                var userinfo = entity as MsUserInfoModify;
                if (userinfo == null)
                {
                    return entity;
                }
                userinfo.attr = JsonCoder.DeserializeObject<MsUserInfoModify_content>(array[1][1], ref errorMsg);
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetUserMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        #endregion

        #region   //SDK接收到聊天室变更通知消息

        /// <summary>
        /// 方法说明：根据消息类型组织聊天室信息
        /// 作    者：曹李刚
        /// 完成时间：2017-05-10
        /// </summary>
        /// <param name="msType">聊天室信息类型</param>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="eventType">返回需要外部识别的消息事件类型</param>
        /// <param name="errorMsg">错误提示信息</param>
        /// <returns>聊天室信息实体</returns>
        public static MsSdkMessageRoomBase ReceiveRoomMsgEntity(int msType, string[][] mqArray,
            ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                MsSdkMessageRoomBase roomObj = null;
                switch (msType)
                {
                    case (int) SdkEnumCollection.ChatRoomNotify.Create: //收到新增聊天室通知
                    {
                        roomObj = GetChatRoomEntity<MsCreateChatRoom>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CreateChatRoom;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.Dismiss: //聊天室解散
                    {
                        roomObj = GetChatRoomEntity<MsDeleteChatRoom>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DismissChatRoom;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.AddMember: //聊天室添加成员
                    {
                        roomObj = GetChatRoomEntity<MsAddChatRoomMembers>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.AddChatRoomMember;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.DeleteMember: //聊天室删除成员
                    {
                        roomObj = GetChatRoomEntity<MsDeleteChatRoomMembers>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DeleteChatRoomMember;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.QuitMember: //聊天室成员退出
                    {
                        roomObj = GetChatRoomEntity<MsQuitChatRoomMember>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.QuitChatRoomMember;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.ModifyMemberInfo: //聊天室成员信息变更
                    {
                        roomObj = GetChatRoomEntity<MsModifyChatRoomMember>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyGroupMember;
                    }
                        break;
                    case (int) SdkEnumCollection.ChatRoomNotify.Modify: //聊天室信息变更
                    {
                        roomObj = GetChatRoomEntity<MsModifyChatRoom>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyChatRoom;
                    }
                        break;
                    default:
                    {
                    }
                        break;
                }
                //返回
                return roomObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveRoomMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取聊天室信息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <typeparam name="T">聊天室消息类型</typeparam>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static T GetChatRoomEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkMessageRoomBase, new()
        {
            var logKey = typeof (T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    sessionId = array[1][0],
                    chatIndex = array[1][1],
                    sendTime = array[1][3]
                };
                //类型转移判断
                var createroom = entity as MsCreateChatRoom;
                if (createroom != null)
                {
                    createroom.content = JsonCoder.DeserializeObject<MsCreateChatRoom_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var deleteroom = entity as MsDeleteChatRoom;
                if (deleteroom != null)
                {
                    deleteroom.content = JsonCoder.DeserializeObject<DeleteChatRoom_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var addmemroom = entity as MsAddChatRoomMembers;
                if (addmemroom != null)
                {
                    addmemroom.content = JsonCoder.DeserializeObject<MsAddChatRoomMembers_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var delmemroom = entity as MsDeleteChatRoomMembers;
                if (delmemroom != null)
                {
                    delmemroom.content = JsonCoder.DeserializeObject<DeleteChatRoomMembers_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var quitexroom = entity as MsQuitChatRoomMember;
                if (quitexroom != null)
                {
                    quitexroom.content = JsonCoder.DeserializeObject<SdkMember>(array[1][2], ref errorMsg);
                    return entity;
                }
                var mdfmemroom = entity as MsModifyChatRoomMember;
                if (mdfmemroom != null)
                {
                    mdfmemroom.content = JsonCoder.DeserializeObject<MsModifyChatRoomMember_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var modifyroom = entity as MsModifyChatRoom;
                if (modifyroom == null)
                {
                    return null;
                }
                modifyroom.content = JsonCoder.DeserializeObject<MsModifyChatRoom_content>(array[1][2], ref errorMsg);
                return entity;
                //如果不是以上类型返回空
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatRoomEntity][{logKey}]:{e.Message},{e.StackTrace}");
                errorMsg = e.Message;
                return null;
            }
        }

        #endregion

        #region   //SDK接收到群组变更通知消息

        /// <summary>
        /// 方法说明：根据消息类型组织聊天室信息
        /// 作    者：曹李刚
        /// 完成时间：2017-05-10
        /// </summary>
        /// <param name="msType">聊天室信息类型</param>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="eventType">返回需要外部识别的消息事件类型</param>
        /// <param name="errorMsg">错误提示信息</param>
        /// <returns>聊天室信息实体</returns>
        public static MsSdkMessageGroupBase ReceiveGroupMsgEntity(int msType, string[][] mqArray,
            ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                MsSdkMessageGroupBase groupObj = null;
                switch (msType)
                {
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.Create: //收到新增群组通知
                    {
                        groupObj = GetChatGroupEntity<CreateGroup>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CreateGroup;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.Dismiss: //群组解散
                    {
                        groupObj = GetChatGroupEntity<MsDeleteGroup>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DissolveGroup;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.AddMember: //群组添加成员
                    {
                        groupObj = GetChatGroupEntity<MsAddGroupMembers>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.AddGroupMember;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.DeleteMember: //群组删除成员
                    {
                        groupObj = GetChatGroupEntity<MsDeleteGroupMembers>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DeleteGroupMember;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.QuitMember: //群组成员退出
                    {
                        groupObj = GetChatGroupEntity<MsQuitGroupMember>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.QuitGroupMember;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.ModifyMemberInfo: //群组成员信息变更
                    {
                        groupObj = GetChatGroupEntity<MsModifyGroupMember>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyGroupMember;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.Modify: //群组信息变更
                    {
                        groupObj = GetChatGroupEntity<MsModifyGroup>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyGroup;
                    }
                        break;
                    case (int)SdkEnumCollection.DiscussGroupInfoNotify.OwnerChanged://群主变更
                    {
                        groupObj = GetChatGroupEntity<MsGroupOwnerChanged>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.GroupOwnerChanged;
                    }
                        break;
                    case (int)SdkEnumCollection.DiscussGroupInfoNotify.AdminSet://管理员授权
                        {
                            groupObj = GetChatGroupEntity<MsGroupAdminSet>(mqArray, ref errorMsg);
                            eventType = SdkMsgType.AdminSet;
                        }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.BurnMode: //群主切换为阅后即焚模式
                    {
                        groupObj = GetChatGroupEntity<MsGroupOwnerBurnMode>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.GroupOwnerBurnMode;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.BurnModeDelete: //群主在阅后即焚模式下删除消息
                    {
                        groupObj = GetChatGroupEntity<MsGroupOwnerBurnDelete>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.GroupOwnerBurnDelete;
                    }
                        break;
                    case (int) SdkEnumCollection.DiscussGroupInfoNotify.NormalMode: //群主切换为正常聊天模式
                    {
                        groupObj = GetChatGroupEntity<MsGroupOwnerNormal>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.GroupOwnerNormal;
                    }
                        break;
                    default:
                    {
                    }
                        break;
                }
                //返回
                return groupObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveGroupMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取讨论组信息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <typeparam name="T">聊天室消息类型</typeparam>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static T GetChatGroupEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkMessageGroupBase, new()
        {
            var logKey = typeof (T).FullName;
            try
            {
                var entity = new T
                {

                    messageType = array[0][0],
                    sessionId = array[1][0],
                    chatIndex = array[1][1],
                    sendTime = array[1].Length >= 4 ? array[1][3] : string.Empty
                };
                //类型转移判断
                var creategroup = entity as CreateGroup;
                if (creategroup != null)
                {
                    creategroup.content = JsonCoder.DeserializeObject<CreateGroup_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var deletegroup = entity as MsDeleteGroup;
                if (deletegroup != null)
                {
                    deletegroup.content = JsonCoder.DeserializeObject<MsDeleteGroup_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var addmemgroup = entity as MsAddGroupMembers;
                if (addmemgroup != null)
                {
                    addmemgroup.content = JsonCoder.DeserializeObject<MsAddGroupMembers_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var delmemgroup = entity as MsDeleteGroupMembers;
                if (delmemgroup != null)
                {
                    delmemgroup.content = JsonCoder.DeserializeObject<MsDeleteGroupMembers_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var quitexgroup = entity as MsQuitGroupMember;
                if (quitexgroup != null)
                {
                    quitexgroup.content = JsonCoder.DeserializeObject<MsQuitGroupMember_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var mdfmemgroup = entity as MsModifyGroupMember;
                if (mdfmemgroup != null)
                {
                    mdfmemgroup.content = JsonCoder.DeserializeObject<MsModifyGroupMember_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                var modifygroup = entity as MsModifyGroup;
                if (modifygroup != null)
                {
                    modifygroup.content = JsonCoder.DeserializeObject<MsModifyGroup_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var ownerchanged = entity as MsGroupOwnerChanged;
                if (ownerchanged != null)
                {
                    ownerchanged.content = JsonCoder.DeserializeObject<MsGroupOwnerChanged_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var owneradminset = entity as MsGroupAdminSet;
                if (owneradminset != null)
                {
                    owneradminset.content = JsonCoder.DeserializeObject<MsGroupAdminSet_content>(array[1][2], ref errorMsg);
                    return entity;
                }
                var gponercburn = entity as MsGroupOwnerBurnMode;
                if (gponercburn != null)
                {
                    if (string.IsNullOrEmpty(array[1][2]))
                    {
                        gponercburn.content = new Models.MsGroupOwnerBurnMode_content();
                    }
                    else
                    {
                        gponercburn.content = JsonCoder.DeserializeObject<MsGroupOwnerBurnMode_content>(array[1][2],
                            ref errorMsg);
                    }
                    //返回
                    return entity;
                }
                var gponerclears = entity as MsGroupOwnerBurnDelete;
                if (gponerclears != null)
                {
                    if (string.IsNullOrEmpty(array[1][2]))
                    {
                        gponerclears.content = new Models.MsGroupOwnerBurnDelete_content();
                    }
                    else
                    {
                        gponerclears.content = JsonCoder.DeserializeObject<MsGroupOwnerBurnDelete_content>(array[1][2],
                            ref errorMsg);
                    }
                    //返回
                    return entity;
                }
                var gponerccomm = entity as MsGroupOwnerNormal;
                if (gponerccomm != null)
                {
                    gponerccomm.content = array[1][2];
                    return entity;
                }
                //如果不是以上类型返回空
                return null;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatGroupEntity]:[{logKey}]{e.Message},{e.StackTrace}");
                errorMsg = e.Message;
                return null;
            }
        }

        #endregion

        #region   //SDK收到回执等其他消息

        /// <summary>
        /// 方法说明：根据消息类型组织其他消息信息
        /// 作    者：曹李刚
        /// 完成时间：2017-05-10
        /// </summary>
        /// <param name="msType">聊天信息类型</param>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="eventType">返回需要外部识别的消息事件类型</param>
        /// <param name="errorMsg">错误提示信息</param>
        /// <returns>聊天信息实体</returns>
        public static SdkMsBase ReceiveOtherMsgEntity(int msType, string[][] mqArray,
            ref SdkMsgType eventType, ref string errorMsg)
        {
            try
            {
                SdkMsBase otherObj = null;
                //转换聊天内容信息
                switch (msType)
                {
                    case (int) SdkEnumCollection.OtherMsgType.MultiTerminalSynch: //多终端同步已读回执 topic：{appKey}/userId 
                    {
                        otherObj = GetMultiTerminalEntity(mqArray, ref errorMsg);
                        eventType = SdkMsgType.MultiTerminalSynch;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.MsgReceipt:
                        //收到消息服务回执:目前只有自己发的聊天消息会收到该回执 topic:{appKey}/{userId}/{os}
                    {
                        otherObj = GetReceiveMsgReceipt(mqArray, ref errorMsg);
                        eventType = SdkMsgType.MsgReceipt;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.PointFileAccepted: //点对点文件消息的已接受的回执 topic：{appKey}/userId 
                    {
                        otherObj = GetMsPointFileAcceptedEntity(mqArray, ref errorMsg);
                        eventType = SdkMsgType.PointFileAccepted;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.PointBurnReaded: //点对点阅后即焚消息已读的回执 topic：{appKey}/userId 
                    {
                        otherObj = GetMsPointBurnReadedEntity(mqArray, ref errorMsg);
                        eventType = SdkMsgType.PointBurnReaded;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.OrganizationModify:
                    {
                        otherObj = GetOrganizationModify(mqArray, ref errorMsg); //组织架构变化通知
                        eventType = SdkMsgType.OrganizationModify;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.VersionHardUpdate: //硬更新  topic：{appKey}/pc/version***  
                    {
                        otherObj = GetVersionHardUpdate(mqArray, ref errorMsg);
                        eventType = SdkMsgType.VersionHardUpdate;
                    }
                        break;
                    case (int) SdkEnumCollection.OtherMsgType.UnvarnishedMsg: //接收透传消息
                    {
                        otherObj = MsgConverter.GetUnvarnishedMsg(mqArray, ref errorMsg);
                        eventType = SdkMsgType.UnvarnishedMsg;
                    }
                        break;
                    case (int)SdkEnumCollection.OtherMsgType.UserIndividuation: //用户个性化设置
                        {
                            otherObj = MsgConverter.GetUserIndividuation(mqArray, ref errorMsg);
                            eventType = SdkMsgType.UserIndividuation;
                        }
                        break;
                    case (int) SdkEnumCollection.NotificationMsgType.UnReadNotifications: //收到群组未读公告消息
                    {
                        otherObj = GetNotificationEntity<MsUnReadNotifications>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.UnReadNotifications;
                    }
                        break;
                    case (int) SdkEnumCollection.NotificationMsgType.AddNotification: //收到新增群组公告消息
                    {
                        otherObj = GetNotificationEntity<MsAddNotification>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.AddNotification;
                    }
                        break;
                    case (int) SdkEnumCollection.NotificationMsgType.DeleteNotification: //收到删除群组公告消息
                    {
                        otherObj = GetNotificationEntity<MsDeleteNotification>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.DeleteNotification;
                    }
                        break;
                    case (int) SdkEnumCollection.NotificationMsgType.ModifyNotificationState: //收到修改群组公告状态已读消息
                    {
                        otherObj = GetNotificationEntity<MsModifyNotificationState>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.ModifyNotificationState;
                    }
                        break;
                    case (int)SdkEnumCollection.OtherMsgType.CheckInVerify://打卡验证
                        otherObj = GetAttendanceRecordEntity<MsAttendanceRecordVerify>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CheckInVerify;
                        break;
                    case (int)SdkEnumCollection.OtherMsgType.CheckInResult://打卡结果
                        otherObj = GetAttendanceRecordEntity<MsAttendanceRecordVerify>(mqArray, ref errorMsg);
                        eventType = SdkMsgType.CheckInResult;
                        break;
                    default:
                    {
                        //自定义消息
                        if (msType >= 4000 && msType <= 9999)
                        {
                            otherObj = GetCustomMsgEntity(mqArray, ref errorMsg);
                            eventType = SdkMsgType.CustomMessage;
                        }
                    }
                        break;
                }
                //返回
                return otherObj;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.ReceiveChatMsgEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：聊天消息:收到消息服务回执(目前只有自己发的聊天消息会收到该回执)
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="array"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static MsReceiveMsgReceipt GetReceiveMsgReceipt(string[][] array, ref string errMsg)
        {
            try
            {
                var entity = new MsReceiveMsgReceipt
                {
                    messageType = array[0][0],
                    messageId = array[1][0],
                    sessionId = array[1][1],
                    chatIndex = array[1][2],
                    sendTime = array[1][3],
                    attr = array[1][4]
                };
                return entity;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetReceiveMsgReceipt]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：获取接收到的组织架构信息
        /// 完成时间：2017-05-15
        /// </summary>
        /// <param name="mqArray">MQ消息信息</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>组织架构信息</returns>
        private static MsOrganizationModify GetOrganizationModify(IReadOnlyList<string[]> mqArray, ref string errorMsg)
        {
            try
            {
                var organization = new MsOrganizationModify
                {
                    messageType = mqArray[0][0],
                    dataVersion = mqArray[1][0],
                    attr = mqArray[1][1]
                };
                //返回
                return organization;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[MsgConverter.GetOrganizationModify][OrganizationModify]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：透传消息
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="array"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static UnvarnishedMsg GetUnvarnishedMsg(string[][] array, ref string errMsg)
        {
            try
            {
                var entity = new UnvarnishedMsg
                {
                    messageType = array[0][0],
                    targetId = array[1][0],
                    content = array[1][1]
                };
                return entity;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetUnvarnishedMsg]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：版本更新接口(硬更新)
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="array"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static MsVersionHardUpdate GetVersionHardUpdate(string[][] array, ref string errorMsg)
        {
            try
            {
                var entity = new MsVersionHardUpdate
                {
                    messageType = array[0][0],
                    version = array[1][0],
                    attr = JsonCoder.DeserializeObject<MsVersionHardUpdate_content>(array[1][1], ref errorMsg)
                };
                return entity;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetVersionHardUpdate]:{e.Message},{e.StackTrace}");
                errorMsg = e.Message;
                return null;
            }
        }

        //SDK收到群组公告通知消息
        /// <summary>
        /// 方法说明：获取群组公告相关实体类
        /// 完成时间：2017-05-12
        /// </summary>
        /// <typeparam name="T">群组公告实体类型</typeparam>
        /// <param name="array">MQTT消息数组</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>公告实体</returns>
        private static T GetNotificationEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkNotificationBase, new()
        {
            var logKey = typeof (T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12]
                };
                var unrnotification = entity as MsUnReadNotifications;
                if (unrnotification != null)
                {
                    unrnotification.content = JsonCoder.DeserializeObject<List<MsNotification_content>>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var addnotification = entity as MsAddNotification;
                if (addnotification != null)
                {
                    addnotification.content = JsonCoder.DeserializeObject<MsNotification_content>(array[1][11],
                        ref errorMsg);
                    return entity;
                }
                var delnotification = entity as MsDeleteNotification;
                if (delnotification != null)
                {
                    delnotification.content = JsonCoder.DeserializeObject<NotificationId>(array[1][11], ref errorMsg);
                    return entity;
                }
                var modnotification = entity as MsModifyNotificationState;
                if (modnotification == null)
                {
                    return entity;
                }
                modnotification.content = JsonCoder.DeserializeObject<NotificationId>(array[1][11], ref errorMsg);
                return entity;
                //返回
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        private static T GetAttendanceRecordEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsAttendanceRecordBase, new()
        {
            var logKey = typeof(T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    sessionId = array[1][0],
                    chatIndex = array[1][1],
                    sourceContent=array[1][2],
                    sendTime = array[1][3],
                };
                var attendancerecord = entity as MsAttendanceRecordVerify;
                if (attendancerecord != null)
                {
                    attendancerecord.content = JsonCoder.DeserializeObject<MsAttendanceRecord_content>(array[1][2],
                        ref errorMsg);
                    return entity;
                }
                return entity;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }
        /// <summary>
        /// 方法说明：获取聊天室信息实体
        /// 完成日期：2017-05-11
        /// </summary>
        /// <typeparam name="T">聊天室消息类型</typeparam>
        /// <param name="array">MQTT消息内容</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室消息实体</returns>
        private static T GetRevocationMsgEntity<T>(IReadOnlyList<string[]> array, ref string errorMsg)
            where T : MsSdkOther.SdkOtherBase, new()
        {
            var logKey = typeof(T).FullName;
            try
            {
                var entity = new T
                {
                    messageType = array[0][0],
                    chatType = int.Parse(array[1][0]),
                    os = int.Parse(array[1][1]),
                    flag = int.Parse(array[1][2]),
                    status = int.Parse(array[1][3]),
                    messageId = array[1][4],
                    appKey = array[1][5],
                    sendUserId = array[1][6],
                    targetId = array[1][7],
                    sessionId = array[1][8],
                    chatIndex = array[1][9],
                    sendTime = array[1][10],
                    attr = array[1][12],
                    sourceContent = array[1][11]
                };
                
                return entity;
                //返回
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetChatMsgEntity][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }



        /// <summary>
        /// 方法说明：用户个性化设置
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="array"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static MsIndividuationSet GetUserIndividuation(string[][] array, ref string errMsg)
        {
            try
            {
                var entity = new MsIndividuationSet
                {
                    messageType = array[0][0],
                    sessionId = array[1][0],
                    chatIndex = array[1][1],
                    sendTime = array[1][3],
                    content = JsonCoder.DeserializeObject<MsIndividuationSet_content>(array[1][2], ref errMsg)
                };
                return entity;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetUserIndividuation]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        #endregion

        #endregion

        #region   //序列化发送的消息（二维数组）

        /// <summary>
        /// 方法说明：发送聊天消息类数据
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">消息实体</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByChatMsg<T>(T entity, ref string errorMsg) where T : MsSdkMessageChat
        {
            var logKey = typeof (T).FullName;
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[13];
                array[1][0] = entity.chatType.ToString();
                array[1][1] = ((int) SdkEnumCollection.OSType.PC).ToString();
                array[1][2] = entity.flag.ToString();
                array[1][3] = entity.status.ToString();
                array[1][4] = entity.messageId;
                array[1][5] = SdkService.SdkSysParam?.Appkey;
                array[1][6] = entity.sendUserId;
                array[1][7] = entity.targetId;
                array[1][8] = entity.sessionId;
                array[1][9] = entity.chatIndex;
                array[1][10] = entity.sendTime;
                array[1][12] = entity.attr;
                //判断消息类型[以MsMessageEntity为基类的消息类型]:文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频、群发消息[文本消息]
                var chatmulsend = entity as MsMultiSendMsg;
                if (chatmulsend != null)
                {
                    array[0][0] = ((int)SdkEnumCollection.ChatMsgType.MultiSendMsg).ToString();
                    array[1][11] = chatmulsend.content;
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgtext = entity as MsChatMsgText;
                if (chatmsgtext != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.Text).ToString();
                    array[1][11] = chatmsgtext.content;
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgpicture = entity as MsChatMsgPicture;
                if (chatmsgpicture != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.Picture).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgpicture.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgaudio = entity as MsChatMsgAudio;
                if (chatmsgaudio != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.Audio).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgaudio.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgvideo = entity as MsChatMsgVideo;
                if (chatmsgvideo != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.Video).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgvideo.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgfile = entity as MsChatMsgFile;
                if (chatmsgfile != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.File).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgfile.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsglocation = entity as MsChatMsgMapLocation;
                if (chatmsglocation != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.MapLocation).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsglocation.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgmiximagetext = entity as MsChatMsgMixMessage;
                if (chatmsgmiximagetext != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.MixImageText).ToString();
                    array[1][11] = chatmsgmiximagetext.content;
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgat = entity as MsChatMsgAt;
                if (chatmsgat != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.At).ToString();
                    array[1][11] = chatmsgat.sourceContent;
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgmultivideo = entity as MsChatMsgMultiAudioVideo;
                if (chatmsgmultivideo != null)
                {
                    array[0][0] = ((int) SdkEnumCollection.ChatMsgType.MultiAudioVideo).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgmultivideo.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgrevocation = entity as MsChatMsgRevocation;
                if (chatmsgrevocation != null)
                {
                    //判断删除消息时messageId不能为空
                    if (!string.IsNullOrEmpty(chatmsgrevocation.content?.messageId))
                    {
                        array[0][0] = ((int) SdkEnumCollection.ChatMsgType.Revocation).ToString();
                        array[1][11] = JsonCoder.SerializeObject(chatmsgrevocation.content);
                        var jsonStr = JsonCoder.SerializeObject(array);
                        return jsonStr;
                    }
                    else
                    {
                        errorMsg = Resources.SdkSendChatMsgRevocationMsIdError;
                        return null;
                    }
                }
                var chatmsgcreatevote = entity as MsChatMsgCreateVote;
                if (chatmsgcreatevote != null)
                {
                    array[0][0] = ((int)SdkEnumCollection.ChatMsgType.CreateVote).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgcreatevote.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgdeletevote = entity as MsChatMsgDeteleVote;
                if (chatmsgdeletevote != null)
                {
                    array[0][0] = ((int)SdkEnumCollection.ChatMsgType.DeleteVote).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgdeletevote.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgcreateactivity = entity as MsChatMsgActivity;
                if (chatmsgcreateactivity != null)
                {
                    array[0][0] = ((int)SdkEnumCollection.ChatMsgType.CreateActivity).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgcreateactivity.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                var chatmsgdeleteactivity = entity as MsChatMsgDeleteActivity;
                if (chatmsgdeleteactivity != null)
                {
                    array[0][0] = ((int)SdkEnumCollection.ChatMsgType.DeleteActivity).ToString();
                    array[1][11] = JsonCoder.SerializeObject(chatmsgdeleteactivity.content);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                //如果不是上述类型则不符合SDK消息发送
                errorMsg += $"{Resources.SdkSendChatMsgTypeError}:[{logKey}]";
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByChatMsg][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送自定义消息类数据[自己完成Sdk消息基类（MsSdkMessageEntity）的继承并且定义自己的content内容]
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">消息实体</param>
        /// <param name="custommsgType">自定义消息类型[4000-9999]</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByCustomMsg(MsSdkCustomEntity entity, string custommsgType, ref string errorMsg)
        {
            try
            {
                if (entity == null)
                {
                    return null;
                }
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[13];
                array[0][0] = custommsgType;
                array[1][0] = entity.chatType.ToString();
                array[1][1] = ((int) SdkEnumCollection.OSType.PC).ToString();
                array[1][2] = entity.flag.ToString();
                array[1][3] = entity.status.ToString();
                array[1][4] = entity.messageId;
                array[1][5] = SdkService.SdkSysParam?.Appkey;
                array[1][6] = entity.sendUserId;
                array[1][7] = entity.targetId;
                array[1][8] = entity.sessionId;
                array[1][9] = entity.chatIndex;
                array[1][10] = entity.sendTime;
                array[1][12] = entity.attr;


                if (entity.content == null)
                {
                    array[1][11] = string.Empty;
                }
                else if (entity.content is string)
                {
                    array[1][11] = entity.content.ToString();
                }
                else
                {
                    array[1][11] = JsonCoder.SerializeObject(entity.content);
                }
                //处理结果
                var jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByChatMsg][MsCustomEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送聊天消息类数据
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">消息实体</param>
        /// <param name="isheartBeat">是否心跳</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByTerminalMsg<T>(T entity, ref bool isheartBeat, ref string errorMsg) where T : SdkMsTerminalBase
        {
            var logKey = typeof (T).FullName;
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[4];
                array[1][0] = SdkService.SdkSysParam?.Appkey;
                array[1][1] = entity.userId;
                array[1][2] = ((int) SdkEnumCollection.OSType.PC).ToString();
                //判断消息类型[以MsTerminalEntity为基类的消息类型]:心跳消息、请求离线消息
                var heartbeatmsg = entity as SdkMsHeartBeat;
                if (heartbeatmsg != null)
                {
                    array[0][0] =
                        SdkEnumCollection.TerminalInfoTypeArray[(int) SdkEnumCollection.TerminalInfoType.HeartInfo];
                    array[1][3] = heartbeatmsg.attr;
                    var jsonStr = JsonCoder.SerializeObject(array);
                    isheartBeat = true;
                    return jsonStr;
                }
                var questoffline = entity as SdkMsQuestOffLine;
                if (questoffline != null)
                {
                    array[0][0] =
                        SdkEnumCollection.TerminalInfoTypeArray[
                            (int) SdkEnumCollection.TerminalInfoType.QuestOffLineInfo];
                    array[1][3] = JsonCoder.SerializeObject(questoffline.attr);
                    var jsonStr = JsonCoder.SerializeObject(array);
                    return jsonStr;
                }
                //如果不是上述类型则不符合SDK消息发送
                errorMsg += $"{Resources.SdkSendTerminalMsgTypeError}:[{logKey}]";
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByTerminalMsg][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送消息已读/已收回执
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string GetJsonByMsgReceipt(SdkMsSendMsgReceipt entity, ref string errMsg)
        {
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[5];
                array[0][0] = entity.messageType;
                array[1][0] = entity.os.ToString();
                array[1][1] = entity.appKey;
                array[1][2] = entity.userId;
                array[1][3] = entity.sessionId;
                array[1][4] = entity.chatIndex;
                var jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByChatMsgReceipt]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送聊天消息类数据
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">接收到的消息实体</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByPointReadedReceipt(MsPointBurnReaded entity, ref string errorMsg)
        {
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[13];
                array[0][0] = entity.messageType;
                array[1][0] = entity.chatType.ToString();
                array[1][1] = ((int) SdkEnumCollection.OSType.PC).ToString();
                array[1][2] = entity.flag.ToString();
                array[1][3] = entity.status.ToString();
                array[1][4] = entity.messageId;
                array[1][5] = SdkService.SdkSysParam?.Appkey;
                array[1][6] = entity.sendUserId;
                array[1][7] = entity.targetId;
                array[1][8] = entity.sessionId;
                array[1][9] = entity.chatIndex;
                array[1][10] = entity.sendTime;
                array[1][12] = entity.attr;
                //判断消息类型[以MsMessageEntity为基类的点对点阅后即焚消息类型]:文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频
                array[1][11] = JsonCoder.SerializeObject(entity.content);
                var jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[MsgConverter.GetJsonByPointReadedReceipt][MsSdkMessageEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送群主切换阅后即焚、普通消息、阅后即焚下删除消息
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">接收到的消息实体</param>
        /// <param name="changeMode">切换模式</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByGroupChangeMode(MsSdkMessageChat entity, GroupChangeMode changeMode,
            ref string errorMsg)
        {
            try
            {
                string messageType;
                switch (changeMode)
                {
                    case GroupChangeMode.BurnMode:
                        messageType = "2591";
                        break;
                    case GroupChangeMode.BurnModeDelete:
                        messageType = "2592";
                        break;
                    case GroupChangeMode.NormalMode:
                        messageType = "2593";
                        break;
                    default:
                        messageType = "2593";
                        break;
                }
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[13];
                array[0][0] = messageType;
                array[1][0] = entity.chatType.ToString();
                array[1][1] = ((int) SdkEnumCollection.OSType.PC).ToString();
                array[1][2] = entity.flag.ToString();
                array[1][3] = entity.status.ToString();
                array[1][4] = entity.messageId;
                array[1][5] = SdkService.SdkSysParam?.Appkey;
                array[1][6] = entity.sendUserId;
                array[1][7] = entity.targetId;
                array[1][8] = entity.sessionId;
                array[1][9] = entity.chatIndex;
                array[1][10] = entity.sendTime;
                array[1][12] = entity.attr;
                array[1][11] = string.Empty;
                var jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[MsgConverter.GetJsonByPointReadedReceipt][MsSdkMessageEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送邀请加入会话(邀请加入、同意加入、拒绝加入)
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string GetJsonByInviteJoinSession(InviteJoinSession entity, ref string errMsg)
        {
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[5];
                array[0][0] = entity.messageType;
                array[1][0] = entity.sendUserId;
                array[1][1] = entity.targetId;
                array[1][2] = entity.appKey;
                array[1][3] = entity.sessionId;
                array[1][4] = JsonCoder.SerializeObject(entity.content);
                string jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByInviteJoinSession]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送透传消息
        /// 作    者：赵雪峰
        /// 创建时间：2016/6/20
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string GetJsonByUnvarnishedMsg(UnvarnishedMsg entity, ref string errMsg)
        {
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[2];
                array[0][0] = entity.messageType;
                array[1][0] = entity.targetId;
                array[1][1] = entity.content;
                string jsonStr = JsonCoder.SerializeObject(array);
                return jsonStr;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByUnvarnishedMsg]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 方法说明：发送聊天消息类数据
        /// 创建时间：2017-05-15
        /// </summary>
        /// <param name="entity">消息实体</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天消息类Json串</returns>
        public static string GetJsonByOtherMsg<T>(T entity, ref string errorMsg) where T : MsSdkOther.SdkOtherBase
        {
            var logKey = typeof(T).FullName;
            try
            {
                var array = new string[2][];
                array[0] = new string[1];
                array[1] = new string[13];
                array[1][0] = entity.chatType.ToString();
                array[1][1] = ((int)SdkEnumCollection.OSType.PC).ToString();
                array[1][2] = entity.flag.ToString();
                array[1][3] = entity.status.ToString();
                array[1][4] = entity.messageId;
                array[1][5] = SdkService.SdkSysParam?.Appkey;
                array[1][6] = entity.sendUserId;
                array[1][7] = entity.targetId;
                array[1][8] = entity.sessionId;
                array[1][9] = entity.chatIndex;
                array[1][10] = entity.sendTime;
                array[1][12] = entity.attr;
                //存在其他消息类型的处理发送
                //如果不是上述类型则不符合SDK消息发送
                errorMsg += $"{Resources.SdkSendChatMsgTypeError}:[{logKey}]";
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[MsgConverter.GetJsonByOtherMsg][{logKey}]:{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return null;
            }
        }

        #endregion
    }
}
