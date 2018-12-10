using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 更新成聊天室员属性输入参数
    /// </summary>
    public class UpdateChatRoomMemberInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 用户描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 用户备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;
    }
}
