using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Antenna.Model.ModelEnum;

namespace Antenna.Model.PictureAndTextMix
{
    public class PictureAndTextMixDto:PictureAndTextMixBaseDto
    {
        /// <summary>
        /// 类型 1001(文本(包含表情)、1002(图片)、0000(换行符)
        /// </summary>
        public PictureAndTextMixEnum type { set; get; }
        /// <summary>
        /// 图片MD5
        /// </summary>
        public string ImgGuid { set; get; }
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImgPath { set; get; }
        /// <summary>
        /// 图片ID
        /// </summary>
        public string ImgId { set; get; }
        /// <summary>
        /// At存放
        /// </summary>
        public object obj { set; get; }

        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 被@人的ID(数组)
        /// </summary>
        public List<string> ids { get; set; }

        /// <summary>
        /// 被@人的名字(数组)
        /// </summary>
        public List<string> names { get; set; }

    }
    public class PictureAndTextMixStringDto: PictureAndTextMixBaseDto
    {
        /// <summary>
        /// 类型 1001(文本(包含表情)、1002(图片)、0000(换行符)
        /// </summary>
        public string type { set; get; }
    }
}
