describe("Immunization Service", () => {
    const AGED_OUT_HDID = "286338699276932223";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUS_HDID = "BOGUSHDID";

    let tokens;

    before(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).then((result) => {
            tokens = result;
        });
    });

    beforeEach(() => {
        cy.readConfig().as("config");
        cy.wrap(tokens).as("tokens");
    });

    it("Verify Swagger", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Verifying Swagger exists for Immunization at Endpoint: ${config.serviceEndpoints.Immunization}swagger`
            );
            cy.request(
                `${config.serviceEndpoints.Immunization}swagger/v1/swagger.json`
            ).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body.info.title).to.eq(
                    "Health Gateway Immunization Services documentation"
                );
            });
        });
    });

    it("Verify Immunization V1 Expired Delegate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service V1 Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${AGED_OUT_HDID}&api-version=1.0`,
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

    it("Verify V2 Swagger", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Verifying V2 Swagger exists for Immunization at Endpoint: ${config.serviceEndpoints.Immunization}swagger`
            );
            cy.request(
                `${config.serviceEndpoints.Immunization}swagger/v2/swagger.json`
            ).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body.info.version).to.eq("2.0");
                expect(response.body.paths).to.have.property("/Immunization");
            });
        });
    });

    it("Verify Immunization V2 Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Immunization Service V2 Endpoint: ${config.serviceEndpoints.Immunization}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${HDID}&api-version=2.0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Immunization V2 Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service V2 Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${BOGUS_HDID}&api-version=2.0`,
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

    it("Verify Immunization V2 Expired Delegate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service V2 Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${AGED_OUT_HDID}&api-version=2.0`,
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

    it("Verify Immunization V2 Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service V2 Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${HDID}&api-version=2.0`,
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
