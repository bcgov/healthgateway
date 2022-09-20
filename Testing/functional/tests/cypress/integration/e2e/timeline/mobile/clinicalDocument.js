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

        cy.get("[data-testid=entryDetailsCard]")
            .children()
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=entryCardDetailsTitle]").should(
                    "be.visible"
                );
                cy.get("[data-testid=entryCardDate]").should("be.visible");
                cy.get("[data-testid=clinical-document-discipline]").should(
                    "be.visible"
                );
                cy.get("[data-testid=clinical-document-facility]").should(
                    "be.visible"
                );
            });
    });
});
