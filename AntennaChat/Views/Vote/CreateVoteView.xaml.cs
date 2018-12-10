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
using AntennaChat.ViewModel.Vote;

namespace AntennaChat.Views.Vote
{
    /// <summary>
    /// CreateVoteView.xaml 的交互逻辑
    /// </summary>
    public partial class CreateVoteView : UserControl
    {
        public CreateVoteView()
        {
            InitializeComponent();
        }
        private void TextTitle_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Enter)
                e.Handled = true;
            //txtBoxVote.
           // txtBoxVote.Text=
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}
