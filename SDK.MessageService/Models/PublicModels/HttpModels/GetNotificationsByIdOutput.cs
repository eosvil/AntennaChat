using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class GetNotificationsByIdOutput
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 公告标题
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 公告内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        public string attach { get; set; } = string.Empty;
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 读取状态 0-未读，1-已读
        /// </summary>
        public int readState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public string updateBy { get; set; } = string.Empty;
    }
}
