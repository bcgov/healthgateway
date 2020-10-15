const { AuthMethod } = require("../../support/constants")

describe('Validate Star Rating', () => {
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

    it('Cliking the 5 star button should logout', () => {
        cy.get('[data-testid=logoutBtn]').click()
        cy.get('[data-testid=formRating] > .b-rating-star-empty:last').click()
        cy.url().should('include', '/logout')
    })

    it('Clicking Skip button should logout', () => {cy.get('[data-testid=logoutBtn]').click()
        cy.get('[data-testid=ratingModalSkipBtn]').click()
        cy.url().should('include', '/logout')
    })
})