const { AuthMethod } = require("../../support/constants")

describe('Menu System', () => {
    beforeEach(() => {
        cy.request(`${Cypress.config("baseUrl")}/v1/api/configuration`)
        .should((response) => { expect(response.status).to.eq(200) })
        .its("body").then(config => {
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
    })

    it('Validate Toggle Sidebar', () => {
        cy.get('[data-testid=sidebarUserName]').should('not.be.visible');
        cy.get('[data-testid=sidebarToggle]').click();
        cy.get('[data-testid=sidebarUserName]')
            .should('be.visible')
            .should('have.text', 'Dr Gateway');
        cy.get('[data-testid=sidebarToggle]').click();
        cy.get('[data-testid=sidebarUserName]')
            .should('not.be.visible');
    })
})