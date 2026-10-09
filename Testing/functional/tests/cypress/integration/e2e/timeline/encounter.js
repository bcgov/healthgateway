import { AuthMethod } from "../../../support/constants";

describe("Encounter timeline integration", () => {
    it("Loads a Health Visit from the Encounter service", () => {
        cy.configureSettings({
            datasets: [{ name: "healthVisit", enabled: true }],
        });
        cy.intercept("GET", "**/Encounter/*").as("getEncounters");

        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/timeline"
                );
            }
        );
        cy.wait("@getEncounters", { timeout: 60000 });
        cy.checkTimelineHasLoaded();

        // Detailed card rendering is covered by fixture-backed UI tests. This
        // assertion verifies that real Encounter data reaches the timeline.
        cy.get("[data-testid=healthvisitTitle]").first().should("be.visible");
    });
});
