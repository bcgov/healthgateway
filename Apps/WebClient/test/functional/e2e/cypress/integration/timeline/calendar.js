const { AuthMethod } = require("../../support/constants")

describe('Calendar View', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Navigate to Month View', () => {
    })

    it('Validate Year Selector', () => {
    })
})