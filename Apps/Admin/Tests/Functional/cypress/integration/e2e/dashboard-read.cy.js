describe("Dashboard", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/dashboard"
        );
    });

    it("Verify dashboard counts from seeded data.", () => {
        cy.log("Dashboard test started.");
        cy.get("[data-testid=total-registered-users]").contains(8);
        cy.get("[data-testid=total-dependents]").contains(6);
        cy.get("[data-testid=total-closed-accounts]").contains(1);
        cy.get("[data-testid=recurring-user-count]").contains(2);
        cy.get("[data-testid=total-mobile-users]").contains(4);
        cy.get("[data-testid=total-web-users]").contains(3);
        cy.get("[data-testid=total-salesforce-users]").contains(1);
        cy.get("[data-testid=average-rating]").contains("4.00");

        cy.get("[data-testid=daily-data-table]")
            .first()
            .within(() => {
                cy.get(
                    "[data-testid=daily-data-total-registered-users]"
                ).contains("2");
                cy.get(
                    "[data-testid=daily-data-total-logged-in-users]"
                ).contains("6");
                cy.get("[data-testid=daily-data-dependents]").contains("6");
            });

        cy.log("Change value in unique days input field.");
        cy.get("[data-testid=unique-days-input]").clear().type(5);
        cy.get("[data-testid=recurring-user-count]").click();
        cy.get("[data-testid=recurring-user-count]").contains(0);

        cy.get("[data-testid=unique-days-input]").clear().type(2);
        cy.get("[data-testid=recurring-user-count]").click();
        cy.get("[data-testid=recurring-user-count]").contains(3);

        cy.log("Dashboard test finished.");
    });
});
