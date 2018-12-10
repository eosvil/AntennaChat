using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.MQTT
{
    internal class PingRequest : Message
    {
        public PingRequest()
            : base(MessageType.PingRequest)
        {
            // NOTE: PingRequest has no variable header and no payload

            VariableHeader = null;
        }

        public override byte[] Payload
        {
            get { return null; }
        }
    }
}
