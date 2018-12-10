using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// MQTT收到用户信息变更
    /// </summary>
    public class MsUserInfoModify : MsSdkUserBase
    {
        /// <summary>
        /// 用户信息变更通知
        /// </summary>
        public MsUserInfoModify_content attr { get; set; }
    }

    /// <summary>
    /// 用户信息变更数据
    /// </summary>
    public class MsUserInfoModify_content
    {
        /// <summary>
        /// 职位
        /// </summary>
        public string position { get; set; } = string.Empty;


        /// <summary>
        /// 头像eg http://www.baidu.com
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// 个性签名
        /// </summary>
        public string signature { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public string sex { get; set; } = string.Empty;

        /// <summary>
        /// 部门ID
        /// </summary>
        public string departmentId { get; set; } = string.Empty;

        /// <summary>
        /// 人员名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 工号
        /// </summary>
        public string userNum { get; set; } = string.Empty;

        /// <summary>
        /// 电话号码
        /// </summary>
        public string phone { get; set; } = string.Empty;
    }
}
