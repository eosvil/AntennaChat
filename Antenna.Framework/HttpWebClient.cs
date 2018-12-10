using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using Antenna.Model;

namespace Antenna.Framework
{
    public class HttpWebClient<T>:WebClient
    {
        public T obj;
        public string guid;
        public string userId;
        public SendMessageDto.SendMessage_ctt s_ctt;
        public BackgroundWorker backGround;
    }
}
