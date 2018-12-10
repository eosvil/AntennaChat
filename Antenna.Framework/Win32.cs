using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Antenna.Framework
{
    public class Win32
    {
        public enum HitTest : int
        {
            HTERROR = -2,
            HTTRANSPARENT,
            HTNOWHERE,
            HTCLIENT,
            HTCAPTION,
            HTSYSMENU,
            HTGROWBOX,
            HTSIZE = 4,
            HTMENU,
            HTHSCROLL,
            HTVSCROLL,
            HTMINBUTTON,
            HTREDUCE = 8,
            HTMAXBUTTON,
            HTZOOM = 9,
            HTLEFT,
            HTRIGHT,
            HTTOP,
            HTTOPLEFT,
            HTTOPRIGHT,
            HTBOTTOM,
            HTBOTTOMLEFT,
            HTBOTTOMRIGHT,
            HTBORDER,
            HTCLOSE = 20,
            HTHELP
        }

        public struct RECT
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFOEX
        {
            public int cbSize;

            public Win32.RECT rcMonitor;

            public Win32.RECT rcWork;

            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        public struct POINT
        {
            public int x;

            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(
        string lpstrCommand,
        string lpstrReturnString,
        int uReturnLength,
        int hwndCallback
       );
        public struct MINMAXINFO
        {
            public Win32.POINT ptReserved;

            public Win32.POINT ptMaxSize;

            public Win32.POINT ptMaxPosition;

            public Win32.POINT ptMinTrackSize;

            public Win32.POINT ptMaxTrackSize;
        }

        public const int WM_NCLBUTTONDOWN = 161;

        public const int WM_NCHITTEST = 132;

        public const int WM_GETMINMAXINFO = 36;

        public const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In] [Out] Win32.MONITORINFOEX monitorInfo);

        #region 自动获取ShowDialog窗体所属窗体 
        //调用GetActiveWindow然后调用GetWindowFromHwnd
        public static Window GetTopWindow()
        {
            try
            {
                var hwnd = GetActiveWindow();
                if (hwnd == null|| hwnd==IntPtr.Zero)
                    return null;
                return GetWindowFromHwnd(hwnd);
            }
            catch
            {
                return null;
            }
        }
        //从Handle中获取Window对象
        private static Window GetWindowFromHwnd(IntPtr hwnd)
        {
            return (Window)HwndSource.FromHwnd(hwnd).RootVisual;
        }
        //GetActiveWindow API (GetForegroundWindow)
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        #endregion

        #region 进程间通信
        const int WM_COPYDATA = 0x004A;
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern bool PostMessage(
            int hWnd, // handle to destination window
            int Msg, // message
            int wParam, // first message parameter
            ref COPYDATASTRUCT lParam // second message parameter
        );
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public static int SendMsg(string windowName,string content)
        {
            int count = 0;
            //int WINDOW_HANDLER = FindWindow(null, @"欲发送程序窗口的标题");
            IntPtr WINDOW_HANDLER = FindWindow(null, windowName);
            if (WINDOW_HANDLER != IntPtr.Zero)
            {
                count++;
                byte[] sarr = System.Text.Encoding.Default.GetBytes(content);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)100;
                cds.lpData = content;
                cds.cbData = len + 1;
                PostMessage(WINDOW_HANDLER.ToInt32(), WM_COPYDATA, 0, ref cds);
                while (WINDOW_HANDLER != IntPtr.Zero)
                {
                    WINDOW_HANDLER = FindWindowEx(IntPtr.Zero, WINDOW_HANDLER, null, windowName);
                    if (WINDOW_HANDLER != IntPtr.Zero)
                    {
                        PostMessage(WINDOW_HANDLER.ToInt32(), WM_COPYDATA, 0, ref cds);
                        count++;
                    }
                }
            }
            return count;
        }
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        #endregion

        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Point pt);
    }
}
