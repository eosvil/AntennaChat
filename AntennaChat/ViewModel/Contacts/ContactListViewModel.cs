using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class ContactListViewModel : PropertyNotifyObject
    {
        Style ExpanderHeaderStyle;
        Style FirstLevelExpanderHeaderStyle;
        public List<ContactInfoViewModel> ContactInfoViewModelList = new List<ContactInfoViewModel>();
        GlobalVariable.ContactInfoViewContainer Container;
        object Owner;
        #region 构造器
        public ContactListViewModel(GlobalVariable.ContactInfoViewContainer container, object owner)
        {
            Container = container;
            this.Owner = owner;
            string packUri = @"/AntennaChat;component/Resource/ExpanderStyle.xaml";
            ResourceDictionary myResourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            if (myResourceDictionary != null)
            {
                ExpanderHeaderStyle = myResourceDictionary["ExpanderHeaderStyle"] as Style;
                FirstLevelExpanderHeaderStyle = myResourceDictionary["FirstLevelExpanderHeaderStyle"] as Style;
            }
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => RefreshSource()));
        }
        #endregion

        #region 属性
        private StackPanel _MainStackPanel = new StackPanel();
        /// <summary>
        /// 主面板
        /// </summary>
        public StackPanel MainStackPanel
        {
            get { return this._MainStackPanel; }
            set
            {
                this._MainStackPanel = value;
                RaisePropertyChanged(() => MainStackPanel);
            }
        }
        #endregion

        #region 命令
        private ICommand _Loaded;
        /// <summary>
        /// 加载窗体命令
        /// </summary>
        public ICommand LoadedCommand
        {
            get
            {
                if (this._Loaded == null)
                {
                    this._Loaded = new DefaultCommand(o =>
                    {
                        //System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => RefreshSource()));
                        //System.Threading.ThreadPool.QueueUserWorkItem(c => RefreshSource());
                    });
                }
                return this._Loaded;
            }
        }

        private ICommand _contactDepartItemCommand;
        /// <summary>
        /// 部门Item鼠标右键命令
        /// </summary>
        public ICommand ContactDepartItemCommand
        {
            get
            {
                _contactDepartItemCommand = new DefaultCommand(o =>
                  {
                      if (!(o is string)) return;
                      contactUsers = new List<AntSdkContact_User>();
                      var parentDepartId = o.ToString();
                      GetUserListByDepartId(parentDepartId);
                  });
                return _contactDepartItemCommand;
            }
        }

        private ICommand _departItemCommand;
        /// <summary>
        /// 部门Item鼠标单击命令
        /// </summary>
        public ICommand DepartItemCommand
        {
            get
            {
                _departItemCommand = new DefaultCommand(o =>
                {
                    if (!(o is string)) return;
                    var parentDepartId = o.ToString();
                    //var vm = new StructureDetailsViewModel();
                    //var view = new StructureDetailsView
                    //{
                    //    DataContext = vm,
                    //    DetailType = DetailType.Department
                    //};
                    //MainWindowViewModel.GoStructureDetail(view);
                    //var departList = AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == _parentDepartId).ToList();
                    //contactUsers = new List<AntSdkContact_User>();
                    //GetUserListByDepartId(parentDepartId);
                    //if (departList.Count > 0)
                    //{
                    //    var depart = departList.FirstOrDefault(m => m.departmentId == parentDepartId);
                    //    vm.InitDetails(DetailType.Department, depart, contactUsers);
                    //}
                });
                return _departItemCommand;
            }
        }

        private string _parentDepartId = string.Empty;
        private ICommand _companyItemComand;
        /// <summary>
        /// 公司节点鼠标单击命令
        /// </summary>
        public ICommand CompanyItemComand
        {
            get
            {
                _companyItemComand = new DefaultCommand(o =>
                 {
                     if (!(o is string)) return;
                     var departId = o.ToString();
                     _parentDepartId = departId;
                     //var vm = new StructureDetailsViewModel();
                     //var view = new StructureDetailsView
                     //{
                     //    DataContext = vm,
                     //    DetailType = DetailType.Company
                     //};
                     //MainWindowViewModel.GoStructureDetail(view);
                     //var departList = AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == "0").ToList();
                     //contactUsers = new List<AntSdkContact_User>();
                     //GetUserListByDepartId(_parentDepartId);
                     //if (departList.Count <= 0) return;
                     //var depart = departList.FirstOrDefault(m => m.departmentId == departId);
                     //vm.InitDetails(DetailType.Company, depart, contactUsers);
                 });
                return _companyItemComand;
            }
        }

        Dictionary<Expander, Node> dicExpanderNode;
        /// <summary>
        /// 展开第一层节点
        /// </summary>
        public void RefreshSource()
        {
            if (MainStackPanel != null && MainStackPanel.Children.Count > 0)
                MainStackPanel.Children.Clear();
            if (ContactInfoViewModelList != null && ContactInfoViewModelList.Count > 0)
                ContactInfoViewModelList.Clear();
            if (AntSdkService.AntSdkListContactsEntity == null) return;
            dicExpanderNode = new Dictionary<Expander, Node>();
            if (AntSdkService.AntSdkListContactsEntity.departs != null)
            {
                List<AntSdkContact_Depart> departList = AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == "0").ToList();
                foreach (AntSdkContact_Depart depart in departList)
                {
                    AddFirstLevelDepartSource(depart);
                }
            }
            //Debug.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff ") + "]" + "步骤3");
        }
        /// <summary>
        /// 添加部门数据
        /// </summary>
        /// <param name="depart"></param>
        private void AddFirstLevelDepartSource(AntSdkContact_Depart depart)
        {
            Expander expander = new Expander();
            expander.SetValue(FrameworkElement.StyleProperty, FirstLevelExpanderHeaderStyle);
            //int count = AntSdkService.AntSdkListContactsEntity.contacts.users.Where(c => c.departmentId == depart.departmentId).Count()
            //    + AntSdkService.AntSdkListContactsEntity.contacts.departs.Where(c => c.parentDepartId == depart.departmentId).Count();
            int count = GetContactsCount(depart.departmentId);
            expander.Header = depart.departName + "(" + count + ")";
            expander.Padding = new Thickness(20, 0, 0, 0);
            StackPanel stackPanel = new StackPanel();
            expander.Content = stackPanel;
            expander.Tag = depart.departmentId;
            var mainStackPanel = this.MainStackPanel;
            mainStackPanel?.Children.Add(expander);
            expander.Expanded += ExpanderControlExpanding;
            expander.Collapsed += ExpanderControlCollapsing;
            //expander.MouseLeftButtonDown += ExpanderMouseDoubleClick;
            dicExpanderNode.Add(expander, new Node(depart.departmentId, "", 1, false));


            //RecursionLoadSource(stackPanel, depart.departmentId, 1);
            if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
            {
                //expander.MouseRightButtonUp += Expander_MouseRightButtonUp;
                DepartAddContextMenu(expander);
            }
        }

        /// <summary>
        /// 联系人换部门或换公司
        /// </summary>
        /// <param name="oldDepartmentId">旧部门ID</param>
        /// <param name="newDepartmentId">新部门ID</param>
        public void UserChangeDepartemnt(string oldDepartmentId, string newDepartmentId)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (dicExpanderNode == null || dicExpanderNode.Count <= 0) return;
                if (!string.IsNullOrEmpty(oldDepartmentId))
                {
                    var departemntExpander =
                        dicExpanderNode.Keys.FirstOrDefault(m => m.Tag != null && (string)m.Tag == oldDepartmentId);
                    //如果部门节点已展开，变更成员列表，否则只变更成员数量
                    if (departemntExpander != null)
                    {
                        var departemntStackPanel = departemntExpander.Content as StackPanel;
                        var node = dicExpanderNode[departemntExpander];
                        if (node.IsLoaded)
                        {
                            //var tempContactInfo = ContactInfoViewModelList.FirstOrDefault(m => m.User != null && m.User.userId == userId);
                            //if (tempContactInfo != null)
                            //    ContactInfoViewModelList.Remove(tempContactInfo);
                            RecursionLoadSource(departemntStackPanel, oldDepartmentId, node.Level);
                        }
                        var count = GetContactsCount(oldDepartmentId);
                        var departInfo =
                            AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                                m => m.departmentId == oldDepartmentId);
                        if (departInfo != null)
                        {

                            departemntExpander.Header = departInfo.departName + "(" + count + ")";
                            if (!string.IsNullOrEmpty(departInfo.parentDepartId))
                                DepartmentMembersCountUpdate(departInfo.parentDepartId);

                        }
                    }
                    else
                    {
                        var departInfo =
                            AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                                m => m.departmentId == oldDepartmentId);
                        if (departInfo != null)
                        {
                            if (string.IsNullOrEmpty(departInfo.parentDepartId))
                            {
                                //var parentDepartemntExpander =
                                //    dicExpanderNode.Keys.FirstOrDefault(
                                //        m => m.Tag != null && (string)m.Tag == departInfo.parentDepartId);
                                //var parentDepartInfo =
                                //    AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                                //        m => m.departmentId == departInfo.parentDepartId);
                                //if (parentDepartemntExpander != null && parentDepartInfo != null)
                                //{
                                //    var count = GetContactsCount(parentDepartInfo.departmentId);
                                //    parentDepartemntExpander.Header = parentDepartInfo.departName + "(" + count + ")";
                                //}
                                DepartmentMembersCountUpdate(departInfo.parentDepartId);
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(newDepartmentId)) return;
                //用户被移到新部门
                var newDepartemntExpander =
                    dicExpanderNode.Keys.FirstOrDefault(m => m.Tag != null && (string)m.Tag == newDepartmentId);
                //如果部门节点已展开，变更成员列表，否则只变更成员数量
                if (newDepartemntExpander != null)
                {
                    var departemntStackPanel = newDepartemntExpander.Content as StackPanel;
                    var node = dicExpanderNode[newDepartemntExpander];
                    if (node.IsLoaded)
                    {
                        //var tempContactInfo = ContactInfoViewModelList.FirstOrDefault(m => m.User != null && m.User.userId == userId);
                        //if (tempContactInfo != null)
                        //    ContactInfoViewModelList.Remove(tempContactInfo);
                        RecursionLoadSource(departemntStackPanel, newDepartmentId, node.Level);
                    }
                    var count = GetContactsCount(newDepartmentId);
                    var departInfo =
                        AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                            m => m.departmentId == newDepartmentId);
                    if (departInfo == null) return;
                    {
                        newDepartemntExpander.Header = departInfo.departName + "(" + count + ")";
                        if (string.IsNullOrEmpty(departInfo.parentDepartId)) return;
                        DepartmentMembersCountUpdate(departInfo.parentDepartId);
                    }
                }
                else
                {
                    var departInfo =
                        AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                            m => m.departmentId == newDepartmentId);
                    if (departInfo == null) return;
                    if (!string.IsNullOrEmpty(departInfo.parentDepartId)) return;
                    {
                        DepartmentMembersCountUpdate(departInfo.parentDepartId);
                        //var parentDepartemntExpander = dicExpanderNode.Keys.FirstOrDefault(m => m.Tag != null && (string)m.Tag == departInfo.parentDepartId);
                        //var parentDepartInfo = AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(m => m.departmentId == departInfo.parentDepartId);
                        //if (parentDepartemntExpander != null && parentDepartInfo != null)
                        //{
                        //    var count = GetContactsCount(parentDepartInfo.departmentId);
                        //    parentDepartemntExpander.Header = parentDepartInfo.departName + "(" + count + ")";
                        //}
                    }
                }
            });
        }


        /// <summary>
        /// 更新一级结构人数
        /// </summary>
        /// <param name="parentDepartId"></param>
        private void DepartmentMembersCountUpdate(string parentDepartId)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                var rootDepartemntExpander =
                    dicExpanderNode.Keys.FirstOrDefault(m => m.Tag != null && (string)m.Tag == parentDepartId);
                if (rootDepartemntExpander == null) return;
                {
                    var rootDepartInfo =
                        AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                            m => m.departmentId == parentDepartId);
                    if (rootDepartInfo == null) return;
                    var totalCount = GetContactsCount(rootDepartInfo.departmentId);
                    rootDepartemntExpander.Header = rootDepartInfo.departName + "(" + totalCount + ")";
                }
            });
        }

        //public void DepartmentUpdateMember(string oldDepartmentId)
        //{

        //}

        /// <summary>
        /// 联系人的职位被变更
        /// </summary>
        /// <param name="departmentId"></param>
        public void DepartmentMemberUpdate(string departmentId)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (dicExpanderNode == null || dicExpanderNode.Count <= 0) return;
                if (dicExpanderNode.Keys.Count == 0) return;
                var departemntExpander =
                    dicExpanderNode.Keys.FirstOrDefault(m => m.Tag != null && m.Tag.ToString() == departmentId);
                if (departemntExpander == null) return;
                var departemntStackPanel = departemntExpander.Content as StackPanel;
                var node = dicExpanderNode[departemntExpander];
                if (node.IsLoaded)
                {
                    RecursionLoadSource(departemntStackPanel, departmentId, node.Level);
                }
                var departInfo =
                    AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(m => m.departmentId == departmentId);
                var count = GetContactsCount(departmentId);
                if (departInfo != null)
                {
                    departemntExpander.Header = departInfo.departName + "(" + count + ")";
                    if (string.IsNullOrEmpty(departInfo.parentDepartId)) return;
                    DepartmentMembersCountUpdate(departInfo.parentDepartId);
                }
            });
        }
        /// <summary>
        /// 部门改变
        /// </summary>
        /// <param name="departs">部门</param>
        public void DepartmentUpdate(AntSdkAddListContactsOutput_Departs departs)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (departs.add?.Count > 0)
                {
                    //如果有一级部门，先添加一级部门
                    var firstLevelDeparts = departs.add.Where(m => m.parentDepartId == "0");
                    foreach (var firstLevelDepart in firstLevelDeparts)
                    {
                        if (dicExpanderNode == null || dicExpanderNode.Count <= 0)
                        {
                            AddFirstLevelDepartSource(firstLevelDepart);
                        }
                        else
                        {
                            var departemntExpander =
                                dicExpanderNode.Keys.FirstOrDefault(
                                    m => m.Tag != null && m.Tag.ToString() == firstLevelDepart.departmentId);
                            if (departemntExpander == null)
                                AddFirstLevelDepartSource(firstLevelDepart);
                        }


                    }
                    //添加二级部
                    var tempDeparts = departs.add.Where(m => m.parentDepartId != "0");
                    foreach (var depart in tempDeparts)
                    {
                        if (dicExpanderNode == null || dicExpanderNode.Count <= 0) return;
                        if (dicExpanderNode.Keys.Count == 0) return;
                        var departemntExpander =
                            dicExpanderNode.Keys.FirstOrDefault(
                                m => m.Tag != null && m.Tag.ToString() == depart.parentDepartId);
                        if (departemntExpander == null) return;
                        var departemntStackPanel = departemntExpander.Content as StackPanel;
                        var node = dicExpanderNode[departemntExpander];
                        if (node.IsLoaded)
                        {
                            var tempDepartemntExpander =
                                dicExpanderNode.Keys.FirstOrDefault(
                                    m => m.Tag != null && m.Tag.ToString() == depart.departmentId);
                            if (tempDepartemntExpander == null)
                                AdddepartData(depart, departemntStackPanel, node.Level);
                        }
                    }

                }
                if (departs.delete?.Count > 0)
                {
                    if (dicExpanderNode != null && dicExpanderNode.Count > 0)
                    {
                        var tempDeparts = departs.delete;
                        foreach (var depart in tempDeparts)
                        {
                            var departemntExpander =
                                dicExpanderNode.Keys.FirstOrDefault(
                                    m => m.Tag != null && m.Tag.ToString() == depart.departmentId);
                            if (departemntExpander == null) continue;
                            var departemntStackPanel = departemntExpander.Content as StackPanel;
                            var node = dicExpanderNode[departemntExpander];
                            if (string.IsNullOrEmpty(node.ParentDepartId))
                            {
                                MainStackPanel.Children.Remove(departemntExpander);
                                dicExpanderNode.Remove(departemntExpander);
                            }
                            else
                            {
                                var parentDepartemntExpander =
                                    dicExpanderNode.Keys.FirstOrDefault(
                                        m => m.Tag != null && m.Tag.ToString() == node.ParentDepartId);
                                var parentDepartemntStackPanel = parentDepartemntExpander?.Content as StackPanel;
                                if (parentDepartemntStackPanel == null) continue;
                                parentDepartemntStackPanel.Children.Remove(departemntExpander);
                                dicExpanderNode.Remove(departemntExpander);
                            }
                        }
                        //var tempFirstLevelDeparts = departs.delete.Where(m => m.departmentId == "0");
                        //foreach (var depart in tempFirstLevelDeparts)
                        //{
                        //    var departemntExpander =
                        //        dicExpanderNode.Keys.FirstOrDefault(
                        //            m => m.Tag != null && m.Tag.ToString() == depart.departmentId);
                        //    if (departemntExpander == null) continue;
                        //    var departemntStackPanel = departemntExpander.Content as StackPanel;
                        //    var node = dicExpanderNode[departemntExpander];
                        //    if (node.IsLoaded)
                        //    {
                        //        departemntStackPanel?.Children.Clear();
                        //    }
                        //}
                    }

                }
                if (!(departs.update?.Count > 0)) return;
                {
                    foreach (var depart in departs.update)
                    {
                        var newDepartemntExpander =
                            dicExpanderNode?.Keys.FirstOrDefault(
                                m => m.Tag != null && (string)m.Tag == depart.departmentId);
                        if (newDepartemntExpander != null)
                        {
                            //var node = dicExpanderNode[newDepartemntExpander];
                            var departInfo =
                                AntSdkService.AntSdkListContactsEntity.departs.FirstOrDefault(
                                    m => m.departmentId == depart.departmentId);
                            var count = GetContactsCount(depart.departmentId);
                            if (departInfo != null)
                                newDepartemntExpander.Header = departInfo.departName + "(" + count + ")";
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 展开子节点
        /// </summary>
        /// <param name="stackPanel">父面板</param>
        /// <param name="parentDepartId">父部门ID</param>
        /// <param name="level">在第几层</param>
        private void RecursionLoadSource(StackPanel stackPanel, string parentDepartId, int level)
        {
            if (AntSdkService.AntSdkListContactsEntity == null) return;
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (AntSdkService.AntSdkListContactsEntity.departs != null)
                {
                    List<AntSdkContact_Depart> departList =
                        AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == parentDepartId)
                            .ToList();
                    if (AntSdkService.AntSdkListContactsEntity.users != null)
                    {
                        if (stackPanel == null)
                            stackPanel = new StackPanel();
                        List<AntSdkContact_User> userList =
                            AntSdkService.AntSdkListContactsEntity.users.Where(c => c.departmentId == parentDepartId && c.status == 2)
                                .OrderBy(c => c.userId)
                                .ToList();
                        if (userList.Count > 0)
                        {
                            stackPanel?.Children.Clear();
                            foreach (AntSdkContact_User user in userList)
                            {
                                ContactInfoView contactInfo = new ContactInfoView(user, this.Container);
                                ContactInfoViewModel contactInfoViewModel =
                                    contactInfo.DataContext as ContactInfoViewModel;
                                var tempContactInfo = ContactInfoViewModelList.FirstOrDefault(m => m.User.userId == user.userId);
                                //if (tempContactInfo?.User != null && tempContactInfo.User.departmentId == user.departmentId) continue;
                                if (tempContactInfo?.User != null && tempContactInfo.User.departmentId != user.departmentId)
                                {
                                    tempContactInfo.User = user;
                                }
                                stackPanel.Children.Add(contactInfo);
                                if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
                                {
                                    contactInfo.MouseDown += ModifyColorOnMouseClick;
                                }
                                else if (Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft)
                                {
                                    GroupEditWindowViewModel groupEditVM = Owner as GroupEditWindowViewModel;
                                    if (groupEditVM?.GroupMemberList != null && groupEditVM.GroupMemberList.Select(c => c.User.userId).Contains(user.userId))
                                    {
                                        if (groupEditVM.OriginalMemberIds != null &&
                                            groupEditVM.OriginalMemberIds.Contains(user.userId))
                                        {
                                            contactInfoViewModel?.SetExistGroupMember();
                                        }
                                        else
                                        {
                                            contactInfoViewModel?.OnStateImageClickEventEvent(contactInfoViewModel, true);
                                        }
                                    }
                                }
                                else if (Container == GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft)
                                {
                                    MultiContactsSelectViewModel vm = Owner as MultiContactsSelectViewModel;
                                    if (vm != null && vm.GroupMemberList != null &&
                                        vm.GroupMemberList.Select(c => c.User.userId).Contains(user.userId))
                                    {
                                        //if (vm.OriginalMemberIds != null && vm.OriginalMemberIds.Contains(user.userId))
                                        //{
                                        //    contactInfoViewModel.SetExistGroupMember();
                                        //}
                                        //else
                                        //{
                                        contactInfoViewModel?.OnStateImageClickEventEvent(contactInfoViewModel, true);
                                        //}
                                    }
                                }

                                if (contactInfoViewModel != null)
                                {
                                    contactInfoViewModel.PlaceholderWidth = 30;
                                    ContactInfoViewModelList.Add(contactInfoViewModel);
                                }
                            }
                        }
                        else if (stackPanel?.Children.Count > 0)
                        {
                            stackPanel?.Children.Clear();
                        }
                    }

                    if (departList.Count <= 0) return;
                    foreach (AntSdkContact_Depart depart in departList)
                    {
                        AdddepartData(depart, stackPanel, level);
                    }
                }
            });
        }
        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="depart">部门信息</param>
        /// <param name="stackPanel"></param>
        /// <param name="level">属于几级部门</param>
        private void AdddepartData(AntSdkContact_Depart depart, StackPanel stackPanel, int level)
        {
            Expander expander = new Expander();
            expander.SetValue(FrameworkElement.StyleProperty, ExpanderHeaderStyle);
            //int count = AntSdkService.AntSdkListContactsEntity.contacts.users.Where(c => c.departmentId == depart.departmentId).Count()
            //    + AntSdkService.AntSdkListContactsEntity.contacts.departs.Where(c => c.parentDepartId == depart.departmentId).Count();
            int count = GetContactsCount(depart.departmentId);
            expander.Header = depart.departName + "(" + count + ")";
            ;
            //expander.Padding = new Thickness(12 + 20 * level, 0, 0, 0);
            expander.Padding = new Thickness(30, 0, 0, 0);
            StackPanel childStackPanel = new StackPanel();
            expander.Content = childStackPanel;
            stackPanel.Children.Add(expander);
            //RecursionLoadSource(childStackPanel, depart.departmentId, level + 1);
            expander.Expanded += ExpanderControlExpanding;
            expander.Collapsed += ExpanderControlCollapsing;
            expander.Tag = depart.departmentId;
            //expander.MouseDoubleClick += ExpanderMouseDoubleClick;
            //expander.MouseLeftButtonDown += ExpanderMouseDoubleClick;
            dicExpanderNode.Add(expander, new Node(depart.departmentId, depart.parentDepartId, level + 1, false));
            if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
            {
                //expander.PreviewMouseRightButtonDown += Expander_MouseRightButtonUp;
                DepartAddContextMenu(expander);
            }
        }

        /// <summary>
        /// 增加群发
        /// </summary>
        /// <param name="expander"></param>
        private void DepartAddContextMenu(Expander expander)
        {
            ContextMenu cms = new ContextMenu();
            MenuItem groupSendMsg = new MenuItem() { Header = "群发消息" };
            Image image1 = new Image();
            image1.Height = 14;
            image1.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Images/发送消息.png"));
            //groupSendMsg.Icon = image1;
            cms.Items.Add(groupSendMsg);
            groupSendMsg.Click += GroupSendMsg_Click;
            string packUri = @"/AntennaChat;component/Resource/ContextMenuStyle.xaml";
            ResourceDictionary myResourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            cms.SetValue(Expander.StyleProperty, myResourceDictionary["ContextMenuStyle"]);
            expander.ContextMenu = cms;
        }

        private List<AntSdkContact_User> contactUsers = null;
        private void GroupSendMsg_Click(object sender, RoutedEventArgs e)
        {
            Window owner = Window.GetWindow(sender as MenuItem);
            MassMsgSentView win = new MassMsgSentView();
            MassMsgSentViewModel vm = new MassMsgSentViewModel(contactUsers);
            win.DataContext = vm;
            win.ShowInTaskbar = false;
            win.Owner = owner;
            win.ShowDialog();
            e.Handled = true;
        }
        /// <summary>
        /// 根据部门ID获取部门成员数（递归实现）
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        private int GetContactsCount(string departmentId)
        {
            int count = AntSdkService.AntSdkListContactsEntity.users.Count(c => c.departmentId == departmentId && c.status == 2);
            IEnumerable<AntSdkContact_Depart> departList = AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == departmentId);
            if (departList != null && departList.Count() > 0)
            {
                foreach (AntSdkContact_Depart depart in departList)
                {
                    count += GetContactsCount(depart.departmentId);
                }
            }
            return count;
        }

        DateTime firstTime = DateTime.MinValue;
        /// <summary>
        /// 展开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpanderControlExpanding(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            TimeSpan timeSpan = DateTime.Now - firstTime;
            Debug.WriteLine(string.Format("ExpanderControlExpanding[{0}毫秒]", timeSpan.TotalMilliseconds));
            if (timeSpan.TotalMilliseconds < 300)
            {
                DepartMouseDoubleClick(sender);
                firstTime = DateTime.MinValue;
            }
            else
            {
                firstTime = DateTime.Now;
            }
            System.Threading.ThreadPool.QueueUserWorkItem(m =>
            {
                //if(firstTime !=DateTime .MinValue ) Thread.Sleep(200);
                Expander expander = sender as Expander;
                if (expander == null || !dicExpanderNode.ContainsKey(expander)) return;
                Node node = dicExpanderNode[expander];
                if (node.IsLoaded == false)
                {
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => RecursionLoadSource(expander.Content as StackPanel, node.DepartId, node.Level)));
                    node.IsLoaded = true;
                }

            });
        }

        private void ExpanderControlCollapsing(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            TimeSpan timeSpan = DateTime.Now - firstTime;
            Debug.WriteLine($"ExpanderControlCollapsing[{timeSpan.TotalMilliseconds}毫秒]");
            if (timeSpan.TotalMilliseconds < 300)
            {
                DepartMouseDoubleClick(sender);
                firstTime = DateTime.MinValue;
            }
            else
            {
                firstTime = DateTime.Now;
            }
        }

        #region 双击部门，将部门下所有用户添加到右侧的讨论组列表中
        List<AntSdkContact_User> UserListByDepartId;
        private void DepartMouseDoubleClick(object expander)
        {
            if (Owner == null || (!(Owner is GroupEditWindowViewModel) && !(Owner is MultiContactsSelectViewModel)) || !dicExpanderNode.Keys.Contains(expander)) return;
            string parentDepartId = dicExpanderNode[expander as Expander].DepartId;
            UserListByDepartId = new List<AntSdkContact_User>();
            GetUserListByDepartId(parentDepartId);
            if (UserListByDepartId.Count == 0) return;
            GroupEditWindowViewModel ownerVM = Owner as GroupEditWindowViewModel;
            if (ownerVM != null)//编辑讨论组
            {
                foreach (AntSdkContact_User user in UserListByDepartId)
                {
                    if (ownerVM.GroupMemberList.FirstOrDefault(c => c.User.userId == user.userId) != null) continue;
                    ContactInfoViewModel contactInfoVM = ContactInfoViewModelList.FirstOrDefault(c => c.User.userId == user.userId);
                    if (contactInfoVM == null)
                    {
                        ContactInfoViewModel vm = new ContactInfoViewModel(user, GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight);
                        ownerVM.GroupMemberList.Add(vm);
                    }
                    else
                    {
                        contactInfoVM.OnStateImageClickEventEvent(contactInfoVM, true);
                    }
                }
                ownerVM.MemberCount = string.Format("已选择{0}个联系人", ownerVM.GroupMemberList.Count());
            }
            else
            {
                //选择群发联系人
                MultiContactsSelectViewModel selectVM = Owner as MultiContactsSelectViewModel;
                if (selectVM != null)
                {
                    foreach (AntSdkContact_User user in UserListByDepartId)
                    {
                        if (user.userId == AntSdkService.AntSdkCurrentUserInfo.userId || selectVM.GroupMemberList.FirstOrDefault(c => c.User.userId == user.userId) != null) continue;
                        ContactInfoViewModel contactInfoVM = ContactInfoViewModelList.FirstOrDefault(c => c.User.userId == user.userId);
                        if (contactInfoVM == null)
                        {
                            ContactInfoViewModel vm = new ContactInfoViewModel(user, GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight);
                            selectVM.GroupMemberList.Add(vm);
                        }
                        else
                        {
                            contactInfoVM.OnStateImageClickEventEvent(contactInfoVM, true);
                        }
                    }
                    selectVM.MemberCount = string.Format("已选择{0}个联系人", selectVM.GroupMemberList.Count());
                }
            }
        }

        private void GetUserListByDepartId(string parentDepartId)
        {
            if (UserListByDepartId == null)
                UserListByDepartId = new List<AntSdkContact_User>();
            List<AntSdkContact_User> userList = AntSdkService.AntSdkListContactsEntity.users.Where(c => c.departmentId == parentDepartId && c.status == 2).ToList();
            UserListByDepartId = UserListByDepartId.Union(userList).ToList();
            if (contactUsers != null)
                contactUsers.AddRange(userList);
            List<AntSdkContact_Depart> departList = AntSdkService.AntSdkListContactsEntity.departs.Where(c => c.parentDepartId == parentDepartId).ToList();
            if (departList.Count == 0) return;
            foreach (AntSdkContact_Depart depart in departList)
            {
                GetUserListByDepartId(depart.departmentId);
            }
        }
        #endregion
        /// <summary>
        /// 列表控件点击事件（刷新控件颜色）
        /// </summary>
        /// <param name="ID"></param>
        private void ModifyColorOnMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
            {
                foreach (ContactInfoViewModel control in ContactInfoViewModelList)
                {
                    var dataContext = (sender as ContactInfoView).DataContext;
                    if (dataContext == control)
                    {
                        control.Background = (Brush)(new BrushConverter()).ConvertFromString("#f0f0f0");
                        control.IsMouseClick = true;
                    }
                    else
                    {
                        control.Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                        control.IsMouseClick = false;
                    }
                }
            }
            //else if(Container == GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft)
            //{

            //}
        }

        /// <summary>
        /// 更新组织架构（局部刷新)
        /// </summary>
        /// <param name="addListContacts"></param>
        public void RefreshSource(AntSdkAddListContactsOutput addListContacts)
        {
            #region 刷新部门信息
            //TODO:刷新部门信息
            //新增部门
            if (addListContacts.departs?.add?.Count > 0)
            {

            }
            //更新部门
            if (addListContacts.departs?.update?.Count > 0)
            {

            }
            //删除部门
            if (addListContacts.departs?.delete?.Count > 0)
            {

            }

            #endregion

            #region 刷新成员信息

            //新增部门
            if (addListContacts.users?.add?.Count > 0)
            {

            }
            //更新部门
            if (addListContacts.users?.update?.Count > 0)
            {

            }
            //删除部门
            if (addListContacts.users?.delete?.Count > 0)
            {

            }

            #endregion
        }

        #endregion

        #region 方法
        /// <summary>
        /// 服务断开或重连之后的状态改变
        /// </summary>
        /// <param name="isConnected">是否连接成功</param>
        public void ChangeContactListUserState(bool isConnected)
        {
            if (!isConnected)
            {
                //如果连接断开结构树在线用户改为离线状态
                var onLineContactInfoList = ContactInfoViewModelList.Where(m => !m.IsOfflineState);
                foreach (var onLineContactInfo in onLineContactInfoList)
                {
                    onLineContactInfo.IsOfflineState = true;
                    onLineContactInfo.UserOnlineStateIcon = "";
                }
            }
            else
            {
                //如果连接重连成功结构树用户恢复状态
                foreach (var offLineContactInfo in ContactInfoViewModelList)
                {
                    var userInfo =
                      AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(
                          m => m.userId == offLineContactInfo.User.userId && m.state != (int)GlobalVariable.OnLineStatus.OffLine);
                    if (userInfo == null) continue;
                    offLineContactInfo.IsOfflineState = userInfo.state == (int)GlobalVariable.OnLineStatus.OffLine;
                    if (GlobalVariable.UserOnlineSataeInfo.UserOnlineStateIconDic.ContainsKey(userInfo.state))
                    {
                        offLineContactInfo.UserOnlineStateIcon = GlobalVariable.UserOnlineSataeInfo.UserOnlineStateIconDic[userInfo.state];
                    }
                }
            }
        }

        #endregion
    }

    public class Node
    {
        public Node(string departId, string parentDepartId, int level, bool isLoaded)
        {
            this.DepartId = departId;
            this.ParentDepartId = parentDepartId;
            this.Level = level;
            this.IsLoaded = isLoaded;
        }
        public string ParentDepartId { get; set; }
        public int Level { get; set; }
        public bool IsLoaded { get; set; }
        public string DepartId { get; set; }
    }
}
