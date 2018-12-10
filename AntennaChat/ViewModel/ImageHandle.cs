using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Contacts;
using CSharpWin_JD.CaptureImage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel
{
    /// <summary>
    /// 图片处理类
    /// </summary>
    public class ImageHandle
    {
        /// <summary>
        /// 获取讨论组头像  
        /// </summary>
        /// <param name="Members"></param>
        /// <returns></returns>
        public static string GetGroupPicture(ObservableCollection<string> Members)
        {
            List<string> memList = new List<string>(Members.ToList());
            return GetGroupPicture(memList);
        }

        /// <summary>
        /// 获取讨论组头像
        /// </summary>
        /// <returns></returns>
        public static string GetGroupPicture(List<string> Members)
        {

            string strFilePath = ""; //讨论组头像路径
            if (GetMembersHeadPic(Members))
            {
                Bitmap groupBitmap = MergerImg(_HeadPic);
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Cache\\Group") == false)
                //如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Cache\\Group");
                }
                strFilePath = AppDomain.CurrentDomain.BaseDirectory + "Cache\\Group\\" + Guid.NewGuid().ToString() +
                              ".png";
                groupBitmap.Save(strFilePath);
                //SendCutImageDto scid = new SendCutImageDto();
                //scid.cmpcd = GlobalVariable.CompanyCode;
                //scid.seId = "";
                //scid.fileFileName = "";
                //scid.file = strFilePath;
                //ReturnCutImageDto dto = (new HttpService()).FileUpload<ReturnCutImageDto>(scid);
                //if (dto == null)
                //    strFilePath = "";
                //else
                //    strFilePath = dto.fileUrl;
            }
            return strFilePath;
        }

        /// <summary>
        /// 上传讨论组头像
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static string UploadPicture(string strFilePath)
        {
            SendCutImageDto scid = new SendCutImageDto();
            scid.cmpcd = GlobalVariable.CompanyCode;
            scid.seId = "";
            scid.fileFileName = "";
            scid.file = strFilePath;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkSendFileInput fileInput = new AntSdkSendFileInput();
            fileInput.cmpcd = GlobalVariable.CompanyCode;
            fileInput.seId =
                fileInput.file = strFilePath;
            AntSdkFailOrSucessMessageDto failMessage = new AntSdkFailOrSucessMessageDto();
            failMessage.mtp = (int)AntSdkMsgType.ChatMsgPicture;
            failMessage.content = "";
            //failMessage.sessionid = s_ctt.sessionId;
            DateTime dt = DateTime.Now;
            failMessage.lastDatetime = dt.ToString();
            fileInput.FailOrSucess = failMessage;
            var errCode = 0;
            string errMsg = string.Empty;
            var fileOutput = AntSdkService.FileUpload(fileInput, ref errCode, ref errMsg);
            //ReturnCutImageDto dto = (new HttpService()).FileUpload<ReturnCutImageDto>(scid);
            if (fileOutput == null)
                return string.Empty;
            else
                return fileOutput.dowmnloadUrl;
        }

        /// <summary>
        /// 头像集合
        /// </summary>
        private static List<Image> _HeadPic = new List<Image>();

        /// <summary>
        /// 获取讨论组成员前4个头像
        /// </summary>
        private static bool GetMembersHeadPic(List<string> members)
        {
            try
            {
                _HeadPic.Clear();
                int flag = 0;
                foreach (string m in members)
                {
                    if (flag > 3) break;
                    string path = string.IsNullOrEmpty(m)
                        ? "pack://application:,,,/AntennaChat;Component/Images/27-头像.png"
                        : m;
                    System.Drawing.Image getImg;
                    if (path.Contains("pack"))
                    {
                        getImg =
                            System.Drawing.Image.FromFile(AppDomain.CurrentDomain.BaseDirectory +
                                                          "Images\\36-头像-copy.png");
                        System.Drawing.Image img = DataConverter.GetImageThumb(getImg, 26, 26);
                        _HeadPic.Add(img);
                        flag++;
                    }
                    else //if (path.Contains("http"))
                    {
                        WebRequest webr = WebRequest.Create(path);
                        WebResponse webrr = webr.GetResponse();
                        Stream st = webrr.GetResponseStream();
                        getImg = System.Drawing.Image.FromStream(st);
                        st.Close();
                        System.Drawing.Image img = DataConverter.GetImageThumb(getImg, 26, 26);
                        _HeadPic.Add(img);
                        flag++;
                    }
                }
                if (_HeadPic.Count > 0) return true;
                else return false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[讨论组头像更新失败]:" + ex.Message + "," + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        private static Bitmap MergerImg(List<Image> maps)
        {
            int i = maps.Count;
            if (i == 0)
                throw new Exception("图片数不能够为0");
            //创建要显示的图片对象,根据参数的个数设置宽度
            Bitmap bitmap = new Bitmap(64, 64);
            Graphics g = Graphics.FromImage(bitmap);
            //清除画布,背景设置为白色
            g.Clear(DataConverter.ColorHx16toRGB("#E0E0E0"));
            switch (i)
            {
                case 1:
                    {
                        g.DrawImage(maps[0], 19, 19);
                    }
                    break;
                case 2:
                    {
                        g.DrawImage(maps[0], 4, 19);
                        g.DrawImage(maps[1], 34, 19);
                    }
                    break;
                case 3:
                    {
                        g.DrawImage(maps[0], 19, 4);
                        g.DrawImage(maps[1], 4, 34);
                        g.DrawImage(maps[2], 34, 34);
                    }
                    break;
                default:
                    {
                        g.DrawImage(maps[0], 4, 4);
                        g.DrawImage(maps[1], 34, 4);
                        g.DrawImage(maps[2], 4, 34);
                        g.DrawImage(maps[3], 34, 34);
                    }
                    break;
            }
            g.Dispose();
            return bitmap;
        }

        /// <summary>
        /// 截图到剪切板
        /// </summary>
        public static void CutImg()
        {
            try
            {
                using (CaptureImageTool capImg = new CaptureImageTool())
                {
                    capImg.Closed += CapImg_Closed;
                    GlobalVariable.isCutShow = true;
                    if (capImg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Clipboard.Clear();
                        Clipboard.SetDataObject(capImg.Image);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[ImageHandle-CutImg]" + ex.Message + ex.Source + ex.StackTrace);
            }
        }

        private static void CapImg_Closed(object sender, EventArgs e)
        {
            GlobalVariable.isCutShow = false;
        }

        #region 图片查看器处理
        public static Views.Talk.PictureViewerView PicView;
        public static Talk.PictureViewerViewModel PicViewModel;
        #endregion
        #region 下载图片方法
        /// <summary>
        /// 从http下载图片
        /// </summary>
        /// <param name="picUrl">图片Http地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeOut">Request最大请求时间，如果为-1则无限制</param>
        ///<param name="actualFilePath">压缩之后保存路径（不压缩就不用传参）</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static string DownloadPictureFromHttp(string picUrl, string savePath, int timeOut = -1)
        {
            if (File.Exists(savePath))
                return savePath;
            var value = "";
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                if (timeOut != -1) request.Timeout = timeOut;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("Text/"))
                {
                    var isResult = SaveBinaryFile(response, savePath);
                    if (isResult)
                    {
                        value = savePath;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[下载图片失败]：图片原始Url>>>" + picUrl + ">>>错误信息：" + ex.StackTrace);
                return value;
            }
            finally
            {
                stream?.Close();
                response?.Close();
            }
            return value;
        }
        /// <summary>
        /// 从file下载图片
        /// </summary>
        /// <param name="picUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string DownloadPictureFromFile(string picUrl, string savePath, int timeOut = -1)
        {
            if (File.Exists(savePath))
                return savePath;
            var value = "";
            WebResponse response = null;
            Stream stream = null;
            try
            {
                FileWebRequest request = (FileWebRequest)WebRequest.Create(picUrl);
                if (timeOut != -1) request.Timeout = timeOut;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("Text/"))
                {
                    var isResult = SaveBinaryFile(response, savePath);
                    if (isResult)
                    {
                        value = savePath;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[下载图片失败]：图片原始Url>>>" + picUrl + ">>>错误信息：" + ex.StackTrace);
                // value= picUrl.Replace("file:///", "");
                try
                {
                    File.Copy(picUrl.Replace("file:///", ""), savePath, true);
                    return savePath;
                }
                catch (Exception e)
                {
                    LogHelper.WriteError("[本地图片资源复制]：图片原始Url>>>" + picUrl + ">>>错误信息：" + ex.StackTrace);
                    return value;
                }
            }
            finally
            {
                stream?.Close();
                response?.Close();
            }
            return value;
        }
        /// <summary>
        /// 保存二进制
        /// </summary>
        /// <param name="response"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            var value = false;
            var buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            try
            {
                outStream = File.Create(savePath);
                inStream = response.GetResponseStream();
                var l = 0;
                do
                {
                    if (inStream != null) l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0) outStream.Write(buffer, 0, l);
                } while (l > 0);
                value = true;
            }
            finally
            {
                outStream?.Close();
                inStream?.Close();
            }
            return value;
        }
        #endregion
    }
}
