/*
----------------------------------------------------------------------------------------------------
Fake environment

Runs the SPA with nothing behind it: configuration comes from a fixture, Keycloak is faked
(fakeKeycloak.js), and every API call must be answered by a test intercept. Anything else the
SPA tries to reach is refused with a 501 and fails the test, so a spec using this verifies the
SPA alone.

The fixture's Keycloak realm and service endpoints are moved onto the app's own origin, under
/__fake-keycloak and /__fake-api, so the spec works against any server that hosts the SPA
(Vite dev server, vite preview or a deployed environment) and needs no cross-origin handling.

Usage: setupFakeEnvironment();
Usage: setupFakeEnvironment({ configure: (config) => { config.webClient.... = ...; } });
Then register API intercepts (e.g. setupStandardFixtures()) and visit the app.

Converting a spec that uses cy.configureSettings and cy.login:
    useFakeEnvironment();                       // once, at the top of the spec file
    configureFakeSettings({ datasets: [...] }); // instead of cy.configureSettings(...)
    fakeLogin("/timeline");                     // instead of cy.login(..., "/timeline")

Import these into the spec (or a helper it imports) rather than wrapping them as cy commands:
support files and specs are bundled separately, and each bundle gets its own copy of the state.
----------------------------------------------------------------------------------------------------
*/
import { assertAuthenticatedPage } from "./authentication";
import {
    escapeRegExp,
    FakeUser,
    installFakeKeycloak,
    peekFakeKeycloakState,
    signInToFakeKeycloak,
} from "./fakeKeycloak";
import { visitTestPage } from "./navigation";
import { waitForInitialDataLoad } from "./pageReadiness";
import { applyFeatureOverrides } from "./testConfiguration";

const fakeKeycloakPath = "/__fake-keycloak";
const fakeApiPath = "/__fake-api";
const realmName = "health-gateway-gold";

let unhandledRequests;
// The fixture configuration after setupFakeEnvironment's changes, before any feature overrides.
let baseConfig;
// The configuration the SPA is currently served.
let currentConfig;

function appOrigin() {
    return new URL(Cypress.config("baseUrl")).origin;
}

function refuse(req, reason) {
    unhandledRequests.push(`${req.method} ${req.url} (${reason})`);
    req.reply(501, `Not stubbed by the test: ${req.method} ${req.url}`);
}

function installRequestGuard(origin) {
    const blockedHosts = [Cypress.config("blockHosts") ?? []].flat();

    // Registered before any stub, so every stub takes precedence over these.
    cy.intercept(
        { url: new RegExp(`^${escapeRegExp(origin)}${fakeApiPath}/`) },
        (req) => refuse(req, "no API stub")
    );
    // Only off-origin requests, so the SPA's own assets are not routed through the driver.
    cy.intercept(
        { url: new RegExp(`^(?!${escapeRegExp(origin)}/)https?://`) },
        (req) => {
            const host = new URL(req.url).hostname;
            if (
                blockedHosts.some((blocked) => Cypress.minimatch(host, blocked))
            ) {
                req.continue();
                return;
            }
            refuse(req, "external host");
        }
    );

    // Every page loads the banner (type 0) and in-app (type 2) communications. Unless a test
    // stubs them, answer as the API does when none is active.
    cy.intercept(
        {
            method: "GET",
            url: new RegExp(
                `^${escapeRegExp(origin)}${fakeApiPath}/[^/]+/Communication/\\d+$`
            ),
        },
        {
            statusCode: 200,
            body: {
                resourcePayload: null,
                totalResultCount: 0,
                pageIndex: null,
                pageSize: null,
                resultStatus: 1,
                resultError: null,
            },
        }
    );

    // Snowplow analytics tracker (loaded by public/snowplow.js). An empty script leaves its
    // command queue in place, so tracking calls are accepted and dropped.
    cy.intercept(
        {
            method: "GET",
            hostname: "www2.gov.bc.ca",
            pathname: /\/sp\/sp-.*\.js$/,
        },
        {
            statusCode: 200,
            headers: { "content-type": "application/javascript" },
            body: "",
        }
    );
}

function rewriteConfiguration(config, origin) {
    config.openIdConnect.authority = `${origin}${fakeKeycloakPath}/realms/${realmName}`;
    config.openIdConnect.callbacks = null;
    for (const [name, endpoint] of Object.entries(config.serviceEndpoints)) {
        const service = new URL(endpoint).pathname.replaceAll("/", "");
        config.serviceEndpoints[name] = `${origin}${fakeApiPath}/${service}/`;
    }
    return config;
}

export function setupFakeEnvironment({ configure } = {}) {
    const origin = appOrigin();
    unhandledRequests = [];
    installRequestGuard(origin);

    return cy
        .fixture("WebClient/configuration.json")
        .then((fixture) => {
            const config = rewriteConfiguration(
                structuredClone(fixture),
                origin
            );
            configure?.(config);
            baseConfig = config;
            serveConfiguration(structuredClone(config));

            installFakeKeycloak({
                realmUrl: config.openIdConnect.authority,
                clientId: config.openIdConnect.clientId,
            });

            return cy.wrap(config, { log: false });
        })
        .as("config");
}

function serveConfiguration(config) {
    currentConfig = config;
    // Any host: some local setups point the SPA at a separate config server.
    cy.intercept(
        { method: "GET", pathname: "/configuration" },
        { statusCode: 200, body: config }
    ).as("getConfiguration");
}

/** Sets up the fake environment before each test in the spec (or describe) it is called from. */
export function useFakeEnvironment(options) {
    beforeEach(() => {
        setupFakeEnvironment(options);
    });
}

/**
 * The fake environment's cy.configureSettings: the same feature override rules, applied to the
 * fixture configuration rather than the live one. Yields the configuration as @config.
 */
export function configureFakeSettings(overriddenFeatures = {}) {
    return cy
        .then(() => {
            if (!baseConfig) {
                throw new Error("setupFakeEnvironment() has not been called.");
            }
            const config = applyFeatureOverrides(
                structuredClone(baseConfig),
                structuredClone(overriddenFeatures)
            );
            serveConfiguration(config);
            return cy.wrap(config, { log: false });
        })
        .as("config");
}

/**
 * The fake environment's cy.login: gives the user a fake Keycloak session, opens the page and
 * waits for the same initial data loads as cy.login.
 */
export function fakeLogin(path = "/timeline", { user = FakeUser.Bcsc } = {}) {
    signInToFakeKeycloak(user);
    visitTestPage(path);
    cy.then(() => {
        waitForInitialDataLoad(user.preferred_username, currentConfig, path);
    });
    assertAuthenticatedPage(path);
}

// Fail any test that let the SPA reach something the test did not stub. Only checked when the
// test otherwise passed, so it never hides the original failure. A failing afterEach makes
// Cypress skip the rest of the spec, which is acceptable for a test that escaped its sandbox.
afterEach(function () {
    if (unhandledRequests === undefined) {
        return;
    }
    const unhandled = [
        ...unhandledRequests,
        ...(peekFakeKeycloakState()?.unexpectedRequests ?? []).map(
            (request) => `${request} (not implemented by fake Keycloak)`
        ),
    ];
    unhandledRequests = undefined;
    if (this.currentTest.state === "passed") {
        expect(unhandled, "requests the test did not stub").to.deep.equal([]);
    }
});
