using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace AntennaChat.ViewModel.Talk
{
    public class MenuHandler : IContextMenuHandler
    {
        //private const int ShowCopy = 113;
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            //if (model.Count > 0)
            //{
            //    model.AddSeparator();
            //}
            model.Clear();
            //  model.AddItem(CefMenuCommand.Copy, "复制");
            ////model
            //System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
            //System.Windows.Controls.MenuItem miCopy = new System.Windows.Controls.MenuItem() { Header = "复制", Command = ApplicationCommands.Copy };
            ////cm.Items.Add(miCopy);
            ////model.AddItem()
            //Console.WriteLine("123456789-----------------------");
            //cms.Show(System.Windows.Forms.Cursor.Position);
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            // Console.WriteLine("-----------------OnContextMenuCommand-----------------------");
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            //Console.WriteLine("-----------------OnContextMenuDismissed-----------------------");
        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            // Console.WriteLine("-----------------RunContextMenu-----------------------");
            return false;
        }
    }
}
