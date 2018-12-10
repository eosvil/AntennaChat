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
    /// 上传头像控件
    /// </summary>
    public partial class HeadImageControl : UserControl
    {
        public delegate void CancelButtonClick();//定义取消按钮点击委托
        public event CancelButtonClick CancelButtonClickEvent;//定义取消按钮点击事件
        public delegate void OKButtonClick(string path);//定义取消按钮点击委托
        public event OKButtonClick OKButtonClickEvent;//定义取消按钮点击事件
        private string imagePath = "11111";
        public HeadImageControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClick();
        }
        /// <summary>
        /// 取消按钮触发
        /// </summary>
        private void OnCancelButtonClick()
        {
            CancelButtonClickEvent?.Invoke();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            OnSaveButtonClick();
        }
        /// <summary>
        /// 确定按钮触发
        /// </summary>
        private void OnSaveButtonClick()
        {
            OKButtonClickEvent?.Invoke(imagePath);
        }
    }
}
