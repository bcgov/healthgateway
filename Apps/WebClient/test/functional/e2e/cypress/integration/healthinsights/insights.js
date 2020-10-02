const { AuthMethod } = require("../../support/constants")

describe('Health Insights', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#covid-modal___BV_modal_header_ > .close').click()
        cy.get('#menuBtnHealthInsights').click()
    })

    it('totally has 259 records', () => {
        cy.get('[data-testid=totalRecordsText]').contains('259 ')
    })
})