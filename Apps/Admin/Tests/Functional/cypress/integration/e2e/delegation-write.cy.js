const dependentWithAudit = "9872868128"; // Leroy Desmond Tobias
const dependentWithGuardian = { phn: "9874307175", guardianPhn: "9735353315" };
const dependentToProtect = "9872868095"; // Jeffrey Lawrence Stallings
const guardianToAdd = "9735352488"; // Turpentine Garlandry
const guardianNotFound = "9735352489";
const guardianAlreadyAdded = "9735353315"; // BONNET PROTERVITY
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

describe("Delegation Protect", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/delegation"
        );
    });

    it("Verify protect dependent toggle and delegation cancel.", () => {
        performSearch(dependentWithGuardian.phn);

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "not.be.checked"
        );

        // Protect
        cy.get("[data-testid=dependent-protected-switch]").click();
        cy.get("[data-testid=dependent-protected-switch]").should("be.checked");
        cy.get("[data-testid=cancel-edit-button]").should(
            "be.visible",
            "be.enabled"
        );

        // Delegation Save button
        cy.get("[data-testid=save-button]").click();

        // Confirmation dialog confirmation button
        cy.get("[data-testid=audit-confirm-button]").should(
            "be.visible",
            "not.be.enabled"
        );

        // Enter protect reason
        cy.get("[data-testid=audit-reason-input]").type("test");

        // Cancel confirmation dialog
        cy.get("[data-testid=audit-cancel-button]").click();

        // Delegation Save button
        cy.get("[data-testid=save-button]").click();

        // Protect reason input is empty
        cy.get("[data-testid=audit-reason-input]").should("be.empty");

        // Cancel confirmation dialog
        cy.get("[data-testid=audit-cancel-button]").click();

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should("be.checked");

        // Delegation Save button
        cy.get("[data-testid=save-button]").should("be.visible", "be.enabled");

        // Delegation Cancel button
        cy.get("[data-testid=cancel-edit-button]").click();

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "not.be.checked"
        );
    });

    it("Verify add delegate dialog guardian not found and guardian already added.", () => {
        performSearch(dependentWithAudit);

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should("be.checked");

        // Add guardian
        cy.get("[data-testid=add-button]").click();

        // Delegate modal - phn not found
        cy.intercept("GET", "**/Delegation/Delegate").as("getDelegate");
        cy.get("[data-testid=delegate-phn-input]")
            .clear()
            .type(guardianNotFound);
        cy.get("[data-testid=communication-dialog-modal-text]").within(() => {
            cy.get("[data-testid=search-button]").click();
        });
        cy.wait("@getDelegate", { timeout: defaultTimeout });
        cy.get("[data-testid=delegate-search-error-message]").should(
            "be.visible"
        );

        // Cancel add guardian to list dialog
        cy.get("[data-testid=delegate-dialog-cancel-button]").click();

        // Add guardian
        cy.get("[data-testid=add-button]").click();

        cy.get("[data-testid=delegate-search-error-message]").should(
            "not.exist"
        );

        // Delegate dialog - phn already added
        cy.get("[data-testid=delegate-phn-input]")
            .clear()
            .type(guardianAlreadyAdded);
        cy.get("[data-testid=communication-dialog-modal-text]").within(() => {
            cy.get("[data-testid=search-button]").click();
        });
        cy.get("[data-testid=delegate-search-warning-message]").should(
            "be.visible"
        );
    });

    it("Verify protect/unprotect dependent toggle, add delegate, remove delegate, delegation save and delegation confirmation.", () => {
        performSearch(dependentToProtect);

        // Confirm delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 2);

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "not.be.checked"
        );

        // Protect
        cy.intercept("PUT", "**/Delegation/*/ProtectDependent").as(
            "protectDependent"
        );
        cy.get("[data-testid=dependent-protected-switch]").click();
        cy.get("[data-testid=dependent-protected-switch]").should("be.checked");

        // Delegation Save button
        cy.get("[data-testid=save-button]").click();

        // Delegation Confirmation button
        cy.get("[data-testid=audit-reason-input]").type("test");
        cy.get("[data-testid=audit-confirm-button]").click({ force: true });
        cy.wait("@protectDependent", { timeout: defaultTimeout });

        // Add guardian
        cy.get("[data-testid=add-button]").click();

        // Delegate dialog - search with valid phn
        cy.intercept("GET", "**/Delegation/Delegate").as("getDelegate");
        cy.get("[data-testid=delegate-phn-input]").clear().type(guardianToAdd);
        cy.get("[data-testid=communication-dialog-modal-text]").within(() => {
            cy.get("[data-testid=search-button]").click();
        });
        cy.wait("@getDelegate", { timeout: defaultTimeout });
        cy.get("[data-testid=delegate-search-error-message]").should(
            "not.exist"
        );

        // Delegate dialog - save
        cy.get("[data-testid=delegate-dialog-save-button]").click();

        // Delegation Save button
        cy.get("[data-testid=save-button]").click();

        // Delegation Confirmation button
        cy.get("[data-testid=audit-reason-input]").type("test");
        cy.get("[data-testid=audit-confirm-button]").click({ force: true });
        cy.wait("@protectDependent", { timeout: defaultTimeout });

        // Confirm guardian has been added to delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 3);

        // Confirm protected toggle has been enabled
        cy.get("[data-testid=dependent-protected-switch]").should("be.checked");

        // Delegation Edit button
        cy.get("[data-testid=edit-button]").click();

        // Remove last delegate row in table before protecting dependent
        const rowSelector =
            "[data-testid=delegate-table] tbody tr.mud-table-row";
        cy.get(rowSelector)
            .last()
            .within(() => {
                cy.get("[data-testid=delegate-to-be-removed-checkbox]").click();
            });

        // Delegation Save button
        cy.get("[data-testid=save-button]").click();

        // Delegation Confirmation button
        cy.get("[data-testid=audit-reason-input]").type("test");
        cy.get("[data-testid=audit-confirm-button]").click({ force: true });
        cy.wait("@protectDependent", { timeout: defaultTimeout });

        // Confirm delegate table
        getTableRows("[data-testid=delegate-table]").should("have.length", 2);

        // Unprotect
        cy.intercept("PUT", "**/Delegation/*/UnprotectDependent").as(
            "unprotectDependent"
        );
        cy.get("[data-testid=dependent-protected-switch]").click();

        // Confirmation button
        cy.get("[data-testid=audit-reason-input]").type("test");
        cy.get("[data-testid=audit-confirm-button]").click({ force: true });
        cy.wait("@unprotectDependent", { timeout: defaultTimeout });

        // Protect dependent toggle
        cy.get("[data-testid=dependent-protected-switch]").should(
            "not.be.checked"
        );
    });
});
