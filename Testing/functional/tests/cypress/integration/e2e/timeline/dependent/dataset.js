import { AuthMethod, Dataset } from "../../../../support/constants";

const authorizedSpecialAuthorityDependentHdid = "IASGH65211V6WHXKGQDSEJAHYMYR";
const authorizedDependentHdid = "162346565465464564565463257";

function assertDatasetPresence(dataset, expected = true) {
    const filterSelector = `[data-testid=${toPascalCase(dataset)}-filter]`;
    const timelineCardSelector = `[data-testid=${dataset.toLowerCase()}Title]`;

    cy.get("[data-testid=filterContainer]").should("not.exist");

    if (expected) {
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get(filterSelector).should("exist").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").should("be.visible").click();
        cy.get(timelineCardSelector).should("be.visible");
        cy.get("[data-testid=clear-filters-button]")
            .should("be.visible")
            .click();
    } else {
        cy.get("[data-testid=filterDropdown]").should("not.exist");
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

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        `/dependents/${hdid}/timeline`
    );

    // cy.visit(`/dependents/${hdid}/timeline`);
    cy.checkTimelineHasLoaded();

    cy.log(
        `Checking ${dataset} dataset is not present when dependent dataset is disabled`
    );
    assertDatasetPresence(dataset, false);
}

describe("Dependent Timeline Datasets", () => {
    describe("Validate clinical documents on dependent timeline", () => {
        it("Enabled ClinicalDocument dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.ClinicalDocument);
        });
        it("Disabled ClinicalDocument dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.ClinicalDocument);
        });
        it("Disabled dependent ClinicalDocument dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.ClinicalDocument
            );
        });
    });
    describe("Validate COVID-19 test results on dependent timeline", () => {
        it("Enabled Covid19TestResult dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.Covid19TestResult);
        });
        it("Disabled Covid19TestResult dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.Covid19TestResult);
        });
        it("Disabled dependent Covid19TestResult dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.Covid19TestResult
            );
        });
    });
    describe("Validate immunizations on dependent timeline", () => {
        it("Enabled Immunization dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.Immunization);
        });
        it("Disabled Immunization dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.Immunization);
        });
        it("Disabled dependent Immunization dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(Dataset.Immunization);
        });
    });

    // test should be skipped until PHSA fixes test data for this dependent
    describe("Validate lab results on dependent timeline", () => {
        it.skip("Enabled LabResult dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.LabResult);
        });
        it.skip("Disabled LabResult dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.LabResult);
        });
        it.skip("Disabled dependent LabResult dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(Dataset.LabResult);
        });
    });

    describe("Validate hospital visits on dependent timeline", () => {
        it("Enabled HospitalVisit dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.HospitalVisit);
        });
        it("Disabled HospitalVisit dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.HospitalVisit);
        });
        it("Disabled dependent HospitalVisit dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(Dataset.HospitalVisit);
        });
    });

    describe("Validate health visits on dependent timeline", () => {
        // Michael Testertwo
        const hdid = "BNV554213556";
        it("Enabled HealthVisit dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.HealthVisit, hdid);
        });
        it("Disabled HealthVisit dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.HealthVisit, hdid);
        });
        it("Disabled dependent HealthVisit dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.HealthVisit,
                hdid
            );
        });
    });

    describe("Validate medications on dependent timeline", () => {
        // Michael Testertwo
        const hdid = "BNV554213556";
        it("Enabled Medication dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.Medication, hdid);
        });
        it("Disabled Medication dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.Medication, hdid);
        });
        it("Disabled dependent Medication dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.Medication,
                hdid
            );
        });
    });

    describe("Validate (lack of) notes on dependent timeline", () => {
        it("Disabled Note dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.Note);
        });
        it("Disabled dependent Note dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(Dataset.Note);
        });
    });

    describe("Validate Diagnostic Imaging requests on dependent timeline", () => {
        it("Enabled DiagnosticImaging dataset should be present", () => {
            enabledDatasetShouldBePresent(Dataset.DiagnosticImaging);
        });
        it("Disabled DiagnosticImaging dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(Dataset.DiagnosticImaging);
        });
        it("Disabled dependent DiagnosticImaging dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.DiagnosticImaging
            );
        });
    });

    // test should be skipped until PHSA fixes test data for this dependent
    describe("Validate Special Authority requests on dependent timeline", () => {
        it.skip("Enabled SpecialAuthorityRequest dataset should be present", () => {
            enabledDatasetShouldBePresent(
                Dataset.SpecialAuthorityRequest,
                authorizedSpecialAuthorityDependentHdid
            );
        });
        it.skip("Disabled SpecialAuthorityRequest dataset should not be present", () => {
            disabledDatasetShouldNotBePresent(
                Dataset.SpecialAuthorityRequest,
                authorizedSpecialAuthorityDependentHdid
            );
        });
        it.skip("Disabled dependent SpecialAuthorityRequest dataset should not be present", () => {
            disabledDependentDatasetShouldNotBePresent(
                Dataset.SpecialAuthorityRequest,
                authorizedSpecialAuthorityDependentHdid
            );
        });
    });
});
