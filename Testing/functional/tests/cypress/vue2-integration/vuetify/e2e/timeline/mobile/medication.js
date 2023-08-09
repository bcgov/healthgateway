const { AuthMethod } = require("../../../../../support/constants");

describe("Medication", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details for Mobile", () => {
        cy.get("[data-testid=timelineCard]").first().click();
        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
            cy.get("[data-testid=entryCardDate]").should("be.visible");

            cy.get("[data-testid=medicationTitle]").should("be.visible");
            cy.get("[data-testid=medication-practitioner]").should(
                "be.visible"
            );
        });
    });
});
