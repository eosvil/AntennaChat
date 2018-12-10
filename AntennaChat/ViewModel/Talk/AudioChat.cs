using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Framework;
using static NIM.ClientAPI;
using NIM;
using System.IO;
using System.Windows;
using SDK.AntSdk;
using AntennaChat.ViewModel.AudioVideo;

namespace AntennaChat.ViewModel.Talk
{
    /// <summary>
    /// 音视频聊天
    /// </summary>
    public class AudioChat
    {
        private static readonly string DirPath = Environment.CurrentDirectory + "\\AudioChatData";
        private static NIMVChatSessionStatus _vchatHandlers;
        public static MainWindowViewModel MainWindowVm { get; set; }
        public static AudioChatViewModel audioViewModel;//存储语音电话ViewModel
        private static bool _devicesIsStart = false;//设备是否已经启动
        public static long  _currentChannelId = 0;//当前通话中的频道ID
        public static string targetUid = string.Empty;//对方Uid
        /// <summary>
        /// API是否初始化
        /// </summary>
        public static bool IsInit;

        /// <summary>
        /// 错误提示信息
        /// </summary>
        public static string ErrorMsg;

        /// <summary>
        /// 初始化、登录API
        /// 无论是否初始化成功都返回true 确保程序能继续执行
        /// </summary>
        public static bool InitApi()
        {
            try
            {
                IsInit = false;
                ErrorMsg = string.Empty;
                var appKey = "2827086130b4d1473e59bfdf4b111bab";
                var accid = AntSdkService.AntSdkCurrentUserInfo.accid;
                var accToken = AntSdkService.AntSdkCurrentUserInfo.accToken;
                if (!Directory.Exists(DirPath))
                {
                    Directory.CreateDirectory(DirPath);
                }
                ErrorMsg = "";
                if (!ClientAPI.Init(appKey, DirPath, null, null))
                {
                    NimUtility.Log.Error("API初始化失败");
                    LogHelper.WriteError("API初始化失败");
                    ErrorMsg = "API初始化失败";
                    IsInit = false;
                    return true;
                }
                LogHelper.WriteDebug("API初始化成功.");
                //使用明文密码或者其他加密方式请修改此处代码
                var password = accToken;//ToolsAPI.GetMd5(accToken);
                Login(appKey, accid, password, HandleLoginResult);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[语音电话API初始化失败]："+ex.Message);
                return true;
            }
        }

