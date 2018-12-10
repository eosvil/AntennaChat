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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Antenna.UserControls.SettingAndMessageBox
{
    /// <summary>
    /// 个人资料设置窗体
    /// </summary>
    public partial class Win_Profile : Window
    {
        Dictionary<string, object> controlsDictionary;
        public Win_Profile()
        {
            InitializeComponent();
            ProfileControl profileControl = new ProfileControl();
            profileControl.ImageClickEvent += ProfileControl_ImageClickEvent;
            grid_control.Children.Add(profileControl);
            controlsDictionary = new Dictionary<string, object>();
            controlsDictionary.Add("profile", profileControl);
        }
        /// <summary>
        /// 修改头像处理
        /// </summary>
        private void ProfileControl_ImageClickEvent()
        {
            if (!controlsDictionary.ContainsKey("head"))
            {
                HeadImageControl headImageControl = new HeadImageControl();
                headImageControl.CancelButtonClickEvent += HeadImageControl_CancelButtonClickEvent;
                headImageControl.OKButtonClickEvent += HeadImageControl_SaveButtonClickEvent;
                grid_control.Children.Clear();
                grid_control.Children.Add(headImageControl);
                controlsDictionary.Add("head", headImageControl);
            }
            else
            {
                HeadImageControl control = (HeadImageControl)controlsDictionary["head"];
                grid_control.Children.Clear();
                grid_control.Children.Add(control);
            }
        }
        /// <summary>
        /// 确认按钮处理
        /// </summary>
        /// <param name="path"></param>
        private void HeadImageControl_SaveButtonClickEvent(string path)
        {
            string imagePaht = path;
            ProfileControl control = (ProfileControl)controlsDictionary["profile"];
            grid_control.Children.Clear();
            grid_control.Children.Add(control);
        }

        /// <summary>
        /// 取消按钮处理
        /// </summary>
        private void HeadImageControl_CancelButtonClickEvent()
        {
            ProfileControl control = (ProfileControl)controlsDictionary["profile"];
            grid_control.Children.Clear();
            grid_control.Children.Add(control);
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
