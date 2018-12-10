using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class LoginModel
    {
        private string password = "";
        public string LoginID { get; set; }
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }
    }
    public class AccountInfo
    {
        public string ID { get; set; }
        private string password = "";
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }
        public DateTime LastLoginTime { get; set; }
        public int OnLine { get; set; }    //0-在线，1-繁忙，2-离开
        public bool RememberPwd { get; set; }
        public bool AutoLogin { get; set; }
        public bool IsLogin { get; set; }
        public bool IsIdentifyingCode { get; set; }
    }

    public class InstallLoginInfo
    {
        public string ID { get; set; }
        public bool IsFirstLogin { get; set; }
    }
}
