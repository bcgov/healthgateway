const { AuthMethod } = require("../../support/constants")

describe('MSP Visits', () => {
    beforeEach(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').then(config => {
            config.webClient.modules.Encounter = true;
            cy.route('GET', '/v1/api/configuration/', config);            
        });
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=encounterTitle]')
            .should('be.visible');
        cy.get('[data-testid=encounterDescription]')
            .should('be.visible');
        cy.get('[data-testid=detailsButton').click()
        cy.get('[data-testid=clinicLabel')
            .should('be.visible');
        cy.get('[data-testid=clinicName')
            .should('be.visible');
        cy.get('[data-testid=clinicAddress')
            .should('be.visible');
        cy.get('[data-testid=clinicPhone')
            .should('be.visible');
    })
})