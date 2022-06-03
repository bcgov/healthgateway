const { localDevUri } = require("../../../support/constants");

describe("Admin Login", () => {
    if (Cypress.config().baseUrl !== localDevUri) {
        it("Should login via IDIR and then logout successfully", () => {
            cy.login(
                Cypress.env("idir_username"),
                Cypress.env("idir_password")
            );
            cy.logout();
        });
    } else {
        cy.log("Skipped IDIR login test as running locally");
    }
});
