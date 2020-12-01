const { AuthMethod } = require("../../support/constants")

describe('Reports', () => {
    before(() => {
        cy.readConfig().as("config").then(config => {
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/reports");
        })
    })

    it('Validate Service Selection', () => {       
        cy.get('[data-testid=exportRecordBtn]')
            .should('not.be.enabled', 'be.visible')

        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MED")        

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')
    })

    it('Validate Medication Report', () => {         
        //Validate Main Add Button  
        cy.get('[data-testid=medicationReportSample]')
            .should('be.visible')
        
        cy.get('[data-testid=medicationReportSample] #subject')
            .should('have.text', 'Health Gateway Medication History Report');
    })
}) 