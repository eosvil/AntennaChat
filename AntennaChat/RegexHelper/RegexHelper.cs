using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.RegexHelper
{
    public class RegexHelper
    {
        /// <summary>
        /// 判断是否为手机号码
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsMobileNum(string num)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(num, @"^[1]+[34578]+\d{9}");
        }
    }
}
