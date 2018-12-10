using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk
{
    /// <summary>
    /// 错误代码类
    /// </summary>
    public class AntSdkErrorCodes : SDK.Service.SdkErrorCodes
    {
        private static AntSdkErrorCodes _instance;

        /// <summary>
        /// 错误代码信息
        /// </summary>
        public new static AntSdkErrorCodes Instanece => _instance ?? (_instance = new AntSdkErrorCodes());
    }
}
