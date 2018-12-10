using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Contacts;
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
using SDK.AntSdk.AntModels;

namespace AntennaChat.Views.Contacts
{
    /// <summary>
    /// ContactInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class ContactInfoView : UserControl
    {
        public ContactInfoView()
        {
            InitializeComponent();
        }
        public ContactInfoView(AntSdkContact_User user, GlobalVariable.ContactInfoViewContainer container)
        {
            InitializeComponent();
            ContactInfoViewModel model = new ContactInfoViewModel(user, container);
            DataContext = model;
        }
    }
}
