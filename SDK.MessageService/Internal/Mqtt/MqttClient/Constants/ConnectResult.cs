using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDK.Service.Mqtt
{
    internal enum ConnectResult
    {
        Accepted = 0,
        BadProtocolVersion = 1,
        IdentifierRejected = 2,
        ServerUnavailable = 3,
        FailedAuthentication = 4,
        NotAuthorized = 5
    }
}
