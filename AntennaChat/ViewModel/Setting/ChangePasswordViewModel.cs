using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Resource;
using Microsoft.Expression.Interactivity.Core;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using AntennaChat.Views;

namespace AntennaChat.ViewModel.Setting
{
    public class ChangePasswordViewModel : PropertyNotifyObject
    {
        private static readonly Color NormalColor = Color.FromRgb(0xE1, 0xE1, 0xE1),
            AlarmColor = Color.FromRgb(0xFE, 0x30, 0x00);
        private  static readonly BitmapImage ClearTextImage=new BitmapImage(new Uri("../../Images/开.png", UriKind.Relative)),
            NotClearTextImage=new BitmapImage(new Uri("../../Images/关.png",UriKind.Relative));
        private string _password ;
        /// <summary>
        /// 原密码
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        private string _passwordTip;
        /// <summary>
        /// 原密码输入框下方的提示
        /// </summary>
        public string PasswordTip
        {
            get { return _passwordTip; }
            set
            {
                _passwordTip = value;
                RaisePropertyChanged("PasswordTip");
            }
        }

        private Brush _passwordBorderBrush=new SolidColorBrush(NormalColor);
        /// <summary>
        /// 原密码输入框边框颜色
        /// </summary>
        public Brush PasswordBorderBrush
        {
            get { return _passwordBorderBrush; }
            set
            {
                _passwordBorderBrush = value;
                RaisePropertyChanged("PasswordBorderBrush");
            }
        }

