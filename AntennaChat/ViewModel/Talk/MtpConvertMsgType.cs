using SDK.AntSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaChat.ViewModel.Talk
{
    /// <summary>
    /// mtp转换对应解析的MsgType类
    /// </summary>
    public class MtpConvertMsgType
    {
        public static AntSdkMsgType CovertMtp(string mtp)
        {
            AntSdkMsgType antSdkMsgType = AntSdkMsgType.UnDefineMsg;
            switch (mtp)
            {
                case "64":
                    antSdkMsgType= AntSdkMsgType.ChatMsgText;
                    break;
                case "128":
                    antSdkMsgType = AntSdkMsgType.ChatMsgPicture;
                    break;
                case "256":
                    antSdkMsgType = AntSdkMsgType.ChatMsgAudio;
                    break;
                case "512":
                    antSdkMsgType = AntSdkMsgType.ChatMsgVideo;
                    break;
                case "1024":
                    antSdkMsgType = AntSdkMsgType.ChatMsgFile;
                    break;
                case "2048":
                    antSdkMsgType = AntSdkMsgType.ChatMsgMapLocation;
                    break;
                case "8192":
                    antSdkMsgType = AntSdkMsgType.ChatMsgAt;
                    break;
            }
            return antSdkMsgType;
        }
    }
}
