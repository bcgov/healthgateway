const { AuthMethod } = require("../../../support/constants")

describe('Medication', () => {
    beforeEach(() => {
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
        })
        cy.viewport('iphone-6');
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details for Mobile', () => {
        cy.get('[data-testid=timelineCard]')
          .first()
          .click()
        const entryDetailsModal = cy.get('[data-testid=entryDetailsModal]')
        entryDetailsModal.get('[data-testid=backBtn]')
          .should('be.visible')
        entryDetailsModal.get('[data-testid=entryCardDetailsTitle]')
          .should('be.visible')
        entryDetailsModal.get('[data-testid=entryCardDate]')
            .should('be.visible')
        
        entryDetailsModal.get('[data-testid=medicationTitle]')
            .should('be.visible');
        entryDetailsModal.get('[data-testid=medicationPractitioner]')
            .should('be.visible');
        entryDetailsModal.get('[data-testid=medicationPractitioner]')
            .should('be.visible');
    })
})