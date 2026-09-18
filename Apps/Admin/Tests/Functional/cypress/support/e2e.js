// ***********************************************************
// This example support/e2e.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import "./commands";

// Alternatively you can use CommonJS syntax:
// require('./commands')

function isUiSpec() {
    const normalizedSpecPath = Cypress.spec.relative.replaceAll("\\", "/");
    return /(^|\/)ui\//.test(normalizedSpecPath);
}

beforeEach(() => {
    if (isUiSpec()) {
        cy.fixture("ConfigurationService/configuration.json").then((config) => {
            cy.intercept("GET", "**/v1/api/Configuration*", {
                statusCode: 200,
                body: Cypress._.cloneDeep(config),
            });
        });
    }
});
