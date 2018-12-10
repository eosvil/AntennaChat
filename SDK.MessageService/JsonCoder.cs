using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SDK.Service
{

    #region   //BaseJsonConverter[JSON编解码处理基类]

    /// <summary>
    /// 类 名 称：BaseJsonConverter
    /// 类 说 明：JSON编解码处理基类
    /// 作    者：Leigons
    /// 完成日期：2017/4/15
    /// </summary>
    internal abstract class BaseJsonConverter : JsonConverter
    {
        /// <summary>
        /// 是否允许JSON解码时调用
        /// </summary>
        public override bool CanRead { get; } = true;

        /// <summary>
        /// 是否允许JSON编码时调用
        /// </summary>
        public override bool CanWrite { get; } = true;
    }

    #endregion

    #region   //DataTableConverter[对DataTable的JSON编解码处理]
    /// <summary>
    /// 类 名 称：DataTableConverter
    /// 类 说 明：对DataTable的JSON编解码处理
    /// 作    者：
    /// 完成日期：2017/4/15
    /// </summary>
    internal class DataTableConverter : BaseJsonConverter
    {
        /// <summary>
        /// 判断是否允许转换指定的对象类型
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns>是否允许转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTable);
        }

        /// <summary>
        /// JSON解码
        /// </summary>
        /// <param name="reader">JsonReader对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="existingValue">解码前的值</param>
        /// <param name="serializer">JsonSerializer对象</param>
        /// <returns>解码后的值</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? JsonCoder.JsonToDataTable(reader.Value.ToString()) : null;
        }

        /// <summary>
        /// JSON编码
        /// </summary>
        /// <param name="writer">JsonWriter对象</param>
        /// <param name="value">对象</param>
        /// <param name="serializer">JsonSerializer对象</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonCoder.DataTableToJson(value as DataTable));
        }
    }
    #endregion

    #region   //DataSetConverter[对DataSet的JSON编解码处理]
    /// <summary>
    /// 类 名 称：DataSetConverter
    /// 类 说 明：对DataSet的JSON编解码处理
    /// 作    者：
    /// 完成日期：2017/4/15
    /// </summary>
    internal class DataSetConverter : BaseJsonConverter
    {
        /// <summary>
        /// 判断是否允许转换指定的对象类型
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns>是否允许转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataSet);
        }

        /// <summary>
        /// JSON解码
        /// </summary>
        /// <param name="reader">JsonReader对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="existingValue">解码前的值</param>
        /// <param name="serializer">JsonSerializer对象</param>
        /// <returns>解码后的值</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? JsonCoder.JsonToDataSet(reader.Value.ToString()) : null;
        }

        /// <summary>
        /// JSON编码
        /// </summary>
        /// <param name="writer">JsonWriter对象</param>
        /// <param name="value">对象</param>
        /// <param name="serializer">JsonSerializer对象</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonCoder.DataSetToJson(value as DataSet));
        }
    }
    #endregion

    #region   //DecimalConverter[对decimal的JSON编解码处理]
    /// <summary>
    /// 类 名 称：DecimalConverter
    /// 类 说 明：对decimal的JSON编解码处理
    /// 作    者：
    /// 完成日期：2017/4/15
    /// </summary>
    internal class DecimalConverter : BaseJsonConverter
    {
        /// <summary>
        /// 判断是否允许转换指定的对象类型
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns>是否允许转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        /// <summary>
        /// JSON解码
        /// </summary>
        /// <param name="reader">JsonReader对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="existingValue">解码前的值</param>
        /// <param name="serializer">JsonSerializer对象</param>
        /// <returns>解码后的值</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? (object)decimal.Parse(reader.Value.ToString()) : null;
        }

        /// <summary>
        /// JSON编码
        /// </summary>
        /// <param name="writer">JsonWriter对象</param>
        /// <param name="value">对象</param>
        /// <param name="serializer">JsonSerializer对象</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
    #endregion

    #region   //ByteArrayConverter[对decimal的JSON编解码处理]
    /// <summary>
    /// 类 名 称：ByteArrayConverter
    /// 类 说 明：对decimal的JSON编解码处理
    /// 作    者：
    /// 完成日期：2017/4/15
    /// </summary>
    internal class ByteArrayConverter : BaseJsonConverter
    {
        /// <summary>
        /// 判断是否允许转换指定的对象类型
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns>是否允许转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        /// <summary>
        /// JSON解码
        /// </summary>
        /// <param name="reader">JsonReader对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="existingValue">解码前的值</param>
        /// <param name="serializer">JsonSerializer对象</param>
        /// <returns>解码后的值</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? Convert.FromBase64String(reader.Value.ToString().Trim('"')) : null;
        }

        /// <summary>
        /// JSON编码
        /// </summary>
        /// <param name="writer">JsonWriter对象</param>
        /// <param name="value">对象</param>
        /// <param name="serializer">JsonSerializer对象</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue("\"" + Convert.ToBase64String((byte[])value) + "\"");
        }
    }
    #endregion

    /// <summary>
    /// 类 名 称：SDK专用JsonCoder[Json编解码器]
    /// 类 说 明：JSON编解码类
    /// 作    者：Leigons
    /// 完成日期：2017/4/15
    /// </summary>
    public class JsonCoder
    {
       
        #region   //字段[特殊约定键名]

        /// <summary>
        /// 行状态键名
        /// </summary>
        private const string DataRowStateColumn = "S";

        /// <summary>
        /// DataSet名称键名
        /// </summary>
        private const string DataSetNameColumn = "SN";

        /// <summary>
        /// DataTable名称键名
        /// </summary>
        private const string DataTableNameColumn = "TN";

        /// <summary>
        /// 大小写敏感键名
        /// </summary>
        private const string CaseSensitiveColumn = "CS";

        /// <summary>
        /// 强制使用约束键名
        /// </summary>
        private const string EnforceConstraintsColumn = "EC";

        /// <summary>
        /// 用户界面中显示的表名称键名
        /// </summary>
        private const string DisplayExpressionColumn = "DE";

        /// <summary>
        /// 区域信息键名
        /// </summary>
        private const string LocaleColumn = "LC";

        /// <summary>
        /// XML命名空间键名
        /// </summary>
        private const string NamespaceColumn = "NS";

        /// <summary>
        /// XML前缀键名
        /// </summary>
        private const string PrefixColumn = "PC";

        /// <summary>
        /// 序列化方式键名
        /// </summary>
        private const string RemotingFormatColumn = "RF";

        /// <summary>
        /// 约束信息键名
        /// </summary>
        private const string ConstraintsColumn = "CC";

        /// <summary>
        /// 主键信息键名
        /// </summary>
        private const string PrimaryKeyColumn = "PK";

        /// <summary>
        /// 关联信息键名
        /// </summary>
        private const string RelationsColumn = "RC";

        /// <summary>
        /// 扩展属性信息键名
        /// </summary>
        private const string ExtendedPropertiesColumn = "EP";

        /// <summary>
        /// 转换器数组
        /// </summary>
        private static readonly JsonConverter[] _converters = { new DataTableConverter(), new DataSetConverter(), new DecimalConverter(), new ByteArrayConverter() };

        /// <summary>
        /// 解码方法信息
        /// </summary>
        private static readonly MethodInfo _deserializeMethodInfo;

        #endregion 字段

        #region   //构造方法

        /// <summary>
        /// 方法说明：初始化 <seealso cref="SDK.Service.JsonCoder"/>
        /// 创建时间：2017/4/15
        /// </summary>
        static JsonCoder()
        {
            var type = typeof(JsonConvert);
            _deserializeMethodInfo = type.GetMethods().First(m => m.Name.Equals("DeserializeObject") && m.IsGenericMethod && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 2);
        }

        #endregion 静态构造方法

        #region   //方法

        #region   //获取Json串的数据类型
        /// <summary>
        /// 方法说明：判断Json串的类型：对象，一维数组， 二维数组，三维数组......
        /// 完成时间：2017-07-10
        /// </summary>
        /// <param name="jsonStr">要判断格式的Json串</param>
        /// <returns></returns>
        internal static SdkEnumCollection.SdkJsonType GetJsonType(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return SdkEnumCollection.SdkJsonType.JstError;
            }
            if (jsonStr.StartsWith("{"))
            {
                return SdkEnumCollection.SdkJsonType.JstObject;
            }
            else if (jsonStr.StartsWith("[[["))
            {
                return SdkEnumCollection.SdkJsonType.JstThreeArray;
            }
            else if (jsonStr.StartsWith("[["))
            {
                return SdkEnumCollection.SdkJsonType.JstTwoArray;
            }
            else if (jsonStr.StartsWith("["))
            {
                return SdkEnumCollection.SdkJsonType.JstOneArray;
            }
            //未判断出来
            return SdkEnumCollection.SdkJsonType.JstError;
        }
        #endregion

        #region 转换对象
        /// <summary>
        /// 方法说明：实体转换成JSON字符串
        /// 创建时间：2016-05-20
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="json">返回json串</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>JSON字符串</returns>
        public static bool SerializeJson(object entity, ref string json, ref string errMsg)
        {
            try
            {
                json = JsonConvert.SerializeObject(entity);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter_SerializeJson]:{ e.Message },{ e.StackTrace}");
                errMsg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 方法说明：实体转换成JSON字符串
        /// 创建时间：2016-05-20
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="isnullIgnore">是否忽略空</param>
        /// <param name="json">返回json串</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>JSON字符串</returns>
        public static bool SerializeJson(object entity, bool isnullIgnore, ref string json, ref string errMsg)
        {
            try
            {
                if (isnullIgnore)
                {
                    var jsonSetting = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
                    json = JsonConvert.SerializeObject(entity, Formatting.None, jsonSetting);
                }
                else
                {
                    json = JsonConvert.SerializeObject(entity);
                }
                //返回
                return true;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter_SerializeJson]:{ e.Message },{ e.StackTrace}");
                errMsg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 方法说明：JSON字符串转换成实体
        /// 完成时间：2016-05-20
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="entity">返回实体</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>格式化后的实体</returns>
        public static bool DeserializeJson<T>(string json, ref T entity, ref string errMsg)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return false;
                entity = JsonConvert.DeserializeObject<T>(json);
                if (entity != null) return true;
                errMsg = $"数据格式错误，将JOSN【{json}】转化为实体失败";
                LogHelper.WriteError($"[MsgConverter_DeserializeJson]:{errMsg}");
                return false;
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[MsgConverter_DeserializeJson]:{e.Message},{e.StackTrace}");
                errMsg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 方法说明：根据键值获取Json字符串中特定字段的值
        /// 完成时间：2016-05-20
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="json">json字符串</param>
        /// <param name="value">值</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否执行成功</returns>
        public static bool GetValueByJsonKey(string key, string json, ref string value, ref string errMsg)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }
                //处理
                var jObject = JObject.Parse(json);
                //判断记录
                if (jObject == null)
                {
                    LogHelper.WriteError($"[JsonCoder_GetValueByJsonKey]:Invalid Json,{json}");
                    return false;
                }
                //判断
                var jobjList = jObject.Cast<JProperty>().ToList();
                if (jobjList.Count == 0)
                {
                    LogHelper.WriteError($"[JsonCoder_GetValueByJsonKey]:Invalid Json Exchanged JProperty List Error,{json}");
                    return false;
                }
                //判断
                if (jobjList.Any(item => item.Name == key))
                {
                    value = jObject[key].ToString();
                    return true;
                }
                //未获取到
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[JsonCoder_GetValueByJsonKey]:Invalid Json Exchanged JProperty List Error,{json},{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 方法说明：将指定对象转换成JSON字符串
        /// 创建时间：2015/12/11
        /// </summary>
        /// <param name="obj">指定对象</param>
        /// <returns>转换后的JSON字符串</returns>
        public static string SerializeObject(object obj)
        {
            return (obj == null) ? string.Empty : JsonConvert.SerializeObject(obj, _converters);
        }

        /// <summary>
        /// 方法说明：将JSON字符串转换成指定类型的对象
        /// 创建时间：2015/12/11
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">待转换的JSON字符串</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>转换后的对象</returns>
        public static T DeserializeObject<T>(string json, ref string errorMsg)
        {
            try
            {
                var entity = string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json, _converters);
                if (entity != null) return entity;
                errorMsg = $"数据格式错误，将JOSN【{json}】转化为实体失败";
                LogHelper.WriteError($"[MsgConverter_DeserializeJson]:{errorMsg}");
                return default(T);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[MsgConverter_DeserializeJson]:{json}{Environment.NewLine}{ex.Message},{ex.StackTrace}");
                errorMsg = ex.Message;
                return default(T);
            }
        }

        /// <summary>
        /// 方法说明：实体转换成JSON字符串
        /// </summary>
        /// <param name="entity">实体参数</param>
        /// <param name="json">json串</param>
        /// <param name="errMsg">错误提示</param>
        /// <param name="nullValueHandling">NullValueHandling</param>
        /// <returns>JSON字符串</returns>
        public static bool SerializeJson(object entity, ref string json, ref string errMsg, NullValueHandling nullValueHandling = NullValueHandling.Include)
        {
            try
            {
                var jSetting = new JsonSerializerSettings { NullValueHandling = nullValueHandling };
                json = JsonConvert.SerializeObject(entity, jSetting);
                return true;
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[DataConverter_SerializeJson]:" + e.Message + "," + e.StackTrace);
                errMsg = e.Message;
                return false;
            }
        }

        #endregion 转换对象

        #region 转换DataTable

        /// <summary>
        /// 方法说明：将 <seealso cref="DataTable"/> 转换成JSON字符串
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="dt">待转换的 <seealso cref="DataTable"/></param>
        /// <returns>转换后的JSON字符串</returns>
        public static string DataTableToJson(DataTable dt)
        {
            var list = DataTableToList(dt);
            return JsonConvert.SerializeObject(list, _converters);
        }

        /// <summary>
        /// 方法说明：将 <seealso cref="DataTable"/> 转换成字典列表
        /// 作    者：
        /// 创建时间：2015/1/14
        /// </summary>
        /// <param name="dt">待转换的 <seealso cref="DataTable"/></param>
        /// <returns>转换后的字典列表</returns>
        private static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            DataTableToList(dt, list);

            return list;
        }

        /// <summary>
        /// 方法说明：将 <seealso cref="DataTable"/> 转换成字典，添加到字典列表中
        /// 作    者：
        /// 创建时间：2015/1/14
        /// </summary>
        /// <param name="dt">待转换的 <seealso cref="DataTable"/></param>
        /// <param name="list">字典列表</param>
        private static void DataTableToList(DataTable dt, ICollection<Dictionary<string, object>> list)
        {
            var dictionary = new Dictionary<string, object>
            {
                { DataTableNameColumn, dt.TableName },
                { LocaleColumn, dt.Locale.Name }
            };

            if (dt.CaseSensitive)
            {
                dictionary.Add(CaseSensitiveColumn, 1);
            }

            if (!string.IsNullOrEmpty(dt.DisplayExpression))
            {
                dictionary.Add(DisplayExpressionColumn, dt.DisplayExpression);
            }

            if (!string.IsNullOrEmpty(dt.Namespace))
            {
                dictionary.Add(NamespaceColumn, dt.Namespace);
            }

            if (!string.IsNullOrEmpty(dt.Prefix))
            {
                dictionary.Add(PrefixColumn, dt.Prefix);
            }

            if (dt.RemotingFormat == SerializationFormat.Binary)
            {
                dictionary.Add(RemotingFormatColumn, 1);
            }

            if (dt.Constraints.Count > 0)
            {
                var ucsInfo = (from c in dt.Constraints.OfType<UniqueConstraint>()
                               select c
                                   into uc
                               select new List<string> { uc.ConstraintName, uc.IsPrimaryKey ? "T" : "F", string.Join(",", uc.Columns.Select(column => column.ColumnName).ToArray()), JsonConvert.SerializeObject(uc.ExtendedProperties) }
                                       into ucInfo
                               select JsonConvert.SerializeObject(ucInfo)).ToList();

                if (ucsInfo.Count > 0)
                {
                    dictionary.Add(ConstraintsColumn, JsonConvert.SerializeObject(ucsInfo));
                }
            }

            if (dt.ExtendedProperties.Count > 0)
            {
                dictionary.Add(ExtendedPropertiesColumn, JsonConvert.SerializeObject(dt.ExtendedProperties));
            }

            if (dt.PrimaryKey.Length > 0)
            {
                dictionary.Add(PrimaryKeyColumn, string.Join(",", dt.PrimaryKey.Select(c => c.ColumnName).ToArray()));
            }

            // 键名映射，减少键名出现多次的数据量
            var columnNames = new Dictionary<string, string>();
            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var columnName = "F" + i;
                columnNames.Add(dt.Columns[i].ColumnName, columnName);

                dictionary.Add(columnName, $"{dt.Columns[i].ColumnName}:{dt.Columns[i].DataType.FullName}");
            }

            list.Add(dictionary);

            // 添加数据行
            foreach (DataRow dataRow in dt.Rows)
            {
                dictionary = new Dictionary<string, object>();
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    var value = dataRow.RowState == DataRowState.Deleted ? dataRow[dataColumn, DataRowVersion.Original] : dataRow[dataColumn];
                    value = ((value == DBNull.Value) || (value == null)) ? null : value;
                    dictionary.Add(columnNames[dataColumn.ColumnName], value);
                }

                dictionary.Add(DataRowStateColumn, ((int)dataRow.RowState));

                list.Add(dictionary);
            }
        }

        /// <summary>
        /// 方法说明：将JSON字符串转换成 <seealso cref="DataTable"/>
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="json">待转换的JSON字符串</param>
        /// <returns>转换后的 <seealso cref="DataTable"/></returns>
        public static DataTable JsonToDataTable(string json)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                return ListToDataTable(list);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 方法说明：将字典列表转换成 <seealso cref="DataTable"/>
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="list">待转换字典列表</param>
        /// <returns>转换后的 <seealso cref="DataTable"/></returns>
        private static DataTable ListToDataTable(IList<Dictionary<string, object>> list)
        {
            DataTable dt = null;

            try
            {
                var states = new Dictionary<DataRow, string>();
                if ((list != null) && (list.Count > 0))
                {
                    var tablesInfo = list[0];
                    var columnNames = new Dictionary<string, string>();
                    list.RemoveAt(0);

                    if (tablesInfo.Keys.Count == 0)
                    {
                        return null;
                    }

                    dt = new DataTable
                    {
                        CaseSensitive = false,
                        RemotingFormat = SerializationFormat.Xml
                    };

                    string cc = null;
                    string[] pks = null;
                    foreach (var key in tablesInfo.Keys)
                    {
                        switch (key)
                        {
                            case DataTableNameColumn:
                                dt.TableName = tablesInfo[key].ToString();
                                break;

                            case CaseSensitiveColumn:
                                dt.CaseSensitive = true;
                                break;

                            case DisplayExpressionColumn:
                                dt.DisplayExpression = tablesInfo[key].ToString();
                                break;

                            case LocaleColumn:
                                dt.Locale = new System.Globalization.CultureInfo(tablesInfo[key].ToString());
                                break;

                            case NamespaceColumn:
                                dt.Namespace = tablesInfo[key].ToString();
                                break;

                            case PrefixColumn:
                                dt.Prefix = tablesInfo[key].ToString();
                                break;

                            case RemotingFormatColumn:
                                dt.RemotingFormat = SerializationFormat.Binary;
                                break;

                            case ConstraintsColumn:
                                cc = tablesInfo[key].ToString();
                                break;

                            case ExtendedPropertiesColumn:
                                var pc = JsonConvert.DeserializeObject<PropertyCollection>(tablesInfo[key].ToString());
                                foreach (var c in pc.Keys)
                                {
                                    dt.ExtendedProperties.Add(c, pc[c]);
                                }

                                break;

                            case PrimaryKeyColumn:
                                pks = tablesInfo[key].ToString().Split(',');
                                break;

                            default:
                                var columnInfo = tablesInfo[key].ToString();
                                var index = columnInfo.LastIndexOf(":", StringComparison.Ordinal);
                                var columnName = columnInfo.Substring(0, index);
                                var typeName = columnInfo.Substring(index + 1);
                                var type = Type.GetType(typeName);
                                if (type == null)
                                {
                                    throw new Exception("Invalid type name");
                                }

                                columnNames.Add(key, columnName);
                                dt.Columns.Add(columnName, type);

                                break;
                        }
                    }

                    foreach (var dictionary in list)
                    {
                        var dr = dt.NewRow();
                        foreach (var column in dictionary.Keys)
                        {
                            if (column != DataRowStateColumn)
                            {
                                if ((dictionary[column] == null) || (dictionary[column] == DBNull.Value))
                                {
                                    dr[columnNames[column]] = DBNull.Value;
                                }
                                else if (dictionary[column].GetType() != dt.Columns[columnNames[column]].DataType)
                                {
                                    dr[columnNames[column]] = JsonToObject(dt.Columns[columnNames[column]].DataType, dictionary[column].ToString());
                                }
                                else
                                {
                                    dr[columnNames[column]] = dictionary[column];
                                }
                            }
                            else
                            {
                                states.Add(dr, dictionary[column].ToString());
                            }
                        }

                        dt.Rows.Add(dr);
                    }

                    dt.AcceptChanges();

                    foreach (var dr in states.Keys)
                    {
                        var state = (DataRowState)Enum.Parse(typeof(DataRowState), states[dr]);
                        switch (state)
                        {
                            case DataRowState.Deleted:
                                dr.Delete();
                                break;

                            case DataRowState.Added:
                                dr.SetAdded();
                                break;

                            case DataRowState.Modified:
                                dr.SetModified();
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(cc))
                    {
                        var uniqueConstraintsInfos = JsonConvert.DeserializeObject<List<string>>(cc);
                        foreach (var uniqueConstraintInfos in uniqueConstraintsInfos)
                        {
                            var uniqueConstraintInfo = JsonConvert.DeserializeObject<List<string>>(uniqueConstraintInfos);
                            var name = uniqueConstraintInfo[0];
                            var isPrimaryKey = (uniqueConstraintInfo[1] == "T");
                            var names = uniqueConstraintInfo[2].Split(',');

                            var uc = new UniqueConstraint(name, names.Select(column => dt.Columns[column]).ToArray(), isPrimaryKey);

                            var pc = JsonConvert.DeserializeObject<PropertyCollection>(uniqueConstraintInfo[3]);
                            foreach (var c in pc.Keys)
                            {
                                uc.ExtendedProperties.Add(c, pc[c]);
                            }

                            dt.Constraints.Add(uc);
                        }
                    }

                    if (pks != null)
                    {
                        dt.PrimaryKey = pks.Select(column => dt.Columns[column]).ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }

            return dt;
        }

        #endregion 转换DataTable

        #region 转换DataSet

        /// <summary>
        /// 方法说明：将 <seealso cref="DataSet"/> 转换成JSON字符串
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="ds">待转换的 <seealso cref="DataSet"/></param>
        /// <returns>转换后的JSON字符串</returns>
        public static string DataSetToJson(DataSet ds)
        {
            var list = new List<Dictionary<string, object>>();

            var dictionary = new Dictionary<string, object>
            {
                { DataSetNameColumn, ds.DataSetName },
                { LocaleColumn, ds.Locale.Name }
            };

            if (ds.CaseSensitive)
            {
                dictionary.Add(CaseSensitiveColumn, 1);
            }

            if (!string.IsNullOrEmpty(ds.Namespace))
            {
                dictionary.Add(NamespaceColumn, ds.Namespace);
            }

            if (!string.IsNullOrEmpty(ds.Prefix))
            {
                dictionary.Add(PrefixColumn, ds.Prefix);
            }

            if (ds.EnforceConstraints)
            {
                dictionary.Add(EnforceConstraintsColumn, 1);
            }

            if (ds.ExtendedProperties.Count > 0)
            {
                dictionary.Add(ExtendedPropertiesColumn, JsonConvert.SerializeObject(ds.ExtendedProperties));
            }

            if (ds.RemotingFormat == SerializationFormat.Binary)
            {
                dictionary.Add(RemotingFormatColumn, 1);
            }

            if (ds.Relations.Count > 0)
            {
                var relationsInfo = (from DataRelation r in ds.Relations
                                     select new List<string> { r.RelationName, r.ParentTable.TableName, string.Join(",", r.ParentColumns.Select(column => column.ColumnName).ToArray()), r.ChildTable.TableName, string.Join(",", r.ChildColumns.Select(column => column.ColumnName).ToArray()), r.Nested ? "T" : "F", JsonConvert.SerializeObject(r.ExtendedProperties) }
                                         into rInfo
                                     select JsonConvert.SerializeObject(rInfo)).ToList();

                dictionary.Add(RelationsColumn, JsonConvert.SerializeObject(relationsInfo));
            }

            list.Add(dictionary);

            foreach (DataTable dt in ds.Tables)
            {
                DataTableToList(dt, list);
            }

            return JsonConvert.SerializeObject(list, _converters);
        }

        /// <summary>
        /// 方法说明：将JSON字符串转换成 <seealso cref="DataSet" />
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="json">待转换的JSON字符串</param>
        /// <returns>转换后的 <seealso cref="DataTable" /></returns>
        public static DataSet JsonToDataSet(string json)
        {
            DataSet ds = null;

            try
            {
                var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                if ((list != null) && (list.Count > 0))
                {
                    var dataSetInfos = list[0];
                    list.RemoveAt(0);

                    ds = new DataSet
                    {
                        CaseSensitive = false,
                        EnforceConstraints = false,
                        RemotingFormat = SerializationFormat.Xml
                    };

                    string rc = null;
                    foreach (var key in dataSetInfos.Keys)
                    {
                        switch (key)
                        {
                            case DataSetNameColumn:
                                ds.DataSetName = dataSetInfos[key].ToString();
                                break;

                            case CaseSensitiveColumn:
                                ds.CaseSensitive = true;
                                break;

                            case LocaleColumn:
                                ds.Locale = new System.Globalization.CultureInfo(dataSetInfos[key].ToString());
                                break;

                            case NamespaceColumn:
                                ds.Namespace = dataSetInfos[key].ToString();
                                break;

                            case PrefixColumn:
                                ds.Prefix = dataSetInfos[key].ToString();
                                break;

                            case RemotingFormatColumn:
                                ds.RemotingFormat = SerializationFormat.Binary;
                                break;

                            case EnforceConstraintsColumn:
                                ds.EnforceConstraints = true;
                                break;

                            case ExtendedPropertiesColumn:
                                var pc = JsonConvert.DeserializeObject<PropertyCollection>(dataSetInfos[key].ToString());
                                foreach (var c in pc.Keys)
                                {
                                    ds.ExtendedProperties.Add(c, pc[c]);
                                }

                                break;

                            case RelationsColumn:
                                rc = dataSetInfos[key].ToString();
                                break;
                        }
                    }

                    var tableInfo = new List<Dictionary<string, object>>
                    {
                        list[0]
                    };

                    list.RemoveAt(0);

                    foreach (var item in list)
                    {
                        if (item.ContainsKey(DataTableNameColumn))
                        {
                            var dt = ListToDataTable(tableInfo);
                            ds.Tables.Add(dt);

                            tableInfo.Clear();
                        }

                        tableInfo.Add(item);
                    }

                    if (tableInfo.Count > 0)
                    {
                        var dt = ListToDataTable(tableInfo);
                        ds.Tables.Add(dt);
                    }

                    if (!string.IsNullOrEmpty(rc))
                    {
                        var relationsInfos = JsonConvert.DeserializeObject<List<string>>(rc);
                        foreach (var relationInfos in relationsInfos)
                        {
                            var relationInfo = JsonConvert.DeserializeObject<List<string>>(relationInfos);
                            var name = relationInfo[0];
                            var parentTable = ds.Tables[relationInfo[1]];
                            var parentColumns = GetColumns(parentTable, relationInfo[2].Split(','));
                            var childTable = ds.Tables[relationInfo[3]];
                            var childColumns = GetColumns(childTable, relationInfo[4].Split(','));
                            var nested = (relationInfo[5] == "T");

                            var r = new DataRelation(name, parentColumns, childColumns, true)
                            {
                                Nested = nested
                            };

                            var pc = JsonConvert.DeserializeObject<PropertyCollection>(relationInfo[6]);
                            foreach (var c in pc.Keys)
                            {
                                r.ExtendedProperties.Add(c, pc[c]);
                            }

                            ds.Relations.Add(r);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return ds;
        }

        /// <summary>
        /// 方法说明：获取列对象数组
        /// 作    者：
        /// 创建时间：2015/6/13
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="columnNames">列名数组</param>
        /// <returns>列对象数组</returns>
        private static DataColumn[] GetColumns(DataTable dt, string[] columnNames)
        {
            return columnNames.Select(column => dt.Columns[column]).ToArray();
        }

        #endregion 转换DataSet

        #region 转换参数列表

        /// <summary>
        /// 方法说明：将参数列表转换成JSON字符串
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="types">参数类型数组</param>
        /// <param name="values">参数值数组</param>
        /// <returns>转换后的JSON字符串</returns>
        /// <exception cref="System.ArgumentException">Conflict arguments info.</exception>
        public static string ParamsToJson(Type[] types, object[] values)
        {
            if (((types == null) || (types.Length == 0)) && ((values == null) || (values.Length == 0)))
            {
                return null;
            }

            if (((types == null) || (types.Length == 0)) || ((values == null) || (values.Length == 0)) || (types.Length != values.Length))
            {
                throw new ArgumentException("Conflict arguments info.");
            }

            var list = new List<object>
            {
                string.Join("/", types.Select(t => string.Format("{0},{1}", t.FullName, t.Assembly)).ToArray())
            };

            list.AddRange(values);

            return JsonConvert.SerializeObject(list, _converters);
        }

        /// <summary>
        /// 方法说明：将JSON字符串转换成参数列表
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="json">待转换的JSON字符串</param>
        /// <returns>转换后的参数列表</returns>
        public static object[] JsonToParams(string json)
        {
            object[] paramList = null;

            try
            {
                var list = JsonConvert.DeserializeObject<List<object>>(json);
                if ((list != null) && (list.Count > 0))
                {
                    var typeInfos = list[0].ToString();
                    list.RemoveAt(0);

                    var types = typeInfos.Split('/');
                    if (types.Length != list.Count)
                    {
                        throw new ArgumentException("Conflict arguments info.");
                    }

                    var values = new List<object>();
                    for (var i = 0; i < types.Length; i++)
                    {
                        var type = Type.GetType(types[i]);
                        if (type == null)
                        {
                            throw new ArgumentException("Wrong arguments type.");
                        }

                        object obj = null;
                        if (list[i] != null)
                        {
                            obj = list[i].GetType() == type ? list[i] : JsonToObject(type, list[i].ToString());
                        }

                        values.Add(obj);
                    }

                    paramList = values.ToArray();
                }
            }
            catch
            {
                return null;
            }

            return paramList;
        }

        /// <summary>
        /// 方法说明：将JSON字符串转换成指定类型的对象
        /// 作    者：
        /// 创建时间：2017/4/15
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="json">待转换的JSON字符串</param>
        /// <returns>转换后的对象</returns>
        private static object JsonToObject(Type type, string json)
        {
            if (type == typeof(DataSet))
            {
                return JsonToDataSet(json);
            }

            if (type == typeof(DataTable))
            {
                return JsonToDataTable(json);
            }

            var deserializeMethod = _deserializeMethodInfo.MakeGenericMethod(type);
            var obj = deserializeMethod.Invoke(null, new object[] { json, _converters });

            return obj;
        }

        #endregion 转换参数列表

        #endregion 方法
    }
}
