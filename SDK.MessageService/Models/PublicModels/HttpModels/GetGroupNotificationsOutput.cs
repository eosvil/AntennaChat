using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 当前登陆用户ID输出参数
    /// </summary>
    public class GetGroupNotificationsOutput
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
        /// 是否有附件，"1"表示有 "0"表示没有----String类型
        /// </summary>
        public string hasAttach { get; set; } = string.Empty;

        /// <summary>
        /// 群组ID
        /// </summary>
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
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;
    }
}
