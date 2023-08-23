const { AuthMethod } = require("../../../../support/constants");

function checkPopoverIsVisible() {
    cy.get("[data-testid=laboratory-test-status-info-button]")
        .click()
        .invoke("attr", "id")
        .then((id) => {
            cy.root()
                .closest("html")
                .within(() => {
                    cy.get(`[data-testid=${id}-popover]`).should("exist");
                });
        });
}

const recordDisplayMessage = (lower, upper, total) =>
    `Displaying ${lower} to ${upper} out of ${total} records`;

describe("Laboratory Orders", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
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
                cy.get("[data-testid=entryCardDetailsSubtitle]").should(
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

                cy.document()
                    .find("[data-testid=result-info-popover]")
                    .should("exist");
            });

        cy.get("[data-testid=backBtn]").click({ force: true });
    });

    it("Should have a valid result table", () => {
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();
        cy.get("[data-testid=laboratoryResultTable]").within(() => {
            cy.contains("td", "Alanine Aminotransferase Test")
                .parent("tr")
                .within(() => {
                    // Check the Name Column
                    cy.get("td:nth-child(1)").then(($name) => {
                        const name = $name.text().trim();
                        cy.log(name);
                        expect(name).equal("Alanine Aminotransferase Test");
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
    });

    it("Should have valid collection dates", () => {
        // Validate collection date time when not null in json
        cy.get("[data-testid=timelineCard]").eq(6).scrollIntoView().click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.contains(
                    "[data-testid=laboratory-collection-date]",
                    "2021-Jun-05"
                ).should("be.visible");
                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        // Validate collection date time when attribute is not passed in json
        cy.get("[data-testid=timelineCard]").eq(7).click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=entryDetailsCard]").within(() => {
                    cy.contains(
                        "[data-testid=laboratory-collection-date]",
                        "N/A"
                    ).should("be.visible");
                });
                cy.get("[data-testid=backBtn]").click({ force: true });
            });

        // Validate collection date time when attribute value is null in json
        cy.get("[data-testid=timelineCard]").eq(8).click();
        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=entryDetailsCard]").within(() => {
                    cy.contains(
                        "[data-testid=laboratory-collection-date]",
                        "N/A"
                    ).should("be.visible");
                });
                cy.get("[data-testid=backBtn]").click({ force: true });
            });
    });
});

describe("Laboratory Orders Refresh", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
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
        cy.checkOnTimeline();
    });

    it("Validate Refresh", () => {
        cy.log("Verify on timeline and refresh in progress");
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "not.exist"
        );
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersRefresh.json",
        });
        // Verify initial call
        cy.log(
            "Verify refresh in progress call from PHSA has returned 1 record."
        );
        cy.get("[data-testid=timeline-record-count]")
            .contains("Displaying 1 out of 1 records")
            .should("be.visible");

        // Verify subsequent call
        cy.log(
            "Verify refresh in progress call from PHSA has returned remaining records."
        );
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
        cy.get("[data-testid=loading-toast]").should("exist");
        cy.get("[data-testid=timeline-record-count]")
            .should("be.visible")
            .contains(recordDisplayMessage(1, 9, 9));
        cy.get("[data-testid=loading-toast]").should("not.exist");
    });
});

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersQueued.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
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

    it("Show Queued Alert Message", () => {
        cy.log("Verifying queued alert message displays");
        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });
});
