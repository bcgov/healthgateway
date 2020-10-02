const { AuthMethod } = require("../../support/constants")

describe('Health Insights', () => {
    before(() => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#menuBtnHealthInsights').click()
    })

    it('totally has 259 records', () => {
        cy.get('[data-testid=totalRecordsText]').contains('259 ')
    })
})