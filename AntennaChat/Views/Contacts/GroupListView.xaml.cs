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

namespace AntennaChat.Views.Contacts
{
    /// <summary>
    /// UserControl2.xaml 的交互逻辑
    /// </summary>
    public partial class GroupListView : UserControl
    {
        public GroupListView()
        {
            InitializeComponent();
            this.PreviewMouseWheel += GroupListView_PreviewMouseWheel;
        }

        private void GroupListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer viewer = scrollview;  
            if (viewer == null)
                return;
            double num = Math.Abs((int)(e.Delta / 2));
            double offset = 0.0;
            if (e.Delta > 0)
            {
                offset = Math.Max((double)0.0, (double)(viewer.VerticalOffset - num));
            }
            else
            {
                offset = Math.Min(viewer.ScrollableHeight, viewer.VerticalOffset + num);
            }
            if (offset != viewer.VerticalOffset)
            {
                viewer.ScrollToVerticalOffset(offset);
                e.Handled = true;
            }
        }
    }
}
