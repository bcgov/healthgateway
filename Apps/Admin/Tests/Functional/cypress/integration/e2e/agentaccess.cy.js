const username = Cypress.env("idir_username");
const password = Cypress.env("idir_password");

describe("Provision", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/provision"
        );
    });

    it.only("Create, search, update, delete user.", () => {
        cy.log("Create user.");

        cy.get("[data-testid=create-btn]").click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=username-input]").clear().type("FncTstUser1");
        cy.get("[data-testid=identity-provider-select]").click({
            force: true,
        });
        cy.get("[data-testid=identity-provider]")
            .contains("IDIR")
            .click({ force: true });
        cy.get("[data-testid=roles-select]").click({
            force: true,
        });
        cy.get("[data-testid=role]")
            .contains("AdminUser")
            .click({ force: true });
        cy.get("[data-testid=save-btn]").click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");

        cy.log("Validate user was created.");

        const rowSelector = "[data-testid=agent-table] tbody tr.mud-table-row";

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid^=agent-table-username-]").contains(
                    "fnctstuser1"
                );
                cy.get(
                    "[data-testid^=agent-table-identity-provider-]"
                ).contains("IDIR");
                cy.get("[data-testid^=agent-table-roles-]").contains(
                    "AdminUser"
                );
            });

        cy.log("Create duplicate user");

        cy.get("[data-testid=create-btn]").click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=username-input]").clear().type("FncTstUser1");
        cy.get("[data-testid=identity-provider-select]").click({
            force: true,
        });
        cy.get("[data-testid=identity-provider]")
            .contains("IDIR")
            .click({ force: true });
        cy.get("[data-testid=roles-select]").click({
            force: true,
        });
        cy.get("[data-testid=role]")
            .contains("AdminUser")
            .click({ force: true });
        cy.get("[data-testid=save-btn]").click({ force: true });

        cy.log("Validate duplicate user error.");

        cy.get("[data-testid=add-error-alert]").should("exist");
        cy.get("[data-testid=cancel-btn]").click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");

        cy.log("Edit user.");

        cy.get("[data-testid=query-input]").clear().type("fnct");
        cy.get("[data-testid=search-btn]").click({ force: true });
        cy.get("[data-testid^=agent-table-username-]")
            .contains("fnctstuser1")
            .parents(".mud-table-row")
            .get("[data-testid^=agent-table-edit-btn]")
            .click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=roles-select]").click({
            force: true,
        });
        cy.get("[data-testid=role]")
            .contains("SupportUser")
            .click({ force: true });
        cy.get("[data-testid=save-btn]").click({ force: true });
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");

        cy.log("Validate user edit.");

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid^=agent-table-username-]").contains(
                    "fnctstuser1"
                );
                cy.get(
                    "[data-testid^=agent-table-identity-provider-]"
                ).contains("IDIR");
                cy.get("[data-testid^=agent-table-roles-]").contains(
                    "AdminUser, SupportUser"
                );
            });

        cy.log("Delete user.");

        cy.get("[data-testid=query-input]").clear().type("fnct");
        cy.get("[data-testid=search-btn]").click({ force: true });
        cy.get("[data-testid^=agent-table-username-]")
            .contains("fnctstuser1")
            .parents(".mud-table-row")
            .get("[data-testid^=agent-table-delete-btn]")
            .click({ force: true });

        cy.get("[data-testid=confirm-delete-message]").should("be.visible");
        cy.get("[data-testid=confirm-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");

        cy.log("Validate user delete.");

        cy.contains(
            "[data-testid^=agent-table-username-]",
            "fnctstuser1"
        ).should("not.exist");
    });
});
