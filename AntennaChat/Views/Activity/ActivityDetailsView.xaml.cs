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
using Antenna.Framework;

namespace AntennaChat.Views.Activity
{
    /// <summary>
    /// ActivityDetailsView.xaml 的交互逻辑
    /// </summary>
    public partial class ActivityDetailsView : UserControl
    {
        public ActivityDetailsView()
        {
            InitializeComponent();
            this.Loaded += ActivityDetailsView_Loaded;
        }

        private void ActivityDetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            txtBlock.Visibility = Visibility.Visible;
            var bitmapSource = HeadImage.ImageSource as BitmapSource;
            if (bitmapSource == null)
            {
                txtBlock.Visibility = Visibility.Collapsed;
                return;
            }
            if (bitmapSource.IsDownloading)
            {
                bitmapSource.DownloadCompleted += (obj, args) =>
                {
                    AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                    {
                        txtBlock.Visibility = Visibility.Collapsed;
                    });
                };
            }
            else
            {
                AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                {
                    txtBlock.Visibility = Visibility.Collapsed;
                });
            }
        }
    }
}
