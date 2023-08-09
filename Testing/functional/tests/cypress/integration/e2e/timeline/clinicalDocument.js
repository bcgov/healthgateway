const { AuthMethod } = require("../../../support/constants");

describe("Clinical Document", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=clinicaldocumentTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle").first().click();
        cy.get("[data-testid=clinical-document-discipline").should(
            "be.visible"
        );
        cy.get("[data-testid=clinical-document-facility").should("be.visible");
    });
});
