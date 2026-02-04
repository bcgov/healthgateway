import { AuthMethod } from "../../../support/constants";

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

function expectCardVisible(type) {
    cy.get(selectors(type).card).should("be.visible");
    cy.get(selectors(type).link).should("be.visible");
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

    it("Record source links open URLs and prompt only for AccessMyHealth", () => {
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

        // We stub window.open so Cypress can intercept and assert that a new tab would have opened,
        // without actually opening a real browser tab (which Cypress cannot control or verify).
        cy.window().then((win) => {
            cy.stub(win, "open").as("windowOpen");
        });

        cy.wrap(Object.entries(LINK_CASES)).each(([type, expectedUrl]) => {
            expectCardVisible(type);

            // Clear call history between tiles
            cy.get("@windowOpen").invoke("resetHistory");

            const { link } = selectors(type);
            cy.log(`Clicking link data-testid: ${link}`);

            cy.get(link).should("be.visible").click();

            if (type === "AccessMyHealth") {
                // Confirmation prompt
                cy.get(
                    '[data-testid="external-link-confirmation-dialog"]'
                ).should("be.visible");
                cy.get("@windowOpen").should("not.have.been.called");

                cy.get(
                    '[data-testid="external-link-confirmation-dialog-proceed-button"]'
                ).click();

                // New tab
                cy.get("@windowOpen").should(
                    "have.been.calledWithMatch",
                    expectedUrl,
                    "_blank",
                    "noopener"
                );

                cy.get(
                    '[data-testid="external-link-confirmation-dialog"]'
                ).should("not.exist");
            } else {
                // No confirmation prompt required
                cy.get(
                    '[data-testid="external-link-confirmation-dialog"]'
                ).should("not.exist");

                cy.get("@windowOpen").should(
                    "have.been.calledWithMatch",
                    expectedUrl,
                    "_blank",
                    "noopener"
                );
            }
        });
    });

    it("Card clicks open correct URLs (prompt only for AccessMyHealth)", () => {
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

        cy.window().then((win) => {
            cy.stub(win, "open").as("windowOpen");
        });

        // Non-AccessMyHealth: MyHealth card opens immediately
        cy.get("@windowOpen").invoke("resetHistory");
        cy.get(selectors("MyHealth").card).should("be.visible").click();
        cy.get('[data-testid="external-link-confirmation-dialog"]').should(
            "not.exist"
        );
        cy.get("@windowOpen").should(
            "have.been.calledWithMatch",
            LINK_CASES.MyHealth,
            "_blank",
            "noopener"
        );

        // AccessMyHealth: card click prompts, then opens on Proceed
        cy.get("@windowOpen").invoke("resetHistory");
        cy.get(selectors("AccessMyHealth").card).should("be.visible").click();

        cy.get('[data-testid="external-link-confirmation-dialog"]').should(
            "be.visible"
        );
        cy.get("@windowOpen").should("not.have.been.called");

        cy.get(
            '[data-testid="external-link-confirmation-dialog-proceed-button"]'
        ).click();
        cy.get("@windowOpen").should(
            "have.been.calledWithMatch",
            LINK_CASES.AccessMyHealth,
            "_blank",
            "noopener"
        );
    });

    it("Access My Health prompt closes when Cancel is clicked", () => {
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

        // Stub window.open to ensure it is NOT called
        cy.window().then((win) => {
            cy.stub(win, "open").as("windowOpen");
        });

        cy.get(selectors("AccessMyHealth").link).click();

        cy.get('[data-testid="external-link-confirmation-dialog"]').should(
            "be.visible"
        );

        cy.get(
            '[data-testid="external-link-confirmation-dialog-cancel-button"]'
        ).click();

        cy.get('[data-testid="external-link-confirmation-dialog"]').should(
            "not.exist"
        );

        // Ensure navigation did NOT occur
        cy.get("@windowOpen").should("not.have.been.called");
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
