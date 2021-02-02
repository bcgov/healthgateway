const { AuthMethod } = require("../../support/constants")
const BASEURL = "/v1/api/UserProfile/"
const HDID='P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A'

function verifyAdd(entryTypeCode) {
    cy.log('Validate ADD comment...')
    cy.get('[data-testid=entryCardDetailsButton]')
        .first()
        .click();
    
    cy.get('[data-testid=addCommentTextArea]')
        .first()
        .type('test comment goes here!');
    cy.get('[data-testid=postCommentBtn]')
        .first()
        .click();
    cy.get('[data-testid=commentText]')
        .contains('test comment goes here!');
    
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
    beforeEach(() => {
        cy.getTokens(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'))
            .as("tokens")
    })

    it('Validate Add/Edit/Delete for MED', () => {
        cy.enableModules(["Medication", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd('Med');
        verifyEdit();
        verifyDelete();
    });
    it('Validate Add/Edit/Delete for LAB', () => {
        cy.enableModules(["Laboratory", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd('Lab');
        verifyEdit();
        verifyDelete();
    });
    it('Validate Add/Edit/Delete for ENC', () => {
        cy.enableModules(["Encounter", "Comment"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
        verifyAdd('Enc');
        verifyEdit();
        verifyDelete();
    });
})