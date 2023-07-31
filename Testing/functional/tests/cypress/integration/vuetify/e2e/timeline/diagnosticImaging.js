const { AuthMethod } = require("../../../../support/constants");

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

    it("Validate Card Details", () => {
        cy.get("[data-testid=diagnosticimagingTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle").first().click();
        cy.get("[data-testid=diagnostic-imaging-body-part").should(
            "be.visible"
        );
        cy.get("[data-testid=diagnostic-imaging-procedure-description").should(
            "be.visible"
        );
        cy.get("[data-testid=diagnostic-imaging-health-authority").should(
            "be.visible"
        );
        cy.get("[data-testid=diagnostic-imaging-facility").should("be.visible");
        cy.get("[data-testid=diagnostic-imaging-download-button").should(
            "be.visible"
        );
    });

    it("Validate file download", () => {
        cy.get("[data-testid=diagnosticimagingTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle").first().click();
        cy.get("[data-testid=diagnostic-imaging-download-button").should(
            "be.visible"
        );
        // Test for generic message modal regarding sensitive data
        cy.get("[data-testid=diagnostic-imaging-download-button")
            .first()
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        // Submit the generic message modal, which should close the modal
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");

        // Test for file download with the entry date prefxied to the file name.
        cy.get("[data-testid=entryCardDate]")
            .first()
            .invoke("text")
            .then((text) => {
                var date = new Date(text);
                var datePrefix = date
                    .toISOString()
                    .slice(0, 10)
                    .replace(/-/g, "_");

                cy.verifyDownload(`diagnostic_image_${datePrefix}`, {
                    contains: true,
                });
            });
    });
});
