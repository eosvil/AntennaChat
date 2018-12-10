using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class SdkFailOrSucessMessageDto
    {
        public int mtp { set; get; }
        public string content { set; get; } = string.Empty;
        public string sessionid { set; get; } = string.Empty;
        public string lastDatetime { set; get; } = string.Empty;
        public SdkburnMsg.isSendSucessOrFail IsSendSucessOrFail { set; get; }
        public SdkburnMsg.isBurnMsg IsBurnMsg { set; get; }
    }

    public class SdkburnMsg
    {
        public enum isBurnMsg
        {
            yesBurn,
            notBurn
        }

        public enum isSendSucessOrFail
        {
            sucess,
            fail
        }

        public enum IsReadImg
        {
            read,
            notRead
        }
    }
}
