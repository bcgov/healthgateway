const { AuthMethod } = require("../../../support/constants");

describe("Resource Centre", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Visible", () => {
        cy.visit("/dependents");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
        cy.visit("/reports");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });
});
