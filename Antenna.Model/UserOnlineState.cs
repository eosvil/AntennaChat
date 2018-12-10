using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class UserOnlineState
    {
        /// <summary>
        /// 状态图片
        /// </summary>
        public string StateImage { get; set; }
        /// <summary>
        /// 状态类型(（0、离线， 1、在线，2、忙碌，3 离开,4 手机端在线）)
        /// </summary>
        public int OnlineState { get; set; }
        /// <summary>
        /// 状态内容
        /// </summary>
        public string StateContent { get; set; }
    }
}
