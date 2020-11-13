const { AuthMethod } = require("../../support/constants")

describe('MSP Visits', () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = true
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = false
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
            cy.checkTimelineHasLoaded();
        })
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=encounterTitle]')
            .should('be.visible');
        cy.get('[data-testid=encounterDescription]')
            .should('be.visible');
        cy.get('[data-testid=encounterDetailsButton').first()
            .click()
        cy.get('[data-testid=encounterClinicLabel')
            .should('be.visible');
        cy.get('[data-testid=encounterClinicName')
            .should('be.visible');
        cy.get('[data-testid=encounterClinicAddress')
            .should('be.visible');
    })
})