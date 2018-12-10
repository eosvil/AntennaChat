using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Antenna.Framework;
using static Antenna.Framework.GlobalVariable;

namespace AntennaChat.Views
{
    /// <summary>
    /// MessageBoxWindowxaml.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        private static ShowDialogResult dialogResult;
        private static MessageBoxWindow window;


        /// <summary>
        ///  以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="messageBoxText">提示内容</param>
        public static void Show(string messageBoxText,GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(messageBoxText);
            SetBottomBtnStyle(MessageBoxButton.OK, false,type);
            window.Owner = Antenna.Framework.Win32.GetTopWindow();
            window.ShowDialog();
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="messageBoxText">提示内容</param>
        public static void Show(string caption, string messageBoxText, GlobalVariable.WarnOrSuccess type)
        {

            window = new MessageBoxWindow(caption, messageBoxText);
            SetBottomBtnStyle(MessageBoxButton.OK, false, type);
            window.Owner = Antenna.Framework.Win32.GetTopWindow();
            window.ShowDialog();
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="messageBoxText">提示内容</param>
        /// <param name="messageBoxButton">需显示的按钮类型</param>
        /// <returns></returns>
        public static ShowDialogResult Show(string messageBoxText, MessageBoxButton messageBoxButton,GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(messageBoxText);
            SetBottomBtnStyle(messageBoxButton, false, type);
            window.Owner = Antenna.Framework.Win32.GetTopWindow();
            window.ShowDialog();
            return dialogResult;
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="messageBoxText">提示内容</param>
        /// <param name="messageBoxButton">需显示的按钮类型</param>
        /// <returns></returns>
        public static ShowDialogResult Show(string caption, string messageBoxText, MessageBoxButton messageBoxButton,GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(caption, messageBoxText);
            SetBottomBtnStyle(messageBoxButton, false, type);
            window.Owner = Antenna.Framework.Win32.GetTopWindow();
            window.ShowDialog();
            return dialogResult;
        }

        /// <summary>
        ///  以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="messageBoxText">提示内容</param>
        public static void Show(string messageBoxText, Window owner,GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(messageBoxText);
            SetBottomBtnStyle(MessageBoxButton.OK, false, type);
            window.Owner = owner;
            window.ShowDialog();
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="messageBoxText">提示内容</param>
        public static void Show(string caption, string messageBoxText, Window owner, GlobalVariable.WarnOrSuccess type)
        {

            window = new MessageBoxWindow(caption, messageBoxText);
            SetBottomBtnStyle(MessageBoxButton.OK, false,type);
            window.Owner = owner;
            window.ShowDialog();
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="messageBoxText">提示内容</param>
        /// <param name="messageBoxButton">需显示的按钮类型</param>
        /// <returns></returns>
        public static ShowDialogResult Show(string messageBoxText, MessageBoxButton messageBoxButton, Window owner, GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(messageBoxText);
            SetBottomBtnStyle(messageBoxButton, false,type);
            window.Owner = owner;
            window.ShowDialog();
            return dialogResult;
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="messageBoxText">提示内容</param>
        /// <param name="messageBoxButton">需显示的按钮类型</param>
        /// <returns></returns>
        public static ShowDialogResult Show(string caption, string messageBoxText, MessageBoxButton messageBoxButton, Window owner, GlobalVariable.WarnOrSuccess type)
        {
            window = new MessageBoxWindow(caption, messageBoxText);
            SetBottomBtnStyle(messageBoxButton, false,type);
            window.Owner = owner;
            window.ShowDialog();
            return dialogResult;
        }

        /// <summary>
        /// 以ShowDialog的方式弹出提示框
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="messageBoxText"></param>
        /// <param name="messageBoxButton"></param>
        /// <param name="type"></param>
        /// <param name="winActivate">窗口是否激活</param>
        /// <returns></returns>
        public static ShowDialogResult Show(string caption, string messageBoxText, MessageBoxButton messageBoxButton, GlobalVariable.WarnOrSuccess type, bool winActivate = true)
        {
            window = new MessageBoxWindow(caption, messageBoxText);
            SetBottomBtnStyle(messageBoxButton, false, type);
            window.Topmost = true;
            window.ShowDialog();
            return dialogResult;
        }
        /// <summary>
        /// 设置底部按钮控件属性
        /// </summary>
        /// <param name="messageBoxButton"></param>
        /// <param name="isUserControl"></param>
        private static void SetBottomBtnStyle(MessageBoxButton messageBoxButton, bool isUserControl, GlobalVariable.WarnOrSuccess type)
        {
            if (type == GlobalVariable.WarnOrSuccess.Success)
            {
                window.image.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Images/成功提示.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                window.image.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Images/提示.png", UriKind.RelativeOrAbsolute));
            }
            if (messageBoxButton == MessageBoxButton.OK)
            {
                window.btn_No.Visibility = Visibility.Collapsed;
                window.btn_Cancel.Visibility = Visibility.Collapsed;
                window.btn_YES.Content = "确定";
            }
            else if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                window.btn_No.Visibility = Visibility.Collapsed;
                window.btn_Cancel.Visibility = Visibility.Visible;
                window.btn_YES.Content = "确定";
                window.btn_Cancel.Content = "取消";
            }
            else if (messageBoxButton == MessageBoxButton.YesNo)
            {
                window.btn_No.Visibility = Visibility.Visible;
                window.btn_Cancel.Visibility = Visibility.Collapsed;
                window.btn_YES.Content = "是";
                window.btn_No.Content = "否";
            }
            else if (messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                window.btn_No.Visibility = Visibility.Visible;
                window.btn_Cancel.Visibility = Visibility.Visible;
                window.btn_YES.Content = "是";
                window.btn_No.Content = "否";
                window.btn_Cancel.Content = "取消";
            }
        }
        private MessageBoxWindow(string messageBoxText)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            tbContent.Text = messageBoxText;
        }
        private MessageBoxWindow(string caption, string messageBoxText) : this(messageBoxText)
        {
            txtCaption.Text = caption;
        }

        private MessageBoxWindow(UserControl control)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            gridContent.Children.Clear();
            gridContent.Children.Add(control);
        }

        private MessageBoxWindow(string caption, UserControl control) : this(control)
        {
            txtCaption.Text = caption;
        }

        private void btn_YES_Click(object sender, RoutedEventArgs e)
        {
            if (window.btn_YES.Content.ToString() == "是")
            {
                dialogResult = ShowDialogResult.Yes;
            }
            else
            {
                dialogResult = ShowDialogResult.Ok;
            }
            this.Close();
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            dialogResult = ShowDialogResult.No;
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogResult = ShowDialogResult.Cancel;
            this.Close();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            dialogResult = ShowDialogResult.Cancel;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
