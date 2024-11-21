const dependentWithAudit = "9872868128"; // Leroy Desmond Tobias
const dependentWithoutGuardian = { phn: "9874307168" };
const dependentWithGuardian = { phn: "9874307175", guardianPhn: "9735353315" };
const dependentExceedingAgeCutoff = { phn: "9735353315" };
const defaultTimeout = 60000;

function performSearch(phn) {
    cy.intercept("GET", "**/Delegation/").as("getDelegation");
    cy.get("[data-testid=query-input]").clear().type(phn);
    cy.get("[data-testid=search-button]").click();
    cy.wait("@getDelegation", { timeout: defaultTimeout });
}

function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr.mud-table-row");
}

describe("Delegation Search", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/delegation"
        );
    });

    it("Verify response when searching for dependent without delegate.", () => {
        performSearch(dependentWithoutGuardian.phn);

        getTableRows("[data-testid=dependent-table]")
            .should("have.length", 1)
            .within((_$rows) => {
                cy.get("[data-testid=dependent-name]").should("not.be.empty");
                cy.get("[data-testid=dependent-dob]").should("not.be.empty");
                cy.get("[data-testid=dependent-address]").should(
                    "not.be.empty"
                );
                cy.get("[data-testid=dependent-protected-switch]").should(
                    "not.be.checked"
                );
            });

        getTableRows("[data-testid=delegate-table]").should("have.length", 0);
    });

    it("Verify response when searching for dependent with delegate.", () => {
        performSearch(dependentWithGuardian.phn);

        getTableRows("[data-testid=dependent-table]")
            .should("have.length", 1)
            .within((_$rows) => {
                cy.get("[data-testid=dependent-name]").should("not.be.empty");
                cy.get("[data-testid=dependent-dob]").should("not.be.empty");
                cy.get("[data-testid=dependent-address]").should(
                    "not.be.empty"
                );
                cy.get("[data-testid=dependent-protected-switch]").should(
                    "not.be.checked"
                );
            });

        getTableRows("[data-testid=delegate-table]")
            .should("have.length", 1)
            .within(($rows) => {
                cy.wrap($rows)
                    .eq(0)
                    .within((_$row) => {
                        cy.get("[data-testid=delegate-name]").should(
                            "not.be.empty"
                        );
                        cy.get("[data-testid=delegate-phn]").contains(
                            dependentWithGuardian.guardianPhn
                        );
                        cy.get("[data-testid=delegate-dob]").should(
                            "not.be.empty"
                        );
                        cy.get("[data-testid=delegate-address]").should(
                            "not.be.empty"
                        );
                        cy.get("[data-testid=delegate-status]").contains(
                            "Added"
                        );
                    });
            });
    });

    it("Verify response contains dependent audit in descending datetime order as per seeded data.", () => {
        performSearch(dependentWithAudit);

        // Click delegation change header to show dependent audit
        cy.get("[data-testid=delegation-changes-header")
            .should("be.visible")
            .click();

        cy.get("[data-testid=delegation-change-0]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=agent]")
                    .should("be.visible")
                    .contains("admin");
                cy.get("[data-testid=reason]")
                    .should("be.visible")
                    .contains("Protect");
                cy.get("[data-testid=datetime]").should("be.visible");
            });

        cy.get("[data-testid=delegation-change-1]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=agent]")
                    .should("be.visible")
                    .contains("support");
                cy.get("[data-testid=reason]")
                    .should("be.visible")
                    .contains("Unprotect");
                cy.get("[data-testid=datetime]").should("be.visible");
            });

        cy.get("[data-testid=delegation-change-2]")
            .should("be.visible")
            .within(() => {
                cy.get("[data-testid=agent]")
                    .should("be.visible")
                    .contains("reviewer");
                cy.get("[data-testid=reason]")
                    .should("be.visible")
                    .contains("Protect");
                cy.get("[data-testid=datetime]").should("be.visible");
            });

        // Click delegation change header to not show dependent audit
        cy.get("[data-testid=delegation-changes-header").click();

        cy.get("[data-testid=delegation-change-0]").should("not.be.visible");
        cy.get("[data-testid=delegation-change-1]").should("not.be.visible");
    });

    it("Verify error when searching for person exceeding age cutoff.", () => {
        performSearch(dependentExceedingAgeCutoff.phn);

        cy.get("[data-testid=search-error-message]").should("be.visible");

        cy.get("[data-testid=dependent-table]").should("not.exist");
        cy.get("[data-testid=delegate-table]").should("not.exist");
    });
});
