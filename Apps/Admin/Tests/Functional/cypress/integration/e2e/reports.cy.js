import { getTableRows } from "../../utilities/sharedUtilities";

const blockedHdid = "GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA";

describe("Reports", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/reports"
        );
    });

    it("Verify reports", () => {
        cy.log("Checking protected dependents table");
        getTableRows("[data-testid=protected-dependents-table]").should(
            "have.length.gt",
            0
        );

        cy.log("Checking blocked access table");
        getTableRows("[data-testid=blocked-access-table]").should(
            "have.length.gt",
            0
        );

        cy.log("Checking blocked access row click goes to patient details");
        cy.contains("[data-testid=blocked-access-hdid]", blockedHdid)
            .should("be.visible")
            .click();

        cy.url().should("include", "/patient-details");
        cy.contains("[data-testid=patient-hdid]", blockedHdid).should(
            "be.visible"
        );
    });
});
