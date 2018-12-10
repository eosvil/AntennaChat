using Antenna.Framework;
using AntennaChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AntennaChat.Resource
{
    public class GifImage : System.Windows.Controls.Image
    {

        /// <summary>
        /// gif动画的System.Drawing.Bitmap
        /// </summary>
        private Bitmap _gifBitmap;

        /// <summary>
        /// 用于显示每一帧的BitmapSource
        /// </summary>
        private BitmapSource bitmapSource;

        private string imagePath;

        public string ImagePath
        {
            get { return this.imagePath; }
            set
            {
                this.imagePath = value;
            }
        }

        public GifImage()
        {
        }

        /// <summary>
        /// 更新源
        /// </summary>
        /// <param name="uri"></param>
        public void UpdateSource(string uri)
        {
            ImagePath = uri;
            DisposedImageGif();
            using (var img = Image.FromFile(imagePath))
            {
                if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                {
                    try
                    {
                        this._gifBitmap = new Bitmap(imagePath);
                        this.bitmapSource = this.GetBitmapSource();
                        this.Source = this.bitmapSource;
                        StartAnimate();
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
                else
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(ImagePath, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    this.Source = new BitmapImage(bi.UriSource);
                }
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void DisposedImageGif()
        {
            if (_gifBitmap == null) return;
            StopAnimate();
            _gifBitmap.Dispose();
            _gifBitmap = null;
        }

        /// <summary>
        /// 从System.Drawing.Bitmap中获得用于显示的那一帧图像的BitmapSource
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetBitmapSource()
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                if(_gifBitmap!=null)
                {
                    handle = this._gifBitmap.GetHbitmap();
                    this.bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero,
                        System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    DeleteObject(handle);
                }
            }
            return this.bitmapSource;
        }

        /// <summary>
        /// Start animation
        /// </summary>
        public void StartAnimate()
        {
            ImageAnimator.Animate(this._gifBitmap, this.OnFrameChanged);
        }

        /// <summary>
        /// Stop animation
        /// </summary>
        private void StopAnimate()
        {
            ImageAnimator.StopAnimate(this._gifBitmap, this.OnFrameChanged);
        }

        /// <summary>
        /// Event handler for the frame changed
        /// </summary>
        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ImageAnimator.UpdateFrames(); // 更新到下一帧
                this.bitmapSource?.Freeze();
                // Convert the bitmap to BitmapSource that can be display in WPF Visual Tree
                this.bitmapSource = this.GetBitmapSource();
                Source = this.bitmapSource;
                this.InvalidateVisual();
            }));
        }

        /// <summary>
        /// Delete local bitmap resource
        /// Reference: http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
        /// </summary>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);
    }
}