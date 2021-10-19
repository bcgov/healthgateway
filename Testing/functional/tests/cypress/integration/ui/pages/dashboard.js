const { AuthMethod } = require("../../../support/constants");
const dashboardUrl = "/dashboard";
const covid19Url = "/covid19";
const timelineUrl = "/timeline";

describe("Authenticated User - Dashboard Page", () => {
    it("Dashboard Page exists", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.get("[data-testid=bc-vaccine-card-btn]").should("be.visible");
        cy.get("[data-testid=health-records-card-btn]").should("be.visible");
     
    });

    it("Dashboard - Link to Covid19 page", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.get("[data-testid=bc-vaccine-card-btn]")
            .should("be.visible")
            .click();

        cy.url().should("include", covid19Url);
    });

    it("Dashboard - Link to timeline page", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );

        cy.get("[data-testid=health-records-card-btn]")
            .should("be.visible")
            .click();

        cy.url().should("include", timelineUrl);
    });
});
