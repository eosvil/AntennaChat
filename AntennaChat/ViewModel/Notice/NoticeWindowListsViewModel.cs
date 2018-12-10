using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using AntennaChat.Views.Notice;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Notice
{
    public class NoticeWindowListsViewModel : PropertyNotifyObject
    {
        string groupId = "";
        private bool _isAdminId;
        /// <summary>
        /// 点击创建公告时触发
        /// </summary>
        public event Action<bool> CreateNoticeEvent;
        public NoticeWindowListsViewModel(bool isAdminId, string groupId)
        {
            _isAdminId = isAdminId;
            if (!_isAdminId)
            {
                isShowBtnAddAttachment = "Collapsed";
            }
            //加载数据
            //inFindNoticeList inData = new inFindNoticeList();
            //inData.token = AntSdkService.AntSdkLoginOutput.token;
            //inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
            //inData.version = "";
            //inData.targetId = groupId;
            //ReturnNoticeList rDto = new ReturnNoticeList();
            var errorCode = 0;
            string errorMsg = "";
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var resultNotices = AntSdkService.GetGroupNotifications(AntSdkService.AntSdkCurrentUserInfo.userId, groupId, ref errorCode, ref errorMsg);
            //bool b = false;//new HttpService().SearchNoticeList(inData, ref rDto, ref errorMsg);
            if (resultNotices != null && resultNotices.Length > 0)
            {

                NoticeModel nAdd = null;
                foreach (var notice in resultNotices)
                {
                    AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == notice.createBy);
                    if (user != null) notice.createBy = user.userName;
                    if (!_isAdminId)
                    {
                        nAdd = new NoticeModel();
                        //id = Guid.NewGuid().ToString().Replace("-", "");
                        nAdd.NotificationId = notice.notificationId;

                        if (notice.hasAttach == "1")
                        {
                            isShowAttch = "Visible";
                            nAdd.IsAdjunctNotice = true;
                        }
                        else
                        {
                            isShowAttch = "Collapsed";
                            nAdd.IsAdjunctNotice = false;
                        }

                        nAdd.NoticeTitle = notice.title;
                        //nAdd.NoticeContent = notice.;

                        nAdd.Explain = notice.createBy + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(notice.createTime);
                        _noticeList.Add(nAdd);
                    }
                    else
                    {
                        nAdd = new NoticeModel();
                        if (notice.hasAttach == "1")
                        {
                            isShowAttch = "Visible";
                            nAdd.IsAdjunctNotice = true;
                        }
                        else
                        {
                            isShowAttch = "Collapsed";
                            nAdd.IsAdjunctNotice = false;
                        }

                        nAdd.NotificationId = notice.notificationId;
                        nAdd.IsbtnDeleteVisibility = true;
                        nAdd.NoticeTitle = notice.title;
                        //nAdd.NoticeContent = notice.content;
                        nAdd.Explain = notice.createBy + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(notice.createTime);
                        _noticeList.Add(nAdd);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(errorMsg))
                    Application.Current.Dispatcher.Invoke(new Action(() => MessageBoxWindow.Show("公告加载失败！", GlobalVariable.WarnOrSuccess.Warn)));
                else
                {
                    NoticeNoWindowViewModelVisibility = Visibility.Visible;
                }
            }
        }
        private string _isShowAttch = "Collapsed";
        public string isShowAttch
        {
            set
            {
                this._isShowAttch = value;
                RaisePropertyChanged(() => isShowAttch);
            }
            get { return this._isShowAttch; }
        }
        private string _isShowBtnAddAttachment;
        /// <summary>
        ///id
        /// </summary>
        public string isShowBtnAddAttachment
        {
            get { return this._isShowBtnAddAttachment; }
            set
            {
                this._isShowBtnAddAttachment = value;
                RaisePropertyChanged(() => isShowBtnAddAttachment);
            }
        }

        public ObservableCollection<NoticeModel> _noticeList = new ObservableCollection<NoticeModel>();
        /// <summary>
        /// 通知列表
        /// </summary>
        public ObservableCollection<NoticeModel> NoticeList
        {
            get { return this._noticeList; }
            set
            {
                this._noticeList = value;
                RaisePropertyChanged(() => NoticeList);
            }
        }

        /// <summary>
        /// 回退事件
        /// </summary>
        public ICommand btnCommandBackTalkMsg
        {
            get
            {
                return new DelegateCommand<UserControl>((obj) =>
                {
                    CreateNoticeEvent?.Invoke(false);
                });
            }
        }


        public Visibility _NoticeNoWindowViewModelVisibility = Visibility.Collapsed;
        /// <summary>
        /// 没有通知是否可见
        /// </summary>
        public Visibility NoticeNoWindowViewModelVisibility
        {
            get { return this._NoticeNoWindowViewModelVisibility; }
            set
            {
                this._NoticeNoWindowViewModelVisibility = value;
                RaisePropertyChanged(() => NoticeNoWindowViewModelVisibility);
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public ICommand btnOperate
        {
            get
            {
                return new DelegateCommand<string>((obj) =>
                {
                    try
                    {
                        if (
                            MessageBoxWindow.Show("提示", "是否要删除此条公告？", MessageBoxButton.YesNo,
                                GlobalVariable.WarnOrSuccess.Warn) == GlobalVariable.ShowDialogResult.Yes)
                        {
                            delateNoticeDto dNotice = new delateNoticeDto();
                            dNotice.token = AntSdkService.AntSdkLoginOutput.token;
                            dNotice.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
                            dNotice.version = "1.0";
                            dNotice.notificationId = obj.ToString();
                            baseNotice bn = null;
                            var notificationId = obj.ToString();
                            var errorCode = 0;
                            string errorMsg = "";
                            //TODO:AntSdk_Modify
                            //DONE:AntSdk_Modify
                            bool isResult = AntSdkService.DeleteNotificationsById(AntSdkService.AntSdkCurrentUserInfo.userId,
                                    notificationId, ref errorCode, ref errorMsg);
                            // bool result = false;// new HttpService().DeleteNotice(dNotice, ref bn, ref errorMsg, GlobalVariable.RequestMethod.DELETE);
                            if (isResult)
                            {

                                NoticeModel rDto =
                                    _noticeList.SingleOrDefault(m => m.NotificationId == obj.ToString().Trim());
                                _noticeList.Remove(rDto);
                                if (_noticeList.Count == 0)
                                {
                                    NoticeNoWindowViewModelVisibility = Visibility.Visible;
                                }

                            }
                            else
                            {
                                MessageBoxWindow.Show("提示", "公告删除失败，请稍后再试！", MessageBoxButton.OK,
                                    GlobalVariable.WarnOrSuccess.Warn);
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

        private ICommand _btnCommandAddNotice;
        /// <summary>
        /// 创建新公告命令
        /// </summary>
        public ICommand btnCommandAddNotice
        {
            get
            {
                if (this._btnCommandAddNotice == null)
                {
                    this._btnCommandAddNotice = new DelegateCommand(() =>
                    {
                        CreateNoticeEvent?.Invoke(true);
                    });
                }
                return this._btnCommandAddNotice;
            }
        }

        private NoticeModel _currentSelectedNoticeAddDto;
        /// <summary>
        /// 列表中当前选中公告项
        /// </summary>
        public NoticeModel CurrentSelectedNoticeAddDto
        {
            set
            {
                _currentSelectedNoticeAddDto = value;
                RaisePropertyChanged(() => CurrentSelectedNoticeAddDto);
            }
            get
            {
                return _currentSelectedNoticeAddDto;

            }

        }

        private ICommand _goNoticeDetailCommand;
        /// <summary>
        /// 公告详细信息
        /// </summary>
        public ICommand GoNoticeDetailCommand
        {
            get
            {
                _goNoticeDetailCommand = new DelegateCommand(() =>
                  {
                      if (_currentSelectedNoticeAddDto != null)
                          GoNoticeDetail(_currentSelectedNoticeAddDto);
                  });
                return _goNoticeDetailCommand;
            }
        }

        /// <summary>
        /// 打开公告详情
        /// </summary>
        private void GoNoticeDetail(NoticeModel currentSNoticeAddDto)
        {
            try
            {

                #region 旧代码

                //if (NoticeDetailsWindowViewModelVisibility == Visibility.Collapsed || NoticeDetailsWindowViewModelVisibility == Visibility.Hidden)
                //{
                //    delateNoticeDto inData = new delateNoticeDto();
                //    inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
                //    inData.version = "1.0";
                //    inData.token = AntSdkService.AntSdkLoginOutput.token;
                //    inData.notificationId = currentSNoticeAddDto.NotificationId;
                //    baseNotice bn = null;
                //    string errorMsg = "";
                //    NoticeAddDto preData = _noticeList.SingleOrDefault(m => m.NotificationId == currentSNoticeAddDto.NotificationId);
                //    if (preData.readStatusforeground == "1")
                //    {
                //        NoticeDetailsWindowViewModel = new NoticeDetailsWindowViewModel(this, currentSNoticeAddDto);
                //    }
                //    else
                //    {
                //        bool b = new HttpService().ReadNotice(inData, ref bn, ref errorMsg, GlobalVariable.RequestMethod.PATCH);
                //        if (b == true)
                //        {
                //            if (bn.result == 1)
                //            {
                //                NoticeDetailsWindowViewModel = new NoticeDetailsWindowViewModel(this, currentSNoticeAddDto);

                //                preData.isbtnShowVisibility = "1";
                //                preData.readStatusforeground = "1";
                //            }
                //        }
                //    }
                //}
                //if (NoticeDetailsWindowViewModelVisibility == Visibility.Hidden)
                //{
                //    NoticeDetailsWindowViewModelVisibility = Visibility.Collapsed;
                //}
                //NoticeDetailsWindowViewModelVisibility = NoticeDetailsWindowViewModelVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Hidden;

                #endregion

                //inFindDetailsNotice inData = new inFindDetailsNotice();
                //inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
                //inData.version = "1.0";
                //inData.notificationId = currentSNoticeAddDto.NotificationId;
                //inData.token = AntSdkService.AntSdkLoginOutput.token;
                //ReturnNoticeAddDto rData = null;
                var errorCode = 0;
                string errorMsg = "";
                //TODO:AntSdk_Modify
                //DONE:AntSdk_Modify
                //bool result = false;// new HttpService().SearchNoticeDetailsByNoticeId(inData, ref rData, ref errorMsg, GlobalVariable.RequestMethod.GET);
                var result = AntSdkService.GetNotificationsById(currentSNoticeAddDto.NotificationId, ref errorCode, ref errorMsg);
                if (result != null)
                {
                    NoticeReadViewModel readModel = new NoticeReadViewModel(result);
                    NoticeRead read = new NoticeRead();
                    read.DataContext = readModel;
                    read.Owner = Antenna.Framework.Win32.GetTopWindow();
                    read.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeWindowListsViewModel_ListBox_SelectionChanged:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
    }
}
