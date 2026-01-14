import { getTodayPlusDaysDate } from "../../utilities/sharedUtilities";

describe("Communications", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/communications"
        );
    });

    it("Verify communications.", () => {
        cy.log("Validating data initialized by seed script.");

        cy.get("[data-testid=banner-tabs]")
            .contains(".mud-tab", "Public Banners")
            .click();
        cy.get("[data-testid=comm-table-subject]").contains("Test Banner");
        cy.get("[data-testid=comm-table-status]").contains("Publish");
        cy.get("[data-testid=comm-table-effective-date]").contains(
            getTodayPlusDaysDate(0)
        );
        cy.get("[data-testid=comm-table-expiry-date]").contains(
            getTodayPlusDaysDate(1)
        );
        cy.get("[data-testid=comm-table-expand-btn]").click();
        cy.get("[data-testid=comm-table-expanded-text]").contains(
            "Test Banner - healthgateway@gov.bc.ca"
        );

        cy.get("[data-testid=banner-tabs]")
            .contains(".mud-tab", "In-App Banners")
            .click();
        cy.get("[data-testid=comm-table-subject]").contains("In-App Banner");
        cy.get("[data-testid=comm-table-status]").contains("Publish");
        cy.get("[data-testid=comm-table-effective-date]").contains(
            getTodayPlusDaysDate(0)
        );
        cy.get("[data-testid=comm-table-expiry-date]").contains(
            getTodayPlusDaysDate(1)
        );
        cy.get("[data-testid=comm-table-expand-btn]").click();
        cy.get("[data-testid=comm-table-expanded-text]").contains(
            "In-App Banner - healthgateway@gov.bc.ca"
        );

        cy.get("[data-testid=banner-tabs]")
            .contains(".mud-tab", "Mobile")
            .click();
        cy.get("[data-testid=comm-table-subject]").contains(
            "Seeded Mobile Comm"
        );
        cy.get("[data-testid=comm-table-status]").contains("Publish");
        cy.get("[data-testid=comm-table-effective-date]").contains(
            getTodayPlusDaysDate(0)
        );
        cy.get("[data-testid=comm-table-expiry-date]").contains(
            getTodayPlusDaysDate(1)
        );
        cy.get("[data-testid=comm-table-expand-btn]").click();
        cy.get("[data-testid=comm-table-expanded-text]").contains(
            "Mobile Communication - healthgateway@gov.bc.ca"
        );

        cy.log("Validating data initialized by seed script finished.");
    });
});
