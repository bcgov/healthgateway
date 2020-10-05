const { AuthMethod } = require("../../support/constants")

describe('User Profile - Account Closure', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.accountclosure.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Profile page url should contain registration', () => {
        cy.url().should('include', '/registration')
    })
})