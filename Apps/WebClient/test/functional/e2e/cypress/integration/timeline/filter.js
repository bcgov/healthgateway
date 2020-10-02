const { AuthMethod } = require("../../support/constants")

describe('Filters', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Test1', () => {

    })

    it('Validate No Records', () => {
        cy.get('[data-testid=filterTextInput]').type('xxxx');
        cy.get('[data-testid=displayCountText]').contains('Displaying 0 out of ');
        cy.get('[data-testid=noTimelineEntriesText]').should('be.visible');
    })
})