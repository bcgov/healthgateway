const { AuthMethod } = require("../../../../support/constants");

function selectShouldBeVisible(selector) {
    cy.get(selector).should("be.visible");
}

describe("Laboratory Orders", () => {
    before(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Validate Card", () => {
        cy.log("Verifying card data");
        cy.get("[data-testid=timelineCard]");
        cy.get("[data-testid=timelineCard]")
            .first()
            .click()
            .then(() => {
                selectShouldBeVisible("[data-testid=backBtn]");
                selectShouldBeVisible("[data-testid=entryCardDetailsTitle]");
                selectShouldBeVisible(
                    "[data-testid=laboratoryHeaderResultCount]"
                );
                selectShouldBeVisible("[data-testid=laboratoryCollectionDate]");
                selectShouldBeVisible(
                    "[data-testid=laboratoryOrderingProvider]"
                );
                selectShouldBeVisible("[data-testid=laboratoryReportingLab]");
            });

        cy.log("Verifying partial status");
        cy.get("[data-testid=laboratoryResultTable]").then(() => {
            // Check the 2nd column for the pending status
            cy.contains("td:nth-child(2)", "Pending")
                .parent("tr")
                .children()
                .first();
            // check the 3rd column for the result
            cy.contains("td:nth-child(3)", "Partial")
                .parent("tr")
                .children()
                .first();
        });

        cy.get("[data-testid=backBtn]").click();
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});
