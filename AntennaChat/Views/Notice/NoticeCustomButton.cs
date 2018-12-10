using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntennaChat.Views.Notice
{
    public class NoticeCustomButton:Button
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var grid = VisualTreeHelper.GetChild(this, 0);
            var button = VisualTreeHelper.GetChild(grid, 0) as Button;
        }
    }
}
