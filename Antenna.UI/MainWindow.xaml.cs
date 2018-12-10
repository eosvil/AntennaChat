using Antenna.Framework;
using Antenna.Model;
using Antenna.UserControls.SettingAndMessageBox;
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
using static Antenna.Framework.GlobalVariable;

namespace Antenna.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 字段
        /// <summary>
        /// 记录正常状态窗口尺寸
        /// </summary>
        private Rect rcnormal;
        #endregion
        public MainWindow(LoginOutput output)
        {
            InitializeComponent();
            InitMqttService(output);
        }

        private void InitMqttService(LoginOutput output)
        {
            string errMsg = string.Empty;
            if(!MqttService .Instance.Connect (output .user.token,ref errMsg))
            {
                MessageBoxWindow.Show(errMsg);
                return;
            }
            List<string> topics = new List<string>();
            topics.Add("C10086");//用户信息相关，组织架构更新(暂时写死用10086)
            topics.Add(output.user.userId);////讨论组新增
            //订阅讨论组ID（讨论组删除，讨论组成员更新，讨论组基本信息更新）
            topics.Add("message_ack");//消息回执格式
            topics.Add(output.user.token);//踢出用户登录
            if (!MqttService.Instance.Subscribe(topics, ref errMsg))
            {
                MessageBoxWindow.Show(errMsg);
                return;
            }
            MqttService.Instance.MessageReceived += MqttMessageReceived;
        }

        private void MqttMessageReceived(string topic, int? mtp, string content)
        {

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
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            MqttService.Instance.MessageReceived -= MqttMessageReceived;
            MqttService.Instance.DisConnect();
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
        /// 最大化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualHeight < SystemParameters.WorkArea.Height || this.ActualWidth < SystemParameters.WorkArea.Width)
            {
                this.WindowState = WindowState.Normal;
                rcnormal = new Rect(this.Left, this.Top, this.Width, this.Height);//保存下当前位置与大小
                this.Left = 0;//设置位置
                this.Top = 0;
                Rect rc = SystemParameters.WorkArea;//获取工作区大小
                this.Width = rc.Width;
                this.Height = rc.Height;
            }
            else
            {
                this.Left = rcnormal.Left;
                this.Top = rcnormal.Top;
                this.Width = rcnormal.Width;
                this.Height = rcnormal.Height;
            }
        }
        /// <summary>
        /// 查看个人资料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void elli_head_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//防止事件冒泡到最顶层
            Win_Profile win = new Win_Profile();
            win.ShowDialog();
        }             
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {
            pop_setting.IsOpen = true;
        }
        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_sys_Click(object sender, RoutedEventArgs e)
        {
            pop_setting.IsOpen = false;
            Win_SystemSetting setting = new Win_SystemSetting();
            setting.ShowDialog();
        }
        /// <summary>
        /// 意见反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_sugg_Click(object sender, RoutedEventArgs e)
        {
            pop_setting.IsOpen = false;
        }
        /// <summary>
        /// 注销登出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_logout_Click(object sender, RoutedEventArgs e)
        {
            pop_setting.IsOpen = false;
        }
    }
}
