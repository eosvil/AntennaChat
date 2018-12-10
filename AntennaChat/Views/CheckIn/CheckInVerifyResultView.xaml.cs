using AntennaChat.ViewModel;
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
using System.Windows.Shapes;

namespace AntennaChat.Views
{
    /// <summary>
    /// CheckInVerifyResultView.xaml 的交互逻辑
    /// </summary>
    public partial class CheckInVerifyResultView : Window
    {
        public CheckInVerifyResultView()
        {
            InitializeComponent();
            this.Loaded += CheckInVerifyResultView_Loaded;
        }

        private void CheckInVerifyResultView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDescription.Text))
            {
                var startindex = txtDescription.Text.LastIndexOf("您在")+2;
                var endindex = txtDescription.Text.LastIndexOf("的打");
                if (startindex > 0)
                {
                    TextEffect tfe = new TextEffect();
                    tfe.Foreground = new SolidColorBrush(Colors.Red);
                    tfe.PositionStart = startindex;
                    tfe.PositionCount = endindex - startindex;
                    txtDescription.TextEffects = new TextEffectCollection();
                    txtDescription.TextEffects.Add(tfe);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
