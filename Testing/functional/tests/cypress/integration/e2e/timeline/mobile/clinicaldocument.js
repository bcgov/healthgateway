const { AuthMethod } = require("../../../../support/constants");

describe("Clinical Document", () => {
    beforeEach(() => {
        cy.enableModules("ClinicalDocument");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details for Mobile", () => {
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        const entryDetails = cy.get("[data-testid=entryDetailsCard]");
        cy.get("[data-testid=backBtn]").should("be.visible");
        entryDetails
            .get("[data-testid=entryCardDetailsTitle]")
            .should("be.visible");
        entryDetails.get("[data-testid=entryCardDate]").should("be.visible");
        entryDetails
            .get("[data-testid=clinical-document-discipline")
            .should("be.visible");
        entryDetails
            .get("[data-testid=clinical-document-facility")
            .should("be.visible");
    });
});
