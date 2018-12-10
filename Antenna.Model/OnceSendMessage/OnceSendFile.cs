using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Antenna.Model.SendMessageDto;

namespace Antenna.Model.OnceSendMessage
{
    public class OnceSendFile
    {
        public SendMessage_ctt ctt { set; get; }
        public AntSdkGroupInfo GroupInfo { set; get; }
    }
}
