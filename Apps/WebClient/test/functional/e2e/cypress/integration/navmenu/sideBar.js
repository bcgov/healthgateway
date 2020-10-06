const { AuthMethod } = require("../../support/constants")

describe('Validate Side bar menu', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
    })

    it('Side bar contains visible nav links (Profile, Timeline, Add Note, Print, Insights, Report, Sidebar toggle, Feedback', () => {
        cy.get('[data-testid=menuBtnProfileLink]').should('have.attr', 'href', '/profile')
        cy.get('[data-testid=menuBtnTimelineLink]').should('have.attr', 'href', '/timeline')
        cy.get('[data-testid=addNoteBtn]').should('be.visible')
        cy.get('[data-testid=printViewBtn]').should('be.visible')
        cy.get('[data-testid=menuBtnHealthInsightsLink]').should('have.attr', 'href', '/healthInsights')
        cy.get('[data-testid=menuBtnReportsLink]').should('have.attr', 'href', '/reports')
        cy.get('[data-testid=sidebarToggle]').should('be.visible')
        cy.get('[data-testid=feedbackContainer]').should('be.visible')
    })

})