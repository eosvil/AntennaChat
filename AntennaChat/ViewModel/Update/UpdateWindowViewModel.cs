using Antenna.Framework;
using Antenna.Model;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Update
{
    public class UpdateWindowViewModel : PropertyNotifyObject
    {
        private AntSdkReceivedOtherMsg.VersionHardUpdate _iData = null;
        bool update = false;
        public UpdateWindowViewModel(string describe, bool update)
        {
            this.update = update;
            if (update == true)
            {
                isVisibility = "Hidden";
                isShowBtton = "Collapsed";
                winTitle = "硬更新";
            }
            else
            {
                winTitle = "软更新";
            }
            //else
            //{
            //    isVisibility = "Visible";
            //}
            //this._iData = iData;
            
            txtUpdateContent = describe;
        }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        private string _isVisibility;
        public string isVisibility
        {
            get { return _isVisibility; }
            set
            {
                this._isVisibility = value;
                RaisePropertyChanged(() => isVisibility);
            }
        }
        /// <summary>
        /// 是否显示暂不更新按钮
        /// </summary>
        private string _isShowBtton;
        public string isShowBtton
        {
            get { return _isShowBtton; }
            set
            {
                this._isShowBtton = value;
                RaisePropertyChanged(() => isShowBtton);
            }
        }
        /// <summary>
        /// 窗体标识
        /// </summary>
        private string _winTitle;
        public string winTitle
        {
            get { return _winTitle; }
            set
            {
                this._winTitle = value;
                RaisePropertyChanged(() => winTitle);
            }
        }
        /// <summary>
        /// 更新内容
        /// </summary>
        private string _txtUpdateContent;
        public string txtUpdateContent
        {
            get { return _txtUpdateContent; }
            set
            {
                this._txtUpdateContent = value;
                RaisePropertyChanged(() => txtUpdateContent);
            }
        }
        #region 命令
        /// <summary>
        /// 立即更新
        /// </summary>
        public ICommand btnUpdateCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Task<bool> isTask = Task.Factory.StartNew(() => isStart());
                    if (isTask.Result == true)
                    {
                        Environment.Exit(0);
                    }
                });
            }
        }
        /// <summary>
        /// 启动更新程序
        /// </summary>
        /// <returns></returns>
        public static bool isStart()
        {
            bool b = false;
            b = publicMethod.startApplication();
            return b;
        }
        /// <summary>
        ///暂不更新
        /// </summary>
        public ICommand btnNoUpdateCommand
        {
            get
            {
                return new DelegateCommand<Window>((obj) =>
                {
                    if (update)
                    {
                        MessageBoxResult dr = MessageBox.Show("本次为强制更新,请点击立即更新!", "温馨提示", MessageBoxButton.YesNo);
                    }
                    else
                    {
                        obj.Hide();
                    }
                });
            }
        }
        /// <summary>
        /// 关闭更新窗体
        /// </summary>
        public ICommand CloseWindow
        {
            get
            {
                return new DelegateCommand<Window>((obj) =>
                {
                    if (update == false)
                    {
                        MessageBoxResult dr = MessageBox.Show("确定退出更新吗?", "温馨提示", MessageBoxButton.YesNo);
                        if (dr.ToString() == "Yes")
                        {
                            obj.Hide();
                        }
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                });
            }
        }
        /// <summary>
        /// 窗体移动事件
        /// </summary>
        public ICommand updateMove
        {
            get
            {
                return new DelegateCommand<Window>((obj) =>
                {
                    obj.DragMove();
                });
            }
        }
        #endregion


    }
}