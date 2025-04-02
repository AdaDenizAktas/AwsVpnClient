using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AwsVpnClient.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenWireGuardSite(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.wireguard.com/install/",
                UseShellExecute = true
            });
        }
    }
}
