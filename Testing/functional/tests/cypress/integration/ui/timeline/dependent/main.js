import { AuthMethod } from "../../../../support/constants";
import { setupStandardFixtures } from "../../../../support/functions/intercept";

const authorizedDependentHdid = "645645767756756767";
const unauthorizedDependentHdid = "343222434345442257";
const formattedDependentName = "Sam T";
const dependentsPath = "/dependents";

function loginToDependentTimeline(dependentHdid) {
    const waitForDependentFixture = prepareDependentFixtureWait();

    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        `/dependents/${dependentHdid}/timeline`
    );

    waitForDependentFixture();
}

function prepareDependentFixtureWait() {
    let requestStarted = false;

    cy.intercept("GET", "**/UserProfile/*/Dependent*", (req) => {
        requestStarted = true;
        req.reply({
            fixture: "UserProfileService/dependent.json",
        });
    }).as("getDependentFixture");

    return () =>
        cy.then(() => {
            if (requestStarted) {
                return cy.wait("@getDependentFixture");
            }

            cy.log("Using previously loaded dependent data.");
        });
}

describe("Dependent Timeline", () => {
    beforeEach(() => {
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
        });
        setupStandardFixtures();
    });

    it("Redirects an unauthorized dependent timeline", () => {
        loginToDependentTimeline(unauthorizedDependentHdid);

        cy.location("pathname").should("eq", "/unauthorized");
    });

    it("Displays the authorized dependent and supports page navigation", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        loginToDependentTimeline(authorizedDependentHdid);

        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=page-title]")
            .should("be.visible")
            .and("contain", formattedDependentName);
        cy.get("[data-testid=breadcrumb-dependent-name]")
            .should("be.visible")
            .and("contain", formattedDependentName);
        cy.get("[data-testid=add-comment-text-area]").should("not.exist");
        cy.get("[data-testid=post-comment-btn]").should("not.exist");

        cy.get("[data-testid=breadcrumb-dependents]").click();
        cy.location("pathname").should("eq", dependentsPath);

        cy.visit(dependentTimelinePath);
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=backBtn]").should("be.visible").click();
        cy.location("pathname").should("eq", dependentsPath);
    });
});
