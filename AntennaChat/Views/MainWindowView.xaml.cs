using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Antenna.Framework.Win32;

namespace AntennaChat.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowView : Window
    {
        private MainWindowViewModel mainWindowVM = null;
        public MainWindowView(MainWindowViewModel model)
        {
            InitializeComponent();
            mainWindowVM = model;
            MainWindowViewModel.MainExhibitControl = ThirdPartViewModel;
            DataContext = model;
            this.StateChanged += MainWindowView_StateChanged;
        }

        private void MainWindowView_StateChanged(object sender, EventArgs e)
        {
            
        }

        public MainWindowView()
        {
            InitializeComponent();
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
            string id = (string)this.Tag;
            const int WM_COPYDATA = 0x004A;
            if (msg == WM_COPYDATA)
            {
                COPYDATASTRUCT cds =
                    (COPYDATASTRUCT)
                        System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                //this.tbReturn.Text = ("已收到消息:" + cds.lpData);
                //if (cds.lpData == mainWindowParams.companyCode + ":" + mainWindowParams.loginName)
                //{
                Win32.SendMsg("七讯登录", id);
                //}
            }
            return IntPtr.Zero;
            TextBox tb = new TextBox();
        }

        #endregion

        private void btn_sugg_Click(object sender, RoutedEventArgs e)
        {
            Talk.PictureViewerView picView = new Talk.PictureViewerView();
            picView.Show();
        }



        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Enter)
            {
                txtSearch.Focusable = true;
                txtSearch.Focus();
            }
        }

        private void MainWindowView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel)
                mainWindowVM = this.DataContext as MainWindowViewModel;
        }
        /// <summary>
        /// 记录正常状态窗口尺寸
        /// </summary>
        private Rect rcnormal;
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                if (this.ActualHeight < SystemParameters.WorkArea.Height ||
                    this.ActualWidth < SystemParameters.WorkArea.Width)
                {
                    if (mainWindowVM != null)
                    {
                        mainWindowVM.IsMaxWin = true;
                        mainWindowVM.MainWindowMargin = 0;

                    }
                    this.WindowState = WindowState.Normal;
                    rcnormal = new Rect(this.Left, this.Top, this.Width, this.Height); //保存下当前位置与大小
                    this.Left = 0; //设置位置
                    this.Top = 0;
                    Rect rc = SystemParameters.WorkArea; //获取工作区大小
                    this.Width = rc.Width;
                    this.Height = rc.Height;

                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    if (mainWindowVM != null)
                    {
                        mainWindowVM.IsMaxWin = false;
                        mainWindowVM.MainWindowMargin = 8;
                    }
                    this.Left = rcnormal.Left;
                    this.Top = rcnormal.Top;
                    this.Width = rcnormal.Width;
                    this.Height = rcnormal.Height;

                }
            }
        }
        private void MainWindowView_OnDrop(object sender, DragEventArgs e)
        {
            
        }

       
    }
}
