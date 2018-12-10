using Antenna.Framework;
using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Antenna.Model;
using AntennaChat.Views;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using AntennaChat.Resource;

namespace AntennaChat.ViewModel.Talk
{
    public class PictureViewerViewModel : WindowBaseViewModel, IDisposable
    {
        ~PictureViewerViewModel()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        /// <summary>
        /// 消息id
        /// </summary>
        private string _chatIndex = "";
        /// <summary>
        /// 当前索引
        /// </summary>
        private int _currentIndex = 0;
        /// <summary>
        /// 上一张图片索引
        /// </summary>
        private int _oldIndex = -1;
        /// <summary>
        /// true-上一张  false-下一张
        /// </summary>
        private bool _isPre = false;
        /// <summary>
        /// 图片是否发送出去
        /// </summary>
        private bool _isNoSend = false;
        /// <summary>
        /// 图片地址
        /// </summary>
        private string imagePath;
        /// <summary>
        /// 图片下载地址
        /// </summary>
        private string _downloadGifPath = publicMethod.DownloadFilePath() + "\\ChatImage\\";
        private List<AddImgUrlDto> _imgUrl;
        public List<AddImgUrlDto> ImgUrl
        {
            set { this._imgUrl = value; }
            get { return this._imgUrl; }
        }
        public PictureViewerViewModel()
        {
        }
        public PictureViewerViewModel(string filePath)
        {
            _PicturePath = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
            LogHelper.WriteDebug("图片查看器图片路径：" + filePath);
            PreButtonVisibility = Visibility.Collapsed;
            NextButtonVisibility = Visibility.Collapsed;
            _isNoSend = true;
            imagePath = filePath;
        }
        public PictureViewerViewModel(string filePath, GlobalVariable.BurnFlag flag, string index, List<AddImgUrlDto> imgUrl)
        {
            _isNoSend = false;
            this._imgUrl = imgUrl;
            imagePath = filePath;
            IsBurn = flag;
            _currentIndex = ImgUrl.FindIndex(v => v.ChatIndex == index);
            _oldIndex = _currentIndex;
            if (ImgUrl.Count > _currentIndex)
                this._chatIndex = ImgUrl[_currentIndex].ChatIndex;
            SetButtonVisibility();
            if (IsBurn == GlobalVariable.BurnFlag.IsBurn)
            {
                ContextMenuVisibility = Visibility.Collapsed;
                BorderVisibility = Visibility.Collapsed;
            }
            LogHelper.WriteDebug("图片查看器图片路径：" + filePath);
        }

        /// <summary>
        /// 移除阅后即焚图片
        /// </summary>
        public static event EventHandler RemoveImgUrlEventHandler;

        private void RemoveImgUrl(string chatIndex)
        {
            RemoveImgUrlEventHandler?.Invoke(chatIndex, null);
        }

        /// <summary>
        /// 设置按钮显示状态
        /// </summary>
        private void SetButtonVisibility()
        {
            //PreIsEnabled = this._currentIndex != 0;
            //NextIsEnabled = this._currentIndex != _imgUrl.Count - 1;
        }
        #region 字段
        private Window picWindow;
        private double min = 0.1, max = 5.0;//最小/最大放大倍数
        private bool mouseDown;
        private Point mouseXY;
        private TransformGroup transformGroup;
        private GifImage image;
        private GlobalVariable.BurnFlag IsBurn = GlobalVariable.BurnFlag.NotIsBurn;
        /// <summary>
        /// 图片原始宽度
        /// </summary>
        private double imgOriginalWidth;
        /// <summary>
        /// 图片原始高度
        /// </summary>
        private double imgOriginalHeigh;
        /// <summary>
        /// 图片变换之后的宽度
        /// </summary>
        private double imgNewWidth;
        /// <summary>
        /// 图片变换之后的高度
        /// </summary>
        private double imgNewHeigh;
        /// <summary>
        /// 旋转角度倍数
        /// </summary>
        private int times;
        #endregion

