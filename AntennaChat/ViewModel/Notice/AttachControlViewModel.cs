using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.FileUpload;
using AntennaChat.ViewModel.Talk;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AntennaChat.ViewModel.Notice
{
    public class AttachControlViewModel : PropertyNotifyObject
    {
        private data FileInfo;
        public AttachControlViewModel(data data)
        {
            FileInfo = data;
            InitInfo();
        }
        #region 属性
        /// <summary>
        /// 附件下载地址
        /// </summary>
        public string downloadURL = "";
        public string filePath = "";
        /// <summary>
        /// 附件名
        /// </summary>
        private string _FileName;
        public string FileName
        {
            set
            {
                this._FileName = value;
                RaisePropertyChanged(() => FileName);
            }
            get { return this._FileName; }
        }
        /// <summary>
        /// 附件大小
        /// </summary>
        private string _FileSize;
        public string FileSize
        {
            set
            {
                this._FileSize = value;
                RaisePropertyChanged(() => FileSize);
            }
            get { return this._FileSize; }
        }
        /// <summary>
        /// 下载按钮
        /// </summary>
        private string _DownLoadButtonContent;
        public string DownLoadButtonContent
        {
            set
            {
                this._DownLoadButtonContent = value;
                RaisePropertyChanged(() => DownLoadButtonContent);
            }
            get { return this._DownLoadButtonContent; }
        }
        /// <summary>
        /// 附件类型
        /// </summary>
        private string _FileTypeImage;
        public string FileTypeImage
        {
            set
            {
                this._FileTypeImage = value;
                RaisePropertyChanged(() => FileTypeImage);
            }
            get { return this._FileTypeImage; }
        }
        /// <summary>
        /// 下载进度
        /// </summary>
        private string _DownloadProgress;
        public string DownloadProgress
        {
            set
            {
                this._DownloadProgress = value;
                RaisePropertyChanged(() => DownloadProgress);
            }
            get { return this._DownloadProgress; }
        }
        #endregion
        /// <summary>
        /// 初始化附件信息
        /// </summary>
        private void InitInfo()
        {
            downloadURL = FileInfo.downloadURL;
            FileName = FileInfo.fileName;
            if (!string.IsNullOrEmpty(FileInfo.fileSize))
            {
                if (Convert.ToInt32(FileInfo.fileSize) < 1024)
                {
                    FileSize = FileInfo.fileSize + "B";
                }
                else if (Convert.ToInt32(FileInfo.fileSize) < 1024 * 1024)
                {
                    FileSize = Math.Round((double)Convert.ToInt32(FileInfo.fileSize) / 1024, 2) + "KB";
                }
                else
                {
                    FileSize = Math.Round((double)Convert.ToInt32(FileInfo.fileSize) / 1024 / 1024, 2) + "MB";
                }
            }
            DownLoadButtonContent = "下载";
            if (!string.IsNullOrEmpty(FileInfo.fileType))
                FileTypeImage = fileShowImage.showFileImagePath(FileInfo.fileType, "");
            else if (!string.IsNullOrEmpty(FileInfo.fileName))
            {
                var lastIndex = FileInfo.fileName.LastIndexOf(".", StringComparison.Ordinal)+1;
                var fileSuffix = FileInfo.fileName.Substring(lastIndex, FileInfo.fileName.Length - lastIndex);
                if (!string.IsNullOrEmpty(fileSuffix))
                    FileTypeImage = fileShowImage.showFileImagePath(fileSuffix, "");
            }
        }
        #region 下载相关
        BackgroundWorker backdownFile = null;
        public ICommand btnCommandDownOperate
        {
            get
            {
                return new DelegateCommand<string>((obj) =>
                {
                    if (DownLoadButtonContent == "下载")
                    {
                        System.Windows.Forms.SaveFileDialog openFile = new System.Windows.Forms.SaveFileDialog();
                        openFile.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt|Excel文件(*.xlsx,*.xls)|*.xlsx;*.xls|Word文件(*.docx,*.doc)|*.docx;*.doc|ppt文件(*.ppt)|*.ppt|图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png|压缩文件(*.rar;*.zip)|*.rar;*.zip|Pdf文件(*.pdf)|*.pdf|Html文件(*.html,*.htm)|*.html;*.htm|音频文件(*.mp3;*.mp4)|*.mp3;*.mp4|可执行文件(*.exe)|*exe ";
                        openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFile.FilterIndex = 0;
                        openFile.FileName = FileName;
                        if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            filePath = openFile.FileName;
                            backdownFile = new BackgroundWorker();
                            backdownFile.DoWork += BackdownFile_DoWork;
                            backdownFile.RunWorkerCompleted += BackdownFile_RunWorkerCompleted;
                            backdownFile.RunWorkerAsync(filePath);

                        }
                    }
                    else
                    {
                        PublicTalkMothed.OpenFile(filePath);
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
                var sendmsg = e.Argument as string;
                downMethodFile(sendmsg);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[AttachControlViewModel_Back_DoWork]" + ex.Message + ex.StackTrace);
            }
        }
        public void downMethodFile(string fileName)
        {
            try
            {
                HttpWebClient<string> web = new HttpWebClient<string>();
                web.DownloadFileAsync(new Uri(downloadURL), fileName);
                web.DownloadProgressChanged += Web_DownloadProgressChanged;
                web.DownloadFileCompleted += Web_DownloadFileCompleted;
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 下载进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            HttpWebClient<string> hbc = sender as HttpWebClient<string>;
            if (e.ProgressPercentage != 100)
            {
                DownloadProgress = e.ProgressPercentage + "%";
                DownLoadButtonContent = string.Empty;
            }
            else
            {
                DownloadProgress = string.Empty;
                DownLoadButtonContent = "查看";
            }

        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                DownloadProgress = string.Empty;
                DownLoadButtonContent = "查看";
            }
            else
            {
                DownloadProgress = string.Empty;
                DownLoadButtonContent = "下载";
            }

        }
        #endregion
    }
}
