using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 心跳消息 (该接口已经被废止)
    /// </summary>
    /// 创建者：赵雪峰 20160910
    public class Heartbeat
    {
        public Ctt ctt { get; set; } = new Ctt();
    }
    public class Ctt
    {
        public string token { get; set; }
    }
}
