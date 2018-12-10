using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntSdkDAL
{
    public class C_GROUP : IBaseDal<AntSdkGroupInfo>
    {
        /// <summary>
        /// 根据key查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AntSdkGroupInfo GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(AntSdkGroupInfo model)
        {
            var deleteStr = GetDeleteSqlStr(model);
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 全部清空
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int AllDelete(AntSdkGroupInfo t = null)
        {
            var deleteStr = "delete from C_GROUP";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 根据群ID删除群
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int DeleteByParentDepartId(string groupId)
        {
            var deleteStr = $"delete from C_GROUP where groupId='{groupId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IList<AntSdkGroupInfo> GetList(AntSdkGroupInfo t = null)
        {
            var dt = GetDataTable();
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;
            var dataList = AntSdkSqliteHelper.ModelConvertHelper<AntSdkGroupInfo>.ConvertToModel(dt);
            return dataList;
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <returns></returns>
        private DataTable GetDataTable()
        {
            string selectStr = "select  * from C_GROUP ";
            string dbPath =
                $@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkLoginOutput.userId}\{
                    AntSdkService.AntSdkLoginOutput.userId}.db";
            return AntSdkSqliteHelper.ExecuteDataTable(selectStr, dbPath);
        }
        /// <summary>
        /// 单条插入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Insert(AntSdkGroupInfo model)
        {
            var sqlStr = GetInsertSqlStr(model);
            return !string.IsNullOrEmpty(sqlStr) ? AntSdkSqliteHelper.ExecuteNonQuery(sqlStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId) : 0;
        }
        /// <summary>
        /// 单条更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(AntSdkGroupInfo model)
        {
            var updateStr = GetUpdateSqlStr(model);
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public bool Insert(List<AntSdkGroupInfo> modelList)
        {
            #region 采用拼接方式
            var sbSql = new StringBuilder();
            sbSql.Append(
                "INSERT OR REPLACE INTO C_GROUP(groupId,groupName,groupPicture,COMPANY_CODE,groupOwnerId) values");
            foreach (var list in modelList)
            {
                sbSql.Append("('" + list.groupId + "','" + list.groupName + "','" + list.groupPicture + "','" + AntSdkService.AntSdkLoginOutput.companyCode + "','" + list.groupOwnerId + "'),");
            }
            var str = sbSql.ToString().Substring(0, sbSql.ToString().Length - 1);
            return AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                AntSdkService.AntSdkCurrentUserInfo.userId);

            #endregion
        }
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="modelList"></param>
        public void Update(List<AntSdkGroupInfo> modelList)
        {
            var sqlList = new ArrayList();
            foreach (var sqlStr in modelList.Select(GetUpdateSqlStr).Where(sqlStr => !string.IsNullOrEmpty(sqlStr)))
            {
                sqlList.Add(sqlStr);
            }
            if (sqlList.Count > 0)
            {
                AntSdkSqliteHelper.ExecuteCommand(sqlList,
                    $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}\{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db");
            }
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="modelList"></param>
        public void Delete(List<AntSdkGroupInfo> modelList)
        {
            var sqlList = new ArrayList();
            foreach (var sqlStr in modelList.Select(GetDeleteSqlStr).Where(sqlStr => !string.IsNullOrEmpty(sqlStr)))
            {
                sqlList.Add(sqlStr);
            }
            if (sqlList.Count > 0)
            {
                AntSdkSqliteHelper.ExecuteCommand(sqlList,
                    $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}\{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db");
            }
        }
        /// <summary>
        /// 获取SQL插入语句
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GetInsertSqlStr(AntSdkGroupInfo user)
        {
            var sqlStr = $"INSERT INTO C_GROUP(groupId,groupName,groupPicture,COMPANY_CODE,groupOwnerId) VALUES " +
            $"('{user.groupId}','{user.groupName}','{user.groupPicture}','{AntSdkService.AntSdkLoginOutput.companyCode}','{user.groupOwnerId}')";
            return sqlStr;
        }
        /// <summary>
        /// 获取SQL更新语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetUpdateSqlStr(AntSdkGroupInfo model)
        {
            var updateStr = $"update C_GROUP set groupName='{model.groupName}',groupPicture='{model.groupPicture}' ,COMPANY_CODE='{AntSdkService.AntSdkLoginOutput.companyCode}',groupOwnerId='{model.groupOwnerId}' where groupId='{model.groupId}'";
            return updateStr;
        }
        /// <summary>
        /// 获取SQL删除语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetDeleteSqlStr(AntSdkGroupInfo model)
        {
            var deleteStr = $"delete from C_DEPARTMENT where groupId='{model.groupId}'";
            return deleteStr;
        }
    }
}
