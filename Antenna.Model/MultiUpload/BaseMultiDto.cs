using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model.MultiUpload
{
    public class BaseMultiDto
    {
        public string errorCode { set; get; }
        public string errorMsg { set; get; }
        public List<UploadDataContent> data { set; get; }
    }
    public class UploadDataContent
    {
        public string guid { set; get; }
        public string fileld { set; get; }
        public string fileMd5 { set; get; }
        public string dowmnloadUrl { set; get; }
        public string fileName { set; get; }
    }
}
