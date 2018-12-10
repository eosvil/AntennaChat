using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntennaChat.Resource;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using AntennaChat.ViewModel.Contacts;
using static SDK.AntSdk.AntModels.AntSdkReceivedUserMsg;

namespace AntennaChat.ViewModel.Setting
{
    public class ProfileViewModel : PropertyNotifyObject
    {
        public ProfileViewModel()
        {
            //_HeadPic = AntSdkService.AntSdkCurrentUserInfo.picture;
            //_Sex = AntSdkService.AntSdkCurrentUserInfo.sex;
            //_Signature = AntSdkService.AntSdkCurrentUserInfo.signature;
            //if (string.IsNullOrEmpty(AntSdkService.AntSdkCurrentUserInfo.userNum))
            //{
            //    _UserName = AntSdkService.AntSdkCurrentUserInfo.userName;
            //}
            //else
            //{
            //    _UserName = AntSdkService.AntSdkCurrentUserInfo.userNum + "-" + AntSdkService.AntSdkCurrentUserInfo.userName;
            //}

        }
        /// <summary>
        /// UI显示信息
        /// </summary>
        private void SetUserInfo()
        {
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
            Position = CurrentProfile.position;
            Phone = CurrentProfile.phone;
            Email = CurrentProfile.email;
            DepartName = CurrentProfile.departName;
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


        private string _UserName;
        /// <summary>
        /// 工号-姓名
        /// </summary>
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

        private string _HeadPic;
        /// <summary>
        /// 头像
        /// </summary>
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
        private bool _SaveButtonEnabled = true;
        public bool SaveButtonEnabled
        {
            get { return _SaveButtonEnabled; }
            set
            {
                if (_SaveButtonEnabled == value)
                {
                    return;
                }
                _SaveButtonEnabled = value;
                RaisePropertyChanged(() => SaveButtonEnabled);
            }
        }
        /// <summary>
        /// 编辑头像事件
        /// </summary>
        public event EventHandler SetHeadImage;
        protected virtual void OnSetHeadImage()
        {
            this.SetHeadImage?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 保存用户信息事件
        /// </summary>
        public event EventHandler SaveProfile;
        private void OnSaveProfile()
        {
            this.SaveProfile?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// 查询用户信息同步事件
        /// </summary>
        /// <param name="modify"></param>
        public delegate void QueryInfo(AntSdkReceivedUserMsg.Modify modify);
        public event QueryInfo QueryInfoHandler;
        protected virtual void OnQueryInfoHandler(Modify modify)
        {
            QueryInfoHandler?.Invoke(modify);
        }
        /// <summary>
        /// 头像点击
        /// </summary>
        private ICommand _HeadCommand;
        public ICommand HeadCommand
        {
            get
            {
                if (this._HeadCommand == null)
                {
                    this._HeadCommand = new DefaultCommand(
                              o =>
                              {
                                  OnSetHeadImage();
                              });
                }
                return this._HeadCommand;
            }
        }
        /// <summary>
        /// 保存用户信息
        /// </summary>
        private ICommand _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (this._SaveCommand == null)
                {
                    this._SaveCommand = new DefaultCommand(
                        o =>
                        {
                            SaveButtonEnabled = false;
                            Task.Factory.StartNew(SaveInfo);
                        });
                }
                return this._SaveCommand;
            }
        }
        /// <summary>
        /// 加载用户信息
        /// </summary>
        public void InitUserInfo()
        {
            QueryUserInfo();
        }
        private void SaveInfo()
        {
            SaveUserInfo();
            Task.WaitAll();
            OnSaveProfile();
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <returns></returns>
        private void SaveUserInfo()
        {
            var errCode = 0;
            string errMsg = string.Empty;
            if (string.IsNullOrEmpty(HeadPic)) return;
            AntSdkUpdateUserInput userInfoInput = new AntSdkUpdateUserInput();
            if (!HeadPic.StartsWith("pack://application:,,,/AntennaChat;Component/Images/198-头像.png"))
            {
                if (HeadPic != CurrentProfile.picture)
                {
                    SendCutImageDto scid = new SendCutImageDto();
                    scid.cmpcd = GlobalVariable.CompanyCode;
                    scid.seId = "";
                    scid.fileFileName = "";
                    scid.file = HeadPic;
                    //TODO:AntSdk_Modify

                    AntSdkSendFileInput fileInput = new AntSdkSendFileInput();
                    fileInput.cmpcd = GlobalVariable.CompanyCode;
                    fileInput.seId =
                        fileInput.file = HeadPic;
                    AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
                    failMessage.mtp = (int) AntSdkMsgType.ChatMsgPicture;
                    failMessage.content = "";
                    //failMessage.sessionid = s_ctt.sessionId;
                    DateTime dt = DateTime.Now;
                    failMessage.lastDatetime = dt.ToString();
                    fileInput.FailOrSucess = failMessage;
                    var fileOutput = AntSdkService.FileUpload(fileInput, ref errCode, ref errMsg);
                    //ReturnCutImageDto dto = (new HttpService()).FileUpload<ReturnCutImageDto>(scid);
                    if (fileOutput != null)
                    {
                        string url = fileOutput.dowmnloadUrl;
                        HeadPic = url;
                        AntSdkService.AntSdkCurrentUserInfo.picture = url;
                    }
                    else
                    {
                        LogHelper.WriteError("上传新头像失败:" + errMsg);
                        userInfoInput.picture = "";
                    }
                }
            }
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            if (HeadPic.StartsWith("pack://application:,,,/AntennaChat;Component/Images/198-头像.png"))
            {
                userInfoInput.picture = "";
            }
            else
            {
                userInfoInput.picture = HeadPic;
            }
            userInfoInput.sex = Sex;
            userInfoInput.signature = this.Signature;
            //userInfoInput.voiceMode = CurrentProfile.voiceMode;
            //userInfoInput.vibrateMode = CurrentProfile.vibrateMode;
            bool isResult = AntSdkService.AntSdkUpdateUser(userInfoInput, ref errCode, ref errMsg);
            if (!isResult)
            {
                LogHelper.WriteError("更新用户信息失败:" + errMsg);
            }
            else
            {
                AntSdkService.AntSdkCurrentUserInfo.signature = this.Signature;
                AntSdkService.AntSdkCurrentUserInfo.sex = Sex;
            }
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        private async void QueryUserInfo()
        {
            await Task.Run(() =>
            {
                CurrentProfile = GroupPublicFunction.QueryUserInfo(AntSdkService.AntSdkCurrentUserInfo.userId);
            });
            if (CurrentProfile == null)
                CurrentProfile = AntSdkService.AntSdkCurrentUserInfo;
            SetUserInfo();
            var modify = new AntSdkReceivedUserMsg.Modify();
            if (CurrentProfile != null)
            {
                var user = new Modify_content { picture = CurrentProfile.picture };
                modify.attr = user;
            }
            modify.userId = AntSdkService.AntSdkCurrentUserInfo.userId;
            OnQueryInfoHandler(modify);
        }
    }
}
