using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 获取用户信息输入参数
    /// </summary>
   public class UserInput: BaseInput
    {
        //public string token { get; set; }//登录返回的有效token
        //public string version { get; set; }//当前app的版本号
        //public string userId { get; set; }//用户Id
        public string targetUserId { get; set; }//需要获取信息的用户ID
    }
    /// <summary>
    /// 获取用户信息返回参数
    /// </summary>
    public class UserOutput: BaseOutput
    {
        public UserInfo user { get; set; }
    }
    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserInfo
    {
        public string userId { get; set; }//用户Id
        public string userName { get; set; }//用户名
        public string loginName { get; set; }//登录账号（账号或者邮箱）
        public string picture { get; set; }//用户头像地址
        public int sex { get; set; }//0：女 1：男 2：未知
        public string phone { get; set; }//用户手机号
        public string email { get; set; }//邮箱号码
        public string departName { get; set; }//部门名称
        public string departmentId { get; set; }//部门ID
        public string position { get; set; }//职位
        public string companyName { get; set; }//公司名称
        public string qq { get; set; }//qq号码
        public string voiceMode { get; set; }//“0”：禁用“1”：启用
        public string vibrateMode { get; set; }//“0”：禁用“1”：启用
        public string signature { get; set; }//个性签名信息

        public string userNum { get; set; } //工号
    }
}
