using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class baseNotice
    {
        public int result { set; get; }
        public string errorCode { set; get; }
        public string errorMsg { set; get; }
    }
    public class ReturnNoticeAddDto:baseNotice
    {
        public noticeData data { set; get; }
    }
    public class noticeData
    {
        public string notificationId { set; get; }
        public string title { set; get; }
        public string content { set; get; }
        public string attach { set; get; }
        public string hasAttach { set; get; }
        public string targetId { set; get; }
        public string readState { set; get; }
        public string createTime { set; get; }
        public string updateTime { set; get; }
        public string createBy { set; get; }
        public double gridHeight { set; get; }
    }
    public class ReturnNoticeList : baseNotice
    {
        public List<noticeData> data { set; get; }
    }
    public class inFindNoticeList
    {
        public string token { set; get; }
        public string version { set; get; }
        public string userId { set; get; }
        public string targetId { set; get; }
    }
    public class inFindDetailsNotice
    {
        public string token { set; get; }
        public string version { set; get; }
        public string userId { set; get; }
        public string notificationId { set; get; }
    }
    public class delateNoticeDto
    {
        public string token { set; get; }
        public string version { set; get; }
        public string userId { set; get; }
        public string notificationId { set; get; }
        public string targetId { set; get; } 
    }
}
