using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model.PictureAndTextMix
{
    public class MixMsg
    {
        public string MessageId { set; get; }
        public List<MixMessageTagDto> TagDto { set; get; }
    }
    public class MixMessageTagDto
    {
        public string Path { set; get; }
        public string PreGuid { set; get; }
    }
}
