using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 输入参数基础类（包括result和errorCode字段）
    /// </summary>
    public class BaseInput
    {
        public string token { get; set; }//登录返回的有效token
        public string version { get; set; }//当前app的版本号
        public string userId { get; set; }//用户Id
    }
}
