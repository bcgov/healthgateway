const { AuthMethod } = require("../../support/constants")

describe('Reports', () => {
    let sensitiveDocText = ' The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ';
    before(() => {
        cy.setupDownloads();
        cy.enableModules(["Encounter", "Medication", "Laboratory", "Immunization"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/reports");
    })

    it('Validate Advanced - Dates, Cancel and Apply', () => {    
        cy.get('[data-testid=advancedPanel]')
            .should('not.be.visible');
    
        cy.get('[data-testid=advancedBtn]')
            .should('be.enabled', 'be.visible')
            .click();

        cy.get('[data-testid=startDateInput] input')
        .should('be.enabled', 'be.visible')
        .should('have.value','')
        .click()
        .focus()
        .type("2021-FEB-03")
    
        cy.get('[data-testid=endDateInput] input')
        .should('be.enabled', 'be.visible')
        .should('have.value','')
        .click()
        .focus()
        .type("2021-FEB-05")
        .focus()

        cy.get('[data-testid=applyFilterBtn]')
            .click()
        cy.get('[data-testid=selectedDatesFilter]')
            .contains('From 2021-Feb-03 To 2021-Feb-05')

        // Validate filters - Cancel  button
        cy.get('[data-testid=advancedBtn]')
            .click();
        
        // Cancel button should not set the newly entered values
        cy.get('[data-testid=startDateInput] input')
            .type("2020-FEB-03")
        cy.get('[data-testid=endDateInput] input')
            .type("2020-FEB-05")
        cy.get('[data-testid=clearBtn]')
            .should('be.enabled', 'be.visible')
            .click();
        cy.get('[data-testid=startDateInput] input')
            .should('have.value','2021-FEB-03')
        cy.get('[data-testid=endDateInput] input')
            .should('have.value','2021-FEB-05')

        cy.get('[data-testid=clearBtn]')
            .should('not.be.visible')
        cy.get('[data-testid=applyFilterBtn]')
            .should('not.be.visible')
        cy.get('[data-testid=startDateInput] input')
            .should('not.be.visible')        
        cy.get('[data-testid=endDateInput] input')
            .should('not.be.visible')
        
        cy.get('[data-testid=clearFilter] button')
            .should('be.visible')
            .click();
        
        cy.get('[data-testid=clearFilter] button')
            .should('not.exist');
    })

    it('Validate Service Selection', () => {
        cy.get('[data-testid=exportRecordBtn]')
            .should('be.disabled', 'be.visible')

        cy.get('[data-testid=infoText]')
            .should('have.text', ' Select a record type above to create a report ')

        // display visual when no record type selected (mobile and desktop) 
        cy.get('[data-testid=infoImage]')
            .should('be.visible')
        cy.viewport('iphone-6');
        cy.get('[data-testid=infoImage]')
            .should('be.visible')
        cy.viewport(1000, 600);

        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MED")

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')

        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("")        

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.disabled', 'be.visible')
    })
    
    it('Validate Medication Report', () => {
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MED");
        cy.get('[data-testid=medicationReportSample]')
            .should('be.visible');

        cy.viewport('iphone-6');
        cy.get('[data-testid=medicationReportSample]')
            .should('not.be.visible');
        cy.viewport(1000, 600);

        cy.get('[data-testid=exportRecordBtn]')
            .click();

        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');

        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocText);

        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();
            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');
    })

    it('Validate MSP Visits Report', () => {         
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MSP")        

        cy.get('[data-testid=mspVisitsReportSample]')
            .should('be.visible')
        
        cy.viewport('iphone-6');
        cy.get('[data-testid=mspVisitsReportSample]')
            .should('not.be.visible');
        cy.viewport(1000, 600);

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')
            .click();

        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');

        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocText);

        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();
            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');
    })

    it('Validate COVID-19 Report', () => {         
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("COVID-19")        

        cy.get('[data-testid=covid19ReportSample]')
            .should('be.visible')
        cy.get('[data-testid=covid19ItemDate]')
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        
        cy.viewport('iphone-6');
        cy.get('[data-testid=covid19ReportSample]')
            .should('not.be.visible');
        cy.viewport(1000, 600);

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')
            .click();

        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');

        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocText);

        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();
            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');
    })

    it('Validate Immunization Report', () => {         
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("Immunization")        

        cy.get('[data-testid=immunizationHistoryReportSample]')
            .should('be.visible')
        cy.get('[data-testid=immunizationItemDate]')
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        
        cy.viewport('iphone-6');
        cy.get('[data-testid=immunizationHistoryReportSample]')
            .should('not.be.visible');
        cy.viewport(1000, 600);

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')
            .click();

        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');

        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocText);

        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();
            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');
    })
}) 