using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SDK.AntSdk.AntModels;

namespace Antenna.Model
{
    public class UpLoadFilesDto
    {
        public string progressId { set; get; }
        public string fileGuid { set; get; }
        public string fileName { set; get; }
        public string fileSize { set; get; }
        public string localOrServerPath { set; get; }
        public string fileExtendName { set; get; }
        public string cmpcd { set; get; }
        public string seId { set; get; }
        public string fileImgGuid { set; get; }
        public string fileTextGuid { set; get; }
        public string fileOpenguid { set; get; }
        public string fileOpenDirectory { set; get; }
        //后加
        public string Size { set; get; }
        public string fileUrl { set; get; }

        public string messageId { set; get; }
        /// <summary>
        /// 重发image ID
        /// </summary>
        public string imageTipId { set; get; }
        public string imageSendingId { set; get; }
        public AntSdkFailOrSucessMessageDto FailOrSucess { set; get; }
        public DateTime DtTime { set; get; }
        /// <summary>
        /// 发送来源
        /// </summary>
        public AntSdkSendFrom.SendFrom from { set; get; }
        /// <summary>
        /// 机器人标识
        /// </summary>
        public bool IsRobot { set; get; }
        /// <summary>
        /// 针对上传图片、文件有用
        /// </summary>
        public MessageStateArg ImgOrFileArg { set; get; }
    }
}
