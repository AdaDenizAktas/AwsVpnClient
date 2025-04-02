using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AwsVpnClient.Core.Enums;
using AwsVpnClient.Core.Interfaces;
using AwsVpnClient.Infrastructure.Aws;
using AwsVpnClient.Infrastructure.Security;
using AwsVpnClient.Infrastructure.WireGuard;
using AwsVpnClient.UI.Views;

namespace AwsVpnClient.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAwsVpnProvisioner _provisioner;
        private readonly IVpnConnector _connector;

        [ObservableProperty] private string accessKey;
        [ObservableProperty] private string secretKey;
        [ObservableProperty] private string selectedRegion;
        [ObservableProperty] private string status;
        [ObservableProperty] private bool isWireGuardInstalled;
        [ObservableProperty] private VpnConnectionStatus connectionStatus;
        [ObservableProperty] private string? serverIp;
        [ObservableProperty] private string serverKey;

        public bool IsConnected => ConnectionStatus == VpnConnectionStatus.Connected;
        public bool CanConnect => ConnectionStatus == VpnConnectionStatus.Disconnected;

        public MainViewModel()
        {
            _provisioner = new AwsVpnProvisioner();
            _connector = new WireGuardConnector();

            Status = "Disconnected";
            ConnectionStatus = VpnConnectionStatus.Disconnected;
            IsWireGuardInstalled = WireGuardDetector.IsWireGuardInstalled();

            LoadCredentials(); 
            
           
        }

        private void LoadCredentials()
        {
            var loaded = CredentialStorage.Load();
            if (loaded is not null)
            {
                AccessKey = loaded.Value.accessKey;
                SecretKey = loaded.Value.secretKey;
                SelectedRegion = loaded.Value.region;

                _provisioner.SetCredentials(AccessKey, SecretKey);
                ServerIp = loaded.Value.publicIp;
            }
        }

        partial void OnConnectionStatusChanged(VpnConnectionStatus value)
        {
            Status = value.ToString();
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(CanConnect));
        }

        [RelayCommand]
        private async Task ConnectVpn()
        {
            if (string.IsNullOrEmpty(SelectedRegion) ||
                string.IsNullOrEmpty(AccessKey) ||
                string.IsNullOrEmpty(SecretKey) ||
                string.IsNullOrEmpty(ServerIp))
            {
                MessageBox.Show("Server not configured. Open Server Manager first.");
                return;
            }

            ConnectionStatus = VpnConnectionStatus.Connecting;

            try
            {
                _provisioner.SetCredentials(AccessKey, SecretKey);

                var config = $"""
                [Interface]
                PrivateKey = {WireGuardKeyHelper.GeneratePrivateKey()}
                [Peer]
                PublicKey = {ServerKey}
                Endpoint = {ServerIp}:51820
                """;

                File.WriteAllText("vpn.conf", config);

                await _connector.ConnectAsync("vpn.conf");
                ConnectionStatus = _connector.IsConnected ? VpnConnectionStatus.Connected : VpnConnectionStatus.Error;
            }
            catch
            {
                ConnectionStatus = VpnConnectionStatus.Error;
            }
        }



        [RelayCommand]
        private async Task Disconnect()
        {
            await _connector.DisconnectAsync();
            ConnectionStatus = VpnConnectionStatus.Disconnected;
        }

        [RelayCommand]
        private void OpenServerManager()
        {
            var window = new ServerManagerWindow();
            window.ShowDialog();

            // Reload saved credentials in case user changed them
            LoadCredentials();
        }
    }
}
