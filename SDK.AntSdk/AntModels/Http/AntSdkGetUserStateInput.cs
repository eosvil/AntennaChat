using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取部门下所有用户的在线状态
    /// </summary>
    public class AntSdkGetUserStateInput : AntSdkBaseInput
    {
        /// <summary>
        /// 部门ID，查询当前部门下所有用户的状态
        /// </summary>
        public string departmentId { get; set; } = string.Empty;
    }
}
