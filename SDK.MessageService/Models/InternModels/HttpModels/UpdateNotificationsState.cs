using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    internal class UpdateNotificationsStateInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 群公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;
    }
}
