using AntennaChat.Command;
using AntennaChat.Views.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class MassMsgSentViewModel : WindowBaseViewModel
    {
        public List<AntSdkContact_User> ContactUsers = new List<AntSdkContact_User>();
        #region 构造器
        public MassMsgSentViewModel(List<AntSdkContact_User> contactUsers = null)
        {
            if (contactUsers != null)
            {
                ContactUsers.Clear();
                ContactNames = string.Empty;
                ContactUsers = contactUsers;
                foreach (var contactUser in contactUsers)
                {
                    if (contactUser.userId == AntSdkService.AntSdkLoginOutput.userId) continue;
                    ContactNames += contactUser.userName + "，";
                }
                ContactNames = ContactNames.TrimEnd('，');
            }
        }
        #endregion
        #region 属性
        private string _contactNames;
        /// <summary>
        /// 选择人姓名的显示
        /// </summary>
        public string ContactNames
        {
            get { return _contactNames; }
            set
            {
                _contactNames = value;
                RaisePropertyChanged(() => ContactNames);
            }
        }

        private string _msgContent;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MsgContent
        {
            get { return _msgContent; }
            set
            {
                _msgContent = value;
                RaisePropertyChanged(() => MsgContent);
            }
        }
        #endregion
        #region 命令
        private ICommand _okCommand;
        /// <summary>
        /// 点击确定发送消息
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                return this._okCommand ??
                       (this._okCommand = new DefaultCommand(o =>
                       {
                           SendMassMsg(o);
                       }));
            }
        }
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                return this._cancelCommand ??
                       (this._cancelCommand = new DefaultCommand(o =>
                       {
                           (o as System.Windows.Window)?.Close();
                       }));
            }
        }
        private ICommand _selectContactsCommand;
        public ICommand SelectContactsCommand
        {
            get
            {
                if (this._selectContactsCommand == null)
                {
                    this._selectContactsCommand = new DefaultCommand(o =>
                    {
                        MultiContactsSelectView win = new MultiContactsSelectView();
                        MultiContactsSelectViewModel vm = new MultiContactsSelectViewModel(ContactUsers.Select(c => c.userId).ToList());
                        win.DataContext = vm;
                        win.ShowInTaskbar = false;
                        win.Owner = (o as System.Windows.Window);
                        win.ShowDialog();
                        if (vm.GroupMemberList != null && vm.GroupMemberList.Count > 0)
                        {
                            ContactUsers.Clear();
                            ContactNames = string.Empty;
                            foreach (ContactInfoViewModel tempVm in vm.GroupMemberList)
                            {
                                if (tempVm.User.userId == AntSdkService.AntSdkLoginOutput.userId) continue;
                                ContactNames += tempVm.User.userName + "，";
                                ContactUsers.Add(tempVm.User);
                            }
                            ContactNames = ContactNames.TrimEnd('，');
                        }
                    });
                }
                return this._selectContactsCommand;
            }
        }
        #endregion
        #region 其他方法
        public delegate void SendMassMsgDelegate(AntSdkMassMsgCtt msg);
        public static event SendMassMsgDelegate SendMassMsgEvent;
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="o"></param>
        private void SendMassMsg(object o)
        {
            if (ContactUsers == null || ContactUsers.Count == 0)
            {
                MessageBoxWindow.Show("请先选择消息接收人!", GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            else if (string.IsNullOrWhiteSpace(MsgContent))
            {
                MessageBoxWindow.Show("消息内容不能为空!", GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            else if (MsgContent.Length > 500)
            {
                MessageBoxWindow.Show("消息内容不能超过500个字符!", GlobalVariable.WarnOrSuccess.Warn);
                return;
            }
            MassMsg msg = new MassMsg();
            msg.mtp = (int)GlobalVariable.MsgType.MassMsg;
            //TODO:AntSdk_Modify
            AntSdkMassMsgCtt ctt = new AntSdkMassMsgCtt();
            //ctt.messageId = PublicTalkMothed.timeStampAndRandom();
            ctt.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
            if (ContactUsers != null && ContactUsers.Count > 0)
                ctt.targetId = string.Join(",", ContactUsers.Where(m => m.userId != AntSdkService.AntSdkCurrentUserInfo.userId).Select(c => c.userId).ToArray());
            ctt.companyCode = GlobalVariable.CompanyCode;
            ctt.content = MsgContent;
            ctt.os = (int)GlobalVariable.OSType.PC;
            ctt.sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, GlobalVariable.MassAssistantId);
            ctt.chatIndex = null;
            ctt.sendTime = null;
            string errMsg = "";
            //DONE:AntSdk_Modify
            AntSdkChatMsg.MultiSend massMsg = new AntSdkChatMsg.MultiSend();
            massMsg.sendUserId = AntSdkService.AntSdkCurrentUserInfo.userId;
            if (ContactUsers != null && ContactUsers.Count > 0)
                massMsg.targetId = string.Join(",", ContactUsers.Where(m => m.userId != AntSdkService.AntSdkCurrentUserInfo.userId).Select(c => c.userId).ToArray());
            massMsg.content = MsgContent;
            massMsg.chatType = (int)AntSdkchatType.Point;
            massMsg.messageId = PublicTalkMothed.timeStampAndRandom();
            massMsg.sessionId = DataConverter.GetSessionID(AntSdkService.AntSdkCurrentUserInfo.userId, GlobalVariable.MassAssistantId);
            var isResult = AntSdkService.SdkPublishChatMsg(massMsg, ref errMsg);

            // AntSdkMassMsgCtt
            if (isResult)
            {
                ctt.messageId = massMsg.messageId;
                LogHelper.WriteWarn("---------------------------消息助手群发消息已发送---------------------");
                SendMassMsgEvent?.Invoke(ctt);
                (o as System.Windows.Window)?.Close();
            }
            else
            {
                MessageBoxWindow.Show("消息发送失败!" + errMsg, GlobalVariable.WarnOrSuccess.Warn);
            }
        }
        #endregion
    }
}
