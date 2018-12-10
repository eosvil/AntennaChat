using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class ReceiveOrUploadFileDto
    {
        public string fileUrl { set; get; }
        public string progressId { set; get; }
        public string fileGuid { set; get; }
        public string fileName { set; get; }
        public string Size { set; get; }
        public string localOrServerPath { set; get; }
        public string fileExtendName { set; get; }
        public string cmpcd { set; get; }
        public string seId { set; get; }
        public string fileImgGuid { set; get; }
        public string fileTextGuid { set; get; }
        public string fileOpenGuid { set; get; }
        public string fileDirectoryGuid { set; get; }
        public string guid { set; get; }
        public string haveDownFile { get; set; }

        public string chatIndex { set; get; }
        public string flag { set; get; }
        public string messageId { set; get; }
        public string downloadPath { get; set; }
    }
}
