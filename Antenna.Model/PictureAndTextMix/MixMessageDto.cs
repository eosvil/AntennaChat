using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model.PictureAndTextMix
{
    //public class AtContent:MixMessageBase
    //{
    //    public List<string> ids { set; get; }
    //    public List<string> names { set; get; }
    //}
    public class PictureDto
    {
        public string picUrl { set; get; }
        public string width { set; get; }
        public string height { set; get; }
    }
    public class MixMessageBase
    {
        public string type { set; get; }
    }
    public class MixMessageDto:MixMessageBase
    {
        public string content { set; get; }
    }
    public class MixMessageObjDto:MixMessageBase
    {
        public object content { set; get; }
    }

    public class ATMessage
    {
        public string[] ids { get; set; }
        public string[] names { get; set; }
        public string type { get; set; }
    }
}
