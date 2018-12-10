using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// SDK里获取token
    /// </summary>
    internal class GetTokenInput
    {
        /// <summary>
        /// 在创建app时生成的appSecret 
        /// </summary>
        public string appSecret { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID 或者 服务器ID
        /// </summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// sdk 类型 1:PC，2：WEB，3：ANDROID，4：IOS，5：JAVA SERVER
        /// </summary>
        public int sdkType { get; set; }

        /// <summary>
        /// SDK版本信息
        /// </summary>
        public string version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    /// <summary>
    /// SDK里获取token
    /// </summary>
    internal class GetTokenOutput
    {
        /// <summary>
        /// sdk 第一次调用接口的时候必须用到，其他的接口都需要这个接口返回的token 值作为参数
        /// </summary>
        public string token { get; set; } = string.Empty;
    }
}
