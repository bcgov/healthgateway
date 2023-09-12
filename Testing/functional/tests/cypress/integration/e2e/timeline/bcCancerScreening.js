const { AuthMethod } = require("../../../support/constants");

describe("Diagnostic Imaging", () => {
    function downloadFileTests(fileNamePrefix) {
        // Expand the card (require force as the title isn't a natural click target)
        cy.get("[data-testid=bccancerscreeningTitle]")
            .should("be.visible")
            .click({ force: true });
        // Test for generic message modal regarding sensitive data
        cy.get("[data-testid=bc-cancer-screening-download-button]")
            .should("be.visible")
            .click();
        cy.document()
            .find("[data-testid=generic-message-modal]")
            .should("be.visible");
        // Submit the generic message modal, which should close the modal
        cy.document().find("[data-testid=generic-message-submit-btn]").click();
        cy.document()
            .find("[data-testid=generic-message-modal]")
            .should("not.exist");

        // Test for file download with the entry date suffix.
        cy.get("[data-testid=entryCardDate]")
            .first()
            .invoke("text")
            .then((text) => {
                var date = new Date(text);
                var dateSuffix = date
                    .toISOString()
                    .slice(0, 10)
                    .replace(/-/g, "_");

                cy.verifyDownload(`${fileNamePrefix}_${dateSuffix}`, {
                    contains: true,
                });
            });
    }

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

    it("Validate result file download", () => {
        cy.get("[data-testid=timelineCard")
            .filter(`:contains(BC Cancer Result)`)
            .first()
            .within(() => downloadFileTests("bc_cancer_result"));
    });

    it("Validate recall file download", () => {
        cy.get("[data-testid=timelineCard]")
            .filter(`:contains(BC Cancer Screening)`)
            .first()
            .within(() => downloadFileTests("bc_cancer_screening"));
    });
});
