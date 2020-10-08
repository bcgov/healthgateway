const { AuthMethod } = require("../../support/constants")

describe('Health Insights', () => {
    before(() => {
        cy.server()
        cy.fixture('AllDisabledConfig').then(config => {
            config.webClient.modules.Medication = true;
            cy.route('GET', '/v1/api/configuration/', config);            
        });
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.checkTimelineHasLoaded();
    })

    it('Validate medication records count.', () => {
        cy.get('[data-testid=menuBtnHealthInsightsLink]').click()
        cy.get('[data-testid=totalRecordsText]').should('not.contain', '0 ')
    })
})