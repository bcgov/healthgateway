describe("Gateway Api Data Access Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const DEPENDENT_HDID = "35224807075386271";
    const DELEGATE_HDID =
        "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Swagger", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Verifying Swagger exists for Gateway Api Endpoint: ${config.serviceEndpoints.GatewayApi}swagger`
            );
            cy.visit(`${config.serviceEndpoints.GatewayApi}swagger`).contains(
                "Health Gateway API Services documentation"
            );
        });
    });

    it("Verify Data Access Blocked Datasets Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Data Access Blocked Datasets Service Endpoint: ${config.serviceEndpoints.GatewayApi}DataAccess/BlockedDatasets`
            );
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}DataAccess/BlockedDatasets/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Data Access Blocked Datasets Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Blocked Datasets Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/BlockedDatasets`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApit}DataAccess/BlockedDatasets/${BOGUSHDID}`,
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

    it("Verify Data Access Blocked Datasets Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Blocked Datasets Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/BlockedDatasets`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApit}DataAccess/BlockedDatasets/${HDID}`,
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

    it("Verify Data Access Contact Info Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Data Access Contact Info Service Endpoint: ${config.serviceEndpoints.GatewayApi}DataAccess/ContactInfo`
            );
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}DataAccess/ContactInfo/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Data Access Contact Info Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Contact Info Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/ContactInfo`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApit}DataAccess/ContactInfo/${BOGUSHDID}`,
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

    it("Verify Data Access Contact Info Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Contact Info Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/ContactInfo`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApit}DataAccess/ContactInfo/${HDID}`,
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

    it("Verify Data Access Protected Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Data Access Protected Service Endpoint: ${config.serviceEndpoints.GatewayApi}DataAccess/Protected`
            );
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}DataAccess/Protected/${DEPENDENT_HDID}/${DELEGATE_HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Data Access Protected Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Protected Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/Protected`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}DataAccess/Protected/${BOGUSHDID}/${DELEGATE_HDID}`,
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

    it("Verify Data Access Protected Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Data Access Protected Service Endpoint: ${config.serviceEndpoints.GatewayApit}DataAccess/Protected`
                );
                cy.request({
                    url: `${config.serviceEndpoints.GatewayApi}DataAccess/Protected/${DEPENDENT_HDID}/${DELEGATE_HDID}`,
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
