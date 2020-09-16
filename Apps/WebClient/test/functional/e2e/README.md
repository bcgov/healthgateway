#Functional Tests

Implements functional tests for Health Gateway using Cypress.io tooling.

## Prequisites

A Developer should have gone through the Health Gateway installation and configuration.

## Cypress

### Installation

Installation of Cypress is as easy as running npm install.

```bash
npm install
```

### Running Interactively

While creating and debugging tests you will want to run Cypress interactively.  

```bash
npm run launch
```

### Running via CLI

You can run Cypress on the CLI and have the system record videos.

```bash
npm run e2e
```

You can also run Cypress as a specific browser by executing although this will launch and close Chrome many times. 

```bash
npx cypress run --browser chrome
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

