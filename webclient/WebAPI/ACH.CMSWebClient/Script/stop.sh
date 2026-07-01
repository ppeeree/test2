#!/bin/bash
systemctl stop ACH.CMSWebClient.service
systemctl disable ACH.CMSWebClient.service
echo 'ACH.CMSWebClient.service stop Success'
