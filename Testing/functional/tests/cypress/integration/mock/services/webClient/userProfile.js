describe("WebClient UserProfile Service", () => {
    const BASEURL = "/UserProfile/";
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
    const BOGUSHDID = "BOGUSHDID";
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Get UserProfile Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get UserProfile Forbidden", () => {
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

    it("Verify Get UserProfile Authorized", () => {
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
                cy.log("Response", response);
                expect(response.status).to.eq(200);
                expect(response.body).to.not.be.null;
                expect(response.body.resourcePayload.hdId).to.equal(HDID);
            });
        });
    });

    it("Verify Post UserProfile Unauthorized", () => {
        cy.request({
            method: "POST",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Post UserProfile Forbidden", () => {
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

    it("Verify Delete UserProfile Unauthorized", () => {
        cy.request({
            method: "DELETE",
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Delete UserProfile Forbidden", () => {
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

    it("Verify Get UserProfile TermsOfService", () => {
        cy.request({
            url: `${BASEURL}termsofservice`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(200);
        });
    });

    it("Verify Get UserProfile Validate Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}/Validate`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get UserProfile Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/Validate`,
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

    it("Verify Get UserProfile Recover Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}/recover`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get UserProfile Recover Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/recover`,
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

    it("Verify Get UserProfile Email Validate Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}/email/validate/123`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get UserProfile Email Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/email/validate/123`,
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

    it("Verify Put UserProfile Email Unauthorized", () => {
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}/email`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Put UserProfile Email Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "PUT",
                url: `${BASEURL}${BOGUSHDID}/email`,
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

    it("Verify Get UserProfile SMS Validate Unauthorized", () => {
        cy.request({
            url: `${BASEURL}${HDID}/sms/validate/123`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Get UserProfile SMS Validate Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/sms/validate/123`,
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

    it("Verify Put UserProfile SMS Unauthorized", () => {
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}/sms`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Put UserProfile SMS Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "PUT",
                url: `${BASEURL}${BOGUSHDID}/sms`,
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

    it("Verify Put UserProfile Preference Unauthorized", () => {
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}/preference`,
            followRedirect: false,
            failOnStatusCode: false,
        }).should((response) => {
            expect(response.status).to.eq(401);
        });
    });

    it("Verify Put UserProfile Preference Forbidden", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.request({
                method: "PUT",
                url: `${BASEURL}${BOGUSHDID}/preference`,
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
