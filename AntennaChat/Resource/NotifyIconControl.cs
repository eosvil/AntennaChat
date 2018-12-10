using Antenna.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AntennaChat.Resource
{
    /// <summary>
    /// 应用程序最小化托盘控件
    /// </summary>
    /// 创建者：赵雪峰 20161121
    public class NotifyIconControl
    {
        public NotifyIcon notifyIcon;
        #region 单例模式（线程安全）
        private volatile static NotifyIconControl _instance = null;
        private static readonly object lockHelper = new object();
        public static NotifyIconControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockHelper)
                    {
                        if (_instance == null)
                            _instance = new NotifyIconControl();
                    }
                }
                return _instance;
            }
        }
        private NotifyIconControl()
        {
           // CreateNotifyIcon();
        }
        #endregion
        public NotifyIcon CreateNotifyIcon()
        {
            try
            {
                notifyIcon = new NotifyIcon();
                notifyIcon.ContextMenu = CreateContextMenu();
                //notifyIcon.BalloonTipText = "Hello, 乐盈通！";
                notifyIcon.Text = "七讯";
                notifyIcon.Icon = new System.Drawing.Icon(System.Environment.CurrentDirectory + "/Images/七讯.ico");
                notifyIcon.Visible = true;
                return notifyIcon;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// 创建鼠标右键弹出菜单
        /// </summary>
        /// 20160602 
        /// <returns></returns>
        private ContextMenu CreateContextMenu()
        {
            ContextMenu cms = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem itemLogout = new System.Windows.Forms.MenuItem("注销");
            itemLogout.Name = "Logout";
            
            cms.MenuItems.Add(itemLogout);
            //System.Windows.Forms.MenuItem itemSeparator = new System.Windows.Forms.MenuItem("-");
            //itemSeparator.Name = "Separator";
            //cms.MenuItems.Add(itemSeparator);
            System.Windows.Forms.MenuItem itemExit = new System.Windows.Forms.MenuItem("退出");
            itemExit.Name = "ExitApp";
            cms.MenuItems.Add(itemExit);
            return cms;
        }

        public void SetNotifyIconText(string userName)
        {
            notifyIcon.Text = string.IsNullOrEmpty(userName) ? "七讯" : "七讯：" + userName;
        }
    }
}
