import { selectTab } from "../../utilities/sharedUtilities";

const validEmail = "nobody@healthgateway.gov.bc.ca";
const notFoundEmail = "nobody@salesforce.gov.bc.ca";
const invalidEmail = "nobody@";

describe("Beta feature access", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/beta-access"
        );
    });

    it("Verify View tab is empty due to no seed data.", () => {
        cy.log("Verify view tab has no data.");
        selectTab("[data-testid=beta-access-tabs]", "View");
        cy.get("[data-testid=beta-access-table]").should("not.exist");
    });

    it("Verify error when search with invalid email", () => {
        cy.log("Verify search fails with invalid email.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=query-input]").clear().type(invalidEmail);
        cy.get(".d-flex").contains("Invalid email format").should("be.visible");
        cy.get("[data-testid=beta-access-table]").should("not.exist");
    });

    it("Verify error when search with not found email", () => {
        cy.log("Verify search fails with not found email.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=query-input]").clear().type(notFoundEmail);
        cy.get(".d-flex").contains("Invalid email format").should("not.exist");
        cy.get("[data-testid=search-button]").click();

        cy.get("[data-testid=get-user-access-error-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=beta-access-table]").should("not.exist");
        cy.get("[data-testid=beta-email]").should("not.exist");
    });

    it("Verify successful assign/un-assign feature access", () => {
        // Tab to Search and search with a valid email
        cy.log("Verify search with valid email.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=query-input]").clear().type(validEmail);
        cy.get(".d-flex").contains("Invalid email format").should("not.exist");
        cy.get("[data-testid=search-button]").click();

        cy.get("[data-testid=get-user-access-error-message]").should(
            "not.exist"
        );
        cy.get("[data-testid=beta-access-table]").should("be.visible");
        cy.get("[data-testid=salesforce-access-switch]").should(
            "not.be.checked"
        );

        // Assign salesforce feature
        cy.log("Verify assign salesforce feature on Search.");
        cy.get("[data-testid=salesforce-access-switch]").click();
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Search and verify results again
        cy.log(
            "After assigning salesforce access, search and verify assigning of salesforce feature again."
        );
        cy.get("[data-testid=query-input]").clear().type(validEmail);
        cy.get("[data-testid=search-button]").click();
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Tab to View and verify assigned feature(s)
        cy.log("Verify assigned feature(s) on View tab.");
        selectTab("[data-testid=beta-access-tabs]", "View");
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Tab back to Search and verify salesforce is still assigned
        cy.log("Tab back to search to verify salesforce is still assigned.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=email]").contains(validEmail);
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Tab back to View and salesforce is still assigned
        cy.log("Tab back to view to validate salesforce still assigned.");
        selectTab("[data-testid=beta-access-tabs]", "View");
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Un-assign salesforce feature on View
        cy.log("Verify un-assign salesforce feature access on View tab.");
        cy.get("[data-testid=salesforce-access-switch]").click();
        cy.get("[data-testid=beta-access-table]").should("not.exist");

        // Tab back to Search and verify salesforce is un-assigned
        cy.log("Verify salesforce feature un-assigned on Search tab.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=email]").contains(validEmail);
        cy.get("[data-testid=salesforce-access-switch]").should(
            "not.be.checked"
        );

        // Assign salesforce feature again on Search
        cy.log("Verify re-assign salesforce feature access on Search tab.");
        cy.get("[data-testid=salesforce-access-switch]").click();
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Tab back to View and verify salesforce is assigned
        selectTab("[data-testid=beta-access-tabs]", "View");
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Tab back to Search and un-assign salesforce feature
        cy.log("Verify salesforce feature still assigned on Search tab.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=email]").contains(validEmail);
        cy.get("[data-testid=salesforce-access-switch]").should("be.checked");

        // Un-assign salesforce on Search
        cy.log("Verify un-assign salesforce feature on Search tab.");
        cy.get("[data-testid=salesforce-access-switch]").click();
        cy.get("[data-testid=salesforce-access-switch]").should(
            "not.be.checked"
        );

        // Tab back to View and verify salesforce is un-assigned
        cy.log("Tab back to View to verify salesforce still un-assigned.");
        selectTab("[data-testid=beta-access-tabs]", "View");
        cy.get("[data-testid=beta-access-table]").should("not.exist");
    });
});
