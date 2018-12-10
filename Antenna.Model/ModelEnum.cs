using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class ModelEnum
    {
        /// <summary>
        /// 图文混合类型枚举
        /// </summary>
        public enum PictureAndTextMixEnum
        {
            /// <summary>
            /// 文本
            /// </summary>
            Text = 1001,
            /// <summary>
            /// 图片
            /// </summary>
            Image = 1002,
            /// <summary>
            /// 换行
            /// </summary>
            LineBreak = 0000,
            /// <summary>
            /// @全体成员
            /// </summary>
            AtAll= 1111,
            /// <summary>
            /// @单人
            /// </summary>
            AtPerson= 1112,
            /// <summary>
            /// @消息
            /// </summary>
            AtMsg= 1008
        }
    }
}
