import { useFakeEnvironment } from "../../../support/functions/fakeEnvironment";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();
const pageUrls = ["/termsOfService"];

describe("Breadcrumbs", () => {
    it("Breadcrumbs hidden when logged out", () => {
        for (const url of pageUrls) {
            cy.visit(url);
            cy.get("[data-testid=breadcrumbs]", { timeout: 2500 }).should(
                "not.exist"
            );
        }
    });
});
