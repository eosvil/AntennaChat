using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace AntennaChat.Resource
{
    /// <summary>
    /// TextBox的行为在文本框上显示提示用户应该输入什么内容。
    /// </summary>
    public class WatermarkTextBoxBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// 提示内容
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WatermarkTextBoxBehavior),
                                        new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// 是否在文本中显示提示内容
        /// </summary>
        public static readonly DependencyPropertyKey IsWatermarkedPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsWatermarked", typeof(bool), typeof(WatermarkTextBoxBehavior),
                                        new FrameworkPropertyMetadata(false));


        public static readonly DependencyProperty IsWatermarkedProperty = IsWatermarkedPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取文本框的当前有水印的状态。
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static bool GetIsWatermarked(TextBox tb)
        {
            return (bool)tb.GetValue(IsWatermarkedProperty);
        }

        /// <summary>
        /// 获取或设置文本框的当前有水印的状态。
        /// </summary>
        private bool IsWatermarked
        {
            get { return (bool)AssociatedObject.GetValue(IsWatermarkedProperty); }
            set { AssociatedObject.SetValue(IsWatermarkedPropertyKey, value); }
        }

        /// <summary>
        /// 水印文本
        /// </summary>
        [Category("Appearance")]
        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        /// <summary>
        /// 行为附加到关联对象之后发生
        /// </summary>
        /// <remarks>
        /// 重写这个方法连接到关联对象
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
            OnLostFocus(null, null);
        }


        /// <summary>
        /// 文本改变时调用此方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isChangingText && !AssociatedObject.IsFocused)
            {
                OnLostFocus(null, null);
            }
            else
            {
                //if (string.IsNullOrEmpty(AssociatedObject.Text) && !_isChangingText)
                //{
                //    IsWatermarked = true;
                //    AssociatedObject.Text = this.Text;
                //}
                if (AssociatedObject.Text == this.Text)
                {
                    AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGray);
                }
                else
                {
                    AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                }
            }
        }

        /// <summary>
        /// 脱离相关对象之前发生的方法
        /// </summary>
        /// <remarks>
        /// 重写脱离相关对象方法
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        /// <summary>
        /// TextBox获得焦点时移除水印提示文字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsWatermarked)
            {
                IsWatermarked = false;
                ChangeText(string.Empty);
            }
            //Keyboard.Focus((TextBox)sender);
            //(sender as TextBox).SelectAll();
            //AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
            //IsWatermarked = false;
        }

        /// <summary>
        /// TextBox失去焦点时，如果内容为空水印提示文字，反之。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                IsWatermarked = true;
                ChangeText(this.Text);
                //AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGray);
            }
            else if (AssociatedObject.Text != this.Text)
            {
                AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                IsWatermarked = false;
            }
        }

        /// <summary>
        /// 附加关联对象鼠标单击获取焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (!textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                textBox.SelectAll();
                e.Handled = true;
            }

        }

        private bool _isChangingText;

        /// <summary>
        /// 这个方法是用来改变文本。
        /// </summary>
        /// <param name="newText">分配新的文本</param>
        private void ChangeText(string newText)
        {
            _isChangingText = true;
            AssociatedObject.Text = newText;
            _isChangingText = false;
        }
    }
}
