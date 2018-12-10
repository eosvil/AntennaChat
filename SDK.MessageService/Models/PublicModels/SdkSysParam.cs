using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// SDK的系统
    /// </summary>
    public class SdkSysParam
    {
        //是否已经设置相关项
        internal bool SetCompanyCode = false;
        internal bool SetAppKey = false;
        internal bool SetAppSecret = false;
        internal bool SetHttpPrdfix = false;
        internal bool SetLogMode = false;
        internal bool SetUpload = false;
        internal bool SetHeartBeatLog = false;
        internal bool SetHttpTimeOut = false;

        /// <summary>
        /// SDK里的Token
        /// </summary>
        internal string Token { get; set; }

        /// <summary>
        /// 登录SDK的UserId
        /// </summary>
        internal string UserId { get; set; }


        private string _sdkCompanycode = string.Empty;

        /// <summary>
        /// SDK 公司代码
        /// </summary>
        public string Companycode
        {
            get { return _sdkCompanycode; }
            set
            {
                _sdkCompanycode = value;
                SetCompanyCode = true;
            }
        }

        private string _sdkAppkey = string.Empty;

        /// <summary>
        /// SDK AppKey
        /// </summary>
        public string Appkey
        {
            get { return _sdkAppkey; }
            set
            {
                _sdkAppkey = value;
                SetAppKey = true;
            }
        }

        private string _sdkAppsecret = string.Empty;

        /// <summary>
        /// SDK App密钥
        /// </summary>
        public string Appsecret
        {
            get { return _sdkAppsecret; }
            set
            {
                _sdkAppsecret = value;
                SetAppSecret = true;
            }
        }

        private string _sdkHttpprdfix = string.Empty;

        /// <summary>
        /// SDK Http前缀地址
        /// </summary>
        public string HttpPrdfix
        {
            get { return _sdkHttpprdfix; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = $"{value}/";
                }
                //赋值
                _sdkHttpprdfix = value;
                SetHttpPrdfix = true;
            }
        }


        private string _fileUpload = string.Empty;

        /// <summary>
        /// 触角SDK 文件上传地址[获取MQTT参数时获取到]
        /// </summary>
        internal string FileUpload
        {
            get { return _fileUpload; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = $"{value}/";
                }
                //赋值
                _fileUpload = value;
                SetUpload = true;
            }
        }

        /// <summary>
        /// http请求超时时间
        /// </summary>
        private int _httpTimeout = 15000;

        /// <summary>
        /// Http请求超时时间
        /// </summary>
        public int HttpTimeOut
        {
            get
            {
                return _httpTimeout;
            }
            set
            {
                _httpTimeout = value;
                SetHttpTimeOut = true;
            }
        }


        private SdkLogLevel _sdkLogmode;

        /// <summary>
        /// 日志级别模式:SdkLogMode sdklogMode = SdkLogMode.DebugLogEnable | SdkLogMode.ErrorLogEnable | SdkLogMode.FatalLogEnable | SdkLogMode.InfoLogEnable | SdkLogMode.WarnLogEnable;
        /// </summary>
        public SdkLogLevel SdkLogMode
        {
            get { return _sdkLogmode; }
            set
            {
                _sdkLogmode = value;
                if (value.HasFlag(SdkLogLevel.DebugLogEnable))
                {
                    DebugLogEnable = true;
                }
                if (value.HasFlag(SdkLogLevel.InfoLogEnable))
                {
                    InfoLogEnable = true;
                }
                if (value.HasFlag(SdkLogLevel.WarnLogEnable))
                {
                    WarnLogEnable = true;
                }
                if (value.HasFlag(SdkLogLevel.ErrorLogEnable))
                {
                    ErrorLogEnable = true;
                }
                if (value.HasFlag(SdkLogLevel.FatalLogEnable))
                {
                    FatalLogEnable = true;
                }
                SetLogMode = true;
            }
        }

        //日志级别开关
        /// <summary>
        /// 调试日志
        /// </summary>
        internal bool DebugLogEnable { get; private set; }

        /// <summary>
        /// 操作日志
        /// </summary>
        internal bool InfoLogEnable { get; private set; }

        /// <summary>
        /// 警告日志
        /// </summary>
        internal bool WarnLogEnable { get; private set; }

        /// <summary>
        /// 错误日志
        /// </summary>
        internal bool ErrorLogEnable { get; private set; } = true;

        /// <summary>
        /// 严重错误日志
        /// </summary>
        internal bool FatalLogEnable { get; private set; } = true;
    }
}
