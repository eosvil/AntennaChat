/*
 * 接收/发送消息、接收到群组公告信息文件：    
 * 消息内容可为：文本[string];
 *               图片[Picture];
 *               音频[Audio];
 *               视频[Video];
 *               文件[File];
 *               地理位置[MapLocation];
 *               图文混合[MixImageText[]];
 *               @消息[At[]];
 *               多人视频消息开启[MultiAudioVideo]
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK聊天消息信息类
    /// </summary>
    public class AntSdkChatMsg
    {
        /// <summary>
        /// 触角SDK聊天消息基类
        /// </summary>
        public class ChatBase : AntSdkMsBase
        {
            /// <summary>
            /// 消息类型
            /// </summary>
            public string mtp { set; get; }
            /// <summary>
            /// 聊天类型 聊天类型 1：点对点聊天，2：聊天室 4：群（讨论组）
            /// </summary>
            public int chatType { get; set; }

            /// <summary>
            /// 消息发送者的操作系统 1：PC，2：WEB，3：ANDROID，4：iOS
            /// </summary>
            public int os { get; set; } = (int)SdkEnumCollection.OSType.PC;

            /// <summary>
            /// 阅后即焚的标识 1：阅后即焚，0：普通消息
            /// </summary>
            public int flag { get; set; }

            /// <summary>
            /// 消息状态 1：已读，0：未读
            /// </summary>
            public int status { get; set; }

            /// <summary>
            /// 消息ID 
            /// </summary>
            public string messageId { get; set; } = string.Empty;

            /// <summary>
            /// 消息发送者ID
            /// </summary>
            public string sendUserId { get; set; } = string.Empty;

            /// <summary>
            /// 消息接收者ID
            /// </summary>
            public string targetId { get; set; } = string.Empty;

            /// <summary>
            /// 消息发送时间
            /// </summary>
            public string sendTime { get; set; } = string.Empty;

            /// <summary>
            /// 定义消息字段 可以是jsonString
            /// </summary>
            public string attr { get; set; } = string.Empty;

            /// <summary>
            /// 消息的原始Js内容，用来存储或者转储后的解析处理
            /// </summary>
            public string sourceContent { get; set; } = string.Empty;

            /// <summary>
            /// 发送是否成功表示 1成功，0失败，接收到的消息默认是1
            /// </summary>
            public string SENDORRECEIVE { get; set; } = string.Empty;

            /// <summary>
            /// 发送状态 1表示发送成功 0表示发送失败
            /// </summary>
            public int sendsucessorfail { get; set; }

            public string uploadOrDownPath { get; set; } = string.Empty;

            public int typeBurnMsg { get; set; }

            public string readtime { get; set; } = string.Empty;
            public string voiceread { set; get; }
            /// <summary>
            /// 是否来源第一屏显示
            /// </summary>
            public bool isFirstPageShow { set; get; }
            /// <summary>
            /// 机器人标识
            /// </summary>
            public bool IsRobot { set; get; }

            /// <summary>
            /// 投票或活动标识
            /// </summary>
            public string VoteOrActivityID { get; set; }
            /// <summary>
            /// 判断是否加载完成图片之后滚动条置底
            /// </summary>
            public bool IsSetImgLoadComplete { set; get; }

            /// <summary>
            /// 根据当前消息信息传递值给目标消息信息
            /// </summary>
            /// <param name="entity">需要进行赋值的实体信息</param>
            internal T SetAntSdkValues<T>(T entity) where T : ChatBase
            {
                entity.MsgType = MsgType;
                entity.chatIndex = chatIndex;
                entity.sourceContent = sourceContent;
                entity.messageId = messageId;
                entity.sendTime = sendTime;
                entity.sendUserId = sendUserId;
                entity.sessionId = sessionId;
                entity.targetId = targetId;
                entity.SENDORRECEIVE = SENDORRECEIVE;
                entity.sendsucessorfail = sendsucessorfail;
                entity.flag = flag;
                entity.readtime = readtime;
                entity.uploadOrDownPath = uploadOrDownPath;
                entity.voiceread = voiceread;
                return entity;
            }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal T GetSdkSendBase<T>() where T : MsSdkMessageChat, new()
            {
                var antsdkreceivemsgtypeValue = (long)MsgType;
                var sdkreceivemsgType = (SdkMsgType)antsdkreceivemsgtypeValue;
                var result = new T
                {
                    MsgType = sdkreceivemsgType,
                    sessionId = sessionId,
                    chatIndex = chatIndex,
                    chatType = chatType,
                    flag = flag,
                    status = status,
                    messageId = messageId,
                    sendUserId = sendUserId,
                    targetId = targetId,
                    sendTime = sendTime,
                    sourceContent = sourceContent,
                    sendsucessorfail = sendsucessorfail,
                    attr = attr
                };
                return result;
            }
        }

        /// <summary>
        /// 群发消息[目前只支持文本]
        /// </summary>
        public class MultiSend : ChatBase
        {
            public MultiSend()
            {
                MsgType = AntSdkMsgType.MultiUserSend;
            }

            /// <summary>
            /// 群发内容[当前只支持闻榜]
            /// </summary>
            public string content { get; set; } = string.Empty;

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsMultiSendMsg GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsMultiSendMsg>();
                result.content = content;
                return result;
            }
        }

        public class Text : ChatBase
        {
            public Text()
            {
                MsgType = AntSdkMsgType.ChatMsgText;
            }

            /// <summary>
            /// 文本消息内容
            /// </summary>
            public string content { get; set; } = string.Empty;

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgText GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgText>();
                result.content = content;
                return result;
            }
        }

        /// <summary>
        /// 图片消息：对于图片消息，messageType为1002，[1][11]中对应的内容
        /// </summary>
        public class Picture : ChatBase
        {
            public Picture()
            {
                MsgType = AntSdkMsgType.ChatMsgPicture;
            }

            public Picture_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgPicture GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgPicture>();
                result.content = new MsChatMsgPicture_content
                {
                    picUrl = content.picUrl
                };
                return result;
            }
        }

        public class Picture_content
        {
            /// <summary>
            /// 原始图url
            /// </summary>
            public string picUrl { get; set; } = string.Empty;
        }

        /// <summary>
        /// 音频消息：对于音频消息，messageType为1003，[1][11]中对应的内容
        /// </summary>
        public class Audio : ChatBase
        {
            public Audio()
            {
                MsgType = AntSdkMsgType.ChatMsgAudio;
            }

            public Audio_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgAudio GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgAudio>();
                result.content = new MsChatMsgAudio_content
                {
                    audioUrl = content.audioUrl,
                    duration = content.duration
                };
                return result;
            }
        }

        public class Audio_content
        {
            /// <summary>
            /// 语音的url
            /// </summary>
            public string audioUrl { get; set; } = string.Empty;

            /// <summary>
            /// 语音的时长(单位为秒)
            /// </summary>
            public int duration { get; set; }
        }

        /// <summary>
        /// 视频消息格式（消息格式后面再定义）
        /// </summary>
        public class Video : ChatBase
        {
            public Video()
            {
                MsgType = AntSdkMsgType.ChatMsgVideo;
            }

            public Video_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgVideo GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgVideo>();
                result.content = new MsChatMsgVideo_content
                { };
                return result;
            }
        }

        public class Video_content
        {

        }

        /// <summary>
        /// 文件消息：对于文件消息，messageType为1005，[1][11]中对应的内容
        /// </summary>
        public class File : ChatBase
        {

            public File()
            {
                MsgType = AntSdkMsgType.ChatMsgFile;
            }

            public File_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgFile GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgFile>();
                result.content = new MsChatMsgFile_content
                {
                    fileName = content.fileName,
                    fileUrl = content.fileUrl,
                    size = content.size
                };
                return result;
            }
        }

        public class File_content
        {
            /// <summary>
            /// 文件的名称
            /// </summary>
            public string fileName { get; set; } = string.Empty;

            /// <summary>
            /// 文件的地址
            /// </summary>
            public string fileUrl { get; set; } = string.Empty;

            /// <summary>
            /// 文件大小(单位为B)
            /// </summary>
            public string size { get; set; } = string.Empty;
        }

        /// <summary>
        /// 地理位置消息格式：对于地理位置消息，messageType为1006，[1][11]中对应的内容
        /// </summary>
        public class MapLocation : ChatBase
        {

            public MapLocation()
            {
                MsgType = AntSdkMsgType.ChatMsgMapLocation;
            }


            public MapLocation_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgMapLocation GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgMapLocation>();
                result.content = new MsChatMsgMapLocation_content
                {
                    title = content.title,
                    lng = content.lng,
                    lat = content.lat
                };
                return result;
            }
        }

        public class MapLocation_content
        {
            /// <summary>
            /// 地理位置title
            /// </summary>
            public string title { get; set; } = string.Empty;

            /// <summary>
            /// 经度
            /// </summary>
            public string lng { get; set; } = string.Empty;

            /// <summary>
            /// 维度
            /// </summary>
            public string lat { get; set; } = string.Empty;
        }

        /// <summary>
        /// 混合消息(数组形式)：对于图文混合消息，messageType为1007，[1][11]中对应的内容,混合消息特殊处理[content混合类型]
        /// </summary>
        public class MixMessage : ChatBase
        {
            public MixMessage()
            {
                MsgType = AntSdkMsgType.ChatMsgMixMessage;
            }

            public List<MixMessage_content> content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgMixMessage GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgMixMessage>();
                var errMsg = string.Empty;
                var mixsJs = string.Empty;
                JsonCoder.SerializeJson(content, true, ref mixsJs, ref errMsg);
                result.content = mixsJs;
                return result;
            }
        }

        public class MixMessage_content
        {
            /// <summary>
            /// 1001：文本(包含表情)；1002：图片；0000：换行符
            /// </summary>
            public string type { get; set; } = string.Empty;

            /// <summary>
            /// type-1001：文本消息内容；jsonString(图片消息是对象)[{\"picUrl\":\"http://www.baidu.com\"}]
            /// </summary>
            public object content { get; set; }
        }

        /// <summary>
        /// @消息(数组形式)：对于 @消息 中对应的内容如下
        /// </summary>
        public class At : ChatBase
        {
            public List<At_content> content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgAt GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgAt>();
                var atlist = new List<MsChatMsgAt_content>();
                if (content?.Count > 0)
                {
                    atlist.AddRange(content.Select(c => new MsChatMsgAt_content
                    {
                        type = c.type,
                        content = c.content,
                        ids = c.ids,
                        names = c.names
                    }));
                }
                result.content = atlist;
                return result;
            }
        }

        public class At_content
        {
            /// <summary>
            /// 1001：文本(包含表情)；1002：图片；.......0000：换行符
            /// </summary>
            public string type { get; set; } = string.Empty;

            /// <summary>
            /// type-1001：文本消息内容；jsonString(图片消息是对象)[{\"picUrl\":\"http://www.baidu.com\"}].........[jsonString]
            /// </summary>
            public string content { get; set; } = string.Empty;

            /// <summary>
            /// 被@人的ID(数组)
            /// </summary>
            public List<string> ids { get; set; }

            /// <summary>
            /// 被@人的名字(数组)
            /// </summary>
            public List<string> names { get; set; }
        }


        public class BaseDtoContent
        {
            public string type { set; get; } = string.Empty;
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        public class contentText : BaseDtoContent
        {
            public string content { set; get; } = string.Empty;
        }

        /// <summary>
        /// @All 
        /// </summary>
        public class contentAtAll : BaseDtoContent
        {

        }

        /// <summary>
        /// @普通消息
        /// </summary>
        public class contentAtOrdinary : BaseDtoContent
        {
            public List<string> ids { set; get; }
            public List<string> names { set; get; }
        }

        /// <summary>
        /// 换行
        /// </summary>
        public class contentNewLine : BaseDtoContent
        {

        }

        /// <summary>
        /// 多人音频视频开启消息：对于多人音频视频消息，包括开启/结束多人音视频，messageType为1009，[1][11]中对应的内容
        /// </summary>
        public class MultiAudioVideo : ChatBase
        {
            public MultiAudioVideo()
            {
                MsgType = AntSdkMsgType.ChatMsgMultiAudioVideo;
            }

            public MultiAudioVideo_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgMultiAudioVideo GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgMultiAudioVideo>();
                result.content = new MsChatMsgMultiAudioVideo_content
                {
                    roomStatus = content.roomStatus,
                    roomId = content.roomId,
                    roomName = content.roomName,
                    creatorId = content.creatorId,
                    callType = content.callType,
                    duration = content.duration
                };
                return result;
            }
        }

        public class MultiAudioVideo_content
        {
            /// <summary>
            /// 1--开启多人音视频；0--结束多人音视频
            /// </summary>
            public int roomStatus { get; set; }

            /// <summary>
            /// 聊天室ID
            /// </summary>
            public string roomId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室名称
            /// </summary>
            public string roomName { get; set; } = string.Empty;

            /// <summary>
            /// 创建者ID
            /// </summary>
            public string creatorId { get; set; } = string.Empty;

            /// <summary>
            /// 1--音频通话，2--视频通话
            /// </summary>
            public string callType { get; set; } = string.Empty;

            /// <summary>
            /// 多人视频结束：表示多人音频视频持续的时间为1个小时24分10秒    
            /// </summary>
            public string duration { get; set; } = string.Empty;
        }

        /// <summary>
        /// 单人（点对点）音频视频消息:1013
        /// </summary>
        public class PointAudioVideo : ChatBase
        {
            public PointAudioVideo()
            {
                MsgType = AntSdkMsgType.PointAudioVideo;
            }

            /// <summary>
            /// 单人（点对点）音频视频消息内容
            /// </summary>
            public PointAudioVideo_content content { get; set; }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgPointAudioVideo GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgPointAudioVideo>();
                result.content = new MsChatMsgPointAudioVideo_content
                {
                    roomStatus = content.roomStatus,
                    callType = content.callType,
                    duration = content.duration
                };
                return result;
            }
        }

        /// <summary>
        /// 单人（点对点）音频视频消息内容
        /// </summary>
        public class PointAudioVideo_content
        {
            /// <summary>
            /// 1--开启单人音视频,0--结束单人音视频
            /// </summary>
            public int roomStatus { get; set; }

            /// <summary>
            /// 1--音频通话，2--视频通话
            /// </summary>
            public int callType { get; set; }

            /// <summary>
            ///结束单人音视频:表示音频视频持续的时间为1个小时24分10秒
            /// </summary>
            public string duration { get; set; } = string.Empty;
            public string bodyType { get; set; }
            public string eventType { get; set; }
            public string text { get; set; }
        }

        /// <summary>
        /// 消息撤回：1014
        /// </summary>
        public class Revocation : ChatBase
        {

            public Revocation()
            {
                MsgType = AntSdkMsgType.Revocation;
            }

            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgRevocation GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgRevocation>();
                result.content = new MsChatMsgRevocation_content
                {
                    messageId = content?.messageId
                };
                return result;
            }

            /// <summary>
            /// 消息撤回内容
            /// </summary>
            public Revocation_content content { get; set; }

        }

        /// <summary>
        /// 撤销消息内容
        /// </summary>
        public class Revocation_content
        {
            /// <summary>
            /// 要撤回的消息ID
            /// </summary>
            public string messageId { get; set; } = string.Empty;
        }
        /// <summary>
        /// 创建投票 
        /// </summary>
        public class CreateVoteMsg : ChatBase
        {
            /// <summary>
            /// 发送消息时获取SDK消息用于传递
            /// </summary>
            /// <returns></returns>
            internal MsChatMsgCreateVote GetSdkSend()
            {
                var result = base.GetSdkSendBase<MsChatMsgCreateVote>();
                if (content == null) return result;
                result.content = new MsChatMsgCreateVote_content
                {
                    id = content.id,
                    createdBy = content.createdBy,
                    createdDate = content.createdDate,
                    expiryTime = content.expiryTime,
                    maxChoiceNumber = content.maxChoiceNumber,
                    secret = content.secret,
                    title = content.title
                };
                result.content.options.AddRange(content.options.Where(c => c != null).Select(c => new MsChatMsgVoteOption
                {
                    name = c.name
                }));
                return result;
            }
            /// <summary>
            /// 创建投票消息内容
            /// </summary>
            public CreateVote_content content { get; set; }
        }
        /// <summary>
        /// 创建投票消息内容
        /// </summary>
        public class CreateVote_content
        {
            /// <summary>
            /// 投票标识
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// 标题
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// 选项数组
            /// </summary>
            public List<VoteOptionMsg> options { get; set; } = new List<VoteOptionMsg>();

            /// <summary>
            /// 最大选择数
            /// </summary>
            public int maxChoiceNumber { get; set; }

            /// <summary>
            /// 截止时间，格式yyyy-MM-dd HH: mm:ss
            /// </summary>
            public string expiryTime { get; set; }

            /// <summary>
            /// 是否匿名
            /// </summary>
            public bool secret { get; set; }

            /// <summary>
            /// 创建者用户标识
            /// </summary>
            public string createdBy { get; set; }

            /// <summary>
            /// 创建时间，格式yyyy-MM-dd HH: mm:ss
            /// </summary>
            public string createdDate { get; set; }
        }

        /// <summary>
        /// 删除投票
        /// </summary>
        public class DeteleVoteMsg : ChatBase
        {
            /// <summary>
            /// 删除投票消息内容
            /// </summary>
            public DeteleVote_content content { get; set; }
        }
        /// <summary>
        /// 创建或删除投票消息内容
        /// </summary>
        public class DeteleVote_content
        {
            /// <summary>
            /// 投票标识
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// 标题
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// 选项数组
            /// </summary>
            public List<VoteOptionMsg> options { get; set; } = new List<VoteOptionMsg>();

            /// <summary>
            /// 最大选择数
            /// </summary>
            public int maxChoiceNumber { get; set; }

            /// <summary>
            /// 截止时间，格式yyyy-MM-dd HH: mm:ss
            /// </summary>
            public string expiryTime { get; set; }

            /// <summary>
            /// 是否匿名
            /// </summary>
            public bool secret { get; set; }

            /// <summary>
            /// 创建者用户标识
            /// </summary>
            public string createdBy { get; set; }

            /// <summary>
            /// 创建时间，格式yyyy-MM-dd HH: mm:ss
            /// </summary>
            public string createdDate { get; set; }
        }

        public class ActivityMsg : ChatBase
        {
            /// <summary>
            /// 活动消息内容
            /// </summary>
            public Activity_content content { get; set; }
        }

        public class Activity_content
        {
            /// <summary>
            /// 活动标识
            /// </summary>
            public int activityId { get; set; }
            /// <summary>
            /// 群标识
            /// </summary>
            public string groupId { get; set; }
            /// <summary>
            /// 创建者ID
            /// </summary>
            public string userId { get; set; }
            /// <summary>
            /// 活动主题
            /// </summary>
            public string theme { get; set; }
            /// <summary>
            /// 活动主题图片
            /// </summary>
            public string picture { get; set; }
            /// <summary>
            /// 活动地址
            /// </summary>
            public string address { get; set; }
            /// <summary>
            /// 地图纬度
            /// </summary>
            public float longitude { get; set; }
            /// <summary>
            /// 地图经度
            /// </summary>
            public float latitude { get; set; }
            /// <summary>
            /// 活动开始时间
            /// </summary>
            public string startTime { get; set; }
            /// <summary>
            /// 活动结束时间
            /// </summary>
            public string endTime { get; set; }
            /// <summary>
            /// 活动提示时间:分钟
            /// </summary>
            public int remindTime { get; set; }
            /// <summary>
            /// 活动介绍
            /// </summary>
            public string description { get; set; }
            /// <summary>
            /// 进行中（1进行中 2已结束）
            /// </summary>
            public int activityStatus { get; set; }
            /// <summary>
            /// 活动创建时间
            /// </summary>
            public string createTime { get; set; }
            /// <summary>
            /// 是否已参与标识
            /// </summary>
            public bool voteFlag { get; set; }
        }

        /// <summary>
        /// 活动删除消息对象
        /// </summary>
        public class DeleteActivityMsg : ChatBase
        {
            public DeleteActivity_content content { get; set; }
        }
        /// <summary>
        /// 活动删除消息内容对象
        /// </summary>
        public class DeleteActivity_content
        {
            /// <summary>
            /// 活动标识
            /// </summary>
            public int activityId { get; set; }
            /// <summary>
            /// 群标识
            /// </summary>
            public string groupId { get; set; }
        }
        /// <summary>
        /// 投票选项
        /// </summary>
        public class VoteOptionMsg
        {
            public string name { get; set; }
        }
        /// <summary>
        /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static T GetAntSdkReceivedChatBase<T>(MsSdkMessageChat entity) where T : ChatBase, new()
        {
            var sdkreceivemsgtypeValue = (long)entity.MsgType;
            var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
            var result = new T
            {
                MsgType = antsdkreceivemsgType,
                sessionId = entity.sessionId,
                chatIndex = entity.chatIndex,
                chatType = entity.chatType,
                flag = entity.flag,
                os = entity.os,
                status = entity.status,
                messageId = entity.messageId,
                sendUserId = entity.sendUserId,
                targetId = entity.targetId,
                sendTime = entity.sendTime,
                attr = entity.attr,
                sourceContent = entity.sourceContent,
                sendsucessorfail = entity.sendsucessorfail
            };
            return result;
        }

        /// <summary>
        /// 方法说明：获取接收到的平台SDK聊天消息，转化为触角SDK聊天消息
        /// </summary>
        /// <param name="entity">SDK聊天信息</param>
        /// <returns>触角SDK聊天信息</returns>
        internal static ChatBase GetAntSdkReceivedChat(MsSdkMessageChat entity)
        {
            try
            {
                var sdkmlsyObj = entity as MsMultiSendMsg;
                if (sdkmlsyObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Text>(sdkmlsyObj);
                    antsdkchatMsg.content = sdkmlsyObj.content;
                    return antsdkchatMsg;
                }
                var sdktextObj = entity as MsChatMsgText;
                if (sdktextObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Text>(sdktextObj);
                    antsdkchatMsg.content = sdktextObj.content;
                    return antsdkchatMsg;
                }
                var sdkpictureObj = entity as MsChatMsgPicture;
                if (sdkpictureObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Picture>(sdkpictureObj);
                    antsdkchatMsg.content = new Picture_content
                    {
                        picUrl = sdkpictureObj.content?.picUrl
                    };
                    return antsdkchatMsg;
                }
                var sdkaudioObj = entity as MsChatMsgAudio;
                if (sdkaudioObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Audio>(sdkaudioObj);
                    antsdkchatMsg.content = new Audio_content
                    {
                        audioUrl = sdkaudioObj.content?.audioUrl,
                        duration = sdkaudioObj.content?.duration ?? 0
                    };
                    return antsdkchatMsg;
                }
                var sdkvideoObj = entity as MsChatMsgVideo;
                if (sdkvideoObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Video>(sdkvideoObj);
                    antsdkchatMsg.content = new Video_content();
                    return antsdkchatMsg;
                }
                var sdkfileObj = entity as MsChatMsgFile;
                if (sdkfileObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<File>(sdkfileObj);
                    antsdkchatMsg.content = new File_content
                    {
                        fileName = sdkfileObj.content?.fileName,
                        fileUrl = sdkfileObj.content?.fileUrl,
                        size = sdkfileObj.content?.size
                    };
                    return antsdkchatMsg;
                }
                var sdkmaplocObj = entity as MsChatMsgMapLocation;
                if (sdkmaplocObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<MapLocation>(sdkmaplocObj);
                    antsdkchatMsg.content = new MapLocation_content
                    {
                        title = sdkmaplocObj.content?.title,
                        lat = sdkmaplocObj.content?.lat,
                        lng = sdkmaplocObj.content?.lng
                    };
                    return antsdkchatMsg;
                }
                var sdkmixamageObj = entity as MsChatMsgMixMessage;
                if (sdkmixamageObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<MixMessage>(sdkmixamageObj);
                    if (antsdkchatMsg?.content != null)
                    {
                        DealWithMixMsg(antsdkchatMsg.content.ToString(), ref antsdkchatMsg);
                    }
                    return antsdkchatMsg;
                }
                var sdkatObj = entity as MsChatMsgAt;
                if (sdkatObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<At>(sdkatObj);
                    var antsdkatCotet = new List<At_content>();
                    if (sdkatObj.content?.Count > 0)
                    {
                        antsdkatCotet.AddRange(sdkatObj.content.Select(at => new At_content
                        {
                            content = at.content,
                            type = at.type,
                            ids = at.ids,
                            names = at.names
                        }));
                    }
                    antsdkchatMsg.content = antsdkatCotet;
                    return antsdkchatMsg;
                }
                var sdkmultiaudiovideoObj = entity as MsChatMsgMultiAudioVideo;
                if (sdkmultiaudiovideoObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<MultiAudioVideo>(sdkmultiaudiovideoObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new MultiAudioVideo_content
                        {
                            roomStatus = sdkmultiaudiovideoObj.content?.roomStatus ?? 0,
                            roomId = sdkmultiaudiovideoObj.content?.roomId,
                            roomName = sdkmultiaudiovideoObj.content?.roomName,
                            creatorId = sdkmultiaudiovideoObj.content?.creatorId,
                            callType = sdkmultiaudiovideoObj.content?.callType,
                            duration = sdkmultiaudiovideoObj.content?.duration,
                        };
                    }
                    //返回暂时注释多人音视频消息处理
                    //return antsdkchatMsg;
                }
                var sdkpointaudiovideoObj = entity as MsChatMsgPointAudioVideo;
                if (sdkpointaudiovideoObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<PointAudioVideo>(sdkpointaudiovideoObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new PointAudioVideo_content
                        {
                            roomStatus = sdkpointaudiovideoObj.content?.roomStatus ?? 0,
                            callType = sdkpointaudiovideoObj.content?.callType ?? 0,
                            duration = sdkpointaudiovideoObj.content?.duration
                        };
                    }
                    //暂时注释音视频消息处理
                    return antsdkchatMsg;
                }
                var sdkmsgrevocationObj = entity as MsChatMsgRevocation;
                if (sdkmsgrevocationObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<Revocation>(sdkmsgrevocationObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new Revocation_content
                        {
                            messageId = sdkmsgrevocationObj.content.messageId
                        };
                    }
                    return antsdkchatMsg;
                }
                var sdkmsgcreatevoteObj = entity as MsChatMsgCreateVote;
                if (sdkmsgcreatevoteObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<CreateVoteMsg>(sdkmsgcreatevoteObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new CreateVote_content
                        {
                            id = sdkmsgcreatevoteObj.content.id,
                            createdBy = sdkmsgcreatevoteObj.content.createdBy,
                            createdDate = sdkmsgcreatevoteObj.content.createdDate,
                            expiryTime = sdkmsgcreatevoteObj.content.expiryTime,
                            maxChoiceNumber = sdkmsgcreatevoteObj.content.maxChoiceNumber,
                            secret = sdkmsgcreatevoteObj.content.secret,
                            title = sdkmsgcreatevoteObj.content.title
                        };
                        antsdkchatMsg.content.options.AddRange(sdkmsgcreatevoteObj.content.options.Where(c => c != null).Select(c => new VoteOptionMsg
                        {
                            name = c.name
                        }));
                    }
                    return antsdkchatMsg;
                }
                var sdkmsgdetelevoteObj = entity as MsChatMsgDeteleVote;
                if (sdkmsgdetelevoteObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<DeteleVoteMsg>(sdkmsgdetelevoteObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new DeteleVote_content
                        {
                            id = sdkmsgdetelevoteObj.content.id,
                            createdBy = sdkmsgdetelevoteObj.content.createdBy,
                            createdDate = sdkmsgdetelevoteObj.content.createdDate,
                            expiryTime = sdkmsgdetelevoteObj.content.expiryTime,
                            maxChoiceNumber = sdkmsgdetelevoteObj.content.maxChoiceNumber,
                            secret = sdkmsgdetelevoteObj.content.secret,
                            title = sdkmsgdetelevoteObj.content.title
                        };
                        antsdkchatMsg.content.options.AddRange(sdkmsgdetelevoteObj.content.options.Where(c => c != null).Select(c => new VoteOptionMsg
                        {
                            name = c.name
                        }));
                    }
                    return antsdkchatMsg;
                }
                var sdkmsgactivityObj = entity as MsChatMsgActivity;
                if (sdkmsgactivityObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<ActivityMsg>(sdkmsgactivityObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new Activity_content
                        {
                            activityId = sdkmsgactivityObj.content.activityId,
                            userId = sdkmsgactivityObj.content.userId,
                            groupId = sdkmsgactivityObj.content.groupId,
                            theme = sdkmsgactivityObj.content.theme,
                            picture = sdkmsgactivityObj.content.picture,
                            address = sdkmsgactivityObj.content.address,
                            latitude = sdkmsgactivityObj.content.latitude,
                            longitude = sdkmsgactivityObj.content.longitude,
                            startTime = sdkmsgactivityObj.content.startTime,
                            endTime = sdkmsgactivityObj.content.endTime,
                            remindTime = sdkmsgactivityObj.content.remindTime,
                            description = sdkmsgactivityObj.content.description,
                            activityStatus = sdkmsgactivityObj.content.activityStatus,
                            createTime = sdkmsgactivityObj.content.createTime,
                            voteFlag = sdkmsgactivityObj.content.voteFlag
                        };
                       
                    }
                    return antsdkchatMsg;
                }
                var sdkmsgdeleteactivityObj = entity as MsChatMsgDeleteActivity;
                if (sdkmsgdeleteactivityObj != null)
                {
                    var antsdkchatMsg = GetAntSdkReceivedChatBase<DeleteActivityMsg>(sdkmsgdeleteactivityObj);
                    if (antsdkchatMsg != null)
                    {
                        antsdkchatMsg.content = new DeleteActivity_content
                        {
                            activityId = sdkmsgdeleteactivityObj.content.activityId,
                            groupId = sdkmsgdeleteactivityObj.content.groupId
                        };
                    }
                    return antsdkchatMsg;
                }

                //返回空
                        return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkChatMsg.GetReceiveAntSdkChatInfo]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 方法说明：根据触角SDK聊天信息转换为平台SDK聊天信息进行发送
        /// </summary>
        /// <param name="antsdksend"></param>
        /// <returns></returns>
        internal static MsSdkMessageChat GetSdkSend(ChatBase antsdksend)
        {
            if (antsdksend == null)
            {
                return null;
            }
            try
            {
                var multisdksend = antsdksend as MultiSend;
                if (multisdksend != null)
                {
                    var sdksend = multisdksend.GetSdkSend();
                    return sdksend;
                }
                var textantsdksend = antsdksend as Text;
                if (textantsdksend != null)
                {
                    var sdksend = textantsdksend.GetSdkSend();
                    return sdksend;
                }
                var pictureantsdksend = antsdksend as Picture;
                if (pictureantsdksend != null)
                {
                    var sdksend = pictureantsdksend.GetSdkSend();
                    return sdksend;
                }
                var audioantsdksend = antsdksend as Audio;
                if (audioantsdksend != null)
                {
                    var sdksend = audioantsdksend.GetSdkSend();
                    return sdksend;
                }
                var videoantsdksend = antsdksend as Video;
                if (videoantsdksend != null)
                {
                    var sdksend = videoantsdksend.GetSdkSend();
                    return sdksend;
                }
                var fileantsdksend = antsdksend as File;
                if (fileantsdksend != null)
                {
                    var sdksend = fileantsdksend.GetSdkSend();
                    return sdksend;
                }
                var maplocationantsdksend = antsdksend as MapLocation;
                if (maplocationantsdksend != null)
                {
                    var sdksend = maplocationantsdksend.GetSdkSend();
                    return sdksend;
                }
                var mixamagetextantsdksend = antsdksend as MixMessage;
                if (mixamagetextantsdksend != null)
                {
                    var sdksend = mixamagetextantsdksend.GetSdkSend();
                    return sdksend;
                }
                var atantsdksend = antsdksend as At;
                if (atantsdksend != null)
                {
                    var sdksend = atantsdksend.GetSdkSend();
                    return sdksend;
                }
                var multiauviantsdksend = antsdksend as MultiAudioVideo;
                if (multiauviantsdksend != null)
                {
                    var sdksend = multiauviantsdksend.GetSdkSend();
                    return sdksend;
                }
                var revercationsdksend = antsdksend as Revocation;
                if (revercationsdksend != null)
                {
                    var sdksend = revercationsdksend.GetSdkSend();
                    return sdksend;
                }
                //没有找到返回空
                return antsdksend.GetSdkSendBase<MsChatMsgText>();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkChatMsg.GetSdkSend]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 方法说明：根据当前聊天父信息，获取具体的触角SDK聊天信息
        /// </summary>
        /// <param name="chatMsg">聊天父信息</param>
        /// <returns>触角SDK聊天信息</returns>
        internal static AntSdkChatMsg.ChatBase GetAntSdkSpecificChatMsg(AntSdkChatMsg.ChatBase chatMsg)
        {
            var errorMsg = string.Empty;
            switch (chatMsg.MsgType)
            {
                case AntSdkMsgType.MultiUserSend:
                    var multisend = new AntSdkChatMsg.MultiSend();
                    multisend = chatMsg.SetAntSdkValues(multisend);
                    multisend.content = chatMsg.sourceContent;
                    return multisend;
                case AntSdkMsgType.Revocation:
                    var recall = new AntSdkChatMsg.Revocation();
                    recall = chatMsg.SetAntSdkValues(recall);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        //recall.content = chatMsg.sourceContent;
                        //JsonCoder.DeserializeObject<AntSdkChatMsg.Revocation_content>(
                        //chatMsg.sourceContent, ref errorMsg);
                    }
                    return recall;
                case AntSdkMsgType.ChatMsgText:
                    var text = new AntSdkChatMsg.Text();
                    text = chatMsg.SetAntSdkValues(text);
                    text.content = chatMsg.sourceContent;
                    return text;
                case AntSdkMsgType.ChatMsgPicture:
                    var picture = new AntSdkChatMsg.Picture();
                    picture = chatMsg.SetAntSdkValues(picture);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        picture.content = JsonCoder.DeserializeObject<AntSdkChatMsg.Picture_content>(
                            chatMsg.sourceContent, ref errorMsg);
                    }
                    return picture;
                case AntSdkMsgType.ChatMsgAudio:
                    var audio = new AntSdkChatMsg.Audio();
                    audio = chatMsg.SetAntSdkValues(audio);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        audio.content = JsonCoder.DeserializeObject<AntSdkChatMsg.Audio_content>(chatMsg.sourceContent,
                            ref errorMsg);
                    }
                    return audio;
                case AntSdkMsgType.ChatMsgVideo:
                    var video = new AntSdkChatMsg.Video();
                    video = chatMsg.SetAntSdkValues(video);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        video.content = JsonCoder.DeserializeObject<AntSdkChatMsg.Video_content>(chatMsg.sourceContent,
                            ref errorMsg);
                    }
                    return video;
                case AntSdkMsgType.ChatMsgFile:
                    var file = new AntSdkChatMsg.File();
                    file = chatMsg.SetAntSdkValues(file);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        file.content = JsonCoder.DeserializeObject<AntSdkChatMsg.File_content>(chatMsg.sourceContent,
                            ref errorMsg);
                    }
                    return file;
                case AntSdkMsgType.ChatMsgMapLocation:
                    var maplocation = new AntSdkChatMsg.MapLocation();
                    maplocation = chatMsg.SetAntSdkValues(maplocation);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        maplocation.content =
                            JsonCoder.DeserializeObject<AntSdkChatMsg.MapLocation_content>(chatMsg.sourceContent,
                                ref errorMsg);
                    }
                    return maplocation;
                case AntSdkMsgType.ChatMsgMixMessage:
                    var miximagetext = new AntSdkChatMsg.MixMessage();
                    miximagetext = chatMsg.SetAntSdkValues(miximagetext);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        DealWithMixMsg(chatMsg.sourceContent, ref miximagetext);
                    }
                    return miximagetext;
                case AntSdkMsgType.ChatMsgAt:
                    var sdkat = chatMsg.GetSdkSendBase<MsChatMsgAt>();
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        sdkat.content = JsonCoder.DeserializeObject<List<MsChatMsgAt_content>>(chatMsg.sourceContent,
                            ref errorMsg);
                    }
                    var antsdkat = AntSdkChatMsg.GetAntSdkReceivedChat(sdkat);
                    return antsdkat;
                case AntSdkMsgType.ChatMsgMultiAudioVideo:
                    var multiav = new AntSdkChatMsg.MultiAudioVideo();
                    multiav = chatMsg.SetAntSdkValues(multiav);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        multiav.content =
                            JsonCoder.DeserializeObject<AntSdkChatMsg.MultiAudioVideo_content>(chatMsg.sourceContent,
                                ref errorMsg);
                    }
                    return multiav;
                case AntSdkMsgType.CreateVote:
                    var createvotemsg = new AntSdkChatMsg.CreateVoteMsg();
                    createvotemsg = chatMsg.SetAntSdkValues(createvotemsg);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        createvotemsg.content =
                            JsonCoder.DeserializeObject<AntSdkChatMsg.CreateVote_content>(chatMsg.sourceContent,
                                ref errorMsg);
                    }
                    return createvotemsg;
                case AntSdkMsgType.CreateActivity:
                    var createactivitymsg = new AntSdkChatMsg.ActivityMsg();
                    createactivitymsg = chatMsg.SetAntSdkValues(createactivitymsg);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        createactivitymsg.content =
                            JsonCoder.DeserializeObject<AntSdkChatMsg.Activity_content>(chatMsg.sourceContent,
                                ref errorMsg);
                    }
                    return createactivitymsg;
                case AntSdkMsgType.PointAudioVideo:
                    var pointAudioVideoMsg = new AntSdkChatMsg.PointAudioVideo();
                    pointAudioVideoMsg = chatMsg.SetAntSdkValues(pointAudioVideoMsg);
                    if (!string.IsNullOrEmpty(chatMsg.sourceContent))
                    {
                        pointAudioVideoMsg.content =
                            JsonCoder.DeserializeObject<AntSdkChatMsg.PointAudioVideo_content>(chatMsg.sourceContent,
                                ref errorMsg);
                    }
                    return pointAudioVideoMsg;
            }
            return null;
        }

        /// <summary>
        /// 方法说明：个数处理混合消息类型中的At消息
        /// </summary>
        /// <param name="mixinfoContent"></param>
        /// <param name="antsdkchatMsg"></param>
        internal static void DealWithMixMsg(string mixinfoContent, ref MixMessage antsdkchatMsg)
        {
            var miximageTexts = new List<MixMessage_content>();
            if (!string.IsNullOrEmpty(mixinfoContent))
            {
                var errMsg = string.Empty;
                JsonCoder.DeserializeJson(mixinfoContent, ref miximageTexts, ref errMsg);
                if (miximageTexts?.Count > 0)
                {
                    foreach (var mix in miximageTexts)
                    {
                        var dealObj = mix.content;
                        mix.content = null;
                        if (dealObj == null || dealObj.ToString() == string.Empty ||
                            dealObj.ToString().Trim().Length <= 2)
                        {
                            continue;
                        }
                        if (mix.type != SDK.Service.EnumExpress.GetDescription(SdkMsgType.ChatMsgAt))
                        {
                            continue;
                        }
                        //进行@消息处理
                        var atjson = dealObj.ToString().Trim();
                        if (atjson.StartsWith("{"))
                        {
                            atjson = atjson.Substring(1);
                        }
                        if (atjson.EndsWith("}"))
                        {
                            atjson = atjson.Substring(0, atjson.Length - 1);
                        }
                        //处理
                        var atEntitys = new List<At_content>();
                        JsonCoder.DeserializeJson(atjson, ref atEntitys, ref errMsg);
                        if (atEntitys != null)
                        {
                            mix.content = atEntitys;
                        }
                    }
                }
            }
            antsdkchatMsg.content = miximageTexts;
        }
    }
}
