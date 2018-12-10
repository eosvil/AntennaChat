using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service
{
    public class GetComputeInfo
    {
        /// <summary>  
        /// 通过网络适配器获取MAC地址  
        /// </summary>  
        /// <returns></returns>  
        public static string GetMacAddressByNetworkInformation()
        {
            var macAddress = string.Empty;
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in nics.Where(adapter => !adapter.GetPhysicalAddress().ToString().Equals("")))
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();
                    for (var i = 1; i < 6; i++)
                    {
                        macAddress = macAddress.Insert(3 * i - 1, ":");
                    }
                    break;
                }
            }
            catch
            {
                LogHelper.WriteError(@"[HTTPService.GetMacAddressByNetworkInformation]:Error:");
            }
            //返回
            return macAddress;
        }
    }
}
