const { AuthMethod } = require("../../support/constants")

describe('User Profile', () => {
    const emailAddress = "healthgateway@mailinator" + Math.random().toString().substr(2, 5) + ".com"
    before(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = true
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'),
                Cypress.env('keycloak.password'),
                AuthMethod.KeyCloak);
            cy.checkTimelineHasLoaded();
        })
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

    it('Verify Phone Countdown timer', () => {
        cy.get('[data-testid=menuBtnProfileLink]').click()
        cy.get('[data-testid=editSMSBtn]').click()
        let cellPhoneInput = cy.get('[data-testid=smsNumberInput]')
        cellPhoneInput.clear()
        const minm = 10000;
        const phonePostfix = Math.floor(Math 
            .random() * (99999 - minm + 1)) + minm; // auto-generate 5 digits number
        cellPhoneInput.type("60465" + phonePostfix)
        cy.get('[data-testid=saveSMSEditBtn]').click()
        cy.get('[data-testid="countdownText"]')
            .contains(/\d{1,2}s$/) // has 1 or 2 digits before the last 's' character
    })
})