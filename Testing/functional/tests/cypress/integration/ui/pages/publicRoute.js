const vaccineCardUrl = "/vaccineCard";
const covidTestUrl = "/covidTest";

describe("Public Route", () => {
    it("Redirect to Public Vaccine Card", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.visit(vaccineCardUrl);
        cy.get("[data-testid=loginBtn]").should("not.be.visible");
        cy.url().should("include", vaccineCardUrl);
    });

    it("Redirect to Covid Test", () => {
        cy.enableModules(["Laboratory", "PublicLaboratoryResult"]);
        cy.visit(covidTestUrl);
        cy.get("[data-testid=loginBtn]").should("not.be.visible");
        cy.url().should("include", covidTestUrl);
    });
});
