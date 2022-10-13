describe("Clinical Documents Service", () => {
    const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

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
                `Verifying Swagger exists for Clinical Documents at Endpoint: ${config.serviceEndpoints.ClinicalDocument}swagger`
            );
            cy.visit(
                `${config.serviceEndpoints.ClinicalDocument}swagger`
            ).contains(
                "Health Gateway Clinical Document Services documentation"
            );
        });
    });

    it("Verify Clinical Document Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
            );
            cy.request({
                url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Clinical Document Forbidden", () => {
        const BOGUSHDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${BOGUSHDID}`,
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

    it("Verify Clinical Document Records Authorized", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}`,
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
                    expect(response.body.resourcePayload).to.not.be.null;
                    expect(response.body.resourcePayload[0].id).to.not.be.empty;
                    expect(response.body.resourcePayload[0].name).to.not.be
                        .empty;
                    expect(response.body.resourcePayload[0].fileId).to.not.be
                        .empty;
                });
            });
        });
    });

    it("Verify Clinical Document File Authorized", () => {
        const FILEID =
            "14bac0b6-9e95-4a1b-b6fd-d354edfce4e7-710b28fa980440fd93c426e25c0ce52f";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Clinical Document Service Endpoint: ${config.serviceEndpoints.ClinicalDocument}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.ClinicalDocument}ClinicalDocument/${HDID}/file/${FILEID}`,
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
                    expect(response.body.resourcePayload).to.not.be.null;
                });
            });
        });
    });
});
