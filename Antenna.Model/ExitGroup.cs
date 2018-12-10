using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 退出讨论组/解散讨论组输入参数实体
    /// </summary>
    public class ExitGroupInput : BaseInput
    {
        public string groupId { get; set; }
    }
}
