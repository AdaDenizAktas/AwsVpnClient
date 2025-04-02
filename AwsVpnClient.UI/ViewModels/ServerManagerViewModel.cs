using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AwsVpnClient.Infrastructure.Security;
using AwsVpnClient.Infrastructure.Aws;
using AwsVpnClient.Core.Interfaces;
using AwsVpnClient.UI.Views;

namespace AwsVpnClient.UI.ViewModels
{
    public partial class ServerManagerViewModel : ObservableObject
    {
        private readonly IAwsVpnProvisioner _provisioner;

        [ObservableProperty] private string accessKey;
        [ObservableProperty] private string secretKey;
        [ObservableProperty] private ObservableCollection<string> regions = new();
        [ObservableProperty] private string selectedRegion;
        [ObservableProperty] private bool credentialsSaved;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Regions))]
        private string? activeRegion;



        public Action? CloseWindow { get; set; }

        public ServerManagerViewModel()
        {
            _provisioner = new AwsVpnProvisioner();
            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            var saved = CredentialStorage.Load();
            if (saved is null)
            {
                CredentialsSaved = false;
                return;
            }

            AccessKey = saved.Value.accessKey;
            SecretKey = saved.Value.secretKey;
            SelectedRegion = saved.Value.region;
            CredentialsSaved = true;

            _provisioner.SetCredentials(AccessKey, SecretKey);
            _ = LoadRegionsAsync();
        }

        private async Task LoadRegionsAsync()
        {
            try
            {
                var result = await _provisioner.GetRegionsAsync();
                Regions = new ObservableCollection<string>(result);

                ActiveRegion = await _provisioner.GetActiveRegionAsync(result);
            }
            catch
            {
                MessageBox.Show("Failed to load regions.");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(SecretKey))
            {
                MessageBox.Show("Access and Secret keys are required.");
                return;
            }

            CredentialStorage.Save(AccessKey, SecretKey, SelectedRegion ?? "");
            _provisioner.SetCredentials(AccessKey, SecretKey);

            CredentialsSaved = true;
            await LoadRegionsAsync();

            MessageBox.Show("Credentials saved.");
        }

        [RelayCommand]
        private async Task CreateServerAsync(string region)
        {
            if (!string.IsNullOrEmpty(ActiveRegion) && ActiveRegion != region)
            {
                var confirm = MessageBox.Show(
                    $"You already created a server in {ActiveRegion}. Proceeding will incur charges. Terminate it first?",
                    "Warning", MessageBoxButton.YesNo);

                if (confirm != MessageBoxResult.Yes)
                    return;

                await _provisioner.TerminateVpnServerAsync(ActiveRegion);
            }

            SelectedRegion = region;
            var ip = await _provisioner.CreateVpnServerAsync(region);
            ActiveRegion = region;

            CredentialStorage.Save(AccessKey, SecretKey, region, ip);

            (Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.DataContext as MainViewModel)!.ServerIp = ip;

            MessageBox.Show($"VPN server created in {region}.");
        }


        [RelayCommand]
        private async Task TerminateServerAsync(string region)
        {
            await _provisioner.TerminateVpnServerAsync(region);
            if (ActiveRegion == region)
                ActiveRegion = null;

            MessageBox.Show($"VPN server in {region} terminated.");
        }

        [RelayCommand]
        private void ResetCredentials()
        {
            var confirm = MessageBox.Show("Are you sure you want to reset your AWS credentials?", "Confirm Reset", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            AccessKey = string.Empty;
            SecretKey = string.Empty;
            SelectedRegion = string.Empty;
            Regions.Clear();
            CredentialsSaved = false;
            CredentialStorage.Clear();

            (Application.Current.Windows.OfType<ServerManagerWindow>().FirstOrDefault())?.ClearInputFields();
        }
        partial void OnActiveRegionChanged(string? oldValue, string? newValue)
        {
            // Forces UI to re-evaluate all bindings depending on ActiveRegion
            OnPropertyChanged(nameof(ActiveRegion));
        }

    }
}
