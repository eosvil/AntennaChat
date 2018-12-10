using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model.PictureAndTextMix
{
    public class PictureAndTextMixTypeBaseDto
    {
        /// <summary>
        /// 类型 1001(文本(包含表情)、1002(图片)、0000(换行符)
        /// </summary>
        public string type { set; get; }
    }
}
