describe("WebClient Service", () => {
    it("Verify Swagger", () => {
        cy.log("Verifying Swagger exists for WebClient at Endpoint: ./swagger");
        cy.visit("/swagger").contains(
            "Health Gateway WebClient Services documentation"
        );
    });
});
