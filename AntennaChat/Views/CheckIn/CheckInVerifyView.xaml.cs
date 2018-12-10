using AntennaChat.ViewModel;
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
using System.Windows.Shapes;

namespace AntennaChat.Views
{
    /// <summary>
    /// CheckInVerifyView.xaml 的交互逻辑
    /// </summary>
    public partial class CheckInVerifyView : Window
    {
        public CheckInVerifyView(bool isPassword=true)
        {
            InitializeComponent();
            if (!isPassword)
                this.Height = 210;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as CheckInVerifyViewModel;
            if (vm != null)
                vm.timer?.Stop();
            MainWindowViewModel.VerifyView = null;
            this.Close();
            //MainWindowViewModel.CloseExitVerify();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
