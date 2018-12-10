using AntennaChat.Command;
using Microsoft.Practices.Prism.Commands;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntennaChat.ViewModel.Talk;
using SDK.AntSdk;
using AntennaChat.Views;
using Antenna.Framework;

namespace AntennaChat.ViewModel.AudioVideo
{
    public class RequestViewModel:WindowBaseViewModel
    {
        private Window _requestWindow;
        private Action _close;
        public AntSdkContact_User _targetUser;
        /// <summary>
        /// 频道ID
        /// </summary>
        public long ModelId;
        public RequestViewModel(Action close, AntSdkContact_User user, long modelId)
        {
            _close = close;
            ModelId = modelId;
            _targetUser = user;
            Title= $"{_targetUser.userName}向您发送了一个语音电话请求...";
        }
        /// <summary>
        /// 加载
        /// </summary>
        private ActionCommand<Window> _loadedCommand;
        public ActionCommand<Window> LoadedCommand
        {
            get
            {
                if (this._loadedCommand == null)
                {
                    this._loadedCommand = new ActionCommand<Window>(
                           win =>
                           {
                               _requestWindow = win;
                               SetWindowLocation();
                               GlobalVariable.isRequestShow = true;
                           });
                }
                return this._loadedCommand;
            }
        }
        /// <summary>
        /// 设置窗体位置
        /// </summary>
        private void SetWindowLocation()
        {
            _requestWindow.Top = SystemParameters.WorkArea.Height - _requestWindow.Height - 4;
            _requestWindow.Left = SystemParameters.WorkArea.Width - _requestWindow.Width - 4;
            _requestWindow.Topmost = true;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void CloseWin()
        {
            GlobalVariable.isRequestShow = false;
            ModelId = 0;
            this._close.Invoke();
        }
        /// <summary>
        /// 接听处理
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channel_id"></param>
        public delegate void DelAcceptRequest(AntSdkContact_User user, long channel_id);
        public event DelAcceptRequest HandlerAcceptRequest;
        public void OnHandlerAcceptRequest(AntSdkContact_User user, long channel_id)
        {
            HandlerAcceptRequest?.Invoke(user, channel_id);
        }
        /// <summary>
        /// 接听语音电话请求
        /// </summary>
        public ICommand AcceptCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    OnHandlerAcceptRequest(_targetUser, ModelId);
                    CloseWin();
                });
            }
        }
        /// <summary>
        /// 拒绝语音电话
        /// </summary>
        public ICommand RefuseCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    RefuseAudio();
                });
            }
        }
        /// <summary>
        /// 标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }
        /// <summary>
        /// 拒绝
        /// </summary>
        public void RefuseAudio()
        {
            if (!AntSdkService.AntSdkIsConnected)
            {
                MessageBoxWindow.Show("提示", "网络连接已断开,无法进行语音电话！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                AudioChat._currentChannelId = 0;
                CloseWin();
                return;
            }
            AudioChat.AudioResult(ModelId, false);
            AudioChat._currentChannelId = 0;
            CloseWin();
        }
    }
}
