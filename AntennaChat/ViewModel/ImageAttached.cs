using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Antenna.Framework;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel
{
    public class ImageAttached
    {
        private static bool isLoadImage = false;
        private static BitmapSource _tempBitampSource = null;
        public static readonly DependencyProperty IsOfflineProperty =
        DependencyProperty.RegisterAttached("IsOffline", typeof(bool), typeof(ImageAttached),
            new FrameworkPropertyMetadata(false,
                new PropertyChangedCallback(OnIsOfflineChanged)));

        public static bool GetIsOffline(DependencyObject d)
        {
            var value = d.GetValue(IsOfflineProperty);
            return value != null && (bool)value;
        }

        public static void SetIsOffline(DependencyObject d, bool value)
        {
            d.SetValue(IsOfflineProperty, value);
        }

        private static void OnIsOfflineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AsyncHandler.AsyncCall(Application.Current.Dispatcher, () =>
            {
                var currentImage = d as ImageBrush;
                //Image currentImage=d as Image;
                if (currentImage == null)
                {
                    return;
                }
                var value = d.GetValue(IsOfflineProperty);
                var isGray8 = value != null && (bool)value;
                if (isGray8)
                {
                    var bitmapSource = currentImage.ImageSource as BitmapSource;
                    if (bitmapSource == null) return;
                    var url = currentImage.ImageSource.ToString();
                    if (url == GlobalVariable.DefaultImage.UserHeadDefaultImage)
                    {
                        var tempBitampSource = bitmapSource.ToGrayBitmap();
                        currentImage.ImageSource = tempBitampSource;
                    }
                    else
                    {
                        try
                        {
                            if (bitmapSource.IsDownloading)
                            {
                                bitmapSource.DownloadCompleted += (sender, args) =>
                                {
                                    var backupBitmapSource = bitmapSource.CloneCurrentValue();
                                    currentImage.SetValue(BitmapSourceBackupProperty, backupBitmapSource);
                                    currentImage.ImageSource = bitmapSource.ToGrayBitmap();
                                };
                            }
                            else
                            {

                                var backupBitmapSource = bitmapSource.CloneCurrentValue();
                                currentImage.SetValue(BitmapSourceBackupProperty, backupBitmapSource);
                                var tempBitampSource = bitmapSource.ToGrayBitmap();
                                currentImage.ImageSource = tempBitampSource;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    //// 建立Gray32Float的BitmapSource
                    //FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
                    //newFormatedBitmapSource.BeginInit();
                    //newFormatedBitmapSource.Source = currentImage.ImageSource as BitmapSource;
                    //newFormatedBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
                    ////List<Color> colors = new List<Color>();
                    ////colors.Add(Colors.White);
                    ////colors.Add(Colors.DarkGoldenrod);
                    ////BitmapPalette palette = new BitmapPalette(colors);
                    ////newFormatedBitmapSource.DestinationPalette = palette;
                    //newFormatedBitmapSource.EndInit();
                    // 替换ImageSource

                }
                else
                {

                    // 图像恢复操作
                    object obj = currentImage.GetValue(BitmapSourceBackupProperty);

                    BitmapSource bs = obj as BitmapSource;
                    if (bs == null)
                    {
                        return;
                    }
                    currentImage.ImageSource = bs;

                }
            }, DispatcherPriority.Background);
        }
        // 备份用源图像的附加属性，当Gray8变更时，自动附加
        public static readonly DependencyProperty BitmapSourceBackupProperty =
                DependencyProperty.RegisterAttached("BitmapSourceBackup", typeof(BitmapSource), typeof(ImageAttached),
                    new FrameworkPropertyMetadata(null));

        public static BitmapSource GetBitmapSourceBackup(DependencyObject d)
        {
            return (BitmapSource)d.GetValue(BitmapSourceBackupProperty);
        }

        public static void SetBitmapSourceBackup(DependencyObject d, BitmapSource value)
        {
            d.SetValue(BitmapSourceBackupProperty, value);
        }
    }
    /// <summary>  
    /// 图像处理：灰度化  
    /// </summary>  
    public static class Gray
    {
        /// <summary>  
        /// 将位图转换为彩色数组  
        /// </summary>  
        /// <param name="bitmap">原始位图</param>  
        /// <returns>彩色数组</returns>  
        /// <remarks>  
        ///     1.扩展方法  
        ///     2.忽视Alpha通道  
        /// </remarks>  
        public static Color[,] ToColorArray(this BitmapSource bitmap)
        {   // 将像素格式统一到Bgr32，并提取图像数据  
            Int32 PixelHeight = bitmap.PixelHeight; // 图像高度  
            Int32 PixelWidth = bitmap.PixelWidth;   // 图像宽度  
            Int32 Stride = PixelWidth << 2;         // 扫描行跨距  
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            if (bitmap.Format == PixelFormats.Bgr32 || bitmap.Format == PixelFormats.Bgra32)
            {   // 拷贝像素数据  
                bitmap.CopyPixels(Pixels, Stride, 0);
            }
            else
            {   // 先进行像素格式转换，再拷贝像素数据  
                new FormatConvertedBitmap(bitmap, PixelFormats.Bgr32, null, 0).CopyPixels(Pixels, Stride, 0);
            }

            // 将像素数据转换为彩色数组  
            Color[,] ColorArray = new Color[PixelHeight, PixelWidth];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Int32 Index = i * Stride + (j << 2);
                    ColorArray[i, j].B = Pixels[Index];
                    ColorArray[i, j].G = Pixels[Index + 1];
                    ColorArray[i, j].R = Pixels[Index + 2];
                    ColorArray[i, j].A = Pixels[Index + 3];
                }
            }

            return ColorArray;
        }

        /// <summary>  
        /// 将位图转换为灰度数组（256级灰度）  
        /// </summary>  
        /// <param name="bitmap">原始位图</param>  
        /// <returns>灰度数组</returns>  
        /// <remarks>扩展方法</remarks>  
        public static Byte[,] ToGrayArray(this BitmapSource bitmap)
        {   // 将像素格式统一到Bgr32，并提取图像数据  
            Int32 PixelHeight = bitmap.PixelHeight; // 图像高度  
            Int32 PixelWidth = bitmap.PixelWidth;   // 图像宽度  
            Int32 Stride = PixelWidth << 2;         // 扫描行跨距  
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            if (bitmap.Format == PixelFormats.Bgr32 || bitmap.Format == PixelFormats.Bgra32)
            {   // 拷贝像素数据  
                bitmap.CopyPixels(Pixels, Stride, 0);
            }
            else
            {   // 先进行像素格式转换，再拷贝像素数据  
                new FormatConvertedBitmap(bitmap, PixelFormats.Bgr32, null, 0).CopyPixels(Pixels, Stride, 0);
            }

            // 将像素数据转换为灰度数组  
            Byte[,] GrayArray = new Byte[PixelHeight, PixelWidth];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Int32 Index = i * Stride + (j << 2);
                    GrayArray[i, j] = Convert.ToByte((Pixels[Index + 2] * 19595 + Pixels[Index + 1] * 38469 + Pixels[Index] * 7471 + 32768) >> 16);
                }
            }

            return GrayArray;
        }

        /// <summary>  
        /// 位图灰度化  
        /// </summary>  
        /// <param name="bitmap">原始位图</param>  
        /// <returns>灰度位图</returns>  
        /// <remarks>扩展方法</remarks>  
        public static BitmapSource ToGrayBitmap(this BitmapSource bitmap)
        {   // 将像素格式统一到Bgr32，并提取图像数据  

            Int32 PixelHeight = bitmap.PixelHeight; // 图像高度  
            Int32 PixelWidth = bitmap.PixelWidth;   // 图像宽度  
            Int32 Stride = PixelWidth << 2;         // 扫描行跨距  
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            if (bitmap.Format == PixelFormats.Bgr32 || bitmap.Format == PixelFormats.Bgra32)
            {   // 拷贝像素数据  
                bitmap.CopyPixels(Pixels, Stride, 0);
            }
            else
            {   // 先进行像素格式转换，再拷贝像素数据  
                new FormatConvertedBitmap(bitmap, PixelFormats.Bgr32, null, 0).CopyPixels(Pixels, Stride, 0);
            }

            // 将像素数据转换为灰度数据  
            Int32 GrayStride = ((PixelWidth + 3) >> 2) << 2;
            Byte[] GrayPixels = new Byte[PixelHeight * GrayStride];

            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Int32 Index = i * Stride + (j << 2);
                    GrayPixels[i * GrayStride + j] = Convert.ToByte((Pixels[Index + 2] * 19595 + Pixels[Index + 1] * 38469 + Pixels[Index] * 7471 + 32768) >> 16);
                }
            }

            // 从灰度数据中创建灰度图像  
            return BitmapSource.Create(PixelWidth, PixelHeight, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256, GrayPixels, GrayStride);
        }

        /// <summary>  
        /// 将灰度数组转换为灰度图像（256级灰度）  
        /// </summary>  
        /// <param name="grayArray">灰度数组</param>  
        /// <returns>灰度图像</returns>  
        public static BitmapSource GrayArrayToGrayBitmap(Byte[,] grayArray)
        {   // 将灰度数组转换为灰度数据  
            Int32 PixelHeight = grayArray.GetLength(0);     // 图像高度  
            Int32 PixelWidth = grayArray.GetLength(1);      // 图像宽度  
            Int32 Stride = ((PixelWidth + 3) >> 2) << 2;        // 扫描行跨距  
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    Pixels[i * Stride + j] = grayArray[i, j];
                }
            }

            // 从灰度数据中创建灰度图像  
            return BitmapSource.Create(PixelWidth, PixelHeight, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256, Pixels, Stride);
        }

        /// <summary>  
        /// 将二值化数组转换为二值化图像  
        /// </summary>  
        /// <param name="binaryArray">二值化数组</param>  
        /// <returns>二值化图像</returns>  
        public static BitmapSource BinaryArrayToBinaryBitmap(Byte[,] binaryArray)
        {   // 将二值化数组转换为二值化数据  
            Int32 PixelHeight = binaryArray.GetLength(0);
            Int32 PixelWidth = binaryArray.GetLength(1);
            Int32 Stride = ((PixelWidth + 31) >> 5) << 2;
            Byte[] Pixels = new Byte[PixelHeight * Stride];
            for (Int32 i = 0; i < PixelHeight; i++)
            {
                Int32 Base = i * Stride;
                for (Int32 j = 0; j < PixelWidth; j++)
                {
                    if (binaryArray[i, j] != 0)
                    {
                        Pixels[Base + (j >> 3)] |= Convert.ToByte(0x80 >> (j & 0x7));
                    }
                }
            }

            // 从灰度数据中创建灰度图像  
            return BitmapSource.Create(PixelWidth, PixelHeight, 96, 96, PixelFormats.Indexed1, BitmapPalettes.BlackAndWhite, Pixels, Stride);
        }
    }
}
