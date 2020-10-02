const { AuthMethod } = require("../../support/constants")

describe('User Profile', () => {
    const emailAddress = "healthgateway@mailinator" + Math.random().toString().substr(2, 5) + ".com"
    before(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
        cy.get('#covid-modal___BV_modal_header_ > .close').click()
        cy.get('#menuBtnProfile').click()
    })

    it('Profile - Email Validation - email fields should be identical', () => {
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