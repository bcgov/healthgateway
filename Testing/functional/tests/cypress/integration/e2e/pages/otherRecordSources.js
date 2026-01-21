const { AuthMethod } = require("../../../support/constants");

const PATH = "/otherRecordSources";
const UNAUTHORIZED_URL = "/unauthorized";

const LINK_CASES = {
    AccessMyHealth: "https://dev.vpp.patientportal.ca-1.healtheintent.com/",
    MyHealth:
        "https://www.islandhealth.ca/our-services/virtual-care-services/myhealth",
    MyHealthPortal: "https://www.interiorhealth.ca/myhealthportal",
    HealthElife:
        "https://www.northernhealth.ca/services/digital-health/healthelife",
    MyHealthKey:
        "https://www.northernhealth.ca/services/digital-health/myhealthkey",
    FraserHealth:
        "https://www.fraserhealth.ca/patients-and-visitors/request-a-health-record",
};

function selectors(type) {
    return {
        card: `[data-testid="other-record-sources-card-${type}"]`,
        link: `[data-testid="other-record-sources-link-${type}"]`,
    };
}

function expectCardVisibleWithUrl(type, expectedUrl) {
    const { card, link } = selectors(type);

    cy.get(card).should("be.visible");
    cy.get(link).should("have.attr", "href").and("include", expectedUrl);
}

describe("Other Record Sources Page", () => {
    it("Other record sources URL goes to Unauthorized when other record resouces feature is turned off", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: false,
                },
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            PATH
        );

        cy.location("pathname", { timeout: 10000 }).should(
            "eq",
            UNAUTHORIZED_URL
        );
    });

    it("Cards are rendered and link to the correct URLs", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: true,
                    sources: [{ name: "accessMyHealth", enabled: true }],
                },
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            PATH
        );

        Object.entries(LINK_CASES).forEach(([type, url]) => {
            expectCardVisibleWithUrl(type, url);
        });
    });

    it("Access My Health card is not rendered when source is disabled", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: true,
                    sources: [{ name: "accessMyHealth", enabled: false }],
                },
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            PATH
        );

        cy.get(selectors("AccessMyHealth").card).should("not.exist");
    });
});
