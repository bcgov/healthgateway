const { AuthMethod } = require("../../../support/constants");
const {
    waitForCovid19Orders,
    waitForImmunizations,
    waitForLaboratoryOrders,
} = require("../../../support/functions/timeline");
const {
    testDatasetTimelineFiltering,
    verifyActiveFilters,
} = require("../../../support/functions/filter");

describe("Disabled Filters", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate disabled filters", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=MedicationCount]").should("be.visible");
        cy.get("[data-testid=ImmunizationCount]").should("not.exist");
        cy.get("[data-testid=HealthVisitCount]").should("not.exist");
        cy.get("[data-testid=NoteCount]").should("not.exist");
        cy.get("[data-testid=Covid19TestResultCount]").should("not.exist");
        cy.get("[data-testid=LabResultCount]").should("not.exist");
        cy.get("[data-testid=SpecialAuthorityRequestCount]").should(
            "not.exist"
        );
        cy.get("[data-testid=ClinicalDocumentCount]").should("not.exist");
        cy.get("[data-testid=HospitalVisitCount]").should("not.exist");
        cy.get("[data-testid=btnFilterCancel]").click();
    });
});

describe("Filters", () => {
    before(() => {
        cy.configureSettings({
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
                {
                    name: "diagnosticImaging",
                    enabled: true,
                },
                {
                    name: "bcCancerScreening",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/Laboratory/Covid19Orders*").as(
            "getCovid19Orders"
        );
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*").as(
            "getLaboratoryOrders"
        );
        cy.intercept("GET", "**/Immunization?hdid=*").as("getImmunizations");
        cy.intercept("GET", "**/MedicationStatement/*").as("getMedications");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        waitForCovid19Orders("@getCovid19Orders");
        waitForLaboratoryOrders("@getLaboratoryOrders");
        waitForImmunizations("@getImmunizations");
        cy.wait("@getMedications", { timeout: 60000 });
        cy.checkTimelineHasLoaded();
    });

    function validateFilterCounts() {
        const countRegex = /^.*?\((\d+)K?\).*$/;
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ImmunizationCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=MedicationCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=LabResultCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=Covid19TestResultCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=HealthVisitCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=NoteCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=SpecialAuthorityRequestCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=ClinicalDocumentCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=HospitalVisitCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=DiagnosticImagingCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=btnFilterCancel]").click();
    }

    function validateDateRangeFilter() {
        // No records text should be hidden when medication data is available.
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        // Select 05/10/2023 to 05/10/2023 to display medication data.
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear()
            .type("2023-MAY-10");
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear()
            .type("2023-MAY-10")
            .focus();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
        verifyActiveFilters(["From 2023-May-10 To 2023-May-10"]);
        cy.get("[data-testid=clear-filters-button]").click();
    }

    function validateNoRecordsOnLinearTimeline() {
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type(
            "no-data-should-match-this-unique-string"
        );
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.contains(
            "[data-testid=filter-label]",
            '"no-data-should-match-this-unique-string"'
        )
            .children(".v-chip__close")
            .click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
    }

    function validateFilterCheckboxesAreVisible() {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Medication-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Note-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Immunization-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get(
            "[data-testid=Covid19TestResult-filter].v-chip--selected"
        ).should("not.exist");
        cy.get("[data-testid=HealthVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=ClinicalDocument-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=HospitalVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=btnFilterCancel]").click();
    }

    function validateApplyAndCancelButtons() {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Medication-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Note-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Immunization-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get(
            "[data-testid=Covid19TestResult-filter].v-chip--selected"
        ).should("not.exist");
        cy.get("[data-testid=LabResult-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=HealthVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get(
            "[data-testid=SpecialAuthorityRequest-filter].v-chip--selected"
        ).should("not.exist");
        cy.get("[data-testid=ClinicalDocument-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=HospitalVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Immunization-filter]").click({
            force: true,
        });
        cy.get("[data-testid=Medication-filter]").click({ force: true });
        cy.get("[data-testid=HealthVisit-filter]").click({ force: true });
        cy.get("[data-testid=Covid19TestResult-filter]").click({
            force: true,
        });
        cy.get("[data-testid=LabResult-filter]").click({ force: true });
        cy.get("[data-testid=Note-filter]").click({ force: true });
        cy.get("[data-testid=SpecialAuthorityRequest-filter]").click({
            force: true,
        });
        cy.get("[data-testid=ClinicalDocument-filter]").click({
            force: true,
        });
        cy.get("[data-testid=HospitalVisit-filter]").click({
            force: true,
        });
        cy.get("[data-testid=Medication-filter].v-chip--selected").should(
            "exist"
        );
        cy.get("[data-testid=Note-filter].v-chip--selected").should("exist");
        cy.get("[data-testid=Immunization-filter].v-chip--selected").should(
            "exist"
        );
        cy.get(
            "[data-testid=Covid19TestResult-filter].v-chip--selected"
        ).should("exist");
        cy.get("[data-testid=LabResult-filter].v-chip--selected").should(
            "exist"
        );
        cy.get("[data-testid=HealthVisit-filter].v-chip--selected").should(
            "exist"
        );
        cy.get(
            "[data-testid=SpecialAuthorityRequest-filter].v-chip--selected"
        ).should("exist");
        cy.get("[data-testid=ClinicalDocument-filter].v-chip--selected").should(
            "exist"
        );
        cy.get("[data-testid=HospitalVisit-filter].v-chip--selected").should(
            "exist"
        );
        cy.get("[data-testid=btnFilterCancel]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Medication-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Note-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=Immunization-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get(
            "[data-testid=Covid19TestResult-filter].v-chip--selected"
        ).should("not.exist");
        cy.get("[data-testid=LabResult-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=HealthVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get(
            "[data-testid=SpecialAuthorityRequest-filter].v-chip--selected"
        ).should("not.exist");
        cy.get("[data-testid=ClinicalDocument-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=HospitalVisit-filter].v-chip--selected").should(
            "not.exist"
        );
        cy.get("[data-testid=btnFilterCancel]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");
    }

    it("Validate filters", () => {
        validateFilterCounts();
        validateDateRangeFilter();
        validateNoRecordsOnLinearTimeline();
        validateFilterCheckboxesAreVisible();
        validateApplyAndCancelButtons();
    });
});

describe("Diagnostic Imaging Filter", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "diagnosticImaging",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Filter Diagnostic Imaging", () => {
        testDatasetTimelineFiltering(
            "[data-testid=DiagnosticImaging-filter]",
            "[data-testid=diagnosticimagingTitle]",
            ["Imaging Reports"]
        );
    });
});
