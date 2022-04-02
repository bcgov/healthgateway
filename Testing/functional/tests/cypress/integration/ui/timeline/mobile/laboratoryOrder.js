const { AuthMethod } = require("../../../../support/constants");

before(() => {
    cy.deleteDownloadsFolder();
});

beforeEach(() => {
    cy.viewport("iphone-6");
    cy.restoreAuthCookies();
    cy.enableModules("AllLaboratory");
});

describe("Laboratory Orders - Report", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });

        cy.intercept(
            "GET",
            "**/v1/api/Laboratory/*/Report?hdid=P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A&isCovid19=false",
            {
                fixture: "LaboratoryService/laboratoryReportPdf.json",
            }
        );

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Download", () => {
        cy.log("Verifying Laboratory Report PDF download");
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("[data-testid=laboratory-report-download-btn]")
            .should("be.visible")
            .contains("Incomplete")
            .click({ force: true });

        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true })
            .within(() => {
                cy.verifyDownload(
                    "Laboratory_Report_YYYY_04_Apr 4, 2021-08_43.pdf"
                );
            });
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Laboratory Orders Not Queued", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });
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

        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=laboratoryHeaderResultCount]").should(
            "be.visible"
        );
        cy.get("[data-testid=laboratoryCollectionDate]").should("be.visible");
        cy.get("[data-testid=laboratoryOrderingProvider]").should("be.visible");
        cy.get("[data-testid=laboratoryReportingLab]").should("be.visible");

        cy.log("Verifying partial status");
        cy.get("[data-testid=laboratoryResultTable]").within(() => {
            cy.contains("td", "Alanine Aminotransferase Test")
                .parent("tr")
                .within(() => {
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
                        expect(status).equal("Partial");
                    });
                });
        });

        cy.get("[data-testid=backBtn]").click({ force: true });
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});

describe("Laboratory Orders Refresh", () => {
    beforeEach(() => {
        let isLoading = false;
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*", (req) => {
            req.reply((res) => {
                if (!isLoading) {
                    res.send({
                        fixture:
                            "LaboratoryService/laboratoryOrdersRefresh.json",
                    });
                } else {
                    res.send({
                        fixture: "LaboratoryService/laboratoryOrders.json",
                    });
                }
                isLoading = !isLoading;
            });
        });

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

        // Validate collection date time when not null in json
        cy.get("[data-testid=timelineCard]").eq(6).scrollIntoView().click();
        cy.get("[data-testid=laboratory-collection-date-value]").should(
            "be.visible"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });

        // Validate collection date time when attribute is not passed in json
        cy.get("[data-testid=timelineCard]").eq(7).click();
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.get("[data-testid=laboratory-collection-date-value]").should(
                "not.exist"
            );
        });
        cy.get("[data-testid=backBtn]").click({ force: true });

        // Validate collection date time when attribute value is null in json
        cy.get("[data-testid=timelineCard]").eq(8).click();
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.get("[data-testid=laboratory-collection-date-value]").should(
                "not.exist"
            );
        });
        cy.get("[data-testid=backBtn]").click({ force: true });
        cy.get("[data-testid=filterTextInput]").should("be.visible");
    });
});

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/v1/api/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersQueued.json",
        });

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
