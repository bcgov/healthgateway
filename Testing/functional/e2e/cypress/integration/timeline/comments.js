const { AuthMethod } = require("../../support/constants")

function verifyAdd() {
    cy.log('Validate ADD comment...')
    cy.get('[data-testid=addCommentTextArea]')
        .first()
        .type('test comment goes here!');
    cy.get('[data-testid=postCommentBtn]')
        .first()
        .click();
    cy.get('[data-testid=commentText]')
        .contains('test comment goes here!');
}
function verifyEdit() {
    cy.log('Validate EDIT comment...')
    cy.get('[data-testid=commentMenuBtn]')
        .first()
        .click();
    cy.get('[data-testid=commentMenuEditBtn]')
        .first()
        .click();
    cy.get('[data-testid=editCommentInput]')
        .type('edited comment');
    cy.get('[data-testid=saveCommentBtn]')
        .click();
    cy.get('[data-testid=commentText]')
        .contains('edited comment');
}
function verifyDelete() {
    cy.log('Validate DELETE comment...')
    cy.get('[data-testid=commentMenuBtn]')
        .first()
        .click();
    cy.on('window:confirm', (str) => {
        expect(str).to.eq('Are you sure you want to delete this comment?');
    });
    cy.get('[data-testid=commentMenuDeleteBtn]')
        .first()
        .click();
    cy.get('[data-testid=commentText]')
        .should('not.exist');
}

describe('Comments', () => {
    it('Validate Add/Edit/Delete for MED', () => {
        cy.enableModules(["Medication", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd();
        verifyEdit();
        verifyDelete();
    });
    it('Validate Add/Edit/Delete for LAB', () => {
        cy.enableModules(["Laboratory", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd();
        verifyEdit();
        verifyDelete();
    });
    it('Validate Add/Edit/Delete for ENC', () => {
        cy.enableModules(["Encounter", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd();
        verifyEdit();
        verifyDelete();
    });
})