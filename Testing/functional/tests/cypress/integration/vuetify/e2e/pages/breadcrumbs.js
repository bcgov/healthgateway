const { AuthMethod } = require("../../../../support/constants");

function testPageBreadcrumb(url, dataTestId) {
    cy.visit(url);
    cy.get("[data-testid=breadcrumbs]").should("be.visible");
    cy.get(`[data-testid='${dataTestId}'].v-breadcrumbs-item--active`).should(
        "be.visible"
    );
    cy.get("[data-testid=breadcrumb-home]").should("be.visible").click();
    cy.url().should("include", "/home");
}

describe("Breadcrumbs", () => {
    beforeEach("Breadcrumbs present when logged in", () => {
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
    });
    it("Breadcrumbs present on timeline", () =>
        testPageBreadcrumb("/timeline", "breadcrumb-timeline"));
    it("Breadcrumbs present on covid19", () =>
        testPageBreadcrumb("/covid19", "breadcrumb-covid-19"));
    it("Breadcrumbs present on dependents", () =>
        testPageBreadcrumb("/dependents", "breadcrumb-dependents"));
    it("Breadcrumbs present on reports", () =>
        testPageBreadcrumb("/reports", "breadcrumb-export-records"));
    it("Breadcrumbs present on profile", () =>
        testPageBreadcrumb("/profile", "breadcrumb-profile"));
    it("Breadcrumbs present on release notes", () =>
        testPageBreadcrumb("/release-notes", "breadcrumb-release-notes"));
    it("Breadcrumbs present on termsOfService", () =>
        testPageBreadcrumb("/termsOfService", "breadcrumb-terms-of-service"));
});
