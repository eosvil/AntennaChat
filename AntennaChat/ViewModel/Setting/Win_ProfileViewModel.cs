using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AntennaChat.Views;
using Microsoft.Expression.Interactivity.Core;
using System.Windows.Media;
using SDK.AntSdk;
using Color = System.Windows.Media.Color;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Setting
{
    public class Win_ProfileViewModel : WindowBaseViewModel
    {
        private Action _close;
        public Win_ProfileViewModel(Action close)
        {
            _profileViewModel = new ProfileViewModel();
            _changePasswordView = new ChangePasswordViewModel();
            _sysSettingContent=new ChangeKeyViewModel();
            _sysSettingContent.ChangeKeySuccessEvent += _sysSettingContent_ChangeKeySuccessEvent;
            _profileViewModel.SetHeadImage += _profileViewModel_SetHeadImage;
            _profileViewModel.SaveProfile += _profileViewModel_SaveProfile;
            _profileViewModel.QueryInfoHandler += _profileViewModel_QueryInfoHandler;
            _changePasswordView.ChangePasswordSuccessEvent += _changePasswordView_ChangePasswordSuccessEvent;
            this._close = close;
        }
        #region 属性
        /// <summary>
        /// 要绑定和切换的ViewModel
        /// 用于个人信息、头像设置
        /// </summary>
        private object _viewModel;
        public object ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel == value)
                {
                    return;
                }
                _viewModel = value;
                RaisePropertyChanged(() => ViewModel);
            }
        }
        /// <summary>
        /// 个人信息设置
        /// </summary>
        private ProfileViewModel _profileViewModel;
        public ProfileViewModel ProfileViewModel
        {
            get { return _profileViewModel; }
            set
            {
                _profileViewModel = value;
                RaisePropertyChanged(() => ProfileViewModel);
            }
        }
        /// <summary>
        /// 头像设置
        /// </summary>
        private HeadImageViewModel _headImageViewModel;
        public HeadImageViewModel HeadImageViewModel
        {
            get { return _headImageViewModel; }
            set
            {
                _headImageViewModel = value;
                RaisePropertyChanged(() => HeadImageViewModel);
            }
        }
        /// <summary>
        /// 密码设置
        /// </summary>
        private ChangePasswordViewModel _changePasswordView;
        public ChangePasswordViewModel ChangePasswordView
        {
            get { return _changePasswordView; }
            set
            {
                _changePasswordView = value;
                RaisePropertyChanged(() => ChangePasswordView);
            }
        }
        /// <summary>
        /// 热键设置
        /// </summary>
        private ChangeKeyViewModel _sysSettingContent;
        public ChangeKeyViewModel SysSettingContent
        {
            get { return _sysSettingContent; }
            set
            {
                _sysSettingContent = value;
                RaisePropertyChanged(() => SysSettingContent);
            }
        }
        #endregion

        private void _changePasswordView_ChangePasswordSuccessEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this._close.Invoke();
            }));
            OnCloseWin();
            ChangePasswordSuccessEvent?.Invoke(this,EventArgs.Empty);
        }

        public event EventHandler ChangePasswordSuccessEvent;


        /// <summary>
        /// 保存用户信息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _profileViewModel_SaveProfile(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this._close.Invoke();
            }));
            OnCloseWin();
        }

        /// <summary>
        /// 保存按键修改处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _sysSettingContent_ChangeKeySuccessEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this._close.Invoke();
            }));
            OnCloseWin();
        }
        /// <summary>
        /// 窗体关闭
        /// </summary>
        public delegate void CloseWinHandler();//关闭窗体委托
        public event CloseWinHandler CloseWinEvent;
        private void OnCloseWin()
        {
            this.CloseWinEvent?.Invoke();
        }
        /// <summary>
        /// 点击设置头像处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _profileViewModel_SetHeadImage(object sender, EventArgs e)
        {
            _headImageViewModel = new HeadImageViewModel(AntSdkService.AntSdkCurrentUserInfo.picture);
            _headImageViewModel.ButtonClickEvent += _headImageViewModel_ButtonClickEvent;
            ViewModel = _headImageViewModel;
        }
        /// <summary>
        /// 上传完成或者取消上传头像
        /// </summary>
        /// <param name="filePath"></param>
        private void _headImageViewModel_ButtonClickEvent(string filePath)
        {
            _profileViewModel.HeadPic = filePath;
            ViewModel = _profileViewModel;
        }
        /// <summary>
        /// 窗体加载
        /// </summary>
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
                              ViewModel = _profileViewModel;
                              _profileViewModel.InitUserInfo();
                          });
                }
                return this._Loaded;
            }
        }
        /// <summary>
        /// 查询用户信息之后用于同步头像
        /// </summary>
        /// <param name="modify"></param>
        private void _profileViewModel_QueryInfoHandler(AntSdkReceivedUserMsg.Modify modify)
        {
            OnQueryInfoHandler(modify);
        }
        /// <summary>
        /// 查询用户信息同步事件
        /// </summary>
        /// <param name="modify"></param>
        public delegate void QueryInfo(AntSdkReceivedUserMsg.Modify modify);
        public event QueryInfo QueryInfoHandler;
        protected virtual void OnQueryInfoHandler(AntSdkReceivedUserMsg.Modify modify)
        {
            QueryInfoHandler?.Invoke(modify);
        }
    }
}
