import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const dependentHdid = "645645767756756767";
const dependentPhn = "9874307168";

function getDependentCardSelector(timelineEnabled) {
    const dependentId = timelineEnabled ? dependentHdid : dependentPhn;
    return `[data-testid=dependent-card-${dependentId}]`;
}

function setupDependentsPage(
    datasets,
    setupDatasetFixtures,
    timelineEnabled = false
) {
    setupStandardFixtures();
    cy.intercept("GET", "**/UserProfile/*/Dependent", {
        fixture: "UserProfileService/dependent.json",
    });
    setupDatasetFixtures?.();
    cy.configureSettings({
        dependents: {
            enabled: true,
            timelineEnabled,
        },
        datasets,
    });
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        "/dependents"
    );

    cy.get(getDependentCardSelector(timelineEnabled)).should("be.visible");
}

describe("Mobile - Laboratory Orders Report Download", () => {
    beforeEach(() => {
        setupStandardFixtures();

        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });

        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 500,
        });

        cy.log(
            "Verifying Laboratory Report Download returns Internal Server Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=laboratory-report-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=errorBanner]").contains(
            "Unable to download laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Mobile - Covid19 Orders Report Download", () => {
    beforeEach(() => {
        setupStandardFixtures();

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("GET", "**/Laboratory/*/Report*", {
            statusCode: 500,
        });

        cy.log(
            "Verifying Covid19 Orders Report Download returns Internal Server Error"
        );
        cy.get("[data-testid=timelineCard]").last().scrollIntoView().click();

        cy.get("#entry-details-modal")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=covid-result-download-btn]")
                    .should("be.visible")
                    .click({ force: true });
            });

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]")
            .should("be.visible")
            .click({ force: true });

        cy.get("[data-testid=errorBanner]").contains(
            "Unable to download COVID‑19 laboratory report"
        );
        cy.get("[data-testid=backBtn]").click({ force: true });
    });
});

describe("Dependent - Immunizaation History Tab - report download error handling", () => {
    beforeEach(() => {
        setupDependentsPage(
            [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
            () => {
                cy.intercept("GET", "**/Immunization?hdid=*", {
                    fixture: "ImmunizationService/dependentImmunization.json",
                });
            }
        );
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 500,
        });

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .should("be.visible")
            .click();

        // History tab
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("button", "History").click();
            }
        );
        cy.get(
            `[data-testid=immunization-history-table-${dependentHdid}]`
        ).should("be.visible");

        // Click download dropdown under History tab
        cy.log("Validating download history report button.");
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click PDF
        cy.log("Selecting PDF as download report type.");
        cy.get(
            `[data-testid=download-immunization-history-report-pdf-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=errorBanner]").should("not.be.empty");
    });
});

describe("Export Records - Immunizaation - report download error handling", () => {
    beforeEach(() => {
        setupStandardFixtures();
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunization.json",
        }).as("getImmunizations");
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports",
            { waitForInitialDataLoad: true }
        );
    });

    it("Unsuccessful Response: Internal Server Error", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 500,
        });

        cy.vSelect("[data-testid=report-type]", "Immunizations");
        cy.wait("@getImmunizations");

        cy.get("[data-testid=export-record-btn]").click();
        cy.get("[data-testid=export-record-menu] .v-list-item").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");
        cy.get("[data-testid=errorBanner]").should("not.be.empty");
    });
});
