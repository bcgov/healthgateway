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
        cy.get("[data-testid=laboratory-header-result-count").should(
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

                                if (status === "Partial") {
                                    expect(result).equal("Pending");
                                }

                                if (result === "Out of Range") {
                                    expect(status).equal("Final");
                                }
                            });
                    });
            });

        cy.get("[data-testid=backBtn]").click({ force: true });
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});
