const { AuthMethod } = require("../../support/constants")

describe('Authentication', () => {
    beforeEach(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').as('config');
        cy.route('GET', '/v1/api/configuration/', '@config');
    })


    it('BCSC Login', () => {
        cy.login(Cypress.env('bcsc.username'), Cypress.env('bcsc.password'), AuthMethod.BCSC)
        cy.get('[data-testid=logoutBtn]')
            .should('be.visible')
            .should('not.be.disabled');
    })

    it('KeyCloak Login', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('[data-testid=logoutBtn]')
            .should('be.visible')
            .should('not.be.disabled');
    })

    it('Logout', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('[data-testid=logoutBtn]')
            .click();
        cy.get('[data-testid=ratingModalSkipBtn]')
            .click();
        cy.contains('h3', 'You signed out of your account');
        cy.get('[data-testid=loginBtn]')
            .should('be.visible')
            .should('not.be.disabled')
            .should('have.attr', 'href', '/login');
    })

    it('IDIR Blocked', () => {
        cy.visit('/login');
        cy.log(`Authenticating as IDIR user ${Cypress.env('idir.username')}`);
        cy.get('[data-testid=IDIRBtn]')
            .should('be.visible')
            .should('not.be.disabled')
            .click();
        cy.get('#user')
            .type(Cypress.env('idir.username'));
        cy.get('#password')
            .type(Cypress.env('idir.password'));
        cy.get('input[name="btnSubmit"')
            .click();
        cy.contains('h1', '403');
        cy.contains('h2', 'IDIR Login');
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