using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取MQTT连接参数（输入参数）
    /// </summary>
    internal class GetConnectConfigInput
    {
        /// <summary>
        /// 0:表示运行多端登录，1：允许一端登录
        /// </summary>
        public int ways { get; set; }
    }

    /// <summary>
    /// 获取MQTT连接参数（输出参数）
    /// </summary>
    internal class GetConnectConfigOutput
    {
        /// <summary>
        /// 连接的用户名
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 连接的密码
        /// </summary>
        public string password { get; set; } = string.Empty;

        /// <summary>
        /// clientId
        /// </summary>
        public string clientId { get; set; } = string.Empty;

        /// <summary>
        /// 连接的地址
        /// </summary>
        public string host { get; set; } = string.Empty;

        /// <summary>
        /// 连接端口号
        /// </summary>
        public string port { get; set; } = string.Empty;

        /// <summary>
        /// 上传文件地址
        /// </summary>
        public string fileUploadUrl { get; set; } = string.Empty;

        /// <summary>
        /// 消息服务器地址
        /// </summary>
        public string antMessageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 可选择参数
        /// </summary>
        public List<string> topicIds { get; set; }
    }
}
