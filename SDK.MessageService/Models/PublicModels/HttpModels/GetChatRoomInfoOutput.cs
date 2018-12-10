using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    ///  查询单个聊天室详细信息(输出参数)
    /// </summary>
    public class GetChatRoomInfoOutput
    {
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
        /// 聊天室名称
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
        /// 创建时间（时间戳）
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间（时间戳）
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

        /// <summary>
        /// 是否开启机器人
        /// </summary>
        public string robotFlag { get; set; } = string.Empty;

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<SdkMember> members { get; set; }
    }
}
