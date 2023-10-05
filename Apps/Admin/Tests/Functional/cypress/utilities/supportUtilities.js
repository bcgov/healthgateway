import { getTableRows } from "./sharedUtilities";

export function performSearch(queryType, queryString) {
    cy.get("[data-testid=query-type-select]").click({ force: true });
    cy.get("[data-testid=query-type]")
        .contains(queryType)
        .click({ force: true });

    if (queryString) {
        cy.get("[data-testid=query-input]").clear().type(queryString);
    } else {
        cy.get("[data-testid=query-input]").clear();
    }

    cy.get("[data-testid=search-btn]").click();
}

export function verifySearchInput(queryType, queryString) {
    cy.url().should("include", "/support");
    cy.get('[data-testid="query-type-select"]').should("have.value", queryType);
    cy.get('[data-testid="query-input"]').should("have.value", queryString);
}

export function verifySingleSupportResult(expectedHdid, expectedPhn) {
    cy.get("[data-testid=user-table]").should("not.exist");
    if (expectedHdid) {
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(expectedHdid);
    }
    cy.get("[data-testid=patient-phn]")
        .should("be.visible")
        .contains(expectedPhn);
    cy.get("[data-testid=patient-details-back-button]")
        .should("be.visible")
        .click();
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
