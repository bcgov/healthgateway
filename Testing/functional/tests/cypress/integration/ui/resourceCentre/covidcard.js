const { AuthMethod } = require("../../../support/constants");

describe("Resource Centre", () => {
    before(() => {
        cy.enableModules("Immunization");
    });

    it("Validate Disabled Covid Card", () => {
        cy.intercept("GET", "/v1/api/Immunization", (req) => {
            req.reply((res) => {
                res.send({
                    fixture: "ImmunizationService/immunizationNoRecords.json",
                });
            });
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
        cy.intercept("GET", "/v1/api/Immunization", (req) => {
            req.reply((res) => {
                res.send({
                    fixture: "ImmunizationService/immunization.json",
                });
            });
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
            .click();

        // Vaccine Card
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");

        // Navigate Left
        cy.get("[data-testid=vc-chevron-left-btn]")
            .should("be.enabled", "be.visible")
            .click();

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
});
