using AntennaChat.ViewModel.Contacts;
using AntennaChat.Views.Contacts;
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

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// MassMsgListView.xaml 的交互逻辑
    /// </summary>
    public partial class MassMsgListView : UserControl
    {
        public MassMsgListView()
        {
            InitializeComponent();
        }

        private void btnMassMsgSend_Click(object sender, RoutedEventArgs e)
        {
            Window owner = Window.GetWindow(this);
            MassMsgSentView win = new MassMsgSentView();
            MassMsgSentViewModel vm = new MassMsgSentViewModel();
            win.DataContext = vm;
            win.ShowInTaskbar = false;
            win.Owner = owner;
            win.ShowDialog();
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            element.BringIntoView();
        }
    }
}
