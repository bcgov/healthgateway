const { AuthMethod } = require("../../../support/constants")

describe('Keycloak Login', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#covid-modal___BV_modal_header_ > .close').click()
    })

    it('Timeline page should display', () => {
        cy.contains('#subject', 'Health Care Timeline')
    })
})
