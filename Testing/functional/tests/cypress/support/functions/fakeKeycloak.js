/*
----------------------------------------------------------------------------------------------------
Fake Keycloak

Stands in for the Keycloak realm so the SPA can sign in without leaving the test. It answers the
OIDC endpoints keycloak-js calls (auth, token, userinfo, logout, the session status iframe and the
third-party cookie check)
with cy.intercept, and issues unsigned JWTs. keycloak-js decodes tokens but does not verify
their signatures, so the SPA accepts them. It does check the nonce, which is echoed back here.

The realm lives on the app's own origin (see fakeEnvironment.js), so the login redirects never
leave it and no cy.origin is needed.

Usage: installFakeKeycloak({ realmUrl, clientId });
Usage: signInToFakeKeycloak(FakeUser.Bcsc);   // as if the user already has a Keycloak session
Usage: fakeKeycloakState().authRequests        // inspect what the SPA sent
Usage: blockThirdPartyCookiesInFakeKeycloak(); // as Safari, Firefox strict or Brave would
----------------------------------------------------------------------------------------------------
*/

const defaultHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const tokenLifetimeSeconds = 3600;

export const FakeUser = {
    // Initials "DG", as the specs expect from the real default test account.
    Bcsc: {
        sub: "fake-bcsc-user",
        preferred_username: "fake-bcsc-user",
        hdid: defaultHdid,
        idp: "BCSC",
        given_name: "Dev",
        family_name: "Gateway",
        name: "Dev Gateway",
        email: "fakeemail@healthgateway.gov.bc.ca",
        email_verified: "true",
    },
    // Matches UserProfileService/userProfileAcceptTos.json.
    AcceptTermsOfService: {
        sub: "fake-accept-tos-user",
        preferred_username: "fake-accept-tos-user",
        hdid: "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A",
        idp: "BCSC",
        given_name: "Fake",
        family_name: "Tos",
        name: "Fake Tos",
        email: "fakeemail@healthgateway.gov.bc.ca",
        email_verified: "true",
    },
    // Matches PatientService/patientUnregistered.json and the unregistered profile fixtures.
    // Like the real unregistered test account, it has no email, so registration starts blank.
    Unregistered: {
        sub: "fake-unregistered-user",
        preferred_username: "fake-unregistered-user",
        hdid: "S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA",
        idp: "BCSC",
        given_name: "Fake",
        family_name: "Unregistered",
        name: "Fake Unregistered",
    },
    Idir: {
        sub: "fake-idir-user",
        preferred_username: "fake-idir-user",
        idp: "IDIR",
        given_name: "Fake",
        family_name: "Idir",
        name: "Fake Idir",
        email: "fakeidir@gov.bc.ca",
        email_verified: "true",
    },
};

let state;

function resetState(realmUrl, clientId) {
    state = {
        realmUrl,
        clientId,
        // The user Keycloak will authenticate on an interactive login.
        user: FakeUser.Bcsc,
        // Whether Keycloak holds a session (answers prompt=none with a code).
        signedIn: false,
        sessionId: undefined,
        codes: new Map(),
        accessTokens: new Set(),
        refreshTokens: new Set(),
        authRequests: [],
        tokenRequests: [],
        userInfoRequests: [],
        logoutRequests: [],
        loginStatusIframeLoads: 0,
        unexpectedRequests: [],
        // The browser blocks third-party cookies: the cookie check says so, and requests from an
        // iframe do not carry Keycloak's session, so prompt=none in an iframe finds none.
        thirdPartyCookiesBlocked: false,
        // prompt=none answers lose their state and code, as if the app's URL lost its fragment.
        dropSilentCallbacks: false,
    };
}

/** Read inside cy.then(); the fake is installed from the command queue. */
export function fakeKeycloakState() {
    if (!state) {
        throw new Error("installFakeKeycloak() has not been called.");
    }
    return state;
}

