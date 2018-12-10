using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AntennaChat.Resource
{
    [StyleTypedProperty(Property = "WatermarkStyle", StyleTargetType = typeof(TextBlock))]
    public class WatermarkPasswordBox : TextBox
    {
        static WatermarkPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkPasswordBox), new FrameworkPropertyMetadata(typeof(WatermarkPasswordBox)));
        }

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public Style WatermarkStyle
        {
            get { return (Style)GetValue(WatermarkStyleProperty); }
            set { SetValue(WatermarkStyleProperty, value); }
        }

        public static Style GetWatermarkStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(WatermarkStyleProperty);
        }

        public static void SetWatermarkStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(WatermarkStyleProperty, value);
        }

        public static readonly DependencyProperty WatermarkStyleProperty =
            DependencyProperty.RegisterAttached("WatermarkStyle", typeof(Style), typeof(WatermarkPasswordBox));

        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(WatermarkPasswordBox),
            new FrameworkPropertyMetadata(OnWatermarkChanged));

        private static void OnWatermarkChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PasswordBox)
            {
                PasswordBox pwdBox = sender as PasswordBox;
                if (pwdBox == null)
                {
                    return;
                }
                pwdBox.PasswordChanged -= OnPasswordChanged;
                pwdBox.PasswordChanged += OnPasswordChanged;
            }
            else
            {
                TextBox tb = sender as TextBox;
                if (tb == null) return;
                tb.TextChanged -= Tb_TextChanged;
                tb.TextChanged += Tb_TextChanged;
            }
        }
        private static void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox pwdBox = sender as TextBox;
            Label watermarkTextBlock = pwdBox.Template.FindName("ChildWateMark", pwdBox) as Label;

            if (watermarkTextBlock != null)
            {
                watermarkTextBlock.Visibility = pwdBox.Text.Length == 0
         ? Visibility.Visible : Visibility.Hidden;
            }
        }
        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pwdBox = sender as PasswordBox;
            TextBlock watermarkTextBlock = pwdBox.Template.FindName("WatermarkTextBlock", pwdBox) as TextBlock;

            if (watermarkTextBlock != null)
            {
                watermarkTextBlock.Visibility = pwdBox.SecurePassword.Length == 0
         ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
    public class NullOrEmptyStringToVisibilityConverter : IValueConverter
    {
        public NullOrEmptyStringToVisibilityConverter()
        {
            NullOrEmpty = Visibility.Collapsed;
            NotNullOrEmpty = Visibility.Visible;
        }

        public Visibility NullOrEmpty { get; set; }
        public Visibility NotNullOrEmpty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value == null ? string.Empty : value.ToString();
            return string.IsNullOrEmpty(strValue) ? NullOrEmpty : NotNullOrEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
