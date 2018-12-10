using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Antenna.Framework
{
    public class PinYinHelper
    {
        public static string ToPinYin(string txt)
        {
            txt = txt.Trim();
            if (!InputIsChinese(txt)) return txt;
            byte[] arr = new byte[2]; //每个汉字为2字节  
            StringBuilder result = new StringBuilder();//使用StringBuilder优化字符串连接              
            char[] arrChar = txt.ToCharArray();
            foreach (char c in arrChar)
            {
                arr = System.Text.Encoding.Default.GetBytes(c.ToString());//根据系统默认编码得到字节码  
                if (arr.Length == 1)//如果只有1字节说明该字符不是汉字               
                {
                    result.Append(c.ToString());
                    continue;
                }
                ChineseChar chineseChar = new ChineseChar(c);
                result.Append(chineseChar.Pinyins[0].Substring(0, chineseChar.Pinyins[0].Length - 1).ToLower());
                //result.Append(" ");
            }
            return result.ToString();
        }
        /// <summary>
        /// 处理多音字
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static List<string> ToPinYinList(string txt)
        {
            List<string> resultList = new List<string>();
            txt = txt.Trim();
            if (!InputIsChinese(txt))
            {
                resultList.Add(txt);
                return resultList;
            }
            byte[] arr = new byte[2]; //每个汉字为2字节  
            StringBuilder result = new StringBuilder();//使用StringBuilder优化字符串连接              
            char[] arrChar = txt.ToCharArray();
            foreach (char c in arrChar)
            {
                arr = System.Text.Encoding.Default.GetBytes(c.ToString());//根据系统默认编码得到字节码  
                if (arr.Length == 1)//如果只有1字节说明该字符不是汉字               
                {
                    result.Append(c.ToString());
                    continue;
                }
                ChineseChar chineseChar = new ChineseChar(c);
                for (int i = 0; i < chineseChar.Pinyins.Count; i++)
                {
                    result.Clear();
                    if (chineseChar.Pinyins[i] == null) continue;
                    result.Append(chineseChar.Pinyins[i].Substring(0, chineseChar.Pinyins[i].Length - 1).ToLower());
                    resultList.Add(result.ToString());
                }
            }
            return resultList;
        } 
        /// <summary>
        /// 判断输入的是否是汉字
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool InputIsChinese(string strText)
        {
            bool result = false;
            Regex reg = new Regex("^[\u4e00-\u9fa5]$");//验证是否输入汉字
            for (int i = 0; i < strText.Length; i++)
            {
                if (reg.IsMatch(strText.Substring(i, 1)))
                    result = true;
                else
                    result = false;
            }
            return result;
        }

        public static PingYinModel GetTotalPingYin(string str)
        {
            var chs = str.ToCharArray();
            //记录每个汉字的全拼
            Dictionary<int, List<string>> totalPingYins = new Dictionary<int, List<string>>();
            for (int i = 0; i < chs.Length; i++)
            {
                var pinyins = new List<string>();
                var ch = chs[i];
                //是否是有效的汉字
                if (ChineseChar.IsValidChar(ch))
                {
                    ChineseChar cc = new ChineseChar(ch);
                    pinyins = cc.Pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                }
                else
                {
                    pinyins.Add(ch.ToString());
                }

                //去除声调，转小写
                pinyins = pinyins.ConvertAll(p => Regex.Replace(p, @"\d", "").ToLower());
                //去重
                pinyins = pinyins.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                if (pinyins.Any())
                {
                    totalPingYins[i] = pinyins;
                }
            }
            PingYinModel result = new PingYinModel();
            foreach (var pinyins in totalPingYins)
            {
                var items = pinyins.Value;
                if (result.TotalPingYin.Count <= 0)
                {
                    result.TotalPingYin = items;
                    result.FirstPingYin = items.ConvertAll(p => p.Substring(0, 1)).Distinct().ToList();
                }
                else
                {
                    //全拼循环匹配
                    var newTotalPingYins = new List<string>();
                    foreach (var totalPingYin in result.TotalPingYin)
                    {
                        newTotalPingYins.AddRange(items.Select(item => totalPingYin + item));
                    }
                    newTotalPingYins = newTotalPingYins.Distinct().ToList();
                    result.TotalPingYin = newTotalPingYins;

                    //首字母循环匹配
                    var newFirstPingYins = new List<string>();
                    foreach (var firstPingYin in result.FirstPingYin)
                    {
                        newFirstPingYins.AddRange(items.Select(item => firstPingYin + item.Substring(0, 1)));
                    }
                    newFirstPingYins = newFirstPingYins.Distinct().ToList();
                    result.FirstPingYin = newFirstPingYins;
                }
            }
            return result;
        }
    }
    public class PingYinModel
    {
        public PingYinModel()
        {
            TotalPingYin = new List<string>();
            FirstPingYin = new List<string>();
        }

        //全拼
        public List<string> TotalPingYin { get; set; }

        //首拼
        public List<string> FirstPingYin { get; set; }
    }
}
