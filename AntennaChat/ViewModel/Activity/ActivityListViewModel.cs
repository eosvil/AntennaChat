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
using AntennaChat.Command;
using AntennaChat.ViewModel.Vote;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Activity
{
    public class ActivityListViewModel : PropertyNotifyObject
    {
        private string _groupId;
        private int _page = 1;
        private int _size = 10;
        private bool _isFirst;
        private bool _isLast;
        private bool _isAdminId;
        public ActivityListViewModel(bool isAdminId, string groupId)
        {
            _isAdminId = isAdminId;
            if (!_isAdminId)
            {
                IsShowBtnAddActivity = false;
            }
            _groupId = groupId;
            _activityInfoList = new ObservableCollection<ActivityInfoModel>();
            var errCode = 0;
            var errMsg = string.Empty;
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                () =>
                {
                    AntSdkGetGroupActivitysInput intput = new AntSdkGetGroupActivitysInput
                    {
                        groupId = _groupId,
                        activityStatus = 0,
                        pageNum = _page,
                        pageSize = _size,
                        userId = AntSdkService.AntSdkCurrentUserInfo.userId
                    };
                    var activityList = AntSdkService.GetGroupActivitys(intput, ref errCode, ref errMsg);
                    return activityList;
                },
                (ex, datas) =>
                {
                    if (datas?.list != null && datas.list.Count > 0)
                    {
                        foreach (var activityInfo in datas.list)
                        {
                            var tempInfoModel = new ActivityInfoModel();
                            tempInfoModel.IsHaveActivity = activityInfo.voteFlag;
                            if (activityInfo.userId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                tempInfoModel.IsbtnDeleteVisibility = true;
                            tempInfoModel.ActivityId = activityInfo.activityId;

                            tempInfoModel.ActivitySate = activityInfo.activityStatus == 2;
                            tempInfoModel.ActivityTitle = activityInfo.theme;

                            //AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == activityInfo.userId);
                            if (!string.IsNullOrEmpty(activityInfo.picture))
                            {
                                var index = activityInfo.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                                var fileNameIndex = activityInfo.picture.LastIndexOf(".", StringComparison.Ordinal);
                                var fileName = activityInfo.picture.Substring(index, fileNameIndex - index);
                                string strUrl = activityInfo.picture.Replace(fileName, fileName + "_80x80");
                                try
                                {
                                    BitmapImage image = new BitmapImage();

                                    image.BeginInit();

                                    image.UriSource = new System.Uri(strUrl);

                                    image.DecodePixelWidth = 800;

                                    image.EndInit();

                                    //image.Freeze();
                                    tempInfoModel.ActivityThemePicture = image;
                                }
                                catch (Exception e)
                                {
                                    LogHelper.WriteError("[ActivityListViewModel_ImageOnLoad]:" + e.Message + e.StackTrace + e.Source);
                                }
                            }

                            tempInfoModel.ActivityAddress = activityInfo.address;
                            tempInfoModel.CreatedActivityDate = activityInfo.createTime;
                            if (!string.IsNullOrEmpty(activityInfo.startTime))
                                tempInfoModel.ActivityDate =
                                    Convert.ToDateTime(activityInfo.startTime).ToString("yyyy-MM-dd HH:mm");
                            _activityInfoList.Add(tempInfoModel);
                        }
                        _isFirst = datas.isFirstPage;
                        _isLast = datas.isLastPage;
                        IsPaging = !_isLast;
                    }
                    else
                    {
                        IsActivityData = true;
                        IsPaging = false;
                    }
                });
        }

        public event Action<bool, ActivityViewType, int> GoActivityOperationEvent;
        /// <summary>
        /// 发送@消息时间
        /// </summary>
        public event Action<List<object>> SendAtMsgEvent;

        #region 属性

        private ActivityInfoModel _currentSelectedActivity;
        /// <summary>
        /// 活动详情
        /// </summary>
        public ActivityInfoModel CurrentSelectedActivity
        {
            get { return _currentSelectedActivity; }
            set
            {
                _currentSelectedActivity = value;
                RaisePropertyChanged(() => CurrentSelectedActivity);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<ActivityInfoModel> _activityInfoList;
        /// <summary>
        /// 活动列表集合
        /// </summary>
        public ObservableCollection<ActivityInfoModel> ActivityInfoList
        {
            get { return _activityInfoList; }
            set
            {
                _activityInfoList = value;
                RaisePropertyChanged(() => ActivityInfoList);
            }
        }
        private bool _isActivityData;
        /// <summary>
        /// 是否有活动数据
        /// </summary>
        public bool IsActivityData
        {
            get { return _isActivityData; }
            set
            {
                _isActivityData = value;
                RaisePropertyChanged(() => IsActivityData);
            }
        }

        private bool _isPaging;
        /// <summary>
        /// 是否要分页
        /// </summary>
        public bool IsPaging
        {
            get { return _isPaging; }
            set { _isPaging = value; }
        }
        private bool _isShowBtnAddActivity = true;
        /// <summary>
        /// 是否显示增加投票
        /// </summary>
        public bool IsShowBtnAddActivity
        {
            get { return _isShowBtnAddActivity; }
            set
            {
                _isShowBtnAddActivity = value;
                RaisePropertyChanged(() => IsShowBtnAddActivity);

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
                    GoActivityOperationEvent?.Invoke(false, ActivityViewType.ReleaseActivity, 0);
                });
            }
        }
        /// <summary>
        /// 查看投票详情
        /// </summary>
        public ICommand GoActivityDetailCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (CurrentSelectedActivity != null)
                    {
                        GoActivityOperationEvent?.Invoke(true, CurrentSelectedActivity.IsHaveActivity || CurrentSelectedActivity.ActivitySate ? ActivityViewType.ActivityResult : ActivityViewType.ActivityDetail, CurrentSelectedActivity.ActivityId);
                    }
                });
            }
        }
        /// <summary>
        /// 删除某个活动
        /// </summary>
        public ICommand DeleteActivityCommand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null) return;
                    var activityId = (int)obj;
                    try
                    {
                        if (MessageBoxWindow.Show("提示", "确定要删除该活动吗？", MessageBoxButton.YesNo,
                                GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Yes)
                        {
                            var errorCode = 0;
                            var errorMsg = "";
                            AntSdkDeleteGroupActivityInput input = new AntSdkDeleteGroupActivityInput
                            {
                                activityId = activityId.ToString(),
                                userId = AntSdkService.AntSdkCurrentUserInfo.userId
                            };
                            var isResult = AntSdkService.DeleteGroupActivity(input, ref errorCode, ref errorMsg);
                            if (isResult)
                            {
                                var activityInfo = ActivityInfoList.FirstOrDefault(m => m.ActivityId == activityId);
                                if (activityInfo != null)
                                {
                                    if (!activityInfo.ActivitySate)
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
                                        var msgContent = new AntSdkChatMsg.contentText();
                                        msgContent.type = AntSdkAtMsgType.Text;
                                        msgContent.content = "[" + activityInfo.ActivityTitle + "]" + "活动已删除" + "\r\n活动地点：" + activityInfo.ActivityAddress + "\r\n活动时间：" + activityInfo.ActivityDate;
                                        sendContent.Add(msgContent);
                                        SendAtMsgEvent?.Invoke(sendContent);
                                    }
                                    ActivityInfoList.Remove(activityInfo);
                                }
                                if (ActivityInfoList.Count == 0)
                                {
                                    IsActivityData = true;
                                }
                            }
                            else
                            {

                                if (errorCode != 0)
                                {
                                    MessageBoxWindow.Show("提示", errorMsg, MessageBoxButton.OK,
                                                      GlobalVariable.WarnOrSuccess.Warn);
                                    //if (errorCode == 9009)
                                    //{
                                    //    var activityInfo = ActivityInfoList.FirstOrDefault(m => m.ActivityId == activityId);
                                    //    if (activityInfo != null)
                                    //        ActivityInfoList.Remove(activityInfo);
                                    //}
                                }
                                else
                                {
                                    MessageBoxWindow.Show("提示", "活动删除失败，请稍后再试！", MessageBoxButton.OK,
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
        public ICommand ReleaseActivityCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    GoActivityOperationEvent?.Invoke(true, ActivityViewType.ReleaseActivity, 0);
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
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                () =>
                {
                    var errCode = 0;
                    var errMsg = string.Empty;
                    AntSdkGetGroupActivitysInput intput = new AntSdkGetGroupActivitysInput
                    {
                        groupId = _groupId,
                        activityStatus = 0,
                        pageNum = _page,
                        pageSize = _size,
                        userId = AntSdkService.AntSdkCurrentUserInfo.userId
                    };
                    var activityList = AntSdkService.GetGroupActivitys(intput, ref errCode, ref errMsg);
                    return activityList;
                },
                (ex, datas) =>
                {
                    ActivityInfoList.Clear();
                    if (datas?.list != null && datas.list.Count > 0)
                    {
                        foreach (var activityInfo in datas.list)
                        {
                            var tempInfoModel = new ActivityInfoModel();
                            tempInfoModel.IsHaveActivity = activityInfo.voteFlag;
                            if (activityInfo.userId == AntSdkService.AntSdkCurrentUserInfo.userId)
                                tempInfoModel.IsbtnDeleteVisibility = true;
                            tempInfoModel.ActivityId = activityInfo.activityId;

                            tempInfoModel.ActivitySate = activityInfo.activityStatus == 2;
                            tempInfoModel.ActivityTitle = activityInfo.theme;

                            AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == activityInfo.userId);
                            if (user != null)
                            {
                                if (!string.IsNullOrEmpty(activityInfo.picture))
                                {
                                    BitmapImage image = new BitmapImage();

                                    image.BeginInit();

                                    image.UriSource = new System.Uri(activityInfo.picture);

                                    image.DecodePixelWidth = 800;

                                    image.EndInit();

                                    image.Freeze();
                                    tempInfoModel.ActivityThemePicture = image;
                                }
                                tempInfoModel.ActivityAddress = activityInfo.address;
                                tempInfoModel.CreatedActivityDate = activityInfo.createTime;
                                if (!string.IsNullOrEmpty(activityInfo.startTime))
                                    tempInfoModel.ActivityDate = Convert.ToDateTime(activityInfo.startTime).ToString("yyyy-MM-dd HH:mm");
                            }
                            //tempInfoModel.UserHeadUrl=
                            _activityInfoList.Add(tempInfoModel);
                        }
                    }
                    if (datas != null)
                    {
                        _isFirst = datas.isFirstPage;
                        _isLast = datas.isLastPage;
                    }
                });
        }

        #endregion
        #region 方法
        #endregion
    }

    public enum ActivityViewType
    {
        /// <summary>
        /// 创建活动
        /// </summary>
        ReleaseActivity,
        /// <summary>
        /// 活动详情
        /// </summary>
        ActivityDetail,
        /// <summary>
        /// 参与活动
        /// </summary>
        ActivityResult
    }

    public enum SendAtMsgType
    {
        /// <summary>
        /// 删除投票发送At消息
        /// </summary>
        DeleteActivity,
        /// <summary>
        /// 通知所有成员来投票
        /// </summary>
        SendAtMsg
    }
    public class ActivityInfoModel
    {
        /// <summary>
        /// 活动标识
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// 活动状态（进行中、已结束）
        /// </summary>
        public bool ActivitySate { get; set; }

        /// <summary>
        /// 活动图片
        /// </summary>
        public BitmapImage ActivityThemePicture { get; set; }

        /// <summary>
        /// 活动创建时间
        /// </summary>
        public string CreatedActivityDate { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public string ActivityDate { get; set; }

        /// <summary>
        /// 活动地址
        /// </summary>
        public string ActivityAddress { get; set; }

        /// <summary>
        /// 是否已参与活动
        /// </summary>
        public bool IsHaveActivity { get; set; }

        /// <summary>
        /// 投票活动是否是当前用户发起的（显示删除按钮）
        /// </summary>
        public bool IsbtnDeleteVisibility { get; set; }

    }
}
