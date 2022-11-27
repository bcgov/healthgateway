const { AuthMethod } = require("../../../support/constants");

describe("Medication Request", () => {
    beforeEach(() => {
        cy.enableModules("MedicationRequest");
        cy.intercept("GET", "**/MedicationRequest/*", {
            fixture: "MedicationService/medicationRequest.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=medicationrequestTitle]").should("be.visible");
        cy.get("[data-testid=medicationPractitioner]").should("not.be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").first().click();
        cy.get("[data-testid=medicationPractitioner]").should("be.visible");
    });
});
