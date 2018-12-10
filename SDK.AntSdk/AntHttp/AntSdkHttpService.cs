using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using SDK.AntSdk.AntModels;
using SDK.Service;
using SDK.AntSdk.AntModels.Http;

namespace SDK.AntSdk
{
    internal class AntSdkHttpService
    {
        public AntSdkHttpService()
        {
            if (AntSdkService.MdAntsdkhttpMethod == null)
            {
                AntSdkService.MdAntsdkhttpMethod = new AntSdkHttpMethod();
            }
        }

        public event EventHandler AntSdkTokenErrorEvent;

        #region Http服务类通用方法

        /// <summary>
        /// 触角业务HTTP请求统一入口
        /// </summary>
        /// <typeparam name="TIn">输入类型约束</typeparam>
        /// <typeparam name="TOut">输出类型约束</typeparam>
        /// <param name="methodName"></param>
        /// <param name="requestType">HTTP请求类型</param>
        /// <param name="input">输入参数</param>
        /// <param name="output">输出参数</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <param name="customersRequest">客户系统请求</param>
        /// <returns>是否成功</returns>
        public bool AntSdkHttpCommonMethod<TIn, TOut>(string methodName,
            TIn input, ref TOut output, ref int errorCode, ref string errorMsg, RequestMethod requestType, bool customersRequest = false)
        {
            var antsdkhttpCommon = new AntSdkHttpCommon<TIn, TOut>();
            if (AntSdkTokenErrorEvent != null)
            {
                antsdkhttpCommon.AntSdkTokenErrorEvent += (s, e) =>
                    AntSdkTokenErrorEvent.Invoke(s, e);
            }
            //调用公共方法处理
            return antsdkhttpCommon.AntSdkHttpCommonMethod(methodName, input, ref output, ref errorCode, ref errorMsg, requestType, customersRequest);
        }

        #endregion

        /// <summary>
        /// 1、 APP检查更新[GET]:v1/{companyCode}/{appKey}/app?os={os}
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckUpgrade(ref AntSdkUpgradeOutput output, ref int errorCode, ref string errorMsg)
        {
            var inputParam = new AntSdkUpgradeInput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.CheckUpdate, inputParam, ref output,
                ref errorCode, ref errorMsg, RequestMethod.GET);
        }

