using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AntennaChat.ViewModel.Talk
{
    public class MediaPlayers: MediaPlayer
    {
        /// <summary>
        /// 图片播放Id
        /// </summary>
        public string IsImgPlayId { set; get; }
        /// <summary>
        /// 消息Id
        /// </summary>
        public string messageId { set; get; }
        /// <summary>
        /// 左边gif或者是右边gif 0为左边 1为右边
        /// </summary>
        public string isLeftOrRight { set; get; }
    }
}
