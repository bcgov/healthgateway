const { AuthMethod } = require("../../../support/constants")

describe('Keycloak Login', () => {
    before(() => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Timeline page should display', () => {
        cy.contains('#subject', 'Health Care Timeline')
    })
})
