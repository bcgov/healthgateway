const suggestionTag = "suggestion";
const questionTag = "question";

function validateTableRowCount(tableSelector, count) {
    cy.log(`Validating table contains ${count} rows of data.`);
    cy.get(tableSelector)
        .find("tbody tr.mud-table-row")
        .should("have.length", count);
}

const rowSelector = "[data-testid=feedback-table] tbody tr.mud-table-row";

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
        validateTableRowCount("[data-testid=feedback-table]", 2);

        cy.log("Reviewing feedback.");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-review-button]")
                    .should("be.visible")
                    .should("have.class", "mud-error-text")
                    .click();
            });
        cy.validateTableLoad("[data-testid=feedback-table]");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-review-button]")
                    .should("be.visible")
                    .should("have.class", "mud-success-text")
                    .click();
            });

        cy.log("Adding tags.");
        cy.get("[data-testid=add-tag-input]").clear().type(suggestionTag);
        cy.get("[data-testid=add-tag-button]").click();
        cy.get("[data-testid=tags-updating-indicator").should("be.visible");
        cy.get("[data-testid=tag-collection-item]").contains(suggestionTag);
        cy.get("[data-testid=add-tag-input]").clear().type(questionTag);
        cy.get("[data-testid=add-tag-button]").click();
        cy.get("[data-testid=tags-updating-indicator").should("be.visible");
        cy.get("[data-testid=tag-collection-item]").contains(questionTag);

        cy.log("Assigning tags.");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").click();
            });
        cy.get("[data-testid=feedback-tag]").contains(suggestionTag).click();
        cy.validateTableLoad("[data-testid=feedback-table]");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").should(
                    "have.value",
                    suggestionTag
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
        cy.validateTableLoad("[data-testid=feedback-table]");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-tag-select]").should(
                    "not.have.value",
                    suggestionTag
                );
            });

        cy.log("Removing tags.");
        cy.get("[data-testid=tag-collection-item]")
            .contains(suggestionTag)
            .children("button")
            .click();
        cy.get("[data-testid=tags-updating-indicator").should("be.visible");
        cy.get("[data-testid=tag-collection-item]")
            .contains(questionTag)
            .children("button")
            .click();
        cy.get("[data-testid=tags-updating-indicator").should("be.visible");
        cy.get("[data-testid=tag-collection-item]").should("not.exist");
    });

    it("Navigating to Support Page", () => {
        cy.log("Looking up feedback author.");
        cy.get(rowSelector)
            .first()
            .within(() => {
                cy.get("[data-testid=feedback-person-search-button]")
                    .should("be.visible")
                    .click();
            });
        cy.location("pathname").should("eq", "/support");
        //cy.validateTableLoad("[data-testid=user-table]");
        validateTableRowCount("[data-testid=user-table]", 1);
    });
});
