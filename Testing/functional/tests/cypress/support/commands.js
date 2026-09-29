// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
import { AuthMethod, localDevUri } from "./constants";
import { ensureKeycloakSession } from "./functions/authentication";
import {
    setupStandardAliases,
    waitForInitialDataLoad,
} from "./functions/intercept";
import {
    startLoginDiagnostics,
    stopLoginDiagnostics,
} from "./functions/loginDiagnostics";
import { visitTestPage } from "./functions/navigation";
import {
    assertAuthenticatedPage,
    waitForScenarioData,
} from "./functions/pageReadiness";
import {
    configureTestSettings,
    readEnvironmentConfig,
    resetTestSettings,
    resolveTestSettings,
} from "./functions/testConfiguration";
const { globalStorage } = require("./globalStorage");
require("cy-verify-downloads").addCustomCommand();

// Test configuration must not be saved/restored with authentication storage.
beforeEach(() => {
    stopLoginDiagnostics();
    resetTestSettings();
});

afterEach(() => {
    stopLoginDiagnostics();
});

function generateRandomString(length) {
    let text = "";
    const possible =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (let i = 0; i < length; i++) {
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }
    return text;
}

// function loginWithKeycloakUICached(username, password, config, path = "/home") {
//     cy.session(
//         [`keycloak-${username}`, path], // unique key (array form allows multiple args)
//         () => {
//             // Call your full UI-based login function
//             loginWithKeycloakUI(username, password, config, path);
//         },
//         {
//             validate: () => {
//                 // Minimal check that you're still authenticated
//                 cy.visit(path, { failOnStatusCode: false });
//                 cy.get("body").should("not.contain", "Login"); // Or something more specific to your app
//             },
//         }
//     );
// }

function loginWithKeycloakUI(username, password, config, path = "/home") {
    const defaultPath = "/home";

    cy.log(
        `Authenticating as KeyCloak user ${username} using the UI with default path: ${defaultPath}`
    );

    cy.visit("/login");
    cy.get("#KeyCloakBtn")
        .should("be.visible")
        .should("have.text", "KeyCloak")
        .click();

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
                const configPromise = config
                    ? cy.wrap(config)
                    : cy.readConfig();

                configPromise.then((resolvedConfig) => {
                    cy.window().then((window) => {
                        window.sessionStorage.setItem(
                            "configSettingsKey",
                            JSON.stringify(resolvedConfig)
                        );
                    });

                    setupStandardAliases();

                    // Optional re-navigation only if path is different from default
                    if (path !== defaultPath) {
                        cy.visit(path, { timeout: 60000 });
                    } else {
                        cy.reload(); // if already on home, just reload to trigger page load with config + intercepts
                    }

                    // Wait on all registered intercepts
                    waitForInitialDataLoad(username, resolvedConfig, path);
                });
            });
    });
}

function logoutWithUI() {
    cy.get("body").then(($body) => {
        if ($body.find("[data-testid=headerDropdownBtn]").length > 0) {
            cy.get("[data-testid=headerDropdownBtn]")
                .scrollIntoView()
                .click({ force: true });
            cy.get("[data-testid=logoutBtn]").click();
            cy.get("[data-testid=ratingModalSkipBtn]").click();
            cy.get("[data-testid=logout-complete-msg]").should("be.visible");
            cy.get("[data-testid=loginBtn]")
                .should("be.visible")
                .should("not.be.disabled");
        } else {
            cy.log("Logout button not found — likely already logged out.");
        }
    });
}

Cypress.Commands.add("logout", () => {
    let baseWebClientUrl = Cypress.config("baseUrl");

    cy.clearCookies();
    cy.clearLocalStorage();

    if (baseWebClientUrl == localDevUri) {
        logoutWithUI();
    } else {
        cy.readConfig().then((config) => {
            cy.log(`Performing Keycloak logout`);
            cy.request({
                url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
                failOnStatusCode: false,
            });
        });
    }
});

