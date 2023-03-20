const dependentWithoutGuardian = { phn: "9874307168" };
const dependentWithGuardian = { phn: "9874307175", guardianPhn: "9735353315" };
const dependentExceedingAgeCutoff = { phn: "9735353315" };

function performSearch(phn) {
    cy.get("[data-testid=query-input]").clear().type(phn);
    cy.get("[data-testid=search-btn]").click();
}

function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr");
}

describe("Delegation", () => {
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
        cy.get("[data-testid=search-btn]").click();

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
