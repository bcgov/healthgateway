const { AuthMethod, localDevUri } = require("../../support/constants")
describe('Immunization', () => {
    before(() => {
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
      cy.login(Cypress.env('keycloak.username'),
          Cypress.env('keycloak.password'),
          AuthMethod.KeyCloak);
      cy.checkTimelineHasLoaded();
    })

    it('Validate Immunization Loading', () => {
      cy.get('[data-testid=immunizationLoading]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLoading]')
        .should('not.exist')
      cy.get('[data-testid=immunizationReady]')
        .should('be.visible')
        .find('[data-testid=immunizationBtnReady]')
        .should('be.visible')
        .click()
    })

    it('Validate Card Details', () => {
      cy.get('[data-testid=entryCardDetailsButton')
        .first()
        .click()
      cy.get('[data-testid=immunizationTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProductTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationProviderTitle]')
        .should('be.visible')
      cy.get('[data-testid=immunizationLotTitle]')
        .should('be.visible')
      
      // Verify Forecast
      cy.get('[data-testid=forecastDisplayName]').first().should('not.be.visible');
      cy.get('[data-testid=forecastDueDate]').first().should('not.be.visible');
      cy.get('[data-testid=forecastStatus]').first().should('not.be.visible');
      cy.get('[data-testid=forecastFollowDirections]').first().should('not.be.visible');

      cy.get('[data-testid=detailsBtn]').first().click();

      cy.get('[data-testid=forecastDisplayName]').first().should('be.visible');
      cy.get('[data-testid=forecastDisplayName]').first().contains('Covid-191');
      cy.get('[data-testid=forecastDueDate]').first().should('be.visible');
      cy.get('[data-testid=forecastDueDate]').first().contains('2021-01-31');
      cy.get('[data-testid=forecastStatus]').first().should('be.visible');
      cy.get('[data-testid=forecastStatus]').first().contains('Eligible');
      cy.get('[data-testid=forecastFollowDirections]').first().contains(' Please follow directions from your COVID vaccine provider for information on COVID-19 2nd dose. For information on recommended immunizations, please visit ');
      cy.get('[data-testid=forecastFollowDirections]').first().contains('https://immunizebc.ca/');
      cy.get('[data-testid=forecastFollowDirections]').first().contains('or contact your local Public Health Unit.');
    })
})