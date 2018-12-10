using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AntennaChat.Resource
{
    public class AntennaChatHtmlFilter
    {
        public static string RegexHtmlFilter(string html)
        {
            string str = "";
            Regex headhtml = new Regex(@"^(\w|\W)*<body(.*?)>", RegexOptions.IgnoreCase);
            String TempStr = headhtml.Replace(html, "");
            Regex bodyhtml = new Regex(@"^(\w|\W)*<body>", RegexOptions.IgnoreCase);
            TempStr = bodyhtml.Replace(TempStr, "");
            Regex styleHtml = new Regex(@"<style([^>])*>(\w|\W)*?</style([^>])*>", RegexOptions.IgnoreCase);
            TempStr = styleHtml.Replace(TempStr, "");
            Regex RightUserImgRemove = new Regex("<div class=\"rightimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
            TempStr = RightUserImgRemove.Replace(TempStr, "");
            Regex LeftUserImgRemove = new Regex("<div class=\"leftimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
            TempStr = LeftUserImgRemove.Replace(TempStr, "");
            Regex OnceSendImgRemove = new Regex("<div class=\"onceSendFail\".*?>.*?</div>", RegexOptions.IgnoreCase);
            TempStr = OnceSendImgRemove.Replace(TempStr, "");
            return str;
        }
    }
}
