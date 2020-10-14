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

Cypress.Commands.add(
    "login",
    (username, password, authMethod = AuthMethod.BCSC, path = "/timeline") => {
        if (authMethod == AuthMethod.KeyCloak) {
            cy.log(`Authenticating as KeyCloak User ${username}`);
            cy.request(`${Cypress.config("baseUrl")}/v1/api/configuration`)
                .should((response) => { expect(response.status).to.eq(200) })
                .its("body").then(config => {
                    const stateStore = {
                        id: "d0b27ba424b64b358b65d40cfdbc040b",
                        created: new Date().getTime(),
                        request_type: "si:r",
                        code_verifier: "cd68894d45d84646b4b1bc4bbca482c9730850f72af8471b9240b128310adf505d9622e5e5ee43c1ba034f500e8ef7e2",
                        redirect_uri: config.openIdConnect.callbacks.Logon,
                        authority: config.openIdConnect.authority,
                        client_id: config.openIdConnect.clientId,
                        response_mode: "query",
                        scope: config.openIdConnect.scope,
                        extraTokenParams: {}
                    }
                    cy.log("Creating OIDC StateStore in Local storage")
                    window.localStorage.setItem(`oidc.${stateStore.id}`, JSON.stringify(stateStore))

                    cy.log("Creating OIDC Active Route in Session storage")
                    window.sessionStorage.setItem('vuex_oidc_active_route', path)

                    cy.log("Requesting Keycloak Authentication form")
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
                        }
                    })
                        .then(response => {
                            cy.log("Posting credentials")
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
                                    password: password
                                }
                            });
                        })
                        .then(response => {
                            let callBackQS = response.headers["location"];
                            const callbackURL = `${callBackQS}`
                            cy.log("Visiting Callback")
                            cy.visit(callbackURL)
                        })
                });
        }
        else if (authMethod == AuthMethod.BCSC) {
            cy.log(`Authenticating as BC Services Card user ${username} using the UI`);
            cy.visit(path)
            cy.get("#BCSCBtn")
                .should("be.visible")
                .should("have.text", "BC Services Card")
                .click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/entry#start"
            );
            cy.get("#tile_btn_virtual_device_div_id > h2").click();
            cy.get("#csn").click();
            cy.get("#csn").type(username);
            cy.get("#continue").click();
            cy.url().should(
                "contains",
                "https://idtest.gov.bc.ca/login/identify"
            );
            cy.get("#passcode").click();
            cy.get("#passcode").type(password);
            cy.get("#btnSubmit").click();
            cy.get("#btnSubmit").click();
        } else {
            cy.log(`Authenticating as KeyCloak user ${username} using the UI`);
            cy.visit(path)
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

Cypress.Commands.add("checkTimelineHasLoaded", () => {
    cy.get('#subject')
        .should('have.text', 'Health Care Timeline')
    cy.get("[data-testid=timelineLoading]").should("not.exist");
});

Cypress.Commands.add("closeCovidModal", () => {
    cy.get("[data-testid=covidModal] .close").click();
});
