using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AntennaChat.ViewModel.Notice
{
    [ValueConversion(typeof(string), typeof(string))]
    public class statusConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                   switch(strValue)
                    {
                        case "look":
                            textShow= "查看";
                            break;
                        case "upLoad":
                            textShow = "上传";
                            break;
                        case "downLoad":
                            textShow = "下载";
                            break;
                        case "sucess":
                            textShow = "上传成功";
                            break;
                        case "resucess":
                            textShow = "查看";
                            break;
                        case "fail":
                            textShow = "上传失败";
                            break;
                        default:
                            textShow=strValue;
                            break;
                    }
                }
            }
            return textShow;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    public class isShowVisibility:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        case "0":
                            textShow = "Collapsed";
                            break;
                        case "1":
                            textShow = "Visible";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    public class isDeleteVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        case "0":
                            textShow = "Collapsed";
                            break;
                        case "1":
                            textShow = "Visible";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    [ValueConversion(typeof(string), typeof(string))]
    public class readStatusforeground: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        case "0":
                            textShow = "#242424";
                            break;
                        case "1":
                            textShow = "#666666";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    [ValueConversion(typeof(string), typeof(string))]
    public class fontShowWeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        case "0":
                            textShow = "Blod";
                            break;
                        default:
                            textShow = "";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    public class foregroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        case "0":
                            textShow = "#22aeff";
                            break;
                        case "1":
                            textShow = "#ff2222";
                            break;
                        case "2":
                            textShow = "#999999";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    public class noticeOperate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textShow = "";
            if (value != null)
            {
                string strValue = value.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    switch (strValue)
                    {
                        //case "0":
                        //    textShow = "";
                        //    break;
                        case "1":
                            textShow = "pack://application:,,,/AntennaChat;Component/Images/已读.png";
                            break;
                        case "2":
                            textShow = "pack://application:,,,/AntennaChat;Component/Images/删除红.png";
                            break;
                        default:
                            textShow = "pack://application:,,,/AntennaChat;Component/Images/删除红.png";
                            break;
                    }
                }
            }
            return textShow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
    /// <summary>
    /// 按钮状态
    /// </summary>
    public enum textStatus
    {
        fail,
        look,
        upLoad,
        downLoad,
        progress,
        sucess,
        resucess
    }
}
