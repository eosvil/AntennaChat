using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SDK.AntSdk.DAL
{
    /// <summary>
    /// 数据访问层接口规范
    /// </summary>
    /// <typeparam name="T">对应的实体</typeparam>
    public interface IDAL<T> :IBaseDal<T> where T : class
    {
        #region 从基类IBaseDAL继承
        ///// <summary>
        ///// 添加操作
        ///// </summary>
        ///// <param name="model">对应的实体</param>
        ///// <returns>返回添加的结果</returns>
        //int Insert(T model);
        ///// <summary>
        ///// 修改操作
        ///// </summary>
        ///// <param name="model">对应的实体</param>
        ///// <returns>返回修改的结果</returns>
        //int Update(T model);
        ///// <summary>
        ///// 删除操作
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //int Delete(T model);
        ///// <summary>
        ///// 获取数据集合
        ///// </summary>
        ///// <param name="model">对应的实体</param>
        ///// <returns>返回数据集合</returns>
        //IList<T> GetList(T model);
        #endregion

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="session_id">会话ID</param>
        /// <param name="session_id">客服ID</param>
        /// <param name="companyCode">公司编码</param>
        /// <param name="fEnum">内部通话、讨论组</param>
        /// <param name="index">索引</param>
        /// <param name="pageIndex">页大小</param>
        /// <returns></returns>
        IList<T> GetListPage(string session_id, string userId, string companyCode, int index, int pageSize);
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="session_id">会话ID</param>
        /// <param name="session_id">客服ID</param>
        /// <param name="companyCode">公司编码</param>
        /// <param name="fEnum">内部通话、讨论组</param>
        /// <param name="index">索引</param>
        /// <param name="pageIndex">页大小</param>
        /// <returns></returns>
        DataTable GetDataTable(string session_id, string userId, string targetid, string companyCode, int index, int pageSize);
        /// <summary>
        /// 返回最大条数
        /// </summary>
        /// <returns></returns>
        int GetMaxCount(string session_id, string userId, string companyCode);

    }
}
