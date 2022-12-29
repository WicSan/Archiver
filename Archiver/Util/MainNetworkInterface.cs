using System.Linq;
using System.Net.NetworkInformation;

namespace Archiver.Util
{
    public class MainNetworkInterface
    {
        public static double GetSpeedInKB()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                      && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                      && nic.OperationalStatus == OperationalStatus.Up)
                .FirstOrDefault()?
                .Speed / (1024.0 * 8) ?? 0;
        }
    }
}
