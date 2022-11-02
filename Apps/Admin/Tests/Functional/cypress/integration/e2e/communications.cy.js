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

    it("Verify notification CRUD functions.", () => {
        cy.log("Create notification.");

        cy.get("[data-testid=create-btn]").should("be.visible").click();
        cy.get("[data-testid=broadcast-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=subject-input]")
            .should("not.be.disabled")
            .clear()
            .focus()
            .type("Test Notification Subject");
        cy.get("[data-testid=content-input]")
            .should("not.be.disabled")
            .clear()
            .focus()
            .type("Test Notification Content");
        cy.get("[data-testid=save-btn]").click();
        cy.get("[data-testid=broadcast-dialog-modal-text]").should("not.exist");

        cy.log("Validate notification was created.");

        const rowSelector =
            "[data-testid=broadcast-table] tbody tr.mud-table-row";

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-subject]")
                    .should("be.visible")
                    .contains("Test Notification Subject");
                cy.get("[data-testid=broadcast-table-action-type]")
                    .should("be.visible")
                    .contains("None");
                cy.get("[data-testid=broadcast-table-effective-date]").contains(
                    getTodayPlusDaysDate(0)
                );
                cy.get("[data-testid=broadcast-table-expiry-date]").contains(
                    getTodayPlusDaysDate(1)
                );
                cy.get("[data-testid=broadcast-table-expand-btn]").click();
            });

        cy.get("[data-testid=broadcast-table-expanded-text]").contains(
            "Test Notification Content"
        );

        cy.log("Edit notification.");

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-edit-btn]").click();
            });

        cy.get("[data-testid=broadcast-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=subject-input]")
            .should("not.be.disabled")
            .clear()
            .focus()
            .type("Edited Test Notification Subject");
        cy.get("[data-testid=publish-select]").click({ force: true });
        cy.get("[data-testid=publish-value]")
            .contains("Publish")
            .click({ force: true });
        cy.get("[data-testid=action-type-select]").click({ force: true });
        cy.get("[data-testid=action-type]")
            .contains("Internal Link")
            .click({ force: true });
        cy.get("[data-testid=action-url-input]")
            .should("not.be.disabled")
            .clear()
            .focus()
            .type("https://www.healthgateway.gov.bc.ca");

        cy.get("[data-testid=save-btn]").click();
        cy.get("[data-testid=broadcast-dialog-modal-text]").should("not.exist");

        cy.log("Validate notification was edited.");

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-subject]")
                    .should("be.visible")
                    .contains("Edited Test Notification Subject");
                cy.get("[data-testid=broadcast-table-action-type]")
                    .should("be.visible")
                    .contains("Internal Link");
            });

        cy.log("Delete notification.");

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-delete-btn]").click();
            });

        cy.get("[data-testid=confirm-delete-message]").should("be.visible");
        cy.get("[data-testid=confirm-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");

        cy.log("Validate notification was deleted.");

        cy.validateTableLoad("[data-testid=broadcast-table]");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-subject]").should(
                    "not.contain",
                    "Edited Test Notification Subject"
                );
            });
    });

    it("Verify communication CRUD functions.", () => {
        cy.log("Validating data initialized by seed script.");

        cy.get("[data-testid=banner-tabs]")
            .contains(".mud-tab", "Public Banners")
            .click();
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
        cy.get("[data-testid=communication-dialog-modal-text]").within(() => {
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

        cy.log("Validate add communication.");

        cy.get("[data-testid=banner-tabs]")
            .contains(".mud-tab", "Mobile")
            .click();
        cy.get("[data-testid=create-btn]").click();
        cy.get("[data-testid=communication-dialog-modal-text]").within(() => {
            cy.get("[data-testid=subject-input]")
                .clear()
                .focus()
                .type("New Mobile Comm");
        });
        cy.get("[data-testid=status-select]").click({ force: true });
        cy.get("[data-testid=status-type]")
            .contains("New")
            .click({ force: true });
        cy.get("[data-testid=save-btn]").click({ force: true });
        cy.get("[data-testid=comm-table-subject]").contains(
            "New Mobile Comm"
        );

        cy.log("Validate add communication finished.");
    });
});
