import { localDevUri } from "../support/constants";
import {
    assertAuthenticatedPage,
    ensureKeycloakSession,
} from "../support/functions/authentication";
import { setupStandardFixtures } from "../support/functions/intercept";
import { visitTestPage } from "../support/functions/navigation";
import { waitForInitialDataLoad } from "../support/functions/pageReadiness";

// Run only this pair together, in filename order, in one Cypress process:
// --config 'specPattern=cypress/regression/keycloakSessionAcrossSpecs*.cy.js'
// Spec 02 deliberately requires spec 01: a fresh login there must fail the test.
// This exercises the opt-in helper before ordinary cy.login callers enable it.
describe("Keycloak session reuse across specs", { retries: 0 }, () => {
    it("creates a shared session with notifications enabled", () => {
        expect(
            Cypress.config("baseUrl"),
            "use deployed authentication"
        ).not.to.eq(localDevUri);
        expect(
            Boolean(Cypress.env("keycloak.username")) &&
                Boolean(Cypress.env("keycloak.password")),
            "Keycloak credentials are configured"
        ).to.eq(true);
        cy.then(() => Cypress.session.clearAllSavedSessions());
        cy.then(() => Cypress.session.clearCurrentSessionData());
        cy.configureSettings({
            profile: { notifications: { enabled: true } },
        });
        setupStandardFixtures();

        let credentialSubmissions = 0;
        const visitPaths = [];
        cy.on("command:start", (command) => {
            if (command.attributes.name === "visit") {
                visitPaths.push(
                    new URL(
                        command.attributes.args[0],
                        Cypress.config("baseUrl")
                    ).pathname
                );
            }
        });
        // Read the overridden object shared by the support command via its alias.
        cy.get("@config").then((settings) => {
            const features = settings.webClient.featureToggleConfiguration;
            expect(
                features.notificationCentre.enabled,
                "notification centre is disabled for this scenario"
            ).to.eq(false);
            expect(
                features.profile.notifications.enabled,
                "profile notification preferences use this spec's setting"
            ).to.eq(true);
            cy.intercept(
                "POST",
                `${settings.openIdConnect.authority}/login-actions/authenticate*`,
                () => {
                    credentialSubmissions += 1;
                }
            );
            ensureKeycloakSession(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                settings,
                "cross-spec-session-regression",
                { cacheAcrossSpecs: true }
            );
            visitTestPage("/profile");
            waitForInitialDataLoad(
                Cypress.env("keycloak.username"),
                settings,
                "/profile"
            );
            assertAuthenticatedPage("/profile");
        });
        cy.location("pathname").should("eq", "/profile");
        cy.get("[data-testid=profile-notification-preferences-label]").should(
            "be.visible"
        );
        cy.then(() => {
            expect(credentialSubmissions, "credential submissions").to.eq(1);
            // Count explicit visits, not redirects or SPA navigation.
            expect(visitPaths, "application visits").to.deep.eq([
                "/login",
                "/profile",
            ]);
        });
    });
});
