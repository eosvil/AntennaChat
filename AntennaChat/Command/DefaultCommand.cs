using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace AntennaChat.Command
{
    public class DefaultCommand: ICommand
    {/// <summary>
     /// 检查命令是否可以执行的事件，在UI事件发生导致控件状态或数据发生变化时触发
     /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// 判断命令是否可以执行的方法
        /// </summary>
        private Func<object, bool> _canExecute;

        /// <summary>
        /// 命令需要执行的方法
        /// </summary>
        private Action<object> _execute;

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        public DefaultCommand(Action<object> execute):this(execute,null)
        {
        }

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="execute">命令要执行的方法</param>
        /// <param name="canExecute">判断命令是否能够执行的方法</param>
        public DefaultCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令传入的参数</param>
        /// <returns>是否可以执行</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute(parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (_execute != null && CanExecute(parameter))
            {
                _execute(parameter);
            }
        }
    }
    public class ActionCommand<T> : ICommand where T : class
    {
        private Predicate<T> _canExecuteMethod;
        private Action<T> _executeMethod;

        public ActionCommand(Action<T> executeMethod)
        {
            _canExecuteMethod = null;
            _executeMethod = executeMethod;
        }

        public ActionCommand(Action<T> executeMethod, Predicate<T> canExecuteMethod)
        {
            _canExecuteMethod = canExecuteMethod;
            _executeMethod = executeMethod;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod == null ? true : _canExecuteMethod(parameter as T);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_executeMethod != null)
            {
                _executeMethod(parameter as T);
            }
            UpdateCanExecute();
        }

        public void UpdateCanExecute()
        {
            var handls = CanExecuteChanged;
            if (handls != null)
            {
                handls(this, new EventArgs());
            }
        }
    }

    public class InvokeEventCommand : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeEventCommand));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            if (Command != null && Command.CanExecute(parameter))
                Command.Execute(parameter);
        }
    }
}
