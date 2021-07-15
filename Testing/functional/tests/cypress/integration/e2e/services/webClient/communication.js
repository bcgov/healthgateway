describe("WebClient Communication Service", () => {
    const BASEURL = "/v1/api/Communication/";

    it("Verify Get Communication", () => {
        cy.request({
            url: `${BASEURL}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(200);
            expect(response.body).to.not.be.null;
        });
    });
});
