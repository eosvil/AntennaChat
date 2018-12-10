using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{

    /// <summary>
    /// 获取联系人信息，返回数组格式(输入参数)
    /// </summary>
    public class AntSdkListContactsInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        public string GetQuery() => $"?version={version}";
    }

    /// <summary>
    /// 获取联系人信息，返回数组格式(输出参数)
    /// </summary>
    public class AntSdkListContactsOutput
    {
        /// <summary>
        /// 本次数据版本号
        /// </summary>
        public string dataVersion { get; set; } = string.Empty;

        /// <summary>
        /// 所有联系人
        /// </summary>
        public List<AntSdkContact_User> users { get; set; }

        /// <summary>
        /// 所有部门
        /// </summary>
        public List<AntSdkContact_Depart> departs { get; set; }
    }

    /// <summary>
    /// 联系人信息
    ///  保持与数据库列名一致
    /// </summary>
    public class AntSdkContact_User: ICloneable
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        public string departmentId { get; set; } = string.Empty;

        /// <summary>
        /// 人员职位
        /// </summary>
        public string position { get; set; } = string.Empty;

        /// <summary>
        /// 人员id
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 人员名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 人员头像（有数据传值）
        /// </summary>
        public string picture { get; set; } = string.Empty;
        /// <summary>
        /// 人员头像复制
        /// </summary>
        public string copyPicture { set; get; }
        /// <summary>
        /// 工号
        /// </summary>
        public string userNum { get; set; } = string.Empty;

        /// <summary>
        /// 网易云信的ID
        /// </summary>
        public string accid { get; set; } = string.Empty;

        /// <summary>
        /// 网易云信的密码
        /// </summary>
        public string accToken { get; set; } = string.Empty;

        /// <summary>
        /// 签名
        /// </summary>
        public string signature { get; set; } = string.Empty;

        /// <summary>
        ///在线状态（0、离线， 1、在线，2、忙碌，3 离开,4 手机端在线）
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 账号使用状态（0 停用  2正常）
        /// </summary>
        public int status { get; set; } = 2;


        public AntSdkContact_User ContactUser { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 部门信息
    /// 保持与数据库列名一致
    /// </summary>
    public class AntSdkContact_Depart
    {
        /// <summary>
        /// 部门名称
        /// </summary>
        public string departName { get; set; } = string.Empty;

        /// <summary>
        /// 部门ID
        /// </summary>
        public string departmentId { get; set; } = string.Empty;

        /// <summary>
        /// 上级部门ID，如果没有，值为”0”
        /// </summary>
        public string parentDepartId { get; set; } = string.Empty;
    }
}
