const timeout = 45000;

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

        cy.intercept("GET", "**/CsvExport/GetInactiveUsers*").as(
            "getInactiveUsers"
        );

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/analytics"
        );
    });

    it("Verify system analytics stats downloads.", () => {
        cy.log("System Analytics stats download test started.");

        cy.get("[data-testid=user-profile-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getUserProfiles", { timeout });
        cy.verifyDownload(getFileName("UserProfile"));

        cy.get("[data-testid=user-comments-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getComments", { timeout });
        cy.verifyDownload(getFileName("Comments"));

        cy.get("[data-testid=user-notes-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getNotes", { timeout });
        cy.verifyDownload(getFileName("Notes"));

        cy.get("[data-testid=user-ratings-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getRatings", { timeout });
        cy.verifyDownload(getFileName("Ratings"));

        cy.get("[data-testid=days-inactive-input]").clear().type(100);
        cy.get("[data-testid=inactive-users-download-btn]")
            .should("be.visible")
            .click();
        cy.wait("@getInactiveUsers", { timeout });
        cy.verifyDownload(getFileName("InactiveUsers"));

        cy.log("System Analytics stats download test finished.");
    });
});
