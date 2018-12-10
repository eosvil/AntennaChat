using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class AddAttachmentDto : INotifyPropertyChanged
    {
        public string result { set; get; }
        public data data { set; get; }
        public string fileGuid { set; get; }
        public string fileName { set; get; }
        //public string fileExtendName { set; get; }

        private string _fileExtendName;
        public string fileExtendName
        {
            set
            {
                if(_fileExtendName==null)
                {
                    _fileExtendName = "1";
                }
                _fileExtendName = value;
                OnPropertyChanged("fileExtendName");
            }
            get { return _fileExtendName; }
        }
        private string _btnStatus;
        public string btnStatus
        {
            set
            {
                if (_btnStatus == null)
                {
                    _btnStatus = "1";
                }
                _btnStatus = value;
                OnPropertyChanged("btnStatus");
            }
            get { return _btnStatus; }
        }
        private string _btnforeground="1";
        public string btnforeground
        {
            set
            {
                _btnforeground = value;
                OnPropertyChanged("btnforeground");
            }
            get { return _btnforeground; }
        }
        public string imageIcon { set; get; }
        public string fileLength { set; get; }
        public string btnImageStatus { set; get; }
        public string localPath { set; get; }
        public string errorCode { set; get; }
        public string path { set; get; }
        public int downFileSucess { set; get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public int uploadFileSucess { set; get; }
        public string fileimageShow { set; get; }
    }
    public class data
    {
        public string fileMD5 { set; get; }
        public string downloadURL { set; get; }
        public string fileSize { set; get; }
        public string fileType { set; get; }
        public string createTime { set; get; }
        public string fileName { set; get; }
    }
}
