using AntennaChat.ViewModel.Talk;
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

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// AfterReadBrunWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AfterReadBrunWindow : Window
    {
        public AfterReadBrunWindow()
        {
            InitializeComponent();
            this.DataContext = new AfterReadBrunViewModel();
        }
    }
}
