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
    public class AntSdkUserInfoInput : IAntSdkInputQuery
    {
        /// <summary>
        /// 用户状态
        /// </summary>
        public int state { get; set; }
        public string GetQuery() => $"?state={state}";
    }
    /// <summary>
    /// 用户信息
    /// </summary>
    public class AntSdkUserInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 登录账号（账号或者邮箱）
        /// </summary>
        public string loginName { get; set; } = string.Empty;

        /// <summary>
        /// 用户头像地址
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// 0：女 1：男 2：未知
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱号码
        /// </summary>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// 部门名称
        /// </summary>
        public string departName { get; set; } = string.Empty;

        /// <summary>
        /// 部门ID
        /// </summary>
        public string departmentId { get; set; } = string.Empty;

        /// <summary>
        /// 职位
        /// </summary>
        public string position { get; set; } = string.Empty;

        /// <summary>
        /// qq号码
        /// </summary>
        public string qq { get; set; } = string.Empty;

        /// <summary>
        /// “0”：禁用“1”：启用
        /// </summary>
        public string voiceMode { get; set; } = string.Empty;

        /// <summary>
        /// “0”：禁用“1”：启用
        /// </summary>
        public string vibrateMode { get; set; } = string.Empty;

        /// <summary>
        /// 网易云信的ID
        /// </summary>
        public string accid { get; set; } = string.Empty;

        /// <summary>
        /// 网易云信的密码
        /// </summary>
        public string accToken { get; set; } = string.Empty;

        /// <summary>
        /// 工号
        /// </summary>
        public string userNum { get; set; } = string.Empty;

        /// <summary>
        /// 公司代码
        /// </summary>
        public string companyCode { get; set; } = string.Empty;

        /// <summary>
        /// 公司名称
        /// </summary>
        public string companyName { get; set; } = string.Empty;

        /// <summary>
        /// AppKey
        /// </summary>
        public string appkey { get; set; } = string.Empty;

        /// <summary>
        /// App密钥
        /// </summary>
        public string appsecret { get; set; } = string.Empty;

        /// <summary>
        /// 签名
        /// </summary>
        public string signature { get; set; } = string.Empty;

        /// <summary>
        /// 机器人ID
        /// </summary>
        public string robotId { get; set; } = string.Empty;
        /// <summary>
        /// 账号使用状态（0 停用  2正常）
        /// </summary>
        public int status { get; set; } = 2;
    }
}
