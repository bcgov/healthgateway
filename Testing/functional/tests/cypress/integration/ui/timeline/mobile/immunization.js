const { AuthMethod } = require("../../../../support/constants");

describe("Immunization", () => {
    beforeEach(() => {
        cy.enableModules("Immunization");
        cy.intercept("GET", "**/v1/api/Immunization?*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details on Mobile", () => {
        cy.get("[data-testid=timelineCard]").first().click();

        cy.get("[data-testid=entryDetailsCard]").should("be.visible");
        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDate]").should("be.visible");
        cy.get("[data-testid=immunizationProductTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProviderTitle]").should("be.visible");
        cy.get("[data-testid=immunizationLotTitle]").should("be.visible");
        cy.get("[data-testid=cardBtn]").should("be.visible");
    });
});
