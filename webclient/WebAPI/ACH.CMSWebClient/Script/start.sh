#!/bin/bash
systemctl daemon-reload
systemctl enable ACH.CMSWebClient.service
systemctl start ACH.CMSWebClient.service
echo 'ACH.CMSWebClient.service start Success'

