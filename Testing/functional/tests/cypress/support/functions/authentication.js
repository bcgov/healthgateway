import { startLoginDiagnostics } from "./loginDiagnostics";
import { assertAuthenticatedPage } from "./pageReadiness";

function loginWithApplicationKeycloak(username, password, config, path) {
    // Let keycloak-js create and consume its own state, nonce and PKCE verifier.
    const diagnostics = startLoginDiagnostics();
    cy.then(() => diagnostics.stage("opening-hg-login"));
    cy.visit(`/login?redirect=${encodeURIComponent(path)}`);
    cy.get("#KeyCloakBtn")
        .should("be.visible")
        .and("not.be.disabled")
        .then(() => {
            diagnostics.stage("leaving-hg-for-keycloak");
        })
        .click();
    cy.origin(
        new URL(config.openIdConnect.authority).origin,
        { args: { username, password } },
        ({ username, password }) => {
            cy.get("#username").should("be.visible").clear().type(username);
            cy.get("#password").should("be.visible").clear().type(password, {
                log: false,
            });
            cy.get("#kc-login").click();
        }
    );
    cy.then(() => diagnostics.stage("keycloak-commands-completed"));
    assertAuthenticatedPage(path);
    cy.then(() => diagnostics.stage("callback-completed"));
}

// Session management owns the neutral authentication route, never the test destination.
export function ensureKeycloakSession(username, password, settings, sessionId) {
    let authenticatedDuringSetup = false;
    cy.session(
        [
            "keycloak-ui",
            Cypress.config("baseUrl"),
            settings.openIdConnect.authority,
            settings.openIdConnect.clientId,
            username,
            sessionId,
        ],
        () => {
            loginWithApplicationKeycloak(
                username,
                password,
                settings,
                "/profile"
            );
            authenticatedDuringSetup = true;
        },
        {
            validate() {
                // Setup already opened and authenticated this page.
                // Restored sessions still need a real SSO check.
                if (authenticatedDuringSetup) {
                    // Cypress reuses these callbacks on restoration.
                    authenticatedDuringSetup = false;
                } else {
                    cy.visit("/profile");
                }
                assertAuthenticatedPage("/profile");
            },
        }
    );
}
