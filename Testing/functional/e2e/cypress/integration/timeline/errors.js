const { AuthMethod } = require("../../support/constants")

describe('Banner Error', () => {
    before(() => {
        cy.intercept("GET", "/v1/api/Note/*", (req) => {
            req.reply((res) => {
                res.body.resultStatus = 0;
                res.body['resultError'] = {
                    resultMessage: "Error ABC",
                    errorCode: "Error Code",
                    traceId: "123456789"
                };
            })
        })
        cy.enableModules("Note");

        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })
    
    it('Verify banner error', () => {
        cy.get('[data-testid=errorTextDescription]')
            .contains('Error Code');
            it('Verify banner error', () => {
                cy.get('[data-testid=errorTextDetails]')
                    .contains('Error ABC');
        cy.get('[data-testid=errorSupportDetails]')
            .contains('If this issue persists, contact HealthGateway@gov.bc.ca and provide 123456789');
    })
})