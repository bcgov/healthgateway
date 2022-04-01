const { AuthMethod } = require("../../../../support/constants");

describe("Laboratory Orders Not Queued", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
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
        cy.get("[data-testid=laboratoryHeaderResultCount]").should(
            "be.visible"
        );
        cy.get("[data-testid=laboratoryCollectionDate]").should("be.visible");
        cy.get("[data-testid=laboratoryOrderingProvider]").should("be.visible");
        cy.get("[data-testid=laboratoryReportingLab]").should("be.visible");

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

        cy.get("[data-testid=backBtn]").click();
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});

describe("Laboratory Orders Queud", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.login(
            Cypress.env("keycloak.laboratory.queued.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Show Queued Alert Message", () => {
        cy.log("Verifying queued alert message displays");
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });
});

describe("Laboratory Orders Refresh in progress", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*").as(
            "getLaboratoryOrders"
        );
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Validate Refresh", () => {
        cy.log("Verify on timeline and refresh in progress");

        // Verify initial call
        cy.wait("@getLaboratoryOrders").then((interception) => {
            let refreshInProgress =
                !interception.response.body.resourcePayload.loaded;
            cy.log("Verify on timeline and refresh in progress");
            cy.log(
                `Resource Payload Loaded:  ${interception.response.body.resourcePayload.loaded}`
            );

            cy.log(`Refresh in Progress:  ${refreshInProgress}`);
            cy.checkOnTimeline();
            cy.get(
                "[data-testid=laboratory-orders-queued-alert-message]"
            ).should("not.exist");

            // PHSA data is always changing so just check if display count text is displayed.
            // We just need to confirm the call was made and was successful.
            cy.get("[data-testid=displayCountText]").should("be.visible");

            // Verify subsequent call
            if (refreshInProgress) {
                cy.log(
                    "Refresh in progress is true so verifying subsequent Laboratory Orders call from PHSA."
                );
                cy.get("[data-testid=loading-in-progress]").should("exist");
                cy.wait("@getLaboratoryOrders");

                // PHSA data is always changing so just check if display count text is displayed.
                // We just need to confirm the call was made and was successful.
                cy.get("[data-testid=displayCountText]").should("be.visible");
            }
            cy.get("[data-testid=loading-in-progress]").should("not.exist");
        });
    });
});
