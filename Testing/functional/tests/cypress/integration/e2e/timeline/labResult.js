const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
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
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
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
