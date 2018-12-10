using Antenna.Model;
using AntennaChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Antenna.Framework;
using static Antenna.Framework.Win32;
using System.Net;
using System.IO;
using SDK.AntSdk;

namespace AntennaChat.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindowView : Window
    {
        LoginWindowViewModel model;                                                                           
        public LoginWindowView()
        {
            InitializeComponent();
            model = new LoginWindowViewModel();
            //model.LoginSuccessEvent += Model_LoginSuccessEvent;
            DataContext = model;
            //this.Closed += LoginWindowView_Closed;
            //Application.Current.Exit += (s, e) =>
            //{
            //    taskbarIcon.Dispose();
            //};
        }

        private void LoginWindowView_Closed(object sender, EventArgs e)
        {
            taskbarIcon.Dispose();
        }

        /// <summary>
        /// 登录成功
        /// </summary>
        private void Model_LoginSuccessEvent()
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindowViewModel model = new MainWindowViewModel();
                //model.GetGroupList();
                MainWindowView mainWindow = new MainWindowView { DataContext = model };
                //model.InitMainVM();
                mainWindow.Show();
                this.Close();
            });
            stopWatch.Stop();
            Antenna.Framework.LogHelper.WriteDebug($"[Model_LoginSuccessEvent({stopWatch.Elapsed.TotalMilliseconds}毫秒)]");
        }

        #region 接收进程间消息
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                IntPtr handle = hwndSource.Handle;
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            ProcessParam param = (ProcessParam)this.Tag;
            if (!param.IsLoginValidate) return IntPtr.Zero;
            const int WM_COPYDATA = 0x004A;
            if (msg == WM_COPYDATA)
            {
                param.ReceiveProcessCount++;
                COPYDATASTRUCT cds = (COPYDATASTRUCT)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                //this.tbReturn.Text = ("已收到消息:" + cds.lpData);
                if (cds.lpData == this.cmb_id.Text)
                {
                    param.IsLoginValidate = false;
                    //ThreadPool.QueueUserWorkItem(o =>
                    //{
                    //    this.Dispatcher.Invoke(new Action(() => LoginFault()));
                    //});
                }
            }
            this.Tag = param;
            return IntPtr.Zero;
        }
        public void LoginFault()
        {
            loading_Grid.Visibility = Visibility.Collapsed;
            pop_tip.IsOpen = true;
            lab_tips.Text = "该账号已经登录，不能重复登录";
        }
        #endregion
    }
}
