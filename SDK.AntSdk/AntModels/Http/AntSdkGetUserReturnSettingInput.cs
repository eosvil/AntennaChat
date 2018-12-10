using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取用户回复用语设置输入
    /// </summary>
    public class AntSdkGetUserReturnSettingInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        /// <summary>
        /// 1：回复用语(现在个人设置只有一个信息，所有这个传固定的值1 就可以了，参考 通用返回码 [用户个人配置类型])
        /// </summary>
        public int configType { get; set; }

        /// <summary>
        /// 获取Query型参数对应的路径信息
        /// </summary>
        string IAntSdkInputQuery.GetQuery() => $"?configType={configType}&version={version}";
    }
}
