const { AuthMethod } = require("../../support/constants");

describe("Filters", () => {
    it("Validate disabled filters", () => {
        cy.server();
        cy.fixture("AllDisabledConfig").as("config");
        cy.fixture("AllDisabledConfig")
            .then((config) => {
                config.webClient.modules["Medication"] = true;
            })
            .as("config");
        cy.route("GET", "/v1/api/configuration/", "@config");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=medicationCount]").should("be.visible");
        cy.get("[data-testid=immunizationCount]").should("not.be.visible");
        cy.get("[data-testid=encounterCount]").should("not.be.visible");
        cy.get("[data-testid=noteCount]").should("not.be.visible");
        cy.get("[data-testid=laboratoryCount]").should("not.be.visible");
    });
});