        #region 属性
        private string _TextPoint = "";
        public string TextPoint
        {
            get { return this._TextPoint; }
            set
            {
                this._TextPoint = value;
                RaisePropertyChanged(() => TextPoint);
            }
        }
        /// <summary>
        /// 图片地址
        /// </summary>
        private BitmapImage _PicturePath;
        public BitmapImage PicturePath
        {
            get { return this._PicturePath; }
            set
            {
                this._PicturePath = value;
                RaisePropertyChanged(() => PicturePath);
            }
        }
        /// <summary>
        ///  x 轴的缩放比例
        /// </summary>
        private double _Scale_X = 1;
        public double Scale_X
        {
            get { return this._Scale_X; }
            set
            {
                this._Scale_X = value;
                RaisePropertyChanged(() => Scale_X);
            }
        }
        /// <summary>
        ///  y 轴的缩放比例
        /// </summary>
        private double _Scale_Y = 1;
        public double Scale_Y
        {
            get { return this._Scale_Y; }
            set
            {
                this._Scale_Y = value;
                RaisePropertyChanged(() => Scale_Y);
            }
        }
        /// <summary>
        /// 沿 x 轴平移的距离
        /// </summary>
        private double _T_X = 0;
        public double T_X
        {
            get { return this._T_X; }
            set
            {
                this._T_X = value;
                RaisePropertyChanged(() => T_X);
            }
        }
        /// <summary>
        /// 沿 y 轴平移（移动）对象的距离
        /// </summary>
        private double _T_Y = 0;
        public double T_Y
        {
            get { return this._T_Y; }
            set
            {
                this._T_Y = value;
                RaisePropertyChanged(() => T_Y);
            }
        }
        /// <summary>
        /// 宽度
        /// </summary>
        private double _ScrollViewerWidth = 0;
        public double ScrollViewerWidth
        {
            get { return this._ScrollViewerWidth; }
            set
            {
                this._ScrollViewerWidth = value;
                RaisePropertyChanged(() => ScrollViewerWidth);
            }
        }
        /// <summary>
        /// 高度
        /// </summary>
        private double _ScrollViewerHeight = 0;
        public double ScrollViewerHeight
        {
            get { return this._ScrollViewerHeight; }
            set
            {
                this._ScrollViewerHeight = value;
                RaisePropertyChanged(() => ScrollViewerHeight);
            }
        }
        /// <summary>
        /// 旋转角度
        /// </summary>
        private int _Angel = 0;
        public int Angel
        {
            get { return this._Angel; }
            set
            {
                this._Angel = value;
                RaisePropertyChanged(() => Angel);
            }
        }
        /// <summary>
        /// 上一张按钮显示状态
        /// </summary>
        private Visibility _preButtonVisibility = Visibility.Collapsed;
        public Visibility PreButtonVisibility
        {
            get { return this._preButtonVisibility; }
            set
            {
                this._preButtonVisibility = value;
                RaisePropertyChanged(() => PreButtonVisibility);
            }
        }
        /// <summary>
        /// 下一张按钮显示状态
        /// </summary>
        private Visibility _nextButtonVisibility = Visibility.Collapsed;
        public Visibility NextButtonVisibility
        {
            get { return this._nextButtonVisibility; }
            set
            {
                this._nextButtonVisibility = value;
                RaisePropertyChanged(() => NextButtonVisibility);
            }
        }

