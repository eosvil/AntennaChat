using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkConfirmPunchInput
    {
        /// <summary>
        /// 打卡ID
        /// </summary>
        public string attendId { get; set; }

        /// <summary>
        /// 用户IP
        /// </summary>
        public string userIp { get; set; }

        /// <summary>
        /// 用户IP
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string passwd { get; set; }
    }
}
