#!/bin/bash
ServerPath='/lib/systemd/system'
FileName='ACH.CMSWebClient.service'

chmod +x start.sh
chmod +x stop.sh
chmod +x uninstall.sh
chmod +x ACH.CMSWebClient

cp $FileName $ServerPath/$FileName
echo 'copy success'
./start.sh
echo 'ACH.CMSWebClient.service Init Success'
