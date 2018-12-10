using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Antenna.Framework;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using AntennaChat.Views.Activity;

namespace AntennaChat.ViewModel.Activity
{
    public class ReleaseActivityViewModel : PropertyNotifyObject
    {
        public event Action ReleasedActivityEvent;
        public string _groupId;
        /// <summary>
        /// 经度
        /// </summary>
        private float _latitude;
        /// <summary>
        /// 纬度
        /// </summary>
        private float _longitude;
        public ReleaseActivityViewModel(string groupId)
        {
            _groupId = groupId;
            LoadActivityThemeData();
            ActivityDateTimeChange();
        }


        #region 属性

        public string ActivityTitle { get; set; }
        private List<ActivityThemePicInfo> _activityThemePicList = new List<ActivityThemePicInfo>();
        /// <summary>
        /// 活动主题图片集合
        /// </summary>
        public List<ActivityThemePicInfo> ActivityThemePicList
        {
            get { return _activityThemePicList; }
            set
            {
                _activityThemePicList = value;
                RaisePropertyChanged(() => ActivityThemePicList);
            }
        }

        private DateTime _activityStartDate = DateTime.Now;
        /// <summary>
        /// 活动开始日期
        /// </summary>
        public DateTime ActivityStartDate
        {
            get { return _activityStartDate; }
            set
            {
                _activityStartDate = value;
                if (DateTime.Compare(_activityStartDate.Date, DateTime.Now.Date) < 0)
                    _activityStartDate = DateTime.Now;
                var satrtDateTime = Convert.ToDateTime(_activityStartDate.Date.ToString("yyyy-MM-dd") + " " + _startHourSelected);
                var enDateTime = Convert.ToDateTime(ActivityEndDate.Date.ToString("yyyy-MM-dd") + " " + EndHourSelected);
                if (DateTime.Compare(enDateTime, satrtDateTime) <= 0)
                {
                    ActivityDateTimeError = "结束时间必须大于开始时间";
                    IsActivityDateTimeError = true;
                }
                else
                {
                    ActivityDateTimeError = "";
                    IsActivityDateTimeError = false;
                }
                RaisePropertyChanged(() => ActivityStartDate);
            }
        }

        private DateTime _activityEndDate = DateTime.Now;
        /// <summary>
        /// 活动结束日期
        /// </summary>
        public DateTime ActivityEndDate
        {
            get { return _activityEndDate; }
            set
            {
                _activityEndDate = value;
                if (DateTime.Compare(_activityEndDate.Date, DateTime.Now.Date) < 0)
                    _activityEndDate = DateTime.Now;
                DateTime satrtDateTime = Convert.ToDateTime(ActivityStartDate.Date.ToString("yyyy-MM-dd") + " " + _startHourSelected);
                DateTime enDateTime = Convert.ToDateTime(_activityEndDate.Date.ToString("yyyy-MM-dd") + " " + EndHourSelected);
                if (DateTime.Compare(enDateTime, satrtDateTime) < 0)
                {
                    ActivityDateTimeError = "结束时间必须大于开始时间";
                    IsActivityDateTimeError = true;
                }
                else
                {
                    ActivityDateTimeError = "";
                    IsActivityDateTimeError = false;
                }
                RaisePropertyChanged(() => ActivityEndDate);
            }
        }

        private string _startHourSelected;

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public string StartHourSelected
        {
            get { return _startHourSelected; }
            set
            {
                _startHourSelected = value;
                DateTime satrtDateTime = Convert.ToDateTime(ActivityStartDate.Date.ToString("yyyy-MM-dd") + " " + _startHourSelected);
                DateTime enDateTime = Convert.ToDateTime(_activityEndDate.Date.ToString("yyyy-MM-dd") + " " + EndHourSelected);
                if (DateTime.Compare(satrtDateTime, DateTime.Now.AddMinutes(30)) < 0)
                {
                    ActivityDateTimeError = "活动开始时间必须大于当前时间30分钟";
                    IsActivityDateTimeError = true;
                }
                else if (DateTime.Compare(enDateTime, satrtDateTime) < 0)
                {
                    ActivityDateTimeError = "结束时间必须大于开始时间";
                    IsActivityDateTimeError = true;
                }
                else
                {
                    ActivityDateTimeError = "";
                    IsActivityDateTimeError = false;
                }
                RaisePropertyChanged(() => StartHourSelected);
            }
        }

