using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.AntSdkDAL;
using SDK.Service;
using SDK.Service.Models;
using SDK.AntSdk.Properties;
using System.Diagnostics;
using System.IO;
using GroupVoteContent = SDK.AntSdk.AntModels.GroupVoteContent;
using VoteOptionInfo = SDK.AntSdk.AntModels.VoteOptionInfo;
using SDK.AntSdk.AntModels.Http;
using SDK.AntSdk.DAL;
using System.Text;
using System.Threading;

namespace SDK.AntSdk
{
    /// <summary>
    /// 触角服务唯一接口类（所有APP的UI需要的调用都在此）
    /// </summary>
    public class AntSdkService
    {
        /// <summary>
        /// 触角SDK：启动状态：true 启动；false 停止；
        /// </summary>
        internal static bool AntSdkActivateState { get; private set; }

        /// <summary>
        /// 触角SDK：配置信息
        /// </summary>
        public static AntSdkConfig AntSdkConfigInfo { get; private set; }

        /// <summary>
        /// 触角SDK：组织架构
        /// </summary>
        public static AntSdkListContactsOutput AntSdkListContactsEntity { get; private set; }

        /// <summary>
        /// 触角SDK：登录返回信息
        /// </summary>
        public static AntSdkLoginUserInfo AntSdkLoginOutput { get; private set; }

        /// <summary>
        /// 触角SDK：当前用户信息
        /// </summary>
        public static AntSdkUserInfo AntSdkCurrentUserInfo { get; set; }

        /// <summary>
        /// 用于过滤重复数据,暂时保存到系统缓存，后续需改为本地数据库
        /// </summary>
        public static volatile List<AntSdkMsBase> SysUserMsgList;

        /// <summary>
        /// 用于过滤重复数据,暂时保存到系统缓存，后续需改为本地数据库
        /// </summary>
        public static volatile List<AntSdkReceivedGroupMsg.GroupBase> SysGroupMsgList;

        /// <summary>
        /// 用于过滤重复数据，暂时保存到系统缓存，后续需改为本地数据库
        /// </summary>
        public static volatile List<AntSdkChatMsg.ChatBase> ChaMsgList;

        /// <summary>
        /// 触角SDK：群发助手
        /// </summary>
        public const string AntSdkMassAssistantId = "P123456789000000";

        /// <summary>
        /// 触角SDK：处理条件信息
        /// </summary>
        public static string AntSdkWhereFrom = string.Empty;

        /// <summary>
        /// 触角SDK：MQTT连接状态
        /// </summary>
        public static MsConnectionState AntSdkConnectionState => SdkService.ConnectionState;

        /// <summary>
        /// 触角SDK：暴露的平台SDKToken（主要是为UI使用SDK上传接口地址校验使用）
        /// </summary>
        public static string AntSdkToken => SdkService.SdkSysParam.Token;

        /// <summary>
        /// 触角SDK：MQTT是否连接
        /// </summary>
        public static bool AntSdkIsConnected => SdkService.IsConnected;

        /// <summary>
        /// 触角SDK当前用户数据库地址
        /// </summary>
        internal static string SqliteLocalDbPath { get; private set; }

        /// <summary>
        /// 触角SDKHttp请求方法设定
        /// </summary>
        internal static AntSdkHttpMethod MdAntsdkhttpMethod { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public static C_User_InfoDAL _cUserInfoDal = new C_User_InfoDAL();
        /// <summary>
        /// 部门信息
        /// </summary>
        private static C_DepartmentDAL _cDepartmentDal = new C_DepartmentDAL();
        /// <summary>
        /// 组织架构版本号
        /// </summary>
        private static C_Version _cVersion = new C_Version();
        /// <summary>
        /// 触角SDK：触角服务内主题集合类
        /// </summary>
        public static class AntSdkTopicClass
        {
            /// <summary>
            /// 版本更新
            /// </summary>
            public const string UpdatePcVersion = "update/pc/version";

            /// <summary>
            /// 消息已读
            /// </summary>
            public const string MessageRead = "message_read";

            /// <summary>
            /// 消息已收
            /// </summary>
            public const string MessageReceive = "message_receive";

            /// <summary>
            /// 消息发送
            /// </summary>
            public const string MessageSend = "message_send";

            /// <summary>
            /// 用户B收到A的阅后即焚消息，发送已读回执给消息服务
            /// </summary>
            public const string MessageBurn = "message_burn";
        }
        #region   //触角SDK 私有处理辅助完成触角SDK
        /// <summary>
        /// 提示消息模式:true-显示SDK提示，false-不显示SDK提示
        /// </summary>
        private static bool _errorMode;

        /// <summary>
        /// 接收离线消息处理
        /// </summary>
        private static readonly object Objct = new object();

        /// <summary>
        /// 接收离线消息处理
        /// </summary>
        private static bool _isFlag = false;

        /// <summary>
        /// 消息控制器集合
        /// </summary>
        //private static List<IMessageHelper> _lstMessageHelper;

        /// <summary>
        /// 本地未读消息缓存集合
        /// </summary>
        private static AntSdkDictionary<string, AntSdkBurnFlag, List<AntSdkChatMsg.ChatBase>> LocalUnreadMsgList;
        /// <summary>
        /// 所有的离线消息缓存集合
        /// </summary>
        private static AntSdkDictionary<string, AntSdkBurnFlag, List<AntSdkChatMsg.ChatBase>> OfflineMsgs;

        /// <summary>
        /// 类型定义
        /// </summary>
        /// <typeparam name="TKey1"></typeparam>
        /// <typeparam name="TKey2"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public class AntSdkDictionary<TKey1, TKey2, TValue> : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue>
        {

            public TValue this[TKey1 key1, TKey2 key2]
            {
                get { return base[Tuple.Create(key1, key2)]; }
                set { base[Tuple.Create(key1, key2)] = value; }
            }

            public void Add(TKey1 key1, TKey2 key2, TValue value)
            {
                base.Add(Tuple.Create(key1, key2), value);
            }

            public bool ContainsKey(TKey1 key1, TKey2 key2)
            {
                return base.ContainsKey(Tuple.Create(key1, key2));
            }
        }

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
        /// 触角SDK Http服务对象
        /// </summary>
        private static AntSdkHttpService _antsdkhttpService;

        /// <summary>
        /// 方法说明：检查触角 SDK 状态
        /// 完成时间：2016-05-23
        /// </summary>
        /// <param name="errorMsg">错误提示</param>
        /// <returns></returns>
        private static bool CheckSdkServiceState(ref string errorMsg)
        {
            //第一步，检查启动状态
            if (AntSdkActivateState) { return true; }
            //后续如果再检查，在这里补充调用函数条件
            errorMsg += Resources.AntSdkNotStart;
            return false;
            //返回
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
        #endregion

        #region   //触角SDK 启动|登录|停止SDK服务

        /// <summary>
        /// 方法说明：使用触角SDK之前需要启动触角SDK服务
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="antsdkConfig">触角SDK配置信息</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功开启平台SDK</returns>
        public static bool StartAntSdk(AntSdkConfig antsdkConfig, ref string errorMsg)
        {
            if (string.IsNullOrEmpty(antsdkConfig?.AntServiceHttpPrdfix))
            {
                errorMsg += Resources.AntSdkServiceHttpPrifxNotNull;
                return false;
            }

            if (string.IsNullOrEmpty(antsdkConfig.CustomersHttpPrdfix))
            {
                errorMsg += Resources.AntSdkCustomsHttpPrifxNotNull;
                return false;
            }

            if (string.IsNullOrEmpty(antsdkConfig.AppVersion))
            {
                errorMsg += Resources.AntSdkAppVersionNotNull;
                return false;
            }

            if (
                !(antsdkConfig.SetCompanyCode & antsdkConfig.SetAppKey & antsdkConfig.SetAppSecret &
                  antsdkConfig.SetHttpPrdfix & antsdkConfig.SetDataBaseAddress & antsdkConfig.SetLogMode))
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
                    if (!antsdkConfig.SetCompanyCode && keysList.Contains("SdkCompanyCode"))
                    {
                        antsdkConfig.AntSdkCompanyCode = config.AppSettings.Settings["SdkCompanyCode"].Value;
                    }
                    if (!antsdkConfig.SetAppKey && keysList.Contains("SdkAppKey"))
                    {
                        antsdkConfig.AntSdkAppKey = config.AppSettings.Settings["SdkAppKey"].Value;
                    }
                    if (!antsdkConfig.SetAppSecret && keysList.Contains("SdkAppSecret"))
                    {
                        antsdkConfig.AntSdkAppSecret = config.AppSettings.Settings["SdkAppSecret"].Value;
                    }
                    if (!antsdkConfig.SetHttpPrdfix && keysList.Contains("SdkHttpPrdfix"))
                    {
                        antsdkConfig.AntSdkHttpPrdfix = config.AppSettings.Settings["SdkHttpPrdfix"].Value;
                    }
                    if (!antsdkConfig.SetDataBaseAddress && keysList.Contains("SdkDataBaseAddress"))
                    {
                        string path = "";
                        if (AntSdkDataConverter.IsPreRelease())
                        {
                            //程序运行目录
                            path = AppDomain.CurrentDomain.BaseDirectory + "\\AntennaChat";
                        }
                        else
                        {
                            //公共目录
                            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AntennaChat";
                        }
                        antsdkConfig.AntSdkDatabaseAddress = path;

                        //antsdkConfig.AntSdkDatabaseAddress = config.AppSettings.Settings["SdkDataBaseAddress"].Value;
                    }
                    if (!antsdkConfig.SetLogMode)
                    {
                        bool setbool;
                        if (keysList.Contains("SdkDebugLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkDebugLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            antsdkConfig.AntSdkLogMode |= AntSdkLogLevel.DebugLogEnable;
                        }
                        if (keysList.Contains("SdkInfoLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkInfoLogEnable"].Value, out setbool) && setbool)
                        {
                            antsdkConfig.AntSdkLogMode |= AntSdkLogLevel.InfoLogEnable;
                        }
                        if (keysList.Contains("SdkWarnLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkWarnLogEnable"].Value, out setbool) && setbool)
                        {
                            antsdkConfig.AntSdkLogMode |= AntSdkLogLevel.WarnLogEnable;
                        }
                        if (keysList.Contains("SdkErrorLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkErrorLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            antsdkConfig.AntSdkLogMode |= AntSdkLogLevel.ErrorLogEnable;
                        }
                        if (keysList.Contains("SdkFatalLogEnable") &&
                            bool.TryParse(config.AppSettings.Settings["SdkFatalLogEnable"].Value, out setbool) &&
                            setbool)
                        {
                            antsdkConfig.AntSdkLogMode |= AntSdkLogLevel.FatalLogEnable;
                        }
                    }
                }
            }

            //检查判断SDK启动必备条件[当前是登录时赋值]
            if (string.IsNullOrEmpty(antsdkConfig.AntSdkCompanyCode))
            {
                errorMsg += Resources.AntSdkStartCompanyCodeNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(antsdkConfig.AntSdkAppKey))
            {
                errorMsg += Resources.AntSdkStartAppKeyNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(antsdkConfig.AntSdkAppSecret))
            {
                errorMsg += Resources.AntSdkStartAppSecretNotNull;
                return false;
            }
            if (string.IsNullOrEmpty(antsdkConfig.AntSdkHttpPrdfix))
            {
                errorMsg += Resources.AntSdkStartHttpPrefStuffNotNull;
                return false;
            }

            var sdkparam = new SdkSysParam();
            //校验赋值情况处理设置
            if (antsdkConfig.SetCompanyCode)
            {
                sdkparam.Companycode = antsdkConfig.AntSdkCompanyCode;
            }
            if (antsdkConfig.SetAppKey)
            {
                sdkparam.Appkey = antsdkConfig.AntSdkAppKey;
            }
            if (antsdkConfig.SetAppSecret)
            {
                sdkparam.Appsecret = antsdkConfig.AntSdkAppSecret;
            }
            if (antsdkConfig.SetHttpPrdfix)
            {
                sdkparam.HttpPrdfix = antsdkConfig.AntSdkHttpPrdfix;
            }
            if (antsdkConfig.SetLogMode)
            {
                if (antsdkConfig.AntSdkLogMode.HasFlag(AntSdkLogLevel.DebugLogEnable))
                {
                    sdkparam.SdkLogMode |= SdkLogLevel.DebugLogEnable;
                }
                if (antsdkConfig.AntSdkLogMode.HasFlag(AntSdkLogLevel.InfoLogEnable))
                {
                    sdkparam.SdkLogMode |= SdkLogLevel.InfoLogEnable;
                }
                if (antsdkConfig.AntSdkLogMode.HasFlag(AntSdkLogLevel.WarnLogEnable))
                {
                    sdkparam.SdkLogMode |= SdkLogLevel.WarnLogEnable;
                }
                if (antsdkConfig.AntSdkLogMode.HasFlag(AntSdkLogLevel.ErrorLogEnable))
                {
                    sdkparam.SdkLogMode |= SdkLogLevel.ErrorLogEnable;
                }
                if (antsdkConfig.AntSdkLogMode.HasFlag(AntSdkLogLevel.FatalLogEnable))
                {
                    sdkparam.SdkLogMode |= SdkLogLevel.FatalLogEnable;
                }
            }
            //证书验证触角SDK先设置为不验证
            HttpCertificate.IsCertificate = false;
            //启动平台SDK
            if (!SdkService.StartSdk(sdkparam, ref errorMsg)) { return false; }
            AntSdkConfigInfo = antsdkConfig;
            //触角服务HTTP对象
            _antsdkhttpService = new AntSdkHttpService();
            AntSdkActivateState = true;
            //设置http请求方法
            MdAntsdkhttpMethod = new AntSdkHttpMethod();
            //启动成功
            return true;
        }

