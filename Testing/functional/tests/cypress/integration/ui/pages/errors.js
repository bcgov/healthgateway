describe("Error Pages", () => {
    beforeEach(() => {});

    it("HTTP 401", () => {
        cy.visit("/unauthorized");
        cy.contains("h1", "401");
    });

    it("HTTP 404", () => {
        cy.visit("/nonexistentpage");
        cy.contains("h1", "404");
    });
});
