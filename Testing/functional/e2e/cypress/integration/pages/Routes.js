const { AuthMethod } = require("../../support/constants");

describe("Bookmark", () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = true
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
        })
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
