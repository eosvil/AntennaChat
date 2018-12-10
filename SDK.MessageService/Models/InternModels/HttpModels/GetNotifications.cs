using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取群的所有公告输入参数
    /// </summary>
    internal class GetNotificationsInput : IInputQuery
    {
        /// <summary>
        /// 当前登陆用户ID[path]
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 当前app的版本[path]
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 目标ID（一般指群ID）[request] 
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 实现Request类型参数处理
        /// </summary>
        /// <returns></returns>
        public string GetQuery() => $"?userId={userId}&targetId={targetId}";
    }
}
