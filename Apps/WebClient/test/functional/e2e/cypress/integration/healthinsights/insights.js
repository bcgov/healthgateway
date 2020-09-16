const { AuthMethod } = require("../../support/constants")

describe('Health Insights', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Test 1', () => {
    })

    it('Test 2', () => {
    })
})