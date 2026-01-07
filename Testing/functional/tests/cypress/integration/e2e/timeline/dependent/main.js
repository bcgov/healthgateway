import { AuthMethod } from "../../../../support/constants";

const authorizedDependentHdid = "162346565465464564565463257";
const unauthorizedDependentHdid = "343222434345442257";
const formattedDependentName = "JENNIFER T";

const homePath = "/home";
const unauthorizedPath = "/unauthorized";

describe("Dependent Timeline", () => {
    beforeEach(() => {
        cy.configureSettings({
            timeline: {
                comment: true,
            },
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "diagnosticImaging",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
            ],
            dependents: {
                enabled: true,
                timelineEnabled: true,
                datasets: [
                    {
                        name: "healthVisit",
                        enabled: false,
                    },
                    {
                        name: "hospitalVisit",
                        enabled: false,
                    },
                    {
                        name: "note",
                        enabled: false,
                    },
                ],
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
        cy.location("pathname").should("eq", homePath);
    });

    it("Validate Health Records for unauthorized dependent is inaccessible", () => {
        cy.visit(`/dependents/${unauthorizedDependentHdid}/timeline`);
        cy.location("pathname").should("eq", unauthorizedPath);
    });

    it("Validate Health Records for authorized dependent is accessible and commenting is unavailable", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=add-comment-text-area]").should("not.exist");
        cy.get("[data-testid=post-comment-btn]").should("not.exist");
    });

    it("Validate back button goes to dependents page", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=backBtn]").should("be.visible").click();
        cy.location("pathname").should("eq", `/dependents`);
    });

    it("Validate page title includes formatted name", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=page-title]")
            .should("be.visible")
            .contains(formattedDependentName);
    });

    it("Validate bread crumb includes formatted name", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=breadcrumb-dependent-name]")
            .should("be.visible")
            .contains(formattedDependentName);
    });

    it("Validate bread crumb link goes to dependents page", () => {
        const dependentTimelinePath = `/dependents/${authorizedDependentHdid}/timeline`;
        cy.visit(dependentTimelinePath);
        cy.location("pathname").should("eq", dependentTimelinePath);
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=breadcrumb-dependents]")
            .should("be.visible")
            .click();
        cy.location("pathname").should("eq", `/dependents`);
    });
});
