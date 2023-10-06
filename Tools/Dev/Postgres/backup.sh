#!/bin/sh
now="$(date +'%Y%m%d_%H_%M_%S')"
filename="gateway_${now}"
backupfolder="backups"
fullpathbackupfile="$backupfolder/$filename"
echo "pg_dump started at $(date +'%d-%m-%Y %H:%M:%S')s" 
docker exec gatewaydb pg_dump -Fd -v -U hglocal -j 9 -f ${fullpathbackupfile}
echo "pg_dump finished at $(date +'%d-%m-%Y %H:%M:%S')" 
