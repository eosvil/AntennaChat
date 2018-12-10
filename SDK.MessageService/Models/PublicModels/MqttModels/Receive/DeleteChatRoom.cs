using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 解散聊天室（MQTT接收）
    /// </summary>
    public class MsDeleteChatRoom : MsSdkMessageRoomBase
    {
        /// <summary>
        /// 通知内容
        /// </summary>
        public DeleteChatRoom_content content { get; set; }
    }

    /// <summary>
    /// 删除聊天室
    /// </summary>
    public class DeleteChatRoom_content
    {
        /// <summary>
        /// 创建者ID
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
        /// 描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;

        /// <summary>
        /// 是否开启机器人
        /// </summary>
        public string robotFlag { get; set; } = string.Empty;

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 删除聊天室（HTTP）
    /// </summary>
    public class DeleteChatRoomInput
    {
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;
    }
}
