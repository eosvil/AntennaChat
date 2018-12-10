using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace AntennaChat.Views.Talk
{
    public class RenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            const string script = "document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });";

            frame.ExecuteJavaScriptAsync(script);
        }

        void IRenderProcessMessageHandler.OnFocusedNodeChanged(IWebBrowser browserControl, IBrowser browser,IFrame frame,IDomNode iDomNode)
        {
            
        }
    }
}