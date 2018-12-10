using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取联系人信息—-增量信息(返回值区分组织架构变化和不变化两种情况)
    /// </summary>
    public class AntSdkAddListContactsOutput
    {
        /// <summary>
        /// 表示只用增量更新
        /// </summary>
        public string incrementFlag { get; set; } = string.Empty;

        /// <summary>
        /// 联系人版本号
        /// </summary>
        public string dataVersion { get; set; } = string.Empty;

        /// <summary>
        /// 增量用户信息
        /// </summary>
        public AntSdkAddListContactsOutput_Users users { get; set; }

        /// <summary>
        /// 增量部门信息
        /// </summary>
        public AntSdkAddListContactsOutput_Departs departs { get; set; }
    }

    /// <summary>
    /// 增量用户信息
    /// </summary>
    public class AntSdkAddListContactsOutput_Users
    {
        public List<AntSdkContact_User> add { get; set; }
        public List<AntSdkContact_User> update { get; set; }
        public List<AntSdkContact_User> delete { get; set; }
    }

    /// <summary>
    /// 增量部门信息
    /// </summary>
    public class AntSdkAddListContactsOutput_Departs
    {
        public List<AntSdkContact_Depart> add { get; set; }
        public List<AntSdkContact_Depart> update { get; set; }
        public List<AntSdkContact_Depart> delete { get; set; }
    }
}
