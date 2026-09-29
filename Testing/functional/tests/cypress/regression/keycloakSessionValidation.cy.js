import { AuthMethod, localDevUri } from "../support/constants";
import { setupStandardFixtures } from "../support/functions/intercept";

// Compatibility probe only: production session validation is unchanged.
// Run with existing credentials and:
// --config specPattern=cypress/regression/keycloakSessionValidation.cy.js
function probeSession(config, authenticated) {
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
            expect(
                Boolean(redirect.searchParams.get("code")),
                "authorization code present"
            ).to.eq(authenticated);
            expect(
                redirect.searchParams.get("error"),
                "authorization error"
            ).to.eq(authenticated ? null : "login_required");
        });
}

describe(
    "HTTP Keycloak session validation compatibility",
    { retries: 0 },
    () => {
        beforeEach(() => {
            expect(
                Cypress.config("baseUrl"),
                "use deployed authentication"
            ).not.to.eq(localDevUri);
            cy.then(() => Cypress.session.clearAllSavedSessions());
            cy.then(() => Cypress.session.clearCurrentSessionData());
        });

        it("rejects a browser without authentication cookies", () => {
            cy.readConfig().then((config) => probeSession(config, false));
        });

        it("recognizes a real login without navigating to the callback", () => {
            expect(
                Boolean(Cypress.env("keycloak.password")),
                "Keycloak password is configured"
            ).to.eq(true);
            cy.configureSettings({});
            setupStandardFixtures();
            cy.login(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                AuthMethod.KeyCloak,
                "/profile",
                "http-validation-regression"
            );
            cy.readConfig().then((config) => probeSession(config, true));
            cy.location("pathname").should("eq", "/profile");
            cy.get("[data-testid=headerDropdownBtn]").should("exist");
        });
    }
);
