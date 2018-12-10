using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SDK.Service.Mqtt
{
    internal abstract class HeaderData
    {
        public abstract byte[] Serialize();
    }
}
