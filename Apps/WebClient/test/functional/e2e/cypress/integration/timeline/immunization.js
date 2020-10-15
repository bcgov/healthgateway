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
        })
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details', () => {
        if (Cypress.config().baseUrl != localDevUri) {
            cy.get('[data-testid=immunizationTitle]')
                .should('be.visible');
        }
        else {
            cy.log("Skipped Filter Immunization as running locally")
        }
    })
})