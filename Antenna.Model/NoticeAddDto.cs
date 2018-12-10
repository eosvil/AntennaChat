using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class NoticeAddDto: INotifyPropertyChanged
    {
        public string notificationId { set; get; }
        public string token { set; get; }
        public string version { set; get; }
        public string userId { set; get; }
        public string title { set; get; }
        public string content { set; get; }
        public string attach { set; get; }
        public string targetId { set; get; }
        public string explain { set; get; }
        public string isbtnShowVisibility { set; get; }
        public string isbtnDeleteVisibility { set; get; }
        public string fontShowWeight { set; get; }
        //public string readStatusforeground { set; get; }

        private string _readStatusforeground;
        public string readStatusforeground
        {
            set { _readStatusforeground = value; OnPropertyChanged("readStatusforeground"); }
            get { return _readStatusforeground; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
