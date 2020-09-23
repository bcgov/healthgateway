const { AuthMethod } = require("../../support/constants")

describe('Menu System', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Test1', () => {

    })

    it('Test2', () => {

    })
})