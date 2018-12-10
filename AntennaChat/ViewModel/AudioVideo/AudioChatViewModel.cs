using Antenna.Framework;
using AntennaChat.Views;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AntennaChat.ViewModel.Talk;
using NIM;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using AntennaChat.Views.AudioVideo;
using AntennaChat.ViewModel.Contacts;
using SDK.AntSdk.AntSdkDAL;
using System.Threading;
using System.Windows.Threading;

namespace AntennaChat.ViewModel.AudioVideo
{
    /// <summary>
    /// 语音电话
    /// </summary>
    public class AudioChatViewModel : WindowBaseViewModel
    {
        /// <summary>
        /// 目标ID
        /// </summary>
        public AntSdkContact_User _targetUser;
        private Action _close;
        private C_User_InfoDAL _userInfoDal;
        private int _currentVolunm = 120;//默认耳机音量
        public bool IsTalking = false;//是否通话中
        public string sessionId = string.Empty;//会话ID
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="user">目标</param>
        /// <param name="sendOrReceive">发送方or接收方</param>
        public AudioChatViewModel(AntSdkContact_User user, GlobalVariable.AudioSendOrReceive sendOrReceive,Action close)
        {
            _targetUser = user;
            _close = close;
            InitAudio(sendOrReceive);
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        public void Window_Closing(object sender, CancelEventArgs e)
        {
            GlobalVariable.isAudioShow = false;
            GlobalVariable.currentAudioUserId = string.Empty;
        }

        #region 属性

        /// <summary>
        /// 标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }
        /// <summary>
        /// 头像
        /// </summary>
        private string _photo;
        public string Photo
        {
            get { return this._photo; }
            set
            {
                this._photo = value;
                RaisePropertyChanged(() => Photo);
            }
        }
        /// <summary>
        /// 通话时间
        /// </summary>
        private string _talkTime;
        public string TalkTime
        {
            get { return this._talkTime; }
            set
            {
                this._talkTime = value;
                RaisePropertyChanged(() => TalkTime);
            }
        }
        /// <summary>
        /// 接听按钮是否显示
        /// </summary>
        private Visibility _acceptVisibility = Visibility.Collapsed;

        public Visibility AcceptVisibility
        {
            get { return _acceptVisibility; }
            set
            {
                _acceptVisibility = value;
                RaisePropertyChanged(() => AcceptVisibility);
            }
        }

        /// <summary>
        /// 语音请求中页面是否显示
        /// </summary>
        private Visibility _askStackPanelVisibility = Visibility.Collapsed;

        public Visibility AskStackPanelVisibility
        {
            get { return _askStackPanelVisibility; }
            set
            {
                _askStackPanelVisibility = value;
                RaisePropertyChanged(() => AskStackPanelVisibility);
            }
        }

        /// <summary>
        /// 语音电话中页面是否显示
        /// </summary>
        private Visibility _talkingStackPanelVisibility = Visibility.Collapsed;

        public Visibility TalkingStackPanelVisibility
        {
            get { return _talkingStackPanelVisibility; }
            set
            {
                _talkingStackPanelVisibility = value;
                RaisePropertyChanged(() => TalkingStackPanelVisibility);
            }
        }

        /// <summary>
        /// 静音-关是否显示
        /// </summary>
        private Visibility _muteCloseVisibility = Visibility.Collapsed;

        public Visibility MuteCloseVisibility
        {
            get { return _muteCloseVisibility; }
            set
            {
                _muteCloseVisibility = value;
                RaisePropertyChanged(() => MuteCloseVisibility);
            }
        }

        /// <summary>
        /// 静音-开是否显示
        /// </summary>
        private Visibility _muteOpenVisibility = Visibility.Visible;

        public Visibility MuteOpenVisibility
        {
            get { return _muteOpenVisibility; }
            set
            {
                _muteOpenVisibility = value;
                RaisePropertyChanged(() => MuteOpenVisibility);
            }
        }
        /// <summary>
        /// 调节音量页面是否显示
        /// </summary>
        private bool _opAudioVolumnOpen;
        public bool PopAudioVolumnOpen
        {
            get { return this._opAudioVolumnOpen; }
            set
            {
                this._opAudioVolumnOpen = value;
                RaisePropertyChanged(() => PopAudioVolumnOpen);
            }
        }
        /// <summary>
        /// 耳机静音-开是否显示
        /// </summary>
        private Visibility _audioPlayMuteOpenVisibility = Visibility.Visible;

        public Visibility AudioPlayMuteOpenVisibility
        {
            get { return _audioPlayMuteOpenVisibility; }
            set
            {
                _audioPlayMuteOpenVisibility = value;
                RaisePropertyChanged(() => AudioPlayMuteOpenVisibility);
            }
        }
        /// <summary>
        /// 耳机静音-关是否显示
        /// </summary>
        private Visibility _audioPlayMuteCloseVisibility = Visibility.Collapsed;

        public Visibility AudioPlayMuteCloseVisibility
        {
            get { return _audioPlayMuteCloseVisibility; }
            set
            {
                _audioPlayMuteCloseVisibility = value;
                RaisePropertyChanged(() => AudioPlayMuteCloseVisibility);
            }
        }
        #endregion

        #region Command
        /// <summary>
        /// 取消语音请求
        /// </summary>
        public ICommand HangUpRequestCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    AudioChat.End();
                    CloseAudio();
                });
            }
        }
        /// <summary>
        /// 挂断语音通话
        /// </summary>
        public ICommand HangUpTalkingCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    AudioChat.End();
                    CloseAudio();
                });
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public ICommand CloseAudioView
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {//关闭语音
                    if (
                        MessageBoxWindow.Show("提示", "关闭窗口会结束语音通话，是否确认关闭？", MessageBoxButton.YesNo,
                            GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Yes)
                    {
                        AudioChat.End();
                        CloseAudio();
                    }
                });
            }
        }
        /// <summary>
        /// 打开调节音量页面
        /// </summary>
        public ICommand AudioVolumnCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    PopAudioVolumnOpen = true;
                }); 
            }
        }
        /// <summary>
        /// 调节音量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var volumnSlider = sender as System.Windows.Controls.Slider;
            if (volumnSlider == null) return;
            var value = Convert.ToInt32(volumnSlider.Value);
            AudioChat.SetAudioPlayVolumn(value);
            _currentVolunm = value;
        }
        /// <summary>
        /// 静音-开
        /// </summary>
        public ICommand MuteOpenCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    MuteOpenVisibility=Visibility.Collapsed;
                    MuteCloseVisibility=Visibility.Visible;
                    AudioChat.SetAudioCaptureVolumn(0);
                });
            }
        }
        /// <summary>
        /// 静音-关
        /// </summary>
        public ICommand MuteCloseCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {

                    MuteOpenVisibility = Visibility.Visible;
                    MuteCloseVisibility = Visibility.Collapsed;
                    AudioChat.SetAudioCaptureVolumn(255);
                });
            }
        }
        /// <summary>
        /// 耳机静音-开
        /// </summary>
        public ICommand AudioPlayMuteOpenCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    AudioPlayMuteOpenVisibility = Visibility.Collapsed;
                    AudioPlayMuteCloseVisibility = Visibility.Visible;
                    AudioChat.SetAudioPlayVolumn(0);
                });
            }
        }
        /// <summary>
        /// 耳机静音-关
        /// </summary>
        public ICommand AudioPlayMuteCloseCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {

                    AudioPlayMuteOpenVisibility = Visibility.Visible;
                    AudioPlayMuteCloseVisibility = Visibility.Collapsed;
                    AudioChat.SetAudioPlayVolumn(_currentVolunm);
                });
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sendOrReceive"></param>
        private void InitAudio(GlobalVariable.AudioSendOrReceive sendOrReceive)
        {
            if (sendOrReceive == GlobalVariable.AudioSendOrReceive.Send)
            {
                Title = $"正在呼叫{_targetUser.userName}...";
                AcceptVisibility = Visibility.Collapsed;
                SetStanckPanel(true);
                var targetAccid = _targetUser.accid;
                if (string.IsNullOrEmpty(targetAccid))
                {
                    QueryUserInfo(_targetUser.userId);
                }
                AudioChat.Start(_targetUser.accid, NIMVideoChatMode.kNIMVideoChatModeAudio);
                //构造一条语音电话消息
                AudioChat.targetUid = _targetUser.userId;
            }
            else
            {
                Title = $"正在与{_targetUser.userName}通话中...";
                Countdown();
                SetStanckPanel(false);
            }
            SetContactPhoto();
            AudioChat.SetAudioPlayVolumn(120);
        }
        /// <summary>
        /// 设置通话请求中页面跟通话中页面的显示
        /// </summary>
        /// <param name="isShowAskStanckPanel"></param>
        public void SetStanckPanel(bool isShowAskStanckPanel)
        {
            //请求中
            if (isShowAskStanckPanel)
            {
                AskStackPanelVisibility = Visibility.Visible;
                TalkingStackPanelVisibility = Visibility.Collapsed;
            }
            //通话中
            else
            {
                AskStackPanelVisibility = Visibility.Collapsed;
                TalkingStackPanelVisibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void CloseAudio()
        {
            this._close.Invoke();
        }
        /// <summary>
        /// 修改通话页面信息
        /// </summary>
        public void UpdateAudioInfo()
        {
            Title = $"正在与{_targetUser.userName}通话中...";
            Countdown();
        }
        /// <summary>
        /// 设置联系人头像
        /// </summary>
        private void SetContactPhoto()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_targetUser?.picture) && publicMethod.IsUrlRegex(_targetUser.picture))
                {
                    if (publicMethod.IsUrlRegex(_targetUser.picture))
                    {
                        var userImage =
                            GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(
                                m => m.UserID == _targetUser.userId);
                        this.Photo = string.IsNullOrEmpty(userImage?.Url) ? _targetUser.picture : userImage.Url;
                    }
                    else
                    {
                        this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                    }
                }
                else
                {
                    this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
                }
            }
            catch (Exception)
            {
                this.Photo = GlobalVariable.DefaultImage.UserHeadDefaultImage;
            }
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="id"></param>
        private void QueryUserInfo(string id)
        {
            var info = GroupPublicFunction.QueryUserInfo(id);
            _targetUser.accid = info.accid;
            ThreadPool.QueueUserWorkItem(m => UpdateAccid());
        }
        /// <summary>
        /// 更新本地库Accid
        /// </summary>
        private void UpdateAccid()
        {
            if(_userInfoDal==null)
                _userInfoDal=new C_User_InfoDAL();
            _userInfoDal.Update(_targetUser);
        }
        #region 计时处理
        public int _totalSeconds = 0;//总计时
        private DispatcherTimer _countdownTimer = null;

        private void Countdown()
        {
            _totalSeconds = 0;
            _countdownTimer?.Stop();
            _countdownTimer = null;
            _countdownTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            _countdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            IsTalking = true;
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _totalSeconds++;
            TalkTime = DataConverter.FormatTimeHourBySeconds(_totalSeconds);
        }
        #endregion
        #endregion
    }
}
