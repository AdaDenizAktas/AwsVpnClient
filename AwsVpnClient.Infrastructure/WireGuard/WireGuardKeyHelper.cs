using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwsVpnClient.Infrastructure.WireGuard
{
    public class WireGuardKeyHelper
    {
        public static string GeneratePrivateKey()
        {
            return Execute("wg", "genkey");
        }

        public static string GeneratePublicKey(string privateKey)
        {
            return Execute("wg", "pubkey", privateKey);
        }

        private static string Execute(string file, string args, string input = "")
        {
            var startInfo = new ProcessStartInfo(file, args)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            if (!string.IsNullOrEmpty(input))
                process.StandardInput.WriteLine(input);

            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result.Trim();
        }

    }
}
