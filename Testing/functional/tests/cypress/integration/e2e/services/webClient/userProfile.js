describe("GatewayApi UserProfile Service", () => {
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

    it("Verify Get UserProfile Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get UserProfile Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}?api-version=2.0`,
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

    it("Verify Get UserProfile Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}?api-version=2.0`,
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
                    expect(response.body.hdId).to.equal(HDID);
                });
            });
        });
    });

    it("Verify Post UserProfile Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "POST",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Post UserProfile Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "POST",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}?api-version=2.0`,
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

    it("Verify Delete UserProfile Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "DELETE",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Delete UserProfile Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "DELETE",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}?api-version=2.0`,
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

    it("Verify Get UserProfile TermsOfService", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}termsofservice?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(200);
            });
        });
    });

    it("Verify Get UserProfile Validate Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/Validate?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get UserProfile Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/Validate?api-version=2.0`,
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

    it("Verify Get UserProfile Recover Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/recover?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get UserProfile Recover Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/recover?api-version=2.0`,
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

    it("Verify Get UserProfile Email Validate Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/email/validate/123?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get UserProfile Email Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/email/validate/123?api-version=2.0`,
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

    it("Verify Put UserProfile Email Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "PUT",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/email?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Put UserProfile Email Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "PUT",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/email?api-version=2.0`,
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

    it("Verify Get UserProfile SMS Validate Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/sms/validate/123?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Get UserProfile SMS Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/sms/validate/123?api-version=2.0`,
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

    it("Verify Put UserProfile SMS Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "PUT",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/sms?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Put UserProfile SMS Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "PUT",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/sms?api-version=2.0`,
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

    it("Verify Put UserProfile Preference Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.request({
                method: "PUT",
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${HDID}/preference?api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Put UserProfile Preference Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.request({
                    method: "PUT",
                    url: `${config.serviceEndpoints.GatewayApi}${BASEURL}${BOGUSHDID}/preference?api-version=2.0`,
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
