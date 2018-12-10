using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using SDK.AntSdk.AntModels;
using SDK.Service;
using System.Data.SQLite;
using System.Configuration;

namespace SDK.AntSdk
{
    /// <summary>
    /// 类型说明：SQLite数据库操作处理
    /// </summary>
    public class AntSdkSqliteHelper
    {
        private static readonly object Obj = new object();

        private static readonly object ObjTable = new object();
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
        /// <summary>
        /// 连接创建
        /// </summary>
        /// <returns></returns>
        public static string ConnStr(string comoanyCode, string userId)
        {
            string path = "";
            if (IsPreRelease())
            {
                //程序运行目录
                path = AppDomain.CurrentDomain.BaseDirectory + "AntennaChat\\localData\\" + comoanyCode;
            }
            else
            {
                //公共目录
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AntennaChat\\localData\\" + comoanyCode;
            }
            return
                $@"Data Source={path}\{userId}\{userId}.db;PRAGMA journal_mode=WAL;Version=3";
        }


        /// <summary>
        /// 方法说明：对数据进行增加、删除、修改、创建数据库、创建表操作
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="comoanyCode">公司代码</param>
        /// <param name="userId">用户ID</param>
        /// <returns>SQL执行影响行数</returns>
        public static int ExecuteNonQuery(string sql, string comoanyCode, string userId)
        {
            try
            {
                lock (Obj)
                {
                    using (var conn = new SQLiteConnection(ConnStr(comoanyCode, userId)))
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            var result = cmd.ExecuteNonQuery();
                            if (result == 0)
                            {
                                LogHelper.WriteWarn(sql);
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_ExecuteNonQuery]:{ex.Message}{Environment.NewLine}{sql}");
                return -1;
            }
        }

        /// <summary>
        /// 方法说明：查询单条数据
        /// </summary>
        /// <param name="conn">SQLite连接</param>
        /// <param name="sql">sql语句</param>
        /// <returns>SQLite数据读取器</returns>
        public static SQLiteDataReader ExecuteReader(SQLiteConnection conn, string sql)
        {
            try
            {
                lock (Obj)
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    var cmd = new SQLiteCommand(sql, conn);
                    var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return myReader;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_ExecuteReader]:{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 方法说明：ExecuteScalar
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="dbPath">数据库路径</param>
        /// <returns>SQL语句执行结果对象</returns>
        public static object ExecuteScalar(string sql, string dbPath)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3"))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand(sql, conn);
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_ExecuteScalar]:{ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 方法说明：根据版本号提取SQL语句
        /// </summary>
        /// <param name="varFileName">sql文件路劲</param>
        /// <param name="verNum">当前数据库版本号</param>
        /// <returns>执行SQL列表</returns>
        public static ArrayList GetSqlFile(string varFileName, int verNum)
        {
            var alSql = new ArrayList();
            var ver = 0;
            if (!File.Exists(varFileName))
            {
                return alSql;
            }
            var rs = new StreamReader(varFileName, Encoding.Default);//注意编码
            var commandText = string.Empty;
            while (rs.Peek() > -1)
            {
                var varLine = rs.ReadLine();
                if (varLine == string.Empty)
                {
                    continue;
                }
                if (varLine != null && varLine.ToUpper().Contains("VER_"))
                {
                    if ((varLine.Length > 4))
                    {
                        var verStr = varLine.Replace(" ", string.Empty).Substring(4).Replace(".", string.Empty);
                        ver = verStr.Length > 0 ? Convert.ToInt32(verStr) : 0;
                    }
                    continue;
                }
                if (varLine != null && (varLine.Trim() != "GO" && varLine.Trim() != "go"))
                {
                    commandText += varLine;
                    commandText += "\r\n";
                }
                else
                {
                    alSql.Add(commandText);
                    commandText = "";
                    if (verNum >= ver)
                    {
                        alSql.Clear();
                    }
                }
            }
            rs.Close();
            return alSql;
        }

        /// <summary>
        /// 方法说明：事务处理多条语句
        /// </summary>
        /// <param name="varSqlList">sql语句数组</param>
        /// <param name="dbPath">数据库路径</param>
        public static bool ExecuteCommand(ArrayList varSqlList, string dbPath)
        {

            using (var myConnection = new SQLiteConnection($"Data Source={dbPath};Version=3"))
            {
                myConnection.Open();
                using (var varTrans = myConnection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand
                        {
                            Connection = myConnection,
                            Transaction = varTrans
                        })
                        {
                            foreach (string varcommandText in varSqlList)
                            {
                                command.CommandText = varcommandText;
                                command.ExecuteNonQuery();
                            }
                            varTrans.Commit();
                        }
                        LogHelper.WriteInfo("[创建表成功]");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        varTrans.Rollback();
                        LogHelper.WriteError($"[执行数据库事务失败]:{ex.Message}，{ex.StackTrace}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 方法说明：返回数据表信息
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="dbPath">数据库地址</param>
        /// <returns>返回数据集</returns>
        public static DataTable ExecuteDataTable(string sql, string dbPath)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3"))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        using (var sda = new SQLiteDataAdapter(cmd))
                        {
                            sda.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_ExecuteDataTable]:{e.Message}");
                return dt;
            }
            //返回数据表
            return dt;
        }

        /// <summary>
        /// 方法说明：批量插入大数据
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="comoanyCode">公司编码</param>
        /// <param name="userId">用户ID</param>
        /// <returns>是否成功插入数据</returns>
        public static bool InsertBigData(string sql, string comoanyCode, string userId)
        {
            SQLiteTransaction transaction = null;
            var conn = new SQLiteConnection(ConnStr(comoanyCode, userId));
            var cmd = new SQLiteCommand(sql, conn);
            conn.Open();
            try
            {
                lock (Obj)
                {
                    transaction = conn.BeginTransaction();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[AntSdkSqliteHelper_InsertBigData]:" + ex.Message);
                transaction?.Rollback();
                return false;
            }
            finally
            {
                conn.Close();
                transaction?.Dispose();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 方法说明：创建数据库
        /// </summary>
        /// <param name="path">数据库路径</param>
        /// <param name="databaseName">数据库名称</param>
        /// <returns>0 失败；1 成功</returns>
        public static int CreateDataBase(string path, string databaseName)
        {
            int result = 0;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    SQLiteConnection.CreateFile($@"{path}\{databaseName}");
                    result = 1;
                }
                else if (!File.Exists($@"{path}\{databaseName}"))
                {
                    SQLiteConnection.CreateFile($@"{path}\{databaseName}");
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[AntSdkSqliteHelper_CreateDataBase]:" + ex.Message);
                return 0;
            }
            return result;
        }

        /// <summary>
        /// 方法说明：创建表
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        /// <param name="tableName">表名</param>
        /// <param name="creatTableStr">创建表</param>
        /// <returns>0 失败；1 成功；</returns>
        public static int CreateTable(string dbPath, string tableName, string creatTableStr)
        {
            try
            {
                lock (ObjTable)
                {
                    using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3"))
                    {

                        conn.Open();
                        var sqlStr = $"create table {tableName} {creatTableStr}";
                        using (var cmd = new SQLiteCommand(sqlStr, conn))
                        {
                            cmd.ExecuteNonQuery();
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_CreateTable]:{ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 方法说明：DataTable转换IList类
        /// </summary>
        /// <typeparam name="T">转换类型</typeparam>
        public class ModelConvertHelper<T> where T : new()
        {
            public static IList<AntSdkChatMsg.ChatBase> ConvertToChatMsgModel(DataTable dt)
            {
                try
                {
                    var chatmsgList = new List<AntSdkChatMsg.ChatBase>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        var mtype = AntSdkMsgType.UnDefineMsg;
                        if (dt.Columns.Contains("MTP"))
                        {
                            long mp;
                            if (long.TryParse(dr["MTP"].ToString(), out mp))
                            {
                                mtype = (AntSdkMsgType)mp;
                            }
                        }
                        var chatMsg = new AntSdkChatMsg.ChatBase
                        {
                            MsgType = mtype,
                            chatIndex = dt.Columns.Contains("CHATINDEX") ? dr["CHATINDEX"].ToString() : string.Empty,
                            sourceContent = dt.Columns.Contains("CONTENT") ? dr["CONTENT"].ToString() : string.Empty,
                            messageId = dt.Columns.Contains("MESSAGEID") ? dr["MESSAGEID"].ToString() : string.Empty,
                            sendTime = dt.Columns.Contains("SENDTIME") ? dr["SENDTIME"].ToString() : string.Empty,
                            sendUserId = dt.Columns.Contains("SENDUSERID") ? dr["SENDUSERID"].ToString() : string.Empty,
                            sessionId = dt.Columns.Contains("SESSIONID") ? dr["SESSIONID"].ToString() : string.Empty,
                            targetId = dt.Columns.Contains("TARGETID") ? dr["TARGETID"].ToString() : string.Empty,
                            SENDORRECEIVE =
                                dt.Columns.Contains("SENDORRECEIVE") ? dr["SENDORRECEIVE"].ToString() : string.Empty,
                            sendsucessorfail =
                                dt.Columns.Contains("SENDSUCESSORFAIL")
                                    ? int.Parse(dr["SENDSUCESSORFAIL"].ToString())
                                    : 0,
                            flag = dt.Columns.Contains("FLAG") ? int.Parse(dr["FLAG"].ToString()) : 0,
                            readtime = dt.Columns.Contains("READTIME") ? dr["READTIME"].ToString() : string.Empty,
                            uploadOrDownPath =
                                dt.Columns.Contains("UPLOADORDOWNPATH")
                                    ? dr["UPLOADORDOWNPATH"].ToString()
                                    : string.Empty,
                            voiceread = dt.Columns.Contains("VOICEREAD") ? dr["VOICEREAD"].ToString() : string.Empty
                        };
                        var antsdkchatMsg = AntSdkChatMsg.GetAntSdkSpecificChatMsg(chatMsg);
                        if (antsdkchatMsg != null)
                        {
                            chatmsgList.Add(antsdkchatMsg);
                        }
                    }
                    return chatmsgList;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError($"[AntSdkSqliteHelper_ConvertToChatMsgModel]:{ex.Message}");
                    return null;
                }
            }

            /// <summary>
            ///  DataTable转换IList
            /// </summary>
            /// <param name="dt">DataTable数据源</param>
            /// <returns></returns>
            public static IList<T> ConvertToModel(DataTable dt)
            {
                try
                {
                    var ts = new List<T>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        var t = new T();
                        var propertys = t.GetType().GetProperties();
                        foreach (var pi in propertys)
                        {
                            var tempName = pi.Name;
                            if (!dt.Columns.Contains(tempName))
                            {
                                continue;
                            }
                            if (!pi.CanWrite)
                            {
                                continue;
                            }
                            var value = dr[tempName];
                            if (value != DBNull.Value)
                            {

                                if (pi.PropertyType.FullName == "System.Int32")
                                {
                                    pi.SetValue(t, Convert.ToInt32(value), null);
                                }
                                else
                                {
                                    pi.SetValue(t, value, null);
                                }
                            }
                        }
                        //添加转换类型信息
                        ts.Add(t);
                    }
                    return ts;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError($"[AntSdkSqliteHelper_ConvertToModel]:{ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 方法说明：启动初始化数据库和表
        /// </summary>
        /// <param name="companyCode">公司编码</param>
        /// <param name="sendUserId">当前登录用户ID</param>
        /// <returns>是否成功启动</returns>
        public static bool AntSdkDataBaseInitialize(string companyCode, string sendUserId)
        {
            try
            {
                //检查有没有数据库
                if (
                    !File.Exists(
                        $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{companyCode}\{sendUserId}\{sendUserId}.db"))
                {
                    //创建数据库
                    var path = $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{companyCode}\{sendUserId}";
                    if (CreateDataBase(path, $"{sendUserId}.db") > 0)
                    {
                        //要修改初始化SQL语句，请修改InitializeSQLite.sql，按照格式
                        var sqlList = GetSqlFile($@"{AppDomain.CurrentDomain.BaseDirectory}InitializeSQLite.sql", 0);
                        //获取初始化sql语句
                        if (sqlList != null && sqlList.Count > 0)
                        {
                            ExecuteCommand(sqlList, $@"{path}\{sendUserId}.db");
                        }
                        var newSqliteVersion =
                            AntSdkXmlHelper.XmlFind($"{AppDomain.CurrentDomain.BaseDirectory}projectStatic.xml",
                                "updateAntenna", "sqliteversion");
                        var insertStr =
                            $"insert into T_Version (VERSION,SENDTIME) values ('{newSqliteVersion}','{AntSdkDataConverter.ConvertDateTimeInt(DateTime.Now)}')";
                        ExecuteNonQuery(insertStr, companyCode, sendUserId);
                    }
                    return true;
                }
                //数据库已经存在 检查版本
                else
                {
                    string selectStr = "select VERSION from T_Version order by SENDTIME desc limit 0,1";
                    string dbPath =
                        $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{companyCode}\{sendUserId}\{sendUserId}.db";
                    if (File.Exists(dbPath))
                    {
                        //数据库中存储的版本号
                        string oldSqliteVersion = ExecuteScalar(selectStr, dbPath).ToString();
                        if (!string.IsNullOrEmpty(oldSqliteVersion))
                        {
                            var verStr = oldSqliteVersion.Replace(" ", string.Empty).Replace(".", string.Empty);
                            LogHelper.WriteDebug($"DataBase Version：{verStr}");
                            var oldVer = verStr.Length > 0 ? Convert.ToInt32(verStr) : 0;
                            var sqlList = GetSqlFile($"{AppDomain.CurrentDomain.BaseDirectory}UpdateSQLite.sql", oldVer);
                            //获取初始化sql语句
                            if (sqlList != null && sqlList.Count > 0)
                            {
                                bool result = ExecuteCommand(sqlList,
                                     $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{companyCode}\{sendUserId}\{sendUserId}.db");
                                if (!result)
                                {
                                    return false;
                                }
                            }
                            //发布的数据库版本号
                            var newSqliteVersion =
                                AntSdkXmlHelper.XmlFind($"{AppDomain.CurrentDomain.BaseDirectory}projectStatic.xml",
                                    "updateAntenna", "sqliteversion");
                            var newVer = newSqliteVersion.Replace(" ", string.Empty).Replace(".", string.Empty).Length > 0
                                ? Convert.ToInt32(newSqliteVersion.Replace(" ", string.Empty).Replace(".", string.Empty))
                                : 0;
                            if (oldVer >= newVer) { return true; }
                            string insertStr =
                                $"insert into T_Version (VERSION,SENDTIME) values ('{newSqliteVersion}','{AntSdkDataConverter.ConvertDateTimeInt(DateTime.Now)}')";
                            ExecuteNonQuery(insertStr, companyCode, sendUserId);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError($"[AntSdkSqliteHelper_Initialize]:{ex.Message},{ex.Source}");
                return false;
            }
        }
    }
}
