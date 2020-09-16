const { AuthMethod } = require("../../support/constants")

describe('Medication', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Protective Word', () => {

    })

    it('Test 2', () => {

    })
})