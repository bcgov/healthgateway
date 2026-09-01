import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const path = "/otherRecordSources";
const linkCases = {
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

const selectors = (type) => ({
    card: `[data-testid=other-record-sources-card-${type}]`,
    link: `[data-testid=other-record-sources-link-${type}]`,
});

function configureOtherRecordSources(accessMyHealthEnabled = true) {
    cy.configureSettings({
        homepage: {
            otherRecordSources: {
                enabled: true,
                sources: [
                    {
                        name: "accessMyHealth",
                        enabled: accessMyHealthEnabled,
                    },
                ],
            },
        },
    });
    setupStandardFixtures();
}

function login() {
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        path,
        { waitForInitialDataLoad: true }
    );
}

function stubWindowOpen() {
    cy.window().then((window) => {
        cy.stub(window, "open").as("windowOpen");
    });
}

function proceedThroughAccessMyHealthPrompt(expectedUrl) {
    cy.get("[data-testid=external-link-confirmation-dialog]").should(
        "be.visible"
    );
    cy.get("@windowOpen").should("not.have.been.called");
    cy.get(
        "[data-testid=external-link-confirmation-dialog-proceed-button]"
    ).click();
    cy.get("@windowOpen").should(
        "have.been.calledWithMatch",
        expectedUrl,
        "_blank",
        "noopener"
    );
}

describe("Other Record Sources Page", () => {
    it("Redirects to unauthorized when the feature is disabled", () => {
        cy.configureSettings({
            homepage: {
                otherRecordSources: {
                    enabled: false,
                },
            },
        });
        setupStandardFixtures();
        login();

        cy.location("pathname").should("eq", "/unauthorized");
    });

    it("Opens source links and prompts only for Access My Health", () => {
        configureOtherRecordSources();
        login();
        stubWindowOpen();

        cy.wrap(Object.entries(linkCases)).each(([type, expectedUrl]) => {
            const { card, link } = selectors(type);
            cy.get(card).should("be.visible");
            cy.get(link).should("be.visible");
            cy.get("@windowOpen").invoke("resetHistory");
            cy.get(link).click();

            if (type === "AccessMyHealth") {
                proceedThroughAccessMyHealthPrompt(expectedUrl);
                cy.get(
                    "[data-testid=external-link-confirmation-dialog]"
                ).should("not.exist");
            } else {
                cy.get(
                    "[data-testid=external-link-confirmation-dialog]"
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

    it("Handles card clicks and cancellation", () => {
        configureOtherRecordSources();
        login();
        stubWindowOpen();

        cy.get(selectors("MyHealth").card).click();
        cy.get("@windowOpen").should(
            "have.been.calledWithMatch",
            linkCases.MyHealth,
            "_blank",
            "noopener"
        );

        cy.get("@windowOpen").invoke("resetHistory");
        cy.get(selectors("AccessMyHealth").card).click();
        cy.get(
            "[data-testid=external-link-confirmation-dialog-cancel-button]"
        ).click();
        cy.get("[data-testid=external-link-confirmation-dialog]").should(
            "not.exist"
        );
        cy.get("@windowOpen").should("not.have.been.called");
    });

    it("Hides Access My Health when that source is disabled", () => {
        configureOtherRecordSources(false);
        login();

        cy.get(selectors("AccessMyHealth").card).should("not.exist");
    });
});
