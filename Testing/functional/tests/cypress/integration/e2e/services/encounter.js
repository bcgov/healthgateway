describe("Encounter Service", () => {
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
                `Verifying Swagger exists for Encounter at Endpoint: ${config.serviceEndpoints.Encounter}swagger`
            );
            cy.visit(`${config.serviceEndpoints.Encounter}swagger`).contains(
                "Health Gateway Encounter Services documentation"
            );
        });
    });

    it("Verify Encounter Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Encounter}Encounter/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Encounter Forbidden", () => {
        const HDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Encounter}Encounter/${HDID}`,
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

    it("Verify Distinct Encounters", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Encounter}Encounter/${HDID}`,
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
                    let distinctEncounters = new Set();
                    let error = "";
                    response.body.resourcePayload.forEach((enc) => {
                        let combinedKey = `${enc.encounterDate}${enc.specialtyDescription}${enc.practitionerName}${enc.clinic.name}${enc.clinic.addressLine1}${enc.clinic.addressLine2}${enc.clinic.addressLine3}${enc.clinic.addressLine4}${enc.clinic.city}${enc.clinic.postalCode}${enc.clinic.province}`;
                        if (distinctEncounters.has(combinedKey)) {
                            error += `dup encounter: ${combinedKey} |`;
                        } else {
                            distinctEncounters.add(combinedKey);
                        }
                    });
                    expect(error).to.equal("");
                });
            });
        });
    });

    it("Verify Hospital Visit Unauthorized", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Encounter}Encounter/HospitalVisit/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A`,
                followRedirect: false,
                failOnStatusCode: false,
            }).should((response) => {
                expect(response.status).to.eq(401);
            });
        });
    });

    it("Verify Hospital Visit Forbidden", () => {
        const HDID = "BOGUSHDID";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Encounter}Encounter/HospitalVisit/${HDID}`,
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

    it("Verify Hospital Visits Delegate Authorized", () => {
        const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`
                );
                cy.request({
                    url: `${config.serviceEndpoints.Encounter}Encounter/HospitalVisit/${HDID}`,
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
                });
            });
        });
    });
});
