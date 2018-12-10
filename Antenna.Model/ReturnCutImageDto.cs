using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class ReturnCutImageDto:BaseBoolDto
    {
        public string fileUrl { set; get; }
        public string thumbUrl { set; get; }
        public string imgSize { set; get; }
        public string thumbSize { set; get; }
        /// <summary>
        /// 上传之前路径
        /// </summary>
        public string prePath { set; get; }
        public string messageId { set; get; }
        /// <summary>
        /// 显示发送状态图片id
        /// </summary>
        public string imagedTipId { set; get; }
        public string imageSendingId { set; get; }
        /// <summary>
        /// 状态 0 失败 1成功
        /// </summary>
        public string isState { set; get; }
        public AntSdkFailOrSucessMessageDto FailOrSucess { set; get; }
        /// <summary>
        /// 提前赋值chatIndex
        /// </summary>
        public int preChatIndex { set; get; }
        /// <summary>
        /// 消息来源 true为重新发送 反之不是
        /// </summary>
        public bool isOnceSendMsg { set; get; }
        /// <summary>
        /// 是否真正上传成功 1上传失败  0上传成功
        /// </summary>
        public string isSucessOrFail { set; get; }
        /// <summary>
        /// 发送者ID
        /// </summary>
        public string sendUserId { set; get; }
        /// <summary>
        /// 目标ID
        /// </summary>
        public string targetId { set; get; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionid { set; get; }
    }
}