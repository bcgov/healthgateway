const { AuthMethod } = require("../../support/constants")

describe('Notes', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Add, Edit and Delete', () => {
        cy.get('[data-testid=addNoteBtn]').click();
        cy.get('[data-testid=noteTitleInput]').type('Note Title!');
        cy.get('[data-testid=noteDateInput]').type('2050-01-01');
        cy.get('[data-testid=noteTextInput]').type('Test');
        cy.get('[data-testid=saveNoteBtn]').click();
        cy.get('[data-testid=noteTitle]').should('have.text', 'Note Title!');
        cy.get('[data-testid=dateGroup]').should('have.text', 'Jan 01, 2050');
        cy.get('[data-testid=noteMenuBtn]').click();
        cy.get('[data-testid=editNoteMenuBtn]').click();
        cy.get('[data-testid=noteTitleInput]').type('Test Edit');
        cy.get('[data-testid=saveNoteBtn]').click();
        cy.get('[data-testid=noteTitle]').should('have.text', 'Test Edit');
        cy.get('[data-testid=noteMenuBtn]').click();
        cy.on('window:confirm', (str) => {
            expect(str).to.eq('Are you sure you want to delete this note?');
        });
        cy.get('[data-testid=deleteNoteMenuBtn]').click();
        cy.get('[data-testid=dateGroup]').should('not.have.text', 'Jan 01, 2050');
    })
})