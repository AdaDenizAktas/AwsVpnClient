using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AwsVpnClient.Core.Interfaces;

namespace AwsVpnClient.Infrastructure.WireGuard
{
    public class WireGuardConnector : IVpnConnector
    {
        private Process _process;
        private string _configPath;

        public bool IsConnected
        {
            get
            {
                try
                {
                    var tunnelName = Path.GetFileNameWithoutExtension(_configPath);

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sc.exe",
                            Arguments = $"query {tunnelName}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }




        public async Task ConnectAsync(string configPath)
        {
            _configPath = configPath;

            var tunnelName = Path.GetFileNameWithoutExtension(_configPath);

            // Ensure the config file is named correctly for service recognition
            var targetPath = Path.Combine(AppContext.BaseDirectory, $"{tunnelName}.conf");
            File.Copy(_configPath, targetPath, overwrite: true);

            // Install the service
            Process.Start(new ProcessStartInfo
            {
                FileName = "wireguard.exe",
                Arguments = $"/installtunnelservice \"{targetPath}\"",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            })?.WaitForExit();

            // Start the service
            Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"start {tunnelName}",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            })?.WaitForExit();

            await Task.Delay(5000); // allow time for the service to start
        }


        public Task DisconnectAsync()
        {
            if (IsConnected)
            {
                Process.Start("wireguard.exe", $"/uninstalltunnelservice \"{_configPath}\"");
            }
            return Task.CompletedTask;
        }
    }
}
