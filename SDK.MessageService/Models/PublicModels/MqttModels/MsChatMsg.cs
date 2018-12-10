/*
 * 接收/发送消息文件：    
 * 消息内容可为：文本[string];
 *               图片[ChatMsgPicture];
 *               音频[ChatMsgAudio];
 *               视频[ChatMsgVideo];
 *               文件[ChatMsgFile];
 *               地理位置[ChatMsgMapLocation];
 *               图文混合[ChatMsgMixImageText[]];
 *               @消息[ChatMsgAt[]];
 *               多人视频消息开启[ChatMsgMultiAudioVideo]
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 群发消息[目前只支持文本]
    /// </summary>
    public class MsMultiSendMsg : MsSdkMessageChat
    {
        public string content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 聊天消息基本实体
    /// </summary>
    public class MsChatMsgText : MsSdkMessageChat
    {
        /// <summary>
        /// 文本消息内容
        /// </summary>
        public string content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 图片消息：对于图片消息，messageType为1002，[1][11]中对应的内容
    /// </summary>
    public class MsChatMsgPicture : MsSdkMessageChat
    {
        public MsChatMsgPicture_content content { get; set; }
    }

    public class MsChatMsgPicture_content
    {
        /// <summary>
        /// 原始图url
        /// </summary>
        public string picUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 音频消息：对于音频消息，messageType为1003，[1][11]中对应的内容
    /// </summary>
    public class MsChatMsgAudio : MsSdkMessageChat
    {
        public MsChatMsgAudio_content content { get; set; }
    }

    public class MsChatMsgAudio_content
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
    public class MsChatMsgVideo : MsSdkMessageChat
    {
        public MsChatMsgVideo_content content { get; set; }
    }

    public class MsChatMsgVideo_content
    {

    }

    /// <summary>
    /// 文件消息：对于文件消息，messageType为1005，[1][11]中对应的内容
    /// </summary>
    public class MsChatMsgFile : MsSdkMessageChat
    {
        public MsChatMsgFile_content content { get; set; }
    }

    public class MsChatMsgFile_content
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
    public class MsChatMsgMapLocation : MsSdkMessageChat
    {
        public MsChatMsgMapLocation_content content { get; set; }
    }

    public class MsChatMsgMapLocation_content
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
    /// 混合消息(数组形式)：对于图文混合消息，messageType为1007，[1][11]中对应的内容
    /// 由于混合消息中的子content是一个混合类型，所以在底层是，先不做转换处理，直接
    /// 将Json串抛出，让在外面的程序做自定义处理
    /// </summary>
    public class MsChatMsgMixMessage : MsSdkMessageChat
    {
        /// <summary>
        /// 混合消息内容
        /// </summary>
        public string content { get; set; }
    }

    /// <summary>
    /// 混合消息(数组形式)：对于图文混合消息，messageType为1007，[1][11]中对应的内容
    /// 由于混合消息中的子content是一个混合类型，所以在底层是，先不做转换处理，直接
    /// 将Json串抛出，让在外面的程序做自定义处理
    /// </summary>
    public class MsChatMsgMixMessage_content
    {
        /// <summary>
        /// 1001：文本(包含表情)；1002：图片；0000：换行符
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// type-1001：文本消息内容；jsonString(图片消息是对象)[{\"picUrl\":\"http://www.baidu.com\"}]
        /// </summary>
        public string content { get; set; } = string.Empty;
    }

    /// <summary>
    /// @消息(数组形式)：对于@消息，messageType为1008，[1][11]中对应的内容如下：
    /// </summary>
    public class MsChatMsgAt : MsSdkMessageChat
    {
        public List<MsChatMsgAt_content> content { get; set; }
    }


     /*
     *A发送的消息如下所示：
     *大家好 @全体成员 华海乐盈 @张三@李四 您好
     *@李四 图片url
     *对应的消息解析如下所示：
     *[
     *    {
     *        "type":"1001",                                //消息类型，可能是文本、图片等
     *        "content": "大家好",                           //内容
     *    },
     *    {
     *        "type":"1111",                                //@全体成员
     *    },
     *    {
     *        "type":"1001",                                //消息类型，可能是文本、图片等
     *        "content": "华海乐盈",                         //内容
     *    },
     *    {
     *        "type":"1112",                                //@普通成员
     *        "ids": [
     *            "P146650175733711",
     *            "P146650175733712"
     *        ],                                            //被@人的ID(数组)        
     *        "names": [
     *            "张三",
     *            "李四"
     *        ]                                            //被@人的名字(数组)
     *    },
     *    {
     *        "type":"1001",                                //消息类型，可能是文本、图片等
     *        "content": "您好",                            //内容
     *    },
     *    {
     *        "type":"0000"                            //换行符
     *    },
     *    {
     *        "type":"1112",                                     //@普通成员
     *        "ids": [
     *            "P146650175733712"                             
     *        ],        
     *        "names": [
     *            "李四"                  //被@人的名字
     *        ]                                 
     *    },
     *    {
     *        "type":"1002",
     *        "content":"{\"picUrl\":\"http://www.baidu.com\"}"  //jsonString
     *    }
     *]
     */
    public class MsChatMsgAt_content
    {
        /// <summary>
        /// 0000：回车换行；1001：文本(包含表情)内容；1002：图片内容；1111：@全体成员；1112：@普通成员；
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// type-1001：文本消息内容；type-1002：jsonString(图片消息是对象)[{\"picUrl\":\"http://www.baidu.com\"}].........[jsonString]
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

    /// <summary>
    /// 多人音频视频开启消息：对于多人音频视频消息，包括开启/结束多人音视频，messageType为1009，[1][11]中对应的内容
    /// </summary>
    public class MsChatMsgMultiAudioVideo : MsSdkMessageChat
    {
        public MsChatMsgMultiAudioVideo_content content { get; set; }
    }

    public class MsChatMsgMultiAudioVideo_content 
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
    public class MsChatMsgPointAudioVideo : MsSdkMessageChat
    {
        /// <summary>
        /// 单人（点对点）音频视频消息内容
        /// </summary>
        public MsChatMsgPointAudioVideo_content content { get; set; }
    }

    /// <summary>
    /// 单人（点对点）音频视频消息内容
    /// </summary>
    public class MsChatMsgPointAudioVideo_content
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
    }

    /// <summary>
    /// 消息撤回：1014
    /// </summary>
    public class MsChatMsgRevocation : MsSdkMessageChat
    {
        /// <summary>
        /// 消息撤回内容
        /// </summary>
        public MsChatMsgRevocation_content content { get; set; }
    }

    /// <summary>
    /// 撤销消息内容
    /// </summary>
    public class MsChatMsgRevocation_content
    {
        /// <summary>
        /// 要撤回的消息ID
        /// </summary>
        public string messageId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建投票：1015 
    /// </summary>
    public class MsChatMsgCreateVote : MsSdkMessageChat
    {
        /// <summary>
        /// 创建投票消息内容
        /// </summary>
        public MsChatMsgCreateVote_content content { get; set; }
    }
    /// <summary>
    /// 创建投票消息内容
    /// </summary>
    public class MsChatMsgCreateVote_content
    {
        /// <summary>
        /// 投票id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 选项数组
        /// </summary>
        public List<MsChatMsgVoteOption> options { get; set; } = new List<MsChatMsgVoteOption>();

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
    /// 删除投票：1017
    /// </summary>
    public class MsChatMsgDeteleVote : MsSdkMessageChat
    {
        /// <summary>
        /// 删除投票消息内容
        /// </summary>
        public MsChatMsgDeteleVote_content content { get; set; }
    }
    /// <summary>
    /// 创建或删除投票消息内容
    /// </summary>
    public class MsChatMsgDeteleVote_content
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
        public List<MsChatMsgVoteOption> options { get; set; } = new List<MsChatMsgVoteOption>();

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
    /// 投票选项
    /// </summary>
    public class MsChatMsgVoteOption
    {
        public string name { get; set; }
    }
    /// <summary>
    /// 活动消息对象
    /// </summary>
    public class MsChatMsgActivity: MsSdkMessageChat
    {
        /// <summary>
        /// 活动消息内容
        /// </summary>
        public MsChatMsgActivity_content content { get; set; }
    }
    /// <summary>
    /// 活动消息内容对象
    /// </summary>
    public class MsChatMsgActivity_content
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
    public class MsChatMsgDeleteActivity: MsSdkMessageChat
    {
        public MsChatMsgDeleteActivity_content content { get; set; }
    }
    /// <summary>
    /// 活动删除消息内容对象
    /// </summary>
    public class MsChatMsgDeleteActivity_content
    {
        // <summary>
        /// 活动标识
        /// </summary>
        public int activityId { get; set; }
        /// <summary>
        /// 群标识
        /// </summary>
        public string groupId { get; set; }
    }
}
