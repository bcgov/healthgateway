const { AuthMethod } = require("../../../support/constants");

describe("Resource Centre", () => {
    beforeEach(() => {
        cy.restoreAuthCookies();
    });
    before(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Visible on HealthInsights", () => {
        cy.visit("/healthInsights");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });

    it("Validate Visible on Dependents", () => {
        cy.visit("/dependents");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });

    it("Validate Visible on Reports", () => {
        cy.visit("/reports");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });
});
