const { AuthMethod } = require("../../../support/constants");

describe("Resource Centre", () => {
    it("Validate Disabled Covid Card", () => {
        cy.intercept("GET", "**/Immunization", {
            fixture: "ImmunizationService/immunizationNoRecords.json",
        });
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=hg-resource-centre]").should("be.visible").click();

        cy.get("[data-testid=hg-resource-centre-covid-card]")
            .should("be.visible")
            .should("not.be.enabled");
    });

    it("Validate on Timeline", () => {
        cy.intercept("GET", "**/Immunization", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=hg-resource-centre]").should("be.visible").click();

        cy.get("[data-testid=hg-resource-centre-covid-card]")
            .should("be.visible")
            .should("have.attr", "href", "/covid19");
    });
});
