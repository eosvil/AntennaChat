using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SDK.Service.Models;
using SDK.Service.Properties;

namespace SDK.Service
{
    /// <summary>
    /// Http服务类通用方法
    /// </summary>
    internal class HttpCommon<TIn, TOut>
    {
        /// <summary>
        /// 用户认证失败
        /// </summary>
        public event EventHandler TokenErrorEvent;

        /// <summary>
        /// SDK_HTTP请求统一入口
        /// </summary>
        /// <typeparam name="TIn"> 输入参数实体类型</typeparam>
        /// <typeparam name="TOut">输出参数实体类型</typeparam>
        /// <param name="methodName">方法名</param>
        /// <param name="input">输入参数</param>
        /// <param name="output">输出参数</param>
        /// <param name="errorCode">错误码（特定情况）</param>
        /// <param name="errorMsg">错误信息</param>
        /// <param name="requestType">请求类型（默认为post）</param>
        /// <returns></returns>
        public bool HttpCommonMethod(string methodName, TIn input, ref TOut output, ref int errorCode, ref string errorMsg
            , SdkEnumCollection.RequestMethod requestType = SdkEnumCollection.RequestMethod.POST)
        {
            var strUrl = string.Empty;
            var paramData = string.Empty;
            var stopWatch = new Stopwatch();
            var responseText = string.Empty;
            var transId = Guid.NewGuid();
            try
            {
                stopWatch.Start();
                strUrl = $"{SdkService.SdkSysParam.HttpPrdfix}{methodName}";
                HttpWebRequest request;
                //证书认证处理
                ServicePointManager.ServerCertificateValidationCallback = HttpCertificate.CheckValidationResult;
                //开始请求
                if (requestType == SdkEnumCollection.RequestMethod.POST ||
                    requestType == SdkEnumCollection.RequestMethod.PUT)
                {
                    var serializeerrMsg = string.Empty;
                    if (input != null && !JsonCoder.SerializeJson(input, ref paramData, ref serializeerrMsg))
                    {
                        stopWatch.Stop();
                        errorMsg += Resources.SdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},{Environment.NewLine} Input:{typeof (TOut)} paramData SerializeJson Error:{serializeerrMsg}");
                        return false;
                    }
                    // 创建一个HTTP请求
                    request = (HttpWebRequest) WebRequest.Create(strUrl);
                    //设置超时时间(先设置为15秒)
                    request.Timeout = SdkService.SdkSysParam.HttpTimeOut;
                    request.Method = requestType.ToString();
                    request.ContentType = "application/json;charset=utf-8";
                    request.Headers.Add(HttpRequestHeader.Authorization, SdkService.SdkSysParam.Token);
                    request.Headers.Add("sourceId",
                        $"{(int) SdkEnumCollection.OSType.PC}{SdkService.SdkSysParam.Appkey}");
                    request.Headers.Add("transId", $"{transId}");
                    request.Headers.Add("sourceInsId", $"{GetComputeInfo.GetMacAddressByNetworkInformation()}");
                    if (!string.IsNullOrEmpty(paramData))
                    {
                        var byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                        request.ContentLength = byteArray.Length;
                        var newStream = request.GetRequestStream();
                        newStream.Write(byteArray, 0, byteArray.Length); //写入参数
                        newStream.Close();
                    }
                }
                else
                {
                    // 创建一个HTTP请求
                    if (input != null)
                    {
                        //判断是否是Path类型/Request类型的输入参数
                        var inputprepath = input as IInputPrefPath;
                        var inputsufpath = input as IInputSuffPath;
                        var inputrequest = input as IInputQuery;
                        //Path方式处理
                        if (inputprepath != null || inputsufpath != null)
                        {
                            var strUrLs = strUrl.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
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
                    //创建请求
                    request = (HttpWebRequest) WebRequest.Create(strUrl);
                    //设置超时时间(先设置为15秒)
                    request.Timeout = SdkService.SdkSysParam.HttpTimeOut;
                    request.Method = requestType.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    request.Headers.Add(HttpRequestHeader.Authorization, SdkService.SdkSysParam.Token);
                    request.Headers.Add("sourceId",
                        $"{(int) SdkEnumCollection.OSType.PC}{SdkService.SdkSysParam.Appkey}");
                    request.Headers.Add("transId", $"{Guid.NewGuid()}");
                    request.Headers.Add("sourceInsId", $"{GetComputeInfo.GetMacAddressByNetworkInformation()}");
                }
                var response = (HttpWebResponse) request.GetResponse();
                var streams = response.GetResponseStream();
                if (streams == null)
                {
                    stopWatch.Stop();
                    errorMsg += Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}{Environment.NewLine}Response's ResponseStream Is Null");
                    return false;
                }
                //处理
                var myreader = new System.IO.StreamReader(streams, Encoding.UTF8);
                responseText = myreader.ReadToEnd();
                myreader.Close();
                response.Close();
                stopWatch.Stop();
                //没有返回信息
                if (string.IsNullOrEmpty(responseText))
                {
                    errorMsg += Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}{Environment.NewLine}responseText:{responseText},{Resources.SdkHttpResponseNull}");
                    return false;
                }
                //判断errorCode[有ErrorCode的情况下按照ErrorCode规则处理]
                /*按照服务器统一返回结构，解析errorCode及errorMsg
                 * {
                 * "errorCode": "0",
                 * "errorMsg": "请求成功",
                 * "data": "bf8cad5f-928b-403b-8ea7-5b15ae8ab0e9"
                 * }
                 */
                var responseErrcode = string.Empty;
                var responseErrmsg = string.Empty;
                var getrespsErrmsg = string.Empty;
                if (GetResponseByJson(responseText, ref responseErrcode, ref responseErrmsg, ref getrespsErrmsg))
                {
                    if (responseErrcode != "0")
                    {
                        if (string.IsNullOrEmpty(responseErrmsg))
                        {
                            errorMsg += SetResources.Resources.GetString($"E_{responseErrcode}");
                        }
                        else
                        {
                            errorMsg += responseErrmsg;
                        }
                        //非零如果使用BaseOutput传递
                        var errorsetout = output as BaseOutput;
                        if (errorsetout != null)
                        {
                            errorsetout.errorCode = responseErrcode;
                            errorsetout.errorMsg = errorMsg;
                            int tempErrorCode = 0;
                            int.TryParse(responseErrcode,out tempErrorCode);
                            errorCode = tempErrorCode;
                        }
                        if (responseErrcode == "1004")
                            errorCode = Convert.ToInt32(responseErrcode);
                        //记录错误日志：正常的服务器返回请求错误
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}，Output:{typeof (TOut)}{Environment.NewLine}{responseText}");
                        //返回错误
                        return false;
                    }
                    //如果外部传入的就是 BaseOutput 则直接返回请求正确
                    var setout = output as BaseOutput;
                    if (setout != null)
                    {
                        //记录Debug日志
                        LogHelper.WriteDebug(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}，Output:{typeof (TOut)}{Environment.NewLine}{responseText}");
                        return true;
                    }
                    setout = new BaseOutput();
                    var temperrMsg = string.Empty;
                    if (!JsonCoder.DeserializeJson(responseText, ref setout, ref temperrMsg))
                    {
                        errorMsg += Resources.SdkHttpRequestFail;
                        //记录Error日志
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}，Output:{typeof (TOut)}{Environment.NewLine}{responseText} Change BaseOutput Error:{Resources.SdkHttpCommonBaseSchemaError}{temperrMsg}");
                        return false;
                    }
                    else if (setout.data == null)
                    {
                        errorMsg += Resources.SdkHttpRequestFail;
                        //记录Error日志
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}，Output:{typeof (TOut)}{Environment.NewLine}{responseText} Change BaseOutput Data is null:{Resources.SdkHttpCommonBaseSchemaNull}，errorCode = 0,data = null");
                        return false;
                    }
                    else if (output is string)
                    {
                        output = (TOut) setout.data;
                    }
                    else if (!JsonCoder.DeserializeJson(setout.data.ToString(), ref output, ref temperrMsg))
                    {
                        errorMsg += Resources.SdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}{Environment.NewLine}{responseText} DealWith = {typeof (TOut)};Value={setout.data};Error:{temperrMsg}{Resources.SdkHttpCommonOutSchemaError}");
                        return false;
                    }
                }
                else
                {
                    //如果请求返回的信息没有errorCode则直接反序列为输出实体
                    var directerrorMsg = string.Empty;
                    var getReturnInfo = JsonCoder.DeserializeJson(responseText, ref output, ref directerrorMsg);
                    if (!getReturnInfo)
                    {
                        errorMsg += Resources.SdkHttpRequestFail;
                        LogHelper.WriteError(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}{Environment.NewLine} Get ErrorCode Deserialize error:{getrespsErrmsg} And Derect Deserialize {typeof (TOut)} {responseText}{directerrorMsg}");
                        return false;
                    }
                }
                //记录Debug日志
                LogHelper.WriteDebug(
                    $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{paramData}，Output:{typeof (TOut)}{Environment.NewLine}{responseText}");
                //返回
                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    errorCode = SdkErrorCodes.Instanece.HTTP_REQUREST_TIMEOUT;
                    errorMsg = Resources.SdkHttpRequestTimeOut;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status}),Transid:{transId}]:{strUrl}?{paramData}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                else
                {
                    errorMsg = Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status}),Transid:{transId}]:{strUrl}?{paramData}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                //返回
                return false;
            }
            catch (Exception ex)
            {
                errorMsg = Resources.SdkHttpRequestFail;
                LogHelper.WriteError(
                    $"[SDK_HttpService.HttpCommonMethod,Transid:{transId}]:{strUrl}?{paramData}{Environment.NewLine} responseText:{responseText}{ex.Message}{ex.StackTrace}");
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
                errCodeStr != "1004" || TokenErrorEvent == null)
            {
                return;
            }
            //Token 失效
            LogHelper.WriteError(
                $"[HTTPService.SDK_HttpCommonMethod(UserId:[{SdkService.SdkSysParam?.UserId}];Token:[{SdkService.SdkSysParam?.Token}])]:Token lose valid");
            //调用
            TokenErrorEvent?.Invoke(null, null);
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
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }
                //处理
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
            string tStr = string.Empty;
            if (entity == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties =
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
