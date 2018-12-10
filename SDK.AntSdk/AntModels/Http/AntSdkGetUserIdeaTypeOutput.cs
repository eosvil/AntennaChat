using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取意见反馈类型输出
    /// </summary>
    public class AntSdkGetUserIdeaTypeOutput
    {
        /// <summary>
        /// 类型ID
        /// </summary>
        public int typeId { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string typeName { get; set; } = string.Empty;
    }
}
