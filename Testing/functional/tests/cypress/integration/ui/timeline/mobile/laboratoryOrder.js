const { AuthMethod } = require("../../../../support/constants");

function checkPopoverIsVisible() {
    cy.get("[data-testid=laboratory-test-status-info-button]")
        .click()
        .invoke("attr", "id")
        .then((id) => {
            cy.root()
                .closest("html")
                .within(() => {
                    cy.get(`[data-testid=${id}-popover]`).should("be.visible");
                });
        });
}

describe("Laboratory Orders", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
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
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=backBtn]").should("be.visible");
                cy.get("[data-testid=entryCardDetailsTitle]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratory-header-order-status]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratory-collection-date]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratory-ordering-provider]").should(
                    "be.visible"
                );
                cy.get("[data-testid=laboratory-reporting-lab]").should(
                    "be.visible"
                );
                cy.get("[data-testid=reporting-lab-information-text]").should(
                    "be.visible"
                );
                cy.get("[data-testid=other-resources-info-button]")
                    .should("be.visible")
                    .click();
                cy.get("[data-testid=other-resources-info-popover]").should(
                    "be.visible"
                );
                cy.get("[data-testid=result-info-button]")
                    .should("be.visible")
                    .click();
                cy.get("[data-testid=result-info-popover]").should(
                    "be.visible"
                );

                cy.log("Verifying partial status");
                cy.get("[data-testid=laboratoryResultTable]").within(() => {
                    cy.contains("td", "Alanine Aminotransferase Test")
                        .parent("tr")
                        .within(() => {
                            // Check the Name Column
                            cy.get("td:nth-child(1)").then(($name) => {
                                const name = $name.text().trim();
                                cy.log(name);
                                expect(name).equal(
                                    "Alanine Aminotransferase Test"
                                );
                            });
                            // Check the Result Column
                            cy.get("td:nth-child(2)").then(($result) => {
                                const result = $result.text().trim();
                                cy.log(result);
                                expect(result).equal("Pending");
                            });
                            // Check the Status Column
                            cy.get("td:nth-child(3)").then(($status) => {
                                const status = $status.text().trim();
                                cy.log(status);
                                expect(status).equal("Pending");
                            });
                            // Check the Status Popover
                            checkPopoverIsVisible();
                        });

                    cy.contains(
                        "td",
                        "Creatinine & Glomerular Filtration Rate Predicted Panel"
                    )
                        .parent("tr")
                        .within(() => {
                            // Check the Name Column
                            cy.get("td:nth-child(1)").then(($name) => {
                                const name = $name.text().trim();
                                cy.log(name);
                                expect(name).equal(
                                    "Creatinine & Glomerular Filtration Rate Predicted Panel"
                                );
                            });
                            // Check the Result Column
                            cy.get("td:nth-child(2)").then(($result) => {
                                const result = $result.text().trim();
                                cy.log(result);
                                expect(result).equal("Out of Range");
                            });
                            // Check the Status Column
                            cy.get("td:nth-child(3)").then(($status) => {
                                const status = $status.text().trim();
                                cy.log(status);
                                expect(status).equal("Completed");
                            });
                            // Check the Status Popover
                            checkPopoverIsVisible();
                        });

                    cy.contains("td", "Glucose Random")
                        .parent("tr")
                        .within(() => {
                            // Check the Name Column
                            cy.get("td:nth-child(1)").then(($name) => {
                                const name = $name.text().trim();
                                cy.log(name);
                                expect(name).equal("Glucose Random");
                            });
                            // Check the Result Column
                            cy.get("td:nth-child(2)").then(($result) => {
                                const result = $result.text().trim();
                                cy.log(result);
                                expect(result).equal("In Range");
                            });
                            // Check the Status Column
                            cy.get("td:nth-child(3)").then(($status) => {
                                const status = $status.text().trim();
                                cy.log(status);
                                expect(status).equal("Completed");
                            });
                            // Check the Status Popover
                            checkPopoverIsVisible();
                        });

                    cy.contains("td", "CBC & Differential")
                        .parent("tr")
                        .within(() => {
                            // Check the Name Column
                            cy.get("td:nth-child(1)").then(($name) => {
                                const name = $name.text().trim();
                                cy.log(name);
                                expect(name).equal("CBC & Differential");
                            });
                            // Check the Result Column
                            cy.get("td:nth-child(2)").then(($result) => {
                                const result = $result.text().trim();
                                cy.log(result);
                                expect(result).equal("Cancelled");
                            });
                            // Check the Status Column
                            cy.get("td:nth-child(3)").then(($status) => {
                                const status = $status.text().trim();
                                cy.log(status);
                                expect(status).equal("Cancelled");
                            });
                            // Check the Status Popover
                            checkPopoverIsVisible();
                        });
                });

                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        cy.log("Verifying collection date");

        // Validate collection date time when not null in json
        cy.get("[data-testid=timelineCard]").eq(6).scrollIntoView().click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratory-collection-date-value]").should(
                    "be.visible"
                );
                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        // Validate collection date time when attribute is not passed in json
        cy.get("[data-testid=timelineCard]").eq(7).click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=entryDetailsCard]").within(() => {
                    cy.get(
                        "[data-testid=laboratory-collection-date-value]"
                    ).should("not.exist");
                });
                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        // Validate collection date time when attribute value is null in json
        cy.get("[data-testid=timelineCard]").eq(8).click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=entryDetailsCard]").within(() => {
                    cy.get(
                        "[data-testid=laboratory-collection-date-value]"
                    ).should("not.exist");
                });
                cy.get("[data-testid=backBtn]").click({ force: true });
            });
    });
});

describe("Laboratory Orders Refresh", () => {
    beforeEach(() => {
        let isLoading = false;
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: "LaboratoryService/laboratoryOrdersRefresh.json",
                });
            } else {
                req.reply({
                    fixture: "LaboratoryService/laboratoryOrders.json",
                });
            }
            isLoading = !isLoading;
        });
        cy.enableModules("AllLaboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkOnTimeline();
    });

    it("Validate Refresh", () => {
        cy.log("Verify on timeline and refresh in progress");
        cy.get("[data-testid=loading-in-progress]").should("exist");
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "not.exist"
        );

        // Verify initial call
        cy.log(
            "Verify refresh in progress call from PHSA has returned 1 record."
        );
        cy.get("[data-testid=displayCountText]")
            .should("be.visible")
            .contains("Displaying 1 out of 1 records");

        // Verify subsequent call
        cy.log(
            "Verify refresh in progress call from PHSA has returned remaining records."
        );
        cy.get("[data-testid=loading-in-progress]").should("exist");
        cy.get("[data-testid=displayCountText]")
            .should("be.visible")
            .contains("Displaying 9 out of 9 records");
        cy.get("[data-testid=loading-in-progress]").should("not.exist");
    });
});

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersQueued.json",
        });
        cy.enableModules("AllLaboratory");
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
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