        /// <summary>
        /// 上一张按钮是否可用
        /// </summary>
        private bool _preIsEnabled;
        public bool PreIsEnabled
        {
            get { return this._preIsEnabled; }
            set
            {
                this._preIsEnabled = value;
                RaisePropertyChanged(() => PreIsEnabled);
            }
        }
        /// <summary>
        /// 下一张按钮是否可用
        /// </summary>
        private bool _nextIsEnabled;
        public bool NextIsEnabled
        {
            get { return this._nextIsEnabled; }
            set
            {
                this._nextIsEnabled = value;
                RaisePropertyChanged(() => NextIsEnabled);
            }
        }
        /// <summary>
        /// 右键菜单是否可用
        /// </summary>
        private Visibility _contextMenuVisibility = Visibility.Visible;
        public Visibility ContextMenuVisibility
        {
            get { return this._contextMenuVisibility; }
            set
            {
                this._contextMenuVisibility = value;
                RaisePropertyChanged(() => ContextMenuVisibility);
            }
        }
        /// <summary>
        /// 下方工具栏显示状态
        /// </summary>
        private Visibility _borderVisibility = Visibility.Hidden;
        public Visibility BorderVisibility
        {
            get { return this._borderVisibility; }
            set
            {
                this._borderVisibility = value;
                RaisePropertyChanged(() => BorderVisibility);
            }
        }
        /// <summary>
        /// 放大百分比
        /// </summary>
        private Visibility _percentVisibility = Visibility.Collapsed;
        public Visibility PercentVisibility
        {
            get { return this._percentVisibility; }
            set
            {
                this._percentVisibility = value;
                RaisePropertyChanged(() => PercentVisibility);
            }
        }
        /// <summary>
        /// 百分比
        /// </summary>
        private string _percentText = "100%";
        public string PercentText
        {
            get { return this._percentText; }
            set
            {
                this._percentText = value;
                RaisePropertyChanged(() => PercentText);
            }
        }
        /// <summary>
        /// 翻页提示
        /// </summary>
        private Visibility _tipsVisibility = Visibility.Collapsed;
        public Visibility TipsVisibility
        {
            get { return this._tipsVisibility; }
            set
            {
                this._tipsVisibility = value;
                RaisePropertyChanged(() => TipsVisibility);
            }
        }
        /// <summary>
        /// 翻页提示
        /// </summary>
        private string _tipsText = "";
        public string TipsText
        {
            get { return this._tipsText; }
            set
            {
                this._tipsText = value;
                RaisePropertyChanged(() => TipsText);
            }
        }
        /// <summary>
        /// 加载动画
        /// </summary>
        private Visibility _isShowLoading = Visibility.Collapsed;
        public Visibility IsShowLoading
        {
            get { return this._isShowLoading; }
            set
            {
                this._isShowLoading = value;
                RaisePropertyChanged(() => IsShowLoading);
            }
        }
        #endregion

        #region 命令

        /// <summary>
        /// 窗体关闭
        /// </summary>
        private ActionCommand<Window> _closePicWindow;

        public ActionCommand<Window> ClosePicWindow
        {
            get
            {
                if (this._closePicWindow == null)
                {
                    this._closePicWindow = new ActionCommand<Window>(
                        o =>
                        {
                            if (o != null)
                            {
                                o.Close();
                            }
                        });
                }
                return this._closePicWindow;
            }
        }
        /// <summary>
        /// 窗体鼠标进入
        /// </summary>
        private ICommand _mouseEnterCommand;
        public ICommand MouseEnterCommand
        {
            get
            {
                if (this._mouseEnterCommand == null)
                {
                    this._mouseEnterCommand = new DefaultCommand(o =>
                    {
                        if (IsBurn == GlobalVariable.BurnFlag.NotIsBurn)
                        {
                            BorderVisibility = Visibility.Visible;
                        }
                        else
                        {
                            BorderVisibility = Visibility.Collapsed;
                        }
                    });
                }
                return this._mouseEnterCommand;
            }
        }
        /// <summary>
        /// 窗体鼠标移出
        /// </summary>
        private ICommand _mouseLeaveCommand;
        public ICommand MouseLeaveCommand
        {
            get
            {
                if (this._mouseLeaveCommand == null)
                {
                    this._mouseLeaveCommand = new DefaultCommand(o =>
                    {
                        BorderVisibility = Visibility.Hidden;
                    });
                }
                return this._mouseLeaveCommand;
            }
        }
        /// <summary>
        /// 加载
        /// </summary>
        private ActionCommand<Window> _loadedCommand;
        public ActionCommand<Window> LoadedCommand
        {
            get
            {
                if (this._loadedCommand == null)
                {
                    this._loadedCommand = new ActionCommand<Window>(
                           win =>
                           {
                               picWindow = win;
                               SetViewSize();
                               _oldPicWindowWidth = picWindow.Width;
                               _oldPicWindowHeight = picWindow.Height;
                           });
                }
                return this._loadedCommand;
            }
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            ContentControl contentControl = sender as ContentControl;
            if (contentControl != null)
                image = (GifImage)contentControl.Content;
            if (image != null)
            {
                transformGroup = (TransformGroup)image.RenderTransform;
                LoadImage();
            }
        }
        private async void LoadImage()
        {
            await Task.Run(() =>
            {
                imagePath=LoadImage(imagePath);
            });
            image.UpdateSource(imagePath);
            SetPicWindowSize();
        }
        /// <summary>
        /// 改变窗体
        /// </summary>
        private ICommand _sizeChangedCommand;
        public ICommand SizeChangedCommand
        {
            get
            {
                if (this._sizeChangedCommand == null)
                {
                    this._sizeChangedCommand = new DefaultCommand(o =>
                    {
                        SetViewSize();
                    });
                }
                return this._sizeChangedCommand;
            }
        }
        private ICommand _picWindowClosed;
        public ICommand PicWindowClosed
        {
            get
            {
                if (this._picWindowClosed == null)
                {
                    this._picWindowClosed = new DefaultCommand(o =>
                    {
                        image.DisposedImageGif();
                    });
                }
                return this._picWindowClosed;
            }
        }
        /// <summary>
        /// 上一张
        /// </summary>
        private ICommand _preCommand;
        public ICommand PreCommand
        {
            get
            {
                return this._preCommand ?? (this._preCommand = new DefaultCommand(o =>
                {
                    _isPre = true;
                    PercentVisibility = Visibility.Collapsed;
                    TipsVisibility = Visibility.Collapsed;
                    if (_oldIndex != -1)
                    {
                        _currentIndex = _oldIndex;
                        _chatIndex = ImgUrl[_currentIndex].ChatIndex;
                    }
                    PrePic();
                }));
            }
        }

