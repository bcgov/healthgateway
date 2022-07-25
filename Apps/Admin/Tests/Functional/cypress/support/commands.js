// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
import { AuthMethod } from "./constants";
import { globalStorage } from "./globalStorage";

require("cy-verify-downloads").addCustomCommand();

function generateRandomString(length) {
    var text = "";
    var possible =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

function logout(config) {
    cy.log("Logging out.");
    cy.request({
        url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
        failOnStatusCode: false,
    });
}

Cypress.Commands.add("logout", () => {
    cy.readConfig().then((config) => logout(config));
});

Cypress.Commands.add(
    "login",
    (username, password, path, authMethod = AuthMethod.Keycloak) => {
        if (authMethod == AuthMethod.Keycloak) {
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
                    client_id: config.openIdConnect.clientId,
                    response_mode: "query",
                    scope: config.openIdConnect.scope,
                    extraTokenParams: {},
                };

                cy.log("Creating OIDC StateStore in local storage.");
                cy.log(`State ID:  ${stateId}`);
                cy.log(`Generated Code Verifier: ${codeVerifier}`);
                window.sessionStorage.setItem(
                    `oidc.${stateStore.id}`,
                    JSON.stringify(stateStore)
                );

                const escapedRedirectPath = encodeURI(path);
                const redirectUri = `${config.openIdConnect.callbacks.Logon}?redirect=${escapedRedirectPath}`;

                cy.log(`Requesting Keycloak Authentication form.`);
                cy.request({
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/auth`,
                    followRedirect: false,
                    qs: {
                        scope: config.openIdConnect.scope,
                        response_type: config.openIdConnect.responseType,
                        approval_prompt: "auto",
                        redirect_uri: redirectUri,
                        client_id: config.openIdConnect.clientId,
                        response_mode: "query",
                        state: stateStore.id,
                    },
                })
                    .then((response) => {
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
                    })
                    .then(() => {
                        cy.visit(path, { timeout: 60000 });

                        // wait for cookies to be set before storing them in Cypress
                        cy.get("[data-testid=user-account-icon]").should(
                            "exist"
                        );

                        // store auth cookies
                        cy.getCookies().then(
                            (cookies) => (globalStorage.authCookies = cookies)
                        );
                    });
            });
        }
    }
);

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
