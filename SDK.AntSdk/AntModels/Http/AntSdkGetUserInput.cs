using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkGetUserInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        /// <summary>
        /// 需要获取信息的用户ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 组织Url地址
        /// </summary>
        /// <returns></returns>
        public string GetQuery() => $"?targetId={targetId}&version={version}";
    }
}
