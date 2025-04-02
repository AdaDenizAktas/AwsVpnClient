#!/bin/bash

apt update
apt install wireguard -y

# Enable IP forwarding
echo "net.ipv4.ip_forward=1" >> /etc/sysctl.conf
sysctl -p

# Generate keys
mkdir -p /etc/wireguard
umask 077
wg genkey | tee /etc/wireguard/server_private.key | wg pubkey > /etc/wireguard/server_public.key

# Create minimal config (manual peer config is skipped for security)
cat <<EOF > /etc/wireguard/wg0.conf
[Interface]
Address = 10.0.0.1/24
ListenPort = 51820
PrivateKey = $(cat /etc/wireguard/server_private.key)
EOF

ufw allow 51820/udp
systemctl enable wg-quick@wg0
systemctl start wg-quick@wg0


echo "SERVER_PUBLIC_KEY: $(cat /etc/wireguard/server_public.key)"
