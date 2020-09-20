const { AuthMethod } = require("../../support/constants")

describe('Authentication', () => {
    beforeEach(() => {
    })

    it('BCSC Login', () => {
        cy.login(Cypress.env('bcsc.username'), Cypress.env('bcsc.password'), AuthMethod.BCSC)
        cy.get('#menuBtnLogout').should('be.visible')
            .should('have.text', ' Logout ')
    })

    it('KeyCloak Login', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#menuBtnLogout').should('be.visible')
            .should('have.text', ' Logout ')
    })

    it('Logout', () => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#menuBtnLogout').click()
        cy.get('#skipButton').click()
        cy.contains('h3', 'You signed out of your account')
        cy.get('#menuBtnLogin').should('be.visible')
            .should('have.attr', 'href', '/login')
            .should('have.text', ' Login ')
    })

    it('Idle Timeout', () => {
        cy.server()
        cy.fixture('AllDisabledConfig').then((config) => {
            config.webClient.timeouts.idle = 1000
        }).as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('.modal-header').contains('Are you still there?')
        cy.get('.modal-body').contains('You will be automatically logged out in 59 seconds.')
        cy.get('.modal-body').contains('You will be automatically logged out in 55 seconds.')
        cy.get('.btn-primary').should('have.text', "I'm here!");
    })
})