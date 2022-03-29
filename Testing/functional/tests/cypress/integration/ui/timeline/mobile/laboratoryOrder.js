const { AuthMethod } = require("../../../../support/constants");

describe("Laboratory Orders Not Queued", () => {
    beforeEach(() => {
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

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
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
