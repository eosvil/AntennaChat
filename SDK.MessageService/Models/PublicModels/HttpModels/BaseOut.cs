using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// SDK错误代码及错误信息Model
    /// </summary>
    public class ErrorOutput
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public string errorCode { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMsg { get; set; } = string.Empty;
    }

    /// <summary>
    /// SDK基础输出Model
    /// </summary>
    public class BaseOutput : ErrorOutput
    {
        /// <summary>
        /// 数据信息[SDK的Http请求返回数据包在data中]
        /// </summary>
        public object data { get; set; }
    }
}
