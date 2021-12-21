const { AuthMethod } = require("../../../support/constants");

describe("User Profile", () => {
    beforeEach(() => {
        cy.enableModules("Patient");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("PHN Visible and Correct", () => {
        cy.get("[data-testid=PHN]").should("be.visible").contains("9735353315");
    });
});
