#!/usr/bin/env bash
set -e
if [ "$#" -ne 3 ]; then
    echo "Usage: $0 <working_directory> <buildId> <tags>"
    #Release should use:
    #   workDir=_FunctionalTests/drop/gateway
    #   BuildId=$(Release.ReleaseName)-$(Release.AttemptNumber)
    #   tags=CICD,Release,Dev
    #Pipeline should use:
    #   workDir=Testing/functional/tests
    #   BuildId=$(Build.BuildNumber)-$(System.JobAttempt)
    #   tags="dev,$(Build.Reason)"
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

echo "Running Cypress Functional Tests"
TZ=America/Vancouver npx cypress run \
  --env "bcsc.password=$BCSC_PW,keycloak.password=$KEYCLOAK_PW,idir.password=$IDIR_PASSWORD,phoneNumber=$PHONENUMBER,keycloak.phsa.client=$KEYCLOAK_PHSA_CLIENT,keycloak.phsa.secret=$KEYCLOAK_PHSA_SECRET" \
  --record \
  --key $CYPRESS_KEY \
  --parallel \
  --ci-build-id "$buildId" \
  --group "$buildId" \
  --tag "$tags" \
  --spec "cypress/integration/ui/**/*,cypress/integration/e2e/**/*" \
  --headless \
  --browser chrome
popd