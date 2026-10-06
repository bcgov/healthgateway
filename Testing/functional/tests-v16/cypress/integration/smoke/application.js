describe("Health Gateway application", () => {
    it("loads the unauthenticated landing page", () => {
        cy.visit("/");
        cy.get("body").should("be.visible");
    });
});
