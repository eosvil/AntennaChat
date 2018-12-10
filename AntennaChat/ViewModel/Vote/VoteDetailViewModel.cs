using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Antenna.Framework;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Vote
{
    public class VoteDetailViewModel : PropertyNotifyObject
    {
        #region 私有变量

        private int _voteId;
        private bool _isSingle;
        /// <summary>
        /// 创建完后继续下一步
        /// </summary>
        public event Action<int, VoteViewType> GoVoteEvent;
        #endregion
        public VoteDetailViewModel(VoteViewType type, int voteId, int groupMemberCount)
        {
            _voteId = voteId;
            var errCode = 0;
            var errMsg = string.Empty;
            var output = AntSdkService.GetVoteInfo(voteId, AntSdkService.AntSdkCurrentUserInfo.userId, ref errCode, ref errMsg);
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            if (output != null)
                SetVoteData(output, type, groupMemberCount, serverDateTime);
        }

        public VoteDetailViewModel(AntSdkGetVoteInfoOutput output, VoteViewType type, int groupMemberCount, DateTime serverDateTime)
        {
            _voteId = output.id;
            SetVoteData(output, type, groupMemberCount, serverDateTime);
        }

        private void SetVoteData(AntSdkGetVoteInfoOutput output, VoteViewType type, int groupMemberCount, DateTime serverDateTime)
        {

            AntSdkContact_User contactUser = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == output.createdBy);
            if (contactUser != null)
            {
                if (!string.IsNullOrWhiteSpace(contactUser.picture) && publicMethod.IsUrlRegex(contactUser.picture))
                {
                    var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                        m => m.UserID == contactUser.userId);
                    this.InitiatorHeadPic = string.IsNullOrEmpty(userImage?.Url)
                        ? contactUser.picture
                        : userImage.Url;
                }
                else
                {
                    this.InitiatorHeadPic = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                }
                this.InitiatorName = contactUser.userName;

            }
            InitiateDateTime = DataConverter.FormatTimeByCreateTime(output.createdDate);
            VoteTitle = output.title;
            VoteState = DateTime.Compare(Convert.ToDateTime(output.expiryTime), serverDateTime) < 0;
            VoteType = output.maxChoiceNumber > 1 ? "多选" : "单选" + (output.secret ? "（匿名）" : "");
            _isSingle = output.maxChoiceNumber <= 1;
            IsDisplayDeleteVote = output.createdBy == AntSdkService.AntSdkCurrentUserInfo.userId;
            VoteEndDateTime = Convert.ToDateTime(output.expiryTime).ToString("yyyy-MM-dd HH:mm");

            //TotalVotes = groupMemberCount;
            IsDisplayNoVotes = !output.secret;

            if (VoteState)
            {
                IsDisplayAtBtn = !VoteState;
                IsDisplayNoVotes = !VoteState;
            }
            else
                IsDisplayAtBtn = !output.secret;
            if (groupMemberCount > 0)
            {
                var memberCount = groupMemberCount - output.voters;
                NoVotes = memberCount > 0 ? memberCount : 0;
            }

            if (NoVotes == 0)
                IsDisplayAtBtn = false;
            if (type == VoteViewType.VoteDetail)
            {
                if (output.options != null && output.options.Count > 0)
                {
                    foreach (var opition in output.options)
                    {
                        VoteOptionList.Add(new OptionContent
                        {
                            OptionId = opition.id,
                            OptionName = opition.name
                        });
                    }
                }
            }
            else
            {
                if (output.options != null && output.options.Count > 0)
                {
                    foreach (var opition in output.options)
                    {
                        VoteOptionList.Add(new OptionContent
                        {
                            OptionId = opition.id,
                            OptionName = opition.name,
                            OptionVotes = opition.total,
                            IsCurrentUserSelected = opition.voted
                        });
                    }
                    TotalVotes = VoteOptionList.Sum(m => m.OptionVotes);
                }
                else
                {
                    TotalVotes = output.voters;
                }
            }
        }

        /// <summary>
        /// 创建公告之后触发
        /// </summary>
        public event Action CloseVoteViewEvent;
        /// <summary>
        /// 发送@消息时间
        /// </summary>
        public event Action<List<object>> SendAtMsgEvent;
        #region 属性
        //private int _totalVotes;
        ///// <summary>
        /////群成员数据量
        ///// </summary>
        //public int TotalVotes
        //{
        //    get { return _totalVotes; }
        //    set
        //    {
        //        _totalVotes = value;
        //        RaisePropertyChanged(() => TotalVotes);
        //    }
        //}
        private string _initiatorHeadPic;
        /// <summary>
        /// 投票发起者头像
        /// </summary>
        public string InitiatorHeadPic
        {
            get { return _initiatorHeadPic; }
            set
            {
                _initiatorHeadPic = value;
                RaisePropertyChanged(() => InitiatorHeadPic);
            }
        }

        private string _initiatorName;
        /// <summary>
        /// 投票发起者名称
        /// </summary>
        public string InitiatorName
        {
            get { return _initiatorName; }
            set
            {
                _initiatorName = value;
                RaisePropertyChanged(() => InitiatorName);
            }
        }

        private string _initiateDateTime;
        /// <summary>
        ///投票发起时间 
        /// </summary>
        public string InitiateDateTime
        {
            get { return _initiateDateTime; }
            set
            {
                _initiateDateTime = value;
                RaisePropertyChanged(() => InitiateDateTime);
            }
        }

        private string _voteTitle;
        /// <summary>
        /// 投票活动标题
        /// </summary>
        public string VoteTitle
        {
            get { return _voteTitle; }
            set
            {
                _voteTitle = value;
                RaisePropertyChanged(() => VoteTitle);
            }
        }

        private bool _voteState;
        /// <summary>
        /// 投票活动状态(true：已结束 false:进行中)
        /// </summary>
        public bool VoteState
        {
            get { return _voteState; }
            set
            {
                _voteState = value;
                RaisePropertyChanged(() => VoteState);
            }
        }
        private string _voteType;
        /// <summary>
        /// 投票类型（单选、多选）
        /// </summary>
        public string VoteType
        {
            get { return _voteType; }
            set
            {
                _voteType = value;
                RaisePropertyChanged(() => VoteType);
            }
        }
        private ObservableCollection<OptionContent> _voteOptionList = new ObservableCollection<OptionContent>();
        /// <summary>
        /// 投票选项集合
        /// </summary>
        public ObservableCollection<OptionContent> VoteOptionList
        {
            get { return _voteOptionList; }
            set
            {
                _voteOptionList = value;
                RaisePropertyChanged(() => VoteOptionList);
            }
        }
        private string _voteEndDateTime;
        /// <summary>
        /// 投票活动截止时间
        /// </summary>
        public string VoteEndDateTime
        {
            get { return _voteEndDateTime; }
            set
            {
                _voteEndDateTime = value;
                RaisePropertyChanged(() => VoteEndDateTime);
            }
        }
        private bool _isDisplayDeleteVote;
        /// <summary>
        /// 是否可以删除投票
        /// </summary>
        public bool IsDisplayDeleteVote
        {
            get { return _isDisplayDeleteVote; }
            set
            {
                _isDisplayDeleteVote = value;
                RaisePropertyChanged(() => IsDisplayDeleteVote);
            }
        }

        private bool _isDisplayAtBtn;
        /// <summary>
        /// 是否显示AT按钮投票
        /// </summary>
        public bool IsDisplayAtBtn
        {
            get { return _isDisplayAtBtn; }
            set
            {
                _isDisplayAtBtn = value;
                RaisePropertyChanged(() => IsDisplayAtBtn);
            }
        }

        private int _totalVotes;
        /// <summary>
        /// 投票总数
        /// </summary>
        public int TotalVotes
        {
            get { return _totalVotes; }
            set
            {
                _totalVotes = value;
                RaisePropertyChanged(() => TotalVotes);
            }
        }

        private bool _isDisplayNoVotes;
        /// <summary>
        /// 是否显示未投票数量（匿名不显示）
        /// </summary>
        public bool IsDisplayNoVotes
        {
            get { return _isDisplayNoVotes; }
            set
            {
                _isDisplayNoVotes = value;
                RaisePropertyChanged(() => IsDisplayNoVotes);
            }
        }

        private int _noVotes;
        /// <summary>
        /// 未投票人数量
        /// </summary>
        public int NoVotes
        {
            get { return _noVotes; }
            set
            {
                _noVotes = value;
                RaisePropertyChanged(() => NoVotes);
            }
        }

        private string _promptInfo;
        /// <summary>
        /// 提示语
        /// </summary>
        public string PromptInfo
        {
            get { return _promptInfo; }
            set
            {
                _promptInfo = value;
                RaisePropertyChanged(() => PromptInfo);
            }
        }

        private bool _isShowPrompt;
        /// <summary>
        /// 是否有温馨提示
        /// </summary>
        public bool IsShowPrompt
        {
            get { return _isShowPrompt; }
            set
            {
                _isShowPrompt = value;
                RaisePropertyChanged(() => IsShowPrompt);
            }
        }

        private double _voteOptionScale;



        /// <summary>
        /// 
        /// </summary>
        public double VoteOptionScale
        {
            get { return _voteOptionScale; }
            set
            {
                _voteOptionScale = value;
                RaisePropertyChanged(() => VoteOptionScale);
            }
        }

        #endregion
        #region 命令
        /// <summary>
        /// 查看发起投票者信息
        /// </summary>
        public ICommand ParticipatorInfoCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {

                });
            }

        }
        /// <summary>
        /// 发送@消息
        /// </summary>
        public ICommand SendAtMsgCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var sendContent = new List<object>();
                    var atAll = new AntSdkChatMsg.contentAtAll { type = AntSdkAtMsgType.AtAll };
                    sendContent.Add(atAll);
                    var text = new AntSdkChatMsg.contentText
                    {
                        type = AntSdkAtMsgType.Text,
                        content = " "
                    };
                    sendContent.Add(text);
                    AntSdkChatMsg.contentText msgContent = new AntSdkChatMsg.contentText();
                    msgContent.type = AntSdkAtMsgType.Text;
                    msgContent.content = "小伙伴们 快来投票啦！";
                    sendContent.Add(msgContent);
                    SendAtMsgEvent?.Invoke(sendContent);
                    CloseVoteViewEvent?.Invoke();
                });
            }
        }

        public ICommand OptionSelectedCommand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null) return;
                    var optionId = (int)obj;
                    if (_isSingle)
                    {
                        var tempVoteOptionList = VoteOptionList.Where(m => m.OptionId != optionId);
                        foreach (var voteOption in tempVoteOptionList)
                        {
                            voteOption.IsOptionSelectd = false;
                        }

                    }
                    else
                    {
                        var tempVoteOptionList = VoteOptionList.Where(m => m.IsOptionSelectd);
                        var tempVoteOption = VoteOptionList.FirstOrDefault(m => m.OptionId == optionId);
                        if (tempVoteOptionList.Count() > 2)
                        {
                            if (tempVoteOption != null)
                                tempVoteOption.IsOptionSelectd = false;
                            IsShowPrompt = true;
                            PromptInfo = "最多只能选两项。";
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 投票
        /// </summary>
        public ICommand VoteCommand
        {
            get
            {
                return new DelegateCommand(() =>
                 {
                     var tempVoteOptionList = VoteOptionList.Where(m => m.IsOptionSelectd);
                     AntSdkGroupVoteOptionInput input = new AntSdkGroupVoteOptionInput();
                     foreach (var voteOption in tempVoteOptionList)
                     {
                         input.votes.Add(new VoteOption
                         {
                             createdBy = AntSdkService.AntSdkCurrentUserInfo.userId,
                             votingOptionId = voteOption.OptionId
                         });
                     }
                     if (input.votes.Count <= 0)
                     {
                         IsShowPrompt = true;
                         PromptInfo = "请选择投票项。";
                         return;
                     }
                     var errCode = 0;
                     var errMsg = string.Empty;
                     var result = AntSdkService.SubmitGroupVoteOptions(input, _voteId, ref errCode, ref errMsg);
                     if (result)
                         GoVoteEvent?.Invoke(_voteId, VoteViewType.VoteResult);
                     else
                     {
                         if (errCode == 404)
                         {
                             MessageBoxWindow.Show("提示", "该投票已被删除，无法再进行投票。", MessageBoxButton.OK,
                                 GlobalVariable.WarnOrSuccess.Warn);
                             CloseVoteViewEvent?.Invoke();
                         }
                         else if(errCode== 409)
                         {
                             MessageBoxWindow.Show("提示", "您已参与投票。", MessageBoxButton.OK,
                                GlobalVariable.WarnOrSuccess.Warn);
                             GoVoteEvent?.Invoke(_voteId, VoteViewType.VoteResult);
                         }
                         else
                         {
                             var output = AntSdkService.GetVoteInfo(_voteId, AntSdkService.AntSdkCurrentUserInfo.userId, ref errCode, ref errMsg);
                             if (output == null)
                             {
                                 MessageBoxWindow.Show("提示", "该投票已被删除，无法再进行投票。", MessageBoxButton.OK,
                                      GlobalVariable.WarnOrSuccess.Warn);
                                 CloseVoteViewEvent?.Invoke();
                             }
                             else if (output.voted)
                             {
                                 MessageBoxWindow.Show("提示", "您已参与投票。", MessageBoxButton.OK,
                                     GlobalVariable.WarnOrSuccess.Warn);
                                 GoVoteEvent?.Invoke(_voteId, VoteViewType.VoteResult);
                             }
                             else
                             {
                                 MessageBoxWindow.Show("提示", "投票失败，请重新投票。", MessageBoxButton.OK,
                                     GlobalVariable.WarnOrSuccess.Warn);
                             }
                         }
                         
                     }
                     //CloseVoteViewEvent?.Invoke();
                 });
            }
        }

        /// <summary>
        ///删除投票（投票发起者才能删除） 
        /// </summary>
        public ICommand DeleteVoteCommand
        {
            get
            {
                return new DelegateCommand(() =>
               {
                   try
                   {
                       if (MessageBoxWindow.Show("提示", "确定要删除该投票吗？", MessageBoxButton.YesNo,
                               GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Yes)
                       {
                           var errorCode = 0;
                           var errorMsg = "";
                           var isResult = AntSdkService.DeleteGroupVote(_voteId, ref errorCode, ref errorMsg);
                           if (isResult)
                           {
                               if (!VoteState)
                               {
                                   var sendContent = new List<object>();
                                   AntSdkChatMsg.contentAtAll atAll = new AntSdkChatMsg.contentAtAll
                                   {
                                       type = AntSdkAtMsgType.AtAll
                                   };
                                   sendContent.Add(atAll);
                                   AntSdkChatMsg.contentText text = new AntSdkChatMsg.contentText
                                   {
                                       type = AntSdkAtMsgType.Text,
                                       content = " "
                                   };
                                   sendContent.Add(text);
                                   var msgContent = new AntSdkChatMsg.contentText();
                                   msgContent.type = AntSdkAtMsgType.Text;
                                   msgContent.content = "[" + VoteTitle + "]" + " 投票已删除。";
                                   sendContent.Add(msgContent);
                                   SendAtMsgEvent?.Invoke(sendContent);
                               }
                               CloseVoteViewEvent?.Invoke();
                           }
                           else
                           {
                               if (errorCode == 404)
                               {
                                   MessageBoxWindow.Show("提示", "该投票已被删除。", MessageBoxButton.OK,
                                       GlobalVariable.WarnOrSuccess.Warn);
                                   CloseVoteViewEvent?.Invoke();
                               }
                               else
                               {
                                   MessageBoxWindow.Show("提示", "投票删除失败，请稍后再试！", MessageBoxButton.OK,
                                       GlobalVariable.WarnOrSuccess.Warn);
                               }
                           }
                       }

                   }
                   catch (Exception ex)
                   {
                       LogHelper.WriteError("NoticeWindowListsViewModel_btnOperate:" + ex.Message + ex.Source +
                                            ex.StackTrace);
                   }
               });
            }
        }
        /// <summary>
        /// 返回
        /// </summary>
        public ICommand CommandBackTalkMsg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    CloseVoteViewEvent?.Invoke();
                });
            }
        }

        #endregion
        #region 方法
        #endregion
    }
}
