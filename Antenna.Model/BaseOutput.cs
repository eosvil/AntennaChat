using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 输出参数基础类（包括result和errorCode字段）
    /// </summary>
    public class BaseOutput
    {
        public int result { get; set; }
        public string errorCode { get; set; }
    }
}
