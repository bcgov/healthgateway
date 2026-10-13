describe("Health Gateway Admin application", () => {
    it("loads the application", () => {
        cy.visit("/");
        cy.get("body").should("be.visible");
    });
});
