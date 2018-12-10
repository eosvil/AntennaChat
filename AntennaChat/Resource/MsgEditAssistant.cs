/*
Author: tanqiyan
Crate date: 2017-05-09
Description：编辑框辅助类
--------------------------------------------------------------------------------------------------------
Versions：
    V1.00 2017-05-09 tanqiyan 描述：消息编辑框不同类型的内容处理
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.Talk;
using AntennaChat.Views.Talk;
using CSharpWin_JD.CaptureImage;
using System.Xml.Linq;
using AntennaChat.Views;
using AntennaChat.ViewModel;

namespace AntennaChat.Resource
{
    /// <summary>
    /// 消息编辑框辅助类
    /// </summary>
    public class MsgEditAssistant
    {
        private RichTextBox _richTextBox;
        string format = "EditMsgList";
        string userIMImageSavePath = string.Empty;
        private string imageSuffix = ".png";
        int imgIndex = 0;
        int hrefIndex = 0;
        int index = 0;
        public MsgEditAssistant(RichTextBox richTextBox, string imageSavePath)
        {
            _richTextBox = richTextBox;
            userIMImageSavePath = imageSavePath;
            if (!Directory.Exists(userIMImageSavePath))
            {
                Directory.CreateDirectory(userIMImageSavePath);
            }
        }
        /// <summary>
        /// 编辑框内复制消息内容处理
        /// </summary>
        public List<MessageContent> CopyMsgContent()
        {
            var selection = _richTextBox.Selection;

            List<MessageContent> elementList = new List<MessageContent>();
            var uiElements = from block in _richTextBox.Document.Blocks
                             from inline in (block as Paragraph).Inlines
                             where inline.GetType() == typeof(InlineUIContainer) || inline.GetType() == typeof(Run)
                             select inline;

            var enumerable = uiElements as Inline[] ?? uiElements.ToArray();
            if (!enumerable.Any()) return elementList;
            foreach (var element in enumerable)
            {
                var containsLeft = selection.Contains(element.ContentStart);
                var containsRight = selection.Contains(element.ContentEnd);
                if (containsLeft == containsRight && containsLeft)
                {
                    if (element is InlineUIContainer)
                    {
                        InlineUIContainer line = element as InlineUIContainer;
                        if (line.Child is Image)
                        {
                            var image = line.Child as Image;
                            if (image.Tag != null && image.Tag.ToString() == "Emoji")
                            {
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Emoji,
                                    MsgContent = image.Source?.ToString() ?? string.Empty
                                });
                            }
                            else
                            {
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Image,
                                    MsgContent = image.Source?.ToString() ?? string.Empty
                                });
                            }

                        }
                    }
                    else if (element is Run)
                    {
                        Run run = element as Run;
                        //Run tempRun = new Run(run.Text);
                        if (elementList.Count == 0 || elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji
                            || elementList[elementList.Count - 1].MsgType != MsgContentType.Image)
                            elementList.Add(new MessageContent() { MsgType = MsgContentType.Text, MsgContent = run.Text });
                        else
                            elementList[elementList.Count - 1].MsgContent += run.Text;
                    }
                }
                else if (element is Run)
                {
                    if (containsRight)
                    {
                        var partialText = selection.Start.GetTextInRun(LogicalDirection.Forward);
                        if (!string.IsNullOrEmpty(partialText))
                        {
                            Run run = new Run(partialText);
                            if (elementList.Count == 0 ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Image)
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Text,
                                    MsgContent = run.Text
                                });
                            else
                                elementList[elementList.Count - 1].MsgContent += run.Text;
                        }
                    }
                    else if (containsLeft)
                    {
                        var partialText = selection.End.GetTextInRun(LogicalDirection.Backward);
                        if (!string.IsNullOrEmpty(partialText))
                        {
                            Run run = new Run(partialText);
                            if (elementList.Count == 0 ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Emoji ||
                                elementList[elementList.Count - 1].MsgType != MsgContentType.Image)
                                elementList.Add(new MessageContent()
                                {
                                    MsgType = MsgContentType.Text,
                                    MsgContent = run.Text
                                });
                            else
                                elementList[elementList.Count - 1].MsgContent += run.Text;
                        }
                    }
                }

            }
            var isHasImg = elementList.Exists(m => m.MsgType == MsgContentType.Emoji || m.MsgType == MsgContentType.Image);
            var isHasEmoji = elementList.Exists(m => m.MsgType == MsgContentType.Emoji);
            var isHasText = elementList.Exists(m => m.MsgType == MsgContentType.Text && !string.IsNullOrEmpty(m.MsgContent));

            var isHasTextImg = elementList.Exists(m =>
                m.MsgType == MsgContentType.Emoji && m.MsgType == MsgContentType.Image &&
                m.MsgType == MsgContentType.Text);

            //如果只有复制图片或文字，就不重新定义剪切板格式（为了给外部软件使用，比如：QQ）
            if ((isHasText && isHasImg) || isHasEmoji)
            {
                Clipboard.Clear();
                Clipboard.SetData(format, elementList);
            }
            else if (isHasImg)
            {
                string[] file = new string[elementList.Count];
                for (int i = 0; i < elementList.Count; i++)
                {
                    if (elementList[i].MsgContent.Contains("file:///"))
                    {
                        file[i] = elementList[i].MsgContent.Replace("file:///", "").Replace("/", "//");
                    }
                    else if (elementList[i].MsgContent.Contains("pack://application:,,,/AntennaChat;Component"))
                    {
                        file[i] = elementList[i].MsgContent.Replace("pack://application:,,,/AntennaChat;Component/", AppDomain.CurrentDomain.BaseDirectory).Replace("/", "//");
                    }


                }
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.FileDrop, file);
                Clipboard.SetDataObject(dataObject, true);
            }
            else if (isHasText)
            {
                Clipboard.Clear();
            }
            return elementList;
        }
        /// <summary>
        /// 编辑框内粘贴消息内容处理
        /// </summary>
        public void PasteMsgContent()
        {
            var result = Clipboard.GetData(format);
            if (result != null)
            {
                if (!_richTextBox.Selection.IsEmpty)
                {
                    TextRange textRange = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End);
                    textRange.Text = "";
                }
                var elementList = result as List<MessageContent>;
                foreach (var element in elementList)
                {
                    switch (element.MsgType)
                    {
                        //表情
                        case MsgContentType.Emoji:
                            Win_Face_GetUrl("", element.MsgContent);
                            break;
                        //图片
                        case MsgContentType.Image:
                            ImageOnLoad(element.MsgContent);
                            break;
                        //文字
                        case MsgContentType.Text:
                            InsertText(_richTextBox, element.MsgContent);
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// 编辑框外部复制的消息内容处理
        /// </summary>
        public void PasteExternalMsgContent()
        {
            try
            {
                //如果有选中内容先删除
                if (!_richTextBox.Selection.IsEmpty)
                {
                    TextRange textRange = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End);
                    textRange.Text = "";
                }
                imgIndex = 0;
                hrefIndex = 0;
                index = 0;
                //粘贴资源图文混合（来源：网站、QQ）
                var setHtmlData = Clipboard.GetData(System.Windows.Forms.DataFormats.Html);
                bool isContainsPar = false;
                if (setHtmlData != null)
                {
                    var strHtml = ReplaceHtml_IPB((string)setHtmlData);
                    strHtml = strHtml.Replace("<br/>", "\r\n");
                    string[] msgContents;
                    if (strHtml.Contains("</p>"))
                    {
                        isContainsPar = true;
                        msgContents = System.Text.RegularExpressions.Regex.Split(strHtml, "</p>");
                    }
                    else
                    {
                        strHtml = strHtml.Replace("<img>", "</p><img></p>");
                        strHtml = strHtml.Replace("<br/>", "\r\n");
                        msgContents = System.Text.RegularExpressions.Regex.Split(strHtml, "</p>");
                    }

                    if (imgUrlList.Length == 0 && hrefUrlList.Length == 0)
                    {
                        System.Windows.IDataObject textData = Clipboard.GetDataObject();
                        if (textData != null && textData.GetDataPresent(System.Windows.Forms.DataFormats.UnicodeText))
                        {
                            //string text = textData.GetData(System.Windows.Forms.DataFormats.UnicodeText) as string;
                            strHtml = strHtml.Replace("</p>", "\r\n");
                            strHtml = Regex.Replace(strHtml, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                            strHtml = Regex.Replace(strHtml, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                            strHtml = Regex.Replace(strHtml, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                            strHtml = Regex.Replace(strHtml, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                            InsertText(_richTextBox, strHtml);
                            return;
                        }
                    }
                    HtmlMsgContentChange(msgContents, isContainsPar);
                    return;
                }
                //粘贴资源只是图片
                var imagelist = Clipboard.GetData(System.Windows.Forms.DataFormats.FileDrop);
                var tempImageList = imagelist as string[];
                if (tempImageList != null)
                {
                    foreach (var imageUrl in tempImageList)
                    {
                        if (!File.Exists(imageUrl))
                            return;

                        var fileInfo = new FileInfo(imageUrl);
                        //File.Open(imageUrl, FileMode.Open);
                        //var filePath = userIMImageSavePath + Guid.NewGuid().ToString("n") + fileInfo.Extension;
                        //File.Copy(imageUrl, filePath, true);
                        var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                        if (size > 10)
                        {
                            MessageBoxWindow.Show("图片超过10M，请以文件方式发出。", GlobalVariable.WarnOrSuccess.Warn);
                            return;
                        }
                        ImageOnLoad(imageUrl);
                    }
                    return;
                }
                var imageLst = Clipboard.GetFileDropList();
                if (imageLst.Count > 0)
                {
                    foreach (var imageUrl in imageLst)
                    {
                        if (!File.Exists(imageUrl))
                            return;
                        var fileInfo = new FileInfo(imageUrl);
                        //var filePath = userIMImageSavePath + Guid.NewGuid().ToString("n") + fileInfo.Extension;
                        //File.Copy(imageUrl, filePath, true);
                        var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                        if (size > 10)
                        {
                            MessageBoxWindow.Show("图片超过10M，请以文件方式发出。", GlobalVariable.WarnOrSuccess.Warn);
                            return;
                        }
                        ImageOnLoad(imageUrl);
                    }
                    return;
                }

                System.Windows.IDataObject IData = Clipboard.GetDataObject();
                if (IData == null)
                    return;
                var list = IData.GetFormats();
                if (IData.GetDataPresent(System.Windows.Forms.DataFormats.UnicodeText))
                {
                    string text = IData.GetData(System.Windows.Forms.DataFormats.UnicodeText) as string;
                    InsertText(_richTextBox, text);
                }
                if (IData.GetDataPresent(System.Windows.DataFormats.Bitmap))
                {
                    MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
                    try
                    {
                        string path = string.Empty;
                        if (ms != null)
                        {
                            GC.Collect();
                            byte[] dibBuffer = new byte[ms.Length];
                            ms.Read(dibBuffer, 0, dibBuffer.Length);

                            BITMAPINFOHEADER infoHeader =
                                BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                            int infoHeaderSize = infoHeader.biSize;
                            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                            fileHeader.bfType = BITMAPFILEHEADER.BM;
                            fileHeader.bfSize = fileSize;
                            fileHeader.bfReserved1 = 0;
                            fileHeader.bfReserved2 = 0;
                            fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                            byte[] fileHeaderBytes =
                                BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

                            MemoryStream msBitmap = new MemoryStream();
                            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
                            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
                            msBitmap.Seek(0, SeekOrigin.Begin);
                            string imgName = Guid.NewGuid().ToString();
                            path = userIMImageSavePath + imgName;
                            System.Drawing.Image imagePre = System.Drawing.Image.FromStream(msBitmap, true);
                            SaveImg(imagePre, path);
                            imagePre.Dispose();
                            msBitmap.Dispose();
                            ms.Dispose();

                            GC.Collect();
                        }
                        ImageOnLoad(path + imageSuffix);
                    }
                    catch (Exception ex)
                    {
                        if (ms != null)
                            LogHelper.WriteError("DeviceIndependentBitmap----资源流长度：" + ms.Length + "----------" + ex.Source + ex.StackTrace + ex.Message);
                        else
                        {
                            LogHelper.WriteError("DeviceIndependentBitmap" + ex.Source + ex.StackTrace + ex.Message);
                        }
                        ms.Dispose();
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("TalkViewModel_MiPaste_Click:" + ex.Source + ex.StackTrace + ex.Message);
            }
        }
        /// <summary>
        /// HTML 剪贴板格式中的消息内容
        /// </summary>
        /// <param name="msgContents"></param>
        /// <param name="isContainsPar"></param>
        private void HtmlMsgContentChange(string[] msgContents, bool isContainsPar)
        {
            foreach (var content in msgContents)
            {
                if (!string.IsNullOrEmpty(content.Trim()))
                {
                    int imgCount = SubstringCount(content, "<img>");
                    int hrefCount = SubstringCount(content, "<a>");
                    if (imgCount > 0)
                    {

                        var tempContent = content.Replace("<img>", "");
                        if (!string.IsNullOrEmpty(tempContent.Trim()))
                        {
                            tempContent = content.Replace("<img>", "</p><img></p>");
                            var msgImageContents = System.Text.RegularExpressions.Regex.Split(tempContent, "</p>");
                            if (msgImageContents.Length > 0)
                            {
                                HtmlMsgContentChange(msgImageContents, false);
                                continue;
                            }

                        }
                        for (int i = 0; i < imgCount; i++)
                        {
                            if (imgUrlList.Length > imgIndex)
                            {

                                var imgurl = imgUrlList[imgIndex].Replace("file:///", "");
                                //是否是本地图片
                                if (File.Exists(imgurl))
                                {
                                    if (imgUrlList[imgIndex].Contains("file:///"))
                                    {
                                        var isImg = IsPicture(imgurl);
                                        FileInfo fileInfo = new FileInfo(imgurl);
                                        var filePath = "";
                                        if (isImg)
                                        {
                                            filePath = Guid.NewGuid().ToString("n") + fileInfo.Extension;
                                        }
                                        else
                                        {
                                            filePath = userIMImageSavePath + Guid.NewGuid().ToString("n") + ".png";
                                        }

                                        var imagePath = ImageHandle.DownloadPictureFromFile(imgUrlList[imgIndex], userIMImageSavePath + Guid.NewGuid().ToString("n") + fileInfo.Extension);
                                        if (File.Exists(imagePath))
                                            ImageOnLoad(imagePath);
                                    }
                                    else
                                    {
                                        FileInfo fileInfo = new FileInfo(imgurl);
                                        if (fileInfo.Directory != null && fileInfo.Directory.FullName == AppDomain.CurrentDomain.BaseDirectory + "Emoji")
                                        {
                                            Win_Face_GetUrl(fileInfo.Name, fileInfo.FullName);
                                        }
                                        else
                                        {
                                            //string filePath = userIMImageSavePath + Guid.NewGuid().ToString("n") + fileInfo.Extension;
                                            //File.Copy(imgurl, filePath, true);
                                            ImageOnLoad(imgurl);
                                        }
                                    }

                                }
                                else
                                {
                                    if (imgUrlList[imgIndex].Contains("file:///") && !publicMethod.IsUrlRegex(imgurl))
                                    {
                                        var isImg = IsPicture(imgurl);
                                        FileInfo fileInfo = new FileInfo(imgurl);
                                        var filePath = "";
                                        if (isImg)
                                        {
                                            filePath = Guid.NewGuid().ToString("n") + fileInfo.Extension;
                                        }
                                        else
                                        {
                                            filePath = userIMImageSavePath + Guid.NewGuid().ToString("n") + ".png";
                                        }
                                        var imagePath = ImageHandle.DownloadPictureFromFile(imgUrlList[imgIndex], userIMImageSavePath + Guid.NewGuid().ToString("n") + fileInfo.Extension);
                                        if (File.Exists(imagePath))
                                            ImageOnLoad(imagePath);
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(imgurl))
                                        {
                                            if (imgurl.Length > 33)
                                            {
                                                var isImg = IsPicture(imgurl);
                                                string filePath;
                                                var imageExtension = imgurl.Substring(imgurl.Length - 33,
                                                       imgurl.Length - (imgurl.Length - 33));
                                                imageExtension = RemoveSpecialCharacter(imageExtension);
                                                if (isImg)
                                                {
                                                    filePath = userIMImageSavePath + imageExtension;
                                                }
                                                else
                                                {
                                                    filePath = userIMImageSavePath + imageExtension + ".png";
                                                }
                                                if (!File.Exists(filePath) && publicMethod.IsUrlRegex(imgurl))
                                                {
                                                    DownloadPicture(imgurl, filePath, -1);
                                                }
                                                if (File.Exists(filePath))
                                                    ImageOnLoad(filePath);
                                            }
                                            else
                                            {
                                                var imageExtension = RemoveSpecialCharacter(imgurl);
                                                var filePath = userIMImageSavePath + imageExtension;
                                                if (!File.Exists(filePath) && publicMethod.IsUrlRegex(imgurl))
                                                {
                                                    DownloadPicture(imgurl, filePath, -1);
                                                }
                                                if (File.Exists(filePath))
                                                {
                                                    FileInfo fileInfo = new FileInfo(filePath);
                                                    var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                                                    if (size > 10)
                                                    {
                                                        MessageBoxWindow.Show("消息内容中不能包含超过10M的图片，请换张图。", GlobalVariable.WarnOrSuccess.Warn);
                                                        return;
                                                    }
                                                    ImageOnLoad(imgurl);
                                                }
                                            }
                                        }
                                    }
                                }
                                imgIndex++;
                                //if (imgIndex < i || imgIndex == 0)
                            }
                        }
                        if (isContainsPar)
                        {
                            if (index < msgContents.Length - 1)
                                _richTextBox.CaretPosition.InsertLineBreak();
                        }
                        continue;
                    }
                    //文本为超链接
                    if (hrefCount > 0)
                    {
                        string tempContent = content;
                        if (!string.IsNullOrEmpty(tempContent))
                        {
                            for (int i = 0; i < hrefCount; i++)
                            {
                                if (hrefUrlList.Length > hrefIndex)
                                {
                                    var hrefUrl = hrefUrlList[hrefIndex];
                                    var indexOf = tempContent.IndexOf("<a>", StringComparison.Ordinal);
                                    tempContent = tempContent.Remove(indexOf, 3);
                                    tempContent = tempContent.Insert(indexOf, hrefUrl);
                                    hrefIndex++;
                                }
                            }
                            tempContent = Regex.Replace(tempContent, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                            tempContent = Regex.Replace(tempContent, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                            tempContent = Regex.Replace(tempContent, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                            tempContent = Regex.Replace(tempContent, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                            if (index < msgContents.Length - 1 && isContainsPar)
                                InsertText(_richTextBox, tempContent + "\r\n");
                            else
                                InsertText(_richTextBox, tempContent);
                        }
                        continue;
                    }
                    var msgContent = content;
                    msgContent = Regex.Replace(msgContent, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                    msgContent = Regex.Replace(msgContent, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                    msgContent = Regex.Replace(msgContent, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                    msgContent = Regex.Replace(msgContent, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                    if (index < msgContents.Length && isContainsPar)
                        InsertText(_richTextBox, msgContent + "\r\n");
                    else
                        InsertText(_richTextBox, msgContent);
                }
            }

        }
        // 判断文件是否是图片
        public bool IsPicture(string fileName)
        {
            string strFilter = ".jpeg|.gif|.jpg|.png|.bmp|.pic|.tiff|.ico|.iff|.lbm|.mag|.mac|.mpt|.opt|";
            char[] separtor = { '|' };
            string[] tempFileds = StringSplit(strFilter, separtor);
            foreach (var str in tempFileds)
            {
                if (str.ToUpper() == fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal), fileName.Length - fileName.LastIndexOf(".", StringComparison.Ordinal)).ToUpper()) { return true; }
            }
            return false;
        }
        // 通过字符串，分隔符返回string[]数组 
        public string[] StringSplit(string s, char[] separtor)
        {
            string[] tempFileds = s.Trim().Split(separtor); return tempFileds;
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="picUrl">图片Http地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeOut">Request最大请求时间，如果为-1则无限制</param>
        /// <returns></returns>
        public static bool DownloadPicture(string picUrl, string savePath, int timeOut)
        {
            bool value = false;
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                if (timeOut != -1) request.Timeout = timeOut;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("Text/"))
                    value = SaveBinaryFile(response, savePath);
            }
            catch (System.Net.WebException x)
            {

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
            bool value = false;
            byte[] buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            try
            {
                if (File.Exists(savePath))
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
            finally
            {
                outStream?.Close();
                inStream?.Close();
            }
            return value;
        }
        public static String RemoveSpecialCharacter(string hexData)
        {
            return Regex.Replace(hexData, "[ \\[ \\] \\^ \\-_*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,;\"/‘’“”-]", "").ToLower();
        }
        /// <summary>
        ///统计指定字符串出现的次数 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring"></param>
        private static int SubstringCount(string str, string substring)
        {
            if (str.Contains(substring))
            {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }
            return 0;
        }

        private string stripHtml(string strHtml)
        {
            Regex objRegExp = new Regex("<(.|\n)+?>");
            string strOutput = objRegExp.Replace(strHtml, "");
            strOutput = strOutput.Replace("<", "&lt;");
            strOutput = strOutput.Replace(">", "&gt;");
            return strOutput;
        }

        //图片Url
        private string[] imgUrlList;
        //超链接Url
        private string[] hrefUrlList;
        /// <summary>
        /// html解析
        /// </summary>
        /// <param name="str"></param>
        private string ReplaceHtml_IPB(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                //删除内含的 样式表代码
                Regex headhtml = new Regex(@"^(\w|\W)*<body(.*?)>", RegexOptions.IgnoreCase);
                string TempStr = headhtml.Replace(str, "");

                Regex bodyhtml = new Regex(@"^(\w|\W)*<body>", RegexOptions.IgnoreCase);
                TempStr = bodyhtml.Replace(TempStr, "");
                Regex bodyhtml1 = new Regex(@"^(\w|\W)*<BODY([^>])*>", RegexOptions.IgnoreCase);
                TempStr = bodyhtml1.Replace(TempStr, "");
                Regex styleHtml = new Regex(@"<style([^>])*>(\w|\W)*?</style([^>])*>", RegexOptions.IgnoreCase);
                TempStr = styleHtml.Replace(TempStr, "");
                Regex RightUserImgRemove = new Regex("<div class=\"rightimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = RightUserImgRemove.Replace(TempStr, "");
                Regex LeftUserImgRemove = new Regex("<div class=\"leftimg\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = LeftUserImgRemove.Replace(TempStr, "");
                Regex OnceSendFailImgRemove = new Regex("<div class=\"onceSendFail\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = OnceSendFailImgRemove.Replace(TempStr, "");
                Regex OnceSendImgRemove = new Regex("<div class=\"onceSend\".*?>.*?</div>", RegexOptions.IgnoreCase);
                TempStr = OnceSendImgRemove.Replace(TempStr, "");
                //< div class="onceSend"
                //<([^>]+)> 不过滤 img标签
                TempStr = TempStr.Replace("</p>", "[/p]");
                TempStr = TempStr.Replace("</P>", "[/p]");
                TempStr = TempStr.Replace("<p>", "[p]");
                TempStr = TempStr.Replace("<P>", "[p]");
                TempStr = TempStr.Replace("</div>", "[/div]");
                TempStr = TempStr.Replace("</Div>", "[/div]");
                TempStr = TempStr.Replace("<div>", "[div]");
                TempStr = TempStr.Replace("<Div>", "[div]");
                Regex pHtml = new Regex("<p(.*?)>", RegexOptions.IgnoreCase);
                TempStr = pHtml.Replace(TempStr, "[/p]");
                //TempStr = Regex.Replace(TempStr, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
                //TempStr = Regex.Replace(TempStr, @"&#32;", " ", RegexOptions.IgnoreCase);
                TempStr = Regex.Replace(TempStr, @"&#(\d+);", " ", RegexOptions.IgnoreCase);
                hrefUrlList = GetHrefUrls(TempStr);
                imgUrlList = GetHvtImgUrls(TempStr);
                Regex trHtml = new Regex("<tr(.*?)>", RegexOptions.IgnoreCase);
                TempStr = trHtml.Replace(TempStr, "[br/]");
                Regex tdHtml = new Regex("<td(.*?)>", RegexOptions.IgnoreCase);
                TempStr = tdHtml.Replace(TempStr, "   ");
                Regex liHtml = new Regex("<li([^>])*>", RegexOptions.IgnoreCase);
                TempStr = liHtml.Replace(TempStr, "[br/]");
                Regex brHtml = new Regex("<br(.*?)>", RegexOptions.IgnoreCase);
                TempStr = brHtml.Replace(TempStr, "[br/]");
                Regex spanHtml1 = new Regex("<span(.*?)>", RegexOptions.IgnoreCase);
                TempStr = spanHtml1.Replace(TempStr, "[span]");
                Regex spanHtml2 = new Regex("</span>", RegexOptions.IgnoreCase);
                TempStr = spanHtml2.Replace(TempStr, "[/span]");
                Regex imgHtml = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\t\r]*[""']?[\t\r\n]*(?<imgUrl>[^\t\r\n""'<>]*)[^<>]*?/?[\t\r\n]*>", RegexOptions.IgnoreCase);
                TempStr = imgHtml.Replace(TempStr, "[img]");
                //Regex imgHtml1 = new Regex("<IMG(.*?)>", RegexOptions.IgnoreCase);
                //TempStr = imgHtml1.Replace(TempStr, "[img]");
                Regex cutHtml = new Regex("<([^>]+)>", RegexOptions.IgnoreCase);
                TempStr = cutHtml.Replace(TempStr, "");
                Regex aHtml = new Regex("<a class=\"sup-anchor\".*?>", RegexOptions.IgnoreCase);
                TempStr = aHtml.Replace(TempStr, "");
                Regex aHtml1 = new Regex("<a class=\"sub-anchor\".*?>", RegexOptions.IgnoreCase);
                TempStr = aHtml1.Replace(TempStr, "");
                Regex aHtml2 = new Regex("<a name(.*?)></a>", RegexOptions.IgnoreCase);
                TempStr = aHtml2.Replace(TempStr, "");
                Regex aHtml3 = new Regex("<a id(.*?)>", RegexOptions.IgnoreCase);
                TempStr = aHtml3.Replace(TempStr, "");
                Regex regTitle = new Regex(@"<a\s+.*?href=""([^""]*)""\s+.*?title=""([^""]*)"".*?>", RegexOptions.IgnoreCase);
                TempStr = regTitle.Replace(TempStr, "");
                Regex aHtml4 = new Regex("<a(.*?)>.*?</a>", RegexOptions.IgnoreCase);
                TempStr = aHtml4.Replace(TempStr, "[a] ");
                //TempStr = TempStr.Replace ("/>" , ">");
                //Regex ImgHtml=new Regex("<img",RegexOptions.IgnoreCase);
                //格式化现有代码
                //TempStr = HttpUtility.HtmlEncode(TempStr);

                //TempStr = TempStr.Replace("[body]", "<body>");
                //TempStr = TempStr.Replace("[/body]", "</body>");
                TempStr = TempStr.Replace("[img]", " <img>");
                TempStr = TempStr.Replace("[span]", "");
                //TempStr = TempStr.Replace("[a", "<a");
                TempStr = TempStr.Replace("[a]", "<a>");
                TempStr = TempStr.Replace("[p]", "<p>");
                TempStr = TempStr.Replace("\r\n", "");
                TempStr = TempStr.Replace("[/p]", "</p>");
                TempStr = TempStr.Replace("[br/]", "<br/>");
                TempStr = TempStr.Replace("[/span]", "");
                TempStr = TempStr.Replace("[/div]", "</p>");
                TempStr = TempStr.Replace("[div]", "");
                TempStr = TempStr.Trim("\r\n".ToCharArray());
                return TempStr;

            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 解析HTML中Image的Url
        /// </summary>
        /// <param name="sHtmlText"></param>
        private static string[] GetHvtImgUrls(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex m_hvtRegImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\t\r]*[""']?[\t\r\n]*(?<imgUrl>[^\t\r\n""'<>]*)[^<>]*?/?[\t\r\n]*>", RegexOptions.IgnoreCase);
            // 搜索匹配的字符串 
            MatchCollection matches = m_hvtRegImg.Matches(sHtmlText);
            int m_i = 0;
            string[] sUrlList = new string[matches.Count];
            // 取得匹配项列表 
            foreach (Match match in matches)
                sUrlList[m_i++] = match.Groups["imgUrl"].Value.Replace("&amp;", "&");
            return sUrlList;
        }
        /// <summary>
        /// 解析HTML中超链的Url
        /// </summary>
        /// <param name="sHtmlText"></param>
        private static string[] GetHrefUrls(string sHtmlText)
        {
            Regex reg = new Regex(@"(?is)<a[^>]*?href=""(?<href>([^>]*))""\s*>(?<value>(.*?))</a>", RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(sHtmlText);
            string[] sUrlList = new string[matches.Count];
            int m_i = 0;
            foreach (Match m in matches)
            {
                var TempStr = string.Empty;
                var url = m.Groups["value"].Value;
                TempStr = url.Replace("<u>", "");
                TempStr = TempStr.Replace("</u>", "");
                Regex spanHtml1 = new Regex("<span(.*?)>", RegexOptions.IgnoreCase);
                TempStr = spanHtml1.Replace(TempStr, "");
                TempStr = TempStr.Replace("</span>", "");
                Regex emRemove = new Regex("<em .*?>.*?</em>", RegexOptions.IgnoreCase);
                TempStr = emRemove.Replace(TempStr, "");
                Regex imgHtml = new Regex("<img(.*?)>", RegexOptions.IgnoreCase);
                TempStr = imgHtml.Replace(TempStr, "");
                sUrlList[m_i++] = TempStr;
            }
            if (matches.Count == 0)
            {
                Regex regTitle = new Regex(@"(?is)<a[^>]+?href=(['""]?)(?<url>[^'""\s>]+).*?title=""(.*?)""\1[^>]*>(?<value>(?:(?!</?a\b).)*)</a>", RegexOptions.IgnoreCase);
                matches = reg.Matches(sHtmlText);
                sUrlList = new string[matches.Count];
                foreach (Match m in matches)
                {
                }
            }
            return sUrlList;
        }

        /// <summary>
        /// 装载表情图片
        /// </summary>
        /// <param name="faceTag"></param>
        /// <param name="completeUrl"></param>
        public void Win_Face_GetUrl(string faceTag, string completeUrl = "")
        {
            try
            {
                string imgurl = faceTag;
                var bi = string.IsNullOrEmpty(completeUrl) ? new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Emoji/" + imgurl)) : new BitmapImage(new Uri(completeUrl));
                Image image = new Image
                {
                    Source = bi,
                    Tag = "Emoji"
                };

                if (bi.Height > 100 || bi.Width > 100)
                {
                    image.Height = bi.Height * 0.2;
                    image.Width = bi.Width * 0.2;
                }
                else
                {
                    image.Height = bi.Height;
                    image.Width = bi.Width;
                }
                image.Stretch = System.Windows.Media.Stretch.Fill;

                AddElement(image);
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 图片装载
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        public void ImageOnLoad(string imagePath)
        {
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(imagePath);
                if (File.Exists(imagePath))
                {
                    FileInfo fileInfo = new FileInfo(imagePath);
                    var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                    if (size > 1)
                        bi.DecodePixelWidth = 600;
                }
                bi.EndInit();
                bi.Freeze();
                Image image = new Image();
                image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
                image.Source = bi;
                image.ToolTip = "双击查看原图";
                image.Tag = "cut";
                if (bi.Height > 80 || bi.Width > 150)
                {
                    image.Height = bi.Height * 0.3;
                    image.Width = bi.Width * 0.3;
                }
                else
                {
                    image.Height = bi.Height;
                    image.Width = bi.Width;
                }

                image.Stretch = System.Windows.Media.Stretch.Fill;
                AddElement(image);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[MsgEditAssistant_ImageOnLoad]图片路径:" + imagePath + "----------------------------------" + ex.Message + ex.StackTrace + ex.Source);
            }

        }
        /// <summary>
        ///双击查看编辑框图片
        /// </summary>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Image image = sender as Image;
                if (string.IsNullOrEmpty(image.Source.ToString())) return;
                //PictureViewerView win = new PictureViewerView();
                //PictureViewerViewModel model = new PictureViewerViewModel(image.Source.ToString());
                //win.DataContext = model;
                //win.Owner = Antenna.Framework.Win32.GetTopWindow();
                //win.Show();
                if (ImageHandle.PicView == null)
                {
                    ImageHandle.PicView = new PictureViewerView();
                    ImageHandle.PicViewModel = new PictureViewerViewModel(image.Source.ToString());
                    ImageHandle.PicView.DataContext = ImageHandle.PicViewModel;
                    ImageHandle.PicView.Owner = Antenna.Framework.Win32.GetTopWindow();
                    ImageHandle.PicView.Show();
                }
                else
                {
                    ImageHandle.PicView.Close();
                    ImageHandle.PicView = null;
                    ImageHandle.PicViewModel = null;
                    ImageHandle.PicView = new PictureViewerView();
                    ImageHandle.PicViewModel = new PictureViewerViewModel(image.Source.ToString());
                    ImageHandle.PicView.DataContext = ImageHandle.PicViewModel;
                    ImageHandle.PicView.Owner = Antenna.Framework.Win32.GetTopWindow();
                    ImageHandle.PicView.Show();
                }
            }
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="path"></param>
        public void SaveImg(System.Drawing.Image img, string path)
        {
            try
            {

                System.Drawing.Image imgSave = img;
                imgSave.Save(path + imageSuffix, System.Drawing.Imaging.ImageFormat.Png);
                imgSave.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_SaveImg]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 添加图片元素并定位
        /// </summary>
        public void AddElement(UIElement element)
        {
            _richTextBox.Focus();
            TextPointer point = _richTextBox.Selection.Start;
            InlineUIContainer uiContainer = new InlineUIContainer(element, point);
            TextPointer nextPoint = uiContainer.ContentEnd;
            _richTextBox.CaretPosition = nextPoint;
        }
        /// <summary>
        /// 编辑框添加字符串
        /// </summary>
        public void InsertText(System.Windows.Controls.RichTextBox tb, string text)
        {
            if (tb.IsSelectionActive)
            {
                TextPointer textPointer = tb.Selection.Start;
                Run run = new Run(text, textPointer);
                TextPointer pointer = run.ContentEnd;
                if (!tb.Selection.IsEmpty)
                {
                    TextRange textRange = new TextRange(pointer, tb.Selection.End);
                    textRange.Text = "";
                }
                tb.CaretPosition = pointer;
            }
            else
            {
                TextPointer textPointer = tb.Selection.Start;
                Run run = new Run(text, textPointer);
                TextPointer pointer = run.ContentEnd;
                tb.CaretPosition = pointer;
            }
        }

        /// <summary>
        /// 截图方法
        /// </summary>
        public void CutImage()
        {
            try
            {
                CaptureImageTool capImg = new CaptureImageTool();
                capImg.Closed += CapImg_Closed;
                GlobalVariable.isCutShow = true;
                if (capImg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string imgName = Guid.NewGuid().ToString();
                    string path = userIMImageSavePath + imgName;
                    SaveImg(capImg.Image, path);


                    //SendCutImageDto scid = new SendCutImageDto();
                    //scid.cmpcd = s_ctt.companyCode;
                    //scid.seId = s_ctt.sessionId;
                    //scid.file = path + imageSuffix;
                    //scid.fileFileName = imgName;

                    //System.Drawing.Bitmap pic = new System.Drawing.Bitmap(scid.file);

                    //scid.imageWidth = pic.Width.ToString();
                    //scid.imageHeight = pic.Height.ToString();

                    //pic.Dispose();

                    ImageOnLoad(path + imageSuffix);
                }
                capImg.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[MsgEditAssistant-CutImage]" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        private void CapImg_Closed(object sender, EventArgs e)
        {
            GlobalVariable.isCutShow = false;
        }
        /// <summary>
        /// 添加本地图片消息
        /// </summary>
        public void ImgImagesUpload()
        {
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.Filter = "图片文件(*.jpg;*.jpeg;*.bmp;*.png)|*.jpg;*.jpeg;*.bmp;*.png";
            openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFile.FilterIndex = 0;
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var listFile in openFile.FileNames)
                {

                    //string imgName = Guid.NewGuid().ToString();
                    //string path = userIMImageSavePath + imgName;
                    //SaveImg(System.Drawing.Image.FromFile(listFile), path);
                    FileInfo fileInfo = new FileInfo(listFile);
                    var size = Math.Round((double)fileInfo.Length / 1024 / 1024, 2);
                    if (size > 10)
                    {
                        MessageBoxWindow.Show("图片超过10M，请以文件方式发出。", GlobalVariable.WarnOrSuccess.Warn);
                        return;
                    }
                    ImageOnLoad(listFile);
                }
            }
        }

        #region 加载表情
        WrapPanel st = null;
        Popup p;
        public void ShowPopupWin(object popup)
        {
            p = popup as Popup;
            st = p.Child as WrapPanel;
            p.VerticalOffset = -190;
            p.HorizontalOffset = 2;
            if (this.p.IsOpen == false)
            {
                if (this.st.Children.Count == 0)
                {
                    Init();
                }
                this.p.IsOpen = true;

            }
            else
            {
                this.p.IsOpen = false;
            }
        }
        Border Def_bd;
        /// <summary>
        /// 加载表情
        /// </summary>
        public void Init()
        {
            XDocument adList = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "Emoji\\face.xml");
            var CM_FRIEND_TREEVIEW = from cm_FRIEND_TREEVIEW
                                         in adList.Descendants("StockFace").Elements("Face")
                                     select cm_FRIEND_TREEVIEW;
            //开始读取默认表情菜单
            foreach (var item in CM_FRIEND_TREEVIEW)
            {
                Def_bd = new Border();
                Def_bd.MouseDown += Def_bd_MouseDown;
                Def_bd.Height = 30;
                Def_bd.Width = 30;
                Def_bd.BorderBrush = Brushes.SkyBlue;
                Def_bd.BorderThickness = new System.Windows.Thickness(0.5);
                Def_bd.ToolTip = item.Attribute("name").Value + " 快捷键: " + item.Attribute("shortcut").Value;
                Def_bd.Style = (System.Windows.Style)System.Windows.Application.Current.FindResource("Normal_Border_Style");
                Image Def_img = new Image();
                Def_img.Stretch = Stretch.None;
                Def_img.Margin = new Thickness(2, 2, 2, 2);
                Def_img.Source = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Emoji/" + item.Attribute("md5").Value + "." + item.Attribute("type").Value));
                Def_img.Tag = item.Attribute("md5").Value + "." + item.Attribute("type").Value;
                Def_bd.Child = Def_img;
                this.st.Children.Add(Def_bd);
            }
        }

        private void Def_bd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            object o = e.OriginalSource;
            Image img = (sender as Border).Child as Image;
            string imgURL = img.Tag.ToString();
            Win_Face_GetUrl(imgURL);
            this.p.IsOpen = false;

        }
        #endregion
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }
    }
    [Serializable]
    public class MessageContent
    {
        public MsgContentType MsgType { get; set; }
        public string MsgContent { get; set; }
    }
    /// <summary>
    /// 消息内容类型
    /// </summary>
    public enum MsgContentType
    {
        Text,
        Image,
        Emoji
    }

    public static class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
