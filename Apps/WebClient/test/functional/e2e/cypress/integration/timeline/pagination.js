require('cypress-xpath')
const { AuthMethod } = require("../../support/constants")

describe('Pagination', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Count Records', () => {
        // xpath is an additional library and we should probably not use it much but this should
        // help in migrating over Selenium Tests
        cy.xpath('//*[contains(@class, "entryCard")]').should('have.length', 13)
        cy.get('#listControls').find('.col').contains('Displaying 13 out of 442 records')
    })

    it('Validate Pages', () => {
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jul 15, 2020');
        cy.get('[data-testid=pagination]').contains("Next").click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jul 13, 2020');
        cy.get('[data-testid=pagination]').contains("Prev").click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jul 15, 2020');
        cy.get('[data-testid=pagination]').contains("4").click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jul 10, 2020');
    })
})