using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class GetGroupNotificationsInput : IInputQuery
    {
        /// <summary>
        /// 当前登陆用户ID输入参数
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 目标ID（一般指群ID） 
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 实现Request类型参数处理
        /// </summary>
        /// <returns></returns>
        public string GetQuery() => $"?userId={userId}&targetId={targetId}";
    }
}
