import { removeUserIfExists } from "../../utilities/kcUtilities";

const username = Cypress.env("idir_username");
const password = Cypress.env("idir_password");
const user = "FncTstUser1";
const timeout = 45000;

describe("Provision", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/provision"
        );
    });

    it("Create and Validate User", () => {
        // Clean User
        removeUserIfExists(user);

        cy.log("Create user.");
        cy.get("[data-testid=create-btn]").click();
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=username-input]").clear().type(user);
        cy.get("[data-testid=identity-provider-select]").click({ force: true });
        cy.get("[data-testid=identity-provider]").contains("IDIR").click();
        cy.get("[data-testid=roles-select]").click();
        cy.get("[data-testid=role]").contains("AdminUser").click();
        cy.get("[data-testid=save-btn]").parent().parent().click(0, 0);
        cy.get("[data-testid=save-btn]").click();
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");

        cy.log("Validate user was created.");
        const rowSelector = "[data-testid=agent-table] tbody tr.mud-table-row";
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid^=agent-table-username-]").contains(
                    user.toLowerCase()
                );
                cy.get(
                    "[data-testid^=agent-table-identity-provider-]"
                ).contains("IDIR");
                cy.get("[data-testid^=agent-table-roles-]").contains(
                    "AdminUser"
                );
            });
    });

    it("Create Duplicate User", () => {
        cy.get("[data-testid=create-btn]").click();
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=username-input]").clear().type(user);
        cy.get("[data-testid=identity-provider-select]").click({ force: true });
        cy.get("[data-testid=identity-provider]").contains("IDIR").click();
        cy.get("[data-testid=roles-select]").click();
        cy.get("[data-testid=role]").contains("AdminUser").click();
        cy.get("[data-testid=save-btn]").parent().parent().click(0, 0);
        cy.get("[data-testid=save-btn]").click();

        cy.log("Validate duplicate user error.");
        cy.get("[data-testid=add-error-alert]").should("exist");
        cy.get("[data-testid=cancel-btn]").click();
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");
    });

    it("Edit User", () => {
        cy.intercept("GET", `**/AgentAccess/?query=${user}`).as("getUser");
        cy.get("[data-testid=query-input]").clear().type(user);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid^=agent-table-username-]")
            .contains(user.toLowerCase())
            .parents(".mud-table-row")
            .get("[data-testid^=agent-table-edit-btn]")
            .click();
        cy.wait("@getUser", { timeout });
        cy.get("[data-testid=provision-dialog-modal-text]").should(
            "be.visible"
        );
        cy.get("[data-testid=roles-select]").click();
        cy.get("[data-testid=role]").contains("AdminAnalyst").click();
        cy.get("[data-testid=save-btn]").parent().parent().click(0, 0);
        cy.get("[data-testid=save-btn]").click();
        cy.get("[data-testid=provision-dialog-modal-text]").should("not.exist");

        cy.log("Validate user edit.");
        const rowSelector = "[data-testid=agent-table] tbody tr.mud-table-row";
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid^=agent-table-username-]").contains(
                    user.toLowerCase()
                );
                cy.get(
                    "[data-testid^=agent-table-identity-provider-]"
                ).contains("IDIR");
                cy.get("[data-testid^=agent-table-roles-]").contains(
                    "AdminAnalyst, AdminUser"
                );
            });
    });

    it("Delete User", () => {
        cy.get("[data-testid=query-input]").clear().type(user);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid^=agent-table-username-]")
            .contains(user.toLowerCase())
            .parents(".mud-table-row")
            .get("[data-testid^=agent-table-delete-btn]")
            .click();
        cy.get("[data-testid=confirm-delete-message]").should("be.visible");
        cy.get("[data-testid=confirm-delete-btn]").click();
        cy.get("[data-testid=confirm-delete-message]").should("not.exist");

        cy.log("Validate user delete.");
        cy.contains("[data-testid=agent-table-username-]", user).should(
            "not.exist"
        );
    });
});
