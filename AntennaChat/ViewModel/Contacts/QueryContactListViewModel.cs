using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AntennaChat.Views.Contacts;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class QueryContactListViewModel : PropertyNotifyObject
    {
        Dispatcher dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        GlobalVariable.ContactInfoViewContainer Container;
        private GroupListViewModel _groupListViewModel;//群组信息
        private ListBox listBox;
        private string _condition;
        private DispatcherTimer QueryTimer = new DispatcherTimer();
        #region 构造器
        public QueryContactListViewModel(string condition, GlobalVariable.ContactInfoViewContainer container, GroupListViewModel groupListViewModel)
        {
            _condition = condition.ToLower();
            //ContactInfoViewModel.MouseDoubleClickEvent += ContactInfoViewMouseDoubleClick;
            ContactInfoViewModel.GetFocusedEvent += ContactInfoViewModel_GetFocusedEvent;
            this._groupListViewModel = groupListViewModel;
            this.Container = container;
            //ResetQueryCondition(_condition);
            QueryTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            QueryTimer.Start();
            QueryTimer.Tick += timer_Tick;

        }



        public QueryContactListViewModel(string condition, GlobalVariable.ContactInfoViewContainer container)
        {
            _condition = condition.ToLower();
            //ResetQueryCondition(_condition);
            QueryTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            QueryTimer.Tick += timer_Tick;
            QueryTimer.Start();
            //ContactInfoViewModel.MouseDoubleClickEvent += ContactInfoViewMouseDoubleClick;
            this.Container = container;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_condition.Trim()))
            {
                ResetQueryCondition(_condition);
            }
            QueryTimer.Stop();
        }
        #endregion

        #region 属性
        private ObservableCollection<ContactInfoViewModel> _QueryContactList = new ObservableCollection<ContactInfoViewModel>();
        /// <summary>
        /// 联系人列表
        /// </summary>
        public ObservableCollection<ContactInfoViewModel> QueryContactList
        {
            get { return this._QueryContactList; }
            set
            {
                this._QueryContactList = value;
                RaisePropertyChanged(() => QueryContactList);
            }
        }
        /// <summary>
        /// 背景是否显示
        /// </summary>
        private Visibility _BackImage = Visibility.Collapsed;
        public Visibility BackImage
        {
            get { return this._BackImage; }
            set
            {
                this._BackImage = value;
                RaisePropertyChanged(() => BackImage);
            }
        }

        private ContactInfoViewModel _selectContactItem;
        /// <summary>
        /// 搜索列表当前选中行
        /// </summary>
        public ContactInfoViewModel SelectContactItem
        {
            get { return _selectContactItem; }
            set
            {
                _selectContactItem = value;
                RaisePropertyChanged(() => SelectContactItem);
            }
        }

        private int _selectIndex = 0;
        /// <summary>
        /// 搜索列表当前选中索引
        /// </summary>
        public int SelectIndex
        {
            get { return _selectIndex; }
            set
            {
                _selectIndex = value;
                RaisePropertyChanged(() => SelectIndex);
            }
        }
        private bool _isQueryListFocus;
        /// <summary>
        /// 查询列表获得焦点
        /// </summary>
        public bool IsQueryListFocus
        {
            get { return _isQueryListFocus; }
            set
            {
                _isQueryListFocus = value;
                RaisePropertyChanged(() => IsQueryListFocus);

            }
        }

        #endregion

        #region 命令/方法
        /// <summary>
        /// 加载
        /// </summary>
        private ActionCommand<ListBox> _loadedCommand;
        public ActionCommand<ListBox> ListBoxLoadedCommand
        {
            get
            {
                if (this._loadedCommand == null)
                {
                    this._loadedCommand = new ActionCommand<ListBox>(
                           o =>
                           {
                               listBox = o;
                           });
                }
                return this._loadedCommand;
            }
        }
        private string rememberCondition = "";
        private List<AntSdkContact_User> userList = new List<AntSdkContact_User>();
        /// <summary>
        /// 搜索联系人、群组
        /// </summary>
        /// <param name="condition"></param>
        public void QueryCondition(string condition)
        {
            QueryTimer?.Stop();
            QueryTimer?.Start();
            _condition = condition.ToLower();

        }
        /// <summary>
        /// 查询（暂时支持汉字和首字母查询）
        /// </summary>
        /// <param name="condition"></param>
        public void ResetQueryCondition(string condition)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (string.IsNullOrWhiteSpace(condition)) return;
                if (QueryContactList == null)
                    QueryContactList = new ObservableCollection<ContactInfoViewModel>();
                QueryContactList.Clear();
                try
                {
                    if (AntSdkService.AntSdkListContactsEntity.users != null)
                    {
                        //var userList =
                        //    AntSdkService.AntSdkListContactsEntity.contacts.users.Where(
                        //        m => IsconditionsSatisfy(m.userName, m.userNum));
                        //foreach (AntSdkContact_User user in userList)
                        //{
                        //    ContactInfoViewModel contactInfoViewModel = new ContactInfoViewModel(user, Container, condition);
                        //    contactInfoViewModel.MouseEnter = new DefaultCommand(mouseEnter);
                        //    contactInfoViewModel.MouseLeave = new DefaultCommand(mouseEnter);
                        //    contactInfoViewModel.PlaceholderWidth = 20;
                        //    QueryContactList.Add(contactInfoViewModel);
                        //}
                        var tempUsers = AntSdkService.AntSdkListContactsEntity.users.Where(m => m.status == 2);
                        var i = 0;
                        foreach (var user in tempUsers)
                        {
                            string pinYin = string.Empty;
                            if (IsconditionsSatisfy(user.userName, user.userNum, ref pinYin))
                            {
                                i++;
                                ContactInfoViewModel contactInfoViewModel = new ContactInfoViewModel(user, Container,
                                    condition, pinYin);
                                contactInfoViewModel.MouseEnter = new DefaultCommand(mouseEnter);
                                contactInfoViewModel.MouseLeave = new DefaultCommand(mouseEnter);
                                contactInfoViewModel.PlaceholderWidth = 20;
                                QueryContactList.Add(contactInfoViewModel);
                                if (i > 80)
                                    break;
                            }
                        }
                    }
                    if (_groupListViewModel != null)
                    {
                        //var groupList =
                        //    _groupListViewModel.GroupInfoList.Where(c => IsconditionsSatisfy(c.GroupInfo.groupName, string.Empty));
                        //foreach (GroupInfoViewModel group in groupList)
                        //{
                        //    ContactInfoViewModel contactInfoViewModel = new ContactInfoViewModel(group, Container, condition)
                        //    {
                        //        MouseEnter = new DefaultCommand(mouseEnter),
                        //        MouseLeave = new DefaultCommand(mouseEnter)
                        //    };
                        //    QueryContactList.Add(contactInfoViewModel);
                        //}
                        foreach (var group in _groupListViewModel.GroupInfoList)
                        {
                            string pinYin = string.Empty;
                            if (IsconditionsSatisfy(group.GroupName, string.Empty, ref pinYin))
                            {
                                ContactInfoViewModel contactInfoViewModel = new ContactInfoViewModel(group, Container,
                                    condition, pinYin)
                                {
                                    MouseEnter = new DefaultCommand(mouseEnter),
                                    MouseLeave = new DefaultCommand(mouseEnter)
                                };
                                QueryContactList.Add(contactInfoViewModel);
                            }
                        }
                    }
                    //RaisePropertyChanged(() => QueryContactList);
                }
                catch (Exception e)
                {
                    return;
                }
                if (QueryContactList.Count == 0)
                    BackImage = Visibility.Visible;
                else
                {
                    SelectContactItem = QueryContactList[0];
                    BackImage = Visibility.Collapsed;
                }
            }, DispatcherPriority.Background);
        }

        private void mouseEnter(object obj)
        {
        }

        /// <summary>
        /// 搜索满足条件
        /// </summary>
        /// <param name="userName">成员名称</param>
        /// <param name="userNum">成员工号</param>
        private bool IsconditionsSatisfy(string userName, string userNum, ref string resultPinYin)
        {
            var nameNum = string.Empty;
            if (DataConverter.InputIsNum(_condition))
            {
                if ((!string.IsNullOrEmpty(userNum) && userNum.Contains(_condition)) || (!string.IsNullOrEmpty(userName) && userName.Contains(_condition)))
                    return true;
            }
            else if (DataConverter.InputIsChinese(_condition))
            {
                nameNum = userNum + userName;
                if (!string.IsNullOrEmpty(nameNum) && nameNum.ToLower().Contains(_condition))
                    return true;
            }
            else
            {
                bool isSpell = false;
                var pinyinName = DataConverter.GetChineseSpellList(userName);
                if (pinyinName.Any())
                {
                    foreach (var t in pinyinName)
                    {
                        nameNum = userNum + t;
                        if (nameNum.Contains(_condition))
                        {
                            resultPinYin = nameNum;
                            isSpell = true;
                            break;
                        }
                    }
                    if (isSpell || (!string.IsNullOrEmpty(userName) && userName.Contains(_condition)))
                        return true;
                }
            }
            return false;
        }

        public void ContactInfoViewModel_GetFocusedEvent(object sender, EventArgs args)
        {
            ContactInfoViewModel contractInfoVm = QueryContactList.FirstOrDefault(p => p.IsFocused);
            if (contractInfoVm != null && Container == GlobalVariable.ContactInfoViewContainer.ContactListView)
            {
                contractInfoVm.IsFocused = false;
            }
        }
        public ICommand OpenContactCommand
        {
            get
            {
                return new DefaultCommand((o) =>
                {
                    if (SelectContactItem != null)
                    {
                        SelectContactItem?.MouseDoubleClick.Execute(null);
                    }
                });
            }
        }

        /// <summary>
        /// 上下选中
        /// </summary>
        public void ChangeSelectIndex(Key key)
        {
            if (QueryContactList == null || QueryContactList.Count == 0) return;
            switch (key)
            {
                case Key.Down:
                    {
                        if (SelectIndex == -1)
                        {
                            SelectIndex = 0;
                            return;
                        }
                        if (SelectIndex < QueryContactList.Count - 1)
                        {
                            SelectIndex++;
                            listBox?.ScrollIntoView(listBox.Items[SelectIndex]);
                            return;
                        }
                        if (SelectIndex == QueryContactList.Count - 1 && QueryContactList.Count > 1)
                        {
                            SelectIndex = 0;
                            listBox?.ScrollIntoView(listBox.Items[SelectIndex]);
                        }
                    }
                    break;
                case Key.Up:
                    {
                        if (SelectIndex < 1) return;
                        if (SelectIndex > 0)
                        {
                            SelectIndex--;
                            listBox?.ScrollIntoView(listBox.Items[SelectIndex]);
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
