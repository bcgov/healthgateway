describe("Medication Service", () => {
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
                `Verifying Swagger exists for Medication at Endpoint: ${config.serviceEndpoints.Medication}swagger`
            );
            cy.visit(`${config.serviceEndpoints.Medication}swagger`).contains(
                "Health Gateway Medication Services documentation"
            );
        });
    });

    it("Verify MedicationStatement Unauthorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@config").then((config) => {
            cy.log(
                `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Medication}MedicationStatement/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify MedicationStatement Forbidden", () => {
        const HDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Medication}MedicationStatement/${HDID}`,
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

    it("Verify MedicationStatement Authorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Medication}MedicationStatement/${HDID}`,
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
                    expect(
                        response.body.resourcePayload.length
                    ).to.be.greaterThan(250);
                    expect(
                        response.body.resourcePayload[100]
                            .prescriptionIdentifier
                    ).to.match(/^\d{7}$/);
                });
            });
        });
    });

    it("Verify MedicationRequest Unauthorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@config").then((config) => {
            cy.log(
                `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Medication}MedicationRequest/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify MedicationRequest Forbidden", () => {
        const HDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Medication}MedicationRequest/${HDID}`,
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

    it("Verify MedicationRequest Authorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Medication}MedicationRequest/${HDID}`,
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
                    expect(
                        response.body.resourcePayload.length
                    ).to.be.greaterThan(0);
                    expect(response.body.resourcePayload[0].referenceNumber).to
                        .not.be.empty;
                });
            });
        });
    });
});
