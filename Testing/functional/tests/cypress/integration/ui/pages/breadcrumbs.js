import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const breadcrumbPages = [
    ["/timeline", "breadcrumb-timeline"],
    ["/dependents", "breadcrumb-dependents"],
    ["/reports", "breadcrumb-download-records"],
    ["/profile", "breadcrumb-profile"],
    ["/termsOfService", "breadcrumb-terms-of-service"],
];

function verifyBreadcrumb(path, activeBreadcrumbTestId, shouldVisit = true) {
    if (shouldVisit) {
        cy.visit(path);
    }

    cy.get("[data-testid=breadcrumbs]").should("be.visible");
    cy.get(
        `[data-testid=${activeBreadcrumbTestId}].v-breadcrumbs-item--active`
    ).should("be.visible");
    cy.get("[data-testid=breadcrumb-home]").should("be.visible").click();
    cy.location("pathname").should("eq", "/home");
}

describe("Breadcrumbs", () => {
    it("Breadcrumbs hidden when logged out", () => {
        cy.configureSettings({});
        cy.visit("/termsOfService");
        cy.get("[data-testid=breadcrumbs]", { timeout: 2500 }).should(
            "not.exist"
        );
    });

    it("Displays the active breadcrumb and navigates home", () => {
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

        const [firstPath, firstTestId] = breadcrumbPages[0];
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            firstPath
        );
        verifyBreadcrumb(firstPath, firstTestId, false);

        breadcrumbPages.slice(1).forEach(([path, testId]) => {
            verifyBreadcrumb(path, testId);
        });
    });
});
