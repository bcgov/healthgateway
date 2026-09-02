describe("Immunization Service", () => {
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
});
