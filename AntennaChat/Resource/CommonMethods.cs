using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antenna.Framework;

namespace AntennaChat.Resource
{
    public class CommonMethods
    {
        public static bool StartApplication(string fileAddress)
        {
            bool isStart = false;
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = fileAddress;
                startInfo.Arguments = "source";
                startInfo.Verb = "runas";
                System.Diagnostics.Process.Start(startInfo);
                isStart = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[CommonMethods.StartApplication]:" + fileAddress + ex.Message);
                isStart = false;
            }
            return isStart;
        }
    }
}
