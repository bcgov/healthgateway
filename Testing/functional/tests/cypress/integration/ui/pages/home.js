const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";
const covid19Url = "/covid19";
const timelineUrl = "/timeline";

describe("Authenticated User - Home Page", () => {
    it("Home Page exists", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=bc-vaccine-card-btn]").should("be.visible");
        cy.get("[data-testid=health-records-card-btn]").should("be.visible");
    });

    it("Home - Federal Card button enabled", () => {
        cy.enableModules(["FederalCardButton"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("be.visible");
    });

    it("Home - Link to Covid19 page", () => {
        cy.enableModules(["VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=bc-vaccine-card-btn]")
            .should("be.visible")
            .click();

        cy.url().should("include", covid19Url);
    });

    it("Home - Link to timeline page", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=health-records-card-btn]")
            .should("be.visible")
            .click();

        cy.url().should("include", timelineUrl);
    });

    it("Home - Federal Card button disabled", () => {
        cy.enableModules([]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("not.exist");
    });
});
