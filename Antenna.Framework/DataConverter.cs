using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.Win32;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Antenna.Framework
{
    /// <summary>
    /// 数据转换类
    /// </summary>
    /// 创建者:赵雪峰  2016-05-20
    public class DataConverter
    {
        /// <summary>
        /// 将类序列化为XML文件
        /// </summary>
        /// 作者：赵雪峰  20160523
        /// <param name="fileName">xml文件完整路径</param>
        /// <param name="entity">实体</param>
        public static bool EntityToXml<T>(String xmlPath, T entity, ref string errMsg)
        {
            try
            {
                if (!File.Exists(xmlPath))
                {
                    FileStream fs = File.Create(xmlPath);//创建该文件
                    fs.Close();
                }
                XmlSerializer s = new XmlSerializer(entity.GetType());
                using (TextWriter myWriter = new StreamWriter(xmlPath))
                {
                    s.Serialize(myWriter, entity);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[DataConverter_EntityToXml]:" + ex.Message + "," + ex.StackTrace);
                errMsg += ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将XML解析到实体中
        /// </summary>
        /// 作者：赵雪峰  20160523
        /// <typeparam name="T">泛型实体类型</typeparam>
        /// <param name="fileName">xml文件完整路径</param>
        /// <returns>实体</returns>
        public static bool XmlToEntity<T>(String xmlPath, ref T entity, ref string errMsg)
        {
            if (!File.Exists(xmlPath))
            {
                errMsg = "XML文件" + xmlPath + "不存在";
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(xmlPath, FileMode.Open))
                {
                    if (fs.Length > 0)
                    {
                        XmlSerializer xs = new XmlSerializer(entity.GetType());
                        entity = (T)xs.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[DataConverter_XmlToEntity]:" + ex.Message + "," + ex.StackTrace);
                errMsg += ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 实体转换成JSON字符串
        /// </summary>
        /// 作者：赵雪峰  20160520
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="t">实体参数</param>
        /// <returns>JSON字符串</returns>
        public static bool SerializeJson(object entity, ref string json, ref string errMsg, NullValueHandling nullValueHandling = NullValueHandling.Include)
        {
            try
            {
                var jSetting = new JsonSerializerSettings { NullValueHandling = nullValueHandling };
                json = JsonConvert.SerializeObject(entity, jSetting);
                //JavaScriptSerializer jsonSer = new JavaScriptSerializer();
                //json = jsonSer.Serialize(t);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[DataConverter_SerializeJson]:" + e.Message + "," + e.StackTrace);
                errMsg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// JSON字符串转换成实体
        /// </summary>
        /// 作者：赵雪峰  20160520
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>格式化后的实体</returns>
        public static bool DeserializeJson<T>(string json, ref T entity, ref string errMsg)
        {
            try
            {
                entity = JsonConvert.DeserializeObject<T>(json);
                if (entity == null)
                {
                    errMsg = string.Format("数据格式错误，将JOSN【{0}】转化为实体失败", json);
                    LogHelper.WriteError("[DataConverter_DeserializeJson]:" + errMsg);
                    return false;
                }
                // jsonSer = new JavaScriptSerializer();
                //entity = jsonSer.Deserialize<T>(json);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[DataConverter_DeserializeJson]:" + e.Message + "," + e.StackTrace);
                errMsg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 根据键值获取Json字符串中特定字段的值
        /// </summary>
        /// 作者：赵雪峰  20160523
        /// <param name="key">键值</param>
        /// <param name="json">json字符串</param>
        /// <param name="value">值</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否执行成功</returns>
        public static bool GetValueByJsonKey(string key, string json, ref string value, ref string errMsg)
        {
            try
            {
                var jObject = JObject.Parse(json);
                bool hasErr = jObject.Properties().Any(p => p.Name == key);
                IEnumerable<JProperty> properties = jObject.Properties();
                if (hasErr)
                {
                    foreach (JProperty item in properties)
                    {
                        if (item.Name == key)
                        {
                            value = item.Value.ToString();

                            break;
                        }
                    }
                    return true;
                }
                //value = jObject[key].ToString();
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(string.Format("[DataConverter.GetValueByJsonKey]:key={0},json={1},error={2}", key, json, ex.Message));
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 时间格式化_秒转换为分秒
        /// </summary>
        /// 作者：赵雪峰  20160526
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string FormatTimeBySeconds(int seconds)
        {
            TimeSpan ts = new TimeSpan(0, 0, seconds);
            string ret = string.Empty;
            if ((int)ts.TotalHours > 0)
            {
                ret += (int)ts.TotalHours * 60 + ts.Minutes + "'";
            }
            else if (ts.Minutes > 0)
            {
                ret += ts.Minutes + "'";
            }
            ret += ts.Seconds.ToString().PadLeft(2, '0') + "\"";
            return ret;
        }
        /// <summary>
        /// 秒转换成时分秒
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string FormatTimeHourBySeconds(int seconds)
        {
            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(seconds));
            string str = "";
            if (ts.Hours > 0)
            {
                str = ts.Hours.ToString().PadLeft(2, '0') + ": " + ts.Minutes.ToString().PadLeft(2, '0') + ": " + ts.Seconds.ToString().PadLeft(2, '0');
            }
            if (ts.Hours == 0 && ts.Minutes > 0)
            {
                str =  "00: " + ts.Minutes.ToString().PadLeft(2, '0') + ": " + ts.Seconds.ToString().PadLeft(2, '0');
            }
            if (ts.Hours == 0 && ts.Minutes == 0)
            {
                str = "00: 00: " + ts.Seconds.ToString().PadLeft(2, '0');
            }
            return str;
        }
        /// <summary>
        /// 时间格式化_分秒转换成秒
        /// </summary>
        /// 作者：赵雪峰  20160526
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static int FormatTimeToSeconds(string time)
        {
            int seconds = 0;
            if (time.Contains("'"))
            {
                seconds += int.Parse(time.Substring(0, time.IndexOf("'"))) * 60;
                time = time.Substring(time.IndexOf("'") + 1);
            }
            seconds += int.Parse(time.Substring(0, time.Length - 1));
            return seconds;
        }

        /// <summary>
        /// 将集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns></returns>
        public static DataTable ToDataTable(IList list)
        {
            DataTable result = new DataTable();
            if (list == null)
                return result;
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }

        /// <summary>
        /// 格式化特殊字符
        /// </summary>
        /// 作者：赵雪峰 20160706
        /// <param name="sourceStr"></param>
        /// <returns></returns>
        public static string FormatSpecialCharacter(string sourceStr)
        {
            sourceStr = sourceStr.Replace("&#160;", " ");
            sourceStr = sourceStr.Replace("&lt;", "<");
            sourceStr = sourceStr.Replace("&gt;", ">");
            sourceStr = sourceStr.Replace("&amp;", "&");
            return sourceStr;
        }

        /// <summary>
        /// 时间戳转换为时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime GetTimeByTimeStamp(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 时间戳转换为格式化后的时间字符串
        /// 当天的消息展示几点几分（xx:xx） ，昨天的消息展示昨天，一周内的展示周x，超过一周的展示x月/x日,非今年的现实 年/月/日
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string FormatTimeByTimeStamp(string timeStamp)
        {
            if (string.IsNullOrEmpty(timeStamp)) return string.Empty;
            DateTime timeOld = GetTimeByTimeStamp(timeStamp);
            DateTime timeNow = DateTime.Now;
            int yearDiff = timeNow.Year - timeOld.Year;
            int monDiff = timeNow.Month - timeOld.Month;
            int dayDiff = timeNow.Day - timeOld.Day;
            if (yearDiff > 0)
                return timeOld.ToString("yy/MM/dd");
            if (monDiff < 1)
            {
                if (dayDiff < 1 && dayDiff > -1)//当天
                    return timeOld.ToString("HH:mm");
                if (dayDiff == 1)//昨天
                    return "昨天";
                if (dayDiff > 1 && dayDiff < 7)//一周
                    return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(timeOld.DayOfWeek);
            }
            return timeOld.ToString("MM/dd");
        }

        /// <summary>
        /// 创建时间转换为格式化后的时间字符串
        /// 当天的消息展示几点几分（xx:xx） ，昨天的消息展示昨天，一周内的展示周x，超过一周的展示x月/x日,非今年的现实 年/月/日
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string FormatTimeByCreateTime(string createTime)
        {
            if (string.IsNullOrEmpty(createTime)) return string.Empty;
            DateTime timeOld = Convert.ToDateTime(createTime);
            DateTime timeNow = DateTime.Now;
            int yearDiff = timeNow.Year - timeOld.Year;
            int monDiff = timeNow.Month - timeOld.Month;
            int dayDiff = timeNow.Day - timeOld.Day;
            int hourDiff = timeNow.Hour - timeOld.Hour;
            int minuteDiff = timeNow.Minute - timeOld.Minute;
            //if (yearDiff > 0)
            //    return timeOld.ToString("yyyy-M-d");
            if (yearDiff < 1)
            {
                if (dayDiff == 0 && monDiff == 0 && hourDiff == 0 && minuteDiff <= 3)
                    return "刚刚";
                if (dayDiff < 1 && dayDiff > -1)//当天
                    return timeOld.ToString("HH:mm");
                if (dayDiff > 1)//昨天
                    return timeOld.ToString("MM-dd HH:mm");

                //if (dayDiff > 1 && dayDiff < 7)//一周
                //    return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(timeOld.DayOfWeek);
            }
            else
            {
                return timeOld.ToString("yyyy-M-d");
            }
            return timeOld.ToString("MM-dd");
        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name=”time”></param>
        /// <returns></returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        /// <summary>
        /// 邮箱校验
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool EmailCheck(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
        /// <summary>
        /// 数字校验
        /// </summary>
        /// <param name="telephone"></param>
        /// <returns></returns>
        public static bool NumberCheck(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^[0-9]+$");
        }

        /// <summary>
        /// 根据UserID生成SessionId(点对点聊天,两个参数位置可互换）
        /// </summary>
        /// <param name="sendUserId">发送者UserId</param>
        /// <param name="targetId">目标UserId</param>
        /// <returns>生成的SessionId</returns>
        //public static string GetSessionID(string sendUserId, string targetId)
        //{
        //    string sourceStr = string.Empty;
        //    if (string.Compare(sendUserId, targetId) > 0)
        //    {
        //        sourceStr = targetId + sendUserId;
        //    }
        //    else
        //    {
        //        sourceStr = sendUserId + targetId;
        //    }
        //    return MD5Encrypt(sourceStr, Encoding.UTF8);
        //}

        public static int compare(string sendUserId, string targetId)
        {
            int result = 1;
            sendUserId = Regex.Replace(sendUserId, "[^0-9A-Za-z]", "");
            targetId = Regex.Replace(targetId, "[^0-9a-zA-Z]", "");
            if (sendUserId.Length > targetId.Length)
            {
                result = 1;
            }
            else if (sendUserId.Length < targetId.Length)
            {
                result = -1;
            }
            else
            {
                char[] charArray = sendUserId.ToCharArray();
                char[] charArray2 = targetId.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    if (charArray[i] > charArray2[i])
                    {
                        result = 1;
                        break;
                    }
                    else if (charArray[i] < charArray2[i])
                    {
                        result = -1;
                        break;
                    }
                }
            }
            return result;
        }
        public static string GetSessionID(string sendUserId, string targetId)
        {
            string sessionId = null;
            if (compare(sendUserId, targetId) > 0)
            {
                sessionId = targetId + sendUserId;
            }
            else
            {
                sessionId = sendUserId + targetId;
            }
            sessionId = MD5Encrypt(sessionId, Encoding.UTF8);
            return sessionId;
        }

        /// <summary>
        /// 更加讨论组ID生成SessionId(讨论组)
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        //public static string GetSessionID(string groupId)
        //{
        //    return MD5Encrypt(groupId, Encoding.UTF8);
        //}
        /// <summary>
        /// 删除非空文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectory(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    DirectoryInfo[] childs = dir.GetDirectories();
                    foreach (DirectoryInfo child in childs)
                    {
                        child.Delete(true);
                    }
                    dir.Delete(true);
                }
            }
            catch (Exception e)
            { }
        }
        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="sourceImg"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static System.Drawing.Image GetImageThumb(System.Drawing.Image sourceImg, int width, int height)
        {
            System.Drawing.Image targetImg = new System.Drawing.Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(targetImg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(sourceImg, new System.Drawing.Rectangle(0, 0, width, height), new System.Drawing.Rectangle(0, 0, sourceImg.Width,
                    sourceImg.Height), System.Drawing.GraphicsUnit.Pixel);
                g.Dispose();
            }
            return targetImg;
        }
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// bitmap转换为bitmapsource 以适应wpf的image
        /// </summary>
        /// <param name="pic"></param>
        /// <returns></returns>
        public static BitmapSource GetMapSource(Bitmap pic)
        {
            IntPtr ip = pic.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }
        /// <summary>
        /// [颜色：16进制转成RGB]
        /// </summary>
        /// <param name="strColor">设置16进制颜色(#FFFFFF) </param>
        /// <returns></returns>
        public static System.Drawing.Color ColorHx16toRGB(string strHxColor)
        {
            return System.Drawing.Color.FromArgb(System.Int32.Parse(strHxColor.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier), System.Int32.Parse(strHxColor.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier), System.Int32.Parse(strHxColor.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
        }
        /// <summary>
        /// 根据给的汉字获得起首拼音字母
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string GetChineseSpell(string strText)
        {
            int len = strText.Length;
            string myStr = "";
            for (int i = 0; i < len; i++)
            {
                string pinyin = PinYinHelper.ToPinYin(strText.Substring(i, 1));
                if (string.IsNullOrEmpty(pinyin)) continue;
                myStr += pinyin.Substring(0, 1); //GetSpell(strText.Substring(i, 1));
            }
            return myStr.ToLower();
        }
        /// <summary>
        /// 根据给的汉字获得起首拼音字母数组
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static List<string> GetChineseSpellList(string strText)
        {
            var pingyins = PinYinHelper.GetTotalPingYin(strText);
            return pingyins?.FirstPingYin;
        }
        /// <summary>
        /// 根据一个汉字获得其首拼音
        /// </summary>
        /// <param name="cnChar"></param>
        /// <returns></returns>
		public static string GetSpell(string cnChar)
        {
            byte[] arrCN = Encoding.Default.GetBytes(cnChar);
            if (arrCN.Length > 1)
            {
                int area = (short)arrCN[0];
                int pos = (short)arrCN[1];
                int code = (area << 8) + pos;
                int[] areacode = { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481 };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25) max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                    {
                        return Encoding.Default.GetString(new byte[] { (byte)(65 + i) });
                    }
                }
                return "*";
            }
            else return cnChar;
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

        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool InputIsNum(string strText)
        {
            int num = 0;
            return int.TryParse(strText, out num);
        }
        #region MD5加密/解密相关方法
        /// <summary>
        /// 32位MD5加密(不带秘钥)
        /// </summary>
        /// 作者：赵雪峰  20160523
        /// <param name="input">需加密字符串</param>
        /// <param name="encode">编码格式</param>
        /// <returns>加密后字符串</returns>
        public static String MD5Encrypt(String input, Encoding encode)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] t = md5.ComputeHash(encode.GetBytes(input));

            StringBuilder sb = new StringBuilder(32);

            for (int i = 0; i < t.Length; i++)

                sb.Append(t[i].ToString("x").PadLeft(2, '0'));

            return sb.ToString();
        }

        /// <summary>
        /// 创建Key
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }
        /// <summary>
        /// 16位MD5加密 （带秘钥）       
        /// </summary>
        /// <returns></returns>
        public static string MD5Encrypt(string pToEncrypt, string sKey)
        {
            if (string.IsNullOrWhiteSpace(sKey)) sKey = "?\rV?\u0015\u001feD";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();


        }

        /// <summary>
        /// 16位MD5解密 （带秘钥）       
        /// </summary>
        /// <returns></returns>
        public static string MD5Decrypt(string pToDecrypt, string sKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sKey)) sKey = "?\rV?\u0015\u001feD";
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
                for (int x = 0; x < pToDecrypt.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                StringBuilder ret = new StringBuilder();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 集合转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ConvertToObservableCollection<T>(IEnumerable<T> from)
        {
            ObservableCollection<T> to = new ObservableCollection<T>();
            foreach (var f in from)
            {
                to.Add(f);
            }
            return to;
        }
        #endregion

        #region AES加密/解密相关方法 add by 赵雪峰 20160905
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="toEncrypt">加密前字符串</param>
        /// <param name="key">秘钥</param>
        /// <param name="iv">偏移量</param>
        /// <returns>加密后字符串</returns>
        public static string AESEncrypt(string toEncrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.BlockSize = 128;
            rDel.KeySize = 256;
            rDel.FeedbackSize = 128;
            rDel.Padding = PaddingMode.PKCS7;
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="toDecrypt">解密前字符串</param>
        /// <param name="key">秘钥</param>
        /// <param name="iv">偏移量</param>
        /// <returns>解密后字符串</returns>
        public static string AESDecrypt(string toDecrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        #endregion

        #region GZIP压缩/解压
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] GzipCompress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] GzipDecompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }
        #endregion
        /// <summary>
        /// 设置窗体位置
        /// </summary>
        /// <param name="mainWin"></param>
        /// <param name="win"></param>
        public static void SetWindowLocation(Window mainWin, Window win)
        {
            if(mainWin.WindowState == WindowState.Minimized|| mainWin.WindowState == WindowState.Maximized) return;
            var x = SystemParameters.WorkArea.Width;//得到屏幕工作区域宽度
            var width = x - mainWin.Left - mainWin.Width - 10 - win.Width;//计算主窗体右边空余宽度  显示器工作区域宽度-主窗体左边距离-主窗体宽度-间隙-子窗体宽度
            if (width < 10)
            {
                win.Left = x - win.Width - 10;
                win.Top = mainWin.Top;
            }
            else
            {
                win.Left = mainWin.Left + mainWin.Width + 10;
                win.Top = mainWin.Top;
            }
        }
        /// <summary>
        /// 获得根节点的句柄，常数是固定的
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        public static IntPtr GetHiveHandle(RegistryHive hive)
        {
            IntPtr preexistingHandle = IntPtr.Zero;
            IntPtr HKEY_CLASSES_ROOT = new IntPtr(-2147483648);
            IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
            IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
            IntPtr HKEY_USERS = new IntPtr(-2147483645);
            IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(-2147483644);
            IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);
            IntPtr HKEY_DYN_DATA = new IntPtr(-2147483642);
            switch (hive)
            {
                case RegistryHive.ClassesRoot: preexistingHandle = HKEY_CLASSES_ROOT; break;
                case RegistryHive.CurrentUser: preexistingHandle = HKEY_CURRENT_USER; break;
                case RegistryHive.LocalMachine: preexistingHandle = HKEY_LOCAL_MACHINE; break;
                case RegistryHive.Users: preexistingHandle = HKEY_USERS; break;
                case RegistryHive.PerformanceData: preexistingHandle = HKEY_PERFORMANCE_DATA; break;
                case RegistryHive.CurrentConfig: preexistingHandle = HKEY_CURRENT_CONFIG; break;
                case RegistryHive.DynData: preexistingHandle = HKEY_DYN_DATA; break;
            }
            return preexistingHandle;
        }
        /// <summary>
        /// 操作注册表
        /// </summary>
        /// <param name="hive">根级别的名称</param>
        /// <param name="path">不包括根级别的名称</param>
        /// <param name="parameters">项/（值/值类型） 参数</param>
        /// <param name="view">注册表视图</param>
        [RegistryPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void OperateReg(RegistryHive hive, string path, Dictionary<string, string[]> parameters, RegistryView view)
        {
            SafeRegistryHandle handle = new SafeRegistryHandle(GetHiveHandle(hive), true);
            RegistryKey r = RegistryKey.FromHandle(handle, view).CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            //一般情况是使用如下代码：
            //RegistryKey rk = Registry.LocalMachine.CreateSubKey(path);
            if (parameters == null && parameters.Count <= 0)
                return;
            List<string> keys = parameters.Keys.ToList();
            for (int i = 0; i < parameters.Count; i++)
            {  //string to RegistryValueKind
                RegistryValueKind rv = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), parameters[keys[i]][1].ToString(), true);
                r.SetValue(keys[i], parameters[keys[i]][0], rv);
                LogHelper.WriteWarn("修改注册表成功");
            }
        }
        /// <summary>
        /// 修复文件拖拽
        /// 无论是否修改成功都返回true 确保程序能继续执行
        /// </summary>
        /// <returns></returns>
        public static bool RegisterFileMove()
        {
            if (!IsNeedEditRegister()) return true;
            try
            {
                var is64BitLync = Environment.Is64BitOperatingSystem;
                var paraCapture = new Dictionary<string, string[]>
                {
                    ["EnableLUA"] = new string[] {"0", RegistryValueKind.DWord.ToString()}
                };
                var rv = is64BitLync ? RegistryView.Registry64 : RegistryView.Default;
                OperateReg(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System",
                    paraCapture, rv);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[修改注册表失败]:" + ex.Message + ex.StackTrace + ex.Source);
                return true;
            }
        }
        /// <summary>
        /// 读取注册表 判断是否需要修改
        /// </summary>
        /// <returns></returns>
        private static bool IsNeedEditRegister()
        {
            RegistryKey localKey = null;
            try
            {
                //判断操作系统版本（64位\32位）打开注册表项
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                //得到注册表键值
                var EnableLUA = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System").GetValue("EnableLUA", "").ToString();
                LogHelper.WriteWarn("注册表EnableLUA的值："+ EnableLUA);
                return EnableLUA == "1" ? true : false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}
