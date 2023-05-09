const { AuthMethod, Dataset } = require("../../../support/constants");

const authorizedDependentHdid = "162346565465464564565463257";
const unauthorizedDependentHdid = "343222434345442257";
const formattedDependentName = "JENNIFER T";

const homePath = "/home";
const unauthorizedPath = "/unauthorized";

function assertDatasetPresence(dataset, expected = true) {
    const filterSelector = `[data-testid=${toPascalCase(dataset)}-filter]`;
    const timelineCardSelector = `[data-testid=${dataset.toLowerCase()}Title]`;

    cy.get("[data-testid=filterContainer]").should("not.exist");
    cy.get("[data-testid=filterDropdown]").click();
    cy.get("[data-testid=filterContainer]").should("be.visible");

    if (expected) {
        cy.get(filterSelector).should("exist").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").should("be.visible").click();
        cy.get(timelineCardSelector).should("be.visible");
        cy.get("[data-testid=clear-filters-button]")
            .should("be.visible")
            .click();
    } else {
        cy.get(filterSelector).should("not.exist");
        cy.get("[data-testid=btnFilterCancel]").click();
    }
}

function toPascalCase(s) {
    return s.charAt(0).toUpperCase() + s.slice(1);
}

function enabledDatasetShouldBePresent(dataset, overrideDependentHdid = null) {
    const hdid = overrideDependentHdid ?? authorizedDependentHdid;
    cy.configureSettings({
        datasets: [
            {
                name: dataset,
                enabled: true,
            },
        ],
        dependents: {
            enabled: true,
            timelineEnabled: true,
        },
    });

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        `/dependents/${hdid}/timeline`
    );
    cy.checkTimelineHasLoaded();

    cy.log(`Checking ${dataset} dataset is present when enabled`);
    assertDatasetPresence(dataset, true);
}

function disabledDatasetShouldNotBePresent(
    dataset,
    overrideDependentHdid = null
) {
    const hdid = overrideDependentHdid ?? authorizedDependentHdid;
    cy.configureSettings({
        datasets: [
            {
                name: dataset,
                enabled: false,
            },
        ],
        dependents: {
            enabled: true,
            timelineEnabled: true,
        },
    });

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        `/dependents/${hdid}/timeline`
    );
    cy.checkTimelineHasLoaded();

    cy.log(`Checking ${dataset} dataset is not present when disabled`);
    assertDatasetPresence(dataset, false);
}

function disabledDependentDatasetShouldNotBePresent(
    dataset,
    overrideDependentHdid = null
) {
    const hdid = overrideDependentHdid ?? authorizedDependentHdid;
    cy.configureSettings({
        datasets: [
            {
                name: dataset,
                enabled: true,
            },
        ],
        dependents: {
            enabled: true,
            timelineEnabled: true,
            datasets: [
                {
                    name: dataset,
                    enabled: false,
                },
            ],
        },
    });

    cy.visit(`/dependents/${hdid}/timeline`);
    cy.checkTimelineHasLoaded();

    cy.log(
        `Checking ${dataset} dataset is not present when dependent dataset is disabled`
    );
    assertDatasetPresence(dataset, false);
}

describe("Dependent Timeline", () => {
    beforeEach(() => {
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "diagnosticImaging",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
            dependents: {
                enabled: true,
                timelineEnabled: true,
                datasets: [
                    {
                        name: "healthVisit",
                        enabled: false,
                    },
                    {
                        name: "hospitalVisit",
                        enabled: false,
                    },
                    {
                        name: "note",
                        enabled: false,
                    },
                ],
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.location("pathname").should("eq", homePath);
    });

    it("Validate timeline for unauthorized dependent is inaccessible", () => {
        cy.visit(`/dependents/${unauthorizedDependentHdid}/timeline`);
        cy.location("pathname").should("eq", unauthorizedPath);
    });

    it("Validate timeline for authorized dependent is accessible and commenting is unavailable", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=addCommentTextArea]").should("not.exist");
        cy.get("[data-testid=postCommentBtn]").should("not.exist");
    });

    it("Validate back button goes to dependents page", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=backBtn]").should("be.visible").click();
        cy.location("pathname").should("eq", `/dependents`);
    });

    it("Validate page title includes formatted name", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=page-title]")
            .should("be.visible")
            .contains(formattedDependentName);
    });

    it("Validate bread crumb includes formatted name", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=breadcrumb-dependent-name]")
            .should("be.visible")
            .contains(formattedDependentName);
    });

    it("Validate bread crumb link goes to dependents page", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=breadcrumb-dependents]")
            .should("be.visible")
            .click();
        cy.location("pathname").should("eq", `/dependents`);
    });
});

describe("Dependent Timeline Datasets", () => {
    it("Validate clinical documents on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.ClinicalDocument);
        disabledDatasetShouldNotBePresent(Dataset.ClinicalDocument);
        disabledDependentDatasetShouldNotBePresent(Dataset.ClinicalDocument);
    });
    it("Validate COVID-19 test results on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.Covid19TestResult);
        disabledDatasetShouldNotBePresent(Dataset.Covid19TestResult);
        disabledDependentDatasetShouldNotBePresent(Dataset.Covid19TestResult);
    });
    it("Validate immunizations on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.Immunization);
        disabledDatasetShouldNotBePresent(Dataset.Immunization);
        disabledDependentDatasetShouldNotBePresent(Dataset.Immunization);
    });
    it("Validate lab results on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.LabResult);
        disabledDatasetShouldNotBePresent(Dataset.LabResult);
        disabledDependentDatasetShouldNotBePresent(Dataset.LabResult);
    });
    it("Validate hospital visits on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.HospitalVisit);
        disabledDatasetShouldNotBePresent(Dataset.HospitalVisit);
        disabledDependentDatasetShouldNotBePresent(Dataset.HospitalVisit);
    });
    it("Validate health visits on dependent timeline", () => {
        // Michael Testertwo
        const hdid = "BNV554213556";
        enabledDatasetShouldBePresent(Dataset.HealthVisit, hdid);
        disabledDatasetShouldNotBePresent(Dataset.HealthVisit, hdid);
        disabledDependentDatasetShouldNotBePresent(Dataset.HealthVisit, hdid);
    });
    it("Validate medications on dependent timeline", () => {
        // Michael Testertwo
        const hdid = "BNV554213556";
        enabledDatasetShouldBePresent(Dataset.Medication, hdid);
        disabledDatasetShouldNotBePresent(Dataset.Medication, hdid);
        disabledDependentDatasetShouldNotBePresent(Dataset.Medication, hdid);
    });
    it("Validate (lack of) notes on dependent timeline", () => {
        disabledDatasetShouldNotBePresent(Dataset.Note);
        disabledDependentDatasetShouldNotBePresent(Dataset.Note);
    });
    it("Validate Diagnostic Imaging requests on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.DiagnosticImaging);
        disabledDatasetShouldNotBePresent(Dataset.DiagnosticImaging);
        disabledDependentDatasetShouldNotBePresent(Dataset.DiagnosticImaging);
    });
    it("Validate Special Authority requests on dependent timeline", () => {
        enabledDatasetShouldBePresent(Dataset.SpecialAuthorityRequest);
        disabledDatasetShouldNotBePresent(Dataset.SpecialAuthorityRequest);
        disabledDependentDatasetShouldNotBePresent(
            Dataset.SpecialAuthorityRequest
        );
    });
});
