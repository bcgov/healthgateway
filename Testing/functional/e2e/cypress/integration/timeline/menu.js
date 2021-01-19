const { AuthMethod } = require("../../support/constants")

function login(isMobile) {
    cy.enableModules("Note");
    if (isMobile) {
        cy.viewport('iphone-6')  // Set viewport to 375px x 667px
    }
    cy.login(Cypress.env('keycloak.username'),
        Cypress.env('keycloak.password'),
        AuthMethod.KeyCloak);
    cy.checkTimelineHasLoaded();
}

describe('Menu System', () => {
    beforeEach(() => {
    })

    it('Validate Toggle Sidebar', () => {
        login(false);
        cy.get('[data-testid=sidebarUserName]')
            .should('be.visible')
            .should('have.text', 'Dr Gateway');
        cy.get('[data-testid=sidebarToggle]').click();
        cy.get('[data-testid=sidebarUserName]')
            .should('not.be.visible');
        cy.get('[data-testid=sidebarToggle]').click();
        cy.get('[data-testid=sidebarUserName]')
            .should('be.visible')
            .should('have.text', 'Dr Gateway');
    })

    it('Side bar contains nav links', () => {
        login(false);
        cy.get('[data-testid=menuBtnProfileLink]').should('have.attr', 'href', '/profile')
        cy.get('[data-testid=menuBtnTimelineLink]').should('have.attr', 'href', '/timeline')
        cy.get('[data-testid=addNoteBtn]').should('be.visible')
        cy.get('[data-testid=menuBtnHealthInsightsLink]').should('have.attr', 'href', '/healthInsights')
        cy.get('[data-testid=menuBtnReportsLink]').should('have.attr', 'href', '/reports')
        cy.get('[data-testid=menuBtnDependentsLink]').should('have.attr', 'href', '/dependents')
        cy.get('[data-testid=sidebarToggle]').should('be.visible')
        cy.get('[data-testid=feedbackContainer]').should('be.visible')
    })

    it('Side bar expands on login for desktop', () => {
        login(true);
        cy.get('[data-testid=timelineLabel]').should('not.be.visible');
    })
})