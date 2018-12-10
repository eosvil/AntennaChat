/*
Author: tanqiyan
Crate date: 2017-06-14
Description：总体控制类
--------------------------------------------------------------------------------------------------------
Versions：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntennaChat.Helper.IHelper;

namespace AntennaChat.Helper
{
    public class WindowMonitor
    {
        /// <summary>
        /// 注册静态事件
        /// </summary>
        public static void Start()
        {
            MessageMonitor.Start();
            SessionMonitor.Start();
        }
        /// <summary>
        /// 取消静态事件的注册
        /// </summary>
        public static void End()
        {
            MessageMonitor.End();
            SessionMonitor.End();
        }

        #region 窗体控制器集合及相应处理方法

        /// <summary>
        /// 窗体控制器集合
        /// </summary>
        private static List<IWindowHelper> _lstWindowHelper;

        /// <summary>
        /// 添加新的窗体控制器
        /// </summary>
        /// <param name="windowHelper"></param>
        public static void AddWindowHelper(IWindowHelper windowHelper)
        {
            if (_lstWindowHelper == null)
            {
                _lstWindowHelper = new List<IWindowHelper>();
            }
            lock (typeof(WindowHelper))
            {
                if (!CheckWindowHelperIsHad(windowHelper.WindowID))
                {
                    windowHelper.IsCurrentTalkWin = true;
                    if (_lstWindowHelper.Count > 0)
                        _lstWindowHelper = _lstWindowHelper.Select(m => { m.IsCurrentTalkWin = false; return m; }).ToList();
                    _lstWindowHelper.Add(windowHelper);
                }
            }
        }
        /// <summary>
        /// 改变窗体信息
        /// </summary>
        /// <param name="windowHelper"></param>
        public static void ChanageWindowHelper(IWindowHelper windowHelper)
        {
            lock (typeof(WindowHelper))
            {
                if (windowHelper != null)
                {
                    if (CheckWindowHelperIsHad(windowHelper.WindowID))
                    {
                        if (_lstWindowHelper.Count > 0)
                            _lstWindowHelper = _lstWindowHelper.Select(m =>
                            {
                                m.IsCurrentTalkWin = m.WindowID == windowHelper.WindowID;
                                return m;
                            }).ToList();
                    }
                }
                else
                {
                    if (_lstWindowHelper?.Count > 0)
                        _lstWindowHelper = _lstWindowHelper.Select(m =>
                        {
                            m.IsCurrentTalkWin = false;
                            return m;
                        }).ToList();
                }
            }
        }

        /// <summary>
        /// 按窗体ID移除窗体控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID、SessionID)</param>
        public static void RemoveWindowHelper(string strID)
        {
            if (CheckWindowHelperIsHad(strID))
            {
                IWindowHelper windowHelper = GetWindowHelperByWindowID(strID);
                _lstWindowHelper.Remove(windowHelper);
            }
        }

        /// <summary>
        /// 按窗体ID获取窗体控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID、SessionID)</param>
        /// <returns></returns>
        public static IWindowHelper GetWindowHelperByWindowID(string strID)
        {
            if (CheckWindowHelperIsHad(strID))
            {
                return _lstWindowHelper.Find(m => m.WindowID == strID);
            }
            return null;
        }

        /// <summary>
        /// 按窗体ID检查是否有符合的窗体控制器
        /// </summary>
        /// <param name="strID">窗体ID(WindowID、SessionID)</param>
        /// <returns></returns>
        public static bool CheckWindowHelperIsHad(string strID)
        {
            if (_lstWindowHelper == null)
            {
                return false;
            }
            return _lstWindowHelper.Exists(m => m.WindowID == strID);
        }
        #endregion
    }
}
