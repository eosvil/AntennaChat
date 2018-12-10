using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Talk
{
    public class GroupMemberListViewModel : PropertyNotifyObject,IDisposable
    {
        private string GroupOwnerId;//讨论组管理员ID
        private AntSdkGroupMember groupAdminUser;
        AntSdkGroupInfo GroupInfo;
        private List<AntSdkGroupMember> _groupMembers;
        private int adminCount;
        #region 构造器
        public GroupMemberListViewModel(List<AntSdkGroupMember> GroupMembers, AntSdkGroupInfo groupInfo)
        {
            this.GroupInfo = groupInfo;
            _groupMembers = GroupMembers;
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                AntSdkGroupMember groupOwner =
                    GroupMembers.FirstOrDefault(c => c.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner);
                GroupOwnerId = groupOwner == null ? "" : groupOwner.userId;
                LogHelper.WriteFatal(groupInfo.groupId + "-------" + groupInfo.groupName + "群组的群主： " +
                                     groupOwner?.userNum + groupOwner?.userName);
                groupAdminUser = GroupMembers.FirstOrDefault(c =>
                    c.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin &&
                    c.userId == AntSdkService.AntSdkLoginOutput.userId);
                adminCount = GroupMembers.Count(m => m.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin);
                var orderByDesGroupMembers = GroupMembers.OrderByDescending(m => m.roleLevel).ToList();
                foreach (AntSdkGroupMember user in orderByDesGroupMembers)
                {

                    GroupMemberViewModel groupMemberViewModel = new GroupMemberViewModel(user, GroupOwnerId,
                        GroupInfo.groupId, this, SearchGroupName, adminCount);
                    if (user.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner)
                        GroupMemberControlList.Insert(0, groupMemberViewModel);
                    else if (user.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin)
                    {
                        GroupMemberControlList.Insert(GroupMemberControlList.Count > 0 ? 1 : 0, groupMemberViewModel);
                    }
                    else
                    {
                        var userinfo =
                            AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == user.userId);
                        groupMemberViewModel.KickoutGroupVisibility = groupAdminUser != null ||
                                                                      GroupOwnerId ==
                                                                      AntSdkService.AntSdkLoginOutput.userId
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        if (userinfo != null && userinfo.state != (int)GlobalVariable.OnLineStatus.OffLine)
                        {
                            var index =
                                GroupMemberControlList.Count(
                                    m =>
                                        !m.IsGroupAdminImage && !m.IsOfflineState &&
                                        m.AdminImageVisibility != Visibility.Visible);
                            GroupMemberControlList.Insert(index + adminCount + 1, groupMemberViewModel);
                        }
                        else
                        {
                            GroupMemberControlList.Add(groupMemberViewModel);
                        }

                    }
                }
            });
            SearchGroupNameCommand = new DelegateCommand(GroupNameSearch);
            GoMemberSessionCommand = new DelegateCommand(goMemberSession);
            GroupMemberViewModel.KickoutGroupEvent += KickOutGroup;
            GroupMemberViewModel.ChangeManagerCompletedEvent += GroupMemberViewModel_ChangeManagerCompletedEvent;
            MemberCountPrompt = string.Format("群成员({0}人)", GroupMembers.Count);
        }

        
        #endregion

        #region 属性
        private ObservableCollection<GroupMemberViewModel> _GroupMemberControlList = new ObservableCollection<GroupMemberViewModel>();
        /// <summary>
        /// 讨论组成员列表
        /// </summary>
        public ObservableCollection<GroupMemberViewModel> GroupMemberControlList
        {
            get { return this._GroupMemberControlList; }
            set
            {
                this._GroupMemberControlList = value;
                RaisePropertyChanged(() => GroupMemberControlList);
            }
        }

        private ObservableCollection<GroupMemberViewModel> _searchGroupMemberControlList = new ObservableCollection<GroupMemberViewModel>();
        /// <summary>
        /// 讨论组成员搜索列表
        /// </summary>
        public ObservableCollection<GroupMemberViewModel> SearchGroupMemberControlList
        {
            get { return this._searchGroupMemberControlList; }
            set
            {
                this._searchGroupMemberControlList = value;
                RaisePropertyChanged(() => SearchGroupMemberControlList);
            }
        }

        private string _MemberCountPrompt;
        /// <summary>
        /// 成员数提示
        /// </summary>
        public string MemberCountPrompt
        {
            get { return this._MemberCountPrompt; }
            set
            {
                this._MemberCountPrompt = value;
                RaisePropertyChanged(() => MemberCountPrompt);
            }
        }

        private string _searchGroupName;
        /// <summary>
        /// 讨论组成员搜索关键字
        /// </summary>
        public string SearchGroupName
        {
            get { return _searchGroupName; }
            set
            {
                this._searchGroupName = value;
                RaisePropertyChanged(() => SearchGroupName);
            }
        }


        private bool _isShowSearchList;
        /// <summary>
        /// 搜索列表是否显示
        /// </summary>
        public bool IsShowSearchList
        {
            get { return _isShowSearchList; }
            set
            {
                _isShowSearchList = value;
                RaisePropertyChanged(() => IsShowSearchList);
            }
        }

        private bool _isExistData;
        /// <summary>
        /// 是否存在搜索数据
        /// </summary>
        public bool IsExistData
        {
            get { return _isExistData; }
            set
            {
                _isExistData = value;
                RaisePropertyChanged(() => IsExistData);
            }
        }

        private bool _isSearchGroupMeber;
        /// <summary>
        /// 是否进行组成员搜索
        /// </summary>
        public bool IsSearchGroupMeber
        {
            get
            {
                if (!_isSearchGroupMeber)
                {
                    IsShowSearchList = false;
                    SearchGroupName = "";
                }
                return _isSearchGroupMeber;
            }
            set
            {
                _isSearchGroupMeber = value;
                RaisePropertyChanged(() => IsSearchGroupMeber);
            }
        }

        private GroupMemberViewModel _groupMeberSelected;
        /// <summary>
        /// 搜索列表当前选中行
        /// </summary>
        public GroupMemberViewModel GroupMeberSelected
        {
            get { return _groupMeberSelected; }
            set
            {
                _groupMeberSelected = value;
                RaisePropertyChanged(() => GroupMeberSelected);
            }
        }



        #endregion
        /// <summary>
        /// Enter跳转到会话
        /// </summary>
        public ICommand GoMemberSessionCommand { get; set; }
        /// <summary>
        /// 讨论组搜索命令
        /// </summary>
        public ICommand SearchGroupNameCommand { get; set; }
        /// <summary>
        /// 点击关闭群列表时触发
        /// </summary>
        public event Action CloseGroupListEvent;
        /// <summary>
        /// 回退事件
        /// </summary>
        public ICommand btnCommandBackTalkMsg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    CloseGroupListEvent?.Invoke();
                });
            }
        }
        #region 命令/方法
        private void KickOutGroup(string groupId, string userID, string picture)
        {
            if (GroupInfo.groupId != groupId) return;
            GroupMemberViewModel vm = GroupMemberControlList.FirstOrDefault(c => c.Member.userId == userID);
            GroupMemberControlList.Remove(vm);
            MemberCountPrompt = string.Format("群成员({0}人)", GroupMemberControlList.Count);
        }

        /// <summary>
        /// 变更管理员成功时
        /// </summary>
        /// <param name="groupId"></param>
        private void GroupMemberViewModel_ChangeManagerCompletedEvent(string groupId)
        {
            //if (GroupInfo.groupId != groupId) return;
            //GroupMemberViewModel vm = GroupMemberControlList.FirstOrDefault(c => c.Member.userId ==AntSdkService.AntSdkLoginOutput.userId &&c.GroupId==groupId);
            //if (vm != null)
            //{
            //    vm.AdminImageVisibility = Visibility.Hidden;
            //    GroupMemberControlList.Remove(vm);
            //    GroupMemberControlList.Insert(0, vm);
            //}
        }
        /// <summary>
        /// 搜索群组成员
        /// </summary>
        public void GroupNameSearch()
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                if (string.IsNullOrWhiteSpace(SearchGroupName))
                {
                    IsShowSearchList = false;
                    SearchGroupMemberControlList.Clear();
                    return;
                }
                if (SearchGroupMemberControlList == null)
                    SearchGroupMemberControlList = new ObservableCollection<GroupMemberViewModel>();
                SearchGroupMemberControlList.Clear();
                if (_groupMembers != null && _groupMembers.Any())
                {

                    //var groupMemberList = _groupMembers.Where(m => IsconditionsSatisfy(m.userName, m.userNum));
                    //if (groupMemberList != null && groupMemberList.Any())
                    //{
                    //    var groupMemberOrderByList = groupMemberList.OrderBy(m => m.userName);
                    //    foreach (var user in groupMemberOrderByList)
                    //    {
                    //        GroupMemberViewModel groupMemberViewModel = new GroupMemberViewModel(user, GroupAdminId, GroupInfo.groupId, this, SearchGroupName);
                    //        groupMemberViewModel.AdminImageVisibility = Visibility.Collapsed;
                    //        SearchGroupMemberControlList.Add(groupMemberViewModel);
                    //    }
                    //}
                    foreach (var member in _groupMembers)
                    {
                        string pinYin = string.Empty;
                        var contactUser= AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(m=>m.userId==member.userId);
                        if (contactUser != null && contactUser.status == 0)
                            continue;
                        if (IsconditionsSatisfy(member.userName, member.userNum, ref pinYin))
                        {
                            GroupMemberViewModel groupMemberViewModel = new GroupMemberViewModel(member, GroupOwnerId,
                                GroupInfo.groupId, this, SearchGroupName, pinYin);
                            groupMemberViewModel.AdminImageVisibility = Visibility.Collapsed;
                            SearchGroupMemberControlList.Add(groupMemberViewModel);
                        }
                    }

                }
                if (!SearchGroupMemberControlList.Any())
                    IsExistData = false;
                else
                {
                    GroupMeberSelected = SearchGroupMemberControlList[0];
                    IsExistData = true;
                }
                IsShowSearchList = true;
            });
        }
        /// <summary>
        /// Enter跳转到会话
        /// </summary>
        public void goMemberSession()
        {
            if (GroupMeberSelected != null)
            {
                if (GroupMeberSelected.Member.userId == AntSdkService.AntSdkLoginOutput.userId) return;
                GroupMeberSelected.OnMouseDoubleClickEvent(GroupMeberSelected);
                SearchGroupName = string.Empty;
            }
        }

        /// <summary>
        /// 搜索满足条件
        /// </summary>
        /// <param name="userName">成员名称</param>
        /// <param name="userNum">成员工号</param>
        private bool IsconditionsSatisfy(string userName, string userNum, ref string resultPinYin)
        {
            string nameNum = string.Empty;
            if (DataConverter.InputIsNum(SearchGroupName))
            {
                if ((!string.IsNullOrEmpty(userNum) && userNum.Contains(SearchGroupName)) || (!string.IsNullOrEmpty(userName) && userName.Contains(SearchGroupName)))
                    return true;
            }
            else if (DataConverter.InputIsChinese(SearchGroupName))
            {
                nameNum = userNum + userName;
                if (!string.IsNullOrEmpty(nameNum) && nameNum.ToLower().Contains(SearchGroupName))
                    return true;
            }
            else
            {
                bool isSpell = false;
                var pinyinName = DataConverter.GetChineseSpellList(userName);
                //if (!string.IsNullOrEmpty(pinyinName))
                //{
                //    nameNum = userNum + pinyinName;
                //    isSpell = nameNum.Contains(SearchGroupName);
                //}
                //if (isSpell || (!string.IsNullOrEmpty(userName) && userName.Contains(SearchGroupName)))
                //    return true;
                if (pinyinName.Any())
                {
                    for (int i = 0; i < pinyinName.Count; i++)
                    {
                        nameNum = userNum + pinyinName[i];
                        if (nameNum.Contains(SearchGroupName))
                        {
                            resultPinYin = nameNum;
                            isSpell = true;
                            break;
                        }
                    }
                    if (isSpell || (!string.IsNullOrEmpty(userName) && userName.Contains(SearchGroupName)))
                        return true;
                }
            }
            return false;
        }

        public void AddNewMember(List<AntSdkContact_User> newGroupMemberList)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                foreach (AntSdkContact_User user in newGroupMemberList)
                {
                    AntSdkGroupMember member = new AntSdkGroupMember();
                    member.picture = user.picture;
                    member.position = user.position;
                    member.roleLevel = (int)GlobalVariable.GroupRoleLevel.Ordinary;
                    member.userId = user.userId;
                    member.userName = user.userName;
                    GroupMemberViewModel groupMemberViewModel = new GroupMemberViewModel(member, GroupOwnerId,
                        GroupInfo.groupId, this, SearchGroupName, adminCount);
                    //groupMemberViewModel.AdminCount = adminCount;
                    var userinfo =
                        AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == user.userId);
                    groupMemberViewModel.KickoutGroupVisibility = groupAdminUser != null ||
                                                                  GroupOwnerId == AntSdkService.AntSdkLoginOutput.userId
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    if (userinfo != null && userinfo.state != (int)GlobalVariable.OnLineStatus.OffLine)
                    {
                        var index =
                            GroupMemberControlList.Count(
                                m =>
                                    !m.IsGroupAdminImage && !m.IsOfflineState &&
                                    m.AdminImageVisibility != Visibility.Visible);
                        GroupMemberControlList.Insert(index + adminCount + 1, groupMemberViewModel);
                    }
                    else
                    {
                        GroupMemberControlList.Add(groupMemberViewModel);
                    }
                    //groupMemberViewModel.KickoutGroupVisibility = groupAdminUser != null ? Visibility.Visible : Visibility.Collapsed;
                    //GroupMemberControlList.Add(groupMemberViewModel);
                }
                MemberCountPrompt = string.Format("群成员({0}人)", GroupMemberControlList.Count);
            });
        }

        public void UpdateGroupMembers(List<AntSdkGroupMember> GroupMembers)
        {
            AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            {
                GroupMemberControlList.Clear();
                AntSdkGroupMember admin =
                    GroupMembers.FirstOrDefault(c => c.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner);
                _groupMembers = GroupMembers;
                groupAdminUser =
                    GroupMembers.FirstOrDefault(
                        c =>
                            c.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin &&
                            c.userId == AntSdkService.AntSdkLoginOutput.userId);
                GroupOwnerId = (admin == null ? "" : admin.userId);
                LogHelper.WriteFatal(this.GroupInfo.groupId + "-------" + this.GroupInfo.groupName + "群组的群主： " +
                                     admin?.userNum + admin?.userName);
                adminCount = GroupMembers.Count(m => m.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin);
                var orderByDesGroupMembers = GroupMembers.OrderByDescending(m => m.roleLevel).ToList();
                foreach (AntSdkGroupMember user in orderByDesGroupMembers)
                {
                    GroupMemberViewModel groupMemberViewModel = new GroupMemberViewModel(user, GroupOwnerId,
                        GroupInfo.groupId, this, SearchGroupName, adminCount);
                    //groupMemberViewModel.AdminCount = adminCount;
                    if (user.userId == GroupOwnerId)
                        GroupMemberControlList.Insert(0, groupMemberViewModel);
                    else if (user.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin)
                    {
                        GroupMemberControlList.Insert(GroupMemberControlList.Count > 0 ? 1 : 0, groupMemberViewModel);
                    }
                    else
                    {
                        var userinfo =
                            AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(c => c.userId == user.userId);
                        groupMemberViewModel.KickoutGroupVisibility = groupAdminUser != null ||
                                                                      GroupOwnerId ==
                                                                      AntSdkService.AntSdkLoginOutput.userId
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        if (userinfo != null && userinfo.state != (int)GlobalVariable.OnLineStatus.OffLine)
                        {
                            var index =
                                GroupMemberControlList.Count(
                                    m =>
                                        !m.IsGroupAdminImage && !m.IsOfflineState &&
                                        m.AdminImageVisibility != Visibility.Visible);
                            GroupMemberControlList.Insert(index + adminCount + 1, groupMemberViewModel);
                        }
                        else
                        {
                            GroupMemberControlList.Add(groupMemberViewModel);
                        }
                    }
                }
                MemberCountPrompt = string.Format("群成员({0}人)", GroupMembers.Count);
            });
        }

        public void Dispose()
        {
            _GroupMemberControlList = null;
            GroupMemberControlList = null;
            _searchGroupMemberControlList = null;
            SearchGroupMemberControlList = null;
            GC.Collect();
        }
        #endregion
    }
}
