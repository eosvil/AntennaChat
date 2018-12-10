using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel.Talk
{
    public class BackgroundWorkers: BackgroundWorker
    {
        /// <summary>
        ///消息ID
        /// </summary>
        public string messageId { set; get; }
    }
}
