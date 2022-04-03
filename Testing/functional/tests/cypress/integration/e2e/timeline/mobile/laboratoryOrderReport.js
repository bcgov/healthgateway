const { AuthMethod } = require("../../../../support/constants");

beforeEach(() => {
    cy.deleteDownloadsFolder();
    cy.viewport("iphone-6");
    cy.restoreAuthCookies();
    cy.enableModules("AllLaboratory");
});

describe("Laboratory Orders - Download Report", () => {
    beforeEach(() => {
        cy.viewport("iphone-6");
        cy.restoreAuthCookies();
        cy.enableModules("AllLaboratory");
        cy.intercept(
            "GET",
            "**/v1/api/Laboratory/*/Report?hdid=P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A&isCovid19=false"
        ).as("getLaboratoryReport");

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Download", () => {
        cy.log("Verifying Laboratory Report PDF download");

        cy.get("[data-testid=timelineCard]").first().click();

        cy.get("[data-testid=laboratory-report-download-btn]")
            .should("be.visible")
            .contains("Incomplete")
            .click({ force: true });

        cy.get("[data-testid=genericMessageSubmitBtn]")
            .should("be.visible")
            .click({ force: true });

        cy.wait("@getLaboratoryReport");
        cy.verifyDownload("Laboratory_Report_YYYY_07_Jul 4, 2021-01_43.pdf");

        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});
