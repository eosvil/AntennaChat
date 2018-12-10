using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SDK.AntSdk.AntModels;
using SDK.Service;

namespace SDK.AntSdk.DAL
{
    /// <summary>
    /// 会话列表数据访问类
    /// </summary>
    public class T_SessionDAL : IBaseDal<AntSdkTsession>
    {
        public AntSdkTsession GetModelByKey(string key)
        {
            try
            {
                AntSdkTsession session = null;
                string selectStr = string.Format("select  * from T_Session where SessionId= '{0}'", key);
                using (SQLiteConnection conn = new SQLiteConnection(AntSdkSqliteHelper.ConnStr(AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                    AntSdkService.AntSdkLoginOutput.userId)))
                {
                    SQLiteDataReader dataReader = AntSdkSqliteHelper.ExecuteReader(conn, selectStr);
                    if (dataReader == null) return null;
                    if (dataReader.Read())
                    {
                        //"(SESSIONID VARCHAR(64),USERID VARCHAR(64),GROUPID VARCHAR(64),UNREADCOUNT INT, LASTMSG VARCHAR(4000), LASTMSGTIMESTAMP  VARCHAR(64),LASTCHATINDEX VARCHAR(64),BURNUNREADCOUNT INT, BURNLASTMSG VARCHAR(4000), BURNLASTMSGTIMESTAMP  VARCHAR(64),BURNLASTCHATINDEX VARCHAR(64)，PRIMARY KEY(SESSIONID))";
                        session = new AntSdkTsession();
                        session.SessionId = dataReader.GetValue(0).ToString();
                        session.UserId = dataReader.GetValue(1).ToString();
                        session.GroupId = dataReader.GetValue(2).ToString();
                        session.UnreadCount = dataReader.GetInt32(3);
                        session.LastMsg = dataReader.GetValue(4).ToString();
                        session.LastMsgTimeStamp = dataReader.GetValue(5).ToString();
                        session.LastChatIndex = dataReader.GetValue(6).ToString();
                        session.BurnUnreadCount = dataReader.GetInt32(7);
                        session.BurnLastMsg = dataReader.GetValue(8).ToString();
                        session.BurnLastMsgTimeStamp = dataReader.GetValue(9).ToString();
                        session.BurnLastChatIndex = dataReader.GetValue(10).ToString();
                        session.IsBurnMode = dataReader.GetInt32(11);
                        if(dataReader["topIndex"] == DBNull.Value)
                            session.TopIndex=null;
                        else
                            session.TopIndex = dataReader.GetInt32(12);
                    }
                    dataReader.Close();
                    return session;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[T_SessionDAL_GetModelByKey]" + ex.Message);
                return null;
            }

        }

        public int Delete(AntSdkTsession model)
        {
            string deleteStr = string.Format("delete from T_Session where SESSIONID='{0}'", model.SessionId);
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public IList<AntSdkTsession> GetList(AntSdkTsession t = null)
        {
            DataTable dt = GetDataTable();
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0) return null;
            IList<AntSdkTsession> dataList = AntSdkSqliteHelper.ModelConvertHelper<AntSdkTsession>.ConvertToModel(dt);
            return dataList;
        }

        private DataTable GetDataTable()
        {
            string selectStr = "select  * from T_Session ";
            string dbPath =
                $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkLoginOutput.userId}\{
                    AntSdkService.AntSdkLoginOutput.userId}.db";
            return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
        }

        public int Insert(AntSdkTsession model)
        {
            if (model.LastMsg != null) model.LastMsg = model.LastMsg.Replace("'", "''");
            if (model.BurnLastMsg != null) model.BurnLastMsg = model.BurnLastMsg.Replace("'", "''");
            string topIndex  = model.TopIndex.HasValue ? model.TopIndex.Value.ToString() : "null";
            //string createSessionTable = "(SESSIONID VARCHAR(64),USERID VARCHAR(64),GROUPID VARCHAR(64),UNREADCOUNT INT,LASTMSG VARCHAR(4000), LASTMSGTIME  VARCHAR(64),LASTMODIFYTIME DATETIME,primary key(SESSIONID))";
            string insertStr =
                $"INSERT INTO T_Session(SESSIONID,USERID,GROUPID,UNREADCOUNT,LASTMSG,LASTMSGTIMESTAMP,LASTCHATINDEX,BURNUNREADCOUNT,BURNLASTMSG,BURNLASTMSGTIMESTAMP,BURNLASTCHATINDEX,ISBURNMODE,TopIndex) VALUES " +
                $"('{model.SessionId}','{model.UserId}','{model.GroupId}','{model.UnreadCount}','{model.LastMsg}','{model.LastMsgTimeStamp}','{model.LastChatIndex}','{model.BurnUnreadCount}','{model.BurnLastMsg}','{model.BurnLastMsgTimeStamp}','{model.BurnLastChatIndex}','{model.IsBurnMode}',{topIndex})";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public int Update(AntSdkTsession model)
        {
            if (model.LastMsg != null) model.LastMsg = model.LastMsg.Replace("'", "''");
            if (model.BurnLastMsg != null) model.BurnLastMsg = model.BurnLastMsg.Replace("'", "''");
            string topIndex = model.TopIndex.HasValue ? model.TopIndex.Value.ToString() : "null";
            string updateStr = $"update T_Session set  UNREADCOUNT='{model.UnreadCount}',LASTMSG='{model.LastMsg}',LastMsgTimeStamp='{model.LastMsgTimeStamp}',LastChatIndex='{model.LastChatIndex}', BURNUNREADCOUNT='{model.BurnUnreadCount}',BURNLASTMSG='{model.BurnLastMsg}',BURNLastMsgTimeStamp='{model.BurnLastMsgTimeStamp}',BURNLastChatIndex='{model.BurnLastChatIndex}' ,ISBURNMODE='{model.IsBurnMode}',TopIndex={topIndex} where SESSIONID='{model.SessionId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public int UpdateTopIndex(string sessionId,int? topIndex)
        {
            string updateStr;
            if (topIndex.HasValue)
                updateStr = @"update T_Session set  TOPINDEX=" + topIndex.Value+" where SESSIONID='"+ sessionId+"'";
            else
            {
                updateStr = @"update T_Session set  TOPINDEX=null where SESSIONID='" + sessionId + "'";
            }
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public int NoReadCount()
        {
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkLoginOutput.userId}\{
                    AntSdkService.AntSdkLoginOutput.userId}.db";
            string strCount = "select max(unreadcount) as count from T_Session";
            return Convert.ToInt32(AntSdkSqliteHelper.ExecuteScalar(strCount, dbPath));
        }
    }
}
