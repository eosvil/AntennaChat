using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到群主切换为阅后即焚模式
    /// </summary>
    public class MsGroupOwnerBurnMode : MsSdkMessageGroupBase
    {
        public MsGroupOwnerBurnMode_content content { get; set; }
    }

    /// <summary>
    /// 群主切换为阅后即焚模式
    /// </summary>
    public class MsGroupOwnerBurnMode_content
    {
        /// <summary>
        /// 删除chatIndex小于maxIndex的所有阅后即焚消息
        /// </summary>
        public int maxIndex { get; set; }
    }
}
