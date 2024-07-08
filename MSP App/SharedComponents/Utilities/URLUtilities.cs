using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Utilities
{
    public class URLUtilities
    {
        public async static Task<string?> GetConnectedWANAddress()
        {
            using (var client = new HttpClient())
            {
                string? wanIpAddress = await client.GetStringAsync("http://api.ipify.org");
                if (!string.IsNullOrEmpty(wanIpAddress))
                {
                    return wanIpAddress;
                }
            }
            return null;
        }
        public static string? GetConnectedIPv4Address()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Filter out loopback and other non-operational interfaces
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var ipProps = ni.GetIPProperties();

                    // Check for a default gateway to ensure it's the interface used for WAN
                    if (ipProps.GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork))
                    {
                        foreach (var ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
