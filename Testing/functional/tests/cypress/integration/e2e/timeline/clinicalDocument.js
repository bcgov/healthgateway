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
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=clinicaldocumentTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle")
            .first()
            .click({ force: true });
        cy.get("[data-testid=clinical-document-discipline").should(
            "be.visible"
        );
        cy.get("[data-testid=clinical-document-facility").should("be.visible");
    });

    it("Validate file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=clinical-document-download-button]"
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
