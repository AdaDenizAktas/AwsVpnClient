# AWS VPN Client

A minimal Windows VPN client to provision and connect to WireGuard servers on AWS EC2.

---

## 🔧 Features
- One-click VPN server creation using AWS
- Connect/disconnect to WireGuard via GUI
- Stores AWS credentials securely (locally encrypted)
- WiX installer included for packaging

---

## 📦 Requirements
- .NET 8.0 Desktop Runtime
- AWS EC2 access + SSH key (.pem)
- [WireGuard for Windows](https://www.wireguard.com/install/)

---

## 🚀 Installation
1. Download the `.msi` installer from Releases (or build from source).
2. Run the installer.
3. Launch the app and paste your AWS credentials.

---

## 🏗 Build Instructions

### Prerequisites
- .NET 8 SDK
- Visual Studio (with WPF)
- WiX Toolset 3.11

### Build Steps
```bash
dotnet publish -c Release -o publish AwsVpnClient.UI
msbuild AwsVpnClient.Installer/AwsVpnClient.Installer.wixproj
