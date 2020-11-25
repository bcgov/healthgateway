const { AuthMethod } = require("../../support/constants")

describe('User Feedback', () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = true
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'),
                Cypress.env('keycloak.password'),
                AuthMethod.KeyCloak);
            cy.checkTimelineHasLoaded();
        })
    })

    it('Send feedback', () => {
        cy.get('[data-testid=feedbackContainer]').click()
        cy.get('[data-testid=feedbackCommentInput]')
            .type('Great job team!');
        cy.get('[data-testid=sendFeedbackMessageBtn]').click()
        cy.get('[data-testid=noNeedBtn]').should('be.visible');
        cy.get('[data-testid=updateMyEmailButton]').should('be.visible');
    })

})