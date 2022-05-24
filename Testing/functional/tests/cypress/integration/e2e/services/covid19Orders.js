describe("Patient Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const EXPIREDELEGATEDHDID = "232434345442257";
    const DELEGATEDHDID = "162346565465464564565463257";

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
                `Verifying Swagger exists for Laboratory at Endpoint: ${config.serviceEndpoints.Laboratory}swagger`
            );
            cy.visit(`${config.serviceEndpoints.Laboratory}swagger`).contains(
                "Health Gateway Laboratory Services documentation"
            );
        });
    });

    it("Verify COVID-19 Orders Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Laboratory}Laboratory/Covid19Orders?hdid=${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify COVID-19 Orders Expired Delegate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Laboratory}Laboratory/Covid19Orders?hdid=${EXPIREDELEGATEDHDID}`,
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

    it("Verify COVID-19 Orders Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Laboratory}Laboratory/Covid19Orders?hdid=${HDID}`,
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

    it("Verify COVID-19 Orders Delegate Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Laboratory}Laboratory/Covid19Orders?hdid=${DELEGATEDHDID}`,
                    followRedirect: false,
                    failOnStatusCode: false,
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
