using Antenna.Framework;
using AntennaChat.Command;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AntennaChat.ViewModel
{
    public class CheckInListViewModel : PropertyNotifyObject
    {
        private DateTime checkInDataTime = DateTime.Now.AddMinutes(-19);
        public DispatcherTimer timer;
        private AttendanceRecord todayAttendanceRecords = null;
        private ProcessCount processCount;
        private int _page = 0;
        private int _size = 10;
        private bool _isFirst;
        private bool _isLast;

        private bool _isWaitVerifty;
        public CheckInListViewModel()
        {
            //_isWaitVerifty = isWaitVerifty;
        }

        #region 属性
        private ObservableCollection<AttendanceRecord> _attendanceRecords = new ObservableCollection<AttendanceRecord>();
        public ObservableCollection<AttendanceRecord> AttendanceRecords
        {
            get { return _attendanceRecords; }
            set
            {
                _attendanceRecords = value;
                RaisePropertyChanged(() => AttendanceRecords);
            }
        }
        #endregion
        #region 命令
        CheckInVerifyViewModel verifyVm;
        public ICommand VerifyCommand
        {
            get
            {
                return new DefaultCommand(obj =>
                {
                    if (obj == null || MainWindowViewModel.VerifyView != null)
                        return;
                    var attendId = (string)obj;
                    var tempAttendanceRescords = AttendanceRecords.FirstOrDefault(m => m.AttendId == attendId);
                    if (tempAttendanceRescords != null)
                    {
                        var content = new AntSdkReceivedOtherMsg.AttendanceRecord_content
                        {
                            address = tempAttendanceRescords.PuncherAddress,
                            attendId = tempAttendanceRescords.AttendId,
                            attendTime = tempAttendanceRescords.PuncherTimeStamp,
                        };
                        //MainWindowViewModel.CloseExitVerify();
                        if (verifyVm != null)
                            verifyVm.timer?.Stop();
                        var errCode = 0;
                        var errMsg = string.Empty;
                        AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                        DateTime serverDateTime = DateTime.Now;
                        if (serverResult != null)
                        {
                            serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                        }
                        var diff = serverDateTime - GlobalVariable.LastLoginDatetime;
                        var isPassword = diff.TotalMinutes > 5;
                        MainWindowViewModel.VerifyView = new CheckInVerifyView(isPassword);
                        verifyVm = new CheckInVerifyViewModel(content, null, isPassword);
                        verifyVm.VerifySucceed += VerifyVm_VerifySucceed;
                        MainWindowViewModel.VerifyView.DataContext = verifyVm;
                        verifyVm.VerifyView = MainWindowViewModel.VerifyView;
                        MainWindowViewModel.VerifyView.Topmost = true;
                        MainWindowViewModel.VerifyView.Show();
                    }

                });
            }
        }
        /// <summary>
        /// 打卡验证成功
        /// </summary>
        private void VerifyVm_VerifySucceed()
        {
            //VerifyResultView view = new Views.VerifyResultView();
            //view.Topmost = true;
            //view.Show();
            todayAttendanceRecords.IsbtnVerify = false;
            todayAttendanceRecords.VerifyDescribe = "打卡成功";
            todayAttendanceRecords.VerifyState = 2;
            timer?.Stop();
        }
        #endregion
        #region 方法
        /// <summary>
        /// 获取服务器考勤记录
        /// </summary>
        public void GetServicePunchClocksData(bool isNewRecord = false)
        {
            if (isNewRecord)
            {
                _isLast = false;
                
            }
            if (_isLast) return;
            AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher,
               () =>
               {
                   if (isNewRecord)
                   {
                       timer?.Stop();
                       timer = null;
                       _page = 0;
                       todayAttendanceRecords = null;
                   }
                   _page++;
                   var errorCode = 0;
                   var errorMsg = string.Empty;
                   var punchClocksOutput = AntSdkService.GetPunchClocks(AntSdkService.AntSdkLoginOutput.userId, _page, _size, ref errorCode, ref errorMsg);
                   return punchClocksOutput;
               },
                (ex, datas) =>
                {
                    if (isNewRecord)
                        AttendanceRecords.Clear();
                    if (datas != null && datas.list.Count > 0)
                    {

                        var serverDateTime = PublicTalkMothed.ConvertStringToDateTime(datas.systemTime);
                        var punchClocksOutputList = datas.list.OrderByDescending(m => m.attendId);
                        foreach (var punchClocks in datas.list)
                        {
                            var exitAttendanceRecord = AttendanceRecords.FirstOrDefault(m => m.AttendId == punchClocks.attendId);
                            if (exitAttendanceRecord != null) continue;
                            var attendanceRecord = new AttendanceRecord();
                            attendanceRecord.AttendId = punchClocks.attendId;
                            attendanceRecord.VerifyState = punchClocks.status;
                            attendanceRecord.PuncherTimeStamp = punchClocks.attendTime;

                            if (attendanceRecord.VerifyState == 0)
                            {
                                attendanceRecord.IsbtnVerify = true;
                                attendanceRecord.VerifyDescribe = "待验证";
                            }
                            else if (attendanceRecord.VerifyState == 1)
                            {
                                attendanceRecord.VerifyDescribe = "验证过期";
                            }
                            else if (attendanceRecord.VerifyState == 2)
                            {
                                attendanceRecord.VerifyDescribe = "打卡成功";
                            }
                            var diffMinute = serverDateTime - PublicTalkMothed.ConvertStringToDateTime(punchClocks.attendTime);
                            if ((diffMinute.Days > 0 || diffMinute.TotalMinutes > 20) && attendanceRecord.VerifyState == 0)
                            {
                                attendanceRecord.IsbtnVerify = false;
                                attendanceRecord.VerifyDescribe = "验证过期";
                                attendanceRecord.VerifyState = 1;
                            }
                            if (!string.IsNullOrEmpty(punchClocks.attendTime))
                                attendanceRecord.PuncherDateTime = PublicTalkMothed.ConvertStringToDateTime(punchClocks.attendTime);
                            else
                                attendanceRecord.PuncherDateTime = DateTime.Now;
                            attendanceRecord.PuncherAddress = punchClocks.address;
                            if (attendanceRecord.PuncherDateTime.Year - DateTime.Now.Year > 0)
                                attendanceRecord.PuncherDate = attendanceRecord.PuncherDateTime.ToString("yyyy年MM月dd日");
                            else
                                attendanceRecord.PuncherDate = attendanceRecord.PuncherDateTime.ToString("MM月dd日");
                            attendanceRecord.PuncherTime = attendanceRecord.PuncherDateTime.ToString("HH:mm");
                            AttendanceRecords.Insert(0, attendanceRecord);
                        }

                        _isFirst = datas.isFirstPage;
                        _isLast = datas.isLastPage;
                        var tempAttendanceRecord = AttendanceRecords.FirstOrDefault(m => m.IsbtnVerify);
                        if (tempAttendanceRecord != null)
                            LoadData(tempAttendanceRecord);
                    }
                    else
                    {

                    }
                });
        }
        private void LoadData(AttendanceRecord attendanceRecord)
        {
            var errCode = 0;
            var errMsg = string.Empty;
            todayAttendanceRecords = attendanceRecord;
            if (todayAttendanceRecords == null) return;
            checkInDataTime = todayAttendanceRecords.PuncherDateTime;
            AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
            DateTime serverDateTime = DateTime.Now;
            if (serverResult != null)
            {
                serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
            }
            var diffMinute = serverDateTime - checkInDataTime;
            if (checkInDataTime.ToShortDateString() != serverDateTime.ToShortDateString())
                return;
            if (diffMinute.Hours > 6)
                return;
            if (diffMinute.Days > 0 || diffMinute.TotalMinutes > 20 || !todayAttendanceRecords.IsbtnVerify)
                return;
            //设置定时器
            timer?.Stop();
            timer = null;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 1);   //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
            var minuteChangeSecond = 0d;
            if (diffMinute.TotalMinutes < 0)
                minuteChangeSecond = (20 - 0.01) * 60;
            else
            {
                minuteChangeSecond = (20 - diffMinute.TotalMinutes) * 60;
            }

            //处理倒计时的类
            processCount = new ProcessCount((int)minuteChangeSecond);
            //Application.Current.Dispatcher.Invoke((Action)(() =>
            //{
            todayAttendanceRecords.ChcekInTimer = "（" + processCount.GetMinute() + "分" + processCount.GetSecond() + "秒" + "）";
            //}));
            CountDown += new CountDownHandler(processCount.ProcessCountDown);

            //开启定时器
            timer.Start();
        }

        /// <summary>
        /// Timer触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (OnCountDown())
            {
                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //{
                if (todayAttendanceRecords != null)
                    todayAttendanceRecords.ChcekInTimer = "（" + processCount.GetMinute() + "分" + processCount.GetSecond() + "秒" + "）";
                ThreadPool.QueueUserWorkItem(m =>
                {
                    if (processCount.timerTotalSecond >= 20)
                    {
                        var errCode = 0;
                        var errMsg = string.Empty;
                        AntSdkQuerySystemDateOuput serverResult = AntSdkService.AntSdkGetCurrentSysTime(ref errCode, ref errMsg);
                        DateTime serverDateTime = DateTime.Now;
                        if (serverResult != null)
                        {
                            serverDateTime = PublicTalkMothed.ConvertStringToDateTime(serverResult.systemCurrentTime);
                        }
                        var diffMinute = serverDateTime - checkInDataTime;
                        if (checkInDataTime.ToShortDateString() != serverDateTime.ToShortDateString())
                            return;
                        if (diffMinute.Days > 0 || diffMinute.TotalMinutes > 20)
                            return;
                        var minuteChangeSecond = (20 - diffMinute.TotalMinutes) * 60;
                        processCount.ResetTotalSecond((int)minuteChangeSecond);
                    }
                });
                //}));
            }
            else
            {
                todayAttendanceRecords.IsbtnVerify = false;
                todayAttendanceRecords.VerifyDescribe = "验证过期";
                todayAttendanceRecords.VerifyState = 1;
                timer.Stop();
            }
        }
        /// <summary>
        /// 处理事件
        /// </summary>
        public event CountDownHandler CountDown;
        public bool OnCountDown()
        {
            if (CountDown != null)
                return CountDown();
            return false;
        }
        #endregion

    }

    public class AttendanceRecord : PropertyNotifyObject
    {
        /// <summary>
        /// 考勤ID
        /// </summary>
        public string AttendId { get; set; }

        private int _verifyState;
        /// <summary>
        /// 打卡状态0:待验证1:验证失败2:成功
        /// </summary>
        public int VerifyState
        {
            get { return _verifyState; }
            set
            {
                _verifyState = value;
                RaisePropertyChanged(() => VerifyState);
            }
        }

        private string _verifyDescribe = string.Empty;
        /// <summary>
        /// 验证描述
        /// </summary>
        public string VerifyDescribe
        {
            get { return _verifyDescribe; }
            set
            {
                _verifyDescribe = value;
                RaisePropertyChanged(() => VerifyDescribe);
            }
        }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public string PuncherTime { get; set; } = string.Empty;
        /// <summary>
        /// 打卡日期
        /// </summary>
        public string PuncherDate { get; set; } = string.Empty;
        /// <summary>
        /// 打卡地址
        /// </summary>
        public string PuncherAddress { get; set; } = string.Empty;
        /// <summary>
        /// 打卡时间戳
        /// </summary>
        public string PuncherTimeStamp { get; set; }

        public DateTime PuncherDateTime { get; set; }

        private string _chcekInTimer = string.Empty;
        /// <summary>
        /// 待打卡倒计时
        /// </summary>
        public string ChcekInTimer
        {
            get { return _chcekInTimer; }
            set
            {
                _chcekInTimer = value;
                RaisePropertyChanged(() => ChcekInTimer);
            }
        }
        private bool _isbtnVerify;
        /// <summary>
        /// 是否要验证
        /// </summary>
        public bool IsbtnVerify
        {
            get { return _isbtnVerify; }
            set
            {
                _isbtnVerify = value;
                RaisePropertyChanged(() => IsbtnVerify);
            }
        }

    }
}
