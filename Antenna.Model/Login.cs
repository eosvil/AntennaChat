using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 登录接口输入参数类
    /// </summary>
    public  class LoginInput
    {
        public int os { get; set; }
        public string osStr { get; set; }
        public string version { get; set; }
        public string loginName { get; set; }
        public string password { get; set; }
        public string manufacture { get; set; }
    }
    /// <summary>
    /// 登录接口返回实体类
    /// </summary>
    public class  LoginOutput: BaseOutput
    {      
        public LoginUserInfo user { get; set; }
    }
    public class LoginUserInfo
    {
        public string userId { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string PWD { get; set; }
        public string token { get; set; }
        public string companyName { get; set; }
        public string companyCode { get; set; }
    }
}
