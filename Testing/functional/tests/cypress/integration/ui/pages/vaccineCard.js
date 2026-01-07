const vaccineCardUrl = "/vaccinecard";

describe("Vaccine Card Page", () => {
    it("Landing Page - Vaccination Card - unauthenticated user", () => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                },
            },
        });
        cy.logout();
        cy.visit(vaccineCardUrl);

        cy.get("[data-testid=vaccineCardFormTitle]").should("be.visible");
        cy.get("[data-testid=btnCancel]").should("be.visible");
        cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible");
        cy.get("[data-testid=privacy-statement-button]").should("be.visible");
        cy.get("[data-testid=btnLogin]").should("be.visible");
    });
});
