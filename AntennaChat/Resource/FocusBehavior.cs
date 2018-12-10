using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AntennaChat.Resource
{
    /// <summary>
    /// 获取焦点帮助类
    /// </summary>
    public static class FocusBehavior
    {
        public static bool? GetIsFocused(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(FocusBehavior), new UIPropertyMetadata()
        {
            DefaultValue = null,
            PropertyChangedCallback =
                    (s, e) =>
                    {
                        UIElement sender = (UIElement)s;
                        if (e.NewValue != null)
                        {
                            if ((bool)e.NewValue)
                            {
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                                {
                                    if (sender is ListBox)
                                    {
                                        var listBox = sender as ListBox;
                                        listBox.UpdateLayout();
                                        if (listBox.Items.Count > 0)
                                        {
                                            FocusManager.SetFocusedElement(sender, listBox);
                                            var lvi = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[0]);
                                            if (lvi != null)
                                            {
                                                lvi.Focusable = true;
                                                lvi.Focus();
                                                //lvi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                                            }
                                        }
                                    }
                                    else if (sender is TextBox)
                                    {
                                        var textbox = sender as TextBox;
                                        textbox.Focusable = true;
                                        textbox.Focus();
                                    }
                                }));

                            }
                        }
                    }
        });
    }
}
