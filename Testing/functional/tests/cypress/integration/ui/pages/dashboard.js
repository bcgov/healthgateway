const { AuthMethod } = require("../../../support/constants");
const dashboardUrl = "/dashboard";
const covid19Url = "/covid19";
const timelineUrl = "/timeline";
const profileUrl = "/profile";

describe("Authenticated User - Dashboard Page", () => {
    it("Dashboard Page exists", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.contains("h2", "What do you want to focus on today?");
        cy.get("[data-testid=bcVaccineCard]").should("be.visible");
        cy.get("[data-testid=healthRecordsCard]").should("be.visible");

        cy.contains("h4", "Verify Contact Information");
        cy.get("[data-testid=profileLinkDashboard]")
            .should("be.visible")
            .click();

        cy.url().should("include", profileUrl);
    });

    it("Dashboard - Redirect to Covid19 page", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.get("[data-testid=bcVaccineCard]").should("be.visible").click();

        cy.url().should("include", covid19Url);
    });

    it("Dashboard - Redirect to timeline page", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.get("[data-testid=healthRecordsCard]").should("be.visible").click();

        cy.url().should("include", timelineUrl);
    });
});
