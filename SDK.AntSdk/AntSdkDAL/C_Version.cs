using SDK.AntSdk.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntSdkDAL
{
    /// <summary>
    /// 组织架构版本号
    /// </summary>
    public class C_Version : IBaseDal<string>
    {
        /// <summary>
        /// 根据key查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetModelByKey(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(string model)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IList<string> GetList(string t = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 单条插入
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public int Insert(string version)
        {
            var sqlStr = $"INSERT INTO C_Version(VERSION) VALUES ('{version}')";
            return !string.IsNullOrEmpty(sqlStr) ? AntSdkSqliteHelper.ExecuteNonQuery(sqlStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId) : 0;

        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public int Update(string version)
        {
            var updateStr = $"update C_Version set  VERSION='{version}'";
            return AntSdkSqliteHelper.ExecuteNonQuery(updateStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public string Select()
        {
            const string sqlStr = "select VERSION from C_Version";
            string dbPath =$@"{AntSdkService.SqliteLocalDbPath}{AntSdkService.AntSdkLoginOutput.userId}\{AntSdkService.AntSdkLoginOutput.userId}.db";
            var checkStr = AntSdkSqliteHelper.ExecuteScalar(sqlStr, dbPath);
            if (checkStr == null)
            {
                return string.Empty;
            }
            var s = checkStr.ToString();
            return s;
        }
        /// <summary>
        /// 全部清空
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int AllDelete(string t = null)
        {
            var deleteStr = "delete from C_Version";
            return AntSdkSqliteHelper.ExecuteNonQuery(deleteStr, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkLoginOutput.userId);
        }
    }
}
