using AntennaChat.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AntennaChat.ViewModel.Contacts
{
    public class ContactInfoViewModel : BaseViewModel
    {
        public bool IsMouseClick = false;
        #region 构造器
        public ContactInfoViewModel(string picture, string name, string position)
        {
            if (!string.IsNullOrWhiteSpace(picture))
            {
                _Photo = new BitmapImage(new Uri(picture));
            }
            else
            {
                _Photo = new BitmapImage(new Uri("pack://application:,,,/AntennaChat;Component/Images/logo-预览-100.png"));
            }
            _Name = name;
            _Position = position;
        }
        #endregion

        #region 属性
        private ImageSource _Photo;
        /// <summary>
        /// 头像
        /// </summary>
        public ImageSource Photo
        {
            get { return this._Photo; }
            set
            {
                this._Photo = value;
                RaisePropertyChanged(() => Photo);
            }
        }

        private string _Name="赵雪峰";
        /// <summary>
        /// 头像
        /// </summary>
        public string Name
        {
            get { return this._Name; }
            set
            {
                this._Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private string _Position;
        /// <summary>
        /// 头像
        /// </summary>
        public string Position
        {
            get { return this._Position; }
            set
            {
                this._Position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        private Brush _Background;
        /// <summary>
        /// 背景色
        /// </summary>
        public Brush Background
        {
            get { return this._Background; }
            set
            {
                this._Background = value;
                RaisePropertyChanged(() => Background);
            }
        }


        private double _PlaceholderWidth;
        /// <summary>
        /// 占位符宽度
        /// </summary>
        public double PlaceholderWidth
        {
            get { return this._PlaceholderWidth; }
            set
            {
                this._PlaceholderWidth = value;
                RaisePropertyChanged(() => PlaceholderWidth);
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 鼠标进入颜色变化
        /// </summary>
        private ICommand _MouseEnter;
        public ICommand MouseEnter
        {
            get
            {
                if (this._MouseEnter == null)
                {
                    this._MouseEnter = new DefaultCommand(o =>
                    {
                        _Background = (Brush)(new BrushConverter()).ConvertFromString("#F0F4FA");
                    });
                }
                return this._MouseEnter;
            }
        }
        /// <summary>
        /// 鼠标进入颜色变化
        /// </summary>
        private ICommand _MouseLeave;
        public ICommand MouseLeave
        {
            get
            {
                if (this._MouseLeave == null)
                {
                    this._MouseLeave = new DefaultCommand(o =>
                    {
                        if (this.IsMouseClick)
                        {
                            this.Background = (Brush)(new BrushConverter()).ConvertFromString("#E9ECF2");
                        }
                        else
                        {
                            this.Background = (Brush)(new BrushConverter()).ConvertFromString("#FFFFFF");
                        }
                    });
                }
                return this._MouseLeave;
            }
        }
        #endregion
    }
}
