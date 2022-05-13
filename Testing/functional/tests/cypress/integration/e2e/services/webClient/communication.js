describe("WebClient Communication Service", () => {
    const BASEURL = "Communication/";

    it("Verify Get Communication", () => {
        cy.logout();
        cy.readConfig().then((config) => {
            cy.request({
                url: `${config.serviceEndpoints.GatewayApi}${BASEURL}/0`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body).to.not.be.null;
            });
        });
    });
});
