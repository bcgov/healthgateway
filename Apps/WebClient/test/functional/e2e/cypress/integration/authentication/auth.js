const { AuthMethod } = require("../../support/constants")

describe('Authentication', () => {
    beforeEach(() => {
    })

    it('BCSC Login', () => {
        cy.login(Cypress.env('bcsc.username'), Cypress.env('bcsc.password'), AuthMethod.BCSC)
        cy.get('#menuBtnLogout')
            .should('be.visible')
    })

    it('KeyCloak Login', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#menuBtnLogout')
            .should('be.visible')
    })

    it('Logout', () => {
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('#menuBtnLogout').click()
        cy.get('#skipButton').click()
        cy.contains('h3', 'You signed out of your account')
        cy.get('#menuBtnLogin')
            .should('be.visible')
            .should('have.attr', 'href', '/login')
    })

    // it('Idle Timeout', () => {
    // // Work in Progress, clock not working correctly.
    //     cy.server()
    //     cy.fixture('AllDisabledConfig').then((config) => {
    //         config.webClient.timeouts.idle = 1000
    //     }).as('config')
    //     cy.route('GET', '/v1/api/configuration/', '@config')
    //     cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
    //     const now = Date.now()
    //     cy.clock(now)
    //       .tick(1000)
    //     cy.get('[data-testid=idleModal]').contains('Are you still there?')
    //     cy.get('[data-testid=idleModalText]').contains('You will be automatically logged out in 60 seconds.')
    //     cy.tick(55000)
    //     cy.get('[data-testid=idleModalText]').contains('You will be automatically logged out in 5 seconds.')
    //     cy.get('[data-testid=idleModal]')
    //       .find('footer').find('button').should('have.text', "I'm here!")
    // })
})