using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AntennaChat.ViewModel
{
    public class PropertyNotifyObject : INotifyPropertyChanged
    {
        /// <summary>
		/// 属性更改事件
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 提示属性已更改
        /// </summary>
        /// <param name="propertyName">更改的属性名称</param>
        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 提示属性已更改
        /// </summary>
        /// <typeparam name="TProperty">更改的属性类型</typeparam>
        /// <param name="expr">更改的属性表达式</param>
        public void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> expr)
        {
            MemberExpression memberExpr = (MemberExpression)expr.Body;
            string memberName = memberExpr.Member.Name;
            RaisePropertyChanged(memberName);
        }
    }    
}
