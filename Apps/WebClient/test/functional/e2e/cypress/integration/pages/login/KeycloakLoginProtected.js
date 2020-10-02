const { AuthMethod } = require("../../../support/constants")

describe('Keycloak Login Protected', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.protected.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#covid-modal___BV_modal_header_ > .close').click()
    })

    it('Timeline page should display', () => {
        cy.contains('#subject', 'Health Care Timeline')
    })
})
