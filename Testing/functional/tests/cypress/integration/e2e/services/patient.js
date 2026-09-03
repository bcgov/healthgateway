describe("Patient Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";

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
                `Verifying Swagger exists for Patient at Endpoint: ${config.serviceEndpoints.Patient}swagger`
            );
            cy.request(
                `${config.serviceEndpoints.Patient}swagger/v1/swagger.json`
            ).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body.info.title).to.eq(
                    "Health Gateway Patient Services documentation"
                );
            });
        });
    });

    it("Verify Patient Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Patient Service Endpoint: ${config.serviceEndpoints.Patient}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Patient}Patient/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Patient Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Patient Service Endpoint: ${config.serviceEndpoints.Patient}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Patient}Patient/${BOGUSHDID}`,
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

    it("Verify Patient Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Patient Service Endpoint: ${config.serviceEndpoints.Patient}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Patient}Patient/${HDID}`,
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