        /// <summary>
        /// 方法说明：触角SDK登录 触角服务登录->平台SDK登录->获取当前用户->获取组织架构->初始化当前本地数据库
        /// 完成时间：2017-06-02
        /// </summary>
        /// <param name="loginName">用户登录ID</param>
        /// <param name="passWord">用户登录密码</param>
        /// <param name="state">当前用户状态</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>SDK是否登录成功</returns>
        public static bool AntSdkLogin(string loginName, string passWord, int state, string validateCode, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //先做触角服务登录
            if (!CustomerLogin(loginName, passWord, state, validateCode, ref errorCode, ref errorMsg))
            {
                return false;
            }
            //赋值
            if (!string.IsNullOrEmpty(AntSdkLoginOutput?.companyCode))
            {
                //触角 SDK [启动触角SDK时会赋值->这里重新赋值]公司代码赋值
                AntSdkConfigInfo.AntSdkCompanyCode = AntSdkLoginOutput.companyCode;
                //平台 SDK 公司代码赋值[启动平台SDK时会赋值->这里重新赋值]
                SdkService.SdkSysParam.Companycode = AntSdkLoginOutput.companyCode;
                //如果公司代码有变动，则重新进行构造HTTP请求方法
                MdAntsdkhttpMethod = new AntSdkHttpMethod();
                SdkService.MdsdkhttpMethod = new SdkHttpMethod();
            }
            //再用服务登录的UserID做SDK登录
            if (!SdkService.SdkLogin(AntSdkLoginOutput?.userId, ref errorCode, ref errorMsg))
            {
                return false;
            }
            //文件上传接口地址
            AntSdkConfigInfo.AntSdkFileUpload = SdkService.SdkSysParam.FileUpload;
            //多文件上传接口地址
            AntSdkConfigInfo.AntSdkMultiFileUpload = SdkService.SdkSysParam.FileUpload;
            //登录赋值
            SqliteLocalDbPath =
                $@"{AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkCurrentUserInfo.companyCode}\";
            if (AntSdkLoginOutput == null)
            {
                return false;
            }
            //初始化数据库
            if (AntSdkSqliteHelper.AntSdkDataBaseInitialize(AntSdkConfigInfo.AntSdkCompanyCode, AntSdkLoginOutput.userId))
            {
                //获取联系人，组织结构信息
                if (AntSdkContacts())
                {
                    #region 判断离线状态是否有组织架构变更

                    var currentVersion = string.IsNullOrEmpty(AntSdkListContactsEntity.dataVersion)
                        ? 0
                        : Convert.ToInt32(AntSdkListContactsEntity.dataVersion);
                    //获取增量
                    var errMsg = string.Empty;
                    var input = new AntSdkAddListContactsInput { dataVersion = currentVersion.ToString() };
                    AntSdkGetAddContacts(input, ref errorCode, ref errMsg);
                    #endregion
                    #region 更新组织结构中用户的在线状态
                    var userStateList = new List<AntSdkUserStateOutput>();
                    var isResult = _antsdkhttpService.GetUserStateList(ref userStateList, null, ref errorCode, ref errMsg);
                    if (isResult)
                    {
                        AntSdkListContactsEntity.users = AntSdkListContactsEntity.users?.Select(
                            m =>
                            {
                                var userstate = userStateList.FirstOrDefault(n => n.userId == m.userId);
                                if (userstate != null)
                                    m.state = string.IsNullOrEmpty(userstate.state) ? 0 : int.Parse(userstate.state);
                                return m;
                            }).ToList();
                    }
                    #endregion
                    SdkService.ChangeAppRunStatus(AntSdkConfigInfo.AppVersion, AntSdkLoginOutput.userId, ref errorCode, ref errorMsg);
                    return true;
                }
                errorMsg = "获取组织架构失败";
                return false;
            }
            errorMsg = "数据库初始化失败";
            var dbPath = $@"{AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkConfigInfo.AntSdkCompanyCode}\{
                AntSdkLoginOutput.userId}\{AntSdkLoginOutput.userId}.db";
            if (!File.Exists(dbPath)) return false;
            //判断是否为0KB
            var file = new FileInfo(dbPath);
            var size = file.Length;
            if (size != 0) return false;
            var path =
                $@"{AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkConfigInfo.AntSdkCompanyCode}\{
                    AntSdkLoginOutput.userId}";
            AntSdkDataConverter.DeleteDirectory(path);
            return false;
        }

        /// <summary>
        /// 方法说明：触角SDK登录成功后的默认主题定于
        /// 完成时间：2017-08-08
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool AntSdkDefaultTopicSubscribe(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //为登录，则不能订阅
            if (AntSdkLoginOutput == null)
            {
                return false;
            }
            //平台SDK订阅
            if (!SdkService.SdkDefaultTopicSubscribe(AntSdkLoginOutput.userId, ref errorMsg))
            {
                return false;
            }
            //登录成功，订阅触角默认主题
            var topics = new List<string>
            {
                $"ant/{SdkService.SdkSysParam.Token}",
                $"ant/{AntSdkConfigInfo.AntSdkCompanyCode}"
            };
            //订阅默认主题
            return SdkService.Subscribe(topics.ToArray(), ref errorMsg);
        }

        /// <summary>
        /// 方法说明：获取联系人信息
        /// 完成时间：2017-06-06
        /// </summary>
        /// <returns>是否成功获取</returns>
        private static bool AntSdkContacts()
        {
            //用于判断是否需要全量获取的标识文件
            var flagFileName = $@"{AntSdkService.AntSdkConfigInfo.AntSdkDatabaseAddress}localData\{AntSdkConfigInfo.AntSdkCompanyCode}\{AntSdkLoginOutput.userId}\{AntSdkLoginOutput.userId}.txt";
            var errorCode = 0;
            var errMsg = string.Empty;
            //判断数据库中组织架构是否已经存在数据
            var contactList = _cUserInfoDal.GetList();
            var deparmentList = _cDepartmentDal.GetList();
            //HTTP全量获取
            if (contactList == null || contactList.Count == 0 || deparmentList == null || deparmentList.Count == 0)
            {
                //清空一次 防止联系人或者部门信息不为空 信息入库失败, 
                _cDepartmentDal.AllDelete();
                _cUserInfoDal.AllDelete();
                _cVersion.AllDelete();
                if (!File.Exists(flagFileName))
                {
                    File.Create(flagFileName);
                }
                return AntSdkGetListContacts(ref errorCode, ref errMsg);
            }
            if (!File.Exists(flagFileName))
            {
                //清空一次 防止联系人或者部门信息不为空 信息入库失败, 
                _cDepartmentDal.AllDelete();
                _cUserInfoDal.AllDelete();
                _cVersion.AllDelete();
                File.Create(flagFileName);
                return AntSdkGetListContacts(ref errorCode, ref errMsg);
            }
            //读取本地数据库
            else
            {
                var ver = _cVersion.Select();
                if (AntSdkListContactsEntity == null)
                    AntSdkListContactsEntity = new AntSdkListContactsOutput();
                AntSdkListContactsEntity.users = contactList.ToList();
                AntSdkListContactsEntity.departs = deparmentList.ToList();
                AntSdkListContactsEntity.dataVersion = ver;
                return true;
            }
            #region 临时解决方案
            //var errorCode = 0;
            //var errMsg = string.Empty;
            ////是否删除数据库
            //var isGetAll = AntSdkDataConverter.xmlFind("isGetAllUser",
            //    AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
            //if (isGetAll == "true")
            //{
            //    LogHelper.WriteDebug("清除成员表、部门表、版本号表信息，重新获取全量。");
            //    _cDepartmentDal.AllDelete();
            //    _cUserInfoDal.AllDelete();
            //    _cVersion.AllDelete();
            //    if (AntSdkGetListContacts(ref errorCode, ref errMsg))
            //    {
            //        try
            //        {
            //            LogHelper.WriteDebug("获取全量信息成功，修改配置文件。");
            //            AntSdkDataConverter.xmlModify("isGetAllUser", "false",
            //                 AppDomain.CurrentDomain.BaseDirectory + "projectStatic.xml");
            //            return true;
            //        }
            //        catch (Exception ex)
            //        {
            //            LogHelper.WriteError("[修改配置文件projectStatic.xml出错]："+ex.Message+ex.StackTrace);
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            ////读取本地数据库
            //else
            //{
            //    LogHelper.WriteDebug("从本地获取组织架构信息。");
            //    var contactList = _cUserInfoDal.GetList();
            //    var deparmentList = _cDepartmentDal.GetList();
            //    var ver = _cVersion.Select();
            //    if (contactList == null || contactList.Count == 0 || deparmentList == null || deparmentList.Count == 0)
            //    {
            //        //清空一次 防止联系人或者部门信息不为空 信息入库失败
            //        _cDepartmentDal.AllDelete();
            //        _cUserInfoDal.AllDelete();
            //        _cVersion.AllDelete();
            //        return AntSdkGetListContacts(ref errorCode, ref errMsg);
            //    }
            //    if (AntSdkListContactsEntity == null)
            //        AntSdkListContactsEntity = new AntSdkListContactsOutput();
            //    AntSdkListContactsEntity.users = contactList.ToList();
            //    AntSdkListContactsEntity.departs = deparmentList.ToList();
            //    AntSdkListContactsEntity.dataVersion = ver;
            //    return true;
            //}
            #endregion
        }

        /// <summary>
        /// 方法说明：停止SDK服务
        /// 完成时间：2017-04-20
        /// </summary>
        public static bool StopAntSdk(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //先做触角服务的注销登录
            if (!CustomerLoginOut(ref errorCode, ref errorMsg))
            {
                return false;
            }
            //先做平台SDK服务停止
            if (!SdkService.StopSdk(ref errorMsg))
            {
                return false;
            }
            //停止
            AntSdkActivateState = false;
            return true;
        }

        #endregion

        #region   //触角SDK 触角服务HTTP方法

        /// <summary>
        /// 方法说明：触角APP检查更新
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>检查更新结果</returns>
        public static AntSdkUpgradeOutput AntSdkCheckUpgrade(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new AntSdkUpgradeOutput();
            var tempMsg = string.Empty;
            if (_antsdkhttpService.CheckUpgrade(ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkCheckUpdateError, tempMsg);
            return null;
        }
        public static bool GetVerifyCodeMethod(string mobileNum, ref int errorCode, ref string errorMsg)
        {
            AntSdkBaseOutput antSdkBaseOutput = new AntSdkBaseOutput();
            return _antsdkhttpService.GetVerifyCodeInfo(mobileNum, ref errorCode, ref errorMsg);
        }
        public static bool SentVerifyCodeMethod(AntSdkSendVerifyCodeInput input, ref int errorCode, ref string errorMsg, ref string data)
        {
            return _antsdkhttpService.SendVerifyCode(input, ref errorCode, ref errorMsg, ref data);
        }
        public static bool ResetPassWordMethod(AntSdkResetPassWoldInput input, ref int errorCode, ref string errorMsg)
        {
            return _antsdkhttpService.ResetPassWord(input, ref errorCode, ref errorMsg);
        }
        /// <summary>
        /// 方法说明：触角客户端登录（B端系统接口）
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="passWord">密码</param>
        /// <param name="state">当前用户的登录状态</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功登录</returns>
        private static bool CustomerLogin(string loginName, string passWord, int state, string validateCode, ref int errorCode, ref string errorMsg)
        {
            var input = new AntSdkLoginInput
            {
                loginName = loginName,
                password = passWord,
                validateCode = validateCode
            };
            var tempMsg = string.Empty;
            var token = _antsdkhttpService.CustomerLogin(input, ref errorCode, ref tempMsg);
            if (!string.IsNullOrEmpty(token))
            {
                AntSdkLoginOutput = new AntSdkLoginUserInfo
                {
                    PWD = passWord,
                    token = token
                };
                //获取登录输出用户信息的用户ID
                var output = new AntSdkUserInfo();
                AntSdkUserInfoInput userInfoInput = new AntSdkUserInfoInput { state = state };
                if (_antsdkhttpService.GetCurrentUserInfo(userInfoInput, ref output, ref errorCode, ref tempMsg) && output != null)
                {
                    AntSdkLoginOutput.userId = output.userId;
                    AntSdkLoginOutput.companyCode = output.companyCode;
                    AntSdkCurrentUserInfo = output;
                    AntSdkCurrentUserInfo.loginName = loginName;
                    if (!string.IsNullOrEmpty(output.companyCode))
                    {
                        AntSdkConfigInfo.AntSdkCompanyCode = output.companyCode;
                    }
                    if (!string.IsNullOrEmpty(output.appkey))
                    {
                        AntSdkConfigInfo.AntSdkAppKey = output.appkey;
                    }
                    if (!string.IsNullOrEmpty(output.appsecret))
                    {
                        AntSdkConfigInfo.AntSdkAppSecret = output.appsecret;
                    }
                    //返回成功
                    return true;
                }
            }
            //返回错误
            errorMsg = FormatErrorMsg(Resources.AntSdkCustomerLoginError, tempMsg);
            return false;
        }
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static string GetVerifyCodeImage(string mobile, ref int errorCode, ref string errorMsg)
        {
            return _antsdkhttpService.GetVerifyCodeImage(mobile, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：获取用户信息
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>用户信息</returns>
        public static AntSdkUserInfo AntSdkGetUserInfo(string userId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new AntSdkGetUserInput
            {
                targetId = userId
            };
            var output = new AntSdkUserInfo();
            var tempMsg = string.Empty;
            if (_antsdkhttpService.GetUserInfo(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output;
            }
            //返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：用户退出登录（B端系统接口）
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns>是否成功退出登录</returns>
        private static bool CustomerLoginOut(ref int errorCode, ref string errorMsg)
        {
            var tempMsg = string.Empty;
            if (_antsdkhttpService.CustomerLoginOut(ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkLoginOutError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：切换用户状态（在线、离开、忙碌等）
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">切换用户状态输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功切换</returns>
        public static bool AntSdkChangeUserState(AntSdkChangeUserStateInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.ChangeUserState(input, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkChangeUserStateError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：切换当前用户状态（在线、离开、忙碌等）
        /// 完成时间：2017-08-22
        /// </summary>
        /// <param name="state">切换用户状态输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功切换</returns>
        public static bool AntSdkUpdateCurrentUserState(int state, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.UpdateCurrentUserState(state, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkChangeUserStateError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：获取所有用户的状态
        /// 完成时间：2017-08-22
        /// </summary>
        /// <param name="output">所有在线用户状态输出</param>
        /// <param name="users">用户信息</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns></returns>
        public static bool AntSdkGetUserStateList(ref List<AntSdkUserStateOutput> output, string[] users, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.GetUserStateList(ref output, users, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkChangeUserStateError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：获取部门下所有用户在线状态
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="departmentId">部门ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>部门下所有用户在线状态</returns>
        public static AntSdkGetUserStateOutput[] AntSdkGetDepartmentUserState(string departmentId, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var input = new AntSdkGetUserStateInput
            {
                departmentId = departmentId
            };
            var tempMsg = string.Empty;
            var output = new List<AntSdkGetUserStateOutput>();
            if (_antsdkhttpService.GetDepartmentUserState(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output.ToArray();
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetDepartUserStateError, tempMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：更新当前用户信息
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">更新用户输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否更新成功</returns>
        public static bool AntSdkUpdateUser(AntSdkUpdateUserInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.UpdateUser(input, ref errorCode, ref tempMsg))
            {
                AntSdkCurrentUserInfo.picture = input.picture;
                AntSdkCurrentUserInfo.sex = input.sex;
                //AntSdkCurrentUserInfo.voiceMode = input.voiceMode;
                //AntSdkCurrentUserInfo.vibrateMode = input.vibrateMode;
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetDepartUserStateError, tempMsg);
            return false;
        }


        /// <summary>
        /// 方法说明：获取用户回复用语设置
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">取用户回复用语设置输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取</returns>
        public static AntSdkGetUserReturnSettingOutput[] AntSdkGetUserReturnSetting(AntSdkGetUserReturnSettingInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var tempMsg = string.Empty;
            var output = new List<AntSdkGetUserReturnSettingOutput>();
            if (_antsdkhttpService.GetUserReturnSetting(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output.ToArray();
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetUserReturnSettingError, tempMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：更新用户系统设置
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">更新用户系统设置输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public static bool AntSdkUpdateUserSystemSetting(AntSdkUpdateUserSystemSettingInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.UpdateUserSystemSetting(input, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetUserReturnSettingError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：获取意见反馈类型
        /// 完成时间：2017-06-09
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取意见反馈类型</returns>
        public static AntSdkGetUserIdeaTypeOutput[] AntSdkGetUserIdeaType(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var tempMsg = string.Empty;

            var input = new AntSdkGetUserIdeaTypeInput();
            var output = new List<AntSdkGetUserIdeaTypeOutput>();
            if (_antsdkhttpService.GetUserIdeaType(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output.ToArray();
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetUserIdeaTypeError, tempMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：添加意见反馈
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">添加意见反馈输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">添加意见反馈输出</param>
        /// <returns>是否添加成功</returns>
        public static bool AntSdkAddIdeaFeedBack(AntSdkAddIdeaFeedBackInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.AddIdeaFeedBack(input, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkAddIdeaFeedBackError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：修改密码
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="input">修改密码输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功修改密码</returns>
        public static bool AntSdkChangePassword(AntSdkChangePasswordInput input, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var tempMsg = string.Empty;
            if (_antsdkhttpService.ChangePassword(input, ref errorCode, ref tempMsg))
            {
                AntSdkLoginOutput.PWD = input.newPassword;
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkChangePasswordError, tempMsg);
            return false;
        }

        /// <summary>
        /// 方法说明：获取表情
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>表情信息</returns>
        public static AntSdkGetFaceInfoOutput[] AntSdkGetFaceInfo(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var tempMsg = string.Empty;
            var input = new AntSdkGetFaceInfoInput();
            var output = new List<AntSdkGetFaceInfoOutput>();
            if (_antsdkhttpService.GetFaceInfo(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output.ToArray();
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkChangePasswordError, tempMsg);
            return null;
        }

        /// <summary>
        /// 方法说明：获取联系人信息
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取</returns>
        public static bool AntSdkGetListContacts(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            var input = new AntSdkListContactsInput();
            var output = new AntSdkListContactsOutput();
            var tempMsg = string.Empty;
            var stopWatch = new Stopwatch();
            var result = _antsdkhttpService.GetListContacts(input, ref output, ref errorCode, ref tempMsg);
            if (result)
            {
                AntSdkListContactsEntity = output;
                //TODO:本地化入库
                //DONE:本地化入库
                if (AntSdkListContactsEntity?.users != null && AntSdkListContactsEntity.users.Count > 0)
                {
                    stopWatch.Start();
                    if (_cUserInfoDal.Insert(AntSdkListContactsEntity.users))
                    {
                        LogHelper.WriteDebug($"[联系人信息入库]总条数：{AntSdkListContactsEntity.users.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                    }
                    else
                    {
                        return false;
                    }
                }
                if (AntSdkListContactsEntity?.departs != null && AntSdkListContactsEntity.departs.Count > 0)
                {
                    stopWatch.Restart();
                    if (_cDepartmentDal.Insert(AntSdkListContactsEntity.departs))
                        LogHelper.WriteDebug(
                            $"[部门信息入库]总条数：{AntSdkListContactsEntity.departs.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                    else
                    {
                        return false;
                    }
                }
                //TODO:当前版本号入库
                //DONE:当前版本号入库
                _cVersion.Insert(output.dataVersion);
                stopWatch.Stop();
                return true;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetContactInfoError, tempMsg);
            return false;
        }

        /// <summary>
        /// 获取当前用的公司信息
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>公司信息</returns>
        public static AntSdkGetCompayInfoOutput AntSdkGetCompanyInfo(ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new AntSdkGetCompayInfoOutput();
            var tempMsg = string.Empty;
            if (_antsdkhttpService.GetCompanyInfo(ref output, ref errorCode, ref tempMsg) && output != null)
            {
                return output;
            }
            //返回
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            errorMsg = tempMsg;
            return null;
        }

        /// <summary>
        /// 方法说明：获取联系人信息—增量信息(返回值区分组织架构变化和不变化两种情况)
        /// 完成时间：2017-06-06
        /// </summary>
        /// <param name="input">获取联系人信息增量信息输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否获取成功</returns>
        public static AntSdkAddListContactsOutput AntSdkGetAddContacts(AntSdkAddListContactsInput input,
            ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return null;
            }
            var output = new AntSdkAddListContactsOutput();
            var tempMsg = string.Empty;
            if (_antsdkhttpService.GetAddContacts(input, ref output, ref errorCode, ref tempMsg) && output != null)
            {
                if (AntSdkListContactsEntity == null)
                {
                    AntSdkListContactsEntity = new AntSdkListContactsOutput();
                }
                var stopWatch = new Stopwatch();
                //增量处理用户信息
                if (output.users?.add?.Count > 0)
                {
                    if (AntSdkListContactsEntity.users == null)
                    {
                        AntSdkListContactsEntity.users = new List<AntSdkContact_User>();
                    }
                    //去重
                    foreach (var user in output.users.add.Select(addUser => AntSdkListContactsEntity.users.FirstOrDefault(v => v.userId == addUser.userId)).Where(user => user != null))
                    {
                        AntSdkListContactsEntity.users.Remove(user);
                        LogHelper.WriteDebug($"[联系人信息出现重复信息]：{user.userName}");
                    }
                    //添加
                    AntSdkListContactsEntity.users.AddRange(output.users.add);
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Start();
                    _cUserInfoDal.Insert(output.users.add);
                    LogHelper.WriteDebug(
                        $"[新增联系人信息入库]总条数：{output.users.add.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                }
                if (output.users?.update?.Count > 0 && AntSdkListContactsEntity.users?.Count > 0)
                {
                    foreach (var user in output.users.update)
                    {
                        var setuser = AntSdkListContactsEntity.users.FirstOrDefault(u => u.userId == user.userId);
                        if (setuser == null) continue;
                        setuser.userId = user.userId;
                        setuser.userName = user.userName;
                        setuser.userNum = user.userNum;
                        setuser.departmentId = user.departmentId;
                        setuser.accid = user.accid;
                        setuser.accToken = user.accToken;
                        setuser.picture = user.picture;
                        setuser.position = user.position;
                        setuser.signature = user.signature;
                        setuser.status = user.status;
                    }
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Restart();
                    _cUserInfoDal.Update(output.users.update);
                    LogHelper.WriteDebug(
                        $"[更新联系人信息入库]总条数：{output.users.update.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                }
                if (output.users?.delete?.Count > 0 && AntSdkListContactsEntity.users != null)
                {
                    foreach (
                        var setuser in
                            output.users.delete.Select(
                                user => AntSdkListContactsEntity.users.FirstOrDefault(u => u.userId == user.userId))
                                .Where(setuser => setuser != null))
                    {
                        AntSdkListContactsEntity.users.Remove(setuser);
                    }
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Restart();
                    _cUserInfoDal.Delete(output.users.delete);
                    LogHelper.WriteDebug(
                        $"[删除联系人信息]总条数：{output.users.delete.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                }
                //增量处理部门信息
                if (output.departs?.add?.Count > 0)
                {
                    if (AntSdkListContactsEntity.departs == null)
                    {
                        AntSdkListContactsEntity.departs = new List<AntSdkContact_Depart>();
                    }
                    //去重
                    foreach (var depart in output.departs.add.Select(addDepart => AntSdkListContactsEntity.departs.FirstOrDefault(v => v.departmentId == addDepart.departmentId)).Where(depart => depart != null))
                    {
                        AntSdkListContactsEntity.departs.Remove(depart);
                        LogHelper.WriteDebug($"[部门信息出现重复信息]：{depart.departName}");
                    }
                    //添加
                    AntSdkListContactsEntity.departs.AddRange(output.departs.add);
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Restart();
                    _cDepartmentDal.Insert(output.departs.add);
                    LogHelper.WriteDebug(
                        $"[新增部门信息入库]总条数：{output.departs.add.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                }
                if (output.departs?.update?.Count > 0 && AntSdkListContactsEntity.departs?.Count > 0)
                {
                    foreach (var dept in output.departs.update)
                    {
                        var setdept =
                            AntSdkListContactsEntity.departs.FirstOrDefault(d => d.departmentId == dept.departmentId);
                        if (setdept == null) continue;
                        setdept.departmentId = dept.departmentId;
                        setdept.departName = dept.departName;
                        setdept.parentDepartId = dept.parentDepartId;
                    }
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Restart();
                    _cDepartmentDal.Update(output.departs.update);
                    LogHelper.WriteDebug(
                        $"[更新部门信息]总条数：{output.departs.update.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");
                }
                if (output.departs?.delete?.Count > 0 && AntSdkListContactsEntity.departs?.Count > 0)
                {
                    foreach (
                        var setdept in
                            output.departs.delete.Select(
                                dept =>
                                    AntSdkListContactsEntity.departs.FirstOrDefault(
                                        d => d.departmentId == dept.departmentId)).Where(setdept => setdept != null))
                    {
                        AntSdkListContactsEntity.departs.Remove(setdept);
                    }
                    //TODO:本地化入库
                    //DONE:本地化入库
                    stopWatch.Restart();
                    _cDepartmentDal.Delete(output.departs.delete);
                    LogHelper.WriteDebug(
                        $"[删除部门信息]总条数：{output.departs.delete.Count}，耗时：{stopWatch.Elapsed.TotalMilliseconds}");

                    #region 删除该部门下对应的子部门 暂时只支持两级 

                    foreach (var deleteDepart in output.departs.delete)
                    {
                        _cDepartmentDal.DeleteByParentDepartId(deleteDepart.departmentId);
                    }
                    #endregion
                }
                //版本号入库
                _cVersion.Update(output.dataVersion);
                AntSdkListContactsEntity.dataVersion = output.dataVersion;
                //返回
                return output;
            }
            //组织提示，并返回
            errorMsg = FormatErrorMsg(Resources.AntSdkGetAddContactsError, tempMsg);
            return null;
        }

        #endregion

        #region   //触角SDK 调用平台SDK订阅|取消订阅主题

        /// <summary>
        /// 方法说明：订阅主题
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="topics">要订阅的主题</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否订阅成功</returns>
        public static bool Subscribe(string[] topics, ref string errorMsg)
        {
            return SdkService.Subscribe(topics, ref errorMsg);
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
            return SdkService.UnSubscribe(topics, ref errorMsg);
        }

        #endregion

        #region   //触角SDK 调用平台SDK接收MQTT消息固定分类事件

        /// <summary>
        /// 属性说明：SDK自定义Http处理Token错误事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Http DealWith Error Token Self")]
        public static event EventHandler TokenErrorEvent
        {
            remove
            {
                SdkService.TokenErrorEvent -= value;
                _antsdkhttpService.AntSdkTokenErrorEvent -= value;
            }
            add
            {
                SdkService.TokenErrorEvent += value;
                _antsdkhttpService.AntSdkTokenErrorEvent += value;
            }
        }



        /// <summary>
        /// 聊天的消息接收
        /// </summary>
        /// <summary>
        /// 属性说明：SDK自定义Mqtt聊天室消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Receive Chat Message Self")]
        public static event AntSdkPublicationReceivedHandler MsChatsReceived
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageChat;
                    var antsdkchatMsg = AntSdkChatMsg.GetAntSdkReceivedChat(sdkchatMsg);
                    if (antsdkchatMsg != null)
                        value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsChatsReceived -= sdkreceiveHandler;
            }
            add
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageChat;
                    var antsdkchatMsg = AntSdkChatMsg.GetAntSdkReceivedChat(sdkchatMsg);
                    if (antsdkchatMsg != null)
                        value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsChatsReceived += sdkreceiveHandler;
            }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt聊天室消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Receive Room Message Self")]
        public static event AntSdkPublicationReceivedHandler MsRoomsReceived
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageRoomBase;
                    var antsdkchatMsg = AntSdkReceivedRoomMsg.GetReceiveAntSdkRoomInfo(sdkchatMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsRoomsReceived -= sdkreceiveHandler;
            }
            add
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageRoomBase;
                    var antsdkchatMsg = AntSdkReceivedRoomMsg.GetReceiveAntSdkRoomInfo(sdkchatMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsRoomsReceived += sdkreceiveHandler;
            }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt群组型消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Receive Group Message Self")]
        public static event AntSdkPublicationReceivedHandler MsGroupReceived
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageGroupBase;
                    var antsdkchatMsg = AntSdkReceivedGroupMsg.GetReceiveAntSdkGroupInfo(sdkchatMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsGroupReceived -= sdkreceiveHandler;
            }
            add
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkchatMsg = e as MsSdkMessageGroupBase;
                    var antsdkchatMsg = AntSdkReceivedGroupMsg.GetReceiveAntSdkGroupInfo(sdkchatMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
                });
                SdkService.MsGroupReceived += sdkreceiveHandler;
            }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt个人的消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Receive User Message Self")]
        public static event AntSdkPublicationReceivedHandler MsUsersReceived
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkuserMsg = e as MsSdkUserBase;
                    var antsdkuserMsg = AntSdkReceivedUserMsg.GetAntSdkReceiveUserMsg(sdkuserMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkuserMsg);
                });
                SdkService.MsUsersReceived -= sdkreceiveHandler;
            }
            add
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkuserMsg = e as MsSdkUserBase;
                    var antsdkuserMsg = AntSdkReceivedUserMsg.GetAntSdkReceiveUserMsg(sdkuserMsg);
                    value?.Invoke(antsdkreceivemsgType, antsdkuserMsg);
                });
                SdkService.MsUsersReceived += sdkreceiveHandler;
            }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt其他的消息接收事件属性
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Receive Other Message Self")]
        public static event AntSdkPublicationReceivedHandler MsOtherReceived
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    var antsdkOther = new object();
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkother = e as SdkMsBase;
                    if (sdkother != null)
                    {
                        var setother = new AntSdkOffLineSimpleMsg();
                        antsdkOther = GetAntSdkEntity(sdkother, ref setother);
                    }
                    value?.Invoke(antsdkreceivemsgType, antsdkOther);
                });
                SdkService.MsOtherReceived -= sdkreceiveHandler;
            }
            add
            {
                SdkService.MsOtherReceived += (s, e) =>
                {
                    var antsdkreceivemsgtypeValue = (long)s;
                    var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                    var antsdkOther = new object();
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkother = e as SdkMsBase;
                    if (sdkother != null)
                    {
                        var setother = new AntSdkOffLineSimpleMsg();
                        antsdkOther = GetAntSdkEntity(sdkother, ref setother);
                    }
                    value?.Invoke(antsdkreceivemsgType, antsdkOther);
                };
            }
        }

        /// <summary>
        /// 触角SDK 离线消息接收[正常聊天消息]
        /// </summary>
        public static event AntSdkPublicationReceivedHandler MsOfflineReceivedChatMsg
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkMsg = e as List<SdkMsBase>;
                    var offlineothermsgDic = new Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>();
                    var offlinechatmsgList = new List<AntSdkChatMsg.ChatBase>();
                    if (sdkMsg != null)
                    {
                        GetAntSdkOfflineMsgList(sdkMsg, ref offlineothermsgDic, ref offlinechatmsgList);
                    }
                    if (!(offlinechatmsgList?.Count > 0)) return;
                    object antsdk = offlinechatmsgList;
                    value?.Invoke(AntSdkMsgType.OffLineMessageChatMsg, antsdk);
                });
                SdkService.MsOfflineReceived -= sdkreceiveHandler;
            }
            add
            {
                SdkService.MsOfflineReceived += (s, e) =>
                {
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkMsg = e as List<SdkMsBase>;
                    var offlineothermsgDic = new Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>();
                    var offlinechatmsgList = new List<AntSdkChatMsg.ChatBase>();
                    if (sdkMsg != null)
                    {
                        GetAntSdkOfflineMsgList(sdkMsg, ref offlineothermsgDic, ref offlinechatmsgList);
                    }
                    if (!(offlinechatmsgList?.Count > 0)) return;
                    object antsdk = offlinechatmsgList;
                    value?.Invoke(AntSdkMsgType.OffLineMessageChatMsg, antsdk);
                };
            }
        }

        /// <summary>
        /// 触角SDK 离线消息接收[正常聊天消息之外的消息]
        /// </summary>
        public static event AntSdkPublicationReceivedHandler MsOfflineReceivedOtherMsg
        {
            remove
            {
                var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
                {
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkMsg = e as List<SdkMsBase>;
                    var offlineothermsgDic = new Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>();
                    var offlinechatmsgList = new List<AntSdkChatMsg.ChatBase>();
                    if (sdkMsg != null)
                    {
                        GetAntSdkOfflineMsgList(sdkMsg, ref offlineothermsgDic, ref offlinechatmsgList);
                    }
                    if (!(offlineothermsgDic?.Count > 0)) return;
                    object antsdk = offlineothermsgDic;
                    value?.Invoke(AntSdkMsgType.OffLineMessageOtherMsg, antsdk);
                });
                SdkService.MsOfflineReceived -= sdkreceiveHandler;
            }
            add
            {
                SdkService.MsOfflineReceived += (s, e) =>
                {
                    //进行数据类型转换（SDK->AntSdk）
                    var sdkMsg = e as List<SdkMsBase>;
                    var offlineothermsgDic = new Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>();
                    var offlinechatmsgList = new List<AntSdkChatMsg.ChatBase>();
                    if (sdkMsg != null)
                    {
                        GetAntSdkOfflineMsgList(sdkMsg, ref offlineothermsgDic, ref offlinechatmsgList);
                    }
                    if (!(offlineothermsgDic?.Count > 0)) return;
                    object antsdk = offlineothermsgDic;
                    value?.Invoke(AntSdkMsgType.OffLineMessageOtherMsg, antsdk);
                };
            }
        }

        /// <summary>
        /// 方法说明：根据平台SDK接收到的离线消息，转化为触角SDK离线消息进行接收
        /// </summary>
        /// <param name="sdkmsgList">平台SDK接收到的离线消息集合</param>
        /// <param name="offlineothermsgDic">离线其他[正常聊天消息之外的消息]</param>
        /// <param name="offlinechatmsgList">离线聊天[正常聊天消息]</param>
        /// <returns></returns>
        private static void GetAntSdkOfflineMsgList(IEnumerable<SdkMsBase> sdkmsgList,
            ref Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase> offlineothermsgDic,
            ref List<AntSdkChatMsg.ChatBase> offlinechatmsgList)
        {
            if (offlineothermsgDic == null)
            {
                offlineothermsgDic = new Dictionary<AntSdkOffLineSimpleMsg, AntSdkMsBase>();
            }
            if (offlinechatmsgList == null)
            {
                offlinechatmsgList = new List<AntSdkChatMsg.ChatBase>();
            }
            foreach (var sdkmsg in sdkmsgList)
            {
                var antsdksim = new AntSdkOffLineSimpleMsg();
                var antsdkmsg = GetAntSdkEntity(sdkmsg, ref antsdksim);
                if (antsdkmsg == null)
                {
                    continue;
                }
                var antsdkchatmsg = antsdkmsg as AntSdkChatMsg.ChatBase;
                if (antsdkchatmsg != null)
                {
                    offlinechatmsgList.Add(antsdkchatmsg);
                }
                else
                {
                    offlineothermsgDic.Add(antsdksim, antsdkmsg);
                }
            }
        }

        /// <summary>
        /// 方法说明：获取触角SDK转化的平台SDK消息
        /// </summary>
        /// <param name="sdkmsgEntity">平台SDK实体</param>
        /// <param name="simpleEntity">离线消息简单类型（用来进行已读回执处理）</param>
        /// <returns></returns>
        private static AntSdkMsBase GetAntSdkEntity(SdkMsBase sdkmsgEntity, ref AntSdkOffLineSimpleMsg simpleEntity)
        {
            var sdkchatmsg = sdkmsgEntity as MsSdkMessageChat;
            if (sdkchatmsg != null)
            {
                var antsdkchatMsg = AntSdkChatMsg.GetAntSdkReceivedChat(sdkchatmsg);
                if (antsdkchatMsg != null)
                {
                    simpleEntity = new AntSdkOffLineSimpleMsg
                    {
                        MsgType = antsdkchatMsg.MsgType,
                        sessionId = antsdkchatMsg.sessionId,
                        chatIndex = antsdkchatMsg.chatIndex
                    };
                }
                //返回
                return antsdkchatMsg;
            }
            var sdkroommsg = sdkmsgEntity as MsSdkMessageRoomBase;
            if (sdkroommsg != null)
            {
                var antsdkroomMsg = AntSdkReceivedRoomMsg.GetReceiveAntSdkRoomInfo(sdkroommsg);
                if (antsdkroomMsg != null)
                {
                    simpleEntity = new AntSdkOffLineSimpleMsg
                    {
                        MsgType = antsdkroomMsg.MsgType,
                        sessionId = antsdkroomMsg.sessionId,
                        chatIndex = antsdkroomMsg.chatIndex
                    };
                }
                //返回
                return antsdkroomMsg;
            }
            var sdkgroupmsg = sdkmsgEntity as MsSdkMessageGroupBase;
            if (sdkgroupmsg != null)
            {
                var antsdkgroupMsg = AntSdkReceivedGroupMsg.GetReceiveAntSdkGroupInfo(sdkgroupmsg);
                if (antsdkgroupMsg != null)
                {
                    simpleEntity = new AntSdkOffLineSimpleMsg
                    {
                        MsgType = antsdkgroupMsg.MsgType,
                        sessionId = antsdkgroupMsg.sessionId,
                        chatIndex = antsdkgroupMsg.chatIndex
                    };
                }
                //返回
                return antsdkgroupMsg;
            }
            var sdkusermsg = sdkmsgEntity as MsSdkUserBase;
            if (sdkusermsg != null)
            {
                var antsdkuserMsg = AntSdkReceivedUserMsg.GetAntSdkReceiveUserMsg(sdkusermsg);
                if (antsdkuserMsg != null)
                {
                    simpleEntity = new AntSdkOffLineSimpleMsg
                    {
                        MsgType = antsdkuserMsg.MsgType
                    };
                }
                //返回
                return antsdkuserMsg;
            }
            var sdkreceipt = sdkmsgEntity as MsReceiveMsgReceipt;
            if (sdkreceipt != null)
            {
                var antsdkreceipt = AntSdkReceivedOtherMsg.MsgReceipt.GetReceiveAntSdkMsgReceiptInfo(sdkreceipt);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkreceipt.MsgType,
                    sessionId = antsdkreceipt.sessionId,
                    chatIndex = antsdkreceipt.chatIndex
                };
                return antsdkreceipt;
            }
            var sdksynchs = sdkmsgEntity as MsMultiTerminalSynch;
            if (sdksynchs != null)
            {
                var antsdksynchs =
                    AntSdkReceivedOtherMsg.MultiTerminalSynch.GetReceiveAntSdkMterminalSynchMsgInfo(sdksynchs);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdksynchs.MsgType,
                    sessionId = antsdksynchs.sessionId,
                    chatIndex = antsdksynchs.chatIndex
                };
                return antsdksynchs;
            }
            var sdkcustom = sdkmsgEntity as MsSdkCustomEntity;
            if (sdkcustom != null)
            {
                var antsdkcustom = AntSdkReceivedOtherMsg.Custom.GetReceiveAntSdkCustomMsgInfo(sdkcustom);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkcustom.MsgType,
                    sessionId = antsdkcustom.sessionId,
                    chatIndex = antsdkcustom.chatIndex
                };
                return antsdkcustom;
            }
            var sdkptbnrd = sdkmsgEntity as MsPointBurnReaded;
            if (sdkptbnrd != null)
            {
                var antsdkptbnrd =
                    AntSdkReceivedOtherMsg.PointBurnReaded.GetReceiveAntSdkMterminalSynchMsgInfo(sdkptbnrd);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkptbnrd.MsgType,
                    sessionId = antsdkptbnrd.sessionId,
                    chatIndex = antsdkptbnrd.chatIndex
                };
                return antsdkptbnrd;
            }
            var sdkptfiacpt = sdkmsgEntity as MsPointFileAccepted;
            if (sdkptfiacpt != null)
            {
                var antsdkptfiacpt =
                    AntSdkReceivedOtherMsg.PointFileAccepted.GetReceiveAntSdkMterminalSynchMsgInfo(sdkptfiacpt);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkptfiacpt.MsgType,
                    sessionId = antsdkptfiacpt.sessionId,
                    chatIndex = antsdkptfiacpt.chatIndex
                };
                return antsdkptfiacpt;
            }
            var sdkhardupdate = sdkmsgEntity as MsVersionHardUpdate;
            if (sdkhardupdate != null)
            {
                var antsdkhardupdt = AntSdkReceivedOtherMsg.VersionHardUpdate.GetAntSdkVersionUpdate(sdkhardupdate);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkhardupdt.MsgType
                };
                return antsdkhardupdt;
            }
            var sdkorginamodify = sdkmsgEntity as MsOrganizationModify;
            if (sdkorginamodify != null)
            {
                var antsdkorginamodify = AntSdkReceivedOtherMsg.OrganizationModify.GetOrganizationModify(sdkorginamodify);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkorginamodify.MsgType
                };
                return antsdkorginamodify;
            }
            var sdkunrnotification = sdkmsgEntity as MsUnReadNotifications;
            if (sdkunrnotification != null)
            {
                var antsdkunrnotification =
                    AntSdkReceivedOtherMsg.Notifications.GetReceiveAntSdkNotificationInfo(sdkunrnotification);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkunrnotification.MsgType,
                    sessionId = antsdkunrnotification.sessionId,
                    chatIndex = antsdkunrnotification.chatIndex
                };
                return antsdkunrnotification;
            }
            var sdkaddnotification = sdkmsgEntity as MsAddNotification;
            if (sdkaddnotification != null)
            {
                var antsdkaddnotification =
                    AntSdkReceivedOtherMsg.Notifications.GetReceiveAntSdkNotificationInfo(sdkaddnotification);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkaddnotification.MsgType,
                    sessionId = antsdkaddnotification.sessionId,
                    chatIndex = antsdkaddnotification.chatIndex
                };
                return antsdkaddnotification;
            }
            var sdknotificationstate = sdkmsgEntity as MsModifyNotificationState;
            if (sdknotificationstate != null)
            {
                var antsdknotificationstate =
                    AntSdkReceivedOtherMsg.Notifications.GetReceiveAntSdkNotificationInfo(sdknotificationstate);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdknotificationstate.MsgType,
                    sessionId = antsdknotificationstate.sessionId,
                    chatIndex = antsdknotificationstate.chatIndex
                };
                return antsdknotificationstate;
            }
            var sdknotificationdelete = sdkmsgEntity as MsDeleteNotification;
            if (sdknotificationdelete != null)
            {
                var antsdknotificationdelete =
                    AntSdkReceivedOtherMsg.Notifications.GetReceiveAntSdkNotificationInfo(sdknotificationdelete);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdknotificationdelete.MsgType,
                    sessionId = antsdknotificationdelete.sessionId,
                    chatIndex = antsdknotificationdelete.chatIndex
                };
                return antsdknotificationdelete;
            }
            var sdkattendancerecord = sdkmsgEntity as MsAttendanceRecordVerify;
            if (sdkattendancerecord != null)
            {
                var antsdkattendancerecord =
                    AntSdkReceivedOtherMsg.AttendanceRecordVerify.GetAntSdkAttendanceRecordVerify(sdkattendancerecord);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkattendancerecord.MsgType,
                    sessionId = antsdkattendancerecord.sessionId,
                    chatIndex = antsdkattendancerecord.chatIndex
                };
                return antsdkattendancerecord;
            }
            var sdkinvicationset = sdkmsgEntity as MsIndividuationSet;
            if (sdkinvicationset != null)
            {
                var antsdkinvicationset =
                    AntSdkReceivedOtherMsg.Individuation.GetAntSdkIndividuation(sdkinvicationset);
                simpleEntity = new AntSdkOffLineSimpleMsg
                {
                    MsgType = antsdkinvicationset.MsgType,
                    sessionId = antsdkinvicationset.sessionId,
                    chatIndex = antsdkinvicationset.chatIndex
                };
                //个性化设置主题处理：接收消息-订阅主题；屏蔽-取消订阅主题
                if (antsdkinvicationset.content?.chatType == (int)AntSdkchatType.Group &&
                    !string.IsNullOrEmpty(antsdkinvicationset.content?.targetId) &&
                    !string.IsNullOrEmpty(antsdkinvicationset.content?.state))
                {
                    //接收消息需要订阅讨论组ID主题
                    if (antsdkinvicationset.content.state == ((int)AntSdkIndividState.Accept).ToString())
                    {
                        //接收消息订阅讨论组主题
                        var topics = new List<string> { antsdkinvicationset.content.targetId };
                        //取消订阅主题
                        var temperrorMsg = string.Empty;
                        if (!SdkService.Subscribe(topics.ToArray(), ref temperrorMsg))
                        {
                            //记录收到删除讨论组通知后取消订阅讨论组主题失败日志
                            LogHelper.WriteError(
                                $"Received Personal Set Accept Message Subscribe Group Topic,{Resources.AntSdkSubscribePersonalAcceptTopicsError}：{temperrorMsg}");
                        }
                    }
                    else if (antsdkinvicationset.content.state == ((int)AntSdkIndividState.Block).ToString())
                    {
                        //接收消息取消订阅讨论组主题
                        var topics = new List<string> { antsdkinvicationset.content.targetId };
                        //取消订阅主题
                        var temperrorMsg = string.Empty;
                        if (!SdkService.UnSubscribe(topics.ToArray(), ref temperrorMsg))
                        {
                            //记录收到删除讨论组通知后取消订阅讨论组主题失败日志
                            LogHelper.WriteError(
                                $"Received Personal Set Block Message UnSubscribe Group Topic,{Resources.AntSdkSubscribePersonalBlockTopicsError}：{temperrorMsg}");
                        }
                    }
                }
                else if (antsdkinvicationset.content?.chatType == (int)AntSdkchatType.Point &&
                         !string.IsNullOrEmpty(antsdkinvicationset.content?.targetId) &&
                         !string.IsNullOrEmpty(antsdkinvicationset.content?.state))
                {
                    //接收消息增删黑名单
                    if (antsdkinvicationset.content.state == ((int)AntSdkIndividState.Accept).ToString())
                    {
                        var temperrorCode = 0;
                        var temperrorMsg = string.Empty;
                        //从黑名单中移除
                        if (!SdkService.DelBlacklist(AntSdkLoginOutput.userId, antsdkinvicationset.content.targetId,
                            ref temperrorCode, ref temperrorMsg))
                        {

                            //记录收到从黑名单中删除目标失败日志
                            LogHelper.WriteError(
                                $"Received Personal Set Accept Message DelBlack,{Resources.AntSdkSubscribePersonalAcceptTopicsError}：{temperrorMsg}");
                        }
                    }
                    else if (antsdkinvicationset.content.state == ((int)AntSdkIndividState.Block).ToString())
                    {
                        var temperrorCode = 0;
                        var temperrorMsg = string.Empty;
                        //增加到黑名单中
                        if (!SdkService.AddBlacklist(AntSdkLoginOutput.userId, antsdkinvicationset.content.targetId,
                            ref temperrorCode, ref temperrorMsg))
                        {
                            //记录收到从添加目标到黑名单失败日志
                            LogHelper.WriteError(
                                $"Received Personal Set Accept Message AddBlack,{Resources.AntSdkSubscribePersonalAcceptTopicsError}：{temperrorMsg}");
                        }
                    }
                }
                //返回
                return antsdkinvicationset;
            }
            //返回空
            return null;
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt断开重连事件
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Disconnect Reconnect Self")]
        public static event EventHandler ReconnectedMqtt
        {
            remove { SdkService.ReconnectedMqtt -= value; }
            add { SdkService.ReconnectedMqtt += value; }
        }

        /// <summary>
        /// 属性说明：SDK自定义Mqtt断开事件
        /// 完成日期：2017-04-20
        /// </summary>
        [Category("SDK.MessageService.SdkService.Instance"), Description("Mqtt Disconnect Reconnect Self")]
        public static event EventHandler DisconnectedMqtt
        {
            remove { SdkService.DisconnectedMqtt -= value; }
            add { SdkService.DisconnectedMqtt += value; }
        }

        #endregion

        #region   //触角SDK 调用平台SDK回调注册方式接收消息

        /// <summary>
        /// 方法说明：回调方式注册接收消息函数
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="antsdkmsgType">接收消息枚举标记类型：
        /// Example:只需要接收 文本、视频和自定义消息则该参数的传递方式为：
        /// SdkEnumCollection.SdkReceiveMsgType MsgType = dkEnumCollection.SdkReceiveMsgType.ChatMsgText  | 
        ///                                               dkEnumCollection.SdkReceiveMsgType.ChatMsgVideo |
        ///                                               dkEnumCollection.SdkReceiveMsgType.CustomMessage
        /// </param>
        /// <param name="antsdkreceiveHandler">接收消息回调的注册方法</param>
        /// <param name="errorMsg">错误提示</param>
        public static void SdkCallBackReceiveMsg(AntSdkMsgType antsdkmsgType,
            AntSdkPublicationReceivedHandler antsdkreceiveHandler, ref string errorMsg)
        {

            var sdkmsgtypeValue = (long)antsdkmsgType;
            var sdkmsgType = (SdkMsgType)sdkmsgtypeValue;
            var sdkreceiveHandler = new SdkPublicationReceivedHandler((s, e) =>
            {
                var antsdkreceivemsgtypeValue = (long)s;
                var antsdkreceivemsgType = (AntSdkMsgType)antsdkreceivemsgtypeValue;
                //进行数据类型转换（SDK->AntSdk）
                var sdkchatMsg = e as MsSdkMessageChat;
                var antsdkchatMsg = AntSdkChatMsg.GetAntSdkReceivedChat(sdkchatMsg);
                antsdkreceiveHandler?.Invoke(antsdkreceivemsgType, antsdkchatMsg);
            });
            //转换设置
            SdkService.SdkCallBackReceiveMsg(sdkmsgType, sdkreceiveHandler, ref errorMsg);
        }

        #endregion

        #region   //触角SDK 调用平台SDK通过MQTT发送消息

        /// <summary>
        /// 方法说明：SDK发送终端消息接口：心跳消息、请求离线消息
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishTerminalMsg<T>(T entity, ref string errorMsg) where T : AntSdkSendMsg.Terminal.TerminalBase
        {
            var sdkSend = AntSdkSendMsg.Terminal.GetSdkSend(entity);
            return SdkService.SdkPublishTerminalMsg(sdkSend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishChatMsg<T>(T entity, ref string errorMsg) where T : AntSdkChatMsg.ChatBase
        {
            var sdksend = AntSdkChatMsg.GetSdkSend(entity);
            return SdkService.SdkPublishChatMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（重发）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkRePublishChatMsg<T>(T entity, ref string errorMsg) where T : AntSdkChatMsg.ChatBase
        {
            var sdksend = AntSdkChatMsg.GetSdkSend(entity);
            return SdkService.SdkRePublishChatMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（机器人）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkRobotPublishChatMsg<T>(T entity, ref string errorMsg) where T : AntSdkChatMsg.ChatBase
        {
            var sdksend = AntSdkChatMsg.GetSdkSend(entity);
            return SdkService.SdkRobotPublishChatMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送聊天消息接口：文本、图片、音频、视频、文件、地理位置、图文混合、@消息、多人音频视频（@机器人重发）
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkReRobotPublishChatMsg<T>(T entity, ref string errorMsg) where T : AntSdkChatMsg.ChatBase
        {
            var sdksend = AntSdkChatMsg.GetSdkSend(entity);
            return SdkService.SdkReRobotPublishChatMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：通过触角SDK发送自定义消息接口：[支持4000-9999的自定义消息，仅仅是传输][自定义content内容]
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="entity">发送消息实体</param>
        /// <param name="customTopic">自定义主题</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否成功发送消息</returns>
        public static bool SdkPublishCustomMsg(AntSdkReceivedOtherMsg.Custom entity, string customTopic,
            ref string errorMsg)
        {
            var sdkctom = entity.GetSdkCtom();
            int msgtype;
            if (!int.TryParse(entity.messageType, out msgtype))
            {
                errorMsg = $"{Resources.AntSdkSendCtomMsgTypeError}({entity.MsgType})";
                return false;
            }
            return SdkService.SdkPublishCustomMsg(sdkctom, msgtype, customTopic, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK普通消息 已读/收回执
        /// 完成时间：2017-05-16
        /// </summary>
        /// <param name="sendReceipt">已读/收回执实体</param>
        /// <param name="receiptType">已读/收类型</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送回执</returns>
        public static bool SdkPublishReceiptMsg(AntSdkReceiptMsg sendReceipt,
            AntSdkReceiptType receiptType, ref string errorMsg)
        {
            var sdksend = sendReceipt.GetSdkSend();
            var sdkType = SdkReceiptType.ReceiveReceipt;
            if (receiptType == AntSdkReceiptType.ReadReceipt)
            {
                sdkType = SdkReceiptType.ReadReceipt;
            }
            return SdkService.SdkPublishReceiptMsg(sdksend, sdkType,
                ref errorMsg);
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
        public static bool SdkPublishCustomReceiptMsg(AntSdkReceiptMsg sendReceipt,
            AntSdkReceiptType receiptType, string customTopic, ref string errorMsg)
        {
            var sdksend = sendReceipt.GetSdkSend();
            var sdkType = SdkReceiptType.ReceiveReceipt;
            if (receiptType == AntSdkReceiptType.ReadReceipt)
            {
                sdkType = SdkReceiptType.ReadReceipt;
            }
            return SdkService.SdkPublishCustomReceiptMsg(sdksend, sdkType, customTopic,
                ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送点对点阅后即焚消息已读回执
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="receiptedchatmsgEntity">点对点聊天收到的阅后即焚消息实体</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送点对点阅后即焚已读回执</returns>
        public static bool SdkPublishPointBurnReadReceiptMsg(AntSdkSendMsg.PointBurnReaded receiptedchatmsgEntity, ref string errorMsg)
        {
            var sdksend = receiptedchatmsgEntity.GetSdkSend();
            return SdkService.SdkPublishPointBurnReadReceiptMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送标准其他消息（结构与聊天消息一致的其他消息类型）
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="sendEntity"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool SdkPublishOtherMsg(AntSdkOtherMsg.OtherBase sendEntity, ref string errorMsg)
        {
            var sdksend = AntSdkOtherMsg.GetSdkSend(sendEntity);
            return SdkService.SdkPublishOtherMsg(sdksend, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：SDK发送群阅后即焚消息[群主改变阅后即焚状态]
        /// 完成时间：2015-05-16
        /// </summary>
        /// <param name="changeMode">要切换的阅后即焚状态</param>
        /// <param name="sendmodeEntity">发送实体信息</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功发送群阅后即焚消息</returns>
        public static bool SdkPublishGpOwnerChangeMode(AntSdkGroupChangeMode changeMode,
            AntSdkChatMsg.ChatBase sendmodeEntity, ref string errorMsg)
        {
            var sdkmode = GroupChangeMode.NormalMode;
            if (changeMode == AntSdkGroupChangeMode.BurnMode)
            {
                sdkmode = GroupChangeMode.BurnMode;
            }
            if (changeMode == AntSdkGroupChangeMode.BurnModeDelete)
            {
                sdkmode = GroupChangeMode.BurnModeDelete;
            }
            var sdkmodeEntity = AntSdkChatMsg.GetSdkSend(sendmodeEntity);
            return SdkService.SdkPublishGpOwnerChangeMode(sdkmode, sdkmodeEntity, ref errorMsg);
        }

        #endregion

        #region   //触角SDK 调用平台SDK按照规则生成点对点会话SessionID

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
            return SdkService.GetPointToPointSessionId(sendUserId, targetId, ref errorMsg);
        }

        #endregion

        #region   //触角SDK 调用平台SDK提供的HTTP请求方法

        /// <summary>
        /// 方法说明：更新用户信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新用户信息输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新用户信息成功</returns>
        public static bool UpdateUserInfo(AntSdkUpdateUserInfoInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.UpdateUserInfo(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：添加用户黑名单
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="targetId">屏蔽的用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功添加用户黑名单</returns>
        public static bool AddBlacklist(string userId, string targetId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.AddBlacklist(userId, targetId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：查询黑名单ID列表
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>黑名单数组</returns>
        public static string[] FindBlacklists(string userId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.FindBlacklists(userId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：移除用户黑名单
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="targetId">屏蔽的用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功移除用户黑名单</returns>
        public static bool DelBlacklist(string userId, string targetId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.DelBlacklist(userId, targetId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：创建聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">创建聊天室输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>创建成功的聊天室ID</returns>
        public static string CreateChatRoom(AntSdkCreateChatRoomInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.CreateChatRoom(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：删除聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="operateId">操作者ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功删除聊天室</returns>
        public static bool DeleteChatRoom(string roomId, string operateId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.DeleteChatRoom(roomId, operateId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：添加聊天室成员
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">添加聊天室成员输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否添加成功</returns>
        public static bool AddChatRoomMembers(AntSdkAddChatRoomMembersInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.AddChatRoomMembers(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：删除聊天室成员
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">删除聊天室输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteChatRoomMembers(AntSdkDeleteChatRoomMembersInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.DeleteChatRoomMembers(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：更新聊天室员属性
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新聊天室成员属性输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateChatRoomMembers(AntSdkUpdateChatRoomMemberInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.UpdateChatRoomMembers(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：修改聊天室信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更改聊天室输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateChatRoom(AntSdkUpdateChatRoomInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.UpdateChatRoom(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：成员退出聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功退出聊天室</returns>
        public static bool ExitChatRoom(string roomId, string userId, string userName, ref int errorCode, ref string errorMsg)
        {
            return SdkService.ExitChatRoom(roomId, userId, userName, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：查询用户所在聊天室，返回聊天室列表
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室数组</returns>
        public static AntSdkFindRoomsOutput[] FindRooms(string userId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.FindRooms(userId, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0) { return null; }
            var antsdkoutput = new List<AntSdkFindRoomsOutput>();
            antsdkoutput.AddRange((new List<FindRoomsOutput>(sdkoutput)).Select(c => new AntSdkFindRoomsOutput
            {
                roomId = c?.roomId,
                roomName = c?.roomName
            }));
            return antsdkoutput.ToArray();
        }

        /// <summary>
        /// 方法说明：查询单个聊天室详细信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="ope">返回类型-1表示带上群成员列表，0表示不带群成员列表，只返回群信息(默认不带群成员列表)</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室详细信息</returns>
        public static AntSdkGetChatRoomInfoOutput GetChatRoomInfo(string roomId, string ope, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetChatRoomInfo(roomId, ope, ref errorCode, ref errorMsg);
            if (sdkoutput == null) { return null; }
            var antsdkMembers = new List<AntSdkMember>();
            if (sdkoutput.members?.Count > 0)
            {
                antsdkMembers.AddRange(sdkoutput.members.Select(c => new AntSdkMember
                {
                    userId = c?.userId,
                    userName = c?.userName
                }));
            }
            var antsdkopt = new AntSdkGetChatRoomInfoOutput
            {
                roomName = sdkoutput.roomName,
                desc = sdkoutput.desc,
                remark = sdkoutput.remark,
                attr1 = sdkoutput.attr1,
                attr2 = sdkoutput.attr2,
                attr3 = sdkoutput.attr3,
                createTime = sdkoutput.createTime,
                updateTime = sdkoutput.updateTime,
                createBy = sdkoutput.createBy,
                updateBy = sdkoutput.updateBy,
                robotFlag = sdkoutput.robotFlag,
                robotType = sdkoutput.robotType,
                members = antsdkMembers
            };
            //返回
            return antsdkopt;
        }

        /// <summary>
        /// 方法说明：查询聊天室所有成员基本信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室成员基本信息数组</returns>
        public static AntSdkFindRoomMembersOutput[] FindRoomMembers(string roomId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.FindRoomMembers(roomId, ref errorCode, ref errorMsg);
            var antsdkopt = new List<AntSdkFindRoomMembersOutput>();
            if (sdkoutput == null || sdkoutput.Length == 0)
            {
                return null;
            }
            antsdkopt.AddRange(sdkoutput.Select(c => new AntSdkFindRoomMembersOutput
            {
                userId = c?.userId,
                userName = c?.userName
            }));
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：查询聊天室单个成员详细信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室单个成员信息</returns>
        public static AntSdkGetRoomMemberInfoOutput GetRoomMemberInfo(string userId, string roomId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetRoomMemberInfo(userId, roomId, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
            {
                return null;
            }
            var antsdkopt = new AntSdkGetRoomMemberInfoOutput
            {
                userName = sdkoutput.userName,
                desc = sdkoutput.desc,
                remark = sdkoutput.remark,
                attr1 = sdkoutput.attr1,
                attr2 = sdkoutput.attr2,
                attr3 = sdkoutput.attr3,
                createTime = sdkoutput.createTime,
                updateTime = sdkoutput.updateTime
            };
            return antsdkopt;
        }

        /// <summary>
        /// 方法说明：查询app下的所有聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>所有app下聊天室数组</returns>
        public static AntSdkFindRoomsOutput[] FindAllRooms(ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.FindAllRooms(ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0)
            {
                return null;
            }
            var antsdkoutput = new List<AntSdkFindRoomsOutput>();
            antsdkoutput.AddRange((new List<FindRoomsOutput>(sdkoutput)).Select(c => new AntSdkFindRoomsOutput
            {
                roomId = c?.roomId,
                roomName = c?.roomName
            }));
            return antsdkoutput.ToArray();
        }

        /// <summary>
        /// 方法说明：邀请加入聊天室
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">邀请加入聊天室输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否邀请成功</returns>
        public static bool InviteJoinRoom(AntSdkInviteJoinRoomInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.InviteJoinRoom(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：处理邀请
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">处理邀请加入聊天室输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否处理邀请成功</returns>
        public static bool HandleInvite(AntSdkHandleInviteInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.HandleInvite(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：查询用户所在聊天室，返回聊天室列表以及用户个性化设置
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>聊天室列表数组</returns>
        public static AntSdkFindIndividRoomsOutput[] FindIndividRooms(string userId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.FindIndividRooms(userId, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0) { return null; }
            var antsdkopt = new List<AntSdkFindIndividRoomsOutput>();
            antsdkopt.AddRange(sdkoutput.Select(c => new AntSdkFindIndividRoomsOutput
            {
                roomId = c?.roomId,
                roomName = c?.roomName,
                receiveType = c?.receiveType ?? 0
            }));
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：设置聊天室的接收消息类型
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roomId">聊天室ID</param>
        /// <param name="receiveType">接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息）</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否设置成功</returns>
        public static bool SetRoomReceiveType(string userId, string roomId, int receiveType, ref int errorCode, ref string errorMsg)
        {
            return SdkService.SetRoomReceiveType(userId, roomId, receiveType, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：设置用户的接收消息类型
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="receiveType">接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息）</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否设置成功</returns>
        public static bool SetUserReceiveType(string userId, int receiveType, ref int errorCode, ref string errorMsg)
        {
            return SdkService.SetUserReceiveType(userId, receiveType, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：查询离线消息(客户端上线调用http接口)
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="oSystem">1–pc 2–web 3–android 4–ios</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功</returns>
        public static object QueryOfflineMsg(string userId, string oSystem, ref int errorCode, ref string errorMsg)
        {
            return SdkService.QueryOfflineMsg(userId, oSystem, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：手机端切换运行状态
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功切换</returns>
        public static bool ChangeAppRunStatus(string version, string userId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.ChangeAppRunStatus(version, userId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：获取用户的讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>用户所在讨论组数组</returns>
        public static AntSdkGroupInfo[] GetGroupList(string userId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetGroupList(userId, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0) { return null; }
            var antsdkopt = new List<AntSdkGroupInfo>();
            antsdkopt.AddRange(sdkoutput.Select(c => new AntSdkGroupInfo
            {
                groupId = c?.groupId,
                groupName = c?.groupName,
                groupPicture = c?.groupPicture,
                memberCount = c?.memberCount,
                groupOwnerId = c?.groupOwnerId,
                managerIds = c?.managerIds,
                state = c?.state ?? 0
            }));
            //订阅讨论组主题
            if (antsdkopt.Count > 0)
            {
                //连接成功需要订阅的默认主题
                var topics = new List<string>();
                topics.AddRange(antsdkopt.Select(c => $"{c.groupId}"));
                //订阅默认主题
                var temperrorMsg = string.Empty;
                if (!SdkService.Subscribe(topics.ToArray(), ref temperrorMsg))
                {
                    errorMsg = FormatErrorMsg(Resources.AntSdkSubscribeGroupTopicsError, temperrorMsg);
                    return null;
                }
            }
            //返回用户讨论组
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：获取讨论组成员信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>讨论组成员数组</returns>
        public static AntSdkGroupMember[] GetGroupMembers(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetGroupMembers(userId, groupId, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0)
            {
                return null;
            }
            var antsdkopt = new List<AntSdkGroupMember>();
            antsdkopt.AddRange(sdkoutput.Where(c => c != null).Select(c => new AntSdkGroupMember
            {
                userId = c.userId,
                userName = c.userName,
                userNum = c.userNum,
                picture = c.picture,
                position = c.position,
                roleLevel = c.roleLevel

            }));
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：退出讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功退出</returns>
        public static bool GroupExitor(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            var result = SdkService.GroupExitor(userId, groupId, ref errorCode, ref errorMsg);
            if (!result)
            {
                return false;
            }
            //订阅默认主题
            var topics = new List<string>
            {
                groupId
            };
            var temperrorMsg = string.Empty;
            if (!SdkService.UnSubscribe(topics.ToArray(), ref temperrorMsg))
            {
                errorMsg = FormatErrorMsg(Resources.AntSdkSubscribeExitGroupTopicsError, temperrorMsg);
                return false;
            }
            //返回
            return true;
        }

        /// <summary>
        /// 方法说明：群主转让
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">群主转让输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功转让</returns>
        public static bool GroupOwnerChange(AntSdkGroupOwnerChangeInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.GroupOwnerChange(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：群管理员设置
        /// 完成时间：2017-08-17
        /// </summary>
        /// <param name="input">群管理员设置输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功设置</returns>
        public static bool GroupManagerSet(AntSdkGroupManagerChangeInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.GroupManagerSet(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：创建讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">创建讨论组输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>创建成功的讨论组信息</returns>
        public static AntSdkCreateGroupOutput CreateGroup(AntSdkCreateGroupInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            var sdkoutput = SdkService.CreateGroup(sdk, ref errorCode, ref errorMsg);
            if (sdkoutput == null) { return null; }
            //连接成功需要订阅的默认主题
            var topics = new List<string>
            {
                sdkoutput.groupId
            };
            //订阅默认主题
            var temperrorMsg = string.Empty;
            if (!SdkService.Subscribe(topics.ToArray(), ref temperrorMsg))
            {
                errorMsg = FormatErrorMsg(Resources.AntSdkSubscribeCreateGroupTopicsError, temperrorMsg);
                return null;
            }
            //构建触角讨论组输出信息
            var antsdkopt = new AntSdkCreateGroupOutput
            {
                groupId = sdkoutput.groupId,
                groupName = sdkoutput.groupName,
                groupPicture = sdkoutput.groupPicture,
                memberCount = sdkoutput.memberCount,
                groupOwnerId = sdkoutput.groupOwnerId
            };
            return antsdkopt;
        }

        /// <summary>
        /// 方法说明：更新讨论组信息
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新讨论组信息输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更新讨论组信息</returns>
        public static bool UpdateGroup(AntSdkUpdateGroupInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.UpdateGroup(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：更新用户在讨论组的设置
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">更新用户在讨论组的设置输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更新用户在讨论组的设置</returns>
        public static bool UpdateGroupConfig(AntSdkUpdateGroupConfigInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            return SdkService.UpdateGroupConfig(sdk, ref errorCode, ref errorMsg);
        }

        /// <summary> 
        /// 方法说明：解散讨论组
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">讨论组ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功解散讨论组</returns>
        public static bool DissolveGroup(string userId, string groupId, ref int errorCode, ref string errorMsg)
        {
            var result = SdkService.DissolveGroup(userId, groupId, ref errorCode, ref errorMsg);
            if (!result)
            {
                return false;
            }
            //订阅默认主题
            var topics = new List<string>
            {
                groupId
            };
            var temperrorMsg = string.Empty;
            if (!SdkService.UnSubscribe(topics.ToArray(), ref temperrorMsg))
            {
                errorMsg = FormatErrorMsg(Resources.AntSdkSubscribeDissolveGroupTopicsError, temperrorMsg);
                return false;
            }
            //返回
            return true;
        }

        /// <summary>
        /// 方法说明：获取群的所有公告
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">群组ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>群的所有公告实体数组</returns>
        public static AntSdkGetGroupNotificationsOutput[] GetGroupNotifications(string userId, string groupId,
            ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetGroupNotifications(userId, groupId, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0)
            {
                return null;
            }
            var antsdkopt = new List<AntSdkGetGroupNotificationsOutput>();
            antsdkopt.AddRange(sdkoutput.Where(c => c != null).Select(c => new AntSdkGetGroupNotificationsOutput
            {
                notificationId = c.notificationId,
                title = c.title,
                hasAttach = c.hasAttach,
                targetId = c.targetId,
                readState = c.readState,
                createTime = c.createTime,
                createBy = c.createBy,
            }));
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：通过ID获取群公告
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>公告信息</returns>
        public static AntSdkGetNotificationsByIdOutput GetNotificationsById(string notificationId, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.GetNotificationsById(notificationId, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
            {
                return null;
            }
            var antsdkopt = new AntSdkGetNotificationsByIdOutput
            {
                notificationId = sdkoutput.notificationId,
                title = sdkoutput.title,
                content = sdkoutput.content,
                attach = sdkoutput.attach,
                targetId = sdkoutput.targetId,
                readState = sdkoutput.readState,
                createTime = sdkoutput.createTime,
                updateTime = sdkoutput.updateTime,
                createBy = sdkoutput.createBy,
                updateBy = sdkoutput.updateBy,
            };
            return antsdkopt;
        }

        /// <summary>
        /// 方法说明：添加群公告（群主才有权限）
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">添加群组公告输入实体</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>添加成功的群组公告信息</returns>
        public static AntSdkAddNotificationsOutput AddNotifications(AntSdkAddNotificationsInput input,
            ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            var sdkoutput = SdkService.AddNotifications(sdk, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
            {
                return null;
            }
            var antsdkopt = new AntSdkAddNotificationsOutput
            {
                notificationId = sdkoutput.notificationId,
                title = sdkoutput.title,
                content = sdkoutput.content,
                attach = sdkoutput.attach,
                targetId = sdkoutput.targetId,
                readState = sdkoutput.readState,
                createTime = sdkoutput.createTime,
                updateTime = sdkoutput.updateTime,
                createBy = sdkoutput.createBy,
                updateBy = sdkoutput.updateBy
            };
            return antsdkopt;
        }
        #region 消息漫游
        private static List<SynchronusMsgOutput> DeleteChatMsgList = new List<SynchronusMsgOutput>();
        /// <summary>
        /// 查询离线消息(客户端上线调用http接口)
        /// </summary>
        public static object SynchronusMsgs(AntSdkSynchronusMsgInput antSdkInput, ref List<AntSdkChatMsg.ChatBase> antSdkOutput, ref int errorCode, ref string errorMsg)
        {
            if (!CheckSdkServiceState(ref errorMsg))
            {
                return false;
            }
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var output = new List<SynchronusMsgOutput>();
            if (antSdkInput != null)
            {
                var input = new SynchronusMsgInput
                {
                    userId = antSdkInput.userId,
                    sessionId = antSdkInput.sessionId,
                    chatIndex = antSdkInput.chatIndex,
                    count = antSdkInput.count,
                    flag = antSdkInput.flag,
                    first = antSdkInput.isFirst,
                    chatType = antSdkInput.chatType
                };
                if (antSdkInput.isFirst)
                {
                    input.chatIndex = 0;
                    input.count = antSdkInput.count;
                    var listChatdata = new List<AntSdkChatMsg.ChatBase>();
                    if (antSdkInput.chatType == (int)AntSdkchatType.Group)//群组
                    {
                        var tempChatGroupMsgs = GetServiceGroupSynchronusMsgs(input, ref output, null, ref errorCode, ref errMsg);
                        if (tempChatGroupMsgs != null && tempChatGroupMsgs.Count > 0)
                        {
                            var chatIndex = 0;
                            int.TryParse(tempChatGroupMsgs[0].chatIndex, out chatIndex);
                            if (chatIndex > 0)
                                chatIndex = chatIndex + 1;
                            antSdkInput.chatIndex = chatIndex;
                        }
                        var localChatGroupMsgs = new List<AntSdkChatMsg.ChatBase>();
                        GetLocalMsgData(antSdkInput, ref localChatGroupMsgs, false, true);
                        if (tempChatGroupMsgs == null || tempChatGroupMsgs.Count == 0)
                        {
                            listChatdata = localChatGroupMsgs;
                        }
                        else
                        {
                            if (localChatGroupMsgs.Count == 0)
                                listChatdata = tempChatGroupMsgs;
                            else
                            {
                                listChatdata = localChatGroupMsgs;
                                foreach (var synMsg in tempChatGroupMsgs)
                                {
                                    var tempMsg = listChatdata.FirstOrDefault(m => m.chatIndex == synMsg.chatIndex && m.messageId == synMsg.messageId);
                                    if (tempMsg == null)
                                        listChatdata.Add(synMsg);
                                }
                            }
                        }

                        if (listChatdata != null && listChatdata.Count > 0)
                        {
                            listChatdata = listChatdata.Where(m => !string.IsNullOrEmpty(m.sendUserId) && !string.IsNullOrEmpty(m.sourceContent)).ToList();
                            return antSdkOutput = listChatdata.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                        }
                        //else
                        //{
                        //    var result = GetLocalMsgData(antSdkInput, ref listChatdata);
                        //    if (listChatdata.Count > 0)
                        //        return antSdkOutput = listChatdata.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                        //}
                    }
                    else//单人
                    {
                        var tempChatMsg = GetServiceSynchronusMsgs(input, ref output, null, ref errorCode, ref errMsg);
                        if (tempChatMsg != null && tempChatMsg.Count > 0)
                        {
                            var chatIndex = 0;
                            int.TryParse(tempChatMsg[0].chatIndex, out chatIndex);
                            if (chatIndex > 0)
                                chatIndex = chatIndex + 1;
                            antSdkInput.chatIndex = chatIndex;
                        }
                        var localChatMsgs = new List<AntSdkChatMsg.ChatBase>();
                        GetLocalMsgData(antSdkInput, ref localChatMsgs, false, true);
                        if (tempChatMsg == null || tempChatMsg.Count == 0)
                        {
                            listChatdata = localChatMsgs;
                        }
                        else
                        {
                            if (localChatMsgs.Count == 0)
                                listChatdata = tempChatMsg;
                            else
                            {
                                listChatdata = localChatMsgs;
                                foreach (var synMsg in tempChatMsg)
                                {
                                    var tempMsg = listChatdata.FirstOrDefault(m => m.chatIndex == synMsg.chatIndex && m.messageId == synMsg.messageId);
                                    if (tempMsg == null)
                                        listChatdata.Add(synMsg);
                                }
                            }
                        }
                        if (listChatdata != null && listChatdata.Count > 0)
                        {
                            listChatdata = listChatdata.Where(m => !string.IsNullOrEmpty(m.sendUserId) && !string.IsNullOrEmpty(m.sourceContent)).ToList();
                            return antSdkOutput = listChatdata.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                        }
                        //else
                        //{

                        //    var result = GetLocalMsgData(antSdkInput, ref listChatdata);
                        //    if (listChatdata.Count > 0)
                        //        return antSdkOutput = listChatdata.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                        //}

                    }
                    return null;
                }
                else
                {
                    var listChatdata = new List<AntSdkChatMsg.ChatBase>();
                    var result = GetLocalMsgData(antSdkInput, ref listChatdata);
                    if (result)
                    {
                        if (antSdkInput.chatType == (int)AntSdkchatType.Group)//群组
                        {
                            input.chatIndex = antSdkInput.chatIndex;
                            var tempChatGroupMsg = GetServiceGroupSynchronusMsgs(input, ref output, listChatdata, ref errorCode, ref errMsg);
                            if (tempChatGroupMsg == null || tempChatGroupMsg.Count == 0)
                            {
                                tempChatGroupMsg = listChatdata;
                                //GetLocalMsgData(antSdkInput, ref tempChatGroupMsg, false);
                            }
                            if (tempChatGroupMsg != null && tempChatGroupMsg.Count > 0)
                            {
                                tempChatGroupMsg = tempChatGroupMsg.Where(m => !string.IsNullOrEmpty(m.sendUserId) && !string.IsNullOrEmpty(m.sourceContent)).ToList();
                                return antSdkOutput = tempChatGroupMsg.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                            }
                            return null;
                        }
                        else//单人
                        {
                            input.chatIndex = antSdkInput.chatIndex;
                            var tempChatMsg = GetServiceSynchronusMsgs(input, ref output, listChatdata, ref errorCode, ref errMsg);
                            if (tempChatMsg == null || tempChatMsg.Count == 0)
                            {
                                tempChatMsg = listChatdata;
                                //GetLocalMsgData(antSdkInput, ref tempChatMsg, false);
                            }
                            if (tempChatMsg != null && tempChatMsg.Count > 0)
                            {
                                tempChatMsg = tempChatMsg.Where(m => !string.IsNullOrEmpty(m.sendUserId) && !string.IsNullOrEmpty(m.sourceContent)).ToList();
                                return antSdkOutput = tempChatMsg.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                            }
                            return null;
                        }
                    }
                    else
                    {
                        listChatdata = listChatdata.Where(m => !string.IsNullOrEmpty(m.sendUserId) && !string.IsNullOrEmpty(m.sourceContent)).ToList();
                        return antSdkOutput = listChatdata.OrderBy(m => int.Parse(m.chatIndex)).ToList();
                    }
                }
            }
            //错误返回
            errorMsg = errMsg;
            return null;
        }
        static T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
        static T_Chat_Message_GroupDAL t_GroupChat = new T_Chat_Message_GroupDAL();
        static T_Chat_Message_GroupBurnDAL t_GroupBurnChat = new T_Chat_Message_GroupBurnDAL();
        /// <summary>
        /// 获取本地库消息数据
        /// </summary>
        /// <param name="antSdkInput"></param>
        /// <param name="msgOutput"></param>
        /// <returns>true:需要从服务器取数据 false:直接用本地库数据</returns>
        public static bool GetLocalMsgData(AntSdkSynchronusMsgInput antSdkInput, ref List<AntSdkChatMsg.ChatBase> msgOutput, bool isSynchronus = true, bool isFirst = false)
        {
            if (antSdkInput.chatType == (int)AntSdkchatType.Point)
            {
                //获取本地单人会话消息
                var listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat.getDataByScroll(antSdkInput.sessionId, AntSdkService.AntSdkCurrentUserInfo.companyCode,
                    AntSdkService.AntSdkCurrentUserInfo.userId, antSdkInput.chatIndex.ToString(), antSdkInput.count));

                if (listChatdata != null && listChatdata.Count > 0)
                {
                    var tempLocalMsgData = listChatdata.ToList();
                    var min = tempLocalMsgData.Min(m => int.Parse(m.chatIndex));
                    var max = antSdkInput.chatIndex;
                    var count = max - min;
                    var loseChatIndexList = Enumerable.Range(min, count).Where(m => !tempLocalMsgData.Exists(n => int.Parse(n.chatIndex) == m));
                    //如果有漏消息，就返回true（去查询漫游服务器消息）
                    if (loseChatIndexList != null && loseChatIndexList.Count() > 0 && isSynchronus)
                    {
                        msgOutput = tempLocalMsgData;
                        return true;
                    }
                    else
                    {
                        var msgList = tempLocalMsgData.Where(m => string.IsNullOrEmpty(m.sourceContent) && string.IsNullOrEmpty(m.sendUserId)).ToList();
                        if (msgList.Count > 0)
                        {
                            var tempMsgList = tempLocalMsgData.Where(m => !string.IsNullOrEmpty(m.messageId) && !string.IsNullOrEmpty(m.sourceContent) && !string.IsNullOrEmpty(m.sendUserId)).ToList();
                            msgOutput.AddRange(tempMsgList);
                            antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                            if (msgOutput.Count < antSdkInput.count)
                                GetLocalMsgData(antSdkInput, ref msgOutput);
                        }
                        else if (msgOutput.Count > 0)
                        {
                            msgOutput.AddRange(tempLocalMsgData);
                        }
                        else if (tempLocalMsgData.Count == antSdkInput.count)
                        {
                            msgOutput = tempLocalMsgData;
                        }
                        else if (tempLocalMsgData.Count < antSdkInput.count)
                        {
                            msgOutput = tempLocalMsgData;
                            antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                            return true;
                        }
                        if (msgOutput.Count < antSdkInput.count)
                            return true;
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (antSdkInput.flag == (int)AntSdkBurnFlag.NotIsBurn)//普通消息
                {
                    //获取本地群组会话消息
                    var listGroupChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_GroupChat.getDataByScroll(antSdkInput.sessionId, AntSdkService.AntSdkCurrentUserInfo.companyCode,
                      AntSdkService.AntSdkCurrentUserInfo.userId, antSdkInput.chatIndex.ToString(), antSdkInput.count));
                    if (listGroupChatdata != null && listGroupChatdata.Count > 0)
                    {
                        var tempLocalMsgData = listGroupChatdata.ToList();
                        var min = tempLocalMsgData.Min(m => int.Parse(m.chatIndex));
                        var max = antSdkInput.chatIndex;
                        var count = max - min;
                        var loseChatIndexList = Enumerable.Range(min, count).Where(m => !tempLocalMsgData.Exists(n => int.Parse(n.chatIndex) == m));
                        //如果有漏消息，就返回true（去查询漫游服务器消息）
                        if (loseChatIndexList != null && loseChatIndexList.Count() > 0)
                        {
                            msgOutput = tempLocalMsgData;
                            return true;
                        }
                        else
                        {
                            var msgList = tempLocalMsgData.Where(m => string.IsNullOrEmpty(m.sourceContent) && string.IsNullOrEmpty(m.sendUserId)).ToList();
                            //如果有无效消息，继续查询本地库
                            if (msgList.Count > 0)
                            {
                                var tempMsgList = tempLocalMsgData.Where(m => !string.IsNullOrEmpty(m.messageId) && !string.IsNullOrEmpty(m.sourceContent) && !string.IsNullOrEmpty(m.sendUserId)).ToList();
                                msgOutput.AddRange(tempMsgList);
                                antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                                if (msgOutput.Count < antSdkInput.count)
                                    GetLocalMsgData(antSdkInput, ref msgOutput);
                            }
                            else if (msgOutput.Count > 0)
                            {
                                msgOutput.AddRange(tempLocalMsgData);
                            }
                            else if (tempLocalMsgData.Count == antSdkInput.count)
                            {
                                msgOutput = tempLocalMsgData;
                            }
                            else if (tempLocalMsgData.Count < antSdkInput.count)
                            {
                                msgOutput = tempLocalMsgData;
                                antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                                return true;
                            }
                            if (msgOutput.Count < antSdkInput.count)
                                return true;
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    //获取本地群组无痕会话消息
                    var listGroupChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_GroupBurnChat.getDataByScroll(antSdkInput.sessionId, AntSdkService.AntSdkCurrentUserInfo.companyCode,
                                             AntSdkService.AntSdkCurrentUserInfo.userId, antSdkInput.chatIndex.ToString(), antSdkInput.count));
                    if (listGroupChatdata != null && listGroupChatdata.Count > 0)
                    {
                        var tempLocalMsgData = listGroupChatdata.ToList();
                        var min = tempLocalMsgData.Min(m => int.Parse(m.chatIndex));
                        var max = antSdkInput.chatIndex;
                        var count = max - min;
                        var loseChatIndexList = Enumerable.Range(min, count).Where(m => !tempLocalMsgData.Exists(n => int.Parse(n.chatIndex) == m));
                        //如果有漏消息，就返回true（去查询漫游服务器消息）
                        if (loseChatIndexList != null && loseChatIndexList.Count() > 0)
                        {
                            msgOutput = tempLocalMsgData;
                            return true;
                        }
                        else
                        {
                            var msgList = tempLocalMsgData.Where(m => string.IsNullOrEmpty(m.sourceContent) && string.IsNullOrEmpty(m.sendUserId)).ToList();
                            //如果有无效消息，继续查询本地库
                            if (msgList.Count > 0)
                            {
                                var tempMsgList = tempLocalMsgData.Where(m => !string.IsNullOrEmpty(m.messageId) && !string.IsNullOrEmpty(m.sourceContent) && !string.IsNullOrEmpty(m.sendUserId)).ToList();
                                msgOutput.AddRange(tempMsgList);
                                antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                                if (msgOutput.Count < antSdkInput.count)
                                    GetLocalMsgData(antSdkInput, ref msgOutput);
                            }
                            else if (msgOutput.Count > 0)
                            {
                                msgOutput.AddRange(tempLocalMsgData);
                            }
                            else if (tempLocalMsgData.Count == antSdkInput.count)
                            {
                                msgOutput = tempLocalMsgData;
                            }
                            else if (tempLocalMsgData.Count < antSdkInput.count)
                            {
                                msgOutput = tempLocalMsgData;
                                antSdkInput.chatIndex = int.Parse(tempLocalMsgData[tempLocalMsgData.Count - 1].chatIndex);
                                return true;
                            }
                            if (msgOutput.Count < antSdkInput.count)
                                return true;
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        /// <summary>
        /// 获取服务器群组历史消息数据
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static List<AntSdkChatMsg.ChatBase> GetServiceGroupSynchronusMsgs(SynchronusMsgInput input, ref List<SynchronusMsgOutput> output, List<AntSdkChatMsg.ChatBase> localMsgList, ref int errorCode, ref string errorMsg)
        {
            var antSdkOutput = new List<AntSdkChatMsg.ChatBase>();
            try
            {
                SdkService.SynchronusMsgs(input, ref output, ref errorCode, ref errorMsg);
                if (output != null && output.Count > 0)
                {
                    var contentobjList = new List<SdkMsBase>();
                    var sbSql = new StringBuilder();
                    if (input.flag == (int)AntSdkBurnFlag.NotIsBurn)//普通消息
                    {
                        DeleteChatMsgList.AddRange(output.Where(m => m.messageType == "1014" || m.messageType == "1017" || m.messageType == "1018").ToList());
                        var min = output.Min(m => int.Parse(m.chatIndex));
                        var max = input.chatIndex;
                        sbSql.Append("INSERT OR IGNORE INTO T_Chat_Message_Group(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL,SPARE1) values");
                        var count = max - min;
                        if (max - min > input.count - 1)
                        {
                            //var loseChatIndex = Enumerable.Range(min, count).Where(m => !output.Exists(n =>int.Parse(n.chatIndex)==m));
                            for (int i = min; i <= max; i++)
                            {
                                var msgInfo = output.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);
                                if (msgInfo == null)
                                {
                                    var chatBase = new AntSdkChatMsg.ChatBase();
                                    chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                    chatBase.sessionId = output[0].sessionId;
                                    chatBase.chatIndex = i.ToString();
                                    //chatBase.messageId=
                                    sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                       + chatBase.messageId + "','" + chatBase.sendTime +
                                       "','" + chatBase.sendUserId + "','" + chatBase.sessionId + "','" + chatBase.targetId +
                                       "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.VoteOrActivityID + "'),");
                                }
                                else
                                {
                                    var tempChatMsg = ChatMsgModelChange(msgInfo, AntSdkchatType.Group, ref errorMsg);
                                    if (tempChatMsg == null || RemoveMsg(tempChatMsg, ref errorMsg))
                                    {
                                        var chatBase = new AntSdkChatMsg.ChatBase();
                                        chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                        chatBase.sessionId = msgInfo.sessionId;
                                        chatBase.chatIndex = msgInfo.chatIndex;
                                        chatBase.messageId = msgInfo.messageId;
                                        sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                           + chatBase.messageId + "','" + chatBase.sendTime +
                                           "','" + chatBase.sendUserId + "','" + chatBase.sessionId + "','" + chatBase.targetId +
                                           "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.VoteOrActivityID + "'),");
                                        continue;
                                    }
                                    if (localMsgList != null && localMsgList.Count > 0)
                                    {
                                        var tempMsgInfo = localMsgList.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);
                                        if (tempMsgInfo != null)
                                        {
                                            if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                                antSdkOutput.Add(tempMsgInfo);
                                            continue;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                        antSdkOutput.Add(tempChatMsg);
                                    string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                                    sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                        + tempChatMsg.messageId + "','" + tempChatMsg.sendTime +
                                        "','" + tempChatMsg.sendUserId + "','" + tempChatMsg.sessionId + "','" + tempChatMsg.targetId +
                                        "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "','" + tempChatMsg.VoteOrActivityID + "'),");
                                }
                            }

                        }
                        else
                        {
                            foreach (var chatMsg in output)
                            {
                                var tempChatMsg = ChatMsgModelChange(chatMsg, AntSdkchatType.Group, ref errorMsg);
                                if (tempChatMsg == null || RemoveMsg(tempChatMsg, ref errorMsg))
                                {
                                    var chatBase = new AntSdkChatMsg.ChatBase();
                                    chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                    chatBase.sessionId = chatMsg.sessionId;
                                    chatBase.chatIndex = chatMsg.chatIndex;
                                    chatBase.messageId = chatMsg.messageId;
                                    sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                       + chatBase.messageId + "','" + chatBase.sendTime +
                                       "','" + chatBase.sendUserId + "','" + chatBase.sessionId + "','" + chatBase.targetId +
                                       "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.VoteOrActivityID + "'),");
                                    continue;
                                }
                                if (localMsgList != null && localMsgList.Count > 0)
                                {
                                    var tempMsgInfo = localMsgList.FirstOrDefault(m => m.chatIndex == chatMsg.chatIndex && input.sessionId == m.sessionId && m.messageId == chatMsg.messageId);
                                    if (tempMsgInfo != null)
                                    {
                                        if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                            antSdkOutput.Add(tempMsgInfo);
                                        continue;
                                    }
                                }
                                if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                    antSdkOutput.Add(tempChatMsg);
                                string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                                sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                    + tempChatMsg.messageId + "','" + tempChatMsg.sendTime +
                                    "','" + tempChatMsg.sendUserId + "','" + tempChatMsg.sessionId + "','" + tempChatMsg.targetId +
                                    "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "','" + tempChatMsg.VoteOrActivityID + "'),");
                            }
                        }

                    }
                    else//无痕消息
                    {
                        var min = output.Min(m => int.Parse(m.chatIndex));
                        var max = input.chatIndex;
                        sbSql.Append("INSERT OR IGNORE INTO t_chat_message_groupburn(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL) values");
                        var count = max - min;
                        if (max - min > input.count - 1)
                        {
                            //var loseChatIndex = Enumerable.Range(min, count).Where(m => !output.Exists(n =>int.Parse(n.chatIndex)==m));
                            for (int i = min; i <= max; i++)
                            {
                                var msgInfo = output.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);
                                if (msgInfo == null)
                                {
                                    var chatBase = new AntSdkChatMsg.ChatBase();
                                    chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                    chatBase.sessionId = output[0].sessionId;
                                    chatBase.chatIndex = i.ToString();
                                    //chatBase.messageId=
                                    sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                       + chatBase.messageId + "','" + chatBase.sendTime +
                                       "','" + chatBase.sendUserId + "','" + chatBase.sessionId + "','" + chatBase.targetId +
                                       "','" + chatBase.SENDORRECEIVE + "','" + 1 + "'),");
                                }
                                else
                                {
                                    var tempChatMsg = ChatMsgModelChange(msgInfo, AntSdkchatType.Group, ref errorMsg);
                                    if (tempChatMsg == null) continue;
                                    if (localMsgList != null && localMsgList.Count > 0)
                                    {
                                        var tempMsgInfo = localMsgList.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);
                                        if (tempMsgInfo != null)
                                        {
                                            if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                                antSdkOutput.Add(tempMsgInfo);
                                            continue;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                        antSdkOutput.Add(tempChatMsg);
                                    string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                                    sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                        + tempChatMsg.messageId + "','" + tempChatMsg.sendTime +
                                        "','" + tempChatMsg.sendUserId + "','" + tempChatMsg.sessionId + "','" + tempChatMsg.targetId +
                                        "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "'),");
                                }
                            }

                        }
                        else
                        {
                            foreach (var chatMsg in output)
                            {
                                var tempChatMsg = ChatMsgModelChange(chatMsg, AntSdkchatType.Group, ref errorMsg);
                                if (tempChatMsg == null) continue;
                                if (localMsgList != null && localMsgList.Count > 0)
                                {
                                    var tempMsgInfo = localMsgList.FirstOrDefault(m => m.chatIndex == chatMsg.chatIndex && input.sessionId == m.sessionId && m.messageId == chatMsg.messageId);
                                    if (tempMsgInfo != null)
                                    {
                                        if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                            antSdkOutput.Add(tempMsgInfo);
                                        continue;
                                    }
                                }
                                if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                    antSdkOutput.Add(tempChatMsg);
                                string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                                sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                    + tempChatMsg.messageId + "','" + tempChatMsg.sendTime +
                                    "','" + tempChatMsg.sendUserId + "','" + tempChatMsg.sessionId + "','" + tempChatMsg.targetId +
                                    "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "'),");
                            }
                        }
                    }
                    var str = sbSql.ToString().Substring(0, sbSql.ToString().Length - 1);
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        LogHelper.WriteDebug("群组会话漫游消息入本地库-----------" + input.sessionId + "-------------：" + str);
                        var isMsgRecord = AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);
                    });
                }
                return antSdkOutput;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("群组消息漫游：---------" + ex.Message);
                return antSdkOutput;
            }

        }
        /// <summary>
        /// 删除撤销、投票活动被删除的消息
        /// </summary>
        /// <param name="deleteMsgList"></param>
        /// <param name="tempChatMsg"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static bool RemoveMsg(AntSdkChatMsg.ChatBase tempChatMsg, ref string errorMsg)
        {

            var isFlag = false;
            if (tempChatMsg == null)
                return true;
            if (tempChatMsg.MsgType != AntSdkMsgType.Revocation && tempChatMsg.MsgType != AntSdkMsgType.CreateActivity && tempChatMsg.MsgType != AntSdkMsgType.CreateVote)
                return false;
            if (DeleteChatMsgList != null && DeleteChatMsgList.Count > 0)
            {
                for (int i = 0; i < DeleteChatMsgList.Count; i++)
                {
                    var deleteMsg = DeleteChatMsgList[i];
                    if (tempChatMsg.sessionId != deleteMsg.sessionId)
                        continue;
                    var tempDeleteChatMsg = ChatMsgModelChange(deleteMsg, AntSdkchatType.Group, ref errorMsg);
                    if (tempDeleteChatMsg != null)
                    {
                        if (tempDeleteChatMsg.MsgType == AntSdkMsgType.Revocation)
                        {
                            var tempRevocationChatMsg = (AntSdkChatMsg.Revocation)tempDeleteChatMsg;
                            var msgID = tempRevocationChatMsg.content?.messageId;
                            if (!string.IsNullOrEmpty(msgID) && msgID == tempChatMsg.messageId)
                            {
                                isFlag = true;
                                DeleteChatMsgList.Remove(deleteMsg);
                                var isResult = t_GroupChat.DeleteGroupMsgByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                         AntSdkService.AntSdkCurrentUserInfo.userId, msgID);
                                break;
                            }
                        }
                        else if (tempChatMsg.MsgType == AntSdkMsgType.CreateActivity && tempDeleteChatMsg.MsgType == AntSdkMsgType.DeleteActivity)
                        {
                            var newActivityMsgID = string.Empty;
                            var tempMsg = (AntSdkChatMsg.ActivityMsg)tempChatMsg;
                            if (tempMsg.content != null)
                                newActivityMsgID = tempMsg.content.activityId.ToString();
                            var activityStrID = string.Empty;
                            var tempDeleteActivityChatMsg = (AntSdkChatMsg.DeleteActivityMsg)tempDeleteChatMsg;
                            if (tempDeleteActivityChatMsg.content != null)
                            {
                                activityStrID = tempDeleteActivityChatMsg.content.activityId.ToString();
                            }
                            if (!string.IsNullOrEmpty(newActivityMsgID) && !string.IsNullOrEmpty(activityStrID) && newActivityMsgID == activityStrID)
                            {
                                isFlag = true;
                                DeleteChatMsgList.Remove(deleteMsg);
                                var baseChatMsg = t_GroupChat.GetGroupMsgByVoteOrActivityId(activityStrID,
                                 tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateVote).ToString(),
                                 AntSdkService.AntSdkLoginOutput.companyCode,
                                 AntSdkService.AntSdkCurrentUserInfo.userId);
                                break;
                            }
                        }
                        else if (tempChatMsg.MsgType == AntSdkMsgType.CreateVote && tempDeleteChatMsg.MsgType == AntSdkMsgType.DeleteVote)
                        {
                            var newVoteMsgID = string.Empty;
                            var tempMsg = (AntSdkChatMsg.CreateVoteMsg)tempChatMsg;
                            if (tempMsg.content != null)
                                newVoteMsgID = tempMsg.content.id.ToString();
                            var voteStrID = string.Empty;
                            var tempDeleteVoteChatMsg = (AntSdkChatMsg.DeteleVoteMsg)tempDeleteChatMsg;
                            if (tempDeleteVoteChatMsg.content != null)
                            {
                                voteStrID = tempDeleteVoteChatMsg.content.id.ToString();
                            }
                            if (!string.IsNullOrEmpty(newVoteMsgID) && !string.IsNullOrEmpty(voteStrID) && newVoteMsgID == voteStrID)
                            {
                                isFlag = true;
                                DeleteChatMsgList.Remove(deleteMsg);
                                var baseChatMsg = t_GroupChat.GetGroupMsgByVoteOrActivityId(voteStrID,
                                tempChatMsg.sessionId, ((int)AntSdkMsgType.CreateActivity).ToString(),
                                AntSdkService.AntSdkLoginOutput.companyCode,
                                AntSdkService.AntSdkCurrentUserInfo.userId);
                                break;
                            }
                        }
                    }

                }
            }
            return isFlag;
        }

        /// <summary>
        /// 获取服务器单人息数据
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static List<AntSdkChatMsg.ChatBase> GetServiceSynchronusMsgs(SynchronusMsgInput input, ref List<SynchronusMsgOutput> output, List<AntSdkChatMsg.ChatBase> localMsgList, ref int errorCode, ref string errorMsg)
        {
            var antSdkOutput = new List<AntSdkChatMsg.ChatBase>();
            try
            {
                SdkService.SynchronusMsgs(input, ref output, ref errorCode, ref errorMsg);
                if (output != null && output.Count > 0)
                {
                    var contentobjList = new List<SdkMsBase>();
                    var tempDeleteMsgList = new List<AntSdkChatMsg.ChatBase>();
                    var sbSql = new StringBuilder();
                    var min = output.Min(m => int.Parse(m.chatIndex));
                    var max = input.chatIndex;
                    DeleteChatMsgList.AddRange(output.Where(m => m.messageType == "1014").ToList());
                    sbSql.Append("INSERT OR IGNORE INTO T_Chat_Message(MTP, CHATINDEX, CONTENT, MESSAGEID, SENDTIME, SENDUSERID, SESSIONID, TARGETID, SENDORRECEIVE, SENDSUCESSORFAIL,flag) values");
                    var count = max - min;
                    //if (deleteMsgList != null && deleteMsgList.Count > 0)
                    //{
                    //    foreach (var msgInfo in deleteMsgList)
                    //    {
                    //        var tempChatMsg = ChatMsgModelChange(msgInfo, AntSdkchatType.Group, ref errorMsg);
                    //        if (tempChatMsg != null)
                    //            tempDeleteMsgList.Add(tempChatMsg);
                    //    }
                    //}
                    if (max - min > input.count - 1)
                    {
                        //var loseChatIndex = Enumerable.Range(min, count).Where(m => !output.Exists(n =>int.Parse(n.chatIndex)==m));
                        for (int i = min; i <= max; i++)
                        {

                            var msgInfo = output.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);

                            if (msgInfo == null)
                            {
                                var chatBase = new AntSdkChatMsg.ChatBase();
                                chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                chatBase.sessionId = output[0].sessionId;
                                chatBase.chatIndex = i.ToString();
                                //chatBase.messageId=
                                sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                    + chatBase.messageId + "','" + chatBase.sendTime + "','" + chatBase.sendUserId + "','"
                                    + chatBase.sessionId + "','" + chatBase.targetId + "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.flag + "'),");
                            }
                            else
                            {
                                var isFlag = false;
                                var tempChatMsg = ChatMsgModelChange(msgInfo, AntSdkchatType.Group, ref errorMsg);
                                if (DeleteChatMsgList != null && DeleteChatMsgList.Count > 0)
                                {
                                    for (int msgCount = 0; msgCount < DeleteChatMsgList.Count; msgCount++)
                                    {

                                        var deleteMsg = DeleteChatMsgList[msgCount];
                                        if (tempChatMsg.sessionId != deleteMsg.sessionId)
                                            continue;
                                        var tempDeleteChatMsg = ChatMsgModelChange(deleteMsg, AntSdkchatType.Point, ref errorMsg);
                                        if (tempDeleteChatMsg != null)
                                        {
                                            var tempRevocationChatMsg = (AntSdkChatMsg.Revocation)tempDeleteChatMsg;
                                            var msgID = tempRevocationChatMsg.content?.messageId;
                                            if (!string.IsNullOrEmpty(msgID) && msgID == tempChatMsg.messageId)
                                            {
                                                isFlag = true;
                                                DeleteChatMsgList.Remove(deleteMsg);
                                                var isResult = t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                        AntSdkService.AntSdkCurrentUserInfo.userId, msgID);
                                                break;
                                            }
                                        }

                                    }
                                }
                                if (tempChatMsg == null || isFlag)
                                {
                                    var chatBase = new AntSdkChatMsg.ChatBase();
                                    chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                    chatBase.sessionId = msgInfo.sessionId;
                                    chatBase.chatIndex = msgInfo.chatIndex;
                                    chatBase.sendTime = msgInfo.sendTime;
                                    sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                         + chatBase.messageId + "','" + chatBase.sendTime + "','" + chatBase.sendUserId + "','"
                                         + chatBase.sessionId + "','" + chatBase.targetId + "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.flag + "'),");
                                    continue;
                                }
                                if (localMsgList != null && localMsgList.Count > 0)
                                {
                                    var tempMsgInfo = localMsgList.FirstOrDefault(m => int.Parse(m.chatIndex) == i && input.sessionId == m.sessionId);
                                    if (tempMsgInfo != null)
                                    {
                                        if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                            antSdkOutput.Add(tempMsgInfo);
                                        continue;
                                    }
                                }
                                if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                    antSdkOutput.Add(tempChatMsg);
                                string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                                sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                    + tempChatMsg.messageId + "','" + tempChatMsg.sendTime + "','" + tempChatMsg.sendUserId + "','"
                                    + tempChatMsg.sessionId + "','" + tempChatMsg.targetId + "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "','" + tempChatMsg.flag + "'),");
                            }
                        }
                    }
                    else
                    {
                        foreach (var chatMsg in output)
                        {

                            var tempChatMsg = ChatMsgModelChange(chatMsg, AntSdkchatType.Group, ref errorMsg);
                            var isFlag = false;
                            if (DeleteChatMsgList != null && DeleteChatMsgList.Count > 0)
                            {
                                for (int msgCount = 0; msgCount < DeleteChatMsgList.Count; msgCount++)
                                {
                                    var deleteMsg = DeleteChatMsgList[msgCount];
                                    if (tempChatMsg.sessionId != deleteMsg.sessionId)
                                        continue;
                                    var tempDeleteChatMsg = ChatMsgModelChange(deleteMsg, AntSdkchatType.Point, ref errorMsg);
                                    if (tempDeleteChatMsg != null)
                                    {
                                        var tempRevocationChatMsg = (AntSdkChatMsg.Revocation)tempDeleteChatMsg;
                                        var msgID = tempRevocationChatMsg.content?.messageId;
                                        if (!string.IsNullOrEmpty(msgID) && msgID == tempChatMsg.messageId)
                                        {
                                            isFlag = true;
                                            DeleteChatMsgList.Remove(deleteMsg);
                                            var isResult = t_chat.DeleteByMessageId(AntSdkService.AntSdkLoginOutput.companyCode,
                                        AntSdkService.AntSdkCurrentUserInfo.userId, msgID);
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tempChatMsg == null || isFlag)
                            {
                                var chatBase = new AntSdkChatMsg.ChatBase();
                                chatBase.MsgType = AntSdkMsgType.ChatMsgText;
                                chatBase.sessionId = chatMsg.sessionId;
                                chatBase.chatIndex = chatMsg.chatIndex;
                                chatBase.sendTime = chatMsg.sendTime;
                                sbSql.Append("('" + (int)chatBase.MsgType + "','" + chatBase.chatIndex + "','" + chatBase.sourceContent + "','"
                                     + chatBase.messageId + "','" + chatBase.sendTime + "','" + chatBase.sendUserId + "','"
                                     + chatBase.sessionId + "','" + chatBase.targetId + "','" + chatBase.SENDORRECEIVE + "','" + 1 + "','" + chatBase.flag + "'),");
                                continue;
                            }
                            if (localMsgList != null && localMsgList.Count > 0)
                            {
                                var tempMsgInfo = localMsgList.FirstOrDefault(m => m.chatIndex == chatMsg.chatIndex && input.sessionId == m.sessionId && m.messageId == chatMsg.messageId);
                                if (tempMsgInfo != null)
                                {
                                    if (!antSdkOutput.Exists(m => m.chatIndex == tempMsgInfo.chatIndex && m.messageId == tempMsgInfo.messageId))
                                        antSdkOutput.Add(tempMsgInfo);
                                    continue;
                                }
                            }
                            if (!string.IsNullOrEmpty(tempChatMsg.sendTime) && !string.IsNullOrEmpty(tempChatMsg.sourceContent))
                                antSdkOutput.Add(tempChatMsg);
                            string content = string.IsNullOrEmpty(tempChatMsg.sourceContent) ? "" : tempChatMsg.sourceContent.Replace("'", "''");
                            sbSql.Append("('" + (int)tempChatMsg.MsgType + "','" + tempChatMsg.chatIndex + "','" + content + "','"
                                     + tempChatMsg.messageId + "','" + tempChatMsg.sendTime + "','" + tempChatMsg.sendUserId + "','"
                                     + tempChatMsg.sessionId + "','" + tempChatMsg.targetId + "','" + tempChatMsg.SENDORRECEIVE + "','" + 1 + "','" + tempChatMsg.flag + "'),");
                        }
                    }

                    var str = sbSql.ToString().Substring(0, sbSql.ToString().Length - 1);
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        LogHelper.WriteDebug("单人会话漫游消息入本地库-----------" + input.sessionId + "-------------：" + str);
                        var isMsgRecord = AntSdkSqliteHelper.InsertBigData(str, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId);

                    });
                }
                return antSdkOutput;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("单人消息漫游：---------" + ex.Message);
                return antSdkOutput;
            }
        }
        /// <summary>
        /// 消息实体类转换
        /// </summary>
        /// <param name="msgInfo"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static AntSdkChatMsg.ChatBase ChatMsgModelChange(SynchronusMsgOutput msgInfo, AntSdkchatType chatType, ref string errorMsg)
        {
            SdkMsgType eventType = SdkMsgType.UnDefineMsg;
            var tempChatMsg = MsgConverter.ReceiveChatMsgEntity(msgInfo, ref eventType, ref errorMsg);
            if (tempChatMsg == null) return null;
            tempChatMsg.MsgType = eventType;
            var msgListNormalMode = AntSdkChatMsg.GetAntSdkReceivedChat(tempChatMsg);
            switch (msgListNormalMode.MsgType)
            {
                case AntSdkMsgType.Revocation:
                    string sourceContent;
                    if (msgListNormalMode.sendUserId == AntSdkService.AntSdkLoginOutput.userId)
                    {
                        sourceContent = "你撤回了一条消息";
                    }
                    else if (chatType == AntSdkchatType.Group && msgListNormalMode.sendUserId != AntSdkService.AntSdkLoginOutput.userId)
                    {
                        var userInfo =
                            AntSdkService.AntSdkListContactsEntity?.users?.FirstOrDefault(
                                m => m.userId == msgListNormalMode.sendUserId);
                        string userName = string.Empty;
                        if (userInfo != null)
                            userName = userInfo.userNum + userInfo.userName;
                        sourceContent = userName + "撤回了一条消息";
                    }
                    else
                    {
                        sourceContent = "对方撤回了一条消息";
                    }
                    msgListNormalMode.sourceContent = sourceContent;
                    break;
                case AntSdkMsgType.CreateVote:
                    var tempCreateVoteMsg = (AntSdkChatMsg.CreateVoteMsg)msgListNormalMode;
                    if (tempCreateVoteMsg.content != null)
                        msgListNormalMode.VoteOrActivityID = tempCreateVoteMsg.content.id.ToString();
                    break;
                case AntSdkMsgType.CreateActivity:
                    var tempActivityMsg = (AntSdkChatMsg.ActivityMsg)msgListNormalMode;
                    if (tempActivityMsg.content != null)
                        msgListNormalMode.VoteOrActivityID = tempActivityMsg.content.activityId.ToString();
                    break;
                case AntSdkMsgType.DeleteActivity:
                    var activityStrID = string.Empty;
                    var tempDeleteActivityChatMsg = (AntSdkChatMsg.DeleteActivityMsg)msgListNormalMode;
                    if (tempDeleteActivityChatMsg.content != null)
                    {
                        activityStrID = tempDeleteActivityChatMsg.content.activityId.ToString();
                    }
                    break;
                case AntSdkMsgType.DeleteVote:
                    var voteStrID = string.Empty;
                    var tempDeleteVoteChatMsg = (AntSdkChatMsg.DeteleVoteMsg)msgListNormalMode;
                    if (tempDeleteVoteChatMsg.content != null)
                    {
                        voteStrID = tempDeleteVoteChatMsg.content.id.ToString();
                    }
                    break;
            }
            if (msgListNormalMode.sendUserId == AntSdkService.AntSdkLoginOutput.userId)
            {
                msgListNormalMode.SENDORRECEIVE = "1";
            }
            else
            {
                msgListNormalMode.SENDORRECEIVE = "0";
            }
            msgListNormalMode.sendsucessorfail = 1;
            return msgListNormalMode;
        }
        #endregion
        #region 投票
        /// <summary>
        /// 发起投票
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns></returns>
        public static AntSdkGetVoteInfoOutput CreateGroupVote(AntSdkCreateGroupVoteInput input, string groupId, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            var sdkoutput = SdkService.CreateGroupVote(sdk, groupId, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetVoteInfoOutput
            {
                id = sdkoutput.id,
                groupId = sdkoutput.groupId,
                createdBy = sdkoutput.createdBy,
                createdDate = sdkoutput.createdDate,
                expiryTime = sdkoutput.expiryTime,
                maxChoiceNumber = sdkoutput.maxChoiceNumber,
                options = new List<VoteOptionInfo>(),
                secret = sdkoutput.secret,
                title = sdkoutput.title,
                voted = sdkoutput.voted,
                voters = sdkoutput.voters
            };
            foreach (var optionInfo in sdkoutput.options)
            {
                antsdkoutput.options.Add(new VoteOptionInfo
                {
                    id = optionInfo.id,
                    name = optionInfo.name,
                    total = optionInfo.total,
                    voted = optionInfo.voted
                });
            }
            return antsdkoutput;
        }

        /// <summary>
        /// 获取投票详情
        /// </summary>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static AntSdkGetVoteInfoOutput GetVoteInfo(int voteId, string userId, ref int errorCode, ref string errorMsg)
        {
            //if (!CheckSdkServiceState(ref errorMsg))
            //{
            //    return null;
            //}
            var sdkoutput = SdkService.GetVoteInfo(voteId, userId, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetVoteInfoOutput
            {
                id = sdkoutput.id,
                groupId = sdkoutput.groupId,
                createdBy = sdkoutput.createdBy,
                createdDate = sdkoutput.createdDate,
                expiryTime = sdkoutput.expiryTime,
                maxChoiceNumber = sdkoutput.maxChoiceNumber,
                options = new List<VoteOptionInfo>(),
                secret = sdkoutput.secret,
                title = sdkoutput.title,
                voted = sdkoutput.voted,
                voters = sdkoutput.voters
            };
            foreach (var optionInfo in sdkoutput.options)
            {
                antsdkoutput.options.Add(new VoteOptionInfo
                {
                    id = optionInfo.id,
                    name = optionInfo.name,
                    total = optionInfo.total,
                    voted = optionInfo.voted
                });
            }
            return antsdkoutput;
        }

        /// <summary>
        /// 根据群ID获取群的所有投票活动
        /// </summary>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static AntSdkGetGroupVotesOutput GetGroupVotes(string groupId, ref int errorCode, ref string errorMsg, int page = 0, int size = 0, string userId = "")
        {

            var sdkoutput = SdkService.GetGroupVotes(groupId, ref errorCode, ref errorMsg, page, size, userId);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetGroupVotesOutput
            {
                first = sdkoutput.first,
                last = sdkoutput.last,
                number = sdkoutput.number,
                numberOfElements = sdkoutput.numberOfElements,
                size = sdkoutput.size,
                totalElements = sdkoutput.totalElements,
                totalPages = sdkoutput.totalPages,
            };

            if (sdkoutput.content != null && sdkoutput.content.Count > 0)
            {
                antsdkoutput.content.AddRange(sdkoutput.content.Where(c => c != null)
                    .Select(c => new GroupVoteContent
                    {
                        createdBy = c.createdBy,
                        createdDate = c.createdDate,
                        expiryTime = c.expiryTime,
                        id = c.id,
                        maxChoiceNumber = c.maxChoiceNumber,
                        secret = c.secret,
                        title = c.title,
                        voted = c.voted
                    }));
            }
            return antsdkoutput;
        }

        /// <summary>
        /// 获取所有弃权票信息
        /// </summary>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static AntSdkGroupAbstentionVoteOutput GetGroupAbstentionVote(int voteId, ref int errorCode,
            ref string errorMsg)
        {
            //if (!CheckSdkServiceState(ref errorMsg))
            //{
            //    return null;
            //}
            var sdkoutput = SdkService.GetGroupAbstentionVote(voteId, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGroupAbstentionVoteOutput
            {
                voters = sdkoutput.voters
            };
            return antsdkoutput;
        }

        /// <summary>
        /// 根据ID删除投票活动
        /// </summary>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool DeleteGroupVote(int voteId, ref int errorCode, ref string errorMsg)
        {
            var tempMsg = string.Empty;
            var sdkoutput = SdkService.DeleteGroupVote(voteId, AntSdkService.AntSdkCurrentUserInfo.userId, ref errorCode, ref errorMsg);
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return sdkoutput;
        }


        /// <summary>
        /// 提交群投票选项
        /// </summary>
        /// <param name="input"></param>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool SubmitGroupVoteOptions(AntSdkGroupVoteOptionInput input, int voteId, ref int errorCode,
            ref string errorMsg)
        {
            var tempMsg = string.Empty;
            var sdkinput = input.GetSdk();
            var sdkoutput = SdkService.SubmitGroupVoteOptions(sdkinput, voteId, ref errorCode, ref errorMsg);
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return sdkoutput;
        }
        #endregion
        #region 活动
        /// <summary>
        /// 发布活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool ReleaseGroupActivity(AntSdkReleaseGroupActivityInput input, string groupId, ref int errorCode, ref string errorMsg)
        {
            var tempMsg = string.Empty;
            var sdkinput = input.GetSdk();
            var sdkoutput = SdkService.ReleaseGroupActivity(sdkinput, groupId, ref errorCode, ref errorMsg);
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return sdkoutput;
        }

        /// <summary>
        /// 根据群ID获取群的活动列表
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static AntSdkGetGroupActivitysOutput GetGroupActivitys(AntSdkGetGroupActivitysInput input, ref int errorCode, ref string errorMsg)
        {
            var tempMsg = string.Empty;
            var sdkinput = input.GetSdk();

            var sdkoutput = SdkService.GetGroupActivitys(sdkinput, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetGroupActivitysOutput
            {
                pageNum = sdkoutput.pageNum,
                pageSize = sdkoutput.pageSize,
                total = sdkoutput.total,
                isFirstPage = sdkoutput.isFirstPage,
                isLastPage = sdkoutput.isLastPage
            };
            if (sdkoutput.list.Count > 0)
            {
                antsdkoutput.list.AddRange(sdkoutput.list.Where(m => m != null).Select(m => new AntSdkGroupActivityDetailOutput
                {
                    activityId = m.activityId,
                    userId = m.userId,
                    groupId = m.groupId,
                    theme = m.theme,
                    picture = m.picture,
                    address = m.address,
                    latitude = m.latitude,
                    longitude = m.longitude,
                    startTime = m.startTime,
                    endTime = m.endTime,
                    remindTime = m.remindTime,
                    description = m.description,
                    activityStatus = m.activityStatus,
                    createTime = m.createTime,
                    voteFlag = m.voteFlag
                }));
            }
            errorMsg = tempMsg;
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return antsdkoutput;
        }

        /// <summary>
        /// 获取活动详情
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static AntSdkGroupActivityDetailOutput GetActivityInfo(AntSdkGetGroupActivityDetailsInput input, ref int errorCode, ref string errorMsg)
        {
            var tempMsg = string.Empty;
            var sdkinput = input.GetSdk();

            var sdkoutput = SdkService.GetActivityInfo(sdkinput, ref errorCode, ref errorMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGroupActivityDetailOutput
            {
                activityId = sdkoutput.activityId,
                userId = sdkoutput.userId,
                groupId = sdkoutput.groupId,
                theme = sdkoutput.theme,
                picture = sdkoutput.picture,
                address = sdkoutput.address,
                latitude = sdkoutput.latitude,
                longitude = sdkoutput.longitude,
                startTime = sdkoutput.startTime,
                endTime = sdkoutput.endTime,
                remindTime = sdkoutput.remindTime,
                description = sdkoutput.description,
                activityStatus = sdkoutput.activityStatus,
                createTime = sdkoutput.createTime,
                voteFlag = sdkoutput.voteFlag,
                voteCount = sdkoutput.voteCount
            };
            errorMsg = tempMsg;
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return antsdkoutput;
        }
        /// <summary>
        /// 根据ID删除活动
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool DeleteGroupActivity(AntSdkDeleteGroupActivityInput input, ref int errorCode, ref string errorMsg)
        {
            var sdkinput = input.GetSdk();
            //SDK异常错误记日志
            var tempMsg = string.Empty;
            if (SdkService.DeleteGroupActivity(sdkinput, ref errorCode, ref tempMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = tempMsg;
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
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
        public static AntSdkGetGroupActivityParticipatorsOutput GetGroupActivityParticipators(AntSdkGetGroupActivityParticipatorInput input, ref int errorCode, ref string errorMsg)
        {
            //SDK异常错误记日志
            var tempMsg = string.Empty;
            var sdkintput = input.GetSdk();
            var sdkoutput = SdkService.GetGroupActivityParticipators(sdkintput, ref errorCode, ref tempMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetGroupActivityParticipatorsOutput
            {
                pageNum = sdkoutput.pageNum,
                pageSize = sdkoutput.pageSize,
                total = sdkoutput.total
            };
            if (sdkoutput.list.Count > 0)
            {
                antsdkoutput.list.AddRange(
                    sdkoutput.list.Where(m => m != null).Select(m => new AntSdkGroupActivityParticipator
                    {
                        userId = m.userId,
                        userName = m.userName,
                        picture = m.picture,
                        userNum = m.userNum
                    }));
            }
            //错误返回
            errorMsg = tempMsg;
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, tempMsg);
            return antsdkoutput;
        }

        /// <summary>
        /// 提交参与活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool ParticipateActivities(AntSdkParticipateActivitiesInput input, ref int errorCode, ref string errorMsg)
        {
            var sdkinput = input.GetSdk();
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (SdkService.ParticipateActivities(sdkinput, ref errorCode, ref errMsg))
            {
                return true;
            }
            //错误返回
            errorMsg = errMsg;
            return false;

        }

        /// <summary>
        /// 获取活动默认主题图片
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static AntSdkGetGroupActivityDefaultImageOutput GetActivityImages(ref int errorCode, ref string errorMsg)
        {

            //SDK异常错误记日志
            var errMsg = string.Empty;
            var sdkoutput = SdkService.GetActivityImages(ref errorCode, ref errMsg);
            if (sdkoutput == null)
                return null;
            var antsdkoutput = new AntSdkGetGroupActivityDefaultImageOutput
            {
                sing = sdkoutput.sing,
                dinner = sdkoutput.dinner,
                game = sdkoutput.game,
                outdoor = sdkoutput.outdoor,
                sport = sdkoutput.sport
            };
            //错误返回
            errorMsg = errMsg;
            //errorMsg = FormatErrorMsg(Resources.AntSdkGetUserInfoError, errMsg);
            return antsdkoutput;
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
        public static bool ConfirmVerify(string attendId, string userIp, string userId, string loginPwd, ref int errorCode, ref string errorMsg)
        {
            //SDK异常错误记日志
            var errMsg = string.Empty;
            if (_antsdkhttpService.ConfirmVerify(attendId, userIp, userId, loginPwd, ref errorCode, ref errMsg))
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
        public static AntSdkGetPunchClocksOutput GetPunchClocks(string userId, int pageNum, int pageSize, ref int errorCode, ref string errorMsg)
        {
            //SDK异常错误记日志
            var errMsg = string.Empty;
            var input = new AntSdkGetPunchClocksInput
            {
                userId = userId,
                pageNum = pageNum,
                pageSize = pageSize
            };
            var antsdkoutput = new AntSdkGetPunchClocksOutput();
            var output = _antsdkhttpService.GetPunchClocks(input, ref antsdkoutput, ref errorCode, ref errMsg);
            //错误返回
            errorMsg = errMsg;
            if (antsdkoutput != null && antsdkoutput.list.Count > 0) return antsdkoutput;
            //var antsdkoutput = new AntSdkGetPunchClocksOutput
            //{
            //    isFirstPage = output.isFirstPage,
            //    isLastPage = output.isLastPage,
            //    pageNum = output.pageNum,
            //    pageSize = output.pageSize,
            //    total = output.total
            //};
            //if (output.list != null && output.list.Count > 0)
            //{
            //    antsdkoutput.list.AddRange(output.list.Where(m => m != null).Select(m => new SDK.AntSdk.AntModels.PunchClockDetail
            //    {
            //        userIP = m.userIP,
            //        attendId = m.attendId,
            //        configId=m.configId,
            //        userId = m.userId,
            //        attendTime = m.attendTime,
            //        address = m.address,
            //        latitude = m.latitude,
            //        longItude = m.longItude,
            //        signTime = m.signTime,
            //        remark = m.remark,
            //        status = m.status
            //    }));
            //}
            return null;
        }
        #endregion
        /// <summary>
        /// 方法说明：用户修改公告状态为已读
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="groupId">群组ID</param>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功更改公告的状态为已读</returns>
        public static bool UpdateNotificationsState(string userId, string groupId, string notificationId,
            ref int errorCode, ref string errorMsg)
        {
            return SdkService.UpdateNotificationsState(userId, groupId, notificationId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：删除群公告(群主才有权限)
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="notificationId">公告ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功删除公告</returns>
        public static bool DeleteNotificationsById(string userId, string notificationId, ref int errorCode, ref string errorMsg)
        {
            return SdkService.DeleteNotificationsById(userId, notificationId, ref errorCode, ref errorMsg);
        }

        /// <summary>
        /// 方法说明：消息漫游
        /// 完成时间：2017-04-20
        /// </summary>
        /// <param name="input">消息漫游输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns>成功漫游的消息数组</returns>
        public static AntSdkRoamMessageOutput[] RoamMessage(AntSdkRoamMessageInput input, ref int errorCode, ref string errorMsg)
        {
            var sdk = input.GetSdk();
            var sdkoutput = SdkService.RoamMessage(sdk, ref errorCode, ref errorMsg);
            if (sdkoutput == null || sdkoutput.Length == 0)
            {
                return null;
            }
            var antsdkopt = new List<AntSdkRoamMessageOutput>();
            antsdkopt.AddRange(sdkoutput.Where(c => c != null).Select(c => new AntSdkRoamMessageOutput
            {
                messageId = c.messageId,
                sendUserId = c.sendUserId,
                targetId = c.targetId,
                appKey = c.appKey,
                content = c.content,
                sendTime = c.sendTime,
                chatIndex = c.chatIndex,
                sessionId = c.sessionId
            }));
            return antsdkopt.ToArray();
        }

        /// <summary>
        /// 方法说明：文件上传
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="scid">文件上传信息</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>上传数据信息</returns>
        public static AntSdkFileUpLoadOutput FileUpload(AntSdkSendFileInput scid, ref int errorCode, ref string errorMsg)
        {
            var sdkinput = scid.GetSdk();
            var sdkoutput = SdkService.FileUpload(sdkinput, ref errorCode, ref errorMsg);
            var antsdkopt = AntSdkFileUpLoadOutput.GetAntSdk(sdkoutput);
            return antsdkopt;
        }

        /// <summary>
        /// 方法说明：文件上传MD5校验
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="msgMd5">MD5信息</param>
        /// <param name="fileName">文件物理路径</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>MD5校验信息</returns>
        public static AntSdkFileUpLoadOutput CompareFileMd5(string msgMd5, string fileName, ref int errorCode, ref string errorMsg)
        {
            var sdkoutput = SdkService.CompareFileMd5(msgMd5, fileName, ref errorCode, ref errorMsg);
            var antsdkopt = AntSdkFileUpLoadOutput.GetAntSdk(sdkoutput);
            return antsdkopt;
        }
        /// <summary>
        /// 获取系统当前时间[GET]
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        public static AntSdkQuerySystemDateOuput AntSdkGetCurrentSysTime(ref int errorCode, ref string errorMsg)
        {
            AntSdkQuerySystemDateOuput antsdkoutput = null;
            var sdkoutput = SdkService.GetCurrentSysTime(ref errorCode, ref errorMsg);
            if (sdkoutput != null)
                antsdkoutput = new AntSdkQuerySystemDateOuput() { systemCurrentTime = sdkoutput.systemCurrentTime };
            return antsdkoutput;
        }
        #endregion
        #region 忘记密码发送短信

        #endregion
    }
}
