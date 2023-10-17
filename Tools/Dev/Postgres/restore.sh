#!/bin/sh
backupDir=$1
echo "pg_dump started at $(date +'%d-%m-%Y %H:%M:%S')s" 
docker exec gatewaydb pg_restore -Fd -v -U hglocal -d hglocal -O -j 9 -c --if-exists ${backupDir}
echo "pg_dump finished at $(date +'%d-%m-%Y %H:%M:%S')" 
