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

# UI specs run against a fake Keycloak and stubbed APIs, so no credentials are passed.
echo "Running Cypress UI Functional Tests"
TZ=America/Vancouver npx cypress run \
  --record \
  --key "$CYPRESS_KEY" \
  --parallel \
  --ci-build-id "${buildId}-ui" \
  --group "ui" \
  --tag "$tags" \
  --spec "cypress/integration/ui/**/!(auth.js)" \
  --headless \
  --browser chrome
popd
