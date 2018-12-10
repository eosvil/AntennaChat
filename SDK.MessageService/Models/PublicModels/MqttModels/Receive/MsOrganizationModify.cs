using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    //[0][0] messageType 消息类型 String 2301 
    //[1][0] dataVersion 版本号 String
    //[1][1] attr 自定义消息字段 String jsonString
    /// <summary>
    /// MQTT收到组织架构信息
    /// </summary>
    public class MsOrganizationModify : SdkMsBase
    {
        /// <summary>
        /// dataVersion 版本号 String
        /// </summary>
        public string dataVersion { get; set; } = string.Empty;

        /// <summary>
        /// attr 自定义消息字段 String jsonString
        /// </summary>
        public string attr { get; set; } = string.Empty;
    }
}
