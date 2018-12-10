using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 更新用户信息输入参数
    /// </summary>
    public class UpdateUserInput: BaseInput
    {
        //public string token { get; set; }//登录返回的有效token
        //public string version { get; set; }//当前app的版本号
        //public string userId { get; set; }//用户Id
        public string picture { get; set; }//用户头像地址
        public int sex { get; set; }//性别
        public string voiceMode { get; set; }//“0”：禁用“1”：启用
        public string vibrateMode { get; set; }//“0”：禁用“1”：启用
        public string signature { get; set; }//个性签名信息
    }
    /// <summary>
    /// 更新用户信息返回参数
    /// </summary>
    public class UpdateUserOut: BaseOutput
    {

    }
}
