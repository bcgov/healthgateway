const { AuthMethod } = require("../../../support/constants");

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

    it.skip("Validate file download", () => {
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
        // Test for file download
        cy.verifyDownload("diagnostic_image_2022_05_09-07_42.pdf");
    });
});
