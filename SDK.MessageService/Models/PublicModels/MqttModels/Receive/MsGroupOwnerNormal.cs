using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT接收到群主切换为普通消息模式
    /// </summary>
    public class MsGroupOwnerNormal : MsSdkMessageGroupBase
    {
        /// <summary>
        /// 群主切换为普通消息模式
        /// </summary>
        public string content { get; set; } = string.Empty;
    }
}
