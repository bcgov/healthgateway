const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("Clinical Document", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/ClinicalDocument/*").as("getClinicalDocument");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline",
            { waitForInitialDataLoad: false }
        );
        cy.wait("@getClinicalDocument", { timeout: 60000 });
        cy.checkTimelineHasLoaded();
    });

    it("Validate file and attachment downloads", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=clinical-document-download-button]"
                );
                validateAttachmentDownload();
            });
    });
});
