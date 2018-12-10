using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Antenna.Framework;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.AntSdkDAL;

namespace AntennaChat.ViewModel.Vote
{
    public class CreateVoteViewModel : PropertyNotifyObject
    {
        private string _groupId;
        private DateTime serverDateTime=DateTime.Now;
        public CreateVoteViewModel(string groupId)
        {
            _groupId = groupId;
            var errCode = 0;
            var errMsg = string.Empty;
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            _displayDateEnd = serverDateTime.AddMonths(1);
            InitData();
        }
        /// <summary>
        /// 创建投票之后触发
        /// </summary>
        public event Action CreatedVoteEvent;

        /// <summary>
        /// 创建完后继续下一步
        /// </summary>
        public event Action<int, VoteViewType> GoVoteEvent;
        #region 属性

        private string _voteSubject;
        /// <summary>
        /// 投票项目的主题
        /// </summary>
        public string VoteSubject
        {
            get { return _voteSubject; }
            set
            {
                _voteSubject = value;
                RaisePropertyChanged(() => VoteSubject);
            }
        }

        private int _voteSubjectNum = 0;
        /// <summary>
        /// 主题字数
        /// </summary>
        public int VoteSubjectNum
        {
            get { return _voteSubjectNum; }
            set
            {
                _voteSubjectNum = value;
                RaisePropertyChanged(() => VoteSubjectNum);
            }
        }

        private DateTime _voteEndDate = DateTime.Now;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime VoteEndDate
        {
            get { return _voteEndDate; }
            set
            {
                _voteEndDate = value;
                IsShowPrompt = false;
                if (DateTime.Compare(_voteEndDate.Date, serverDateTime.Date) < 0 )
                {
                    _voteEndDate = serverDateTime.Date;
                }
                else if(DateTime.Compare(_voteEndDate.Date, serverDateTime.Date.AddDays(30)) > 0)
                {
                    IsShowPrompt = true;
                    PromptInfo = GlobalVariable.VotePrompt.VoteEndDateExceedCurrent;
                    return;
                }
                
                RaisePropertyChanged(() => VoteEndDate);
            }
        }

        private DateTime _displayDateEnd;
        /// <summary>
        /// 时间控件显示最后日期
        /// </summary>
        public DateTime DisplayDateEnd
        {
            get { return _displayDateEnd; }
            set
            {
                _displayDateEnd = value;
            }
        }

        //private List<string> _hourList = new List<string>();
        ///// <summary>
        ///// 时列表
        ///// </summary>
        //public List<string> HourList
        //{
        //    get { return _hourList; }
        //    set
        //    {
        //        _hourList = value;
        //        RaisePropertyChanged(() => HourList);
        //    }
        //}


        private string _hourSelected = DateTime.Now.AddMinutes(35).ToString("HH");
        private string tempHourSelected = DateTime.Now.AddMinutes(35).ToString("HH");
        /// <summary>
        /// 时
        /// </summary>
        public string HourSelected
        {
            get { return _hourSelected; }
            set
            {

                _hourSelected = value;
                if (DateTime.Compare(VoteEndDate.Date, DateTime.Now.Date) == 0 && int.Parse(_hourSelected) < int.Parse(tempHourSelected))
                {
                    _hourSelected = tempHourSelected;
                }
                //else
                //{
                //    tempHourSelected = _hourSelected;
                //}

                RaisePropertyChanged(() => HourSelected);
            }
        }

        //private List<string> _minuteList = new List<string>();
        ///// <summary>
        ///// 分钟列表
        ///// </summary>
        //public List<string> MinuteList
        //{
        //    get { return _minuteList; }
        //    set
        //    {
        //        _minuteList = value;
        //        RaisePropertyChanged(() => MinuteList);
        //    }
        //}
        private string _minuteSelected = DateTime.Now.AddMinutes(35).ToString("mm");
        private string tempMinuteSelected = DateTime.Now.AddMinutes(35).ToString("mm");
        /// <summary>
        /// 分钟
        /// </summary>
        public string MinuteSelected
        {
            get { return _minuteSelected; }
            set
            {
                _minuteSelected = value;
                if (DateTime.Compare(VoteEndDate.Date, DateTime.Now.Date) == 0
                    && int.Parse(_hourSelected) == int.Parse(tempHourSelected)
                    && int.Parse(_minuteSelected) < int.Parse(tempMinuteSelected))
                {
                    _minuteSelected = tempMinuteSelected;
                }
                //else
                //{
                //    tempMinuteSelected = _minuteSelected;
                //}
                RaisePropertyChanged(() => MinuteSelected);
            }
        }
        private bool _isAnonymousVote;
        /// <summary>
        /// 是否匿名投票
        /// </summary>
        public bool IsAnonymousVote
        {
            get { return _isAnonymousVote; }
            set
            {
                _isAnonymousVote = value;
                RaisePropertyChanged(() => IsAnonymousVote);
            }
        }

