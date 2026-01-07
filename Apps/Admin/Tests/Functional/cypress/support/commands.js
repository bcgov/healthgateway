// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
import {
    setupStandardAliases,
    waitForInitialDataLoad,
} from "./functions/intercept";
require("cy-verify-downloads").addCustomCommand();

const openIdConnectClientId = "hg-admin";
const resizeObserverLoopErr = "ResizeObserver loop limit exceeded";

function generateRandomString(length) {
    var text = "";
    var possible =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

function loginWithKeycloakUI(username, password, path = "/dashboard") {
    let defaultPath =
        username === Cypress.env("keycloak_support_username")
            ? "/support"
            : "/dashboard";

    cy.log(
        `Authenticating as KeyCloak user ${username} using the UI with default path: ${defaultPath}`
    );
    cy.visit("/login");
    cy.get("[data-testid=sign-in-btn]").should("be.visible").click();

    // Ensure all post-login logic happens only after origin flow completes
    cy.origin(
        "https://dev.loginproxy.gov.bc.ca",
        { args: { username, password } },
        ({ username, password }) => {
            cy.get("#kc-page-title", { timeout: 10000 }).should("be.visible");
            cy.get("form[action*='login-actions']", { timeout: 10000 }).within(
                () => {
                    cy.get("#username")
                        .should("be.visible")
                        .clear()
                        .type(username);
                    cy.get("#password")
                        .should("be.visible")
                        .clear()
                        .type(password, { log: false });
                    cy.get("#kc-login").click();
                }
            );
        }
    ).then(() => {
        // Now you're guaranteed to be back on your app domain
        cy.url()
            .should("include", defaultPath)
            .then(() => {
                setupStandardAliases(path);

                // Optional re-navigation only if path is different from default
                if (path !== defaultPath) {
                    cy.visit(path, { timeout: 60000 });
                } else {
                    cy.reload(); // if already on dashboard, just reload to trigger page load with intercepts
                }

                cy.disableServiceWorker();

                // Wait on all registered intercepts
                waitForInitialDataLoad(path);

                // wait for log in to complete
                cy.get("[data-testid=user-account-icon]").should("exist");
            });
    });
}

function logout(config) {
    cy.log("Logging out.");
    cy.request({
        url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
        failOnStatusCode: false,
    });
}

Cypress.on("uncaught:exception", (err) => {
    /* returning false here prevents Cypress from failing the test */
    if (err.message.includes(resizeObserverLoopErr)) {
        return false;
    }
});

Cypress.Commands.add("logout", () => {
    cy.readConfig().then((config) => logout(config));
});

Cypress.Commands.add("login", (username, password, path) => {
    // Clear cookies and local storage before starting the login process
    cy.clearCookies();
    cy.clearLocalStorage();

    const baseWebClientUrl = Cypress.config("baseUrl");
    const localDevUri = "http://localhost:5027";

    if (baseWebClientUrl == localDevUri) {
        loginWithKeycloakUI(username, password, path);
    } else {
        cy.session(username, () => {
            cy.log("Logging in using Keycloak.");
            cy.readConfig().then((config) => {
                logout(config);

                const stateId = generateRandomString(32); //"d0b27ba424b64b358b65d40cfdbc040b"
                const codeVerifier = generateRandomString(96);
                const stateStore = {
                    id: stateId,
                    created: new Date().getTime(),
                    request_type: "si:r",
                    code_verifier: codeVerifier,
                    redirect_uri: config.openIdConnect.callbacks.Logon,
                    authority: config.openIdConnect.authority,
                    client_id: openIdConnectClientId,
                    response_mode: "query",
                    extraTokenParams: {},
                };

                cy.log("Creating OIDC StateStore in local storage.");
                cy.log(`State ID:  ${stateId}`);
                cy.log(`Generated Code Verifier: ${codeVerifier}`);
                window.sessionStorage.setItem(
                    `oidc.${stateStore.id}`,
                    JSON.stringify(stateStore)
                );

                cy.log(`Requesting Keycloak Authentication form.`);
                cy.request({
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/auth`,
                    followRedirect: false,
                    qs: {
                        response_type: config.openIdConnect.responseType,
                        approval_prompt: "auto",
                        redirect_uri: config.openIdConnect.callbacks.Logon,
                        client_id: openIdConnectClientId,
                        response_mode: "query",
                        state: stateStore.id,
                    },
                }).then((response) => {
                    cy.log("Posting credentials.");
                    const html = document.createElement("html");
                    html.innerHTML = response.body;
                    const form = html.getElementsByTagName("form")[0];
                    const body = {
                        username: username,
                        password: password,
                    };
                    const url = form.action;
                    return cy.request({
                        method: "POST",
                        url,
                        followRedirect: false,
                        form: true,
                        body,
                    });
                });
            });
        });
        cy.log(`Visiting ${path}`);
        setupStandardAliases(path);

        cy.disableServiceWorker();

        cy.visit(path, { timeout: 60000 });
        waitForInitialDataLoad(path);

        // wait for log in to complete
        cy.get("[data-testid=user-account-icon]").should("exist");
    }
});

Cypress.Commands.add("readConfig", () => {
    cy.log(`Reading Environment Configuration`);
    return cy
        .request(`${Cypress.config("baseUrl")}/v1/api/Configuration`)
        .should((response) => {
            expect(response.status).to.eq(200);
        })
        .its("body");
});

Cypress.Commands.overwrite(
    "select",
    (originalFn, subject, valueOrTextOrIndex, options) => {
        cy.wrap(subject).should("be.visible", "be.enabled");
        cy.wrap(originalFn(subject, valueOrTextOrIndex, options));
    }
);

Cypress.Commands.add("validateTableLoad", (tableSelector) => {
    cy.get(tableSelector)
        .find(".mud-table-loading-progress")
        .should("be.visible");
    cy.get(tableSelector)
        .find(".mud-table-loading-progress")
        .should("not.exist");
});

Cypress.Commands.add("disableServiceWorker", () => {
    cy.log("Unregistering ServiceWorker.");
    cy.window().then((_win) => {
        if ("serviceWorker" in navigator) {
            navigator.serviceWorker.getRegistrations().then((registrations) => {
                registrations.forEach((registration) => {
                    registration.unregister();
                });
            });
        }
    });
});
