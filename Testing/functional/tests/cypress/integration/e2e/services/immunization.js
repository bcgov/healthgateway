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
});
