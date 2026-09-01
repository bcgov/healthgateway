import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const breadcrumbPages = [
    ["/timeline", "breadcrumb-timeline"],
    ["/dependents", "breadcrumb-dependents"],
    ["/reports", "breadcrumb-download-records"],
    ["/profile", "breadcrumb-profile"],
    ["/termsOfService", "breadcrumb-terms-of-service"],
];

function verifyBreadcrumb(path, activeBreadcrumbTestId) {
    cy.visit(path);
    cy.get("[data-testid=breadcrumbs]").should("be.visible");
    cy.get(
        `[data-testid=${activeBreadcrumbTestId}].v-breadcrumbs-item--active`
    ).should("be.visible");
    cy.get("[data-testid=breadcrumb-home]").should("be.visible").click();
    cy.location("pathname").should("eq", "/home");
}

describe("Breadcrumbs", () => {
    beforeEach(() => {
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
        });
        setupStandardFixtures();
        cy.intercept("GET", "**/UserProfile/*/Dependent*", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.intercept("GET", "**/UserProfile/termsofservice*", {
            fixture: "UserProfileService/termsOfService.json",
        });
    });

    it("Breadcrumbs hidden when logged out", () => {
        cy.visit("/termsOfService");
        cy.get("[data-testid=breadcrumbs]", { timeout: 2500 }).should(
            "not.exist"
        );
    });

    it("Displays the active breadcrumb and navigates home", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );

        breadcrumbPages.forEach(([path, testId]) => {
            verifyBreadcrumb(path, testId);
        });
    });
});
