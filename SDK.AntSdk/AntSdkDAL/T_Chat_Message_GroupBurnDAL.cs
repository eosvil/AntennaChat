using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.DAL
{
    public class T_Chat_Message_GroupBurnDAL : IDAL<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>
    {
        public SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }

        public int Delete(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
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
        public DataTable GetDataTable(string session_id, string userId, string targetid, string companyCode, int index,
            int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' order by cast(chatindex as int) desc limit " + index + "," + pageSize + ") order by cast(chatindex as int); ";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 根据messageId获取消息
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public DataTable GetChatMessageByMsessageID(string sessionId,string messageId)
        {
            string selectStr = "select  * from t_chat_message_groupburn where sessionid='" + sessionId + "' and MESSAGEID='"+ messageId +"'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }

        public IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> GetList(
            SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            throw new NotImplementedException();
        }

        public IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> GetListPage(string session_id, string userId,
            string companyCode, int index, int pageSize)
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
            string selectStr = "select count(*) from t_chat_message_groupburn where sessionid='" + session_id + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(AntSdkSqliteHelper.ExecuteScalar(selectStr, dbPath));
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
        public int Insert(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            if (model.sourceContent != null) model.sourceContent = model.sourceContent.Replace("'", "''");
            string insertStr =
                "insert into t_chat_message_groupburn(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL,uploadordownpath) values ('" +
                (int)model.MsgType + "','" + model.chatIndex + "','" + model.sourceContent + "','" + model.messageId +
                "','" + AntSdkDataConverter.ConvertDateTimeToIntLong(DateTime.Now) + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId +
                "','" + model.SENDORRECEIVE + "','" + model.sendsucessorfail + "','" + model.uploadOrDownPath + "')";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                model.sendUserId);
        }
        /// <summary>
        /// 批量上传
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool InsertBig(List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into t_chat_message_groupburn(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL,uploadordownpath) values ");
            foreach (var model in list)
            {
                if (model.sourceContent != null) model.sourceContent = model.sourceContent.Replace("'", "''");
                sb.Append("('" + (int)model.MsgType + "','" + model.chatIndex + "','" + model.sourceContent + "','" + model.messageId + "','" + AntSdkDataConverter.ConvertDateTimeToIntLong(DateTime.Now) + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId + "','" + model.SENDORRECEIVE + "','" + model.sendsucessorfail + "','" + model.uploadOrDownPath + "'),");
            }
            var str = sb.ToString().Substring(0, sb.ToString().Length - 1);
            bool b = AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
            return b;
        }
        /// <summary>
        /// 接受消息入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ReceiveInsert(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            if (model.sourceContent != null) model.sourceContent = model.sourceContent.Replace("'", "''");
            string insertStr =
                "insert into T_Chat_Message_Group(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL) values ('" +
                (int)model.MsgType + "','" + model.chatIndex + "','" + model.sourceContent + "','" + model.messageId +
                "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId +
                "','" + model.SENDORRECEIVE + "','" + 1 + "')";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                AntSdkService.AntSdkLoginOutput.userId);
        }

        /// <summary>
        /// 群组接受阅后即焚消息入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ReceiveInsertBurn(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            if (model.sourceContent != null) model.sourceContent = model.sourceContent.Replace("'", "''");
            string insertStr =
                "insert into t_chat_message_groupburn(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL) values ('" +
                (int)model.MsgType + "','" + model.chatIndex + "','" + model.sourceContent + "','" + model.messageId +
                "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId +
                "','" + model.SENDORRECEIVE + "','" + 1 + "')";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                AntSdkService.AntSdkLoginOutput.userId);
        }

        /// <summary>
        /// 多端数据插入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertSelfData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            if (model.sourceContent != null) model.sourceContent = model.sourceContent.Replace("'", "''");
            string insertStr =
                "insert into t_chat_message_groupburn(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL) values ('" +
                (int)model.MsgType + "','" + model.chatIndex + "','" + model.sourceContent + "','" + model.messageId +
                "','" + model.sendTime + "','" + model.sendUserId + "','" + model.sessionId + "','" + model.targetId +
                "','" + model.SENDORRECEIVE + "','" + 1 + "')";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                AntSdkService.AntSdkLoginOutput.userId);
        }

        public int Update(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase model)
        {
            string insertStr = "update t_chat_message_groupburn set chatindex='" + model.chatIndex + "',sendtime='" +
                               model.sendTime + "',SENDSUCESSORFAIL='1' where messageid='" + model.messageId + "'";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                model.sendUserId);
        }

        /// <summary>
        /// 查询当天前N条聊天记录
        /// </summary>
        /// <param name="session_id">会话ID</param>
        /// <param name="startTimestamp">开始时间戳</param>
        /// <param name="endTimestamp">结束时间戳</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        public DataTable GetCurrentDayHistoryMsg(string session_id, string startTimestamp, string endTimestamp,
            int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and sendtime between '" + startTimestamp + "' and '" + endTimestamp +
                               "' order by sendtime asc limit 0," + pageSize + ") order by sendtime; ";
            string dbPath =
                $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                    AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
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
            string selectStr = "select count(*) from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and chatindex<'" + start_index + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(AntSdkSqliteHelper.ExecuteScalar(selectStr, dbPath));
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
            string selectStr = "select count(*) from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and chatindex>'" + end_index + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return Convert.ToInt32(AntSdkSqliteHelper.ExecuteScalar(selectStr, dbPath));
            }
            else
            {
                return 0;
            }
        }

        public DataTable GetHistoryPrevious(string session_id, string start_index, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and chatindex< '" + start_index + "' order by chatindex desc limit 0," + pageSize +
                               ") order by sendtime asc;";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }

        public DataTable GetHistoryNext(string session_id, string end_index, int pageSize)
        {
            string selectStr = "select * from (select  * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and chatindex> '" + end_index + "' order by chatindex asc limit 0," + pageSize +
                               ") order by sendtime asc;";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 群聊滚动第一次查询
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="startChatIndex"></param>
        /// <returns></returns>
        public DataTable getDataByScroll(string session_id, string companyCode, string userId, string startChatIndex,
            int pageCount)
        {
            string selectStr = "select * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and cast(chatindex as int) <" + startChatIndex +
                               " order by cast(chatindex as int) desc limit '" + 0 + "',10";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 群聊滚动大于1次查询
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="startChatIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public DataTable getDataByMoreThanScroll(string session_id, string companyCode, string userId,
            string startChatIndex, int pageCount)
        {
            string selectStr = "select * from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and cast(chatindex as int) <" + startChatIndex + " order by cast(chatindex as int) desc limit '" + 0 +
                               "',10";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
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
            string selectStr =
                "select max(cast(chatindex as int)) as chatindex from t_chat_message_groupburn where sessionid='" +
                session_id + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                var result = AntSdkSqliteHelper.ExecuteScalar(selectStr, dbPath);
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
            string selectStr = "select chatindex from t_chat_message_groupburn where sessionid='" + session_id +
                               "' order by cast(chatindex as int) desc limit " + unreadCount + ",1;";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                var result = AntSdkSqliteHelper.ExecuteScalar(selectStr, dbPath);
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
                return "0";
            }
        }

        /// <summary>
        /// 接收文件之后更新文件路径
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="chatIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public int updateFilePathAndFlag(string session_id, string companyCode, string userId, string chatIndex,
            string path)
        {
            string updateStr = "update t_chat_message_groupburn set uploadordownpath='" + path + "' where sessionid='" +
                               session_id + "' and messageid='" + chatIndex + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 删除阅后即焚对应的session会话内容
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int DeleteBurnData(string session_id, string companyCode, string userId)
        {
            string deleteStr = "delete from t_chat_message_groupburn where sessionid='" + session_id + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据maxchatindex删除阅后即焚内容
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="chatIndex"></param>
        /// <returns></returns>
        public int DeleteBurnDataByChatIndex(string session_id, string companyCode, string userId, string chatIndex)
        {
            string deleteStr = "delete from t_chat_message_groupburn where sessionid='" + session_id +
                               "' and cast(chatindex as int)<=" + chatIndex + "";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        public int GetPreOneChatIndex(string userId,string sessionid)
        {
            int result = 0;
            var selectStr = "select max(cast(chatindex as int)) from t_chat_message_groupburn where sessionid='"+sessionid+"'";
            string dbpath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbpath))
            {
                var maxChatIndex = AntSdkSqliteHelper.ExecuteScalar(selectStr, dbpath).ToString().Trim();
                if (maxChatIndex == "")
                {
                    result = 0;
                }
                else
                {
                    result = Convert.ToInt32(maxChatIndex);
                }
            }
            return result;
        }
        /// <summary>
        /// 更新发送成功标识 成功SENDORRECEIVE为1
        /// </summary>
        /// <param name="messageid"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int UpdateSendMsgState(string messageid, string companyCode, string userId)
        {
            string updateStr = "update t_chat_message_groupburn set sendsucessorfail='1' where messageid='" + messageid + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 更新语音消息发送成功标识
        /// </summary>
        /// <param name="messageid"></param>
        /// <param name="content"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int UpdateSendVoiceMsgState(string messageid, string content, string companyCode, string userId)
        {
            string updateStr = "update t_chat_message_groupburn set CONTENT='" + content + "' where messageid='" + messageid + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 消息重发状态更改
        /// </summary>
        /// <param name="messageid"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int UpdateReSendMsgState(string messageid, string companyCode, string userId, string chatindex)
        {
            string updateStr = "update t_chat_message_groupburn set sendsucessorfail='1',chatindex='" + chatindex + "' where messageid='" + messageid + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 内容更改
        /// </summary>
        /// <param name="messageid"></param>
        /// <param name="companyCode"></param>
        /// <param name="userId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public int UpdateContent(string messageid, string companyCode, string userId, string content)
        {
            string updateStr = "update t_chat_message_groupburn set content='" + content + "' where messageid='" + messageid + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{userId}\{userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, companyCode, userId);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 更新数据库语音状态 0未读 1已读
        /// </summary>
        /// <param name="messageid"></param>
        /// <returns></returns>
        public int UpdateVoiceState(string messageid)
        {
            string updateStr = "update t_chat_message_groupburn set voiceread='1' where messageid='" + messageid + "'";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db";
            if (File.Exists(dbPath))
            {
                return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkLoginOutput.companyCode, AntSdkService.AntSdkLoginOutput.userId);
            }
            else
            {
                return 0;
            }
        }
    }
}
