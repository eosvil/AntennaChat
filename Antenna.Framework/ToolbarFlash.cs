using System;
using System.Runtime.InteropServices;

namespace Antenna.Framework
{
    public class ToolbarFlash
    {
        [DllImport("user32.dll")]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr handle, bool invert);

        struct FLASHWINFO
        {
            public UInt32 cbSize; //该结构的字节大小 
            public IntPtr hwnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的 
            public UInt32 dwFlags; //闪烁的状态，可以是下面取值之一或组合： 
            public UInt32 uCount;//闪烁窗口的次数 
            public UInt32 dwTimeout;//窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度 
        }

        enum falshType : uint
        {
            FLASHW_STOP = 0,//停止闪烁
            FLASHW_CAPTION = 1,//只闪烁标题
            FLASHW_TRAY = 2,//只闪烁任务栏
            FLASHW_ALL = 3,//标题和任务栏同时闪烁
            FLASHW_TIMER = 4,//不停地闪烁，直到FLASHW_STOP标志被设置 
            FLASHW_TIMERNOFG = 12// 不停地闪烁，直到窗口前端显示 
        }

        public static void Flash(IntPtr handle, uint count)
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = handle;
            //fInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMER;
            fInfo.dwFlags =(uint) falshType.FLASHW_TRAY;
            //fInfo.uCount = UInt32.MaxValue;
            fInfo.uCount = count;
            fInfo.dwTimeout = 0;
            FlashWindowEx(ref fInfo);
        }

        public static void Flash(IntPtr handle)
        {
            FlashWindow(handle, true);
        }
    }
}
