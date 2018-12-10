using SDK.AntSdk.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDK.AntSdk.BLL
{
    /// <summary>
    /// 业务逻辑层基类规范
    /// </summary>
    public class BaseBLL<T, D>
        where T : class
        where D : IBaseDal<T>, new()
    {
        private D dal = new D();
        /// <summary>
        /// 插入数据操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int Insert(T model)
        {
            return dal.Insert(model);
        }
        /// <summary>
        /// 更新数据操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int Update(T model)
        {
            return dal.Update(model);
        }
        /// <summary>
        /// 删除数据操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int Delete(T model)
        {
            return dal.Delete(model);
        }
        /// <summary>
        /// 获取对应条件的集合
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual IList<T> GetList(T model=null )
        {
            return dal.GetList(model);
        }

        public virtual T GetModelByKey(string key)
        {
            return dal.GetModelByKey(key);
        }
    }
}
