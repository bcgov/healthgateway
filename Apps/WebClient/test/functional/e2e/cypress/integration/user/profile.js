const { AuthMethod } = require("../../support/constants")

describe('User Profile', () => {
    const emailAddress = "healthgateway@mailinator" + Math.random().toString().substr(2, 5) + ".com"
    before(() => {        
        cy.server()
        cy.fixture('AllDisabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Profile - Email Validation - email fields should be identical', () => {
        cy.get('#menuBtnProfile').click()
        cy.get('[data-testid=editEmailBtn]').click()
        let emailInput = cy.get('[data-testid=emailInput]')
        emailInput.clear()
        emailInput.type(emailAddress)
        let emailConfirmationInput = cy.get('[data-testid=emailConfirmationInput]')
        emailConfirmationInput.clear()
        emailConfirmationInput.type('diff' + emailAddress)
        cy.contains('.invalid-feedback', ' Emails must match ')
    })

    it('Profile - Email Validation - Can edit and save email address', () => {
        let emailConfirmationInput = cy.get('[data-testid=emailConfirmationInput]')
        emailConfirmationInput.clear()
        emailConfirmationInput.type(emailAddress)
        cy.get('[data-testid=editEmailSaveBtn]').click()
    })
})