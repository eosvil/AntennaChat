using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class ChangeGroupAdminIn
    {
        public string token { get; set; }
        public string version { get; set; }
        public string groupId { get; set; }
        public string userId { get; set; }
        public string newManagerId { get; set; }
    }

    public class ChangeGroupAdminOut
    {
        public int result { get; set; }
        public string errorCode { get; set; }
    }
}
