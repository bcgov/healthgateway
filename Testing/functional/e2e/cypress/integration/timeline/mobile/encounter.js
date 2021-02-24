const { AuthMethod } = require("../../../support/constants")

describe('MSP Visits', () => {
    beforeEach(() => {
        cy.enableModules("Encounter");
        cy.viewport('iphone-6');
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Validate Card Details for Mobile', () => {
        cy.get('[data-testid=timelineCard]')
          .first()
          .click()
        const entryDetailsModal = cy.get('[data-testid=entryDetailsModal]')
        entryDetailsModal.get('[data-testid=backBtn]')
          .should('be.visible')
        entryDetailsModal.get('[data-testid=entryCardDetailsButton]')
          .should('be.visible')
        entryDetailsModal.get('[data-testid=entryCardDate]')
            .should('be.visible')
        entryDetailsModal.get('[data-testid=encounterClinicLabel')
            .should('be.visible');
        entryDetailsModal.get('[data-testid=encounterClinicName')
            .should('be.visible');
        entryDetailsModal.get('[data-testid=encounterClinicAddress')
            .should('be.visible');
    })
})