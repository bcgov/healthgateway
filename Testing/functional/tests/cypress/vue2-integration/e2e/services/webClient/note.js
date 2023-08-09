describe("GatewayApi Note Service", () => {
    const BASEURL = "Note/";
    const HDID = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get Notes Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get Notes Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
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

    it("Verify Get Notes Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}`,
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
                    expect(response.body.resourcePayload).to.be.an("array").that
                        .is.not.empty;
                    expect(response.body.totalResultCount).to.eq(3);
                    expect(response.body.resultStatus).to.eq(1);
                    expect(response.body.resultError).to.eq(null);
                });
            });
        });
    });

    it("Verify Post Note Unauthorized", () => {
        cy.get("@config").then((config) => {
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

    it("Verify Post Note Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
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

    it("Verify Put Note Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "PUT",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Put Note Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "PUT",
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

    it("Verify Delete Note Unauthorized", () => {
        cy.get("@config").then((config) => {
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

    it("Verify Delete Note Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
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
