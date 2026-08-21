const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("Laboratory Orders", () => {
    function waitForLabResults() {
        return cy
            .wait("@getLabResults", { timeout: 120000 })
            .its("response.statusCode")
            .should("eq", 200);
    }

    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders?hdid=*").as(
            "getLabResults"
        );
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        waitForLabResults();
        cy.checkTimelineHasLoaded();
    });

    it("Validate file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=laboratory-report-download-btn]"
                );
            });
    });

    it("Validate attachment download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });
});
