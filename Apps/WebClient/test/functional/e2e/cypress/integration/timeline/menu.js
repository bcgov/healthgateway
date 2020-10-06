const { AuthMethod } = require("../../support/constants")

describe('Menu System', () => {
    beforeEach(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').as('config');
        cy.route('GET', '/v1/api/configuration/', '@config');
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
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