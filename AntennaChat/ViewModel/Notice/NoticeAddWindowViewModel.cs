using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.FileUpload;
using AntennaChat.ViewModel.Talk;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using AntennaChat.Views;
using Newtonsoft.Json.Linq;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Notice
{
    public class NoticeAddWindowViewModel : PropertyNotifyObject
    {
        //NoticeWindowListsViewModel notice;
        string groupId = "";
        /// <summary>
        /// 创建公告之后触发
        /// </summary>
        public event Action CreatedNoticeEvent;
        public NoticeAddWindowViewModel(string groupId)
        {
            this.groupId = groupId;
        }

        private string _showText;
        public string showText
        {
            get { return this._showText; }
            set
            {
                this._showText = value;
                RaisePropertyChanged(() => showText);
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
        public Grid _gridVisibality;
        /// <summary>
        /// 通知列表
        /// </summary>
        public Grid gridVisibality
        {
            get { return this._gridVisibality; }
            set
            {
                this._gridVisibality = value;
                RaisePropertyChanged(() => gridVisibality);
            }
        }
        /// <summary>
        /// 返回
        /// </summary>
        private ICommand _btnCommandBackTalkMsg;
        public ICommand btnCommandBackTalkMsg
        {
            get
            {
                if (this._btnCommandBackTalkMsg == null)
                {
                    this._btnCommandBackTalkMsg = new DelegateCommand(() =>
                    {
                        try
                        {
                            GlobalVariable.ShowDialogResult dr = MessageBoxWindow.Show("确定要放弃本次编辑吗?", MessageBoxButton.YesNo, GlobalVariable.WarnOrSuccess.Warn);
                            // MessageBoxResult dr = System.Windows.MessageBox.Show("确定要放弃本次编辑吗?", "温馨提示", MessageBoxButton.YesNo);
                            if (dr.ToString() == "Yes")
                            {
                                if (CreatedNoticeEvent != null)
                                    CreatedNoticeEvent();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteError("[NoticeAddWindowViewModel_btnCommandBackTalkMsg]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._btnCommandBackTalkMsg;
            }
        }

        private ICommand _btnCommandSendNotice;
        public ICommand SendNoticeCommand
        {
            get
            {
                if (this._btnCommandSendNotice == null)
                {
                    this._btnCommandSendNotice = new DelegateCommand(() =>
                    {
                        //var ss = this.noticeTitle;
                        //var nn = this.noticeContent;
                        if (string.IsNullOrEmpty(noticeTitle + "".Trim()))
                        {
                            showTextMethod("标题不能为空!");
                            return;
                        }
                        if (!string.IsNullOrEmpty(noticeTitle + "".Trim()))
                        {
                            if (noticeTitle.Length > 40)
                            {
                                showTextMethod("标题不能为超过40个字!");
                                return;
                            }
                        }
                        if (string.IsNullOrEmpty(noticeContent + "".Trim()))
                        {
                            showTextMethod("内容不能为空!");
                            return;
                        }
                        if (!string.IsNullOrEmpty(noticeContent + "".Trim()))
                        {
                            if (noticeContent.Length > 500)
                            {
                                showTextMethod("内容不能为超过500个字!");
                                return;
                            }
                        }
                        int count = _attachment.Count(m => m.uploadFileSucess == 1);
                        if (count != _attachment.Count())
                        {
                            showTextMethod("还有附件未上传!");
                            return;
                        }

                        //构造发送通知
                        AntSdkAddNotificationsInput addNotice = new AntSdkAddNotificationsInput();
                        addNotice.title = (this.noticeTitle + "").Trim();
                        addNotice.content = (this.noticeContent + "").Trim();
                        addNotice.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
                        addNotice.targetId = groupId;
                        if (_attachment.Any())
                        {
                            List<data> dts = new List<data>();
                            foreach (var list in _attachment)
                            {
                                if (list != null)
                                {
                                    dts.Add(list.data);
                                }
                            }
                            addNotice.attach = JsonConvert.SerializeObject(dts);
                        }
                        else
                        {
                            addNotice.attach = "";
                        }
                        ReturnNoticeAddDto returnDto = new ReturnNoticeAddDto();
                        var errorCode = 0;
                        string errorMsg = string.Empty;
                        //TODO:AntSdk_Modify
                        //DONE:AntSdk_Modify
                        var noticeOutput = AntSdkService.AddNotifications(addNotice, ref errorCode, ref errorMsg);//new HttpService().AddNotice(add, ref returnDto, ref errorMsg);
                        if (noticeOutput != null)
                        {
                            NoticeModel model = new NoticeModel();
                            model.NotificationId = noticeOutput.notificationId;
                            model.TargetId = noticeOutput.targetId;
                            model.UserId = addNotice.userId;
                            model.NoticeTitle = noticeOutput.title;
                            model.NoticeContent = noticeOutput.content;
                            model.Explain = AntSdkService.AntSdkCurrentUserInfo.userName + "  编辑于  " +
                                          DataConverter.FormatTimeByTimeStamp(noticeOutput.createTime);
                            model.NoticeAttach = string.IsNullOrEmpty(noticeOutput.attach) ?"0" : "1";
                            model.IsbtnDeleteVisibility = true;
                            //int attachFlag = 0;
                            //if (!string.IsNullOrEmpty(noticeOutput.attach))
                            //    int.TryParse(noticeOutput.attach, out attachFlag);
                            model.IsAdjunctNotice = !string.IsNullOrEmpty(noticeOutput.attach);

                            GlobalVariable.ShowDialogResult dr = MessageBoxWindow.Show("公告已成功发布！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Success);
                            if (dr.ToString() == "Ok")
                            {
                                CreatedNoticeEvent?.Invoke();
                            }

                        }
                        else
                        {
                            GlobalVariable.ShowDialogResult dr = MessageBoxWindow.Show(errorMsg + ",是否继续发布公告？",
                                MessageBoxButton.YesNo, GlobalVariable.WarnOrSuccess.Warn);
                            if (dr.ToString() == "No")
                            {
                                CreatedNoticeEvent?.Invoke();
                            }
                        }
                    });
                }
                return this._btnCommandSendNotice;
            }
        }
        public void showTextMethod(string tipsMsg)
        {
            showText = tipsMsg;
            if (pShow != null)
            {
                pShow.IsOpen = true;
                pShow.Placement = PlacementMode.Center;
                pShow.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                pShow.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
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
        RowDefinition rd;
        public ICommand btnCommandAddAttachment
        {
            get
            {
                return new DelegateCommand<RowDefinition>((obj) =>
                {
                    rd = obj as RowDefinition;
                    System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
                    //openFile.Filter = "文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|Rar文件(*.rar)|*.rar|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|mp3文件(*.mp3)|*.mp3";
                    //openFile.Filter = "所有文件(*.txt;*.xlsx;*.xls;*.docx;*.doc;*.rar;*.pdf;*.html;*.htm;*.mp3|*.txt;*.xlsx;*.xls;*.docx;*.doc;*.rar;*.pdf;*.html;*.htm;*.mp3|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|Rar文件(*.rar)|*.rar|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|mp3文件(*.mp3)|*.mp3";
                    openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe";
                    openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    openFile.FilterIndex = 0;
                    openFile.Multiselect = true;
                    if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {

                        if (openFile.FileNames.Count() > 5)
                        {
                            System.Windows.Forms.MessageBox.Show("上传文件不能超过5个!", "温馨提示");
                            return;
                        }
                        if (openFile.FileNames.Count() >= 1)
                        {
                            foreach (var listFile in openFile.FileNames)
                            {
                                string msg = "单个上传文件不能超过3MB" + "\r\n";
                                System.IO.FileInfo fileInfo = new FileInfo(listFile);
                                int type = 0;
                                if (Math.Round((double)fileInfo.Length / 1024 / 1024, 2) > 3)
                                {
                                    type = 1;
                                    msg += fileInfo.Name + "\r\n";
                                }
                                if (type == 1)
                                {
                                    MessageBoxWindow.Show("温馨提示", msg, GlobalVariable.WarnOrSuccess.Warn);
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i < openFile.FileNames.Length; i++)
                        {
                            if (PublicTalkMothed.IsFileInUsing(openFile.FileNames[i]))
                            {
                                MessageBoxWindow.Show("温馨提示", $"文件 {openFile.SafeFileNames[i]} 已经被占用，请关闭文件再上传！",
                                    MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                                return;
                            }
                        }
                        foreach (var listFile in openFile.FileNames)
                        {
                            if (_attachment.Count() > 4)
                            {
                                MessageBoxWindow.Show("温馨提示", "上传文件不能超过5个!", GlobalVariable.WarnOrSuccess.Warn);
                                return;
                            }
                            AddAttachmentDto upFileDto = new AddAttachmentDto();
                            upFileDto.fileGuid = "fj" + Guid.NewGuid().ToString().Replace("-", "");
                            upFileDto.fileName = listFile.Substring((listFile.LastIndexOf('\\') + 1), listFile.Length - 1 - listFile.LastIndexOf('\\'));

                            System.IO.FileInfo fileInfo = new FileInfo(listFile);
                            if (fileInfo.Length < 1024)
                            {
                                upFileDto.fileLength = fileInfo.Length + "B";
                            }
                            if (fileInfo.Length > 1024)
                            {
                                upFileDto.fileLength = Math.Round((double)fileInfo.Length / 1024, 2) + "KB";
                            }
                            if (fileInfo.Length > 1024 * 1024)
                            {
                                upFileDto.fileLength = Math.Round((double)fileInfo.Length / 1024 / 1024, 2) + "MB";
                            }
                            upFileDto.localPath = listFile;
                            upFileDto.fileExtendName = listFile.Substring((listFile.LastIndexOf('.') + 1), listFile.Length - 1 - listFile.LastIndexOf('.'));
                            upFileDto.fileimageShow = fileShowImage.showImageHtmlPath(upFileDto.fileExtendName, "");
                            upFileDto.btnStatus = "upLoad";
                            upFileDto.btnforeground = "0";
                            _attachment.Add(upFileDto);
                        }
                        rd.Height = new System.Windows.GridLength(90);
                    }
                });
            }
        }
        public ICommand btnCommandFileClose
        {
            get
            {
                return new DelegateCommand<string>((obj) =>
                {
                    AddAttachmentDto remove = _attachment.SingleOrDefault(m => m.fileGuid == obj.ToString());
                    _attachment.Remove(remove);
                    if (_attachment.Count() == 0)
                    {
                        rd.Height = new System.Windows.GridLength(0);
                    }
                });
            }
        }
        BackgroundWorker backUploadFile;
        public ICommand btnCommandAttachmentOperate
        {
            get
            {
                return new DelegateCommand<string>((obj) =>
                {
                    string fileid = obj as string;
                    AddAttachmentDto findUpload = _attachment.SingleOrDefault(m => m.fileGuid == obj.ToString());
                    if (PublicTalkMothed.IsFileInUsing(findUpload.localPath))
                    {
                        MessageBoxWindow.Show("温馨提示", $"文件 {findUpload.fileName} 已经被占用，请关闭文件再上传！",
                            MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    string md5Value = PublicTalkMothed.getFileMd5Value(findUpload.localPath);
                    if (!string.IsNullOrEmpty(md5Value))
                    {
                        //TODO:AntSdk_Modify
                        //DODE:AntSdk_Modify
                        var errorCode = 0;
                        string errorMsg = string.Empty;
                        AntSdkFileUpLoadOutput fileOutput = AntSdkService.CompareFileMd5(md5Value, findUpload.fileName, ref errorCode, ref errorMsg);
                        if (fileOutput == null)
                        {
                            backUploadFile = new BackgroundWorker();
                            backUploadFile.RunWorkerCompleted += BackUploadFile_RunWorkerCompleted;
                            backUploadFile.DoWork += BackUploadFile_DoWork;
                            backUploadFile.RunWorkerAsync(findUpload);
                        }
                        else
                        {
                            if (findUpload.data == null)
                                findUpload.data = new data();
                            findUpload.data.createTime = fileOutput.createTime;
                            findUpload.data.downloadURL = fileOutput.dowmnloadUrl;
                            findUpload.data.fileMD5 = fileOutput.fileMD5;
                            findUpload.data.fileName = fileOutput.fileName;
                            findUpload.data.fileSize = fileOutput.fileSize;
                            findUpload.data.fileType = fileOutput.fileType;
                            findUpload.uploadFileSucess = 1;
                            findUpload.btnforeground = "0";
                            findUpload.btnStatus = "上传成功";
                        }


                    }
                });
            }
        }
        private void BackUploadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var sendmsg = e.Argument as AddAttachmentDto;
                NoticeFileUpload(sendmsg);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeAddWindowViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }

        private void BackUploadFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null)
                {

                }
                else
                {
                    backUploadFile.Dispose();
                    //System.Windows.Forms.MessageBox.Show("上传完成");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeAddWindowViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 上传接口2.0
        /// </summary>
        /// <param name="scid"></param>
        public void NoticeFileUpload(AddAttachmentDto scid)
        {
            try
            {
                //string url = ConfigurationManager.AppSettings["NewUpLoadAddress"];
                var url = AntSdkService.AntSdkConfigInfo.AntSdkFileUpload;
                string parm = string.Format("?&key={0}&requestTime={1}&File={2}&token={3}", 20000, (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, "file", AntSdkService.AntSdkToken);
                HttpWebClient<AddAttachmentDto> client = new HttpWebClient<AddAttachmentDto>();
                client.Encoding = Encoding.UTF8;

               
                client.UploadProgressChanged += Client_UploadProgressChanged; ;
                client.UploadFileCompleted += Client_UploadFileCompleted; ;
                client.UploadFileAsync(new Uri(url + parm), "POST", scid.localPath);
                client.obj = scid;

            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeAddWindowViewModel_NoticeFileUpload]:" + ex.Message + ex.StackTrace);
            }
        }


        /// <summary>
        /// 方法说明：根据键值获取Json字符串中特定字段的值
        /// 完成时间：2016-05-20
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="json">json字符串</param>
        /// <param name="value">值</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否执行成功</returns>
        private static bool GetValueByJsonKey(string key, string json, ref string value, ref string errMsg)
        {
            try
            {
                var jObject = JObject.Parse(json);
                value = jObject[key].ToString();
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        private void Client_UploadFileCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            HttpWebClient<AddAttachmentDto> hbc = sender as HttpWebClient<AddAttachmentDto>;
            var str = Encoding.UTF8.GetString(e.Result);

            var dataStr = string.Empty;
            var temperrorMsg = string.Empty;
            if (!GetValueByJsonKey("data", str, ref dataStr, ref temperrorMsg))
            {
                LogHelper.WriteError($"[NoticeAddWindowViewModel_NoticeFileUploadReturnError data is null]:{temperrorMsg}");
                return;
            }
            //处理
            var upoutput = JsonConvert.DeserializeObject<AntSdkFileUpLoadOutput>(dataStr);
            if (upoutput != null)
            {
                var fDto = new AddAttachmentDto
                {
                    data = new data
                    {
                        createTime = upoutput.createTime,
                        downloadURL = upoutput.dowmnloadUrl,
                        fileMD5 = upoutput.fileMD5,
                        fileName = upoutput.fileName,
                        fileSize = upoutput.fileSize,
                        fileType = upoutput.fileType
                    },
                    uploadFileSucess = 1
                };
                AddAttachmentDto changed =
                    _attachment.SingleOrDefault(m => m.fileGuid == (hbc.obj as AddAttachmentDto).fileGuid);

                changed.data = fDto.data;
                changed.uploadFileSucess = 1;
                changed.btnforeground = "0";
                changed.btnStatus = "上传成功";
            }
        }
        public class upLoadFileReturn
        {
            public string result { set; get; }
            public data data { set; get; }
        }
        private void Client_UploadProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            try
            {
                //Thread.Sleep(20);
                HttpWebClient<AddAttachmentDto> hbc = sender as HttpWebClient<AddAttachmentDto>;
                AddAttachmentDto dtos = hbc.obj as AddAttachmentDto;
                AddAttachmentDto changed = _attachment.SingleOrDefault(m => m.fileGuid == (hbc.obj as AddAttachmentDto).fileGuid);
                if (e.ProgressPercentage != 100)
                {
                    changed.btnStatus = e.ProgressPercentage + "%";
                    if (changed.btnforeground != "2")
                    {
                        changed.btnforeground = "2";
                    }
                }
                else
                {
                    changed.btnforeground = "0";
                    changed.btnStatus = "上传成功";
                }
                //Console.WriteLine(e.ProgressPercentage.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[NoticeAddWindowViewModel_UploadProgressChanged]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        ///窗体大小变化
        /// </summary>
        private ICommand _IsSizeChanged;
        public ICommand IsSizeChanged
        {
            get
            {
                if (this._IsSizeChanged == null)
                {
                    this._IsSizeChanged = new DelegateCommand<System.Windows.Controls.UserControl>((obj) =>
                    {
                        System.Windows.Controls.UserControl uc = obj as System.Windows.Controls.UserControl;

                    });
                }
                return this._IsSizeChanged;
            }
        }
        Popup pShow = null;
        /// <summary>
        ///窗体大小变化
        /// </summary>
        private ICommand _IsLoaded;
        public ICommand IsLoaded
        {
            get
            {
                if (this._IsLoaded == null)
                {
                    this._IsLoaded = new DelegateCommand<Popup>((obj) =>
                    {
                        pShow = obj as Popup;
                    });
                }
                return this._IsLoaded;
            }
        }
    }
}
