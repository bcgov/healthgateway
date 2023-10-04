#!/usr/bin/env bash

if [ -z "$$1" ]; then
  echo "No Additional Parameters supplied for DB Connection"
  echo "If you're connecting to an Azure DB you'll want to add: sslmode=VerifyFull"
  dbHost=${PATRONI_POSTGRES_MASTER_SERVICE_HOST}
  xtraParms=""
else
  echo "Will connect to Azure DB with additional parameters"
  dbHost=${DB_HOST}  
  xtraParms=$1
  echo "Additional Parameters supplied for DB Connection ${xtraParms}"
fi

pushd _Database/drop
migrations=${RELEASE_ARTIFACTS__DATABASE_BUILDNUMBER}.efbundle
echo "Running migrations from ${migrations}"
chmod u+x $migrations
./$migrations --connection "Host=${dbHost};Database=${DB_NAME};User ID=${DB_USER};Password=${DB_PASSWORD};Command Timeout=300;$xtraParms"
popd