using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Antenna.Framework
{
    public class SqliteHelper
    {
        private SqliteHelper() { }
        /// <summary>
        /// 连接构造
        /// </summary>
        /// <returns></returns>
        public static string ConnStr(string comoanyCode, string userId)
        {
            return "Data Source=" + publicMethod.localDataPath() + comoanyCode + "\\" + userId + "\\" + userId + ".db;PRAGMA journal_mode=WAL;" + "Version=3";
        }
        private static object obj = new object();
        /// <summary>
        /// 对数据进行增加、删除、修改、创建数据库、创建表操作
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, string comoanyCode, string userId)
        {
            try
            {
                lock (obj)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnStr(comoanyCode, userId)))
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                        {
                            int result = 0;
                            result = cmd.ExecuteNonQuery();
                            //cmd.Dispose();
                            //conn.Close();
                            //conn.Dispose();
                            if (result == 0 || result < 0)
                            {
                                LogHelper.WriteDebug(sql);
                            }
                            return result;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_ExecuteNonQuery]:" + ex.Message + Environment.NewLine + sql);
                return -1;
            }
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        public static SQLiteDataReader ExecuteReader(SQLiteConnection conn, string sql)
        {
            try
            {
                lock (obj)
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(ConnStr(comoanyCode, userId)))
                    //{
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    var cmd = new SQLiteCommand(sql, conn);
                    SQLiteDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return myReader;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_ExecuteReader]:" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, string dbPath)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbPath + ";" + "Version=3"))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[SqliteHelper_ExecuteScalar]:" + e.Message);
                return null;
            }
        }
        /// <summary>
        /// 根据版本号提取SQL语句
        /// </summary>
        /// <param name="varFileName">sql文件路劲</param>
        /// <param name="verNum">当前数据库版本号</param>
        /// <returns></returns>
        public static ArrayList GetSqlFile(string varFileName, int verNum)
        {
            var alSql = new ArrayList();
            var ver = 0;
            if (!File.Exists(varFileName))
            {
                return alSql;
            }
            var rs = new StreamReader(varFileName, Encoding.Default);//注意编码
            var commandText = "";
            while (rs.Peek() > -1)
            {
                var varLine = rs.ReadLine();
                if (varLine == "")
                {
                    continue;
                }
                if (varLine != null && varLine.ToUpper().Contains("VER_"))
                {
                    if ((varLine.Length > 4))
                    {
                        var verStr = varLine.Replace(" ", "").Substring(4).Replace(".", "");
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
        /// 事务处理多条语句
        /// </summary>
        /// <param name="varSqlList">sql语句数组</param>
        /// <param name="dbPath">数据库路径</param>
        public static bool ExecuteCommand(ArrayList varSqlList, string dbPath)
        {

            using (var myConnection = new SQLiteConnection("Data Source=" + dbPath + ";" + "Version=3"))
            {
                myConnection.Open();
                using (SQLiteTransaction varTrans = myConnection.BeginTransaction())
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
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        varTrans.Rollback();
                        LogHelper.WriteError("[执行数据库事务失败]:" + ex.Message);
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 返回数据集合
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string sql, string dbPath)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbPath + ";" + "Version=3"))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        using (SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd))
                        {
                            sda.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteError("[SqliteHelper_ExecuteDataTable]:" + e.Message);
                return dt;
            }
            return dt;
        }
        /// <summary>
        /// 批量插入大数据
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="comoanyCode">公司编码</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static bool InsertBigData(string sql, string comoanyCode, string userId)
        {
            SQLiteTransaction transaction = null;
            try
            {
                lock (obj)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnStr(comoanyCode, userId)))
                    {
                        conn.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                        {
                            transaction = conn.BeginTransaction();
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            //conn.Close();
                            transaction.Commit();
                            //cmd.Dispose();
                            //conn.Dispose();
                        }
                        LogHelper.WriteDebug("SqliteHelper_InsertBigData:" + sql);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_InsertBigData]:" + ex.Message);
                transaction.Rollback();
                return false;
            }
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns></returns>
        public static int CreateDataBase(string path, string databaseName)
        {
            int result = 0;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    SQLiteConnection.CreateFile(path + "\\" + databaseName);
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_CreateDataBase]:" + ex.Message);
                return 0;
            }
            return result;
        }
        private static object objTable = new object();
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        /// <param name="tableName">表名</param>
        /// <param name="creatTableStr">创建表</param>
        /// <returns></returns>
        public static int CreateTable(string dbPath, string tableName, string creatTableStr)
        {
            try
            {
                lock (objTable)
                {
                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbPath + ";" + "Version=3"))
                    {

                        conn.Open();
                        string sqlStr = "create table " + tableName + " " + creatTableStr;
                        using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, conn))
                        {
                            cmd.ExecuteNonQuery();
                            //cmd.Dispose();
                            //conn.Close();
                            //conn.Dispose();
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_CreateTable]:" + ex.Message);
                return -1;
            }
        }
        /// <summary>
        /// DataTable转换IList类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ModelConvertHelper<T> where T : new()
        {
            /// <summary>
            ///  DataTable转换IList
            /// </summary>
            /// <param name="dt">DataTable数据源</param>
            /// <returns></returns>
            public static IList<T> ConvertToModel(DataTable dt)
            {
                try
                {
                    IList<T> ts = new List<T>();
                    Type type = typeof(T);
                    string tempName = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        T t = new T();
                        PropertyInfo[] propertys = t.GetType().GetProperties();
                        foreach (PropertyInfo pi in propertys)
                        {
                            tempName = pi.Name;
                            if (dt.Columns.Contains(tempName))
                            {
                                if (!pi.CanWrite) continue;
                                object value = dr[tempName];
                                if (value != DBNull.Value)
                                    pi.SetValue(t, value, null);
                            }
                        }
                        ts.Add(t);
                    }
                    return ts;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError("[SqliteHelper_ConvertToModel]:" + ex.Message);
                    return null;
                }
            }
        }
        /// <summary>
        /// 启动初始化数据库和表
        /// </summary>
        /// <param name="companyCode">公司编码</param>
        /// <param name="sendUserId">当前登录用户ID</param>
        /// <returns></returns>
        public static bool Initialize(string companyCode, string sendUserId)
        {
            try
            {
                //检查有没有数据库
                if (!File.Exists(publicMethod.localDataPath()  + companyCode + "\\" + sendUserId + "\\" + sendUserId + ".db"))
                {
                    //创建数据库
                    string path = publicMethod.localDataPath()  + companyCode + "\\" + sendUserId;
                    if (SqliteHelper.CreateDataBase(path, sendUserId + ".db") > 0)
                    {
                        #region
                        ////创建单聊表
                        //string tableStr = "(MTP VARCHAR(64),CHATINDEX VARCHAR(64),CONTENT VARCHAR(4000),MESSAGEID VARCHAR(64),SENDTIME VARCHAR(64),SENDUSERID VARCHAR(64),SESSIONID VARCHAR(64),TARGETID VARCHAR(64),SENDORRECEIVE VARCHAR(2),SENDSUCESSORFAIL int,flag varchar(2),READTIME VARCHAR(100),uploadOrDownPath VARCHAR(200), primary key(SESSIONID,CHATINDEX))";
                        //int one = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_Chat_Message", tableStr);

                        ////创建群聊表
                        //string group = "(MTP VARCHAR(64),CHATINDEX VARCHAR(64),CONTENT VARCHAR(4000),MESSAGEID VARCHAR(64),SENDTIME VARCHAR(64),SENDUSERID VARCHAR(64),SESSIONID VARCHAR(64),TARGETID VARCHAR(64),SENDORRECEIVE VARCHAR(2),SENDSUCESSORFAIL int,uploadOrDownPath VARCHAR(200),primary key(SESSIONID,CHATINDEX))";
                        //int two = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_Chat_Message_Group", group);

                        ////创建群聊阅后即焚表
                        //string groupBurn = "(MTP VARCHAR(64),CHATINDEX VARCHAR(64),CONTENT VARCHAR(4000),MESSAGEID VARCHAR(64),SENDTIME VARCHAR(64),SENDUSERID VARCHAR(64),SESSIONID VARCHAR(64),TARGETID VARCHAR(64),SENDORRECEIVE VARCHAR(2),SENDSUCESSORFAIL int,uploadOrDownPath VARCHAR(200),primary key(SESSIONID,CHATINDEX))";
                        //int twos = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_Chat_Message_GroupBurn", groupBurn);

                        ////创建会话列表(只有讨论组才在这里记录阅后即焚相关数据)
                        //string createSessionTable = "(SESSIONID VARCHAR(64),USERID VARCHAR(64),GROUPID VARCHAR(64),UNREADCOUNT INT, LASTMSG VARCHAR(4000), LASTMSGTIMESTAMP  VARCHAR(64),LASTCHATINDEX VARCHAR(64),BURNUNREADCOUNT INT, BURNLASTMSG VARCHAR(4000), BURNLASTMSGTIMESTAMP  VARCHAR(64),BURNLASTCHATINDEX VARCHAR(64),ISBURNMODE int,  PRIMARY KEY(SESSIONID))";
                        //int three = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_Session", createSessionTable);

                        ////创建消息免打扰讨论组,且未读消息数>0(只有讨论组才在这里记录阅后即焚相关数据)
                        ////string createNoRemindGroupTable = "(GROUPID VARCHAR(64),UNREADCOUNT INT,LASTMSG VARCHAR(4000), LASTMSGTIMESTAMP  VARCHAR(64), LASTCHATINDEX VARCHAR(64),BURNUNREADCOUNT INT,BURNLASTMSG VARCHAR(4000), BURNLASTMSGTIMESTAMP  VARCHAR(64), BURNLASTCHATINDEX VARCHAR(64),PRIMARY KEY(GROUPID))";
                        ////int four = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_NoRemindGroup", createNoRemindGroupTable);

                        ////群发助手消息记录
                        //string createMassMsgTable = "(MESSAGEID VARCHAR(64),SENDUSERID VARCHAR(64),TARGETID  VARCHAR(64), COMPANYCODE  VARCHAR(64), CONTENT VARCHAR(4000),SENDTIME VARCHAR(64),CHATINDEX VARCHAR(64), OS INT,SESSIONID VARCHAR(64),PRIMARY KEY(MESSAGEID))";
                        //int five = SqliteHelper.CreateTable(path + "\\" + sendUserId + ".db", "T_MassMsg", createMassMsgTable);
                        //result = 1;
                        #endregion
                        //要修改初始化SQL语句，请修改InitializeSQLite.sql，按照格式
                        var sqlList = SqliteHelper.GetSqlFile(AppDomain.CurrentDomain.BaseDirectory + "InitializeSQLite.sql", 0);//获取初始化sql语句
                        if (sqlList != null && sqlList.Count > 0)
                        {
                            SqliteHelper.ExecuteCommand(sqlList, path + "\\" + sendUserId + ".db");
                        }
                        string newSqliteVersion = publicMethod.xmlFind("sqliteversion", AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                        string insertStr =
                            $"insert into T_Version (VERSION,SENDTIME) values ('{newSqliteVersion}','{DataConverter.ConvertDateTimeInt(DateTime.Now)}')";
                        SqliteHelper.ExecuteNonQuery(insertStr, companyCode, sendUserId);
                    }
                    return true;
                }
                //数据库已经存在 检查版本
                else
                {
                    string selectStr = "select VERSION from T_Version order by SENDTIME desc limit 0,1";
                    string dbPath = publicMethod.localDataPath() + companyCode + "\\" + sendUserId + "\\" + sendUserId + ".db";
                    if (File.Exists(dbPath))
                    {
                        //数据库中存储的版本号
                        string oldSqliteVersion = ExecuteScalar(selectStr, dbPath).ToString();
                        if (!string.IsNullOrEmpty(oldSqliteVersion))
                        {
                            var verStr = oldSqliteVersion.Replace(" ", "").Replace(".", "");
                            LogHelper.WriteDebug("数据库版本号：" + verStr);
                            int oldVer = verStr.Length > 0 ? Convert.ToInt32(verStr) : 0;
                            var sqlList = SqliteHelper.GetSqlFile(AppDomain.CurrentDomain.BaseDirectory + "UpdateSQLite.sql", oldVer);//获取初始化sql语句
                            if (sqlList != null && sqlList.Count > 0)
                            {
                                SqliteHelper.ExecuteCommand(sqlList, publicMethod.localDataPath() + companyCode + "\\" + sendUserId + "\\" + sendUserId + ".db");
                            }
                            //发布的数据库版本号
                            string newSqliteVersion = publicMethod.xmlFind("sqliteversion", AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
                            var newVer = newSqliteVersion.Replace(" ", "").Replace(".", "").Length > 0
                                ? Convert.ToInt32(newSqliteVersion.Replace(" ", "").Replace(".", ""))
                                : 0;
                            if (oldVer < newVer)
                            {
                                string insertStr =
                                    $"insert into T_Version (VERSION,SENDTIME) values ('{newSqliteVersion}','{DataConverter.ConvertDateTimeInt(DateTime.Now)}')";
                                SqliteHelper.ExecuteNonQuery(insertStr, companyCode, sendUserId);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[SqliteHelper_Initialize]:" + ex.Message + ex.Source);
                return false;
            }
        }
    }
}
