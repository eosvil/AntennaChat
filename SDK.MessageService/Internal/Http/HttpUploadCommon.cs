using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Internal.Http
{
    internal class HttpUploadCommon
    {
        /// <summary>
        /// Http上传文件
        /// </summary>
        public string HttpUploadFile(string url, string path)
        {
            HttpWebResponse response=null;
            HttpWebRequest request = null;

            StreamReader streamReader = null;
            Stream instream = null;
            try
            {
                // 设置参数
                request = WebRequest.Create(url) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                //超时设置 15000毫秒
                request.Timeout = 20000;
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                string boundary = DateTime.Now.Ticks.ToString("X");
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
                byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                int pos = path.LastIndexOf("/");
                if (pos == -1)
                {
                    pos = path.LastIndexOf("\\");
                }
                string fileName = path.Substring(pos + 1);
                //请求头部信息 
                StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fs.Length];
                fs.Read(bArr, 0, bArr.Length);
                fs.Close();
                Stream postStream = request.GetRequestStream();
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bArr, 0, bArr.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                postStream.Close();
                postStream.Dispose();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                streamReader = new StreamReader(instream, Encoding.UTF8);
                //返回结果网页（html）代码
                string content = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();
                instream.Close();
                instream.Dispose();
                return content;
            }
            catch(WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                if (response != null)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string strHtml = sr.ReadToEnd();
                    LogHelper.WriteError("[SDK.Service.Internal.Http-HttpUploadFile]:" + strHtml);
                    sr?.Close();
                    sr?.Dispose();
                    response?.Close();
                    response?.Dispose();
                    instream?.Close();
                    instream?.Dispose();
                }
                return null;
            }
        }
    }
}
