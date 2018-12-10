using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AntennaChat.Views.Activity
{
    /// <summary>
    /// ReleaseActivityView.xaml 的交互逻辑
    /// </summary>
    public partial class ReleaseActivityView : UserControl
    {
        public ReleaseActivityView()
        {
            InitializeComponent();
            this.Loaded += ReleaseActivityView_Loaded;
        }

       

        private void ReleaseActivityView_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtBoxActivity.Focus();
            this.txtBoxActivity.Focusable = true;
        }

        private void TextTitle_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Enter)
                e.Handled = true;
        }

        private void TxtBoxActivity_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxActivity.Text))
            {
                activityTitleBorder.Visibility = Visibility.Visible;
                txtBlockActivityTitle.Visibility = Visibility.Visible;
                txtBlockActivityTitle.Text = "活动名称不能为空";
            }

        }

        private void TxtBoxActivity_OnGotFocus(object sender, RoutedEventArgs e)
        {
            activityTitleBorder.Visibility = Visibility.Collapsed;
            txtBlockActivityTitle.Text = "";
            txtBlockActivityTitle.Visibility = Visibility.Collapsed;

        }

        private void TxtActivityIntroduce_OnGotFocus(object sender, RoutedEventArgs e)
        {
            tbActivityIntroduceError.Visibility = Visibility.Collapsed;
            //borderActivityIntroduce.BorderBrush = new SolidColorBrush(Color.FromArgb(255,244,244,244));
        }

        private void TxtActivityIntroduce_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtActivityIntroduce.Text))
            {
                tbActivityIntroduceError.Visibility=Visibility.Visible;
                //borderActivityIntroduce.BorderBrush =new SolidColorBrush(Colors.Red);
            }
        }
    }
}
