// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
require("cy-verify-downloads").addCustomCommand();

const openIdConnectClientId = "hg-admin-blazor";
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
    cy.visit(path, { timeout: 60000 });
    // wait for log in to complete
    cy.get("[data-testid=user-account-icon]").should("exist");
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
