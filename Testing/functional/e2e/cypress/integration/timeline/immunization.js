const { AuthMethod, localDevUri } = require("../../support/constants")
describe('Immunization', () => {
    before(() => {
      let isLoading = false;  
      cy.enableModules("Immunization");
      cy.intercept('GET', "v1/api/Immunization/*", (req) => { 
        req.reply(res => {     
          if (!isLoading) {
            res.send({ fixture: "ImmunizationService/immunizationrefresh.json" })
          } else {
            res.send({ fixture: "ImmunizationService/immunization.json" })
          }
          isLoading = !isLoading;
        })
      });
      cy.login(Cypress.env('keycloak.username'),
          Cypress.env('keycloak.password'),
          AuthMethod.KeyCloak);
      cy.checkTimelineHasLoaded();
    })

    it('Validate Immunization Loading', () => {
      cy.get('[data-testid=immunizationLoading]')
        .should('be.visible')
      cy.intercept('GET', "v1/api/Immunization/*", );
      cy.get('[data-testid=immunizationLoading]')
        .should('not.exist')
      cy.get('[data-testid=immunizationReady]')
        .should('be.visible')
        .find('[data-testid=immunizationBtnReady]')
        .should('be.visible')
        .click()
    })

    it('Validate Card Details', () => {
      cy.get('[data-testid=immunizationTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProductTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProviderTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLotTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProductName]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProviderName]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLotNumber]')
        .should('be.visible')
    })
})