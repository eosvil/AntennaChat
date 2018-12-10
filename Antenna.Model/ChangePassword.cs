using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class ChangePasswordInput
    {
        public string token { get; set; }
        public string version { get; set; }
        public string userId { get; set; }
        public string oldPwd { get; set; }
        public string newPwd { get; set; }
    }

    public class ChangePasswordOutput
    {
        public int result { get; set; }
        public string errorCode { get; set; }
    }
}
