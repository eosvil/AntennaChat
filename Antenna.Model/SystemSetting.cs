using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    /// <summary>
    /// 系统设置信息
    /// </summary>
    public class SystemSetting
    {
        /// <summary>
        /// 截图快捷键
        /// </summary>
        public string KeyShortcuts;
        /// <summary>
        /// 发送消息快捷键 0-按Enter键发送消息  1-按Ctrl+Enter键发送消息
        /// </summary>
        public int SendKeyType = 0;
    }
}
