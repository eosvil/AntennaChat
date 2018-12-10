using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Antenna.Framework.GlobalVariable;

namespace AntennaChat.ViewModel.Contacts
{
    /// <summary>
    /// Sqlite队列处理（暂未使用）
    /// </summary>
    public class SqliteQueneHelper
    {
        static object syncLock = new object();
        public static Queue<object> EntityQueue = new Queue<object>();//实体队列
        #region 单例模式（线程安全）
        private volatile static SqliteQueneHelper _instance = null;
        private static readonly object lockObj = new object();
        private SqliteQueneHelper()
        {
            StartMonitor();
        }
        public static SqliteQueneHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObj)
                    {
                        if (_instance == null)
                            _instance = new SqliteQueneHelper();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private  void Enqueue(EntityQueue entity)
        {
            lock (syncLock)
            {
                EntityQueue.Enqueue(entity);
            }
        }

        /// <summary>
        /// 开始监视队列
        /// </summary>
        private  void StartMonitor()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    if (EntityQueue.Count > 0)
                    {
                        EntityQueue entityQueue = null;
                        lock (syncLock)
                        {
                            entityQueue = EntityQueue.Dequeue() as EntityQueue;
                        }
                        if (entityQueue != null)
                        {
                            switch (entityQueue.sqlType)
                            {
                                case SqlType.Insert:
                                    break;
                                case SqlType.Delete:
                                    break;
                                case SqlType.Update:
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }
    }
    /// <summary>
    /// 日志信息和级别
    /// </summary>
    public class EntityQueue
    {
        public EntityQueue(SqlType sqlType, object entity)
        {
            this.sqlType = sqlType;
            this.entity = entity;
        }
        public SqlType sqlType { get; set; }
        public object entity { get; set; }
    }
}
