const { AuthMethod } = require("../../../support/constants")
describe('Immunization', () => {
  beforeEach(() => {
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
      cy.viewport('iphone-6');
      cy.login(Cypress.env('keycloak.username'),
          Cypress.env('keycloak.password'),
          AuthMethod.KeyCloak);
      cy.checkTimelineHasLoaded();
    })

    it('Validate Immunization Loading', () => {
      cy.get('[data-testid=immunizationLoading]')
        .should('be.visible')
        .contains('Still searching for immunization records')
      cy.get('[data-testid=immunizationLoading]')
        .should('not.exist')
      cy.get('[data-testid=immunizationReady]')
        .should('be.visible')
        .find('[data-testid=immunizationBtnReady]')
        .should('be.visible')
        .click()
    })
    
    it('Validate Card Details on Mobile', () => {
      cy.get('[data-testid=immunizationBtnReady]')
      .first()
      .click()
      cy.get('[data-testid=timelineCard]')
        .first()
        .click()
      const entryDetailsModal = cy.get('[data-testid=entryDetailsModal]')
      entryDetailsModal.get('[data-testid=backBtn]')
        .should('be.visible')
      entryDetailsModal.get('[data-testid=entryCardDetailsTitle]')
        .should('be.visible')
      entryDetailsModal.get('[data-testid=entryCardDate]')
          .should('be.visible')
      entryDetailsModal.get('[data-testid=immunizationProductTitle]')
        .should('be.visible')
      entryDetailsModal.get('[data-testid=immunizationProviderTitle]')
        .should('be.visible')
      entryDetailsModal.get('[data-testid=immunizationLotTitle]')
        .should('be.visible')
      entryDetailsModal.get('[data-testid=cardBtn]')
        .should('be.visible')
    })
})