/** The current state, or undefined if the fake was never installed. */
export function peekFakeKeycloakState() {
    return state;
}

// Queued, so these run after installFakeKeycloak() even when called in the same test body.

/** Gives Keycloak a session for the user, as if they had signed in earlier. */
export function signInToFakeKeycloak(user = FakeUser.Bcsc) {
    return cy.then(() => {
        const s = fakeKeycloakState();
        s.user = user;
        s.signedIn = true;
        s.sessionId = randomId();
    });
}

/** Makes the browser block third-party cookies, as Safari, Firefox strict or Brave would. */
export function blockThirdPartyCookiesInFakeKeycloak() {
    return cy.then(() => {
        fakeKeycloakState().thirdPartyCookiesBlocked = true;
    });
}

/** Makes prompt=none answers come back to the app without their callback parameters. */
export function dropFakeKeycloakSilentCallbacks() {
    return cy.then(() => {
        fakeKeycloakState().dropSilentCallbacks = true;
    });
}

/** Sets the user an interactive login will authenticate, without starting a session. */
export function setFakeKeycloakUser(user) {
    return cy.then(() => {
        fakeKeycloakState().user = user;
    });
}

export function escapeRegExp(value) {
    return value.replaceAll(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function randomId() {
    return crypto.randomUUID();
}

function base64Url(value) {
    const bytes = new TextEncoder().encode(JSON.stringify(value));
    return btoa(String.fromCharCode(...bytes))
        .replaceAll("+", "-")
        .replaceAll("/", "_")
        .replaceAll("=", "");
}

function createJwt(claims) {
    // keycloak-js never verifies the signature, so a placeholder is enough.
    return `${base64Url({ alg: "none", typ: "JWT" })}.${base64Url(claims)}.fake-signature`;
}

function issueTokens(nonce) {
    const s = state;
    const now = Math.floor(Date.now() / 1000);
    const common = {
        iss: s.realmUrl,
        sub: s.user.sub,
        azp: s.clientId,
        sid: s.sessionId,
        session_state: s.sessionId,
        iat: now,
        auth_time: now,
        exp: now + tokenLifetimeSeconds,
        ...(nonce ? { nonce } : {}),
    };
    const accessToken = createJwt({
        ...common,
        jti: randomId(),
        typ: "Bearer",
        aud: s.clientId,
        scope: "openid profile web-origins email",
        hdid: s.user.hdid,
        idp: s.user.idp,
    });
    const idToken = createJwt({
        ...common,
        jti: randomId(),
        typ: "ID",
        aud: s.clientId,
        ...s.user,
    });
    const refreshToken = createJwt({
        ...common,
        jti: randomId(),
        typ: "Refresh",
        aud: s.realmUrl,
        exp: now + 2 * tokenLifetimeSeconds,
    });

    s.accessTokens.add(accessToken);
    s.refreshTokens.add(refreshToken);

    return {
        access_token: accessToken,
        id_token: idToken,
        refresh_token: refreshToken,
        token_type: "Bearer",
        expires_in: tokenLifetimeSeconds,
        refresh_expires_in: 2 * tokenLifetimeSeconds,
        session_state: s.sessionId,
        scope: "openid profile web-origins email",
    };
}

function redirectWithParams(req, redirectUri, responseMode, params) {
    const target = new URL(redirectUri);
    const encoded = new URLSearchParams(params).toString();
    if (responseMode === "query") {
        for (const [key, value] of Object.entries(params)) {
            target.searchParams.set(key, value);
        }
    } else {
        target.hash = encoded;
    }
    req.redirect(target.toString(), 302);
}

function handleAuth(req) {
    const s = state;
    const query = new URL(req.url).searchParams;
    const request = {
        clientId: query.get("client_id"),
        redirectUri: query.get("redirect_uri"),
        responseType: query.get("response_type"),
        responseMode: query.get("response_mode") ?? "fragment",
        scope: query.get("scope"),
        state: query.get("state"),
        nonce: query.get("nonce"),
        prompt: query.get("prompt"),
        idpHint: query.get("kc_idp_hint"),
        codeChallenge: query.get("code_challenge"),
        codeChallengeMethod: query.get("code_challenge_method"),
    };
    // keycloak-js's hidden check-sso iframe answers into silent-check-sso.html; anything else is a
    // page. (Sec-Fetch-Dest cannot tell them apart: Cypress runs the whole app in an iframe.)
    request.silentIframe =
        request.prompt === "none" &&
        URL.canParse(request.redirectUri) &&
        new URL(request.redirectUri).pathname === "/silent-check-sso.html";
    s.authRequests.push(request);

    const problem =
        (request.clientId !== s.clientId && "unknown client_id") ||
        (request.responseType !== "code" && "unsupported response_type") ||
        (!request.state && "missing state") ||
        (!request.redirectUri && "missing redirect_uri") ||
        (new URL(request.redirectUri).origin !== new URL(s.realmUrl).origin &&
            "redirect_uri is not on the app origin");
    if (problem) {
        // Keycloak shows an error page rather than redirecting back.
        req.reply(400, `Fake Keycloak rejected the auth request: ${problem}`);
        return;
    }

    if (request.prompt === "none" && s.dropSilentCallbacks) {
        req.redirect(request.redirectUri, 302);
        return;
    }

    const sessionVisible =
        s.signedIn && !(s.thirdPartyCookiesBlocked && request.silentIframe);
    if (request.prompt === "none" && !sessionVisible) {
        redirectWithParams(req, request.redirectUri, request.responseMode, {
            error: "login_required",
            state: request.state,
        });
        return;
    }

    // Anything other than prompt=none is an interactive login; the user "enters" credentials.
    if (!s.signedIn) {
        s.signedIn = true;
        s.sessionId = randomId();
    }

    const code = randomId();
    s.codes.set(code, {
        nonce: request.nonce,
        redirectUri: request.redirectUri,
        codeChallenge: request.codeChallenge,
    });
    redirectWithParams(req, request.redirectUri, request.responseMode, {
        state: request.state,
        session_state: s.sessionId,
        code,
    });
}

function handleToken(req) {
    const s = state;
    const body = new URLSearchParams(req.body);
    const grantType = body.get("grant_type");
    s.tokenRequests.push({
        grantType,
        clientId: body.get("client_id"),
        redirectUri: body.get("redirect_uri"),
        hasCodeVerifier: Boolean(body.get("code_verifier")),
    });

    const invalidGrant = (description) =>
        req.reply(400, {
            error: "invalid_grant",
            error_description: description,
        });

    if (grantType === "authorization_code") {
        const issued = s.codes.get(body.get("code"));
        s.codes.delete(body.get("code"));
        if (!issued) {
            invalidGrant("Code not valid");
        } else if (issued.redirectUri !== body.get("redirect_uri")) {
            invalidGrant("Incorrect redirect_uri");
        } else if (issued.codeChallenge && !body.get("code_verifier")) {
            invalidGrant("PKCE verification failed");
        } else {
            req.reply(200, issueTokens(issued.nonce));
        }
        return;
    }

    if (grantType === "refresh_token") {
        if (!s.signedIn || !s.refreshTokens.has(body.get("refresh_token"))) {
            invalidGrant("Session not active");
        } else {
            req.reply(200, issueTokens());
        }
        return;
    }

    req.reply(400, { error: "unsupported_grant_type" });
}

function handleUserInfo(req) {
    const s = state;
    const token = (req.headers.authorization ?? "").replace(/^Bearer /i, "");
    const authorized = s.signedIn && s.accessTokens.has(token);
    s.userInfoRequests.push({ authorized });
    if (!authorized) {
        req.reply(401, { error: "invalid_token" });
        return;
    }
    req.reply(200, s.user);
}

function handleLogout(req) {
    const s = state;
    const query = new URL(req.url).searchParams;
    const request = {
        clientId: query.get("client_id"),
        postLogoutRedirectUri: query.get("post_logout_redirect_uri"),
        hasIdTokenHint: Boolean(query.get("id_token_hint")),
    };
    s.logoutRequests.push(request);
    s.signedIn = false;
    s.sessionId = undefined;
    s.accessTokens.clear();
    s.refreshTokens.clear();

    if (request.postLogoutRedirectUri) {
        req.redirect(request.postLogoutRedirectUri, 302);
    } else {
        req.reply(200, "You are logged out");
    }
}

// keycloak-js loads this in a hidden iframe when silent check-sso or the session status iframe is
// configured. Keycloak's real check (step1.html, then step2.html) sets and reads a cookie.
function thirdPartyCookiesPage(blocked) {
    const answer = blocked ? "unsupported" : "supported";
    return `<!doctype html><html><body><script>
parent.postMessage("${answer}", "*");
</script></body></html>`;
}

// keycloak-js's session status iframe (checkLoginIframe). The SPA posts "<clientId> <sessionId>";
// like Keycloak's, this answers "unchanged" when that matches the realm's session (both empty
// when nobody is signed in) and "changed" otherwise. The session is captured when the iframe
// loads; the fake only changes it through redirects or test helpers, which reload the page.
function handleLoginStatusIframe(req) {
    const s = state;
    s.loginStatusIframeLoads += 1;
    const config = JSON.stringify({
        clientId: s.clientId,
        sessionId: s.signedIn ? s.sessionId : "",
    });
    req.reply({
        statusCode: 200,
        headers: { "content-type": "text/html" },
        body: `<!doctype html><html><body><script>
const realm = ${config};
window.addEventListener("message", (event) => {
    const [clientId, sessionId = ""] = String(event.data).split(" ");
    const result =
        clientId !== realm.clientId
            ? "error"
            : sessionId === realm.sessionId
              ? "unchanged"
              : "changed";
    event.source.postMessage(result, event.origin);
});
</script></body></html>`,
    });
}

/**
 * Registers the fake realm's endpoints. Call before visiting the app, and before any other
 * intercepts that should take precedence.
 */
export function installFakeKeycloak({ realmUrl, clientId }) {
    resetState(realmUrl, clientId);
    const realm = new URL(realmUrl);
    const oidc = `${realm.pathname}/protocol/openid-connect`;
    const endpoint = (method, path) => ({
        method,
        hostname: realm.hostname,
        port: Number(realm.port || (realm.protocol === "https:" ? 443 : 80)),
        pathname: `${oidc}/${path}`,
    });

    // Registered first so the specific endpoints below take precedence.
    cy.intercept({ url: new RegExp(`^${escapeRegExp(realmUrl)}/`) }, (req) => {
        state.unexpectedRequests.push(`${req.method} ${req.url}`);
        req.reply(501, `Fake Keycloak does not implement ${req.url}`);
    });

    cy.intercept(endpoint("GET", "auth"), handleAuth).as("fakeKeycloakAuth");
    cy.intercept(endpoint("POST", "token"), handleToken).as(
        "fakeKeycloakToken"
    );
    cy.intercept(endpoint("GET", "userinfo"), handleUserInfo).as(
        "fakeKeycloakUserInfo"
    );
    cy.intercept(endpoint("GET", "logout"), handleLogout).as(
        "fakeKeycloakLogout"
    );
    cy.intercept(
        endpoint("GET", "login-status-iframe.html"),
        handleLoginStatusIframe
    );
    cy.intercept(endpoint("GET", "3p-cookies/step1.html"), (req) =>
        req.reply({
            statusCode: 200,
            headers: { "content-type": "text/html" },
            body: thirdPartyCookiesPage(state.thirdPartyCookiesBlocked),
        })
    );
}
