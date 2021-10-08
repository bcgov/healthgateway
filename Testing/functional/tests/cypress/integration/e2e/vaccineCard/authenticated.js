const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Authenticated User - Vaccine Card Page", () => {
    before(() => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
    });
    it("Vaccination Card - Partially Vaccinated - Registered Keycloak user", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
        cy.get("[data-testid=dose-1]").should("be.visible");
        cy.get("[data-testid=dose-2]").scrollIntoView().should("be.visible");
    });

    it("Vaccination Card - Show only valid doses for Partially Vaccinated with 1 Valid Dose and 2 Invalid Doses - Registered Keycloak user", () => {
        cy.login(
            Cypress.env("keycloak.invaliddoses.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
        cy.get("[data-testid=dose-1]").should("be.visible");
        cy.get("[data-testid=dose-2]").should("not.exist");
        cy.get("[data-testid=dose-3]").should("not.exist");
    });
});
