using Antenna.Framework;
using Antenna.Model;
using Antenna.UserControls.SettingAndMessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Antenna.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        LoginOutput output;
        ObservableCollection<string> UserCollection = new ObservableCollection<string>();
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for(int i=1;i<4;i++)
            {
                string id = i.ToString() + "369852" + i.ToString();
                UserCollection.Add(id);
            }
            cmb_id.ItemsSource = UserCollection;
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_small_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            if (Login())
            {
                MainWindow mainWindow = new MainWindow( output);
                mainWindow.Show();
                this.Close();
            }
        }

        private bool Login()
        {
            LoginInput input = new LoginInput();
            input.os = (int)GlobalVariable.OSType.PC;
            input.osStr = "";
            input.version = GlobalVariable.Version;
            input.loginName = cmb_id.Text;
            input.password = txt_pwd.PasswordStr;
            input.manufacture = "";
            output = new LoginOutput();
            string errMsg = string.Empty;
            if( !HttpService.Instance.Login(input, ref output, ref errMsg))
            {
                MessageBoxWindow.Show("登录失败:" + errMsg,this);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        /// <summary>
        /// 删除选中项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string tag = button.Tag.ToString();
            UserCollection.Remove(tag);
        }
    }
}
