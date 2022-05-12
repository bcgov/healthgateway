describe("WebClient Communication Service", () => {
    const BASEURL = "/Communication/";

    it("Verify Get Communication", () => {
        cy.logout();
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
