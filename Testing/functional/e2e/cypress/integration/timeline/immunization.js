const { AuthMethod, localDevUri } = require("../../support/constants")

describe('Immunization', () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = true
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = false
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.log("Configuration", config);
        })
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details', () => {
      cy.get('[data-testid=immunizationTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProductTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProviderTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLotTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProductName]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProviderName]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLotNumber]')
        .should('be.visible')
    })
})