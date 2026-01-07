import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("Filters", () => {
    function testFilteredResultAlerts(filter, alert) {
        cy.get(`[data-testid=${alert}]`).should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get(`[data-testid=${filter}-filter]`).click();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(`[data-testid=${alert}]`).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter]").click();
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(`[data-testid=${alert}]`).should("not.exist");
    }

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

        setupStandardFixtures();

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
        cy.get("[data-testid=Immunization-filter]").click();
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

    it(`Verify health visit alert appears when the health visit filter is the only active filter`, () => {
        testFilteredResultAlerts("HealthVisit", "timeline-health-visit-alert");
    });

    it(`Verify immunization alert appears when the immunization filter is the only active filter`, () => {
        testFilteredResultAlerts("Immunization", "timeline-immunization-alert");
    });

    it(`Verify clinical document alert appears when the clinical document filter is the only active filter`, () => {
        testFilteredResultAlerts(
            "ClinicalDocument",
            "timeline-clinical-document-alert"
        );
    });

    it(`Verify diagnostic imaging alert appears when the diagnostic imaging filter is the only active filter`, () => {
        testFilteredResultAlerts(
            "DiagnosticImaging",
            "timeline-diagnostic-imaging-alert"
        );
    });

    it(`Verify cancer screening alert appears when the cancer screening filter is the only active filter`, () => {
        testFilteredResultAlerts(
            "BcCancerScreening",
            "timeline-cancer-screening-alert"
        );
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

        setupStandardFixtures({
            userProfileFixture:
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

        setupStandardFixtures({
            userProfileFixture:
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
