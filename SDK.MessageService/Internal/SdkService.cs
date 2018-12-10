using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SDK.Service.Models;
using SDK.Service.Properties;

namespace SDK.Service
{
    /// <summary>
    /// 类型说明：.NET SDK
    /// 作    者：caolg
    /// 创建时间：2017-04-20
    /// </summary>
    public class SdkService
    {
        /// <summary>
        /// SDK启动状态：true 启动；false 停止；
        /// </summary>
        public static bool SdkActivateState { get; set; }

        /// <summary>
        /// 暴露.NET SDK初始化参数
        /// </summary>
        public static SdkSysParam SdkSysParam { get; private set; }

        /// <summary>
        /// MQTT连接状态
        /// </summary>
        public static MsConnectionState ConnectionState => _sdkMqttService.ConnectionState;

        /// <summary>
        /// MQTT是否连接
        /// </summary>
        public static bool IsConnected => _sdkMqttService.IsConnected;

        /// <summary>
        /// 心跳日志是否记录
        /// </summary>
        public static bool HeartBeatLog { get; private set; }

        #region   //内部辅助完成SDK方法
        /// <summary>
        /// 设置处理
        /// </summary>
        internal static SdkHttpMethod MdsdkhttpMethod { get; set; }

        /// <summary>
        /// .NET SDK HTTP服务
        /// </summary>
        private static SdkHttpService _sdkHttpService;

        /// <summary>
        /// .NET SDK MQTT服务
        /// </summary>
        private static SdkMqttService _sdkMqttService;

        /// <summary>
        /// 提示消息模式:true-显示SDK提示，false-不显示SDK提示
        /// </summary>
        private static bool _errorMode = false;

