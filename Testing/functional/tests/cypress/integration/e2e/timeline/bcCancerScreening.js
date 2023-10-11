const { AuthMethod } = require("../../../support/constants");
const {
    validateAttachmentDownload,
    validateFileDownload,
} = require("../../../support/functions/timeline");

describe("BC Cancer", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "bcCancerScreening",
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
        cy.get("[data-testid=timelineCard")
            .first()
            .within(() => {
                cy.get("[data-testid=bccancerscreeningTitle]")
                    .should("be.visible")
                    .click({ force: true });
                cy.get(
                    "[data-testid=bc-cancer-screening-download-button]"
                ).should("be.visible");
            });
    });

    it("Validate screening file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .filter(`:contains("BC Cancer Screening")`)
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=bc-cancer-screening-download-button]"
                );
            });
    });

    it("Validate screening attachment download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .filter(`:contains("BC Cancer Screening")`)
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });

    it("Validate result file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .filter(`:contains("BC Cancer Result")`)
            .first()
            .within(() => {
                validateFileDownload(
                    "[data-testid=bc-cancer-screening-download-button]"
                );
            });
    });

    it("Validate result attachment download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(":has([data-testid=attachment-button])")
            .filter(`:contains("BC Cancer Result")`)
            .first()
            .within(() => {
                validateAttachmentDownload();
            });
    });
});
