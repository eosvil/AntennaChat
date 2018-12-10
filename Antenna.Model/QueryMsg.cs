using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 消息查询(讨论组)
    /// </summary>
    public class QueryMsgInput
    {
        public string token { get; set; }
        public string param { get; set; }
    }
    public class QueryMsgInput_Param
    {
        public string token { get; set; }//登录返回的有效token
        public string companyCode { get; set; }
        public string sendUserId { get; set; }
        public int os { get; set; }
        public List<QueryMsgInput_Group> groups { get; set; }
    }
    public class QueryMsgInput_Group
    {
        public string sessionId { get; set; }
        public string chatIndex { get; set; }
    }
}
