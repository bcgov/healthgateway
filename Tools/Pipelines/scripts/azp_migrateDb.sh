#!/usr/bin/env bash
pushd _Database/drop
migrations=${RELEASE_ARTIFACTS__DATABASE_BUILDNUMBER}.efbundle
echo "Running migrations from ${migrations}"
chmod u+x $migrations
./$migrations --connection "Host=${DB_HOST};Database=${DB_NAME};User ID=${DB_USER};Password=${DB_PASSWORD};Command Timeout=300;sslmode=VerifyFull"
popd