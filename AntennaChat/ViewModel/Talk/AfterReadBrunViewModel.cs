using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AntennaChat.ViewModel.Talk
{
    public class AfterReadBrunViewModel: PropertyNotifyObject
    {
        public ICommand btnKnow
        {
            get
            {
                return new DelegateCommand<Window>((obj) =>
                {
                    Window win=obj as Window;
                    win.Close();
                });
            }
        }
    }
}
