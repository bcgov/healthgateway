const { AuthMethod } = require("../../support/constants")

describe('Menu System', () => {
    beforeEach(() => {
        cy.readConfig().as("config").then(config => {
                config.webClient.modules.CovidLabResults = false
                config.webClient.modules.Comment = false
                config.webClient.modules.Encounter = false
                config.webClient.modules.Immunization = false
                config.webClient.modules.Laboratory = false
                config.webClient.modules.Medication = false
                config.webClient.modules.MedicationHistory = false
                config.webClient.modules.Note = true
                cy.server();
                cy.route('GET', '/v1/api/configuration/', config);
                cy.login(Cypress.env('keycloak.username'),
                    Cypress.env('keycloak.password'),
                    AuthMethod.KeyCloak);
                cy.checkTimelineHasLoaded();
            })
    })

    it('Validate Toggle Sidebar', () => {
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
        cy.get('[data-testid=menuBtnProfileLink]').should('have.attr', 'href', '/profile')
        cy.get('[data-testid=menuBtnTimelineLink]').should('have.attr', 'href', '/timeline')
        cy.get('[data-testid=addNoteBtn]').should('be.visible')
        cy.get('[data-testid=printViewBtn]').should('be.visible')
        cy.get('[data-testid=menuBtnHealthInsightsLink]').should('have.attr', 'href', '/healthInsights')
        cy.get('[data-testid=menuBtnReportsLink]').should('have.attr', 'href', '/reports')
        cy.get('[data-testid=sidebarToggle]').should('be.visible')
        cy.get('[data-testid=feedbackContainer]').should('be.visible')
    })

    it('Side bar expands on login for desktop', () => {
        cy.get('[data=testid=timelineLabel]').should('be.visible');
        cy.viewport('iphone-6')  // Set viewport to 375px x 667px
        cy.get('[data-testid="timelineLabel]').should('not.be.visible');
    })
})