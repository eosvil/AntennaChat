using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 聊天消息接收
    /// </summary>
    /// 创建者：赵雪峰 20160921
    public class ChatMsgReceive
    {
        //如果messageId为null则消息本人发出去的消息，服务给的回执。如果不为NULL则是好友发过来的消息
        //如果是回执"sendUserId","targetId",这两个字段值都是发送者ID 
        public string messageId { get; set; }//消息ID
        public string sendUserId { get; set; }//消息发送者ID
        public string targetId { get; set; }//消息接收者ID
        public string companyCode { get; set; }//公司代码
        public string content { get; set; } = string.Empty;//消息内容
        public string sendTime { get; set; }//消息发送时间
        public string chatIndex { get; set; }//消息游标
        public string os { get; set; }//操作系统类型
        public string sessionId { get; set; }//会话ID
        /// <summary>
        /// 用来标识消息模式为阅后即焚（阅后即焚相关字段）1代表阅后即焚消息
        /// </summary>
        public string flag { get; set; }

        #region  非后台接口字段
        public string MTP { set; get; }//消息类型
        public string SENDORRECEIVE { set; get; }//接受或发送标识
        public int sendsucessorfail { set; get; }//发送成功或失败标识
        public string username { set; get; }//发送人名字

        public string status { get; set; }//1--表示已读、0--表示未读

        /// <summary>
        /// 阅后即焚已读时间
        /// </summary>
        public string readtime { set; get; }
        /// <summary>
        /// 针对文件和图片用
        /// </summary>
        public string uploadOrDownPath { set; get; }
        public int typeBurnMsg { set; get; }
        #endregion 
    }

}
