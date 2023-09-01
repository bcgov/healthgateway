const { AuthMethod } = require("../../../support/constants");
const HDID = "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A";

describe("Filters", () => {
    beforeEach(() => {
        // 2 records
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        // 9 records
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunization.json",
        });
        // 23 records
        cy.intercept("GET", "**/Encounter/*", {
            fixture: "EncounterService/encounters.json",
        });
        // 3 records
        cy.intercept("GET", "**/PatientData/*?patientDataTypes=*", {
            fixture: "PatientData/allTimelineData.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
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

    it("Verify filtered record count", () => {
        const totalRecordsUnfiltered = 37;
        const pageSize = 25;
        const recordDisplayMessage = (lower, upper, total) =>
            `Displaying ${lower} to ${upper} out of ${total} records`;

        cy.get("[data-testid=timeline-record-count]").contains(
            recordDisplayMessage(1, pageSize, totalRecordsUnfiltered)
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter] input").click();
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=immunizationTitle]").should("be.visible");

        cy.get("[data-testid=timeline-record-count]").contains(
            recordDisplayMessage(1, 9, 9)
        );

        cy.contains("[data-testid=filter-label]", "Immunizations").within(
            () => {
                cy.get(".v-chip__close").click();
            }
        );

        cy.get("[data-testid=timeline-record-count]").contains(
            recordDisplayMessage(1, pageSize, totalRecordsUnfiltered)
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=btnFilterApply]").focus().click();

        cy.contains(
            "[data-testid=filter-label]",
            "From 2022-Jun-09 To 2022-Jun-09"
        );

        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");

        cy.get("[data-testid=clear-filters-button]").click();
        cy.get("[data-testid=timeline-record-count]").contains(
            recordDisplayMessage(1, pageSize, totalRecordsUnfiltered)
        );
    });

    const alertTestCases = [
        {
            name: "immunization",
            filter: "Immunization",
            alert: "timeline-immunization-alert",
        },
        {
            name: "clinical document",
            filter: "ClinicalDocument",
            alert: "timeline-clinical-document-alert",
        },
        {
            name: "diagnostic imaging",
            filter: "DiagnosticImaging",
            alert: "timeline-diagnostic-imaging-alert",
        },
        {
            name: "cancer screening",
            filter: "BcCancerScreening",
            alert: "timeline-cancer-screening-alert",
        },
    ];

    alertTestCases.forEach((testCase) => {
        it(`Verify ${testCase.name} alert appears when the ${testCase.filter} filter is the only active filter`, () => {
            cy.get(`[data-testid=${testCase.alert}]`).should("not.exist");

            cy.get("[data-testid=filterDropdown]").click();
            cy.get(`[data-testid=${testCase.filter}-filter] input`).click();
            cy.get("[data-testid=btnFilterApply]").click();
            cy.get("[data-testid=btnFilterApply]").should("not.exist");

            cy.get(`[data-testid=${testCase.alert}]`).should("be.visible");

            cy.get("[data-testid=filterDropdown]").click();
            cy.get("[data-testid=HealthVisit-filter] input").click();
            cy.get("[data-testid=btnFilterApply]").click();

            cy.get(`[data-testid=${testCase.alert}]`).should("not.exist");
        });
    });

    it("Verify 'immunization' alert appears when the 'immunization' filter is the only active filter", () => {
        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter] input").click();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter] input").click();
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.exist");
    });

    it("Verify 'clinical document' alert appears when the 'clinical document' filter is the only active filter", () => {
        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ClinicalDocument-filter] input").click();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter] input").click();
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("not.exist");
    });

    it("Verify 'diagnostic imaging' alert appears when the 'diagnostic imaging' filter is the only active filter", () => {
        cy.get(
            "[data-testid=linear-timeline-diagnostic-imaging-disclaimer-alert]"
        ).should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=DiagnosticImaging-filter] input").click({
            force: true,
        });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-diagnostic-imaging-disclaimer]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter] input").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-diagnostic-imaging-disclaimer]"
        ).should("not.exist");
    });

    it("Verify 'cancer screening' alert appears when the 'cancer screening' filter is the only active filter", () => {
        cy.get(
            "[data-testid=linear-timeline-cancer-screening-disclaimer]"
        ).should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=BcCancerScreening-filter] input").click({
            force: true,
        });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-cancer-screening-disclaimer]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter] input").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-cancer-screening-disclaimer]"
        ).should("not.exist");
    });
});

describe("Describe Filters when all datasets blocked", () => {
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
            ],
        });
        cy.intercept("GET", `**/UserProfile/*`, {
            fixture:
                "UserProfileService/userProfileMultipleDatasetsBlocked.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Filter Counts and Error Message", () => {
        cy.get("[data-testid=filterDropdown]").should("not.exist");
        cy.get("[data-testid=errorBanner")
            .should("be.visible")
            .contains(
                "Multiple records are unavailable at this time. Please try again later."
            );
    });
});

describe("Describe Filters when clinical doc dataset is blocked but immunization dataset is not", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", `**/UserProfile/*`, {
            fixture:
                "UserProfileService/userProfileClinicalDocDatasetBlocked.json",
        });
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Filter Counts and Error Message", () => {
        const expectedImmunizationCount = 9;
        const expectedClinicalDocCount = 0;
        cy.get("[data-testid=filterDropdown]").click();

        cy.get("[data-testid=ImmunizationCount]")
            .should("be.visible")
            .contains(expectedImmunizationCount);
        cy.get("[data-testid=ClinicalDocumentCount]")
            .should("be.visible")
            .contains(expectedClinicalDocCount);

        cy.get("[data-testid=MedicationCount]").should("not.exist");
        cy.get("[data-testid=LabResultCount]").should("not.exist");
        cy.get("[data-testid=Covid19TestResultCount]").should("not.exist");
        cy.get("[data-testid=HealthVisitCount]").should("not.exist");
        cy.get("[data-testid=NoteCount]").should("not.exist");
        cy.get("[data-testid=SpecialAuthorityRequestCount]").should(
            "not.exist"
        );
        cy.get("[data-testid=HospitalVisitCount]").should("not.exist");
        cy.get("[data-testid=DiagnosticImagingCount]").should("not.exist");

        cy.get("[data-testid=btnFilterCancel]").click();

        cy.get("[data-testid=errorBanner")
            .should("be.visible")
            .contains(
                "Clinical Documents are unavailable at this time. Please try again later."
            );
    });
});
