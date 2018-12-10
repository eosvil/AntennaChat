using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Resource;
using AntennaChat.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class GroupEditWindowViewModel : WindowBaseViewModel
    {
        private Action close;
        private QueryContactListViewModel _QueryContactList;
        string QueryCondition;
        public AntSdkCreateGroupOutput CreateGroupOutput;
        public bool IsCreateGroup;
        public List<string> OriginalMemberIds;
        private AntSdkGroupInfo GroupInfo;
        #region 构造器
        public GroupEditWindowViewModel(Action close, List<string> memberIds)
        {
            OriginalMemberIds = memberIds;
            IsCreateGroup = true;
            Title = "创建讨论组";
            this.close = close;
            ContactInfoViewModel.StateImageClickEvent += StateImageClickEvent;
            ContactListViewModel = new ContactListViewModel(GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft, this);
            LeftPartViewModel = ContactListViewModel;
            foreach (string id in memberIds)
            {
                ContactInfoViewModel vm = new ContactInfoViewModel(AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == id), GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight);
                vm.StateImageVisibility = Visibility.Collapsed;
                GroupMemberList.Add(vm);
            }
            MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
        }
        public GroupEditWindowViewModel(Action close, List<string> memberIds, AntSdkGroupInfo groupInfo)
        {
            IsCreateGroup = false;
            Title = "邀请加入讨论组";
            this.GroupInfo = groupInfo;
            OriginalMemberIds = memberIds;
            this.close = close;
            ContactInfoViewModel.StateImageClickEvent += StateImageClickEvent;
            ContactListViewModel = new ContactListViewModel(GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft, this);
            LeftPartViewModel = ContactListViewModel;
            foreach (string memberId in memberIds)
            {
                AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == memberId);
                if (user == null)//离职人员信息
                {
                    user = new AntSdkContact_User();
                    user.picture = "pack://application:,,,/AntennaChat;Component/Images/离职人员.png";
                    user.userName = "离职人员";
                    user.userId = memberId;
                }
                ContactInfoViewModel myselfVM = new ContactInfoViewModel(user, GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight);
                if (myselfVM == null) continue;
                myselfVM.StateImageVisibility = Visibility.Collapsed;
                GroupMemberList.Add(myselfVM);
            }
            MemberCount = string.Format("已选择{0}个联系人", _GroupMemberList.Count());
            //GroupName = groupInfo.groupName;
            GroupNameWateMark = groupInfo.groupName;
            GroupNameIsReadOnly = true;
        }
        #endregion

        #region 属性
        private ObservableCollection<ContactInfoViewModel> _GroupMemberList = new ObservableCollection<ContactInfoViewModel>();
        /// <summary>
        /// 会话列表
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

        private string _GroupName;
        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string GroupName
        {
            get { return _GroupName; }
            set
            {
                if (_GroupName == value)
                {
                    return;
                }
                _GroupName = value;
                RaisePropertyChanged(() => GroupName);
            }
        }

        private bool _BtnOKIsEnabled = true;
        /// <summary>
        /// 讨论组名称
        /// </summary>
        public bool BtnOKIsEnabled
        {
            get { return _BtnOKIsEnabled; }
            set
            {
                if (_BtnOKIsEnabled == value)
                {
                    return;
                }
                _BtnOKIsEnabled = value;
                RaisePropertyChanged(() => BtnOKIsEnabled);
            }
        }

        private string _GroupNameWateMark = "讨论组名称（必填）";
        /// <summary>
        /// 讨论组名称水印提示信息
        /// </summary>
        /// （讨论组名初始值通过该变量赋值，用GroupName变量赋值不了）
        public string GroupNameWateMark
        {
            get { return _GroupNameWateMark; }
            set
            {
                if (_GroupNameWateMark == value)
                {
                    return;
                }
                _GroupNameWateMark = value;
                RaisePropertyChanged(() => GroupNameWateMark);
            }
        }

        private bool _GroupNameIsReadOnly = false;
        /// <summary>
        /// 讨论组名称是否只读
        /// </summary>
        public bool GroupNameIsReadOnly
        {
            get { return _GroupNameIsReadOnly; }
            set
            {
                if (_GroupNameIsReadOnly == value)
                {
                    return;
                }
                _GroupNameIsReadOnly = value;
                RaisePropertyChanged(() => GroupNameIsReadOnly);
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

        //private Thickness _GroupNameBorderThickness = new Thickness(0);
        ///// <summary>
        ///// 讨论组名称边框厚度
        ///// </summary>
        //public Thickness GroupNameBorderThickness
        //{
        //    get { return _GroupNameBorderThickness; }
        //    set
        //    {
        //        if (_GroupNameBorderThickness == value)
        //        {
        //            return;
        //        }
        //        _GroupNameBorderThickness = value;
        //        RaisePropertyChanged(() => GroupNameBorderThickness);
        //    }
        //}

        private System.Windows.Media.Brush _GroupNameBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e0e0e0"));
        /// <summary>
        /// 讨论组名称边框颜色
        /// </summary>
        public System.Windows.Media.Brush GroupNameBorderBrush
        {
            get { return _GroupNameBorderBrush; }
            set
            {
                if (_GroupNameBorderBrush == value)
                {
                    return;
                }
                _GroupNameBorderBrush = value;
                RaisePropertyChanged(() => GroupNameBorderBrush);
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
                        App.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.close.Invoke();
                        }));
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
                        BtnOKIsEnabled = false;
                        if (IsCreateGroup)
                        {
                            CreateGroup();
                        }
                        else
                        {
                            UpdateGroup();
                        }
                        BtnOKIsEnabled = true;
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
                        var wateMarkTextBox = o as WateMarkTextBox;
                        if (wateMarkTextBox != null) QueryCondition = wateMarkTextBox.Text;
                        if (string.IsNullOrEmpty(QueryCondition))
                        {
                            this.LeftPartViewModel = ContactListViewModel;
                            if (_QueryContactList?.QueryContactList != null &&
                                _QueryContactList.QueryContactList?.Count > 0)
                            {
                                _QueryContactList.QueryContactList.Clear();
                                _QueryContactList.QueryContactList = null;
                                _QueryContactList = null;
                            }
                        }
                        else
                        {
                            if (_QueryContactList == null)
                            {
                                _QueryContactList = new QueryContactListViewModel(QueryCondition, GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewLeft);
                            }
                            else
                            {
                                _QueryContactList.QueryCondition(QueryCondition);
                            }
                            if (_QueryContactList.QueryContactList != null && _QueryContactList.QueryContactList.Count > 0)
                            {
                                foreach (ContactInfoViewModel vm in _QueryContactList.QueryContactList)
                                {
                                    if (OriginalMemberIds != null && OriginalMemberIds.Contains(vm.User.userId))
                                    {
                                        vm.SetExistGroupMember();
                                    }
                                    else
                                    {
                                        ContactInfoViewModel tempVM = GroupMemberList.FirstOrDefault(c => c.User.userId == vm.User.userId);
                                        if (tempVM != null)
                                        {
                                            vm.OnStateImageClickEventEvent(vm, true);
                                        }
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
                    ContactInfoViewModel tempVM = new ContactInfoViewModel(senderVM.User, GlobalVariable.ContactInfoViewContainer.GroupEditWindowViewRight);
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
        private void CreateGroup()
        {
            // 讨论组名不能为空
            //if (string.IsNullOrWhiteSpace(GroupName))
            //{
            //    GroupNameBorderBrush = new SolidColorBrush(Colors.Red);
            //    //GroupNameBorderThickness = new Thickness(1, 1, 1, 1);
            //    return;
            //}
            //else
            //{
            //    GroupNameBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e0e0e0"));
            //    //GroupNameBorderThickness = new Thickness(0);
            //}
            // 讨论组成员不能少于3
            if (GroupMemberList == null || GroupMemberList.Count < 3)
            {
                MessageBoxWindow.Show("讨论组至少需要包括三个成员", GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            if (string.IsNullOrWhiteSpace(GroupName.Trim(' ')))//自定义群名为空 取默认值
            {
                GroupName = GetNewGroupName();
            }
            GroupName = GroupName.TrimStart(' ');
            #region 旧代码
            CreateGroupInput input = new CreateGroupInput();
            input.token = AntSdkService.AntSdkLoginOutput.token;
            input.version = GlobalVariable.Version;
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.groupName = GroupName;
            //获取讨论组成员头像
            //List<string> picList = new List<string>();
            //foreach (ContactInfoViewModel m in GroupMemberList)
            //{
            //    picList.Add(m.Photo);
            //}
            //input.groupPicture = ImageHandle.GetGroupPicture(picList);
            //input.groupPicture =;
            input.userIds = string.Join(",", GroupMemberList.Select(c => c.User.userId).ToArray());
            //CreateGroupOutput output = new CreateGroupOutput();
            var errCode = 0;
            var errMsg = string.Empty;

            //if (!(new HttpService()).CreateGroup(input, ref output, ref errMsg))
            //{
            //    if (output.errorCode != "1004")
            //    {
            //        MessageBoxWindow.Show(errMsg,GlobalVariable.WarnOrSuccess.Warn);
            //    }
            //    return;
            //}
            //output.group.groupPicture = ImageHandle.GetGroupPicture(GroupMemberList.Select(c => c.Photo).ToList());
            //ThreadPool.QueueUserWorkItem(m =>
            //{

            //});
            #endregion

            AntSdkCreateGroupInput newGroupInput = new AntSdkCreateGroupInput();
            newGroupInput.userId = AntSdkService.AntSdkLoginOutput.userId;
            newGroupInput.groupName = GroupName;
            newGroupInput.userIds = GroupMemberList.Select(c => c.User.userId).ToArray();
            //newGroupInput.groupPicture = ImageHandle.GetGroupPicture(GroupMemberList.Select(c => c.Photo).ToList());
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkCreateGroupOutput createGroupOutput = AntSdkService.CreateGroup(newGroupInput, ref errCode, ref errMsg);
            if (createGroupOutput == null)
            {
                if (!AntSdkService.AntSdkIsConnected)
                    errMsg = "网络已断开，请检查网络";
                if (!string.IsNullOrEmpty(errMsg))
                {
                    MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                }
                return;
            }
            createGroupOutput.groupPicture = ImageHandle.GetGroupPicture(GroupMemberList.Select(c => c.Photo).ToList());
            //异步更新讨论组头像，这里需要用前台线程，因此不能用线程池
            string[] ThreadParams = new string[3];
            ThreadParams[0] = createGroupOutput.groupId;
            ThreadParams[1] = createGroupOutput.groupPicture;
            ThreadParams[2] = string.IsNullOrEmpty(createGroupOutput.groupName) ? "" : createGroupOutput.groupName;
            Thread UpdateGroupPictureThread = new Thread(GroupPublicFunction.UpdateGroupPicture);
            UpdateGroupPictureThread.Start(ThreadParams);
            this.CreateGroupOutput = createGroupOutput;
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.close.Invoke();
            }));
        }
        /// <summary>
        /// 根据成员名字组成默认群名
        /// </summary>
        /// <returns></returns>
        private string GetNewGroupName()
        {
            var newName = string.Empty;
            var newNameBd = new StringBuilder();
            var nameList= GroupMemberList.Select(c => c.User.userName).ToArray();
            foreach(var name in nameList)
            {
                if(newNameBd.Length>=10)
                {                    
                    break;
                }
                else
                {
                    newNameBd.Append(name + ",");
                }
            }
            newName = newNameBd.ToString().Substring(0, 10) + "...";
            return newName;
        }

        public List<AntSdkContact_User> NewGroupMemberList;
        //public string NewGroupPicture;
        /// <summary>
        /// 更新讨论组
        /// </summary>
        private void UpdateGroup()
        {
            if (GroupInfo == null) return;
            AntSdkUpdateGroupInput input = new AntSdkUpdateGroupInput();
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.groupId = this.GroupInfo.groupId;
            NewGroupMemberList = new List<AntSdkContact_User>();
            foreach (ContactInfoViewModel vm in GroupMemberList)
            {
                if (!OriginalMemberIds.Contains(vm.User.userId))
                {
                    if (input.userIds == null)
                        input.userIds = new List<string>();
                    input.userIds.Add(vm.User.userId);
                    if (input.userNames == null)
                        input.userNames = new List<string>();
                    input.userNames.Add(vm.User.userName);
                    NewGroupMemberList.Add(vm.User);
                }
            }
            if (NewGroupMemberList.Count != 0)
            {
                BaseOutput output = new BaseOutput();
                var errCode = 0;
                string errMsg = string.Empty;
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                var isResult = AntSdkService.UpdateGroup(input, ref errCode, ref errMsg);
                if (!isResult)
                {
                    NewGroupMemberList = null;
                    MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
                    return;
                }
                //if (!(new HttpService()).UpdateGroup(input, ref output, ref errMsg))
                //{
                //    NewGroupMemberList = null;
                //    if (output.errorCode != "1004")
                //    {
                //        MessageBoxWindow.Show(errMsg,GlobalVariable.WarnOrSuccess.Warn);
                //    }
                //    return;
                //    //OnUpdateGroupEvent(input.groupId, input.userIds);
                //}
                string[] ThreadParams = new string[3];
                ThreadParams[0] = this.GroupInfo.groupId;
                ThreadParams[1] = ImageHandle.GetGroupPicture(GroupMemberList.Select(c => c.Photo).ToList());
                ThreadParams[2] = string.IsNullOrEmpty(this.GroupInfo.groupName) ? "" : this.GroupInfo.groupName;
                Thread UpdateGroupPictureThread = new Thread(GroupPublicFunction.UpdateGroupPicture);
                UpdateGroupPictureThread.Start(ThreadParams);
                //this.NewGroupPicture = input.groupPicture;
            }
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.close.Invoke();
            }));
        }


        //public delegate void UpdateGroupDelegate(string groupId, string newUserIds);
        //public static event UpdateGroupDelegate UpdateGroupEvent;
        //private void OnUpdateGroupEvent(string groupId, string newUserIds)
        //{
        //    if (UpdateGroupEvent != null)
        //    {
        //        UpdateGroupEvent(groupId, newUserIds);
        //    }
        //}
        #endregion
    }
}
