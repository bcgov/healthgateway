import { AuthMethod } from "../constants";

// Centralizes the selectors and labels used by timeline filter tests. Specs refer
// to dataset names so ClientApp test IDs are maintained in one place.
export const timelineFilterDefinitions = {
    medication: {
        filterSelector: "[data-testid=Medication-filter]",
        countSelector: "[data-testid=MedicationCount]",
        titleSelector: "[data-testid=medicationTitle]",
        label: "Medication",
    },
    healthVisit: {
        filterSelector: "[data-testid=HealthVisit-filter]",
        countSelector: "[data-testid=HealthVisitCount]",
        titleSelector: "[data-testid=healthvisitTitle]",
        label: "Health Visits",
    },
    note: {
        filterSelector: "[data-testid=Note-filter]",
        countSelector: "[data-testid=NoteCount]",
        titleSelector: "[data-testid=noteTitle]",
        label: "My Notes",
    },
    immunization: {
        filterSelector: "[data-testid=Immunization-filter]",
        countSelector: "[data-testid=ImmunizationCount]",
        titleSelector: "[data-testid=immunizationTitle]",
        label: "Immunization",
    },
    covid19TestResult: {
        filterSelector: "[data-testid=Covid19TestResult-filter]",
        countSelector: "[data-testid=Covid19TestResultCount]",
        titleSelector: "[data-testid=covid19testresultTitle]",
        label: "COVID‑19 Tests",
    },
    labResult: {
        filterSelector: "[data-testid=LabResult-filter]",
        countSelector: "[data-testid=LabResultCount]",
        titleSelector: "[data-testid=labresultTitle]",
        label: "Lab Results",
    },
    specialAuthorityRequest: {
        filterSelector: "[data-testid=SpecialAuthorityRequest-filter]",
        countSelector: "[data-testid=SpecialAuthorityRequestCount]",
        titleSelector: "[data-testid=specialauthorityrequestTitle]",
        label: "Special Authority",
    },
    clinicalDocument: {
        filterSelector: "[data-testid=ClinicalDocument-filter]",
        countSelector: "[data-testid=ClinicalDocumentCount]",
        titleSelector: "[data-testid=clinicaldocumentTitle]",
        label: "Clinical Documents",
    },
    hospitalVisit: {
        filterSelector: "[data-testid=HospitalVisit-filter]",
        countSelector: "[data-testid=HospitalVisitCount]",
        titleSelector: "[data-testid=hospitalvisitTitle]",
        label: "Hospital Visits",
    },
    diagnosticImaging: {
        filterSelector: "[data-testid=DiagnosticImaging-filter]",
        countSelector: "[data-testid=DiagnosticImagingCount]",
        titleSelector: "[data-testid=diagnosticimagingTitle]",
        label: "Imaging Reports",
    },
    bcCancerScreening: {
        filterSelector: "[data-testid=BcCancerScreening-filter]",
        countSelector: "[data-testid=BcCancerScreeningCount]",
        titleSelector: "[data-testid=bccancerscreeningTitle]",
        label: "BC Cancer Screening",
    },
};

function getFilterDefinition(dataset) {
    const definition = timelineFilterDefinitions[dataset];
    if (!definition) {
        throw new Error(`Unknown timeline filter dataset: ${dataset}`);
    }
    return definition;
}

export function openTimelineFilters() {
    cy.get("[data-testid=filterContainer]").should("not.exist");
    cy.get("[data-testid=filterDropdown]").click();
    cy.get("[data-testid=filterContainer]").should("be.visible");
}

export function applyTimelineFilters() {
    cy.get("[data-testid=btnFilterApply]").click();
    cy.get("[data-testid=filterContainer]").should("not.exist");
}

export function cancelTimelineFilters() {
    cy.get("[data-testid=btnFilterCancel]").click();
    cy.get("[data-testid=filterContainer]").should("not.exist");
}

export function selectTimelineFilters(datasets) {
    datasets.forEach((dataset) => {
        cy.get(getFilterDefinition(dataset).filterSelector).click({
            force: true,
        });
    });
}

export function verifyTimelineFilterSelection(datasets, selected) {
    datasets.forEach((dataset) => {
        cy.get(
            `${getFilterDefinition(dataset).filterSelector}.v-chip--selected`
        ).should(selected ? "exist" : "not.exist");
    });
}

export function setupTimelineFilter(datasets, requests = []) {
    const datasetNames = Array.isArray(datasets) ? datasets : [datasets];
    const requestConfigs = Array.isArray(requests)
        ? requests
        : requests
          ? [requests]
          : [];

    // Health Visits provide a known second record type so selecting another dataset
    // proves that the filter removes unrelated timeline cards.
    const enabledDatasets = [...new Set([...datasetNames, "healthVisit"])];
    cy.configureSettings({
        datasets: enabledDatasets.map((name) => ({ name, enabled: true })),
    });

    // Deferred PHSA datasets provide their own waiter; ordinary datasets can
    // use a single cy.wait supplied by the spec.
    requestConfigs.forEach((request) => {
        cy.intercept("GET", request.endpoint).as(request.alias);
    });

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );

    requestConfigs.forEach((request) => {
        request.waitForData(`@${request.alias}`);
    });

    cy.checkTimelineHasLoaded();
}

export function verifyActiveFilters(filterLabels) {
    filterLabels.forEach((label) => {
        cy.contains("[data-testid=filter-label]", label);
    });
}

export function testDatasetTimelineFiltering(dataset) {
    const selectedFilter = getFilterDefinition(dataset);

    openTimelineFilters();
    selectTimelineFilters([dataset]);
    applyTimelineFilters();

    // Only cards belonging to the selected dataset should remain visible.
    Object.values(timelineFilterDefinitions).forEach(({ titleSelector }) => {
        cy.get(titleSelector).should(
            titleSelector === selectedFilter.titleSelector
                ? "be.visible"
                : "not.exist"
        );
    });
    verifyActiveFilters([selectedFilter.label]);
}
