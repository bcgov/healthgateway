const { AuthMethod } = require("../../support/constants")

describe('Comments', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Add, Edit and Delete', () => {
        cy.get('[data-testid=addCommentTextArea]')
            .type('test comment goes here!');
        cy.get('[data-testid=postCommentBtn]').click();
        cy.get('[data-testid=commentText]')
            .should('have.text', 'test comment goes here!');
        cy.get('[data-testid=commentMenuBtn]').click();
        cy.get('[data-testid=commentMenuEditBtn]').click();
        cy.get('[data-testid=editCommentInput]')
            .type('edited comment');
        cy.get('[data-testid=saveCommentBtn]').click();
        cy.get('[data-testid=commentText]')
            .should('have.text', 'edited comment');
        cy.get('[data-testid=commentMenuBtn]').click();
        cy.on('window:confirm', (str) => {
            expect(str).to.eq('Are you sure you want to delete this comment?');
        });
        cy.get('[data-testid=commentMenuDeleteBtn]').click();
        cy.get('[data-testid=commentText]')
            .should('not.have.text', 'edited comment');
    })

})