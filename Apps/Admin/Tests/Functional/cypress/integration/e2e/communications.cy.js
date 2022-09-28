function getTodayPlusDaysDate(days) {
    let ms = new Date(Date.now());
    ms.setDate(ms.getDate() + days);
    return ms.toLocaleDateString("en-CA");
}

describe("Communications", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/communications"
        );
    });

    it("Verify banner crud functions.", () => {
        cy.log("Validating data initialized by seed script.");

        cy.get("[data-testid=comm-table-subject]").contains("Test Banner");
        cy.get("[data-testid=comm-table-status]").contains("New");
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
        cy.get("[data-testid=comm-table-status]").contains("New");
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
        cy.get("[data-testid=comm-table-status]").contains("New");
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

        cy.log("Validate edit subject.");

        cy.get("[data-testid=comm-table-edit-btn]").click();
        cy.get("[data-testid=banner-dialog-modal-text]").within(() => {
            cy.get("[data-testid=subject-input]")
                .clear()
                .focus()
                .type("Edited Mobile Comm");
        });
        cy.get("[data-testid=save-btn]").click();
        cy.get("[data-testid=comm-table-subject]").contains(
            "Edited Mobile Comm"
        );

        cy.log("Validate edit subject finished.");

        cy.log("Validate delete communication.");

        cy.get("[data-testid=comm-table-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("be.visible");
        cy.get("[data-testid=confirm-cancel-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");
        cy.get("[data-testid=comm-table-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");
        cy.get("[data-testid=comm-table-subject]").should("not.exist");

        cy.log("Validate delete communication finished.");
    });
});
