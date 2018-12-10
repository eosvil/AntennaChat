using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SDK.AntSdk.AntModels;
using SDK.Service;

namespace SDK.AntSdk
{
    internal class AntSdkHttpCommon<TIn, TOut>
    {
        /// <summary>
        /// Token校验失败时间
        /// </summary>
        public event EventHandler AntSdkTokenErrorEvent;

        /// <summary>
        /// 触角业务HTTP请求统一入口
        /// </summary>
        /// <typeparam name="TIn">输入类型约束</typeparam>
        /// <typeparam name="TOut">输出类型约束</typeparam>
        /// <param name="methodName"></param>
        /// <param name="input">输入参数</param>
        /// <param name="output">输出参数</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误提示</param>
        /// <param name="requestType">HTTP请求类型</param>
        /// <param name="customersRequest">是否客户系统</param>
        /// <returns>是否成功</returns>
        public bool AntSdkHttpCommonMethod(string methodName,
            TIn input, ref TOut output, ref int errorCode, ref string errorMsg, RequestMethod requestType = RequestMethod.POST,
            bool customersRequest = false)
        {
            var strUrl = string.Empty;
            var logparamData = string.Empty;
            var stopWatch = new Stopwatch();
            var responseText = string.Empty;
            try
            {
                stopWatch.Start();
                //如果是客户系统请求
                strUrl = customersRequest
                    ? $"{AntSdkService.AntSdkConfigInfo.CustomersHttpPrdfix}{methodName}"
                    : $"{AntSdkService.AntSdkConfigInfo.AntServiceHttpPrdfix}{methodName}";
                var paramData = string.Empty;
                HttpWebRequest request;
                if (requestType == RequestMethod.POST || requestType == RequestMethod.PUT)
                {
                    var serializeerrMsg = string.Empty;
                    if (input != null && !JsonCoder.SerializeJson(input, ref paramData, ref errorMsg))
                    {
                        stopWatch.Stop();
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl}{Environment.NewLine}Input:{typeof(TOut)} paramData SerializeJson Error:{serializeerrMsg}");
                        return false;
                    }
                    //日志参数赋值
                    logparamData = methodName == "v1/ant/client/account/login" ? string.Empty : paramData;
                    //创建一个HTTP请求
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    //设置超时时间(先设置为15秒)
                    request.Timeout = SdkService.SdkSysParam.HttpTimeOut;
                    request.Method = requestType.ToString();
                    request.ContentType = "application/json;charset=utf-8";
                    if (!string.IsNullOrEmpty(AntSdkService.AntSdkLoginOutput?.token))
                    {
                        request.Headers.Add(HttpRequestHeader.Authorization, AntSdkService.AntSdkLoginOutput.token);
                    }
                    var byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                    request.ContentLength = byteArray.Length;
                    var newStream = request.GetRequestStream();
                    newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                    newStream.Close();
                }
                else
                {
                    // 创建一个HTTP请求
                    if (input != null)
                    {
                        //判断是否是Path类型/Request类型的输入参数
                        var inputprepath = input as IAntSdkInputPrefPath;
                        var inputsufpath = input as IAntSdkInputSuffPath;
                        var inputrequest = input as IAntSdkInputQuery;
                        //Path方式处理
                        if (inputprepath != null || inputsufpath != null)
                        {
                            var strUrLs = strUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            if (strUrLs.Length > 0)
                            {
                                //前缀路径
                                if (inputprepath != null)
                                {
                                    strUrLs[strUrLs.Length - 1] =
                                        $"{inputprepath.GetPrefPath()}{strUrLs[strUrLs.Length - 1]}";
                                }
                                //后缀路径
                                if (inputsufpath != null)
                                {
                                    strUrLs[strUrLs.Length - 1] =
                                        $"{strUrLs[strUrLs.Length - 1]}{inputsufpath.GetSuffPath()}";
                                }
                            }
                            //http头会被/分离，则进行特殊处理
                            for (var i = 0; i < strUrLs.Length; i++)
                            {
                                if (strUrLs[i].ToUpper() != "HTTP:")
                                {
                                    continue;
                                }
                                strUrLs[i] = $"{strUrLs[i]}/";
                                break;
                            }
                            //合成路径
                            strUrl = string.Join("/", strUrLs);
                        }
                        //Request方式处理
                        if (inputrequest != null)
                        {
                            strUrl += inputrequest.GetQuery();
                        }
                        //Body方式处理
                        if (inputprepath == null && inputrequest == null && inputsufpath == null)
                        {
                            paramData = FormatInputEntity(input);
                            strUrl = $"{strUrl}?{paramData}";
                        }
                    }
                    //创建一个HTTP请求
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    //设置超时时间(先设置为15秒)
                    request.Timeout = SdkService.SdkSysParam.HttpTimeOut;
                    request.Method = requestType.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    if (!string.IsNullOrEmpty(AntSdkService.AntSdkLoginOutput?.token))
                    {
                        request.Headers.Add(HttpRequestHeader.Authorization, AntSdkService.AntSdkLoginOutput.token);
                    }
                }
                //获取请求结果
                var response = (HttpWebResponse)request.GetResponse();
                var streams = response.GetResponseStream();
                if (streams == null)
                {
                    stopWatch.Stop();
                    errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}{Environment.NewLine}Response's ResponseStream is null");
                    return false;
                }
                //处理请求结果
                var myreader = new StreamReader(streams, Encoding.UTF8);
                responseText = myreader.ReadToEnd();
                myreader.Close();
                response.Close();
                stopWatch.Stop();
                //没有返回信息
                if (string.IsNullOrEmpty(responseText))
                {
                    errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}{Environment.NewLine}responseText is null，{Properties.Resources.AntSdkHttpResponseNull}");
                    return false;
                }
                //判断errorCode[有ErrorCode的情况下按照ErrorCode规则处理]
                var responseErrcode = string.Empty;
                var responseErrmsg = string.Empty;
                var getrespsErrmsg = string.Empty;
                if (GetResponseByJson(responseText, ref responseErrcode, ref responseErrmsg, ref getrespsErrmsg))
                {
                    if (responseErrcode != "0")
                    {
                        if (string.IsNullOrEmpty(responseErrmsg))
                        {
                            errorMsg += AntSdkSetResources.Resources.GetString($"AntSdkE_{responseErrcode}");
                        }
                        else
                        {
                            errorMsg += responseErrmsg;
                        }
                        if (responseErrcode == "1004" || responseErrcode == "10023" ||responseErrcode=="10022" ||responseErrcode=="10014" ||responseErrcode== "9023" ||responseErrcode== "9024"||responseErrcode== "10013")
                            errorCode = Convert.ToInt32(responseErrcode);
                        //非零如果使用BaseOutput传递
                        var errorsetout = output as AntSdkBaseOutput;
                        if (errorsetout != null)
                        {
                            errorsetout.errorCode = responseErrcode;
                            errorsetout.errorMsg = errorMsg;
                        }
                        //记录错误日志：正常的服务器返回请求错误
                        LogHelper.WriteDebug(
                            $"[HTTPService.AntSdkHttpHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}，Output:{typeof(TOut)}{Environment.NewLine}{responseText}");
                        //返回错误
                        return false;
                    }
                    var setout = output as AntSdkBaseOutput;
                    if (setout != null)
                    {
                        //记录Debug日志,正常基类请求操作，无返回
                        LogHelper.WriteDebug(
                            $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}，Output:{typeof(TOut)}{Environment.NewLine}{responseText}");
                        return true;
                    }
                    setout = new AntSdkBaseOutput();
                    var temperrMsg = string.Empty;
                    if (!JsonCoder.DeserializeJson(responseText, ref setout, ref temperrMsg))
                    {
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        //记录Error日志
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}，Output:{typeof(TOut)}{Environment.NewLine}{responseText} Change AntSdkBaseOutput Error:{Properties.Resources.AntSdkHttpCommonBaseSchemaError}{temperrMsg}");
                        return false;
                    }
                    else if (setout.data == null)
                    {
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        //记录Error日志
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}，Output:{typeof(TOut)}{Environment.NewLine}{responseText} Change AntSdkBaseOutput Data is null:{Properties.Resources.AntSdkHttpCommonBaseSchemaNull}，errorCode = 0,data = null");
                        return false;
                    }
                    else if (output is string)
                    {
                        output = (TOut)setout.data;
                    }
                    else if (!JsonCoder.DeserializeJson(setout.data.ToString(), ref output, ref temperrMsg))
                    {
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}{Environment.NewLine}{responseText} DealWith = {typeof(TOut)};Value={setout.data} Error:{temperrMsg}{Properties.Resources.AntSdkHttpCommonOutSchemaError}");
                        return false;
                    }
                }
                else
                {
                    //如果请求返回的信息没有errorCode则直接反序列为输出实体
                    var temperrorMsg = string.Empty;
                    var getReturnInfo = JsonCoder.DeserializeJson(responseText, ref output, ref temperrorMsg);
                    if (!getReturnInfo)
                    {
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}{Environment.NewLine} Get ErrorCode Deserialize error:{getrespsErrmsg} and direct Deserialize error:{responseText},{typeof(TOut)},{temperrorMsg}");
                        return false;
                    }
                }
                //记录Debug日志
                LogHelper.WriteDebug(
                    $"[HTTPService.AntSdkHttpHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData}，Output:{typeof(TOut)}{Environment.NewLine}{responseText}");
                //返回
                return true;
            }
            catch (WebException wbex)
            {
                if (wbex.Status == WebExceptionStatus.Timeout)
                {
                    errorCode = AntSdkErrorCodes.Instanece.HTTP_REQUREST_TIMEOUT;
                    errorMsg = Properties.Resources.SdkHttpRequestTimeOut;
                    LogHelper.WriteError(
                        $"[HTTPService.AntSdkHttpHttpCommonMethod({wbex.Status})({stopWatch.Elapsed.TotalMilliseconds}毫秒)]]:{strUrl}?{logparamData}{Environment.NewLine} responseText:{responseText}{wbex.Message}{wbex.StackTrace}");
                    return false;
                }
                try
                {
                    var response = (HttpWebResponse)wbex.Response;
                    var streams = response.GetResponseStream();
                    if (streams == null)
                    {
                        errorMsg += Properties.Resources.AntSdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData},Nomal Response exist WebException{Environment.NewLine} WebException Response's ResponseStream Is Null,{wbex.Message}{wbex.StackTrace}");
                        return false;
                    }
                    //请求处理
                    var myreader = new StreamReader(streams, Encoding.UTF8);
                    responseText = myreader.ReadToEnd();
                    myreader.Close();
                    response.Close();
                    var errCodeStr = string.Empty;
                    if (JsonCoder.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errorMsg)
                        && errCodeStr == "1004") //获取errorCode
                    {
                        AntSdkTokenErrorEvent?.Invoke(null, null);
                    }
                    errorCode = Convert.ToInt32(errCodeStr);
                    errorMsg = "访问后台服务失败";
                    LogHelper.WriteError(
                        $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData},Nomal Response exist WebException{Environment.NewLine}{wbex.Message},{wbex.StackTrace},{responseText}");
                    return false;
                }
                catch
                {
                    errorMsg = "访问后台服务失败";
                    LogHelper.WriteError(
                        $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData},Nomal Response exist WebException{Environment.NewLine}{wbex.Message},{wbex.StackTrace},{responseText}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                errorMsg = "访问后台服务失败";
                LogHelper.WriteError(
                    $"[HTTPService.AntSdkHttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒)]:{strUrl},Param:{logparamData},Nomal Response exist WebException But Exception {Environment.NewLine}{ex.Message}");
                return false;
            }
            finally
            {
                //检查：用户认证失败错误的特殊处理
                HandleTokenError(responseText, ref errorMsg);
            }
        }

        /// <summary>
        /// 对token错误的特殊处理(先预留该方法)
        /// </summary>
        private void HandleTokenError(string responseText, ref string errMsg)
        {
            var errCodeStr = string.Empty;
            if (!JsonCoder.GetValueByJsonKey("errorCode", responseText, ref errCodeStr, ref errMsg) ||
                errCodeStr != "1004" || AntSdkTokenErrorEvent == null)
            {
                return;
            }
            //Token 失效
            LogHelper.WriteError(
                $"[HTTPService.SDK_HttpCommonMethod(UserId:[{AntSdkService.AntSdkLoginOutput?.userId}];Token:[{AntSdkService.AntSdkLoginOutput?.token}])]:Token lose valid");
            //调用
            AntSdkTokenErrorEvent?.Invoke(null, null);
        }

        /// <summary>
        /// 方法说明：根据Http请求返回结构获取结果信息
        /// 完成时间：2016-05-20
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <param name="responseErrcode">Http请求返回错误码</param>
        /// <param name="responseErrmsg">Http请求返回错误信息</param>
        /// <param name="errMsg">处理过程的错误信息</param>
        /// <returns>是否执行成功</returns>
        private static bool GetResponseByJson(string json, ref string responseErrcode, ref string responseErrmsg, ref string errMsg)
        {
            /*按照服务器统一返回结构，解析errorCode及errorMsg
             * {
             * "errorCode": "0",
             * "errorMsg": "请求成功",
             * "data": "bf8cad5f-928b-403b-8ea7-5b15ae8ab0e9"
             * }
             */
            try
            {
                var jObject = JObject.Parse(json);
                //判断记录
                if (jObject == null)
                {
                    LogHelper.WriteError($"[JsonCoder_GetValueByJsonKey]:Invalid Json,{json}");
                    return false;
                }
                responseErrcode = jObject["errorCode"].ToString();
                try
                {
                    responseErrmsg = jObject["errorMsg"].ToString();
                }
                catch
                {
                    responseErrmsg = string.Empty;
                }
                //返回
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
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
            var tStr = string.Empty;
            if (entity == null)
            {
                return tStr;
            }
            var properties =
                entity.GetType()
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                var name = item.Name;
                var value = item.GetValue(entity, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    tStr += $"&{name}={value}";
                }
                else
                {
                    FormatInputEntity(value);
                }
            }
            tStr = tStr.TrimStart('&');
            return tStr;
        }
    }
}
