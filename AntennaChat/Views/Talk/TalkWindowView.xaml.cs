using Antenna.Framework;
using AntennaChat.ViewModel.Talk;
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
using static Antenna.Model.SendMessageDto;
using static AntennaChat.ViewModel.Talk.TalkViewModel;

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// TalkWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class TalkWindowView : UserControl
    {
        public TalkWindowView()
        {
            InitializeComponent();
        }

        private void Rich_OnLostFocus(object sender, RoutedEventArgs e)
        {
            KeyboardHookLib.isHook = true;
            rich.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            hello.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            gridSend.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
        }

        private void Rich_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //KeyboardHookLib.isHook = false;
            rich.Background = new SolidColorBrush(Colors.Transparent);
            hello.Background = new SolidColorBrush(Colors.Transparent);
            gridSend.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
