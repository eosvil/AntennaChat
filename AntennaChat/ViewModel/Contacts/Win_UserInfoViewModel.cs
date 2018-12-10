using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System.Windows;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel.Contacts
{
    public class Win_UserInfoViewModel : WindowBaseViewModel
    {
        private string _targetId;
        public Win_UserInfoViewModel(string id)
        {
            this._targetId = id;
        }
        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="id"></param>
        private async void InitUserInfo(string id)
        {
            var temperrorCode = 0;
            var temperrorMsg = string.Empty;
            await Task.Run(() =>
            {
                CurrentProfile= AntSdkService.AntSdkGetUserInfo(id, ref temperrorCode, ref temperrorMsg);
            });
            SetUserInfo();
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void SetUserInfo()
        {
            if(CurrentProfile==null) return;
            HeadPic = string.IsNullOrEmpty(CurrentProfile.picture)
                ? "pack://application:,,,/AntennaChat;Component/Images/198-头像.png"
                : CurrentProfile.picture;
            Sex = CurrentProfile.sex;
            Signature = CurrentProfile.signature;
            if (string.IsNullOrEmpty(CurrentProfile.userNum))
            {
                UserName = CurrentProfile.userName;
            }
            else
            {
                UserName = CurrentProfile.userNum + "-" + CurrentProfile.userName;
            }
            if (CurrentProfile.status == 0)
                UserName = UserName + "（停用）";
            Position = CurrentProfile.position;
            Phone = CurrentProfile.phone;
            Email = CurrentProfile.email;
            DepartName = CurrentProfile.departName;
            #region 更新数据库

            var targetUserInfo = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(v => v.userId == _targetId);
            if (targetUserInfo == null) return;
            if (targetUserInfo.picture == CurrentProfile.picture || string.IsNullOrEmpty(CurrentProfile.picture))
                return;
            targetUserInfo.picture = CurrentProfile.picture;
            ThreadPool.QueueUserWorkItem(m => AntSdkService._cUserInfoDal.Update(targetUserInfo));

            #endregion
        }
        /// <summary>
        /// 用户头像
        /// </summary>
        private string _HeadPic= "pack://application:,,,/AntennaChat;Component/Images/198-头像.png";
        public string HeadPic
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
        /// 工号-用户名
        /// </summary>
        private string _UserName ;
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
        private string _Signature;
        /// <summary>
        /// 个性签名
        /// </summary>
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

        private int _Sex = -1;
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex
        {
            get
            {
                return _Sex;
            }
            set
            {
                _Sex = value;
                RaisePropertyChanged(() => Sex);
            }
        }
        private string _Position;
        /// <summary>
        /// 职位
        /// </summary>
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
        private string _departName;
        /// <summary>
        /// 部门
        /// </summary>
        public string DepartName
        {
            get
            {
                return _departName;
            }
            set
            {
                _departName = value;
                RaisePropertyChanged(() => DepartName);
            }
        }
        private string _phone;
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                _phone = value;
                RaisePropertyChanged(() => Phone);
            }
        }
        private string _email;
        /// <summary>
        /// Email
        /// </summary>
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                RaisePropertyChanged(() => Email);
            }
        }
        private AntSdkUserInfo _currentProfile;
        /// <summary>
        /// 用户信息
        /// </summary>
        public AntSdkUserInfo CurrentProfile
        {
            get { return _currentProfile; }
            set
            {
                _currentProfile = value;
                RaisePropertyChanged(() => CurrentProfile);
            }
        }
        private ICommand _Loaded;
        public ICommand LoadedCommand
        {
            get
            {
                if (this._Loaded == null)
                {
                    this._Loaded = new DefaultCommand(
                        o =>
                        {
                            if (_targetId == AntSdkService.AntSdkLoginOutput.userId)
                            {
                                CurrentProfile = AntSdkService.AntSdkCurrentUserInfo;
                                SetUserInfo();
                            }
                            else
                                InitUserInfo(_targetId);
                        });
                }
                return this._Loaded;
            }
        }
    }
}
