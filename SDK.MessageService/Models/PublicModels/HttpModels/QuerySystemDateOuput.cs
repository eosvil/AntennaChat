using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 获取当前系统时间
    /// </summary>
    public class QuerySystemDateOuput
    {
        /// <summary>
        /// 当前系统时间
        /// </summary>
        public string systemCurrentTime { get; set; } = string.Empty;
    }
}
