using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class LogConfigInfo
    {
        //日志级别开关
        public bool DebugLogEnable { get; set; }
        public bool InfoLogEnable { get; set; }
        public bool WarnLogEnable { get; set; } 
        public bool ErrorLogEnable { get; set; }
        public bool FatalLogEnable { get; set; }
    }
}
