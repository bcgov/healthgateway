const { AuthMethod } = require("../../support/constants")

describe('MSP Visits', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=encounter-filter]').click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jul 15, 2020');
        cy.get('[data-testid=encounterTitle]').should('have.text', 'ILCVAPPOJ CLPXWG');
        cy.get('[data-testid=encounterDescription]').should('have.text', 'Specialty Description: GENERAL PRACTICE');
    })
})