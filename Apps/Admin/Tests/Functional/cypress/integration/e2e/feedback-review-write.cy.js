const suggestionTag = "suggestion";
const questionTag = "question";
const defaultTimeout = 60000;
const rowSelector = "[data-testid=feedback-table] tbody tr.mud-table-row";

function validateTableRowCount(tableSelector, count) {
    cy.log(`Validating table contains ${count} rows of data.`);
    cy.get(tableSelector)
        .find("tbody tr.mud-table-row")
        .should("have.length.gte", count);
}

describe("Feedback Review", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/feedback"
        );
    });

    it("Tag and Feedback Table Functionality", () => {
        cy.log("Validating data initialized by seed script.");
        cy.get("[data-testid=tag-collection]").should("exist");
        cy.get("[data-testid=tag-collection-item]").should("not.exist");
        validateTableRowCount("[data-testid=feedback-table]", 4);

        cy.log("Reviewing feedback.");
        cy.intercept("PATCH", "**/UserFeedback/").as("patchUserFeedback");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-review-button]")
                    .should("be.visible")
                    .should("have.class", "mud-error-text")
                    .click();
            });
        cy.wait("@patchUserFeedback", { timeout: defaultTimeout });

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-review-button]")
                    .should("be.visible")
                    .should("have.class", "mud-success-text")
                    .click();
            });
        cy.wait("@patchUserFeedback", { timeout: defaultTimeout });

        cy.log("Adding tags.");
        cy.intercept("POST", "**/Tag/").as("postTag");
        cy.get("[data-testid=add-tag-input]").clear().type(suggestionTag);
        cy.get("[data-testid=add-tag-button]").click();
        cy.wait("@postTag", { timeout: defaultTimeout });
        cy.get("[data-testid=tag-collection-item]").contains(suggestionTag);
        cy.get("[data-testid=add-tag-input]").clear().type(questionTag);
        cy.get("[data-testid=add-tag-button]").click();
        cy.wait("@postTag", { timeout: defaultTimeout });
        cy.get("[data-testid=tag-collection-item]").contains(questionTag);

        cy.log("Assigning tags.");
        cy.intercept("PUT", "**/UserFeedback/*/Tag").as("putUserFeedbackTag");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").click();
            });
        cy.get("[data-testid=feedback-tag]").contains(suggestionTag).click();
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-save-button]")
                    .should("be.enabled")
                    .click();
            });
        cy.wait("@putUserFeedbackTag", { timeout: defaultTimeout });

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").should(
                    "have.value",
                    suggestionTag
                );
                cy.get("[data-testid=feedback-tag-save-button]").should(
                    "not.be.enabled"
                );
            });

        cy.log("Filtering tags.");
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .click();
        validateTableRowCount("[data-testid=feedback-table]", 1);
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .click();
        validateTableRowCount("[data-testid=feedback-table]", 2);

        cy.log("Validating tags cannot be removed while assigned to feedback.");
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .children("button")
            .click();
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .should("be.visible");

        cy.log("Unassigning tags.");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").click();
            });
        cy.get("[data-testid=feedback-tag]").contains(suggestionTag).click();
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-save-button]")
                    .should("be.enabled")
                    .click();
            });
        cy.wait("@putUserFeedbackTag", { timeout: defaultTimeout });

        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").should(
                    "not.have.value",
                    suggestionTag
                );
                cy.get("[data-testid=feedback-tag-save-button]").should(
                    "not.be.enabled"
                );
            });

        cy.log("Removing tags.");
        cy.intercept("DELETE", "**/Tag/").as("deleteTag");
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .children("button")
            .click();
        cy.wait("@deleteTag", { timeout: defaultTimeout });
        cy.get("[data-testid=tag-collection-item]")
            .contains(questionTag)
            .children("button")
            .click();
        cy.wait("@deleteTag", { timeout: defaultTimeout });
        cy.get("[data-testid=tag-collection-item]").should("not.exist");
    });
});
