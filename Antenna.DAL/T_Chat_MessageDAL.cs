using Antenna.DAL;
using Antenna.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Antenna.Framework;
using System.IO;
using System.Data.SQLite;

namespace Antenna.DAL
{
    public class T_Chat_MessageDAL : IDAL<ChatMsgReceive>
    {
        public ChatMsgReceive GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }

        public int Delete(ChatMsgReceive model)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="userId"></param>
        /// <param name="targetid"></param>
        /// <param name="companyCode"></param>
        /// <param name="index"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string session_id, string userId, string targetid, string companyCode, int index, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message where sessionid='" + session_id + "' order by sendtime desc limit " + index + "," + pageSize + ") order by sendtime; ";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                LogHelper.WriteDebug("[t_chat_message_groupburn-GetDataTable:]:" + "sql:" + selectStr + "数据库:" + dbPath);
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }

        }

        public IList<ChatMsgReceive> GetList(ChatMsgReceive model)
        {
            throw new NotImplementedException();
        }

        public IList<ChatMsgReceive> GetListPage(string session_id, string userId, string companyCode, int index, int pageSize)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 查询记录总条数
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="userId"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public int GetMaxCount(string session_id, string userId, string companyCode)
        {
            string selectStr = "select count(*) from t_chat_message where sessionid='" + session_id + "'";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(SqliteHelper.ExecuteScalar(selectStr, dbPath));
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 发送消息入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Insert(ChatMsgReceive model)
        {
            if (model.content != null) model.content = model.content.Replace("'", "''");
            string insertStr = "insert into T_Chat_Message(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL,uploadOrDownPath,flag) values ('" + model.MTP + "','" + model.chatIndex + "','" + model.content + "','" + model.messageId + "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId + "','" + model.SENDORRECEIVE + "','" + model.sendsucessorfail + "','" + model.uploadOrDownPath + "','" + model.flag + "')";
            return SqliteHelper.ExecuteNonQuery(insertStr, model.companyCode, model.sendUserId);
        }
        /// <summary>
        /// 接受消息入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ReceiveInsert(ChatMsgReceive model)
        {
            if (model.content != null) model.content = model.content.Replace("'", "''");
            string insertStr = "insert into T_Chat_Message(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL,flag) values ('" + model.MTP + "','" + model.chatIndex + "','" + model.content + "','" + model.messageId + "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId + "','" + model.SENDORRECEIVE + "','" + model.sendsucessorfail + "','" + model.flag + "')";
            return SqliteHelper.ExecuteNonQuery(insertStr, model.companyCode, model.targetId);
        }
        /// <summary>
        /// 多端数据插入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertSelfData(ChatMsgReceive model)
        {
            if (model.content != null) model.content = model.content.Replace("'", "''");
            string insertStr = "insert into T_Chat_Message(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL,flag) values ('" + model.MTP + "','" + model.chatIndex + "','" + model.content + "','" + model.messageId + "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId + "','" + model.SENDORRECEIVE + "','" + model.sendsucessorfail + "','" + model.flag + "')";
            return SqliteHelper.ExecuteNonQuery(insertStr, model.companyCode, model.sendUserId);
        }
        public int Update(ChatMsgReceive model)
        {
            if (model.content != null) model.content = model.content.Replace("'", "''");
            string insertStr = "update t_chat_message set chatindex='" + model.chatIndex + "',sendtime='" + model.sendTime + "',SENDSUCESSORFAIL='1' where messageid='" + model.messageId + "'";
            return SqliteHelper.ExecuteNonQuery(insertStr, model.companyCode, model.sendUserId);
        }
        /// <summary>
        /// 查询当天前N条聊天记录
        /// </summary>
        /// <param name="session_id">会话ID</param>
        /// <param name="startTimestamp">开始时间戳</param>
        /// <param name="endTimestamp">结束时间戳</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        public DataTable GetCurrentDayHistoryMsg(string session_id, string startTimestamp, string endTimestamp, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message where sessionid='" + session_id + "' and sendtime between '" + startTimestamp + "' and '" + endTimestamp + "' order by sendtime asc limit 0," + pageSize + ") order by sendtime; ";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 查询选中日期之前的记录总条数
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="start_index"></param>
        /// <returns></returns>
        public int GetHistoryCountPrevious(string session_id, string start_index)
        {
            string selectStr = "select count(*) from t_chat_message where sessionid='" + session_id + "' and chatindex<'" + start_index + "'";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + ".db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(SqliteHelper.ExecuteScalar(selectStr, dbPath));
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 查询选中日期返回的记录最后一条之后的记录条数
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="end_index"></param>
        /// <returns></returns>
        public int GetHistoryCountNext(string session_id, string end_index)
        {
            string selectStr = "select count(*) from t_chat_message where sessionid='" + session_id + "' and chatindex>'" + end_index + "'";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + ".db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(SqliteHelper.ExecuteScalar(selectStr, dbPath));
            }
            else
            {
                return 0;
            }
        }
        public DataTable GetHistoryPrevious(string session_id, string start_index, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message where sessionid='" + session_id + "' and chatindex< '" + start_index + "' order by chatindex desc limit 0," + pageSize + ") order by sendtime asc;";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        public DataTable GetHistoryNext(string session_id, string end_index, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message where sessionid='" + session_id + "' and chatindex> '" + end_index + "' order by chatindex asc limit 0," + pageSize + ") order by sendtime asc;";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + "\\" + AntSdkService.AntSdkCurrentUserInfo.userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 个人滚动第一次查询
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="startChatIndex"></param>
        /// <returns></returns>
        public DataTable getDataByScroll(string session_id, string companyCode, string userId, string startChatIndex, int pageCount)
        {
            string selectStr = "select * from t_chat_message where sessionid='" + session_id + "' and cast(chatindex as int) <='" + startChatIndex + "' and sendtime!='' order by sendtime desc limit '" + 0 + "',10";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 个人滚动大于1次查询
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="startChatIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public DataTable getDataByMoreThanScroll(string session_id, string companyCode, string userId, string startChatIndex, int pageCount)
        {
            string selectStr = "select * from t_chat_message where sessionid='" + session_id + "' and sendtime!='' and cast(chatindex as int) <'" + startChatIndex + "' order by sendtime desc limit '" + 0 + "',10";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 查询当前会话为unreadCount为0情况时候chatindex的值
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string getQueryZeroChatIndex(string session_id, string companyCode, string userId)
        {
            string selectStr = "select max(cast(chatindex as int)) as chatindex from t_chat_message where sessionid='" + session_id + "' and  sendtime!=''";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                var result = SqliteHelper.ExecuteScalar(selectStr, dbPath);
                LogHelper.WriteDebug("[T_Chat_MessageDAL]:" + "sql:" + selectStr + "result:" + result + "数据库:" + dbPath);
                if (result == null)
                {
                    return "0";
                }
                else
                {
                    return result.ToString();
                }

            }
            else
            {
                LogHelper.WriteDebug("[T_Chat_MessageDAL]:没有数据库");
                return "0";
            }
        }
        /// <summary>
        /// 查询当前会话为unreadCount不为0情况时候chatindex的值
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="unreadCount"></param>
        /// <returns></returns>
        public string getQueryNotZeroChatIndex(string session_id, string companyCode, string userId, string unreadCount)
        {
            string selectStr = "select chatindex from t_chat_message where sessionid='" + session_id + "' and sendtime!='' order by cast(chatindex as int) desc limit " + unreadCount + ",1;";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                var result = SqliteHelper.ExecuteScalar(selectStr, dbPath);
                LogHelper.WriteDebug("[T_Chat_MessageDAL_getQueryNotZeroChatIndex]:" + "sql:" + selectStr + "result:" + result + "数据库:" + dbPath);
                if (result == null)
                {
                    return "0";
                }
                else
                {
                    return result.ToString();
                }

            }
            else
            {
                LogHelper.WriteDebug("[T_Chat_MessageDAL_getQueryNotZeroChatIndex]:没有数据库");
                return "0";
            }
        }
        /// <summary>
        /// 更新阅后即焚阅读时间
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="chatIndex"></param>
        /// <returns></returns>
        public int UpdateAfterReadBurnTime(string session_id, string companyCode, string userId, string chatIndex, string readtime)
        {
            string updateStr = "update t_chat_message set readtime='" + readtime + "' where sessionid='" + session_id + "' and chatindex='" + chatIndex + "';";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 阅后即焚倒计时删除
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="chatIndex"></param>
        /// <param name="readtime"></param>
        /// <returns></returns>
        public int DeleteAfterReadBurnMessageByChatIndex(string session_id, string companyCode, string userId, string chatIndex)
        {
            string updateStr = "delete from t_chat_message where sessionid='" + session_id + "' and chatindex='" + chatIndex + "';";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 接收文件之后更新文件路径和是否为阅后即焚消息
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="chatIndex"></param>
        /// <param name="path"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int updateFilePathAndFlag(string session_id, string companyCode, string userId, string chatIndex, string path)
        {
            string updateStr = "update t_chat_message set uploadordownpath='" + path + "' where sessionid='" + session_id + "' and chatindex='" + chatIndex + "'";
            string dbPath = publicMethod.localDataPath() + companyCode + "\\" + userId + "\\" + userId + ".db";
            if (File.Exists(dbPath))
            {
                return SqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {


                return 0;
            }
        }

        /// <summary>
        /// 获取上一条消息
        /// </summary>
        /// <param name="chatIndex"></param>
        /// <returns></returns>
        public ChatMsgReceive GetBeforeRecordByChatIndex(string chatIndex, string sessionId)
        {
            ChatMsgReceive charMsg = null;
            string selectStr = $"select  * from t_chat_message  where chatIndex<>'{chatIndex}' and sendtime<>'' and sendtime not null  and sessionId='{sessionId}' order by rowid desc limit 0,1";
            using (SQLiteConnection conn = new SQLiteConnection(SqliteHelper.ConnStr(GlobalVariable.CompanyCode, AntSdkService.AntSdkLoginOutput.userId)))
            {
                SQLiteDataReader dataReader = SqliteHelper.ExecuteReader(conn,selectStr);
                if (dataReader == null) return null;
                if (dataReader.Read())
                {
                    charMsg = new ChatMsgReceive();
                    //直接用GetString,如果值为DBNull，会报错
                    //charMsg.MTP = dataReader.GetString(0);
                    charMsg.MTP = dataReader.GetValue(0).ToString();
                    charMsg.chatIndex = dataReader.GetValue(1).ToString();
                    charMsg.content = dataReader.GetValue(2).ToString();
                    charMsg.messageId = dataReader.GetValue(3).ToString();
                    charMsg.sendTime = dataReader.GetValue(4).ToString();
                    charMsg.sendUserId = dataReader.GetValue(5).ToString();
                    charMsg.sessionId = dataReader.GetValue(6).ToString();
                    charMsg.targetId = dataReader.GetValue(7).ToString();
                    charMsg.SENDORRECEIVE = dataReader.GetValue(8).ToString();
                    charMsg.sendsucessorfail = int.Parse(dataReader.GetValue(9).ToString());
                    charMsg.flag = dataReader.GetValue(10).ToString();
                    charMsg.readtime = dataReader.GetValue(11).ToString();
                    charMsg.uploadOrDownPath = dataReader.GetValue(12).ToString();
                }
                dataReader.Close();
                return charMsg;
            }
        }
    }
}
