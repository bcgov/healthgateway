function getLastLoginDate(days) {
    let lostLoginDate = new Date(new Date().toString());
    lostLoginDate.setDate(lostLoginDate.getDate() - days);
    lostLoginDate.setUTCHours(0, 0, 0, 0);
    cy.log(`Last Login Date: ${lostLoginDate.toISOString()}`);
    return lostLoginDate.toISOString();
}

describe("Dashboard", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Dashboard/RegisteredCount*", {
            body: {
                [getLastLoginDate(120)]: 1,
                [getLastLoginDate(2)]: 1,
                [getLastLoginDate(1)]: 2,
                [getLastLoginDate(0)]: 2,
            },
        });

        cy.intercept("GET", "**/Dashboard/LoggedInCount*", {
            body: {
                [getLastLoginDate(120)]: 1,
                [getLastLoginDate(3)]: 1,
                [getLastLoginDate(2)]: 3,
                [getLastLoginDate(1)]: 2,
                [getLastLoginDate(0)]: 6,
            },
        });

        cy.intercept("GET", "**/Dashboard/DependentCount*", {
            body: {
                [getLastLoginDate(0)]: 2,
            },
        });

        // used to calculate [data-testid=average-rating]
        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary.json",
        });

        // matches [data-testid=total-unique-users]
        cy.intercept("GET", "**/Dashboard/RecurringUsers?days=3*", {
            body: 2,
        });

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/"
        );
    });

    it("Verify dashboard counts and skeletons.", () => {
        cy.log("Dashboard test started.");
        cy.get("[data-testid=total-registered-users]").contains(6);
        cy.get("[data-testid=total-dependents]").contains(2);
        cy.get("[data-testid=average-rating]").contains("4.00");
        cy.get("[data-testid=total-unique-users]").contains(2);

        cy.get("[data-testid=daily-data-table]")
            .first()
            .within(() => {
                cy.get(
                    "[data-testid=daily-data-total-registered-users]"
                ).contains("2");
                cy.get(
                    "[data-testid=daily-data-total-logged-in-users]"
                ).contains("6");
                cy.get("[data-testid=daily-data-dependents]").contains("2");
            });

        cy.log("Change value in unique days input field.");
        cy.intercept("GET", "**/Dashboard/RecurringUsers?days=5*", {
            body: 0,
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(5);
        cy.get("[data-testid=total-unique-users]").click();
        cy.get("[data-testid=total-unique-users]").contains(0);

        cy.intercept("GET", "**/Dashboard/RecurringUsers?days=2*", {
            body: 3,
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(2);
        cy.get("[data-testid=total-unique-users]").click();
        cy.get("[data-testid=total-unique-users]").contains(3);

        cy.log("Clicking refresh button.");
        cy.intercept("GET", "**/Dashboard/RegisteredCount*", {
            body: {
                [getLastLoginDate(120)]: 1,
                [getLastLoginDate(2)]: 1,
                [getLastLoginDate(1)]: 2,
                [getLastLoginDate(0)]: 3,
            },
        });

        cy.intercept("GET", "**/Dashboard/LoggedInCount*", {
            body: {
                [getLastLoginDate(120)]: 1,
                [getLastLoginDate(3)]: 1,
                [getLastLoginDate(2)]: 3,
                [getLastLoginDate(1)]: 2,
                [getLastLoginDate(0)]: 7,
            },
        });

        cy.intercept("GET", "**/Dashboard/DependentCount*", {
            body: {
                [getLastLoginDate(0)]: 3,
            },
        });

        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary-refresh.json",
        });

        cy.intercept("GET", "**/Dashboard/RecurringUsers?days=2*", {
            body: 10,
        });

        cy.get("[data-testid=refresh-btn]").click();
        cy.get("[data-testid=total-registered-users]").contains(7);
        cy.get("[data-testid=total-dependents]").contains(3);
        cy.get("[data-testid=average-rating]").contains("3.00");
        cy.get("[data-testid=total-unique-users]").contains(10);

        cy.get("[data-testid=daily-data-table]")
            .first()
            .within(() => {
                cy.get(
                    "[data-testid=daily-data-total-registered-users]"
                ).contains("3");
                cy.get(
                    "[data-testid=daily-data-total-logged-in-users]"
                ).contains("7");
                cy.get("[data-testid=daily-data-dependents]").contains("3");
            });

        cy.get("[data-testid=refresh-btn]").click();
        cy.get("[data-testid=skeleton-registered-users]").should("be.visible");
        cy.get("[data-testid=skeleton-dependents]").should("be.visible");
        cy.get("[data-testid=skeleton-selected-date-range]").should(
            "be.visible"
        );
        cy.get("[data-testid=skeleton-unique-days]").should("be.visible");
        cy.get("[data-testid=skeleton-user-count]").should("be.visible");
        cy.get("[data-testid=skeleton-rating-summary]").should("be.visible");
        cy.log("Dashboard test finished.");
    });
});
