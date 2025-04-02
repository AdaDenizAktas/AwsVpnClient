using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwsVpnClient.Infrastructure.WireGuard
{
    public static class WireGuardDetector
    {
        public static bool IsWireGuardInstalled()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "wireguard.exe",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return !string.IsNullOrWhiteSpace(output) && File.Exists(output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            catch
            {
                return false;
            }
        }
    }
}
