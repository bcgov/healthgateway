const { AuthMethod } = require("../../support/constants")

describe('Immunization', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=immunization-filter]').click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jan 22, 2020');
        cy.get('[data-testid=immunizationTitle]').should('have.text', 'Shingles');
    })
})