        private string _endHourSelected;

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public string EndHourSelected
        {
            get { return _endHourSelected; }
            set
            {
                _endHourSelected = value;
                DateTime satrtDateTime = Convert.ToDateTime(ActivityStartDate.Date.ToString("yyyy-MM-dd") + " " + StartHourSelected);
                DateTime enDateTime = Convert.ToDateTime(_activityEndDate.Date.ToString("yyyy-MM-dd") + " " + _endHourSelected);
                if (DateTime.Compare(enDateTime, satrtDateTime) < 0)
                {
                    ActivityDateTimeError = "结束时间必须大于开始时间";
                    IsActivityDateTimeError = true;
                }
                else
                {
                    ActivityDateTimeError = "";
                    IsActivityDateTimeError = false;
                }
                RaisePropertyChanged(() => EndHourSelected);
            }
        }

        private List<string> _activityRemindList=new List<string>();
        /// <summary>
        /// 活动提醒集合
        /// </summary>
        public List<string> ActivityRemindList
        {
            get { return _activityRemindList; }
            set
            {
                _activityRemindList = value;
                RaisePropertyChanged(() => ActivityRemindList);
            }
        }

        private string _activityRemindSelectedValue;
        /// <summary>
        /// 活动提醒时间
        /// </summary>
        public string ActivityRemindSelectedValue
        {
            get { return _activityRemindSelectedValue; }
            set
            {
                _activityRemindSelectedValue = value;
                RaisePropertyChanged(() => ActivityRemindSelectedValue);
            }
        }


        private string _activityAddress;
        /// <summary>
        /// 活动地址
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

        private string _activityIntroduceError;
        /// <summary>
        /// 活动介绍验证错误提示
        /// </summary>
        public string ActivityIntroduceError
        {
            get { return _activityIntroduceError; }
            set
            {
                _activityIntroduceError = value;
                RaisePropertyChanged(() => ActivityIntroduceError);
            }
        }
        public bool IsShowPrompt { get; set; }
        public string PromptInfo { get; set; }

        private string _activityDateTimeError;
        /// <summary>
        /// 活动值验证时间错误提示
        /// </summary>
        public string ActivityDateTimeError
        {
            get { return _activityDateTimeError; }
            set
            {
                _activityDateTimeError = value;
                RaisePropertyChanged(() => ActivityDateTimeError);
            }
        }

        private string _activityAddressTimeError;
        /// <summary>
        /// 活动值验证时间错误提示
        /// </summary>
        public string ActivityAddressError
        {
            get { return _activityAddressTimeError; }
            set
            {
                _activityAddressTimeError = value;
                RaisePropertyChanged(() => ActivityAddressError);
            }
        }
        private bool _isActivityDateTimeError;
        /// <summary>
        /// 活动值验证时间错误提示
        /// </summary>
        public bool IsActivityDateTimeError
        {
            get { return _isActivityDateTimeError; }
            set
            {
                _isActivityDateTimeError = value;
                RaisePropertyChanged(() => IsActivityDateTimeError);
            }
        }


        #endregion
        #region 命令

        public ICommand ReleaseActivityCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (string.IsNullOrEmpty(ActivityTitle))
                    {
                        return;
                    }
                    var errCode = 0;
                    var errMsg = string.Empty;
                    var satrtDateTime = Convert.ToDateTime(ActivityStartDate.Date.ToString("yyyy-MM-dd") + " " + StartHourSelected);
                    AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                    DateTime serverDateTime = DateTime.Now;
                    if (serverResult != null)
                    {
                        serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                    }
                    if (DateTime.Compare(satrtDateTime, serverDateTime.AddMinutes(30)) < 0)
                    {
                        ActivityDateTimeError = "活动开始时间必须大于当前时间30分钟";
                        IsActivityDateTimeError = true;
                        return;
                    }
                    var endDateTime =
                        Convert.ToDateTime(ActivityEndDate.ToString("yyyy-MM-dd") + " " + EndHourSelected);
                    if (DateTime.Compare(endDateTime, satrtDateTime) <= 0)
                    {
                        ActivityDateTimeError = "活动结束时间必须大于开始时间";
                        IsActivityDateTimeError = true;
                        return;
                    }

