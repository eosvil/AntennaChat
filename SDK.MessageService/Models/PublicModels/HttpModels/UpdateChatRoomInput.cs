using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 修改聊天室信息输入参数
    /// </summary>
    public class UpdateChatRoomInput
    {
        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;

        /// <summary>
        /// “0”未开启，”1”开启机器人
        /// </summary>
        public string robotFlag { get; set; } = string.Empty;

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;
    }
}
