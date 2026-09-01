const { AuthMethod } = require("../../../support/constants");
const {
    waitForCovid19Orders,
    waitForImmunizations,
    waitForLaboratoryOrders,
} = require("../../../support/functions/timeline");
const {
    applyTimelineFilters,
    cancelTimelineFilters,
    openTimelineFilters,
    selectTimelineFilters,
    testDatasetTimelineFiltering,
    timelineFilterDefinitions,
    verifyActiveFilters,
    verifyTimelineFilterSelection,
} = require("../../../support/functions/filter");

// Dataset names match the ClientApp configuration. The shared definitions map
// each name to its filter chip, count, card title, and active-filter label.
const enabledFilterDatasets = [
    "clinicalDocument",
    "covid19TestResult",
    "healthVisit",
    "hospitalVisit",
    "immunization",
    "labResult",
    "medication",
    "note",
    "specialAuthorityRequest",
    "diagnosticImaging",
    "bcCancerScreening",
];

// These datasets expose both a selectable chip and a count in the filter menu.
const selectableFilterDatasets = [
    "medication",
    "note",
    "immunization",
    "covid19TestResult",
    "labResult",
    "healthVisit",
    "specialAuthorityRequest",
    "clinicalDocument",
    "hospitalVisit",
];
const countedFilterDatasets = [
    "immunization",
    "medication",
    "labResult",
    "covid19TestResult",
    "healthVisit",
    "note",
    "specialAuthorityRequest",
    "clinicalDocument",
    "hospitalVisit",
    "diagnosticImaging",
];

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
        openTimelineFilters();
        cy.get(timelineFilterDefinitions.medication.countSelector).should(
            "be.visible"
        );

        // Only Medication is enabled for this scenario.
        selectableFilterDatasets
            .filter((dataset) => dataset !== "medication")
            .forEach((dataset) => {
                cy.get(timelineFilterDefinitions[dataset].countSelector).should(
                    "not.exist"
                );
            });
        cancelTimelineFilters();
    });
});

describe("Filters", () => {
    before(() => {
        cy.configureSettings({
            datasets: enabledFilterDatasets.map((name) => ({
                name,
                enabled: true,
            })),
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
        openTimelineFilters();
        countedFilterDatasets.forEach((dataset) => {
            cy.get(timelineFilterDefinitions[dataset].countSelector)
                .should("be.visible")
                .contains(countRegex);
        });
        cancelTimelineFilters();
    }

    function validateDateRangeFilter() {
        // No records text should be hidden when medication data is available.
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        // Select 05/10/2023 to 05/10/2023 to display medication data.
        openTimelineFilters();
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear()
            .type("2023-MAY-10");
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear()
            .type("2023-MAY-10")
            .focus();
        applyTimelineFilters();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
        verifyActiveFilters(["From 2023-May-10 To 2023-May-10"]);
        cy.get("[data-testid=clear-filters-button]").click();
    }

    function validateNoRecordsOnLinearTimeline() {
        openTimelineFilters();
        cy.get("[data-testid=filterTextInput]").type(
            "no-data-should-match-this-unique-string"
        );
        applyTimelineFilters();
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
        openTimelineFilters();
        verifyTimelineFilterSelection(selectableFilterDatasets, false);
        cancelTimelineFilters();
    }

    function validateApplyAndCancelButtons() {
        // Applying with no selections keeps the timeline unfiltered.
        openTimelineFilters();
        verifyTimelineFilterSelection(selectableFilterDatasets, false);
        applyTimelineFilters();

        // Cancelling discards all pending chip selections.
        openTimelineFilters();
        selectTimelineFilters(selectableFilterDatasets);
        verifyTimelineFilterSelection(selectableFilterDatasets, true);
        cancelTimelineFilters();

        openTimelineFilters();
        verifyTimelineFilterSelection(selectableFilterDatasets, false);
        cancelTimelineFilters();
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
        testDatasetTimelineFiltering("diagnosticImaging");
    });
});
