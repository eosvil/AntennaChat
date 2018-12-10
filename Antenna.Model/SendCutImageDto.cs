using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SDK.AntSdk.AntModels;
using Antenna.Model.PictureAndTextMix;

namespace Antenna.Model
{
    public class SendCutImageDto:BaseBoolDto
    {
        public string cmpcd { set; get; }
        public string seId { set; get; }
        public string fileFileName { set; get; }
        public string file { set; get; }
        public string progressId { set; get; }
        public string imgguid { set; get; }
        public string textguid { set; get; }
        public string filesize { set; get; }
        public string fileFileExtendName { set; get; }
        public string imageHeight { set; get; }
        public string imageWidth { set; get; }
        public string messageId { set; get; }
        public BackgroundWorker back { set; get; }
        /// <summary>
        /// 发送状态图片id
        /// </summary>
        public string imgeTipId { set; get; }
        /// <summary>
        /// 发送中图片id
        /// </summary>
        public string imageSendingId { set; get; }
        public string prePaths { set; get; }
        public AntSdkFailOrSucessMessageDto FailOrSucess { set; get; }
        /// <summary>
        /// 发送来源
        /// </summary>
        public AntSdkSendFrom.SendFrom from { set; get; }
        /// <summary>
        /// 是否是阅后即焚
        /// </summary>
        public  bool isBurn { set; get; }
        /// <summary>
        /// 针对上传图片、文件有用
        /// </summary>
        public MessageStateArg ImgOrFileArg { set; get; }
        public CefSharp.Wpf.ChromiumWebBrowser Cef { get; set; }
        public CurrentChatDto Dto { get; set; }
    }
}