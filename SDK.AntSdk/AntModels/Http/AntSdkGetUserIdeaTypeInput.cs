using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkGetUserIdeaTypeInput : AntSdkBaseInput, IAntSdkInputQuery
    {
        public string GetQuery() => $"?version={version}";
    }
}
