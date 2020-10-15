const { AuthMethod } = require("../../support/constants")

describe('Medication', () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = true
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
        })
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=medicationTitle]')
            .should('be.visible');
        cy.get('[data-testid=medicationPractitioner]')
            .should('not.be.visible');
        cy.get('[data-testid=medicationViewDetailsBtn]')
            .first()
            .click();
        cy.get('[data-testid=medicationPractitioner]')
            .should('be.visible');
    })
})