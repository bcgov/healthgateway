const { AuthMethod } = require("../../support/constants")

describe('Notes', () => {
    beforeEach(() => {
    })

    it('Empty Timeline', () => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#listControls').find('.col').contains('Displaying 0 out of 0 records')
        // TODO: Detect the Empty image
    })
})