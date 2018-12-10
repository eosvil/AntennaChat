using Antenna.Model;
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AntennaChat.ViewModel.Contacts
{
    public class ContactListViewModel : BaseViewModel
    {
        Object ExpanderHeaderStyle;
        Object FirstLevelExpanderHeaderStyle;
        ListContactsOutput output;
        List<ContactInfoView> ContactInfoViewList;
        #region 构造器
        public ContactListViewModel(ListContactsOutput output)
        {
            string packUri = @"/AntennaChat;component/Resource/ExpanderStyle.xaml";
            ResourceDictionary myResourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            ExpanderHeaderStyle = myResourceDictionary["ExpanderHeaderStyle"];
            FirstLevelExpanderHeaderStyle = myResourceDictionary["FirstLevelExpanderHeaderStyle"];
        }
        #endregion

        #region 属性
        private bool _PwdPopuIsOpen;
        /// <summary>
        /// 密码提示窗是否打开
        /// </summary>
        public bool PwdPopuIsOpen
        {
            get { return this._PwdPopuIsOpen; }
            set
            {
                this._PwdPopuIsOpen = value;
                RaisePropertyChanged(() => PwdPopuIsOpen);
            }
        }
        #endregion

        #region 命令
        //public void RefreshSource(ListContactsOutput output)
        //{
        //    this.output = output;
        //    this.mainStackPanel.Children.Clear();
        //    List<Contact_Depart> departList = output.contacts.departs.Where(c => c.parentDepartId == "0").ToList();
        //    foreach (Contact_Depart depart in departList)
        //    {
        //        Expander expander = new Expander();
        //        expander.SetValue(Expander.StyleProperty, FirstLevelExpanderHeaderStyle);
        //        int count = output.contacts.users.Where(c => c.departmentId == depart.departmentId).Count() + output.contacts.departs.Where(c => c.parentDepartId == depart.departmentId).Count();
        //        expander.Header = depart.departName + "(" + count + ")";
        //        expander.Padding = new Thickness(12, 0, 0, 0);
        //        StackPanel stackPanel = new StackPanel();
        //        expander.Content = stackPanel;
        //        this.mainStackPanel.Children.Add(expander);
        //        RecursionLoadSource(stackPanel, depart.departmentId, 1);
        //    }
        //}

        //private void RecursionLoadSource(StackPanel stackPanel, string parentDepartId, int level)
        //{
        //    if (output == null) return;
        //    List<Contact_Depart> departList = output.contacts.departs.Where(c => c.parentDepartId == parentDepartId).ToList();
        //    List<Contact_User> userList = output.contacts.users.Where(c => c.departmentId == parentDepartId).ToList();
        //    if (userList != null && userList.Count > 0)
        //    {
        //        if (ContactInfoControlList == null) ContactInfoControlList = new List<ContactInfoControl>();
        //        foreach (Contact_User user in userList)
        //        {
        //            ContactInfoControl contactInfo = new ContactInfoControl(user.picture, user.userName, user.position);
        //            stackPanel.Children.Add(contactInfo);
        //            contactInfo.txtPlaceholder.Width = 20 * level;
        //            contactInfo.MouseDown += ModifyColorOnMouseClick;
        //            ContactInfoControlList.Add(contactInfo);
        //        }
        //    }
        //    if (departList != null && departList.Count > 0)
        //    {

        //        foreach (Contact_Depart depart in departList)
        //        {
        //            Expander expander = new Expander();
        //            expander.SetValue(Expander.StyleProperty, ExpanderHeaderStyle);
        //            int count = output.contacts.users.Where(c => c.departmentId == depart.departmentId).Count() + output.contacts.departs.Where(c => c.parentDepartId == depart.departmentId).Count();
        //            expander.Header = depart.departName + "(" + count + ")"; ;
        //            expander.Padding = new Thickness(12 + 20 * level, 0, 0, 0);
        //            StackPanel childStackPanel = new StackPanel();
        //            expander.Content = childStackPanel;
        //            stackPanel.Children.Add(expander);
        //            RecursionLoadSource(childStackPanel, depart.departmentId, level + 1);
        //        }
        //    }
        //}

        #endregion
    }
}
