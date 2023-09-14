#!/usr/bin/env bash
set -e
if [ "$#" -ne 3 ]; then
    echo "Usage: $0 <working_directory> <buildId> <tags>"
    #Release should use:
    #   workDir=_FunctionalTests/drop/admin
    #   BuildId=$(Release.ReleaseName)-$(Release.AttemptNumber)
    #   tags=CICD,Release,Dev
    #Pipeline should use:
    #   workDir=Apps/Admin/Tests/Functional
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

echo "Running Cypress Functional Tests"
TZ=America/Vancouver npx cypress run \
  --env "keycloak_password=$(keycloak.pw),\
  idir_password=$(idir.password),\
  keycloak_admin_secret=$(keycloak.admin.secret)" \
  --record \
  --key $(cypress.admin.key) \
  --parallel \
  --ci-build-id "$buildId" \
  --group "$buildId" \
  --tag "$tags" \
  --spec "cypress/integration/ui/**/*,cypress/integration/e2e/**/*" \
  --headless \
  --browser chrome
popd