using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取讨论组成员信息输入参数
    /// </summary>
    internal class GetGroupMembersInput : IInputQuery
    {
        /// <summary>
        /// 当前app的版本号
        /// </summary>
        public string version { get; set; } = string.Empty;

        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?groupId={groupId}&userId={userId}";
    }
}
