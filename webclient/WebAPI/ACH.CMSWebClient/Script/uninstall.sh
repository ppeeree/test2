#!/bin/bash
systemctl stop ACH.CMSWebClient.service
systemctl disable ACH.CMSWebClient.service
rm /etc/systemd/system/ACH.CMSWebClient.service
systemctl daemon-reload

echo 'uninstall ACH.CMSWebClient.service success'

