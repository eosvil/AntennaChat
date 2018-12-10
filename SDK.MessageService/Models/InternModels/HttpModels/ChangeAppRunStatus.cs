using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 修改当前APP运行状态Model
    /// </summary>
    internal class ChangeAppRunStatusInput
    {
        /// <summary>
        /// 当前app的版本号
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 当前用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 运行状态 1–后台运行 0–前台运行
        /// </summary>
        public int isBackground { get; set; }
    }

    /// <summary>
    /// 更新小米、华为、魅族、信鸽的信息
    /// </summary>
    internal class UpdatePushDeviceTokenInput
    {
        /// <summary>
        /// 终端类型 3:android 4:ios
        /// </summary>
        public string os { get; set; } = "3";

        /// <summary>
        /// 当前app的版本号
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 当前用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 手机注册第三方推送的唯一标识
        /// </summary>
        public string deviceToken { get; set; } = "123";

        /// <summary>
        /// 1—小米 2—华为 3—魅族 0—苹果,其他第三方厂商的手机用小米推送
        /// </summary>
        public string manufacture { get; set; } = "1";

        /// <summary>
        /// 只有ios才需要传值
        /// </summary>
        public string sound { get; set; } = string.Empty;
    }
}
