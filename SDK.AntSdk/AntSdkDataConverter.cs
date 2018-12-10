using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDK.Service;
using System.Xml;
using System.Configuration;

namespace SDK.AntSdk
{
    /// <summary>
    /// 触角SDK 数据转换类
    /// </summary>
    public class AntSdkDataConverter
    {
        /// <summary>
        /// 方法说明：将类序列化为XML文件
        /// </summary>
        /// <param name="xmlPath">XML文件完整路径</param>
        /// <param name="entity">实体</param>
        /// <param name="errorMsg">错误提示</param>
        public static bool EntityToXml<T>(string xmlPath, T entity, ref string errorMsg)
        {
            try
            {
                if (!File.Exists(xmlPath))
                {
                    var fs = File.Create(xmlPath);//创建该文件
                    fs.Close();
                }
                var s = new XmlSerializer(entity.GetType());
                using (var myWriter = new StreamWriter(xmlPath))
                {
                    s.Serialize(myWriter, entity);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[DataConverter_EntityToXml]:{ex.Message},{ex.StackTrace}");
                errorMsg += ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 方法说明：将XML解析到实体中
        /// </summary>
        /// <typeparam name="T">泛型实体类型</typeparam>
        /// <param name="xmlPath">XML文件完成路径</param>
        /// <param name="entity">实体信息</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功解析</returns>
        public static bool XmlToEntity<T>(string xmlPath, ref T entity, ref string errorMsg)
        {
            if (!File.Exists(xmlPath))
            {
                errorMsg = $"XML File {xmlPath} no exist";
                return false;
            }
            try
            {
                using (var fs = new FileStream(xmlPath, FileMode.Open))
                {
                    if (fs.Length > 0)
                    {
                        var xs = new XmlSerializer(entity.GetType());
                        entity = (T)xs.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[DataConverter_XmlToEntity]:{ex.Message},{ex.StackTrace}");
                errorMsg += ex.Message;
                return false;
            }
            return true;
        }

        

        /// <summary>
        /// 时间格式化_秒转换为分秒
        /// </summary>
        /// 作者：赵雪峰  20160526
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string FormatTimeBySeconds(int seconds)
        {
            var ts = new TimeSpan(0, 0, seconds);
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
        /// 方法说明：时间格式化_分秒转换成秒
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int FormatTimeToSeconds(string time)
        {
            var seconds = 0;
            if (time.Contains("'"))
            {
                seconds += int.Parse(time.Substring(0, time.IndexOf("'", StringComparison.Ordinal))) * 60;
                time = time.Substring(time.IndexOf("'", StringComparison.Ordinal) + 1);
            }
            seconds += int.Parse(time.Substring(0, time.Length - 1));
            return seconds;
        }

        /// <summary>
        /// 方法说明：将集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns></returns>
        public static DataTable ToDataTable(IList list)
        {
            var result = new DataTable();
            if (list == null)
                return result;
            if (list.Count <= 0) { return result;}
            var propertys = list[0].GetType().GetProperties();
            foreach (var pi in propertys)
            {
                result.Columns.Add(pi.Name, pi.PropertyType);
            }
            foreach (var t in list)
            {
                var tempList = new ArrayList();
                foreach (var pi in propertys)
                {
                    var obj = pi.GetValue(t, null);
                    tempList.Add(obj);
                }
                var array = tempList.ToArray();
                result.LoadDataRow(array, true);
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
        /// 将c# DateTime时间格式转换为Unix时间戳格式13位  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToIntLong(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
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
        public static string GetSessionID(string sendUserId, string targetId)
        {
            string sourceStr = string.Empty;
            if (string.Compare(sendUserId, targetId) > 0)
            {
                sourceStr = targetId + sendUserId;
            }
            else
            {
                sourceStr = sendUserId + targetId;
            }
            return MD5Encrypt(sourceStr, Encoding.UTF8);
        }

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
        /// 检测是否开启预发布
        /// </summary>
        /// <returns></returns>
        public static bool IsPreRelease()
        {
            bool result = false;
            string config = ConfigurationManager.AppSettings["SwitchDataBase"]+"".ToString();
            if (config == "" || config.ToLower() == "false")
            {
                result = false;
            }
            else if (config.ToLower() == "true")
            {
                result = true;
            }
            return result;
        }
        #region XML操作
        /// <summary>
        /// 聊天记录保存文件
        /// </summary>
        /// <returns></returns>
        public static string localDataPath()
        {
            string path = "";
            if(IsPreRelease())
            {
                //程序运行目录
                path = AppDomain.CurrentDomain.BaseDirectory + "AntennaChat\\localData\\";
            }
            else
            {
                //公共目录
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AntennaChat\\localData\\";
            }
            return path;
        }
        /// <summary>
        /// 查找Xml节点的值
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static string xmlFind(string nodeName, string path)
        {
            string value = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlElt = xmlDoc.DocumentElement;
            XmlNode node = xmlElt.SelectSingleNode("//updateAntenna");
            if (node != null)
            {
                value = (node.SelectSingleNode(nodeName))?.InnerText;
            }
            return value;
        }

        /// <summary>
        /// 修改Xml节点数据
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="newValue">新值</param>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static bool xmlModify(string nodeName, string newValue, string path)
        {
            bool modify = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xmlElt = xmlDoc.DocumentElement;
                XmlNode node = xmlElt.SelectSingleNode("//updateAntenna");
                if (node != null)
                {
                    (node.SelectSingleNode(nodeName)).InnerText = newValue;
                    xmlDoc.Save(path);
                    modify = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[publicMethod_xmlModify]:" + ex.Message + ex.Source);
                modify = false;
            }
            return modify;
        }
        #endregion
    }
}
