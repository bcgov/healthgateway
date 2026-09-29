import { AuthMethod, localDevUri } from "../support/constants";
import { setupStandardFixtures } from "../support/functions/intercept";

// Opt-in baseline for the deployed Keycloak path used by Azure Pipelines.
// Run with the existing Keycloak credentials supplied through Cypress env:
// npx cypress run --browser chrome --config specPattern=cypress/regression/keycloakSession.cy.js
// Add --env sessionBaseline=true only when collecting the temporary baseline report.
// This folder is outside the normal integration spec pattern.
describe("Keycloak session regression", { retries: 0 }, () => {
    it("authenticates once and restores the saved session for another destination", () => {
        expect(
            Cypress.config("baseUrl"),
            "use the deployed login path"
        ).not.to.eq(localDevUri);
        // Assert presence without including credentials in assertion output.
        expect(
            Boolean(Cypress.env("keycloak.username")) &&
                Boolean(Cypress.env("keycloak.password")),
            "Keycloak credentials are configured"
        ).to.eq(true);

        cy.then(() => Cypress.session.clearAllSavedSessions());
        cy.then(() => Cypress.session.clearCurrentSessionData());
        cy.configureSettings({});
        setupStandardFixtures();

        let credentialSubmissions = 0;
        let submissionsAfterLogin;
        let phase;
        let startedAt;
        const measurements = {};
        // Count explicit cy.visit calls only, not redirects or SPA route changes.
        // The test-scoped listener is removed automatically when the test ends.
        cy.on("command:start", (command) => {
            if (phase && command.attributes.name === "visit") {
                const path = new URL(
                    command.attributes.args[0],
                    Cypress.config("baseUrl")
                ).pathname;
                measurements[phase].visitPaths.push(path);
            }
        });

        function startMeasurement(name) {
            phase = name;
            measurements[name] = { visitPaths: [] };
            startedAt = performance.now();
        }

        function finishMeasurement(submissions) {
            measurements[phase].elapsedMs = Math.round(
                performance.now() - startedAt
            );
            measurements[phase].credentialSubmissions = submissions;
            phase = undefined;
        }
        cy.readConfig().then((config) => {
            cy.intercept(
                "POST",
                `${config.openIdConnect.authority}/login-actions/authenticate*`,
                () => {
                    // Observe only; do not stub authentication or inspect credentials.
                    credentialSubmissions += 1;
                }
            );
        });

        cy.then(() => startMeasurement("freshLogin"));
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile",
            "session-regression"
        );
        cy.location("pathname").should("eq", "/profile");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");
        cy.then(() => {
            expect(
                credentialSubmissions,
                "fresh login submits credentials"
            ).to.be.greaterThan(0);
            submissionsAfterLogin = credentialSubmissions;
            finishMeasurement(credentialSubmissions);
        });

        // Remove active browser authentication while retaining the saved session.
        // Both phases are in one test so this also works when run in isolation.
        cy.then(() => Cypress.session.clearCurrentSessionData());
        cy.then(() => startMeasurement("restoredSession"));
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home",
            "session-regression"
        );
        cy.location("pathname").should("eq", "/home");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");
        cy.then(() => {
            expect(
                credentialSubmissions,
                "restoring the session does not resubmit credentials"
            ).to.eq(submissionsAfterLogin);
            finishMeasurement(credentialSubmissions - submissionsAfterLogin);
        });
        // Temporary diagnostic output is opt-in; ordinary runs write no baseline.
        // Contains paths and counts only; never write credentials, cookies or tokens.
        if (String(Cypress.env("sessionBaseline")).toLowerCase() === "true") {
            cy.then(() => {
                return cy.writeFile(
                    "reports/keycloak-session-baseline.json",
                    measurements
                );
            });
        }
    });
});

// Seed authentication explicitly: neither test depends on another test passing.
describe(
    "Configuration isolation with a saved Keycloak session",
    { retries: 0 },
    () => {
        const sessionId = "configuration-isolation-regression";
        const notificationHeader =
            "[data-testid=profile-notification-preferences-label]";
        let credentialSubmissions = 0;

        function login() {
            cy.login(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                AuthMethod.KeyCloak,
                "/profile",
                sessionId
            );
            cy.location("pathname").should("eq", "/profile");
            cy.get("[data-testid=headerDropdownBtn]").should("exist");
        }

        before(() => {
            expect(
                Cypress.config("baseUrl"),
                "use the deployed login path"
            ).not.to.eq(localDevUri);
            expect(
                Boolean(Cypress.env("keycloak.password")),
                "Keycloak password is configured"
            ).to.eq(true);
            cy.then(() => Cypress.session.clearAllSavedSessions());
            cy.then(() => Cypress.session.clearCurrentSessionData());
            cy.configureSettings({
                profile: { notifications: { enabled: true } },
            });
            setupStandardFixtures();
            login();
            cy.get(notificationHeader).should("be.visible");
        });

        beforeEach(() => {
            credentialSubmissions = 0;
            setupStandardFixtures();
            cy.readConfig().then((config) => {
                cy.intercept(
                    "POST",
                    `${config.openIdConnect.authority}/login-actions/authenticate*`,
                    () => {
                        credentialSubmissions += 1;
                    }
                );
            });
        });

        [false, true].forEach((enabled) => {
            it(`uses this test's notification setting (${enabled}) when restoring authentication`, () => {
                cy.configureSettings({
                    profile: { notifications: { enabled } },
                });
                login();
                // Check rendered behavior, not merely the configuration object.
                cy.get(notificationHeader).should(
                    enabled ? "be.visible" : "not.exist"
                );
                cy.then(() => {
                    expect(
                        credentialSubmissions,
                        "the seeded session was reused"
                    ).to.eq(0);
                });
            });
        });
    }
);
