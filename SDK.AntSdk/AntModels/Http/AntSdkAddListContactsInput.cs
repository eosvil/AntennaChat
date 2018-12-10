using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkAddListContactsInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        /// <summary>
        /// 联系人版本号
        /// </summary>
        public string dataVersion { get; set; } = string.Empty;

        /// <summary>
        /// 获取联系人信息—-增量信息(返回值区分组织架构变化和不变化两种情况)
        /// </summary>
        /// <returns>Url组成</returns>
        public string GetQuery() => $"?version={version}&dataVersion={dataVersion}";
    }
}
