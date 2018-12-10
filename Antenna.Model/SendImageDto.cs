using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class SendImageDto
    {
        public string picUrl { set; get; }
        public string thumbnailUrl { set; get; }
        public string imageHeight { set; get; }
        public string imageWidth { set; get; }
        public string imgSize { set; get; }
    }
}
