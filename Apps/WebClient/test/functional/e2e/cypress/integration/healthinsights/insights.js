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
        cy.get('#menuBtnHealthInsights').click()
    })

    it('Total records count (for medication) should not have 0 records.', () => {
        cy.get('[data-testid=totalRecordsText]').should('not.contain', '0 ')
    })
})