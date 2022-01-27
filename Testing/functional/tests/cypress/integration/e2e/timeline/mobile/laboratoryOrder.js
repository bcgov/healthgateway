const { AuthMethod } = require("../../../../support/constants");

function selectShouldBeVisible(selector) {
    cy.get(selector).should("be.visible");
}

describe("Laboratory Orders", () => {
    before(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Validate Card", () => {
        cy.log("Verifying card data");
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

        cy.get("[data-testid=laboratoryResultTable]")
            .first()
            .then(() => {
                cy.get("td:nth-child(3)")
                    .eq(1)
                    .then(($status) => {
                        cy.get("td:nth-child(2)")
                            .eq(1)
                            .then(($result) => {
                                cy.log($result.text().trim());
                                cy.log($status.text().trim());
                                let result = $result.text().trim();
                                let status = $status.text().trim();

                                if (status === "Partial") {
                                    expect(result).equal("Pending");
                                }

                                if (result === "Out of Range") {
                                    expect(status).equal("Final");
                                }
                            });
                    });
            });
        cy.get("[data-testid=backBtn]").click();
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});
