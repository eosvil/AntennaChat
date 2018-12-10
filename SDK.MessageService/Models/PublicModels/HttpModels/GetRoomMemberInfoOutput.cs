using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 查询聊天室单个成员详细信息(输出参数)
    /// </summary>
    public class GetRoomMemberInfoOutput
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

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
        /// 创建时间（时间戳）
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间（时间戳）
        /// </summary>
        public string updateTime { get; set; } = string.Empty;
    }
}
