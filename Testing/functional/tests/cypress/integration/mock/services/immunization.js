const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Immunization Service", () => {
    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
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
                    url: `${config.serviceEndpoints.Immunization}v1/api/Immunization?hdid=${HDID}`,
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
                    cy.log(`response.body: ${JSON.stringify(response.body)}`);
                    expect(response.body.resourcePayload).to.not.be.null;
                });
            });
        });
    });
});
