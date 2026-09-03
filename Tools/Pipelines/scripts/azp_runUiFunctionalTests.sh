#!/usr/bin/env bash
set -e
if [ "$#" -ne 3 ]; then
    echo "Usage: $0 <working_directory> <buildId> <tags>"
    exit 1
fi

workDir="$1"
buildId="$2"
tags="$3"

if [ ! -d "$workDir" ]; then
    echo "Error: The directory '$workDir' does not exist."
    exit 1
fi

pushd "$workDir"
echo "Installing dependencies"
npm ci

if [ -z "$IDIR_PASSWORD" ]; then
    echo "ERROR: The IDIR_PASSWORD variable is not set or is empty."
    exit 1
fi

echo "Running Cypress UI Functional Tests"
TZ=America/Vancouver npx cypress run \
  --env "bcsc.password=$BCSC_PW,keycloak.password=$KEYCLOAK_PW,idir.password=$IDIR_PASSWORD,phoneNumber=$PHONENUMBER,keycloak.erebus.client=$KEYCLOAK_EREBUS_CLIENT,keycloak.erebus.secret=$KEYCLOAK_EREBUS_SECRET,keycloak.phsa.client=$KEYCLOAK_PHSA_CLIENT,keycloak.phsa.secret=$KEYCLOAK_PHSA_SECRET" \
  --record \
  --key "$CYPRESS_KEY" \
  --parallel \
  --ci-build-id "$buildId-ui" \
  --group "ui" \
  --tag "$tags" \
  --spec "cypress/integration/ui/**/!(auth.js)" \
  --headless \
  --browser chrome
popd
