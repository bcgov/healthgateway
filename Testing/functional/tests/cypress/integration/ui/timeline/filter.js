const { AuthMethod } = require("../../../support/constants");
const HDID = "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A";

describe("Filters", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.intercept("GET", "**/Encounter/*", {
            fixture: "EncounterService/encounters.json",
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
        const unfilteredRecordsMessage = "Displaying 25 out of 34 records";

        cy.get("[data-testid=timeline-record-count]").contains(
            unfilteredRecordsMessage
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=immunizationTitle]").should("be.visible");

        cy.get("[data-testid=timeline-record-count]").contains(
            "Displaying 9 out of 9 records"
        );

        cy.contains("[data-testid=filter-label]", "Immunizations")
            .children("button")
            .click();

        cy.get("[data-testid=timeline-record-count]").contains(
            unfilteredRecordsMessage
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
            unfilteredRecordsMessage
        );
    });

    it("Verify immunization record alert appears when only immunization is selected", () => {
        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.be.visible");
    });

    it("Verify clinical document record alert appears when only clinical document is selected", () => {
        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("not.be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ClinicalDocument-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=timeline-clinical-document-disclaimer-alert]"
        ).should("not.be.visible");
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
        const expectedCount = 0;
        cy.get("[data-testid=filterDropdown]").click();

        cy.get("[data-testid=ImmunizationCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=ClinicalDocumentCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=MedicationCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=LabResultCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=Covid19TestResultCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=HealthVisitCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=NoteCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=SpecialAuthorityRequestCount]")
            .should("be.visible")
            .contains(expectedCount);

        cy.get("[data-testid=HospitalVisitCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=DiagnosticImagingCount]")
            .should("be.visible")
            .contains(expectedCount);
        cy.get("[data-testid=singleErrorHeader")
            .should("be.visible")
            .contains(
                "Multiple records are unavailable at this time. Please try again later."
            );

        cy.get("[data-testid=btnFilterCancel]").click();
    });
});

describe("Describe Filters when clinical doc dataset is blocked but immmunization dataset is not", () => {
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
        cy.intercept("GET", "**/Immunization?*", {
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
        const expectedCount = 0;
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

        cy.get("[data-testid=singleErrorHeader")
            .should("be.visible")
            .contains(
                "Clinical Documents are unavailable at this time. Please try again later."
            );

        cy.get("[data-testid=btnFilterCancel]").click();
    });
});
