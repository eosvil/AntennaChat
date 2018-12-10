using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    //[0][0] messageType 消息类型 String 2100：用户离线，2101：用户在线，2102：用户离开，2103：用户忙碌，2109：用户账号被停用
    //[1][0] userId 用户ID String 表示是用户登录或者退出的，终端收到这个消息后，可能需要在本地修改该用户的在线状态。 

    /// <summary>
    /// 用户通知信息基类
    /// </summary>
    public class MsSdkUserBase : SdkMsBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
    }
}
