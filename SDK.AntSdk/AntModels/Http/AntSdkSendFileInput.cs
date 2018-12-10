using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkSendFileInput
    {
        public string cmpcd { set; get; } = string.Empty;
        public string seId { set; get; } = string.Empty;
        public string fileFileName { set; get; } = string.Empty;
        public string file { set; get; } = string.Empty;
        public string progressId { set; get; } = string.Empty;
        public string imgguid { set; get; } = string.Empty;
        public string textguid { set; get; } = string.Empty;
        public string filesize { set; get; } = string.Empty;
        public string fileFileExtendName { set; get; } = string.Empty;
        public string imageHeight { set; get; } = string.Empty;
        public string imageWidth { set; get; } = string.Empty;
        public BackgroundWorker back { set; get; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { set; get; }
        /// <summary>
        /// 发送者ID
        /// </summary>
        public string sendUserId { set; get; }
        /// <summary>
        /// 目标ID
        /// </summary>
        public string targetId { set; get; }
        public bool isOnceSend { set; get; }
        /// <summary>
        /// 提前赋值chatIndex
        /// </summary>
        public int preChatIndex { set; get; }
        /// <summary>
        /// 消息来源 true为重新发送 反之不是
        /// </summary>
        public bool isOnceSendMsg { set; get; }

        /// <summary>
        /// 发送状态图片id
        /// </summary>
        public string imgeTipId { set; get; } = string.Empty;

        /// <summary>
        /// 发送中图片id
        /// </summary>
        public string imageSendingId { set; get; } = string.Empty;

        public string prePaths { set; get; } = string.Empty;
        public AntSdkFailOrSucessMessageDto FailOrSucess { set; get; }

        /// <summary>
        /// 获取SDK文件上传所需信息
        /// </summary>
        /// <returns></returns>
        internal SdkSendFileInput GetSdk()
        {
            var entity = new SdkSendFileInput
            {
                cmpcd = cmpcd,
                seId = seId,
                fileFileName = fileFileName,
                file = file,
                progressId = progressId,
                imgguid = imgguid,
                textguid = textguid,
                filesize = filesize,
                fileFileExtendName = fileFileExtendName,
                imageHeight = imageHeight,
                imageWidth = imageWidth,
                messageId = messageId,
                back = back,
                imgeTipId = imgeTipId,
                imageSendingId = imageSendingId,
                prePaths = prePaths,
                FailOrSucess = new SdkFailOrSucessMessageDto
                {
                    mtp = FailOrSucess.mtp,
                    content = FailOrSucess.content,
                    sessionid = FailOrSucess.sessionid,
                    lastDatetime = FailOrSucess.lastDatetime,
                    IsSendSucessOrFail =
                        FailOrSucess.IsSendSucessOrFail == AntSdkburnMsg.isSendSucessOrFail.sucess
                            ? SdkburnMsg.isSendSucessOrFail.sucess
                            : SdkburnMsg.isSendSucessOrFail.fail,
                    IsBurnMsg =
                        FailOrSucess.IsBurnMsg == AntSdkburnMsg.isBurnMsg.yesBurn
                            ? SdkburnMsg.isBurnMsg.yesBurn
                            : SdkburnMsg.isBurnMsg.notBurn
                }
            };
            return entity;
        }
    }

    public class AntSdkFailOrSucessMessageDto
    {
        public int mtp { set; get; }
        public string content { set; get; } = string.Empty;
        public string sessionid { set; get; } = string.Empty;
        public string lastDatetime { set; get; } = string.Empty;
        public AntSdkburnMsg.isSendSucessOrFail IsSendSucessOrFail { set; get; }
        public AntSdkburnMsg.isBurnMsg IsBurnMsg { set; get; }
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
        public string targetId { set; get; }
        public object obj { set; get; }
    }

    public class AntSdkburnMsg
    {
        public enum isBurnMsg
        {
            yesBurn,
            notBurn
        }

        public enum isSendSucessOrFail
        {
            sending,
            sucess,
            fail
        }
    }
    public class AntSdkSendFrom
    {
        public enum SendFrom
        {
            Send,
            OnceSend
        }
    }
    public class PointOrGroupFrom
    {
        public enum PointOrGroup
        {
           Point,
           Group
        }
    }
}
