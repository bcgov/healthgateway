const { AuthMethod } = require("../../support/constants")

describe('User Profile', () => {
    const emailAddress = "healthgateway@mailinator" + Math.random().toString().substr(2, 5) + ".com"
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

    it('Validate email fields', () => {
        cy.get('[data-testid=menuBtnProfileLink]').click()
        cy.get('[data-testid=editEmailBtn]').click()
        let emailInput = cy.get('[data-testid=emailInput]')
        emailInput.clear()
        emailInput.type(emailAddress)
        let emailConfirmationInput = cy.get('[data-testid=emailConfirmationInput]')
        emailConfirmationInput.clear()
        emailConfirmationInput.type('diff' + emailAddress)
        cy.contains('.invalid-feedback', ' Emails must match ')
    })

    it('Edit and Save email address', () => {
        let emailConfirmationInput = cy.get('[data-testid=emailConfirmationInput]')
        emailConfirmationInput.clear()
        emailConfirmationInput.type(emailAddress)
        cy.get('[data-testid=editEmailSaveBtn]').click()
    })
})