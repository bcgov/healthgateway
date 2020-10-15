const { AuthMethod } = require("../../support/constants");
const registrationPage = "/registration"

describe("Registration Page", () => {
    before(() => {
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
            cy.login(Cypress.env('keycloak.unregistered.username'), Cypress.env('keycloak.unregistered.password'), AuthMethod.KeyCloak, registrationPage);
        })
    });

    it("No sidebar or footer", () => {
        cy.url().should("include", registrationPage);
        cy.contains("footer").should("not.exist");
        cy.get('[data-testid="sidebar"]').should('not.be.visible');
    });
});
