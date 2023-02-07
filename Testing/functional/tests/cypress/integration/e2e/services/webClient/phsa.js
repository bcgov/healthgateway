function getPhsaTokens(config) {
    return cy
        .request({
            method: "POST",
            url: `${config.openIdConnect.authority}/protocol/openid-connect/token`,
            form: true,
            body: {
                grant_type: "client_credentials",
                client_id: Cypress.env("keycloak.phsa.client"),
                client_secret: Cypress.env("keycloak.phsa.secret"),
            },
        })
        .its("body");
}

describe("GatewayApi PHSA Access", () => {
    const BASEURL = "Phsa/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

    it("Verify Get Dependents for User Unauthorized", () => {
        cy.readConfig().then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get Dependents for User Forbidden", () => {
        cy.readConfig().then((config) => {
            cy.getTokens(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password")
            ).then((tokens) => {
                cy.log("Tokens", tokens);
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents/${HDID}`,
                    followRedirect: false,
                    failOnStatusCode: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(403);
                });
            });
        });
    });

    it("Verify Get Dependents for User Authorized", () => {
        cy.readConfig().then((config) => {
            getPhsaTokens(config).then((tokens) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents/${HDID}`,
                    followRedirect: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(200);
                    expect(response.body).to.not.be.null;
                });
            });
        });
    });

    it("Verify Get Dependents Unauthorized", () => {
        cy.readConfig().then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get Dependents Forbidden", () => {
        cy.readConfig().then((config) => {
            cy.getTokens(
                Cypress.env("keycloak.username"),
                Cypress.env("keycloak.password")
            ).then((tokens) => {
                cy.log("Tokens", tokens);
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents`,
                    followRedirect: false,
                    failOnStatusCode: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(403);
                });
            });
        });
    });

    it("Verify Get Dependents Authorized", () => {
        cy.readConfig().then((config) => {
            getPhsaTokens(config).then((tokens) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}dependents`,
                    followRedirect: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(200);
                    expect(response.body).to.not.be.null;
                });
            });
        });
    });
});
