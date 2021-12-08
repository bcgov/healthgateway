const { AuthMethod } = require("../../../support/constants");

describe("Health Insights", () => {
    before(() => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/healthInsights"
        );
    });

    it("Validate medication records count.", () => {
        cy.get("[data-testid=totalRecordsText]").should("not.contain", "0 ");
    });
});
