describe("WebClient Note Service", () => {
    const BASEURL = "/v1/api/Note/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get Notes Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get Notes Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
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

    it("Verify Get Notes Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${HDID}`,
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
                expect(response.body.resourcePayload).to.be.an("array").that.is
                    .not.empty;
                expect(response.body.totalResultCount).to.eq(3);
                expect(response.body.resultStatus).to.eq(1);
                expect(response.body.resultError).to.eq(null);
            });
        });
    });

    it("Verify Post Note Unauthorized", () => {
        cy.request({
            method: "POST",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Post Note Forbidden", () => {
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

    it("Verify Put Note Unauthorized", () => {
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Put Note Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "PUT",
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

    it("Verify Delete Note Unauthorized", () => {
        cy.request({
            method: "DELETE",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Delete Note Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "DELETE",
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
