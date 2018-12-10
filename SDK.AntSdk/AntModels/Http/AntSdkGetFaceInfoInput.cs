using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 获取表情输入
    /// </summary>
    public class AntSdkGetFaceInfoInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        public string GetQuery() => $"?version={version}";
    }
}
