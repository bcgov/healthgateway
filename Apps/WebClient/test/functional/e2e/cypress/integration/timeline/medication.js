const { AuthMethod } = require("../../support/constants")

describe('Medication', () => {
    beforeEach(() => {
        cy.login(Cypress.env('keycloak.username'), 
                 Cypress.env('keycloak.password'), 
                 AuthMethod.KeyCloak)
    })

    it('Validate Protective Word', () => {

    })

    it('Validate Card Details', () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get('[data-testid=medication-filter]').click();
        cy.get('[data-testid=dateGroup]').should('have.text', 'Dec 30, 2019');
        cy.get('[data-testid=medicationTitle]').should('have.text', 'Methadone (Maintenance) 1mg/Ml');
        cy.get('[data-testid=medicationViewDetailsBtn]').click();
        cy.get('[data-testid=medicationPractitioner]').should('have.text', 'Practitioner: PZVVPS');
        
    })
})