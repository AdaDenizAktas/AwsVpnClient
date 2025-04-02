#!/bin/bash
apt update
apt install wireguard -y
# Server keygen + config creation
echo "net.ipv4.ip_forward=1" >> /etc/sysctl.conf
sysctl -p
ufw allow 51820/udp
systemctl enable wg-quick@wg0
systemctl start wg-quick@wg0
