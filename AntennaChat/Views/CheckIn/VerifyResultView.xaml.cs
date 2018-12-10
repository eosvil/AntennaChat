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
using System.Windows.Threading;

namespace AntennaChat.Views
{
    /// <summary>
    /// VerifyResultView.xaml 的交互逻辑
    /// </summary>
    public partial class VerifyResultView : Window
    {
        private DispatcherTimer timer;
        public VerifyResultView()
        {
            InitializeComponent();
            this.Loaded += VerifyResultView_Loaded;
        }

        private void VerifyResultView_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
            }
            this.Close();
        }
    }
}
