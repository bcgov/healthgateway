// midnight on 2022-07-02, the middle of the year
const utcDate = new Date(Date.UTC(2022, 7 - 1, 2));
const localDate = new Date(2022, 7 - 1, 2);

function getPastDate(daysAgo) {
    // initialize date to utcDate minus a number of days
    const date = new Date(utcDate);
    date.setDate(date.getDate() - daysAgo);

    // the calculated date may be 1 hour off because of Daylight Savings
    // the value will be positive (+1) so long as theDate is in the summer
    // resetting the hours to 0 will fix this, since the day boundary hasn't been crossed
    date.setUTCHours(0);

    cy.log(`"${daysAgo} days ago" was ${date.toISOString()}`);
    return date.toISOString();
}

describe("Dashboard", () => {
    beforeEach(() => {
        cy.log(`"Today" is ${utcDate.toISOString()}`);

        cy.intercept("GET", "**/Dashboard/RegisteredCount*", {
            body: {
                [getPastDate(120)]: 1,
                [getPastDate(2)]: 1,
                [getPastDate(1)]: 2,
                [getPastDate(0)]: 2,
            },
        });

        cy.intercept("GET", "**/Dashboard/LoggedInCount*", {
            body: {
                [getPastDate(120)]: 1,
                [getPastDate(3)]: 1,
                [getPastDate(2)]: 3,
                [getPastDate(1)]: 2,
                [getPastDate(0)]: 6,
            },
        });

        cy.intercept("GET", "**/Dashboard/DependentCount*", {
            body: {
                [getPastDate(0)]: 2,
            },
        });

        // used to calculate [data-testid=average-rating]
        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary.json",
        });

        // matches [data-testid=total-unique-users]
        cy.intercept("GET", "**/Dashboard/RecurringUserCounts?days=3*", {
            body: {
                Mobile: 1,
                Web: 4,
                RecurringUserCount: 2,
            },
        });

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/analytics"
        );

        // changing the date must be done after logging in
        cy.clock(localDate, ["Date"]);

        // the dashboard page must be loaded (or reloaded) after the date is changed
        cy.visit("/dashboard");
    });

    it("Verify dashboard counts and skeletons.", () => {
        cy.log("Dashboard test started.");
        cy.get("[data-testid=total-registered-users]").contains(6);
        cy.get("[data-testid=total-dependents]").contains(2);
        cy.get("[data-testid=average-rating]").contains("4.00");
        cy.get("[data-testid=total-unique-users]").contains(2);
        cy.get("[data-testid=total-mobile-users]").contains(1);
        cy.get("[data-testid=total-web-users]").contains(4);

        cy.get("[data-testid=daily-data-table]")
            .find("tbody tr.mud-table-row")
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
        cy.intercept("GET", "**/Dashboard/RecurringUserCounts?days=5*", {
            body: {
                Mobile: 0,
                Web: 0,
                RecurringUserCount: 0,
            },
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(5);
        cy.get("[data-testid=total-unique-users]").click();
        cy.get("[data-testid=total-unique-users]").contains(0);

        cy.intercept("GET", "**/Dashboard/RecurringUserCounts?days=2*", {
            body: {
                Mobile: 0,
                Web: 0,
                RecurringUserCount: 3,
            },
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(2);
        cy.get("[data-testid=total-unique-users]").click();
        cy.get("[data-testid=total-unique-users]").contains(3);

        cy.log("Clicking refresh button.");
        cy.intercept("GET", "**/Dashboard/RegisteredCount*", {
            body: {
                [getPastDate(120)]: 1,
                [getPastDate(2)]: 1,
                [getPastDate(1)]: 2,
                [getPastDate(0)]: 3,
            },
        });

        cy.intercept("GET", "**/Dashboard/LoggedInCount*", {
            body: {
                [getPastDate(120)]: 1,
                [getPastDate(3)]: 1,
                [getPastDate(2)]: 3,
                [getPastDate(1)]: 2,
                [getPastDate(0)]: 7,
            },
        });

        cy.intercept("GET", "**/Dashboard/DependentCount*", {
            body: {
                [getPastDate(0)]: 3,
            },
        });

        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary-refresh.json",
        });

        cy.intercept("GET", "**/Dashboard/RecurringUserCounts?days=2*", {
            body: {
                Mobile: 0,
                Web: 0,
                RecurringUserCount: 10,
            },
        });

        cy.get("[data-testid=refresh-btn]").click();
        cy.get("[data-testid=total-registered-users]").contains(7);
        cy.get("[data-testid=total-dependents]").contains(3);
        cy.get("[data-testid=average-rating]").contains("3.00");
        cy.get("[data-testid=total-unique-users]").contains(10);

        cy.get("[data-testid=daily-data-table]")
            .find("tbody tr.mud-table-row")
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
