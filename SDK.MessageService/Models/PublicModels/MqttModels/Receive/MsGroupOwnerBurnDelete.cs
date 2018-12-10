using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到群主在阅后即焚模式下删除消息
    /// </summary>
    public class MsGroupOwnerBurnDelete : MsSdkMessageGroupBase
    {
        public MsGroupOwnerBurnDelete_content content { get; set; }
    }

    /// <summary>
    /// 群主在阅后即焚模式下删除消息
    /// </summary>
    public class MsGroupOwnerBurnDelete_content
    {
        /// <summary>
        /// 删除chatIndex小于maxIndex的所有阅后即焚消息
        /// </summary>
        public int maxIndex { get; set; }
    }
}
