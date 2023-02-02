describe("GatewayApi Dependent Service", () => {
    const BASEURL = "UserProfile/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get Dependents Unauthorized", () => {
        cy.readConfig().then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Dependent`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get Dependents Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.readConfig().then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Dependent`,
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
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.readConfig().then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Dependent`,
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

    it("Verify Post Dependent Unauthorized", () => {
        cy.readConfig().then((config) => {
            cy.request({
                method: "POST",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Post Dependent Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.readConfig().then((config) => {
                cy.request({
                    method: "POST",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}`,
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

    it("Verify Delete Dependent Unauthorized", () => {
        cy.readConfig().then((config) => {
            cy.request({
                method: "DELETE",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Delete Dependent Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.readConfig().then((config) => {
                cy.request({
                    method: "DELETE",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}`,
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
});
