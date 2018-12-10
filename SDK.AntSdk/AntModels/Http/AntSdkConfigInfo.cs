using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK配置信息
    /// </summary>
    public class AntSdkConfig
    {
        //是否已经设置相关项
        internal bool SetCompanyCode = false;
        internal bool SetAppKey = false;
        internal bool SetAppSecret = false;
        internal bool SetHttpPrdfix = false;
        internal bool SetServiceHttpPrdfix = false;
        internal bool SetCustomersHttpPrdfix = false;
        internal bool SetLogMode = false;
        internal bool SetDataBaseAddress = false;

        /// <summary>
        /// 客户端App版本
        /// </summary>
        public string AppVersion { get; set; }

        private string _antsdkCompanycode = string.Empty;

        /// <summary>
        /// 触角SDK 公司代码[可输：如果设置则不读取配置信息，否则读取IM SDK配置信息]
        /// </summary>
        public string AntSdkCompanyCode
        {
            get { return _antsdkCompanycode; }
            set
            {
                _antsdkCompanycode = value;
                SetCompanyCode = true;
            }
        }

        private string _antsdkAppkey = string.Empty;

        /// <summary>
        /// 触角SDK AppKey[可输：如果设置则不读取配置信息，否则读取IM SDK配置信息]
        /// </summary>
        public string AntSdkAppKey
        {
            get { return _antsdkAppkey; }
            set
            {
                _antsdkAppkey = value;
                SetAppKey = true;
            }
        }

        private string _antsdkAppsecret = string.Empty;

        /// <summary>
        /// 触角SDK App密钥[可输：如果设置则不读取配置信息，否则读取IM SDK配置信息]
        /// </summary>
        public string AntSdkAppSecret
        {
            get { return _antsdkAppsecret; }
            set
            {
                _antsdkAppsecret = value;
                SetAppSecret = true;
            }
        }

        private string _antsdkHttpprdfix = string.Empty;

        /// <summary>
        /// 触角SDK Http前缀地址[可输：如果设置则不读取配置信息，否则读取IM SDK配置信息]
        /// </summary>
        public string AntSdkHttpPrdfix
        {
            get { return _antsdkHttpprdfix; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = $"{value}/";
                }
                //赋值
                _antsdkHttpprdfix = value;
                SetHttpPrdfix = true;
            }
        }

        private string _antserviceHttpprdfix = string.Empty;

        /// <summary>
        /// 触角服务 HTTP前缀地址[必输项]
        /// </summary>
        public string AntServiceHttpPrdfix
        {
            get { return _antserviceHttpprdfix; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = $"{value}/";
                }
                //赋值
                _antserviceHttpprdfix = value;
                SetServiceHttpPrdfix = true;
            }
        }

        private string _customersHttpprdfix = string.Empty;
        
        /// <summary>
        /// 客户系统登录地址前缀
        /// </summary>
        public string CustomersHttpPrdfix
        {
            get { return _customersHttpprdfix; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = $"{value}/";
                }
                //赋值
                _customersHttpprdfix = value;
                SetCustomersHttpPrdfix = true;
            }
        }

        private AntSdkLogLevel _antsdkLogmode;

        /// <summary>
        /// 日志级别模式[默认不记录任何日志]:日志级别模式:AntSdkLogLevel sdklogMode = AntSdkLogLevel.DebugLogEnable | AntSdkLogLevel.ErrorLogEnable | AntSdkLogLevel.FatalLogEnable | AntSdkLogLevel.InfoLogEnable | AntSdkLogLevel.WarnLogEnable;
        /// </summary>
        public AntSdkLogLevel AntSdkLogMode
        {
            get { return _antsdkLogmode; }
            set
            {
                _antsdkLogmode = value;
                SetLogMode = true;
            }
        }

        private string _antsdkdatabaseAddress = string.Empty;

        public string AntSdkDatabaseAddress
        {
            get { return _antsdkdatabaseAddress; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith(@"\"))
                {
                    value = $@"{value}\";
                }
                //赋值
                _antsdkdatabaseAddress = value;
                SetDataBaseAddress = true;
            }
        }

        private string _antsdkfileUpload = string.Empty;

        /// <summary>
        /// 文件上传接口及方法
        /// </summary>
        public string AntSdkFileUpload
        {
            get { return _antsdkfileUpload; }
            set
            {
                _antsdkfileUpload = value;
                if (!string.IsNullOrEmpty(value) && !value.EndsWith(@"/"))
                {
                    value = $@"{value}/";
                }
                _antsdkfileUpload = $"{value}v1/file/upload";
            }
        }
        public string _antSdkMultiFileUpload = string.Empty;
        /// <summary>
        /// 多文件上传接口
        /// </summary>
        public string AntSdkMultiFileUpload
        {
            get { return _antSdkMultiFileUpload; }
            set
            {
                _antSdkMultiFileUpload = value;
                if(!string.IsNullOrEmpty(value)&&!value.EndsWith(@"/"))
                {
                    value = "$@{ value}/";
                }
                _antSdkMultiFileUpload = $"{ value}/v1/file/batch/upload";
            }
        }
    }
}
