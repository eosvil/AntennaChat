using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk
{
    /// <summary>
    /// 触角文件上传输出
    /// </summary>
    public class AntSdkFileUpLoadOutput
    {
        public string fileId { get; set; } = string.Empty;
        public string fileName { get; set; } = string.Empty;
        public string fileType { get; set; } = string.Empty;
        public string dowmnloadUrl { get; set; } = string.Empty;
        public string fileSize { get; set; } = string.Empty;
        public string fileMD5 { get; set; } = string.Empty;
        public string requestSource { get; set; } = string.Empty;
        public string userId { get; set; } = string.Empty;
        public string cmpcd { get; set; } = string.Empty;
        public string funSource { get; set; } = string.Empty;
        public string fileTarget { get; set; } = string.Empty;
        public string fileStatus { get; set; } = string.Empty;
        public string createTime { get; set; } = string.Empty;
        public string thumbnailUrl { get; set; }

        internal static AntSdkFileUpLoadOutput GetAntSdk(SdkFileUpLoadOutput sdkentity)
        {
            if(sdkentity == null) { return null; }
            var entity = new AntSdkFileUpLoadOutput
            {
                fileId = sdkentity.fileId,
                fileName = sdkentity.fileName,
                fileType = sdkentity.fileType,
                dowmnloadUrl = sdkentity.dowmnloadUrl,
                fileSize = sdkentity.fileSize,
                fileMD5 = sdkentity.fileMD5,
                requestSource = sdkentity.requestSource,
                userId = sdkentity.userId,
                cmpcd = sdkentity.cmpcd,
                funSource = sdkentity.funSource,
                fileTarget = sdkentity.fileTarget,
                fileStatus = sdkentity.fileStatus,
                createTime = sdkentity.createTime
            };
            return entity;
        }
    }
}
