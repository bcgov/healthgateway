const { AuthMethod } = require("../../support/constants")

describe('Menu System', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
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