using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntennaChat.ViewModel.FileUpload
{
    public class fileShowImage
    {
        /// <summary>
        /// WPF 图片显示方法
        /// </summary>
        /// <param name="extendName"></param>
        /// <returns></returns>
        public static string switchShowImage(string extendName)
        {
            string path = "";
            switch(extendName.ToLower())
            {
                case "docx":
                case "doc":
                    path = "pack://application:,,,/AntennaChat;Component/Images/word.png";
                    break;
                case "xlsx":
                case "xls":
                    path = "pack://application:,,,/AntennaChat;Component/Images/excel.png";
                    break;
                default:
                    path = "";
                    break;
            }
            return path;
        }
        public static string showImageHtmlPath(string extendName,string imgPath)
        {
            string path = "";
            switch (extendName.ToLower())
            {
                case "docx":
                case "doc":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/word.png").Replace(@"\", @"/");
                    break;
                case "xlsx":
                case "xls":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/excel.png").Replace(@"\", @"/");
                    break;
                case "txt":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/txt.png").Replace(@"\", @"/");
                    break;
                case "pdf":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/pdf.png").Replace(@"\", @"/");
                    break;
                case "html":
                case "htm":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/html.png").Replace(@"\", @"/");
                    break;
                case "onging":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/上传中.png").Replace(@"\", @"/");
                    break;
                case "reveiving":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/上传中.png").Replace(@"\", @"/");
                    break;
                case "success":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接受成功.png").Replace(@"\", @"/");
                    break;
                case "fail":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/上传失败.png").Replace(@"\", @"/");
                    break;
                case "receiveFail":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接收失败.png").Replace(@"\", @"/");
                    break;
                case "mp3":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/mp3.png").Replace(@"\", @"/");
                    break;
                case "mp4":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/mp4.png").Replace(@"\", @"/");
                    break;
                case "rar":
                case "zip":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/zip.png").Replace(@"\", @"/");
                    break;
                case "ppt":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/ppt.png").Replace(@"\", @"/");
                    break;
                case "exe":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/exe.png").Replace(@"\", @"/");
                    break;
                case "jpg":
                case "jpeg":
                case "bmp":
                case "png":
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/html.png").Replace(@"\", @"/");
                    break;
                default:
                    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/其他.png").Replace(@"\", @"/");
                    break;
            }
            return path;
        }
        public static string showFileImagePath(string extendName, string imgPath)
        {
            string path = "";
            switch (extendName.ToLower())
            {
                case "docx":
                case "doc":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/word.png";
                    break;
                case "xlsx":
                case "xls":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/excel.png";
                    break;
                case "rar":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/zip.png";
                    break;
                case "txt":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/test.png";
                    break;
                //case "pdf":
                //    path = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/fileTypeImage/pdf.png").Replace(@"\", @"/");
                //    break;
                case "html":
                case "htm":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/html.png";
                    break;               
                case "mp3":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/mp3.png";
                    break;
                case "mp4":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/mp4.png";
                    break;
                case "zip":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/zip.png";
                    break;
                case "ppt":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/ppt.png";
                    break;
                case "exe":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/exe.png";
                    break;
                case "jpg":
                case "jpeg":
                case "bmp":
                case "png":
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/html.png";
                    break;
                default:
                    path = "pack://application:,,,/AntennaChat;Component/Images/fileTypeImage/其他.png";
                    break;
            }
            return path;
        }
    }
}
