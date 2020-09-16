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

    it('Test2', () => {

    })
})