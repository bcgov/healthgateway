const { AuthMethod } = require("../../support/constants")
describe('Immunization History Report', () => {
    let sensitiveDocText = ' The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ';
    
    before(() => {
        cy.setupDownloads();
        let isLoading = false;  
        cy.enableModules("Immunization");
        cy.intercept('GET', "**/v1/api/Immunization/*", (req) => { 
          req.reply(res => {     
            if (!isLoading) {
              res.send({ fixture: "ImmunizationService/immunizationrefresh.json" })
            } else {
              res.send({ fixture: "ImmunizationService/immunization.json" })
            }
            isLoading = !isLoading;
          })
        });
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/reports");
    })

    it('Validate Immunization Loading', () => {      
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("Immunization")    
        cy.get('[data-testid=timelineLoading]')
            .should('be.visible')
        cy.get('[data-testid=timelineLoading]')
            .should('not.exist')
    })

    it('Validate Immunization History Report', () => { 
        cy.get('[data-testid=reportType]')
            .should('be.enabled', 'be.visible')
            .select("Immunization")

        cy.get('[data-testid=immunizationHistoryReportSample]')
            .should('be.visible')
        
        cy.get('[data-testid=immunizationDateTitle]')
        .should('be.visible')
        cy.get('[data-testid=immunizationProviderTitle]')
        .should('be.visible')
        cy.get('[data-testid=immunizationItemTitle]')
        .should('be.visible')
        cy.get('[data-testid=immunizationAgentTitle]')
        .should('be.visible')
        cy.get('[data-testid=immunizationStatusTitle]')
        .should('be.visible')

        cy.get('[data-testid=immunizationItemDate]')
            .last()
            .contains(/\d{4}-\d{2}-\d{2}/);
        cy.get('[data-testid=immunizationItemName]')
            .should('be.visible')
        cy.get('[data-testid=immunizationItemProviderClinic]')
            .should('be.visible')
        cy.get('[data-testid=immunizationItemAgent]')
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
}) 