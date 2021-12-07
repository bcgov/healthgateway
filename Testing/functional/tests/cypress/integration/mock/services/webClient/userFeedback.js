describe("WebClient UserFeedback Service", () => {
    const BASEURL = "/v1/api/UserFeedback/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Post UserFeedback Unauthorized", () => {
        cy.request({
            method: "POST",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Post UserFeedback Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "POST",
                url: `${BASEURL}${BOGUSHDID}`,
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
