using Antenna.Framework;
using Antenna.Model;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Notice
{
    public class NoticeReadViewModel : PropertyNotifyObject
    {
        List<Notice_content> noticeList;
        public NoticeReadViewModel()
        {

        }
        /// <summary>
        /// 加载公告详情数据
        /// </summary>
        /// <param name="lists"></param>
        public void LoadNoticeData(List<Notice_content> lists)
        {

            this.noticeList = lists;
            if (noticeList == null || noticeList.Count == 0) return;
            var errorCode = 0;
            string errorMsg = string.Empty;
            var result = AntSdkService.GetNotificationsById(noticeList[0].notificationId, ref errorCode, ref errorMsg);
            if (result != null)
                txtContent = result.content;
            txtTitles = noticeList[0].title;
            NoticeDetailWinHeight = 300;
            AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == lists[0].createId);
            if (user != null) createBy = user.userName;
            //createBy = lists[0].createBy;
            IsReadStutes(noticeList[0].notificationId, noticeList[0].targetId);
            if (lists[0].hasAttach == 1)
            {
                SetGridStatus(true);
                UpdateAttachment(noticeList[0].notificationId);
                //isShowAttch = Visibility.Visible;
            }
            else
            {
                SetGridStatus(false);
                //isShowAttch = Visibility.Collapsed;
            }
            btnRemarkText = createBy + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(noticeList[0].createTime);
            if (noticeList.Count() == 1)
            {
                noticeCount = "我知道了";
            }
            else
            {
                noticeCount = "下一条(" + Convert.ToInt32(noticeList.Count() - 1) + ")";
            }
        }

        /// <summary>
        /// 公告列表查看时需调用的构造函数
        /// </summary>
        public NoticeReadViewModel(AntSdkGetNotificationsByIdOutput rData)
        {
            txtTitles = rData.title;
            txtContent = rData.content;
            noticeCount = "我知道了";
            AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == rData.createBy);
            if (user != null) rData.createBy = user.userName;
            btnRemarkText = rData.createBy + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(rData.createTime);
            if (rData.attach != null)
            {
                UpdateAttachment(rData.notificationId);
            }
        }

        private ObservableCollection<AttachControlViewModel> _AttachControlViewModelList = new ObservableCollection<AttachControlViewModel>();
        public ObservableCollection<AttachControlViewModel> AttachControlViewModelList
        {
            get { return this._AttachControlViewModelList; }
            set
            {
                this._AttachControlViewModelList = value;
                RaisePropertyChanged(() => AttachControlViewModelList);
            }
        }
        private Visibility _isShowAttch = Visibility.Collapsed;
        public Visibility isShowAttch
        {
            set
            {
                this._isShowAttch = value;
                RaisePropertyChanged(() => isShowAttch);
            }
            get { return this._isShowAttch; }
        }
        private int _hasAttach;
        public int hasAttach
        {
            set
            {
                this._hasAttach = value;
                RaisePropertyChanged(() => hasAttach);
            }
            get { return this._hasAttach; }
        }
        private string _createBy;
        public string createBy
        {
            set
            {
                this._createBy = value;
                RaisePropertyChanged(() => createBy);
            }
            get { return this._createBy; }
        }
        private string _txtTitles;
        public string txtTitles
        {
            set
            {
                this._txtTitles = value;
                RaisePropertyChanged(() => txtTitles);
            }
            get { return this._txtTitles; }
        }
        private string _txtContent;
        public string txtContent
        {
            set
            {
                this._txtContent = value;
                RaisePropertyChanged(() => txtContent);
            }
            get { return this._txtContent; }
        }
        private string _btnRemarkText;
        public string btnRemarkText
        {
            set
            {
                this._btnRemarkText = value;
                RaisePropertyChanged(() => btnRemarkText);
            }
            get { return this._btnRemarkText; }
        }
        private string _noticeCount;
        public string noticeCount
        {
            set
            {
                this._noticeCount = value;
                RaisePropertyChanged(() => noticeCount);
            }
            get { return this._noticeCount; }
        }

        private double _noticeDetailWinHeight = 300;
        /// <summary>
        /// 公告详情界面高度
        /// </summary>
        public double NoticeDetailWinHeight
        {
            get { return _noticeDetailWinHeight; }
            set
            {
                _noticeDetailWinHeight = value;
                RaisePropertyChanged(() => NoticeDetailWinHeight);
            }
        }

        private int _AttachGridHeight = 90;
        public int AttachGridHeight
        {
            set
            {
                this._AttachGridHeight = value;
                RaisePropertyChanged(() => AttachGridHeight);
            }
            get { return this._AttachGridHeight; }
        }
        private int _TextHeight = 130;
        public int TextHeight
        {
            set
            {
                this._TextHeight = value;
                RaisePropertyChanged(() => TextHeight);
            }
            get { return this._TextHeight; }
        }
        int index = 0;
        /// <summary>
        /// 下一条命令
        /// </summary>
        public ICommand btnNextCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (noticeList == null)
                    {
                        notice?.Close();
                        return;
                    }
                    if (index == noticeList.Count() - 1)
                    {
                        notice?.Close();
                    }
                    else
                    {
                        index++;
                        Notice_content notice = noticeList[index];
                        int preindex = noticeList.Count() - index;
                        if (preindex == 0)
                        {
                            noticeCount = "我知道了";
                        }
                        else
                        {
                            var errorCode = 0;
                            string errorMsg = string.Empty;
                            var result = AntSdkService.GetNotificationsById(noticeList[0].notificationId, ref errorCode, ref errorMsg);
                            if (result != null)
                                txtContent = result.content;
                            txtTitles = notice.title;
                            if (notice.hasAttach == 1)
                            {
                                SetGridStatus(true);
                                UpdateAttachment(notice.notificationId);
                                //isShowAttch = Visibility.Visible;//"Visible";
                            }
                            else
                            {
                                SetGridStatus(false);
                                //isShowAttch = Visibility.Collapsed;//"Collapsed";
                            }
                            AntSdkContact_User user = AntSdkService.AntSdkListContactsEntity.users.Find(c => c.userId == notice.createId);
                            btnRemarkText = (user == null ? "" : user.userName) + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(notice.createTime);
                            noticeCount = "下一条(" + (preindex - 1) + ")";
                            if (preindex == 1)
                            {
                                noticeCount = "我知道了";
                            }
                        }
                        IsReadStutes(notice.notificationId, notice.targetId);
                    }

                });
            }
        }
        /// <summary>
        /// 更改状态
        /// </summary>
        /// <param name="noticeId"></param>
        public void IsReadStutes(string noticeId, string targetId)
        {
            delateNoticeDto inData = new delateNoticeDto();
            inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
            inData.version = "1.0";
            inData.token = AntSdkService.AntSdkLoginOutput.token;
            inData.notificationId = noticeId;
            inData.targetId = targetId;
            baseNotice bn = null;
            var errorCode = 0;
            string errorMsg = "";
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var isResult = AntSdkService.UpdateNotificationsState(AntSdkService.AntSdkCurrentUserInfo.userId, targetId, noticeId, ref errorCode, ref errorMsg);
            bool b = false;// new HttpService().ReadNotice(inData, ref bn, ref errorMsg, GlobalVariable.RequestMethod.PATCH);
            if (isResult)
            {
                //if (bn.result == 1)
                //{

                //}
            }
        }
        Window notice = null;
        public ICommand isLoaded
        {
            get
            {
                return new DelegateCommand<Window>((obj) =>
                {
                    notice = obj as Window;
                });
            }
        }

        /// <summary>
        /// 窗口移动
        /// </summary>
        public ICommand updateMove
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    notice?.DragMove();
                });
            }
        }
        /// <summary>
        /// 根据是否有附件 设置Grid布局
        /// </summary>
        /// <param name="isAttachment"></param>
        private void SetGridStatus(bool isAttachment)
        {
            if (isAttachment)
            {
                AttachGridHeight = 90;
                TextHeight = 130;
                isShowAttch = Visibility.Visible;
            }
            else
            {
                AttachGridHeight = 0;
                TextHeight = 220;
                isShowAttch = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 查询附件信息
        /// </summary>
        /// <param name="id"></param>
        private void UpdateAttachment(string id)
        {
            AttachControlViewModelList.Clear();
            //inFindDetailsNotice inData = new inFindDetailsNotice();
            //inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
            //inData.version = "1.0";
            //inData.notificationId = id;
            //inData.token = AntSdkService.AntSdkLoginOutput.token;
            //ReturnNoticeAddDto rData = null;
            var errorCode = 0;
            string errorMsg = "";
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var result = AntSdkService.GetNotificationsById(id, ref errorCode, ref errorMsg);
            // bool result = false;//new HttpService().SearchNoticeDetailsByNoticeId(inData, ref rData, ref errorMsg, GlobalVariable.RequestMethod.GET);
            if (result != null)
            {
                if (result.attach != null)
                {
                    List<data> datas = JsonConvert.DeserializeObject<List<data>>(result.attach);
                    foreach (var list in datas)
                    {
                        if (list == null)
                        {
                            continue;
                        }
                        AttachControlViewModel vm = new AttachControlViewModel(list);
                        AttachControlViewModelList.Add(vm);
                    }
                }
            }
        }
    }
}