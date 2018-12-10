/*
Author: tanqiyan
Crate date: 2017-06-16
Description：结构详情类
--------------------------------------------------------------------------------------------------------
Versions：
V1.00 2017-06-16 tanqiyan 描述：创建
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Threading;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views.Contacts;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class StructureDetailsViewModel : PropertyNotifyObject
    {
        private DetailType _type;
        private object _info;
        private List<AntSdkContact_User> _contactUsers;
        private GroupInfoViewModel _groupInfoViewModel;
        private ContactInfoViewModel _contactInfoViewModel;

        public static event Action<string, string> SendMsgHandler;
        public StructureDetailsViewModel()
        {
            _groupMembers = new ObservableCollection<GroupMemberViewModel>();
        }

        #region 属性

        private string _logoBitmapImage;

        /// <summary>
        /// 详情logo
        /// </summary>
        public string LogoBitmapImage
        {
            get { return _logoBitmapImage; }
            set
            {
                this._logoBitmapImage = value;
                RaisePropertyChanged(() => LogoBitmapImage);
            }
        }

        private string _infoName;

        /// <summary>
        /// 名称
        /// </summary>
        public string InfoName
        {
            get { return _infoName; }
            set
            {
                this._infoName = value;
                RaisePropertyChanged(() => InfoName);
            }
        }

        private string _introduce;

        /// <summary>
        ///介绍
        /// </summary>
        public string Introduce
        {
            get { return _introduce; }
            set
            {
                this._introduce = value;
                RaisePropertyChanged(() => Introduce);
            }
        }

        private string _managerName;

        /// <summary>
        /// 管理人名称
        /// </summary>
        public string ManagerName
        {
            set
            {
                _managerName = value;
                RaisePropertyChanged(() => ManagerName);
            }
            get { return _managerName; }
        }

        private ObservableCollection<AntSdkContact_User> _contactUserList;

        /// <summary>
        /// 成员集合
        /// </summary>
        public ObservableCollection<AntSdkContact_User> ContactUserList
        {
            get { return _contactUserList; }
            set
            {
                _contactUserList = value;
                RaisePropertyChanged(() => ContactUserList);
            }
        }

        private ObservableCollection<GroupMemberViewModel> _groupMembers;
        /// <summary>
        /// 群组成员集合
        /// </summary>
        public ObservableCollection<GroupMemberViewModel> GroupMembers
        {
            get { return _groupMembers; }
            set
            {
                _groupMembers = value;
                RaisePropertyChanged(() => GroupMembers);
            }
        }

        private string _attribution;

        /// <summary>
        /// 归属（公司、部门）名称
        /// </summary>
        public string Attribution
        {
            get { return _attribution; }

            set
            {
                _attribution = value;
                RaisePropertyChanged(() => Attribution);
            }
        }

        private string _position;

        /// <summary>
        /// 职位
        /// </summary>
        public string Position
        {
            get { return _position; }

            set
            {
                _position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        private string _telphoneNumber;

        /// <summary>
        /// 电话号码
        /// </summary>
        public string TelphoneNumber
        {
            get { return _telphoneNumber; }

            set
            {
                _telphoneNumber = value;
                RaisePropertyChanged(() => TelphoneNumber);
            }
        }

        private string _email;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get { return _email; }

            set
            {
                _email = value;
                RaisePropertyChanged(() => Email);
            }
        }

        private bool _remindMsg;

        /// <summary>
        /// 接收的消息是否提示
        /// </summary>
        public bool RemindMsg
        {
            get
            {
                return _remindMsg;
            }
            set
            {
                _remindMsg = value;
                RaisePropertyChanged(() => RemindMsg);
            }
        }

        private bool _noRemindMsg;

        /// <summary>
        /// 接收的消息是否提示
        /// </summary>
        public bool NoRemindMsg
        {
            get
            {
                return _noRemindMsg;
            }
            set
            {
                _noRemindMsg = value;
                RaisePropertyChanged(() => NoRemindMsg);
            }
        }

        private bool _isAdministrator;
        /// <summary>
        /// 是否群管理员
        /// </summary>
        public bool IsAdministrator
        {
            get { return _isAdministrator; }
            set
            {
                _isAdministrator = value;
                RaisePropertyChanged(() => IsAdministrator);
            }
        }

        private bool _isShowMembers = false;
        /// <summary>
        /// 是否显示成员列表
        /// </summary>
        public bool IsShowMembers
        {
            get { return _isShowMembers; }
            set
            {
                _isShowMembers = value;
                RaisePropertyChanged(() => IsShowMembers);
            }
        }

        private bool _isShowUserInfo = false;
        /// <summary>
        /// 是否显示成员信息
        /// </summary>
        public bool IsShowUserInfo
        {
            get { return _isShowUserInfo; }
            set
            {
                _isShowUserInfo = value;
                RaisePropertyChanged(() => IsShowUserInfo);
            }
        }
        private GroupMemberViewModel _currentGroupMember;
        /// <summary>
        /// 当前选中组成员
        /// </summary>
        public GroupMemberViewModel CurrentGroupMember
        {
            get
            {

                return _currentGroupMember;
            }
            set
            {
                _currentGroupMember = value;
                RaisePropertyChanged(() => CurrentGroupMember);
            }
        }

        private UserInfoViewModel _userInfoViewModel;
        /// <summary>
        /// 个人详细信息
        /// </summary>
        public UserInfoViewModel UserInfoViewModel
        {
            get
            {

                return _userInfoViewModel;
            }
            set
            {
                _userInfoViewModel = value;
                RaisePropertyChanged(() => UserInfoViewModel);
            }
        }

        private bool _isGroup = false;
        /// <summary>
        /// 是否是群组
        /// </summary>
        public bool IsGroup
        {
            get { return _isGroup; }
            set
            {
                _isGroup = value;
                RaisePropertyChanged(() => IsGroup);
            }
        }

        #endregion

        #region 命令

        private ICommand _sendMsgCommand;
        /// <summary>
        /// 消息发送命令
        /// </summary>
        public ICommand SendMsgCommand
        {
            get
            {
                _sendMsgCommand = new DefaultCommand(m =>
                {
                    switch (_type)
                    {
                        case DetailType.Company:
                        case DetailType.Department:
                            Window owner = Antenna.Framework.Win32.GetTopWindow();
                            MassMsgSentView win = new MassMsgSentView();
                            MassMsgSentViewModel vm = new MassMsgSentViewModel(_contactUsers);
                            win.DataContext = vm;
                            win.ShowInTaskbar = false;
                            win.Owner = owner;
                            win.ShowDialog();
                            break;
                        case DetailType.Group:
                            _groupInfoViewModel?.OnMouseDoubleClickEvent();
                            break;
                        case DetailType.Personal:
                            _contactInfoViewModel?.OnMouseDoubleClickEvent();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
                    }
                });
                return _sendMsgCommand;
            }
        }


        private ICommand _checkMembersCommand;
        /// <summary>
        /// 查看成员
        /// </summary>
        public ICommand CheckMembersCommand
        {
            get
            {
                _checkMembersCommand = new DefaultCommand(m =>
                {
                    LoadGroupMeberData();
                    if (_contactUsers != null && _contactUsers.Count > 0)
                    {
                        GroupMembers.Clear();
                        foreach (var user in _contactUsers)
                        {
                            var groupUser = new AntSdkGroupMember()
                            {
                                picture = user.picture,
                                userId = user.userId,
                                position = user.position,
                                userName = user.userName,
                                userNum = user.userNum
                            };
                            GroupMemberViewModel userModel = new GroupMemberViewModel(groupUser, "", "", null, "");
                            GroupMembers.Add(userModel);
                        }
                        IsShowMembers = true;
                    }
                });
                return _checkMembersCommand;
            }
        }

        private ICommand _exitGroupCommand;
        /// <summary>
        /// 退出讨论组
        /// </summary>
        public ICommand ExitGroupCommand
        {
            get
            {
                _exitGroupCommand = new DefaultCommand(m =>
                {
                    if (IsAdministrator)
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要解散{0}吗？", _groupInfoViewModel.GroupInfo.groupName),
                            MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) ==
                        GlobalVariable.ShowDialogResult.Ok)
                        {
                            GroupPublicFunction.DismissGroup(_groupInfoViewModel.GroupInfo.groupId);
                        }
                    }
                    else
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要退出{0}吗？", _groupInfoViewModel.GroupInfo.groupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) ==
                       GlobalVariable.ShowDialogResult.Ok)
                        {
                            _groupInfoViewModel?.ExitGroup();
                        }
                    }

                });
                return _exitGroupCommand;
            }
        }

        private ICommand _remindMsgCommand;
        /// <summary>
        /// 接收消息是否提示命令
        /// </summary>
        public ICommand RemindMsgCommand
        {
            get
            {
                _remindMsgCommand = new DefaultCommand(m =>
                  {
                      if (RemindMsg)
                          _groupInfoViewModel.SetMsgRemind(GlobalVariable.MsgRemind.Remind);
                      else if (NoRemindMsg)
                          _groupInfoViewModel.SetMsgRemind(GlobalVariable.MsgRemind.NoRemind);

                  });
                return _remindMsgCommand;
            }
        }

        private ICommand _closeCommand;
        /// <summary>
        /// 关闭成员列表
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                _closeCommand = new DefaultCommand(m =>
                  {
                      IsShowMembers = false;
                      IsShowUserInfo = false;
                  });
                return _closeCommand;
            }
        }

        private ICommand _showUserInfoCommand;
        /// <summary>
        /// 展示成员信息
        /// </summary>
        public ICommand ShowUserInfoCommand
        {
            get
            {
                _showUserInfoCommand = new DefaultCommand(obj =>
                  {
                      var groupUserInfo = obj as GroupMemberViewModel;
                      if (groupUserInfo != null)
                      {
                          
                          if (_userInfoViewModel == null)
                          {
                              UserInfoViewModel = new UserInfoViewModel(groupUserInfo.Member.userId, GlobalVariable.BurnFlag.IsBurn);
                          }
                          else
                              UserInfoViewModel.InitUserInfo(groupUserInfo.Member.userId, GlobalVariable.BurnFlag.IsBurn);
                          UserInfoViewModel.SendOrAtEvent += SendMsgEvent;
                          IsShowUserInfo = true;
                      }
                  });
                return _showUserInfoCommand;
            }
        }

        private static void SendMsgEvent(string methodType, string id)
        {
            SendMsgHandler?.Invoke(methodType, id);
        }

        #endregion

        #region 方法
        /// <summary>
        /// 加载详情数据（公司、部门、个人）
        /// </summary>
        public void InitDetails(DetailType type, object obj, List<AntSdkContact_User> contactUsers = null)
        {
            _type = type;
            _info = obj;
            _groupInfoViewModel = null;
            if (contactUsers != null)
                _contactUsers = contactUsers;
            switch (type)
            {
                case DetailType.Company:
                    CompanyDetail(_info);
                    break;
                case DetailType.Department:
                    DepartmentDetail(_info);
                    break;
                case DetailType.Personal:
                    PersonalDetail(_info);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        /// <summary>
        /// 加载详情数据（群组）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupInfo"></param>
        /// <param name="groupMembers"></param>
        public void InitDetails(DetailType type, GroupInfoViewModel groupInfo, List<AntSdkGroupMember> groupMembers, bool isFirst = true)
        {
            _contactUsers = null;
            if (groupInfo != null)
                _groupInfoViewModel = groupInfo;
            _type = type;
            IsGroup = true;
            GroupDetail();
            if (!isFirst)
            {
                if (!IsShowMembers) return;
                LoadGroupMeberData();
            }
            else
            {
                IsShowMembers = false;
            }
        }
        /// <summary>
        /// 设置消息接收方式
        /// </summary>
        /// <param name="state"></param>
        public void SetMsgRemindState(int state)
        {
            if (state ==(int)GlobalVariable.MsgRemind.NoRemind)
                NoRemindMsg = true;
            else
                RemindMsg = true;
        }

        /// <summary>
        /// 加载群成员数据
        /// </summary>
        private void LoadGroupMeberData()
        {
            if (_groupInfoViewModel != null && _groupInfoViewModel.Members.Count > 0)
            {
                var groupMembers = _groupInfoViewModel.Members;
                GroupMembers.Clear();
                var admin = groupMembers.FirstOrDefault(c => c.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner);
                var groupAdminId = (admin == null ? "" : admin.userId);
                foreach (var groupMember in groupMembers.Select(groupUser => new GroupMemberViewModel(groupUser, groupAdminId, _groupInfoViewModel.GroupInfo.groupId, null, "")))
                {
                    if (admin?.userId == groupMember.Member.userId)
                        GroupMembers.Insert(0, groupMember);
                    else
                        GroupMembers.Add(groupMember);
                }
                IsShowMembers = true;
            }
        }

        /// <summary>
        /// 头像图片处理
        /// </summary>
        /// <param name="url">图片路径</param>
        private void LoadLogoBitmapImage(string imageUrl)
        {
            //var tempLogoBitmapImage = new BitmapImage();
            //tempLogoBitmapImage.BeginInit();
            //tempLogoBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //tempLogoBitmapImage.UriSource = new Uri(imageUrl);
            //tempLogoBitmapImage.EndInit();
            //LogoBitmapImage = null;
            //LogoBitmapImage = tempLogoBitmapImage;
            LogoBitmapImage = imageUrl;
        }

        /// <summary>
        /// 公司详情
        /// </summary>
        private void CompanyDetail(object company)
        {
            if (company is AntSdkContact_Depart)
            {
                var companyInfo = company as AntSdkContact_Depart;
                InfoName = companyInfo.departName;
                LogoBitmapImage = @"../Images/CompanyLogo.png";
                Introduce = string.Empty;
                ManagerName = string.Empty;
                Attribution = string.Empty;
            }
        }

        /// <summary>
        /// 部门详情
        /// </summary>
        private void DepartmentDetail(object department)
        {
            if (department is AntSdkContact_Depart)
            {
                var departmentInfo = department as AntSdkContact_Depart;
                InfoName = departmentInfo.departName;
                LogoBitmapImage = @"../Images/DepartmentLogo.png";
                Introduce = string.Empty;
                ManagerName = string.Empty;
                Attribution = string.Empty;
            }
        }
        private AntSdkUserInfo userInfo;
        /// <summary>
        /// 个人详情
        /// </summary>
        private void PersonalDetail(object personal)
        {
            if (personal is ContactInfoViewModel)
            {
                _contactInfoViewModel = personal as ContactInfoViewModel;
                if (_contactInfoViewModel.User.userId == AntSdkService.AntSdkLoginOutput.userId)
                {
                    if (!string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.userNum))
                    {
                        InfoName = AntSdkService.AntSdkCurrentUserInfo.userNum + AntSdkService.AntSdkCurrentUserInfo.userName;
                    }
                    else
                    {
                        InfoName = AntSdkService.AntSdkCurrentUserInfo.userName;
                    }
                    LogoBitmapImage = !string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.picture)
                        ? AntSdkService.AntSdkCurrentUserInfo.picture
                        : "pack://application:,,,/AntennaChat;Component/Images/198-头像.png";
                    Introduce = !string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.signature)
                        ? AntSdkService.AntSdkCurrentUserInfo.signature
                        : "这个人很懒，什么都没留下";
                    Attribution = AntSdkService.AntSdkCurrentUserInfo.departName;
                    Position = AntSdkService.AntSdkCurrentUserInfo.position;
                    TelphoneNumber = AntSdkService.AntSdkCurrentUserInfo.phone;
                    Email = AntSdkService.AntSdkCurrentUserInfo.email;
                }
                else
                {
                    var load = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        QueryUserInfo(_contactInfoViewModel.User.userId);
                    }));
                    load.Completed += Load_Completed;
                }
            }
        }
        /// <summary>
        /// 查询完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Completed(object sender, EventArgs e)
        {
            if (userInfo == null) return;
            if (!string.IsNullOrEmpty(userInfo.userNum))
            {
                InfoName = userInfo.userNum + userInfo.userName;
            }
            else
            {
                InfoName = userInfo.userName;
            }
            LogoBitmapImage = !string.IsNullOrEmpty(userInfo.picture) ? userInfo.picture :
                "pack://application:,,,/AntennaChat;Component/Images/198-头像.png";
            Introduce = !string.IsNullOrEmpty(userInfo.signature) ? userInfo.signature :
                "这个人很懒，什么都没留下";
            Attribution = userInfo.departName;
            Position = userInfo.position;
            TelphoneNumber = userInfo.phone;
            Email = userInfo.email;
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="id"></param>
        private void QueryUserInfo(string id)
        {
            if (userInfo != null && id == userInfo.userId) return;
            var errMsg = string.Empty;
            //var userOutput = new UserOutput();
            //var input = new UserInput
            //{
            //    token = AntSdkService.AntSdkLoginOutput.token,
            //    version = GlobalVariable.Version,
            //    userId = AntSdkService.AntSdkLoginOutput.userId,
            //    targetUserId = id
            //};
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
           //var user= AntSdkService.AntSdkGetUserInfo(id, ref errMsg);
           // if (user != null)
           // {
           //     userInfo = user;
           // }
            //if (!(new HttpService()).GetUserInfo(input, ref userOutput, ref errMsg))
            //{
            //    userInfo = null;
            //    return;
            //}
            //if (userOutput?.user == null)
            //{
            //    userInfo = null;
            //    return;
            //}
            userInfo = GroupPublicFunction.QueryUserInfo(id);
        }

        /// <summary>
        /// 群组详情
        /// </summary>
        private void GroupDetail()
        {
            if (_groupInfoViewModel.GroupClassify == 1)
            {
                IsAdministrator = true;
                ManagerName = AntSdkService.AntSdkCurrentUserInfo.userName;
            }
            else
            {
                //if(_groupInfoViewModel.Members)
                AntSdkGroupMember adminUser = _groupInfoViewModel.Members.FirstOrDefault(c => c.roleLevel == (int)GlobalVariable.GroupRoleLevel.GroupOwner);
                IsAdministrator = false;
                if (adminUser != null) ManagerName = adminUser.userName;
            }
            InfoName = _groupInfoViewModel.GroupName + _groupInfoViewModel.GroupMemberCount;
            RemindMsg = _groupInfoViewModel.MessageNoticeIsChecked;
            NoRemindMsg = _groupInfoViewModel.MessageHideIsChecked;
            LoadLogoBitmapImage(_groupInfoViewModel.GroupPicture);
        }

        #endregion
    }

    public enum DetailType
    {
        /// <summary>
        /// 其它
        /// </summary>
        Other,
        /// <summary>
        /// 公司
        /// </summary>
        Company,

        /// <summary>
        /// 部门
        /// </summary>
        Department,

        /// <summary>
        /// 个人
        /// </summary>
        Personal,

        /// <summary>
        /// 群组
        /// </summary>
        Group
    }
}
