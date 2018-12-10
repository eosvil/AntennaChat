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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// GroupMemberList.xaml 的交互逻辑
    /// </summary>
    public partial class GroupMemberListView : UserControl
    {
        public GroupMemberListView()
        {
            InitializeComponent();
        }
        private void UIElement_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gridSearch.Visibility == Visibility.Visible)
                gridSearch.Visibility = Visibility.Collapsed;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            txtSearch.Focusable = true;
            txtSearch.Focus();
            btnClear.Visibility = Visibility.Collapsed;
        }
        private int _flag = 0;
        private void GroupMemberListView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Up || e.Key == Key.Down) && _flag == 0)
            {
                FocusManager.SetFocusedElement(this, listSearch);
                if (listSearch.SelectedItem != null)
                {
                    var lvi = (ListBoxItem) listSearch.ItemContainerGenerator.ContainerFromItem(listSearch.SelectedItem);
                    lvi.Focusable = true;
                    lvi.Focus();
                    _flag = 1;
                }
            }
            else if (e.Key != Key.Up && e.Key != Key.Down)
            {
                txtSearch.Focusable = true;
                txtSearch.Focus();
                _flag = 0;
            }

        }


        private void GroupMemberListView_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
