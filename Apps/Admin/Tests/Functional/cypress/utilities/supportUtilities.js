import { getTableRows, selectTab } from "./sharedUtilities";

const defaultTimeout = 60000;

function selectPatientTab(tabText) {
    selectTab("[data-testid=patient-details-tabs]", tabText);
}

export function performSearch(queryType, queryString, options = {}) {
    const { waitForUser = true, waitForPatientSupportDetails = true } = options;

    cy.get("[data-testid=query-type-select]").click({ force: true });
    cy.get("[data-testid=query-type]")
        .contains(queryType)
        .click({ force: true });

    if (queryString) {
        cy.get("[data-testid=query-input]").clear().type(queryString);
    } else {
        cy.get("[data-testid=query-input]").clear();
    }

    cy.intercept("GET", "**/Support/Users*").as("getUsers");
    cy.intercept("GET", "**/Support/PatientSupportDetails*").as(
        "getPatientSupportDetails"
    );

    cy.get("[data-testid=search-btn]").click();

    if (waitForUser) {
        cy.wait("@getUsers", { timeout: defaultTimeout });
    }

    if (waitForPatientSupportDetails) {
        cy.wait("@getPatientSupportDetails", { timeout: defaultTimeout });
    }
}

export function verifySearchInput(queryType, queryString) {
    cy.url().should("include", "/support");
    cy.get('[data-testid="query-type-select"]').should("have.value", queryType);
    cy.get('[data-testid="query-input"]').should("have.value", queryString);
}

export function verifySingleSupportResult(expectedHdid, expectedPhn) {
    cy.url().should("include", "/patient-details");
    cy.get("[data-testid=patient-phn]")
        .should("be.visible")
        .contains(expectedPhn);
    if (expectedHdid) {
        selectPatientTab("Account");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(expectedHdid);
    }
    cy.get("[data-testid=patient-details-back-button]")
        .should("be.visible")
        .click();
    cy.url().should("include", "/support");
    getTableRows("[data-testid=user-table]").should("have.length", 1);
}

export function verifySupportTableResults(
    expectedHdid,
    expectedPhn,
    expectedRowCount
) {
    getTableRows("[data-testid=user-table]")
        .should("have.length", expectedRowCount)
        .first((_$rows) => {
            cy.get(`[data-testid=user-table-hdid-${expectedHdid}]`).contains(
                expectedHdid
            );

            cy.get(`[data-testid=user-table-phn-${expectedHdid}]`).contains(
                expectedPhn
            );
        });
}

export function formatPhn(phnNumber) {
    const phnString = phnNumber.toString();
    const formattedPhn = phnString.replace(/(\d{4})(\d{3})(\d{3})/, "$1 $2 $3");
    return formattedPhn;
}
