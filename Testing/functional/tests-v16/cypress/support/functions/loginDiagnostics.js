// One bounded collector per test attempt; never retain credentials or URL queries.
let collector;
const disabledDiagnostics = {
    stop() {},
    stage() {},
    configurationRequest() {
        return () => {};
    },
};

export function stopLoginDiagnostics() {
    collector?.stop();
    collector = undefined;
}
export function startLoginDiagnostics() {
    // Azure environment values may arrive as strings rather than booleans.
    if (String(Cypress.expose("loginDiagnostics")).toLowerCase() !== "true") {
        stopLoginDiagnostics();
        return disabledDiagnostics;
    }
    if (collector) {
        return collector;
    }
    const started = Date.now();
    const startedAt = new Date(started).toISOString();
    const stages = [];
    const windows = new Map();
    const requests = [];
    const browserErrors = [];
    let active = true;
    let page;
    let providers;
    const origin = new URL(Cypress.config("baseUrl")).origin;

    function safePath(value) {
        try {
            const url = new URL(value, origin);
            if (url.origin !== origin) {
                return "external-origin";
            }
            // Never include query strings, auth callbacks or API identifiers.
            return /^\/(login|loginCallback|configuration|home|timeline|profile|registration|acceptTermsOfService|patientRetrievalError|unauthorized)$/.test(
                url.pathname
            ) || /^\/assets\/[\w./-]+\.(js|css)$/.test(url.pathname)
                ? url.pathname
                : "other-resource";
        } catch {
            return "unknown-resource";
        }
    }

    function stage(name) {
        if (active && stages.length < 50) {
            stages.push({ name, elapsedMs: Date.now() - started });
        }
    }

    function navigation(url) {
        stage(`navigation:${safePath(url)}`);
    }

    function errorKind(message) {
        return /dynamically imported module|module script|Loading chunk/i.test(
            message
        )
            ? "module-load-error"
            : /3rd party check iframe/i.test(message)
              ? "keycloak-iframe-timeout"
              : "javascript-error";
    }

    function recordError(event) {
        if (!active || browserErrors.length >= 10) {
            return;
        }
        const message = String(event.message || event.reason?.message || "");
        browserErrors.push({
            event: event.type,
            kind:
                event.target?.src || event.target?.href
                    ? "resource-load-error"
                    : errorKind(message),
            resource: safePath(
                event.filename || event.target?.src || event.target?.href || ""
            ),
        });
    }

    function observeWindow(win) {
        if (!active) {
            return;
        }
        page = win;
        if (windows.has(win)) {
            return;
        }
        // Initialization errors may be caught by the app and only logged.
        const originalError = win.console.error;
        const diagnosticError = function (...args) {
            if (active && browserErrors.length < 10) {
                const message = args
                    .map((arg) =>
                        typeof arg === "string" ? arg : arg?.message || ""
                    )
                    .join(" ");
                browserErrors.push({
                    event: "console.error",
                    kind: errorKind(message),
                });
            }
            return originalError.apply(this, args);
        };
        windows.set(win, { originalError, diagnosticError });
        win.console.error = diagnosticError;
        win.addEventListener("error", recordError, true);
        win.addEventListener("unhandledrejection", recordError);
    }

    function stop() {
        active = false;
        cy.removeListener("window:before:load", observeWindow);
        cy.removeListener("fail", reportFailure);
        cy.removeListener("url:changed", navigation);
        for (const [win, { originalError, diagnosticError }] of windows) {
            try {
                win.removeEventListener("error", recordError, true);
                win.removeEventListener("unhandledrejection", recordError);
                if (win.console.error === diagnosticError) {
                    win.console.error = originalError;
                }
            } catch {
                // A window may have navigated to the identity provider.
            }
        }
        windows.clear();
    }

    function reportFailure(error) {
        let documentState = "unavailable";
        try {
            documentState = {
                path: safePath(page.location.href),
                readyState: page.document.readyState,
                loginPicker: !!page.document.querySelector("#loginPicker"),
                keycloakButton: !!page.document.querySelector("#KeyCloakBtn"),
            };
        } catch {
            // A missing or inaccessible page must not replace the real error.
        }
        const details = JSON.stringify({
            startedAt,
            spec: Cypress.spec.relative,
            test: Cypress.currentTest?.titlePath,
            attempt: Cypress.currentRetry + 1,
            elapsedMs: Date.now() - started,
            stages,
            documentState,
            providers: providers ?? "not-observed",
            requests: requests.map((entry) => ({
                ...entry,
                durationMs:
                    (entry.completedMs ?? Date.now() - started) -
                    entry.startedMs,
            })),
            browserErrors,
        });
        const suffix = `\nHG LOGIN DIAGNOSTICS: ${details}`;
        error.message += suffix;
        if (error.stack) {
            error.stack += suffix;
        }
        stop();
        throw error;
    }

    function configurationRequest(url) {
        const entry = {
            source: "cy.request",
            path: safePath(url),
            startedMs: Date.now() - started,
            status: "no-response-observed",
        };
        if (requests.length < 100) {
            requests.push(entry);
        }
        stage("configuration-request");
        return (response) => {
            entry.status = response.status;
            entry.completedMs = Date.now() - started;
        };
    }

    cy.on("url:changed", navigation);
    cy.on("window:before:load", observeWindow);
    cy.on("fail", reportFailure);
    cy.intercept(
        {
            method: "GET",
            hostname: new URL(origin).hostname,
            pathname: /^\/(login|configuration|assets\/.*\.(js|css))$/,
            middleware: true,
        },
        (request) => {
            if (!active || requests.length >= 100) {
                return;
            }
            const entry = {
                source: "browser",
                path: safePath(request.url),
                startedMs: Date.now() - started,
                status: "no-response-observed",
            };
            requests.push(entry);
            request.on("after:response", (response) => {
                if (!active) {
                    return;
                }
                entry.status = response.statusCode;
                entry.completedMs = Date.now() - started;
                if (entry.path === "/configuration") {
                    const list = response.body?.identityProviders;
                    providers = {
                        count: Array.isArray(list) ? list.length : null,
                        keycloakPresent:
                            Array.isArray(list) &&
                            list.some((p) => p.id === "KeyCloak"),
                        keycloakDisabled:
                            Array.isArray(list) &&
                            list.some(
                                (p) =>
                                    p.id === "KeyCloak" && p.disabled === true
                            ),
                    };
                }
            });
            // Fall through: existing fixtures and aliases still handle requests.
        }
    );
    collector = { stop, stage, configurationRequest };
    return collector;
}
