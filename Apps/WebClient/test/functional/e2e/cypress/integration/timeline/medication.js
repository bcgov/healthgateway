const { AuthMethod } = require("../../support/constants")

describe('Medication', () => {
    beforeEach(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').then(config => {
            config.webClient.modules.Medication = true;
            cy.route('GET', '/v1/api/configuration/', config);            
        });
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Protective Word', () => {

    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=medicationTitle]')
            .should('be.visible');
        cy.get('[data-testid=medicationPractitioner]')
            .should('not.be.visible');
        cy.get('[data-testid=medicationViewDetailsBtn]')
            .first()
            .click();
        cy.get('[data-testid=medicationPractitioner]')
            .should('be.visible');
    })
})