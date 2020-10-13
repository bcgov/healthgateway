const { AuthMethod } = require("../../support/constants")

describe('Health Insights', () => {
    before(() => {
        cy.request(`${Cypress.config("baseUrl")}/v1/api/configuration`)
        .should((response) => { expect(response.status).to.eq(200) })
        .its("body").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/healthInsights");
        })
    })

    it('Validate medication records count.', () => {
        cy.get('[data-testid=totalRecordsText]').should('not.contain', '0 ')
    })
})