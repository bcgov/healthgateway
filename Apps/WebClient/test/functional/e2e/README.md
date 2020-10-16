# Functional Tests

Implements functional tests for Health Gateway using Cypress.io tooling.

## Prequisites

A Developer should have gone through the Health Gateway installation and configuration.

## Cypress

### Installation and configuration

Installation of Cypress is as easy as running npm install.

```bash
npm install
```

Create a cypress.env.json and update with passwords or any other environment variables you would like to override.

```bash
{
    "bcsc.password": "THE PASSWORD",
    "keycloak.password": "THE PASSWORD",
    "idir.password": "THE PASSWORD",
    "keycloak.unregistered.password": "THE PASSWORD"
}
```

### Running Interactively

While creating and debugging tests you will want to run Cypress interactively.  

```bash
export CYPRESS_BASE_URL=http://localhost:5000 
npm run launch
```

or

```bash
export CYPRESS_BASE_URL=http://localhost:5000
npx cypress open
```

If you want to verify the tests againt https://dev.healthgateway.gov.bc.ca then do not set the CYPRESS_BASE_URL environment variable.

### Running via CLI

You can run Cypress on the CLI and have the system record videos.

```bash
npm run e2e
```

You can also run Cypress as a specific browser by executing although this will launch and close Chrome many times. 

```bash
npx cypress run --browser chrome
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
