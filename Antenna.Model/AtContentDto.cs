using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class AtContentDto
    {
        public string mtp { set; get; }
        public contnets ctt { set; get; }
    }

    public class contnets
    {
        public string content { set; get; }
        public List<object> ids { set; get; }
    }

    public class AtIds
    {
        public string name { set; get; }
    }

    public class AtIdsName:AtIds
    {
        public string id { set; get; }
    }
}
