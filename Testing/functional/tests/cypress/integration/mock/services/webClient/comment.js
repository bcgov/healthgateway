describe("WebClient Comment Service", () => {
    const BASEURL = "/v1/api/UserProfile/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get Comment Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}/Comment/`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Get Comment Authorized", () => {
        cy.fixture("WebClientService/comments.json").then(
            (expectedResponse) => {
                cy.get("@tokens").then((tokens) => {
                    cy.log("Tokens", tokens);
                    cy.request({
                        url: `${BASEURL}${HDID}/Comment/`,
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
                        expect(response.body).to.deep.equal(expectedResponse);
                    });
                });
            }
        );
    });

    it("Verify Post Comment Unauthorized", () => {
        cy.request({
            method: "POST",
            url: `${BASEURL}${HDID}/Comment/`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Post Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "POST",
                url: `${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Put Comment Unauthorized", () => {
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}/Comment/`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Put Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "PUT",
                url: `${BASEURL}${BOGUSHDID}/Comment/`,
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

    it("Verify Delete Comment Unauthorized", () => {
        cy.request({
            method: "DELETE",
            url: `${BASEURL}${HDID}/Comment/`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Delete Comment Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "DELETE",
                url: `${BASEURL}${BOGUSHDID}/Comment/`,
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
