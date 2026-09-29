import { AuthMethod, localDevUri } from "../support/constants";
import { validateKeycloakSession } from "../support/functions/authentication";
import { setupStandardFixtures } from "../support/functions/intercept";

// Exercise the same HTTP validation used by restored production test sessions.
// Run with existing credentials and:
// --config specPattern=cypress/regression/keycloakSessionValidation.cy.js
function probeSession(config, authenticated) {
    return validateKeycloakSession(config).should("eq", authenticated);
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

        it("rejects and recreates a revoked cached session", () => {
            let idToken;
            let savedCookies;
            let configuration;
            let credentialSubmissions = 0;
            expect(
                Boolean(Cypress.env("keycloak.password")),
                "Keycloak password is configured"
            ).to.eq(true);
            cy.configureSettings({});
            setupStandardFixtures();
            cy.readConfig().then((config) => {
                configuration = config;
                cy.intercept(
                    "POST",
                    `${config.openIdConnect.authority}/login-actions/authenticate*`,
                    () => {
                        credentialSubmissions += 1;
                    }
                );
                cy.intercept(
                    "POST",
                    `${config.openIdConnect.authority}/protocol/openid-connect/token`,
                    (request) => {
                        request.on("response", (response) => {
                            // Keep the token in memory only, for this session's logout.
                            if (response.body?.id_token) {
                                idToken = response.body.id_token;
                            }
                        });
                    }
                );
            });
            cy.login(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                AuthMethod.KeyCloak,
                "/profile",
                "revoked-session-regression"
            );
            cy.then(() => {
                expect(credentialSubmissions, "initial login").to.eq(1);
            });
            cy.then(() => probeSession(configuration, true));
            cy.getAllCookies({ log: false }).then((cookies) => {
                const host = new URL(configuration.openIdConnect.authority)
                    .hostname;
                savedCookies = cookies.filter((cookie) => {
                    const domain = cookie.domain.replace(/^\./, "");
                    return host === domain || host.endsWith(`.${domain}`);
                });
                expect(
                    savedCookies.length,
                    "identity provider cookies were captured"
                ).to.be.greaterThan(0);
            });
            cy.then(() => {
                expect(
                    Boolean(idToken),
                    "ID token captured for session logout"
                ).to.eq(true);
                return cy
                    .request({
                        method: "POST",
                        url: `${configuration.openIdConnect.authority}/protocol/openid-connect/logout`,
                        form: true,
                        body: {
                            client_id: configuration.openIdConnect.clientId,
                            id_token_hint: idToken,
                        },
                        followRedirect: false,
                        failOnStatusCode: false,
                        log: false,
                    })
                    .then((response) => {
                        expect(response.status, "logout response").to.be.oneOf([
                            200, 204, 302, 303,
                        ]);
                        idToken = undefined;
                    });
            });
            // Replay the original identity-provider cookies, as a stale cache would.
            // This must fail because the server session is gone, not cookies missing.
            cy.clearAllCookies({ log: false });
            cy.then(() => {
                savedCookies.forEach((cookie) => {
                    cy.setCookie(cookie.name, cookie.value, {
                        domain: cookie.domain,
                        path: cookie.path,
                        secure: cookie.secure,
                        httpOnly: cookie.httpOnly,
                        sameSite: cookie.sameSite,
                        expiry: cookie.expiry,
                        hostOnly: cookie.hostOnly,
                        log: false,
                    });
                });
            });
            cy.getAllCookies({ log: false }).then((cookies) => {
                expect(
                    savedCookies.every((saved) =>
                        cookies.some(
                            (cookie) =>
                                cookie.name === saved.name &&
                                cookie.domain === saved.domain &&
                                cookie.path === saved.path &&
                                cookie.value === saved.value
                        )
                    ),
                    "old cookies were restored"
                ).to.eq(true);
            });
            cy.then(() => probeSession(configuration, false));

            // Keep the saved Cypress session and its ID: login must detect that
            // the cached server session was revoked and recreate it automatically.
            cy.login(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password"),
                AuthMethod.KeyCloak,
                "/home",
                "revoked-session-regression"
            );
            cy.location("pathname").should("eq", "/home");
            cy.get("[data-testid=headerDropdownBtn]").should("exist");
            cy.then(() => {
                expect(credentialSubmissions, "one recovery login").to.eq(2);
            });
            cy.then(() => probeSession(configuration, true));
        });
    }
);
