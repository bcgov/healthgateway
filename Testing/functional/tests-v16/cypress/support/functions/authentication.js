import { startLoginDiagnostics } from "./loginDiagnostics";

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
// Cross-spec reuse is opt-in; account-changing and authentication tests stay scoped
// to their spec unless their caller explicitly chooses otherwise.
export function ensureKeycloakSession(
    username,
    password,
    settings,
    sessionId,
    { cacheAcrossSpecs = false } = {}
) {
    let authenticatedDuringSetup = false;
    cy.session(
        [
            "keycloak-ui",
            Cypress.config("baseUrl"),
            settings.openIdConnect.authority,
            settings.openIdConnect.clientId,
            username,
            sessionId,
            // Keep shared and spec-scoped entries distinct for the same user/ID.
            cacheAcrossSpecs ? "run" : "spec",
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
            cacheAcrossSpecs,
            validate() {
                // Setup already opened and authenticated this page.
                // Restored sessions are checked with Keycloak without loading the app.
                if (authenticatedDuringSetup) {
                    // Cypress reuses these callbacks on restoration.
                    authenticatedDuringSetup = false;
                    assertAuthenticatedPage("/profile");
                    return;
                }
                return validateKeycloakSession(settings);
            },
        }
    );
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

export function validateKeycloakSession(config) {
    const oidc = config.openIdConnect;
    const callback =
        oidc.callbacks?.Logon || `${Cypress.config("baseUrl")}/loginCallback`;
    const state = crypto.randomUUID();

    // Use S256 PKCE without persisting a verifier or redeeming the returned code.
    return cy
        .then(async () => {
            const verifier = crypto.randomUUID() + crypto.randomUUID();
            const digest = await crypto.subtle.digest(
                "SHA-256",
                new TextEncoder().encode(verifier)
            );
            return btoa(String.fromCharCode(...new Uint8Array(digest)))
                .replaceAll("+", "-")
                .replaceAll("/", "_")
                .replaceAll("=", "");
        })
        .then((challenge) => {
            return cy.request({
                url: `${oidc.authority}/protocol/openid-connect/auth`,
                qs: {
                    client_id: oidc.clientId,
                    redirect_uri: callback,
                    response_type: "code",
                    response_mode: "query",
                    scope: oidc.scope,
                    prompt: "none",
                    state,
                    code_challenge: challenge,
                    code_challenge_method: "S256",
                },
                followRedirect: false,
                failOnStatusCode: false,
                log: false,
            });
        })
        .then((response) => {
            expect(response.status, "authorization response").to.eq(302);
            // Never log a Location header or authorization code.
            expect(Boolean(response.headers.location), "redirect exists").to.eq(
                true
            );
            const redirect = new URL(response.headers.location);
            const expected = new URL(callback);
            expect(redirect.origin, "callback origin").to.eq(expected.origin);
            expect(redirect.pathname, "callback path").to.eq(expected.pathname);
            expect(
                redirect.searchParams.get("state") === state,
                "state matches"
            ).to.eq(true);
            const error = redirect.searchParams.get("error");
            const hasCode = Boolean(redirect.searchParams.get("code"));
            if (error === "login_required" && !hasCode) {
                // Yielding false tells cy.session to recreate an invalid session.
                return false;
            }
            expect(error, "authorization error").to.eq(null);
            expect(hasCode, "authorization code present").to.eq(true);
            return true;
        });
}
