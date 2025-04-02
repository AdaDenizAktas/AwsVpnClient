using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwsVpnClient.Core.Enums
{
    public enum VpnConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }
}
