using System;
using System.Windows;
using AwsVpnClient.UI.ViewModels;

namespace AwsVpnClient.UI.Views
{
    public partial class ServerManagerWindow : Window
    {
        private ServerManagerViewModel ViewModel => (ServerManagerViewModel)DataContext;

        public ServerManagerWindow()
        {
            InitializeComponent();
            ViewModel.CloseWindow = Close;
        }

        private void SecretKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServerManagerViewModel vm && sender is System.Windows.Controls.PasswordBox box)
                if (box.Password != "")
                    vm.SecretKey = box.Password;
        }
        public void ClearInputFields()
        {
            AccessKeyBox.Text = "";
            SecretKeyBox.Password = "";
        }

    }
}
