using Antenna.Framework;
using AntennaChat.Command;
using AntennaChat.Resource;
using AntennaChat.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SDK.AntSdk;

namespace AntennaChat.ViewModel.Contacts
{
    public class MultiContactsSelectViewModel : WindowBaseViewModel
    {
        private QueryContactListViewModel _QueryContactList;
        private string QueryCondition;
        public List<string> OriginalMemberIds;
        #region 构造器
        public MultiContactsSelectViewModel(List<string> memberIds)
        {
            OriginalMemberIds = memberIds;
            Title = "选择联系人";
            ContactInfoViewModel.StateImageClickEvent += StateImageClickEvent;
            ContactListViewModel = new ContactListViewModel(GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft, this);
            LeftPartViewModel = ContactListViewModel;
            if (memberIds != null)
            {
                foreach (string id in memberIds)
                {
                    ContactInfoViewModel vm = new ContactInfoViewModel(AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == id), GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight);
                    GroupMemberList.Add(vm);
                }
            }
            MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
        }
        #endregion

        #region 属性
        private ObservableCollection<ContactInfoViewModel> _GroupMemberList = new ObservableCollection<ContactInfoViewModel>();
        /// <summary>
        /// 消息接收人列表
        /// </summary>
        public ObservableCollection<ContactInfoViewModel> GroupMemberList
        {
            get { return this._GroupMemberList; }
            set
            {
                this._GroupMemberList = value;
                RaisePropertyChanged(() => GroupMemberList);
            }
        }

