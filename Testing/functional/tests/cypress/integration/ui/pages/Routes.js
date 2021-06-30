const { AuthMethod } = require("../../../support/constants");

describe("Bookmark", () => {
    beforeEach(() => {
        cy.enableModules(["Medication", "Comment"]);
    });

    it("Redirect to UserProfile", () => {
        let path = "/profile";
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", path);
    });

    it("Redirect to Insights", () => {
        let path = "/healthInsights";
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", path);
    });
});
