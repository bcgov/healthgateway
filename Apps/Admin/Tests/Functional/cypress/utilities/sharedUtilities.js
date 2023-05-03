export function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr.mud-table-row");
}
