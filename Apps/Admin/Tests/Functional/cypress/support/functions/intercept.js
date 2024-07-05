const defaultTimeout = 60000;

export function setupStandardAliases(page) {
    switch (page) {
        case "/dashboard":
            setupDashboardAliases();
            break;
        case "/communications":
            setupCommunicationAliases();
            break;
        case "/feedback":
            setupFeedbackAliases();
            break;
        case "/beta-access":
            setupBetaAccessAliases();
            break;
        case "/reports":
            setupReportAliases();
            break;
        default:
            cy.log(`Alias setup on login not required for page: ${page}`);
            break;
    }
}

export function waitForInitialDataLoad(page) {
    switch (page) {
        case "/dashboard":
            waitForDashboard();
            break;
        case "/communications":
            waitForCommunication();
            break;
        case "/feedback":
            waitForFeedback();
            break;
        case "/beta-access":
            waitForBetaAccess();
            break;
        case "/reports":
            waitForReport();
            break;
        default:
            cy.log(
                `Wait for initial data load on login not required for page: ${page}`
            );
            break;
    }
}

function setupDashboardAliases() {
    cy.log("Setting up dashboard aliases.");

    cy.intercept("GET", "**/Dashboard/AllTimeCounts").as("getAllTimeCounts");
    cy.intercept("GET", `**/Dashboard/AppLoginCounts*`).as("getAppLoginCounts");
    cy.intercept("GET", "**/Dashboard/Ratings/Summary*").as(
        "getRatingsSummary"
    );
    cy.intercept("GET", "**/Dashboard/AgeCounts*").as("getAgeCounts");
    cy.intercept("GET", "**/Dashboard/DailyUsageCounts*").as(
        "getDailyUsageCounts"
    );
    cy.intercept("GET", "**/Dashboard/RecurringUserCount*").as(
        "getRecurringUserCount"
    );
}

function setupCommunicationAliases() {
    cy.log("Setting up communication aliases.");

    cy.intercept("GET", "**/Broadcast/").as("getBroadcast");
    cy.intercept("GET", `**/Communication/`).as("getCommunication");
}

function setupFeedbackAliases() {
    cy.log("Setting up feedback aliases.");

    cy.intercept("GET", "**/Tag/").as("getTag");
    cy.intercept("GET", `**/UserFeedback/`).as("getUserFeedback");
}

function setupBetaAccessAliases() {
    cy.log("Setting up beta access aliases.");

    cy.intercept("GET", "**/BetaFeature/AllUserAccess").as("getAllUserAccess");
}

function setupReportAliases() {
    cy.log("Setting up report aliases.");

    cy.intercept("GET", "**/AdminReport/BlockedAccess").as("getBlockedAccess");
    cy.intercept("GET", "**/AdminReport/ProtectedDependents*").as(
        "getProtectedDependents"
    );
}

function waitForDashboard() {
    cy.log("Wait on dashboard");
    cy.wait(
        [
            "@getAllTimeCounts",
            "@getAppLoginCounts",
            "@getRatingsSummary",
            "@getAgeCounts",
            "@getDailyUsageCounts",
            "@getRecurringUserCount",
        ],
        { timeout: defaultTimeout }
    );
}

function waitForCommunication() {
    cy.log("Wait on communication");
    cy.wait(["@getBroadcast", "@getCommunication"], {
        timeout: defaultTimeout,
    });
}

function waitForFeedback() {
    cy.log("Wait on feedback");
    cy.wait(["@getTag", "@getUserFeedback"], { timeout: defaultTimeout });
}

function waitForBetaAccess() {
    cy.log("Wait on beta access");
    cy.wait("@getAllUserAccess", { timeout: defaultTimeout });
}

function waitForReport() {
    cy.log("Wait on report");
    cy.wait(["@getBlockedAccess", "@getProtectedDependents"], {
        timeout: defaultTimeout,
    });
}
