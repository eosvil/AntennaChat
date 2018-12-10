using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
   
    public class AntSdkUserStateOutput
    {
        public string userId { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
    }

    public class AntSdkUserIdsStateInput
    {
        public string[] userIds { get; set; }
    }

    public class AntSdkUserStateInput
    {
        public int state { get; set; } 
    }
}
