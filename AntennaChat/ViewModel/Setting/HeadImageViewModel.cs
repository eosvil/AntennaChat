using AntennaChat.Command;
using AntennaChat.Views.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AntennaChat.ViewModel.Setting
{
    public class HeadImageViewModel : WindowBaseViewModel
    {
        private ImageDealer _dealer;
        public delegate void ButtonClick(string filePath);
        public event ButtonClick ButtonClickEvent;
        /// <summary>
        /// 保存之前的图片
        /// </summary>
        private string _defaultFilePath;
        private string _imageFilePath;
        private BitmapSource _newHeadImage;
        public HeadImageViewModel(string path)
        {
            this._defaultFilePath = path;
            _imageFilePath =path;
        }
        /// <summary>
        /// 截图处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ImageDealerControl_OnCutImaging(object sender, System.Windows.RoutedEventArgs e)
        {
            HeadImagePath = (BitmapSource)e.OriginalSource;
        }
        /// <summary>
        /// 图片源
        /// </summary>
        public BitmapSource FilePath
        {
            get { return new BitmapImage(new Uri(_imageFilePath));}
            set
            {
                RaisePropertyChanged(() => FilePath);
            }
        }
        /// <summary>
        /// 剪切后的头像
        /// </summary>
        public BitmapSource HeadImagePath
        {
            get
            {
                if (_newHeadImage == null)
                    return new BitmapImage(new Uri(_imageFilePath));
                else
                    return _newHeadImage;
            }
            set
            {
                _newHeadImage = value;
                RaisePropertyChanged(() => HeadImagePath);
            }
        }
        private string _Tips = "图片大小<5MB";
        public string Tips
        {
            get { return _Tips; }
            set
            {
                _Tips = value;
                RaisePropertyChanged(() => Tips);
            }
        }
        private string _Foreground = "#A5A5A5";
        public string Foreground
        {
            get { return _Foreground; }
            set
            {
                _Foreground = value;
                RaisePropertyChanged(() => Foreground);
            }
        }
        protected virtual void OnButtonClick()
        {
            this.ButtonClickEvent?.Invoke(_imageFilePath);
        }
        private ICommand _OKCommand;
        public ICommand OKCommand
        {
            get
            {
                if (this._OKCommand == null)
                {
                    this._OKCommand = new DefaultCommand(
                          o =>
                          {
                              if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory+"\\Cache\\Contact") == false)//如果不存在就创建file文件夹
                              {
                                  Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Cache\\Contact");
                              }
                              string strFilePath = AppDomain.CurrentDomain.BaseDirectory+ "Cache\\Contact\\" + Guid.NewGuid().ToString()+".png";
                              try
                              {
                                  if (File.Exists(strFilePath) == true)
                                  {
                                      File.Delete(strFilePath);
                                  }
                                  JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                  encoder.Frames.Add(BitmapFrame.Create(HeadImagePath));
                                  FileStream fileStream = new FileStream(strFilePath, FileMode.Create, FileAccess.ReadWrite);
                                  encoder.Save(fileStream);
                                  fileStream.Close();
                                  FileInfo finfo = new FileInfo(strFilePath);
                                  if (finfo.Length > 204800)
                                  {
                                      //图片压缩
                                  }
                                  _imageFilePath = strFilePath;
                              }
                              catch(Exception e)
                              {
                              }
                              //存储剪切后的图片 todo
                              OnButtonClick();
                          });
                }
                return this._OKCommand;
            }
        }
        private ICommand _CancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (this._CancelCommand == null)
                {
                    this._CancelCommand = new DefaultCommand(
                          o =>
                          {
                              _imageFilePath = _defaultFilePath;//还原
                              OnButtonClick();
                          });
                }
                return this._CancelCommand;
            }
        }
        private ICommand _UpLoadCommand;
        public ICommand UpLoadCommand
        {
            get
            {
                if (this._UpLoadCommand == null)
                {
                    this._UpLoadCommand = new DefaultCommand(
                          o =>
                          {
                              OpenFileDialog openFile = new OpenFileDialog
                              {
                                  Filter = @"图片文件(*.jpg,*.png,*.jpeg)|*.jpg;*.png;*.jpeg",
                                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                  FilterIndex = 0
                              };
                              if (openFile.ShowDialog() == DialogResult.OK)
                              {
                                  FileInfo finfo = new FileInfo(openFile.FileName);
                                  if (finfo.Length > 5242880)
                                  {
                                      Tips = "图片太大,上传失败!";
                                      Foreground = "Red";
                                      return;
                                  }
                                  else
                                  {
                                      Tips = "图片大小<5MB";
                                      Foreground = "#A5A5A5";
                                      if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Cache\\Contact") == false)//如果不存在就创建file文件夹
                                      {
                                          Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Cache\\Contact");
                                      }
                                      string strFilePath = AppDomain.CurrentDomain.BaseDirectory + "Cache\\Contact\\" + Guid.NewGuid().ToString() + ".png";
                                      try
                                      {
                                          if (File.Exists(strFilePath))
                                          {
                                              File.Delete(strFilePath);
                                          }
                                          File.Copy(finfo.FullName, strFilePath, true);
                                          _imageFilePath = strFilePath;
                                          FilePath = new BitmapImage(new Uri(strFilePath, UriKind.RelativeOrAbsolute));//finfo.FullName;
                                          _dealer.CutImage();
                                      }
                                      catch (Exception ex)
                                      {

                                      }
                                  }
                              }
                          });
                }
                return this._UpLoadCommand;
            }
        }
        private ActionCommand<ImageDealer> _Loaded;
        public ActionCommand<ImageDealer> LoadedCommand
        {
            get
            {
                if (this._Loaded == null)
                {
                    this._Loaded = new ActionCommand<ImageDealer>(
                          o =>
                          {
                              _dealer = o;
                              o.OnCutImaging += _ImageDealerControl_OnCutImaging;
                          });
                }
                return this._Loaded;
            }
        }
    }
}
