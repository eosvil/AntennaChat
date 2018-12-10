using Antenna.Framework;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace AntennaChat.ViewModel
{
    public class WindowBaseViewModel : PropertyNotifyObject
    {
        protected Point OriginalPoint = new Point();

        /// <summary>
        /// 记录正常状态窗口尺寸
        /// </summary>
        private Rect rcnormal;

        /// <summary>
        /// 窗体关闭
        /// </summary>
        private ActionCommand<Window> _CloseWindow;

        public ActionCommand<Window> CloseWindow
        {
            get
            {
                if (this._CloseWindow == null)
                {
                    this._CloseWindow = new ActionCommand<Window>(
                        o =>
                        {
                            if (o != null)
                            {
                                o.Close();
                            }
                        });
                }
                return this._CloseWindow;
            }
        }

        /// <summary>
        /// 最小化窗体到托盘
        /// </summary>
        private ActionCommand<Window> _MinWindowToTray;

        public ActionCommand<Window> MinWindowToTray
        {
            get
            {
                if (this._MinWindowToTray == null)
                {
                    this._MinWindowToTray = new ActionCommand<Window>(
                        o =>
                        {
                            if (o != null)
                            {
                                o.WindowState = WindowState.Minimized;
                                o.Hide();
                                o.ShowInTaskbar = false;
                            }
                        });
                }
                return this._MinWindowToTray;
            }
        }

        /// <summary>
        /// 最大化
        /// </summary>
        private ActionCommand<Window> _MaxWindow;

        public ActionCommand<Window> MaxWindow
        {
            get
            {
                if (this._MaxWindow == null)
                {
                    this._MaxWindow = new ActionCommand<Window>(MaxWindowAction);
                }
                return this._MaxWindow;
            }
        }

        private bool _isMaxWin;

        /// <summary>
        /// 窗体是否最大化
        /// </summary>
        public bool IsMaxWin
        {
            get { return _isMaxWin; }
            set
            {
                _isMaxWin = value;
                RaisePropertyChanged(() => IsMaxWin);
            }
        }

        private int _mainWindowMargin = 8;

        public int MainWindowMargin
        {
            get { return _mainWindowMargin; }
            set
            {
                _mainWindowMargin = value;
                RaisePropertyChanged(() => MainWindowMargin);
            }
        }

        /// <summary>
        /// 最大化Action
        /// </summary>
        /// <param name="win"></param>
        private void MaxWindowAction(Window win)
        {
            if (win.ActualHeight < SystemParameters.WorkArea.Height || win.ActualWidth < SystemParameters.WorkArea.Width)
            {
                win.WindowState = WindowState.Normal;
                rcnormal = new Rect(win.Left, win.Top, win.Width, win.Height); //保存下当前位置与大小
                win.Left = 0; //设置位置
                win.Top = 0;
                Rect rc = SystemParameters.WorkArea; //获取工作区大小
                win.Width = rc.Width;
                win.Height = rc.Height;
                IsMaxWin = true;
                MainWindowMargin = 0;
            }

            else
            {
                win.WindowState = WindowState.Normal;
                win.Left = rcnormal.Left;
                win.Top = rcnormal.Top;
                win.Width = rcnormal.Width;
                win.Height = rcnormal.Height;
                IsMaxWin = false;
                MainWindowMargin = 8;
            }

        }

        /// <summary>
        /// 最小化
        /// </summary>
        private ActionCommand<Window> _MinimizeWindow;

        public ActionCommand<Window> MinimizeWindow
        {
            get
            {
                if (this._MinimizeWindow == null)
                {
                    this._MinimizeWindow = new ActionCommand<Window>(
                        o =>
                        {
                            if (o != null)
                            {
                                o.WindowState = WindowState.Minimized;
                            }
                        });
                }
                return this._MinimizeWindow;
            }
        }

        /// <summary>
        /// 移动
        /// </summary>
        private ActionCommand<Window> _MoveWindow;

        public ActionCommand<Window> MoveWindow
        {
            get
            {
                if (this._MoveWindow == null)
                {
                    this._MoveWindow = new ActionCommand<Window>(
                        o =>
                        {
                            if (o != null)
                            {
                                try
                                {
                                    MainClick();
                                    Win32.SendMessage(new WindowInteropHelper(o).Handle, WM_SYSCOMMAND,
                                        SC_MOVE + HTCAPTION, 0);
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        });
                }
                return this._MoveWindow;
            }
        }

        /// <summary>
        /// 主窗口点击
        /// </summary>
        public virtual void MainClick()
        {

        }

        #region 主窗体移动变量

        public const int WM_SYSCOMMAND = 0x0112; //该变量表示将向Windows发送的消息类型
        public const int SC_MOVE = 0xF010; //该变量表示发送消息的附加消息
        public const int HTCAPTION = 0x0002; //该变量表示发送消息的附加消息

        #endregion

        // 创建结构体用于返回捕获时间 
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            // 设置结构体块容量 
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            // 捕获的时间 
            [MarshalAs(UnmanagedType.U4)] public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        //获取键盘和鼠标没有操作的时间
        public static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            // 捕获时间 
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;
            var tickCount = Environment.TickCount - vLastInputInfo.dwTime;
            var second = tickCount/1000;
            long minute = 0;
            if (second > 60)
            {
                minute = second/60;
            }
            return minute;
        }
    }
}
