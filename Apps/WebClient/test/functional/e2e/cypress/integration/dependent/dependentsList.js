const { AuthMethod } = require("../../support/constants")

describe('view dependent', () => {
    before(() => {
        cy.readConfig().as("config").then(config => {
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
            cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/dependents");
        })
    })

    it('Validate dependents details.', () => {
        cy.get('[data-testid=dependentPHN]')
            .first().invoke('val')
            .then(phnNumber => expect(phnNumber).to.equal('987438****'));
        cy.get('[data-testid=dependentDOB]')
            .first().invoke('val')
            .then(dateOfBirth => expect(dateOfBirth).to.equal('2008-12-01'));
        cy.get('[data-testid=dependentGender]')
            .first().invoke('val')
            .then(dateOfBirth => expect(dateOfBirth).to.equal('Male'));
    })

    it('Validate covid tab.', () => {
        cy.get('[data-testid=covid19TabTitle]').parent().click();
        cy.get('[data-testid=covid19NoRecords]').first().should('have.text', 'No records found.');
    })
})