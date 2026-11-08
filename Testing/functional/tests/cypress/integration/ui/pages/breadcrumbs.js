import { useFakeEnvironment } from "../../../support/functions/fakeEnvironment";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Breadcrumbs", () => {
    it("Breadcrumbs hidden when logged out", () => {
        // The page loads its content on mount; waiting for it means the breadcrumb check runs
        // against the rendered page, not before the app has mounted.
        cy.intercept("GET", "**/UserProfile/termsofservice?api-version=2.0", {
            fixture: "UserProfileService/termsOfService.json",
        }).as("getTermsOfService");

        cy.visit("/termsOfService");
        cy.wait("@getTermsOfService");

        cy.get("[data-testid=breadcrumbs]").should("not.exist");
    });
});
