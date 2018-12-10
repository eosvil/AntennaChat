using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;
using SDK.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntSdkDAL
{
    /// <summary>
    /// 组织架构-联系人信息
    /// </summary>
    public class C_User_InfoDAL : IBaseDal<AntSdkContact_User>
    {
        /// <summary>
        /// 根据key查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AntSdkContact_User GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(AntSdkContact_User model)
        {
            var deleteStr = GetDeleteSqlStr(model);
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 全部清空
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int AllDelete(AntSdkContact_User t = null)
        {
            var deleteStr = "delete from C_USER_INFO";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 根据部门ID删除成员
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public int DeleteByDepartmentId(string departmentId)
        {
            var deleteStr = $"delete from C_USER_INFO where departmentId='{departmentId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IList<AntSdkContact_User> GetList(AntSdkContact_User t = null)
        {
            var dt = GetDataTable();
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;
            var dataList = AntSdkSqliteHelper.ModelConvertHelper<AntSdkContact_User>.ConvertToModel(dt);
            return dataList;
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <returns></returns>
        private DataTable GetDataTable()
        {
            string selectStr = "select  * from C_USER_INFO ";
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
        public int Insert(AntSdkContact_User model)
        {
            var sqlStr = GetInsertSqlStr(model);
            return !string.IsNullOrEmpty(sqlStr) ? AntSdkSqliteHelper.ExecuteNonQuery(sqlStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId) : 0;
        }
        /// <summary>
        /// 单条更新q
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(AntSdkContact_User model)
        {
            var updateStr = GetUpdateSqlStr(model);
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(AntSdkReceivedUserMsg.Modify model)
        {
            var updateStr = $"update C_USER_INFO set departmentId='{model.attr.departmentId}',position='{model.attr.position}',picture='{model.attr.picture}'" +
                $",SEX='{model.attr.sex}',signature='{model.attr.signature}' where userId='{model.userId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(string userId,int status)
        {
            var updateStr = $"update C_USER_INFO set status='{status}' where userId='{userId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public bool Insert(List<AntSdkContact_User> modelList)
        {
            #region 采用拼接方式
            var sbSql = new StringBuilder();
            sbSql.Append(
                "INSERT OR REPLACE INTO C_USER_INFO(departmentId,position,userId,userName,picture,userNum,accToken,signature,status) values");
            foreach (var list in modelList)
            {
                sbSql.Append("('" + list.departmentId + "','" + list.position + "','" + list.userId + "','" +
                             list.userName + "','" + list.picture + "','" + list.userNum + "','" + list.accToken + "','" +
                             list.signature + "','" +
                             list.status + "'),");
            }
            var str = sbSql.ToString().Substring(0, sbSql.ToString().Length - 1);
            return AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode,
                AntSdkService.AntSdkCurrentUserInfo.userId);
            #endregion

            //var sqlList = new ArrayList();
            //foreach (var sqlStr in modelList.Select(GetInsertSqlStr).Where(sqlStr => !string.IsNullOrEmpty(sqlStr)))
            //{
            //    sqlList.Add(sqlStr);
            //}
            //if (sqlList.Count > 0)
            //{
            //    return AntSdkSqliteHelper.ExecuteCommand(sqlList,
            //          $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}\{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db");
            //}
            //return true;
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="modelList"></param>
        public void Update(List<AntSdkContact_User> modelList)
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
        public void Delete(List<AntSdkContact_User> modelList)
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
        private string GetInsertSqlStr(AntSdkContact_User user)
        {
            var sqlStr = $"INSERT INTO C_USER_INFO(departmentId,position,userId,userName,picture,userNum,accToken,signature,accid) VALUES " +
            $"('{user.departmentId}','{user.position}','{user.userId}','{user.userName}','{user.picture}','{user.userNum}','{user.accToken}','{user.signature}','{user.accid}')";
            return sqlStr;
        }
        /// <summary>
        /// 获取SQL更新语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetUpdateSqlStr(AntSdkContact_User model)
        {
            var updateStr = $"update C_USER_INFO set  departmentId='{model.departmentId}',position='{model.position}',userName='{model.userName}', picture='{model.picture}'" +
                                             $",userNum='{model.userNum}',accToken='{model.accToken}',accid='{model.accid}',signature='{model.signature}' where userId='{model.userId}'";
            return updateStr;
        }
        /// <summary>
        /// 获取SQL删除语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetDeleteSqlStr(AntSdkContact_User model)
        {
            var deleteStr = $"delete from C_USER_INFO where userId='{model.userId}'";
            return deleteStr;
        }
    }
}
