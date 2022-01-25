// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
const { AuthMethod } = require("./constants");
const { globalStorage } = require("./globalStorage");
require("cy-verify-downloads").addCustomCommand();

function storeAuthCookies() {
    cy.getCookies().then((cookies) => {
        globalStorage.authCookies = cookies;
    });
}

function generateCodeVerifier() {
    var code_verifier = generateRandomString(96);
    return code_verifier;
}

function generateRandomString(length) {
    var text = "";
    var possible =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

Cypress.Commands.add(
    "login",
    (username, password, authMethod = AuthMethod.BCSC, path = "/timeline") => {
        if (authMethod == AuthMethod.KeyCloak) {
            cy.readConfig().then((config) => {
                cy.log(`Performing Keycloak logout`);
                cy.request({
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
                });
                let stateId = generateRandomString(32); //"d0b27ba424b64b358b65d40cfdbc040b"
                let codeVerifier = generateRandomString(96);
                cy.log(
                    `State Id:  ${stateId}, Generated Code Verifier: ${codeVerifier}`
                );
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
                cy.log("Creating OIDC StateStore in Local storage");
                window.sessionStorage.setItem(
                    `oidc.${stateStore.id}`,
                    JSON.stringify(stateStore)
                );

                cy.log(
                    `Creating OIDC Active Route: ${path} in Session storage`
                );
                window.sessionStorage.setItem("vuex_oidc_active_route", path);

                cy.log("Requesting Keycloak Authentication form");
                cy.request({
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/auth`,
                    followRedirect: false,
                    qs: {
                        scope: config.openIdConnect.scope,
                        response_type: config.openIdConnect.responseType,
                        approval_prompt: "auto",
                        redirect_uri: config.openIdConnect.callbacks.Logon,
                        client_id: config.openIdConnect.clientId,
                        response_mode: "query",
                        state: stateStore.id,
                    },
                })
                    .then((response) => {
                        cy.log("Posting credentials");
                        const html = document.createElement("html");
                        html.innerHTML = response.body;
                        const form = html.getElementsByTagName("form")[0];
                        const url = form.action;
                        return cy.request({
                            method: "POST",
                            url,
                            followRedirect: false,
                            form: true,
                            body: {
                                username: username,
                                password: password,
                            },
                        });
                    })
                    .then((response) => {
                        let callBackQS = response.headers["location"];
                        const callbackURL = `${callBackQS}`;
                        cy.log(`Visiting Callback ${callBackQS}`, response);
                        cy.visit(callbackURL);
                        // Wait for cookies are set before store them in cypress.
                        cy.get("[data-testid=headerDropdownBtn]").should(
                            "be.visible"
                        );
                        storeAuthCookies();
                    });
            });
        } else if (authMethod == AuthMethod.BCSC) {
            cy.log(
                `Authenticating as BC Services Card user ${username} using the UI`
            );
            cy.visit(path);
            cy.get("#BCSCBtn")
                .should("be.visible")
                .should("have.text", "BC Services Card")
                .click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/entry#start"
            );
            cy.get("#tile_btn_virtual_device_div_id > h2").click();
            cy.get("#csn").click({ force: true });
            cy.get("#csn").type(username);
            cy.get("#continue").click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/identify"
            );
            cy.get("#passcode").click({ force: true });
            cy.get("#passcode").type(password);
            cy.get("#btnSubmit").click();
        } else {
            cy.log(`Authenticating as KeyCloak user ${username} using the UI`);
            cy.visit(path);
            cy.get("#KeyCloakBtn")
                .should("be.visible")
                .should("have.text", "KeyCloak")
                .click();
            cy.get("#username").type(username);
            cy.get("#password").type(password);
            cy.get("#kc-login").click();
        }
    }
);

Cypress.Commands.add("getTokens", (username, password) => {
    cy.readConfig().then((config) => {
        cy.log(`Performing Keycloak logout`);
        cy.request({
            url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
        });

        cy.log("Performing Keycloak Authentication");
        let stateId = generateRandomString(32); //"d0b27ba424b64b358b65d40cfdbc040b"
        cy.request({
            url: `${config.openIdConnect.authority}/protocol/openid-connect/auth`,
            followRedirect: false,
            qs: {
                scope: config.openIdConnect.scope,
                response_type: config.openIdConnect.responseType,
                approval_prompt: "auto",
                redirect_uri: config.openIdConnect.callbacks.Logon,
                client_id: config.openIdConnect.clientId,
                response_mode: "query",
                state: stateId,
            },
        })
            .then((response) => {
                cy.log("Posting credentials");
                const html = document.createElement("html");
                html.innerHTML = response.body;
                const form = html.getElementsByTagName("form")[0];
                const url = form.action;
                return cy.request({
                    method: "POST",
                    url,
                    followRedirect: false,
                    form: true,
                    body: {
                        username: username,
                        password: password,
                    },
                });
            })
            .then((response) => {
                cy.log(
                    `CALLBACK for Posting credentials : response: ${JSON.stringify(
                        response
                    )}`
                );
                let callBackQS = response.headers["location"];
                const url = new URL(callBackQS);
                const params = url.search.substring(1).split("&");
                let code;
                for (const param of params) {
                    const [key, value] = param.split("=");
                    if (key === "code") {
                        code = value;
                        break;
                    }
                }
                cy.request({
                    method: "post",
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/token`,
                    body: {
                        client_id: config.openIdConnect.clientId,
                        redirect_uri: config.openIdConnect.callbacks.Logon,
                        code,
                        grant_type: "authorization_code",
                    },
                    form: true,
                    followRedirect: false,
                }).its("body");
            });
    });
});

