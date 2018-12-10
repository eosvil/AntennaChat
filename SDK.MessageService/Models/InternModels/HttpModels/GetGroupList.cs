using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取用户的讨论组列表输入参数
    /// </summary>
    internal class GetGroupListInput : IInputQuery
    {
        ///<summary>
        /// 当前登陆用户ID
        /// </summary> 
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 获取Query型参数对应的路径信息
        /// </summary>
        string IInputQuery.GetQuery() => $"?userId={userId}";
    }
}
