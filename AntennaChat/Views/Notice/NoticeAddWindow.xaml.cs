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

namespace AntennaChat.Views.Notice
{
    /// <summary>
    /// NoticeAddWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NoticeAddWindow : UserControl
    {
        public NoticeAddWindow()
        {
            InitializeComponent();
        }

        private void TextTitle_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Enter)
                e.Handled = true;
        }

        private void NoticeAddWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Margin = "40,20,40,40"
            if (e.WidthChanged)
            {
                if (e.NewSize.Width <= 700)
                {
                    borderNotice.Margin = new Thickness(40, 20, 60, 40);
                }
                else
                {
                    borderNotice.Margin = new Thickness(40, 20, 40, 40);
                }
            }
        }
    }
}
