const vaccineCardPath = "/vaccineCard";
const covidTestPath = "/covidTest";

describe("Public Route", () => {
    it("Redirect to Public Vaccine Card", () => {
        cy.enableModules(["Immunization"]);
        cy.logout();
        cy.visit(vaccineCardPath);
        cy.get("[data-testid=loginBtn]").should("not.exist");
        cy.location("pathname").should("eq", vaccineCardPath);
    });

    it("Redirect to Covid Test", () => {
        cy.enableModules(["Laboratory", "PublicLaboratoryResult"]);
        cy.logout();
        cy.visit(covidTestPath);
        cy.get("[data-testid=loginBtn]").should("not.exist");
        cy.location("pathname").should("eq", covidTestPath);
    });
});
