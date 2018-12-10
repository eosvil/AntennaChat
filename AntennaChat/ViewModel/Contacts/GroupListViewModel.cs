/*
Author: tanqiyan
Update date: 2017-04-20
Description：讨论组列表
--------------------------------------------------------------------------------------------------------
Versions：
    V1.00 2017-04-20 tanqiyan 描述：增加讨论组分类功能
*/
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using AntennaChat.Views.Contacts;
using Microsoft.Practices.Prism;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class GroupListViewModel : PropertyNotifyObject
    {
        #region 属性

        private ObservableCollection<GroupInfoViewModel> _manageGroupInfoList = new ObservableCollection<GroupInfoViewModel>();
        public ObservableCollection<GroupInfoViewModel> ManageGroupInfoList
        {
            get
            {
                return this._manageGroupInfoList;
            }
            set
            {
                this._manageGroupInfoList = value;
            }
        }

        private ObservableCollection<GroupInfoViewModel> _joinGroupInfoList = new ObservableCollection<GroupInfoViewModel>();
        public ObservableCollection<GroupInfoViewModel> JoinGroupInfoList
        {
            get
            {
                return this._joinGroupInfoList;
            }
            set
            {
                this._joinGroupInfoList = value;
            }
        }

        private ObservableCollection<GroupInfoViewModel> _GroupInfoList = new ObservableCollection<GroupInfoViewModel>();
        public ObservableCollection<GroupInfoViewModel> GroupInfoList
        {
            get
            {
                return this._GroupInfoList;
            }
            set
            {
                this._GroupInfoList = value;
            }
        }

        private GroupInfoViewModel _currentSelectedGroupInfoVM;
        private GroupInfoViewModel tempGroupInfoViewModel;
        /// <summary>
        /// 讨论组当前选中项
        /// </summary>
        public GroupInfoViewModel CurrentSelectedGroupInfoVM
        {
            get
            {
                return this._currentSelectedGroupInfoVM;
            }
            set
            {
                this._currentSelectedGroupInfoVM = value;
                RaisePropertyChanged(() => CurrentSelectedGroupInfoVM);
            }
        }

        private int _manageGroupCount;

        public int ManageGroupCount
        {
            get { return _manageGroupCount; }
            set
            {
                _manageGroupCount = value;
                RaisePropertyChanged(() => ManageGroupCount);
            }

        }

        private int _joinGroupCount;

        public int JoinGroupCount
        {
            get { return _joinGroupCount; }
            set
            {
                _joinGroupCount = value;
                RaisePropertyChanged(() => JoinGroupCount);
            }
        }

        private bool _isManageGroup = true;

        public bool IsManageGroup
        {
            get { return _isManageGroup; }
            set
            {
                _isManageGroup = value;
                RaisePropertyChanged(() => IsManageGroup);
            }

        }

        private bool _isJoinGroup = true;

        public bool IsJoinGroup
        {
            get { return _isJoinGroup; }
            set
            {
                _isJoinGroup = value;
                RaisePropertyChanged(() => IsJoinGroup);
            }
        }


        public List<AntSdkGroupInfo> GroupInfos;

        #endregion

        #region 事件
        public static event Action DropOutGroupEvent;
        public static event Action AddEvent;

        #endregion
        private volatile List<string> GroupIds = new List<string>();//用于去重（用GroupInfoList判读太慢，多线程有问题）

        public GroupListViewModel()
        {

        }
        /// <summary>
        /// 群组列表初始化
        /// </summary>
        public void InitGroupVM()
        {
            GroupMemberViewModel.DismissGroupHandlerHidden += GroupMemberViewModel_DismissGroupHandlerHidden;
            //ResetGroupList();
            GroupInfoViewModel.DropOutGroupEvent += DropOutGroup;
            TalkGroupViewModel.ExitGroupEvent += DropOutGroup;
            TalkGroupViewModel.InviteToGroupEvent += AddGroupMember;
            TalkViewModel.CreateGroupEvent += AddGroupAndClick;
        }

        private void GroupMemberViewModel_DismissGroupHandlerHidden(object sender, EventArgs e)
        {
            if (GroupInfoList != null && GroupInfoList.Any())
            {

                GroupInfoViewModel groupInfo =
                    GroupInfoList.SingleOrDefault(m => m.GroupInfo.groupId == sender as string);
                if (groupInfo != null)
                {
                    groupInfo.DeleteGroupVisibility = Visibility.Collapsed;
                    groupInfo.GroupClassify = 2;
                    //groupInfo.GroupClassifyName = "我加入的";
                    ManageGroupInfoList.Remove(groupInfo);
                    JoinGroupInfoList.Insert(0, groupInfo);
                    GroupInfoList.Remove(groupInfo);
                    GroupInfoList.Add(groupInfo);
                    //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(GroupInfoList);
                    //if (view != null)
                    //{
                    //    view.GroupDescriptions.Clear();
                    //    PropertyGroupDescription groupDescription = new PropertyGroupDescription("GroupClassifyName");
                    //    view.GroupDescriptions.Add(groupDescription);
                    //}
                }
            }
        }

        #region 命令/方法
        private ICommand _mouseDoubleClick;
        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        public ICommand MouseDoubleClick
        {
            get
            {
                if (this._mouseDoubleClick == null)
                {
                    this._mouseDoubleClick = new DefaultCommand(o =>
                    {
                        CurrentSelectedGroupInfoVM?.OnMouseDoubleClickEvent();
                    });
                }
                return this._mouseDoubleClick;
            }
        }
        /// <summary>
        /// 获取当前用户的所有群组，并请求群组离线消息
        /// </summary>
        public void GetGroupList()
        {
            var temperrorCode = 0;
            var temperrorMsg = string.Empty;
            var groups = AntSdkService.GetGroupList(AntSdkService.AntSdkLoginOutput.userId, ref temperrorCode, ref temperrorMsg);
            if (!string.IsNullOrEmpty(temperrorMsg))
            {
                Application.Current.Dispatcher.Invoke(new Action(() => MessageBoxWindow.Show(temperrorMsg, GlobalVariable.WarnOrSuccess.Warn)));
            }
            if (groups == null || groups.Length == 0) { return; }
            GroupInfos = new List<AntSdkGroupInfo>(groups);
        }
        /// <summary>
        /// 群组展示
        /// </summary>
        public void ResetGroupList()
        {
            if (GroupInfos == null)
            {
                return;
            }
            //List<string> topics = new List<string>();
            List<QueryMsgInput_Group> queryMsgInput_Groups = new List<QueryMsgInput_Group>();
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                foreach (AntSdkGroupInfo info in GroupInfos)
                {
                    //stopWatch.Start();
                    //int unreadCount = 0;
                    ////删除取消消息免打扰功能的记录
                    //if (noRemindGroupList != null
                    //    && noRemindGroupList.Count > 0)
                    //{
                    //    T_NoRemindGroup noRemindGroup = noRemindGroupList.FirstOrDefault(c => c.GroupId == info.groupId);
                    //    if (noRemindGroup != null)
                    //    {
                    //        if (info.state == (int)GlobalVariable.MsgRemind.Remind)
                    //        {
                    //            ThreadPool.QueueUserWorkItem(m => T_NoRemindGroupBll.Delete(noRemindGroup));
                    //        }
                    //        else
                    //        {
                    //            unreadCount = noRemindGroup.UnreadCount;
                    //            //T_NoRemindGroupBll.GetModelByKey(info.groupId);
                    //        }
                    //    }
                    //}
                    GroupIds.Add(info.groupId);
                    GroupInfoViewModel groupInfoVM = new GroupInfoViewModel(info);

                    _GroupInfoList.Add(groupInfoVM);
                    //topics.Add(info.groupId);

                    QueryMsgInput_Group queryMsgInput_Group = new QueryMsgInput_Group();
                    //queryMsgInput_Group.companyCode = GlobalVariable.CompanyCode;
                    queryMsgInput_Group.sessionId = info.groupId;
                    //queryMsgInput_Group.sendUserId = AntSdkService.AntSdkLoginOutput.userId;
                    queryMsgInput_Group.chatIndex = string.Empty; //chatIndex为空，服务端返回100条消息
                    queryMsgInput_Groups.Add(queryMsgInput_Group);
                    //stopWatch.Stop();
                    //LogHelper.WriteDebug(string.Format("[LoadAction_GroupInfoViewModel({0}毫秒)]", stopWatch.Elapsed.TotalMilliseconds));
                }


            }, DispatcherPriority.Background);



        }
        //加载界面集合数据
        public void LoadGroupListData()
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                if (GroupInfoList != null && GroupInfoList.Count > 0)
                {
                    //var groupInfoLst = GroupInfoList.Where(m => m.GroupClassify == 0);
                    //if (groupInfoLst != null)
                    //{
                    //    var tempGroupLst = groupInfoLst.ToList();
                    //    for (int i = 0; i < tempGroupLst.Count; i++)
                    //    {
                    //        var groupInfo = tempGroupLst[i];
                    //        groupInfo.GetMembers();
                    //        tempGroupLst[i] = groupInfo;
                    //    }
                    //}

                    //我管理的
                    var tempGroupInfoLst = GroupInfoList.Where(m => m.GroupClassify == 1);
                    var groupInfoViewModels = tempGroupInfoLst as IList<GroupInfoViewModel> ?? tempGroupInfoLst.ToList();
                    if (ManageGroupInfoList.Count == 0 || ManageGroupInfoList.Count != groupInfoViewModels.Count())
                    {
                        foreach (var groupInfo in groupInfoViewModels)
                        {
                            if (!ManageGroupInfoList.Contains(groupInfo) && !JoinGroupInfoList.Contains(groupInfo))
                                ManageGroupInfoList.Add(groupInfo);
                        }
                    }

                    //我加入的
                    var tempJoinGroupInfoLst = GroupInfoList.Where(m => m.GroupClassify == 2);
                    var joinGroupInfoLst = tempJoinGroupInfoLst as IList<GroupInfoViewModel> ?? tempJoinGroupInfoLst.ToList();
                    if (JoinGroupInfoList.Count == 0 || JoinGroupInfoList.Count != joinGroupInfoLst.Count())
                    {
                        foreach (var groupInfo in joinGroupInfoLst)
                        {
                            if (!JoinGroupInfoList.Contains(groupInfo) && !ManageGroupInfoList.Contains(groupInfo))
                                JoinGroupInfoList.Add(groupInfo);
                        }
                    }
                    ManageGroupCount = ManageGroupInfoList.Count;
                    JoinGroupCount = JoinGroupInfoList.Count;
                    IsManageGroup = ManageGroupCount != 0;
                    IsJoinGroup = JoinGroupCount != 0;
                }
            }, DispatcherPriority.Background);
        }

        object objLock = new object();
        /// <summary>
        /// 创建新讨论组时调用
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public GroupInfoViewModel AddGroup(AntSdkCreateGroupOutput group)
        {
            //不为空||去除重复的
            if (group == null)
            {
                return null;
            }
            else if (GroupIds.Contains(group.groupId))
            {
                return GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == group.groupId);
            }
            var newGroup = new AntSdkGroupInfo
            {
                groupId = @group.groupId,
                groupName = @group.groupName,
                groupPicture = @group.groupPicture,
                state = (int)GlobalVariable.MsgRemind.Remind,
                groupOwnerId = group.groupOwnerId,
                memberCount = group.memberCount
            };
            return AddGroupInfoModel(newGroup);
        }
        /// <summary>
        /// 离线时增加的群组（场景：服务断开重连）
        /// </summary>
        /// <param name="groupInfos"></param>
        public void AddGroups(List<AntSdkGroupInfo> groupInfos)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                for (int i = 0; i < groupInfos.Count; i++)
                {
                    var groupInfo =groupInfos[i];
                    AddGroupInfoModel(groupInfo);
                }
            });
        }
        /// <summary>
        /// 新增群组model组装
        /// </summary>
        /// <param name="newGroup"></param>
        /// <returns></returns>
        private GroupInfoViewModel AddGroupInfoModel(AntSdkGroupInfo newGroup)
        {
            //if (GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == group.groupId) != null) return;
            if (GroupIds.Contains(newGroup.groupId))
            {
                var tempGroupInfoVm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == newGroup.groupId);
                if (tempGroupInfoVm != null)
                {
                    if (tempGroupInfoVm.GroupPicture != newGroup.groupPicture)
                    {
                        tempGroupInfoVm.GroupPicture = newGroup.groupPicture;
                    }
                    var classify = newGroup.groupOwnerId == AntSdkService.AntSdkCurrentUserInfo.userId
                        || (newGroup.managerIds != null && newGroup.managerIds.Contains(AntSdkService.AntSdkCurrentUserInfo.userId)) ? 1 : 2;
                    if (tempGroupInfoVm.GroupClassify != classify)
                    {
                        tempGroupInfoVm.GroupClassify = classify;
                        if (ManageGroupInfoList.Contains(tempGroupInfoVm))
                            ManageGroupInfoList.Remove(tempGroupInfoVm);
                        if (JoinGroupInfoList.Contains(tempGroupInfoVm))
                            JoinGroupInfoList.Remove(tempGroupInfoVm);
                        if (tempGroupInfoVm.GroupClassify == 1)
                        {
                            ManageGroupInfoList.Insert(0, tempGroupInfoVm);
                        }
                        else
                        {
                            JoinGroupInfoList.Insert(0, tempGroupInfoVm);
                        }
                        ManageGroupCount = ManageGroupInfoList.Count;
                        JoinGroupCount = JoinGroupInfoList.Count;
                    }
                }
                //tempGroupInfoVm = groupInfoVM;
                return tempGroupInfoVm;
            }
            GroupInfoViewModel groupInfoVM = new GroupInfoViewModel(newGroup);
            if (GroupInfos == null)
                GroupInfos = new List<AntSdkGroupInfo>();
            GroupInfos.Add(newGroup);
            GroupIds.Add(newGroup.groupId);
            GroupInfoList.Add(groupInfoVM);
            if (groupInfoVM.GroupClassify == 1)
                ManageGroupInfoList.Insert(0, groupInfoVM);
            else
                JoinGroupInfoList.Insert(0, groupInfoVM);
            ManageGroupCount = ManageGroupInfoList.Count;
            JoinGroupCount = JoinGroupInfoList.Count;
            if (ManageGroupCount > 0)
                IsManageGroup = true;
            if (JoinGroupCount > 0)
                IsJoinGroup = true;
            //if (GroupInfoList != null && GroupInfoList.Any())
            //{
            //    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(GroupInfoList);
            //    if (view != null)
            //    {
            //        view.GroupDescriptions.Clear();
            //        PropertyGroupDescription groupDescription = new PropertyGroupDescription("GroupClassify");
            //        view.GroupDescriptions.Add(groupDescription);
            //    }
            //}
            List<string> topics = new List<string>();
            topics.Add(newGroup.groupId);
            return groupInfoVM;
        }

        public void AddGroupAndClick(AntSdkCreateGroupOutput group)
        {
            GroupInfoViewModel vm = AddGroup(group);
            vm?.OnMouseDoubleClickEvent();
        }
        /// <summary>
        /// 退出讨论组时调用
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="args"></param>
        private void DropOutGroup(object viewModel, EventArgs args)
        {
            GroupInfoViewModel vm = viewModel as GroupInfoViewModel;
            if (vm == null) return;
            GroupIds.Remove(vm.GroupInfo.groupId);
            GroupInfoList.Remove(vm);
            GroupInfos.Remove(vm.GroupInfo);
            if (ManageGroupInfoList.Contains(vm))
                ManageGroupInfoList.Remove(vm);
            if (JoinGroupInfoList.Contains(vm))
                JoinGroupInfoList.Remove(vm);
            ManageGroupCount = ManageGroupInfoList.Count;
            JoinGroupCount = JoinGroupInfoList.Count;
            if (ManageGroupCount == 0)
                IsManageGroup = false;
            if (JoinGroupCount == 0)
                IsJoinGroup = false;
            List<string> topics = new List<string>();
            topics.Add(vm.GroupInfo.groupId);
        }

        /// <summary>
        /// 被移出讨论组时调用
        /// </summary>
        /// <param name="groupId"></param>
        public void DropOutGroup(string groupId)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                GroupInfoViewModel vm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == groupId);
                if (vm == null) return;
                GroupIds.Remove(vm.GroupInfo.groupId);
                //GroupInfoList.Remove(vm);
                GroupInfoList.Remove(vm);
                GroupInfos.Remove(vm.GroupInfo);
                if (ManageGroupInfoList.Contains(vm))
                    ManageGroupInfoList.Remove(vm);
                if (JoinGroupInfoList.Contains(vm))
                    JoinGroupInfoList.Remove(vm);
                ManageGroupCount = ManageGroupInfoList.Count;
                JoinGroupCount = JoinGroupInfoList.Count;
                if (ManageGroupCount == 0)
                    IsManageGroup = false;
                if (JoinGroupCount == 0)
                    IsJoinGroup = false;
                List<string> topics = new List<string>();
                topics.Add(vm.GroupInfo.groupId);
            });
        }
        /// <summary>
        /// 讨论组成员信息被修改时调用（比如：被转让为管理员角色）
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="members"></param>
        public void UpdateGroupMemeber(string groupId, List<AntSdkGroupMember> members)
        {
            GroupInfoViewModel vm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == groupId);
            if (vm == null) return;
            if (members != null)
                vm.UpdateMembers(members);
            GroupInfoListUpdate(vm);
            CurrentSelectedGroupInfoVM?.StructureDetailsVM?.InitDetails(DetailType.Group, vm, vm.Members, false);
        }
        /// <summary>
        /// 讨论组信息被修改时调用
        /// </summary>
        /// <param name="sysMsg"></param>
        public void UpdateGroupInfo(AntSdkReceivedGroupMsg.Modify sysMsg)
        {
            GroupInfoViewModel vm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == sysMsg.sessionId);
            if (vm == null) return;
            vm.UpdateGroupInfo(sysMsg);
            GroupInfoListUpdate(vm);
        }

        /// <summary>
        /// 更新讨论组免打扰设置
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="state"></param>
        public void UpdateMsgRemind(string groupId, int state)
        {
            GroupInfoViewModel vm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == groupId);
            if (state == (int)GlobalVariable.MsgRemind.NoRemind)
            {
                if (vm == null) return;
                vm.MessageHideIsChecked = true;
                vm.ImageNoRemindVisibility = Visibility.Visible;
                vm.MessageNoticeIsChecked = false;
                //CurrentSelectedGroupInfoVM?.StructureDetailsVM?.SetMsgRemindState(GlobalVariable.MsgRemind.NoRemind);
            }
            else
            {
                if (vm == null) return;
                vm.MessageHideIsChecked = false;
                vm.ImageNoRemindVisibility = Visibility.Collapsed;
                vm.MessageNoticeIsChecked = true;
                //CurrentSelectedGroupInfoVM?.StructureDetailsVM?.SetMsgRemindState(GlobalVariable.MsgRemind.Remind);
            }
        }
        private void AddGroupMember(string groupId, string picture, List<AntSdkContact_User> newGroupMemberList)
        {
            GroupInfoViewModel vm = GroupInfoList.FirstOrDefault(c => c.GroupInfo.groupId == groupId);
            vm?.AddNewMember(picture, newGroupMemberList);
        }
        /// <summary>
        /// 讨论组成员信息修改后刷新列表
        /// </summary>
        /// <param name="vm"></param>
        private void GroupInfoListUpdate(GroupInfoViewModel vm)
        {
            if (vm.Members == null)
                return;
            var tempUser = vm.Members.Find(c => c.userId == AntSdkService.AntSdkLoginOutput.userId
            && (c.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner || c.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin));
            if (tempUser != null)
            {
                if (JoinGroupInfoList.Contains(vm))
                    JoinGroupInfoList.Remove(vm);
                if (!ManageGroupInfoList.Contains(vm))
                {
                    vm.GroupClassify = 1;
                    vm.DeleteGroupVisibility = Visibility.Visible;
                    ManageGroupInfoList.Insert(0, vm);
                }
            }
            else
            {
                if (ManageGroupInfoList.Contains(vm))
                    ManageGroupInfoList.Remove(vm);
                if (!JoinGroupInfoList.Contains(vm))
                {
                    vm.GroupClassify = 2;
                    vm.DeleteGroupVisibility = Visibility.Collapsed;
                    JoinGroupInfoList.Insert(0, vm);
                }
            }
            ManageGroupCount = ManageGroupInfoList.Count;
            JoinGroupCount = JoinGroupInfoList.Count;
            IsManageGroup = ManageGroupCount != 0;
            IsJoinGroup = JoinGroupCount != 0;
        }

        #endregion
    }
}
