const { AuthMethod } = require("../../support/constants");

describe("Bookmark", () => {
    before(() => {
        cy.server();
        cy.fixture("AllDisabledConfig").then((config) => {
            config.webClient.modules.Comment = true;
            config.webClient.modules.Medication = true;
            cy.route("GET", "/v1/api/configuration/", config);
        });
    });

    it("Redirect to UserProfile", () => {
        let path = "/profile";
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", path);
    });
    it("Redirect to Insights", () => {
        let path = "/healthInsights";
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", path);
    });
});
