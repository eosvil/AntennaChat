using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AntennaChat.ViewModel.FileUpload
{
    public class FileUserControlViewModel
    {
        public ICommand btnFileCommandClose
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    
                });
            }
        }
    }
}
