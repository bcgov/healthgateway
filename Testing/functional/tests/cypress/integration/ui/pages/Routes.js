const { AuthMethod } = require("../../../support/constants");
const profilePath = "/profile";
const healthInsightsPath = "/healthInsights";
const homePath = "/home";

describe("Bookmark", () => {
    beforeEach(() => {
        cy.enableModules(["Medication", "Comment"]);
    });

    it("Redirect to UserProfile", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            profilePath
        );
        cy.url().should("include", profilePath);
    });

    it("Redirect to Insights", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            healthInsightsPath
        );
        cy.url().should("include", healthInsightsPath);
    });

    it("Redirect to home", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.url().should("include", homePath);
    });
});
