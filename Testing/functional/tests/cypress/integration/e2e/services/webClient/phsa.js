function getPhsaTokens(config) {
    cy.log("Requesting access token");
    return cy
        .env(["keycloak.phsa.client", "keycloak.phsa.secret"])
        .then(
            ({
                "keycloak.phsa.client": clientId,
                "keycloak.phsa.secret": clientSecret,
            }) =>
                cy
                    .request({
                        method: "POST",
                        url: `${config.openIdConnect.authority}/protocol/openid-connect/token`,
                        form: true,
                        body: {
                            grant_type: "client_credentials",
                            client_id: clientId,
                            client_secret: clientSecret,
                        },
                        failOnStatusCode: false, // prevent leaking credentials on failures
                    })
                    .should((response) => {
                        expect(response.status).to.eq(200);
                        expect(response.body?.access_token).to.exist;
                    })
                    .its("body")
        );
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
            cy.env(["keycloak.password"]).then(
                ({ "keycloak.password": password }) => {
                    cy.getTokens(
                        Cypress.expose("keycloak.username"),
                        password
                    ).then((tokens) => {
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
                }
            );
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
            cy.env(["keycloak.password"]).then(
                ({ "keycloak.password": password }) => {
                    cy.getTokens(
                        Cypress.expose("keycloak.username"),
                        password
                    ).then((tokens) => {
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
                }
            );
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