        private string _newPassword;
        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value;
                RaisePropertyChanged("NewPassword");
            }
        }

        private string _newPasswordTip;
        /// <summary>
        /// 新密码输入框下方的提示
        /// </summary>
        public string NewPasswordTip
        {
            get { return _newPasswordTip; }
            set
            {
                _newPasswordTip = value;
                RaisePropertyChanged("NewPasswordTip");
            }
        }

        private Brush _newPasswordBorderBrush = new SolidColorBrush(Color.FromRgb(0xE1, 0xE1, 0xE1));
        /// <summary>
        /// 新密码输入框边框颜色
        /// </summary>
        public Brush NewPasswordBorderBrush
        {
            get { return _newPasswordBorderBrush; }
            set
            {
                _newPasswordBorderBrush = value;
                RaisePropertyChanged("NewPasswordBorderBrush");
            }
        }

        private bool _isShowNewPasswordEncryption = true;
        /// <summary>
        /// 新密码是否以加密显示
        /// </summary>
        public bool IsShowNewPasswordEncryption
        {
            get { return _isShowNewPasswordEncryption; }
            set
            {
                _isShowNewPasswordEncryption = value;
                RaisePropertyChanged("IsShowNewPasswordEncryption");
                RaisePropertyChanged("IsShowNewPasswordClearText");
            }
        }

        /// <summary>
        /// 新密码是否明文显示
        /// </summary>
        public bool IsShowNewPasswordClearText => !_isShowNewPasswordEncryption;

        ImageSource _isShowNewPasswordButtonImageSource= NotClearTextImage;
        /// <summary>
        /// 调整新密码显示方式的按钮的图标
        /// </summary>
        public ImageSource IsShowNewPasswordButtonImageSource
        {
            get { return _isShowNewPasswordButtonImageSource; }
            set
            {
                _isShowNewPasswordButtonImageSource = value;
                RaisePropertyChanged("IsShowNewPasswordButtonImageSource");
            }
        }

        private string _confirmPassword;
        /// <summary>
        /// 二次确认的新密码
        /// </summary>
        public  string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
                RaisePropertyChanged("ConfirmPassword");
            }
        }

        private string _confirmPasswordTip;
        /// <summary>
        /// 二次确认的新密码输入框下方的提示
        /// </summary>
        public string ConfirmPasswordTip
        {
            get { return _confirmPasswordTip; }
            set
            {
                _confirmPasswordTip = value;
                RaisePropertyChanged("ConfirmPasswordTip");
            }
        }

        private Brush _confirmPasswordBorderBrush = new SolidColorBrush(Color.FromRgb(0xE1, 0xE1, 0xE1));
        /// <summary>
        /// 二次确认密码输入框边框颜色
        /// </summary>
        public Brush ConfirmPasswordBorderBrush
        {
            get { return _confirmPasswordBorderBrush; }
            set
            {
                _confirmPasswordBorderBrush = value;
                RaisePropertyChanged("ConfirmPasswordBorderBrush");
            }
        }

        private bool _isShowConfirmPasswordEncryption = true;
        /// <summary>
        /// 二次确认的密码是否以加密显示
        /// </summary>
        public bool IsShowConfirmPasswordEncryption
        {
            get { return _isShowConfirmPasswordEncryption; }
            set
            {
                _isShowConfirmPasswordEncryption = value;
                RaisePropertyChanged("IsShowConfirmPasswordEncryption");
                RaisePropertyChanged("IsShowConfirmPasswordClearText");
            }
        }

        /// <summary>
        /// 二次确认的密码是否以明文显示
        /// </summary>
        public bool IsShowConfirmPasswordClearText
        {
            get { return !_isShowConfirmPasswordEncryption; }
        }

        ImageSource _isShowConfirmPasswordButtonImageSource = NotClearTextImage;
        /// <summary>
        /// 调整二次确认密码显示方式的按钮的图标
        /// </summary>
        public ImageSource IsShowConfirmPasswordButtonImageSource
        {
            get { return _isShowConfirmPasswordButtonImageSource; }
            set
            {
                _isShowConfirmPasswordButtonImageSource = value;
                RaisePropertyChanged("IsShowConfirmPasswordButtonImageSource");
            }
        }

        private string _errMsg;
        /// <summary>
        /// 保存错误提示
        /// </summary>
        public string ErrMsg
        {
            get { return _errMsg; }
            set
            {
                _errMsg = value;
                RaisePropertyChanged("ErrMsg");
            }
        }


        private bool _isShowErrMsg;
        /// <summary>
        /// 是否显示保存时的错误提示
        /// </summary>
        public bool IsShowErrMsg
        {
            get { return _isShowErrMsg; }
            set
            {
                _isShowErrMsg = value;
                RaisePropertyChanged("IsShowErrMsg");
            }
        }

        /// <summary>
        /// 是否能保存
        /// </summary>
        public bool CanSaveFlag=> ValidateOldPasswordFlag && ValidateNewPasswordFlag && ValidateConfirmPasswordFlag;

        /// <summary>
        /// 验证原密码是否输入正确
        /// </summary>
        public ICommand ValidateOldPasswordCommand => new DefaultCommand(ValidateOldPassword);

        /// <summary>
        /// 验证新密码是否符合规则
        /// </summary>
        public ICommand ValidateNewPasswordCommand => new DefaultCommand(ValidateNewPassword);
        /// <summary>
        /// 验证二次确认的密码是否与输入的新密码一致
        /// </summary>
        public ICommand ValidateConfirmPasswordCommand=> new DefaultCommand(ValidateConfirmPassword);
        /// <summary>
        /// 保存密码变更
        /// </summary>
        public ICommand SaveNewPasswordCommand => new DefaultCommand(SaveNewPassword);


        public ICommand SetNewPasswordIsClearTextCommand
        {
            get
            {
                return  new ActionCommand(() =>
                {
                    if (IsShowNewPasswordEncryption)
                    {
                        IsShowNewPasswordEncryption = false;
                        IsShowNewPasswordButtonImageSource = ClearTextImage;
                    }
                    else
                    {
                        IsShowNewPasswordEncryption = true;
                        IsShowNewPasswordButtonImageSource = NotClearTextImage;
                    }
                });
            }
        }

        public ICommand SetConfirmPasswordIsClearTextCommand
        {
            get
            {
                return new ActionCommand(() =>
                {
                    if (IsShowConfirmPasswordEncryption)
                    {
                        IsShowConfirmPasswordEncryption = false;
                        IsShowConfirmPasswordButtonImageSource = ClearTextImage;
                    }
                    else
                    {
                        IsShowConfirmPasswordEncryption = true;
                        IsShowConfirmPasswordButtonImageSource = NotClearTextImage;
                    }
                });
            }
        }

        private void SaveNewPassword(object obj)
        {
            if (ChangePassword())
            {
                ErrMsg = string.Empty;
                //MessageBoxWindow.Show("提示", "密码修改成功，请重新登录！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Success);
                //ChangePasswordSuccessEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBoxWindow.Show("提示", ErrMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
            }
        }

        private bool _validateOldPasswordFlag = false;
        /// <summary>
        /// 指示原密码是否验证通过
        /// </summary>
        public bool ValidateOldPasswordFlag
        {
            get { return _validateOldPasswordFlag; }
            set
            {
                if (_validateOldPasswordFlag != value)
                {
                    _validateOldPasswordFlag = value;
                    PasswordBorderBrush = _validateOldPasswordFlag ? new SolidColorBrush(NormalColor) : new SolidColorBrush(AlarmColor);
                    RaisePropertyChanged("CanSaveFlag");
                }
            }
        }

        private void ValidateOldPassword(object obj)
        {
            string msg = string.Empty;
            ValidateOldPasswordFlag = ValidateOldPassword(Password, ref msg);
            PasswordTip = msg;
        }

        private bool ValidateOldPassword(string oldPassword,ref string msg)
        {
            if (string.IsNullOrEmpty(Password))
            {
                msg = "原密码不能为空！";
                return false;
            }
            if (oldPassword != AntSdkService.AntSdkLoginOutput.PWD)
            {
                msg = "*密码输入有误";
                return false;
            }
            return true;
        }

        private bool _validateNewPasswordFlag = false;
        /// <summary>
        /// 指示新密码是否符合规范
        /// </summary>
        public bool ValidateNewPasswordFlag
        {
            get { return _validateNewPasswordFlag; }
            set
            {
                if (_validateNewPasswordFlag != value)
                {
                    _validateNewPasswordFlag = value;
                    RaisePropertyChanged("CanSaveFlag");
                }
                NewPasswordBorderBrush = _validateNewPasswordFlag ? new SolidColorBrush(NormalColor) : new SolidColorBrush(AlarmColor);
            }
        }

        private void ValidateNewPassword(object obj)
        {
            string msg = string.Empty;
            ValidateNewPasswordFlag = ValidateNewPassword(NewPassword, ref msg);
            NewPasswordTip = msg;
            ValidateConfirmPassword(null);
        }

        private bool ValidateNewPassword(string newPassword, ref string msg)
        {
            bool validateResult = false;
            msg = string.Empty;
            if (string.IsNullOrEmpty(newPassword)||newPassword.Length<6||newPassword.Length>16)
            {
                msg = "请输入6-16位密码,数字/字母/下划线";
                return false;
            }
            else
            {
                Regex reg = new Regex(@"^\w+$");
                if (!reg.IsMatch(newPassword))
                {
                    msg = "请输入6-16位密码,数字/字母/下划线";
                }
                else
                    validateResult = true;
            }
            return validateResult;
        }

        private bool _validateConfirmPasswordFlag = false;
        /// <summary>
        /// 指示二次确认密码是否与新密码一致
        /// </summary>
        public bool ValidateConfirmPasswordFlag
        {
            get { return _validateConfirmPasswordFlag; }
            set
            {
                if (_validateConfirmPasswordFlag != value)
                {
                    _validateConfirmPasswordFlag = value;
                    RaisePropertyChanged("CanSaveFlag");
                }
            }
        }

        private void ValidateConfirmPassword(object obj)
        {
            string msg = string.Empty;
            ValidateConfirmPasswordFlag = ValidateConfirmPassword(NewPassword,ConfirmPassword);
        }

        private bool ValidateConfirmPassword(string newPassword,string confirmPassword)
        {
            string msg = string.Empty;
            bool validateResult = false;
            if (!string.IsNullOrEmpty(ConfirmPassword) && !newPassword.Equals(confirmPassword))
            {
                msg = "新密码输入不一致";
            }
            else if(!string.IsNullOrEmpty(ConfirmPassword) && newPassword.Equals(confirmPassword))
            {
                validateResult = true;
            }
            ConfirmPasswordBorderBrush = string.IsNullOrEmpty(msg) ? new SolidColorBrush(NormalColor) : new SolidColorBrush(AlarmColor);
            ConfirmPasswordTip = msg;
            return validateResult;
        }

        private bool ChangePassword()
        {
            bool changeResult = false;
            try
            {
                var errCode = 0;
                string errMsg = string.Empty;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Restart();

                //ChangePasswordInput input = new ChangePasswordInput
                //{
                //    newPwd = NewPassword,
                //    oldPwd = Password,
                //    version = GlobalVariable.Version,
                //    token = AntSdkService.AntSdkLoginOutput.token,
                //    userId = AntSdkService.AntSdkCurrentUserInfo.userId
                //};
                //ChangePasswordOutput output = new ChangePasswordOutput();
                //TODO:AntSdk_Modify 
                //DONE:AntSdk_Modify 
                AntSdkChangePasswordInput changePasswordInput = new AntSdkChangePasswordInput();
                changePasswordInput.newPassword = NewPassword;
                changePasswordInput.password = Password;
                var isResult= AntSdkService.AntSdkChangePassword(changePasswordInput, ref errCode, ref errMsg);
                if (isResult)
                {
                    changeResult = true;
                    string errMessage = string.Empty;
                    List<AccountInfo> listAccountInfo = new List<AccountInfo>();
                    DataConverter.XmlToEntity(Environment.CurrentDirectory + "/AccountCache.xml", ref listAccountInfo,
                        ref errMessage);
                    if (listAccountInfo.Any())
                    {
                        if (AntSdkService.AntSdkCurrentUserInfo != null)
                        {
                            AccountInfo info =
                                listAccountInfo.FirstOrDefault(
                                    c => c.ID == AntSdkService.AntSdkCurrentUserInfo.loginName);
                            if (info != null)
                            {
                                info.LastLoginTime = DateTime.Now;
                                info.OnLine = 0;
                                info.RememberPwd = false;
                                info.AutoLogin = false;
                                info.Password = string.Empty;
                            }
                            DataConverter.EntityToXml<List<AccountInfo>>(
                                System.Environment.CurrentDirectory + "/AccountCache.xml", listAccountInfo,
                                ref errMessage);
                        }
                    }
                }
                //if ((new HttpService()).ChangePassword(input, ref output, ref errMsg))
                //{
                //    changeResult = true;
                //    string errMessage = string.Empty;
                //    List<AccountInfo> listAccountInfo = new List<AccountInfo>();
                //    DataConverter.XmlToEntity(Environment.CurrentDirectory + "/AccountCache.xml", ref listAccountInfo, ref errMessage);
                //    if (listAccountInfo.Any())
                //    {
                //        if (AntSdkService.AntSdkCurrentUserInfo != null)
                //        {
                //            AccountInfo info = listAccountInfo.FirstOrDefault(c => c.ID == AntSdkService.AntSdkCurrentUserInfo.loginName);
                //            if (info != null)
                //            {
                //                info.LastLoginTime = DateTime.Now;
                //                info.OnLine = 0;
                //                info.RememberPwd = false;
                //                info.AutoLogin = false;
                //                info.Password = string.Empty;
                //            }
                //            DataConverter.EntityToXml<List<AccountInfo>>(System.Environment.CurrentDirectory + "/AccountCache.xml", listAccountInfo, ref errMessage);
                //        }
                //    }
                //}
                stopWatch.Stop();
                LogHelper.WriteDebug(string.Format("[ChangePassword failed ({0}毫秒)]", stopWatch.Elapsed.TotalMilliseconds));
                ErrMsg = errMsg;
            }
            catch (Exception ex)
            {
                changeResult = false;
                ErrMsg = $"修改密码时发生异常：{ex.Message}";
                LogHelper.WriteError(ErrMsg);
            }
            return changeResult;
        }

        /// <summary>
        /// 修改密码成功
        /// </summary>
        public event EventHandler  ChangePasswordSuccessEvent = null;
    }
}
