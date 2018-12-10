using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Framework;
using Antenna.Model;

namespace Antenna.DAL
{
    public class T_MassMsgDAL : IBaseDAL<MassMsg_ctt>
    {
        public MassMsg_ctt GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }

        public int Delete(MassMsg_ctt model)
        {
            string deleteStr = $"delete from T_MassMsg where MESSAGEID='{model.messageId}'";
            return SqliteHelper.ExecuteNonQuery(deleteStr, GlobalVariable.CompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public IList<MassMsg_ctt> GetList(MassMsg_ctt t = null)
        {
            DataTable dt = GetDataTable();
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;
            IList<MassMsg_ctt> dataList = SqliteHelper.ModelConvertHelper<MassMsg_ctt>.ConvertToModel(dt);
            return dataList;
        }

        private DataTable GetDataTable()
        {
            string selectStr = "select * from T_MassMsg  order by rowid   limit 0,50";
            string dbPath = publicMethod.localDataPath() + GlobalVariable.CompanyCode + "\\" + AntSdkService.AntSdkLoginOutput.userId + "\\" + AntSdkService.AntSdkLoginOutput.userId + ".db";
            return SqliteHelper.ExecuteDataTable(selectStr, dbPath);
        }

        public int Insert(MassMsg_ctt model)
        {
            if (model.content != null)
                model.content = model.content.Replace("'", "''");
            string insertStr = $"insert into T_MassMsg(messageId,sendUserId,targetId,companyCode,content,os,sessionId,sendTime,chatIndex) values " +
                              $"('{model.messageId}','{model.sendUserId}','{model.targetId}','{model.companyCode}','{model.content}','{model.os}','{model.sessionId}','{model.sendTime}','{model.chatIndex}')";
            return SqliteHelper.ExecuteNonQuery(insertStr, GlobalVariable.CompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }

        public int Update(MassMsg_ctt model)
        {
            if (string.IsNullOrEmpty(model.chatIndex))
            {
                return 0;
            }
            string updateStr = $"update T_MassMsg set sendTime='{model.sendTime}',chatIndex='{model.chatIndex}' where messageId='{model.messageId}'";
            return SqliteHelper.ExecuteNonQuery(updateStr, GlobalVariable.CompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
    }
}