                    if (string.IsNullOrEmpty(ActivityAddress))
                    {
                        ActivityAddressError = "活动地址不能为空";
                        return;
                    }
                    if (string.IsNullOrEmpty(ActivityIntroduce))
                    {
                        ActivityIntroduceError = "活动介绍不能为空";
                        return;
                    }

                    var intput = new AntSdkReleaseGroupActivityInput
                    {
                        groupId = _groupId,
                        userId = AntSdkService.AntSdkCurrentUserInfo.userId,
                        address = ActivityAddress,
                        description = ActivityIntroduce,
                        theme = ActivityTitle,
                        startTime = ActivityStartDate.ToString("yyyy-MM-dd") + " " + StartHourSelected + ":00",
                        endTime = ActivityEndDate.ToString("yyyy-MM-dd") + " " + EndHourSelected + ":00"
                    };
                    var result = System.Text.RegularExpressions.Regex.Replace(ActivityRemindSelectedValue, @"[^0-9]+", "");
                    int remind;
                    int.TryParse(result, out remind);
                    intput.remindTime = remind != 0 ? remind : 30;
                    var activityTheme = ActivityThemePicList.FirstOrDefault(m => m.SelectedThemePic);
                    if (activityTheme != null)
                    {
                        if (activityTheme.PicId == 6)
                        {
                            try
                            {
                                AntSdkSendFileInput fileInput = new AntSdkSendFileInput();
                                fileInput.cmpcd = GlobalVariable.CompanyCode;
                                fileInput.seId =
                                    fileInput.file = activityTheme.ActivityThemePic;
                                AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                                failMessage.mtp = (int)AntSdkMsgType.ChatMsgPicture;
                                failMessage.content = "";
                                DateTime dt = DateTime.Now;
                                failMessage.lastDatetime = dt.ToString();
                                fileInput.FailOrSucess = failMessage;
                                var fileOutput = AntSdkService.FileUpload(fileInput, ref errCode, ref errMsg);
                                if (fileOutput != null)
                                {
                                    var picUrl = fileOutput.dowmnloadUrl;
                                    intput.picture = picUrl;
                                }
                                else
                                {
                                    MessageBoxWindow.Show("提示", "活动主题图片设置失败,请重新选择图片！", MessageBoxButton.OK,
                                        GlobalVariable.WarnOrSuccess.Warn);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteError("上传活动图片接口异常:" + ex.Message + ex.Source +
                                           ex.StackTrace);
                            }
                        }
                        else
                        {
                            intput.picture = activityTheme.ActivityThemePic;
                        }


                    }

                    intput.latitude = _latitude;
                    intput.longitude = _longitude;
                    try
                    {
                        var releaseResult = AntSdkService.ReleaseGroupActivity(intput, _groupId, ref errCode, ref errMsg);

                        if (releaseResult)
                            ReleasedActivityEvent?.Invoke();
                        else
                        {
                            GlobalVariable.ShowDialogResult dr = MessageBoxWindow.Show("发布活动接口异常" + ",是否继续发起活动？",
                            MessageBoxButton.YesNo, GlobalVariable.WarnOrSuccess.Warn);
                            if (dr.ToString() == "No")
                            {
                                ReleasedActivityEvent?.Invoke();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError("发布活动接口异常:" + ex.Message + ex.Source +
                                          ex.StackTrace);
                    }
                });
            }
        }