Cypress.Commands.add(
    "login",
    (
        username,
        password,
        authMethod = AuthMethod.BCSC,
        path = "/timeline",
        sessionId = "default"
    ) => {
        if (authMethod == AuthMethod.KeyCloak) {
            return resolveTestSettings().then((settings) => {
                if (Cypress.config("baseUrl") === localDevUri) {
                    return loginWithKeycloakUI(
                        username,
                        password,
                        settings,
                        path
                    );
                }

                const diagnostics = startLoginDiagnostics();
                cy.then(() => diagnostics.stage("session-setup-or-restore"));
                ensureKeycloakSession(username, password, settings, sessionId);

                // Session setup/validation may consume requests. Register fresh
                // aliases before the final visit used by the test's data waits.
                cy.then(() => diagnostics.stage("opening-requested-hg-page"));
                visitTestPage(path);
                waitForScenarioData(username, settings, path);
                assertAuthenticatedPage(path);
                cy.then(() => diagnostics.stage("requested-hg-page-ready"));
                cy.getCookies({ log: false }).then((cookies) => {
                    globalStorage.authCookies = cookies;
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
            cy.origin(
                "https://idtest.gov.bc.ca",
                { args: { username, password } },
                ({ username, password }) => {
                    cy.url().should(
                        "contains",
                        "https://idtest.gov.bc.ca/login/entry#start"
                    );
                    // within Cypress v14.0+, the idtest login page will load with display: none
                    // requiring { force: true } to interact with any elements on that page
                    cy.get(
                        "#tile_btn_test_with_username_password_device_div_id > h2"
                    ).click({ force: true });
                    cy.get("#username").type(username, { force: true });
                    cy.get("#password").type(password, {
                        log: false,
                        force: true,
                    });
                    cy.get("#submit-btn").click({ force: true });
                }
            );
        } else {
            cy.log(`Authenticating as KeyCloak user ${username} using the UI`);
            cy.visit(path);
            cy.get("#KeyCloakBtn")
                .should("be.visible")
                .should("have.text", "KeyCloak")
                .click();
            cy.origin(
                "https://dev.loginproxy.gov.bc.ca",
                { args: { username, password } },
                ({ username, password }) => {
                    cy.get("#kc-page-title", { timeout: 10000 }).should(
                        "be.visible"
                    );
                    cy.get("#username").should("be.visible").type(username);
                    cy.get("#password")
                        .should("be.visible")
                        .type(password, { log: false });
                    cy.get("#kc-login").click();
                }
            );
        }
    }
);

Cypress.Commands.add("getTokens", (username, password) => {
    return cy.readConfig().then((config) => {
        cy.log(`Performing Keycloak logout`);
        cy.request({
            url: `${config.openIdConnect.authority}/protocol/openid-connect/logout`,
        });

        if (username && password) {
            cy.log(
                "Performing Keycloak Authentication with username and password"
            );
            let stateId = generateRandomString(32); //"d0b27ba424b64b358b65d40cfdbc040b"
            const loginCallback =
                config.openIdConnect.callbacks?.Logon ||
                `${Cypress.config().baseUrl}/loginCallback`;
            return cy
                .request({
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/auth`,
                    followRedirect: false,
                    qs: {
                        scope: config.openIdConnect.scope,
                        response_type: config.openIdConnect.responseType,
                        approval_prompt: "auto",
                        redirect_uri: loginCallback,
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
                            redirect_uri: loginCallback,
                            code,
                            grant_type: "authorization_code",
                        },
                        form: true,
                        followRedirect: false,
                    }).its("body");
                });
        } else {
            cy.log(
                "Performing Keycloak Authenticaiton with cleint credentials"
            );

            return cy
                .request({
                    method: "POST",
                    url: `${config.openIdConnect.authority}/protocol/openid-connect/token`,
                    form: true,
                    body: {
                        grant_type: "client_credentials",
                        client_id: Cypress.env("keycloak.erebus.client"),
                        client_secret: Cypress.env("keycloak.erebus.secret"),
                    },
                })
                .its("body");
        }
    });
});

Cypress.Commands.add("readConfig", readEnvironmentConfig);

Cypress.Commands.add("checkOnTimeline", () => {
    cy.contains("#subject", "Health Records").should("be.visible");
});

Cypress.Commands.add("checkTimelineHasLoaded", () => {
    cy.contains("#subject", "Health Records").should("exist");
    cy.get("[data-testid=loadingSpinner]").should("not.exist");
    // Vuetify can retain the overlay element after closing the snackbar.
    cy.get("[data-testid=loading-toast].v-overlay--active").should("not.exist");
});

Cypress.Commands.add("configureSettings", configureTestSettings);

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

Cypress.Commands.add("vSelect", (selector, value) => {
    cy.get(selector).click({ force: true }).trigger("mousedown");
    cy.document()
        .find(".v-overlay--active.v-menu .v-list-item")
        .contains(value)
        .click({ force: true });
});
