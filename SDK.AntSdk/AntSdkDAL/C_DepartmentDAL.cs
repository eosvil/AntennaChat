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
    /// <summary>
    /// 组织架构-部门信息
    /// </summary>
    public class C_DepartmentDAL : IBaseDal<AntSdkContact_Depart>
    {
        /// <summary>
        /// 根据key查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AntSdkContact_Depart GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(AntSdkContact_Depart model)
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
            var deleteStr = "delete from C_DEPARTMENT";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 根据父级部门ID删除部门
        /// </summary>
        /// <param name="DeleteByParentDepartId"></param>
        /// <returns></returns>
        public int DeleteByParentDepartId(string parentDepartId)
        {
            var deleteStr = $"delete from C_DEPARTMENT where parentDepartId='{parentDepartId}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IList<AntSdkContact_Depart> GetList(AntSdkContact_Depart t = null)
        {
            var dt = GetDataTable();
            if (dt?.Rows == null || dt.Rows.Count == 0) return null;
            var dataList = AntSdkSqliteHelper.ModelConvertHelper<AntSdkContact_Depart>.ConvertToModel(dt);
            return dataList;
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <returns></returns>
        private DataTable GetDataTable()
        {
            string selectStr = "select  * from C_DEPARTMENT ";
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
        public int Insert(AntSdkContact_Depart model)
        {
            var sqlStr = GetInsertSqlStr(model);
            return !string.IsNullOrEmpty(sqlStr) ? AntSdkSqliteHelper.ExecuteNonQuery(sqlStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId) : 0;
        }
        /// <summary>
        /// 单条更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(AntSdkContact_Depart model)
        {
            var updateStr = GetUpdateSqlStr(model);
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns></returns>
        public bool Insert(List<AntSdkContact_Depart> modelList)
        {
            #region 采用拼接方式
            var sbSql = new StringBuilder();
            sbSql.Append(
                "INSERT OR REPLACE INTO C_DEPARTMENT(departName,departmentId,parentDepartId) values");
            foreach (var list in modelList)
            {
                sbSql.Append("('" + list.departName + "','" + list.departmentId + "','" + list.parentDepartId + "'),");
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
            //        $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}\{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db");
            //}
            //return false;
        }
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="modelList"></param>
        public void Update(List<AntSdkContact_Depart> modelList)
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
        public void Delete(List<AntSdkContact_Depart> modelList)
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
        private string GetInsertSqlStr(AntSdkContact_Depart user)
        {
            var sqlStr = $"INSERT INTO C_DEPARTMENT(departName,departmentId,parentDepartId) VALUES " +
            $"('{user.departName}','{user.departmentId}','{user.parentDepartId}')";
            return sqlStr;
        }
        /// <summary>
        /// 获取SQL更新语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetUpdateSqlStr(AntSdkContact_Depart model)
        {
            var updateStr = $"update C_DEPARTMENT set departName='{model.departName}',parentDepartId='{model.parentDepartId}' where departmentId='{model.departmentId}'";
            return updateStr;
        }
        /// <summary>
        /// 获取SQL删除语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetDeleteSqlStr(AntSdkContact_Depart model)
        {
            var deleteStr = $"delete from C_DEPARTMENT where departmentId='{model.departmentId}'";
            return deleteStr;
        }
    }
}