        /// <summary>
        /// 登录结果处理
        /// </summary>
        /// <param name="result"></param>
        private static void HandleLoginResult(NIM.NIMLoginResult result)
        {
            if (result.LoginStep == NIM.NIMLoginStep.kNIMLoginStepLogin)
            {
                //登录成功
                if (result.Code == NIM.ResponseCode.kNIMResSuccess)
                {
                    if (!NIM.VChatAPI.Init())
                    {
                        NimUtility.Log.Error("音视频通话功能初始化失败");
                        LogHelper.WriteError("音视频通话功能初始化失败");
                        ErrorMsg = "音视频通话功能初始化失败";
                        IsInit = false;
                        #region 登出->清理API
                        System.Threading.Semaphore s = new System.Threading.Semaphore(0, 1);
                        Logout(NIMLogoutType.kNIMLogoutAppExit, (r) =>
                        {
                            s.Release();
                        });
                        //需要logout执行完才能退出程序
                        s.WaitOne(TimeSpan.FromSeconds(10));
                        ClientAPI.Cleanup();
                        #endregion
                        return;
                    }
                    IsInit = true;
                    LogHelper.WriteDebug("音视频通话功能初始化成功.");
                    InitVChatInfo();
                }
                else
                {
                    NimUtility.Log.Error("音视频通话API登录失败");
                    LogHelper.WriteError("音视频通话API登录失败");
                    ErrorMsg = "音视频通话API登录失败";
                    IsInit = false;
                    #region 登出->清理API
                    System.Threading.Semaphore s = new System.Threading.Semaphore(0, 1);
                    Logout(NIMLogoutType.kNIMLogoutChangeAccout, (r) =>
                    {
                        s.Release();
                    });
                    //需要logout执行完才能退出程序
                    s.WaitOne(TimeSpan.FromSeconds(10));
                    ClientAPI.Cleanup();
                    #endregion
                }
            }
        }
        /// <summary>
        /// 设置通话回调函数
        /// </summary>
        public static void InitVChatInfo()
        {
            _vchatHandlers.onSessionStartRes = OnSessionStartRes;
            _vchatHandlers.onSessionInviteNotify = OnSessionInviteNotify;
            _vchatHandlers.onSessionCalleeAckRes = OnSessionCalleeAckRes;
            _vchatHandlers.onSessionCalleeAckNotify = OnSessionCalleeAckNotify;
            _vchatHandlers.onSessionControlRes = OnSessionControlRes;
            _vchatHandlers.onSessionControlNotify = OnSessionControlNotify;
            _vchatHandlers.onSessionConnectNotify = OnSessionConnectNotify;
            _vchatHandlers.onSessionPeopleStatus = OnSessionPeopleStatus;
            _vchatHandlers.onSessionNetStatus = OnSessionNetStatus;
            _vchatHandlers.onSessionHangupRes = OnSessionHangupRes;
            _vchatHandlers.onSessionHangupNotify = OnSessionHangupNotify;
            //本人其他端响应通知
            _vchatHandlers.onSessionSyncAckNotify = (channel_id, code, uid, mode, accept, time, client) =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (string.IsNullOrEmpty(uid))
                        uid = targetUid;
                    MainWindowVm.HandlerSessionSyncAckNotify(channel_id, code, uid, mode, accept, time, client);
                }));
            };
            //注册音视频会话交互回调
            VChatAPI.SetSessionStatusCb(_vchatHandlers);
            //注册音频接收数据回调
            DeviceAPI.SetAudioReceiveDataCb(AudioDataRecHandler, null);
            //注册视频接收数据回调
            //DeviceAPI.SetVideoReceiveDataCb(VideoDataRecHandler, null);
            //注册视频采集数据回调
            //DeviceAPI.SetVideoCaptureDataCb(VideoDataCaptureHandler, null);

            //DeviceAPI.AddDeviceStatusCb(NIM.NIMDeviceType.kNIMDeviceTypeVideo, DeviceStatusHandler);
            SetAudioCaptureVolumn(255);
            SetAudioPlayVolumn(120);
        }
        /// <summary>
        /// 邀请会话
        /// </summary>
        /// <param name="targetId">目标accid</param>
        /// <param name="mode"></param>
        public static void Start(string targetId, NIMVideoChatMode mode)
        {
            var info = new NIMVChatInfo {Uids = new List<string> {targetId}};
            VChatAPI.Start(mode, info);
        }

        /// <summary>
        /// 挂断
        /// </summary>
        public static void End()
        {
            VChatAPI.End();
        }

        /// <summary>
        /// 退出程序清理
        /// </summary>
        public static void ExitClearApi()
        {
            //退出前需要结束音视频设备，防止错误的数据上报\
            if (_devicesIsStart)
                EndDevices();
            if(!IsInit)return;
            System.Threading.Thread.Sleep(500);
            //在释放前需要按步骤清理音视频模块和nim client模块
            VChatAPI.Cleanup();
            System.Threading.Semaphore s = new System.Threading.Semaphore(0, 1);
            Logout(NIMLogoutType.kNIMLogoutAppExit, (r) =>
            {
                s.Release();
            });
            //需要logout执行完才能退出程序
            s.WaitOne(TimeSpan.FromSeconds(10));
            ClientAPI.Cleanup();
        }
        #region 回调

        /// <summary>
        /// 音频接收数据回调
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="rate"></param>
        private static void AudioDataRecHandler(UInt64 time, IntPtr data, UInt32 size, Int32 rate)
        {

        }

        /// <summary>
        /// 收到视频帧回调函数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="json_extension"></param>
        private static void VideoDataRecHandler(UInt64 time, IntPtr data, UInt32 size, UInt32 width, UInt32 height,
            string json_extension)
        {

        }

        /// <summary>
        /// 捕获视频帧回调函数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="json_extension"></param>
        private static void VideoDataCaptureHandler(UInt64 time, IntPtr data, UInt32 size, UInt32 width, UInt32 height,
            string json_extension)
        {

        }

        /// <summary>
        /// 设备状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="devicePath"></param>
        private static void DeviceStatusHandler(NIM.NIMDeviceType type, uint status, string devicePath)
        {

        }

        /// <summary>
        /// 发起结果回调
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        private static void OnSessionStartRes(long channel_id, int code)
        {
            if (code == 200)
                _currentChannelId = channel_id;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MainWindowVm.HandlerSessionStartRes(channel_id, code);
            }));
        }

        /// <summary>
        /// 对方邀请通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="mode"></param>
        /// <param name="time"></param>
        private static void OnSessionInviteNotify(long channel_id, string uid, int mode, long time)
        {
            if (_currentChannelId != 0 && _currentChannelId != channel_id)
            {
                NIM.VChatAPI.ChatControl(channel_id, NIMVChatControlType.kNIMTagControlBusyLine); //发送自己为忙碌状态
                return;
            }
            targetUid = uid.ToUpper();
            if (mode == (int)NIM.NIMVideoChatMode.kNIMVideoChatModeAudio)
            {
                _currentChannelId = channel_id;
                //向你发起实时语音
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MainWindowVm.HandlerSessionInviteNotify(channel_id, uid, mode);
                }));
            }
            else
            {
                //向你发起视频聊天
            }
        }
        /// <summary>
        /// 对方邀请处理结果
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="isAccept">是否接听</param>
        public static void AudioResult(long channel_id, bool isAccept)
        {
            if (!isAccept) _currentChannelId = 0;//重置
            NIM.NIMVChatInfo info = new NIM.NIMVChatInfo();
            NIM.VChatAPI.CalleeAck(channel_id, isAccept, info);
        }
        /// <summary>
        /// 邀请响应的结果回调
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        private static void OnSessionCalleeAckRes(long channel_id, int code)
        {
           
        }

        /// <summary>
        /// 发起后对方响应通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="mode"></param>
        /// <param name="accept"></param>
        /// <param name="customInfo"></param>
        private static void OnSessionCalleeAckNotify(long channel_id, string uid, int mode, bool accept,
            string customInfo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                _currentChannelId = accept ? channel_id : 0;
                MainWindowVm.HandlerSessionCalleeAckNotify(channel_id,uid,mode,accept);
            }));
        }

        /// <summary>
        /// 控制操作结果回调
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        /// <param name="type"></param>
        private static void OnSessionControlRes(long channel_id, int code, int type)
        {

        }

        /// <summary>
        /// 控制操作通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="type"></param>
        private static void OnSessionControlNotify(long channel_id, string uid, int type)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MainWindowVm.HandlerSessionControlNotify(channel_id, uid, type);
            }));
        }

        /// <summary>
        /// 连接通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        /// <param name="record_file"></param>
        /// <param name="video_record_file"></param>
        private static void OnSessionConnectNotify(long channel_id, int code, string record_file,
            string video_record_file)
        {
            if (code == 200)
            {
                //成功建立链接
                StartDevices();
            }
            else
            {
                //断开连接
                NIM.VChatAPI.End();
            }
        }

        /// <summary>
        /// 网络状态通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="status"></param>
        /// <param name="uid"></param>
        private static void OnSessionNetStatus(long channel_id, int status, string uid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MainWindowVm.HandlerSessionNetStatus(channel_id, status, uid);
            }));
        }

        /// <summary>
        /// 成员状态通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="uid"></param>
        /// <param name="status"></param>
        private static void OnSessionPeopleStatus(long channel_id, string uid, int status)
        {

        }

        /// <summary>
        /// 主动挂断结果回调
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        private static void OnSessionHangupRes(long channel_id, int code)
        {
            EndDevices();
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                MainWindowVm.HandlerSessionHangupRes(channel_id);
            }));
        }

        /// <summary>
        /// 对方挂断通知
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="code"></param>
        private static void OnSessionHangupNotify(long channel_id, int code)
        {
            if(_currentChannelId!=channel_id)return;
            EndDevices();
            if (_currentChannelId == channel_id)
                _currentChannelId = 0;//重置
            if (code == 200)
            {
                //对方挂断
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MainWindowVm.HandlerSessionHangupNotify(channel_id);
                }));
            }
        }

        #endregion
        /// <summary>
        /// 设备检测
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool CheckDevices(ref string errMsg)
        {
            var micphoneDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);//麦克风
            var audioOutDeviceinfolist = NIM.DeviceAPI.GetDeviceList(NIM.NIMDeviceType.kNIMDeviceTypeAudioOut);//听筒
            if (micphoneDeviceinfolist == null || audioOutDeviceinfolist == null)
            {
                errMsg = "检测不到麦克风或者听筒";
                return false;
            }
            if (micphoneDeviceinfolist.DeviceList == null ||
                (micphoneDeviceinfolist.DeviceList != null && micphoneDeviceinfolist.DeviceList.Count == 0))
            {
                errMsg = "检测不到麦克风";
                return false;

            }
            if (audioOutDeviceinfolist.DeviceList == null ||
             (audioOutDeviceinfolist.DeviceList != null && audioOutDeviceinfolist.DeviceList.Count == 0))
            {
                errMsg = "检测不到听筒";
                return false;
            }
            return true;
        }
        /// <summary>
        /// 开启设备
        /// </summary>
        private static void StartDevices()
        {
            NIM.DeviceAPI.StartDeviceResultHandler handle = (type, ret) =>
            {
                NimUtility.Log.Info(type.ToString() + ":" + ret.ToString());
            };
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn, "", 0, null, handle);//开启麦克风
            NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat, "", 0, null, handle);//开启扬声器播放对方语音
            //NIM.DeviceAPI.StartDevice(NIM.NIMDeviceType.kNIMDeviceTypeVideo, "", 0, null, handle);//开启摄像头
            _devicesIsStart = true;
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        public static void EndDevices()
        {
            NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioIn);
            NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeAudioOutChat);
            //NIM.DeviceAPI.EndDevice(NIM.NIMDeviceType.kNIMDeviceTypeVideo);
            _devicesIsStart = false;
        }
        /// <summary>
        /// 调节麦克风音量
        /// </summary>
        /// <param name="value"></param>
        public static void SetAudioCaptureVolumn(int value)
        {
            DeviceAPI.AudioCaptureVolumn = Convert.ToByte(value);
        }
        /// <summary>
        /// 设置播放器音量
        /// </summary>
        /// <param name="value"></param>
        public static void SetAudioPlayVolumn(int value)
        {
            DeviceAPI.AudioPlayVolumn = Convert.ToByte(value);
        }
    }
}
