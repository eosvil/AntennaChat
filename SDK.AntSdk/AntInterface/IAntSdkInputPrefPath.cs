using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk
{
    /// <summary>
    /// 函数输入类型[集成后主要区分输入参数类型为Path的情况，需要输出GetPath(最后一个路径的前缀路径)路径]
    /// </summary>
    internal interface IAntSdkInputPrefPath
    {
        string GetPrefPath();
    }
}
