const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("PCR Lab Test Kit Service", () => {
    beforeEach(() => {
        cy.readConfig().as("config");
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
    });

    it("Verify Laboratory Swagger", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Verifying Swagger exists for Laboratory at Endpoint: ${config.serviceEndpoints.Laboratory}swagger`
            );
            cy.visit(`${config.serviceEndpoints.Laboratory}swagger`).contains(
                "Health Gateway Laboratory Services documentation"
            );
        });
    });

    it("Verify Laboratory PCR lab test kit service unavailable", () => {
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            cy.get("@config").then((config) => {
                cy.log(
                    `Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
                );
                cy.request({
                    method: "POST",
                    url: `${config.serviceEndpoints.Laboratory}Laboratory/${HDID}/LabTestKit`,
                    followRedirect: false,
                    failOnStatusCode: false,
                    auth: {
                        bearer: tokens.access_token,
                    },
                    headers: {
                        accept: "application/json",
                    },
                    body: {
                        testTakenMinutesAgo: 5,
                        testKitCid: "222BAAB1-8C6E-4FA1-86ED-C4E3517A16A2",
                        shortCodeFirst: "",
                        shortCodeSecond: "",
                    },
                }).should((response) => {
                    expect(response.status).to.eq(503);
                });
            });
        });
    });

    it("Verify Public Laboratory PCR lab test kit service unavailable", () => {
        cy.get("@config").then((config) => {
            cy.log(
                `Public Laboratory Service Endpoint: ${config.serviceEndpoints.Laboratory}`
            );

            cy.request({
                method: "POST",
                url: `${config.serviceEndpoints.Laboratory}PublicLaboratory/LabTestKit`,
                followRedirect: false,
                failOnStatusCode: false,
                headers: {
                    "Content-Type": "application/json",
                    "api-version": "1.0",
                },
                body: {
                    phn: "9879454009",
                    dob: "1928-07-15T00:00:00",
                    firstName: "ZELDA",
                    lastName: "BCYPCST",
                    testTakenMinutesAgo: 360,
                    testKitCid: "",
                    shortCodeFirst: "45YFKE7",
                    shortCodeSecond: "LEKT3",
                    contactPhoneNumber: "7782223693",
                    streetAddress: "",
                    city: "",
                    postalOrZip: "",
                },
            }).should((response) => {
                expect(response.status).to.eq(503);
            });
        });
    });
});