        /// <summary>
        /// 上一张
        /// </summary>
        private void PrePic()
        {
            _currentIndex = ImgUrl.FindIndex(v => v.ChatIndex == _chatIndex);
            switch (_currentIndex)
            {
                case -1:
                    return;
                case 0:
                    TipsText = "已经是第一张";
                    TipsVisibility = Visibility.Visible;
                    Application.Current.Dispatcher.Invoke(Timing);
                    break;
                default:
                    _currentIndex--;
                    _chatIndex = ImgUrl[_currentIndex].ChatIndex;
                    var img = ImgUrl[_currentIndex];
                    if ((img.IsBurn == burnMsg.isBurnMsg.yesBurn && img.IsRead == burnMsg.IsReadImg.notRead) || (img.IsRead == burnMsg.IsReadImg.read && img.IsEffective == burnMsg.IsEffective.NotEffective))
                    {
                        PrePic();
                    }
                    else
                    {
                        _oldIndex = _currentIndex;
                        UpdateImage();
                    }
                    break;
            }
        }
        /// <summary>
        /// 下一张
        /// </summary>
        private ICommand _nextCommand;
        public ICommand NextCommand
        {
            get
            {
                return this._nextCommand ?? (this._nextCommand = new DefaultCommand(o =>
                {
                    _isPre = false;
                    PercentVisibility = Visibility.Collapsed;
                    TipsVisibility = Visibility.Collapsed;
                    if (_oldIndex != -1)
                    {
                        _currentIndex = _oldIndex;
                        _chatIndex = ImgUrl[_currentIndex].ChatIndex;
                    }
                    NextPic();
                }));
            }
        }
        /// <summary>
        /// 下一张
        /// </summary>
        private void NextPic()
        {
            _currentIndex = ImgUrl.FindIndex(v => v.ChatIndex == _chatIndex);
            if (_currentIndex == -1) return;
            if (_currentIndex >= ImgUrl.Count - 1)
            {
                _currentIndex = ImgUrl.Count - 1;
                TipsText = "已经是最后一张";
                TipsVisibility = Visibility.Visible;
                Application.Current.Dispatcher.Invoke(Timing);
            }
            else
            {
                _currentIndex++;
                _chatIndex = ImgUrl[_currentIndex].ChatIndex;
                var img = ImgUrl[_currentIndex];
                if ((img.IsBurn == burnMsg.isBurnMsg.yesBurn && img.IsRead == burnMsg.IsReadImg.notRead) || (img.IsRead == burnMsg.IsReadImg.read && img.IsEffective == burnMsg.IsEffective.NotEffective))
                {
                    NextPic();
                }
                else
                {
                    _oldIndex = _currentIndex;
                    UpdateImage();
                }
            }
        }
        /// <summary>
        /// 旋转
        /// </summary>
        private ICommand _rotateCommand;
        public ICommand RotateCommand
        {
            get
            {
                if (this._rotateCommand == null)
                {
                    this._rotateCommand = new DefaultCommand(o =>
                    {
                        //Rotate();
                    });
                }
                return this._rotateCommand;
            }
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        private ICommand _fileSaveCommand;
        public ICommand FileSaveCommand
        {
            get
            {
                if (this._fileSaveCommand == null)
                {
                    this._fileSaveCommand = new DefaultCommand(o =>
                    {
                        if (string.IsNullOrEmpty(imagePath)) return;
                        if (PublicTalkMothed.SavePicture(image.ImagePath))
                        {
                            TipsText = "保存图片成功";
                            TipsVisibility = Visibility.Visible;
                            Application.Current.Dispatcher.Invoke(Timing);
                        }
                    });
                }
                return this._fileSaveCommand;
            }
        }
        /// <summary>
        /// 复制图片
        /// </summary>
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (this._copyImageCommand == null)
                {
                    this._copyImageCommand = new DefaultCommand(o =>
                    {
                        CopyPic();
                    });
                }
                return this._copyImageCommand;
            }
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PicWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isNoSend) return;
            try
            {
                int preButtonRecXRight = 150;
                int buttonRecYTop = 50;//Convert.ToInt32(picWindow.ActualHeight / 2) - 50;
                int buttonRecYBottom = Convert.ToInt32(picWindow.ActualHeight) - 50;//Convert.ToInt32(picWindow.ActualHeight / 2) + 50;
                int nextButtonRecXLeft = Convert.ToInt32(picWindow.ActualWidth - 150);
                int nextButtonRecXRight = Convert.ToInt32(picWindow.ActualWidth);
                Point p = e.GetPosition(picWindow);
                if (p.X > 0 && p.X < preButtonRecXRight && p.Y > buttonRecYTop && p.Y < buttonRecYBottom)
                {
                    PreButtonVisibility = Visibility.Visible;
                    NextButtonVisibility = Visibility.Hidden;
                }
                else if (p.X > nextButtonRecXLeft && p.X < nextButtonRecXRight && p.Y > buttonRecYTop && p.Y < buttonRecYBottom)
                {
                    PreButtonVisibility = Visibility.Hidden;
                    NextButtonVisibility = Visibility.Visible;
                }
                else
                {
                    PreButtonVisibility = Visibility.Hidden;
                    NextButtonVisibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[获取鼠标位置异常]:" + ex.Message + "," + ex.StackTrace);
            }
        }
        /// <summary>
        /// 鼠标左键点下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
            Win32.GetCursorPos(out base.OriginalPoint);//API方法 记录鼠标按下时候的初始坐标
        }
        /// <summary>
        /// 鼠标左键放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.ReleaseMouseCapture();
            mouseDown = false;
            Point newPoint = new Point();
            if (Win32.GetCursorPos(out newPoint))
            {
                if (newPoint.X == base.OriginalPoint.X && newPoint.Y == base.OriginalPoint.Y)
                {
                    picWindow.Close();
                }
            }
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentControl_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }
        /// <summary>
        /// 鼠标滚轮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            var point = e.GetPosition(img);
            var delta = e.Delta * 0.001;
            DowheelZoom(point, delta);
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 更新图片显示
        /// </summary>
        private async void UpdateImage()
        {
            var path = ImgUrl[_currentIndex].ImageUrl;
            await Task.Run(() =>
            {
                imagePath = LoadImage(path);
            });
            image.UpdateSource(imagePath);
            Scale_X = 1;
            Scale_Y = 1;
            PercentText = "100%";
            imgNewWidth = 0;
            imgNewHeigh = 0;
            SetPicWindowSize();
            SetButtonVisibility();
        }
        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path"></param>
        private string LoadImage(string path)
        {
            string errMsg = string.Empty;
            return DownLoadImage(path, ref errMsg);
        }
        /// <summary>
        /// 图片本地化
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private string DownLoadImage(string fileUrl,ref string errMsg)
        {
            if (!Directory.Exists(_downloadGifPath))
            {
                Directory.CreateDirectory(_downloadGifPath);
            }
            if (!fileUrl.StartsWith("http:") && !fileUrl.StartsWith("file"))
            {
                try
                {
                    //复制一份 防止图片占用
                    var gifIndex = fileUrl.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                    var gifName = fileUrl.Substring(gifIndex, fileUrl.Length - gifIndex);
                    var gifPath = _downloadGifPath + "tmp~" + gifName;
                    if (!File.Exists(gifPath))
                    {
                        File.Copy(fileUrl, gifPath, true);
                    }
                    return gifPath;
                }
                catch (Exception ex)
                {
                    errMsg = "图片下载失败";
                    return fileUrl;
                }
            }
            if (fileUrl.StartsWith("file"))
            {
                try
                {
                    var index = fileUrl.LastIndexOf("/", StringComparison.Ordinal) + 1;
                    var fileName = fileUrl.Substring(index, fileUrl.Length - index);
                    var localFile = ImageHandle.DownloadPictureFromFile(fileUrl, _downloadGifPath + fileName, 10000);//10s超时
                    //复制一份 防止打开之后被占用
                    var tmpPath = _downloadGifPath + "tmp~" + fileName;
                    if (!File.Exists(tmpPath))
                    {
                        File.Copy(localFile, tmpPath, true);
                    }
                    return tmpPath;
                }
                catch(Exception ex)
                {
                    errMsg = "图片下载失败";
                    return fileUrl;
                }
            }
            else
            {
                var index = fileUrl.LastIndexOf("/", StringComparison.Ordinal) + 1;
                var fileName = fileUrl.Substring(index, fileUrl.Length - index);
                var localFile = ImageHandle.DownloadPictureFromHttp(fileUrl, _downloadGifPath + fileName, 10000);//10s超时
                return localFile;
            }
        }
        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            Point point = image.TranslatePoint(new Point(0, 0), (UIElement)picWindow);
            Debug.WriteLine(point.X + "     " + point.Y);
            if (OutRange(point))
            {
                T_X = 0;
                T_Y = 0;
                mouseXY = new Point();
                mouseDown = false;
                return;
            }
            var position = e.GetPosition(img);
            T_X -= mouseXY.X - position.X;
            T_Y -= mouseXY.Y - position.Y;
            mouseXY = position;
        }
        /// <summary>
        /// 计算是否超出范围
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool OutRange(Point p)
        {
            var imgScaleWidth = imgActualWidth * Scale_X;
            var imgScaleHeight = imgActualHeight * Scale_Y;
            var marginValue = 30;
            TextPoint = p.X + "   " + p.Y;
            double winWidth = picWindow.ActualWidth;
            double winHeigh = picWindow.ActualHeight;
            double picWidth = imgNewWidth == 0 ? imgOriginalWidth : imgNewWidth;
            double picHeigh = imgNewHeigh == 0 ? imgOriginalHeigh : imgNewHeigh;
            if (p.X > 0 && p.Y > 0)
            {
                if (winWidth - p.X < marginValue) return true;
                if (winHeigh - p.Y < marginValue)
                    return true;
                return false;
            }
            else if (p.X > 0 && p.Y < 0)
            {
                if(isFullScreen)
                {
                    if(Scale_Y!=1)
                    {
                        if (imgActualHeight > winHeigh && p.Y + picHeigh < marginValue)
                            return true;
                        if (imgActualWidth > winWidth && imgScaleHeight + p.Y < marginValue)
                            return true;
                    }
                    else
                    {
                        if (imgActualHeight>winHeigh&& winHeigh + p.Y < marginValue)
                            return true;
                        if (imgActualWidth > winWidth && imgScaleHeight + p.Y < marginValue)
                            return true;
                    }
                    if (winWidth - p.X < marginValue)
                        return true;
                    return false;
                }
                if (picHeigh + p.Y < marginValue) return true;
                if (picWidth - (p.X - winWidth) < marginValue) return true;
                if (winWidth - p.X < marginValue) return true;
                return false;
            }
            else if (p.X < 0 && p.Y > 0)
            {
                if (isFullScreen)
                {
                    if (imgActualWidth > winWidth && p.X + picWidth < marginValue)
                        return true;
                    if (Scale_X >=1 && imgActualHeight > winHeigh && p.X + imgScaleWidth < marginValue+(imgActualWidth-image.ActualWidth)*Scale_X)
                        return true;
                    if (Scale_X < 1 && imgActualHeight > winHeigh && p.X + imgScaleWidth < marginValue)
                        return true;
                    else
                    if (winHeigh - p.Y < marginValue)
                    {
                        return true;
                    }
                    return false;
                }
                if (picWidth + p.X < marginValue) return true;
                if (picHeigh - (p.Y - winHeigh) < marginValue) return true;
                if (winHeigh - p.Y < marginValue) return true;
                return false;
            }
            else if (p.X < 0 && p.Y < 0)
            {
                if (isFullScreen)
                {
                    if (Scale_X >= 1 && p.X + imgScaleWidth < marginValue + (imgActualWidth - image.ActualWidth) * Scale_X)
                        return true;
                    if (Scale_X < 1 && p.X + imgScaleWidth < marginValue)
                        return true;
                    if (picHeigh + p.Y < marginValue)
                        return true;
                    if (imgScaleHeight + p.Y < marginValue)
                        return true; 
                    return false;
                }
                if (picWidth + p.X < marginValue || picHeigh + p.Y < marginValue) return true;
                return false;
            }
            else
                return false;
        }
        /// <summary>
        /// 放大缩小
        /// </summary>
        /// <param name="point"></param>
        /// <param name="delta"></param>
        private void DowheelZoom(Point point, double delta)
        {
            if (delta < 0)
                delta = -0.1;
            else
                delta = 0.1;
            if (transformGroup.Inverse != null)
            {
                var pointToContent = transformGroup.Inverse.Transform(point);
                if (Scale_X + delta < min) return;
                if (Scale_X + delta > max)
                {
                    Scale_X = 5.0;
                    Scale_Y = 5.0;
                    T_X = -1 * ((pointToContent.X * Scale_X) - point.X);
                    T_Y = -1 * ((pointToContent.Y * Scale_Y) - point.Y);
                    imgNewWidth = imgOriginalWidth * Scale_X;
                    imgNewHeigh = imgOriginalHeigh * Scale_Y;
                    ResetLocation();
                    return;
                }
                Scale_X += delta;
                Scale_Y += delta;
                T_X = -1 * ((pointToContent.X * Scale_X) - point.X);
                T_Y = -1 * ((pointToContent.Y * Scale_Y) - point.Y);
                ResetLocation();
            }
            imgNewWidth = imgOriginalWidth * Scale_X;
            imgNewHeigh = imgOriginalHeigh * Scale_Y;
            TextPoint = imgNewWidth + "   " + imgNewHeigh;
            PercentText = Math.Round(Scale_X * 100) + "%";
            PercentVisibility = Visibility.Visible;
            TipsVisibility = Visibility.Collapsed;
            Application.Current.Dispatcher.Invoke(Timing);
        }
        private void ResetLocation()
        { 
            Point p = image.TranslatePoint(new Point(0, 0), (UIElement)picWindow);
            if (OutRange(p))
            {
                T_X = 0;
                T_Y = 0;
            }
        }
        /// <summary>
        /// 设置宽高
        /// </summary>
        private void SetViewSize()
        {
            ScrollViewerWidth = picWindow.ActualWidth;
            ScrollViewerHeight = picWindow.ActualHeight;
            T_X = 0;
            T_Y = 0;
        }

        private double _oldPicWindowWidth = 0;
        private double _oldPicWindowHeight = 0;
        private bool isFullScreen = false;
        private double imgActualWidth =0;//图片实际宽度
        private double imgActualHeight = 0;//图片实际高度
        /// <summary>
        /// 根据图片大小修改窗体
        /// </summary>
        private void SetPicWindowSize()
        {
            isFullScreen = false;
            if (string.IsNullOrEmpty(imagePath)) return;
            using (var webClient = new System.Net.WebClient())
            {
                var imgData = webClient.DownloadData(imagePath);
                using (var stream = new MemoryStream(imgData))
                {
                    double imgWidth = 0;
                    double imgHeight = 0;
                    using (var img = System.Drawing.Image.FromStream(stream))
                    {
                        imgWidth = img.Width;
                        imgHeight = img.Height;
                    }
                    imgActualWidth = imgWidth;
                    imgActualHeight = imgHeight;
                    if (imgHeight > 600 && imgWidth > 800)
                    {
                        if (imgWidth <= SystemParameters.WorkArea.Width - 10 && imgHeight <= SystemParameters.WorkArea.Height - 10)
                        {
                            picWindow.Width = imgWidth + 5;
                            picWindow.Height = imgHeight + 5;
                        }
                        else
                        {
                            isFullScreen = true;
                            picWindow.Width = SystemParameters.WorkArea.Width - 10;
                            picWindow.Height = SystemParameters.WorkArea.Height - 10;
                            if (imgHeight / imgWidth > (SystemParameters.WorkArea.Height - 10) / (SystemParameters.WorkArea.Width - 10))
                            {
                                image.Height = SystemParameters.WorkArea.Height - 15;
                                image.Width = ((SystemParameters.WorkArea.Height - 15) / imgHeight) * imgWidth;
                            }
                            else
                            {
                                image.Width = SystemParameters.WorkArea.Width - 15;
                                image.Height = ((SystemParameters.WorkArea.Width - 15) / imgWidth) * imgHeight;
                            }
                        }
                    }
                    else
                    {
                        image.Width = imgWidth;
                        image.Height = imgHeight;
                        if (imgHeight <= 600 && imgWidth <= 800)
                        {
                            picWindow.Width = 805;
                            picWindow.Height = 605;
                        }
                        else if (imgHeight > 600 && imgWidth <= 800)
                        {
                            if (imgHeight <= SystemParameters.WorkArea.Height - 10)
                            {
                                picWindow.Width = 805;
                                picWindow.Height = imgHeight + 5;
                            }
                            else
                            {
                                picWindow.Width = 805;
                                picWindow.Height = SystemParameters.WorkArea.Height - 10;
                            }
                        }
                        else if (imgHeight <= 600 && imgWidth > 800)
                        {
                            if (imgWidth <= SystemParameters.WorkArea.Width - 10)
                            {
                                picWindow.Width = imgWidth + 5;
                                picWindow.Height = 605;
                            }
                            else
                            {
                                picWindow.Width = SystemParameters.WorkArea.Width - 10;
                                picWindow.Height = 605;
                            }
                        }
                    }
                    if (_oldPicWindowWidth != picWindow.Width || _oldPicWindowHeight != picWindow.Height)
                    {
                        SetWindowLocation();
                        _oldPicWindowWidth = picWindow.Width;
                        _oldPicWindowHeight = picWindow.Height;
                    }
                    SetViewSize();
                    imgOriginalWidth = image.Width;
                    imgOriginalHeigh = image.Height;
                }
            }
        }
        /// <summary>
        /// 设置窗体位置 居中
        /// </summary>
        private void SetWindowLocation()
        {
            picWindow.Left = (SystemParameters.WorkArea.Width - picWindow.ActualWidth) / 2;
            picWindow.Top = (SystemParameters.WorkArea.Height - picWindow.ActualHeight) / 2;
        }
        /// <summary>
        /// 复制图片
        /// </summary>
        private void CopyPic()
        {
            using (var webClient = new System.Net.WebClient())
            {
                var imgData = webClient.DownloadData(imagePath);
                using (var stream = new MemoryStream(imgData))
                {
                    BitmapImage bitImg = new BitmapImage();
                    bitImg.BeginInit();
                    bitImg.StreamSource = stream;
                    bitImg.EndInit();
                    System.Windows.Clipboard.SetImage(bitImg);
                }
            }
        }
        /// <summary>
        /// 旋转
        /// </summary>
        private void Rotate()
        {
            times++;
            if (times > 4) times = 0;
            Angel = 90 * times;
        }
        #endregion

        #region 计时处理
        DispatcherTimer myTimer = null;
        private void Timing()
        {
            if (PercentVisibility == Visibility.Visible || TipsVisibility == Visibility.Visible)
            {
                if (myTimer == null)
                    myTimer = new DispatcherTimer();
                myTimer.Interval = new TimeSpan(0, 0, 2);
                myTimer.Tick += new EventHandler(Timer_Tick);
                myTimer.Start();
            }
            else
            {
                if (myTimer != null)
                {
                    myTimer.Stop();
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (myTimer != null)
            {
                myTimer.Stop();
            }
            PercentVisibility = Visibility.Collapsed;
            TipsVisibility = Visibility.Collapsed;
        }

        public void Dispose()
        {

            GC.Collect();
        }
        #endregion
    }
}
