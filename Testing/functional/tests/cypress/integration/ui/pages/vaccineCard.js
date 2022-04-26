const homeUrl = "/";
const vaccineCardUrl = "/vaccinecard";
const vaccinationStatusModule = "VaccinationStatus";

describe("Vaccine Card Page", () => {
    it("Landing Page - Vaccine Card Button does not exist - Vaccine Status module disabled", () => {
        cy.enableModules([]);
        cy.visit(homeUrl);
        cy.get("[data-testid=btnVaccineCard]").should("not.exist");
    });

    it("Landing Page - Log In - Vaccine Status module disabled and vaccine card URL entered directly", () => {
        cy.enableModules([]);
        cy.visit(vaccineCardUrl);
        cy.get("[data-testid=vaccineCardFormTitle]").should("not.exist");
    });

    it("Landing Page - Vaccine Card Button Exists - Vaccine Status module enabled", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(homeUrl);
    });

    it("Landing Page - Vaccination Card - unauthenticated user", () => {
        cy.enableModules(vaccinationStatusModule);
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=vaccineCardFormTitle]").should("be.visible");
        cy.get("[data-testid=btnCancel]").should("be.visible");
        cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible");
        cy.get("[data-testid=btnPrivacyStatement]").should("be.visible");
        cy.get("[data-testid=btnLogin]").should("be.enabled", "be.visible");
    });
});
