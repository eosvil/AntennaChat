using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.Views;
using System.Windows.Threading;

namespace AntennaChat.ViewModel.Setting
{
    public class ChangeKeyViewModel : PropertyNotifyObject
    {
        private string oldKeyNum;
        private TextBox textBox;
        public event EventHandler ChangeKeySuccessEvent = null;
        public ChangeKeyViewModel()
        {
            SetKey();
        }

        /// <summary>
        /// 快捷键
        /// </summary>
        private string _keyNum;
        public string KeyNum
        {
            get { return this._keyNum; }
            set
            {
                this._keyNum = value;
                RaisePropertyChanged(() => KeyNum);
            }
        }
        /// <summary>
        /// 快捷键按钮是否可用
        /// </summary>
        private bool _keyButtonIsEnable;
        public bool KeyButtonIsEnable
        {
            get { return this._keyButtonIsEnable; }
            set
            {
                this._keyButtonIsEnable = value;
                RaisePropertyChanged(() => KeyButtonIsEnable);
            }
        }
        /// <summary>
        /// 输入控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TxtName_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key < Key.A || e.Key > Key.Z) && e.Key != Key.Back && e.Key != Key.Delete)
            {
                KeyNum = oldKeyNum;
            }
            else if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                KeyNum = e.Key.ToString();
            }
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectionStart = textBox.Text.Length;
            }
            KeyButtonIsEnable = oldKeyNum != KeyNum;
        }
        private void SetKey()
        {
            if (GlobalVariable.systemSetting == null || string.IsNullOrEmpty(GlobalVariable.systemSetting.KeyShortcuts))
                KeyNum = "Q";
            else
            {
                var key = (System.Windows.Forms.Keys)Convert.ToInt32(GlobalVariable.systemSetting.KeyShortcuts);
                KeyNum = key.ToString();
            }
            oldKeyNum = KeyNum;
        }
        /// <summary>
        /// 加载
        /// </summary>
        private ActionCommand<TextBox> _stackPanelLoaded;
        public ActionCommand<TextBox> StackPanelLoaded
        {
            get
            {
                if (this._stackPanelLoaded == null)
                {
                    this._stackPanelLoaded = new ActionCommand<TextBox>(
                           o =>
                           {
                               textBox = o;
                           });
                }
                return this._stackPanelLoaded;
            }
        }
        private ICommand _textBlockMouseLeftButtonDown;
        public ICommand TextBlockMouseLeftButtonDown
        {
            get
            {
                if (this._textBlockMouseLeftButtonDown == null)
                {
                    this._textBlockMouseLeftButtonDown = new DefaultCommand(
                          o =>
                          {
                              if (textBox != null)
                              {
                                  textBox.Focus();
                                  textBox.SelectionStart = textBox.Text.Length;
                              }
                          });
                }
                return this._textBlockMouseLeftButtonDown;
            }
        }
        private ICommand _textBoxLostFocus;
        public ICommand TextBoxLostFocus
        {
            get
            {
                if (this._textBoxLostFocus == null)
                {
                    this._textBoxLostFocus = new DefaultCommand(
                        o =>
                        {
                          
                        });
                }
                return this._textBoxLostFocus;
            }
        }
        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (this._closeCommand == null)
                {
                    this._closeCommand = new DefaultCommand(
                        o =>
                        {
                            if (oldKeyNum != KeyNum)
                                SaveKey();
                            //ChangeKeySuccessEvent?.Invoke(this, EventArgs.Empty);
                        });
                }
                return this._closeCommand;
            }
        }
        /// <summary>
        /// 保存按键
        /// </summary>
        private bool SaveKey()
        {
            try
            {
                string errMsg = string.Empty;
                oldKeyNum = KeyNum;
                var keyChar = Convert.ToChar(KeyNum);
                var key = (System.Windows.Forms.Keys)keyChar;
                if (GlobalVariable.systemSetting == null)
                    GlobalVariable.systemSetting = new SystemSetting();
                GlobalVariable.systemSetting.KeyShortcuts = ((int)key).ToString();
                if (
                    DataConverter.EntityToXml<SystemSetting>(
                        System.Environment.CurrentDirectory + "/SysSettingCache.xml", GlobalVariable.systemSetting,
                        ref errMsg))
                {
                    MessageBoxWindow.Show("提示", "截图快捷键设置成功！", MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Success);
                    KeyButtonIsEnable = false;
                    return true;
                }
                else
                {
                    MessageBoxWindow.Show("提示", "快捷键设置出错！"+errMsg, MessageBoxButton.OK, GlobalVariable.WarnOrSuccess.Warn);
                    LogHelper.WriteError("快捷键设置出错：" + errMsg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("快捷键设置出错：" + ex.Message);
                return false;
            }
        }
    }
}