        private bool _isSingle = true;
        /// <summary>
        /// 是否单选
        /// </summary>
        public bool IsSingle
        {
            get { return _isSingle; }
            set
            {
                _isSingle = value;
                RaisePropertyChanged(() => IsSingle);
            }
        }
        private bool _isMultiple;
        /// <summary>
        /// 是否多选
        /// </summary>
        public bool IsMultiple
        {
            get { return _isMultiple; }
            set
            {
                _isMultiple = value;
                RaisePropertyChanged(() => IsMultiple);
            }
        }
        private ObservableCollection<OptionContent> _optionContentList = new ObservableCollection<OptionContent>();
        /// <summary>
        /// 选项列表
        /// </summary>
        public ObservableCollection<OptionContent> OptionContentList
        {
            get { return _optionContentList; }
            set
            {
                _optionContentList = value;
                RaisePropertyChanged(() => OptionContentList);
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

        private bool _addOptionIsEnabled = true;
        /// <summary>
        /// 添加选项按钮是否禁用
        /// </summary>
        public bool AddOptionIsEnabled
        {
            get { return _addOptionIsEnabled; }
            set
            {
                _addOptionIsEnabled = value;
                RaisePropertyChanged(() => AddOptionIsEnabled);
            }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 删除选项
        /// </summary>
        public ICommand DeleteOptionCommmand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null) return;
                    var optionId = (int)obj;
                    var optionContent = OptionContentList.FirstOrDefault(m => m.OptionId == optionId);
                    if (optionContent == null) return;
                    OptionContentList.Remove(optionContent);
                    for (var i = 0; i < OptionContentList.Count; i++)
                    {
                        var tempOptionContent = OptionContentList[i];
                        tempOptionContent.OptionId = i + 1;
                    }
                    if (OptionContentList.Count < 50 && !AddOptionIsEnabled)
                        AddOptionIsEnabled = true;
                });
            }
        }
        /// <summary>
        /// 增加选项
        /// </summary>
        public ICommand AddOptionCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
                    {
                        OptionContentList.Add(new OptionContent
                        {
                            IsOptionDelete = true,
                            OptionId = _optionContentList.Count + 1,
                        });
                        if (OptionContentList.Count >= 50 && AddOptionIsEnabled)
                        {
                            AddOptionIsEnabled = false;
                        }
                    });
                });
            }
        }
        /// <summary>
        /// 确认发起投票
        /// </summary>
        public ICommand InitiateVoteCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var errCode = 0;
                    var errMsg = string.Empty;

                    if (string.IsNullOrEmpty(VoteSubject))
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteSubjectIsEmpty;
                        return;
                    }
                    var endDateTimeStr = VoteEndDate.Date.ToString("yyyy-M-d") + " " + HourSelected + ":" + MinuteSelected + ":00";
                    DateTime endDateTime = Convert.ToDateTime(endDateTimeStr);
                    if (DateTime.Compare(endDateTime, DateTime.Now) < 0)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteEndDateTimePrompt;
                        return;
                    }
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                    DateTime serverDateTime = DateTime.Now;
                    if (serverResult != null)
                    {
                        serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                    }
                    TimeSpan time = endDateTime - serverDateTime;
                    if (time.TotalMinutes < 30)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteEndDateTimePrompt;
                        return;
                    }
                    if (time.TotalDays > 30)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteEndDateExceedCurrent;
                        return;
                    }
                    var tempOptionContentList = OptionContentList.Where(m => !string.IsNullOrEmpty(m.OptionName));
                    var optionContentList = tempOptionContentList as IList<OptionContent> ?? tempOptionContentList.ToList();
                    if (optionContentList.Count == 0)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteOptionIsEmpty;
                        return;
                    }
                    if (optionContentList.Count == 1)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteOptionNum;
                        return;
                    }
                    var b = OptionContentList.GroupBy(l => l.OptionName).Any(g => g.Count() > 1);
                    if (b)
                    {
                        IsShowPrompt = true;
                        PromptInfo = GlobalVariable.VotePrompt.VoteOptionRepeatContent;
                        return;
                    }
                    AntSdkCreateGroupVoteInput voteInput = new AntSdkCreateGroupVoteInput();
                    voteInput.secret = IsAnonymousVote;
                    voteInput.maxChoiceNumber = IsSingle ? 1 : 2;
                    voteInput.title = VoteSubject;
                    //voteInput.id = _groupId;
                    voteInput.createdBy = AntSdkService.AntSdkCurrentUserInfo.userId;
                    voteInput.expiryTime = endDateTimeStr;
                    //voteInput.createdDate = DateTime.Now.ToString("yy-MM-dd hh:mm:ss");
                    foreach (var content in optionContentList)
                    {
                        voteInput.options.Add(new VoteOptionContent { name = content.OptionName });
                    }


                    //调用投票接口
                    var result = AntSdkService.CreateGroupVote(voteInput, _groupId, ref errCode, ref errMsg);
                    if (result != null)
                    {
                        //CreatedVoteEvent?.Invoke();
                        GoVoteEvent?.Invoke(result.id, VoteViewType.VoteDetail);
                    }
                    else
                    {
                        GlobalVariable.ShowDialogResult dr = MessageBoxWindow.Show("投票接口异常" + ",是否继续发布投票？",
                            MessageBoxButton.YesNo, GlobalVariable.WarnOrSuccess.Warn);
                        if (dr.ToString() == "No")
                        {
                            CreatedVoteEvent?.Invoke();
                        }
                    }

                });
            }
        }
        /// <summary>
        /// 取消发起投票
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(() =>
               {
                   CreatedVoteEvent?.Invoke();
               });
            }
        }

        #endregion

        #region 方法

        private void InitData()
        {
            _optionContentList.Add(new OptionContent { IsOptionDelete = false, OptionId = 1 });
            _optionContentList.Add(new OptionContent { IsOptionDelete = false, OptionId = 2 });
            _optionContentList.Add(new OptionContent { IsOptionDelete = true, OptionId = 3 });
        }

        #endregion
    }

    public class OptionContent : PropertyNotifyObject
    {
        private int _optionId;
        /// <summary>
        /// 选项Id
        /// </summary>
        public int OptionId
        {
            get { return _optionId; }

            set
            {
                _optionId = value;
                RaisePropertyChanged(() => OptionId);
            }
        }

        private string _optionName;
        /// <summary>
        /// 投票选项名称
        /// </summary>
        public string OptionName
        {
            get { return _optionName; }
            set
            {
                _optionName = value;
                RaisePropertyChanged(() => OptionName);
            }
        }

        private bool _isOptionSelectd;
        /// <summary>
        /// 选项是否被投票
        /// </summary>
        public bool IsOptionSelectd
        {
            get { return _isOptionSelectd; }
            set
            {
                _isOptionSelectd = value;
                RaisePropertyChanged(() => IsOptionSelectd);
            }
        }

        private int _optionVotes;
        /// <summary>
        /// 选项被投票的票数
        /// </summary>
        public int OptionVotes
        {
            get { return _optionVotes; }
            set
            {
                _optionVotes = value;
                RaisePropertyChanged(() => OptionVotes);
            }
        }

        private bool _isCurrentUserSelected;
        /// <summary>
        /// 是否当前用户的投票项
        /// </summary>
        public bool IsCurrentUserSelected
        {
            get { return _isCurrentUserSelected; }
            set
            {
                _isCurrentUserSelected = value;
                RaisePropertyChanged(() => IsCurrentUserSelected);
            }
        }


        /// <summary>
        /// 是否删除选项
        /// </summary>
        public bool IsOptionDelete { get; set; }

    }
}
