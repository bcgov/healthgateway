const { AuthMethod } = require("../../../../support/constants");

describe("Laboratory Orders", () => {
    beforeEach(() => {
        cy.enableModules("AllLaboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card", () => {
        cy.log("Verifying card data");
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "not.exist"
        );
        cy.get("[data-testid=timelineCard]").first().click();

        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=laboratory-header-order-status").should(
            "be.visible"
        );
        cy.get("[data-testid=laboratory-collection-date]").should("be.visible");
        cy.get("[data-testid=laboratory-ordering-provider]").should(
            "be.visible"
        );
        cy.get("[data-testid=laboratory-reporting-lab]").should("be.visible");
        cy.get("[data-testid=reporting-lab-information-text]").should(
            "be.visible"
        );

        cy.get("[data-testid=laboratoryResultTable]")
            .first()
            .within(() => {
                cy.get("td:nth-child(3)")
                    .eq(1)
                    .then(($status) => {
                        cy.get("td:nth-child(2)")
                            .eq(1)
                            .then(($result) => {
                                const result = $result.text().trim();
                                const status = $status.text().trim();
                                cy.log(result);
                                cy.log(status);

                                if (status === "Completed") {
                                    expect([
                                        "Out of Range",
                                        "In Range",
                                    ]).to.include(result);
                                }

                                if (status === "Cancelled") {
                                    expect(result).equal("Cancelled");
                                }

                                if (result === "Pending") {
                                    // PHSA stattus is Actuve but has been converted to Pending for user readability
                                    expect(status).equal("Pending");
                                }
                            });
                    });
            });
    });
});
