const { AuthMethod } = require("../../support/constants")

describe('Account Closure Profile', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.accountclosure.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate registration', () => {
        cy.url().should('include', '/registration')
    })
})