describe("Immunization Service", () => {
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
                `Verifying Swagger exists for Immunization at Endpoint: ${config.serviceEndpoints.Immunization}swagger`
            );
            cy.visit(`${config.serviceEndpoints.Immunization}swagger`).contains(
                "Health Gateway Immunization Services documentation"
            );
        });
    });

    it("Verify Immunization Unauthorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@config").then((config) => {
            cy.log(
                `Immunization Service Endpoint: ${config.serviceEndpoints.Immunization}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Immunization}Immunization/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Immunization Forbidden", () => {
        const HDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization/${HDID}`,
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

    it("Verify Immunization Authorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Immunization Service Endpoint: ${config.serviceEndpoints.Immunization}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Immunization}Immunization?hdid=${HDID}`,
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
                    expect(response.body.resourcePayload).to.not.be.null;
                });
            });
        });
    });
});
