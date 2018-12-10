using Antenna.Framework.Properties;
using Antenna.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using static Antenna.Framework.GlobalVariable;

namespace Antenna.Framework
{
    public class HttpService
    {
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Resources));
        public static event EventHandler TokenErrorEvent;
        #region 单例模式（线程安全）
        //private volatile static HttpService _instance = null;
        //private static readonly object lockObj = new object();
        //private HttpService() { }
        //public static HttpService Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            lock (lockObj)
        //            {
        //                if (_instance == null)
        //                    _instance = new HttpService();
        //            }
        //        }
        //        return _instance;
        //    }
        //}
        #endregion

        #region Http服务类通用方法
        /// <summary>
        /// 调用HTTP服务的Post方法
        /// </summary>
        /// 作者：赵雪峰 20160520
        /// <param name="url">传入URL</param>
        /// <param name="msg">接口返回的JSON字符串或错误信息</param>
        /// <returns>是否执行成功</returns>
        //public bool PostMethod<Tin, Tout>(string methodName, Tin input, ref Tout output, ref string errMsg)
        //{
        //    string strURL = string.Empty;
        //    string paramData = string.Empty;
        //    try
        //    {
        //        Stopwatch stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        //DataConverter.SerializeJson(input, ref paramData, ref errMsg);
        //        paramData = FormatInputEntity(input);
        //        strURL = GlobalVariable.ConfigEntity.HttpPrdfix + methodName;
        //        // 创建一个HTTP请求
        //        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
        //        request.Method = "post";
        //        request.ContentType = "application/x-www-form-urlencoded;charset=utf-8"; 
        //        byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
        //        request.ContentLength = byteArray.Length; 
        //         Stream newStream = request.GetRequestStream();
        //        newStream.Write(byteArray, 0, byteArray.Length);//写入参数
        //        newStream.Close();

        //        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
        //        System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //        string responseText = myreader.ReadToEnd();
        //        myreader.Close();
        //        response.Close();
        //        //msg = responseText;
        //        bool getReturnInfo = DataConverter.DeserializeJson<Tout>(responseText, ref output, ref errMsg);
        //        if (!getReturnInfo)
        //        {
        //            return false;
        //        }
        //        stopWatch.Stop();
        //        if (methodName != "/antuser/user.listContacts.do")//获取组织结构数据量太大， 不在这里记日志
        //        {
        //            LogHelper.WriteDebug(string.Format("[HTTPService.PostMethod({0}毫秒)]:{1}", stopWatch.Elapsed.TotalMilliseconds, strURL + "?"+paramData+ Environment.NewLine + responseText));
        //        }
        //        string errCodeStr = string.Empty;
        //        if (DataConverter.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errMsg)
        //            && errCodeStr == "1004"
        //            && TokenErrorEvent != null)//获取errorCode
        //        {
        //            if (TokenErrorEvent != null) TokenErrorEvent(null, null);

        //        }
        //        return true;
        //    }
        //    catch (WebException e)
        //    {

        //        string status = null;
        //        HttpWebResponse response = (HttpWebResponse)e.Response;
        //        System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //        string responseText = myreader.ReadToEnd();
        //        myreader.Close();
        //        response.Close();
        //        if (response.StatusCode == HttpStatusCode.NotFound)
        //            status = "404";
        //        if (response.StatusCode == HttpStatusCode.InternalServerError)
        //            status = "500";
        //        if (response.StatusCode == HttpStatusCode.BadGateway)
        //            status = "502";

        //        errMsg = "访问后台服务失败";
        //        if (methodName == "/antuser/user.login.do")//登录接口，日志不记录密码
        //        {
        //            LogHelper.WriteError("[HTTPService.PostMethod]:" + GlobalVariable.ConfigEntity.HttpPrdfix + methodName + Environment.NewLine + e.Message + "," + e.StackTrace);
        //        }
        //        else
        //        {
        //            LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + Environment.NewLine + e.Message + "," + e.StackTrace);
        //        }
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="Tin"></typeparam>
        ///// <typeparam name="Tout"></typeparam>
        ///// <param name="methodName"></param>
        ///// <param name="input"></param>
        ///// <param name="output"></param>
        ///// <param name="errMsg"></param>
        ///// <returns></returns>
        //public bool GetMethod<Tin, Tout>(string methodName, Tin input, ref Tout output, ref string errMsg)
        //{
        //    string strURL = string.Empty;
        //    string paramData = string.Empty;
        //    try
        //    {
        //        Stopwatch stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        //DataConverter.SerializeJson(input, ref paramData, ref errMsg);
        //        paramData = FormatInputEntity(input);
        //        strURL = GlobalVariable.ConfigEntity.HttpPrdfix + methodName;
        //        // 创建一个HTTP请求
        //        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(strURL+"?"+ paramData);
        //        request.Method = "GET";
        //        request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

        //        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
        //        System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //        string responseText = myreader.ReadToEnd();
        //        myreader.Close();
        //        response.Close();
        //        bool getReturnInfo = DataConverter.DeserializeJson<Tout>(responseText, ref output, ref errMsg);
        //        if (!getReturnInfo)
        //        {
        //            return false;
        //        }
        //        stopWatch.Stop();
        //        if (methodName != "/antuser/user.listContacts.do")//获取组织结构数据量太大， 不在这里记日志
        //        {
        //            LogHelper.WriteDebug(string.Format("[HTTPService.PostMethod({0}毫秒)]:{1}", stopWatch.Elapsed.TotalMilliseconds, strURL + "?" + paramData + Environment.NewLine + responseText));
        //        }
        //        string errCodeStr = string.Empty;
        //        if (DataConverter.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errMsg)
        //            && errCodeStr == "1004"
        //            && TokenErrorEvent != null)//获取errorCode
        //        {
        //            if (TokenErrorEvent != null) TokenErrorEvent(null, null);

        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        errMsg = "访问后台服务失败";
        //        if (methodName == "/antuser/user.login.do")//登录接口，日志不记录密码
        //        {
        //            LogHelper.WriteError("[HTTPService.PostMethod]:" + GlobalVariable.ConfigEntity.HttpPrdfix + methodName + Environment.NewLine + e.Message + "," + e.StackTrace);
        //        }
        //        else
        //        {
        //            LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + Environment.NewLine + e.Message + "," + e.StackTrace);
        //        }
        //        return false;
        //    }
        //}

        /// <summary>
        /// HTTP请求统一入口
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool HttpCommonMethod<Tin, Tout>(string methodName,RequestMethod requestType, Tin input, ref Tout output, ref string errMsg)
        {
            string strURL = string.Empty;
            string paramData = string.Empty;
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                paramData = FormatInputEntity(input);
                strURL = GlobalVariable.ConfigEntity.HttpPrdfix + methodName;

                System.Net.HttpWebRequest request = null;
                if (requestType.ToString().ToLower() == "post")
                {
                    // 创建一个HTTP请求
                    request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
                    request.Method = requestType.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                    request.ContentLength = byteArray.Length;
                    Stream newStream = request.GetRequestStream();
                    newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                    newStream.Close();
                }
                else
                {
                    // 创建一个HTTP请求
                    request = (System.Net.HttpWebRequest)WebRequest.Create(strURL + "?" + paramData);
                    request.Method = requestType.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                }

                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string responseText = myreader.ReadToEnd();
                myreader.Close();
                response.Close();
                //msg = responseText;
                bool getReturnInfo = DataConverter.DeserializeJson<Tout>(responseText, ref output, ref errMsg);
                if (!getReturnInfo)
                {
                    return false;
                }
                stopWatch.Stop();
                if (methodName != "/antuser/user.listContacts.do")//获取组织结构数据量太大， 不在这里记日志
                {
                    LogHelper.WriteDebug(string.Format("[HTTPService.PostMethod({0}毫秒)]:{1}", stopWatch.Elapsed.TotalMilliseconds, strURL + "?" + paramData + Environment.NewLine + responseText));
                }
                string errCodeStr = string.Empty;
                if (DataConverter.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errMsg)
                    && errCodeStr == "1004"
                    && TokenErrorEvent != null)//获取errorCode
                {
                    if (TokenErrorEvent != null) TokenErrorEvent(null, null);

                }
                return true;
            }
            catch (WebException e)
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse) e.Response;
                    System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(),
                        Encoding.UTF8);
                    string responseText = myreader.ReadToEnd();
                    myreader.Close();
                    response.Close();
                    string errCodeStr = string.Empty;
                    if (DataConverter.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errMsg)
                        && errCodeStr == "1004"
                        && TokenErrorEvent != null) //获取errorCode
                    {
                        if (TokenErrorEvent != null) TokenErrorEvent(null, null);
                    }
                    errMsg = "访问后台服务失败";
                    if (methodName == "/antuser/user.login.do") //登录接口，日志不记录密码
                    {
                        LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + "?" + paramData +
                                             Environment.NewLine + e.Message + "," + responseText);
                    }
                    else
                    {
                        LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + Environment.NewLine + e.Message +
                                             "," + responseText);
                    }
                }
                catch
                {
                    errMsg = "访问后台服务失败";
                    if (methodName == "/antuser/user.login.do")//登录接口，日志不记录密码
                    {
                        LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + "?" + paramData + Environment.NewLine + e.Message);
                    }
                    else
                    {
                        LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + Environment.NewLine + e.Message + e.StackTrace);
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                errMsg = "访问后台服务失败";
                if (methodName == "/antuser/user.login.do")//登录接口，日志不记录密码
                {
                    LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + "?" + paramData + Environment.NewLine + e.Message);
                }
                else
                {
                    LogHelper.WriteError("[HTTPService.PostMethod]:" + strURL + Environment.NewLine + e.Message+e.StackTrace);
                }
                return false;
            }
        }

        /// <summary>
        /// 格式化输入参数类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string FormatInputEntity<T>(T entity)
        {
            string tStr = string.Empty;
            if (entity == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties = entity.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(entity, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    tStr += string.Format("&{0}={1}", name, value);
                }
                else
                {
                    FormatInputEntity(value);
                }
            }
            tStr =  tStr.TrimStart('&');
            return tStr;
        }
        #endregion

        /// <summary>
        /// 登录接口
        /// </summary>
        /// 作者：赵雪峰 20160906
        public bool Login(LoginInput input, ref LoginOutput output, ref string errMsg)
        {
            bool doLogin = HttpCommonMethod<LoginInput, LoginOutput>("/antuser/user.login.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (doLogin && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "登录失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doLogin;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool ChangePassword(ChangePasswordInput input,ref ChangePasswordOutput output,ref string errMsg)
        {
            bool doChange = HttpCommonMethod<ChangePasswordInput, ChangePasswordOutput>("/antuser/user.resetPassword.do", GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (doChange && output.result == (int) GlobalVariable.Result.Failure)
            {
                errMsg=string.IsNullOrEmpty(output.errorCode) ? "修改密码失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doChange;
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool LoginOut(LoginOutInput input, ref BaseOutput output, ref string errMsg)
        {
            bool doLoginOut = HttpCommonMethod<LoginOutInput, BaseOutput>("/antuser/user.loginOut.do", GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (doLoginOut && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "注销登录失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doLoginOut;
        }
        /// <summary>
        /// 获取联系人信息，返回数组格式
        /// </summary>
        /// 作者：赵雪峰 20160913
        public bool ListContacts(ListContactsInput input, ref ListContactsOutput output, ref string errMsg)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bool doResult = HttpCommonMethod<ListContactsInput, ListContactsOutput>("/antuser/user.listContacts.do",GlobalVariable.RequestMethod .POST, input, ref output, ref errMsg);
            stopWatch.Stop();
            if (doResult)
            {
                LogHelper.WriteDebug(string.Format("[HTTPService.ListContacts({0}毫秒)]:获取组织结构成功--部门数{1},成员数{2}", stopWatch.Elapsed.TotalMilliseconds, output.contacts.departs.Count, output.contacts.users.Count));
            }
            if (doResult && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "获取组织结构失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doResult;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool GetUserInfo(UserInput input, ref UserOutput output, ref string errMsg)
        {
            bool doResult = HttpCommonMethod<UserInput, UserOutput>("/antuser/user.getUser.do",GlobalVariable.RequestMethod .POST, input, ref output, ref errMsg);
            if (doResult && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "获取用户信息失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doResult;
        }
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool UpdateUser(UpdateUserInput input, ref UpdateUserOut output, ref string errMsg)
        {
            bool doResult = HttpCommonMethod<UpdateUserInput, UpdateUserOut>("/antuser/user.updateUser.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (doResult && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "更新用户信息失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return doResult;
        }
        /// <summary>
        /// 创建讨论组
        /// </summary>
        public bool CreateGroup(CreateGroupInput input, ref CreateGroupOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<CreateGroupInput, CreateGroupOutput>("/antuser/group.createGroup.do",GlobalVariable.RequestMethod .POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "创建讨论组失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 获取讨论组信息
        /// </summary>
        public bool GetGroups(BaseInput input, ref GetGroupsOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<BaseInput, GetGroupsOutput>("/antuser/group.getGroups.do",GlobalVariable.RequestMethod .POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "获取讨论组信息失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 获取讨论组成员信息
        /// </summary>
        public bool GetGroupMembers(GetGroupMembersInput input, ref GetGroupMembersOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<GetGroupMembersInput, GetGroupMembersOutput>("/antuser/group.findGroupMembers.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "获取讨论组成员信息失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 退出讨论组
        /// </summary>
        public bool ExitGroup(ExitGroupInput input, ref BaseOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<ExitGroupInput, BaseOutput>("/antuser/group.exitGroup.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "退出讨论组失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }

        /// <summary>
        /// 退出讨论组
        /// </summary>
        public bool DeleteGroup(ExitGroupInput input, ref BaseOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<ExitGroupInput, BaseOutput>("/antuser/group.deleteGroup.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "解散讨论组失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }

        /// <summary>
        /// 更新讨论组信息
        /// </summary>
        public bool UpdateGroup(UpdateGroupInput input, ref BaseOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<UpdateGroupInput, BaseOutput>("/antuser/group.updateGroup.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "更新讨论组信息失败" : resources.GetString("E_" + output.errorCode);
                LogHelper.WriteError(errMsg);
                return false;
            }
            return result;
        }

        public bool ChangeGroupAdmin(ChangeGroupAdminIn input, ref ChangeGroupAdminOut output, ref string errMsg)
        {
            bool result = HttpCommonMethod<ChangeGroupAdminIn, ChangeGroupAdminOut>("/antuser/group.transferGroupManager.do", GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "管理员转让失败" : resources.GetString("E_" + output.errorCode);
                LogHelper.WriteError(errMsg);
                return false;
            }
            return result;
        }

        /// <summary>
        /// 更新讨论组设置
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool UpdateGroupConfig(UpdateGroupConfigInput input, ref BaseOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<UpdateGroupConfigInput, BaseOutput>("/antuser/group.updateGroupConfig.do",GlobalVariable.RequestMethod.POST, input, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "更新讨论组设置失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 消息查询(讨论组)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool QueryMsg(QueryMsgInput_Param input_param, ref BaseOutput output, ref string errMsg)
        {
            QueryMsgInput queryMsgInput = new QueryMsgInput();
            string param = string.Empty;
            if (DataConverter.SerializeJson(input_param, ref param, ref errMsg))
            {
                queryMsgInput.param = param;
                queryMsgInput.token = GlobalVariable.LoginOutput.token;
                bool result = HttpCommonMethod<QueryMsgInput, BaseOutput>("/antuser/message.queryMsg.do",GlobalVariable.RequestMethod.POST, queryMsgInput, ref output, ref errMsg);
                if (result && output.result == (int)GlobalVariable.Result.Failure)
                {
                    errMsg = string.IsNullOrEmpty(output.errorCode) ? "查询讨论组消息失败" : resources.GetString("E_" + output.errorCode);
                    return false;
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 1.检查更新
        /// </summary>
        /// <returns></returns>
        public bool Upgrade(UpgradeInput input_param, ref UpgradeOutput output, ref string errMsg)
        {
            bool result = HttpCommonMethod<UpgradeInput, UpgradeOutput>("/antuser/app.upgrade.do",GlobalVariable.RequestMethod.POST, input_param, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "检查更新失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 添加通知
        /// </summary>
        /// <param name="input_param"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool AddNotice(NoticeAddDto input_param, ref ReturnNoticeAddDto output, ref string errMsg)
        {
            bool result = HttpCommonMethod<NoticeAddDto, ReturnNoticeAddDto>("/notifications/group/add",GlobalVariable.RequestMethod.POST, input_param, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "添加通知失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="input_param"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool DeleteNotice(delateNoticeDto input_param, ref baseNotice output, ref string errMsg,RequestMethod method)
        {
            bool result = HttpCommonMethod<delateNoticeDto, baseNotice>("/notifications/group/delete", method,input_param, ref output, ref errMsg );
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "删除通知失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 修改已读通知状体
        /// </summary>
        /// <param name="input_param"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool ReadNotice(delateNoticeDto input_param,ref baseNotice output, ref string errMsg, RequestMethod method)
        {
            bool result = HttpCommonMethod<delateNoticeDto, baseNotice>("/notifications/group/update", method, input_param, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "删除通知失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 查询通知列表
        /// </summary>
        /// <param name="input_param"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool SearchNoticeList(inFindNoticeList input_param, ref ReturnNoticeList output, ref string errMsg)
        {
            bool result = HttpCommonMethod<inFindNoticeList, ReturnNoticeList>("/notifications/group/find",GlobalVariable.RequestMethod.GET, input_param, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "添加通知失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }
        /// <summary>
        /// 根据通知ID查询通知详情
        /// </summary>
        /// <param name="input_param"></param>
        /// <param name="output"></param>
        /// <param name="errMsg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool SearchNoticeDetailsByNoticeId(inFindDetailsNotice input_param,ref ReturnNoticeAddDto output,ref string errMsg, RequestMethod method)
        {
            bool result = HttpCommonMethod<inFindDetailsNotice, ReturnNoticeAddDto>("/notifications/group/get", method, input_param, ref output, ref errMsg);
            if (result && output.result == (int)GlobalVariable.Result.Failure)
            {
                errMsg = string.IsNullOrEmpty(output.errorCode) ? "添加通知失败" : resources.GetString("E_" + output.errorCode);
                return false;
            }
            return result;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="mainWindowParams"></param>
        /// <returns></returns>
        public T FileUpload<T>(SendCutImageDto scid)
        {
            try
            {
                string url = GlobalVariable.ConfigEntity.UpLoadAddress;
                string parm = string.Format("?&cmpcd={0}&seId={1}&fileFileName={2}", scid.cmpcd, scid.seId, scid.fileFileName);
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                byte[] postdata = System.Text.Encoding.UTF8.GetBytes(scid.file);
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] responseText = client.UploadFile(url + parm, "POST", scid.file);
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(responseText));
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[HTTPService.FileUpload]:" + GlobalVariable.ConfigEntity.UpLoadAddress + ex.Message + "," + ex.StackTrace);
                return default(T);
            }
        }
        /// <summary>
        /// 文件上传MD5
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public T compareFileMd5<T>(string msg,string fileName)
        {
            string url = string.Empty;
            try
            {
                url = ConfigurationManager.AppSettings["compareFileMd5"];
                string parm = string.Format("?&key={0}&requestTime={1}&fileMD5={2}&fileName={3}", 20000, (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, msg,fileName);
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                byte[] postdata = System.Text.Encoding.UTF8.GetBytes(url+parm);
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] responseText = client.UploadData(url, "POST", postdata);
                string ss = Encoding.UTF8.GetString(responseText);
                return JsonConvert.DeserializeObject<T>(ss);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[HTTPService.compareFileMd5]:" + url + ex.Message + "," + ex.StackTrace);
                return default(T);
            }
        }
        /// <summary>
        /// 上传接口2.0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="mainWindowParams"></param>
        /// <returns></returns>
        public T NoticeFileUpload<T>(string path)
        {
            string url = string.Empty;
            try
            {
                url = ConfigurationManager.AppSettings["NewUpLoadAddress"];
                string parm = string.Format("?&key={0}&requestTime={1}&File={2}", 20000, (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000, "file");
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] responseText = client.UploadFile(url + parm, "POST",path);
                string n = Encoding.UTF8.GetString(responseText);
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(responseText));
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[HTTPService.NoticeFileUpload]:" + url + ex.Message + "," + ex.StackTrace);
                return default(T);
            }
        }
    }
}
