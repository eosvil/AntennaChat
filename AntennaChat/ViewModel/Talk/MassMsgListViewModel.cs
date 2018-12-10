using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Antenna.Framework;
using Antenna.Model;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;

namespace AntennaChat.ViewModel.Talk
{
    public class MassMsgListViewModel : PropertyNotifyObject
    {
        private ObservableCollection<MassMsgViewModel> _MassMsgControlList = new ObservableCollection<MassMsgViewModel>();
        /// <summary>
        /// 群发消息列表
        /// </summary>
        public ObservableCollection<MassMsgViewModel> MassMsgControlList
        {
            get { return this._MassMsgControlList; }
            set
            {
                this._MassMsgControlList = value;
                RaisePropertyChanged(() => MassMsgControlList);
            }
        }

        public MassMsgListViewModel()
        {
            BaseBLL<AntSdkMassMsgCtt, T_MassMsgDAL> massMsgBll = new BaseBLL<AntSdkMassMsgCtt, T_MassMsgDAL>();
            IList<AntSdkMassMsgCtt> massMsgList = massMsgBll.GetList();
            if (massMsgList == null || massMsgList.Count == 0) return;
            foreach (AntSdkMassMsgCtt msg in massMsgList)
            {
                GlobalVariable.SendMsgState state = GlobalVariable.SendMsgState.Success;
                if (string.IsNullOrEmpty(msg.chatIndex))
                {
                    state = GlobalVariable.SendMsgState.Failure;
                }
                MassMsgViewModel controlVm = new MassMsgViewModel(msg, state);
                MassMsgControlList.Add(controlVm);
            }
            MassMsgControlList.Last().IsSelected = true;
        }

        /// <summary>
        /// 发送新的群发消息
        /// </summary>
        public void AddNewMassMsg(AntSdkMassMsgCtt msg)
        {
            MassMsgViewModel controlVm = new MassMsgViewModel(msg, GlobalVariable.SendMsgState.Sending);
            MassMsgControlList.Add( controlVm);
            controlVm.IsSelected = true;
        }

        /// <summary>
        /// 收到群发消息回执时刷新界面
        /// </summary>
        public void RefreshMassMsg(AntSdkChatMsg.ChatBase massMsgReceipt)
        {
            MassMsgViewModel controlVm = MassMsgControlList.FirstOrDefault(c => c.MessageId == massMsgReceipt.messageId);
            if (controlVm == null) return;
            controlVm.HandleMassMsgReceipt(massMsgReceipt);
        }
    }
}
