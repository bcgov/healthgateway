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

echo WorkDir: $workDir
echo buildId: $buildId
echo tags: $tags

if [ -z "$IDIR_PASSWORD" ]; then
  echo "ERROR: The IDIR_PASSWORD variable is not set or is empty."
  exit 1
fi

if [ -z "$KEYCLOAK_ADMIN_SECRET" ]; then
  echo "ERROR: The KEYCLOAK_ADMIN_SECRET variable is not set or is empty."
  exit 1
fi

pushd "$workDir"
echo "Installing dependencies"
npm ci

echo "Running Cypress e2e Read and UI Functional Tests"
# Gather all test files in the `ui` folder
ui_files=$(find cypress/integration/ui -name '*.cy.js' | tr '\n' ',')

# Gather only `-read.cy.js` and `.cy.js` files in the `e2e` folder, excluding specific files
e2e_files=$(find cypress/integration/e2e -name '*.cy.js' \( -name '*-read.cy.js' -o ! -name '*-*' \) \
    ! -name '*-write.cy.js' ! -name 'unauthorized.cy.js' ! -name 'authentication.cy.js' ! -name 'agentaccess.cy.js' | tr '\n' ',')

# Combine `ui_files` and `e2e_files` into a single spec list
spec_files="${ui_files}${e2e_files}"

# Run Cypress with the combined spec list
TZ=America/Vancouver npx cypress run \
    --env "keycloak_password=$KEYCLOAK_PW,idir_password=$IDIR_PASSWORD,keycloak_admin_secret=$KEYCLOAK_ADMIN_SECRET" \
    --record \
    --key $CYPRESS_ADMIN_KEY \
    --parallel \
    --ci-build-id "$buildId-AdminRead" \
    --group "$buildId-AdminRead" \
    --tag "$tags" \
    --spec "${spec_files%,}" \
    --headless \
    --browser chrome
popd
