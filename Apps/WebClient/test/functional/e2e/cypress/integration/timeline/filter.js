const { AuthMethod } = require("../../support/constants")

describe('Filters', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate No Records on timeline vue', () => {
        cy.get('[data-testid=filterTextInput]').type('xxxx');
        cy.get('[data-testid=noTimelineEntriesText]').should('be.visible');
        cy.get('[data-testid=monthViewToggle]').click()        
    })

    it('Validate No Records on calendar vue', () => {
        cy.get('[data-testid=monthViewToggle]').click()
        cy.get('[data-testid=filterTextInput]').type('xxxx');
        cy.get('[data-testid=noTimelineEntriesText]').should('be.visible');
    })
})