const { AuthMethod } = require("../../../support/constants");

function testPageBreadcrumb(url, dataTestId) {
    cy.visit(url);
    cy.get("[data-testid=breadcrumbs]").should("be.visible");
    cy.get(`[data-testid='${dataTestId}'].active`).should("be.visible");
    cy.get("[data-testid=breadcrumb-home]").should("be.visible").click();
    cy.url().should("include", "/home");
}

describe("Breadcrumbs", () => {
    it("Breadcrumbs present when logged in", () => {
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );

        testPageBreadcrumb("/timeline", "breadcrumb-timeline");
        testPageBreadcrumb("/covid19", "breadcrumb-covid-19");
        testPageBreadcrumb("/dependents", "breadcrumb-dependents");
        testPageBreadcrumb("/reports", "breadcrumb-export-records");
        testPageBreadcrumb("/profile", "breadcrumb-profile");
        testPageBreadcrumb("/release-notes", "breadcrumb-release-notes");
        testPageBreadcrumb("/termsOfService", "breadcrumb-terms-of-service");
    });
});
