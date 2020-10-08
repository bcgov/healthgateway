const { AuthMethod } = require("../../support/constants")

describe('Comments', () => {
    before(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').then(config => {
            config.webClient.modules.Comment = true;
            config.webClient.modules.Medication = true;
            cy.route('GET', '/v1/api/configuration/', config);            
        });
        cy.login(Cypress.env('keycloak.username'), 
                Cypress.env('keycloak.password'), 
                AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    });

    it('Validate Add', () => {
        cy.get('[data-testid=addCommentTextArea]')
            .first()
            .type('test comment goes here!');
        cy.get('[data-testid=postCommentBtn]')
            .first()
            .click();
        cy.get('[data-testid=commentText]')
            .contains('test comment goes here!');
    });

    it('Validate Edit', () => {
        cy.get('[data-testid=commentMenuBtn]')
            .first()
            .click();
        cy.get('[data-testid=commentMenuEditBtn]')
            .first({ force: true })
            .click();
        cy.get('[data-testid=editCommentInput]')
            .type('edited comment');
        cy.get('[data-testid=saveCommentBtn]')
            .click();
        cy.get('[data-testid=commentText]')
            .contains('edited comment');
    });

    it('Validate Delete', () => {
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
            .first()
            .should('not.have.text', 'edited comment');
    })

})