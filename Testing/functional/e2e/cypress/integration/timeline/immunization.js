const { AuthMethod, localDevUri } = require("../../support/constants")
let immunizationAPIPath=""
describe('Immunization', () => {
    before(() => {
        cy.readConfig().as("config").then(config => {
          config.webClient.modules.CovidLabResults = false
          config.webClient.modules.Comment = false
          config.webClient.modules.Encounter = false
          config.webClient.modules.Immunization = true
          config.webClient.modules.Laboratory = false
          config.webClient.modules.Medication = false
          config.webClient.modules.MedicationHistory = false
          config.webClient.modules.Note = false
          immunizationAPIPath=config.serviceEndpoints.Immunization
          cy.fixture("ImmunizationService/immunizationrefresh.json").then((ImmunizationResponse) => {
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);            
            cy.route('GET', `${immunizationAPIPath}v1/api/Immunization/*`, ImmunizationResponse);
            cy.log("Configuration", config);
            cy.log("ImmunizationResponse", ImmunizationResponse)
          })
        })
        cy.login(Cypress.env('keycloak.username'),
            Cypress.env('keycloak.password'),
            AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Immunization Loading', () => {
          cy.get('[data-testid=immunizationLoading]')
          .should('be.visible')
          cy.fixture("ImmunizationService/immunization.json").then((ImmunizationResponse) => {
            cy.server();
            cy.route('GET', `${immunizationAPIPath}v1/api/Immunization/*`, ImmunizationResponse);
            cy.log("ImmunizationResponse", ImmunizationResponse);
            cy.get('[data-testid=immunizationLoading]')
              .should('not.exist')
            cy.get('[data-testid=immunizationReady]')
              .should('be.visible')
              .find('[data-testid=immunizationBtnReady]')
                .should('be.visible')
                .click()
          })
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