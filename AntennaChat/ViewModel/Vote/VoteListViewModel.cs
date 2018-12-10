using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Vote
{
    public class VoteListViewModel : PropertyNotifyObject
    {
        private string _groupId;
        private int _page = 0;
        private int _size = 10;
        private bool _isFirst;
        private bool _isLast;
        public VoteListViewModel(string groupId)
        {
            _groupId = groupId;
            _voteInfoList = new ObservableCollection<VoteInfoModel>();
            var errCode = 0;
            var errMsg = string.Empty;
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            var voteList = AntSdkService.GetGroupVotes(groupId, ref errCode, ref errMsg, _page, _size, AntSdkService.AntSdkCurrentUserInfo.userId);
            if (voteList?.content != null && voteList.content.Count > 0)
            {
                foreach (var voteInfo in voteList.content)
                {
                    var tempInfoModel = new VoteInfoModel();
                    tempInfoModel.IsHaveVoted = voteInfo.voted;
                    if (voteInfo.createdBy == AntSdkService.AntSdkCurrentUserInfo.userId)
                        tempInfoModel.IsbtnDeleteVisibility = true;
                    tempInfoModel.VoteId = voteInfo.id;
                    tempInfoModel.VoteSate = DateTime.Compare(Convert.ToDateTime(voteInfo.expiryTime), serverDateTime) < 0;
                    tempInfoModel.VoteTitle = voteInfo.title + (voteInfo.secret ? "(匿名)" : "");
                    tempInfoModel.UserID = voteInfo.createdBy;
                    AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == voteInfo.createdBy);
                    if (user != null)
                    {
                        tempInfoModel.Explain = user.userNum + user.userName + "  编辑于  " +
                                                DataConverter.FormatTimeByCreateTime(voteInfo.createdDate);
                        if (!string.IsNullOrWhiteSpace(user.picture)&&publicMethod.IsUrlRegex(user.picture))
                        {
                            var index = user.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                            var fileNameIndex = user.picture.LastIndexOf(".", StringComparison.Ordinal);
                            var fileName = user.picture.Substring(index, fileNameIndex - index);
                            string strUrl = user.picture.Replace(fileName, fileName + "_35x35");
                            var userImage =
                                GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                    m => m.UserID == user.userId);
                            tempInfoModel.UserHeadUrl = string.IsNullOrEmpty(userImage?.Url) ? strUrl : userImage.Url;
                        }
                        else
                        {
                            tempInfoModel.UserHeadUrl = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                        }
                    }
                    //tempInfoModel.UserHeadUrl=
                    _voteInfoList.Add(tempInfoModel);
                }
                _isFirst = voteList.first;
                _isLast = voteList.last;
                IsPaging = !_isLast;
            }
            else
            {
                IsVoteData = true;
                IsPaging = false;
            }
        }

        public event Action<bool, VoteViewType, int> GoVoteOperationEvent;
        /// <summary>
        /// 发送@消息时间
        /// </summary>
        public event Action<List<object>> SendAtMsgEvent;

        #region 属性

        private VoteInfoModel _currentSelectedVote;
        /// <summary>
        /// 
        /// </summary>
        public VoteInfoModel CurrentSelectedVote
        {
            get { return _currentSelectedVote; }
            set
            {
                _currentSelectedVote = value;
                RaisePropertyChanged(() => CurrentSelectedVote);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<VoteInfoModel> _voteInfoList;
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<VoteInfoModel> VoteInfoList
        {
            get { return _voteInfoList; }
            set
            {
                _voteInfoList = value;
                RaisePropertyChanged(() => VoteInfoList);
            }
        }
        private bool _isVoteData;
        /// <summary>
        /// 是否有股票数据
        /// </summary>
        public bool IsVoteData
        {
            get { return _isVoteData; }
            set
            {
                _isVoteData = value;
                RaisePropertyChanged(() => IsVoteData);
            }
        }

        private bool _isPaging;
        /// <summary>
        /// 是否要分页
        /// </summary>
        public bool IsPaging {
            get { return _isPaging; }
            set { _isPaging = value; }
        }
        private bool _isShowBtnAddVote = true;
        /// <summary>
        /// 是否显示增加投票
        /// </summary>
        public bool IsShowBtnAddVote
        {
            get { return _isShowBtnAddVote; }
            set
            {
                _isShowBtnAddVote = value;
                RaisePropertyChanged(() => IsShowBtnAddVote);

            }
        }

        #endregion
        #region 命令
        /// <summary>
        /// 返回
        /// </summary>
        public ICommand CommandBackTalkMsg
        {
            get
            {
                return new DelegateCommand(() =>
               {
                   GoVoteOperationEvent?.Invoke(false, VoteViewType.CreateVote, 0);
               });
            }
        }
        /// <summary>
        /// 查看投票详情
        /// </summary>
        public ICommand GoVoteDetailCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (CurrentSelectedVote != null)
                    {
                        GoVoteOperationEvent?.Invoke(true, CurrentSelectedVote.IsHaveVoted  || CurrentSelectedVote.VoteSate ? VoteViewType.VoteResult : VoteViewType.VoteDetail, CurrentSelectedVote.VoteId);
                    }
                });
            }
        }
        /// <summary>
        /// 删除某个投票
        /// </summary>
        public ICommand DeleteVoteCommand
        {
            get
            {
                return new DefaultCommand(obj =>
               {
                   if (obj == null) return;
                   var voteId = (int)obj;
                   try
                   {
                       if (MessageBoxWindow.Show("提示", "确定要删除该投票吗？", MessageBoxButton.YesNo,
                               GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Yes)
                       {
                           var errorCode = 0;
                           var errorMsg = "";
                           var isResult = AntSdkService.DeleteGroupVote(voteId, ref errorCode, ref errorMsg);
                           if (isResult)
                           {
                               var voteInfo = VoteInfoList.FirstOrDefault(m => m.VoteId == voteId);
                               if (voteInfo != null)
                               {
                                   if (!voteInfo.VoteSate)
                                   {
                                       var sendContent = new List<object>();
                                       var atAll = new AntSdkChatMsg.contentAtAll {type = AntSdkAtMsgType.AtAll};
                                       sendContent.Add(atAll);
                                       var text = new AntSdkChatMsg.contentText
                                       {
                                           type = AntSdkAtMsgType.Text,
                                           content = " "
                                       };
                                       sendContent.Add(text);
                                       var msgContent = new AntSdkChatMsg.contentText();
                                       msgContent.type = AntSdkAtMsgType.Text;
                                       msgContent.content = "[" + voteInfo.VoteTitle + "]" + " 投票已删除。";
                                       sendContent.Add(msgContent);
                                       SendAtMsgEvent?.Invoke(sendContent);
                                   }
                                   VoteInfoList.Remove(voteInfo);
                               }
                               if (VoteInfoList.Count == 0)
                               {
                                   IsVoteData = true;
                               }
                           }
                           else
                           {
                               if (errorCode == 404)
                               {
                                   MessageBoxWindow.Show("提示", "该投票已被删除。", MessageBoxButton.OK,
                                       GlobalVariable.WarnOrSuccess.Warn);
                                   var voteInfo = VoteInfoList.FirstOrDefault(m => m.VoteId == voteId);
                                   if (voteInfo != null)
                                       VoteInfoList.Remove(voteInfo);
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
        /// 创建投票
        /// </summary>
        public ICommand CreateVoteCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    GoVoteOperationEvent?.Invoke(true, VoteViewType.CreateVote, 0);
                });
            }
        }
        /// <summary>
        /// 上一页
        /// </summary>
        public ICommand GoPreviousPage
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (_isFirst)
                        return;
                    _page--;
                    if (_page < 0)
                        _page = 0;
                    LoadVotesData();
                });
            }
        }
        /// <summary>
        /// 下一页
        /// </summary>
        public ICommand GoNextPage
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (_isLast)
                        return;
                    _page++;
                    LoadVotesData();
                });
            }
        }

        /// <summary>
        /// 分页查询投票数据
        /// </summary>
        private void LoadVotesData()
        {
            DateTime serverDateTime = DateTime.Now;
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                () =>
                {
                    var errCode = 0;
                    var errMsg = string.Empty;
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                    if (serverResult != null)
                    {
                        serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                    }
                    var voteList = AntSdkService.GetGroupVotes(_groupId, ref errCode, ref errMsg, _page, _size,
                        AntSdkService.AntSdkCurrentUserInfo.userId);
                    return voteList;
                },
                (ex, datas) =>
                {
                    VoteInfoList.Clear();
                    if (datas?.content != null && datas.content.Count > 0)
                    {
                        foreach (var voteInfo in datas.content)
                        {
                            var tempInfoModel = new VoteInfoModel();
                            tempInfoModel.IsHaveVoted = voteInfo.voted;
                            if (voteInfo.createdBy == AntSdkService.AntSdkCurrentUserInfo.userId)
                                tempInfoModel.IsbtnDeleteVisibility = true;
                            tempInfoModel.VoteId = voteInfo.id;
                            tempInfoModel.VoteSate =
                                DateTime.Compare(Convert.ToDateTime(voteInfo.expiryTime), serverDateTime) < 0;
                            tempInfoModel.VoteTitle = voteInfo.title + (voteInfo.secret ? "(匿名)" : "");
                            tempInfoModel.UserID = voteInfo.createdBy;
                            AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == voteInfo.createdBy);
                            if (user != null)
                            {
                                tempInfoModel.Explain = user.userNum + user.userName + "  编辑于  " +
                                                        DataConverter.FormatTimeByCreateTime(voteInfo.createdDate);
                                if (!string.IsNullOrWhiteSpace(user.picture) && publicMethod.IsUrlRegex(user.picture))
                                {
                                    var userImage =
                                        GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                            m => m.UserID == user.userId);
                                    tempInfoModel.UserHeadUrl = string.IsNullOrEmpty(userImage?.Url) ? user.picture : userImage.Url;
                                }
                                else
                                {
                                    tempInfoModel.UserHeadUrl = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                                }
                            }
                            //tempInfoModel.UserHeadUrl=
                            _voteInfoList.Add(tempInfoModel);
                        }
                    }
                    if (datas != null)
                    {
                        _isFirst = datas.first;
                        _isLast = datas.last;
                    }
                });
        }

        #endregion
        #region 方法
        #endregion
    }

    public enum VoteViewType
    {
        /// <summary>
        /// 创建投票
        /// </summary>
        CreateVote,
        /// <summary>
        /// 投票详情
        /// </summary>
        VoteDetail,
        /// <summary>
        /// 进行投票
        /// </summary>
        VoteResult
    }

    public enum SendAtMsgType
    {
        /// <summary>
        /// 删除投票发送At消息
        /// </summary>
        DeleteVote,
        /// <summary>
        /// 通知所有成员来投票
        /// </summary>
        SendAtMsg
    }

    public class VoteInfoModel
    {
        /// <summary>
        /// 投票活动标识
        /// </summary>
        public int VoteId { get; set; }

        /// <summary>
        /// 投票活动标题
        /// </summary>
        public string VoteTitle { get; set; }

        /// <summary>
        /// 投票状态（进行中、已结束）
        /// </summary>
        public bool VoteSate { get; set; }

        /// <summary>
        /// 投票发起者信息和时间拼接串
        /// </summary>
        public string Explain { get; set; }

        /// <summary>
        /// 投票发起者头像
        /// </summary>
        public string UserHeadUrl { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 当前用户是否已投票
        /// </summary>
        public bool IsHaveVoted { get; set; }

        /// <summary>
        /// 投票活动是否是当前用户发起的
        /// </summary>
        public bool IsbtnDeleteVisibility { get; set; }


    }
}
