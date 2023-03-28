const dependentWithoutGuardian = { phn: "9874307168" };
const dependentWithGuardian = { phn: "9874307175", guardianPhn: "9735353315" };
const dependentExceedingAgeCutoff = { phn: "9735353315" };
const protectDependent = "9872868095";

function performSearch(phn) {
    cy.get("[data-testid=query-input]").clear().type(phn);
    cy.get("[data-testid=search-button]").click();
}

function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr");
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
        cy.get("[data-testid=query-input]")
            .clear()
            .type(dependentWithoutGuardian.phn);
        cy.get("[data-testid=search-button]").click();

        getTableRows("[data-testid=dependent-table]")
            .should("have.length", 1)
            .within(($rows) => {
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
            .within(($rows) => {
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
                    .within(($row) => {
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

    it("Verify error when searching for person exceeding age cutoff.", () => {
        performSearch(dependentExceedingAgeCutoff.phn);

        cy.get("[data-testid=search-error-message]").should("be.visible");

        cy.get("[data-testid=dependent-table]").should("not.exist");
        cy.get("[data-testid=delegate-table]").should("not.exist");
    });
});

describe("Delegation Protect", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/delegation"
        );
    });

    it("Verify protect dependent toggle, save button, cancel confirmation button and cancel button are selected.", () => {
        cy.get("[data-testid=query-input]")
            .clear()
            .type(dependentWithGuardian.phn);
        cy.get("[data-testid=search-button]").click();

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "false"
        );

        // Protect
        cy.get("[data-testid=dependent-protected-switch]").click();
        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "true"
        );
        cy.get("[data-testid=cancel-edit-button]").should(
            "be.visible",
            "be.enabled"
        );

        // Save button
        cy.get("[data-testid=save-button]").click();

        // Confirmation dialog confirmation button
        cy.get("[data-testid=confirm-button]").should(
            "be.visible",
            "be.enabled"
        );

        // Cancel confirmation
        cy.get("[data-testid=cancel-button]").click();

        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "true"
        );

        // Save button
        cy.get("[data-testid=save-button]").should("be.visible", "be.enabled");

        // Cancel button
        cy.get("[data-testid=cancel-edit-button]").click();

        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "false"
        );
    });

    it("Verify protect/unprotect dependent toggle, select to remove, save button and confirmation button are selected.", () => {
        cy.get("[data-testid=query-input]").clear().type(protectDependent);
        cy.get("[data-testid=search-button]").click();

        // Confirm delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 2);

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "false"
        );

        // Protect
        cy.get("[data-testid=dependent-protected-switch]").click();
        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "true"
        );

        // Remove delegate row before protecting dependent
        const rowSelector =
            "[data-testid=delegate-table] tbody tr.mud-table-row";
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=delegate-to-be-removed-checkbox]").click();
            });

        // Save button
        cy.get("[data-testid=save-button]").click();

        // Confirmation button
        cy.get("[data-testid=confirm-button]").click();

        // Confirm delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 2);

        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "true"
        );

        // Unprotect
        cy.get("[data-testid=dependent-protected-switch]").click();

        // Confirmation button
        cy.get("[data-testid=confirm-button]").click();

        cy.get("[data-testid=dependent-protected-switch]").should(
            "have.attr",
            "aria-checked",
            "false"
        );

        // Confirm delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 1);
    });
});
