using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwsVpnClient.Core.Interfaces
{
    public interface IAwsVpnProvisioner
    {
        void SetCredentials(string accessKey, string secret);
        Task<List<string>> GetRegionsAsync();
        Task<string> CreateVpnServerAsync(string region);
        Task TerminateVpnServerAsync(string region);
        Task<string?> GetActiveRegionAsync(List<string> allRegions);

    }

}
