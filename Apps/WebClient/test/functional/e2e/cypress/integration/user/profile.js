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
        cy.get('#editEmail').click()
        cy.get('#email').clear()
        cy.get('#emailConfirmation').clear()
        cy.get('#email').type(emailAddress)
        cy.get('#emailConfirmation').type('diff' + emailAddress)
        cy.contains('.invalid-feedback', ' Emails must match ')
    })

    it('Profile - Email Validation - Can edit and save email address', () => {
        cy.get('#emailConfirmation').clear()
        cy.get('#emailConfirmation').type(emailAddress)
        cy.get('#saveBtn').click()
    })
})