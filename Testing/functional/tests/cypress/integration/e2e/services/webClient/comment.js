describe("WebClient Comment Service", () => {
    const BASEURL = "UserProfile/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get Comment Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Comment/`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Get Comment Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Comment/`,
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
                    expect(response.body.resourcePayload).to.be.an("object")
                        .that.is.empty;
                    expect(response.body.totalResultCount).to.eq(0);
                    expect(response.body.resultStatus).to.eq(1);
                    expect(response.body.resultError).to.eq(null);
                });
            });
        });
    });

    it("Verify Post Comment Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "POST",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Comment/`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Post Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "POST",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Put Comment Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "PUT",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Comment/`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Put Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "PUT",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Delete Comment Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "DELETE",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Comment/`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Delete Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "DELETE",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Comment/`,
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
