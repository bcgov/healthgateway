const { AuthMethod } = require("../../support/constants")

describe('Reports', () => {
    const downloadsFolder = 'cypress/downloads'
    let sensitiveDocText = ' The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ';
    before(() => {
        cy.enableModules(["Encounter", "Medication", "Laboratory"]);
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/reports");

        // The next command allow downloads in Electron, Chrome, and Edge
        // without any users popups or file save dialogs.
        if (!Cypress.isBrowser('firefox')) {
          // since this call returns a promise, must tell Cypress to wait for it to be resolved
          cy.log('Page.setDownloadBehavior')
          cy.wrap(
            Cypress.automation('remote:debugger:protocol',
              {
                command: 'Page.setDownloadBehavior',
                params: { behavior: 'allow', downloadPath: downloadsFolder },
              }),
            { log: false }
          )
        }
    })

    it('Validate Date Filter and Clear', () => {       
        cy.get('[data-testid=startDateInput]')
          .should('be.enabled', 'be.visible')
          .should('have.value','')
          .type("2020-01-01")

        cy.get('[data-testid=endDateInput]')
          .should('be.enabled', 'be.visible')
          .should('have.value','')
          .type("2020-12-31")

        cy.get('[data-testid=clearBtn]')
          .should('be.enabled', 'be.visible')
          .click()

        cy.get('[data-testid=startDateInput]')
          .should('have.value','')

        cy.get('[data-testid=endDateInput]')
          .should('have.value','')
    })

    it('Validate Service Selection', () => {       
        cy.get('[data-testid=exportRecordBtn]')
            .should('not.be.enabled', 'be.visible')

        cy.get('[data-testid=infoText]')
            .should('have.text', ' Select a record type above to create a report ')

        cy.get('[data-testid=infoImage]')
            .should('be.visible')

        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MED")        

        cy.get('[data-testid=exportRecordBtn]')
            .should('be.enabled', 'be.visible')

        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("")        

        cy.get('[data-testid=exportRecordBtn]')
            .should('not.be.enabled', 'be.visible')

    })

    it('Validate Medication Report', () => {         
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MED")        

        cy.get('[data-testid=medicationReportSample]')
            .should('be.visible')
        
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

    it('Validate MSP Visits Report', () => {         
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("MSP")        

        cy.get('[data-testid=mspVisitsReportSample]')
            .should('be.visible')
        
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
            .contains(/\d{4}-\d{2}-\d{2}/);

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