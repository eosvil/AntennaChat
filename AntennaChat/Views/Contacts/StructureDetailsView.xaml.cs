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
using AntennaChat.ViewModel.Contacts;
using AntennaChat.Views.Control;

namespace AntennaChat.Views.Contacts
{
    /// <summary>
    /// StructureDetails.xaml 的交互逻辑
    /// </summary>
    public partial class StructureDetailsView : UserControl
    {
        public StructureDetailsView()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty DetailTypeProperty =
           DependencyProperty.Register("DetailType", typeof(DetailType), typeof(StructureDetailsView),new FrameworkPropertyMetadata(DetailType.Other, new PropertyChangedCallback(DetailTypeChanged)));
        public DetailType DetailType
        {
            get
            {
                return (DetailType)GetValue(DetailTypeProperty);
            }
            set
            {
                SetValue(DetailTypeProperty, value);
            }
        }

        private static void DetailTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StructureDetailsView u = (StructureDetailsView)d;
            switch ((DetailType)e.NewValue)
            {
                case DetailType.Department:
                    u.ContentControl.Content = new DepartmentDetailView();
                    u.typeName.Text = "部门成员";
                    break;
                case DetailType.Company:
                    u.ContentControl.Content = new CompanyDetailView();
                    u.typeName.Text = "公司成员";
                    break;
                case DetailType.Group:
                    u.ContentControl.Content = new GroupDetailView();
                    u.typeName.Text = "群成员";
                    break;
                case DetailType.Personal:
                    u.ContentControl.Content = new UserDetailView();
                    break;

            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            lstMembers.SelectedValue = null;
        }
    }
}
