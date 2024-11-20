import { getTodayPlusDaysDate } from "../../utilities/sharedUtilities";

const defaultTimeout = 60000;

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

        cy.intercept("POST", "**/Broadcast/").as("postBroadcast");
        cy.get("[data-testid=save-btn]").click();
        cy.wait("@postBroadcast", { timeout: defaultTimeout });
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

        cy.intercept("PUT", "**/Broadcast/").as("putBroadcast");
        cy.get("[data-testid=save-btn]").click();
        cy.wait("@putBroadcast", { timeout: defaultTimeout });
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
        cy.intercept("DELETE", "**/Broadcast/").as("deleteBroadcast");
        cy.get("[data-testid=confirm-delete-btn]").click();
        cy.wait("@deleteBroadcast", { timeout: defaultTimeout });
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");

        cy.log("Validate notification was deleted.");

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=broadcast-table-subject]").should(
                    "not.contain",
                    "Edited Test Notification Subject"
                );
            });
    });
});
