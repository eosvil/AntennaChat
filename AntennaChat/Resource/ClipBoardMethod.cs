using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AntennaChat.Resource
{
    /// <summary>
    /// 控制面板操作类 2017-09-07
    /// </summary>
    public class ClipBoardMethod
    {
        /// <summary>
        /// 拷贝Html内容到剪切板
        /// </summary>
        /// <param name="html">html内容</param>
        public static void CopyHtmlToClipBoard(string html)
        {
            Encoding encoding = Encoding.UTF8;

            string begin = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

            string html_begin = "<html>\r\n<head>\r\n"
               + "<meta http-equiv=\"Content-Type\""
               + " content=\"text/html; charset=" + encoding.WebName + "\">\r\n"
               + "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
               + "<!--StartFragment-->";

            string html_end = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";

            string begin_sample = String.Format(begin, 0, 0, 0, 0);

            int count_begin = encoding.GetByteCount(begin_sample);
            int count_html_begin = encoding.GetByteCount(html_begin);
            int count_html = encoding.GetByteCount(html);
            int count_html_end = encoding.GetByteCount(html_end);

            string html_total = String.Format(
               begin
               , count_begin
               , count_begin + count_html_begin + count_html + count_html_end
               , count_begin + count_html_begin
               , count_begin + count_html_begin + count_html
               ) + html_begin + html + html_end;

            DataObject obj = new DataObject();
            obj.SetData(DataFormats.Html, new System.IO.MemoryStream(
               encoding.GetBytes(html_total)));
            Clipboard.SetDataObject(obj, true);
        }
    }
}
