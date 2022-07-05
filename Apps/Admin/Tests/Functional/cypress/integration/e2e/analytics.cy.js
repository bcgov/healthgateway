import { verifyTestingEnvironment } from "../../support/functions/environment";

function getFileName(name) {
    // yyyy-mm-dd
    const today = new Date().toLocaleDateString("en-CA");
    const fileName = `${name}_export_${today}.csv`;
    cy.log(`File name: ${fileName}`);
    return fileName;
}

describe("System Analytics", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/CsvExport/GetUserProfiles").as(
            "getUserProfiles"
        );

        cy.intercept("GET", "**/CsvExport/GetComments").as("getComments");

        cy.intercept("GET", "**/CsvExport/GetNotes").as("getNotes");

        cy.intercept("GET", "**/CsvExport/GetRatings").as("getRatings");

        cy.intercept(
            "GET",
            "**/CsvExport/GetInactiveUsers?timeOffset=420&inactiveDays=100"
        ).as("getInactiveUsers");

        verifyTestingEnvironment();
        cy.log("Logging in.");
        cy.login(Cypress.env("idir_username"), Cypress.env("idir_password"));
        cy.log("Navigate to analytics page.");
        cy.visit("/analytics");
    });

    afterEach(() => {
        cy.log("Logging out.");
        cy.logout();
    });

    it("Verify system analytics stats downloads.", () => {
        cy.log("System Analytics stats download test started.");

        cy.get("[data-testid=user-profile-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getUserProfiles");
        cy.verifyDownload(getFileName("UserProfile"));

        cy.get("[data-testid=user-comments-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getComments");
        cy.verifyDownload(getFileName("Comments"));

        cy.get("[data-testid=user-notes-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getNotes");
        cy.verifyDownload(getFileName("Notes"));

        cy.get("[data-testid=user-ratings-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getRatings");
        cy.verifyDownload(getFileName("Ratings"));

        cy.get("[data-testid=days-inactive-input]").clear().type(100);
        cy.get("[data-testid=inactive-users-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getInactiveUsers");
        cy.verifyDownload(getFileName("InactiveUsers"));

        cy.log("System Analytics stats download test finished.");
    });
});
