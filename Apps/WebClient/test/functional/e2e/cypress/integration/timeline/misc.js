const { AuthMethod } = require("../../support/constants")

describe('Notes', () => {
    beforeEach(() => {
    })

    it('Empty Timeline', () => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = false
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'),
                Cypress.env('keycloak.password'),
                AuthMethod.KeyCloak);
            cy.checkTimelineHasLoaded();
        })
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#listControls').find('.col').contains('Displaying 0 out of 0 records')
        // TODO: Detect the Empty image
    })
})