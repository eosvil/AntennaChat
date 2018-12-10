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
    /// QueryContactList.xaml 的交互逻辑
    /// </summary>
    public partial class QueryContactListView : UserControl
    {
        public QueryContactListView()
        {
            InitializeComponent();
        }

        private int _flag = 0;
        private void ListSearch_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_flag == 0)
            {
                FocusManager.SetFocusedElement(this, listSearch);
                if (listSearch.SelectedItem != null)
                {
                    var lvi = (ListBoxItem)listSearch.ItemContainerGenerator.ContainerFromItem(listSearch.SelectedItem);
                    lvi.Focusable = true;
                    lvi.Focus();
                    _flag = 1;
                }
                _flag = 1;
            }
        }

        private void ListSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _flag = 0;
        }

        private void GroupMemberListView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if ((e.Key == Key.Up || e.Key == Key.Down) && _flag == 1)
            //{
            //    FocusManager.SetFocusedElement(this, listSearch);
            //    if (listSearch.SelectedItem != null)
            //    {
            //        var lvi = (ListBoxItem)listSearch.ItemContainerGenerator.ContainerFromItem(listSearch.SelectedItem);
            //        lvi.Focusable = true;
            //        lvi.Focus();
            //        _flag = 1;
            //    }
            //}
        }
    }
}
