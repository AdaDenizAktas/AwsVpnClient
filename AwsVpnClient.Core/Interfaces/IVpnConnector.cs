using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwsVpnClient.Core.Interfaces
{
    public interface IVpnConnector
    {
        Task ConnectAsync(string configPath);
        Task DisconnectAsync();
        bool IsConnected { get; }
    }

}
