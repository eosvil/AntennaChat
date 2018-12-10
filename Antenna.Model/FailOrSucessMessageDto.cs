using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.AntSdk.AntModels;

namespace Antenna.Model
{
    public class FailOrSucessMessageDto
    {
        public int mtp { set; get; }
        public string content { set; get; }
        public string sessionid { set; get; }
        public string lastDatetime { set; get; }
        public burnMsg.isSendSucessOrFail IsSendSucessOrFail { set; get; }
        public burnMsg.isBurnMsg IsBurnMsg { set; get; }
    }

    public class burnMsg
    {
        public enum isBurnMsg
        {
            yesBurn,
            notBurn
        }
        public enum isSendSucessOrFail
        {
            sucess,
            fail
        }
        public enum IsReadImg
        {
            read,
            notRead
        }
        /// <summary>
        /// 判断阅后即焚图片是否有效
        /// </summary>
        public enum IsEffective
        {
            /// <summary>
            /// 有效
            /// </summary>
            effective,
            /// <summary>
            /// 无效
            /// </summary>
            NotEffective,
            /// <summary>
            /// 未知
            /// </summary>
            UnKnow
        }
    }

    public class sendTextDto
    {
        public string msgStr { set; get; }
        public string messageid { set; get; }
        public string imageTipId { set; get; }
        public string imageSendingId { set; get; }
        /// <summary>
        /// 针对AT消息 其它消息不用赋值
        /// </summary>
        public string strAtText { set; get; }
        /// <summary>
        /// 针对AT消息 其他消息不用赋值
        /// </summary>
        public object AtArray { set; get; }
        public AntSdkFailOrSucessMessageDto FailOrSucess { set; get; }
        /// <summary>
        /// 消息来源 true为重新发送 反之不是
        /// </summary>
        public bool isOnceSendMsg { set; get; }
    }
}
