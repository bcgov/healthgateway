const { AuthMethod } = require("../../../support/constants");

const PATH = "/otherRecordSources";

const LINK_CASES = {
    AccessMyHealth: "",
    MyHealth:
        "https://www.islandhealth.ca/our-services/virtual-care-services/myhealth",
    MyHealthPortal: "https://www.interiorhealth.ca/myhealthportal",
    HealthElife:
        "https://www.northernhealth.ca/services/digital-health/healthelife",
    MyHealthKey:
        "https://www.northernhealth.ca/services/digital-health/healthelife",
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
    it("Access My Health card is not rendered when services are disabled", () => {
        cy.configureSettings({});

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            PATH
        );

        cy.get(selectors("AccessMyHealth").card).should("not.exist");
    });

    it("Cards are rendered and link to the correct URLs", () => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [{ name: "accessMyHealth", enabled: true }],
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
});
