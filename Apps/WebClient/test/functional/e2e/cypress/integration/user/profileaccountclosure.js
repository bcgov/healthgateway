const { AuthMethod } = require("../../support/constants")

describe('User Profile - Account Closure', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.accountclosure.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#menuBtnProfile').click()
    })

    it('Profile page should contain the Account Closure details', () => {
        cy.get('#recoverBtn').should('have.text', 'Recover Account ')
    })
})