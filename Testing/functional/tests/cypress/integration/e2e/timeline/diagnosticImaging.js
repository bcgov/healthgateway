const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("Diagnostic Imaging", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "diagnosticImaging",
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
        cy.contains("[data-testid=entryCardDate]", "2022-Oct-08")
            .parents("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=diagnostic-imaging-download-button]"
                );
            });
    });

    it("Validate attachment download", () => {
        cy.contains("[data-testid=entryCardDate]", "2022-Oct-08")
            .parents("[data-testid=timelineCard]")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });
});
