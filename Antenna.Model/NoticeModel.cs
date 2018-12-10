using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class NoticeModel: INotifyPropertyChanged
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string NotificationId { set; get; }
      
        /// <summary>
        /// 当前用户ID
        /// </summary>
        public string UserId { set; get; }
        /// <summary>
        /// 公告标题
        /// </summary>
        public string NoticeTitle { set; get; }
        /// <summary>
        /// 公告内容
        /// </summary>
        public string NoticeContent { set; get; }
        /// <summary>
        /// 公告标识附件
        /// </summary>
        public string NoticeAttach { set; get; }
        /// <summary>
        ///公告所属群组ID
        /// </summary>
        public string TargetId { set; get; }
        /// <summary>
        /// 是否包含附件
        /// </summary>
        public bool IsAdjunctNotice { get; set; }

        /// <summary>
        /// 公告发送人、时间
        /// </summary>
        public string Explain { set; get; }
        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool IsbtnDeleteVisibility { set; get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
