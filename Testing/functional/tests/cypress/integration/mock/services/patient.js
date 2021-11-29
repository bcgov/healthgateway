const { AuthMethod } = require("../../../support/constants");

describe("Patient Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";

    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Patient Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Patient Service Endpoint: ${config.serviceEndpoints.Patient}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Patient}v1/api/Patient/${HDID}`,
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
                    expect(response.body.resourcePayload.hdid).to.eq(HDID);
                    expect(response.body.resourcePayload.firstname).to.not.be
                        .empty;
                    expect(response.body.resourcePayload.personalhealthnumber)
                        .to.not.be.empty;
                });
            });
        });
    });
});
