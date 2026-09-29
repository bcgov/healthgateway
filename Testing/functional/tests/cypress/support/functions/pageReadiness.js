import { waitForInitialDataLoad } from "./intercept";

// Preserve existing data readiness until individual scenarios declare their needs.
export function waitForScenarioData(username, settings, path) {
    return waitForInitialDataLoad(username, settings, path);
}

export function assertAuthenticatedPage(path) {
    // Account-state redirects are valid authenticated destinations too.
    const destinations = [
        new URL(path, Cypress.config("baseUrl")).pathname,
        "/registration",
        "/acceptTermsOfService",
        "/profile",
        "/patientRetrievalError",
        "/unauthorized",
    ];
    cy.location("origin").should(
        "eq",
        new URL(Cypress.config("baseUrl")).origin
    );
    cy.location("pathname", { timeout: 60000 }).should(
        "be.oneOf",
        destinations
    );
    // HeaderComponent renders this only when oidcIsAuthenticated is true.
    // Expected dialogs (e.g. Protective Word) may cover the authenticated header.
    cy.get("[data-testid=headerDropdownBtn]", { timeout: 60000 }).should(
        "exist"
    );
}
