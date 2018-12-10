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
    /// 个人资料设置控件
    /// </summary>
    public partial class ProfileControl : UserControl
    {
        #region 委托事件
        public delegate void ImageClick();//定义头像点击委托
        public event ImageClick ImageClickEvent;//定义头像点击事件
        #endregion
        #region 字段
        /// <summary>
        /// true-自己个人资料 false-联系人资料
        /// </summary>
        private bool isOthers = true;
        #endregion
        public ProfileControl()
        {
            InitializeComponent();
            SetProfileReadOnly(false);
        }
        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isOthers)
                OnClick();
        }
        /// <summary>
        /// 头像点击触发
        /// </summary>
        private void OnClick()
        {
            ImageClickEvent?.Invoke();
        }
        /// <summary>
        /// 设置个人资料是否可编辑
        /// </summary>
        /// <param name="isReadOnly"></param>
        private void SetProfileReadOnly(bool isReadOnly)
        {
            txt_Name.IsReadOnly = isReadOnly;
            cmb_Sex.IsEnabled = !isReadOnly;
            txt_Department.IsReadOnly = isReadOnly;
            txt_Email.IsReadOnly = isReadOnly;
            txt_Job.IsReadOnly = isReadOnly;
            txt_Signature.IsReadOnly = isReadOnly;
            txt_Tel.IsReadOnly = IsEnabled;
        }
    }
}
