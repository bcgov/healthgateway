import "cypress-real-events/support";

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

function setupAppLoginCountsFixtures(
    mobileCount = 3,
    androidCount = 1,
    iosCount = 1
) {
    cy.intercept("GET", "**/Dashboard/AppLoginCounts*", {
        body: {
            mobile: mobileCount,
            android: androidCount,
            ios: iosCount,
            web: 4,
            salesforce: 2,
        },
    });
}

describe("Dashboard", () => {
    beforeEach(() => {
        cy.log(`"Today" is ${utcDate.toISOString()}`);

        cy.intercept("GET", "**/Dashboard/AllTimeCounts*", {
            fixture: "DashboardService/all-time-counts.json",
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

        setupAppLoginCountsFixtures();

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
        cy.get("[data-testid=total-mobile-users]").contains(3);
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

describe("Dashboard Mobile Login Count Tooltip", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Dashboard/AllTimeCounts*", {
            fixture: "DashboardService/all-time-counts.json",
        });

        cy.intercept("GET", "**/Dashboard/DailyUsageCounts*", {
            fixture: "DashboardService/daily-usage-counts.json",
        });

        cy.intercept("GET", "**/Dashboard/Ratings/Summary*", {
            fixture: "DashboardService/summary.json",
        });

        cy.intercept("GET", "**/Dashboard/RecurringUserCount?days=3*", {
            body: 2,
        });
    });

    it("Verify tooltip exists with android count > 0 and ios count > 0.", () => {
        cy.log("Dashboard test started.");

        const mobileCount = 3;
        const androidCount = 1;
        const iosCount = 1;

        setupAppLoginCountsFixtures(mobileCount, androidCount, iosCount);

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/dashboard"
        );

        cy.get("[data-testid=total-mobile-users]").contains(mobileCount);

        cy.get("[data-testid=android-count]").should("not.exist");
        cy.get("[data-testid=ios-count]").should("not.exist");

        // Activate tooltip
        cy.get("[data-testid=total-mobile-users]").realHover();

        cy.get("[data-testid=android-count]").contains(androidCount);
        cy.get("[data-testid=ios-count]").contains(iosCount);

        cy.log("Dashboard test finished.");
    });

    it("Verify tooltip exists with android count == 0 and ios count > 0.", () => {
        cy.log("Dashboard test started.");

        const mobileCount = 2;
        const androidCount = 0;
        const iosCount = 1;

        setupAppLoginCountsFixtures(mobileCount, androidCount, iosCount);

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/dashboard"
        );

        cy.get("[data-testid=total-mobile-users]").contains(mobileCount);

        cy.get("[data-testid=android-count]").should("not.exist");
        cy.get("[data-testid=ios-count]").should("not.exist");

        // Activate tooltip
        cy.get("[data-testid=total-mobile-users]").realHover();

        cy.get("[data-testid=android-count]").contains(androidCount);
        cy.get("[data-testid=ios-count]").contains(iosCount);

        cy.log("Dashboard test finished.");
    });

    it("Verify tooltip exists with android count > 0 and ios count == 0..", () => {
        cy.log("Dashboard test started.");

        const mobileCount = 2;
        const androidCount = 1;
        const iosCount = 0;

        setupAppLoginCountsFixtures(mobileCount, androidCount, iosCount);

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/dashboard"
        );

        cy.get("[data-testid=total-mobile-users]").contains(mobileCount);

        cy.get("[data-testid=android-count]").should("not.exist");
        cy.get("[data-testid=ios-count]").should("not.exist");

        // Activate tooltip
        cy.get("[data-testid=total-mobile-users]").realHover();

        cy.get("[data-testid=android-count]").contains(androidCount);
        cy.get("[data-testid=ios-count]").contains(iosCount);

        cy.log("Dashboard test finished.");
    });

    it("Verify tooltip does not exist with android count == 0 and ios count == 0.", () => {
        cy.log("Dashboard test started.");

        const mobileCount = 1;
        const androidCount = 0;
        const iosCount = 0;

        setupAppLoginCountsFixtures(mobileCount, androidCount, iosCount);

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/dashboard"
        );

        cy.get("[data-testid=total-mobile-users]").contains(mobileCount);

        cy.get("[data-testid=android-count]").should("not.exist");
        cy.get("[data-testid=ios-count]").should("not.exist");

        // Activate tooltip
        cy.get("[data-testid=total-mobile-users]").realHover();

        cy.get("[data-testid=android-count]").should("not.exist");
        cy.get("[data-testid=ios-count]").should("not.exist");

        cy.log("Dashboard test finished.");
    });
});
