const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
    waitForLaboratoryOrders,
    waitForTimelineCard,
} = require("../../../support/functions/timeline");

describe("Laboratory Orders", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*").as(
            "getLaboratoryOrders"
        );
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline",
            { waitForInitialDataLoad: false }
        );
        waitForLaboratoryOrders("@getLaboratoryOrders");
        cy.checkTimelineHasLoaded();
        waitForTimelineCard();
    });

    it("Validate file download", () => {
        cy.get("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=laboratory-report-download-btn]"
                );
            });
    });

    it("Validate attachment download", () => {
        cy.get("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });
});
