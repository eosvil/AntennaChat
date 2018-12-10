using Antenna.Framework;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Antenna.Model;
using AntennaChat.Views;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Talk
{
    public class MassMsgViewModel : PropertyNotifyObject
    {
        private DispatcherTimer _waitingTimer;
        public string MessageId;
        private string tempNames;
        private string names;
        private AntSdkMassMsgCtt MassMsgCtt;
        #region 构造器
        //public MassMsgViewModel(MassMsgModel massMsgModel)
        //{
        //    MessageId = massMsgModel.MessageId;
        //    this.SendTime = DataConverter.FormatTimeByTimeStamp(massMsgModel.SendTime);//从消息来的为时间戳
        //    //this.SendStateImage= massMsgModel.SendStateImage;
        //    this.MsgContent = massMsgModel.MsgContent;
        //    this.TargetUsers = massMsgModel.TargetUsers;
        //    this.OperateMode = massMsgModel.OperateMode;
        //    this.names = this.TargetUsers;
        //    if (massMsgModel.SendMsgState == GlobalVariable.SendMsgState.Sending)
        //    {
        //        this.SendFailureImageVisibility = Visibility.Collapsed;
        //        this.SendingAnimationVisible = Visibility.Visible;
        //        StartWaitingTimer();
        //    }
        //    else if (massMsgModel.SendMsgState == GlobalVariable.SendMsgState.Failure)
        //    {
        //        this.SendFailureImageVisibility = Visibility.Visible;
        //        this.SendingAnimationVisible = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        this.SendFailureImageVisibility = Visibility.Collapsed;
        //        this.SendingAnimationVisible = Visibility.Collapsed;
        //    }
        //}

        public MassMsgViewModel(AntSdkMassMsgCtt msg, GlobalVariable.SendMsgState sendMsgState)
        {
            this.MassMsgCtt = msg;
            MessageId = msg.messageId;
            if (string.IsNullOrEmpty(msg.sendTime))
            {
                this.SendTime = DataConverter.FormatTimeByTimeStamp(DataConverter.ConvertDateTimeInt(DateTime.Now).ToString() + "000");
            }
            else
            {
                this.SendTime = DataConverter.FormatTimeByTimeStamp(msg.sendTime);//从消息来的为时间戳
            }
            string[] userIds = msg.targetId.Split(',');
            string[] userNames = AntSdkService.AntSdkListContactsEntity.users.Where((c => userIds.Contains(c.userId))).Select(c => c.userName).ToArray();
            this.TargetUsers = string.Join("，", userNames);
            this.MsgContent = msg.content;
            this.OperateMode = userIds.Length + "人";
            this.names = this.TargetUsers;
            if (sendMsgState == GlobalVariable.SendMsgState.Sending)
            {
                this.SendFailureImageVisibility = Visibility.Collapsed;
                this.SendingAnimationVisible = Visibility.Visible;
                StartWaitingTimer();
            }
            else if (sendMsgState == GlobalVariable.SendMsgState.Failure)
            {
                this.SendFailureImageVisibility = Visibility.Visible;
                this.SendingAnimationVisible = Visibility.Collapsed;
            }
            else
            {
                this.SendFailureImageVisibility = Visibility.Collapsed;
                this.SendingAnimationVisible = Visibility.Collapsed;
            }
        }
        #endregion

        #region 属性
        private string _sendTime;
        /// <summary>
        /// 发送时间
        /// </summary>
        public string SendTime
        {
            get { return this._sendTime; }
            set
            {
                this._sendTime = value;
                RaisePropertyChanged(() => SendTime);
            }
        }

        private bool _isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                this._isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private Visibility _sendFailureImageVisibility;
        /// <summary>
        /// 发送失败状态图标是否可见
        /// </summary>
        public Visibility SendFailureImageVisibility
        {
            get { return this._sendFailureImageVisibility; }
            set
            {
                this._sendFailureImageVisibility = value;
                RaisePropertyChanged(() => SendFailureImageVisibility);
            }
        }

        private Visibility _sendingAnimationVisible;
        /// <summary>
        /// 发送中状态图标是否可见
        /// </summary>
        public Visibility SendingAnimationVisible
        {
            get { return this._sendingAnimationVisible; }
            set
            {
                this._sendingAnimationVisible = value;
                RaisePropertyChanged(() => SendingAnimationVisible);
            }
        }

        private string _MsgContent;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MsgContent
        {
            get { return this._MsgContent; }
            set
            {
                this._MsgContent = value;
                RaisePropertyChanged(() => MsgContent);
            }
        }

        private string _TargetUsers;
        /// <summary>
        /// 群发对象
        /// </summary>
        public string TargetUsers
        {
            get { return this._TargetUsers; }
            set
            {
                this._TargetUsers = value;
                RaisePropertyChanged(() => TargetUsers);
            }
        }

        private string _OperateMode;
        /// <summary>
        /// 操作类型（显示人数、展开、收起）
        /// </summary>
        public string OperateMode
        {
            get { return this._OperateMode; }
            set
            {
                this._OperateMode = value;
                RaisePropertyChanged(() => OperateMode);
            }
        }
        #endregion
        #region 命令
        private ICommand _btnOperate_MouseLeftButtonDown;
        /// <summary>
        /// 鼠标单击展开/收起事件
        /// </summary>
        public ICommand btnOperate_MouseLeftButtonDown
        {
            get
            {
                if (this._btnOperate_MouseLeftButtonDown == null)
                {
                    this._btnOperate_MouseLeftButtonDown = new DefaultCommand(o =>
                    {
                        TextBox txtTargetUsers = o as TextBox;
                        if (o == null) return;
                        if (OperateMode == "收起")
                        {
                            txtTargetUsers.MaxLines = 3;
                            OperateMode = "展开";
                            TargetUsers = tempNames;
                        }
                        else if (OperateMode == "展开")
                        {
                            txtTargetUsers.MaxLines = System.Int16.MaxValue;
                            OperateMode = "收起";
                            TargetUsers = names;
                        }
                    });
                }
                return this._btnOperate_MouseLeftButtonDown;
            }
        }
        private ICommand _SendFailureImage_MouseLeftButtonDown;
        #region 消息重发
        /// <summary>
        /// 消息重发
        /// </summary>
        public ICommand SendFailureImage_MouseLeftButtonDown
        {
            get
            {
                this._SendFailureImage_MouseLeftButtonDown = new DefaultCommand(o =>
                {
                    SendMassMsg();
                });
                return this._SendFailureImage_MouseLeftButtonDown;
            }
        }
        public delegate void SendMassMsgDelegate(AntSdkMassMsgCtt msg);
        public static event SendMassMsgDelegate SendMassMsgEvent;
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="o"></param>
        private void SendMassMsg()
        {
            //msg.ctt = new AntSdkMassMsgCtt();
            //msg.ctt.messageId = PublicTalkMothed.timeStampAndRandom();
            //msg.ctt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
            //msg.ctt.targetId = string.Join(",", ContactUsers.Select(c => c.userId).ToArray());
            //msg.ctt.companyCode = GlobalVariable.CompanyCode;
            //msg.ctt.content = MsgContent;
            //msg.ctt.os = (int)GlobalVariable.OSType.PC;
            //msg.ctt.sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, GlobalVariable.MassAssistantId);
            //msg.ctt.chatIndex = null;
            //msg.ctt.sendTime = null;
            MassMsg msg = new MassMsg();
            msg.mtp = (int)GlobalVariable.MsgType.MassMsg;
            MassMsgCtt.chatIndex = null;
            MassMsgCtt.sendTime = null;
            string errMsg = "";
            //TODO:AntSdk_Modify
            //msg.ctt = this.MassMsgCtt;
            //if (MqttService.Instance.Publish(GlobalVariable.TopicClass.MessageSend, msg, ref errMsg, NullValueHandling.Ignore))
            //{
            //    this.SendFailureImageVisibility = Visibility.Collapsed;
            //    this.SendingAnimationVisible = Visibility.Visible;
            //    StartWaitingTimer();
            //    SendMassMsgEvent?.Invoke(msg.ctt);
            //}
            //else
            //{
            //    MessageBoxWindow.Show("消息发送失败!" + errMsg, GlobalVariable.WarnOrSuccess.Warn);
            //}
        }
        #endregion

        /// <summary>
        /// 加载
        /// </summary>
        private ICommand _LoadedCommand;
        public ICommand LoadedCommand
        {
            get
            {
                if (this._LoadedCommand == null)
                {
                    this._LoadedCommand = new DefaultCommand(o =>
                    {
                        TextBox txtTargetUsers = o as TextBox;
                        if (o == null) return;
                        if (txtTargetUsers.LineCount > 3)
                        {
                            txtTargetUsers.MaxLines = 3;
                            OperateMode = "展开";
                            tempNames = txtTargetUsers.GetLineText(0) + txtTargetUsers.GetLineText(1);
                            string threeLine = txtTargetUsers.GetLineText(2);
                            threeLine = threeLine.Remove(threeLine.Length - 10);
                            tempNames += threeLine + "......";
                            TargetUsers = tempNames;
                        }
                    });
                }
                return this._LoadedCommand;
            }
        }


        #endregion
        #region 其他方法
        public void HandleMassMsgReceipt(AntSdkChatMsg.ChatBase massMsgReceipt)
        {
            this.SendingAnimationVisible = Visibility.Collapsed;
            this.SendFailureImageVisibility = Visibility.Collapsed;
            //this.SendTime = massMsgReceipt.sendTime;
            this.SendTime = DataConverter.FormatTimeByTimeStamp(massMsgReceipt.sendTime);//从消息来的为时间戳
            //this._sendTimeStamp = massMsgReceipt.sendTime;
            _waitingTimer.Stop();
        }

        /// <summary>
        /// 每隔一秒刷新一次等待时间
        /// </summary>
        /// 作者：赵雪峰 20160528
        private void StartWaitingTimer()
        {
            _waitingTimer = new DispatcherTimer();
            _waitingTimer.Tick += waitingTimer_Tick;
            _waitingTimer.Interval = TimeSpan.FromMilliseconds(3000);
            _waitingTimer.Start();
        }
        private void waitingTimer_Tick(object sender, EventArgs e)
        {
            this.SendingAnimationVisible = Visibility.Collapsed;
            this.SendFailureImageVisibility = Visibility.Visible;
            _waitingTimer.Stop();
        }
        #endregion
    }
}
