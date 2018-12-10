using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 添加意见反馈输入
    /// </summary>
    public class AntSdkAddIdeaFeedBackInput : AntSdkBaseInput
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string username { get; set; } = string.Empty;

        /// <summary>
        /// 职位
        /// </summary>
        public string position { get; set; } = string.Empty;

        /// <summary>
        /// 终端类型
        /// </summary>
        public int os { get; set; } = (int) SdkEnumCollection.OSType.PC;

        /// <summary>
        /// 反馈分类的ID
        /// </summary>
        public string typeId { get; set; } = string.Empty;

        /// <summary>
        /// 反馈的内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 部门名称
        /// </summary>
        public string departName { get; set; } = string.Empty;
    }
}
