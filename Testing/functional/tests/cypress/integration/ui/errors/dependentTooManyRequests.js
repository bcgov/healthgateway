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

describe("Dependents", () => {
    const timelineEnabled = false;
    // Use a different dependent because the fixture dependent would fail duplicate validation.
    const alternativeDependent = {
        firstName: "Sammy",
        lastName: "Testfivey",
        doB: "2025-Mar-15",
        testDate: "2020-Mar-21",
        phn: "9735361219",
        hdid: "645645767756756767",
    };

    beforeEach(() => {
        setupDependentsPage(
            [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
            () => {
                cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
                    fixture: "LaboratoryService/covid19Orders.json",
                });
                cy.intercept("GET", "**/Immunization?hdid=*", {
                    fixture: "ImmunizationService/dependentImmunization.json",
                });
            },
            timelineEnabled
        );
    });

    it("Delete Dependent: Too Many Requests Error", () => {
        cy.intercept("DELETE", "**/UserProfile/*/Dependent/*", {
            statusCode: 429,
        });
        cy.get(getDependentCardSelector(timelineEnabled)).within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
        });
        cy.get("[data-testid=deleteDependentMenuBtn]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });

    it("Add Dependent: Too Many Requests Error", () => {
        cy.intercept("POST", "**/UserProfile/*/Dependent", {
            statusCode: 429,
        });
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(alternativeDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(alternativeDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(alternativeDependent.doB);
        cy.get("[data-testid=dependent-phn-input]  input")
            .clear()
            .type(alternativeDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox]  input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
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

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("POST", "**/Report", {
            statusCode: 429,
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

        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    });
});
