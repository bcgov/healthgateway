describe("GatewayApi Service", () => {
    it("Verify Swagger", () => {
        cy.logout();
        cy.readConfig().then((config) => {
            cy.log(
                "Verifying Swagger exists for GatewayAPI at ${config.serviceEndpoints.GatewayApi}swagger"
            );
            cy.request({
                method: "GET",
                url: `${config.serviceEndpoints.GatewayApi}swagger/index.html`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body).to.contain("Swagger UI");
            });
        });
    });

    it("Verify Payload", () => {
        cy.logout();
        cy.readConfig().then((config) => {
            cy.log(
                "Verifying swagger.json payload for GatewayAPI at ${config.serviceEndpoints.GatewayApi}swagger"
            );
            cy.request({
                method: "GET",
                url: `${config.serviceEndpoints.GatewayApi}swagger/v1/swagger.json`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body.info.title).to.contain(
                    "Health Gateway API Services documentation"
                );
            });
        });
    });
});
