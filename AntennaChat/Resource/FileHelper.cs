using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Talk;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.BLL;
using SDK.AntSdk.DAL;

namespace AntennaChat.Resource
{
    public class FileHelper
    {
        /// <summary>
        /// 所有联系人头像下载并压缩保存
        /// </summary>
        /// <param name="downloadFilePath">原文件路径（压缩之后文件夹删除）</param>
        /// <param name="actualFilePath">压缩后的文件路径</param>
        public static void DownloadUserHeadImage()
        {
            var downloadUserHeadImagePath = publicMethod.DownloadFilePath() + "\\DownloadUserHeadImage\\";
            if (!Directory.Exists(downloadUserHeadImagePath))
                Directory.CreateDirectory(downloadUserHeadImagePath);
            if (!Directory.Exists(publicMethod.UserHeadImageFilePath()))
                Directory.CreateDirectory(publicMethod.UserHeadImageFilePath());
            //AsyncHandler.AsyncCall(System.Windows.Application.Current.Dispatcher, () =>
            //{
            var tSessionBll = new BaseBLL<AntSdkTsession, T_SessionDAL>();
            var tSessionList = tSessionBll.GetList();
            if (tSessionList != null && tSessionList.Count > 0)
            {
                var userSessionList = tSessionList.Where(m => !string.IsNullOrEmpty(m.UserId));
                foreach (var userSession in userSessionList)
                {
                    var contactsUser =
                        AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(m => m.userId == userSession.UserId);
                    if (contactsUser == null) continue;
                    {
                        if (string.IsNullOrEmpty(contactsUser.picture)) continue;
                        var tempUserImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(m => m.UserID == contactsUser.userId);
                        if (tempUserImage != null) continue;
                        var index = contactsUser.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                        var fileName = contactsUser.picture.Substring(index, contactsUser.picture.Length - index);
                        if (File.Exists(publicMethod.UserHeadImageFilePath() + fileName) ||
                            !publicMethod.IsUrlRegex(contactsUser.picture)) continue;
                        var filePath = DownloadPicture(contactsUser.picture, downloadUserHeadImagePath + fileName, -1, publicMethod.UserHeadImageFilePath(), contactsUser.userId);
                        if (string.IsNullOrEmpty(filePath)) continue;
                        GlobalVariable.ContactHeadImage.UserHeadImages.Add(new ContactUserImage
                        {
                            Url = filePath,
                            UserID = contactsUser.userId
                        });
                    }
                }
            }

            var contactsUsers = AntSdkService.AntSdkListContactsEntity.users;
            for (var i = 0; i < contactsUsers.Count; i++)
            {
                var contactUser = contactsUsers[i];
                if (string.IsNullOrEmpty(contactUser.picture)) continue;
                var tempUserImage = GlobalVariable.ContactHeadImage.UserHeadImages.FirstOrDefault(m => m.UserID == contactUser.userId);
                if (tempUserImage != null) continue;
                var index = contactUser.picture.LastIndexOf("/", StringComparison.Ordinal) + 1;
                var fileName = contactUser.picture.Substring(index, contactUser.picture.Length - index);
                if (!File.Exists(publicMethod.UserHeadImageFilePath() + fileName) && publicMethod.IsUrlRegex(contactUser.picture))
                {
                    var filePath = DownloadPicture(contactUser.picture, downloadUserHeadImagePath + fileName, -1, publicMethod.UserHeadImageFilePath(), contactUser.userId);
                    if (string.IsNullOrEmpty(filePath)) continue;
                    GlobalVariable.ContactHeadImage.UserHeadImages.Add(new ContactUserImage
                    {
                        Url = filePath,
                        UserID = contactUser.userId
                    });
                }
                else
                {
                    GlobalVariable.ContactHeadImage.UserHeadImages.Add(new ContactUserImage
                    {
                        Url = publicMethod.UserHeadImageFilePath() + fileName,
                        UserID = contactUser.userId
                    });
                }
            }
            if (!Directory.Exists(downloadUserHeadImagePath)) return;
            var files = Directory.GetFiles(downloadUserHeadImagePath);
            if (files.Length == 0)
            {
                Directory.Delete(downloadUserHeadImagePath);
            }
            else
            {
                foreach (string fileName in files)
                {
                    string fName = fileName.Substring(downloadUserHeadImagePath.Length);
                    try
                    {
                        if (File.Exists(Path.Combine(downloadUserHeadImagePath, fName)) && !File.Exists(Path.Combine(publicMethod.UserHeadImageFilePath(), fName)))
                            File.Move(Path.Combine(downloadUserHeadImagePath, fName), Path.Combine(publicMethod.UserHeadImageFilePath(), fName));
                        else
                        {
                            File.Delete(Path.Combine(downloadUserHeadImagePath, fName));
                        }
                    }
                    // 捕捉异常.
                    catch (IOException copyError)
                    {
                        LogHelper.WriteError(copyError.Message);
                    }
                }
                if (files.Length == 0)
                    Directory.Delete(downloadUserHeadImagePath);
            }
        }
        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="picUrl">图片Http地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeOut">Request最大请求时间，如果为-1则无限制</param>
        ///<param name="actualFilePath">压缩之后保存路径（不压缩就不用传参）</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static string DownloadPicture(string picUrl, string savePath, int timeOut = -1, string actualFilePath = "", string userId = "")
        {
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
                        if (!string.IsNullOrEmpty(actualFilePath))
                        {
                            value = GetPicThumbnail(savePath, actualFilePath, 50, 50, 96, userId);
                            if (File.Exists(savePath) && !PublicTalkMothed.IsFileInUsing(savePath))
                            {
                                File.Delete(savePath);

                            }
                        }
                    }
                }
            }
            catch
            {
                return value;
            }
            finally
            {
                stream?.Close();
                response?.Close();
            }
            return value;
        }
        private static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            var value = false;
            var buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            try
            {
                if (!File.Exists(savePath))
                {

                    File.Delete(savePath);
                    outStream = System.IO.File.Create(savePath);
                    inStream = response.GetResponseStream();
                    var l = 0;
                    do
                    {
                        if (inStream != null) l = inStream.Read(buffer, 0, buffer.Length);
                        if (l > 0) outStream.Write(buffer, 0, l);
                    } while (l > 0);
                    value = true;
                }
                else
                {
                    value = true;
                }
            }
            finally
            {
                outStream?.Close();
                inStream?.Close();
            }
            return value;
        }
        /// 无损压缩图片    
        /// <param name="sFile">原图片</param>    
        /// <param name="dFile">压缩后保存位置</param>    
        /// <param name="dHeight">高度</param>    
        /// <param name="dWidth"></param>    
        /// <param name="flag">压缩质量(数字越小压缩率越高) 1-100</param>    
        /// <returns></returns>    
        public static string GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag, string userID = "")
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;

            if (iSource.Width > 50 || iSource.Height > 50)
            {
                //按比例缩放  
                System.Drawing.Size tem_size = new System.Drawing.Size(iSource.Width, iSource.Height);

                if (tem_size.Width > dHeight || tem_size.Width > dWidth)
                {
                    if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                    {
                        sW = dWidth;
                        sH = (dWidth * tem_size.Height) / tem_size.Width;
                    }
                    else
                    {
                        sH = dHeight;
                        sW = (tem_size.Width * dHeight) / tem_size.Height;
                    }
                }
                else
                {
                    sW = tem_size.Width;
                    sH = tem_size.Height;
                }

                Bitmap ob = new Bitmap(dWidth, dHeight);
                Graphics g = Graphics.FromImage(ob);

                g.Clear(Color.WhiteSmoke);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width,
                    iSource.Height, GraphicsUnit.Pixel);

                g.Dispose();
                //以下代码为保存图片时，设置压缩质量    
                EncoderParameters ep = new EncoderParameters();
                long[] qy = new long[1];
                qy[0] = flag; //设置压缩的比例1-100    
                EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                ep.Param[0] = eParam;
                try
                {
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICIinfo = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICIinfo = arrayICI[x];
                            break;
                        }
                    }
                    FileInfo fileInfo = new FileInfo(sFile);
                    var saveFile = dFile + fileInfo.Name;
                    if (File.Exists(saveFile))
                        return saveFile;
                    if (jpegICIinfo != null)
                    {
                        ob.Save(saveFile, jpegICIinfo, ep); //dFile是压缩后的新路径    
                    }
                    else
                    {
                        ob.Save(dFile, tFormat);
                    }
                    //var tempUserImage = GlobalVariable.UserImages.FirstOrDefault(m => m.UserID == userId);
                    //if (tempUserImage == null)
                    //    GlobalVariable.UserImages.Add(new GlobalVariable.ContactUserImage
                    //    {
                    //        Url = saveFile,
                    //        UserID = userId
                    //    });
                    return saveFile;
                }
                catch
                {
                    return string.Empty;
                }
                finally
                {
                    iSource.Dispose();
                    ob.Dispose();
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(sFile);
                var extension = Path.GetExtension(sFile);
                var saveFile = dFile + fileInfo.Name;
                if (!File.Exists(sFile) && !PublicTalkMothed.IsFileInUsing(saveFile))
                    File.Move(sFile, saveFile);
                //var tempUserImage = UserImages.FirstOrDefault(m => m.UserID == userId);
                //if (tempUserImage == null)
                //    UserImages.Add(new GlobalVariable.ContactUserImage
                //    {
                //        Url = saveFile,
                //        UserID = userId
                //    });
                return saveFile;
            }
        }

    }
}
