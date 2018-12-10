using Antenna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Antenna.Model;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AntennaChat.ViewModel.Contacts
{
    public class UserInfoViewModel : PropertyNotifyObject
    {
        private string targetId;
        private AntSdkUserInfo userInfo;
        /// <summary>
        /// 多人
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isBurn"></param>
        public UserInfoViewModel(string id, GlobalVariable.BurnFlag isBurn)
        {
            targetId = id;
            InitUserInfo(id, isBurn);
        }
        /// <summary>
        /// 点对点
        /// </summary>
        /// <param name="id"></param>
        public UserInfoViewModel(string id)
        {
            targetId = id;
            InitUserInfo(id);
        }
        #region 属性
        /// <summary>
        /// 姓名
        /// </summary>
        private string _UserName;
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
                RaisePropertyChanged(() => UserName);
            }
        }
        /// <summary>
        /// 签名
        /// </summary>
        private string _Signature;
        public string Signature
        {
            get
            {
                return _Signature;
            }
            set
            {
                _Signature = value;
                RaisePropertyChanged(() => Signature);
            }
        }
        /// <summary>
        /// 用户头像
        /// </summary>
        private ImageSource _HeadPic;
        public ImageSource HeadPic
        {
            get
            {
                return _HeadPic;
            }
            set
            {
                _HeadPic = value;
                RaisePropertyChanged(() => HeadPic);
            }
        }
        /// <summary>
        /// 部门
        /// </summary>
        private string _DepartName;
        public string DepartName
        {
            get
            {
                return _DepartName;
            }
            set
            {
                _DepartName = value;
                RaisePropertyChanged(() => DepartName);
            }
        }
        /// <summary>
        /// 职位
        /// </summary>
        private string _Position;
        public string Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                RaisePropertyChanged(() => Position);
            }
        }
        /// <summary>
        /// 手机
        /// </summary>
        private string _Phone;
        public string Phone
        {
            get
            {
                return _Phone;
            }
            set
            {
                _Phone = value;
                RaisePropertyChanged(() => Phone);
            }
        }
        /// <summary>
        /// 邮箱
        /// </summary>
        private string _Email;
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                _Email = value;
                RaisePropertyChanged(() => Email);
            }
        }
        /// <summary>
        /// @按钮是否可用
        /// </summary>
        private Visibility _ATVisibility = Visibility.Collapsed;
        public Visibility ATVisibility
        {
            get
            {
                return _ATVisibility;
            }
            set
            {
                _ATVisibility = value;
                RaisePropertyChanged(() => ATVisibility);
            }
        }
        /// <summary>
        /// 发送消息按钮是否可用
        /// </summary>
        private Visibility _SendVisibility = Visibility.Visible;
        public Visibility SendVisibility
        {
            get
            {
                return _SendVisibility;
            }
            set
            {
                _SendVisibility = value;
                RaisePropertyChanged(() => SendVisibility);
            }
        }
        #endregion
        #region 方法、事件
        /// <summary>
        /// 初始化用户信息
        /// 多人
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isBurn"></param>
        public void InitUserInfo(string id, GlobalVariable.BurnFlag isBurn)
        {
            targetId = id;
            ATVisibility = Visibility.Visible;
            if (id == AntSdkService.AntSdkLoginOutput.userId)
            {
                ATVisibility = Visibility.Collapsed;
                SendVisibility = Visibility.Collapsed;
                if (!string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.userNum))
                {
                    UserName = AntSdkService.AntSdkCurrentUserInfo.userNum + AntSdkService.AntSdkCurrentUserInfo.userName;
                }
                else
                {
                    UserName = AntSdkService.AntSdkCurrentUserInfo.userName;
                }
                if (string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.picture))
                {
                    SetPicture("pack://application:,,,/AntennaChat;Component/Images/198-头像.png");
                }
                else
                    SetPicture(AntSdkService.AntSdkCurrentUserInfo.picture);
                Signature = !string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.signature)
                    ? AntSdkService.AntSdkCurrentUserInfo.signature
                    : "这个人很懒，什么都没留下";
                DepartName = AntSdkService.AntSdkCurrentUserInfo.departName;
                Position = AntSdkService.AntSdkCurrentUserInfo.position;
                Phone = AntSdkService.AntSdkCurrentUserInfo.phone;
                Email = AntSdkService.AntSdkCurrentUserInfo.email;
            }
            else
            {
                QueryUserInfo(id);
            }
            if (isBurn == GlobalVariable.BurnFlag.IsBurn)
            {
                ATVisibility = Visibility.Collapsed;
            }
        }
        private void SetPicture(string uri)
        {
            if (!string.IsNullOrWhiteSpace(uri) && publicMethod.IsUrlRegex(uri))
            {
                var index = uri.LastIndexOf("/", StringComparison.Ordinal) + 1;
                var fileNameIndex = uri.LastIndexOf(".", StringComparison.Ordinal);
                var fileName = uri.Substring(index, fileNameIndex - index);
                var headUri = uri.Replace(fileName, fileName + "_70x70");
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(headUri, UriKind.RelativeOrAbsolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                HeadPic = bi;
            }
            else
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                HeadPic = bi;
            }
        }
        /// <summary>
        /// 点对点
        /// </summary>
        /// <param name="userID"></param>
        public void InitUserInfo(string userID)
        {
            ATVisibility = Visibility.Collapsed;
            SendVisibility = Visibility.Collapsed;
            if (userID == AntSdkService.AntSdkLoginOutput.userId)
            {
                if (!string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.userNum))
                {
                    UserName = AntSdkService.AntSdkCurrentUserInfo.userNum + AntSdkService.AntSdkCurrentUserInfo.userName;
                }
                else
                {
                    UserName = AntSdkService.AntSdkCurrentUserInfo.userName;
                }
                if (string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.picture))
                {
                    SetPicture("pack://application:,,,/AntennaChat;Component/Images/198-头像.png");
                }
                else
                    SetPicture(AntSdkService.AntSdkCurrentUserInfo.picture);
                Signature = !string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.signature)
                    ? AntSdkService.AntSdkCurrentUserInfo.signature
                    : "这个人很懒，什么都没留下";
                DepartName = AntSdkService.AntSdkCurrentUserInfo.departName;
                Position = AntSdkService.AntSdkCurrentUserInfo.position;
                Phone = AntSdkService.AntSdkCurrentUserInfo.phone;
                Email = AntSdkService.AntSdkCurrentUserInfo.email;
            }
            else
            {
                QueryUserInfo(userID);
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private void SetUserInfo()
        {
            if (userInfo == null) return;
            if (!string.IsNullOrEmpty(userInfo.userNum))
            {
                UserName = userInfo.userNum + userInfo.userName;
            }
            else
            {
                UserName = userInfo.userName;
            }
            if (userInfo.status == 0)
                UserName = UserName + "（停用）";
            if (string.IsNullOrEmpty(userInfo.picture))
            {
                SetPicture("pack://application:,,,/AntennaChat;Component/Images/198-头像.png");
            }
            else
                SetPicture(userInfo.picture);
            Signature = !string.IsNullOrEmpty(userInfo.signature)
                ? userInfo.signature
                : "这个人很懒，什么都没留下";
            DepartName = userInfo.departName;
            Position = userInfo.position;
            Phone = userInfo.phone;
            Email = userInfo.email;

            #region 更新数据库

            var targetUserInfo = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(v => v.userId == targetId);
            if (targetUserInfo == null) return;
            if (targetUserInfo.picture == userInfo.picture || string.IsNullOrEmpty(userInfo.picture))
                return;
            targetUserInfo.picture = userInfo.picture;
            ThreadPool.QueueUserWorkItem(m => AntSdkService._cUserInfoDal.Update(targetUserInfo));

            #endregion
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="id"></param>
        private async void QueryUserInfo(string id)
        {
            //if(userInfo!=null&&id== userInfo.userId)return;
            //var info = GroupPublicFunction.QueryUserInfo(id);
            //Task.WaitAll();
            //userInfo = info;
            //Application.Current.Dispatcher.Invoke((Action)(SetUserInfo));
            if (userInfo != null && id == userInfo.userId) return;
            await Task.Run(() =>
            {
                userInfo = GroupPublicFunction.QueryUserInfo(id);
            });
            SetUserInfo();
        }
        public delegate void DelSendOrAt(string methodType, string id);
        public event DelSendOrAt SendOrAtEvent;
        private void OnSendOrAtEvent(string methodType, string userId)
        {
            SendOrAtEvent?.Invoke(methodType, userId);
        }
        #endregion
        #region  Command
        /// <summary>
        /// @消息
        /// </summary>      
        public ICommand ATCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnSendOrAtEvent("2", targetId);//1-发送消息 2-@
                });
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        public ICommand SendMsgCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnSendOrAtEvent("1", targetId);
                });
            }
        }
        #endregion
    }
}
