using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class BurnContent
    {
        /// <summary>
        /// 阅后即焚模式 501 yes 502 no
        /// </summary>
        public string type { set; get; }
        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { set; get; }
    }
}
