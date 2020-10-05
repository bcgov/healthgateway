const { AuthMethod } = require("../../support/constants")

describe('Filters', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.closeCovidModal();
    })

    it('Validate No Records', () => {
        cy.get('[data-testid=filterTextInput]')
            .type('xxxx');
        cy.get('[data-testid=noTimelineEntriesText]')
            .should('be.visible');
    })

    it('Validate Filter Checkboxes are Visible', () => {
        cy.get('[data-testid="filterDropdown"]')
            .click();
        cy.get('[data-testid=note-filter]')
            .should('be.visible');
        cy.get('[data-testid=medication-filter]')
            .should('be.visible');
        cy.get('[data-testid=immunization-filter]')
            .should('be.visible');
        cy.get('[data-testid=laboratory-filter]')
            .should('be.visible');
        cy.get('[data-testid=encounter-filter]')
            .should('be.visible');
        
        // Filter Imms
        cy.get('[data-testid=immunization-filter]')
            .click({ force: true });
        cy.get('[data-testid=immunizationTitle]')
            .should('be.visible');
        cy.get('[data-testid=noteTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=encounterTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=laboratoryTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=medicationTitle]')
            .should('not.be.visible');

        cy.get('[data-testid="filterDropdown"]')
            .contains('Clear')
            .click();

        // Filter Meds
        cy.get('[data-testid=medication-filter]')
            .click({ force: true });
        cy.get('[data-testid=immunizationTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=noteTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=encounterTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=laboratoryTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=medicationTitle]')
            .should('be.visible');

        cy.get('[data-testid="filterDropdown"]')
            .contains('Clear')
            .click();

        // Filter Encounter
        cy.get('[data-testid=encounter-filter]')
            .click({ force: true });
        cy.get('[data-testid=encounterTitle]')
            .should('be.visible');
        cy.get('[data-testid=noteTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=immunizationTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=laboratoryTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=medicationTitle]')
            .should('not.be.visible');

        cy.get('[data-testid="filterDropdown"]')
            .contains('Clear')
            .click();

        // Filter Labs
        cy.get('[data-testid=laboratory-filter]')
            .click({ force: true });
        cy.get('[data-testid=encounterTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=noteTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=immunizationTitle]')
            .should('not.be.visible');
        cy.get('[data-testid=laboratoryTitle]')
            .should('be.visible');
        cy.get('[data-testid=medicationTitle]')
            .should('not.be.visible');
    })

})