const { AuthMethod } = require("../../../support/constants");

function verifyActiveFilters(filterLabels) {
    filterLabels.forEach((label) => {
        cy.contains("[data-testid=filter-label]", label);
    });
}

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
    function testDatasetTimelineFiltering(
        filterTestId,
        titleTestId,
        activeFilters
    ) {
        const isVisibleOrNonExistent = (titleId, testingTitleId) =>
            titleId === testingTitleId ? "be.visible" : "not.exist";

        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get(`${filterTestId}`).click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        const titleIds = [
            "[data-testid=healthvisitTitle]",
            "[data-testid=noteTitle]",
            "[data-testid=immunizationTitle]",
            "[data-testid=covid19testresultTitle]",
            "[data-testid=labresultTitle]",
            "[data-testid=medicationTitle]",
            "[data-testid=specialauthorityrequestTitle]",
            "[data-testid=clinicaldocumentTitle]",
            "[data-testid=hospitalvisitTitle]",
            "[data-testid=diagnosticimagingTitle]",
        ];
        for (var i = 0; i < titleIds.length; i++) {
            cy.get(titleIds[i]).should(
                isVisibleOrNonExistent(titleIds[i], titleTestId)
            );
        }
        verifyActiveFilters(activeFilters);
    }

    beforeEach(() => {
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
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Filter Counts", () => {
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
    });

    it("Validate Date Range Filter", () => {
        //Validate No records... text should be hidden by default (or with data)
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        // Validate "No records found with the selected filters" for a Date Range Filter
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2020-SEP-30");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2020-OCT-01")
            .focus();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=noTimelineEntriesText]").should(
            "have.text",
            "No records found with the selected filters"
        );

        // Select 06/14/2020 to 06/14/2020 should display data for this date range.
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear()
            .type("2020-JUN-14");
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear()
            .type("2020-JUN-14")
            .focus();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
        verifyActiveFilters(["From 2020-Jun-14 To 2020-Jun-14"]);
    });

    it("No Records on Linear Timeline", () => {
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
    });

    it("Filter Checkboxes are Visible", () => {
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
    });

    it("Filter Immunization", () => {
        testDatasetTimelineFiltering(
            "[data-testid=Immunization-filter]",
            "[data-testid=immunizationTitle]",
            ["Immunization"]
        );
    });

    it("Filter Immunization By Text", () => {
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("COVID");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=clear-filters-button]").click();

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("EK4241");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=clear-filters-button]").click();

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("Polio");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
    });

    it("Filter Medication", () => {
        testDatasetTimelineFiltering(
            "[data-testid=Medication-filter]",
            "[data-testid=medicationTitle]",
            ["Medication"]
        );
    });

    it("Filter Encounter", () => {
        testDatasetTimelineFiltering(
            "[data-testid=HealthVisit-filter]",
            "[data-testid=healthvisitTitle]",
            ["Health Visits"]
        );
    });

    it("Filter COVID-19", () => {
        testDatasetTimelineFiltering(
            "[data-testid=Covid19TestResult-filter]",
            "[data-testid=covid19testresultTitle]",
            ["COVID‑19 Tests"]
        );
    });

    it("Filter Laboratory", () => {
        testDatasetTimelineFiltering(
            "[data-testid=LabResult-filter]",
            "[data-testid=labresultTitle]",
            ["Lab Results"]
        );
    });

    it("Filter Special Authority", () => {
        testDatasetTimelineFiltering(
            "[data-testid=SpecialAuthorityRequest-filter]",
            "[data-testid=specialauthorityrequestTitle]",
            ["Special Authority"]
        );
    });

    it("Filter Clinical Documents", () => {
        testDatasetTimelineFiltering(
            "[data-testid=ClinicalDocument-filter]",
            "[data-testid=clinicaldocumentTitle]",
            ["Clinical Documents"]
        );
    });

    it("Filter Hospital Visits", () => {
        testDatasetTimelineFiltering(
            "[data-testid=HospitalVisit-filter]",
            "[data-testid=hospitalvisitTitle]",
            ["Hospital Visits"]
        );
    });

    it("Filter Diagnostic Imaging", () => {
        testDatasetTimelineFiltering(
            "[data-testid=DiagnosticImaging-filter]",
            "[data-testid=diagnosticimagingTitle]",
            ["Imaging Reports"]
        );
    });

    it("Filter Cancer Screening", () => {
        testDatasetTimelineFiltering(
            "[data-testid=BcCancerScreening-filter]",
            "[data-testid=bccancerscreeningTitle]",
            ["BC Cancer Screening"]
        );
    });

    it("Validate Apply and Cancel buttons", () => {
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
    });
});
