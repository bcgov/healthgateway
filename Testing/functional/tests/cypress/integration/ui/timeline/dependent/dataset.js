import { AuthMethod, Dataset } from "../../../../support/constants";
import { setupStandardFixtures } from "../../../../support/functions/intercept";

const dependentHdid = "645645767756756767";
const dependentTimelinePath = `/dependents/${dependentHdid}/timeline`;

const enabledDatasets = [
    Dataset.ClinicalDocument,
    Dataset.Covid19TestResult,
    Dataset.Immunization,
    Dataset.DiagnosticImaging,
];

function toPascalCase(value) {
    return value.charAt(0).toUpperCase() + value.slice(1);
}

function setupDependentFixture() {
    cy.intercept("GET", "**/UserProfile/*/Dependent*", {
        fixture: "UserProfileService/dependent.json",
    }).as("getDependentFixture");
}

function setupEnabledDatasetFixtures() {
    cy.intercept("GET", "**/ClinicalDocument/*", {
        fixture: "ClinicalDocumentService/clinicalDocument.json",
    }).as("getDependentClinicalDocuments");
    cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
        fixture: "LaboratoryService/covid19Orders.json",
    }).as("getDependentCovid19Orders");
    cy.intercept("GET", "**/Immunization?hdid=*", {
        fixture: "ImmunizationService/dependentImmunization.json",
    }).as("getDependentImmunizations");
    cy.intercept("GET", "**/PatientData/*?patientDataTypes=*", {
        fixture: "PatientData/allTimelineData.json",
    }).as("getDependentPatientData");
}

function loginToDependentTimeline() {
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        dependentTimelinePath
    );
    cy.wait("@getDependentFixture");
}

describe("Dependent Timeline Datasets", () => {
    beforeEach(() => {
        setupStandardFixtures();
        setupDependentFixture();
    });

    it("Displays enabled dependent datasets", () => {
        cy.configureSettings({
            datasets: enabledDatasets.map((name) => ({
                name,
                enabled: true,
            })),
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
        });
        setupEnabledDatasetFixtures();

        loginToDependentTimeline();
        cy.wait([
            "@getDependentClinicalDocuments",
            "@getDependentCovid19Orders",
            "@getDependentImmunizations",
            "@getDependentPatientData",
        ]);
        cy.checkTimelineHasLoaded();

        // Dataset rendering is checked here; generic filter behavior is covered
        // by the UI filter suite and does not need to be repeated per dataset.
        enabledDatasets.forEach((dataset) => {
            cy.get(`[data-testid=${dataset.toLowerCase()}Title]`).should(
                "be.visible"
            );
        });

        cy.get("[data-testid=filterDropdown]").click();
        enabledDatasets.forEach((dataset) => {
            cy.get(`[data-testid=${toPascalCase(dataset)}-filter]`).should(
                "exist"
            );
        });
    });

    it("Hides a dataset when it is globally disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: Dataset.ClinicalDocument,
                    enabled: false,
                },
            ],
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
        });

        loginToDependentTimeline();
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=filterDropdown]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
    });

    it("Hides a globally enabled dataset when it is disabled for dependents", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: Dataset.ClinicalDocument,
                    enabled: true,
                },
            ],
            dependents: {
                enabled: true,
                timelineEnabled: true,
                datasets: [
                    {
                        name: Dataset.ClinicalDocument,
                        enabled: false,
                    },
                ],
            },
        });

        loginToDependentTimeline();
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=filterDropdown]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
    });
});
