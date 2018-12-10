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

namespace AntennaChat.Views.Setting
{
    /// <summary>
    /// ImageDealer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageDealer : UserControl
    {
        public static readonly RoutedEvent OnCutImagingEventHandler = EventManager.RegisterRoutedEvent("OnCutImaging", RoutingStrategy.Bubble,
          typeof(RoutedEventHandler), typeof(ImageDealer));
        public static readonly DependencyProperty BitSourceProperty = DependencyProperty.Register("BitSource", typeof(BitmapSource), typeof(ImageDealer), new FrameworkPropertyMetadata(OnBitSourceChanged));

        private static void OnBitSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageDealer send = d as ImageDealer;
            _BitSource = send.BitSource;
            _imageDealerControl.BitSource = send.BitSource;
        }
        #region 私有字段
        /// <summary>
        /// 截图控件
        /// </summary>
        private static ImageDealerUnsafe _imageDealerControl = new ImageDealerUnsafe();
        /// <summary>
        /// 图片源
        /// </summary>
        private static BitmapSource _BitSource;
        private int _ChangeMargin = 1;
        #endregion

        #region 公共字段

        /// <summary>
        /// 图片源
        /// </summary>
        public BitmapSource BitSource
        {
            get { return (BitmapSource)GetValue(BitSourceProperty); }
            set
            {
                SetValue(BitSourceProperty, value);           
            }
        }

        /// <summary>
        /// 截图事件
        /// </summary>
        public event RoutedEventHandler OnCutImaging
        {
            add { base.AddHandler(OnCutImagingEventHandler, value); }
            remove { base.RemoveHandler(OnCutImagingEventHandler, value); }
        }

        #endregion

        #region ==方法==

        public ImageDealer()
        {
            InitializeComponent();
            _imageDealerControl.OnCutImage += this.OnCutImage;
        }
        //外部截图
        public void CutImage()
        {
            if (this.IsLoaded == true || _imageDealerControl == null)
            {
                _imageDealerControl.CutImage();
            }
            else
            {
                throw new Exception("尚未创建视图时无法截图！");
            }
        }
        //截图控件位置初始化
        private void LocateInit()
        {
            double Margin = 1;
            if (_BitSource != null)
            {
                double percent = 1;
                //根据最小倍率放大截图控件
                percent = (_BitSource.PixelHeight * 1.0 / this.ActualHeight);
                percent = percent < (_BitSource.PixelWidth * 1.0 / this.ActualWidth) ? (_BitSource.PixelWidth * 1.0 / this.ActualWidth) : percent;
                _imageDealerControl.Width = _BitSource.PixelWidth * 1.0 / percent;
                _imageDealerControl.Height = _BitSource.PixelHeight * 1.0 / percent;
                //初始化截图方块
                _imageDealerControl.ImageArea.Width = _imageDealerControl.ImageArea.Height = 100 + _ChangeMargin;
                _ChangeMargin = -_ChangeMargin;
                _imageDealerControl.ImageArea.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                _imageDealerControl.ImageArea.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                _imageDealerControl.ImageArea.SetValue(MarginProperty, new Thickness(0));
                //截图控件相对父控件Margin
                _imageDealerControl.Width -= 2 * Margin;
                _imageDealerControl.Height -= 2 * Margin;
                _imageDealerControl.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                _imageDealerControl.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                _imageDealerControl.SetValue(MarginProperty, new Thickness(0));
            }
        }

        #endregion

        #region ==事件==

        //截图回调
        private void OnCutImage(BitmapSource bit)
        {
            RaiseEvent(new RoutedEventArgs(OnCutImagingEventHandler, bit));
        }
        //加载完成
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LocateInit();
            if (this.MainGrid.Children.Contains(_imageDealerControl) == false)
            {
                this.MainGrid.Children.Add(_imageDealerControl);
                _imageDealerControl.Width = this.ActualWidth;
                _imageDealerControl.Height = this.ActualHeight;
                _imageDealerControl.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                _imageDealerControl.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                _imageDealerControl.SetValue(MarginProperty, new Thickness(0));
            }
            CutImage();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MainGrid.Children.Clear();
        }
    }
}
