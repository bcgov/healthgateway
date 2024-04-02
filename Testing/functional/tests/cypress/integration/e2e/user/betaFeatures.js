const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";

describe("Beta Features", () => {
    beforeEach(() => {
        cy.configureSettings({});
    });

    it("Link to Beta Site on Home Page for Approved User", () => {
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
        cy.get("[data-testid=beta-alert]").should("be.visible");
    });

    it("No Link to Beta Site on Home Page for Unapproved User", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );
        cy.get("[data-testid=add-quick-link-button]").should("be.visible");
        cy.get("[data-testid=beta-alert]").should("not.exist");
    });
});
