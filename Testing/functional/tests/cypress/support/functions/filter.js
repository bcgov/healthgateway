import { AuthMethod } from "../constants";

export function setupTimelineFilter(datasets, requests = []) {
    const datasetNames = Array.isArray(datasets) ? datasets : [datasets];
    const requestConfigs = Array.isArray(requests)
        ? requests
        : requests
          ? [requests]
          : [];

    cy.configureSettings({
        datasets: [
            ...datasetNames.map((name) => ({ name, enabled: true })),
            { name: "note", enabled: true },
        ],
    });

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

export function testDatasetTimelineFiltering(
    filterTestId,
    titleTestId,
    activeFilters
) {
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
        "[data-testid=bccancerscreeningTitle]",
    ];

    cy.get("[data-testid=filterContainer]").should("not.exist");
    cy.get("[data-testid=filterDropdown]").click();
    cy.get(filterTestId).click({ force: true });
    cy.get("[data-testid=btnFilterApply]").click();

    titleIds.forEach((titleId) => {
        cy.get(titleId).should(
            titleId === titleTestId ? "be.visible" : "not.exist"
        );
    });
    verifyActiveFilters(activeFilters);
}
