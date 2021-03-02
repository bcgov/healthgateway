const { AuthMethod } = require("../../support/constants")

describe('Calendar View', () => {
    beforeEach(() => {
        cy.enableModules("Medication");
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Navigate to Month View', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=monthViewToggle]')
            .first()
            .click();
        cy.get('[data-testid=btnFilterApply]')
            .click();

        cy.get('#currentDate')
            .should('be.visible');
        cy.location('hash')
            .should('eq', '#calendar');
    })

    it('Validate Year Selector', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=monthViewToggle]')
            .first()
            .click();
        cy.get('[data-testid=btnFilterApply]')
            .click();

        cy.get('#currentDate').click();
        cy.get('#currentDate').click();
        cy.get('.years')
            .children()
            .should('be.visible');
        cy.get('[data-testid=yearBtn2019]').click();
        cy.get('[data-testid=monthBtnJan]').click();
        cy.get('#currentDate')
            .should('not.have.text', ' January 2019 ');
        
        cy.get('[data-testid=monthBtnFeb]').click();
        cy.get('#currentDate')
            .should('have.text', ' February 2019 ')
        cy.get('[data-testid=monthBtnFeb]')
            .should('have.class', 'selected');

        cy.get('#currentDate').click();
        cy.get('[data-testid=monthBtnApr]').click();
        cy.get('#currentDate')
            .should('have.text', ' April 2019 ');
        cy.get('[data-testid=monthBtnApr]')
            .should('have.class', 'selected');
        cy.get('[data-testid=monthBtnFeb]')
            .should('not.have.class', 'selected');
    })

    it('Click on Calendar item to Linear Timeline', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=monthViewToggle]')
            .first()
            .click();
        cy.get('[data-testid=btnFilterApply]')
            .click();

        cy.location('hash')
            .should('eq', '#calendar');
        cy.get('#currentDate').click();
        cy.get('#currentDate').click();
        cy.get('.years')
            .children()
            .should('be.visible');
        cy.get('[data-testid=yearBtn2019]').click();
        cy.get('[data-testid=monthBtnApr]').click();
        cy.get('#currentDate')
            .should('have.text', ' April 2019 ');
        cy.get('[data-testid=event-monthday-10]').click();
        cy.location('hash')
            .should('eq', '#linear');
        cy.get('[data-testid=entryCardDetailsTitle]')
            .first()
            .should('have.text', 'Methadone (Maintenance) 1mg/Ml');
        cy.contains('[data-testid=entryCardDate]', '2019-04-10')
            .should('be.visible');
            
    })
})