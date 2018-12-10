using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class SdkSendFileInput
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
        public string messageId { set; get; } = string.Empty;
        public BackgroundWorker back { set; get; }
        /// <summary>
        /// 发送状态图片id
        /// </summary>
        public string imgeTipId { set; get; } = string.Empty;
        /// <summary>
        /// 发送中图片id
        /// </summary>
        public string imageSendingId { set; get; } = string.Empty;
        public string prePaths { set; get; } = string.Empty;
        public SdkFailOrSucessMessageDto FailOrSucess { set; get; }
    }
}
