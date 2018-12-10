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
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class GroupInfoViewModel : PropertyNotifyObject
    {
        public GlobalVariable.BurnFlag IsBurnMode = GlobalVariable.BurnFlag.NotIsBurn;
        public AntSdkGroupInfo GroupInfo;
        public List<AntSdkGroupMember> Members;
        public StructureDetailsViewModel StructureDetailsVM;
        #region 构造器
        public GroupInfoViewModel(AntSdkGroupInfo groupInfo)
        {
            this.GroupInfo = groupInfo;
            if (string.IsNullOrWhiteSpace(groupInfo.groupPicture))
            {
                this.GroupPicture = "pack://application:,,,/AntennaChat;Component/Images/44-头像.png";
            }
            else
            {
                this.GroupPicture = groupInfo.groupPicture;
            }
            //SetUnreadCount(unreadCount);
            this.GroupName = string.Format("{0}", groupInfo.groupName);
            //this.GroupName = string.Format("{0}({1}人)", groupInfo.groupName, groupInfo.members);
            if (groupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
            {
                MessageNoticeIsChecked = false;
                MessageHideIsChecked = true;
                ImageNoRemindVisibility = Visibility.Visible;
            }
            else
            {
                MessageNoticeIsChecked = true;
                MessageHideIsChecked = false;
                ImageNoRemindVisibility = Visibility.Collapsed;
            }
            this.GroupClassify = groupInfo.groupOwnerId == AntSdkService.AntSdkCurrentUserInfo.userId || (groupInfo.managerIds != null && groupInfo.managerIds.Contains(AntSdkService.AntSdkCurrentUserInfo.userId)) ? 1 : 2;
            this.DeleteGroupVisibility = groupInfo.groupOwnerId == AntSdkService.AntSdkCurrentUserInfo.userId
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.GroupMemberCount = string.Format("({0}人)", groupInfo.memberCount);
            //ThreadPool.QueueUserWorkItem(o => Members = GroupPublicFunction.GetMembers(this.GroupInfo.groupId));
            //GetMembers();
            GroupMemberViewModel.KickoutGroupEvent += KickoutGroup;
        }
        #endregion

        #region 属性
        private string _GroupPicture;
        /// <summary>
        /// 讨论组头像
        /// </summary>
        public string GroupPicture
        {
            get { return this._GroupPicture; }
            set
            {
                this._GroupPicture = value;
                RaisePropertyChanged(() => GroupPicture);
            }
        }
        private string _GroupName;
        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string GroupName
        {
            get { return this._GroupName; }
            set
            {
                this._GroupName = value;
                RaisePropertyChanged(() => GroupName);
            }
        }

        private string _groupMemberCount;
        /// <summary>
        /// 讨论组中人数
        /// </summary>
        public string GroupMemberCount
        {
            get { return _groupMemberCount; }
            set
            {
                this._groupMemberCount = value;
                RaisePropertyChanged(() => GroupMemberCount);
            }

        }
        private string _groupClassifyName;
        /// <summary>
        /// 组分类名称
        /// </summary>
        public string GroupClassifyName
        {
            get { return _groupClassifyName; }
            set
            {
                _groupClassifyName = value;
                RaisePropertyChanged(() => GroupClassifyName);
            }

        }

        private int _groupClassify;
        /// <summary>
        /// 组分类(目前暂时1为我管理的，2为我加入的)
        /// </summary>
        public int GroupClassify
        {
            get { return _groupClassify; }
            set
            {
                _groupClassify = value;
                RaisePropertyChanged(() => GroupClassify);
            }

        }

        #region 未读数相关属性（已经不起作用，先保留以便后续扩展）
        private int _UnreadCount;
        /// <summary>
        /// 未读消息数（设置为有消息不提醒的前提）
        /// </summary>
        public int UnreadCount
        {
            get { return this._UnreadCount; }
            set
            {
                this._UnreadCount = value;
                RaisePropertyChanged(() => UnreadCount);
            }
        }

        private int _UnreadCountWidth = 16;
        /// <summary>
        /// 未读消息数显示宽度
        /// </summary>
        public int UnreadCountWidth
        {
            get { return this._UnreadCountWidth; }
            set
            {
                this._UnreadCountWidth = value;
                RaisePropertyChanged(() => UnreadCountWidth);
            }
        }

        private int _UnreadCountRectangleWidth = 0;
        /// <summary>
        /// 未读消息数显示宽度
        /// </summary>
        public int UnreadCountRectangleWidth
        {
            get { return this._UnreadCountRectangleWidth; }
            set
            {
                this._UnreadCountRectangleWidth = value;
                RaisePropertyChanged(() => UnreadCountRectangleWidth);
            }
        }

        private Visibility _UnreadCountVisibility = Visibility.Collapsed;
        /// <summary>
        /// 未读消息是否可见
        /// </summary>
        public Visibility UnreadCountVisibility
        {
            get { return this._UnreadCountVisibility; }
            set
            {
                this._UnreadCountVisibility = value;
                RaisePropertyChanged(() => UnreadCountVisibility);
            }
        }
        #endregion

        private Visibility _ImageNoRemindVisibility = Visibility.Collapsed;
        /// <summary>
        ///消息免打扰图标
        /// </summary>
        public Visibility ImageNoRemindVisibility
        {
            get { return this._ImageNoRemindVisibility; }
            set
            {
                this._ImageNoRemindVisibility = value;
                RaisePropertyChanged(() => ImageNoRemindVisibility);
            }
        }

        private Visibility _DeleteGroupVisibility = Visibility.Collapsed;
        /// <summary>
        /// 解散讨论组的可见性
        /// </summary>
        public Visibility DeleteGroupVisibility
        {
            get { return this._DeleteGroupVisibility; }
            set
            {
                this._DeleteGroupVisibility = value;
                RaisePropertyChanged(() => DeleteGroupVisibility);
            }
        }

        private System.Windows.Media.Brush _Background;
        /// <summary>
        /// 背景色
        /// </summary>
        public System.Windows.Media.Brush Background
        {
            get { return this._Background; }
            set
            {
                this._Background = value;
                RaisePropertyChanged(() => Background);
            }
        }

        private bool _MessageNoticeIsChecked = true;
        /// <summary>
        /// 接收消息并提醒是否选中
        /// </summary>
        public bool MessageNoticeIsChecked
        {
            get { return this._MessageNoticeIsChecked; }
            set
            {
                this._MessageNoticeIsChecked = value;
                RaisePropertyChanged(() => MessageNoticeIsChecked);
            }
        }

        private bool _MessageHideIsChecked = false;
        /// <summary>
        /// 消息不提醒是否选中
        /// </summary>
        public bool MessageHideIsChecked
        {
            get { return this._MessageHideIsChecked; }
            set
            {
                this._MessageHideIsChecked = value;
                RaisePropertyChanged(() => MessageHideIsChecked);
            }
        }
        #endregion

        #region 命令/其他方法
        /// <summary>
        /// 鼠标进入颜色变化
        /// </summary>
        private ICommand _MouseEnter;
        public ICommand MouseEnter
        {
            get
            {
                if (this._MouseEnter == null)
                {
                    //this._MouseEnter = new DefaultCommand(o =>
                    //{
                    //    Background = (System.Windows.Media.Brush)(new BrushConverter()).ConvertFromString("#e4f2fb");
                    //});
                }
                return this._MouseEnter;
            }
        }
        /// <summary>
        /// 鼠标进入颜色变化
        /// </summary>
        private ICommand _MouseLeave;
        public ICommand MouseLeave
        {
            get
            {
                if (this._MouseLeave == null)
                {
                    //this._MouseLeave = new DefaultCommand(o =>
                    //{
                    //    Background = (System.Windows.Media.Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                    //});
                }
                return this._MouseLeave;
            }
        }

        /// <summary>
        /// 退出讨论组
        /// </summary>
        private ICommand _DropOutGroup;
        public ICommand DropOutGroup
        {
            get
            {
                if (this._DropOutGroup == null)
                {
                    this._DropOutGroup = new DefaultCommand(o =>
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要退出{0}吗？", GroupInfo.groupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Ok)
                        {
                            ExitGroup();
                        }
                    });
                }
                return this._DropOutGroup;
            }
        }

        /// <summary>
        /// 解散讨论组
        /// </summary>
        private ICommand _DeleteGroup;
        public ICommand DeleteGroup
        {
            get
            {
                if (this._DeleteGroup == null)
                {
                    this._DeleteGroup = new DefaultCommand(o =>
                    {
                        if (MessageBoxWindow.Show("提醒", string.Format("确定要解散{0}吗？", GroupInfo.groupName), MessageBoxButton.OKCancel, GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Ok)
                        {
                            GroupPublicFunction.DismissGroup(GroupInfo.groupId);
                        }
                    });
                }
                return this._DeleteGroup;
            }
        }

        /// <summary>
        /// 消息提醒
        /// </summary>
        private ICommand _MessageNoticeCommand;
        public ICommand MessageNoticeCommand
        {
            get
            {
                if (this._MessageNoticeCommand == null)
                {
                    this._MessageNoticeCommand = new DefaultCommand(o =>
                    {
                        MessageNoticeIsChecked = true;
                        if (MessageHideIsChecked)
                        {
                            MessageHideIsChecked = false;
                            ImageNoRemindVisibility = Visibility.Collapsed;
                            SetMsgRemind();
                        }
                    });
                }
                return this._MessageNoticeCommand;
            }
        }

        /// <summary>
        /// 消息不提醒
        /// </summary>
        private ICommand _MessageHideCommand;
        public ICommand MessageHideCommand
        {
            get
            {
                if (this._MessageHideCommand == null)
                {
                    this._MessageHideCommand = new DefaultCommand(o =>
                    {
                        MessageHideIsChecked = true;
                        if (MessageNoticeIsChecked)
                        {
                            MessageNoticeIsChecked = false;
                            ImageNoRemindVisibility = Visibility.Visible;
                            SetMsgRemind();
                        }
                    });
                }
                return this._MessageHideCommand;
            }
        }

        private ICommand _mouseLeftButtonDownCommand;
        /// <summary>
        /// 鼠标单击命令
        /// </summary>
        public ICommand MouseLeftButtonDownCommand
        {
            get
            {
                _mouseLeftButtonDownCommand = new DefaultCommand(m =>
                  {
                      //StructureDetailsVM = new StructureDetailsViewModel();
                      //var view = new StructureDetailsView
                      //{
                      //    DataContext = StructureDetailsVM,
                      //    DetailType = DetailType.Group
                      //};
                      //MainWindowViewModel.GoStructureDetail(view);
                      //StructureDetailsVM.InitDetails(DetailType.Group, this, this.Members);
                  });
                return _mouseLeftButtonDownCommand;
            }

        }


        //public delegate void MouseClickDelegate(object groupInfoView);
        public static event EventHandler DropOutGroupEvent;
        private void OnDropOutGroupEvent(object groupInfoView)
        {
            //object dataContext = (groupInfoView as GroupInfoView).DataContext;
            if (DropOutGroupEvent != null)
            {
                DropOutGroupEvent(groupInfoView, null);
            }
        }
        /// <summary>
        /// 退出讨论组
        /// </summary>
        public void ExitGroup()
        {
            //退出讨论组之前需要先更新讨论组头像
            //UpdateGroupInput updateInput = new UpdateGroupInput();
            //updateInput.groupId = this.GroupInfo.groupId;
            //updateInput.token = AntSdkService.AntSdkLoginOutput.token;
            //updateInput.userId = AntSdkService.AntSdkLoginOutput.userId;
            //updateInput.version = GlobalVariable.Version;
            //updateInput.groupPicture = ImageHandle.GetGroupPicture(Members.Where(c => c.userId != updateInput.userId).Select(c => c.picture).ToList ());
            //BaseOutput updateOutput = new BaseOutput();
            //string errMsg = string.Empty;
            //(new HttpService()).UpdateGroup(updateInput, ref updateOutput, ref errMsg);

            //ExitGroupInput input = new ExitGroupInput();
            //input.groupId = this.GroupInfo.groupId;
            //input.token = AntSdkService.AntSdkLoginOutput.token;
            //input.userId = AntSdkService.AntSdkLoginOutput.userId;
            //input.version = GlobalVariable.Version;
            //BaseOutput output = new BaseOutput();
            //string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            if (GroupInfo == null) return;
            var isResult = GroupPublicFunction.ExitGroup(this.GroupInfo.groupId, this.GroupInfo.groupName, Members);
            if (isResult)
            {
                OnDropOutGroupEvent(this);
            }
            //var isResult = AntSdkService.GroupExitor(AntSdkService.AntSdkLoginOutput.userId, this.GroupInfo.groupId, ref errMsg);
            //if (isResult)
            //{
            //    string[] ThreadParams = new string[2];
            //    ThreadParams[0] = this.GroupInfo.groupId;
            //    ThreadParams[1] = ImageHandle.GetGroupPicture(Members.Where(c => c.userId != AntSdkService.AntSdkLoginOutput.userId).Select(c => c.picture).ToList());
            //    Thread UpdateGroupPictureThread = new Thread(GroupPublicFunction.UpdateGroupPicture);
            //    UpdateGroupPictureThread.Start(ThreadParams);

            //    OnDropOutGroupEvent(this);
            //}
            //else
            //{
            //    MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            //}
            //if ((new HttpService()).ExitGroup(input, ref output, ref errMsg))
            //{
            //    string[] ThreadParams = new string[2];
            //    ThreadParams[0] = this.GroupInfo.groupId;
            //    ThreadParams[1] = ImageHandle.GetGroupPicture(Members.Where(c => c.userId != AntSdkService.AntSdkLoginOutput.userId).Select(c => c.picture).ToList());
            //    Thread UpdateGroupPictureThread = new Thread(UpdateGroupPicture);
            //    UpdateGroupPictureThread.Start(ThreadParams);

            //    OnDropOutGroupEvent(this);
            //}
            //else
            //{
            //    if (output.errorCode != "1004")
            //    {
            //        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            //    }
            //}
        }



        public static event EventHandler MouseDoubleClickEvent;
        public void OnMouseDoubleClickEvent()
        {

            if (MouseDoubleClickEvent != null)
            {
                MouseDoubleClickEvent(this, null);
            }
        }
        private ICommand _MouseDoubleClick;
        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        public ICommand MouseDoubleClick
        {
            get
            {
                if (this._MouseDoubleClick == null)
                {
                    this._MouseDoubleClick = new DefaultCommand(o =>
                    {
                        OnMouseDoubleClickEvent();
                    });
                }
                return this._MouseDoubleClick;
            }
        }

        //public void SetUnreadCount(int count)
        //{
        //    this.UnreadCount = count;
        //    UnreadCountVisibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        //    //UnreadCountWidth = count > 99 ? 22 : 17;
        //    if (count > 9)
        //    {
        //        UnreadCountWidth = 22;
        //        UnreadCountRectangleWidth = 6;
        //    }
        //    else
        //    {
        //        UnreadCountWidth = 16;
        //        UnreadCountRectangleWidth = 0;
        //    }
        //}



        public void UpdateMembers(List<AntSdkGroupMember> members)
        {
            //Members = members;
            this.GroupName = string.Format("{0}", GroupInfo.groupName);
            GroupMemberCount = string.Format("({0}人)", Members == null ? 0 : Members.Count());
        }

        public void UpdateGroupInfo(AntSdkReceivedGroupMsg.Modify sysMsg)
        {
            if (!string.IsNullOrEmpty(sysMsg.content?.groupPicture))
            {
                this.GroupPicture = sysMsg.content.groupPicture;
                this.GroupInfo.groupPicture = sysMsg.content.groupPicture;
            }
            if (!string.IsNullOrEmpty(sysMsg.content.groupName))
            {
                this.GroupName = string.Format("{0}", sysMsg.content.groupName);
                GroupMemberCount = string.Format("({0}人)", Members == null ? 0 : Members.Count());
                this.GroupInfo.groupName = sysMsg.content.groupName;
            }
        }

        private void KickoutGroup(string groupId, string userId, string picture)
        {
            if (this.GroupInfo.groupId != groupId) return;
            AntSdkGroupMember user = Members.FirstOrDefault(c => c.userId == userId);
            if (user != null)
                Members.Remove(user);
            this.GroupName = string.Format("{0}", GroupInfo.groupName);
            GroupMemberCount = string.Format("({0}人)", Members == null ? 0 : Members.Count());
            this.GroupPicture = picture;
        }

        public void AddNewMember(string picture, List<AntSdkContact_User> newGroupMemberList)
        {
            foreach (AntSdkContact_User user in newGroupMemberList)
            {
                AntSdkGroupMember member = new AntSdkGroupMember();
                member.picture = user.picture;
                member.position = user.position;
                member.roleLevel = (int)GlobalVariable.GroupRoleLevel.Ordinary;
                member.userId = user.userId;
                member.userName = user.userName;
                if (!Members.Exists(m => m.userId == user.userId))
                    Members.Add(member);
            }
            this.GroupName = string.Format("{0}", GroupInfo.groupName);
            GroupMemberCount = string.Format("({0}人)", Members == null ? 0 : Members.Count());
            this.GroupPicture = picture;
        }

        public delegate void SetMsgRemindDelegate(GroupInfoViewModel sender, GlobalVariable.MsgRemind msgRemind, int unreadCount);
        public static event SetMsgRemindDelegate SetMsgRemindEvent;
        public void OnMsgRemindEvent()
        {
            if (SetMsgRemindEvent != null)
            {
                GlobalVariable.MsgRemind msgRemind = MessageNoticeIsChecked ? GlobalVariable.MsgRemind.Remind : GlobalVariable.MsgRemind.NoRemind;
                SetMsgRemindEvent(this, msgRemind, UnreadCount);
            }
        }
        /// <summary>
        /// 设置消息提醒方式
        /// </summary>
        public void SetMsgRemind(GlobalVariable.MsgRemind? remind = null)
        {
            AntSdkUpdateGroupConfigInput input = new AntSdkUpdateGroupConfigInput();
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.groupId = this.GroupInfo.groupId;
            BaseOutput output = new BaseOutput();
            var errCode = 0;
            string errMsg = string.Empty;

            if (remind == null)
                input.state = MessageNoticeIsChecked
                    ? ((int)GlobalVariable.MsgRemind.Remind).ToString()
                    : ((int)GlobalVariable.MsgRemind.NoRemind).ToString();
            else
                input.state = ((int)remind.Value).ToString();
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var isResult = AntSdkService.UpdateGroupConfig(input, ref errCode, ref errMsg);
            if (isResult)
            {
                GroupInfo.state = int.Parse(input.state);
                if (GroupInfo.state == (int)GlobalVariable.MsgRemind.NoRemind)
                {
                    MessageNoticeIsChecked = false;
                    MessageHideIsChecked = true;
                    ImageNoRemindVisibility = Visibility.Visible;
                }
                else
                {
                    MessageNoticeIsChecked = true;
                    MessageHideIsChecked = false;
                    ImageNoRemindVisibility = Visibility.Collapsed;
                }

                StructureDetailsVM?.SetMsgRemindState(GroupInfo.state);
                OnMsgRemindEvent();
            }
            else
            {
                MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            }
            //if ((new HttpService()).UpdateGroupConfig(input, ref output, ref errMsg))
            //{
            //    GroupInfo.state = input.state;
            //    //todo
            //    OnMsgRemindEvent();
            //}
            //else
            //{
            //    if (output.errorCode != "1004")
            //    {
            //        MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            //    }
            //}
        }
        #endregion
    }
}
