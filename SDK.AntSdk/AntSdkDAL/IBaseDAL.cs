using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDK.AntSdk.DAL
{
    public interface IBaseDal<T> where T : class
    {
        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="model">对应的实体</param>
        /// <returns>返回添加的结果</returns>
        int Insert(T model);
        /// <summary>
        /// 修改操作
        /// </summary>
        /// <param name="model">对应的实体</param>
        /// <returns>返回修改的结果</returns>
        int Update(T model);
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int Delete(T model);

        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="model">对应的实体</param>
        /// <returns>返回数据集合</returns>
        IList<T> GetList(T model=null );

        /// <summary>
        /// 通过关键字获取单条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetModelByKey(string key);
    }
}
