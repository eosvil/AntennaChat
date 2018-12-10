using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class SdkFileUpLoadOutput
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
    }
}