        private string _Title;
        /// <summary>
        /// 页面标题
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title == value)
                {
                    return;
                }
                _Title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string _MemberCount;
        /// <summary>
        /// 讨论组成员数量提示
        /// </summary>
        public string MemberCount
        {
            get { return _MemberCount; }
            set
            {
                if (_MemberCount == value)
                {
                    return;
                }
                _MemberCount = value;
                RaisePropertyChanged(() => MemberCount);
            }
        }

        /// <summary>
        /// 组织结构
        /// </summary>
        private ContactListViewModel _ContactListViewModel;
        public ContactListViewModel ContactListViewModel
        {
            get { return this._ContactListViewModel; }
            set
            {
                this._ContactListViewModel = value;
                RaisePropertyChanged(() => ContactListViewModel);
            }
        }

        private object _LeftPartViewModel;
        /// <summary>
        /// 要绑定和切换的第二部分ViewModel
        /// </summary>
        public object LeftPartViewModel
        {
            get { return _LeftPartViewModel; }
            set
            {
                if (_LeftPartViewModel == value)
                {
                    return;
                }
                _LeftPartViewModel = value;
                RaisePropertyChanged(() => LeftPartViewModel);
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 取消命令
        /// </summary>
        private ICommand _CancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (this._CancelCommand == null)
                {
                    this._CancelCommand = new DefaultCommand(o =>
                    {
                        GroupMemberList = null;//根据该值判断是取消还是确定
                        ContactInfoViewModel.StateImageClickEvent -= StateImageClickEvent;
                        (o as Window)?.Close();
                    });
                }
                return this._CancelCommand;
            }
        }

        /// <summary>
        /// 确定命令
        /// </summary>
        private ICommand _OKCommand;
        public ICommand OKCommand
        {
            get
            {
                if (this._OKCommand == null)
                {
                    this._OKCommand = new DefaultCommand(o =>
                    {
                        if(GroupMemberList.Count ==0)
                        {
                            MessageBoxWindow.Show("请先选择联系人", GlobalVariable.WarnOrSuccess.Warn);
                        }
                        //else if (GroupMemberList.Count > 100)
                        //{
                        //    MessageBoxWindow.Show("已超过100人上限，请先删减部分人员！", GlobalVariable.WarnOrSuccess.Warn);
                        //}
                        else
                        {
                            ContactInfoViewModel.StateImageClickEvent -= StateImageClickEvent;
                            (o as Window).Close();
                        }
                    });
                }
                return this._OKCommand;
            }
        }

        /// <summary>
        /// 搜索条件变更
        /// </summary>   
        private ICommand _QueryConditionChanged;
        public ICommand QueryConditionChanged
        {
            get
            {
                if (this._QueryConditionChanged == null)
                {
                    this._QueryConditionChanged = new DefaultCommand(o =>
                    {
                        QueryCondition = (o as WateMarkTextBox)?.Text;
                        //string condition = (o as WateMarkTextBox).Text;
                        if (string.IsNullOrEmpty(QueryCondition))
                        {
                            this.LeftPartViewModel = ContactListViewModel;

                        }
                        else
                        {
                            if (_QueryContactList == null)
                            {
                                _QueryContactList = new QueryContactListViewModel(QueryCondition, GlobalVariable.ContactInfoViewContainer.MultiContactsSelectLeft);
                            }
                            else
                            {

                                _QueryContactList.QueryCondition(QueryCondition);

                            }
                            if (_QueryContactList.QueryContactList != null && _QueryContactList.QueryContactList.Count > 0)
                            {
                                foreach (ContactInfoViewModel vm in _QueryContactList.QueryContactList)
                                {
                                    ContactInfoViewModel tempVM = GroupMemberList.FirstOrDefault(c => c.User.userId == vm.User.userId);
                                    if (tempVM != null)
                                    {
                                        vm.OnStateImageClickEventEvent(vm, true);
                                    }
                                }
                            }
                            this.LeftPartViewModel = _QueryContactList;
                        }
                    });
                }
                return this._QueryConditionChanged;

            }
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 状态图点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isSelect">True表示在创建讨论组左侧选中，False表示在左侧取消选择，Null表示在右侧点击</param>
        private void StateImageClickEvent(object sender, bool? isSelect)
        {
            ContactInfoViewModel senderVM = sender as ContactInfoViewModel;
            if (senderVM == null) return;
            ContactInfoViewModel existVM = GroupMemberList.FirstOrDefault(c => c.User.userId == senderVM.User.userId);
            ContactInfoViewModel ContactListTempVM = null;
            if (ContactListViewModel.ContactInfoViewModelList != null)
            {
                ContactListTempVM = ContactListViewModel.ContactInfoViewModelList.FirstOrDefault(c => c.User.userId == senderVM.User.userId);
            }
            if (isSelect == true)//选择组织结构列表中联系人
            {
                if (existVM == null)
                {
                    ContactInfoViewModel tempVM = new ContactInfoViewModel(senderVM.User, GlobalVariable.ContactInfoViewContainer.MultiContactsSelectRight);
                    GroupMemberList.Add(tempVM);
                    MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
                    //tempVM.MouseLeftButtonDownEvent += ModifyColorOnMouseClick;
                }
                //if (ContactListTempVM != null && ContactListTempVM != sender)//如果是从搜索列表里来的
                if (ContactListTempVM != null && ContactListTempVM.IsMouseClick == false)//如果是从搜索列表里来的
                {
                    ContactListTempVM.OnStateImageClickEventEvent(ContactListTempVM, true);
                }
            }
            else if (isSelect == false)//取消选中组织结构列表中联系人
            {
                if (existVM != null)
                {
                    GroupMemberList.Remove(existVM);
                    MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
                }
                if (ContactListTempVM != null && ContactListTempVM.IsMouseClick == true)
                {
                    ContactListTempVM.OnStateImageClickEventEvent(ContactListTempVM, false);
                }
            }
            else //从右侧讨论成员中删除联系人
            {
                bool isLeftExist = false;
                if (ContactListTempVM != null && ContactListTempVM != sender)
                {
                    ContactListTempVM.OnStateImageClickEventEvent(ContactListTempVM, false);
                    isLeftExist = true;
                }
                if (_QueryContactList != null && _QueryContactList.QueryContactList != null)
                {
                    ContactInfoViewModel tempVM = _QueryContactList.QueryContactList.FirstOrDefault(c => c.User.userId == senderVM.User.userId);
                    if (tempVM != null)
                    {
                        tempVM.OnStateImageClickEventEvent(tempVM, false);
                        isLeftExist = true;
                    }
                }
                if (isLeftExist == false)//左侧组织结构或者搜索列表中都还未加载，则直接从右侧列表删除即可
                {
                    if (existVM != null)
                    {
                        GroupMemberList.Remove(existVM);
                        MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
                    }
                }
            }
        }
        #endregion
    }
}
