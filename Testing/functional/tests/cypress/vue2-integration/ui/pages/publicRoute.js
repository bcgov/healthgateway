const vaccineCardPath = "/vaccineCard";

describe("Public Route", () => {
    it("Redirect to Public Vaccine Card", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.visit(vaccineCardPath);
        cy.get("[data-testid=loginBtn]").should("not.exist");
        cy.location("pathname").should("eq", vaccineCardPath);
    });
});
