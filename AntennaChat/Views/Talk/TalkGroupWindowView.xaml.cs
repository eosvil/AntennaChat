using Antenna.Framework;
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

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// TalkGroupWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class TalkGroupWindowView : UserControl
    {
        public TalkGroupWindowView()
        {
            InitializeComponent();
        }

        private void TalkGroupWindowView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                pop_Blend.Height = e.NewSize.Height - 3;
                pop_Notice.Height = e.NewSize.Height - 3;
            }
        }

        private void Rich_OnLostFocus(object sender, RoutedEventArgs e)
        {
            KeyboardHookLib.isHook = true;
            rich.Background = new SolidColorBrush(Color.FromArgb(255,245,245,245));
            hello.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            gridSend.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
        }

        private void Rich_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //KeyboardHookLib.isHook = false;
            rich.Background=new SolidColorBrush(Colors.Transparent);
            hello.Background = new SolidColorBrush(Colors.Transparent);
            gridSend.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}