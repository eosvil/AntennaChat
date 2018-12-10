using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class GetNotificationsByIdInput : IInputSuffPath
    {
        /// <summary>
        /// 当前登陆用户ID输入参数
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 实现后缀路径方式
        /// </summary>
        /// <returns></returns>
        public string GetSuffPath() => $"/{notificationId}";
    }
}
