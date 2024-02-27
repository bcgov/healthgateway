const { AuthMethod } = require("../../../../support/constants");

describe("Clinical Document", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        cy.intercept(
            "GET",
            "**/ClinicalDocument/*/file/14bac0b6-9e95-4a1b-b6fd-d354edfce4e7-710b28fa980440fd93c426e25c0ce52f",
            {
                fixture: "ClinicalDocumentService/clinicalDocumentPdf.json",
            }
        );
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
        });

        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Download", () => {
        cy.log("Verifying Clinical Document PDF download");
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=clinical-document-download-button]")
                    .should("be.visible")
                    .click({ force: true });
            });

        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });
        cy.verifyDownload("Clinical_Document_2021_11_15-00_00.pdf");
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});
