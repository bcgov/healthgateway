# Functional Tests

Implements functional tests for Health Gateway using Cypress.io tooling.

## Prequisites

A Developer should have gone through the Health Gateway installation and configuration.

The developer should re-seed his/her database by connecting to the DB using PGAdmin or another client tool and running
cypress/db/seed.sql

## Cypress

### Installation and configuration

Installation of Cypress is as easy as running npm install.

```bash
npm install
```

Create a cypress.env.json and update with passwords or any other environment variables you would like to override.

```bash
{
    "baseUrl": "https://dev.healthgateway.gov.bc.ca",
    "bcsc.password": "THE PASSWORD",
    "keycloak.password": "THE PASSWORD",
    "idir.password": "THE PASSWORD",
    "keycloak.unregistered.password": "THE PASSWORD",
    "phoneNumber": "2505084843"
}
```

### Running Interactively

While creating and debugging tests you will want to run Cypress interactively.

```bash
export CYPRESS_BASE_URL=http://localhost:5002
npx cypress open --e2e
```

If you want to verify the tests against <https://dev.healthgateway.gov.bc.ca> then do not set the CYPRESS_BASE_URL environment variable.

e2e: contains tests that will be run in the dev environment only.
ui: contains tests that are either stubbed or cosmetic only and can be run in any environment
mock: contains tests that are custom to the mock environment.

### Running via CLI

You can run Cypress on the CLI and have the system record videos.

To Run all tests for dev environment execute:

```bash
npx cypress run --spec "cypress/integration/ui/**/*,cypress/integration/e2e/**/*"
```

to Run the tests intended for the mock environment:

```bash
npx cypress run --spec "cypress/integration/ui/**/*,cypress/integration/mock/**/*" --config baseUrl=https://mock.healthgateway.gov.bc.ca
```

You can also run Cypress as a specific browser by executing although this will launch and close Chrome many times.

```bash
npx cypress run --browser chrome
```

You can run a specific test

```bash
npx cypress run --spec "cypress/integration/user/emailValidation.js"
```

Run and record the results to the Cypress dashboard

```bash
npx cypress run --record --key KEY
```

## BrowserStack

Installation

```bash
npm install -g browserstack-cypress-cli
npx browserstack-cypress run --username USERNAME --key KEY
```

Environment variables
BROWSERSTACK_USERNAME
BROWSERSTACK_ACCESS_KEY