        /// <summary>
        /// 读取平台配置信息
        /// </summary>
        private static Configuration GetConfigInfo()
        {
            try
            {
                var configPath = "Sdk.config";
                var map = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
                return ConfigurationManager.OpenMappedExeConfiguration(map,
                    ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                LogHelper.WriteDebug(
                    $"[SdkService.GetConfigInfo]:Sdk.config {Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 方法说明：格式化处理子错误提示信息
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="sdkerrorMsg">SDK指定提示</param>
        /// <param name="output">SDK返回</param>
        /// <param name="errMsg">日志级错误信息</param>
        /// <returns></returns>
        private static string FormatErrorMsg(string sdkerrorMsg, ErrorOutput output, string errMsg)
        {
            var errorMsg = FormatErrorMsg(sdkerrorMsg, output.errorMsg);
            if (string.IsNullOrEmpty(errorMsg))
            {
                errorMsg = FormatErrorMsg(sdkerrorMsg, errMsg);
            }
            //返回
            return errorMsg;
        }

        /// <summary>
        /// 方法说明：格式化处理子错误提示信息
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="sdkerrorMsg">SDK指定提示</param>
        /// <param name="output">SDK返回</param>
        /// <returns></returns>
        private static string FormatErrorMsg(string sdkerrorMsg, ErrorOutput output)
        {
            return FormatErrorMsg(sdkerrorMsg, output.errorMsg);
        }

        /// <summary>
        /// 方法说明：格式化处理子错误提示信息
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="sdkerrorMsg">SDK指定提示</param>
        /// <param name="errorMsg">服务器接口提示</param>
        /// <returns>按照提示模式组织的提示信息</returns>
        private static string FormatErrorMsg(string sdkerrorMsg, string errorMsg)
        {
            return string.IsNullOrEmpty(errorMsg)
                ? sdkerrorMsg
                : (_errorMode ? $"{sdkerrorMsg},{errorMsg}" : errorMsg);
        }

        /// <summary>
        /// 方法说明：32位MD5加密(不带秘钥)
        /// 作    者：赵雪峰  
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="input">需加密字符串</param>
        /// <param name="encode">编码格式</param>
        /// <returns>加密后字符串</returns>
        private static string Md5Encrypt(string input, Encoding encode)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var t = md5.ComputeHash(encode.GetBytes(input));
            var sb = new StringBuilder(32);
            //处理
            foreach (var t1 in t)
            {
                sb.Append(t1.ToString("x").PadLeft(2, '0'));
            }
            //返回
            return sb.ToString();
        }

        /// <summary>
        /// 方法说明：检查.NET SDK 状态
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="errorMsg">错误提示</param>
        /// <returns></returns>
        private static bool CheckSdkServiceState(ref string errorMsg)
        {
            //第一步，检查启动状态
            if (SdkActivateState) { return true; }
            //后续如果再检查，在这里补充调用函数条件
            errorMsg += Resources.SdkNotStart;
            return false;
            //返回
        }

        /// <summary>
        /// 方法说明：字符串比较，用于单聊的点对点SessionId生成
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="sendUserId">发送用户ID</param>
        /// <param name="targetId">目标用户ID</param>
        /// <returns>比较结果</returns>
        private static int Compare(string sendUserId, string targetId)
        {
            var result = 1;
            sendUserId = sendUserId.Replace("[^0-9a-zA-Z]", string.Empty);
            targetId = targetId.Replace("[^0-9a-zA-Z]", string.Empty);
            if (sendUserId.Length > targetId.Length)
            {
                result = 1;
            }
            else if (sendUserId.Length < targetId.Length)
            {
                result = -1;
            }
            else
            {
                var charArray = sendUserId.ToCharArray();
                var charArray2 = targetId.ToCharArray();
                for (var i = 0; i < charArray.Length; i++)
                {
                    if (charArray[i] > charArray2[i])
                    {
                        result = 1;
                        break;
                    }
                    else if (charArray[i] < charArray2[i])
                    {
                        result = -1;
                        break;
                    }
                }
            }
            //返回比较结果
            return result;
        }

        #endregion

        #region   //要暴露的需要转接的SDK接收MQTT消息事件

        /// <summary>
        /// 属性说明：SDK自定义Http处理Token错误事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Http DealWith Error Token Self")]
        public static event EventHandler TokenErrorEvent
        {
            remove { _sdkHttpService.TokenErrorEvent -= value; }
            add { _sdkHttpService.TokenErrorEvent += value; }
        }

        /// <summary>
        /// 聊天的消息接收
        /// </summary>
        /// <summary>
        /// 属性说明：SDK自定义Mqtt聊天室消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive Chat Message Self")]
        public static event SdkPublicationReceivedHandler MsChatsReceived
        {
            remove { if (_sdkMqttService != null) _sdkMqttService.MsChatsReceived -= value; }
            add { if (_sdkMqttService != null) _sdkMqttService.MsChatsReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt聊天室消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive Room Message Self")]
        public static event SdkPublicationReceivedHandler MsRoomsReceived
        {
            remove { _sdkMqttService.MsRoomsReceived -= value; }
            add { _sdkMqttService.MsRoomsReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt群组型消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive Group Message Self")]
        public static event SdkPublicationReceivedHandler MsGroupReceived
        {
            remove { _sdkMqttService.MsGroupReceived -= value; }
            add { _sdkMqttService.MsGroupReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt个人的消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive User Message Self")]
        public static event SdkPublicationReceivedHandler MsUsersReceived
        {
            remove { _sdkMqttService.MsUsersReceived -= value; }
            add { _sdkMqttService.MsUsersReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt其他的消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive Other Message Self")]
        public static event SdkPublicationReceivedHandler MsOtherReceived
        {
            remove { _sdkMqttService.MsOtherReceived -= value; }
            add { _sdkMqttService.MsOtherReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt其他的消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Receive Other Message Self")]
        public static event SdkPublicationReceivedHandler MsOfflineReceived
        {
            remove { _sdkMqttService.MsOfflineReceived -= value; }
            add { _sdkMqttService.MsOfflineReceived += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt断开重连事件
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Disconnect Reconnect Self")]
        public static event EventHandler ReconnectedMqtt
        {
            remove { _sdkMqttService.ReconnectedMqtt -= value; }
            add { _sdkMqttService.ReconnectedMqtt += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt断开事件
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService"), Description("Mqtt Disconnect Reconnect Self")]
        public static event EventHandler DisconnectedMqtt
        {
            remove { _sdkMqttService.DisconnectedMqtt -= value; }
            add { _sdkMqttService.DisconnectedMqtt += value; }
        }

        #endregion

        /// <summary>
        /// 方法说明：使用SDK之前需要先调用该方法对SDK服务进行启动
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="sdksysParam">初始化的SDK系统参数</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功开启平台SDK</returns>
        public static bool StartSdk(SdkSysParam sdksysParam, ref string errorMsg)
        {
            //判断处理
            if (sdksysParam == null)
            {
                sdksysParam = new SdkSysParam();
            }
            //处理是否要读取SDK配置信息
            if (
                !(sdksysParam.SetCompanyCode & sdksysParam.SetAppKey & sdksysParam.SetAppSecret &
                  sdksysParam.SetHttpPrdfix & sdksysParam.SetLogMode & sdksysParam.SetHeartBeatLog &
                  sdksysParam.SetUpload & sdksysParam.SetHttpTimeOut))
            {
                //存在未设置参数，则读取配置
                var config = GetConfigInfo();
                var allKeys = config?.AppSettings?.Settings?.AllKeys;
                if (allKeys?.Length > 0)
                {

                    var keysList = new List<string>(allKeys);

                    bool errormode;
                    if (keysList.Contains("ErrorMode") && bool.TryParse(config.AppSettings.Settings["ErrorMode"].Value, out errormode))
                    {
                        _errorMode = errormode;
                    }
                    var httptimeout = 0;
                    if (!sdksysParam.SetHttpTimeOut && keysList.Contains("SdkHttpTimeout") &&
                        int.TryParse(config.AppSettings.Settings["SdkHttpTimeout"].Value, out httptimeout))
                    {
                        sdksysParam.HttpTimeOut = httptimeout * 1000;
                        if (sdksysParam.HttpTimeOut == 0)
                        {
                            sdksysParam.HttpTimeOut = 15000;
                        }
                    }
                    bool heartbeatLog;
                    if (keysList.Contains("HeartBeatLog") &&
                        bool.TryParse(config.AppSettings.Settings["HeartBeatLog"].Value, out heartbeatLog))
                    {
                        HeartBeatLog = heartbeatLog;
                    }
                    if (!sdksysParam.SetCompanyCode && keysList.Contains("SdkCompanyCode"))
                    {
                        sdksysParam.Companycode = config.AppSettings.Settings["SdkCompanyCode"].Value;
                    }
                    if (!sdksysParam.SetAppKey && keysList.Contains("SdkAppKey"))
                    {
                        sdksysParam.Appkey = config.AppSettings.Settings["SdkAppKey"].Value;
                    }
                    if (!sdksysParam.SetAppSecret && keysList.Contains("SdkAppSecret"))
                    {
                        sdksysParam.Appsecret = config.AppSettings.Settings["SdkAppSecret"].Value;
                    }
                    if (!sdksysParam.SetHttpPrdfix && keysList.Contains("SdkHttpPrdfix"))
                    {
                        sdksysParam.HttpPrdfix = config.AppSettings.Settings["SdkHttpPrdfix"].Value;
                    }
                    if (!sdksysParam.SetLogMode)
                    {
                        bool setbool;
                        if (keysList.Contains("SdkDebugLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkDebugLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            sdksysParam.SdkLogMode |= SdkLogLevel.DebugLogEnable;
                        }
                        if (keysList.Contains("SdkInfoLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkInfoLogEnable"].Value, out setbool) && setbool)
                        {
                            sdksysParam.SdkLogMode |= SdkLogLevel.InfoLogEnable;
                        }
                        if (keysList.Contains("SdkWarnLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkWarnLogEnable"].Value, out setbool) && setbool)
                        {
                            sdksysParam.SdkLogMode |= SdkLogLevel.WarnLogEnable;
                        }
                        if (keysList.Contains("SdkErrorLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkErrorLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            sdksysParam.SdkLogMode |= SdkLogLevel.ErrorLogEnable;
                        }
                        if (keysList.Contains("SdkFatalLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkFatalLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            sdksysParam.SdkLogMode |= SdkLogLevel.FatalLogEnable;
                        }
                    }
                }
            }
            //检查判断SDK启动必备条件
            if (string.IsNullOrEmpty(sdksysParam.Companycode))
            {
                errorMsg += Resources.SdkStartCompanyCodeNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(sdksysParam.Appkey))
            {
                errorMsg += Resources.SdkStartAppKeyNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(sdksysParam.Appsecret))
            {
                errorMsg += Resources.SdkStartAppSecretNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(sdksysParam.HttpPrdfix))
            {
                errorMsg += Resources.SdkStartHttpPrefStuffNotNull;
                return false;
            }
            //系统参数处理
            SdkSysParam = sdksysParam;
            //日志配置处理
            LogHelper.LoadConfig();
            //Http实例
            _sdkHttpService = new SdkHttpService();
            //Mqtt实例
            _sdkMqttService = new SdkMqttService();
            //Sdk成功启动
            SdkActivateState = true;
            //启动成功是处理Http请求方法名
            MdsdkhttpMethod = new SdkHttpMethod();
            //检查Debug日志开关，如果关闭，则删除本地的Debug/Info日志
            CloseLogDeleteFile();
            //成功开启SDK
            return true;
        }

        /// <summary>
        /// 方法说明：检查Debug日志开关，如果关闭，则异步删除本地的Debug/Info日志
        /// </summary>
        private static void CloseLogDeleteFile()
        {
            //Debug日志
            var debugPath = $"{System.Windows.Forms.Application.StartupPath}/SDK_Logs/Debug/";
            if (!SdkSysParam.DebugLogEnable && System.IO.Directory.Exists(debugPath))
            {
                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        System.IO.Directory.Delete(debugPath, true);
                    }
                    catch
                    {
                        //ignore
                    }
                });
            }
            //Info日志
            var infoPath = $"{System.Windows.Forms.Application.StartupPath}/SDK_Logs/Info/";
            if (!SdkSysParam.InfoLogEnable && System.IO.Directory.Exists(infoPath))
            {
                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        System.IO.Directory.Delete(infoPath, true);
                    }
                    catch
                    {
                        //ignore
                    }
                });
            }
        }

        /// <summary>
        /// 方法说明：SDK登录->获取SDK Token->获取MQTT连接参数->连接MQTT->订阅默认主题
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户业务ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>SDK是否登录成功</returns>
        public static bool SdkLogin(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //赋值UserId
            SdkSysParam.UserId = userId;
            //获取SDK的Token
            var getTokenInput = new GetTokenInput { id = userId };
            var getTokenOutput = string.Empty;
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (!_sdkHttpService.GetToken(getTokenInput, ref getTokenOutput, ref errorCode, ref errMsg))
            {
                errorMsg = FormatErrorMsg(Resources.SdkLoginServiceFail, errMsg);
                return false;
            }
            //设置SDK的Token
            SdkSysParam.Token = getTokenOutput;
            //获取MQTT连接参数
            var getConnConfigInput = new GetConnectConfigInput
            {
                ways = (int)SdkEnumCollection.Ways.Multi
            };
            var configOutput = new GetConnectConfigOutput();
            if (!_sdkHttpService.GetMqttConnectConfig(getConnConfigInput, ref configOutput, ref errorCode, ref errMsg))
            {
                errorMsg = FormatErrorMsg(Resources.SdkGetMqttConfigFail, errMsg);
                return false;
            }
            //设置文件上传地址
            SdkSysParam.FileUpload = configOutput.fileUploadUrl;
            //建立连接
            if (_sdkMqttService.Connect(userId, configOutput, ref errMsg))
            {
                //SDK登录成功
                return true;
            }
            //链接错误提示
            errorMsg = $"{Resources.SdkConectServiceFail}{errMsg}";
            return false;
        }

        /// <summary>
        /// 方法说明：登录成功后的默认主题订阅
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户业务ID</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns></returns>
        public static bool SdkDefaultTopicSubscribe(string userId, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //判断是否连接
            if (!_sdkMqttService.IsConnected)
            {
                errorMsg += Resources.SdkConectServiceFail;
                return false;
            }
            //连接成功需要订阅的默认主题
            var topics = new List<string>
            {
                $"{SdkSysParam.Token}",
                $"{SdkSysParam.Appkey}",
                $"{SdkSysParam.Appkey}/{userId}",
                $"{SdkSysParam.Appkey}/{userId}/{(int) SdkEnumCollection.OSType.PC}",
                $"{SdkSysParam.Appkey}/pc/version"
            };
            //订阅默认主题
            var temperrorMsg = string.Empty;
            if (_sdkMqttService.Subscribe(topics, ref temperrorMsg))
            {
                return true;
            }
            //返回
            errorMsg = FormatErrorMsg(Resources.SdkSubscribeDefaultTopicsError, temperrorMsg);
            return false;
        }

        /* 点对点SessionID生成规则
        sendUserId:s134562ade
        targetId:s13frsss4
        Step1 过滤掉sendUserId和targetId中的特殊符号，只保留字母和数字
        Step2 比较sendUserId和targetId的大小
              2.1 如果sendUserId.length()>targetId.length(), 那么sendUserId大
              2.2 如果sendUserId.length()<targetId.length(), 那么targetId大
              2.3 如果sendUserId.length()==targetId.length(),那么进入下面步骤
              2.3.1 将sendUserId和targetId转换为char[]
              2.3.2 循环遍历sendUserId和targetId的char[]
              2.3.3 如果sendUserId的char[] 中元素等于targetId的char[]中元素,那么比较char[] 的下一位元素
              2.3.4 如果sendUserId的char[] 中元素大于targetId的char[]中的元素,那么sendUserId大
              2.3.5 如果sendUserId的char[] 中元素小于targetId的char[]中的元素,那么targetId大
        Step3 小的放在前面，大的放在后面
        tempSessionId = s134562ades13frsss4
        Step4 经过MD5加密
        对tempSessionId进行MD5加密，得到真正的sessionId
        sessionId = 0i88STbHxhNoILFvBSLA9Q
        */

        /// <summary>
        /// 方法说明：根据UserID生成SessionId(点对点聊天,两个参数位置可互换）
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="sendUserId">发送者UserId</param>
        /// <param name="targetId">目标UserId</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>生成的SessionId</returns>
        public static string GetPointToPointSessionId(string sendUserId, string targetId, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return string.Empty;
            }
            //清空非数字、字母字符
            sendUserId = System.Text.RegularExpressions.Regex.Replace(sendUserId, "[^0-9a-zA-Z]", string.Empty);
            targetId = System.Text.RegularExpressions.Regex.Replace(targetId, "[^0-9a-zA-Z]", string.Empty);
            string sourceStr;
            //比较处理
            if (Compare(sendUserId, targetId) > 0)
            {
                sourceStr = targetId + sendUserId;
            }
            else
            {
                sourceStr = sendUserId + targetId;
            }
            //MD5处理
            return Md5Encrypt(sourceStr, Encoding.UTF8);
        }

        /// <summary>
        /// 方法说明：订阅主题
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="topics">要订阅的主题</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否订阅成功</returns>
        public static bool Subscribe(string[] topics, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //针对用户自定义的主题必须满足AppKey/CustomTopics的规则
            var topicsList = topics.Select(n => $"{SdkSysParam.Appkey}/{n}");
            //开始订阅主题
            return _sdkMqttService.Subscribe(topicsList.ToList(), ref errorMsg);
        }

        /// <summary>
        /// 方法说明：取消订阅主题
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="topics">要取消订阅的主题</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否取消订阅成功</returns>
        public static bool UnSubscribe(string[] topics, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //针对用户自定义的主题必须满足AppKey/CustomTopics的规则
            var topicsList = topics.Select(n => $"{SdkSysParam.Appkey}/{n}");
            return _sdkMqttService.UnSubscribe(topicsList.ToList(), ref errorMsg);
        }

        /// <summary>
        /// 方法说明：回调方式注册接收消息函数
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="msgType">接收消息枚举标记类型：
        /// Example:只需要接收 文本、视频和自定义消息则该参数的传递方式为：
        /// SdkEnumCollection.SdkReceiveMsgType MsgType = dkEnumCollection.SdkReceiveMsgType.ChatMsgText  | 
        ///                                               dkEnumCollection.SdkReceiveMsgType.ChatMsgVideo |
        ///                                               dkEnumCollection.SdkReceiveMsgType.CustomMessage
        /// </param>
        /// <param name="receiveHandler">接收消息回调的注册方法</param>
        /// <param name="errorMsg"></param>
        public static void SdkCallBackReceiveMsg(SdkMsgType msgType,
            SdkPublicationReceivedHandler receiveHandler, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return;
            }
            _sdkMqttService.MsCallbackReceived += (s, e) =>
            {
                if (msgType.HasFlag(s) || msgType == SdkMsgType.AllMessage)
                {
                    receiveHandler?.Invoke(s, e);
                }
            };
        }

        /// <summary>
        /// 方法说明：SDK发送终端消息接口：心跳消息、请求离线消息
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishTerminalMsg<T>(T entity, ref string errorMsg) where T : SdkMsTerminalBase
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishTerminalMsg(entity, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishChatMsg<T>(T entity, ref string errorMsg) where T : MsSdkMessageChat
        {
            return CheckSdkServiceState(ref errorMsg) &&
                   _sdkMqttService.SdkPublishChatMsg(entity, SdkEnumCollection.ChatMsgSendType.Nomal, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（重发）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkRePublishChatMsg<T>(T entity, ref string errorMsg) where T : MsSdkMessageChat
        {
            return CheckSdkServiceState(ref errorMsg) &&
                   _sdkMqttService.SdkPublishChatMsg(entity, SdkEnumCollection.ChatMsgSendType.Repeat, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（重发）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkRobotPublishChatMsg<T>(T entity, ref string errorMsg) where T : MsSdkMessageChat
        {
            return CheckSdkServiceState(ref errorMsg) &&
                   _sdkMqttService.SdkPublishChatMsg(entity, SdkEnumCollection.ChatMsgSendType.Robot, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（重发）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkReRobotPublishChatMsg<T>(T entity, ref string errorMsg) where T : MsSdkMessageChat
        {
            return CheckSdkServiceState(ref errorMsg) &&
                   _sdkMqttService.SdkPublishChatMsg(entity, SdkEnumCollection.ChatMsgSendType.Rerobot, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：通过SDK发送自定义消息接口：[支持4000-9999的自定义消息，仅仅是传输][完成自定义消息（MsCustomEntity）中的content内容]
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="custommsgType">自定义消息类型[4000-9999]</param>
        /// <param name="customTopic">自定义主题</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishCustomMsg(MsSdkCustomEntity entity, int custommsgType, string customTopic,
            ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishCustomMsg(entity, custommsgType, customTopic, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK普通消息 已读/收回执
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="sendReceipt">已读/收回执实体</param>
        /// <param name="receiptType">已读/收类型</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送回执</returns>
        public static bool SdkPublishReceiptMsg(SdkMsSendMsgReceipt sendReceipt,
            SdkReceiptType receiptType, ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishReceiptMsg(sendReceipt, receiptType, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：通过SDK发送自定义消息接口：[支持4000-9999的自定义消息已读/收回执，仅仅是传输][完成自定义消息（MsCustomEntity）中的content内容]
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="sendReceipt">已读/收回执实体</param>
        /// <param name="receiptType">已读/收类型</param>
        /// <param name="customTopic">自定义消息主题</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishCustomReceiptMsg(SdkMsSendMsgReceipt sendReceipt,
            SdkReceiptType receiptType, string customTopic, ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) &&
                   _sdkMqttService.SdkPublishCustomReceiptMsg(sendReceipt, receiptType,
                       customTopic, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送点对点阅后即焚消息已读回执
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="receiptedchatmsgEntity">点对点聊天收到的阅后即焚消息实体</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送点对点阅后即焚已读回执</returns>
        public static bool SdkPublishPointBurnReadReceiptMsg(MsPointBurnReaded receiptedchatmsgEntity, ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishPointBurnReadReceiptMsg(receiptedchatmsgEntity, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送群阅后即焚消息[群主改变阅后即焚状态]
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="changeMode">要切换的阅后即焚状态</param>
        /// <param name="sendmodeEntity">发送实体信息</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送群阅后即焚消息</returns>
        public static bool SdkPublishGpOwnerChangeMode(GroupChangeMode changeMode,
            MsSdkMessageChat sendmodeEntity, ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishGpOwnerChangeMode(changeMode, sendmodeEntity, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送其他类型的标准消息[结构和聊天结构一直的消息]
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="otherMsg">其他消息信息</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否发型成功</returns>
        public static bool SdkPublishOtherMsg(MsSdkOther.SdkOtherBase otherMsg, ref string errorMsg)
        {
            return CheckSdkServiceState(ref errorMsg) && _sdkMqttService.SdkPublishOtherMsg(otherMsg, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：停止SDK服务
        /// 完成时间：2017-04-20
        /// </summary>
        public static bool StopSdk(ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //Mqtt断开（取消所有主题订阅、取消所有事件订阅、断开Mqtt服务）
            _sdkMqttService.DisConnect();
            SdkSysParam = null;
            _sdkHttpService = null;
            _sdkMqttService = null;
            //Sdk停止服务
            SdkActivateState = false;
            return true;
        }

        /// <summary>
        /// 方法说明：更新用户信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新用户信息输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新用户信息成功</returns>
        public static bool UpdateUserInfo(UpdateUserInfoInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateUserInfo(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkUpdateUserInfoError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：添加用户黑名单
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="targetId">屏蔽的用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功添加用户黑名单</returns>
        public static bool AddBlacklist(string userId, string targetId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new AddBlacklistInput
            {
                userId = userId,
                targetId = targetId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.AddBlacklist(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkAddBlacklistError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：查询黑名单ID列表
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>黑名单数组</returns>
        public static string[] FindBlacklists(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new FindBlacklistsInput
            {
                userId = userId
            };
            var output = new List<string>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (!_sdkHttpService.FindBlacklists(input, ref output, ref errorCode, ref errMsg))
            {
                return null;
            }
            //SDK判断
            if (output != null)
            {
                return output.ToArray();
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkFindBlacklistsError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：移除用户黑名单
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="targetId">屏蔽的用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功移除用户黑名单</returns>
        public static bool DelBlacklist(string userId, string targetId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new DelBlacklistInput
            {
                userId = userId,
                targetId = targetId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DelBlacklist(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkDelBlacklistError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：创建聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">创建聊天室输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>创建成功的聊天室ID</returns>
        public static string CreateChatRoom(CreateChatRoomInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return string.Empty;
            }
            var output = string.Empty;
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (!_sdkHttpService.CreateChatRoom(input, ref output, ref errorCode, ref errMsg))
            {
                errorMsg = FormatErrorMsg(Resources.SdkCreateChatRoomError, errMsg);
                return null;
            }
            //SDK检测返回结果
            if (!string.IsNullOrEmpty(output))
            {
                //返回聊天室ID
                return output;
            }
            //创建聊天室错误
            errorMsg = FormatErrorMsg(Resources.SdkCreateChatRoomError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：删除聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="operateId">操作者ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功删除聊天室</returns>
        public static bool DeleteChatRoom(string roomId, string operateId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new DeleteChatRoomInput
            {
                roomId = roomId,
                operateId = operateId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DeleteChatRoom(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkDeleteChatRoomError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：添加聊天室成员
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">添加聊天室成员输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否添加成功</returns>
        public static bool AddChatRoomMembers(AddChatRoomMembersInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.AddChatRoomMembers(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkAddChatRoomMembersError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：删除聊天室成员
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">删除聊天室输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteChatRoomMembers(DeleteChatRoomMembersInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DeleteChatRoomMembers(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkDelChartRommMemberError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：更新聊天室员属性
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新聊天室成员属性输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateChatRoomMembers(UpdateChatRoomMemberInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateChatRoomMembers(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkUpdChartRommMemberError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：修改聊天室信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更改聊天室输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateChatRoom(UpdateChatRoomInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateChatRoom(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkUpdChartRommError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：成员退出聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功退出聊天室</returns>
        public static bool ExitChatRoom(string roomId, string userId, string userName, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new ExitChatRoomInput
            {
                roomId = roomId,
                userId = userId,
                userName = userName
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.ExitChatRoom(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //返回错误信息
            errorMsg = FormatErrorMsg(Resources.SdkExtChartRommFail, output);
            return false;
        }

        /// <summary>
        /// 方法说明：查询用户所在聊天室，返回聊天室列表
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室数组</returns>
        public static FindRoomsOutput[] FindRooms(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new FindRoomsInput
            {
                userId = userId
            };
            var output = new List<FindRoomsOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.FindRooms(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQryInChartRommError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：查询单个聊天室详细信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="ope">返回类型-1表示带上群成员列表，0表示不带群成员列表，只返回群信息(默认不带群成员列表)</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室详细信息</returns>
        public static GetChatRoomInfoOutput GetChatRoomInfo(string roomId, string ope, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetChatRoomInfoInput
            {
                roomId = roomId,
                ope = ope
            };
            var output = new GetChatRoomInfoOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetChatRoomInfo(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQrySingleRommError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：查询聊天室所有成员基本信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室成员基本信息数组</returns>
        public static FindRoomMembersOutput[] FindRoomMembers(string roomId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new FindRoomMembersInput
            {
                roomId = roomId
            };
            var output = new List<FindRoomMembersOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.FindRoomMembers(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQryRommMembersInfoError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：查询聊天室单个成员详细信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室单个成员信息</returns>
        public static GetRoomMemberInfoOutput GetRoomMemberInfo(string userId, string roomId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetRoomMemberInfoInput
            {
                userId = userId,
                roomId = roomId
            };
            var output = new GetRoomMemberInfoOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetRoomMemberInfo(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQryRommSingleMembersInfoError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：查询app下的所有聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>所有app下聊天室数组</returns>
        public static FindRoomsOutput[] FindAllRooms(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new List<FindRoomsOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.FindAllRooms(ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQryAllRommInfoError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：邀请加入聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">邀请加入聊天室输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否邀请成功</returns>
        public static bool InviteJoinRoom(InviteJoinRoomInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.InviteJoinRoom(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkInviteJoinRommError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：处理邀请
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">处理邀请加入聊天室输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否处理邀请成功</returns>
        public static bool HandleInvite(HandleInviteInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.HandleInvite(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkHandleInviteRommError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：查询用户所在聊天室，返回聊天室列表以及用户个性化设置
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室列表数组</returns>
        public static FindIndividRoomsOutput[] FindIndividRooms(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new FindIndividRoomsInput
            {
                userId = userId
            };
            var output = new List<FindIndividRoomsOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.FindIndividRooms(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误
            errorMsg = FormatErrorMsg(Resources.SdkQryInAllRommError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：设置聊天室的接收消息类型
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="receiveType">接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息）</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否设置成功</returns>
        public static bool SetRoomReceiveType(string userId, string roomId, int receiveType, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new SetRoomReceiveTypeInput
            {
                userId = userId,
                roomId = roomId,
                receiveType = receiveType
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.SetRoomReceiveType(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误
            errorMsg = FormatErrorMsg(Resources.SdkSetRommMsgTypeError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：设置用户的接收消息类型
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="receiveType">接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息）</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否设置成功</returns>
        public static bool SetUserReceiveType(string userId, int receiveType, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new SetUserReceiveTypeInput
            {
                userId = userId,
                receiveType = receiveType
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.SetUserReceiveType(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误
            errorMsg = FormatErrorMsg(Resources.SdkSetUserMsgTypeError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：查询离线消息(客户端上线调用http接口)
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="oSystem">1–pc 2–web 3–android 4–ios</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功</returns>
        public static object QueryOfflineMsg(string userId, string oSystem, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new QueryOfflineMsgInput
            {
                userId = userId
            };
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var objOuput = new object();
            if (_sdkHttpService.QueryOfflineMsg(input, ref objOuput, ref errorCode, ref errMsg))
            {
                return objOuput;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkQryOffLineInfoError, errMsg);
            return null;
        }
        /// <summary>
        /// 查询离线消息(客户端上线调用http接口)
        /// </summary>
        public static object SynchronusMsgs(SynchronusMsgInput input, ref List<SynchronusMsgOutput> output, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.SynchronusMsgs(input, ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            return null;

        }

        /// <summary>
        /// 方法说明：手机端切换运行状态
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功切换</returns>
        public static bool ChangeAppRunStatus(string version, string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new ChangeAppRunStatusInput
            {
                version = version,
                userId = userId,
                isBackground = 0 //App运行状态 1–后台运行 0–前台运行
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.ChangeAppRunStatus(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkChangeAppStatusError, output);
            return false;
        }

        /// <summary>
        /// 更新小米、华为、魅族、信鸽的信息[POST]
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>更新小米、华为、魅族、信鸽的信息输出</returns>
        public static bool UpdatePushDeviceToken(string version, string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new UpdatePushDeviceTokenInput
            {
                version = version,
                userId = userId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdatePushDeviceToken(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            else
            {
                System.Threading.Thread.Sleep(500);
                if (_sdkHttpService.UpdatePushDeviceToken(input, ref output, ref errorCode, ref errMsg))
                {
                    return true;
                }
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkUpdatePushDeviceTokenError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：获取用户的讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>用户所在讨论组数组</returns>
        public static GetGroupListOutput[] GetGroupList(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetGroupListInput
            {
                userId = userId
            };
            var output = new List<GetGroupListOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetGroupList(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误信息
            errorMsg = FormatErrorMsg(Resources.SdkGetUserGroupError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：获取讨论组成员信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>讨论组成员数组</returns>
        public static GetGroupMembersOutput[] GetGroupMembers(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetGroupMembersInput
            {
                userId = userId,
                groupId = groupId
            };
            var output = new List<GetGroupMembersOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetGroupMembers(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误信息
            errorMsg = FormatErrorMsg(Resources.SdkGroupUserInfoError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：退出讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功退出</returns>
        public static bool GroupExitor(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new GroupExitorInput
            {
                userId = userId,
                groupId = groupId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GroupExitor(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkGroupExitError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：群主转让
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">群主转让输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功转让</returns>
        public static bool GroupOwnerChange(GroupOwnerChangeInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GroupOwnerChange(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkGroupOwnerChangeError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：群管理员设置
        /// 完成时间：2017-08-17
        /// </summary>
        /// <param name="input">群管理员设置输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功设置</returns>
        public static bool GroupManagerSet(GroupManagerChangeInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GroupManagerSet(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkGroupOwnerChangeError, output);
            return false;
        }
        /// <summary>
        /// 方法说明：创建讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">创建讨论组输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>创建成功的讨论组信息</returns>
        public static CreateGroupOutput CreateGroup(CreateGroupInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new CreateGroupOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.CreateGroup(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误信息
            errorMsg = FormatErrorMsg(Resources.SdkCreateGroupError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：更新讨论组信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新讨论组信息输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更新讨论组信息</returns>
        public static bool UpdateGroup(UpdateGroupInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateGroup(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkUpdateGroupError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：更新用户在讨论组的设置
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新用户在讨论组的设置输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更新用户在讨论组的设置</returns>
        public static bool UpdateGroupConfig(UpdateGroupConfigInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateGroupConfig(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkUpdateUserGroupConfigError, output);
            return false;
        }

        /// <summary> 
        /// 方法说明：解散讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功解散讨论组</returns>
        public static bool DissolveGroup(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new DissolveGroupInput
            {
                userId = userId,
                groupId = groupId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DissolveGroup(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkDissolveGroupError, output);
            return false;
        }

        /// <summary>
        /// 方法说明：获取群的所有公告
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">群组ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>群的所有公告实体数组</returns>
        public static GetGroupNotificationsOutput[] GetGroupNotifications(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetGroupNotificationsInput
            {
                userId = userId,
                targetId = groupId
            };
            var output = new List<GetGroupNotificationsOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetGroupNotifications(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkGetGroupNotificationsError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：通过ID获取群公告
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>公告信息</returns>
        public static GetNotificationsByIdOutput GetNotificationsById(string notificationId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetNotificationsByIdInput
            {
                notificationId = notificationId
            };
            var output = new GetNotificationsByIdOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetNotificationsById(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkGetNotificationsByIdError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：添加群公告（群主才有权限）
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">添加群组公告输入实体</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>添加成功的群组公告信息</returns>
        public static AddNotificationsOutput AddNotifications(AddNotificationsInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new AddNotificationsOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.AddNotifications(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkAddNotificationsError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：用户修改公告状态为已读
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">群组ID</param>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更改公告的状态为已读</returns>
        public static bool UpdateNotificationsState(string userId, string groupId, string notificationId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new UpdateNotificationsStateInput
            {
                userId = userId,
                targetId = groupId,
                notificationId = notificationId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.UpdateNotificationsState(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkUpdateNotificationsStateError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：删除群公告(群主才有权限)
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功删除公告</returns>
        public static bool DeleteNotificationsById(string userId, string notificationId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new DeleteNotificationsByIdInput
            {
                userId = userId,
                notificationId = notificationId
            };
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DeleteNotificationsById(input, ref output, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 创建群投票
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static GetVoteInfoOutput CreateGroupVote(CreateGroupVoteInput input, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }

            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new GetVoteInfoOutput();
            if (_sdkHttpService.CreateGroupVoting(input, ref output, groupId, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, errMsg);
            return null;
        }

        /// <summary>
        /// 获取投票详情
        /// </summary>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GetVoteInfoOutput GetVoteInfo(int voteId, string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new GetVoteInfoOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetVoteInfo(ref output, voteId, userId, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkAddNotificationsError, errMsg);
            return null;
        }

        /// <summary>
        /// 获取所有弃权票信息
        /// </summary>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GroupAbstentionVoteOutput GetGroupAbstentionVote(int voteId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new GroupAbstentionVoteOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetGroupAbstentionVote(ref output, voteId, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkAddNotificationsError, errMsg);
            return null;
        }

        /// <summary>
        /// 根据群ID获取群的所有投票活动
        /// </summary>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GetGroupVotesOutput GetGroupVotes(string groupId, ref int errorCode, ref string errorMsg, int page = 0, int size = 0, string userId = "")
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetGroupVotesInput
            {
                page = page,
                size = size,
                userId = userId
            };
            var output = new GetGroupVotesOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetGroupVotes(input, ref output, groupId, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkAddNotificationsError, errMsg);
            return null;
        }

        /// <summary>
        /// 根据ID删除投票活动
        /// </summary>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool DeleteGroupVote(int voteId, string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var input = new DeteleGroupVoteInput
            {
                userId = userId
            };
            if (_sdkHttpService.DeleteGroupVote(input, voteId, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 提交群投票选项
        /// </summary>
        /// <param name="input"></param>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool SubmitGroupVoteOptions(GroupVoteOptionInput input, int voteId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.SubmitGroupVoteOptions(input, voteId, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;
        }
        #region 活动
        /// <summary>
        /// 发布活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool ReleaseGroupActivity(ReleaseGroupActivityInput input, string groupId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.ReleaseGroupActivity(input, groupId, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 根据群ID获取群的活动列表
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GetGroupActivitysOutput GetGroupActivitys(GetGroupActivitysInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new GetGroupActivitysOutput();
            if (_sdkHttpService.GetGroupActivitys(input, ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, errMsg);
            return null;
        }

        /// <summary>
        /// 获取活动详情
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GroupActivityDetail GetActivityInfo(GetGroupActivityDetailsInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new GroupActivityDetail();
            if (_sdkHttpService.GetActivityInfo(input, ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, errMsg);
            return null;
        }
        /// <summary>
        /// 根据ID删除活动
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool DeleteGroupActivity(DeleteGroupActivityInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.DeleteGroupActivity(input, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;
        }

        /// <summary>
        /// 活动参与者列表
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static GetGroupActivityParticipatorsOutput GetGroupActivityParticipators(GetGroupActivityParticipatorInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new GetGroupActivityParticipatorsOutput();
            if (_sdkHttpService.GetGroupActivityParticipators(input, ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, errMsg);
            return null;
        }

        /// <summary>
        /// 提交参与活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool ParticipateActivities(ParticipateActivitiesInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var output = new BaseOutput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.ParticipateActivities(input, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, output, errMsg);
            return false;

        }

        /// <summary>
        /// 获取活动默认主题图片
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static GetGroupActivityDefaultImageOutput GetActivityImages(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new GetGroupActivityDefaultImageOutput();
            if (_sdkHttpService.GetActivityImages(ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.SdkDeleteNotificationsByIdError, errMsg);
            return null;
        }
        #endregion
        #region 打卡
        /// <summary>
        /// 确认打卡
        /// </summary>
        /// <param name="attendId">打卡ID</param>
        /// <param name="userIp">用户IP</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool ConfirmVerify(string attendId, string userIp, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new ConfirmPunchInput { attendId = attendId, userIp = userIp };
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.ConfirmVerify(input, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            return false;
        }
        /// <summary>
        /// 获取打卡记录列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pageNum">当前页</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static GetPunchClocksOutput GetPunchClocks(string userId, int pageNum, int pageSize, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new GetPunchClocksInput { userId = userId, pageNum = pageNum, pageSize = pageSize };
            var errMsg = string.Empty;
            var output = new GetPunchClocksOutput();
            if (_sdkHttpService.GetPunchClocks(input, ref output, ref errorCode, ref errMsg))
            {
                return output;
            }
            //错误返回
            errorMsg = errMsg;
            return null;
        }
        #endregion
        /// <summary>
        /// 方法说明：消息漫游
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">消息漫游输入</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>成功漫游的消息数组</returns>
        public static RoamMessageOutput[] RoamMessage(RoamMessageInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new List<RoamMessageOutput>();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.RoamMessage(input, ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output.ToArray();
            }
            //错误信息
            errorMsg = FormatErrorMsg(Resources.SdkRoamMessageError, errMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：文件上传
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="scid">文件上传信息</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>上传数据信息</returns>
        public static SdkFileUpLoadOutput FileUpload(SdkSendFileInput scid, ref int errorCode, ref string errorMsg)
        {
            return !CheckSdkServiceState(ref errorMsg) ? null : _sdkHttpService.FileUpload(scid, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：文件上传MD5校验
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="msgMd5">MD5信息</param>
        /// <param name="fileName">文件物理路径</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>MD5校验信息</returns>
        public static SdkFileUpLoadOutput CompareFileMd5(string msgMd5, string fileName, ref int errorCode, ref string errorMsg)
        {
            return !CheckSdkServiceState(ref errorMsg) ? null : _sdkHttpService.CompareFileMd5(msgMd5, fileName, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 获取系统当前时间[GET]
        /// </summary>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功</returns>
        public static QuerySystemDateOuput GetCurrentSysTime(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new QuerySystemDateOuput();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_sdkHttpService.GetCurrentSysTime(ref output, ref errorCode, ref errMsg) && output != null)
            {
                return output;
            }
            //错误信息
            errorMsg = FormatErrorMsg(Resources.SdkRoamMessageError, errMsg);
            return null;
        }
    }
}
