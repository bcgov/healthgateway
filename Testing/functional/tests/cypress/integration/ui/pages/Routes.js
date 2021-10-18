const { AuthMethod } = require("../../../support/constants");
const profilePath = "/profile";
const healthInsightsPath = "/healthInsights";
const dashboardPath = "/dashboard";

describe("Bookmark", () => {
    beforeEach(() => {
        cy.enableModules(["Medication", "Comment"]);
    });

    it("Redirect to UserProfile", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", profilePath);
    });

    it("Redirect to Insights", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", healthInsightsPath);
    });

    it("Redirect to Dashboard", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            path
        );
        cy.url().should("include", dashboardPath);
    });
});
