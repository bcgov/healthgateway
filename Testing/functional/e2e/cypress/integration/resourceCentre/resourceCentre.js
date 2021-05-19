const { AuthMethod } = require("../../support/constants");

describe("Resource Centre", () => {
    beforeEach(() => {
        cy.restoreAuthCookies();
    });
    before(() => {
        let isLoading = false;
        cy.intercept("GET", "**/v1/api/Immunization/*", (req) => {
            req.reply((res) => {
                if (!isLoading) {
                    res.send({
                        fixture: "ImmunizationService/immunizationrefresh.json",
                    });
                } else {
                    res.send({
                        fixture: "ImmunizationService/immunization.json",
                    });
                }
                isLoading = !isLoading;
            });
        });
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Disabled Covid Card", () => {
        cy.get("[data-testid=hg-resource-centre]").should("be.visible").click();

        cy.get("[data-testid=hg-resource-centre-covid-card]")
            .should("be.visible")
            .should("not.be.enabled");
    });

    it("Validate on Timeline", () => {
        cy.visit("/timeline");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible").click();

        cy.get("[data-testid=hg-resource-centre-covid-card]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=covidImmunizationCard] .modal-dialog").should(
            "be.visible"
        );
        cy.get("[data-testid=patientBirthdate]").should(
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=doseDate]")
            .first()
            .should("be.visible", "not.be.empty");
        cy.get("[data-testid=doseDate]")
            .last()
            .should("be.visible", "not.be.empty");
    });

    it("Validate Visible on HealthInsights", () => {
        cy.visit("/healthInsights");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });

    it("Validate Visible on Dependents", () => {
        cy.visit("/dependents");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });

    it("Validate Visible on Reports", () => {
        cy.visit("/reports");
        cy.get("[data-testid=hg-resource-centre]").should("be.visible");
    });
});
