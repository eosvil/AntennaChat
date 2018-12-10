using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AntennaChat.Resource
{
    public static class PreviewKeyDownBehavior
    {    

        #region 附加属性

        private static readonly DependencyProperty PreviewDropCommandProperty = DependencyProperty.RegisterAttached("PreviewDropCommand", typeof(ICommand),
            typeof(PreviewKeyDownBehavior), new PropertyMetadata(PreviewDropCommandPropertyChangedCallBack));

        private static readonly DependencyProperty PreviewKeyDownCommandProperty = DependencyProperty.RegisterAttached("PreviewKeyDownCommand", typeof(ICommand),
           typeof(PreviewKeyDownBehavior), new PropertyMetadata(PreviewKeyDownCommandPropertyChangedCallBack));

        /// <summary>
        /// 双击事件Command的附加属性
        /// </summary>
        private static readonly DependencyProperty PreviewMouseDownCommandProperty = DependencyProperty.RegisterAttached("PreviewDoubleClickCommand",
            typeof(ICommand),
            typeof(PreviewKeyDownBehavior));
        public static void SetPreviewDoubleClickCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(PreviewMouseDownCommandProperty, inCommand);
        }

        private static ICommand GetPreviewDoubleClickCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(PreviewMouseDownCommandProperty);
        }
        /// <summary>
        /// 双击事件参数的附加属性
        /// </summary>
        private static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter",
           typeof(string),
           typeof(PreviewKeyDownBehavior), new PropertyMetadata(PreviewDoubleClickCommandChanged));
        public static void SetCommandParameter(this UIElement inUIElement, object parameter)
        {
            inUIElement.SetValue(CommandParameterProperty, parameter);
        }

        private static object GetCommandParameter(UIElement inUIElement)
        {
            return (object)inUIElement.GetValue(CommandParameterProperty);
        }
        #endregion
        #region 设置或获取附加属性的值

        public static void SetPreviewDropCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(PreviewDropCommandProperty, inCommand);
        }

        private static ICommand GetPreviewDropCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(PreviewDropCommandProperty);
        }

        public static void SetPreviewKeyDownCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(PreviewKeyDownCommandProperty, inCommand);
        }

        private static ICommand GetPreviewKeyDownCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(PreviewKeyDownCommandProperty);
        }
        #endregion
        #region 属性改变的回调方法
        /// <summary>
        /// 双击事件处理初始的绑定和改变之后绑定
        /// </summary>
        /// <param name="inDependencyObject"></param>
        /// <param name="inEventArgs"></param>
        private static void PreviewDoubleClickCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = obj as UIElement;
            if (null == uiElement) return;
            var ui = GetCommandParameter(uiElement);
            if (ui is Window)
            {
            }
            uiElement.MouseDown += (sender, args) =>
            {
                if (args.ClickCount == 2)
                {
                    GetPreviewDoubleClickCommand(uiElement).Execute(GetCommandParameter(uiElement));
                    args.Handled = true;
                }
            };
        }
        /// <summary>
        /// 事件处理初始的绑定和改变之后绑定
        /// </summary>
        /// <param name="inDependencyObject"></param>
        /// <param name="inEventArgs"></param>
        private static void PreviewDropCommandPropertyChangedCallBack(
            DependencyObject obj, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = obj as UIElement;
            if (null == uiElement) return;
            uiElement.Drop += (sender, args) =>
            {
                GetPreviewDropCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }

        private static void PreviewKeyDownCommandPropertyChangedCallBack(
           DependencyObject obj, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = obj as UIElement;
            if (null == uiElement) return;
            uiElement.KeyUp += (sender, args) =>
            {
                GetPreviewKeyDownCommand(uiElement).Execute(args.Key);
                args.Handled = true;
            };
        }
        #endregion
    }
}
