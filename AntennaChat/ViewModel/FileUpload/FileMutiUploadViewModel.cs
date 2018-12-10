using Antenna.Model;
using AntennaChat.Views.FileUpload;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AntennaChat.ViewModel.FileUpload
{
    public class FileMutiUploadViewModel : PropertyNotifyObject
    {
        FileMultiUpload fMultis;
        List<UpLoadFilesDto> listDto = null;
        RowDefinition rd;
        public FileMutiUploadViewModel(List<UpLoadFilesDto> listDto, FileMultiUpload fMultis, RowDefinition rd)
        {
            this.fMultis = fMultis;
            this.listDto = listDto;
            this.rd = rd;
            rd.Height = new GridLength(102);

            foreach (var list in listDto)
            {
                FileUserControl fUserControl = new FileUserControl();
                fUserControl.btnClose.Click += BtnClose_Click;
                fUserControl.fileName.Text = list.fileName.Length > 4 ? list.fileName.Substring(0, 4) : list.fileName;

                fUserControl.img.Source = new BitmapImage(new Uri(fileShowImage.switchShowImage(list.fileExtendName), UriKind.RelativeOrAbsolute));
                fUserControl.fileName.ToolTip = list.fileName;
                fUserControl.btnClose.Tag = fUserControl;
                fMultis.wPanel.Children.Add(fUserControl);
            }
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button fileControl = sender as Button;
            fMultis.wPanel.Children.Remove(fileControl.Tag as FileUserControl);
        }
        public ICommand btnCanelUpLoadFile
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    rd.Height = new GridLength(0);
                    fMultis.wPanel.Children.Clear();
                });
            }
        }
        public ICommand btnSendUploadFile
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    
                });
            }
        }
    }
}
