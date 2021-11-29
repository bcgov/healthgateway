const { AuthMethod } = require("../../../support/constants");

describe("MSP Visits", () => {
    beforeEach(() => {
        cy.enableModules("Encounter");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=encounterTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle").first().click();
        cy.get("[data-testid=encounterClinicLabel").should("be.visible");
        cy.get("[data-testid=encounterClinicName").should("be.visible");
    });
});
