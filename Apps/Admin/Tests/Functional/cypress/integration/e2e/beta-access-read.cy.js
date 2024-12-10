import { getTableRows, selectTab } from "../../utilities/sharedUtilities";

const existingEmail = "somebody@healthgateway.gov.bc.ca";
const notFoundEmail = "nobody@salesforce.gov.bc.ca";
const invalidEmail = "nobody@";
const defaultTimeout = 60000;

function setupGetUserAccessAlias() {
    cy.intercept("GET", "**/BetaFeature/UserAccess*").as("getUserAccess");
}

function waitForGetUserAccess() {
    cy.wait("@getUserAccess", { timeout: defaultTimeout });
}

describe("Beta feature access", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/beta-access"
        );
    });

    it("Verify View tab has seed data.", () => {
        cy.log("Verify view tab has 1 entry.");
        selectTab("[data-testid=beta-access-tabs]", "View");
        getTableRows("[data-testid=beta-access-table]")
            .should("have.length", 1)
            .first()
            .within(() => {
                cy.get("[data-testid=email]").contains(existingEmail);
                cy.get("[data-testid=salesforce-access-switch]").should(
                    "be.checked"
                );
            });
    });

    it("Verify error when search with invalid email", () => {
        cy.log("Verify search fails with invalid email.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=query-input]")
            .should("be.visible")
            .should("be.enabled")
            .clear()
            .type(invalidEmail);
        cy.get(".d-flex").contains("Invalid email format").should("be.visible");
        cy.get("[data-testid=beta-access-table]").should("not.exist");
    });

    it("Verify error when search with not found email", () => {
        cy.log("Verify search fails with not found email.");
        selectTab("[data-testid=beta-access-tabs]", "Search");
        cy.get("[data-testid=query-input]")
            .should("be.visible")
            .should("be.enabled")
            .clear()
            .type(notFoundEmail);

        cy.get(".d-flex").contains("Invalid email format").should("not.exist");
        setupGetUserAccessAlias();
        cy.get("[data-testid=search-button]").click();
        waitForGetUserAccess();

        cy.get("[data-testid=get-user-access-error-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=beta-access-table]").should("not.exist");
        cy.get("[data-testid=beta-email]").should("not.exist");
    });
});
