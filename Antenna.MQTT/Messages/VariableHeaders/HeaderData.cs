using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Antenna.MQTT
{
    public abstract class HeaderData
    {
        public abstract byte[] Serialize();
    }
}