Cypress.Commands.add("readConfig", () => {
    cy.log(`Reading Environment Configuration`);
    return cy
        .request(`${Cypress.config("baseUrl")}/v1/api/configuration`)
        .should((response) => {
            expect(response.status).to.eq(200);
        })
        .its("body");
});

Cypress.Commands.add("checkTimelineHasLoaded", () => {
    cy.get("#subject").should("have.text", "Timeline");
    cy.get("[data-testid=loading-toast]").should("not.exist");
});

Cypress.Commands.add("checkVaccineRecordHasLoaded", () => {
    cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
});

Cypress.Commands.add("checkFederalCardButtonLoaded", () => {
    cy.get("[data-testid=proof-vaccination-card-btn]").should("not.be.visible");
});

Cypress.Commands.add("enableModules", (modules) => {
    const isArrayOfModules = Array.isArray(modules);
    return cy
        .readConfig()
        .as("config")
        .then((config) => {
            Object.keys(config.webClient.modules).forEach((key) => {
                if (isArrayOfModules) {
                    config.webClient.modules[key] = modules.includes(key);
                } else {
                    config.webClient.modules[key] = modules === key;
                }
            });
            cy.intercept("GET", "**/v1/api/configuration/", {
                statusCode: 200,
                body: config,
            });
        });
});

Cypress.Commands.add("setupDownloads", () => {
    const downloadsFolder = "cypress/downloads";
    // The next command allow downloads in Electron, Chrome, and Edge
    // without any users popups or file save dialogs.
    if (!Cypress.isBrowser("firefox")) {
        // since this call returns a promise, must tell Cypress to wait for it to be resolved
        cy.log("Page.setDownloadBehavior");
        cy.wrap(
            Cypress.automation("remote:debugger:protocol", {
                command: "Page.setDownloadBehavior",
                params: { behavior: "allow", downloadPath: downloadsFolder },
            }),
            { log: false }
        );
    }
});

Cypress.Commands.add("restoreAuthCookies", () => {
    globalStorage.authCookies.forEach((cookie) => {
        cy.setCookie(cookie.name, cookie.value);
    });
    var names = globalStorage.authCookies.map((x) => x.name);

    Cypress.Cookies.preserveOnce(...names);
});
