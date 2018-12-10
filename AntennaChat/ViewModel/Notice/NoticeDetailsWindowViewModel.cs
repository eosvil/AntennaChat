using Antenna.Framework;
using Antenna.Model;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using AntennaChat.ViewModel.FileUpload;
using System.Windows.Forms;
using System.Net;
using System.ComponentModel;
using AntennaChat.ViewModel.Talk;
using SDK.AntSdk;

namespace AntennaChat.ViewModel.Notice
{
    public class NoticeDetailsWindowViewModel : PropertyNotifyObject
    {
        NoticeWindowListsViewModel notice;
        NoticeAddDto dtos;
        public NoticeDetailsWindowViewModel(NoticeWindowListsViewModel notice, NoticeAddDto dtos)
        {
            this.notice = notice;
            this.dtos = dtos;
            //inFindDetailsNotice inData = new inFindDetailsNotice();
            //inData.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
            //inData.version = "1.0";
            //inData.notificationId = dtos.notificationId;
            //inData.token = AntSdkService.AntSdkLoginOutput.token;
            //ReturnNoticeAddDto rData = null;
            var errorCode = 0;
            string errorMsg = "";
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var result = AntSdkService.GetNotificationsById(dtos.notificationId, ref errorCode, ref errorMsg);
            //bool result = false;//new HttpService().SearchNoticeDetailsByNoticeId(inData, ref rData, ref errorMsg, GlobalVariable.RequestMethod.GET);
            if (result != null)
            {
                noticeTitle = result.title;
                noticeContent = result.content;

                explain = result.createBy + "  编辑于  " + DataConverter.FormatTimeByTimeStamp(result.createTime);
                if (result.attach != null)
                {
                    gridHeight = 90;
                    List<data> datas = JsonConvert.DeserializeObject<List<data>>(result.attach);

                    foreach (var list in datas)
                    {
                        if (list == null)
                        {
                            continue;
                        }
                        AddAttachmentDto add = new AddAttachmentDto();
                        add.fileGuid = Guid.NewGuid().ToString().Replace("-", "");
                        add.fileName = list.fileName;
                        if (Convert.ToInt32(list.fileSize) < 1024)
                        {
                            add.fileLength = list.fileSize + "B";
                        }
                        if (Convert.ToInt32(list.fileSize) > 1024)
                        {
                            add.fileLength = Math.Round((double)Convert.ToInt32(list.fileSize) / 1024, 2) + "KB";
                        }
                        if (Convert.ToInt32(list.fileSize) > 1024 * 1024)
                        {
                            add.fileLength = Math.Round((double)Convert.ToInt32(list.fileSize) / 1024 / 1024, 2) + "MB";
                        }
                        add.localPath = list.downloadURL;
                        add.fileimageShow = fileShowImage.showImageHtmlPath(list.fileType, "");
                        add.btnStatus = "downLoad";
                        add.btnforeground = "0";
                        _attachment.Add(add);
                    }
                }
                else
                {
                    gridHeight = 0;
                }
            }
        }
        private ObservableCollection<AddAttachmentDto> _attachment = new ObservableCollection<AddAttachmentDto>();
        /// <summary>
        /// 上传通知列表
        /// </summary>
        public ObservableCollection<AddAttachmentDto> listAttachment
        {
            get { return this._attachment; }
            set
            {
                this._attachment = value;
                RaisePropertyChanged(() => listAttachment);
            }
        }
        private string _noticeTitle;
        public string noticeTitle
        {
            get { return this._noticeTitle; }
            set
            {
                this._noticeTitle = value;
                RaisePropertyChanged(() => noticeTitle);
            }
        }
        private string _noticeContent;
        public string noticeContent
        {
            get { return this._noticeContent; }
            set
            {
                this._noticeContent = value;
                RaisePropertyChanged(() => noticeContent);
            }
        }
        private string _explain;
        public string explain
        {
            get { return this._explain; }
            set
            {
                this._explain = value;
                RaisePropertyChanged(() => explain);
            }
        }
        public double _gridHeight;
        /// <summary>
        /// 通知列表
        /// </summary>
        public double gridHeight
        {
            get { return this._gridHeight; }
            set
            {
                this._gridHeight = value;
                RaisePropertyChanged(() => _gridHeight);
            }
        }
        BackgroundWorker backdownFile = null;
        public ICommand btnCommandDownOperate
        {
            get
            {
                return new DelegateCommand<string>((obj) =>
                {
                    string id = obj as string;

                    AddAttachmentDto selectFile = _attachment.SingleOrDefault(m => m.fileGuid == id);
                    if (selectFile.downFileSucess == 1)
                    {
                        PublicTalkMothed.OpenFile(selectFile.path);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog openFile = new System.Windows.Forms.SaveFileDialog();
                        //openFile.Filter = "文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|Rar文件(*.rar)|*.rar|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|mp3文件(*.mp3)|*.mp3";
                        openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe ";
                        openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFile.FilterIndex = 0;
                        openFile.FileName = selectFile.fileName;
                        if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            selectFile.path = openFile.FileName;
                            backdownFile = new BackgroundWorker();
                            backdownFile.DoWork += BackdownFile_DoWork;
                            backdownFile.RunWorkerCompleted += BackdownFile_RunWorkerCompleted;
                            backdownFile.RunWorkerAsync(selectFile);

                        }
                    }
                });
            }
        }

        private void BackdownFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backdownFile.Dispose();
        }

        private void BackdownFile_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var sendmsg = e.Argument as AddAttachmentDto;
                downMethodFile(sendmsg);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeDetailsWindowViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }

        public void downMethodFile(AddAttachmentDto msg)
        {
            try
            {
                HttpWebClient<AddAttachmentDto> web = new HttpWebClient<AddAttachmentDto>();
                web.obj = msg;
                web.DownloadFileAsync(new Uri(msg.localPath), msg.path);
                web.DownloadProgressChanged += Web_DownloadProgressChanged;
                web.DownloadFileCompleted += Web_DownloadFileCompleted;
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            AddAttachmentDto changed = _attachment.SingleOrDefault(m => m.fileGuid == ((sender as HttpWebClient<AddAttachmentDto>).obj as AddAttachmentDto).fileGuid);
            changed.uploadFileSucess = 1;
        }
        /// <summary>
        /// 下载进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            HttpWebClient<AddAttachmentDto> hbc = sender as HttpWebClient<AddAttachmentDto>;
            AddAttachmentDto dtos = hbc.obj as AddAttachmentDto;
            AddAttachmentDto downs = _attachment.SingleOrDefault(m => m.fileGuid == dtos.fileGuid);
            if (e.ProgressPercentage != 100)
            {
                downs.btnStatus = e.ProgressPercentage + "%";
                if (downs.btnforeground != "2")
                {
                    downs.btnforeground = "2";
                }
            }
            else
            {
                downs.downFileSucess = 1;
                downs.btnforeground = "0";
                downs.btnStatus = "查看";
            }
        }
        public ICommand btnCommandBackTalkMsg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    //notice.NoticeDetailsWindowViewModelVisibility = System.Windows.Visibility.Collapsed;
                });
            }
        }
    }
}
