const { AuthMethod } = require("../../support/constants")

describe('Filters', () => {
    before(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.closeCovidModal();
    })

    it('Validate Filter Counts',() => {
        const countRegex = /^.*?\((\d+)K?\).*$/
        cy.get('[data-testid=filterDropdown]').click()
        cy.get('[data-testid=immunizationCount]')
            .should('be.visible')
            .contains(countRegex)
        cy.get('[data-testid=medicationCount]')
            .should('be.visible')
            .contains(countRegex)
        cy.get('[data-testid=laboratoryCount]')
            .should('be.visible')
            .contains(countRegex)
        cy.get('[data-testid=encounterCount]')
            .should('be.visible')
            .contains(countRegex)
        cy.get('[data-testid=noteCount]')
            .should('be.visible')
            .contains(countRegex)
    })

    it('No Records on Linear Timeline', () => {
        cy.get('[data-testid=filterTextInput]')
            .type('xxxx');
        cy.get('[data-testid=noTimelineEntriesText]')
            .should('be.visible');
    })

    it('No Records on Calendar Timeline', () => {
        cy.get('[data-testid=monthViewToggle]')
            .first()
            .click();
        cy.get('[data-testid=noTimelineEntriesText]')
            .should('be.visible');
        cy.get('[data-testid=filterTextInput]')
            .clear();
        cy.get('[data-testid=listViewToggle]')
            .last()
            .click()
    })

    it('Filter Checkboxes are Visible', () => {
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
    });


    it('Filter Immunization', () => {
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
    });

    it('Filter Medication', () => {
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
    });

    it('Filter Encounter', () => {
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
    });

    it('Filter Laboratory', () => {
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
        cy.get('[data-testid="filterDropdown"]')
            .contains('Clear')
            .click();
    });
})