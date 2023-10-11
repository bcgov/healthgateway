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

    it("Validate card details with a file", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                cy.get("[data-testid=diagnosticimagingTitle]")
                    .should("be.visible")
                    .click({ force: true });
                cy.get(
                    "[data-testid=diagnostic-imaging-procedure-description]"
                ).should("be.visible");
                cy.get(
                    "[data-testid=diagnostic-imaging-health-authority]"
                ).should("be.visible");
                cy.get(
                    "[data-testid=diagnostic-imaging-download-button]"
                ).should("be.visible");
            });
    });

    it("Validate card details without a file", () => {
        cy.get("[data-testid=timelineCard")
            .not(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                cy.get("[data-testid=diagnosticimagingTitle]")
                    .should("be.visible")
                    .click({ force: true });
                cy.get(
                    "[data-testid=diagnostic-imaging-procedure-description]"
                ).should("be.visible");
                cy.get(
                    "[data-testid=diagnostic-imaging-health-authority]"
                ).should("be.visible");
                cy.get(
                    "[data-testid=diagnostic-imaging-download-button]"
                ).should("not.exist");
            });
    });

    it("Validate file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=diagnostic-imaging-download-button]"
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
