const { AuthMethod } = require("../../support/constants");
const registrationPage = "/registration"

describe("Registration Page", () => {
    beforeEach(() => {
    })
    
    it("Minimum Age error", () => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);cy.login(Cypress.env('bcsc.hlthgw401.username'), Cypress.env('bcsc.hlthgw401.password'), AuthMethod.BCSC);
        });
        cy.url().should("include", registrationPage);
        cy.get('[data-testid="minimumAgeErrorText"]').should('be.visible');
    });

    it("Client Registration error", () => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('bcsc.hthgtwy12.username'), Cypress.env('bcsc.hthgtwy12.password'), AuthMethod.BCSC);   
        });
        cy.url().should("include", registrationPage);
        cy.get('[data-testid="clientRegistryErrorText"]').should('be.visible');
    });

    it("No sidebar or footer", () => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.unregistered.username'), 
                     Cypress.env('keycloak.unregistered.password'), 
                     AuthMethod.KeyCloak);
        });
        cy.checkTimelineHasLoaded();
        cy.url().should("include", registrationPage);
        cy.contains("footer").should("not.exist");
        cy.get('[data-testid="sidebar"]').should('not.be.visible');
    });
});
