const { localDevUri } = require("../../support/constants");

describe("Authentication", () => {
    if (Cypress.config().baseUrl !== localDevUri) {
        it("IDIR login and logout successfully.", () => {
            cy.log("IDIR login/logout test started.");
            cy.login(
                Cypress.env("idir_username"),
                Cypress.env("idir_password")
            );
            cy.logout();
            cy.log("IDIR login/logout test finished.");
        });
    } else {
        cy.log("Skipped IDIR login/logout test as running locally");
    }
});
