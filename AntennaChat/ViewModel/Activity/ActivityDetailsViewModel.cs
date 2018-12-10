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
using AntennaChat.ViewModel.Contacts;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using AntennaChat.Views.Activity;
using AntennaChat.Views.Contacts;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Activity
{
    public class ActivityDetailsViewModel : PropertyNotifyObject
    {
        #region 私有变量
        /// <summary>
        /// 纬度
        /// </summary>
        private float latitude;
        /// <summary>
        /// 经度
        /// </summary>
        private float longitude;
        /// <summary>
        /// 关闭活动详情
        /// </summary>
        public event Action CloseActivityEvent;

        private int pageNum = 1;
        private int pageSize = 30;

        private int _activityId;
        private string _groupId;
        #endregion
        public ActivityDetailsViewModel(int activityId, string groupId)
        {
            _activityId = activityId;
            _groupId = groupId;
            var errCode = 0;
            var errMsg = string.Empty;
            DateTime serverDateTime = DateTime.Now;
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                () =>
                {
                    AntSdkGetGroupActivityDetailsInput input = new AntSdkGetGroupActivityDetailsInput
                    {
                        userId = AntSdkService.AntSdkCurrentUserInfo.userId,
                        activityId = activityId,
                        groupId = groupId
                    };
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode,
                        ref errMsg);

                    if (serverResult != null)
                    {
                        serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                    }
                    var output = AntSdkService.GetActivityInfo(input, ref errCode, ref errMsg);
                    return output;
                },
                (ex, datas) =>
                {
                    if (datas != null)
                    {
                        ActivityTitle = datas.theme;
                        if (!string.IsNullOrEmpty(datas.picture))
                        {
                            try
                            {
                                BitmapImage image = new BitmapImage();

                                image.BeginInit();

                                image.UriSource = new System.Uri(datas.picture);

                                image.DecodePixelWidth = 800;

                                image.EndInit();

                                ActivityThemePic = image;
                            }
                            catch (Exception e)
                            {
                                LogHelper.WriteError("[ActivityDetailsViewModel_ImageOnLoad]:" + e.Message + e.StackTrace + e.Source);
                            }
                        }
                        ActivityAddress = datas.address;
                        ActivityIntroduce = datas.description;
                        latitude = datas.latitude;
                        longitude = datas.longitude;
                        Console.Out.WriteLine("");
                        var startTime = DateTime.Now;
                        var endTime = DateTime.Now.AddMinutes(30);
                        if (!string.IsNullOrEmpty(datas.startTime) && !string.IsNullOrEmpty(datas.endTime))
                        {
                            startTime = Convert.ToDateTime(datas.startTime);
                            endTime = Convert.ToDateTime(datas.endTime);
                            TimeSpan timeSpan = endTime - startTime;
                            if (endTime.Year - startTime.Year == 0 && endTime.Day - startTime.Day == 0)
                                ActivityDateTime = startTime.ToString("yyyy-MM-dd HH:mm") + "-" +
                                                   endTime.ToString("HH:mm");
                            else
                            {
                                ActivityDateTime = startTime.ToString("yyyy-MM-dd HH:mm") + "   " +
                                                   endTime.ToString("yyyy-MM-dd HH:mm");
                            }
                        }
                        else
                        {
                            ActivityDateTime = startTime.ToString("yyyy-MM-dd HH:mm") + "-" +
                                               endTime.ToString("HH:mm");
                        }
                        ParticipatorsCount = datas.voteCount ?? 0;
                        if (datas.activityStatus == 1)
                        {
                            if (!datas.voteFlag)
                            {
                                if (DateTime.Compare(startTime, serverDateTime) <= 0)
                                {
                                    ActivityStateContent = "已开始";
                                    IsParticipatedActivity = false;
                                }
                                else
                                {
                                    ActivityStateContent = "报名参加";
                                    IsParticipatedActivity = true;
                                }


                            }
                            else
                            {
                                ActivityStateContent = "已报名";
                                IsParticipatedActivity = false;
                            }
                        }
                        else
                        {
                            ActivityStateContent = "已结束";
                            IsParticipatedActivity = false;
                        }
                        LoadActivityParticipators();
                    }
                });
        }

        #region 属性

        private BitmapImage _activityThemePic;
        /// <summary>
        /// 活动主题图片
        /// </summary>
        public BitmapImage ActivityThemePic
        {
            get { return _activityThemePic; }
            set
            {
                _activityThemePic = value;
                RaisePropertyChanged(() => ActivityThemePic);
            }
        }

        private string _activityTitle;
        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle
        {
            get { return _activityTitle; }
            set
            {
                _activityTitle = value;
                RaisePropertyChanged(() => ActivityTitle);
            }
        }

        private string _activityDateTime;
        /// <summary>
        /// 活动时间
        /// </summary>
        public string ActivityDateTime
        {
            get { return _activityDateTime; }
            set
            {
                _activityDateTime = value;
                RaisePropertyChanged(() => ActivityDateTime);
            }
        }

        private string _activityAddress;
        /// <summary>
        /// 活动地点
        /// </summary>
        public string ActivityAddress
        {
            get { return _activityAddress; }
            set
            {
                _activityAddress = value;
                RaisePropertyChanged(() => ActivityAddress);
            }
        }

        private string _activityStateContent;
        /// <summary>
        /// 是否已参与显示内容
        /// </summary>
        public string ActivityStateContent
        {
            get { return _activityStateContent; }
            set
            {
                _activityStateContent = value;
                RaisePropertyChanged(() => ActivityStateContent);
            }
        }

        private bool _isParticipatedActivity;
        /// <summary>
        /// 是否已参与
        /// </summary>
        public bool IsParticipatedActivity
        {
            get { return _isParticipatedActivity; }
            set
            {
                _isParticipatedActivity = value;
                RaisePropertyChanged(() => IsParticipatedActivity);
            }
        }

        private string _activityIntroduce;
        /// <summary>
        /// 活动介绍
        /// </summary>
        public string ActivityIntroduce
        {
            get { return _activityIntroduce; }
            set
            {
                _activityIntroduce = value;
                RaisePropertyChanged(() => ActivityIntroduce);
            }
        }

        private bool _isActivityParticipators = true;
        /// <summary>
        /// 是否显示参与人员
        /// </summary>
        public bool IsActivityParticipators
        {
            get { return _isActivityParticipators; }
            set
            {
                _isActivityParticipators = value;
                RaisePropertyChanged(() => IsActivityParticipators);
            }
        }

        private ObservableCollection<ActivityParticipator> _activityParticipators = new ObservableCollection<ActivityParticipator>();
        /// <summary>
        /// 参与人员集合
        /// </summary>
        public ObservableCollection<ActivityParticipator> ActivityParticipators
        {
            get { return _activityParticipators; }
            set
            {
                _activityParticipators = value;
                RaisePropertyChanged(() => ActivityParticipators);
            }
        }

        private int _participatorsCount;
        /// <summary>
        /// 参与活动人数
        /// </summary>
        public int ParticipatorsCount
        {
            get { return _participatorsCount; }
            set
            {
                _participatorsCount = value;
                RaisePropertyChanged(() => ParticipatorsCount);
            }
        }

        #endregion
        #region 命令
        /// <summary>
        /// 报名参与活动
        /// </summary>
        public ICommand ParticipateActivitiesCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    try
                    {
                        var errCode = 0;
                        var errMsg = string.Empty;
                        AntSdkParticipateActivitiesInput input = new AntSdkParticipateActivitiesInput
                        {
                            userId = AntSdkService.AntSdkCurrentUserInfo.userId,
                            activityId = _activityId
                        };
                        var result = AntSdkService.ParticipateActivities(input, ref errCode, ref errMsg);
                        if (result)
                        {
                            ActivityStateContent = "已报名";
                            IsParticipatedActivity = false;
                            LoadActivityParticipators();

                        }
                        else
                        {
                            MessageBoxWindow.Show("提示", errMsg, MessageBoxButton.OK,
                                 GlobalVariable.WarnOrSuccess.Warn);
                            if (errCode == 9008)
                            {
                                ActivityStateContent = "已报名";
                                IsParticipatedActivity = false;
                                LoadActivityParticipators();
                            }
                            else if (errCode == 9006)
                            {
                                ActivityStateContent = "活动已开始";
                                IsParticipatedActivity = false;
                                LoadActivityParticipators();
                            }
                            else if (errCode == 9007)
                            {
                                CloseActivityEvent?.Invoke();
                            }
                            //AntSdkGetGroupActivityDetailsInput detailsInput = new AntSdkGetGroupActivityDetailsInput
                            //{
                            //    userId = AntSdkService.AntSdkCurrentUserInfo.userId,
                            //    activityId = _activityId,
                            //    groupId = _groupId
                            //};
                            //var output = AntSdkService.GetActivityInfo(detailsInput, ref errCode, ref errMsg);
                            //if (output == null)
                            //{

                            //}
                            //else
                            //{
                            //    MessageBoxWindow.Show("提示","活动报名失败，请重新报名", GlobalVariable.WarnOrSuccess.Warn);
                            //}

                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[ActivityDetailsViewModel_ParticipateActivitiesCommand]:" + ex.Message +
                                             ex.StackTrace + ex.Source);
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
                    CloseActivityEvent?.Invoke();
                });
            }
        }

        /// <summary>
        /// 地理位置
        /// </summary>
        public ICommand GetGeographicLocationCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    ActivityMapView view = new ActivityMapView(longitude, latitude, ActivityAddress);
                    view.ShowInTaskbar = false;
                    view.Owner = Antenna.Framework.Win32.GetTopWindow();
                    view.ShowDialog();
                });
            }
        }


        /// <summary>
        /// 查看活动参与者信息
        /// </summary>
        public ICommand ParticipatorInfoCommand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null)
                        return;
                    var userId = (string)obj;
                    try
                    {
                        Win_UserInfoView win = new Win_UserInfoView();
                        win.ShowInTaskbar = false;
                        Win_UserInfoViewModel model = new Win_UserInfoViewModel(userId);
                        win.DataContext = model;
                        win.Owner = Antenna.Framework.Win32.GetTopWindow();
                        win.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("[ActivityDetailsViewModel_ParticipatorCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                    }
                });
            }

        }

        #endregion
        #region 方法
        /// <summary>
        /// 加载活动参与者
        /// </summary>
        private void LoadActivityParticipators()
        {
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
                () =>
                {
                    var errCode = 0;
                    var errMsg = string.Empty;
                    AntSdkGetGroupActivityParticipatorInput participatorInput =
                        new AntSdkGetGroupActivityParticipatorInput
                        {
                            groupId = _groupId,
                            activityId = _activityId,
                            pageNum = pageNum,
                            pageSize = pageSize
                        };
                    try
                    {
                        return AntSdkService.GetGroupActivityParticipators(participatorInput, ref errCode,
                      ref errMsg);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("活动参与者接口异常:" + ex.Message + ex.Source +
                                           ex.StackTrace);
                        return null;

                    }

                },
                (ex, datas) =>
                {
                    if (datas == null)
                        return;
                    if (datas.list.Count > 0)
                    {
                        ActivityParticipators.Clear();
                        foreach (var participatorInfo in datas.list)
                        {
                            AntSdkContact_User contactUser =
                                AntSdkService.AntSdkListContactsEntity.users.Find(
                                    c => c.userId == participatorInfo.userId);
                            var activityParticipatorInfo = new ActivityParticipator();
                            if (contactUser != null)
                            {
                                activityParticipatorInfo.ParticipatorId = contactUser.userId;
                                activityParticipatorInfo.ParticipatorName = contactUser.userNum + contactUser.userName;
                                if (!string.IsNullOrWhiteSpace(contactUser.picture) &&
                                    publicMethod.IsUrlRegex(contactUser.picture))
                                {
                                    var userImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                        m => m.UserID == contactUser.userId);
                                    activityParticipatorInfo.ParticipatorHeadPic = string.IsNullOrEmpty(userImage?.Url)
                                        ? contactUser.picture
                                        : userImage.Url;
                                }
                                else
                                {
                                    activityParticipatorInfo.ParticipatorHeadPic =
                                        GlobalVariable.DefaultImage.UserHeadDefaultImage;
                                }
                            }
                            else
                            {
                                activityParticipatorInfo.ParticipatorId = participatorInfo.userId;
                                activityParticipatorInfo.ParticipatorHeadPic =
                                    !string.IsNullOrEmpty(participatorInfo.picture)
                                        ? participatorInfo.picture
                                        : GlobalVariable.DefaultImage.UserHeadDefaultImage;
                                activityParticipatorInfo.ParticipatorName = participatorInfo.userNum +
                                                                            participatorInfo.userName;
                            }
                            ActivityParticipators.Add(activityParticipatorInfo);
                        }
                        ParticipatorsCount = ActivityParticipators.Count;
                    }
                });
        }

        #endregion
    }

    public class ActivityParticipator
    {
        /// <summary>
        /// 参与者表示
        /// </summary>
        public string ParticipatorId { get; set; }
        /// <summary>
        /// 参与者头像
        /// </summary>
        public string ParticipatorHeadPic { get; set; }
        /// <summary>
        /// 参与者名称（工号+名称）
        /// </summary>
        public string ParticipatorName { get; set; }
    }
}
