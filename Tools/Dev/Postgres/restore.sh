#!/bin/sh
now="$(date +'%d_%m_%Y_%H_%M_%S')"
echo "Restoring $1 started at $(date +'%d-%m-%Y %H:%M:%S')"
docker exec gatewaydb pg_restore --username=gateway --dbname=gateway -v --no-privileges --no-owner -c $1
echo "pg_restore finished at $(date +'%d-%m-%Y %H:%M:%S')"
