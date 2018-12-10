using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class DeleteNotificationsByIdInput : IInputSuffPath
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 实现后缀路径方式
        /// </summary>
        /// <returns></returns>
        public string GetSuffPath() => $"/{userId}/{notificationId}";
    }
}
