#!/bin/sh
now="$(date +'%Y%m%d_%H_%M_%S')"
filename="gateway_$now".gz
backupfolder="backups"
fullpathbackupfile="$backupfolder/$filename"
logfile="$backupfolder/"backup_log_"$(date +'%Y_%m')".txt
echo "pg_dump started at $(date +'%d-%m-%Y %H:%M:%S')s" 
docker exec gatewaydb pg_dump --username=gateway --dbname=gateway --format=c --file=$fullpathbackupfile --blobs 
echo "pg_dump finished at $(date +'%d-%m-%Y %H:%M:%S')" 
echo "operation finished at $(date +'%d-%m-%Y %H:%M:%S')"
echo "*****************" 
