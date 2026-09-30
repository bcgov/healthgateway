import { setupStandardAliases } from "./intercept";

// Authentication can consume requests; aliases belong to the destination visit.
export function visitTestPage(path) {
    setupStandardAliases();
    cy.log(`Visit path: ${path}`);
    return cy.visit(path, { timeout: 60000 });
}
