const { AuthMethod } = require("../../support/constants");

describe("Registration Page", () => {
    before(() => {
        cy.server();
        cy.fixture("AllDisabledConfig").then((config) => {
            config.webClient.modules.Comment = true;
            config.webClient.modules.Medication = true;
            cy.route("GET", "/v1/api/configuration/", config);
        });
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.unregistered.password"),
            AuthMethod.KeyCloak
        );
    });

    it("No sidebar or footer", () => {
        cy.contains("footer").should("not.exist");
        cy.get('[data-testid="sidebar"]').should("not.exist");
    });
});
