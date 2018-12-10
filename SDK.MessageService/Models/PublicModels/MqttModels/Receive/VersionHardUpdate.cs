using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到版本更新接口(硬更新）信息
    /// </summary>
    public class MsVersionHardUpdate : SdkMsBase
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 自定义消息字段
        /// </summary>
        public MsVersionHardUpdate_content attr { get; set; }
    }

    /// <summary>
    /// 版本更新接口(硬更新）信息
    /// </summary>
    public class MsVersionHardUpdate_content
    {
        /// <summary>
        /// 描述更新的内容
        /// </summary>
        public string describe { get; set; } = string.Empty;

        /// <summary>
        /// 更新的名称
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 更新类型,1:软更新，2：硬更新 固定值为2
        /// </summary>
        public string updateType { get; set; } = string.Empty;

        /// <summary>
        /// 更新url
        /// </summary>
        public string url { get; set; } = string.Empty;

        /// <summary>
        /// 是否删除数据库，1–不删除 2–删除，默认值为1（不删除）
        /// </summary>
        public string isDeleteDB { get; set; } = string.Empty;
    }
}
