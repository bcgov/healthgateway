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

function configureDatasets(datasets, deltaset) {
    let newDatasets = [];
    if (deltaset !== undefined && Array.isArray(deltaset)) {
        const setTable = {};
        datasets.forEach((s) => (setTable[s.name] = s.enabled));
        deltaset.forEach((d) => (setTable[d.name] = d.enabled));

        for (const key in setTable) {
            newDatasets.push({ name: key, enabled: setTable[key] });
        }
    }
    return newDatasets;
}

function disableDatasets(datasets) {
    return datasets.map((s) => {
        s.enabled = false;
        return s;
    });
}

function configureObject(baseObj, delta) {
    const deltaKeys = Object.keys(delta);
    if (deltaKeys && deltaKeys.length > 0) {
        for (const key of deltaKeys) {
            const baseValue = baseObj[key];
            if (baseValue === undefined) {
                throw new Error(`Unknown property: ${key}`);
            }
            const baseValueKeys = Object.keys(baseValue);
            if (
                baseValueKeys &&
                baseValueKeys.length > 0 &&
                !Array.isArray(baseValue)
            ) {
                configureObject(baseObj[key], delta[key]);
            } else if (!Array.isArray(baseValue)) {
                baseObj[key] = delta[key];
            }
        }
    }
}

function disableObject(config) {
    const configKeys = Object.keys(config);
    if (configKeys && configKeys.length > 0) {
        for (const key of configKeys) {
            const configValue = config[key];
            const configValueKeys = Object.keys(configValue);
            if (
                configValueKeys &&
                configValueKeys.length > 0 &&
                !Array.isArray(configValue)
            ) {
                disableObject(config[key]);
            } else if (!Array.isArray(configValue)) {
                config[key] = false;
            }
        }
    }
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

Cypress.Commands.add("logout", () => {
    cy.readConfig().then((config) => {
        cy.log(`Performing Keycloak logout`);
        cy.request({
            url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
            failOnStatusCode: false,
        });
    });
});

Cypress.Commands.add(
    "login",
    (username, password, authMethod = AuthMethod.BCSC, path = "/timeline") => {
        if (authMethod == AuthMethod.KeyCloak) {
            cy.session([username, authMethod], () => {
                cy.readConfig().then((config) => {
                    cy.logout();
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

                    const escapedRedirectPath = encodeURI(path);
                    const redirectUri = `${config.openIdConnect.callbacks.Logon}?redirect=${escapedRedirectPath}`;

                    cy.log("Requesting Keycloak Authentication form");
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
                            cy.visit(callbackURL, { timeout: 60000 });
                            // store auth cookies
                            cy.getCookies({ timeout: 60000 }).then(
                                (cookies) => {
                                    globalStorage.authCookies = cookies;
                                }
                            );
                        });
                });
            });
            cy.log(`Visit path: ${path}`);
            cy.visit(path, { timeout: 60000 });
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
        .request(`${Cypress.config("baseUrl")}/configuration`)
        .should((response) => {
            expect(response.status).to.eq(200);
        })
        .its("body");
});

Cypress.Commands.add("checkOnTimeline", () => {
    cy.get("#subject").should("be.visible").and("have.text", "Timeline");
});

Cypress.Commands.add("checkTimelineHasLoaded", () => {
    cy.contains("#subject", "Timeline").should("be.visible");
    cy.get("[data-testid=loading-toast]").should("not.exist");
    cy.get("[data-testid=loading-in-progress]").should("not.exist");
});

Cypress.Commands.add("configureSettings", (settings) => {
    return cy
        .readConfig()
        .as("config")
        .then((config) => {
            // Create new configuration object to be configured and mutated
            const featureToggleConfiguration = {
                ...config.webClient.featureToggleConfiguration,
            };

            // Disable datasets
            const disabledDatasets = disableDatasets(
                config.webClient.featureToggleConfiguration.datasets
            );

            // Disable dependents datasets
            const disabledDependentsDatasets = disableDatasets(
                config.webClient.featureToggleConfiguration.dependents?.datasets
            );

            // Disable non dataset object properties
            disableObject(featureToggleConfiguration);

            // Apply disabled datasets to configuration object
            featureToggleConfiguration.datasets = disabledDatasets;
            featureToggleConfiguration.dependents.datasets = [];

            // Configure datasets with overrides
            const datasets = configureDatasets(
                featureToggleConfiguration.datasets,
                settings.datasets
            );

            // Configure dependents datasets with overrides
            const dependentsDatasets = configureDatasets(
                featureToggleConfiguration.dependents.datasets,
                settings.dependents?.datasets
            );

            // Configure overrides to non dataset object properties
            configureObject(featureToggleConfiguration, settings);

            // Apply dataset overrides to configuration object
            featureToggleConfiguration.datasets = datasets;
            featureToggleConfiguration.dependents.datasets = dependentsDatasets;

            // Apply configured configuration object to configuration to be returned in intercept
            config.webClient.featureToggleConfiguration =
                featureToggleConfiguration;

            cy.intercept("GET", "**/configuration/", {
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

Cypress.Commands.add(
    "shouldContainValue",
    { prevSubject: "element" },
    (subject, value) => {
        cy.wrap(subject)
            .children("[value=" + value + "]")
            .should("exist");
        cy.wrap(subject);
    }
);

Cypress.Commands.add(
    "shouldNotContainValue",
    { prevSubject: "element" },
    (subject, value) => {
        cy.wrap(subject)
            .children("[value=" + value + "]")
            .should("not.exist");
        cy.wrap(subject);
    }
);

Cypress.Commands.add(
    "populateDateDropdowns",
    (yearSelector, monthSelector, daySelector, dateString) => {
        const date = new Date(dateString);

        const year = date.getFullYear();
        const month = date.getMonth() + 1; // add 1 to the returned month since they're indexed starting at 0
        const day = date.getDate();

        cy.get(yearSelector).select(year.toString());
        cy.get(monthSelector).select(month.toString());
        cy.get(daySelector).select(day.toString());
    }
);
