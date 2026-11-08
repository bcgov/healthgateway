import { useFakeEnvironment } from "../../../support/functions/fakeEnvironment";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Error Pages", () => {
    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });

    it("HTTP 404", () => {
        cy.visit("/nonexistentpage");
        cy.contains("h1", "404");
    });
});
