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
import "./commands";

require("cy-verify-downloads").addCustomCommand();

Cypress.on("window:before:load", (window) => {
    window.snowplow = () => undefined;

    const startupErrors = [];
    const consoleError = window.console.error.bind(window.console);

    window.console.error = (...args) => {
        startupErrors.push(
            args
                .map((arg) =>
                    arg instanceof Error
                        ? arg.stack || arg.message
                        : typeof arg === "string"
                          ? arg
                          : JSON.stringify(arg)
                )
                .join(" ")
        );
        consoleError(...args);
    };
    window.__healthGatewayStartupErrors = startupErrors;
});

function isUiSpec() {
    const normalizedSpecPath = Cypress.spec.relative.replaceAll("\\", "/");
    return /(^|\/)ui\//.test(normalizedSpecPath);
}

Cypress.on("fail", (error) => {
    const startupErrors =
        Cypress.state("window")?.__healthGatewayStartupErrors ?? [];
    if (startupErrors.length > 0) {
        error.message += `\n\nApplication startup errors:\n${startupErrors.join(
            "\n"
        )}`;
    }

    throw error;
});

beforeEach(() => {
    cy.intercept("GET", "**/snowplow.js", {
        body: "window.snowplow = () => undefined;",
        headers: {
            "content-type": "application/javascript",
        },
    });

    if (isUiSpec()) {
        cy.fixture("ConfigurationService/configuration.json").then((config) => {
            cy.intercept("GET", "**/configuration", {
                statusCode: 200,
                body: Cypress._.cloneDeep(config),
            });
        });
        cy.intercept("GET", "**/Communication/0", {
            fixture: "CommunicationService/communicationBanner.json",
        });
        cy.intercept("GET", "**/Communication/2", {
            fixture: "CommunicationService/communicationInApp.json",
        });
    }
});
