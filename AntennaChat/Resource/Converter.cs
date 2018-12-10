using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AntennaChat.Resource
{
    /// <summary>
    /// 数值转换
    /// </summary>
    public class NumToBoolConverter : IValueConverter
     {
          object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
          {
             if (value == null) return 0;
              if (string.IsNullOrEmpty(value.ToString())) return 0;
              if (!Regex.IsMatch(value.ToString(), "^[1-9]\\d*$")) return 0;
              int readNum = Convert.ToInt32(value);
              if (readNum > 99)
              {
                  return 3;
              }
              else if (readNum > 10)
              {
                  return 2;
              }
              else
              {
                  return 1;
              }
         }
 
         object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
         {
             return null;
         }
    }
    public class ToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value == null) return 0;
            //if (string.IsNullOrEmpty(value.ToString())) return 0;
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;
            else
            {
                return Visibility.Visible;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
