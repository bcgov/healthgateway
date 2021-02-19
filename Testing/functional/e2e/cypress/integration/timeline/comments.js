const { AuthMethod } = require("../../support/constants")
const BASEURL = "/v1/api/UserProfile/"
const HDID='P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A'
const COMMENT = 'test comment goes here!';

function addComment(comment) {
    cy.log('Adding comment...')
    cy.get('[data-testid=entryCardDetailsButton]')
        .first()
        .click();
    
    cy.get('[data-testid=addCommentTextArea]')
        .first()
        .type(comment);
    cy.get('[data-testid=postCommentBtn]')
        .first()
        .click();
}

function verifyAdd(entryTypeCode) {
    addComment(COMMENT);

    cy.get('[data-testid=commentText]')
        .first()
        .contains(COMMENT);    
    cy.get("@tokens").then(tokens => {
        cy.request({
            url: `${BASEURL}${HDID}/Comment/`,
            followRedirect: false,
            auth: {
                bearer: tokens.access_token
            },
            headers: {
                accept: 'application/json'
            }
        })
        .should((response) => { 
            expect(response.status).to.eq(200)
            expect(response.body).to.not.be.null
            cy.log(`response.body: ${JSON.stringify(response.body)}`)
            expect(JSON.stringify(response.body.resourcePayload)).contains(`"entryTypeCode":"${entryTypeCode}"`);
            
            cy.get('[data-testid=entryCardDetailsButton]')
                .first()
                .click();
        })
    });
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

function deleteComment() {
    cy.log('Deleting a comment...')
    cy.get('[data-testid=commentMenuBtn]')
        .first()
        .click();
    cy.on('window:confirm', (str) => {
        expect(str).to.eq('Are you sure you want to delete this comment?');
    });
    cy.get('[data-testid=commentMenuDeleteBtn]')
        .first()
        .click();
}

function verifyDelete() {
    deleteComment();
    cy.get('[data-testid=commentText]')
        .should('not.exist');
}

function validateComment(moduleName) {
    cy.enableModules([moduleName, "Comment"]);
    cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
    cy.checkTimelineHasLoaded();
    cy.get('[data-testid=commentIcon]')
        .should('not.exist');
    cy.get('[data-testid=commentCount]')
        .should('not.exist');
    verifyAdd('Lab');
    cy.get('[data-testid=commentIcon]')
        .should('exist');
    cy.get('[data-testid=commentCount]')
        .should('not.exist');
    
    // Add 2nd comments to test the comment icon & comment count.
    addComment(COMMENT);
    cy.get('[data-testid=commentIcon]')
        .should('exist');
    cy.get('[data-testid=commentCount]')
        .should('exist');
    
    verifyEdit();
    deleteComment();
    verifyDelete();
}

describe('Comments', () => {
    beforeEach(() => {
        cy.getTokens(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'))
            .as("tokens")
    })

    it('Validate Add/Edit/Delete for LAB', () => {
        validateComment("Laboratory");
    });

    it('Validate Add/Edit/Delete for MED', () => {
        validateComment("Medication");
    });

    it('Validate Add/Edit/Delete for ENC', () => {
        validateComment("Encounter");
    });
})