        /// <summary>
        /// 2、 触角客户端登录（B端系统接口）
        /// </summary>
        /// <param name="input">登录名</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>返回Token</returns>
        public string CustomerLogin(AntSdkLoginInput input, ref int errorCode, ref string errorMsg)
        {
            var outputToken = string.Empty;
            var result = AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.BsytemLogin, input, ref outputToken, ref errorCode, ref errorMsg, RequestMethod.POST, true);
            return result ? outputToken : string.Empty;
        }
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public string GetVerifyCodeImage(string mobile, ref int errorCode, ref string errorMsg)
        {
            var outputImageUrl = AntSdkService.AntSdkConfigInfo.CustomersHttpPrdfix + string.Format(AntSdkService.MdAntsdkhttpMethod.GetVerifyCodeImage, mobile);
            return outputImageUrl;
        }

        /// <summary>
        /// 3、 获取用户信息[GET]：v1/core/{companyCode}/{appKey}/user?targetId={targetId}  version={version}
        /// </summary>
        /// <param name="input">获取用户信息输入</param>
        /// <param name="output">获取用户信息输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errMsg">错误提示</param>
        /// <returns>是否获取成功</returns>
        public bool GetUserInfo(AntSdkGetUserInput input, ref AntSdkUserInfo output, ref int errorCode, ref string errMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.GetUserInfo,
                input, ref output, ref errorCode, ref errMsg, RequestMethod.GET);
        }

        /// <summary>
        /// 4、 用户退出登录（B端系统接口）[POST]：v1/account/logout
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CustomerLoginOut(ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod<object, AntSdkBaseOutput>(AntSdkService.MdAntsdkhttpMethod.BsystemLoginOut, null, ref output,
                ref errorCode, ref errorMsg, RequestMethod.POST, true);
        }

        /// <summary>
        /// 5、 切换用户状态（在线、离开、忙碌等）[PUT]：v1/core/{companyCode}/{appKey}/userState
        /// </summary>
        /// <param name="input">切换用户状态输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功切换</returns>
        public bool ChangeUserState(AntSdkChangeUserStateInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(
                AntSdkService.MdAntsdkhttpMethod.ChangeUserState, input, ref output,
                ref errorCode, ref errorMsg, RequestMethod.POST, true);
        }

        /// <summary>
        /// 6、 获取当前用户信息[GET]： v1/core/{companyCode}/{appKey}/currentUserInfo
        /// </summary>
        /// <param name="input">获取当前用户信息输入</param>
        /// <param name="output">当前用户信息</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetCurrentUserInfo(AntSdkUserInfoInput input, ref AntSdkUserInfo output, ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod<object, AntSdkUserInfo>(AntSdkService.MdAntsdkhttpMethod.GetCurrentUserInfo, input,
                ref output, ref errorCode, ref errorMsg, RequestMethod.GET);
        }

        /// <summary>
        /// 7、 获取部门下所有用户在线状态
        /// </summary>
        /// <param name="input">获取部门下所有用户在线状态输入</param>
        /// <param name="output">获取部门下所有用户在线状态输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetDepartmentUserState(AntSdkGetUserStateInput input, ref List<AntSdkGetUserStateOutput> output,
            ref int errorCode, ref string errorMsg)
        {
            return
                AntSdkHttpCommonMethod(
                    AntSdkService.MdAntsdkhttpMethod.GetUserState, input, ref output, ref errorCode, ref errorMsg, RequestMethod.GET);
        }

        /// <summary>
        /// 8、 更新用户信息[PUT]：v1/core/{companyCode}/{appKey}/user
        /// </summary>
        /// <param name="input">更新用户输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">提示信息</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateUser(AntSdkUpdateUserInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.UpdateUserInfo,
                input, ref output, ref errorCode, ref errorMsg, RequestMethod.PUT);
        }

        /// <summary>
        /// 9、 获取用户回复用语设置[GET]：v1/core/{companyCode}/{appKey}/userConfig?configType={configType}version={version}
        /// </summary>
        /// <param name="input">取用户回复用语设置输入</param>
        /// <param name="output">取用户回复用语设置输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取</returns>
        public bool GetUserReturnSetting(AntSdkGetUserReturnSettingInput input,
            ref List<AntSdkGetUserReturnSettingOutput> output, ref int errorCode, ref string errorMsg)
        {
            return
                AntSdkHttpCommonMethod(
                    AntSdkService.MdAntsdkhttpMethod.GetUserReturnSetting,
                    input, ref output, ref errorCode, ref errorMsg, RequestMethod.GET);
        }

        /// <summary>
        /// 10、 更新用户系统设置[PUT]： v1/core/{companyCode}/{appKey}/userConfig
        /// </summary>
        /// <param name="input">更新用户系统设置输入</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateUserSystemSetting(AntSdkUpdateUserSystemSettingInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.UpdateUserSystemSetting, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.PUT);
        }

        /// <summary>
        /// 11、 获取意见反馈类型[GET]：v1/core/{companyCode}/{appKey}/suggestTypes?version={version}
        /// </summary>
        /// <param name="input">获取意见反馈类型输入</param>
        /// <param name="output">获取意见反馈类型输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取意见反馈类型</returns>
        public bool GetUserIdeaType(AntSdkGetUserIdeaTypeInput input, ref List<AntSdkGetUserIdeaTypeOutput> output,
            ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.UpdateUserSystemSetting, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.PUT);
        }

        /// <summary>
        /// 12、 添加意见反馈[POST]： v1/core/{companyCode}/{appKey}/feedback
        /// </summary>
        /// <param name="input">添加意见反馈输入</param>
        /// <param name="errorMsg">添加意见反馈输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <returns>是否添加成功</returns>
        public bool AddIdeaFeedBack(AntSdkAddIdeaFeedBackInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.AddIdeaFeedBack, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.POST);
        }

        /// <summary>
        /// 13、 修改密码[PUT]：v1/user/{userId}/account
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ChangePassword(AntSdkChangePasswordInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return
                AntSdkHttpCommonMethod(
                    string.Format(AntSdkService.MdAntsdkhttpMethod.ChangePassWord, AntSdkService.AntSdkLoginOutput.userId), input,
                    ref output, ref errorCode, ref errorMsg,
                    RequestMethod.POST);
        }

        /// <summary>
        /// 14、 获取表情[GET]：v1/core/{companyCode}/{appKey}/expression?version={version}
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetFaceInfo(AntSdkGetFaceInfoInput input, ref List<AntSdkGetFaceInfoOutput> output,
            ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.GetFaceInfo, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.PUT);
        }

        /// <summary>
        /// 15、 获取联系人信息，返回数组格式
        /// </summary>
        /// <param name="input">获取联系人输入</param>
        /// <param name="output">获取联系人输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否成功获取</returns>
        public bool GetListContacts(AntSdkListContactsInput input, ref AntSdkListContactsOutput output, ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.GetAllContacts, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.GET);
        }

        /// <summary>
        /// 16、 获取联系人信息—增量信息(返回值区分组织架构变化和不变化两种情况)
        /// </summary>
        /// <param name="input">获取联系人信息增量信息输入</param>
        /// <param name="output">获取联系人信息增量信息输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否获取成功</returns>
        public bool GetAddContacts(AntSdkAddListContactsInput input, ref AntSdkAddListContactsOutput output, ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.GetAddContacts, input, ref output, ref errorCode, ref errorMsg,
                RequestMethod.GET);
        }
        /// <summary>
        /// 17、获取当前用的公司信息
        /// </summary>
        /// <param name="output">获取当前用的公司信息输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>是否返回成功</returns>
        public bool GetCompanyInfo(ref AntSdkGetCompayInfoOutput output, ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod<object, AntSdkGetCompayInfoOutput>(AntSdkService.MdAntsdkhttpMethod.GetCompanyInfo, null,
                ref output, ref errorCode, ref errorMsg, RequestMethod.GET, true);
        }
        /// <summary>
        /// 18、获取所有用户的状态
        /// </summary>
        /// <param name="output">所有在线用户状态输出</param>
        /// <param name="users">用户信息</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetUserStateList(ref List<AntSdkUserStateOutput> output, string[] users, ref int errorCode, ref string errorMsg)
        {
            var input = new AntSdkUserIdsStateInput
            {
                userIds = users
            };
            return AntSdkHttpCommonMethod<object, List<AntSdkUserStateOutput>>(AntSdkService.MdAntsdkhttpMethod.GetUserStateList, input,
                ref output, ref errorCode, ref errorMsg, RequestMethod.POST);
        }

        /// <summary>
        /// 19、更新当前用户状态
        /// </summary>
        /// <param name="state">当前用户状态</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool UpdateCurrentUserState(int state, ref int errorCode, ref string errorMsg)
        {
            var input = new AntSdkUserStateInput { state = state };
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.UpdateCurrentUserState, input,
                ref output, ref errorCode, ref errorMsg, RequestMethod.PUT);
        }
        #region 打卡
        /// <summary>
        /// 确认打卡
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ConfirmVerify(string attendId, string userIp, string userId, string loginPwd, ref int errorCode, ref string errorMsg)
        {
            var input = new AntSdkConfirmPunchInput
            {
                attendId = attendId,
                userIp = userIp,
                userId = userId,
                passwd = loginPwd
            };
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.ConfirmVerify, input,
                ref output, ref errorCode, ref errorMsg, RequestMethod.POST);
        }
        /// <summary>
        /// 发布活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output">打卡记录数据</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetPunchClocks(AntSdkGetPunchClocksInput input, ref AntSdkGetPunchClocksOutput output, ref int errorCode, ref string errorMsg)
        {
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.GetPunchClocks, input,
                ref output, ref errorCode, ref errorMsg, RequestMethod.GET);
        }
        #endregion


        /// <summary>
        /// 6、 获取短信验证码[GET]： /v1/forgotPassword/mobile/{mobile}/sendMsg
        /// </summary>
        /// <param name="input">获取当前用户信息输入</param>
        /// <param name="output">当前用户信息</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetVerifyCodeInfo(string input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod<object, AntSdkBaseOutput>(string.Format(AntSdkService.MdAntsdkhttpMethod.GetVerifyCode, input), null, ref output,
                ref errorCode, ref errorMsg, RequestMethod.GET);
        }
        public bool SendVerifyCode(AntSdkSendVerifyCodeInput input, ref int errorCode, ref string errorMsg, ref string data)
        {
            return AntSdkHttpCommonMethod<object, string>(AntSdkService.MdAntsdkhttpMethod.SendVerifyCode, input, ref data,
           ref errorCode, ref errorMsg, RequestMethod.POST);
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ResetPassWord(AntSdkResetPassWoldInput input, ref int errorCode, ref string errorMsg)
        {
            var output = new AntSdkBaseOutput();
            return AntSdkHttpCommonMethod(AntSdkService.MdAntsdkhttpMethod.ResetPassword, input, ref output,
           ref errorCode, ref errorMsg, RequestMethod.POST);
        }
        /// <summary>
        /// 创建讨论组[平台SDK提供]
        /// 获取讨论组信息[平台SDK提供]
        /// 获取讨论组成员信息[平台SDK提供]
        /// 退出讨论组[平台SDK提供]
        /// 删除讨论组[平台SDK提供]
        /// 更新讨论组信息[平台SDK提供]
        /// 群主转让[平台SDK提供]
        /// 更新讨论组设置[平台SDK提供]
        /// 消息查询(讨论组)[平台SDK提供]
        /// 添加公告[平台SDK提供]
        /// 删除公告[平台SDK提供]
        /// 修改已读公告状态[平台SDK提供]
        /// 查询公告列表[平台SDK提供]
        /// 根据通知ID查询公告详情[平台SDK提供]
        /// 文件上传[平台SDK提供]
        /// 文件上传MD5[平台SDK提供]
        /// </summary>
        private void GetMethodInfo()
        {
        }
    }

    /// <summary>
    /// AntSdk_Http方法名
    /// </summary>
    internal class AntSdkHttpMethod
    {

        /// <summary>
        /// 1、 APP检查更新[GET]： v1/app?os={os}
        /// </summary>
        public string CheckUpdate = $"v1/app";

        /// <summary>
        /// 2、 触角客户端登录（B端系统接口）[POST]：v1/ant/client/account/login
        /// </summary>
        //public string BsytemLogin = $"v1/ant/client/account/login";
        public string BsytemLogin = $"v2/ant/client/account/login";

        /// <summary>
        /// 获取防刷验证码图片
        /// </summary>
        public string GetVerifyCodeImage = "v1/verifyCode/mobile/{0}/image";

        /// <summary>
        /// 3、 获取用户信息[GET]：v1/core/{companyCode}/{appKey}/user?userId={userId}&targetId={targetId}&version={version}
        /// </summary>
        public string GetUserInfo =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/user";

        /// <summary>
        /// 4、 用户退出登录（B端系统接口）[POST]：v1/account/logout
        /// </summary>
        public string BsystemLoginOut = $"v1/account/logout";

        /// <summary>
        /// 5、 切换用户状态（在线、离开、忙碌等）[PUT]：v1/core/{companyCode}/{appKey}/userState
        /// </summary>
        public string ChangeUserState =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/userState";

        /// <summary>
        /// 6、 获取当前用户信息[GET]：v1/core/{companyCode}/{appKey}/currentUserInfo
        /// </summary>
        public string GetCurrentUserInfo =
            $"v1/core/currentUserInfo";

        /// <summary>
        /// 7、 获取部门下所有用户的在线状态[GET]：v1/core/{companyCode}/{appKey}/listUsersState?userId={userId}&departmentId={departmentId}&version={version}
        /// </summary>
        public string GetUserState =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/listUsersState";

        /// <summary>
        /// 8、 更新用户信息[PUT]：v1/core/{companyCode}/{appKey}/user
        /// </summary>
        public string UpdateUserInfo =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/user";

        /// <summary>
        /// 9、 获取用户回复用语设置[GET]：v1/core/{companyCode}/{appKey}/userConfig?userId={userId}&configType={configType}&version={version}
        /// </summary>
        public string GetUserReturnSetting =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/userConfig";

        /// <summary>
        /// 10、 更新用户系统设置[PUT]：v1/core/{companyCode}/{appKey}/userConfig
        /// </summary>
        public string UpdateUserSystemSetting =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/userConfig";

        /// <summary>
        /// 11、 获取意见反馈类型[GET]：v1/core/{companyCode}/{appKey}/suggestTypes?userId={userId}&version={version}
        /// </summary>
        public string GetUserIdeaType =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/suggestTypes";

        /// <summary>
        /// 12、 添加意见反馈[POST]：v1/core/{companyCode}/{appKey}/feedback
        /// </summary>
        public string AddIdeaFeedBack =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/feedback";

        /// <summary>
        /// 13、 修改密码[PUT]：v1/user/{userId}/account
        /// </summary>
        public string ChangePassWord = @"v1/core/updatePassword";

        /// <summary>
        /// 14、 验证B端用户token是否有效(内部接口系统)[GET]：{internal}/v1/user/check/token/b/userinfo
        /// </summary>
        public string BsystemTokenVeryfy = $"{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/v1/user/check/token/b/userinfo";

        /// <summary>
        /// 15、 获取表情[GET]：v1/core/{companyCode}/{appKey}/expression?userId={userId}&version={version}
        /// </summary>
        public string GetFaceInfo =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/expression";

        /// <summary>
        /// 16、 获取联系人信息，返回数组格式[GET]：v1/core/{companyCode}/{appKey}/listContacts?userId={userId}&version={version}
        /// </summary>
        public string GetAllContacts =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/listContacts";

        /// <summary>
        /// 17、 获取联系人信息—-增量信息(返回值区分组织架构变化和不变化两种情况)[GET]：v1/core/{companyCode}/{appKey}/incrementContacts?userId={userId}&version={version}&dataVersion={dataVersion}
        /// </summary>
        public string GetAddContacts =
            $"v1/core/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}/{AntSdkService.AntSdkConfigInfo.AntSdkAppKey}/incrementContacts";

        /// <summary>
        /// 18、 更新联系人缓存(触角后台用)[POST]： v1/user/contact/cache
        /// </summary>
        public string UpdateContacts = $"v1/user/contact/cache";

        /// <summary>
        /// 19、 获取云信用户接口[GET]：v1/netEase/userInfo
        /// </summary>
        public string GetCloundUserInfo = $"v1/netEase/userInfo";

        /// <summary>
        /// 20、更新云信用户信息[PUT]：v1/netEase/userInfo
        /// </summary>
        public string UpdateCloundUserInfo = $"v1/netEase/userInfo";

        /// <summary>
        /// 21、获取公司信息[GET]:/v1/company/{companyCode}
        /// </summary>
        public string GetCompanyInfo = $"v1/company/{AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode}";

        /// <summary>
        /// 22、获取所有用户的状态[POST]
        /// </summary>
        public string GetUserStateList = $"v1/core/userState/list";
        /// <summary>
        /// 23、更新当前用户状态[PUT]
        /// </summary>
        public string UpdateCurrentUserState = $"v1/core/currentUserState";
        /// <summary>
        /// 24、忘记密码发送短信[GET]
        /// </summary>
        public string GetVerifyCode = "/v1/forgotPassword/mobile/{0}/sendMsg";
        /// <summary>
        /// 25、忘记密码检验手机验证码[POST]
        /// </summary>
        public string SendVerifyCode = $"/v1/forgotPassword/checkMobile";
        /// <summary>
        /// 26、重置密码[POST]
        /// </summary>
        public string ResetPassword = "/v1/forgotPassword/setPassword";

        /// <summary>
        /// 确认打卡[POST]
        /// </summary>
        public string ConfirmVerify = $"v1/core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/attend/confirm";

        /// <summary>
        /// 获取打卡记录列表[GET]
        /// </summary>
        public string GetPunchClocks = $"v1/core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/attends";
    }
}
