const { AuthMethod } = require("../../../support/constants")

describe('Keycloak Login Protected', () => {
    before(() => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.protected.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Timeline page should display', () => {
        cy.contains('#subject', 'Health Care Timeline')
    })
})
