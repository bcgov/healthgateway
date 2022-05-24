import {
    verifyProvincialDrug,
    verifyFedDrug,
} from "../../../support/functions/medication";

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

    it("Verify Get Medications List", () => {
        const drugIdentifiersQueryString =
            "drugIdentifiers=66999990&drugIdentifiers=02391724&drugIdentifiers=02212048&drugIdentifiers=02263238&drugIdentifiers=00496499&drugIdentifiers=02042258&drugIdentifiers=02240550&drugIdentifiers=01926799&drugIdentifiers=02237250&drugIdentifiers=02252716&drugIdentifiers=02263254&drugIdentifiers=02046121&drugIdentifiers=02271958&drugIdentifiers=02240606&drugIdentifiers=02247701&drugIdentifiers=02046148&drugIdentifiers=00402753&drugIdentifiers=00029246";
        cy.get("@config").then((config) => {
            cy.log(
                `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Medication}Medication/?${drugIdentifiersQueryString}`,
                followRedirect: false,
                headers: {
                    accept: "application/json",
                },
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body).to.not.be.null;
                cy.log(`response.body: ${response.body}`);
                expect(response.body.totalResultCount).to.equal(18);
                expect(response.body.resourcePayload).to.not.be.null;
                const drug66999990 = response.body.resourcePayload["66999990"];
                verifyProvincialDrug(drug66999990);

                const drugFederal = response.body.resourcePayload["02391724"];
                verifyFedDrug(drugFederal);
            });
        });
    });

    it("Verify GetMedication ProvincialDrug", () => {
        const drugIdentifier = "02391724";
        cy.get("@config").then((config) => {
            cy.log(
                `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Medication}Medication/${drugIdentifier}`,
                followRedirect: false,
                headers: {
                    accept: "application/json",
                },
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body).to.not.be.null;
                cy.log(`response.body: ${response.body}`);
                verifyFedDrug(response.body.resourcePayload);
            });
        });
    });

    it("Verify GetMedication FEDDrug", () => {
        const drugIdentifier = "66999990";
        cy.get("@config").then((config) => {
            cy.log(
                `Medication Service Endpoint: ${config.serviceEndpoints.Medication}`
            );
            cy.request({
                url: `${config.serviceEndpoints.Medication}Medication/${drugIdentifier}`,
                followRedirect: false,
                headers: {
                    accept: "application/json",
                },
            }).should((response) => {
                expect(response.status).to.eq(200);
                expect(response.body).to.not.be.null;
                cy.log(`response.body: ${response.body}`);
                verifyProvincialDrug(response.body.resourcePayload);
            });
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
                    cy.log(`response.body: ${response.body}`);
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
                    cy.log(`response.body: ${response.body}`);
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
