using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.AntSdk.AntModels;

namespace SDK.AntSdk.DAL
{
    public class T_MassMsgDAL : IBaseDal<AntSdkMassMsgCtt>
    {
        public AntSdkMassMsgCtt GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }

        public int Delete(AntSdkMassMsgCtt model)
        {
            string deleteStr = $"delete from T_MassMsg where MESSAGEID='{model.messageId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdk.AntSdkService.AntSdkCurrentUserInfo.userId);
        }

        public IList<AntSdkMassMsgCtt> GetList(AntSdkMassMsgCtt t = null)
        {
            DataTable dt = GetDataTable();
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;
            IList<AntSdkMassMsgCtt> dataList = AntSdkSqliteHelper.ModelConvertHelper<AntSdkMassMsgCtt>.ConvertToModel(dt);
            return dataList;
        }

        private DataTable GetDataTable()
        {
            string selectStr = "select * from T_MassMsg  order by rowid   limit 0,50";
            string dbPath = $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkCurrentUserInfo.userId}\{
                AntSdkService.AntSdkCurrentUserInfo.userId}.db";
            return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
        }

        public int Insert(AntSdkMassMsgCtt model)
        {
            if (model.content != null)
                model.content = model.content.Replace("'", "''");
            string insertStr = $"insert into T_MassMsg(messageId,sendUserId,targetId,companyCode,content,os,sessionId,sendTime,chatIndex) values " +
                              $"('{model.messageId}','{model.sendUserId}','{model.targetId}','{model.companyCode}','{model.content}','{model.os}','{model.sessionId}','{model.sendTime}','{model.chatIndex}')";
            return AntSdkSqliteHelper.ExecuteNonQuery(insertStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdk.AntSdkService.AntSdkCurrentUserInfo.userId);
        }

        public int Update(AntSdkMassMsgCtt model)
        {
            if (string.IsNullOrEmpty(model.chatIndex))
            {
                return 0;
            }
            string updateStr = $"update T_MassMsg set sendTime='{model.sendTime}',chatIndex='{model.chatIndex}' where messageId='{model.messageId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdk.AntSdkService.AntSdkCurrentUserInfo.userId);
        }
    }
}
