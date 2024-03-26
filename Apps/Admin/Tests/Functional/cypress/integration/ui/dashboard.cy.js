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

    const dateString = date.toISOString().substring(0, 10);
    cy.log(`"${daysAgo} days ago" was ${dateString}`);
    return dateString;
}

describe("Dashboard", () => {
    beforeEach(() => {
        cy.log(`"Today" is ${utcDate.toISOString()}`);

        cy.intercept("GET", "**/Dashboard/AllTimeCounts*", {
            body: {
                registeredUsers: 6,
                dependents: 2,
                closedAccounts: 1,
            },
        });

        cy.intercept("GET", "**/Dashboard/DailyUsageCounts*", {
            body: {
                userRegistrations: {
                    [getPastDate(2)]: 1,
                    [getPastDate(1)]: 2,
                    [getPastDate(0)]: 2,
                },
                userLogins: {
                    [getPastDate(3)]: 1,
                    [getPastDate(2)]: 3,
                    [getPastDate(1)]: 2,
                    [getPastDate(0)]: 6,
                },
                dependentRegistrations: {
                    [getPastDate(0)]: 2,
                },
            },
        });

        // used to calculate [data-testid=average-rating]
        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary.json",
        });

        // matches [data-testid=recurring-user-count]
        cy.intercept("GET", "**/Dashboard/RecurringUserCount?days=3*", {
            body: 2,
        });

        cy.intercept("GET", "**/Dashboard/AppLoginCounts*", {
            body: {
                Mobile: 1,
                Web: 4,
                Salesforce: 2,
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
        cy.get("[data-testid=recurring-user-count]").contains(2);
        cy.get("[data-testid=total-mobile-users]").contains(1);
        cy.get("[data-testid=total-web-users]").contains(4);
        cy.get("[data-testid=total-salesforce-users]").contains(2);

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
        cy.intercept("GET", "**/Dashboard/RecurringUserCount?days=5*", {
            body: 0,
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(5);
        cy.get("[data-testid=recurring-user-count]").click();
        cy.get("[data-testid=recurring-user-count]").contains(0);

        cy.intercept("GET", "**/Dashboard/RecurringUserCount?days=2*", {
            body: 3,
        });
        cy.log("Updating unique days input value.");
        cy.get("[data-testid=unique-days-input]").clear().type(2);
        cy.get("[data-testid=recurring-user-count]").click();
        cy.get("[data-testid=recurring-user-count]").contains(3);

        cy.log("Dashboard test finished.");
    });
});