        public ICommand SelectLocalPicCommand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null)
                        return;
                    var picId = (int)obj;
                    var themePicInfo = ActivityThemePicList.FirstOrDefault(m => m.PicId == picId);
                    if (picId == 6)
                    {
                        System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
                        openFile.Filter = "图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png";
                        openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFile.FilterIndex = 0;
                        openFile.Multiselect = false;
                        if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            var fileName = openFile.FileName;
                            FileInfo fileInfo = new FileInfo(fileName);
                            var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                            if (size > 10)
                            {
                                MessageBoxWindow.Show("选择图片超过10M，请重新选择。", GlobalVariable.WarnOrSuccess.Warn);
                                return;
                            }
                            if (themePicInfo != null)
                            {
                                themePicInfo.ActivityThemePic = fileName;
                                BitmapImage customImage = new BitmapImage();

                                customImage.BeginInit();

                                customImage.UriSource = new System.Uri(fileName);

                                customImage.DecodePixelWidth = 800;

                                customImage.EndInit();
                                themePicInfo.ActivityPic = customImage;
                            }
                        }
                        else
                        {
                            return;
                        }

                    }

                    if (themePicInfo != null)
                        themePicInfo.SelectedThemePic = true;
                    var tempVoteOptionList = ActivityThemePicList.Where(m => m.PicId != picId);
                    foreach (var voteOption in tempVoteOptionList)
                    {
                        if (voteOption.SelectedThemePic)
                            voteOption.SelectedThemePic = false;

                    }



                });
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    ReleasedActivityEvent?.Invoke();
                });
            }
        }

        private ActivityMapView view;
        public ICommand MapCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {

                    view = new ActivityMapView(0, 0, "", true);
                    view.ShowInTaskbar = false;
                    view.Owner = Antenna.Framework.Win32.GetTopWindow();
                    view.Closed += View_Closed;
                    view.ShowDialog();
                });
            }
        }

        private void View_Closed(object sender, EventArgs e)
        {
            var info = view?.MapInfo;
            if (info == null) return;
            _latitude = info.Lat;
            _longitude = info.Lng;
            ActivityAddress = info.Geo;
            if (!string.IsNullOrEmpty(ActivityAddress) && !string.IsNullOrEmpty(ActivityAddressError))
                ActivityAddressError = "";
        }


        #endregion
        #region 方法
        /// <summary>
        /// 活动开始和结束时间初始化
        /// </summary>
        private void ActivityDateTimeChange()
        {
            var minute = DateTime.Now.Minute;
            if (minute < 60 && minute > 30)
            {
                //if (minute > 50 && minute < 60)
                //{
                _startHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":30";
                _endHourSelected = DateTime.Now.AddHours(2).ToString("HH") + ":00";
                //}
                //else
                //{
                //    _startHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":00";
                //    _endHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":30";
                //}

            }
            else
            {

                //if (minute > 20 && minute < 30)
                //{
                _startHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":00";
                _endHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":30";
                //}
                //else
                //{
                //    _startHourSelected = DateTime.Now.ToString("HH") + ":30";
                //    _endHourSelected = DateTime.Now.AddHours(1).ToString("HH") + ":00";
                //}

            }
        }
        /// <summary>
        /// 加载活动主题背景图
        /// </summary>
        private void LoadActivityThemeData()
        {
            var errCode = 0;
            string errMsg = string.Empty;
            //AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
            //    () =>
            //    {
            //        var output = AntSdkService.GetActivityImages(ref errCode, ref errMsg);
            //        return output;
            //    },
            //    (ex, datas) =>
            //    {
            var output = AntSdkService.GetActivityImages(ref errCode, ref errMsg);

            if (output != null)
            {
                if (!string.IsNullOrEmpty(output.outdoor))
                {
                    BitmapImage outdoorImage = new BitmapImage();

                    outdoorImage.BeginInit();

                    outdoorImage.UriSource = new System.Uri(output.outdoor);

                    outdoorImage.DecodePixelWidth = 800;

                    outdoorImage.EndInit();
                    _activityThemePicList.Add(new ActivityThemePicInfo
                    {
                        PicId = 1,
                        ActivityTheme = "户外",
                        ActivityThemePic = output.outdoor,
                        ActivityPic = outdoorImage,
                        SelectedThemePic = true
                    });
                }
                if (!string.IsNullOrEmpty(output.dinner))
                {
                    BitmapImage dinnerImage = new BitmapImage();

                    dinnerImage.BeginInit();

                    dinnerImage.UriSource = new System.Uri(output.dinner);

                    dinnerImage.DecodePixelWidth = 800;

                    dinnerImage.EndInit();
                    _activityThemePicList.Add(new ActivityThemePicInfo
                    {
                        PicId = 2,
                        ActivityTheme = "聚餐",
                        ActivityThemePic = output.dinner,
                        ActivityPic = dinnerImage,
                        SelectedThemePic = false
                    });
                }
                if (!string.IsNullOrEmpty(output.game))
                {
                    BitmapImage gameImage = new BitmapImage();

                    gameImage.BeginInit();

                    gameImage.UriSource = new System.Uri(output.game);

                    gameImage.DecodePixelWidth = 800;

                    gameImage.EndInit();
                    _activityThemePicList.Add(new ActivityThemePicInfo
                    {
                        PicId = 3,
                        ActivityTheme = "游戏",
                        ActivityThemePic = output.game,
                        ActivityPic = gameImage,
                        SelectedThemePic = false
                    });
                }

                if (!string.IsNullOrEmpty(output.sport))
                {
                    BitmapImage sportImage = new BitmapImage();

                    sportImage.BeginInit();

                    sportImage.UriSource = new System.Uri(output.sport);

                    sportImage.DecodePixelWidth = 800;

                    sportImage.EndInit();
                    _activityThemePicList.Add(new ActivityThemePicInfo
                    {
                        PicId = 4,
                        ActivityTheme = "体育",
                        ActivityThemePic = output.sport,
                        ActivityPic = sportImage,
                        SelectedThemePic = false
                    });
                }
                if (!string.IsNullOrEmpty(output.sing))
                {
                    BitmapImage singImage = new BitmapImage();

                    singImage.BeginInit();

                    singImage.UriSource = new System.Uri(output.sing);

                    singImage.DecodePixelWidth = 800;

                    singImage.EndInit();
                    _activityThemePicList.Add(new ActivityThemePicInfo
                    {
                        PicId = 5,
                        ActivityTheme = "唱K",
                        ActivityThemePic = output.sing,
                        ActivityPic = singImage,
                        SelectedThemePic = false
                    });
                }

                BitmapImage customImage = new BitmapImage();

                customImage.BeginInit();

                customImage.UriSource = new System.Uri("pack://application:,,,/AntennaChat;component/Images/addActivityThemePic.png");

                customImage.DecodePixelWidth = 800;

                customImage.EndInit();
                _activityThemePicList.Add(new ActivityThemePicInfo
                {
                    PicId = 6,
                    ActivityTheme = "",
                    ActivityThemePic = "/AntennaChat;component/Images/addActivityThemePic.png",
                    ActivityPic = customImage,
                    SelectedThemePic = false
                });
            }
            //});
            string xmlPath = Environment.CurrentDirectory + "/ActivityReminderTime.xml";
            List<string> modelList = new List<string>();

            if (File.Exists(xmlPath))
            {
                XmlTextReader reader = new XmlTextReader(xmlPath);

                while (reader.Read())
                {

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "value")
                        {
                            modelList.Add(reader.ReadElementString().Trim());
                        }
                    }
                }
            }
            if (modelList.Count > 0)
            {
                _activityRemindList = modelList;
            }
            else
            {
                _activityRemindList.Add("提前10分钟");
                _activityRemindList.Add("提前20分钟");
                _activityRemindList.Add("提前30分钟");
                _activityRemindList.Add("提前1小时");

            }
            _activityRemindSelectedValue = _activityRemindList.Count > 2 ? _activityRemindList[2] : "提前30分钟";
        }

        #endregion

    }

    public class ActivityThemePicInfo : PropertyNotifyObject
    {
        /// <summary>
        /// 图片标识
        /// </summary>
        public int PicId { set; get; }
        /// <summary>
        /// 活动图片主题
        /// </summary>
        public string ActivityTheme { set; get; }

        private string _activityThemePic;
        /// <summary>
        /// 活动图片地址
        /// </summary>
        public string ActivityThemePic
        {
            get { return _activityThemePic; }
            set
            {
                _activityThemePic = value;
                RaisePropertyChanged(() => ActivityThemePic);
            }
        }

        private BitmapImage _activityPic;
        /// <summary>
        /// 活动图片
        /// </summary>
        public BitmapImage ActivityPic
        {
            get { return _activityPic; }
            set
            {
                _activityPic = value;
                RaisePropertyChanged(() => ActivityPic);
            }
        }
        private bool _selectedThemePic;
        /// <summary>
        /// 是否选为主题背景图
        /// </summary>
        public bool SelectedThemePic
        {
            get { return _selectedThemePic; }
            set
            {
                _selectedThemePic = value;
                RaisePropertyChanged(() => SelectedThemePic);
            }
        }
    }
}
