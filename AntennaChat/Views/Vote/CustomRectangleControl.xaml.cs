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

namespace AntennaChat.Views.Vote
{
    /// <summary>
    /// CustomRectangleControl.xaml 的交互逻辑
    /// </summary>
    public partial class CustomRectangleControl : UserControl
    {
        public CustomRectangleControl()
        {
            InitializeComponent();
        }

        public static int GetOptionVotes(DependencyObject obj)
        {
            var o = obj.GetValue(OptionVotesProperty);
            if (o != null) return (int)o;
            return 0;
        }

        public static void SetOptionVotes(DependencyObject obj, int value)
        {
            obj.SetValue(OptionVotesProperty, value);
        }

        public static readonly DependencyProperty OptionVotesProperty =
            DependencyProperty.RegisterAttached("OptionVotes", typeof(int), typeof(CustomRectangleControl), new PropertyMetadata(0, OnOptionActualWidthChanged));

        public static int GetTotalVotes(DependencyObject obj)
        {
            var o = obj.GetValue(TotalVotesProperty);
            if (o != null)
                return (int)o;
            return 0;
        }

        public static void SetTotalVotes(DependencyObject obj, int value)
        {
            obj.SetValue(TotalVotesProperty, value);
        }

        public static readonly DependencyProperty TotalVotesProperty =
            DependencyProperty.RegisterAttached("TotalVotes", typeof(int), typeof(CustomRectangleControl), new PropertyMetadata(0, OnOptionActualWidthChanged));

        public static double GetOptionActualWidth(DependencyObject obj)
        {
            var o = obj.GetValue(OptionActualWidthProperty);
            if (o != null)
                return (double)o;
            return 0.0;
        }

        public static void SetOptionActualWidth(DependencyObject obj, double value)
        {
            obj.SetValue(OptionActualWidthProperty, value);
        }
        public static readonly DependencyProperty OptionActualWidthProperty =
           DependencyProperty.RegisterAttached("OptionActualWidth", typeof(double), typeof(CustomRectangleControl), new PropertyMetadata(0.0, OnOptionActualWidthChanged));

        private static void OnOptionActualWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as CustomRectangleControl;
            if (element == null) return;
            var optionVotes = GetOptionVotes(element);
            var totalVotes = GetTotalVotes(element);
            var optionActualWidth = GetOptionActualWidth(element);
            if (totalVotes != 0 && optionVotes != 0)
                element.rectangle.Width = optionActualWidth/totalVotes*optionVotes;
            else
                element.rectangle.Width = 0;

        }


    }
}
